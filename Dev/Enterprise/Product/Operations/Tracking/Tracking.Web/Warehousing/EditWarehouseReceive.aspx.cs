using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public partial class EditWarehouseReceive : WarehousingBasePage
	{
		#region Overrides

		protected override string GetPageName()
		{
			return WebTracker.Pages.EditWarehouseReceive;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.EditWarehouseReceivePage;
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			DisabledSubmitButtons.Add(SaveReceive.ClientID);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (Receive == null)
			{
				PageTitle = Res.GetString("7a0aa299-1dad-43bc-b684-56e47d4c9d3d", "Receipt Not Found");
				WhsReceiveLabel.Text = Res.GetString("7a0aa299-1dad-43bc-b684-56e47d4c9d3d", "Receipt Not Found");
				NotFoundLabel.Text = Res.GetString("01b1a5d3-3d22-4f88-aa02-300dff585c3c", "Receipt was not found in the database or you don't have rights to edit it.");
				ReceiveContents.Visible = false;
			}
			else if (Receive.IsCancelled)
			{
				RedirectToReceiveDetailsPage();
			}
			else
			{
				string newOrderLabelText = Res.GetString("8017499b-0de6-4cd6-b5f2-eca604e4d701", "New Warehouse Receipt");
				string editOrderLabelText = Res.GetString("bd6ec687-21fb-4835-91a5-0519c5be2730", "Edit Warehouse Receipt");
				PageTitle = ReceivePK.IsValid ? editOrderLabelText : newOrderLabelText;
				WhsReceiveLabel.Text = ReceivePK.IsValid ? editOrderLabelText : newOrderLabelText;
				SaveReceive.Text = ReceivePK.IsValid ? Res.GetString("fda0d810-30bf-481c-a675-ba4c52f37c5a", "Update Receipt") : Res.GetString("7a5da47c-0130-4603-9411-425e0464d975", "Save Receipt");
				CancelReceive.Text = ReceivePK.IsValid ? Res.GetString("7105d783-9bef-495a-b7dc-f1b23b5a1373", "Cancel Changes") : Res.GetString("9c30ff2b-6f52-4ede-8e58-6abe45b76e0f", "Cancel Receipt");
				NotFoundError.Visible = false;
				ReceiveContents.Visible = true;
			}

			if (Receive != null && Receive.WhsReceive.Lines.Count > 0)
			{
				WarehouseDropDown.AutoPostBack = true;
			}
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanEditWarehouseReceipts; }
		}

		protected override void SetupAuthorisedContent(bool isAuthorised)
		{
			if (!isAuthorised)
			{
				NotificationFlags.DisplayAll = false;
			}
		}

		protected override void OnPreBind()
		{
			base.OnPreBind();
			if (!Page.IsPostBack)
			{
				NotificationFlags.DisplayAll = ZBool.False;
			}
		}

		#endregion Overrides

		#region BusinessObject

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			TrackingWhsReceive fReceive = null;
			if (ReceivePK.IsValid)
			{
				fReceive = TrackingWhsReceive.FromPKFilteredByContact(Factory, ReceivePK, SiteUser);
			}
			else
			{
				fReceive = TrackingHelper.Get(Factory.New<WhsReceive>());
				fReceive.SiteUser = SiteUser;
				fReceive.WhsReceive.WD_OH_Client = SiteUser.LoggedInOrganisation.PK;
				AssignDefaultWarehouseIfPossible(fReceive);
			}
			if (fReceive != null)
			{
				fReceive.Logs.AutoCreatedLogDefaultSL_Reference = SiteUser.ContactAndCompanyReference;
			}
			return fReceive;
		}

		protected void AssignDefaultWarehouseIfPossible(TrackingWhsReceive receive)
		{
			if (receive.WhsReceive.WD_WW_Whs.IsEmpty)
			{
				WhsWarehouse[] warehouses = Factory.Load<WhsWarehouse>(OrgRestrictionFilterFactory.Instance.GetFilter<WhsWarehouse>());
				if (warehouses.Length == 1)
				{
					receive.WhsReceive.WD_WW_Whs = warehouses[0].PK;
				}
			}
		}

		protected TrackingWhsReceive Receive
		{
			get { return DataSource as TrackingWhsReceive; }
		}

		protected override void OnDataSourceFactorySaved()
		{
			base.OnDataSourceFactorySaved();
			if (!Receive.WhsReceive.HasErrors && !Receive.WhsReceive.HasMessageErrors)
			{
				RedirectToReceiveDetailsPage();
			}
			else
			{
				NotificationFlags.DisplayAll = true;
				NotificationFlags.DisplayWarnings = false;
			}
		}

		protected void RedirectToReceiveDetailsPage()
		{
			Response.Redirect(String.Format((NoResString)"{0}?Ref={1}", AppInstance.WarehouseReceiveDetails, Receive.WhsReceive.PK)); // Its string formater
		}

		#endregion BusinessObject

		#region Grid setup

		protected override void SetupGrids()
		{
			SetupContainersGrid();
			SetupReceiveInventoryGrid();
		}

		protected void SetupContainersGrid()
		{
			SetupWhsDocketContainersGrid(WhsReceiveContainersGrid);
		}

		protected void SetupReceiveInventoryGrid()
		{
			SetupReceiveInventoryGridCore(WhsReceiveInventoryGrid, false);

			WhsReceiveInventoryGrid.AfterUpdateCommand += new DataGridCommandEventHandler(WhsReceiveInventoryGrid_AfterUpdateCommand);
			WhsReceiveInventoryGrid.AfterCancelCommand += new DataGridCommandEventHandler(WhsReceiveInventoryGrid_AfterCancelCommand);
			WhsReceiveInventoryGrid.AfterDeleteCommand += new DataGridCommandEventHandler(WhsReceiveInventoryGrid_AfterDeleteCommand);
			WhsReceiveInventoryGrid.AfterItemCommand += new DataGridCommandEventHandler(WhsReceiveInventoryGrid_AfterItemCommand);
			WhsReceiveInventoryGrid.OnExternallyFiredPostback += new EventHandler(WhsReceiveInventoryGrid_OnExternallyFiredPostback);
			WhsReceiveInventoryGrid.ItemDataBound += new DataGridItemEventHandler(WhsReceiveInventoryGrid_ItemDataBound);

			LinesGridAddOn.Grid = WhsReceiveInventoryGrid;
		}

		protected override WarehouseDocketLineGridAddOn GetNewLinesGridAddOn() => new WarehouseReceiveLineGridAddOn()
		{
			ProductBindTo = TrackingWhsReceiveLine.WrapperSchema.WE_OP,
			DescriptionBindTo = TrackingWhsReceiveLine.WrapperSchema.ProductDesc,
			PacksBindTo = TrackingWhsReceiveLine.WrapperSchema.WE_PackQuantity,
			PacksUQBindTo = TrackingWhsReceiveLine.WrapperSchema.WE_F3_NKPackType,
			QuantityBindTo = TrackingWhsReceiveLine.WrapperSchema.WE_TransactionQuantity,
			ProductUQBindTo = TrackingWhsReceiveLine.WrapperSchema.ProductUQ,
			ExpectedQuantityBindTo = TrackingWhsReceiveLine.WrapperSchema.WE_ClientOrderedUnits,
			Attribute1BindTo = TrackingWhsReceiveLine.WrapperSchema.WE_PartAttrib1,
			Attribute2BindTo = TrackingWhsReceiveLine.WrapperSchema.WE_PartAttrib2,
			Attribute3BindTo = TrackingWhsReceiveLine.WrapperSchema.WE_PartAttrib3,
			SerialNumberBindTo = TrackingWhsReceiveLine.WrapperSchema.WE_SerialNumber,
			ExpiryDateBindTo = TrackingWhsReceiveLine.WrapperSchema.WE_ExpiryDate,
		};

		protected int[] GetPostbackColumns()
		{
			List<int> postbackColumns = new List<int>();

			if (GetColumnIndex(TrackingWhsReceiveLine.WrapperSchema.WE_OP) != -1)
			{
				postbackColumns.Add(GetColumnIndex(TrackingWhsReceiveLine.WrapperSchema.WE_OP));
			}
			if (GetColumnIndex(TrackingWhsReceiveLine.WrapperSchema.WE_PackQuantity) != -1)
			{
				postbackColumns.Add(GetColumnIndex(TrackingWhsReceiveLine.WrapperSchema.WE_PackQuantity));
			}
			if (GetColumnIndex(TrackingWhsReceiveLine.WrapperSchema.WE_F3_NKPackType) != -1)
			{
				postbackColumns.Add(GetColumnIndex(TrackingWhsReceiveLine.WrapperSchema.WE_F3_NKPackType));
			}
			if (GetColumnIndex(TrackingWhsReceiveLine.WrapperSchema.WE_TransactionQuantity) != -1)
			{
				postbackColumns.Add(GetColumnIndex(TrackingWhsReceiveLine.WrapperSchema.WE_TransactionQuantity));
			}

			var columnIndex = GetColumnIndex(TrackingWhsReceiveLine.WrapperSchema.WE_ClientOrderedUnits);
			if (columnIndex != -1)
			{
				postbackColumns.Add(columnIndex);
			}

			return postbackColumns.ToArray();
		}

		protected int GetColumnIndex(string bindTo)
		{
			int columnIndex = -1;

			for (int i = 0; i < WhsReceiveInventoryGrid.Columns.Count; i++)
			{
				if (WhsReceiveInventoryGrid.Columns[i] is IBindTo && ((IBindTo)WhsReceiveInventoryGrid.Columns[i]).BindTo == bindTo)
				{
					columnIndex = i;
					break;
				}
			}

			return columnIndex;
		}

		protected virtual void WhsReceiveInventoryGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.EditItem && e.Item.DataItem is TrackingWhsReceiveLine)
			{
				var postbackColumns = GetPostbackColumns();

				foreach (var i in postbackColumns)
				{
					foreach (Control control in e.Item.Cells[i].Controls)
					{
						if (control is ITextChangedEvent changedControl)
						{
							changedControl.TextChanged += new EventHandler(changedControl_TextChanged);
						}
					}
				}
			}
		}

		void changedControl_TextChanged(object sender, EventArgs e)
		{
			ITextChangedEvent changedControl = sender as ITextChangedEvent;
			if (WhsReceiveInventoryGrid != null && changedControl != null)
			{
				ISelfBindingPostbackWebControl postbackControl = sender as ISelfBindingPostbackWebControl;
				if (postbackControl != null && postbackControl.HasChanges)
				{
					ISelfBindingWebControl editedControl = sender as ISelfBindingWebControl;
					WhsReceiveInventoryGrid.BindControlChangesToDataSource(editedControl);
				}

				changedControl.TextChanged -= changedControl_TextChanged;
			}
		}

		#endregion Grid setup

		#region AutoGenerated

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			WhsReceiveLabel.BindTo = null;
			UnauthorisedLabel.BindTo = null;
			NotFoundLabel.BindTo = null;
			WarehouseLabel.BindTo = null;
			WarehouseDropDown.BindTo = TrackingWhsReceive.WrapperSchema.WD_WW_Whs;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZGuid)(((TrackingWhsReceive)(null)).WhsReceive.WD_WW_Whs)));

			ReceiveRefLabel.BindTo = null;
			ReceiveRef.BindTo = TrackingWhsReceive.WrapperSchema.WD_ExternalReference;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingWhsReceive)(null)).WhsReceive.WD_ExternalReference)));

			ReceiveStatusLabel.BindTo = null;
			ReceiveStatus.BindTo = TrackingWhsReceive.WrapperSchema.WD_DocketStatusDescription;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingWhsReceive)(null)).WhsReceive.WD_DocketStatusDescription)));

			BookingDateLabel.BindTo = null;
			BookingDate.BindTo = TrackingWhsReceive.WrapperSchema.WD_BookingDate;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTimeOffset)(((TrackingWhsReceive)(null)).WhsReceive.WD_BookingDate)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingWhsReceive)(null)).WhsReceive.WD_BookingDateInfo)));

			ETALabel.BindTo = null;
			ETA.BindTo = TrackingWhsReceive.WrapperSchema.WD_ETA;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTimeOffset)(((TrackingWhsReceive)(null)).WhsReceive.WD_ETA)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingWhsReceive)(null)).WhsReceive.WD_ETAInfo)));

			TotalUnitsLabel.BindTo = null;
			TotalUnits.BindTo = TrackingWhsReceive.WrapperSchema.WD_TotalUnits;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((INumericZType)(((TrackingWhsReceive)(null)).WhsReceive.WD_TotalUnits)));

			TotalPalletsLabel.BindTo = null;
			TotalPallets.BindTo = TrackingWhsReceive.WrapperSchema.WD_TotalPallets;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((INumericZType)(((TrackingWhsReceive)(null)).WhsReceive.WD_TotalPallets)));

			this.DataSourceAssemblyName = "Enterprise.Tracking.Business";
			this.DataSourceTypeName = "Enterprise.Tracking.Business.TrackingWhsReceive";
		}
		#endregion

		#endregion

		#region Properties

		#region ReceivePK

		protected ZGuid ReceivePK
		{
			get { return GetGuidFromParameter("Ref"); }
		}
		#endregion

		#region PageTitle

		protected string PageTitle
		{
			get { return fPageTitle ?? Res.GetString("421c5195-f402-49a4-8cb3-ad0e8f863e81", "New Receipt"); }
			set { fPageTitle = value; }
		}
		string fPageTitle;
		#endregion

		#endregion

		#region EventHandlers

		protected void SaveReceive_Click(object sender, EventArgs e)
		{
			NotificationFlags.DisplayAll = true;
			SuppressErrorDialog = false;
			SaveDataGridChanges(sender);
			SaveDataSourceFactory();
		}

		protected void CancelReceive_Click(object sender, EventArgs e)
		{
			DiscardDataGridChanges(sender);
			string navigateUrl = (ReceivePK.IsValid) ? String.Format((NoResString)"{0}?Ref={1}", AppInstance.WarehouseReceiveDetails, ReceivePK) : AppInstance.WarehouseReceipts; // Its string formater
			Response.Redirect(navigateUrl);
		}

		void SaveDataGridChanges(object sender)
		{
			WhsReceiveContainersGrid.SaveChanges(sender);
			WhsReceiveInventoryGrid.SaveChanges(sender);
		}

		void DiscardDataGridChanges(object sender)
		{
			WhsReceiveContainersGrid.DiscardChanges(sender);
			WhsReceiveInventoryGrid.DiscardChanges(sender);
		}

		void WhsOrderReferencesGrid_AfterItemCommand(object source, DataGridCommandEventArgs e)
		{
			SuppressErrorDialog = true;
		}

		void WhsReceiveInventoryGrid_AfterItemCommand(object source, DataGridCommandEventArgs e)
		{
			SuppressErrorDialog = true;
		}

		void WhsReceiveInventoryGrid_AfterUpdateCommand(object source, DataGridCommandEventArgs e)
		{
			NotificationFlags.DisplayAll = true;
			CheckTotalUnits();
		}

		void WhsReceiveInventoryGrid_AfterCancelCommand(object source, DataGridCommandEventArgs e)
		{
			CheckTotalUnits();
		}

		void WhsReceiveInventoryGrid_AfterDeleteCommand(object source, DataGridCommandEventArgs e)
		{
			CheckTotalUnits();
		}

		void CheckTotalUnits()
		{
			if (Receive != null)
			{
				Receive.WhsReceive.Validation.ValidateWD_TotalUnits();
			}
		}

		void WhsReceiveContainersGrid_OnExternallyFiredPostback(object sender, EventArgs e)
		{
			if (sender != CancelReceive)
			{
				WhsReceiveContainersGrid.SaveChanges(sender);
			}
		}

		void WhsReceiveInventoryGrid_OnExternallyFiredPostback(object sender, EventArgs e)
		{
			if (sender != CancelReceive)
			{
				WhsReceiveInventoryGrid.SaveChanges(sender);
			}
		}
		#endregion
	}
}
