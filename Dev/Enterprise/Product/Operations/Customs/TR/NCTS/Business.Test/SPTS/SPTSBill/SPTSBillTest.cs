using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(SPTSBill))]
	public class SPTSBillTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(Universal.CodeDescriptionPairLists.YesNoList.Codes.No, bill.B0_ServiceType);
		}

		protected override BusinessObject GetNewBusinessObject() => bill;

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<SPTSHeader>();
			bill = header.Bills.AddNew();
		}
		SPTSBill bill;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => bill;
	}
}
