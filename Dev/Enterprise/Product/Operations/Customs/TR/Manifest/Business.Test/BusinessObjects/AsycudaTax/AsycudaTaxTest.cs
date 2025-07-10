using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaTax))]
	public class AsycudaTaxTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var tax = (AsycudaTax)GetNewBusinessObjectForDeleteTest(Factory);
			AssertEquals(tax.AET_MethodOfCalculation, "%");
		}

		public void TestDeleteWhenAET_ChargeAmountValueIsZero()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var asycudaTax = bill.AsycudaTaxes.AddNew();
			asycudaTax.AET_MethodOfCalculation = "%";
			asycudaTax.AET_ChargeAmount = 20;
			AssertEquals("AsycudaTaxes should contain 1 records", 1, bill.AsycudaTaxes.Count);
			asycudaTax.AET_ChargeAmount = 0;
			Factory.Save();
			AssertEquals("AsycudaTaxes should contain 0 records", 0, bill.AsycudaTaxes.Count);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var asycudaTax = bill.AsycudaTaxes.AddNew();
			return asycudaTax;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
		}
	}
}
