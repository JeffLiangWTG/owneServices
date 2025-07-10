using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
	public partial class EditOrder : BasePageWithAuthorisation
	{
		#region Overrides

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			DisabledSubmitButtons.Add(SaveOrder.ClientID);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			NotificationFlags.DisplayAll = false;

			if (Order == null)
			{
				PageTitle = Res.GetString("6e00aaa5-2354-45e4-bc44-813347daa2ff", "Order Not Found");
				OrderLabel.Text = Res.GetString("6e00aaa5-2354-45e4-bc44-813347daa2ff", "Order Not Found");
			}
			else if (Order.JD_OrderStatus == Constants.OrderStatus.Cancelled)
			{
				RedirectToOrderDetailsPage();
			}
			PageTitle = OrderPK.IsValid ? Res.GetString("07cc6b58-787c-4834-ab57-e7705fed68dd", "Edit Order") : Res.GetString("3950d1ac-4c87-450a-bf79-a9f91138ed7d", "New Order");
			OrderLabel.Text = OrderPK.IsValid ? Res.GetString("07cc6b58-787c-4834-ab57-e7705fed68dd", "Edit Order") : Res.GetString("3950d1ac-4c87-450a-bf79-a9f91138ed7d", "New Order");
		}

		protected override void OnPreBind()
		{
			if (!Globals.IsTest)
			{
				Weight.Decimals = DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(Order, Order.JD_ActualWeightInfo.PropertyDescriptor);
				Volume.Decimals = DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(Order, Order.JD_ActualVolumeInfo.PropertyDescriptor);
			}

			if (!IsSetupCompleteForOrderLinesGridCustomFields)
			{
				SetupOrderLinesGridCustomFields();
			}
		}

		bool IsSetupCompleteForOrderLinesGridCustomFields;

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanEditOrders; }
		}
		#endregion Overrides

		#region BusinessObject

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			TrackingOrder fOrder;
			if (OrderPK.IsValid)
			{
				fOrder = TrackingOrder.FromPKFilteredByContact(Factory, OrderPK, SiteUser);
				if (fOrder != null)
				{
					fOrder.TemporaryServiceLevelDescription = fOrder.JD_RS_NKServiceLevel_NI;
				}
			}
			else
			{
				fOrder = Factory.New<TrackingOrder>();
				fOrder.LoggedInContact = SiteUser.LoggedInUser;
				fOrder.BuyerPK = SiteUser.LoggedInOrganisation.PK;
			}
			if (fOrder != null)
			{
				fOrder.Logs.AutoCreatedLogDefaultSL_Reference = SiteUser.ContactAndCompanyReference;
			}
			return fOrder;
		}

		protected TrackingOrder Order
		{
			get { return DataSource as TrackingOrder; }
		}

		protected override void OnDataSourceFactorySaved()
		{
			base.OnDataSourceFactorySaved();
			RedirectToOrderDetailsPage();
		}

		#endregion BusinessObject

		#region Properties

		protected
#if DEBUG
		virtual
