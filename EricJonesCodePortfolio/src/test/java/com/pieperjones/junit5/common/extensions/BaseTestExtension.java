package com.pieperjones.junit5.common.extensions;

import static org.junit.jupiter.api.extension.ExtensionContext.Namespace.GLOBAL;

import java.lang.reflect.Field;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;

import com.pieperjones.junit5.common.testrail.reporter.TestrailReporter;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.junit.jupiter.api.extension.BeforeAllCallback;
import org.junit.jupiter.api.extension.ExtensionContext;
import org.junit.jupiter.api.extension.ExtensionContext.Namespace;
import org.junit.jupiter.api.extension.ExtensionContext.Store;

import com.pieperjones.junit5.common.JupiterContextStoreKeys;

/********************************************************************************************
//* Description:    This JUnit5 Extension is meant to handle setup and cleanup for individual
 * test classes.
 */
/********************************************************************************************/
public class BaseTestExtension implements BeforeAllCallback{
	final static Logger log = LogManager.getLogger(BaseTestExtension.class);
	private static Map<Integer, Map<String, Object>> testRunResults = new HashMap<>();

	private static final Namespace NAMESPACE = Namespace
			.create("com", "pieperjones", "common", "TestrailReporterExtension");
	private long testRunID;


	@Override
	public void beforeAll(ExtensionContext ctx) throws Exception{	
		//Get our testclass level and JRun global params
		var store = ctx.getStore(NAMESPACE);
		var global = ctx.getRoot().getStore(GLOBAL);
		Field[] fields = ctx.getTestClass()
				.get()
				.getFields();
		var testrailProjectID = (int)fields[0].get(fields);
		var testrailSuiteID = (int)fields[1].get(fields);
		var testrailSuiteName =(String)fields[3].get(fields);
		var testEnvironment = (String)global.get(JupiterContextStoreKeys.TEST_ENVIRONMENT);
		var isTestrailResultsEnabled = (boolean)global
				.get(JupiterContextStoreKeys.SEND_TESTRAIL_RESULTS);

		// Only upload results if tester has set Testrail Options to enabled
		// If there's no testrun data stored, we need to set it up and get the Testrail ID
		if (isTestrailResultsEnabled) {
			if (!isTestRunCreated(global)) {
				var runName = System.getProperty("RUNNAME",
						String.format("%s-%s_Environment_AUTOMATION", testrailSuiteName,
								testEnvironment));
				runName += String.format("_%s", new SimpleDateFormat("EEE_MMM_dd_yyyy HH:mm:ss z")
						.format(new Date()));

				var result = TestrailReporter
						.addTestRunNoCases(testrailProjectID, testrailSuiteID, runName, false);

				testRunID = (long) result.get("id");
				global.put(JupiterContextStoreKeys.TESTRAIL_TESTRUN_ID_KEY, testRunID);
			}
			else {
				testRunID = (long)global
						.get(JupiterContextStoreKeys.TESTRAIL_TESTRUN_ID_KEY);
			}
		}
		
		// Store our global values and we're ready to run the tests in this class
		store.put(JupiterContextStoreKeys.TESTRUN_RESULTS, testRunResults);
		store.put(JupiterContextStoreKeys.TESTRAIL_PROJECT_ID_KEY, testrailProjectID);
		store.put(JupiterContextStoreKeys.TESTRAIL_SUITE_ID_KEY, testrailSuiteID);
		log.info(String.format("Before all callback for Testclass:  %s, has completed."
				, ctx.getTestClass().toString()));
	}

	/**
	 * This method will track whether or not we actually created a TESTRAIL testrun.
	 * If so, then we're good to go for writing to the Testrail Reporter -> API -> Testrail.
	 * @param globalStore - Global Junit context store
	 * @return - If we actually did create the testrail testrun and have an ID
	 */
	private boolean isTestRunCreated(Store globalStore) {
		if (globalStore.get(JupiterContextStoreKeys.TESTRAIL_TESTRUN_ID_KEY)==null) {
			return false;
		}
		return true;
	}
}
