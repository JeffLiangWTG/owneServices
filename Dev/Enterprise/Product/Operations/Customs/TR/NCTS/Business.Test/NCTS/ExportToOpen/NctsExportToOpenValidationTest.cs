using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class NctsExportToOpenValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber()
		{
			exportToOpen.CSI_Procedure = "E";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(exportToOpen.CSI_ReferenceNumberInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bills = header.MovementHeader.Header.Bills.AddNew();
			var goodsitems = bills.GoodsItems.AddNew();
			exportToOpen = goodsitems.ExportToOpenList.AddNew();
		}
		NctsExportToOpen exportToOpen;
	}
}
