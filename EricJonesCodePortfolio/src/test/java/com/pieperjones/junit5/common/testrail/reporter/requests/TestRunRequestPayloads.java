package com.pieperjones.junit5.common.testrail.reporter.requests;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import org.json.simple.JSONArray;
import org.json.simple.JSONObject;

public class TestRunRequestPayloads {
	//TODO - Need to support the concept of a test plan
	/*
	 * name	string	The name of the test plan (required)
	 *description	string	The description of the test plan
     *milestone_id	int	The ID of the milestone to link to the test plan
     *entries	array	An array of objects describing the test runs of the plan
	 */
	@SuppressWarnings("unchecked")
	public static JSONObject addRunNoCasesPayload(int suiteId, String runName, boolean includeAll) {
		var payload = new JSONObject();
		payload.put("suite_id", suiteId);
		payload.put("name", runName);
		payload.put("include_all", false);
		return payload;
	}
	
	public static JSONObject updateRunWithTestcases(Map<Integer, Map<String, Object>> results) {
		var ids = results.keySet();
		return updateRunWithTestcases(new ArrayList<>(ids));
	}
	
	@SuppressWarnings("unchecked")
	public static JSONObject updateRunWithTestcases(List<Integer> testCaseIds) {
		var payload = new JSONObject();
		var ids = new JSONArray();
		
		testCaseIds.forEach(ids :: add);
		
		payload.put("case_ids", ids);
		return payload;
	}
}

