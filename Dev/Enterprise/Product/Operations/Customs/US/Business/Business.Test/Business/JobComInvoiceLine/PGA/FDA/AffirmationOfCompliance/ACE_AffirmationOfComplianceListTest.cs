using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class ACE_AffirmationOfComplianceListTest : TestCaseWithFactory
	{
		public void TestCanBeSentWithPNS()
		{
			AssertEquals(true, ACE_AffirmationOfComplianceList.CanBeSentWithPNS(ACE_AffirmationOfComplianceList.Codes.SFR));
			AssertEquals(true, ACE_AffirmationOfComplianceList.CanBeSentWithPNS(ACE_AffirmationOfComplianceList.Codes.UFR));
			AssertEquals(true, ACE_AffirmationOfComplianceList.CanBeSentWithPNS(ACE_AffirmationOfComplianceList.Codes.IFR));
			AssertEquals(true, ACE_AffirmationOfComplianceList.CanBeSentWithPNS(ACE_AffirmationOfComplianceList.Codes.TFR));
			AssertEquals(true, ACE_AffirmationOfComplianceList.CanBeSentWithPNS(ACE_AffirmationOfComplianceList.Codes.ORN));
			AssertEquals(true, ACE_AffirmationOfComplianceList.CanBeSentWithPNS(ACE_AffirmationOfComplianceList.Codes.SRN));
			AssertEquals(true, ACE_AffirmationOfComplianceList.CanBeSentWithPNS(ACE_AffirmationOfComplianceList.Codes.CFR));
			AssertEquals(true, ACE_AffirmationOfComplianceList.CanBeSentWithPNS(ACE_AffirmationOfComplianceList.Codes.GFR));
			AssertEquals(true, ACE_AffirmationOfComplianceList.CanBeSentWithPNS(ACE_AffirmationOfComplianceList.Codes.LFR));
			AssertEquals(true, ACE_AffirmationOfComplianceList.CanBeSentWithPNS(ACE_AffirmationOfComplianceList.Codes.RNO));
			AssertEquals(true, ACE_AffirmationOfComplianceList.CanBeSentWithPNS(ACE_AffirmationOfComplianceList.Codes.CAN));
			AssertEquals(true, ACE_AffirmationOfComplianceList.CanBeSentWithPNS(ACE_AffirmationOfComplianceList.Codes.VFT));
			AssertEquals(true, ACE_AffirmationOfComplianceList.CanBeSentWithPNS(ACE_AffirmationOfComplianceList.Codes.VES));
			AssertEquals(true, ACE_AffirmationOfComplianceList.CanBeSentWithPNS(ACE_AffirmationOfComplianceList.Codes.PFR));
			AssertEquals(true, ACE_AffirmationOfComplianceList.CanBeSentWithPNS(ACE_AffirmationOfComplianceList.Codes.FME));

			// check some arbitrary values to make sure that CanBeSentWithPNS is not always true
			AssertEquals(false, ACE_AffirmationOfComplianceList.CanBeSentWithPNS(ACE_AffirmationOfComplianceList.Codes.KIT));
			AssertEquals(false, ACE_AffirmationOfComplianceList.CanBeSentWithPNS(ACE_AffirmationOfComplianceList.Codes.LWC));
		}
	}
}
