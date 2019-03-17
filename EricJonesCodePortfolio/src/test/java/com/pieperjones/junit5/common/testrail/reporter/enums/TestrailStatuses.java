package com.pieperjones.junit5.common.testrail.reporter.enums;

public enum TestrailStatuses {
	PASSED(1),
	BLOCKED(2),
	UNTESTED(3),
	RETEST(4),
	FAILED(5);
	
	private int status;
	
	TestrailStatuses(int status){
		this.status = status;
	}
	
	public int getStatus() {
		return this.status;
	}
}
