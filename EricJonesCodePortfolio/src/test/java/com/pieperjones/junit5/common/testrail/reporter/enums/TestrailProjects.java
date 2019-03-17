package com.pieperjones.junit5.common.testrail.reporter.enums;

public enum TestrailProjects {

	TESTPROJECT1(1),
	TESTPROJECT2(2),
	TESTPROJECT3(3),
	;
	
	private final int projectID;
	
	TestrailProjects(int id){
		projectID = id;
	}
	
	public int getProjectID(){
		return projectID;
	}
}
