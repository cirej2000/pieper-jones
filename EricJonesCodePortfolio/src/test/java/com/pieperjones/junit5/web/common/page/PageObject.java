package com.pieperjones.junit5.web.common.page;


import java.lang.annotation.Annotation;
import java.lang.reflect.Field;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.List;
import java.util.concurrent.TimeUnit;
import java.util.function.Supplier;
import java.util.stream.Collectors;

import com.pieperjones.junit5.common.enums.ApplicationName;
import com.pieperjones.junit5.web.common.annotations.PieperJones_Exists;
import com.pieperjones.junit5.web.common.annotations.PieperJones_NotVisible;
import com.pieperjones.junit5.web.common.annotations.PieperJones_Required;
import com.pieperjones.junit5.web.common.annotations.PieperJones_Transitioning;
import com.pieperjones.junit5.web.common.enums.WebTagAttributes;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.openqa.selenium.support.ui.WebDriverWait;

import org.openqa.selenium.By;
import org.openqa.selenium.ElementNotVisibleException;
import org.openqa.selenium.JavascriptExecutor;
import org.openqa.selenium.Keys;
import org.openqa.selenium.NoSuchElementException;
import org.openqa.selenium.SearchContext;
import org.openqa.selenium.StaleElementReferenceException;
import org.openqa.selenium.TimeoutException;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.interactions.Actions;
import org.openqa.selenium.support.PageFactory;
import org.openqa.selenium.support.ui.ExpectedConditions;


import lombok.Getter;
import lombok.Setter;

import static com.pieperjones.junit5.web.common.page.AdditionalExpectedConditions.elementIsVisible;

/********************************************************************************************
//*
//*     
//* Description:    Base class for PageObject-based Selenium framework - Huge shout out to
//* John Jenkins and Bao Dinh - for everything I learned about this method of building pageObjects
/********************************************************************************************/

public abstract class PageObject<T extends PageObject<T>> {
 
	protected static Logger log = LogManager.getLogger(PageObject.class.getName());
	protected static ApplicationName application = null;

	@Getter @Setter
	protected WebDriver driver;
	
	JavascriptExecutor jsExecutor;
	
	public PageObject (WebDriver driver){
		this.driver = driver;
		PageFactory.initElements(getDriver(), this);
	    waitForPageLoaded();
	}
	
	@SuppressWarnings("static-access")
	public PageObject (WebDriver driver, ApplicationName application) {
		this.driver = driver;
		this.application = application;
		PageFactory.initElements(getDriver(), this);
		waitForPageLoaded();
	}
	
	protected WebElement findElement(Supplier<By> by) {
		if (by != null) {
			log.debug(String.format("Finding Element %s.", by.get().toString()));
			return driver.findElement(by.get());
		}
		return null;
	}
	
	protected final Collection<WebElement> findElements(Supplier<By> identifier){
		if (identifier != null) {
			log.debug(String.format("Finding Elements %s.",
					identifier.get().toString()));
			return driver.findElements(identifier.get());
		}
		return null;
	}
	
	protected void waitForPageTransition(Supplier<WebElement> element) {
		WebDriverWait wait;
		
		try {
			wait = new WebDriverWait(driver, 5);
			wait.until((driver) -> elementIsVisible(element.get()));
		} catch (ElementNotVisibleException | NoSuchElementException e) {
			log.debug("Expected page element not seen...continuing.");
		}
		finally {
			wait = new WebDriverWait(driver, 3);
			wait.until((driver) -> AdditionalExpectedConditions
					.elementIsNotVisible(element.get()));
		}
	}

	/**
	 * Wait for a page element to have one of its annotations start at one value and end at another
	 * Helps prevent stale reference exceptions when a page loads a change in the view such as a
	 * greyed out panel.
	 * @param element - WebElement that we're observing.
	 * @param attrib - The annotation of the WebElement (such as display)
	 * @param fromValue - The initial value we're waiting for.
	 * @param toValue - The value that we're waiting for it to settle on.
	 */
	protected void waitForPageTransition(Supplier<WebElement> element, WebTagAttributes attrib,
			String fromValue, String toValue) {
		WebDriverWait wait;
		
		try {
			wait = new WebDriverWait(driver, 5);
			wait.until((driver) -> AdditionalExpectedConditions
					.elementHasAttributeWithValue(element.get(), attrib, fromValue));	
		} catch (ElementNotVisibleException | NoSuchElementException e) {
			log.debug(String.format("Initial value:  %s, for annotation tag: %s, not found."
					+ "  Looking for next value", fromValue, attrib.getTagName()));
		}
		finally {
			wait = new WebDriverWait(driver, 3);
			wait.until((driver) -> AdditionalExpectedConditions
					.elementHasAttributeWithValue(element.get(), attrib, toValue));
		}
	}

