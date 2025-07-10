using System;
using Enterprise.Tracking.Web.ServerServices;
using Enterprise.ZArchitecture.Web.ServerServices.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBHandlingCodeLookupParametersTest : WebServiceParametersTest<AWBHandlingCodeLookupParameters>
	{
		#region Implementation

		protected override void AssertEquals(AWBHandlingCodeLookupParameters expected, AWBHandlingCodeLookupParameters actual)
		{
			AssertEquals(expected.CodeControlID, actual.CodeControlID);
			AssertEquals(expected.CodeValue, actual.CodeValue);
			AssertEquals(expected.DescriptionControlID, actual.DescriptionControlID);
		}

		protected override void AssertParsedParameters(AWBHandlingCodeLookupParameters parameters)
		{
			AssertEquals("TestCodeControlID", parameters.CodeControlID);
			AssertEquals("TestDescriptionControlID", parameters.DescriptionControlID);
			AssertEquals("TST", parameters.CodeValue);
		}

		protected override string GetExpectedExceptionMessageForStringWithValidationErrors()
		{
			return new ArgumentNullException("CodeControlID").Message;
		}

		protected override AWBHandlingCodeLookupParameters GetNewParameters()
		{
			var result = new AWBHandlingCodeLookupParameters();
			result.CodeControlID = "CodeControlID";
			result.CodeValue = "TST";
			result.DescriptionControlID = "DescriptionControlID";
			return result;
		}

		protected override AWBHandlingCodeLookupParameters GetNewParametersWithValidationErrors()
		{
			return new AWBHandlingCodeLookupParameters();
		}

		protected override string GetParametersInvalidString()
		{
			return "BLAH : BLAH;";
		}

		protected override string GetParametersString()
		{
			return "{CodeControlID: 'TestCodeControlID', CodeValue: 'TST', DescriptionControlID: 'TestDescriptionControlID'}";
		}

		protected override string GetParametersStringWithValidationErrors()
		{
			return "{CodeControlID: ''}";
		}

		#endregion
	}
}
