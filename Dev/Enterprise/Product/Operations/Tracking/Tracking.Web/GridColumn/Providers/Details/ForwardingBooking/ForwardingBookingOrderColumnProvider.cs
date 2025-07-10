using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class ForwardingBookingOrderColumnProvider : GridColumnProvider
	{
		public ForwardingBookingOrderColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZHyperLinkColumn(Res.GetString("9566ba82-26d0-4fc5-8c26-8cc7b1d1f3bd", "Order Number"), Order.Schema.JD_OrderNumberAndSplit)
			{
				DataNavigateUrlFormatString = UrlFormatWithAppRoot(TrackingConstants.RelativePath.OrderDetailsPage) + (NoResString)"?Ref={0}", // Redirection path
				DataNavigateUrlFields = new string[1] { "PK" },
				ColumnKey = WebTracker.Grids.TrackingOrders.OrderNumber
			});

			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("646d52ce-04f1-485a-93a0-43538fa0ea3c", "Date"), Order.Schema.JD_OrderDate)
			{
				ColumnKey = WebTracker.Grids.TrackingOrders.OrderDate,
				ReadOnly = true
			});

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("66009288-879b-471e-84bd-cf5c9663fd9f", "Goods Description"), Order.Schema.JD_OrderGoodsDescription)
			{
				ColumnKey = WebTracker.Grids.TrackingOrders.GoodsDescription,
				ReadOnly = true
			});
		}
	}
}
