package com.pieperjones.junit5.common.testrail.reporter.requests;

import java.util.Map;

import org.json.simple.JSONArray;
import org.json.simple.JSONObject;

public class TestResultRequestPayloads {
	@SuppressWarnings("unchecked")
	public static JSONObject addTestResults(Map<Integer, Map<String, Object>> results) {
		var testCaseIds = results.keySet();
		var resultSet = new JSONArray();
		
		for (var i : testCaseIds) {
			var result = new JSONObject();
			result.put("case_id", i);
			result.putAll(results.get(i));
			resultSet.add(result);
		}
		
		var fullResults = new JSONObject();
		fullResults.put("results", resultSet);
		return fullResults;
	}
}
