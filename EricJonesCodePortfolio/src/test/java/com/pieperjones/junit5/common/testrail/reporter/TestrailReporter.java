package com.pieperjones.junit5.common.testrail.reporter;

import java.io.IOException;
import java.net.MalformedURLException;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

import com.pieperjones.junit5.common.testrail.gurock.APIClient;
import com.pieperjones.junit5.common.testrail.gurock.APIException;
import com.pieperjones.junit5.common.testrail.reporter.enums.TestrailProjects;
import com.pieperjones.junit5.common.testrail.reporter.requests.TestResultRequestPayloads;
import com.pieperjones.junit5.common.testrail.reporter.requests.TestRunRequestPayloads;
import lombok.Getter;
import lombok.Setter;
import org.json.simple.JSONArray;
import org.json.simple.JSONObject;


public class TestrailReporter {
	
	/*
	 * Local Variables Used to Track Testcase Progress
	 */

	//TODO - Move these constants to a configuration file.
	private static final String AUTOMATION_KEY = "YOURKEYHERE";
	private static final String AUTOMATION_ID = "dummy@pieper-jones.com";
	private static final String TESTRAIL_HOSTNAME = "pieperjones.testrail.io";
	
	@Getter @Setter
	private static String testCaseID;
	@Getter @Setter
	private static int mileStoneID;
	@Getter @Setter
	private static int projectID;
	@Getter @Setter
	private static int suiteID;
	@Getter @Setter
	private static int testRunID;
	@Getter @Setter
	private static List<String> references;
	@Getter @Setter
	private static List<String> testSuites;
	@Getter @Setter
	private static List<String> testCases;
	@Getter @Setter
	private static List<String> testRuns;
	@Getter @Setter
	private static APIClient testrailClient = null;
		

	public static void initializeClient() {
		testrailClient = new APIClient(String.format("https://%s/", TESTRAIL_HOSTNAME));
		testrailClient.setUser(AUTOMATION_ID);
		testrailClient.setPassword(AUTOMATION_KEY);
	}
		
	public static JSONArray getAllProjects() throws IOException, APIException {
		clientNullCheck();
		return (JSONArray)testrailClient.sendGet("get_projects");
	}
	
	public static JSONObject getProject(TestrailProjects projectId) throws IOException, APIException {
		clientNullCheck();
		return (JSONObject)testrailClient.sendGet(String.format("get_project/%s", projectId.getProjectID()));
	}
	
	public static JSONArray getMilestones(TestrailProjects projectId) throws IOException, APIException {
		clientNullCheck();
		return (JSONArray)testrailClient.sendGet(String.format("get_milestones/%s", projectId.getProjectID()));
	}
	
	@SuppressWarnings("unchecked")
	public static JSONArray getOpenMilestone(TestrailProjects projectId) throws IOException, APIException {
		var milestones = getMilestones(projectId);
		List<String> jlist = (ArrayList<String>) milestones.stream()
				.filter(( m -> ((JSONObject)m).get("is_completed").equals(false)))
				.collect(Collectors.toList());
		milestones = new JSONArray();
		milestones.add(jlist);
		return milestones;
	}
	
	public static JSONObject addTestRunNoCases(int testrailProjectID, int suiteID,
			String runName, boolean includeAll) throws  IOException, APIException{
		clientNullCheck();
		
		return (JSONObject)testrailClient
				.sendPost(String.format("add_run/%s",testrailProjectID),
						TestRunRequestPayloads.addRunNoCasesPayload(suiteID, runName, includeAll));
	}
	
	public static JSONObject updateTestRunWithCases(long runID, List<Integer> testCaseIDs) 
			throws  IOException, APIException {
		clientNullCheck();
		
		return (JSONObject)testrailClient
				.sendPost(String.format("update_run/%d", runID),
						TestRunRequestPayloads.updateRunWithTestcases(testCaseIDs));
	}
	
	public static JSONObject updateTestRunWithCases(long runID,
			Map<Integer,Map<String, Object>> results) 
			throws  IOException, APIException {
		clientNullCheck();
		
		return (JSONObject)testrailClient
				.sendPost(String.format("update_run/%d", runID),
						TestRunRequestPayloads.updateRunWithTestcases(results));
	}
	
	public static JSONArray updateTestRunWithResults(long runID, 
			Map<Integer, Map<String, Object>> results) throws IOException, APIException {
		clientNullCheck();
		
		return (JSONArray)testrailClient.sendPost(String.format("add_results_for_cases/%d", runID),
				TestResultRequestPayloads.addTestResults(results));
	}
	
	private static void clientNullCheck() {
		if (testrailClient == null) {
			initializeClient();
		}
	}
	
	
	public static void main(String [] args) {
		try {
			JSONArray openMilestones = getOpenMilestone(TestrailProjects.TESTPROJECT1);
			System.out.println(openMilestones);
		} catch (IOException | APIException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
}
