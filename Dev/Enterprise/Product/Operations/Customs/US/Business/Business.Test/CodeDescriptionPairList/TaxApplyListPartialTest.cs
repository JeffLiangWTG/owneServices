namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TaxApplyListTest : NUnit.Framework.TestCase
	{
		public void TestIsTaxApplicable()
		{
			Assert(TaxApplyList.IsTaxApplicable(TaxApplyList.Codes.Override));
			Assert(!TaxApplyList.IsTaxApplicable(TaxApplyList.Codes.No));
			Assert(TaxApplyList.IsTaxApplicable(TaxApplyList.Codes.Yes));
		}
	}
}
