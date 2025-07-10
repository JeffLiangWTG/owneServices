using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMReasonForAmendmentTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var aimReasonForAmendment = new AIMReasonForAmendment("01", "for somehow");
			AssertEquals("01", aimReasonForAmendment.AmendmentCode);
			AssertEquals("for somehow", aimReasonForAmendment.AmendmentExplanation);
		}

		public void TestSetAmendmentReason()
		{
			var aimReasonForAmendment = new AIMReasonForAmendment();
			AssertEquals("", aimReasonForAmendment.AmendmentCode);
			AssertEquals("", aimReasonForAmendment.AmendmentExplanation);
			aimReasonForAmendment.SetAmendmentReason("01", "for somehow");
			AssertEquals("01", aimReasonForAmendment.AmendmentCode);
			AssertEquals("for somehow", aimReasonForAmendment.AmendmentExplanation);
		}
	}
}
