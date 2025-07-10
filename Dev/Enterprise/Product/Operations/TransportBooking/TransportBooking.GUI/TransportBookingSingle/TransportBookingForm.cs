using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Integration.Routing;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.Freight;
using Enterprise.Integration.LandTransport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.GUI;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.TransportBookings.Business.BookingToTransportJobCommonCreator;

namespace Enterprise.TransportBookings.GUI
{
	public partial class TransportBookingForm : ZTemplateForm, INotifications, INotificationSubscriberQueryUser
	{
		public TransportBookingForm(DtbBooking transportBooking)
			: base(transportBooking)
		{
			InitializeComponent();
			ControllerID = ControllerIDs.DtbBooking;

			AddPlugins();

			WorkflowTabPage.Initialize(transportBooking);

			transportBooking.UpdateReadOnly_SingleBookingForm();

			AddActionsMenus();

			HookEvents();
		}

		void AddPlugins()
		{
			if (ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
			{
				PlugIns.Add(ControllerIDs.CO2ePlugin);
			}
			PlugIns.AddJobInvoicing(((IJobInvoicingPlugIn)Booking).InvoicingSupporter);
			PlugIns.Add(ControllerIDs.Apportionment, Env.Security.DtbBookingJobInvoicing, () => AccountingTabControl);
			PlugIns.Add(ControllerIDs.JobInvoicingConsol, Env.Security.DtbBookingJobInvoicing, () => AccountingTabControl);
			PlugIns.Add(ControllerIDs.JobProfitLossConsol, Env.Security.DtbBookingJobInvoicing, () => AccountingTabControl);
			PlugIns.Add(ControllerIDs.DocAddresses);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.DocumentVisualizer);
			if (Booking.KM_IsMaster)
			{
				PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.MasterBookingPackingPlugIn, 1, () => Booking.ConsolidationSingleJob);
			}
			else
			{
				PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.PackingPlugIn, 1, () => Booking.ConsolidationSingleJob);
			}