	protected void waitForPageTransition(Supplier<WebElement> property, WebTagAttributes attrib
			, Integer waitInSecs) {
		WebDriverWait wait;
		waitInSecs = (waitInSecs == null) 
				? 
				2 : waitInSecs;
		try {
			wait = new WebDriverWait(driver, waitInSecs);
			wait.until((driver) -> AdditionalExpectedConditions
					.elementHasAttribute(property.get(), attrib));
		}
		catch(ElementNotVisibleException | NoSuchElementException | TimeoutException e) {
			log.debug(String.format("Could not find page element %s with annotation %s",
					property.get().getText(), attrib.getTagName()));
		}
		finally {
			wait = new WebDriverWait(driver, waitInSecs);
			wait.until((driver) -> AdditionalExpectedConditions
					.elementoesNotHaveAttribute(property.get(), attrib));
		}
	}
	
	@SuppressWarnings({ "rawtypes", "hiding", "unchecked" })
	public <T extends PageObject> T waitForPageLoaded() {
		try {
		driver.manage().timeouts()
		.implicitlyWait(0L, TimeUnit.SECONDS);
		
		Field[] fields = this
				.getClass()
				.getDeclaredFields();
			
			for (Field f : fields) {
				var annotations = Arrays.asList(f.getAnnotations())
						.stream()
						.filter(a -> a.annotationType()
								.getSimpleName()
								.startsWith("PieperJones_"))
								.collect(Collectors.toList());
				
				if (annotations.size() > 0) {
					waitForObjectPropertyToLoad(f, annotations);
				}
			}
				return (T)this;
			}
		finally
	    {
	        driver.manage().timeouts().implicitlyWait(10L, TimeUnit.SECONDS);
	        log.info(String.format("Loading finished for %s page.", this.getClass().getName()));
	    }
	}
	
	private void waitForObjectPropertyToLoad(Field objectField,
			List<Annotation> annotations) {
		for (Object a : annotations) {
			log.debug(String.format("Waiting to load page object field %s...",
					objectField.getName()));
			try {
				waitUntilAnnotationCondition(a, (WebElement)objectField.get(this), objectField.getName());
				System.out.println("Successful Annotation casting");
			} catch (IllegalArgumentException | IllegalAccessException e) {
				e.printStackTrace();
			}
		}
	}
	
	protected void waitUntilAnnotationCondition(Object annotation,
			WebElement webElement, String name) {
		new WebDriverWait(driver, 30)
		.until((driver) -> elementWithCondition(annotation, ()-> webElement, name));
	}
	
    private boolean elementWithCondition(Object annotation, Supplier<WebElement> elementProp,
    		String name)
    {
        try
        {
            return checkElementBasedOnAnnotation(annotation, elementProp, name);
        }
        catch (StaleElementReferenceException | NoSuchElementException e)
        {
            return false;
        }
    }

	/**
	 * This method will grab the objects representing our WebElements via reflection.
	 * If the object is decorated with our special annotations, we'll use that to perform
	 * a wait on a particular condition.  This will allow the page objects for the respective
	 * application pages to have a default page element upon which a wait upon page load will
	 * pivot.
	 * @param annotation - The annotation decorating the given WebElement
	 * @param elementObject
	 * @param name
	 * @return
	 */
	private boolean checkElementBasedOnAnnotation(Object annotation,
    		Supplier<WebElement> elementObject, String name){
    	
        if (elementObject.get() instanceof WebElement){
            log.debug(String.format("Trying to cast %s as an IWebElement", name));
            WebElement element = elementObject.get();
            if (annotation instanceof PieperJones_Required){
                log.debug(String.format("Waiting for WebElement %s to be visible.", name));
                moveToElement(elementObject, true);
                if (element.isDisplayed()){
                    return true;
                }
                return false;
            }
            else if (annotation instanceof PieperJones_Exists){
                log.debug(String.format("Waiting for WebElement %s to exist",name));
                return true;
            }
            else if (annotation instanceof PieperJones_NotVisible){
           		log.debug(String.format("Waiting for WebElement %s to exist, but not be visible.",name));
                if (!element.isDisplayed()){
                    return true;
                }
                return false;
            }
            else if (annotation instanceof PieperJones_Transitioning){
                log.debug(String.format("Waiting for ReadOnlyCollection<WebElement> %s to be visible then invisible", name));
                PieperJones_Transitioning vToVAnnotation = (PieperJones_Transitioning)annotation;
                // If element is displayed, then wait until gone
                // If element is not displayed then wait until timeout, if not gone, then we try again
                    try{
                        waitForPageTransition(elementObject, vToVAnnotation.attributeName(),
                        		vToVAnnotation.valueTextBefore(), vToVAnnotation.valueTextAfter());
                        return true;
                    }
                    catch (TimeoutException 
                    		| NoSuchElementException 
                    		| ElementNotVisibleException e){
                        return false;
                    }
             }
        }
		return false;
    }
    
