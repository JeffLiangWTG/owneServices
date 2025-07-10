using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using static Enterprise.Warehouse.Transit.Business.TransitLogColumnIDs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class ReceiveTransportationUnitTransitLogHelperTest : TransitLogTableHelperTest<WhsItemReceiveTransportationUnit, RTUColumn>
	{
		public override void TestGetColumns()
		{
			var tableHelper = GetTableHelper();
			var rtu1 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU0000001", TestWarehouse.PK, TestStagingLocation.PK, containerID: "CNT1", vehicleRef: "CNT1");
			var rtu2 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU0000002", TestWarehouse.PK, TestStagingLocation.PK, containerID: ZString.Empty, containerType: "20GP", vehicleRef: ZString.Empty);
			rtu2.PackageJob.Packages.ToArray()[0].KP_PackageQty = 2;
			var rtu3 = Helper.CreateReceiveTransportationUnit("RTU0000003", TestWarehouse.PK, TestStagingLocation.PK, vehicleRef: "VEH1");
			var rtu4 = Helper.CreateReceiveTransportationUnit("RTU0000004", TestWarehouse.PK, ZGuid.Empty, vehicleRef: ZString.Empty);
			Factory.Save();

			var column = tableHelper.GetColumn(new WhsItemReceiveTransportationUnit[] { rtu1, rtu2, rtu3, rtu4 }, RTUColumn.RTU);

			AssertEquals("RTU", column.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "CNT1 (RTU0000001)", "20GP (RTU0000002)", "VEH1 (RTU0000003)", "(RTU0000004)" }, column.Values);
		}

		protected override RTUColumn GetDefaultColumn() => RTUColumn.RTU;

		protected override TransitLogTableHelper<WhsItemReceiveTransportationUnit, RTUColumn> GetTableHelper() => new ReceiveTransportationUnitTransitLogHelper();

		protected override (WhsItemReceiveTransportationUnit BusinessObject, string expectedDisplayId) CreateTestBO(int id)
		{
			var rtuId = "RTU" + id.ToString().PadLeft(7, '0');
			var rtu = Helper.CreateReceiveTransportationUnitWithContainerType(rtuId, TestWarehouse.PK, TestStagingLocation.PK, containerID: "CNT1", vehicleRef: "CNT1");
			return (rtu, $"CNT1 ({rtuId})");
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestWarehouse = Helper.CreateTRWWarehouse("WH1");
			TestStagingLocation = Helper.CreateLocation(TestWarehouse);
		}

		WhsLocation TestStagingLocation { get; set; }
		WhsWarehouse TestWarehouse { get; set; }
	}
}
