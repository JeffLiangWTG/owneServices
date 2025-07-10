namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TermsOfDeliveryListTest : NUnit.Framework.TestCase
	{
		public void TestIsApplyForAgreedPlace()
		{
			Assert(TermsOfDeliveryList.IsApplyForAgreedPlace(TermsOfDeliveryList.Codes.CIP));
			Assert(TermsOfDeliveryList.IsApplyForAgreedPlace(TermsOfDeliveryList.Codes.CPT));
			Assert(!TermsOfDeliveryList.IsApplyForAgreedPlace(TermsOfDeliveryList.Codes.CAI));
		}
	}
}