#endif
		ZGuid OrderPK
		{
			get { return GetGuidFromParameter("Ref"); }
		}

		protected string PageTitle
		{
			get { return fPageTitle ?? Res.GetString("3950d1ac-4c87-450a-bf79-a9f91138ed7d", "New Order"); }
			set { fPageTitle = value; }
		}
		string fPageTitle;

		#endregion Properties

		#region Grid setup

		protected override void SetupGrids()
		{
			SetupOrderLinesGrid();
		}

		protected void SetupOrderLinesGridCustomFields()
		{
			var attributes = new AttributeManager().GetLineAttributes(AttributeManager.AttributeModules.Order, Order?.Buyer, SiteUser?.LoggedInOrganisation);
			foreach (var attribute in attributes)
			{
				var attribColumn = attribute.Column;
				var column = ZTemplateColumn.GetNew(attribute.Caption, attribColumn, ZString.Empty);
				OrderLinesGrid.Columns.Add(column);
			}

			IsSetupCompleteForOrderLinesGridCustomFields = true;
		}

		protected void SetupOrderLinesGrid()
		{
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("3690e69f-850a-4532-9d38-b5da3d882318", "Line#"), OrderLine.Schema.JO_LineNo) { Decimals = 0 });
			OrderLinesGrid.Columns.Add(new ZCodeFindBoxColumn(Res.GetString("fded6d9f-18ad-48d4-870d-2ad0a73a691b", "Part#"), OrderLine.Schema.JO_Partno) { ModuleID = WebModuleIDs.OrgSupplierPartTracking, AutoPostBack = true });
			OrderLinesGrid.Columns.Add(new ZTextEditColumn(Res.GetString("43d01ff1-edb6-43b3-b9e4-bb246b278a22", "Description"), OrderLine.Schema.JO_Description));
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("f34b3f31-d6aa-4032-8099-a51867a6f16b", "Inner Packs"), OrderLine.Schema.JO_InnerPacks));
			OrderLinesGrid.Columns.Add(new ZDropEditColumn(Res.GetString("b30be66b-f87e-4d1f-a60d-098a97399799", "Inner Package Type"), OrderLine.Schema.JO_InnerPacksUQ) { BindToList = "JO_F3_NKPackType_List", EditorWidth = 100, DisplayStyle = OComboBoxDropDownStyle.CodeAndDescription });
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("c15307e9-8d7a-4539-8f88-f79957c542ba", "Outer Packs"), OrderLine.Schema.JO_OuterPacks));
			OrderLinesGrid.Columns.Add(new ZDropEditColumn(Res.GetString("b7c14ec9-d2ec-4676-9dc5-91ac517584d7", "Outer Package Type"), OrderLine.Schema.JO_OuterPacksUQ) { BindToList = "JO_F3_NKPackType_List", EditorWidth = 100, DisplayStyle = OComboBoxDropDownStyle.CodeAndDescription });
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("aaf0e8f6-b8b4-4c32-ab87-9771b6069707", "Qty Ordered"), OrderLine.Schema.JO_Quantity));
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("d8a9cda3-dadf-48b3-9d28-c85906d7c909", "Qty Invoiced"), OrderLine.Schema.JO_QtyInvoiced));
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("2a4c6a46-5eeb-4eb0-a25b-63a98adc166d", "Qty Received"), OrderLine.Schema.JO_QtyReceived));
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("2d619696-b474-41f7-884a-ab1f3647e5f5", "Qty Remaining"), OrderLine.Schema.JO_QuantityRemaining) { ReadOnly = true });
			OrderLinesGrid.Columns.Add(new ZDropDownListColumn(Res.GetString("cbad40f4-65a2-46cf-aee6-05ec507d4726", "Unit of Qty"), OrderLine.Schema.JO_F3_NKPackType) { BindToList = "JO_F3_NKPackType_List", EditorWidth = 100 });
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("af6ef4d7-16bc-4d7b-b6fc-6adf6d25e04a", "Item Price"), OrderLine.Schema.JO_ItemPrice));
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("c4d33ce0-d646-41b5-8fcb-e1a31edc80b4", "Total Price"), OrderLine.Schema.JO_LinePrice));
			OrderLinesGrid.Columns.Add(new ZDateTimeColumn(Res.GetString("4b572dfc-7e2f-4aeb-a034-ce1f7658a9d1", "Required In Store Date"), OrderLine.Schema.JO_LineDropDate, ZDateTimePickerFormat.Short));
			OrderLinesGrid.Columns.Add(new ZDropEditColumn(Res.GetString("d5ac4472-d694-6986-43cb-db0b95613066", "Incoterm"), OrderLine.Schema.JO_INCO) { BindToList = "JO_INCO_List", EditorWidth = 100, DisplayStyle = OComboBoxDropDownStyle.CodeAndDescription });
			OrderLinesGrid.Columns.Add(new ZTextEditColumn(Res.GetString("ad6d01ae-c960-474c-8352-406a4e5746b8", "Additional Terms"), OrderLine.Schema.JO_AdditionalTerms) { EditorWidth = 200 });
			OrderLinesGrid.Columns.Add(new ZTextEditColumn(Res.GetString("e5a2aff8-da4c-4a54-ab15-5810b1d24d98", "Confirm Number"), OrderLine.Schema.JO_ConfirmationNum));
			OrderLinesGrid.Columns.Add(new ZDateTimeColumn(Res.GetString("69bbf559-bc47-4267-99a6-cc6e63c02061", "Confirm Date"), OrderLine.Schema.JO_ConfirmationDate, ZDateTimePickerFormat.Short));
			OrderLinesGrid.Columns.Add(new ZDateTimeColumn(Res.GetString("cd811b5c-cf7c-4f0f-a597-38af628efe3e", "Required Ex Works Date"), OrderLine.Schema.JO_ExWorksDate, ZDateTimePickerFormat.Short));

			ZBindToChecker.CheckBindTo(((ICodeDescriptionPairList)((OrderLine)null).JO_F3_NKPackType_List));
			ZBindToChecker.CheckBindTo((ZDateTime)((OrderLine)null).JO_LineDropDate);
			ZBindToChecker.CheckBindTo(((ICodeDescriptionPairList)((OrderLine)null).JO_INCO_List));

			OrderLinesGrid.OnExternallyFiredPostback += OrderLinesGrid_OnExternallyFiredPostback;
			OrderLinesGrid.ItemDataBound += OrderLinesGrid_ItemDataBound;
		}

		void OrderLinesGrid_OnExternallyFiredPostback(object sender, EventArgs e)
		{
			if (sender != CancelOrder)
			{
				OrderLinesGrid.SaveChanges(sender);
			}
		}

		protected virtual void OrderLinesGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.EditItem)
			{
				int productEditCellNumber = GetProductColumnIndex();
				OrderLine orderLine = e.Item.DataItem as OrderLine;

				if (orderLine != null && productEditCellNumber > -1 && e.Item.Cells.Count > productEditCellNumber &&
					e.Item.Cells[productEditCellNumber].Controls.Count > 0)
				{
					ZFindBox editedFindBox = e.Item.Cells[productEditCellNumber].Controls[0] as ZFindBox;
					if (editedFindBox != null)
					{
						editedFindBox.TextChanged += EditedFindBox_TextChanged;
					}
				}
			}
		}

		protected int GetProductColumnIndex()
		{
			int columnIndex = -1;

			for (int i = 0; i < OrderLinesGrid.Columns.Count; i++)
			{
				ZTemplateColumn zColumn = OrderLinesGrid.Columns[i] as ZTemplateColumn;

				if (zColumn != null && zColumn.BindTo == OrderLine.Schema.JO_Partno)
				{
					columnIndex = i;
					break;
				}
			}

			return columnIndex;
		}

		void EditedFindBox_TextChanged(object sender, EventArgs e)
		{
			ZFindBox editedFindBox = sender as ZFindBox;
			if (editedFindBox != null)
			{
				if (OrderLinesGrid != null && editedFindBox.HasChanges)
				{
					ISelfBindingWebControl editedControl = sender as ISelfBindingWebControl;
					OrderLinesGrid.BindControlChangesToDataSource(editedControl);
				}

				editedFindBox.TextChanged -= EditedFindBox_TextChanged;
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
			OrderLabel.BindTo = null;

			UnauthorisedLabel.BindTo = null;

			BuyerOrderNumberLabel.BindTo = null;

			BuyerOrderNumber.BindTo = "JD_OrderNumber";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingOrder)(null)).JD_OrderNumber)));
			CurrencyLabel.BindTo = null;
			ServiceLevelLabel.BindTo = null;
			ServiceLavel.BindTo = "JD_RS_NKServiceLevel_NI";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingOrder)(null)).JD_RS_NKServiceLevel_NIInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingOrder)(null)).JD_RS_NKServiceLevel_NI)));
			INCOTermsLabel.BindTo = null;
			INCOTerms.BindTo = "JD_IncoTerm";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingOrder)(null)).JD_IncoTermInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingOrder)(null)).JD_IncoTerm)));
			TransportModeLabel.BindTo = null;
			TransportMode.BindTo = "JD_TransportMode";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingOrder)(null)).JD_TransportModeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingOrder)(null)).JD_TransportMode)));
			ContainerModeLabel.BindTo = null;
			ContainerMode.BindTo = "JD_ContainerMode";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingOrder)(null)).JD_ContainerModeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingOrder)(null)).JD_ContainerMode)));
			OrderDateLabel.BindTo = null;
			OrderDate.BindTo = "JD_OrderDate";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingOrder)(null)).JD_OrderDate)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingOrder)(null)).JD_OrderDateInfo)));

			ReqExWorksDate.BindTo = "JD_ExWorksRequiredBy";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingOrder)(null)).JD_ExWorksRequiredBy)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingOrder)(null)).JD_ExWorksRequiredByInfo)));
			ReqInStoreDate.BindTo = "JD_DeliveryRequiredBy";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingOrder)(null)).JD_DeliveryRequiredBy)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingOrder)(null)).JD_DeliveryRequiredByInfo)));

			OrderLineDetails.BindTo = null;
			this.DataSourceAssemblyName = "Enterprise.Tracking.Business";
			this.DataSourceTypeName = "Enterprise.Tracking.Business.TrackingOrder";
		}

		#endregion

		#endregion AutoGenerated

		#region Event Handlers

		protected void RedirectToOrderDetailsPage()
		{
			Response.Redirect(String.Format((NoResString)"{0}?Ref={1}", AppInstance.OrderDetailsPage, Order.PK)); // Partial URL
		}

		protected void SaveOrder_Click(object sender, EventArgs e)
		{
			SaveDataSourceFactory();
		}

		protected void CancelOrder_Click(object sender, EventArgs e)
		{
			string navigateUrl = (OrderPK.IsValid) ? String.Format((NoResString)"{0}?Ref={1}", AppInstance.OrderDetailsPage, OrderPK) : AppInstance.OrdersPage; // Partial URL
			Response.Redirect(navigateUrl);
		}

		#endregion Event Handlers

		protected override string GetPageName()
		{
			return WebTracker.Pages.EditOrder;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.EditOrderPage;
		}

		protected void WeightVolumeDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			Bind();
		}
	}
}
