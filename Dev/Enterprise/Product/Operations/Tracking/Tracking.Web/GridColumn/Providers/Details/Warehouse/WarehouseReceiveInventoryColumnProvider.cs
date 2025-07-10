using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
	public class WarehouseReceiveInventoryColumnProvider : LineColumnProvider
	{
		public WarehouseReceiveInventoryColumnProvider(TrackingWhsReceive receive, bool allowEdit, string showInPopupParam, bool isShownInPopup)
		{
			Receive = receive;
			AllowEdit = allowEdit;
			IsShownInPopup = isShownInPopup;
			ShowInPopupParam = showInPopupParam;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();

			ZBindToChecker.CheckBindTo((ZShort)((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_LineNo);
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("db1499dd-f38e-44c3-9d9e-25a01bdf63f4", "Line No"), TrackingWhsReceiveLine.WrapperSchema.WE_LineNo)
			{
				ColumnKey = WebTracker.Grids.WarehouseReceiveLine.LineNo,
				AutoPostBack = true
			});

			if (AllowEdit)
			{
				ZBindToChecker.CheckBindTo((ZGuid)((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_OP);
				ZBindToChecker.CheckBindTo((OrgSupplierPartCollection)((TrackingWhsReceiveLine)null).WhsReceiveLine.Lookups.SupplierParts);
				AddToDictionaryAsDefault(new ZFindBoxColumn(Res.GetString("edfbb5c3-664a-4559-8f7c-6a0f7cf789cd", "Product"), TrackingWhsReceiveLine.WrapperSchema.WE_OP, "WhsReceiveLine.Lookups.SupplierParts")
				{
					ColumnKey = WebTracker.Grids.WarehouseReceiveLine.Product,
					ValueFieldName = "PK",
					ModuleID = WebModuleIDs.OrgSupplierPartTracking,
					AutoPostBack = true
				});
			}
			else
			{
				ZBindToChecker.CheckBindTo((ZString)((TrackingWhsReceiveLine)null).WhsReceiveLine.ProductCode);
				var productColumn = new ZHyperLinkColumn(Res.GetString("edfbb5c3-664a-4559-8f7c-6a0f7cf789cd", "Product"), TrackingWhsReceiveLine.WrapperSchema.ProductCode)
				{
					ColumnKey = WebTracker.Grids.WarehouseReceiveLine.Product,
					DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.ProductProfileDetailsPage) + (NoResString)"?Ref={0}" + ShowInPopupParam, // Its string formater
					DataNavigateUrlFields = new string[1] { TrackingWhsReceiveLine.WrapperSchema.WE_OP }
				};
				if (IsShownInPopup)
				{
					productColumn.Target = (NoResString)"_blank"; // May be inside data
					productColumn.WindowStyle = Global.ProductProfilePopupWindowStyle;
				}
				AddToDictionaryAsDefault(productColumn);
			}

			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsReceiveLine)null).WhsReceiveLine.ProductDesc);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("c32d0bc6-4739-412a-88ac-76c17e69c1a0", "Description"), TrackingWhsReceiveLine.WrapperSchema.ProductDesc)
			{
				ColumnKey = WebTracker.Grids.WarehouseReceiveLine.Description,
				ReadOnly = true
			});

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_PackQuantity);
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("a483426c-49a9-4f67-9d61-fba7e35a95a7", "Packs"), TrackingWhsReceiveLine.WrapperSchema.WE_PackQuantity)
			{
				ColumnKey = WebTracker.Grids.WarehouseReceiveLine.Packs,
				AutoPostBack = true
			});

			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_F3_NKPackType);
			ZBindToChecker.CheckBindTo((CodeDescriptionPairList)((TrackingWhsReceiveLine)null).WhsReceiveLine.Lookups.PackTypes);
			AddToDictionaryAsDefault(new ZDropDownListColumn(Res.GetString("5bedd8fc-04f6-4e26-bc96-f92d3af19a86", "Packs UQ"), TrackingWhsReceiveLine.WrapperSchema.WE_F3_NKPackType, "WhsReceiveLine.Lookups.PackTypes")
			{
				ColumnKey = WebTracker.Grids.WarehouseReceiveLine.PackTypes,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				AutoPostBack = true
			});

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_ClientOrderedUnits);
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("4c59b59f-895c-4430-b290-fe04dbf4800b", "Expected Quantity"), TrackingWhsReceiveLine.WrapperSchema.WE_ClientOrderedUnits)
			{
				ColumnKey = WebTracker.Grids.WarehouseReceiveLine.OrderedQuantity,
				BindToDecimals = "WhsReceiveLine.SupplierPart.OP_CountDecimalPlaces",
				AutoPostBack = true
			});

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_TransactionQuantity);
			ZBindToChecker.CheckBindTo((ZByte)((TrackingWhsReceiveLine)null).WhsReceiveLine.SupplierPart.OP_CountDecimalPlaces);
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("3483bd49-a43f-446e-9bbf-fad71526c910", "Quantity"), TrackingWhsReceiveLine.WrapperSchema.WE_TransactionQuantity)
			{
				ColumnKey = WebTracker.Grids.WarehouseReceiveLine.Quantity,
				BindToDecimals = "WhsReceiveLine.SupplierPart.OP_CountDecimalPlaces",
				AutoPostBack = true
			});

			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsReceiveLine)null).WhsReceiveLine.ProductUQ);
			ZBindToChecker.CheckBindTo((CodeDescriptionPairList)((TrackingWhsReceiveLine)null).WhsReceiveLine.Lookups.PackTypesWithStandardUnits);
			AddToDictionaryAsDefault(new ZDropDownListColumn(Res.GetString("7b2a334d-13f4-498a-a824-a4dd2cb186b0", "UQ"), TrackingWhsReceiveLine.WrapperSchema.ProductUQ, "WhsReceiveLine.Lookups.PackTypesWithStandardUnits")
			{
				ColumnKey = WebTracker.Grids.WarehouseReceiveLine.UQ,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ReadOnly = true
			});

			if (SiteUser != null && SiteUser.IsLoggedIn)
			{
				var partManager = SiteUser.LoggedInOrganisation.PartAttributeManager;
				ZBindToChecker.CheckBindTo((ZString)((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_PartAttrib1);
				ZBindToChecker.CheckBindTo((ZString)((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_PartAttrib2);
				ZBindToChecker.CheckBindTo((ZString)((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_PartAttrib3);
				ZBindToChecker.CheckBindTo((ZString)((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_SerialNumber);
				ZBindToChecker.CheckBindTo((ZDate)((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_ExpiryDate);
				if (partManager.IsPartAttributeUsedByOrganisation(1))
				{
					AddToDictionaryAsDefault(new ZTextEditColumn(partManager.PartAttributeName1, TrackingWhsReceiveLine.WrapperSchema.WE_PartAttrib1) { ColumnKey = WebTracker.Grids.WarehouseReceiveLine.PartAttribute1 });
				}
				if (partManager.IsPartAttributeUsedByOrganisation(2))
				{
					AddToDictionaryAsDefault(new ZTextEditColumn(partManager.PartAttributeName2, TrackingWhsReceiveLine.WrapperSchema.WE_PartAttrib2) { ColumnKey = WebTracker.Grids.WarehouseReceiveLine.PartAttribute2 });
				}
				if (partManager.IsPartAttributeUsedByOrganisation(3))
				{
					AddToDictionaryAsDefault(new ZTextEditColumn(partManager.PartAttributeName3, TrackingWhsReceiveLine.WrapperSchema.WE_PartAttrib3) { ColumnKey = WebTracker.Grids.WarehouseReceiveLine.PartAttribute3 });
				}
				if (partManager.IsSerialNumberUsedByOrganisation)
				{
					AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("320dbb69-6a38-4bbd-9400-a78ed1e1721e", "Serial Number"), TrackingWhsReceiveLine.WrapperSchema.WE_SerialNumber) { ColumnKey = WebTracker.Grids.WarehouseReceiveLine.SerialNumber });
				}
				if (partManager.IsExpiryDateUsedByOrganisation)
				{
					AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("8d8c1e47-7248-4caa-ba3d-914fedc18a32", "Expiry Date"), TrackingWhsReceiveLine.WrapperSchema.WE_ExpiryDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.WarehouseReceiveLine.ExpiryDate });
				}
			}

			if (Receive != null)
			{
				var columnKey = 12000;
#pragma warning disable IDE0001 // Simplify Names
				foreach (CustomLabelInfo field in new WhsReceiveLine.CustomLabelsProvider(null).GetCustomFields(Receive.WhsReceive.Client, Receive.Factory))
#pragma warning restore IDE0001 // Simplify Names
				{
					if (field.IsEnabled && field.LabelName.StartsWith("WhsDocketLine."))
					{
						var column = ZTemplateColumn.GetNew(field.Caption, field.PropertyType, TrackingWhsReceiveLine.SchemaRoot + field.PropertyName);
						column.ColumnKey = columnKey;
						AddToDictionaryAsDefault(column);
						columnKey++;
					}
				}
			}
		}

		protected TrackingWhsReceive Receive { get; }

		protected string ShowInPopupParam { get; }

		protected bool IsShownInPopup { get; }

		protected bool AllowEdit { get; }
	}
}
