using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.Utilities;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingInventoryTemplate : ZItemTemplate
	{
		public TrackingInventoryTemplate(ZNewRowColumn column)
			: base(column)
		{
		}

		new ZNewRowColumn Column
		{
			get { return base.Column as ZNewRowColumn; }
		}

		public static List<ExcelExportColumnBase> GetInventoriesExcelExportColumns(TrackingWhsInventoryCollection collectionToExport)
		{
			var grid = new ZDataGrid { BindTo = Res.GetString("416538cb-1986-469a-8804-da23a0564bb6", "Inventories") };
			SetupGridForExportToExcel(grid);
			DataGridExcelExportHelper exportHelper = new DataGridExcelExportHelper(collectionToExport, grid.Columns);

			return exportHelper.CanContinueWithExport
					? exportHelper.GetExcelExportColumns()
					: new List<ExcelExportColumnBase>();
		}

		protected override ISelfBindingWebControl GetControl()
		{
			var control = new ZDataGrid();
			control.BindTo = Column.BindTo;
			SetupGrid(control);
			return control;
		}

		static void SetupGridForExportToExcel(ZDataGrid grid)
		{
			grid.Columns.Add(new ZTextEditColumn(Res.GetString("6358c758-6c2b-40cf-b6a2-45d2a1225671", "Warehouse"), TrackingWhsInventory.Schema.TrackingWarehouseName));
			grid.Columns.Add(new ZTextEditColumn(Res.GetString("99532392-da49-4253-ac08-5aa01f91a9c6", "Product"), TrackingWhsInventory.Schema.TrackingProductCode));
			grid.Columns.Add(new ZTextEditColumn(Res.GetString("32800867-9a0a-4153-ae2d-27f507a87fb3", "Product Desc."), TrackingWhsInventory.Schema.TrackingProductDescription));
			SetupGrid(grid);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		static void SetupGrid(ZDataGrid grid)
		{
			grid.Style[System.Web.UI.HtmlTextWriterStyle.Width] = "100%";
			grid.CssClass = "InnerDetailsTable";
			grid.ItemStyle.CssClass = "InnerDetailsCell";
			grid.HeaderStyle.CssClass = "InnerDetailsHeader";

			grid.GridLines = System.Web.UI.WebControls.GridLines.None;
			grid.BorderStyle = System.Web.UI.WebControls.BorderStyle.None;
			grid.BorderWidth = System.Web.UI.WebControls.Unit.Pixel(0);
			grid.CellSpacing = 4;

			// COLUMNS
			// ARRIVAL DATE OR ETA
			ZBindToChecker.CheckBindTo((ZDateTimeOffset)((TrackingWhsInventory)null).TrackingArrivalDateOrETA);
			grid.Columns.Add(new ZDateTimeColumn(Res.GetString("8d556a4a-7d6b-442f-a764-ca73bd508f14", "Arrival/ETA"), TrackingWhsInventory.Schema.TrackingArrivalDateOrETA));

			// STATUS
			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsInventory)null).TrackingInventoryStatus);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingWhsInventory)null).Lookups.InventoryStatuses);
			var statusColumn = new ZDropDownListColumn(Res.GetString("97112f6f-24bb-46f5-a631-7108e924ba3e", "Status"), TrackingWhsInventory.Schema.TrackingInventoryStatus, "Lookups.InventoryStatuses");
			statusColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			grid.Columns.Add(statusColumn);

			// HOLD CODE
			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsInventory)null).TrackingHeldCode);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingWhsInventory)null).Lookups.InventoryHeldCodeCollection);
			var heldCodeColumn = new ZDropDownListColumn(Res.GetString("76025a1a-3b4f-4d0b-bbe4-a2bdd4e9c006", "Hold Code"), TrackingWhsInventory.Schema.TrackingHeldCode, "Lookups.InventoryHeldCodeCollection");
			heldCodeColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			grid.Columns.Add(heldCodeColumn);

			var siteUser = WebEnv.AppInstance != null ? WebEnv.AppInstance.SiteUser as TrackingSiteUser : null;
			var partManager = (siteUser != null && siteUser.IsLoggedIn) ? siteUser.LoggedInOrganisation.PartAttributeManager : null;

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingWhsInventory)null).WI_PackingDate);
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingWhsInventory)null).WI_ExpiryDate);
			if (partManager != null)
			{
				// PART ATTRIBUTES (1, 2, 3), Expiry and Packing Dates
				if (partManager.IsPartAttributeUsedByOrganisation(1))
				{
					grid.Columns.Add(new ZTextEditColumn(partManager.PartAttributeName1, TrackingWhsInventory.Schema.TrackingPartAttrib1));
				}

				if (partManager.IsPartAttributeUsedByOrganisation(2))
				{
					grid.Columns.Add(new ZTextEditColumn(partManager.PartAttributeName2, TrackingWhsInventory.Schema.TrackingPartAttrib2));
				}

				if (partManager.IsPartAttributeUsedByOrganisation(3))
				{
					grid.Columns.Add(new ZTextEditColumn(partManager.PartAttributeName3, TrackingWhsInventory.Schema.TrackingPartAttrib3));
				}

				if (partManager.IsSerialNumberUsedByOrganisation)
				{
					grid.Columns.Add(new ZTextEditColumn(Res.GetString("0ff32891-5da6-46b3-a99c-b5393cb04677", "Serial Number"), TrackingWhsInventory.Schema.TrackingSerialNumber));
				}

				if (partManager.IsPackingDateUsedByOrganisation)
				{
					grid.Columns.Add(new ZDateTimeColumn(Res.GetString("e490e9c1-90af-4a1c-9802-4230ed81b618", "Packing Date"), TrackingWhsInventory.Schema.TrackingPackingDate, ZDateTimePickerFormat.Long));
				}

				if (partManager.IsExpiryDateUsedByOrganisation)
				{
					grid.Columns.Add(new ZDateTimeColumn(Res.GetString("ca670333-a18f-4367-9af7-39a22a51f8d6", "Expiry Date"), TrackingWhsInventory.Schema.TrackingExpiryDate, ZDateTimePickerFormat.Long));
				}
			}

			// LOCATION
			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsInventory)null).TrackingCurrentLocation);
			grid.Columns.Add(new ZTextEditColumn(Res.GetString("1b9b6692-dc6a-4527-9c80-ddd63383e3a8", "Location"), TrackingWhsInventory.Schema.TrackingCurrentLocation));

			// PALLET ID
			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsInventory)null).WI_PalletID);
			grid.Columns.Add(new ZTextEditColumn(Res.GetString("bf724df6-f414-4d30-910e-c40ac4dfd28a", "Pallet ID"), TrackingWhsInventory.Schema.TrackingPalletID));

			// AVAILABLE PICK QUANTITY
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsInventory)null).WI_AvailableToPickQuantity);
			grid.Columns.Add(new ZCalcEditColumn(Res.GetString("6ff3a7bd-1b4a-40a7-82a8-c86e9565dcf0", "Available Pick Qty"), TrackingWhsInventory.Schema.TrackingAvailableToPickQuantity, "SupplierPart.OP_CountDecimalPlaces"));

			// COMMITTED QUANTITY
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsInventory)null).TrackingCommittedQuantity);
			grid.Columns.Add(new ZCalcEditColumn(Res.GetString("372ea34b-9e79-4f2b-9b1d-0f994e479d0c", "Committed"), TrackingWhsInventory.Schema.TrackingCommittedQuantity, "SupplierPart.OP_CountDecimalPlaces"));

			// ALLOCATED QUANTITY
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsInventory)null).TrackingCrossDockQuantity);
			grid.Columns.Add(new ZCalcEditColumn(Res.GetString("c5ada275-e23d-4fdc-aa92-5929a6072ff5", "Reserved"), TrackingWhsInventory.Schema.TrackingCrossDockQuantity, "SupplierPart.OP_CountDecimalPlaces"));

			// QUANTITY UNIT
			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsInventory)null).WI_UnitsUQ);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingWhsInventory)null).Lookups.PackTypesWithStandardUnits);
			grid.Columns.Add(new ZDropDownListColumn(Res.GetString("AB3ABE3A-7017-4A4E-B44E-5F0EC7840716", "UQ"), TrackingWhsInventory.Schema.TrackingUnitsUQ, "Lookups.PackTypesWithStandardUnits"));

			// CLIENT QUANTITY
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsInventory)null).TrackingClientQuantity);
			grid.Columns.Add(new ZCalcEditColumn(Res.GetString("72e847d4-91e2-488b-ba72-e1c037fdc278", "Client Qty"), TrackingWhsInventory.Schema.TrackingClientQuantity, "SupplierPart.OP_CountDecimalPlaces"));

			// Total Quantity
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsInventory)null).WI_TotalUnits);
			grid.Columns.Add(new ZCalcEditColumn(Res.GetString("9CC3EC48-2AC3-40CE-B44C-424FBA692AA6", "Total Qty"), TrackingWhsInventory.Schema.TrackingTotalUnits, "SupplierPart.OP_CountDecimalPlaces"));

			// CLIENT QUANTITY UNIT
			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsInventory)null).WI_ClientUQ);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingWhsInventory)null).Lookups.PackTypesWithStandardUnits);
			grid.Columns.Add(new ZDropDownListColumn(Res.GetString("AB3ABE3A-7017-4A4E-B44E-5F0EC7840716", "UQ"), TrackingWhsInventory.Schema.TrackingClientUQ, "Lookups.PackTypesWithStandardUnits"));

			// PRODUCT WEIGHT
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsInventory)null).SupplierPart.OP_Weight);
			grid.Columns.Add(new ZCalcEditColumn(Res.GetString("54c6f365-298b-4ce3-bded-770148b5f22e", "Product Wt."), "SupplierPart+" + OrgSupplierPart.Schema.OP_Weight) { Decimals = 3 });

			// TOTAL WEIGHT
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsInventory)null).TrackingTotalWeight);
			grid.Columns.Add(new ZCalcEditColumn(Res.GetString("8782e55e-d384-4b0f-aa28-c83e78085765", "Total Wt."), TrackingWhsInventory.Schema.TrackingTotalWeight) { Decimals = 3 });

			// WEIGHT UQ
			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsInventory)null).SupplierPart.OP_WeightUQ);
			grid.Columns.Add(new ZTextEditColumn(Res.GetString("ef665360-54fa-4b45-b7a3-3e8a8d086a69", "Wt. UQ"), "SupplierPart+" + OrgSupplierPart.Schema.OP_WeightUQ));

			// PRODUCT VOLUME
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsInventory)null).SupplierPart.OP_Cubic);
			grid.Columns.Add(new ZCalcEditColumn(Res.GetString("1f1b6a90-13c1-4a4b-a6b3-63c34dfdd983", "Product Vol."), "SupplierPart+" + OrgSupplierPart.Schema.OP_Cubic) { Decimals = 3 });

			// TOTAL VOLUME
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsInventory)null).TrackingTotalVolume);
			grid.Columns.Add(new ZCalcEditColumn(Res.GetString("f0b51a95-dee2-4724-a8e3-634d72e8d726", "Total Vol."), TrackingWhsInventory.Schema.TrackingTotalVolume) { Decimals = 3 });

			// VOLUME UQ
			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsInventory)null).SupplierPart.OP_CubicUQ);
			grid.Columns.Add(new ZTextEditColumn(Res.GetString("f9ac8a92-3d85-4bc2-a0e8-dfe6e740a614", "Vol. UQ"), "SupplierPart+" + OrgSupplierPart.Schema.OP_CubicUQ));

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsInventory)null).SupplierPart.OP_LastCost);
			grid.Columns.Add(new ZCalcEditColumn(Res.GetString("6cd8cdd5-6327-4406-a607-c0f232e65744", "Last Cost"), "SupplierPart+" + OrgSupplierPart.Schema.OP_LastCost) { Decimals = 2 });

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsInventory)null).TrackingTotalValue);
			grid.Columns.Add(new ZCalcEditColumn(Res.GetString("35f6cb82-0348-4e73-adaf-bc1077af4162", "Total Value"), TrackingWhsInventory.Schema.TrackingTotalValue));

			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsInventory)null).TrackingCurrency);
			grid.Columns.Add(new ZTextEditColumn(Res.GetString("e2d219ac-0668-4045-8973-a29035bb8b3c", "Currency"), TrackingWhsInventory.Schema.TrackingCurrency));

			// HAS EDOCS OR NOTES
			ZBindToChecker.CheckBindTo((ZBool)((TrackingWhsInventory)null).TrackingHasEDocsAttached);
			grid.Columns.Add(new ZCheckBoxColumn(Res.GetString("8d0e7ebe-7b15-40f2-b97f-eec144f58228", "eDocs"), TrackingWhsInventory.Schema.TrackingHasEDocsAttached));

			// RECEIPT DOCKET ATTRIBUTES
			if (siteUser != null)
			{
				AddCustomColumns(grid, siteUser.LoggedInOrganisation);
			}

			// VIEW DETAILS LINK
			if (WebEnv.AppInstance is Global app)
			{
				grid.Columns.Add(new ZHyperLinkColumn(Res.GetString("da7bfaf2-6242-4281-a335-14bb992e35bf", "View"), "")
				{
					Text = Res.GetString("da7bfaf2-6242-4281-a335-14bb992e35bf", "View"),
					DataNavigateUrlFormatString = app.InventoryDetailsPage + (NoResString)"?Ref={0}&" + TrackingConstants.QueryStringKeys.PopupKey + (NoResString)"=Y", // partial URL
					DataNavigateUrlFields = new[] { "PK" },
					Target = (NoResString)"_blank", // programmatic constant
					WindowStyle = Global.InventoryDetailsPopupWindowStyle
				});
			}

			if (siteUser != null && siteUser.CanEditWarehouseOrders)
			{
				// ALLOCATE QUANTITY
				ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsInventory)null).Quantity);
				ZBindToChecker.CheckBindTo((ZByte)((TrackingWhsInventory)null).SupplierPart.OP_CountDecimalPlaces);

				var allocateColumn = new ZCalcEditColumn(Res.GetString("f2695d44-f261-43cb-a7ef-043d561a8fb2", "Allocate"), TrackingWhsInventory.Schema.Quantity);
				allocateColumn.ItemTemplate = allocateColumn.GetNewEditItemTemplate();
				allocateColumn.BindToDecimals = "SupplierPart.OP_CountDecimalPlaces";
				allocateColumn.ValidateBindToDecimals = true;
				grid.Columns.Add(allocateColumn);
			}

			var pageSize = (int)WebDataRegistry.Instance.WarehouseInventoryDetailsPageSize.Value;
			grid.AllowPaging = pageSize > 0;
			if (grid.AllowPaging)
			{
				grid.PageSize = pageSize;
			}
		}

		protected static void AddCustomColumns(ZDataGrid grid, OrgHeader loggedInOrg)
		{
			if (loggedInOrg != null)
			{
				foreach (CustomLabelInfo field in new WhsInventoryView.CustomLabelsProvider(null).GetCustomFields(loggedInOrg, loggedInOrg.Factory))
				{
					if (field.IsEnabled && field.LabelName.StartsWith("WhsDocketLine."))
					{
						grid.Columns.Add(ZTemplateColumn.GetNew(field.Caption, field.PropertyType, field.PropertyName));
					}
				}
			}
		}
	}
}
