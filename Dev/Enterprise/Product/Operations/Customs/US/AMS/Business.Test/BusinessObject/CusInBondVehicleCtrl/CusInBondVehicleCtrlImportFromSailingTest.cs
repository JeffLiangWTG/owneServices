using System.Linq;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class CusInBondVehicleCtrlImportFromSailingTest : LinkedSailingBillsImportedTest
	{
		public void TestImporting()
		{
			header.ImportBillsOfLadingLinkedToTheSameSailing(new BillImportActionCollection(header.Bills));
			var vehicle = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "BBB").MovementDetail.Containers[0].Vehicles[0];
			AssertEquals("VIN", vehicle.BV_VIN);
		}
	}
}
