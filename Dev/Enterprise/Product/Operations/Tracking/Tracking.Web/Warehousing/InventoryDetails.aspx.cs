using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
	public partial class InventoryDetails : WarehousingBasePage
	{
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			DisabledSubmitButtons.Add(SaveInventory.ClientID);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupCustomAttributes();
			NotificationFlags.DisplayAll = false;
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.WarehouseInventoryDetails;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.InventoryDetailsPage;
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsInventory)null).TrackingProductCode);
			ProductLink.BindTo = TrackingWhsInventory.Schema.TrackingProductCode;
			ProductLink.DataNavigateUrlFormatString = AppInstance.ProductProfileDetailsPage + (NoResString)"?Ref={0}" + ShowInPopupParam; // Request parameter
			ProductLink.DataNavigateUrlFields = new string[1] { WhsInventoryView.Schema.WI_OP };
			ProductLink.Target = (NoResString)"_blank"; // Html target
			ProductLink.WindowStyle = Global.ProductProfilePopupWindowStyle;

			ZBindToChecker.CheckBindTo((ZString)(((TrackingWhsInventory)null).InDocketLine.ReceiptReference));
			ReceiptRefLink.BindTo = "InDocketLine+ReceiptReference";
			ReceiptRefLink.DataNavigateUrlFormatString = AppInstance.WarehouseReceiveDetails + (NoResString)"?Ref={0}" + ShowInPopupParam; // Request parameter
			ReceiptRefLink.DataNavigateUrlFields = new string[1] { WhsInventoryView.Schema.WI_WD };
			ReceiptRefLink.Target = (NoResString)"_blank"; // HTML target
			ReceiptRefLink.WindowStyle = Global.WarehouseReceiveDetailsPopupWindowStyle;

			DocketRef.BindTo = ReceiptRefLink.BindTo;
		}

		protected override void SetupAuthorisedContent(bool isAuthorised)
		{
			base.SetupAuthorisedContent(isAuthorised);
			if (!isAuthorised)
			{
				NotificationFlags.DisplayAll = false;
			}
		}

		protected override bool CanAccessAuthorisedContent { get { return SiteUser.CanViewInventory; } }

		#region Overrides to hide page header and menu

		protected override string PageHeaderControlPath => string.Empty;

		protected override bool ShowLoginStatus => false;

		protected override bool ShowCloseWindowInPopup => false;

		#endregion

		protected override void OnPreBind()
		{
			base.OnPreBind();

			AddLineAttributeColumns(CrossDockedOrderLinesGrid, AttributeManager.AttributeModules.Warehouse, Inventory.Client, "OrderLine");
			SetupAdditionalInformationTable(Inventory.GetAdditionalInformationFields(), AdditionalDetailTable, AdditionalDetailPanel);

			SetupDocumentsGrid(DocumentsGrid);
			SetupReferenceLinkAndText();
			if (!Page.IsPostBack)
			{
				NotificationFlags.DisplayAll = ZBool.False;
			}
		}

		void SetupReferenceLinkAndText()
		{
			var isReceive = (string)Inventory?.InDocketLine?.Docket?.WD_DocketType == DocketType.Codes.Receive;

			ReceiptRefLink.Visible = isReceive;
			DocketRef.Visible = !isReceive;
		}

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		protected override BusinessObject GetNewDataSource()
		{
			return InventoryPK.IsValid
				? OrgRestrictionFilterFactory.LoadFilteredByContact<TrackingWhsInventory>(Factory, WhsInventoryViewSchema.PK, InventoryPK)
				: null;
		}

		protected TrackingWhsInventory Inventory => DataSource as TrackingWhsInventory;

		protected ZGuid InventoryPK => GetGuidFromParameter("Ref");

		#region Setup Custom Attributes

		public void SetupCustomAttributes()
		{
			var partManager = SiteUser.LoggedInOrganisation.PartAttributeManager;
			if (partManager.IsPartAttributeUsedByOrganisation(1))
			{
				CustomAttr1Label.Text = partManager.PartAttributeName1;
			}
			else
			{
				CustomAttr1.Visible = false;
			}

			if (partManager.IsPartAttributeUsedByOrganisation(2))
			{
				CustomAttr2Label.Text = partManager.PartAttributeName2;
			}
			else
			{
				CustomAttr2.Visible = false;
			}

			if (partManager.IsPartAttributeUsedByOrganisation(3))
			{
				CustomAttr3Label.Text = partManager.PartAttributeName3;
			}
			else
			{
				CustomAttr3.Visible = false;
			}

			if (partManager.IsSerialNumberUsedByOrganisation)
			{
				SerialNumberLabel.Text = SerialNumberColumnText;
			}
			else
			{
				SerialNumber.Visible = false;
			}

			ExpiryDate.DateTimeFormat = ZDateTimePickerFormat.Long;
			PackingDate.DateTimeFormat = ZDateTimePickerFormat.Long;

			if (partManager.IsExpiryDateUsedByOrganisation && partManager.IsExpiryDateUsedByProduct(Inventory.SupplierPart))
			{
				ExpiryPackingDatesRow.Visible = true;
				ExpiryDateLabel.Visible = true;
				ExpiryDate.Visible = true;
			}

			if (partManager.IsPackingDateUsedByOrganisation && partManager.IsPackingDateUsedByProduct(Inventory.SupplierPart))
			{
				ExpiryPackingDatesRow.Visible = true;
				PackingDateLabel.Visible = true;
				PackingDate.Visible = true;
			}

			CustomAttr12Row.Visible = CustomAttr1.Visible || CustomAttr2.Visible;
			CustomAttr3Row.Visible = CustomAttr3.Visible || SerialNumber.Visible;
		}

		#endregion

		protected override void SetupGrids()
		{
			SetupCrossDockedOrderLinesGrid();
		}

		protected void SetupCrossDockedOrderLinesGrid()
		{
			PopulateGrid(CrossDockedOrderLinesGrid, GetCrossDockedOrderLinesColumns());
			CrossDockedOrderLinesGrid.AllowDelete = SiteUser.CanEditWarehouseOrders;
			CrossDockedOrderLinesGrid.AllowEdit = SiteUser.CanEditWarehouseOrders;
		}

		DataGridColumn[] GetCrossDockedOrderLinesColumns()
		{
			var crossDockedOrderLinesGridColumns = new List<DataGridColumn>();
			ZBindToChecker.CheckBindTo(((ZString)(((WhsPickLine)(null)).DocketLine.Docket.WD_ExternalReference)));
			ZBindToChecker.CheckBindTo(((ZGuid)(((WhsPickLine)(null)).DocketLine.WE_WD)));
			crossDockedOrderLinesGridColumns.Add(new ZHyperLinkColumn(Res.GetString("32d1d592-f99e-4a5f-9bb5-ef5e055347a6", "Order No"), "DocketLine+Docket+" + WhsDocketSchema.WD_ExternalReference.Name)
			{
				DataNavigateUrlFormatString = AppInstance.WarehouseOrderDetailsPage + (NoResString)"?Ref={0}" + ShowInPopupParam, // Request parameter
				DataNavigateUrlFields = new string[1] { "DocketLine+" + WhsOrderLine.Schema.WE_WD },
				Target = (NoResString)"_blank", // HTML parameter
				WindowStyle = Global.WarehouseOrderDetailsPopupWindowStyle
			});
			ZBindToChecker.CheckBindTo(((ZGuid)(((WhsPickLine)(null)).DocketLine.WE_OP)));
			crossDockedOrderLinesGridColumns.Add(new ZFindBoxColumn(Res.GetString("7a0b6112-6889-4b0f-88da-1a7d5300fde7", "Product"), "DocketLine+" + WhsOrderLine.Schema.WE_OP, "DocketLine+Lookups+SupplierParts")
			{
				ValueFieldName = "PK",
				ModuleID = WebModuleIDs.OrgSupplierPartTracking,
				ReadOnly = true
			});
			ZBindToChecker.CheckBindTo(((ZString)(((WhsPickLine)(null)).DocketLine.ProductDesc)));
			crossDockedOrderLinesGridColumns.Add(new ZTextEditColumn(Res.GetString("254c9fb3-c7c0-4496-8311-4d3d0068b69f", "Description"), "DocketLine+WE_OP_Desc") { ReadOnly = true });

			ZBindToChecker.CheckBindTo(((ZDecimal)(((WhsPickLine)(null)).ReservedQuantity)));
			ZBindToChecker.CheckBindTo(((ZByte)(((WhsPickLine)(null)).Inventory.SupplierPart.OP_CountDecimalPlaces)));
			crossDockedOrderLinesGridColumns.Add(new ZCalcEditColumn(Res.GetString("39076937-2a91-4a4b-be0e-ae271be526ec", "Quantity"), WhsPickLine.Schema.ReservedQuantity)
			{
				BindToDecimals = "Inventory+SupplierPart+" + OrgSupplierPartSchema.OP_CountDecimalPlaces.Name,
			});

			PartAttributeManager partManager = Page.SiteUser.LoggedInOrganisation.PartAttributeManager;

			ZBindToChecker.CheckBindTo(((WhsPickLine)null).DocketLine.WE_PartAttrib1);
			ZBindToChecker.CheckBindTo(((WhsPickLine)null).DocketLine.WE_PartAttrib2);
			ZBindToChecker.CheckBindTo(((WhsPickLine)null).DocketLine.WE_PartAttrib3);
			ZBindToChecker.CheckBindTo(((WhsPickLine)null).DocketLine.WE_SerialNumber);
			ZTextEditColumn clientDependantColumn1 = new ZTextEditColumn(partManager.PartAttributeName1, "DocketLine+" + WhsOrderLine.Schema.WE_PartAttrib1) { ReadOnly = true };
			ZTextEditColumn clientDependantColumn2 = new ZTextEditColumn(partManager.PartAttributeName2, "DocketLine+" + WhsOrderLine.Schema.WE_PartAttrib2) { ReadOnly = true };
			ZTextEditColumn clientDependantColumn3 = new ZTextEditColumn(partManager.PartAttributeName3, "DocketLine+" + WhsOrderLine.Schema.WE_PartAttrib3) { ReadOnly = true };
			ZTextEditColumn serialNumberColumn = new ZTextEditColumn(SerialNumberColumnText, "DocketLine+" + WhsOrderLine.Schema.WE_SerialNumber) { ReadOnly = true };

			AddClientDependantColumn(crossDockedOrderLinesGridColumns, partManager.IsPartAttributeUsedByOrganisation(1), clientDependantColumn1);
			AddClientDependantColumn(crossDockedOrderLinesGridColumns, partManager.IsPartAttributeUsedByOrganisation(2), clientDependantColumn2);
			AddClientDependantColumn(crossDockedOrderLinesGridColumns, partManager.IsPartAttributeUsedByOrganisation(3), clientDependantColumn3);
			AddClientDependantColumn(
				crossDockedOrderLinesGridColumns,
				partManager.IsSerialNumberUsedByOrganisation,
				serialNumberColumn);

			ZBindToChecker.CheckBindTo((ZDateTime)((WhsPickLine)null).DocketLine.WE_ExpiryDate);
			ZBindToChecker.CheckBindTo((ZDateTime)((WhsPickLine)null).DocketLine.WE_PackingDate);
			crossDockedOrderLinesGridColumns.Add(new ZDateTimeColumn(Res.GetString("1ee62588-7691-40c3-bef2-ff60aced3582", "Expiry Date"), "DocketLine+" + WhsOrderLine.Schema.WE_ExpiryDate, ZDateTimePickerFormat.Short) { ReadOnly = true });
			crossDockedOrderLinesGridColumns.Add(new ZDateTimeColumn(Res.GetString("4a4dc7f5-5e53-41f9-830f-8bc002354e8d", "Packing Date"), "DocketLine+" + WhsOrderLine.Schema.WE_PackingDate, ZDateTimePickerFormat.Short) { ReadOnly = true });

			return crossDockedOrderLinesGridColumns.ToArray();
		}

		string SerialNumberColumnText => Res.GetString("90182d34-c37e-4d8a-85d8-1f5b2ba9f173", "Serial Number");

		protected void SaveInventory_Click(object sender, EventArgs e)
		{
			Inventory.Validation.ValidateQuantity();
			NotificationFlags.DisplayAll = true;
			SuppressErrorDialog = false;
			CrossDockedOrderLinesGrid.SaveChanges(sender);
			SaveDataSourceFactory();
		}

		protected override void OnDataSourceFactorySaved()
		{
			ClosePage();
		}

		void ClosePage()
		{
			Response.Write((NoResString)"<script language='javascript'> { window.close();}</script>"); // JavaScript code
		}

		protected void CancelInventory_Click(object sender, EventArgs e)
		{
			CrossDockedOrderLinesGrid.DiscardChanges(sender);
			ClosePage();
		}
	}
}