    public void moveToElement(Supplier<WebElement> element, boolean mustBeInvisible){
        if (element != null && ((mustBeInvisible && !element.get().isDisplayed())
        		|| !mustBeInvisible)){
            log.info(String.format("Moving to %s, which is currently not in view...",
            		element.get().toString()));
            Actions action = new Actions(driver);
            action.moveToElement(element.get()).build().perform();
        }
    }
    
	@SuppressWarnings({ "unchecked", "rawtypes", "hiding" })
	protected <T extends PageObject> T sendKeys(
			Supplier<WebElement> property, String elementName, String keys) {
		if (property!=null){
			WebDriverWait wait = new WebDriverWait(driver, 30);
			wait.until((driver) -> ExpectedConditions
					.elementToBeClickable(property.get()));
			property.get().clear();
			try {
				wait = new WebDriverWait(driver, 1);
				wait.until((driver) -> AdditionalExpectedConditions
						.elementTextIsEmpty(property.get()));
			}
			catch(TimeoutException e) {
				log.error(String.format("Could not clear element %s's text. "
						+ "It currently contains %s", elementName, property.get().getText()));
			}
			property.get().sendKeys(keys);
		}
		return (T)this;
	}
	
	@SuppressWarnings({ "rawtypes", "unchecked", "hiding" })
	protected <T extends PageObject> T EnterDropDownMenuOption(
			Supplier<WebElement> elementProperty,
			String elementName,
			String dropDownSelectionValue){
				if (elementProperty != null) {
					var element = elementProperty.get();
					var wait = new WebDriverWait(driver, 15);
					wait.until((driver) -> ExpectedConditions
							.elementToBeClickable(element));
					sendKeys(() -> element, element.toString(), dropDownSelectionValue);
			}
		return (T)this;
	}
	
	@SuppressWarnings({ "rawtypes", "unchecked", "hiding" })
	protected <T extends PageObject> T EnterDropDownMenuOption(
			Supplier<WebElement> elementProperty,
			String elementName,
			long dropDownSelectionValue){
				if (elementProperty != null) {
					var element = elementProperty.get();
					var wait = new WebDriverWait(driver, 15);
					wait.until((driver) -> ExpectedConditions
							.elementToBeClickable(element));
					Collection<WebElement> options = getChildElements(elementProperty.get());
					for (var option : options) {
						if (Long.parseLong(option.getAttribute("value"))
								== dropDownSelectionValue) {
							wait.until((driver) -> ExpectedConditions.elementToBeClickable(option));
							try {
								option.sendKeys(Keys.TAB);
							}
							catch (IllegalStateException e) {
								//Ignore, for now
							}
							return (T)this;
						}
					}
				}		
				return (T)this;
	}
	
	@SuppressWarnings({ "unchecked", "rawtypes", "hiding" })
	protected <T extends PageObject> T sendFilenameToButton(
			Supplier<WebElement> property,
			String elementName,
			String fileName) {
		if (property!=null) {
			var wait = new WebDriverWait(driver, 30);
			wait.until(elementIsVisible(property.get()
					.findElement(By.xpath(".."))));
			property.get().sendKeys(fileName);
			return (T)this;
		}
		return null;
	}
	
	/**
	 * 
	 * @param property
	 * @param elementName
	 * @param verifyClickable
	 * @return 
	 */
	@SuppressWarnings({ "unchecked", "rawtypes", "hiding" })
	protected <T extends PageObject> T clickButton(Supplier<WebElement> property,
			String elementName, boolean verifyClickable){
		if (property != null) {
			log.debug(String.format("Targeting %s for click", elementName));
			WebDriverWait wait = new WebDriverWait(driver, 30);
			wait.until((driver) ->
				getClickableButton(property, elementName, verifyClickable))
				.click();
			log.info(String.format("%s button clicked.", elementName));
		}
		return (T)this;
	}
	
