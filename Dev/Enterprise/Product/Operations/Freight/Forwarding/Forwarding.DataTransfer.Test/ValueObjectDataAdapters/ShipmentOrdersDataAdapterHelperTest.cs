using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ShipmentOrdersDataAdapterHelperTest : TestCaseWithFactory
	{
		#region Tests

		public void TestImportArgs()
		{
			var helper = new ShipmentOrdersDataAdapterHelper(null);

			AssertNoExceptionThrown(() =>
				{
					helper.ImportOrdersAndOrderReferences(null);
					helper.ImportOrdersAndOrderReferences(new ShipmentOrdersDataAdapterHelper.ImportArgs());
				});

			ShipmentOrdersDataAdapterHelper.ImportArgs args = new ShipmentOrdersDataAdapterHelper.ImportArgs();
			AssertEquals(false, args.IsValid);

			args.Shipment = Factory.New<ForwardingShipment>();
			AssertEquals(false, args.IsValid);

			args.ShipmentValue = new Xsd.Shipment();
			AssertEquals(false, args.IsValid);

			args.Context = new ValueObjectImportContext(Factory, Notify);
			AssertEquals(true, args.IsValid);
		}

		public void TestExportArgs()
		{
			var helper = new ShipmentOrdersDataAdapterHelper(null);

			AssertNoExceptionThrown(() =>
			{
				helper.ExportOrdersAndOrderReferences(null);
				helper.ExportOrdersAndOrderReferences(new ShipmentOrdersDataAdapterHelper.ExportArgs());
			});

			ShipmentOrdersDataAdapterHelper.ExportArgs args = new ShipmentOrdersDataAdapterHelper.ExportArgs();
			AssertEquals(false, args.IsValid);

			args.Shipment = Factory.New<ForwardingShipment>();
			AssertEquals(false, args.IsValid);

			args.ShipmentValue = new Xsd.Shipment();
			AssertEquals(false, args.IsValid);

			args.Context = new ValueObjectExportContext(Notify);
			AssertEquals(true, args.IsValid);
		}

		#endregion

		#region Implementation

		NotificationBuffer Notify
		{
			get
			{
				if (notify == null)
				{
					notify = new NotificationBuffer();
				}
				return notify;
			}
		}
		NotificationBuffer notify;

		#endregion
	}
}
