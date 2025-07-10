using System;
using System.Linq;
using System.Reflection;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.Module;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public partial class WhsOrderLineAllocation : WarehousingBasePage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			SetupCustomAttributes();
			SetupCrossDockGrid();
			SetupButtons();
			if (EditMode)
			{
				NotificationFlags.DisplayAll = true;
				NotificationFlags.DisplayWarnings = false;
			}
			else
			{
				NotificationFlags.DisplayAll = false;
			}
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.WarehouseOrderLineAllocationPage;
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return (EditMode && SiteUser.CanEditWarehouseOrders) || (!EditMode && SiteUser.CanViewWarehouseOrders); }
		}

		#region Overrides

		protected override string PageHeaderControlPath
		{
			get { return string.Empty; }
		}

		protected override bool ShowLoginStatus
		{
			get { return false; }
		}

		protected override bool ShowCloseWindowInPopup
		{
			get { return false; }
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			DisabledSubmitButtons.Add(CloseWindow.ClientID);

			if (EditMode)
			{
				PropertyInfo clickHandlerInfo = InventorySelector.GetType().GetProperty("ButtonClickHandler", BindingFlags.Instance | BindingFlags.NonPublic);
				AttachInventory.OnClientClick = (string)clickHandlerInfo.GetValue(InventorySelector, null) + (NoResString)"; return false;";
			}
		}

		#endregion

		#region Factory

		public new BusinessObjectFactory Factory
		{
			get
			{
				return OrderSessionIndexer.IsValid && HttpContext.Current.Session[OrderSessionIndexer.ToString()] != null ?
					((BusinessObject)HttpContext.Current.Session[OrderSessionIndexer.ToString()]).Factory : base.Factory;
			}
		}

		#endregion

		#region BusinessObject

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			TrackingWhsOrderLine fOrderLine = null;
			if (OrderLinePK.IsValid)
			{
				var orderLine = Order != null ? (WhsOrderLine)Order.WhsOrder.Lines.FindByPK(OrderLinePK) : Factory.Load<WhsOrderLine>(OrderLinePK);
				fOrderLine = TrackingHelper.Get(orderLine);
			}
			if (fOrderLine != null)
			{
				fOrderLine.Logs.AutoCreatedLogDefaultSL_Reference = SiteUser.ContactAndCompanyReference;
			}
			return fOrderLine;
		}

		protected
#if DEBUG
 virtual
