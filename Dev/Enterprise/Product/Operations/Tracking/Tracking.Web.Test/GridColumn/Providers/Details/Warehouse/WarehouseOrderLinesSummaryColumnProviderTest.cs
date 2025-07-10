using System.Collections.Generic;
using System.Web.UI.WebControls;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(WarehouseOrderLinesSummaryColumnProvider))]
	[HttpContextEnabledTest]
	sealed class WarehouseOrderLinesSummaryColumnProviderTest : GridColumnProviderTest
	{
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			if (SiteUser == null || SiteUser.IsShipmentQuickViewUser)
			{
				AddDefaultsColumn(new ZTextEditColumn("Product", TrackingWhsOrderSummaryLine.Schema.ProductCode) { ColumnKey = WebTracker.Grids.WarehouseDocketLine.Product });
			}
			else
			{
				AddDefaultsColumn(new ZHyperLinkColumn("Product", TrackingWhsOrderSummaryLine.Schema.ProductCode)
				{
					ColumnKey = WebTracker.Grids.WarehouseDocketLine.Product,
					DataNavigateUrlFormatString = TrackingConstants.RelativePath.ProductProfileDetailsPage + "?Ref={0}",
					DataNavigateUrlFields = new string[1] { TrackingWhsOrderSummaryLine.Schema.ProductPK }
				});
				AddDefaultsColumn(new ZTextEditColumn("Description", TrackingWhsOrderSummaryLine.Schema.ProductDescription) { ColumnKey = WebTracker.Grids.WarehouseDocketLine.Description });
			}
			AddDefaultsColumn(new ZGroupColumn("Packs", new DataGridColumn[]
			{
				new ZCalcEditColumn("Packs", TrackingWhsOrderSummaryLine.Schema.PacksQuantity),
				new ZDropDownListColumn("Packs UQ", TrackingWhsOrderSummaryLine.Schema.PacksUQ, "PackTypes") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly }
			})
			{ ColumnKey = WebTracker.Grids.WarehouseDocketLine.Packs });
			AddDefaultsColumn(new ZCalcEditColumn("Qty Ordered", TrackingWhsOrderSummaryLine.Schema.OrderedQuantity)
			{
				ColumnKey = WebTracker.Grids.WarehouseDocketLine.OrderedQuantity,
				BindToDecimals = "SupplierPart+OP_CountDecimalPlaces"
			});
			AddDefaultsColumn(new ZDropDownListColumn("UQ", TrackingWhsOrderSummaryLine.Schema.UQ, "PackTypes")
			{
				ColumnKey = WebTracker.Grids.WarehouseDocketLine.UQ,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});
			AddDefaultsColumn(new ZCalcEditColumn("Reserved", TrackingWhsOrderSummaryLine.Schema.ReservedQuantity)
			{
				ColumnKey = WebTracker.Grids.WarehouseDocketLine.Reserved,
				BindToDecimals = "SupplierPart+OP_CountDecimalPlaces"
			});
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.WarehouseDocketLine.Packs
		};

		TrackingSiteUser SiteUser
		{
			get { return WebEnv.AppInstance != null ? WebEnv.AppInstance.SiteUser as TrackingSiteUser : null; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new WarehouseOrderLinesSummaryColumnProvider();
		}
	}
}