			var consolidationSingleJob = Booking.ConsolidationSingleJob;
			if (consolidationSingleJob != null && !consolidationSingleJob.IsParentSupportsDirectSailing)
			{
				RoutingScheduleTabPage.TabVisible = false;
				BusinessObject routingDataSource;
				if (consolidationSingleJob.IsParentSupportsRouting)
				{
					routingDataSource = consolidationSingleJob.ParentBO;
				}
				else
				{
					routingDataSource = Booking.ConsolidationSingleJob;
				}

				PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Routing, 2, () => routingDataSource);
				var plugIn = PlugIns.GetPlugIn(ControllerIDs.Routing);
				if (plugIn != null && consolidationSingleJob.IsParentSupportsRouting)
				{
					((IRoutingPlugin)plugIn).DisableAllControls();
				}
			}
		}

		void HookEvents()
		{
			if (Booking != null)
			{
				Booking.Cancelled += Booking_Cancelled;
				Booking.WorkflowItems.Tasks.TasksViewFilterAdded += UpdateReadOnly;

				if (Booking.ConsolidationSingleJob != null)
				{
					Booking.ConsolidationSingleJob.NotificationManager.Push(this);
				}
			}
		}

		void UpdateReadOnly(object sender, EventArgs e)
		{
			Booking.UpdateReadOnly_SingleBookingForm();
		}

		void Booking_Cancelled(object sender, EventArgs e)
		{
			var booking = (DtbBooking)sender;
			if (booking.ParentJob != null)
			{
				PlugIns.GetPlugIn(ControllerIDs.JobInvoicing).Enabled = false;
			}
		}

		void UnhookEvents()
		{
			var activationMenuItem = FindDeactivateActionsMenuItem();
			if (activationMenuItem != null)
			{
				activationMenuItem.Click -= OnActivateDeactivateMenuClick;
			}

			foreach (MenuItem item in ActionsMenuItem.MenuItems)
			{
				item.Click -= ActionMenuItem_Click;
			}

			if (ActionsMenuItem != null && !IsDisposed)
			{
				ActionsMenuItem.Popup -= ActionMenu_Popup;
			}

			if (Booking != null)
			{
				Booking.Cancelled -= Booking_Cancelled;

				if (Booking.ConsolidationSingleJob != null)
				{
					Booking.ConsolidationSingleJob.NotificationManager.Pop();
				}

				if (Booking.WorkflowItems != null && Booking.WorkflowItems.Tasks != null)
				{
					Booking.WorkflowItems.Tasks.TasksViewFilterAdded -= UpdateReadOnly;
				}
			}
		}

		readonly ResourceString MenuItemMakeAvailableText = ResString.GetMultilingualString("TransportBookingForm|MakeAvailable", "Make Available");
		readonly ResourceString MenuItemMakeCreateTransportJobText = ResString.GetMultilingualString("TransportBookingForm|CreateTransportJobMenu", "Create Transport Job");
		readonly ResourceString MenuItemValidateMessageToCTOText = ResString.GetMultilingualString("TransportBookingForm|ValidateMessageToCTO", "Validate Message To CTO");

		ZMenuItem MenuItemMakeAvailable;
		ZMenuItem MenuItemMakeCreateTransportJob;

		void AddActionsMenus()
		{
			ActionsMenuItem.MenuItems.Add(new ZMenuItem(OverrideTransportCompanyAddressMenuCaption, (s, e) => OverrideAddress(DocAddressType.TransportCompanyDocumentaryAddress)));
			ActionsMenuItem.MenuItems.Add(new ZMenuItem(OverrideBookingPartyAddressMenuCaption, (s, e) => OverrideAddress(DocAddressType.BookingPartyDocumentaryAddress)));

			// make available
			MenuItemMakeAvailable = new ZMenuItem(MenuItemMakeAvailableText, MakeAvailableMenu_Click);
			MenuItemMakeAvailable.Name = "MakeAvailable";
			ActionsMenuItem.MenuItems.Add(MenuItemMakeAvailable);

			// hold
			var holdBookingMenuItem = new ZMenuItem("", HoldMenuItem_Click);
			UpdateHoldStatusMenuItemCaption(holdBookingMenuItem);
			ActionsMenuItem.MenuItems.Add(holdBookingMenuItem);

			// validate booking for sending to CTO
			var menuItemValidateMessageToCTO = new ZMenuItem(MenuItemValidateMessageToCTOText, ValidateMessageToCTOMenu_Click);
			menuItemValidateMessageToCTO.Name = "ValidateMessageToCTO";
			ActionsMenuItem.MenuItems.Add(menuItemValidateMessageToCTO);

			// create transport
			AddCreateTransportJobMenuItems();
			ActivateDeactivateActionMenus();

			// make active / inactive (added by base form)
			var activationMenuItem = FindDeactivateActionsMenuItem();
			if (activationMenuItem != null)
			{
				activationMenuItem.Click += OnActivateDeactivateMenuClick;
			}

			// all action menus may affect other menus
			foreach (MenuItem item in ActionsMenuItem.MenuItems)
			{
				item.Click += ActionMenuItem_Click;
			}

			ActionsMenuItem.Popup += ActionMenu_Popup;
		}

		public MenuItem FindDeactivateActionsMenuItem()
		{
			MenuItem result = null;

			if (Booking != null)
			{
				var menuText = (Booking.IsCancelled)
					? Res.GetString("969c4bb6-2ee0-45b8-afee-0c0f365ddf19", "Make Active")
					: Res.GetString("31d212cf-f8ca-4086-8201-703cc9c70722", "Make Inactive");

				result = ActionsMenuItem.MenuItems.Cast<MenuItem>().FirstOrDefault(item => item.Text == menuText);
			}

			return result;
		}

		ResourceString OverrideTransportCompanyAddressMenuCaption { get { return ResString.GetMultilingualString("TransportBookingForm|OverrideTransportCompanyAddressMenuCaption", "Override Transport Company Address"); } }
		ResourceString OverrideBookingPartyAddressMenuCaption { get { return ResString.GetMultilingualString("TransportBookingForm|OverrideBookingPartyAddressMenuCaption", "Override Booking Party Address"); } }

		void OverrideAddress(DocAddressType type)
		{
			addressPopup = new AddressPopup();
			addressPopup.ShowPopupAddress(this, type);
		}

		AddressPopup addressPopup;

		void AddCreateTransportJobMenuItems()
		{
			MenuItemMakeCreateTransportJob = new ZMenuItem(MenuItemMakeCreateTransportJobText);
			MenuItemMakeCreateTransportJob.Popup += delegate
			{ RefreshCreateTransportJobMenuItems(MenuItemMakeCreateTransportJob); };

			RefreshCreateTransportJobMenuItems(MenuItemMakeCreateTransportJob);
			ActionsMenuItem.MenuItems.Add(MenuItemMakeCreateTransportJob);
		}

		void RefreshCreateTransportJobMenuItems(ZMenuItem menu)
		{
			menu.MenuItems.Clear();

			if (CanCreateTransportJob())
			{
				menu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("TransportBookingForm|CreatePortTransport", "Create Port Transport Job"), CreatePortTransport_Click));
				if (TransportRegistry.Instance.EnableLandTransport.Value)
				{
					menu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("TransportBookingForm|CreateLandTransport", "Create Land Transport Consignment"), CreateLandTransport_Click));
					menu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("TransportBookingForm|CreateDefaultTransport", "Create Default Transport Job"), CreateDefaultTransport_Click));
				}
			}
			else
			{
				menu.Enabled = false;
			}
		}

		void CreatePortTransport_Click(object sender, EventArgs args)
		{
			CreateTransportIfAllowed(
				AutoCreatorTargetModules.Codes.PortTransport,
				Env.Security.FindOrCreateAccessModuleCheckPoint(Env.Security.TransportJob),
				Env.Security.TransportJob,
				Env.Security.TransportJobNew);
		}

		void CreateLandTransport_Click(object sender, EventArgs args)
		{
			CreateTransportIfAllowed(
				AutoCreatorTargetModules.Codes.LandTransportConsignment,
				Env.Security.FindOrCreateAccessModuleCheckPoint(Env.Security.DtbConsignment),
				Env.Security.DtbConsignment);
		}

		void CreateDefaultTransport_Click(object sender, EventArgs args)
		{
			var targetModule = TargetModuleDataObjectWriter.GetBookingDataTarget(Booking, null);

			switch (targetModule)
			{
				case UniversalDataBuss.Integration.DataContextType.LandTransportConsignmentConsol:
					CreateTransportIfAllowed(
						AutoCreatorTargetModules.Codes.LandTransportConsignment,
						Env.Security.FindOrCreateAccessModuleCheckPoint(Env.Security.DtbConsignment),
						Env.Security.DtbConsignment);
					break;

				case UniversalDataBuss.Integration.DataContextType.LocalTransport:
					CreateTransportIfAllowed(
						AutoCreatorTargetModules.Codes.PortTransport,
						Env.Security.FindOrCreateAccessModuleCheckPoint(Env.Security.TransportJob),
						Env.Security.TransportJob,
						Env.Security.TransportJobNew);
					break;
			}
		}

		void CreateTransportIfAllowed(string moduleCode, params SecurityCheckpoint[] checkpoints)
		{
			var failingCheckpoints = checkpoints.Where(c => !c.IsAllowed);
			if (!failingCheckpoints.Any())
			{
				CreateTransportCore(moduleCode);
			}
			else
			{
				Globals.Message.ShowError(Env.Security.GetErrorMessageForNotAllowed(failingCheckpoints.ToArray()));
			}
		}

		void CreateTransportCore(string targetModule)
		{
			var buffer = new NotificationBuffer();
			var processingManager = new BookingToTransportJobCommonCreator(buffer, targetModule, isManualProcess: true);
			processingManager.TryCreateTransportJobsFromTransportBookings(new[] { Booking });

			ZStringBuilder sb = new ZStringBuilder();
			foreach (var error in buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error))
			{
				sb.AppendLine(error.Message);
			}

			foreach (var warning in buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Warning))
			{
				sb.AppendLine(warning.Message);
			}

			Globals.Message.Show(sb.ToString());

			Booking.UpdateReadOnly_SingleBookingForm();
		}

		ICommonCartage GetExistingLocalTransportInNewFactory()
		{
			var query = new ZQuery(JobCartageSchema.JJ_ParentID, Booking.PK);
			query.AddToFilter(JobCartageSchema.JJ_ParentTableCode, Booking.TablePrefix);

			return new BusinessObjectFactory().LoadTop1<ICommonCartage>(query);
		}

		object GetExistingActiveLandTransportConsignmentInNewFactory()
		{
			var query = new ZQuery(DtbConsignmentSchema.LTC_KM_Booking, Booking.PK);
			query.AddToFilter(DtbConsignmentSchema.LTC_IsActive, true);
			return new BusinessObjectFactory().LoadTop1<IDtbConsignment>(query);
		}

		public override string FormCaption
		{
			get { return Booking.HumanReadableName; }
		}

		public override bool IsResizableByTabPageAllowed => true;

		DtbBooking Booking
		{
			get { return (DtbBooking)BusinessEntity; }
		}

		void OnActivateDeactivateMenuClick(object sender, EventArgs args)
		{
			if (Booking.IsCancelled)
			{
				InstructionViewsControl.SetUnAssignPackageMessage();
			}
		}

		void MakeAvailableMenu_Click(object sender, EventArgs args)
		{
			if (CanMakeItAvailable())
			{
				Booking.KM_Status = TransportStatuses.Codes.Available;
			}
		}

		void MultiBookingButton_Click(object sender, EventArgs e)
		{
			var consolidation = Booking.ConsolidationSingleJob;
			if (consolidation != null)
			{
				if (!ShowBookingMessageHelper.ShowErrorIfBookingFormOpen(consolidation, ShowBookingMessageHelper.MessageOperation.Open, Booking))
				{
					Close();        // close itself

					var controller = ZControllerFactory.Create(ControllerIDs.DtbBookingConsolidation);
					SetLastControllerForTest(controller);
					controller.ShowEditForm(consolidation);
				}
			}
		}

		void HoldMenuItem_Click(object sender, EventArgs e)
		{
			ToggleHeldStatus();
			UpdateHoldStatusMenuItemCaption((ZMenuItem)sender);
		}

		void ToggleHeldStatus()
		{
			Booking.ToggleHeldStatus(this);
		}

		void UpdateHoldStatusMenuItemCaption(ZMenuItem holdBookingMenuItem)
		{
			holdBookingMenuItem.Caption = Booking.IsHeld ? TransportBookingsControl.UnholdBookingMenuItemText : TransportBookingsControl.HoldBookingMenuItemText;
		}

		void ValidateMessageToCTOMenu_Click(object sender, EventArgs e)
		{
			try
			{
				Booking.IsValidatingSendingXUSToCTO = true;
				Booking.Validation.ValidateAll();
			}
			finally
			{
				Booking.IsValidatingSendingXUSToCTO = false;
			}

			var stringBuilder = new ZStringBuilder();

			if (Booking.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors() || Booking.NotificationBufferForSendingXUSToCTO.Events.HasErrors())
			{
				stringBuilder.AppendLine(ResString.GetMultilingualString("TransportBookingForm|ValidateMessageToCTOFailed", "Transport Booking Validation failed for the following reasons:"));

				var messageErrors = Booking.NotificationBufferForSendingXUSToCTO.Events.GetMessageErrors().Select(e => e.Message);
				var errors = Booking.NotificationBufferForSendingXUSToCTO.Events.GetErrors().Select(e => e.Message);
				foreach (var error in messageErrors.Concat(errors))
				{
					stringBuilder.AppendLine(error);
				}

				Globals.Message.ShowError(stringBuilder.ToString(), Res.GetString("c74d53d4-2dab-45ef-8574-37aa081457c1", "Transport Booking Failed Validation For Sending To CTO"));
			}
			else
			{
				stringBuilder.AppendLine(ResString.GetMultilingualString("TransportBookingForm|ValidateMessageToCTOSuccessful", "Transport Booking Successfully Validated"));
				Globals.Message.ShowInformation(stringBuilder.ToString(), Res.GetString("07a247c9-0d71-4ca3-a6a8-7e196920fdfe", "Transport Booking Validation For Sending To CTO Successful"));
			}
		}

		void ActionMenu_Popup(object sender, EventArgs e)
		{
			ActivateDeactivateActionMenus();
		}

		void ActionMenuItem_Click(object sender, EventArgs e)
		{
			ActivateDeactivateActionMenus();
		}

		void ActivateDeactivateActionMenus()
		{
			ActivateDeactivateActionMenus(MenuItemMakeAvailable, CanMakeItAvailable());
			ActivateDeactivateActionMenus(MenuItemMakeCreateTransportJob, CanCreateTransportJob());

			if (Booking.BookingIsBeingManagedByAuthorisedCarrierBookingAgent || Booking.IsSub)
			{
				var actionMenuItemsThatShouldAlwaysBeEnabledOnBooking = new string[] {
					(NoResString)"CopyHyperlinkToClipboard",
					(NoResString)"CopyIdToClipboard",
					(NoResString)"CreateDesktopShortcut",
					(NoResString)"CopyFormToClipboardMenuItem",
					(NoResString)"ResetFormSizeToDefault",
					(NoResString)"ViewRelatedCommunications",
					(NoResString)"Actions.AddToFavoriteMenuItem",
					(NoResString)"Actions.DeleteFromFavoriteMenuItem"
				};

				foreach (MenuItem menuItem in ActionsMenuItem.MenuItems)
				{
					if (!actionMenuItemsThatShouldAlwaysBeEnabledOnBooking.Contains(menuItem.Name))
					{
						ActivateDeactivateActionMenus(menuItem, false);
					}
				}
			}
		}

		void ActivateDeactivateActionMenus(MenuItem menuItem, bool enabled)
		{
			if (menuItem != null)
			{
				menuItem.Enabled = enabled;
			}
		}

		bool CanMakeItAvailable()
		{
			return Booking.KM_Status == TransportStatuses.Codes.Incomplete || Booking.KM_Status == TransportStatuses.Codes.Quote || Booking.KM_Status == TransportStatuses.Codes.ActionRequired;
		}

		bool CanCreateTransportJob()
		{
			var existingPortTransportJob = GetExistingLocalTransportInNewFactory();
			var existingLandTransportConsignment = GetExistingActiveLandTransportConsignmentInNewFactory();

			if (existingPortTransportJob != null || existingLandTransportConsignment != null)
			{
				return false;
			}
			return true;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetAccountingTabVisibility();
			SetMultiBookingButtonEnabled();
		}

		public void SetInstructionView(TransportBookingInstructionView instructionView)
		{
			MainTabPage.RunWhenBindingOrFirstShown(delegate
			{
				InstructionViewsControl.View = instructionView;
			});
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEvents();

				if (addressPopup != null)
				{
					addressPopup.Dispose();
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.currentBookingPanel = new BookingControl();
			this.InstructionViewsControl = new DtbInstructionViewsControl();
			this.MainTabPage.SuspendLayout();
			this.currentBookingPanel.SuspendLayout();
			this.InstructionViewsControl.SuspendLayout();
			this.MainTabPage.Controls.Add(this.InstructionViewsControl);
			this.MainTabPage.Controls.Add(this.currentBookingPanel);
			// 
			// currentBookingPanel
			// 
			this.currentBookingPanel.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.currentBookingPanel, ".");
			this.currentBookingPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.currentBookingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.currentBookingPanel.Name = "currentBookingPanel";
			this.currentBookingPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.currentBookingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1463, 137, true);
			this.currentBookingPanel.TabIndex = 0;
			// 
			// InstructionViewsControl
			// 
			this.InstructionViewsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InstructionViewsControl, ".");
			this.InstructionViewsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InstructionViewsControl.IsAdditionalReferencesVisible = false;
			this.InstructionViewsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 137, true);
			this.InstructionViewsControl.Name = "InstructionViewsControl";
			this.InstructionViewsControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 2, true);
			this.InstructionViewsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1463, 466, true);
			this.InstructionViewsControl.TabIndex = 1;
			this.MainTabPage.PerformLayout();
			this.currentBookingPanel.ResumeLayout(true);
			this.currentBookingPanel.PerformLayout();
			this.InstructionViewsControl.ResumeLayout(true);
			this.InstructionViewsControl.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		public void Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}

		public void QueryUser(IQueryUserEventArgs e)
		{
			var args = (QueryUserMsgBoxEventArgs)e;
			args.Response = Globals.Message.Show(args.Message, args.Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		}

		void AdditionalReferencesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.AdditionalReferenceControl = new TransportBookingsAdditionalReferencesUserControl();
			this.AdditionalReferencesTabPage.SuspendLayout();
			this.AdditionalReferenceControl.SuspendLayout();
			this.AdditionalReferencesTabPage.Controls.Add(this.AdditionalReferenceControl);
			// 
			// AdditionalReferenceControl
			// 
			this.AdditionalReferenceControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalReferenceControl, "AdditionalReferencesForBinding");
			this.AdditionalReferenceControl.DisplayDetailPanel = false;
			this.AdditionalReferenceControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalReferenceControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AdditionalReferenceControl.Name = "AdditionalReferenceControl";
			this.AdditionalReferenceControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1457, 597, true);
			this.AdditionalReferenceControl.TabIndex = 0;
			this.AdditionalReferencesTabPage.PerformLayout();
			this.AdditionalReferenceControl.ResumeLayout(true);
			this.AdditionalReferenceControl.PerformLayout();
			this.AdditionalReferencesTabPage.ResumeLayout(true);
		}

		partial void SetLastControllerForTest(ZController controller);

		void WorkflowTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.WorkflowTabPage.SuspendLayout();
			this.WorkflowTabPage.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(true);
		}

		void relatedJobsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.relatedJobsTabPage.SuspendLayout();
			this.relatedJobsTabPage.PerformLayout();
			this.relatedJobsTabPage.ResumeLayout(true);
		}

		void RoutingScheduleTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.SailingDetailsControl = new ScheduleControl();
			this.SailingDetailsControl.SuspendLayout();
			this.RoutingScheduleTabPage.SuspendLayout();
			this.BindingSource.SetBindingMember(this.SailingDetailsControl, ".");
			// 
			// SailingDetailsControl
			// 
			this.SailingDetailsControl.AllowDrop = true;
			this.SailingDetailsControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.SailingDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SailingDetailsControl.Name = "SailingDetailsControl";
			this.SailingDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 143, true);
			this.SailingDetailsControl.TabIndex = 11;
			this.SailingDetailsControl.TabStop = false;
			this.RoutingScheduleTabPage.Controls.Add(this.SailingDetailsControl);
			this.SailingDetailsControl.ResumeLayout(true);
			this.SailingDetailsControl.PerformLayout();
			this.RoutingScheduleTabPage.PerformLayout();
			this.RoutingScheduleTabPage.ResumeLayout(true);
		}

		void AccountingTabPage_InitializeTab(object sender, EventArgs e)
		{
			this.AccountingTabPage.SuspendLayout();
			this.AccountingTabPage.PerformLayout();
			this.AccountingTabPage.ResumeLayout(true);
		}

		void SetAccountingTabVisibility()
		{
			var consolidation = Booking.ConsolidationSingleJob;
			var accountingTabPageShouldBeVisible = consolidation?.Parent?.TablePrefix == JobConsolSchema.Constants.Prefix || Booking.KM_IsMaster;
			var billingPagePlugIn = PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
			var apportionmentPlugIn = PlugIns.GetPlugIn(ControllerIDs.Apportionment);
			var consolInvoicingPlugIn = PlugIns.GetPlugIn(ControllerIDs.JobInvoicingConsol);
			var consolProfitLossPlugIn = PlugIns.GetPlugIn(ControllerIDs.JobProfitLossConsol);
			AccountingTabPage.TabVisible = apportionmentPlugIn.Enabled = consolInvoicingPlugIn.Enabled = consolProfitLossPlugIn.Enabled = accountingTabPageShouldBeVisible;
			if (consolidation?.Parent?.ParentWithWorkflow is IGateway gateway && (gateway.GatewayBillingSupporter?.IsGatewayBillingEnabled() ?? false))
			{
				billingPagePlugIn.Enabled = true;
			}
			else
			{
				billingPagePlugIn.Enabled = !accountingTabPageShouldBeVisible;
			}
		}

		void SetMultiBookingButtonEnabled()
		{
			this.MultiBookingButton.Enabled = !Booking.KM_IsMaster;
		}
	}
}

#if DEBUG

namespace Enterprise.TransportBookings.GUI
{
	public partial class TransportBookingForm
	{
		partial void SetLastControllerForTest(ZController controller)
		{
			LastControllerForTest = controller;
		}

		public bool SetErrorMessageForTest;
		public ZController LastControllerForTest { get; set; }
	}
}

#endif
