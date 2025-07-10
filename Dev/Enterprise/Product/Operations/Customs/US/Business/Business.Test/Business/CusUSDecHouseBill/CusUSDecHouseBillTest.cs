using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusUSDecHouseBill))]
	sealed class CusUSDecHouseBillTest : IAddInfoChildUniqueIndexFailureHandlerSupporterTestCase<CusUSDecHouseBill>
	{
		public void TestIsSavedByFactory()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_HouseBill = "HouseBill1";
			var houseBill = dec.PrimaryHouseBill;
			var usBill = houseBill.USBill;
			Assert("can be saved properly", usBill.IsSavedByFactory);

			dec.MakeNonPersistent();
			Assert("return false if it's parent Bill cannot be saved by factory", !usBill.IsSavedByFactory);
		}

		protected override string ExpectedUniqueIndexName => ZArchitecture.Schema.CusUSDecHouseBillSchema.Constants.Indexes.FK_UX__USB_CU;

		protected override EnterpriseBusinessObject GetParent(CusUSDecHouseBill bizObj) => bizObj.Bill;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			return bill.USBill;
		}
	}
}
