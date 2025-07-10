using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using static Enterprise.Warehouse.Transit.Business.TransitLogColumnIDs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class DispatchTransportationUnitTransitLogHelperTest : TransitLogTableHelperTest<WhsItemDispatchTransportationUnit, DTUColumn>
	{
		public override void TestGetColumns()
		{
			var tableHelper = GetTableHelper();
			var dtu1 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU0000001", TestWarehouse.PK, containerID: "CNT1");
			var dtu2 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU0000002", TestWarehouse.PK, containerID: ZString.Empty);
			dtu2.PackageJob.Packages.ToArray()[0].KP_PackageQty = 2;
			var dtu3 = Helper.CreateDispatchTransportationUnit("DTU0000003", TestWarehouse.PK, vehicleRef: "DTU3");
			var dtu4 = Helper.CreateDispatchTransportationUnit("DTU0000004", TestWarehouse.PK, vehicleRef: ZString.Empty);
			Factory.Save();

			var column = tableHelper.GetColumn(new WhsItemDispatchTransportationUnit[] { dtu1, dtu2, dtu3, dtu4 }, TransitLogColumnIDs.DTUColumn.DTU);

			AssertEquals("DTU", column.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "CNT1 (DTU0000001)", "20GP (DTU0000002)", "DTU3 (DTU0000003)", "(DTU0000004)" }, column.Values);
		}

		protected override DTUColumn GetDefaultColumn() => DTUColumn.DTU;

		protected override TransitLogTableHelper<WhsItemDispatchTransportationUnit, DTUColumn> GetTableHelper() => new DispatchTransportationUnitTransitLogHelper();

		protected override (WhsItemDispatchTransportationUnit BusinessObject, string expectedDisplayId) CreateTestBO(int id)
		{
			var dtuId = "DTU" + id.ToString().PadLeft(7, '0');
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType(dtuId, TestWarehouse.PK, containerID: "CNT1");
			return (dtu, $"CNT1 ({dtuId})");
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestWarehouse = Helper.CreateTRWWarehouse("WH1");
		}

		WhsWarehouse TestWarehouse { get; set; }
	}
}
