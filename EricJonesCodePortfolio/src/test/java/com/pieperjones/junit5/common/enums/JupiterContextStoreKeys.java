package com.pieperjones.junit5.common.enums;

public enum JupiterContextStoreKeys {

    TESTRAIL_PROJECT_ID("Testrail ProjectID"),
    TESTRAIL_TESTRUN_NAME("Testrail Test Run Name"),
    TESTRAIL_TESTSUITE_ID("Testrail Test Suite ID"),
    IS_TESTRAIL_REPORTER_ENABLED("Is Testrail API Reporter Enabled"),
    GLOBAL_TEST_COUNT("Count the total number of tests in this junit run"),
    TEST_ENVIRONMENT("Which Test Environment are we running against"),
    RESULTS_FILE_PATH("Where to store results like screenshots"),
    BROWSER_TYPE("Which browser(s) are we testing against"),
    JUNIT_START_TIME("Global start of all tests"),
    JUNIT_END_TIME("Global end of all tests");


    private String keyName;

    JupiterContextStoreKeys(String key){
        keyName=key;
    }

    public String getKeyName(){
        return keyName;
    }
}
