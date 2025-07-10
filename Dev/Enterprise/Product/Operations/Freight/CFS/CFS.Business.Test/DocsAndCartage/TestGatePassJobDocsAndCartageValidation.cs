using CargoWise.ComponentModel;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class TestGatePassJobDocsAndCartageValidation : JobDocsAndCartageValidationTest
	{
		public void TestValidateReasonForContingencyRelease()
		{
			TestValidateReasonForContingencyReleaseInternal(true, "", true);
			TestValidateReasonForContingencyReleaseInternal(true, "sample text", false);
			TestValidateReasonForContingencyReleaseInternal(false, "", false);
			TestValidateReasonForContingencyReleaseInternal(false, "sample text", false);
		}

		void TestValidateReasonForContingencyReleaseInternal(bool isContingencyRelease, string reason, bool expectError)
		{
			DocsAndCartage.JP_IsContingencyRelease = isContingencyRelease;
			DocsAndCartage.ReasonForContingencyRelease = reason;
			DocsAndCartage.Validation.ValidateReasonForContingencyRelease();
			AssertEquals(expectError, DocsAndCartage.ReasonForContingencyReleaseInfo.HasErrors());
			if (expectError)
			{
				DocsAndCartage.JP_IsContingencyRelease = false;
				AssertNoErrors(DocsAndCartage.ReasonForContingencyReleaseInfo);
			}
		}

		#region Implementation

		GatePassShipment Shipment;
		GatePassDocsAndCartage DocsAndCartage;

		protected override void SetUp()
		{
			Shipment = Factory.New<GatePassShipment>();
			DocsAndCartage = Shipment.DocsAndCartage;
		}

		#endregion

	}
}
