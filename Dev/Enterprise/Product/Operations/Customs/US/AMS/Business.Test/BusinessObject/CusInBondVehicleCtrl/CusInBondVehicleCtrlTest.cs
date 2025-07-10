using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondVehicleCtrl))]
	sealed class CusInBondVehicleCtrlTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var bill = header.Bills.AddNew();
			bill.B0_BH = header.PK;
			moveDetail.B9_B0 = bill.PK;
			var container = moveDetail.Containers.AddNew();
			return container.Vehicles.AddNew();
		}
	}
}
