using CsvHelper;
using PIEPER_JONES.Common.Enums;
using System.Collections.Generic;
using System.IO;

namespace PIEPER_JONES.TEST.LOANAPP.Api.Tests.Tests.Parameters
{
    /// <summary>
    /// This class will take data from a CSV file and use the CsvHelp library and yield key word 
    /// to return a list of objects to the test, into which they're injected.
    /// </summary>
    public class ReviewingStateTransitionsCSVParameters
    {
        private static string filePath = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "Resources\\";
        public static IEnumerable<dynamic []> GetStatusScenarios()
        {
            using (var csv = new CsvReader(new StreamReader($"{filePath}ReviewingTransitionStatus.csv")))
            {
                while (csv.Read())
                {
                    var testCaseName = csv.GetField<string>("TestcaseName");
                    var startingStatus = csv.GetField<LoanApplicationStatus>("StartingStatus");
                    var action = csv.GetField<LoanApplicationWorkflowAction>("WorkflowAction");
                    var expectedState = csv.GetField<LoanApplicationState>("ExpectedState");
                    var expectedStatus = csv.GetField<LoanApplicationStatus>("ExpectedStatus");
                    var agentName = csv.GetField<string>("AgentName");
                    var role = csv.GetField<AgentRole>("AgentRole");
                    yield return new dynamic[] {testCaseName, startingStatus, action, expectedState, expectedStatus, agentName, role};
                }
            }
        }

        public static IEnumerable<dynamic[]> GetStateScenarios()
        {
            using (var csv = new CsvReader(new StreamReader($"{filePath}ReviewingTransitionStates.csv")))
            {
                while (csv.Read())
                {
                    var testCaseName = csv.GetField<string>("TestcaseName");
                    var startingStatus = csv.GetField<LoanApplicationStatus>("StartingStatus");
                    var toState = csv.GetField<LoanApplicationState>("ToState");
                    var expectedState = csv.GetField<LoanApplicationState>("ExpectedState");
                    var expectedStatus = csv.GetField<LoanApplicationStatus>("ExpectedStatus");
                    yield return new dynamic[] { testCaseName, startingStatus, toState, expectedState, expectedStatus };
                }
            }
        }
    }
}
