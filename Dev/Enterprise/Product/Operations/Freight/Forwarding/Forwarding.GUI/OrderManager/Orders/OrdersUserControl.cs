using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class OrdersUserControl : ZUserControl
	{
		public OrdersUserControl()
		{
			InitializeComponent();

			// I think this needs to be done in the next WI - Work Item WI00150310
			//BuyerZAddressWithContactControl.Enter += new EventHandler(BuyerZAddressWithContactControl_Enter);
			//SupplierZAddressWithContactControl.Enter += new EventHandler(SupplierZAddressWithContactControl_Enter);

			SetDataSourceBinding(PlannedContainersGroupBox, "IsVisibleForBinding", Order.Schema.JD_PlannedContainersVisible);
			this.MilestonesTabPage.TabVisible = OrdersDataRegistry.Instance.ShowOrderMilestonesOnMainScreen.Value;

			OrderCustomFieldsDisplayControl.NothingSetupMessageLabelText = Res.GetString("9d3b5ffd-3801-488d-9250-dab7bc02f255", "To make use of this tab, please setup order custom fields in Workflow Manager or on the Organization record.");
		}

		#region Order

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Order Order
		{
			get { return (Order)CurrentDataItem; }
		}

		#endregion

		#region Supplier/Buyer

		// TODO: WI00150310
		//void SupplierZAddressWithContactControl_Enter(object sender, EventArgs e)
		//{
		//	if (Order != null && !SupplierZAddressWithContactControl.ReadOnly && Order.BuyerSupplierLinksHelper.ShouldShowRelatedConsignors)
		//	{
		//		SupplierZAddressWithContactControl.OrganisationFindBox.SelectFromPopupForm();
		//	}
		//}

		// TODO: WI00150310
		//void BuyerZAddressWithContactControl_Enter(object sender, EventArgs e)
		//{
		//	if (Order != null && !BuyerZAddressWithContactControl.ReadOnly && Order.BuyerSupplierLinksHelper.ShouldShowRelatedConsignees)
		//	{
		//		BuyerZAddressWithContactControl.OrganisationFindBox.SelectFromPopupForm();
		//	}
		//}

		#endregion

		#region Properties To Exclude From Additional Detail Tab

		protected virtual string[] GetPropertiesToExcludeFromAdditionalDetailControl()
		{
			return Array.Empty<string>();
		}

		#endregion

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			Order order = dataSource == null ? null : (Order)BindingContext[dataSource, dataMember].GetCurrent();
			if (order != null)
			{
				order.OrderLineDeliveriesEditable = false;
			}
			base.SetDataBinding(dataSource, dataMember);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			OrderCustomFieldsDisplayControl.ForceBindingIncludingParents();
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Order != null)
			{
				Order.JD_EF_ShipmentPrePlanningInfo.ValueChanged -= SetDetachButtonReadOnly;
				Order.JD_JSInfo.ValueChanged -= new EventHandler(ShipmentOrDeclaration_Changed);
				Order.JD_JSInfo.ValueChanged -= new EventHandler(CreatePackLinesFromOrderLines);
				Order.JD_JEInfo.ValueChanged -= new EventHandler(ShipmentOrDeclaration_Changed);
				((ILandedCostHeader)Order).OnLCSupportedChanged -= new EventHandler(OnLCSupportedChanged);
			}
		}

		void OnLCSupportedChanged(object sender, EventArgs e)
		{
			ChangeChargesTabVisibility();
		}

		void ChangeChargesTabVisibility()
		{
			if (Order != null)
			{
				ChargesTabPage.TabVisible = Order.JD_ChargesVisible;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Order != null)
			{
				PlannedContainersGrid.ReadOnly = Order.JD_EF_ShipmentPrePlanning.IsValid;

				this.OrderSplitsButtonGrid.Order = Order;

				SetDetachButtonReadOnly(this, e);
				Order.JD_EF_ShipmentPrePlanningInfo.ValueChanged += SetDetachButtonReadOnly;

				Order.JD_JSInfo.ValueChanged += new EventHandler(ShipmentOrDeclaration_Changed);
				Order.JD_JSInfo.ValueChanged += new EventHandler(CreatePackLinesFromOrderLines);
				Order.JD_JEInfo.ValueChanged += new EventHandler(ShipmentOrDeclaration_Changed);
				((ILandedCostHeader)Order).OnLCSupportedChanged += new EventHandler(OnLCSupportedChanged);

				ShipmentOrDeclaration_Changed(this, EventArgs.Empty);
				ChangeChargesTabVisibility();

				using (ZModule jobDeclarationModuleForThisCountry = ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration))
				{
					bool hasJobDeclarationSupportInThisCountry = (jobDeclarationModuleForThisCountry != null);

					JD_JEBoundFindBox.Visible = hasJobDeclarationSupportInThisCountry;
					JD_JELabel.Visible = hasJobDeclarationSupportInThisCountry;
				}
			}
		}

		protected ZForm OrderForm
		{
			get { return (ZForm)ParentForm; }
		}

		#endregion

		#region Shipment Links

		void ShipmentDetachButton_Click(object sender, EventArgs e)
		{
			if (Order.Shipment != null && !Order.IsAllowedToDetachShipment)
			{
				Globals.Message.ShowError(Res.GetString("67f9008a-3260-4204-ba88-2948a00a5787", "This shipment contains important changes and should be saved before it can be detached."));
			}
			else
			{
				Order.JD_JS = ZGuid.Empty;
				Order.JD_JE = ZGuid.Empty;
			}
		}

		void OpenShipmentButton_Click(object sender, EventArgs e)
		{
			OpenShipment();
		}

		internal void OpenShipment()
		{
			if (Order.JD_JS.IsValid)
			{
				ChildEditableService.SetState(Order.Factory, ChildEditableServiceStates.Shipment);
				if (Order.IsShipmentAttached)
				{
					ZController controller = ZControllerFactory.Create(ControllerIDs.JobShipment);
					controller.ShowEditForm(Order.Shipment);
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("7a7bb489-a073-400e-b542-fdd06cc5136d", "This order is not attached to a shipment."), Res.GetString("e3cae547-d9a6-44f3-b300-4407cd7c8132", "Shipment Error"));
			}
		}

		void ShipmentOrDeclaration_Changed(object sender, EventArgs e)
		{
			RefreshPlanningTabText();
		}

		public void RefreshPlanningTabText()
		{
			if (Order == null || !Order.JD_JS.IsValid && !Order.JD_JE.IsValid)
			{
				this.PlanningTab.Text = Res.GetString("f22f5962-e3be-48ac-ad53-74d457c3081b", "Planning");
			}
			else
			{
				this.PlanningTab.Text = Res.GetString("5b5f20d4-995d-40d9-b6d8-b97fc1a0ca3e", "Planning - Refer to Shipment Details");
			}
		}

		void SetDetachButtonReadOnly(object sender, EventArgs e)
		{
			ShipmentDetachButton.ReadOnly = Order.IsLinkedToPreAdvice;
			JD_JSBoundFindBox.ReadOnly = Order.IsLinkedToPreAdvice;
			JD_VBBoundFindBox.ReadOnly = Order.IsLinkedToPreAdvice;
		}

		#endregion

		#region Organisations Contacts

		void OnSupplierContactsLinkButton_Click(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowContactsForOrg(Order.Supplier);
		}

		void OnBuyerContactsLinkButton_Click(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowContactsForOrg(Order.Buyer);
		}

		void OnSendAgentContactsLinkButton_Click(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowContactsForOrg(Order.SendingAgent);
		}

		void OnReceiveAgentContactsLinkButton_Click(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowContactsForOrg(Order.ReceivingAgent);
		}

		void OnCarrierContactsLinkButton_Click(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowContactsForOrg(Order.Carrier);
		}

		void ShowContactsForOrg(OrgHeader orgToShow)
		{
			if (orgToShow != null)
			{
				IOrganisationController orgController = (IOrganisationController)ZControllerFactory.Create(ControllerIDs.Organisation);
				orgController.ShowForm(orgToShow, OrganisationTabPages.Contacts, FormAction.View);
			}
		}

		#endregion

		#region Inco Terms

		void IncoTermsExplainButton_Click(object sender, EventArgs e)
		{
			if (!Order.JD_IncoTermInfo.HasErrors())
			{
				IncoTermDescriptionForm form = new IncoTermDescriptionForm(Order.JD_IncoTerm);
				ZFormModaliser.Show(form, ParentForm as ZForm);
			}
		}

		#endregion

		void BottomTabControl_Selecting(object sender, TabControlCancelEventArgs e)
		{
			if (e.TabPage == this.ProductQuantitySummaryTabPage)
			{
				Order.ProductQuantitySummary.Populate();
			}
		}

		void CreatePackLinesFromOrderLines(object sender, EventArgs e)
		{
			var shipment = Order.Shipment;

			if (shipment != null && Order.PreAdvice == null)
			{
				shipment.OnOrderLineToPackLineConversion -= new EventHandler<OrderLineToPackLineConversionEventArgs>(AttachOrderToShipmentHelper.OrderLineToPackLineConversion);
				shipment.OnOrderLineToPackLineConversion += new EventHandler<OrderLineToPackLineConversionEventArgs>(AttachOrderToShipmentHelper.OrderLineToPackLineConversion);

				Order.OnPopulateShipment -= AttachedOrder_OnPopulateShipment;
				Order.OnPopulateShipment += AttachedOrder_OnPopulateShipment;

				shipment.CreatePackLinesFromOrderLines(new Order[] { Order });
			}
		}

		void AttachedOrder_OnPopulateShipment(object sender, PopulateShipmentEventArgs e)
		{
			var dialogCaption = ResString.GetMultilingualString("158BE79B-DC9F-46f1-8E50-7421ACB62A48", "Link order lines to pack-lines");

			var dialogContext = new DialogDefaultContext(
				new ZGuid("b23960fd-4530-4d6f-8bd6-86c6098b7396"),
				dialogCaption,
				ZMessageBoxButtons.YesNo,
				ZMessageBoxIcon.Question,
				null,
				showCheckboxOnly: true);

			var dialogResult = Globals.Message.ShowOrDefault(dialogContext, e.Message);
			if (dialogResult == ZDialogResult.No)
			{
				e.ShouldPopulateShipment = false;
			}
		}
	}
}
