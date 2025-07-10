using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class WarehouseOrderLinesSummaryColumnProvider : GridColumnProvider
	{
		public WarehouseOrderLinesSummaryColumnProvider()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			if (SiteUser == null || SiteUser.IsShipmentQuickViewUser)
			{
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("6e3cedc7-650f-4e91-b346-d86570a3dd3f", "Product"), TrackingWhsOrderSummaryLine.Schema.ProductCode) { ColumnKey = WebTracker.Grids.WarehouseDocketLine.Product });
			}
			else
			{
				AddToDictionaryAsDefault(new ZHyperLinkColumn(Res.GetString("6e3cedc7-650f-4e91-b346-d86570a3dd3f", "Product"), TrackingWhsOrderSummaryLine.Schema.ProductCode)
				{
					ColumnKey = WebTracker.Grids.WarehouseDocketLine.Product,
					DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.ProductProfileDetailsPage) + "?" + ZPage.RefParameterName + "={0}",
					DataNavigateUrlFields = new string[1] { TrackingWhsOrderSummaryLine.Schema.ProductPK }
				});
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("f0521da1-5cca-4580-8b76-4cb5469d5e99", "Description"), TrackingWhsOrderSummaryLine.Schema.ProductDescription) { ColumnKey = WebTracker.Grids.WarehouseDocketLine.Description });
			}

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsOrderSummaryLine)null).PacksQuantity);
			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsOrderSummaryLine)null).PacksUQ);
			ZBindToChecker.CheckBindTo((CodeDescriptionPairList)((TrackingWhsOrderSummaryLine)null).PackTypes);
			AddToDictionaryAsDefault(new ZGroupColumn(Res.GetString("02552f28-5d28-4c01-bafa-b9af57d7f9e9", "Packs"), new DataGridColumn[]
			{
				new ZCalcEditColumn(Res.GetString("02552f28-5d28-4c01-bafa-b9af57d7f9e9", "Packs"), TrackingWhsOrderSummaryLine.Schema.PacksQuantity),
				new ZDropDownListColumn(Res.GetString("7f758fbf-b11f-4e12-a2bd-f69a06ba7714", "Packs UQ"), TrackingWhsOrderSummaryLine.Schema.PacksUQ, "PackTypes") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly }
			})
			{ ColumnKey = WebTracker.Grids.WarehouseDocketLine.Packs });

			ZBindToChecker.CheckBindTo((ZByte)((TrackingWhsOrderSummaryLine)null).SupplierPart.OP_CountDecimalPlaces);
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsOrderSummaryLine)null).OrderedQuantity);
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("2f4434bd-42de-48c3-ad91-fde8e282288a", "Qty Ordered"), TrackingWhsOrderSummaryLine.Schema.OrderedQuantity)
			{
				ColumnKey = WebTracker.Grids.WarehouseDocketLine.OrderedQuantity,
				BindToDecimals = "SupplierPart+OP_CountDecimalPlaces"
			});

			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsOrderSummaryLine)null).UQ);
			ZBindToChecker.CheckBindTo((CodeDescriptionPairList)((TrackingWhsOrderSummaryLine)null).PackTypes);
			AddToDictionaryAsDefault(new ZDropDownListColumn(Res.GetString("4ada843d-acf9-4230-955f-7e5a93655830", "UQ"), TrackingWhsOrderSummaryLine.Schema.UQ, "PackTypes")
			{
				ColumnKey = WebTracker.Grids.WarehouseDocketLine.UQ,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			ZBindToChecker.CheckBindTo((ZByte)((TrackingWhsOrderSummaryLine)null).SupplierPart.OP_CountDecimalPlaces);
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsOrderSummaryLine)null).ReservedQuantity);
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("317d8f62-d14b-4db6-a960-96831de50787", "Reserved"), TrackingWhsOrderSummaryLine.Schema.ReservedQuantity)
			{
				ColumnKey = WebTracker.Grids.WarehouseDocketLine.Reserved,
				BindToDecimals = "SupplierPart+OP_CountDecimalPlaces"
			});
		}

		protected TrackingSiteUser SiteUser
		{
			get { return WebEnv.AppInstance != null ? WebEnv.AppInstance.SiteUser as TrackingSiteUser : null; }
		}
	}
}
