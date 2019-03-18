package com.pieperjones.junit5.web.common.page;

import java.util.function.Function;
import java.util.function.Supplier;

import com.pieperjones.junit5.web.common.enums.WebTagAttributes;
import org.openqa.selenium.By;
import org.openqa.selenium.NoSuchElementException;
import org.openqa.selenium.StaleElementReferenceException;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;

/**
 * These conditions are added on top of the Selenium built in conditions, to give us more
 * granular control over our waits.
 */
public class AdditionalExpectedConditions {

	public static Function<WebDriver, WebElement> elementVisible(By ele){
		return driver ->{
			var we = driver.findElement((By) ele);
			try {
				if (we.isDisplayed()) {
					return we;
				}
			} catch (StaleElementReferenceException e) {
				return null;
			}
			return null;
		};	
	}

	public static Function<WebDriver, Boolean> elementIsVisible(WebElement element) {
		return driver -> {
			try {
				if (((WebElement) element).isDisplayed()) {
					return true;
				}
			} catch (StaleElementReferenceException e) {
				return false;
			}
			return false;
		};
	}
	
	public static Function<WebDriver, Boolean> elementIsNotVisible(WebElement element) {
		return driver -> {
			try {
					return !element.isDisplayed();
			}
			catch (StaleElementReferenceException  | NoSuchElementException e) {
				return true;
			}
		};
	}

	public static boolean elementIsActive(WebElement element) {
		try {
			return element.getAttribute("class").contains("active");
		}
		catch (StaleElementReferenceException  | NoSuchElementException e) {
			return true;
		}
	}
	
	public static boolean elementIsNotActive(WebElement element) {
		try {
			return !element.getAttribute("class").contains("active");
		}
		catch (StaleElementReferenceException  | NoSuchElementException e) {
			return true;
		}
	}
	
	public static Function<WebDriver, WebElement> elementIsClickable(WebElement element) {
		return driver -> {
			try {
				if (element.isDisplayed() && element.isEnabled()) {
					return element;
				}
				else {
					return null;
				}
			}
			catch(StaleElementReferenceException e) {
				return null;
			}
		};
	}
	
	public static Function<WebDriver,WebElement> elementContainsClass(WebElement element, String className) {
		return driver -> {
			try {
				if (element.getAttribute("class")
						.toLowerCase().equals(className.toLowerCase())) {
					return element;
				}
			}
			catch(NullPointerException e) {
				return null;
			}
			return null;
		};
	}
	
	public static Function<WebDriver, WebElement> elementTextIsEmpty(WebElement element) {
		return driver -> {
			try {
				if((element.getTagName().equals("div") 
						&& isNullOrEmpty(element.getAttribute("textContent")))) {
					return element;
				}
				else if (isNullOrEmpty(element.getText())) {
					return element;
				}
				else {
					return null;
				}
			}
			catch (StaleElementReferenceException e) {
				return null;
			}
		};
	}
	
	public static Function<WebDriver,WebElement> elementTextIsNotEmpty(Supplier<WebElement> element) {
		return driver -> {
			try {
				if((element.get().getTagName().equals("div") 
						&& !isNullOrEmpty((element.get()
								.getAttribute("textContent"))))) {
					return element.get();
				}
				else if (!isNullOrEmpty((element.get()
						.getText()))) {
					return element.get();
				}
				else {
					return null;
				}
			}
			catch (StaleElementReferenceException e) {
				return null;
			}
		};
	}
	
	public static Function<WebDriver, WebElement> elementTextFieldContains(WebElement element, String subString) {
		return driver -> {
			try {
				if(element.getText().contains(subString)) {
					return element;
				}
			} 
			catch (StaleElementReferenceException e) {
				return null;
			}
			return null;
		};
	}
	
	public static Function<WebDriver, WebElement> elementTextFieldContainsIgnoreCase(WebElement element, String subString) {
		return driver -> {
			try {
				if(element.getText()
						.toLowerCase()
						.contains(subString.toLowerCase())) {
					return element;
				}
			} 
			catch (StaleElementReferenceException e) {
				return null;
			}
			return null;
		};
	}
	