#endif
 TrackingWhsOrderLine OrderLine
		{
			get { return DataSource as TrackingWhsOrderLine; }
		}

		#endregion BusinessObject

		#region OrderLinePK

		protected ZGuid OrderLinePK
		{
			get { return GetGuidFromParameter("Ref"); }
		}

		#endregion

		#region Order

		protected TrackingWhsOrder Order
		{
			get
			{
				return OrderSessionIndexer.IsValid && HttpContext.Current.Session[OrderSessionIndexer.ToString()] != null ?
						HttpContext.Current.Session[OrderSessionIndexer.ToString()] as TrackingWhsOrder : null;
			}
		}

		#endregion

		#region OrderSessionIndex

		protected ZGuid OrderSessionIndexer
		{
			get { return GetGuidFromParameter("Order"); }
		}

		#endregion

		#region EditMode

		bool EditMode
		{
			get { return OrderSessionIndexer.IsValid && !OrderSessionIndexer.IsEmpty; }
		}

		#endregion

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
				SerialNumberLabel.Text = Res.GetString("033a111b-cec6-4019-ab34-e4d7ff5d6894", "Serial Number");
			}
			else
			{
				SerialNumber.Visible = false;
			}

			CustomAttr12Row.Visible = CustomAttr1.Visible || CustomAttr2.Visible;
			CustomAttr3Row.Visible = CustomAttr3.Visible || SerialNumber.Visible;
		}

		#endregion

		#region Setup CrossDock Grid

		protected void SetupCrossDockGrid()
		{
			ZBindToChecker.CheckBindTo(((ZString)(((WhsPickLine)(null)).InventoryLine.ReceiptReference)));
			ZTextEditColumn receiptRef = new ZTextEditColumn(Res.GetString("50e02f2c-d659-48e9-a79f-f0262c367a01", "Receipt Ref"), "InventoryLine+ReceiptReference");
			receiptRef.ReadOnly = true;
			CrossDocksGrid.Columns.Add(receiptRef);

			ZBindToChecker.CheckBindTo(((ZDateTimeOffset)(((WhsPickLine)(null)).Inventory.WI_ArrivalDateOrETA)));
			ZDateTimeColumn arrivalDate = new ZDateTimeColumn(Res.GetString("bf5ef289-5e61-40c3-8fab-aede72094c8c", "ETA/Arrival"), "Inventory+WI_ArrivalDateOrETA");
			arrivalDate.ReadOnly = true;
			CrossDocksGrid.Columns.Add(arrivalDate);

			ZBindToChecker.CheckBindTo(((ZString)(((WhsPickLine)(null)).Inventory.StatusDesc)));
			CrossDocksGrid.Columns.Add(new ZTextEditColumn(Res.GetString("e9fe70ed-e731-4be6-ba88-b14642db2176", "Status"), "Inventory+StatusDesc"));

			ZBindToChecker.CheckBindTo((ZByte)((WhsPickLine)null).DocketLine.SupplierPart.OP_CountDecimalPlaces);
			ZBindToChecker.CheckBindTo(((ZDecimal)(((WhsPickLine)(null)).Inventory.WI_AvailableForCrossDockQuantity)));
			CrossDocksGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("2620516a-e428-4074-b07b-64af42670d92", "Available Pick Qty"), "Inventory+WI_AvailableForCrossDockQuantity")
			{
				BindToDecimals = "DocketLine+SupplierPart+OP_CountDecimalPlaces"
			});

			ZBindToChecker.CheckBindTo(((ZDecimal)(((WhsPickLine)(null)).ReservedQuantity)));
			CrossDocksGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("9ef9b7cb-1cd5-4d75-8b60-b1adc67cf007", "Reserved Qty"), WhsPickLine.Schema.ReservedQuantity)
			{
				BindToDecimals = "DocketLine+SupplierPart+OP_CountDecimalPlaces"
			});

			CrossDocksGrid.AllowDelete = EditMode;
			CrossDocksGrid.AllowEdit = EditMode;
			if (EditMode)
			{
				CrossDocksGrid.AfterDeleteCommand += RefreshAllocatedQuantity;
				CrossDocksGrid.AfterUpdateCommand += RefreshAllocatedQuantity;
			}
		}

		void RefreshAllocatedQuantity(object sender, EventArgs e)
		{
			AllocatedQuantity.Bind(OrderLine);
			UnallocatedQuantity.Bind(OrderLine);
		}

		#endregion

		#region Setup Buttons

		void SetupButtons()
		{
			AttachButtonRow.Visible = EditMode;
			InventorySelector.Visible = EditMode;
			if (EditMode)
			{
				InventorySelector.OnGetAdditionalParameters += AddOrderLineDetails;
			}
			else
			{
				this.CloseWindow.OnClientClick = (NoResString)"javascript: self.close();"; // javascript code
			}
		}

		#endregion

		#region Inventory Selected Handlers

		protected void InventorySelected(object sender, EventArgs e)
		{
			if (sender == InventorySelector)
			{
				ZGuid inventoryPK = (ZGuid)InventorySelector.SelectedValue;
				if (inventoryPK.IsValid)
				{
					if (!IsInventoryAllocated(inventoryPK)) // Should not add duplicate inventory
					{
						AddCrossDock(inventoryPK);
					}
					else
					{
						ZClientScript.RegisterClientForEventScriptBlock((NoResString)"window", (NoResString)"onload", GetType(), "DuplicateInventories_ScriptHandler", (NoResString)"alert('Cannot add duplicate inventories.');"); // JavaScript
					}
				}
			}
		}

		protected void AddCrossDock(ZGuid inventoryPK)
		{
			var inventory = OrderLine.Factory.Load<TrackingWhsInventory>(inventoryPK);
			OrderLine.WhsOrderLine.ReserveStockIfAbleTo(inventory);

			CrossDocksGrid.Bind(OrderLine);
			InventorySelector.SelectedValue = ZGuid.Empty;
		}

		bool IsInventoryAllocated(ZGuid inventoryPK)
		{
			var inventory = OrderLine.Factory.Load<TrackingWhsInventory>(inventoryPK);
			return OrderLine.WhsOrderLine.ReservedPickLines.Any(p => p.WZ_WE_InventoryLine == inventory.WI_WE_InDocketLine);
		}

#if DEBUG
		protected
#endif
 void AddOrderLineDetails(object sender, ZTextIFramePopup.GetAdditionalParametersEventArgs e)
		{
			if (e != null && e.AdditionalParameters != null && OrderLine != null && OrderLine.Order != null)
			{
				e.AdditionalParameters.Add(TrackingConstants.QueryStringKeys.WhsProductRefKey, OrderLine.WhsOrderLine.WE_OP.ToString());
				e.AdditionalParameters.Add(TrackingConstants.QueryStringKeys.WarehouseRefKey, OrderLine.Order.WhsOrder.WD_WW_Whs.ToString());
				e.AdditionalParameters.Add(InventoryFilterBusinessObject.Schema.Status, InventoryStatus.Codes.Available);
			}
		}

		#endregion

		#region Save Button Click Handlers

		public void Save_Click(object sender, EventArgs e)
		{
			CrossDocksGrid.SaveChanges(this);

			if (OrderLine != null)
			{
				OrderLine.RunPreSaveValidation();

				if (!OrderLine.HasErrors)
				{
					RegisterClosingScript();
				}
				else
				{
					NotificationFlags.DisplayAll = true;
					NotificationFlags.DisplayWarnings = false;
				}
			}
		}

		void RegisterClosingScript()
		{
			if (
#if DEBUG
!Globals.IsTest &&
#endif
 Request.Browser.EcmaScriptVersion.Major >= 1 && !ZClientScript.IsClientForEventScriptBlockRegistered((NoResString)"window", (NoResString)"onload", GetType(), "OnLoad")) // Programmatic constant
			{
				ZClientScript.RegisterClientForEventScriptBlock((NoResString)"window", (NoResString)"onload", GetType(), "OnLoad", (NoResString)"javascript: self.close ()"); // Programmatic constant
			}
		}

		#endregion

		protected override string GetPageName()
		{
			return WebTracker.Pages.WarehouseOrderLineAllocation;
		}
	}
}