	protected void waitForElementToDisappear(Supplier<By> selector, Long delayInMillisecs) {
		if (delayInMillisecs == null) {
			delayInMillisecs = 3000L;
		}
		var endTime = System.currentTimeMillis() + delayInMillisecs;
		while (System.currentTimeMillis() < endTime) {
			try {
				driver.manage().timeouts()
				.implicitlyWait(0, TimeUnit.MILLISECONDS);
				findElement(selector);
			}
			catch(Exception e) {
				driver.manage().timeouts()
				.implicitlyWait(10, TimeUnit.SECONDS);
				break;
			}
		}
	}
	
	protected void waitForPageTransitionTextBlankToFull(Supplier<WebElement> identifier) {
		WebDriverWait wait = new WebDriverWait(driver, 5);
		
		try {
			wait.until(AdditionalExpectedConditions
					.elementTextIsEmpty(identifier.get()));
		}
		catch(ElementNotVisibleException
				| NoSuchElementException
				| TimeoutException e) {
			log.debug("Expected page element not found...continuing.");
		}
		finally {
			wait.until(AdditionalExpectedConditions
					.elementTextIsNotEmpty(identifier));
		}
	}
	
	protected void clickJS(WebElement element) {
			jsExecutor = (JavascriptExecutor) driver;
			jsExecutor.executeScript("arguments[0].click();", element);
	}
	
	protected void dragAndDrop(Supplier<WebElement> source, Supplier<WebElement> destination) {
		var actions = new Actions(driver);
		log.info(String.format("We're dragging and dropping from %s to %s."
				,source.get().toString(), destination.get().toString()));
		var dragAndDrop = actions.clickAndHold(source.get())
				.moveToElement(destination.get())
				.release(destination.get())
				.build();
		
		dragAndDrop.perform();
	}
	
	protected void scrollElementIntoView(Supplier<WebElement> element, boolean mustBeInvisible) {
		if (element !=null 
				&& ((mustBeInvisible && !element.get().isDisplayed())
				|| !mustBeInvisible)){
			log.info(String.format("Scrolling %s into view..."
					, element.get().toString()));
			
			((JavascriptExecutor)driver)
			.executeScript("arguments[0].scrollIntoView(true);", element.get());
		}
	}
	
	protected void rightClick(Supplier<WebElement> element) {
		if (element != null) {
			log.info(String.format("Right clicking %s",
					element.get().toString()));
			var wait = new WebDriverWait(driver, 30);
			wait.until(AdditionalExpectedConditions
					.elementIsClickable(element.get()));
			var actions = new Actions(driver);
			actions.contextClick(element.get());
			actions.perform();
		}
	}
	
	protected void mouseOverElement(Supplier<WebElement> element) {
		if (element != null) {
			log.info(String.format("Hover over %s", element.get().toString()));
			var wait = new WebDriverWait(driver,10);
			wait.until(elementIsVisible(element.get()));
			var actions = new Actions(driver);
			actions.perform();
		}
	}
	
	protected void switchToPopup() {
		switchToPopup(1);
	}
	
	protected void switchToPopup(int index) {
		var handles = 
				new ArrayList<String>(driver.getWindowHandles());
		driver.switchTo()
			.window(handles.get(index));
	}
	
	protected void closePopupWindow() {
		var handles = 
				new ArrayList<String>(driver.getWindowHandles());
		driver.close();
		driver.switchTo()
			.window(handles.get(0));		
	}
	
	protected String getUrl() {
		return driver.getCurrentUrl();
	}
	
	private WebElement getClickableButton(Supplier<WebElement> property, String elementName,
			boolean verifyClickable) {
		try {
			log.debug(String.format("Targeting %s for click", elementName));
			WebElement element = property.get();
			
			if (!verifyClickable || (element.isDisplayed() && element.isEnabled())) {
				return element;
			}
			return null;
		}
		catch (StaleElementReferenceException | NoSuchElementException  e) {
			return null;
		}
	}

	protected final Collection<WebElement> getChildElements(WebElement element){
		WebDriverWait wait = new WebDriverWait(driver, 30);
		return wait.until((driver) -> getExistingChildren(element));			
	}
	
	private final Collection<WebElement> getExistingChildren(SearchContext searchContext){
		try {
			log.info(String.format("Loading children of search:  %s", searchContext.toString()));
			//return searchContext.findElements(By.xpath("./*"));
			return searchContext.findElements(By.tagName("option"));
		}
		catch(StaleElementReferenceException e) {
			return null;
		}
	}	
}