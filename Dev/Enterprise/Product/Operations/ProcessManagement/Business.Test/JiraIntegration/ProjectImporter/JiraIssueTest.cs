using CargoWise.EntityFramework.Testing;

namespace Enterprise.ProcessManagement.Business.Test
{
	public class JiraIssueTest : TestCaseWithFactory
	{
		public void TestExtractCustomFieldIdFromName_StandardCustomField()
		{
			var input = "customfield_1234";
			var result = JiraIssue.ExtractCustomFieldIdFromName(input);
			AssertEquals("Expected only the number of the custom field", "1234", result);
		}

		public void TestExtractCustomFieldIdFromName_OtherFormat()
		{
			var input = "some other prefix_5678";
			var result = JiraIssue.ExtractCustomFieldIdFromName(input);
			AssertEquals("Expected only the number of the custom field", "5678", result);
		}

		public void TestExtractCustomFieldIdFromName_NullValue()
		{
			var result = JiraIssue.ExtractCustomFieldIdFromName(null);
			AssertEquals("return input if format does not match", null, result);
		}

		public void TestExtractCustomFieldIdFromName_EmptyString()
		{
			var result = JiraIssue.ExtractCustomFieldIdFromName("");
			AssertEquals("return input if format does not match", "", result);
		}

		public void TestExtractCustomFieldIdFromName_OtherValue()
		{
			var input = "randomly different id value";
			var result = JiraIssue.ExtractCustomFieldIdFromName(input);
			AssertEquals("Expected the input string if it does not match", input, result);
		}
	}
}
