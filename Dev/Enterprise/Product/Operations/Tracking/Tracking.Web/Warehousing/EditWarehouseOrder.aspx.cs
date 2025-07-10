using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
	public partial class EditWarehouseOrder : WarehousingBasePage
	{
		#region Overrides

		protected override string GetPageName()
		{
			return WebTracker.Pages.EditWarehouseOrder;
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			DisabledSubmitButtons.Add(SaveOrder.ClientID);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (Order == null)
			{
				PageTitle = Res.GetString("1057f91c-60c7-4a97-ba78-cd63b2270bff", "Order Not Found");
				WhsOrderLabel.Text = Res.GetString("1057f91c-60c7-4a97-ba78-cd63b2270bff", "Order Not Found");
				NotFoundLabel.Text = Res.GetString("59b1dbd8-b0fe-48e0-bdac-f727472d5b5e", "Order was not found in the database or you don't have rights to edit it.");
				OrderContents.Visible = false;
			}
			else if (Order.IsCancelled)
			{
				RedirectToOrderDetailsPage();
			}
			else
			{
				string newOrderLabelText = Res.GetString("5d0799d1-9951-4291-84ba-b76622106654", "New Warehouse Order");
				string editOrderLabelText = Res.GetString("545eed68-60a4-4084-9b14-bd2afe8e404a", "Edit Warehouse Order");
				PageTitle = OrderPK.IsValid ? editOrderLabelText : newOrderLabelText;
				WhsOrderLabel.Text = OrderPK.IsValid ? editOrderLabelText : newOrderLabelText;
				SaveOrder.Text = OrderPK.IsValid ? Res.GetString("3ba3d5c1-6e4c-4426-80d1-28b130acc6a4", "Update Order") : Res.GetString("4f63d91b-1742-4215-a13e-43873a351a29", "Place Order");
				CancelOrder.Text = OrderPK.IsValid ? Res.GetString("904fb754-73ce-4d78-80c3-84b8980562ef", "Cancel Changes") : Res.GetString("aeb0db3c-d8a6-4ece-8df8-b9f02015550a", "Cancel Order");
				NotFoundError.Visible = false;
				OrderContents.Visible = true;
			}

			if (Order != null && Order.WhsOrder.Lines.Count > 0)
			{
				WarehouseDropDown.AutoPostBack = true;
			}

			if (IsPostBack)
			{
				SaveErrorMessage.Visible = false;
			}
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanEditWarehouseOrders; }
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
			SetupOrderLinesGrid_PreBind();

			if (!Page.IsPostBack)
			{
				NotificationFlags.DisplayAll = false;
				if (Order != null && Order.WhsOrder.GetShortfallExistsStatus())
				{
					NotificationFlags.DisplayWarnings = true;
				}
			}
		}

		#endregion Overrides

		#region BusinessObject

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.EditWarehouseOrderPage;
		}

		protected override BusinessObject GetNewDataSource()
		{
			TrackingWhsOrder fOrder = null;
			if (OrderPK.IsValid)
			{
				fOrder = TrackingWhsOrder.FromPKFilteredByContact(Factory, OrderPK, SiteUser);
			}
			else if (Session[Inventory.ShouldUseWhsOrderShoppingCart] != null && Session[Inventory.WhsOrderShoppingCartIndexer] != null)
			{
				fOrder = Session[Inventory.WhsOrderShoppingCartIndexer] as TrackingWhsOrder;
				Session[Inventory.ShouldUseWhsOrderShoppingCart] = null;
				Session[Inventory.WhsOrderShoppingCartIndexer] = null;
			}
			else
			{
				fOrder = TrackingHelper.Get(Factory.New<WhsOrder>());
				AssignDefaultWarehouseIfPossible(fOrder);
			}
			if (fOrder != null)
			{
				fOrder.SiteUser = SiteUser;
				fOrder.WhsOrder.WD_OH_Client = SiteUser.LoggedInOrganisation.PK;
				fOrder.Logs.AutoCreatedLogDefaultSL_Reference = SiteUser.ContactAndCompanyReference;
				DataSourceIndexer = SaveDataSource(ZGuid.NewZGuid(), fOrder);
			}
			return fOrder;
		}

		protected void AssignDefaultWarehouseIfPossible(TrackingWhsOrder order)
		{
			if (order.WhsOrder.WD_WW_Whs.IsEmpty)
			{
				WhsWarehouse[] warehouses = Factory.Load<WhsWarehouse>(OrgRestrictionFilterFactory.Instance.GetFilter<WhsWarehouse>());
				if (warehouses.Length == 1)
				{
					order.WhsOrder.WD_WW_Whs = warehouses[0].PK;
				}
			}
		}

		protected TrackingWhsOrder Order
		{
			get { return DataSource as TrackingWhsOrder; }
		}

		protected override void OnDataSourceFactorySaved()
		{
			base.OnDataSourceFactorySaved();
			if (!Order.WhsOrder.HasErrors && !Order.WhsOrder.HasMessageErrors)
			{
				Session[Inventory.WhsOrderShoppingCartIndexer] = null;
				RedirectToOrderDetailsPage();
			}
			else
			{
				NotificationFlags.DisplayAll = true;
				NotificationFlags.DisplayWarnings = false;
			}
		}

		protected void RedirectToOrderDetailsPage()
		{
			Response.Redirect(String.Format("{0}?{1}={2}", AppInstance.WarehouseOrderDetailsPage, RefParameterName, Order.WhsOrder.PK));
		}

		#endregion BusinessObject

		#region Grid setup

		protected override void SetupGrids()
		{
			SetupOrderLinesGrid();
			SetupReferenceGrid();
		}

		protected void SetupReferenceGrid()
		{
			WhsOrderReferencesGrid.Columns.Add(new ZDropDownListColumn(Res.GetString("2326c375-6165-4b50-87cd-98bda6e924fa", "Ref Type"), WhsDocketReferenceSchema.WX_RefType.Name, "Lookups.ReferenceTypes"));
			WhsOrderReferencesGrid.Columns.Add(new ZTextEditColumn(Res.GetString("148d3589-f2c9-42fe-b0c7-29ad72710383", "Reference"), WhsDocketReferenceSchema.WX_Reference.Name));
			WhsOrderReferencesGrid.AfterItemCommand += new DataGridCommandEventHandler(WhsOrderReferencesGrid_AfterItemCommand);
			WhsOrderReferencesGrid.OnExternallyFiredPostback += new EventHandler(WhsOrderReferencesGrid_OnExternallyFiredPostback);
		}

		protected void SetupOrderLinesGrid()
		{
			// this is the editable order line

			ZBindToChecker.CheckBindTo((ZGuid)((TrackingWhsOrderLine)null).WhsOrderLine.WE_OP);
			ZBindToChecker.CheckBindTo((OrgSupplierPartCollection)((TrackingWhsOrderLine)null).WhsOrderLine.Lookups.SupplierParts);
			var productColumn = new ZFindBoxColumn(Res.GetString("d3622d89-4c12-41fc-a76c-a5f0824e8841", "Product"), TrackingWhsOrderLine.WrapperSchema.WE_OP, TrackingWhsOrderLine.GetSchemaPath("Lookups.SupplierParts"));
			productColumn.ValueFieldName = "PK";
			productColumn.ModuleID = WebModuleIDs.OrgSupplierPartTracking;
			WhsOrderLinesGrid.Columns.Add(productColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsOrderLine)null).ProductDescription);
			var productDescriptionColumn = new ZTextEditColumn(Res.GetString("585a8978-4808-4961-aea2-50e62edc54f0", "Description"), TrackingWhsOrderLine.WrapperSchema.ProductDescription);
			productDescriptionColumn.ReadOnly = true;
			WhsOrderLinesGrid.Columns.Add(productDescriptionColumn);

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsOrderLine)null).WhsOrderLine.WE_PackQuantity);
			var packsColumn = new ZCalcEditColumn(Res.GetString("30512d3f-9b7b-4d91-a52e-d6d53345a8df", "Packs"), TrackingWhsOrderLine.WrapperSchema.WE_PackQuantity);
			packsColumn.ID = "OrderLinePacks";
			WhsOrderLinesGrid.Columns.Add(packsColumn);

			// ToDo: Why check bind to ProductUQ? We actually bind to WE_F3_NKPackType. should be 'ZBindToChecker.CheckBindTo((ZString)((TrackingWhsOrderLine)null).WrapperSchema.WE_F3_NKPackType);'
			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsOrderLine)null).WhsOrderLine.ProductUQ);
			ZBindToChecker.CheckBindTo((CodeDescriptionPairList)((TrackingWhsOrderLine)null).WhsOrderLine.Lookups.PackTypes);
			var packsUQColumn = new ZDropDownListColumn(Res.GetString("733ff98c-daff-4baf-b639-e25f3380309d", "Packs UQ"), TrackingWhsOrderLine.WrapperSchema.WE_F3_NKPackType, TrackingWhsOrderLine.GetSchemaPath("Lookups.PackTypes"));
			packsUQColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			packsUQColumn.ID = "OrderLinePackType";
			WhsOrderLinesGrid.Columns.Add(packsUQColumn);

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsOrderLine)null).WhsOrderLine.WE_TransactionQuantity);
			ZBindToChecker.CheckBindTo((ZByte)((TrackingWhsOrderLine)null).WhsOrderLine.SupplierPart.OP_CountDecimalPlaces);
			var quantityColumn = new ZCalcEditColumn(Res.GetString("73f8aa23-e7a8-44d5-980f-61ca596b7352", "Quantity"), TrackingWhsOrderLine.WrapperSchema.WE_TransactionQuantity);
			quantityColumn.BindToDecimals = TrackingWhsOrderLine.GetSchemaPath("SupplierPart+OP_CountDecimalPlaces");
			quantityColumn.ID = "OrderLineUnits";
			WhsOrderLinesGrid.Columns.Add(quantityColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsOrderLine)null).WhsOrderLine.ProductUQ);
			ZBindToChecker.CheckBindTo((CodeDescriptionPairList)((TrackingWhsOrderLine)null).WhsOrderLine.Lookups.PackTypes);
			ZDropDownListColumn uQColumn = new ZDropDownListColumn(Res.GetString("f4268285-bb2c-4f6c-82aa-14f45ac7044f", "UQ"), TrackingWhsOrderLine.WrapperSchema.ProductUQ, TrackingWhsOrderLine.GetSchemaPath("Lookups.PackTypes"));
			uQColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			uQColumn.ReadOnly = true;
			WhsOrderLinesGrid.Columns.Add(uQColumn);

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsOrderLine)null).WhsOrderLine.WE_ShortfallQuantityCached);
			ZBindToChecker.CheckBindTo((ZByte)((TrackingWhsOrderLine)null).WhsOrderLine.SupplierPart.OP_CountDecimalPlaces);
			var shortfallColumn = new ZCalcEditColumn(Res.GetString("c4395a48-9b85-44f2-8ec8-1ecdc1fd57e2", "Shortfall Qty."), TrackingWhsOrderLine.WrapperSchema.WE_ShortfallQuantityCached);
			shortfallColumn.BindToDecimals = TrackingWhsOrderLine.GetSchemaPath("SupplierPart+OP_CountDecimalPlaces");
			shortfallColumn.ReadOnly = true;
			WhsOrderLinesGrid.Columns.Add(shortfallColumn);

			var partManager = Page.SiteUser.LoggedInOrganisation.PartAttributeManager;

			ZBindToChecker.CheckBindTo(((TrackingWhsOrderLine)null).WhsOrderLine.WE_PartAttrib1);
			ZBindToChecker.CheckBindTo(((TrackingWhsOrderLine)null).WhsOrderLine.WE_PartAttrib2);
			ZBindToChecker.CheckBindTo(((TrackingWhsOrderLine)null).WhsOrderLine.WE_PartAttrib3);
			ZBindToChecker.CheckBindTo(((TrackingWhsOrderLine)null).WhsOrderLine.WE_SerialNumber);
			var clientDependantColumn1 = new ZTextEditColumn(partManager.PartAttributeName1, TrackingWhsOrderLine.WrapperSchema.WE_PartAttrib1) { CanBeEnabledByClient = true };
			var clientDependantColumn2 = new ZTextEditColumn(partManager.PartAttributeName2, TrackingWhsOrderLine.WrapperSchema.WE_PartAttrib2) { CanBeEnabledByClient = true };
			var clientDependantColumn3 = new ZTextEditColumn(partManager.PartAttributeName3, TrackingWhsOrderLine.WrapperSchema.WE_PartAttrib3) { CanBeEnabledByClient = true };
			var serialNumberColumn = new ZTextEditColumn(Res.GetString("55578909-6fa1-4517-94da-2ddfc7f124c8", "Serial Number"), TrackingWhsOrderLine.WrapperSchema.WE_SerialNumber) { CanBeEnabledByClient = true };

			AddClientDependantColumn(WhsOrderLinesGrid.Columns, partManager.IsPartAttributeUsedByOrganisation(1), clientDependantColumn1);
			AddClientDependantColumn(WhsOrderLinesGrid.Columns, partManager.IsPartAttributeUsedByOrganisation(2), clientDependantColumn2);
			AddClientDependantColumn(WhsOrderLinesGrid.Columns, partManager.IsPartAttributeUsedByOrganisation(3), clientDependantColumn3);
			AddClientDependantColumn(
				WhsOrderLinesGrid.Columns,
				partManager.IsSerialNumberUsedByOrganisation,
				serialNumberColumn);

			CrossDocksLinkColumn = new ZHyperLinkColumn(Res.GetString("db7337d9-62fa-4343-919a-19dd1d716e89", "Reserved"), "");
			CrossDocksLinkColumn.DataTextFormatString = Res.GetString("e151de31-9ea9-494e-847f-7a4105cb318b", "{0} Edit", "{0}");
			CrossDocksLinkColumn.DataTextFields = new string[] { TrackingWhsOrderLine.WrapperSchema.AllocatedQuantityAsString };
			CrossDocksLinkColumn.DataNavigateUrlFields = new string[] { TrackingWhsOrderLine.WrapperSchema.WE_PK };
			CrossDocksLinkColumn.Target = (NoResString)"_blank"; // javascript code
			CrossDocksLinkColumn.WindowStyle = Global.WarehouseOrderLineAllocationEditPopupWindowStyle;
			WhsOrderLinesGrid.Columns.Add(CrossDocksLinkColumn);

			WhsOrderLinesGrid.AfterUpdateCommand += new DataGridCommandEventHandler(WhsOrderLinesGrid_AfterUpdateCommand);
			WhsOrderLinesGrid.AfterCancelCommand += new DataGridCommandEventHandler(WhsOrderLinesGrid_AfterCancelCommand);
			WhsOrderLinesGrid.AfterDeleteCommand += new DataGridCommandEventHandler(WhsOrderLinesGrid_AfterDeleteCommand);
			WhsOrderLinesGrid.AfterItemCommand += new DataGridCommandEventHandler(WhsOrderLinesGrid_AfterItemCommand);
			WhsOrderLinesGrid.OnExternallyFiredPostback += new EventHandler(WhsOrderLinesGrid_OnExternallyFiredPostback);
			WhsOrderLinesGrid.ItemDataBound += new DataGridItemEventHandler(WhsOrderLinesGrid_ItemDataBound);

			LinesGridAddOn.Grid = WhsOrderLinesGrid;
		}

		protected ZHyperLinkColumn CrossDocksLinkColumn { get; set; }

		void SetupOrderLinesGrid_PreBind()
		{
			// Must be run after DataSourceIndexer is set.
			if (CrossDocksLinkColumn == null)
			{
				throw new InvalidOperationException("CrossDocksLink column should exist in OrderLinesGrid after being created and added in OnLoad");
			}

			CrossDocksLinkColumn.DataNavigateUrlFormatString = AppInstance.WarehouseOrderLineAllocationPage + (NoResString)"?Ref={0}&Mode=Edit&Order=" + this.DataSourceIndexer.ToString() + ShowInPopupParam; // Request parameter names
		}

		protected override WarehouseDocketLineGridAddOn GetNewLinesGridAddOn() => new WarehouseOrderLineGridAddOn()
		{
			ProductBindTo = TrackingWhsOrderLine.WrapperSchema.WE_OP,
			DescriptionBindTo = TrackingWhsOrderLine.WrapperSchema.ProductDescription,
			PacksBindTo = TrackingWhsOrderLine.WrapperSchema.WE_PackQuantity,
			PacksUQBindTo = TrackingWhsOrderLine.WrapperSchema.WE_F3_NKPackType,
			QuantityBindTo = TrackingWhsOrderLine.WrapperSchema.WE_TransactionQuantity,
			ProductUQBindTo = TrackingWhsOrderLine.WrapperSchema.ProductUQ,
			ShortfallBindTo = TrackingWhsOrderLine.WrapperSchema.WE_ShortfallQuantityCached,
			Attribute1BindTo = TrackingWhsOrderLine.WrapperSchema.WE_PartAttrib1,
			Attribute2BindTo = TrackingWhsOrderLine.WrapperSchema.WE_PartAttrib2,
			Attribute3BindTo = TrackingWhsOrderLine.WrapperSchema.WE_PartAttrib3,
			SerialNumberBindTo = TrackingWhsOrderLine.WrapperSchema.WE_SerialNumber
		};

		protected int[] GetPostbackColumns()
		{
			List<int> postbackColumns = new List<int>();

			if (GetColumnIndex(TrackingWhsOrderLine.WrapperSchema.WE_OP) != -1)
			{
				postbackColumns.Add(GetColumnIndex(TrackingWhsOrderLine.WrapperSchema.WE_OP));
			}
			if (GetColumnIndex(TrackingWhsOrderLine.WrapperSchema.WE_PackQuantity) != -1)
			{
				postbackColumns.Add(GetColumnIndex(TrackingWhsOrderLine.WrapperSchema.WE_PackQuantity));
			}
			if (GetColumnIndex(TrackingWhsOrderLine.WrapperSchema.WE_F3_NKPackType) != -1)
			{
				postbackColumns.Add(GetColumnIndex(TrackingWhsOrderLine.WrapperSchema.WE_F3_NKPackType));
			}
			if (GetColumnIndex(TrackingWhsOrderLine.WrapperSchema.WE_TransactionQuantity) != -1)
			{
				postbackColumns.Add(GetColumnIndex(TrackingWhsOrderLine.WrapperSchema.WE_TransactionQuantity));
			}

			return postbackColumns.ToArray();
		}

		protected int GetColumnIndex(string bindTo)
		{
			int columnIndex = -1;

			for (int i = 0; i < WhsOrderLinesGrid.Columns.Count; i++)
			{
				if (WhsOrderLinesGrid.Columns[i] is IBindTo && ((IBindTo)WhsOrderLinesGrid.Columns[i]).BindTo == bindTo)
				{
					columnIndex = i;
					break;
				}
			}

			return columnIndex;
		}

		protected virtual void WhsOrderLinesGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.EditItem)
			{
				TrackingWhsOrderLine orderLine = e.Item.DataItem as TrackingWhsOrderLine;

				if (orderLine != null)
				{
					int[] postbackColumns = GetPostbackColumns();

					foreach (int i in postbackColumns)
					{
						foreach (Control control in e.Item.Cells[i].Controls)
						{
							ITextChangedEvent changedControl = control as ITextChangedEvent;
							if (changedControl != null)
							{
								changedControl.TextChanged += new EventHandler(changedControl_TextChanged);
							}
						}
					}
				}
			}
		}

		void changedControl_TextChanged(object sender, EventArgs e)
		{
			ITextChangedEvent changedControl = sender as ITextChangedEvent;
			if (WhsOrderLinesGrid != null && changedControl != null)
			{
				ISelfBindingPostbackWebControl postbackControl = sender as ISelfBindingPostbackWebControl;
				if (postbackControl != null && postbackControl.HasChanges)
				{
					ISelfBindingWebControl editedControl = sender as ISelfBindingWebControl;
					WhsOrderLinesGrid.BindControlChangesToDataSource(editedControl);
				}

				changedControl.TextChanged -= changedControl_TextChanged;
			}
		}

		#endregion Grid setup

		#region Address Controls Setup

		void SetupGoodsBilledToAddressControl()
		{
			GoodsBilledToDocAddress.NewOrgRelationType = NewOrgRelationTypes.Buyer;
			GoodsBilledToDocAddress.OrgModuleID = WebModuleIDs.OrganisationTracking;
		}

		void SetupConsigneeAddressControl()
		{
			ConsigneeAddress.NewOrgRelationType = NewOrgRelationTypes.Buyer;
			ConsigneeAddress.OrgModuleID = WebModuleIDs.OrganisationTracking;
		}

		#endregion

		#region AutoGenerated

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();

			base.OnInit(e);
			SetupGoodsBilledToAddressControl();
			SetupConsigneeAddressControl();
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			WhsOrderLabel.BindTo = null;
			UnauthorisedLabel.BindTo = null;
			NotFoundLabel.BindTo = null;
			WarehouseLabel.BindTo = null;
			WarehouseDropDown.BindTo = TrackingWhsOrder.WrapperSchema.WD_WW_Whs;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZGuid)(((TrackingWhsOrder)(null)).WhsOrder.WD_WW_Whs)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZGuid)(((TrackingWhsOrder)(null)).WhsOrder.WD_WW_Whs)));
			WarehouseDropDown.BindTo = TrackingWhsOrder.WrapperSchema.WD_WW_Whs;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZGuid)(((TrackingWhsOrder)(null)).WhsOrder.WD_WW_Whs)));
			OrderNumberLabel.BindTo = null;
			OrderNumber.BindTo = TrackingWhsOrder.WrapperSchema.WD_ExternalReference;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingWhsOrder)(null)).WhsOrder.WD_ExternalReference)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingWhsOrder)(null)).WhsOrder.WD_ExternalReference)));
			OrderNumber.BindTo = TrackingWhsOrder.WrapperSchema.WD_ExternalReference;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingWhsOrder)(null)).WhsOrder.WD_ExternalReference)));
			OrderStatusLabel.BindTo = null;
			OrderStatus.BindTo = TrackingWhsOrder.WrapperSchema.WD_DocketStatusDescription;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingWhsOrder)(null)).WhsOrder.WD_DocketStatusDescription)));
			RequiredDateLabel.BindTo = null;
			RequiredByDate.BindTo = TrackingWhsOrder.WrapperSchema.TrackingRequiredDate;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingWhsOrder)(null)).TrackingRequiredDate)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingWhsOrder)(null)).TrackingRequiredDateInfo)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingWhsOrder)(null)).TrackingRequiredDate)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingWhsOrder)(null)).TrackingRequiredDateInfo)));
			RequiredByDate.BindTo = TrackingWhsOrder.WrapperSchema.TrackingRequiredDate;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingWhsOrder)(null)).TrackingRequiredDate)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingWhsOrder)(null)).TrackingRequiredDateInfo)));
			TotalUnitsLabel.BindTo = null;
			TotalUnits.BindTo = TrackingWhsOrder.WrapperSchema.WD_TotalUnits;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((INumericZType)(((TrackingWhsOrder)(null)).WhsOrder.WD_TotalUnits)));

			this.DataSourceAssemblyName = "Enterprise.Tracking.Business";
			this.DataSourceTypeName = "Enterprise.Tracking.Business.TrackingWhsOrder";

			SaveErrorMessage.BindTo = null;
			SaveErrorMessage.Visible = false;
		}
		#endregion

		#endregion

		#region Properties

		#region OrderPK

		protected ZGuid OrderPK => GetGuidFromParameter(RefParameterName);
		#endregion

		#region PageTitle

		protected string PageTitle
		{
			get => fPageTitle ?? Res.GetString("2eb73565-1623-4d3e-8440-3116ec04111b", "New Order");
			set => fPageTitle = value;
		}
		string fPageTitle;

		#endregion

		#endregion

		#region EventHandlers

		protected void SaveOrder_Click(object sender, EventArgs e)
		{
			NotificationFlags.DisplayAll = true;
			SuppressErrorDialog = false;
			SaveDataGridChanges(sender);
			var sucessful = DataSourceFactorySave();
			if (!sucessful)
			{
				if (Order.RowErrors.Count() > 0)
				{
					SaveErrorMessage.Visible = true;
					SaveErrorMessage.Text = Order.RowErrors.GetFirstMessage();
				}
			}
		}

		protected virtual bool DataSourceFactorySave()
		{
			return SaveDataSourceFactory();
		}

		protected void CancelOrder_Click(object sender, EventArgs e)
		{
			DiscardDataGridChanges(sender);
			string navigateUrl = (OrderPK.IsValid) ? String.Format("{0}?{1}={2}", AppInstance.WarehouseOrderDetailsPage, RefParameterName, OrderPK) : AppInstance.WarehouseOrders;
			Response.Redirect(navigateUrl);
		}

		void SaveDataGridChanges(object sender)
		{
			WhsOrderReferencesGrid.SaveChanges(sender);
			WhsOrderLinesGrid.SaveChanges(sender);
		}

		void DiscardDataGridChanges(object sender)
		{
			WhsOrderReferencesGrid.DiscardChanges(sender);
			WhsOrderLinesGrid.DiscardChanges(sender);
		}

		void WhsOrderReferencesGrid_AfterItemCommand(object source, DataGridCommandEventArgs e)
		{
			SuppressErrorDialog = true;
		}

		void WhsOrderLinesGrid_AfterItemCommand(object source, DataGridCommandEventArgs e)
		{
			SuppressErrorDialog = true;
		}

		void WhsOrderLinesGrid_AfterUpdateCommand(object source, DataGridCommandEventArgs e)
		{
			NotificationFlags.DisplayAll = true;
			CheckTotalUnits();
		}

		void WhsOrderLinesGrid_AfterCancelCommand(object source, DataGridCommandEventArgs e)
		{
			CheckTotalUnits();
		}

		void WhsOrderLinesGrid_AfterDeleteCommand(object source, DataGridCommandEventArgs e)
		{
			CheckTotalUnits();
		}

		void CheckTotalUnits()
		{
			if (Order != null)
			{
				Order.WhsOrder.Validation.ValidateWD_TotalUnits();
			}
		}

		void WhsOrderReferencesGrid_OnExternallyFiredPostback(object sender, EventArgs e)
		{
			if (sender != CancelOrder)
			{
				WhsOrderReferencesGrid.SaveChanges(sender);
			}
		}

		void WhsOrderLinesGrid_OnExternallyFiredPostback(object sender, EventArgs e)
		{
			if (sender != CancelOrder)
			{
				WhsOrderLinesGrid.SaveChanges(sender);
			}
		}
		#endregion
	}
}
