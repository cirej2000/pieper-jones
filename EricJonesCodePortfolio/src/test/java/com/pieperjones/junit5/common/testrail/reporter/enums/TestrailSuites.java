package com.pieperjones.junit5.common.testrail.reporter.enums;

public enum TestrailSuites {
	PROJECTONE_API(6),
	PROJECTONE_WEB(4),
	PROJECTONE_MOBILE_ADK(5),
	PROJECTONE_MOBILE_IOS(7),
	PROJECTTWO_API(8),
	PROJECTTWO_WEB(9),
	PROJECTTWO_MOBILE_ADK(10),
	PROJECTTWO_MOBILE_IOS(11),
	PROJECTTHREE_API(12);

	private int suiteID;
	
	TestrailSuites(int suite){
		suiteID=suite;
	}
	
	public int getSuiteID() {
		return suiteID;
	}
}
