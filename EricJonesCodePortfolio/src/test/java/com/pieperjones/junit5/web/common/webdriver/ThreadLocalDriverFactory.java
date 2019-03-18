package com.pieperjones.junit5.web.common.webdriver;
import com.pieperjones.junit5.common.enums.BrowserTypes;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.chrome.ChromeDriver;
import org.openqa.selenium.chrome.ChromeOptions;
import org.openqa.selenium.edge.EdgeDriver;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.remote.DesiredCapabilities;
import org.openqa.selenium.remote.RemoteWebDriver;

import java.net.MalformedURLException;
import java.net.URL;


public class ThreadLocalDriverFactory {
	
	private static ThreadLocal<WebDriver> tlDriver = new ThreadLocal<>();
	
	@SuppressWarnings("incomplete-switch")
	public synchronized static void setThreadLocalDriver (BrowserTypes browser) throws MalformedURLException {
		var localRemoteHost = System.getProperty("HOST", "localhost");

		final URL url  = new URL(String.format("http://%s:4444/wd/hub", localRemoteHost));
		switch (browser) {
			case CHROME:
			default:
				System.setProperty("webdriver.chrome.driver",
					"..\\COMMON_WEB\\resources\\chromedriver.exe");
				tlDriver = ThreadLocal.withInitial(
						() -> new ChromeDriver(BrowserOptionsManager
								.getChromeOptions()));
				break;
		case FIREFOX:
			System.setProperty("webdriver.gecko.driver",
					"..\\COMMON_WEB\\resources\\geckodriver.exe");
				tlDriver = ThreadLocal.withInitial(
						() -> new FirefoxDriver(BrowserOptionsManager
								.getFirefoxOptions()));
				break;
		case EDGE:
				System.setProperty("webdriver.edge.driver",
					"..\\COMMON_WEB\\resources\\MicrosoftWebDriver.exe");
				tlDriver = ThreadLocal.withInitial(
						() -> new EdgeDriver());
						break;
		case FIREFOX_GRID:
			System.setProperty("webdriver.gecko.driver",
					"..\\COMMON_WEB\\resources\\geckodriver.exe");

				tlDriver = ThreadLocal.withInitial(
						() -> new RemoteWebDriver(BrowserOptionsManager
								.getFirefoxOptions()));
				break;
		case CHROME_GRID:
			System.setProperty("webdriver.chrome.driver",
					"..\\QAT_COMMON_WEB\\resources\\chromedriver.exe");
				DesiredCapabilities capabilities = DesiredCapabilities.chrome();
				ChromeOptions options = BrowserOptionsManager.getChromeOptions();
				options.addArguments("headless");
				capabilities.setCapability(ChromeOptions.CAPABILITY,
						options);

				tlDriver = ThreadLocal.withInitial(
							() -> new RemoteWebDriver(url,capabilities));
				break;								
		}
	}
	
	public synchronized static WebDriver getThreadLocalDriver() {
		return tlDriver.get();
	}
}
