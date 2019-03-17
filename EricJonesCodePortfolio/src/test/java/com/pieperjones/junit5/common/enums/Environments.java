package com.pieperjones.junit5.common.enums;

public enum Environments {
    TEST("test"),
    DEV("dev"),
    STAGE("stage"),
    UAT("uat"),
    PREPRODUCTION("preprod"),
    BETA("beta"),
    HOTFIX("hotfix");

    private String env;

    Environments(String env){
        this.env = env;
    }

    public String getEnv(){
        return env;
    }
}
