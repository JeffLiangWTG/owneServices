using System;
using Enterprise.Tracking.Web.ServerServices;
using Enterprise.ZArchitecture.Web.ServerServices.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBOtherChargeCodeLookupParametersTest : WebServiceParametersTest<AWBOtherChargeCodeLookupParameters>
	{
		#region Implementation

		protected override void AssertEquals(AWBOtherChargeCodeLookupParameters expected, AWBOtherChargeCodeLookupParameters actual)
		{
			AssertEquals(expected.CodeControlID, actual.CodeControlID);
			AssertEquals(expected.CodeValue, actual.CodeValue);
			AssertEquals(expected.DescriptionControlID, actual.DescriptionControlID);
			AssertEquals(expected.ChargePK, actual.ChargePK);
			AssertEquals(expected.EntitlementCodeControlID, actual.EntitlementCodeControlID);
			AssertEquals(expected.PrepaidCollectFlag, actual.PrepaidCollectFlag);
			AssertEquals(expected.SessionIndex, actual.SessionIndex);
		}

		protected override void AssertParsedParameters(AWBOtherChargeCodeLookupParameters parameters)
		{
			AssertEquals("TestCodeControlID", parameters.CodeControlID);
			AssertEquals("TestDescriptionControlID", parameters.DescriptionControlID);
			AssertEquals("TST", parameters.CodeValue);
			AssertEquals("TestChargePK", parameters.ChargePK);
			AssertEquals("TestEntitlementControlID", parameters.EntitlementCodeControlID);
			AssertEquals("P", parameters.PrepaidCollectFlag);
			AssertEquals("TestSessionIndex", parameters.SessionIndex);
		}

		protected override string GetExpectedExceptionMessageForStringWithValidationErrors()
		{
			return new ArgumentNullException("SessionIndex").Message;
		}

		protected override AWBOtherChargeCodeLookupParameters GetNewParameters()
		{
			var result = new AWBOtherChargeCodeLookupParameters();
			result.CodeControlID = "CodeControlID";
			result.CodeValue = "TST";
			result.DescriptionControlID = "DescriptionControlID";
			result.PrepaidCollectFlag = "PrepaidCollectFlag";
			result.EntitlementCodeControlID = "EntitlementCodeControlID";
			result.ChargePK = "ChargePK";
			result.SessionIndex = "SessionIndex";
			return result;
		}

		protected override AWBOtherChargeCodeLookupParameters GetNewParametersWithValidationErrors()
		{
			return new AWBOtherChargeCodeLookupParameters();
		}

		protected override string GetParametersInvalidString()
		{
			return "BLAH : BLAH;";
		}

		protected override string GetParametersString()
		{
			return "{CodeControlID: 'TestCodeControlID', CodeValue: 'TST', DescriptionControlID: 'TestDescriptionControlID', ChargePK: 'TestChargePK', EntitlementCodeControlID: 'TestEntitlementControlID', PrepaidCollectFlag: 'P', SessionIndex: 'TestSessionIndex'}";
		}

		protected override string GetParametersStringWithValidationErrors()
		{
			return "{CodeControlID: ''}";
		}

		#endregion
	}
}
