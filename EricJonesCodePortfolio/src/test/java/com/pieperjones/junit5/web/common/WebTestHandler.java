package com.pieperjones.junit5.web.common;

import static org.junit.jupiter.api.extension.ExtensionContext.Namespace.GLOBAL;

import java.io.File;
import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Date;

import com.pieperjones.junit5.common.JupiterContextStoreKeys;
import com.pieperjones.junit5.common.enums.BrowserTypes;
import com.pieperjones.junit5.web.common.webdriver.ThreadLocalDriverFactory;
import org.apache.commons.io.FileUtils;
import org.junit.jupiter.api.TestInstance;
import org.junit.jupiter.api.TestInstance.Lifecycle;
import org.junit.jupiter.api.extension.AfterEachCallback;
import org.junit.jupiter.api.extension.BeforeEachCallback;
import org.junit.jupiter.api.extension.ExtensionContext;
import org.openqa.selenium.OutputType;
import org.openqa.selenium.TakesScreenshot;
import org.openqa.selenium.support.ui.WebDriverWait;

@TestInstance(Lifecycle.PER_METHOD)
public class WebTestHandler extends WebBaseTest implements BeforeEachCallback, AfterEachCallback{
	@Override
	public void afterEach(ExtensionContext ctx) throws Exception {
		var ee = ctx.getExecutionException();
		var global = ctx.getRoot().getStore(GLOBAL);
		if (ee != null && ee.isPresent()) {
		
			var testClass = ctx.getTestClass();
			var testMethod = ctx.getTestMethod().get().getName();
			
			if (testClass.toString().toLowerCase().contains("web.tests")) {
				File resultsFilePath = (File) global.get(JupiterContextStoreKeys.RESULTS_FILE_PATH);
				if (!resultsFilePath.exists()) {
					try {
						resultsFilePath.mkdir();
					} catch (Exception e) {
						e.printStackTrace();
					}
				}
				
				var screenshot = ((TakesScreenshot)driver);
				
				File screenshotFile = screenshot.getScreenshotAs(OutputType.FILE);
				File shotFile = new File(String.format("%s\\%s_%s_%s_%s_%s.png",
						resultsFilePath.getPath(),
						testMethod.toString(),
						getProject(),
						getBrowser(),
						getEnv(),
						new SimpleDateFormat("yyyyMMddHHmm").format(new Date())));
				try {
					FileUtils.copyFile(screenshotFile, shotFile);
				} catch (IOException e) {
					e.printStackTrace();
				}
			}
		}

		//Clear all cookies for this test
		if (getBrowser() == BrowserTypes.CHROME
				|| getBrowser() == BrowserTypes.CHROME_GRID) {
			getDriver().close();
		}
		getDriver().quit();
	}

	@Override
	public void beforeEach(ExtensionContext ctx) throws Exception {
		//Before each test, let's point the browser back to home
		ThreadLocalDriverFactory.setThreadLocalDriver(getBrowser());
		setDriver(ThreadLocalDriverFactory.getThreadLocalDriver());
		setWait(new WebDriverWait(driver, 15));
		if (getBrowser() == BrowserTypes.IE
				|| getBrowser() == BrowserTypes.FIREFOX
				|| getBrowser() == BrowserTypes.EDGE
				|| getBrowser() == BrowserTypes.FIREFOX_GRID
				) {
				getDriver().manage().window().maximize();
		}
		getDriver().navigate().to(getHostName());
	}	
}
