package com.pieperjones.junit5.web.common;

import java.net.MalformedURLException;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import com.pieperjones.junit5.common.BaseTest;
import com.pieperjones.junit5.common.JupiterContextStoreKeys;
import com.pieperjones.junit5.common.enums.BrowserTypes;
import com.pieperjones.junit5.common.testrail.reporter.TestrailBase;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.TestInstance;
import org.junit.jupiter.api.TestInstance.Lifecycle;
import org.junit.jupiter.api.extension.BeforeAllCallback;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.extension.ExtensionContext;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.support.ui.WebDriverWait;


import lombok.Getter;
import lombok.Setter;

import static org.junit.jupiter.api.extension.ExtensionContext.Namespace.GLOBAL;

//TODO - Should this class work on a per test method basis
//TODO - keep the same web driver for each testcase  Move some code
//TODO - to the WebTestHandler?
/*************************************************************************************
// * 
 * @author ejones
 *
 */
@ExtendWith(WebTestHandler.class)
@TestInstance(Lifecycle.PER_CLASS)
@TestrailBase
public class WebBaseTest extends BaseTest implements BeforeAllCallback{

	protected Map<String, List<String>> testRunResults = new HashMap<>();
	final static Logger log = LogManager.getLogger(WebBaseTest.class);
	
	@Getter @Setter
	protected static String hostName = "";	

	@Getter @Setter
	protected WebDriverWait wait = null;
	
	@Getter @Setter
	protected static WebDriver driver = null;
	
	@Getter @Setter
	private static String clientKey = "";
	
	@Getter @Setter
	private static String clientSecret = "";
	
	@Getter @Setter
	private static String env = null;
	
	@Getter @Setter
	private static String project = null;
	
	@Getter @Setter
	private static BrowserTypes browser = null;
	
	/*
	 * Get our Environment and service against which to test
	 * Determine whether or not we'll report to testRail
	 * Individual testcases can influence this.	
	 */
	@BeforeAll
	@Override
	protected void setupTests() throws MalformedURLException {
		//TODO - Figure out how to get information from the StartupShutdownExtension
		setHostName(String.format("https://%s.%s.com", getEnv(), getProject()));

		//KEEP THIS PART and the HostName from above, different host from API
		log.info(String.format("Selenium Web Framework is now ready for testing"
			+ " in the %s environment on %s.  Site homepage = %s.",
			getEnv(), getProject(), getHostName()));
	}

	@Override
	public void beforeAll(ExtensionContext ctx) throws Exception {
		var globalStore = ctx.getRoot().getStore(GLOBAL);
		setClientKey((String)globalStore.get(JupiterContextStoreKeys.CLIENTKEY));
		setClientSecret((String)globalStore.get(JupiterContextStoreKeys.CLIENTSECRET));
		setBrowser((BrowserTypes)globalStore.get(JupiterContextStoreKeys.BROWSER));
		setEnv((String)globalStore.get(JupiterContextStoreKeys.TEST_ENVIRONMENT));
		setProject((String)globalStore.get(JupiterContextStoreKeys.APP_UNDER_TEST));
	}
}