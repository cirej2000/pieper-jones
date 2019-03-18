package com.pieperjones.junit5.common.enums;

public enum ApplicationName {
    APPLICATION_ONE("applicationOne"),
    APPLICATION_TWO("applicatoinTwo"),
    APPLICATION_THREE("applicationThree");

    private String applicationName;

    ApplicationName(String appName){
        applicationName = appName;
    }

    public String getApplicationName(){
        return applicationName;
    }
}
