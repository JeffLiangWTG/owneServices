using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(GroupInvoiceCharge))]
	sealed class GroupInvoiceChargeTest : Customs.Business.Testing.BaseGroupInvoiceChargeTest
	{
		public void TestJ7_PercentageCaption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(charge.J7_PercentageInfo);
			AssertEquals("% of Invoice Amount", resourceStringData.Caption);
			AssertEquals("% of Amount", resourceStringData.ShortCaption);
		}

		protected override void SetUp()
		{
			base.SetUp();
			charge = (GroupInvoiceCharge)GetNewBusinessObject();
		}
		GroupInvoiceCharge charge;
	}
}
