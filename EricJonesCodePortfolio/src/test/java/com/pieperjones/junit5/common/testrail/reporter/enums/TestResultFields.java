package com.pieperjones.junit5.common.testrail.reporter.enums;

public enum TestResultFields {
	STATUS("status_id"),
	MESSAGE("comment"),
	ASSIGNEE("assignedto_id"),
	DURATION("elapsed");
	
	private String fieldName;
	
	TestResultFields(String field){
		fieldName = field;
	}
	
	public String getFieldName() {
		return fieldName;
	}
}
