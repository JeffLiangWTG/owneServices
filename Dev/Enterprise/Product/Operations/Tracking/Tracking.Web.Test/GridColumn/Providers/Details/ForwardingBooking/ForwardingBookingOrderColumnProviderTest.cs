using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ForwardingBookingOrderColumnProvider))]
	sealed class ForwardingBookingOrderColumnProviderTest : GridColumnProviderTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZHyperLinkColumn("Order Number", Order.Schema.JD_OrderNumberAndSplit)
			{
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.OrderDetailsPage + "?Ref={0}", // Redirection path
				DataNavigateUrlFields = new string[1] { "PK" },
				ColumnKey = WebTracker.Grids.TrackingOrders.OrderNumber
			});

			AddDefaultsColumn(new ZDateTimeColumn("Date", Order.Schema.JD_OrderDate)
			{
				ColumnKey = WebTracker.Grids.TrackingOrders.OrderDate,
				ReadOnly = true
			});

			AddDefaultsColumn(new ZTextEditColumn("Goods Description", Order.Schema.JD_OrderGoodsDescription)
			{
				ColumnKey = WebTracker.Grids.TrackingOrders.GoodsDescription,
				ReadOnly = true
			});
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new ForwardingBookingOrderColumnProvider();
		}
	}
}
