using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class WarehouseOrderLinesColumnProvider : LineColumnProvider
	{
		public WarehouseOrderLinesColumnProvider()
		{
		}

		public WarehouseOrderLinesColumnProvider(TrackingWhsOrder order, string showInPopupParam)
		{
			this.order = order;
			this.showInPopupParam = showInPopupParam;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			if (SiteUser == null || SiteUser.IsShipmentQuickViewUser)
			{
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("0e2d7120-a437-466e-93a7-44c0bf36e8fc", "Product"), TrackingWhsOrderLine.WrapperSchema.ProductCode) { ColumnKey = WebTracker.Grids.WarehouseDocketLine.Product });
			}
			else
			{
				AddToDictionaryAsDefault(new ZHyperLinkColumn(Res.GetString("0e2d7120-a437-466e-93a7-44c0bf36e8fc", "Product"), TrackingWhsOrderLine.WrapperSchema.ProductCode)
				{
					ColumnKey = WebTracker.Grids.WarehouseDocketLine.Product,
					DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.ProductProfileDetailsPage) + "?" + ZPage.RefParameterName + "={0}",
					DataNavigateUrlFields = new string[1] { TrackingWhsOrderLine.WrapperSchema.WE_OP }
				});
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("ca7969d6-41c0-4398-a7b2-0596ca744c38", "Description"), TrackingWhsOrderLine.WrapperSchema.ProductDescription));
			}

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsOrderLine)null).WhsOrderLine.WE_PackQuantity);
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("f1f32dc9-03c7-4591-92dd-5a6acc41a2a7", "Packs"), TrackingWhsOrderLine.WrapperSchema.WE_PackQuantity)
			{
				ColumnKey = WebTracker.Grids.WarehouseDocketLine.Packs,
				AutoPostBack = true
			});

			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsOrderLine)null).WhsOrderLine.WE_F3_NKPackType);
			ZBindToChecker.CheckBindTo((CodeDescriptionPairList)((TrackingWhsOrderLine)null).WhsOrderLine.Lookups.PackTypes);
			AddToDictionaryAsDefault(new ZDropDownListColumn(Res.GetString("4146d62f-aadf-435f-ac8b-0e897a9947cc", "Packs UQ"), TrackingWhsOrderLine.WrapperSchema.WE_F3_NKPackType, TrackingWhsOrderLine.GetSchemaPath("Lookups.PackTypes"))
			{
				ColumnKey = WebTracker.Grids.WarehouseDocketLine.PackTypes,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			ZBindToChecker.CheckBindTo((ZByte)((TrackingWhsOrderLine)null).WhsOrderLine.SupplierPart.OP_CountDecimalPlaces);
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsOrderLine)null).WhsOrderLine.WE_TransactionQuantity);
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("a699b189-a7d4-4461-b4d3-a319f546af00", "Qty Ordered"), TrackingWhsOrderLine.WrapperSchema.WE_TransactionQuantity)
			{
				ColumnKey = WebTracker.Grids.WarehouseDocketLine.OrderedQuantity,
				BindToDecimals = TrackingWhsOrderLine.GetSchemaPath("SupplierPart+OP_CountDecimalPlaces")
			});

			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsOrderLine)null).WhsOrderLine.ProductUQ);
			ZBindToChecker.CheckBindTo((CodeDescriptionPairList)((TrackingWhsOrderLine)null).WhsOrderLine.Lookups.PackTypes);
			AddToDictionaryAsDefault(new ZDropDownListColumn(Res.GetString("9ae9196c-b02b-4a6d-921a-85206ca9f24f", "UQ"), TrackingWhsOrderLine.WrapperSchema.ProductUQ, TrackingWhsOrderLine.GetSchemaPath("Lookups.PackTypes"))
			{
				ColumnKey = WebTracker.Grids.WarehouseDocketLine.UQ,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddToDictionaryAsDefault(new ZHyperLinkColumn(Res.GetString("6aa577d1-fe5e-4e1c-846a-a6fd160b0994", "Reserved"), "")
			{
				ColumnKey = WebTracker.Grids.WarehouseDocketLine.Reserved,
				DataTextFormatString = Res.GetString("13e91ab3-6e75-4093-96f1-5a05f4a8d62f", "{0} View", "{0}"),
				DataTextFields = new string[] { TrackingWhsOrderLine.WrapperSchema.AllocatedQuantityAsString },
				DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.WarehouseOrderLineAllocationPage) + "?" + ZPage.RefParameterName + "={0}" + ShowInPopupParam,
				DataNavigateUrlFields = new string[] { TrackingWhsOrderLine.WrapperSchema.WE_PK },
				Target = (NoResString)"_blank", // javascript code
				WindowStyle = Global.WarehouseOrderLineAllocationPopupWindowStyle
			});
			if (Order != null)
			{
				AddLineAttributeColumns(AttributeManager.AttributeModules.Warehouse, Order.WhsOrder.Client, ZString.Empty);
			}
		}

		protected TrackingWhsOrder Order
		{
			get { return order; }
		}

		protected string ShowInPopupParam
		{
			get { return showInPopupParam; }
		}

		readonly string showInPopupParam;
		readonly TrackingWhsOrder order;
	}
}
