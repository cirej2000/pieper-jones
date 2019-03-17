package com.pieperjones.junit5.common.enums;

public enum BrowserTypes {
    CHROME("chrome"),
    CHROME_HEADLESS("chrome headless"),
    CHROME_GRID("chrome grid (headless)"),
    FIREFOX("firefox"),
    FIREFOX_GRID("firefox grid"),
    IE("internet explorer"),
    EDGE("edge"),
    SAFARI("safari mac");

    private String browserType;

    BrowserTypes(String browser){
        browserType = browser;
    }

    public String getBrowserType(){
        return browserType;
    }
}
