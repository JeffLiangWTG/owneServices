using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class CartageOrderColumnProvider : GridColumnProvider
	{
		public CartageOrderColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZHyperLinkColumn(Res.GetString("6a94008e-7c43-420f-8b41-16459446945d", "Order #"), TrackingOrderLine.Schema.OrderNumber)
			{
				ColumnKey = WebTracker.Grids.TrackingOrderLines.OrderNumber,
				DataNavigateUrlFormatString = UrlFormatWithAppRoot(TrackingConstants.RelativePath.OrderDetailsPage) + "?" + ZPage.RefParameterName + "={0}",
				DataNavigateUrlFields = new string[] { TrackingOrderLine.Schema.OrderPK }
			});

			AddToDictionaryAsDefault(new ZHyperLinkColumn(Res.GetString("5ada1398-5add-4084-837c-f275baff20f2", "Shipment #"), TrackingOrderLine.Schema.ShipmentNumber)
			{
				ColumnKey = WebTracker.Grids.TrackingOrderLines.ShipmentNumber,
				DataNavigateUrlFormatString = UrlFormatWithAppRoot(TrackingConstants.RelativePath.ShipmentDetailsPage) + "?" + ZPage.RefParameterName + "={0}",
				DataNavigateUrlFields = new string[] { TrackingOrderLine.Schema.ShipmentPK }
			});

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("ac03544a-9347-423c-bd3d-edc8237ef41d", "House Bill"), TrackingOrderLine.Schema.HouseBillNumber) { ColumnKey = WebTracker.Grids.TrackingOrderLines.HouseBill });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("be1592b4-a311-4ee6-9557-eea23bf7cf65", "Supplier"), TrackingOrderLine.Schema.SupplierName) { ColumnKey = WebTracker.Grids.TrackingOrderLines.Supplier });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("73d3bdce-04c0-48b5-8449-a1826cee6625", "Product"), TrackingOrderLine.Schema.ProductNumber) { ColumnKey = WebTracker.Grids.TrackingOrderLines.ProductNumber });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("1a315ece-dd20-4d47-8cd7-47acfa3d7642", "Supplier Part"), TrackingOrderLine.Schema.CustomAttribute1) { ColumnKey = WebTracker.Grids.TrackingOrderLines.CustomAttribute1 });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("e49c7837-b084-4024-9f83-f83a12aedc6c", "Description"), TrackingOrderLine.Schema.Description) { ColumnKey = WebTracker.Grids.TrackingOrderLines.Description });
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("830c8d22-be72-443c-9b26-6e0ad356bc0f", "Container Qty"), TrackingOrderLine.Schema.ContainerQuantity) { ColumnKey = WebTracker.Grids.TrackingOrderLines.ContainerQuantity });
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("2e60bf8b-d9bd-4153-bf59-c72df49ce1f3", "Packs"), TrackingOrderLine.Schema.Packs) { ColumnKey = WebTracker.Grids.TrackingOrderLines.Packs });
		}
	}
}
