package com.pieperjones.junit5.common.extensions;

import java.util.HashMap;
import java.util.Map;

import com.pieperjones.junit5.common.testrail.reporter.TestrailCaseID;
import com.pieperjones.junit5.common.testrail.reporter.enums.TestResultFields;
import com.pieperjones.junit5.common.testrail.reporter.enums.TestrailStatuses;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.junit.jupiter.api.extension.AfterTestExecutionCallback;
import org.junit.jupiter.api.extension.BeforeTestExecutionCallback;
import org.junit.jupiter.api.extension.ExtensionContext;
import org.junit.jupiter.api.extension.ExtensionContext.Namespace;
import org.junit.jupiter.api.extension.ExtensionContext.Store;

import com.pieperjones.junit5.common.JupiterContextStoreKeys;

import static org.junit.jupiter.api.extension.ExtensionContext.Namespace.GLOBAL;


/********************************************************************************************
//* Description:    This JUnit5 extension is intended to setup each test method's execution
//* in preparation for gathering testrail results.  If testrail reporting is enabled via 
//* system parameter "RESULTS", then it will store the results in a global collection to be
//* consumed by the StartupShutdownExtension after all tests have run.
/********************************************************************************************/
public class TestrailReporterExtension implements AfterTestExecutionCallback,
BeforeTestExecutionCallback{
	private static Map<Integer, Map<String, Object>> testRunResults = new HashMap<>();
	private TestrailStatuses result;

	final static Logger log = LogManager.getLogger(TestrailReporterExtension.class);

	//Set a namespace for the test by test context store
	private static final Namespace NAMESPACE = Namespace
			.create("com", "pieperjones", "common", "TestrailReporterExtension");
	
	@Override
	public void beforeTestExecution(ExtensionContext ctx){
		Store global = ctx.getRoot().getStore(GLOBAL);
		log.info("Namespace and localstores obtained from the extension context.");

		getStore(ctx)
		.put(JupiterContextStoreKeys.JUNIT_TEST_START_TIME_KEY, System.currentTimeMillis());

		var testCaseCounter = (int) global.get(JupiterContextStoreKeys.TEST_CLASS_COUNTER);
		global.put(JupiterContextStoreKeys.TEST_CLASS_COUNTER, ++testCaseCounter);
		
		log.info("***TestrailReporter Extension Initialized***");
	}
	
	@SuppressWarnings("unchecked")
	@Override
    /**
     * After each test, we'll check to see if the option to push results to Testrail is enabled.
     * If we did enable the testrail results, we'll track them in our collection.
     * We'll look through the test class context and check for execution exceptions.
     * If it is a test failure we'll update the results as FAILURE.  If it's not a test
     * execution, we'll mark it as ERROR.
     */
	public void afterTestExecution(ExtensionContext ctx){
	    //TODO - remove this step, or add three options:  To Testrail, To File, To Nul
		if (!isTestrailEnabled(ctx)) {
			log.info("Testrail reporting not enabled, skipping test result update code.");
			return;
		}
		
		var testMethod = ctx.getTestMethod();
		
		long startTime = getStore(ctx)
				.remove(JupiterContextStoreKeys
						.JUNIT_TEST_START_TIME_KEY, long.class);
		long executionDuration = System.currentTimeMillis() - startTime;
		double timeInSecs = (executionDuration/1000.000);
		
		int testrailId = testMethod
                .get()
                .getAnnotation(TestrailCaseID.class)
                .value();

		String displayName = ctx.getDisplayName();
		String failureMessage = "Success";

		// Checking to see if our test errored out or failed assertions
		var ee = ctx.getExecutionException();

		if (ee != null && ee.isPresent()) {
			
			failureMessage = ee.get().getMessage();
			Boolean hit = ee.map(e -> e instanceof AssertionError)
					.orElse(false);
			
			if (hit) {
				log.info(String.format("TestrailId %s: %s, in method %s, failed due to test assertion error:  %s."
						+ "totalDuration:  %f (seconds)"
						, testrailId, displayName, testMethod, failureMessage, timeInSecs));
				result = TestrailStatuses.FAILED;
			}
			else {
				log.info(String.format("TestrailId %s: %s, in method %s, encountered a non test error:  %s.  "
						+ "totalDuration:  %f (seconds)"
						,testrailId, displayName, testMethod, failureMessage, timeInSecs));
				result = TestrailStatuses.BLOCKED;
			}
		}
		else {
			result = TestrailStatuses.PASSED;
			log.info(String.format("TestrailId %s: %s, in method %s, ran successfully!!  Duration:  %f (seconds)"
					,testrailId, displayName, testMethod, timeInSecs));
		}
		
		var currentTestRailInfo = new HashMap<String, Object>();

		currentTestRailInfo.put(TestResultFields.STATUS.getFieldName(),
				result.getStatus());
		currentTestRailInfo.put(TestResultFields.DURATION.getFieldName(), 
				String.format("%fs",timeInSecs));
		currentTestRailInfo.put(TestResultFields.MESSAGE.getFieldName(), 
				failureMessage);
		currentTestRailInfo.put(TestResultFields.ASSIGNEE.getFieldName(), 
				(8));
		testRunResults.put(testrailId, currentTestRailInfo);

		((Map<Integer,Map<String, Object>>) ctx
				.getStore(NAMESPACE)
				.get(JupiterContextStoreKeys.TESTRUN_RESULTS))
				.put(testrailId, currentTestRailInfo);
			
		Store global = ctx.getRoot().getStore(GLOBAL);
			
		var testCaseCounter = (int) global.get(JupiterContextStoreKeys.TEST_CLASS_COUNTER);
		global.put(JupiterContextStoreKeys.TEST_CLASS_COUNTER, --testCaseCounter);
	}

	private static Boolean isTestrailEnabled(ExtensionContext ctx) {
		return  ctx
				.getElement()
				.map(e -> e.isAnnotationPresent(TestrailCaseID.class))
				.orElse(false);
	}
	
	private Store getStore(ExtensionContext ctx) {
		return ctx.getStore(Namespace.create(getClass(), ctx.getRequiredTestMethod()));
	}
}
