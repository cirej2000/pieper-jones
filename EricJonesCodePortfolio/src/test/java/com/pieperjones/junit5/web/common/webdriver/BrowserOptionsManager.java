package com.pieperjones.junit5.web.common.webdriver;
import org.openqa.selenium.chrome.ChromeOptions;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxOptions;
import org.openqa.selenium.firefox.FirefoxProfile;
import org.openqa.selenium.firefox.ProfilesIni;
import org.openqa.selenium.ie.InternetExplorerDriver;
import org.openqa.selenium.ie.InternetExplorerOptions;
import org.openqa.selenium.remote.CapabilityType;

public class BrowserOptionsManager {

	public static ChromeOptions getChromeOptions() {
		ChromeOptions options = new ChromeOptions();
		options.addArguments("--start-maximized");
		options.addArguments("--ignore-certificate-errors");
		options.addArguments("disable-infobars");
		options.addArguments("--disable-popup-blocking");
		options.addArguments("--incognitio");
		return options;
	}
	
	public static FirefoxOptions getFirefoxOptions() {
		FirefoxOptions options = new FirefoxOptions();
		ProfilesIni prof = new ProfilesIni();
		FirefoxProfile profile = new FirefoxProfile();
		profile = prof.getProfile("selenium");
		profile.setAcceptUntrustedCertificates(true);
		profile.setAssumeUntrustedCertificateIssuer(false);
		profile.setPreference("network.proxy.type", 0);
		options.setCapability(FirefoxDriver.PROFILE, profile);
		return options;
	}
	
	public static InternetExplorerOptions getIeOptions() {
		InternetExplorerOptions options = new InternetExplorerOptions();
		options.destructivelyEnsureCleanSession();
		options.setCapability(InternetExplorerDriver
				.INTRODUCE_FLAKINESS_BY_IGNORING_SECURITY_DOMAINS, true);
		options.setCapability(CapabilityType.BROWSER_NAME, "IE");
		return options;
	}
}