	public static Function<WebDriver, WebElement> elementHasAttributeWithValue(WebElement element
			,String attributeName, String attributeValue) {
		return driver -> {
			try {
				if (element.getAttribute(attributeName).contains(attributeValue)) {
					return element;			
				}
			}
			catch (NullPointerException e){
				return null;
			}
			return null;
		};
	}
	
	public static Function<WebDriver, WebElement> elementHasAttribute(WebElement element, WebTagAttributes attributeTag) {
		return driver ->{
			try {
				if (!isNullOrEmpty(element.getAttribute(attributeTag.getTagName()))) {
					return element;			
				}
			}
			catch (NullPointerException e){
				return null;
			}
			return null;
		};
	}
	
	public static Function<WebDriver, WebElement> elementHasAttributeWithValue(WebElement element
			, WebTagAttributes attributeTag, String value) {
		return driver -> {
		try {
			if (!isNullOrEmpty(element.getAttribute(attributeTag.getTagName()))
					&& element.getAttribute(attributeTag.getTagName()).contains(value)) {
				return element;			
			}
		}
		catch (NullPointerException e){
			return null;
		}
		return null;
		};
	}
	
	public static Function<WebDriver,WebElement> elementoesNotHaveAttributeWithValue(WebElement element
			,String attributeName, String attributeValue) {
		return driver -> {
			try {
				if (!element.getAttribute(attributeName).contains(attributeValue)) {
					return element;			
				}
			}
			catch (NullPointerException e){
				return null;
			}
			return null;
		};
	}
	
	public static Function<WebDriver,WebElement> elementoesNotHaveAttribute(WebElement element,
																			WebTagAttributes attributeTag) {
		return driver -> {
			try {
				if (isNullOrEmpty(element.getAttribute(attributeTag.getTagName()))) {
					return element;			
				}
			}
			catch (NullPointerException e){
				return null;
			}
			return null;
		};
	}
	
	public static Function<WebDriver,WebElement> elementDoesNotHaveAttributeWithValue(WebElement element
			, WebTagAttributes attributeTag, String value) {
		return driver -> {
			try {
				if (isNullOrEmpty(element.getAttribute(attributeTag.getTagName()))
						|| !element.getAttribute(attributeTag.getTagName()).contains(value)) {
					return element;			
				}
			}
			catch (NullPointerException e){
				return null;
			}
			return null;
		};
	}
	
	public static Function<WebDriver,WebElement> elementEqualsCssValue(WebElement element
			,String cssValue, String value) {
		return driver -> {
			try {
				if(element.getCssValue(cssValue).toLowerCase()
						.equals(value.toLowerCase())) {
					return element;
				}
			}
			catch(NullPointerException e) {
				return null;
			}
			return null;
		};
	}
	
	public static Function<WebDriver,WebElement> elementAttributeIsEmpty(WebElement element, String name) {
		return driver -> {
			try {
				if(isNullOrEmpty(element.getAttribute(name))) {
					return element;
				}
			}
			catch(NullPointerException e) {
				return null;
			}
			return null;
		};
	}

	public static Function<WebDriver,WebElement> elementAttributeIsEmpty(WebElement element, WebTagAttributes tag) {
		return driver -> {
			try {
				if(isNullOrEmpty(element.getAttribute(tag.getTagName()))) {
					return element;
				}
			}
			catch(NullPointerException e) {
				return null;
			}
			return null;
		};
	}
	
	public static Function<WebDriver,WebElement> elementAttributeIsNotEmpty(WebElement element, String name) {
		return driver -> {
			try {
				if(!isNullOrEmpty(element.getAttribute(name))) {
					return element;
				}
			}
			catch(NullPointerException e) {
				return null;
			}
			return null;
		};
	}

	public static Function<WebDriver,WebElement> elementAttributeIsNotEmpty(WebElement element, WebTagAttributes tag) {
		return driver -> {
			try {
				if(!isNullOrEmpty(element.getAttribute(tag.getTagName()))) {
					return element;
				}
			}
			catch(NullPointerException e) {
				return null;
			}
			return null;
		};
	}
	
	public static Function<WebDriver,WebElement> elementExists(WebElement element) {
		return driver -> {
			try {
				element.isDisplayed();
				return element;
			}
			catch(NullPointerException e) {
				return null;
			}
		};
	}
	
	private static boolean isNullOrEmpty(String string) {
		return (string == null || string.isEmpty());
	}
}
