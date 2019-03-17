package com.pieperjones.junit5.common.extensions;

import com.pieperjones.junit5.common.enums.BrowserTypes;
import com.pieperjones.junit5.common.enums.Environments;
import com.pieperjones.junit5.common.testrail.gurock.APIException;
import com.pieperjones.junit5.common.testrail.reporter.TestrailReporter;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.junit.jupiter.api.extension.BeforeAllCallback;
import org.junit.jupiter.api.extension.ExtensionContext;
import org.junit.jupiter.api.extension.ExtensionContext.Namespace;
import org.junit.jupiter.api.extension.ExtensionContext.Store;
import org.junit.jupiter.api.extension.ExtensionContext.Store.CloseableResource;

import com.pieperjones.junit5.common.JupiterContextStoreKeys;


import lombok.Getter;
import lombok.Setter;

import static org.junit.jupiter.api.extension.ExtensionContext.Namespace.GLOBAL;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.net.MalformedURLException;
import java.util.Map;
import java.util.Properties;

import javax.crypto.SealedObject;

/********************************************************************************************
//* Copyright(c) 2018 Secure Channels, Inc. Irvine, CA. All Rights Reserved.
//* Filename:       StartupShutDownExtension.java
//* Revision:       1.0
//* Author:         Eric Jones
//* Created On:     Jan 10, 2018
//* Modified by: 	Eric Jones
//* Modified On: 	February 19, 2018
//*     
//* Description:    This Junit 5 Extension is used to do the global setup/teardown for a given
//* JUnit test run.  It also extends the EnvironmentContext, so that we have access to
//* config.crypto configuration information (primarily for accessing DB info).
/********************************************************************************************/

//TODO - If we move some of the webdriver code up to here?  How do we know when to create a webdriver object?
public class StartupShutDownExtension implements BeforeAllCallback{
	private static boolean started = false;
	final static Logger log = LogManager.getLogger(StartupShutDownExtension.class);
	private Store globalStore = null;
	private String testEnvironment = "";
	private final static String propFile = "p-j_common.properties";

	public static final String RESULTS_PATH = "..\\results\\";
	
	@Getter @Setter
	private static BrowserTypes browser = null;
	
	
	@Override
	public void beforeAll(ExtensionContext ctx) {
		var key = System.getProperty("BROWSER", BrowserTypes.CHROME.getBrowserType()).toUpperCase();
		setBrowser(BrowserTypes.valueOf(key));
		testEnvironment = System
				.getProperty("ENV", Environments.TEST.getEnv())
				.toLowerCase();

		var sendTestrailResults = System
				.getProperty("RESULTS", "FALSE");

		boolean isTestrailResultsEnabled;
		
		if (!started) {
			try {
				Properties prop  = new Properties();
				var filePath = "..\\COMMON\\resources\\" + propFile;
				var fileInput = new FileInputStream(filePath);
				prop.load(fileInput);
				log.debug("project properties have been loaded successfully.");

			started = true;
			File results = new File(RESULTS_PATH);
			if (sendTestrailResults != null && !"".equals(sendTestrailResults)) {
				isTestrailResultsEnabled = Boolean.parseBoolean(sendTestrailResults);
			}
			else {
				isTestrailResultsEnabled = false;
			}

			globalStore = ctx.getRoot().getStore(GLOBAL);
			globalStore.put(JupiterContextStoreKeys.RESULTS_FILE_PATH, results);
			globalStore.put(JupiterContextStoreKeys.SEND_TESTRAIL_RESULTS, isTestrailResultsEnabled);
			globalStore.put(JupiterContextStoreKeys.TESTRAIL_TESTRUN_NAME_KEY, 1);
			globalStore.put(JupiterContextStoreKeys.TEST_ENVIRONMENT, testEnvironment);
			globalStore.put(JupiterContextStoreKeys.TEST_CLASS_COUNTER, 0);
			globalStore.put(JupiterContextStoreKeys.BROWSER, getBrowser());

			log.info("***JUNIT EXECUTION STARTED***");
			} catch (Exception e) {
				e.printStackTrace();
			}
			
			globalStore.put("endit", new CloseThisOnlyAtTheEnd(ctx));
		}
	}

	/**
	 * This innerclass implements the CloseableResource interface, which will be run
	 * at the very end of this Junit Lifecycle.  It's primary purpose is to close out
	 * context stores.  But, as it is the last module run during Junit's execution, we can
	 * use it to close out things that we shouldn't at the method, class or suite level.
	 */
	private static class CloseThisOnlyAtTheEnd implements CloseableResource{
		private ExtensionContext ctx;
		private static final Namespace NAMESPACE = Namespace
				.create("com", "pieperjones", "junit5", "common", "extensions", "TestrailReporterExtension");
		CloseThisOnlyAtTheEnd(ExtensionContext ctx){
			this.ctx = ctx;
		}

		/**
		 * isTestRunCreated()
		 * @param globalStore - This is the all of our global settings and values that we'll need
		 *                    generate our results to testrail and other final bookkeeping measures.
		 * @return - True if we successfully created a testrail test run for this Junit lifecycle
		 */
		private boolean isTestRunCreated(Store globalStore) {
			if (globalStore.get(JupiterContextStoreKeys.TESTRAIL_TESTRUN_ID_KEY)==null) {
				return false;
			}
			return true;
		}

		/**
		 * close() is the interface method from CloseableResource.  In this method, we can close-out
		 * the test run.  It is used to upload testrail results from our collection via calls to the
		 * testrail API.  Via the TestrailReporterExtension.
		 *
		 * @throws MalformedURLException
		 * @throws IOException
		 * @throws APIException
		 */
		@SuppressWarnings("unchecked")
		@Override
		public void close() throws MalformedURLException, IOException, APIException {
			Store global = ctx.getRoot().getStore(GLOBAL);
			if (isTestRunCreated(global)) {
				log.info("***Closing out results update to Testrail***");
				var testStore = ctx.getStore(NAMESPACE);
				var testRunResults = ((Map<Integer, Map<String, Object>>) testStore
						.get(JupiterContextStoreKeys.TESTRUN_RESULTS));
				var totalTestsRun = testRunResults.size();
				var testrailProjectID = (int)testStore
						.get(JupiterContextStoreKeys.TESTRAIL_PROJECT_ID_KEY);
				var testrailSuiteID = (int)testStore
						.get(JupiterContextStoreKeys.TESTRAIL_SUITE_ID_KEY);
		
				var testRunID = (long)global
						.get(JupiterContextStoreKeys.TESTRAIL_TESTRUN_ID_KEY);
				var isTestrailResultsEnabled = (boolean)global
						.get(JupiterContextStoreKeys.SEND_TESTRAIL_RESULTS);

				// Only upload results, if tester has set Testrail Options to enabled
				/**
				 * TODO - Change this logic to do a try/catch block.  Catch the APIException.
				 * TODO - If we catch the exception.  Write the contents of our results to a file.
				 */
				if (isTestrailResultsEnabled) {
					TestrailReporter.updateTestRunWithCases(testRunID, testRunResults);
					TestrailReporter.updateTestRunWithResults(testRunID, testRunResults);
				}
				
				log.info(String.format("There was a total of %d tests executed, for testrunID: %d," +
								" testrail projectID: %d, suiteID: %d"
						,totalTestsRun, testRunID, testrailProjectID, testrailSuiteID));
			}
				log.info("***Completion of all tests***");
		}
	}

	private StartupShutDownExtension() {
		super();
	}
	
}
