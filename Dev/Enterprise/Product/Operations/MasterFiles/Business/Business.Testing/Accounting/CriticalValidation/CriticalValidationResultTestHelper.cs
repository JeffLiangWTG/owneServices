using System.Text;
using static NUnit.Framework.Assertion;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class CriticalValidationResultTestHelper
	{
		public static void AssertCriticalValidationResult(TestCaseDefinition_ForSeparateTestsMethods testCase, CriticalValidationResult result)
		{
			AssertEquals(string.Format("[{0}] Correct error type should be reported.", testCase.Description), testCase.ErrorType, result.ErrorType);
			AssertContains(string.Format("[{0}] Correct error message should be generated.", testCase.Description), testCase.UserErrorMessage, result.UserFriendlyErrorMessage);
			AssertContainsInOrder(string.Format("[{0}] Correct error message should be generated.", testCase.Description), result.DeveloperErrorMessage, testCase.ExpectedTechDetailsInThisOrderIntoErrorMessage);
		}

		public static void AssertCriticalValidationResult(TestCaseDefinition_ForSeparateTestsMethods testCase, OnSavingCriticalCheckException exception)
		{
			AssertEquals(string.Format("[{0}] Correct error type should be reported.", testCase.Description), testCase.ErrorType.ToString(), exception.ErrorType);
			AssertContains(string.Format("[{0}] Correct error message should be generated.", testCase.Description), testCase.UserErrorMessage, exception.Message);
			AssertContainsInOrder(string.Format("[{0}] Correct error message should be generated.", testCase.Description), exception.DeveloperErrorMessage, testCase.ExpectedTechDetailsInThisOrderIntoErrorMessage);
		}
	}

	public class TestCaseDefinition_ForSeparateTestsMethods
	{
		public TestCaseDefinition_ForSeparateTestsMethods(string description)
			: this(description, false, CriticalValidationErrorType.NoError, string.Empty, System.Array.Empty<string>())
		{
		}

		public TestCaseDefinition_ForSeparateTestsMethods(string description, bool criticalCheckShouldFail, CriticalValidationErrorType errorType, string userErrorMessage, params string[] expectedTechDetailsInThisOrderIntoErrorMessage)
		{
			this.Description = description;
			this.CriticalCheckShouldFail = criticalCheckShouldFail;
			this.ErrorType = errorType;
			this.UserErrorMessage = userErrorMessage;
			this.ExpectedTechDetailsInThisOrderIntoErrorMessage = expectedTechDetailsInThisOrderIntoErrorMessage;
		}

		public CriticalValidationErrorType ErrorType { get; private set; }
		public string Description { get; private set; }
		public bool CriticalCheckShouldFail { get; private set; }
		public string UserErrorMessage { get; private set; }
		public string[] ExpectedTechDetailsInThisOrderIntoErrorMessage { get; private set; }

		public string GetExpectedErrorMessagePartsAsString()
		{
			var result = new StringBuilder();
			foreach (var part in ExpectedTechDetailsInThisOrderIntoErrorMessage)
			{
				result.AppendLine(part);
			}
			return result.ToString().TrimEnd('\r', '\n');
		}
	}
}
