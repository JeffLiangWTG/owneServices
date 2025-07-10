using CargoWise.EntityFramework.Testing;
using static Enterprise.Freight.Business.ChargesApplyHelper;
using AsAgreedCodes = Enterprise.Core.Constants.AWB.AsAgreedTypes.Codes;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ChargesApplyHelperTest : TestCaseWithFactory
	{
		public void TestGetDefaultChargesApply()
		{
			var nullCharges = GetDefaultChargesApply(null, null);
			AssertEquals("Expected NON charges", ChargesApplyConstants.NON, nullCharges);

			var emptyCharges = GetDefaultChargesApply("", "");
			AssertEquals("Expected NON charges", ChargesApplyConstants.NON, emptyCharges);

			var invalidCharges = GetDefaultChargesApply(AsAgreedCodes.None, AsAgreedCodes.Collect);
			AssertEquals("Expected NON charges", ChargesApplyConstants.NON, invalidCharges);

			var validNoneCharges = GetDefaultChargesApply(AsAgreedCodes.None, AsAgreedCodes.None);
			AssertEquals("Expected NON charges", ChargesApplyConstants.NON, validNoneCharges);

			var validCPDCharges = GetDefaultChargesApply(AsAgreedCodes.Collect, AsAgreedCodes.Prepaid);
			AssertEquals("Expected CPD charges", ChargesApplyConstants.CPD, validCPDCharges);
		}

		public void TestGetAsAgreedCodesFromChargesApply()
		{
			var emptyCharges = GetAsAgreedCodesFromChargesApply("");
			AssertEquals("Expected item 1 to be NONE", AsAgreedCodes.None, emptyCharges.asAgreedFirstSet);
			AssertEquals("Expected item 2 to be NONE", AsAgreedCodes.None, emptyCharges.asAgreedSecondSet);

			var nullCharges = GetAsAgreedCodesFromChargesApply(null);
			AssertEquals("Expected item 1 to be NONE", AsAgreedCodes.None, nullCharges.asAgreedFirstSet);
			AssertEquals("Expected item 2 to be NONE", AsAgreedCodes.None, nullCharges.asAgreedSecondSet);

			var invalidCharges = GetAsAgreedCodesFromChargesApply("ABC");
			AssertEquals("Expected item 1 to be NONE", AsAgreedCodes.None, invalidCharges.asAgreedFirstSet);
			AssertEquals("Expected item 2 to be NONE", AsAgreedCodes.None, invalidCharges.asAgreedSecondSet);

			var validCALCharges = GetAsAgreedCodesFromChargesApply("CAL");
			AssertEquals("Expected item 1 to be NONE", AsAgreedCodes.Collect, validCALCharges.asAgreedFirstSet);
			AssertEquals("Expected item 2 to be NONE", AsAgreedCodes.All, validCALCharges.asAgreedSecondSet);
		}
	}
}
