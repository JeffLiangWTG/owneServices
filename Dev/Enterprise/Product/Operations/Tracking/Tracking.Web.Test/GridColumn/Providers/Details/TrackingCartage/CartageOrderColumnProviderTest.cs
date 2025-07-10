using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(CartageOrderColumnProvider))]
	sealed class CartageOrderColumnProviderTest : GridColumnProviderTest
	{
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZHyperLinkColumn("Order #", TrackingOrderLine.Schema.OrderNumber)
			{
				ColumnKey = WebTracker.Grids.TrackingOrderLines.OrderNumber,
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.OrderDetailsPage + "?Ref={0}",
				DataNavigateUrlFields = new string[] { TrackingOrderLine.Schema.OrderPK }
			});

			AddDefaultsColumn(new ZHyperLinkColumn("Shipment #", TrackingOrderLine.Schema.ShipmentNumber)
			{
				ColumnKey = WebTracker.Grids.TrackingOrderLines.ShipmentNumber,
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.ShipmentDetailsPage + "?Ref={0}",
				DataNavigateUrlFields = new string[] { TrackingOrderLine.Schema.ShipmentPK }
			});

			AddDefaultsColumn(new ZTextEditColumn("House Bill", TrackingOrderLine.Schema.HouseBillNumber) { ColumnKey = WebTracker.Grids.TrackingOrderLines.HouseBill });
			AddDefaultsColumn(new ZTextEditColumn("Supplier", TrackingOrderLine.Schema.SupplierName) { ColumnKey = WebTracker.Grids.TrackingOrderLines.Supplier });
			AddDefaultsColumn(new ZTextEditColumn("Product", TrackingOrderLine.Schema.ProductNumber) { ColumnKey = WebTracker.Grids.TrackingOrderLines.ProductNumber });
			AddDefaultsColumn(new ZTextEditColumn("Supplier Part", TrackingOrderLine.Schema.CustomAttribute1) { ColumnKey = WebTracker.Grids.TrackingOrderLines.CustomAttribute1 });
			AddDefaultsColumn(new ZTextEditColumn("Description", TrackingOrderLine.Schema.Description) { ColumnKey = WebTracker.Grids.TrackingOrderLines.Description });
			AddDefaultsColumn(new ZCalcEditColumn("Container Qty", TrackingOrderLine.Schema.ContainerQuantity) { ColumnKey = WebTracker.Grids.TrackingOrderLines.ContainerQuantity });
			AddDefaultsColumn(new ZCalcEditColumn("Packs", TrackingOrderLine.Schema.Packs) { ColumnKey = WebTracker.Grids.TrackingOrderLines.Packs });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new CartageOrderColumnProvider();
		}
	}
}
