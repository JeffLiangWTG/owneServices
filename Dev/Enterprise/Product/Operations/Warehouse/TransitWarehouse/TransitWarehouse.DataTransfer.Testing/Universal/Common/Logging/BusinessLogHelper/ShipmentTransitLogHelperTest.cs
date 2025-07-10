using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using static Enterprise.Warehouse.Transit.Business.TransitLogColumnIDs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class ShipmentTransitLogHelperTest : TransitLogTableHelperTest<UniversalShipment, ShipmentColumn>
	{
		public override void TestGetColumns()
		{
			var tableHelper = GetTableHelper();
			var shipmentWithDataSource = new UniversalShipment();
			shipmentWithDataSource.DataContext = DataContextFactory.New();
			shipmentWithDataSource.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C1");
			shipmentWithDataSource.DataContext.AddDataTarget(DataContextType.TransitReceive, "R1");
			var sourceType = shipmentWithDataSource.DataContext.DataSourceCollection.Single().Type;
			var shipmentWithDataTarget = new UniversalShipment();
			shipmentWithDataTarget.DataContext = DataContextFactory.New();
			shipmentWithDataTarget.DataContext.AddDataTarget(DataContextType.TransitReceive, "R1");
			var targetType = shipmentWithDataTarget.DataContext.DataTargetCollection.Single().Type;
			var shipmentWithNoDataContext = new UniversalShipment();

			var columnType = tableHelper.GetColumn(new UniversalShipment[] { shipmentWithDataSource, shipmentWithDataTarget, shipmentWithNoDataContext }, TransitLogColumnIDs.ShipmentColumn.Source);
			var columnKey = tableHelper.GetColumn(new UniversalShipment[] { shipmentWithDataSource, shipmentWithDataTarget, shipmentWithNoDataContext }, TransitLogColumnIDs.ShipmentColumn.Target);

			AssertEquals("Data Source", columnType.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { $"{sourceType.Value} - C1", ZString.Empty, ZString.Empty }, columnType.Values);
			AssertEquals("Data Target", columnKey.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { $"{targetType.Value} - R1", $"{targetType.Value} - R1", ZString.Empty }, columnKey.Values);
		}

		protected override ZString GetDefaultTitle() => "Data Source";

		protected override ShipmentColumn GetDefaultColumn() => ShipmentColumn.Source;

		protected override TransitLogTableHelper<UniversalShipment, ShipmentColumn> GetTableHelper() => new ShipmentTransitLogHelper();

		protected override (UniversalShipment BusinessObject, string expectedDisplayId) CreateTestBO(int id)
		{
			var source = "C" + id.ToString().PadLeft(7, '0');
			var receive = "R" + id.ToString().PadLeft(7, '0');
			var shipmentWithDataSource = new UniversalShipment();
			shipmentWithDataSource.DataContext = DataContextFactory.New();
			shipmentWithDataSource.DataContext.AddDataSource(DataContextType.ForwardingConsol, source);
			shipmentWithDataSource.DataContext.AddDataTarget(DataContextType.TransitReceive, receive);
			var sourceType = shipmentWithDataSource.DataContext.DataSourceCollection.Single().Type;
			return (shipmentWithDataSource, $"{sourceType.Value} - {source}");
		}
	}
}
