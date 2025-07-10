using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.ComplianceRisk.GUI;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI.MenuItems;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.GUI;
using Enterprise.Freight.GUI.Common.TemplateRecords;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Freight.QuotedBookings.DataTransfer;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.QuotedBookings.Business.QuotedBookingToShipmentConverter;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class QuotedBookingForm : ZTemplateForm,
		ITabVisibilityDeciderPersistence,
		IRequireInactivationPrompt,
		ISupportSwitchTabPage,
		INotifications,
		ICarrierContractAssignableJobForm,
		ICustomerServiceMenuSectionCodeOverridable
	{
		protected QuotedBookingForm()
		{
			InitializeComponent();
		}

		public QuotedBookingForm(QuotedBooking quotedBooking)
			: base(quotedBooking)
		{
			Argument.NotNull(quotedBooking, "quotedBooking");
			SetupForm();

			if (quotedBooking != null && quotedBooking.IsTemplate)
			{
				WorkflowTabPage.Dispose();
			}
			else
			{
				WorkflowTabPage.Initialize(quotedBooking);
			}
		}

		#region ITabVisibilityDeciderPersistence Members

		bool ITabVisibilityDeciderPersistence.HasTabVisiblePersisted
		{
			get { return false; }
		}

		bool ITabVisibilityDeciderPersistence.RetrieveTabPageVisible(ZTabPage page)
		{
			return true;
		}

		void ITabVisibilityDeciderPersistence.StoreTabVisible(ZTabPage page)
		{
		}

		#endregion

		#region ICarrierContractAssignableJobForm Members

		ICCACommonAssignmentValidationData ICarrierContractAssignableJobForm.ParentJob => QuotedBooking;

		#endregion

		#region Setup Form

		string setDataBindingStackTrace = string.Empty;
		public override void SetDataBinding(object dataSource, string dataMember)
		{
			setDataBindingStackTrace = dataSource == null ? System.Environment.StackTrace : null;

			if (DataSource != null)
			{
				UnHookEvents();
			}
			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null)
			{
				HookEvents();
			}
		}

		void SetupForm()
		{
			if (QuotedBooking?.ObjectState == QuotedBookingState.BookingOnly)
			{
				MainTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 700, isInStandardDpi: true);
			}

			if (!QuotedBooking.IsTemplate)
			{
				PlugIns.Add(ControllerIDs.DocDataPlugIn);

				if (ComplianceRiskHelper.IsGlobalCommercialInvoiceEnabled && QuotedBooking.Booking != null)
				{
					PlugIns.Add(ControllerIDs.GlobalCommercialInvoicePlugin);
				}

				PlugIns.AddJobInvoicing(QuotedBooking.InvoicingSupporter);
				if (GlbStaff.CurrentUser?.GS_IsDeveloper ?? false)
				{
					PlugIns.Add(ControllerIDs.DocumentVisualizer);
				}

				if ((QuotedBooking.ObjectState == QuotedBookingState.QuoteOnly && QuotedBooking.Quote?.CurrentOneOffQuote != null || ShowBookingControls) && ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
				{
					PlugIns.Add(ControllerIDs.CO2ePlugin);
				}
			}

			if (QuotedBooking.Booking != null)
			{
				PlugIns.Add(ControllerIDs.CartagePlugin);
				PlugIns.Add(ControllerIDs.DtbBooking);
				PlugIns.Add(ControllerIDs.DocAddresses);

				QuotedBooking.Booking.OnExportOrImportBrokerUpdate += new EventHandler<BrokerDefaultingEventArgs>(UpdateExportOrImportBrokerHelper.UpdateExportOrImportBroker);
				QuotedBooking.Booking.GetReasonForChangingDeliveryDueDateEventHandler += MessagePopupHelper.PromptReasonForChangingDeliveryDueDateEventHandler;

				if (GlowRegistry.Instance.NeoEnableConversations.Value)
				{
					PlugIns.Add(ControllerIDs.eConversationPlugIn);
				}

				if (QuotedBooking.Booking.IsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder())
				{
					QuotedBooking.Booking.SetBookingPartyDocumentaryAddressReadonly(true);
				}

				if (ComplianceRiskHelper.IsFreightEnabledComplianceWise && !QuotedBooking.IsTemplate)
				{
					PlugIns.Add(ControllerIDs.ComplianceRiskPlugin);
				}
			}

			AddActionMenus();

			SetupLayout();

			CustomFieldsTabPage.SetVisibilityRule(() => NVOCCModeCheckBox.Checked ? new bool?(false) : null);

			NVOCCModeCheckBox.CheckedChanged += (s, e) =>
			{
				ChangeDisplayMode(NVOCCModeCheckBox.Checked, true);
				RegistryDisplayInNVOCC = NVOCCModeCheckBox.Checked;
			};

			ShipmentDocumentSupporterGuiQueryProvider.Register(QuotedBooking.Factory);
			ServicesSelectionGuiProvider.Register(QuotedBooking.Factory);

			if (ContractsPermissions.IsAllocationsVisible())
			{
				MultiAllocationRouteSelectorProvider.Register(this);
				OverrideAllocationRouteDialogProvider.Register(this);
			}

			if (QuotedBooking.IsTemplate)
			{
				var labelTemplateRecord = new TemplateRecordLabelControl(QuotedBooking.TemplateRecord);
				Controls.Add(labelTemplateRecord);

				labelTemplateRecord.SendToBack();
				NVOCCModeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(911, 20, true);
			}

			AddScreeningLogsTabPage();

			AddComplianceRiskMessageBannerIfNeeded();
		}

		void UpdateHBLBookingStatus(object sender, HBLBookingStatusEventArgs e)
		{
			if (e.HBLBookingStatus == ShipmentStatusList.Codes.BookingRejected)
			{
				var userResponseArgs = new UserResponseArgument
				{
					Caption = Res.GetString("733024c8-13ba-4ecb-ad00-f399ed26e516", "Rejection Reason"),
					Message = Res.GetString("3f88485d-217f-43c6-88cc-4677c62b6870", "Please enter the reason of rejection."),
					Buttons = ZMessageBoxButtons.OKCancel,
					DefaultButton = ZMessageBoxDefaultButton.Button1,
					Icon = ZMessageBoxIcon.Information,
					MinimumResponseLength = 1
				};
				e.StatusUpdatedReason = Globals.Message.QueryUserResponse(userResponseArgs);
			}
		}

		void AddActionMenus()
		{
			var booking = QuotedBooking.Booking;
			if (booking != null)
			{
				ZFormMenuStrategy.AddInterfaceConnectorMenuItems(this, ExportToXmlMenuItems);
			}

			if (!QuotedBooking.IsTemplate)
			{
				ZFormMenuStrategy.AddActionsMenuItem(this, new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.Consolidate", "Consolidate"), ConsolidateMenu_Click));
				ZFormMenuStrategy.AddActionsMenuItem(this, new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.AddToExistingConsol", "Add to Existing Consol"), AddToExistingConsolMenu_Click));
			}
			printMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.PrintBooking", "Print Booking"), PrintQuoteButton_Click)) as ZMenuItem;
			convertQuoteToBookingMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.ConvertQuoteToBooking", "Convert to Booking with Quote"), ConvertQuoteToQuotedBookingButton_Click)) as ZMenuItem;
			approveOneOffMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.ApproveOneOFf", "Approve"), ApproveOneOffButton_Click)) as ZMenuItem;

			if (booking != null)
			{
				if (!QuotedBooking.IsTemplate)
				{
					if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsTransportModeActive(QuotedBooking.TransportMode))
					{
						ZFormMenuStrategy.AddActionsMenuItem(this, new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.CalculateDeliveryDueDate", "Calculate Delivery Due Date"), CalculateDeliveryDueDateMenu_Click));
					}

					ZFormMenuStrategy.AddActionsMenuItem(this, new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.ConvertToShipment", "Convert to Shipment"), ConvertToShipmentMenu_Click));

					var manager = new DeniedPartyScreeningPresentationManager();
					manager.CreateMenusForJob(this, booking, () => QuotedBooking.HasChanges);
					new DeniedPartyScreeningActionsProvider(this, booking).AddJobsMenuItem();
				}

				if (!booking.JS_HouseBill.StartsWith(CommonShipment.PreAllocatedHouseBillPrefix))
				{
					ZFormMenuStrategy.AddActionsMenuItem(this, new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.PreAllocation", "Pre-Allocation"), new EventHandler(PreAllocation_Click)));
				}
			}

			ZFormMenuStrategy.AddActionsMenuItem(this, new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.RecalculateRelatedPartiesForThisCompany", "Recalculate Related Parties for logged in Company"), RecalculateRelatedParties_Click));
			ZFormMenuStrategy.AddActionsMenuItem(this, new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.SelectionOfRateCommodity", "Selection of Rate Commodity (with FMC Tariff ID)"), SelectRateCommodity_Click));

			var oneOffQuote = QuotedBooking.Quote?.CurrentOneOffQuote;

			if (oneOffQuote != null)
			{
				ZFormMenuStrategy.AddActionsMenuItem(this, "-", null);
				ZFormMenuStrategy.AddActionsMenuItem(this, new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.CopyAsNewOneOffQuote", "Copy as New One Off Quote"), CopyAsNewOneOffQuote_Click));
				ZFormMenuStrategy.AddActionsMenuItem(this, new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.CopyAsOneOffQuoteAmendment", "Copy as Amendment"), CopyAsOneOffQuoteAmendment_Click));
				ZFormMenuStrategy.AddActionsMenuItem(this, "-", null);
			}

			ActionsMenuItem.Popup += (s, e) => { ActionsMenuItemsHelper.DisableActionMenuItemsExcludingDefaultsInViewMode(this); };
			ActionsMenuItem.Popup += DisableUniversalCopySchedules;
			ActionsMenuItem.Popup += DisableActionMenuItemsExcludingDefaultsWhenOpeningConsolidatedQuote;
		}

		#region Copy

		void CopyAsNewOneOffQuote_Click(object sender, EventArgs e)
		{
			if (QuotedBooking.HasChanges)
			{
				ShowQuoteNotSavedMessageBox();
				return;
			}

			ShowTemplateCopyForm();
		}

		void CopyAsOneOffQuoteAmendment_Click(object sender, EventArgs e)
		{
			if (QuotedBooking.HasChanges)
			{
				ShowQuoteNotSavedMessageBox();
				return;
			}

			if (QuotedBooking.CanCopyAsOneOffQuoteAmendment)
			{
				QuotedBooking.Quote.AmendmentCopy = ZBool.True;
				try
				{
					ShowTemplateCopyForm();
				}
				finally
				{
					QuotedBooking.Quote.AmendmentCopy = ZBool.False;
				}
			}
			else
			{
				var message = Res.GetString("QuotedBookings|CannotBeCopiedAsAmendment", "The quote cannot be copied as an amendment since it has already been used.");
				var caption = Res.GetString("QuotedBookings|ActionCannotBeCompleted", "Action cannot be completed");
				Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, DialogResult.OK);
			}
		}

		void ShowTemplateCopyForm()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.OneOffQuotes);
			controller.ShowTemplateCopyForm(QuotedBooking);
		}

		void ShowQuoteNotSavedMessageBox()
		{
			Globals.Message.Show(
				Res.GetString("QuotedBookings|MustBeSavedFirstBeforeCopy", "The quote must be saved first before it can be copied."),
				Res.GetString("QuotedBookings|ActionCannotBeCompleted", "Action cannot be completed"),
				MessageBoxButtons.OK, MessageBoxIcon.Exclamation, DialogResult.OK);
		}

		#endregion

		void DisableActionMenuItemsExcludingDefaultsWhenOpeningConsolidatedQuote(object sender, EventArgs e)
		{
			if (QuotedBooking?.IsForwardRegistered != ZBool.True)
			{
				return;
			}

			foreach (var menuItem in ActionsMenuItem.MenuItems.OfType<MenuItem>())
			{
				var menuName = menuItem.Name;
				if (!ZFormMenuStrategy.IsDefaultActionMenuItemName(menuName) && !ZFormMenuStrategy.IsAlwaysEnabledActionMenuItemName(menuName))
				{
					menuItem.Enabled = false;
				}
			}
		}

		void SelectRateCommodity_Click(object sender, EventArgs e)
		{
			var commodity = QuotedBooking.Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, QuotedBooking.Commodity);
			if (commodity == null)
			{
				using (var box = new ZMessageBox(ResString.GetMultilingualString("4a218f58-fb26-45b6-92bc-1d17dabc2ace", "Please make sure the Commodity field is not empty and valid."), (NoResString)"Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Information))
				{
					ZFormModaliser.ShowDialogWithoutDispose(box);
				}
			}
			else
			{
				var factory = new BusinessObjectFactory();
				var supporter = (IRatingSupporter)QuotedBooking;
				var selector = ObjectFactory.Get<IRateCommodityFMCSelectorController>("IRateCommodityFMCSelectorController", this);

				selector.SelectAndUpdate(
					factory,
					supporter,
					new DetailedGoodsDescriptionProxy()
					{
						IsAvailable = !QuotedBooking.IsOneOffQuote,
						IsEmpty = QuotedBooking.DetailedGoodsDescriptionNoteText.IsEmpty
					});
			}
		}

		void RecalculateRelatedParties_Click(object sender, EventArgs e)
		{
			QuotedBooking.AttemptToUpdateExistingValueInRecalculation += QuotedBooking_AttemptToUpdateExistingValueInRecalculation;

			var result = QuotedBooking.RecalculateRelatedParties();

			if (!result.WasSuccessful)
			{
				Globals.Message.ShowError(result.Log);
			}
			QuotedBooking.AttemptToUpdateExistingValueInRecalculation -= QuotedBooking_AttemptToUpdateExistingValueInRecalculation;
		}

		void QuotedBooking_AttemptToUpdateExistingValueInRecalculation(object sender, QuotedBooking.RecalculateRelatedPartyCancelEventArgs e)
		{
			var message = Res.GetString("89988cbf-a579-495b-b697-713ea969a2cf",
				"{0} has already been entered. Do you wish to update the {0} based on your Company Related Party Configuration?", e.RecalculatedPropertyName);

			var context = new DialogDefaultContext(
				new ZGuid("db8c47f0-e3b9-447e-8493-eda17a3005af"),
				ResString.GetMultilingualString("a2e8db52-3351-44c3-9c47-32aba6d6b82c", "Confirmation"),
				ZMessageBoxButtons.YesNo,
				ZMessageBoxIcon.Question,
				null,
				showCheckboxOnly: true);
			var result = Globals.Message.ShowOrDefault(context, message);

			e.Cancel = result == ZDialogResult.No;
		}

		void DisableUniversalCopySchedules(object sender, EventArgs e)
		{
			var universalCopyMenu = ActionsMenuItem.MenuItems.FindByName("UniversalCopy");
			if (universalCopyMenu != null)
			{
				var copySchedulesMenuItem = universalCopyMenu.MenuItems.FindByName("CopySchedules");
				if (copySchedulesMenuItem != null && copySchedulesMenuItem.Visible)
				{
					copySchedulesMenuItem.Enabled = false;
					copySchedulesMenuItem.Visible = false;
				}

				var copySchedulesSplitterMenuItem = universalCopyMenu.MenuItems.FindByName("CopySchedulesSplitter");
				if (copySchedulesSplitterMenuItem != null && copySchedulesSplitterMenuItem.Visible)
				{
					copySchedulesSplitterMenuItem.Enabled = false;
					copySchedulesSplitterMenuItem.Visible = false;
				}
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			IsConsolidatedCheckBox.AllowOverlap(MainPanel);
			NVOCCModeCheckBox.AllowOverlap(MainPanel);

			if (!DesignModeFinder.IsDesigning)
			{
				if (QuotedBooking.Quote != null && QuotedBooking.Quote.TH_IsLocked)
				{
					PrintQuoteButton.Text = Res.GetString("QuotedBookings|PrintQuoteButton", "Re-print");
				}

				tabConfigurationManager = new TabConfigurationManager(null, MainTabControl);
				tabConfigurationManager.Enabled = true;
			}

			if (FormVerb == FormVerbs.Deactivate)
			{
				PromptForInactivation();
			}

			ShowCancelRequestActionFormIfNecessary();

			if (QuotedBooking.Booking != null)
			{
				using (QuotedBooking.Booking.SuspendSettingHasChanges())
				{
					if (QuotedBooking.Booking is not IComplianceItemRiskStatusProvider provider || !provider.IsEnabledComplianceWise)
					{
						new DeniedPartyScreeningPresentationManager().ResynchronizeScreeningStatus(false, new[] { QuotedBooking.Booking }, null);
					}
				}
			}
		}
		TabConfigurationManager tabConfigurationManager;

		public QuotedBooking QuotedBooking
		{
			get { return (QuotedBooking)BusinessEntity; }
		}

		#endregion

		#region Events

		#region Hook / Unhook

		void HookEvents()
		{
			if (QuotedBooking.Quote != null)
			{
				QuotedBooking.Quote.ShowMessage += ShowMessage;
				QuotedBooking.Quote.ShowApprovalDialog += new EventHandler<Quote.ApprovalDialogEventArgs>(Quote_ShowApprovalDialog);
				QuotedBooking.Quote.ShowApprovalMessage += new Quote.ApprovalMessageEventHandler(Quote_ShowApprovalMessage);
				QuotedBooking.Quote.QuoteApprovalSecurityNotGranted += new EventHandler<Quote.QuoteApprovalSecurityEventArgs>(CurrentHeader_OneOffQuoteApprovalSecurityNotGranted);
				QuotedBooking.Quote.SpotQuoteChargesIncorrect += new CancelEventHandler(Quote_SpotQuoteChargesIncorrect);
				QuotedBooking.Quote.SetFinalMode += new EventHandler(Quote_SetFinalMode);
				QuotedBooking.Quote.HookupNoteAddedListener();
				QuotedBooking.Quote.OnQuoteSaved += Quote_OnQuoteSaved;
			}

			if (QuotedBooking.Booking != null)
			{
				QuotedBooking.Booking.UpdateShipmentTotalsPackQuantityVariation += new CancelEventHandler(Shipment_CheckUpdateShipmentTotals);
				QuotedBooking.Booking.DeliveryDueDateNotChangedInManualCalculation += MessagePopupHelper.NotifyDeliveryDueDateNotChangedInManualCalculation;
			}

			QuotedBooking.OnHBLBookingStatusUpdate += new EventHandler<HBLBookingStatusEventArgs>(UpdateHBLBookingStatus);
		}

		void Quote_SetFinalMode(object sender, EventArgs e)
		{
			DialogResult finalModeDialogResult = Globals.Message.Show(Res.GetString("1120b1b2-50cd-4f51-9166-91f8a6fccf79", "Do you want to print this Spot Quote in Final mode? This will mark this Spot Quote as locked and final.\r\n\r\nClick Yes for final, and No for draft."),
				Res.GetString("93680037-6d99-4700-b7fc-768b94578cbf", "Print Mode"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			((Quote.SetFinalModeArgs)e).Result = finalModeDialogResult == DialogResult.Yes;
		}

		void UnHookEvents()
		{
			if (QuotedBooking.Quote != null)
			{
				QuotedBooking.Quote.ShowMessage -= ShowMessage;
				QuotedBooking.Quote.ShowApprovalDialog -= new EventHandler<Quote.ApprovalDialogEventArgs>(Quote_ShowApprovalDialog);
				QuotedBooking.Quote.ShowApprovalMessage -= new Quote.ApprovalMessageEventHandler(Quote_ShowApprovalMessage);
				QuotedBooking.Quote.QuoteApprovalSecurityNotGranted -= new EventHandler<Quote.QuoteApprovalSecurityEventArgs>(CurrentHeader_OneOffQuoteApprovalSecurityNotGranted);
				QuotedBooking.Quote.SpotQuoteChargesIncorrect -= new CancelEventHandler(Quote_SpotQuoteChargesIncorrect);
				QuotedBooking.Quote.SetFinalMode -= new EventHandler(Quote_SetFinalMode);
				QuotedBooking.Quote.OnQuoteSaved -= Quote_OnQuoteSaved;
			}

			if (QuotedBooking.Booking != null)
			{
				QuotedBooking.Booking.UpdateShipmentTotalsPackQuantityVariation -= new CancelEventHandler(Shipment_CheckUpdateShipmentTotals);
				QuotedBooking.Booking.DeliveryDueDateNotChangedInManualCalculation -= MessagePopupHelper.NotifyDeliveryDueDateNotChangedInManualCalculation;
			}

			QuotedBooking.OnHBLBookingStatusUpdate -= new EventHandler<HBLBookingStatusEventArgs>(UpdateHBLBookingStatus);
		}

		#endregion

		#region ShowMessage

		void ShowMessage(string message)
		{
			Globals.Message.Show(message);
		}

		#endregion

		#region Consolidating

		void ConsolidateMenu_Click(object sender, EventArgs e)
		{
			ConsolidateToNewConsol();
		}

		void AddToExistingConsolMenu_Click(object sender, EventArgs e)
		{
			if (QuotedBooking.Booking != null)
			{
				var messageHeader = Res.GetString("0724efd9-79d6-44bb-a843-61e4c0c42999", "Add To Existing Consol");

				if (QuotedBooking.HasChanges || !QuotedBooking.Booking.IsInDatabase)
				{
					Globals.Message.Show(Res.GetString("95c43fa8-f901-4665-b74e-46c380cc0825", "Please save the booking before adding to an existing Consol."), messageHeader, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
				else if (QuotedBooking.Booking.JS_IsDirectBooking)
				{
					Globals.Message.Show(Res.GetString("f3413066-912e-4e22-a8c9-19628fed8f48", "You cannot add a direct booking to an existing Consol. Please choose 'Consolidate' from the menu to create a new Consol."), messageHeader, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
				else if (QuotedBooking.ShipmentStatus != ShipmentStatusList.Codes.Booked)
				{
					Globals.Message.Show(NotConfirmedWarningMessage, messageHeader, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
				else if (!QuotedBooking.Booking.JS_CarrierContractNumber.IsEmpty)
				{
					Globals.Message.Show(Res.GetString("adccabe1-2c69-8f93-4047-17b9612d895a", "Only a Booking that has not been allocated to any Carrier Contract can be added to an existing Consol."),
						messageHeader, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
			}
			else if (QuotedBooking.Quote != null)
			{
				if (!CanConvertQuoteToQuotedBooking(QuotedBooking))
				{
					return;
				}
			}

			var helper = new AttachConsolModuleGridHelper(QuotedBooking);
#if DEBUG
			LastSelectHelper_ForTesting = helper;
#endif
			helper.ShowConsolGrid();
			Close();
		}

		#endregion

		void AddScreeningLogsTabPage()
		{
			if (!LogsTabPage.IsDisposed && QuotedBooking.Booking != null && !QuotedBooking.IsTemplate)
			{
				var screenStatusControl = new RelatedDeniedPartyScreeningStatusControl();
				screenStatusControl.SetBindingMember("Booking.RelatedOrgPartyScreeningStatusCollection");

				ComplianceLogTabHelper.AddLogTabIfNeeded(QuotedBooking, LogsTabPage, screenStatusControl);
			}
		}

		void AddComplianceRiskMessageBannerIfNeeded()
		{
			ComplianceRiskPresentationHelper.AddComplianceRiskWarningMessageBannerIfNeeded(this);
		}

		#region Calculate Delivery Due Date

		void CalculateDeliveryDueDateMenu_Click(object sender, EventArgs args)
		{
			if (QuotedBooking.DeliveryDueDateInfo.ReadOnly)
			{
				Env.Security.QuickBookingDeliveryDueDateOverride.ShowError();
			}
			else
			{
				QuotedBooking.Booking.CalculateDeliveryDueDate();
				QuotedBooking.DeliveryDueDateInfo.RefreshBinding();
			}
		}

		#endregion

		#region ConvertToShipment

		void ConvertToShipmentMenu_Click(object sender, EventArgs args)
		{
			if (QuotedBooking.Booking != null && IsDeniedPartyOrComplianceRiskConditionsValidElseShowMessageIfNeeded(ActionsOnBooking.ConvertToShipment))
			{
				var converter = new QuotedBookingToShipmentConverter(QuotedBooking, BookingToShipmentConversionSource.Form);

				if (converter.HasAnyErrors(out string errorMessage))
				{
					Globals.Message.Show(
						errorMessage,
						Res.GetString("12b6c4d5-62aa-4619-b0ff-8aad7984c9ca", "Convert to shipment"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning
					);
				}
				else
				{
					var shipmentController = ZControllerFactory.Create(ControllerIDs.JobShipment);
					var forwardingShipment = shipmentController.Factory.Load<ForwardingShipment>(QuotedBooking.Booking.PK);
					if (forwardingShipment != null)
					{
						forwardingShipment.DebugLog.AppendLine((NoResString)"Before converting to shipment.");
						forwardingShipment.LogDebugInfo();
						converter.ConvertBookingToShipment(forwardingShipment);
						forwardingShipment.DebugLog.AppendLine((NoResString)"After converting to shipment.");
						forwardingShipment.LogDebugInfo();
						var shipmentForm = (ZForm)shipmentController.ShowEditForm(forwardingShipment);
						if (shipmentForm != null)
						{
							shipmentForm.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing)?.SetSecurityCheckpoint(forwardingShipment.InvoicingSupporter.JobInvoicingSecurity, Env.Security.None);
						}
#if DEBUG
						PopupForm_ForTesting = shipmentForm;
#endif
						Close();
					}
				}
			}
		}

		enum ActionsOnBooking
		{
			ConvertToShipment,
			Consolidate
		}

		bool IsDeniedPartyOrComplianceRiskConditionsValidElseShowMessageIfNeeded(ActionsOnBooking action)
		{
			var isValid = true;

			if (QuotedBookingToShipmentConverter.IsDeniedPartyOrComplianceRiskStatusNotClearAndFreightMovementRestricted(QuotedBooking))
			{
				var caption = action == ActionsOnBooking.ConvertToShipment ? Res.GetString("810E47C6-3FB0-46F4-A3F9-14DDC66FC5EF", "Convert Booking To Shipment") : Res.GetString("EF799BAF-73A9-46E4-ADDA-AC93E5CC81B8", "Consolidate");

				var extraMessage = OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.Value
					? Res.GetString("E0829642-5489-44EC-BFF5-A8A0DF22A344", "CLR - Clear or JCL - Job Cleared") : Res.GetString("956D269B-FEBA-4403-812A-D0CBB1B12A1F", "CLR - Clear");

				var actionMessage = action == ActionsOnBooking.ConvertToShipment ? Res.GetString("5D40B1E3-BC4F-43C8-8A4B-3E3A136375DB", "convert a Booking to Shipment") : Res.GetString("AEEB1404-7F2E-4523-9463-663637CDD8A1", "consolidate");

				if (ComplianceRiskHelper.IsFreightEnabledComplianceWise)
				{
					if (Env.Security.BookingsComplianceAllowOverrideFreightMovementRestrictions.IsAllowed)
					{
						isValid = Globals.Message.Show(Res.GetString("66EB3F3C-570E-4B25-BCCF-188F39C11F50",
							@"You are about to {0} while the Job Compliance Status is not Clear or Override Clear.
Do you want to proceed?", actionMessage), caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes;
					}
					else
					{
						Globals.Message.Show(Res.GetString("86A42616-BE96-4342-99CB-DB22F9834263",
						"Error: You cannot {0} while the Job Compliance Status is not Clear or Override Clear.\r\nDetailed Information: {1}.",
						actionMessage, Env.Security.BookingsComplianceAllowOverrideFreightMovementRestrictions.ErrorMessageForNotAllowed));
						isValid = false;
					}
				}
				else
				{
					if (Env.Security.OrgDeniedPartyScreeningOverrideFreightMvmtRestr.IsAllowed)
					{
						isValid = Globals.Message.Show(Res.GetString("0874305C-25EF-4001-940C-C540B7D827A5",
							"You are about to {0} while the screen status is not {1}. Do you want to proceed?", actionMessage, extraMessage),
							caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes;
					}
					else
					{
						Globals.Message.Show(Res.GetString("530265B7-A03E-423E-AC11-053B49326FE9",
						"Error: You cannot {0} while the screen status is not {1}.\r\nDetailed Information: {2}.", actionMessage, extraMessage,
						Env.Security.OrgDeniedPartyScreeningOverrideFreightMvmtRestr.ErrorMessageForNotAllowed));
						isValid = false;
					}
				}
			}

			return isValid;
		}

		#endregion

		#region PreAllocation

		void PreAllocation_Click(object sender, EventArgs e)
		{
			ShowPreAllocation();
		}

		void ShowPreAllocation()
		{
			if (QuotedBooking.Booking == null)
			{
				return;
			}

			if (QuotedBooking.HasChanges || !QuotedBooking.Booking.IsInDatabase)
			{
				Globals.Message.Show(Res.GetString("7f06049e-8d52-4b61-b182-71ad1ac6f89d", "Please save booking before opening the pre-allocation form."),
					Res.GetString("2fa020b5-4d11-4508-9f99-2049e1bee738", "Pre-Allocation"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else
			{
				ZController controller = ZControllerFactory.Create(ControllerIDs.PreAllocations);
				ViewQuotedBooking viewQuotedBooking = controller.Factory.Load<ViewQuotedBooking>(QuotedBooking.PK);

				if (viewQuotedBooking != null)
				{
					controller.SetFormsModalTo(this);
					controller.ShowEditForm(viewQuotedBooking);
				}
			}
		}

		#endregion

		#region CheckUpdateShipmentTotals

		void Shipment_CheckUpdateShipmentTotals(object sender, CancelEventArgs e)
		{
			if (!e.Cancel)
			{
				string caption = Res.GetString("619f2e26-34cd-4bba-bfe1-d6602e62bc7e", "Totals do not match");
				string message = Res.GetString("27ed1965-e521-4f5e-b887-8a57f8ecf36e", "Total packs, weight and volume do not match the booking total. Would you like to update the booking to match the packline totals?");
				DialogResult result = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				e.Cancel = result == DialogResult.No;
			}
		}

		#endregion

		#region No Charges

		void Quote_SpotQuoteChargesIncorrect(object sender, CancelEventArgs e)
		{
			DialogResult result = Globals.Message.Show(Res.GetString("fd160706-4b21-c285-42ef-1379aa4bbae1", "This Spot Quote either has no charges present, or the Incoterm specified dictates that a Freight charge must be present and it is not. Do you want to continue?"), Res.GetString("47ffb6fb-e3a6-4780-9a38-b14fe64ac1bb", "Freight Charges"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			e.Cancel = (result == DialogResult.No);
		}

		#endregion

		#region ExportToXML

		List<MenuItem> ExportToXmlMenuItems
		{
			get
			{
				return new ExportToXmlMenuItemSet<QuotedBooking>(() => Exporter, QuotedBooking);
			}
		}

		IXmlDataTransferExporter Exporter
		{
			get
			{
				var result = new XmlDataTransferExporter(new QuotedBookingValueObjectDataAdapter(), true);
				result.DefaultFileName = QuotedBooking.Booking.JS_UniqueConsignRef + "_" + ZDateTime.Now.ToString("yyyyMMddhhmmss");
				if (!(new ZString(SystemDataRegistry.Instance.BookingExportDirectory.Value).IsEmpty))
				{
					result.InitialDirectory = SystemDataRegistry.Instance.BookingExportDirectory.Value;
				}
				return result;
			}
		}

		#endregion

		void Quote_OnQuoteSaved(object sender, EventArgs e)
		{
			var savedEventArgs = e as Quote.SavedEventArgs;
			if (savedEventArgs?.HasSaveSucceeded ?? false)
			{
				UpdateQuoteLockedReadOnlyState();
			}
		}

		#endregion

		#region Form Caption

		public override string FormCaption
		{
			get { return QuotedBooking != null ? QuotedBooking.HumanReadableName : (ZString)base.FormCaption; }
		}

		#endregion

		public override bool IsResizableByTabPageAllowed => true;

		#region SetupState

		public bool ShowQuoteControls
		{
			get
			{
				return QuotedBooking.ObjectState == QuotedBookingState.QuoteOnly ||
						QuotedBooking.ObjectState == QuotedBookingState.AcceptedBookingWithQuote ||
						QuotedBooking.ObjectState == QuotedBookingState.UnacceptedBookingWithQuote;
			}
		}

		public bool ShowBookingControls
		{
			get
			{
				return QuotedBooking.ObjectState == QuotedBookingState.BookingOnly ||
							QuotedBooking.ObjectState == QuotedBookingState.AcceptedBookingWithQuote ||
							QuotedBooking.ObjectState == QuotedBookingState.UnacceptedBookingWithQuote;
			}
		}

		public bool ShowQuoteOnlyControls
		{
			get { return QuotedBooking.ObjectState == QuotedBookingState.QuoteOnly; }
		}

		void SetupLayout()
		{
			NVOCCModeCheckBox.Text = Res.GetString("QuotedBookingForm|8f67543d-8a6a-4278-bb73-b9e6450cf655", "NVOCC Display");

			QuotedBookingDetailsControl.Dock = QuotedBookingAdditionalDetailsControl.Dock =
				NVOCCQuotedBookingDetailsControl.Dock = NVOCCQuotedBookingAdditionalDetailsControl.Dock = DockStyle.Fill;

			convertQuoteToBookingMenuItem.Visible = ConvertQuoteToQuotedBookingButton.Visible = !ShowBookingControls;
			approveOneOffMenuItem.Visible = ApproveOneOffButton.Visible = !ShowBookingControls;
			printMenuItem.Visible = PrintQuoteButton.Visible = ShowQuoteControls;

			if (!ShowQuoteControls)
			{
				MainTabControl.TabPages.Remove(DocumentSelectionTabPage);
				DocumentSelectionTabPage.Dispose();
			}

			if (ShowQuoteOnlyControls)
			{
				NVOCCModeCheckBox.Visible = false;
				ChangeDisplayMode(false, false);
				NVOCCQuotedBookingDetailsControl.Parent.Controls.Remove(NVOCCQuotedBookingDetailsControl);
				NVOCCQuotedBookingDetailsControl.Dispose();
				NVOCCQuotedBookingAdditionalDetailsControl.Parent.Controls.Remove(NVOCCQuotedBookingAdditionalDetailsControl);
				NVOCCQuotedBookingAdditionalDetailsControl.Dispose();
				UpdateQuoteLockedReadOnlyState();
			}
			else
			{
				ChangeDisplayMode(RegistryDisplayInNVOCC, false);
				NVOCCModeCheckBox.Checked = RegistryDisplayInNVOCC;
				UpdateConsolidatedQuoteReadOnlyState();
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (QuotedBooking == null)
			{
				ErrorReporter.ReportOnce("BusinessEntity on QuotedBookingForm is Null", "DataSource on QuotedBookingForm has been set to null. This is the stack trace: " + setDataBindingStackTrace);
				return;
			}

			var shouldBeEnabled = this.DisplayMode != ODisplayMode.ReadOnly && !ShowBookingControls;

			ConvertQuoteToQuotedBookingButton.Enabled = shouldBeEnabled;
			ApproveOneOffButton.Enabled = shouldBeEnabled;
		}
		#endregion

		#region Button Clicks

		#region ConvertQuoteToQuotedBooking

		void ConvertQuoteToQuotedBookingButton_Click(object sender, EventArgs e)
		{
			if (CanConvertQuoteToQuotedBooking(QuotedBooking))
			{
				BeginInvoke(new MethodInvoker(TryOpenConvertedInNewForm));
			}
		}

		bool CanConvertQuoteToQuotedBooking(QuotedBooking quotedBooking)
		{
			ZString conversionErrorMessage = "";
			ZString loadOrCreateJobErrorMessage;
			bool HasBeenConvertedByAnotherUser() => quotedBooking.Quote != null
													&& quotedBooking.Factory.ExistsInDatabase(JobShipmentSchema.Constants.TableName, new ZQuery(JobShipmentSchema.JS_TH_OneTimeQuote, quotedBooking.Quote.PK));

			if (quotedBooking.ObjectState != QuotedBookingState.QuoteOnly || HasBeenConvertedByAnotherUser())
			{
				conversionErrorMessage = Res.GetString("749104e0-6cb9-4f5f-a186-f5725016b05d", "Quote has already been converted to a Booking with Quote. Please Close and Reopen.");
			}
			else if (quotedBooking.Quote.HasChanges || !quotedBooking.Quote.IsInDatabase)
			{
				conversionErrorMessage = Res.GetString("c48fadbb-9166-4495-8f20-d6713ea8cfd8", "Please Save and Approve Quote before converting into a Quoted Booking");
			}
			else if (quotedBooking.Quote.IsInternalApprovalRequired && !quotedBooking.Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager)
			{
				conversionErrorMessage = Res.GetString("88677681-17ba-4b8c-ad96-043a372f10f9", "Please Approve Quote before converting into a Quoted Booking");
			}
			else if (!string.IsNullOrEmpty(loadOrCreateJobErrorMessage = quotedBooking.TryLoadOrCreateJob()))
			{
				conversionErrorMessage = Res.GetString("a0c0c396-b995-4682-a990-1500ba4f7265", "Error converting Quote Charges.\r\n\r\n{0}", loadOrCreateJobErrorMessage);
			}
			else if (quotedBooking.Quote.TH_IsOneOffQuoteConsumed)
			{
				conversionErrorMessage = Res.GetString("0418FB8B-BE5C-4F3C-B45F-1B1B8480DABE", "One Off Quote has already been used.");
			}

			var hasErrorPreventingConversion = !string.IsNullOrEmpty(conversionErrorMessage);
			if (hasErrorPreventingConversion)
			{
				Globals.Message.ShowError(conversionErrorMessage, Res.GetString("c0d314fb-105e-4108-881b-c5f5a9c38ce5", "Quoted Booking Conversion"));
				return false;
			}

			return PromptForCarrierIfNeeded(quotedBooking);
		}

		void TryOpenConvertedInNewForm()
		{
			ZController controller = ZControllerFactory.Create(ControllerIDs.QuotedBookings);
			ViewQuotedBooking vQuotedBooking = controller.Factory.Load<ViewQuotedBooking>(QuotedBooking.Quote.PK);

			EnterpriseFormLookStrategy.SavePositionAndSize(this);
			UnHookEvents();
			vQuotedBooking.QuotedBooking.ConvertQuoteToQuotedBooking();
			Close();

			if (controller is IQuotedBookingController quotedBookingController)
			{
				quotedBookingController.SkipRecentItems = true;
			}

			IZForm newForm = controller.ShowEditForm(vQuotedBooking);

#if DEBUG
			PopupForm_ForTesting = (ZForm)newForm;
#endif
			Env.Licence.Booking.Login(newForm);
			vQuotedBooking.QuotedBooking.HasChanges = true;
		}

		/// <summary>
		/// Poupulates carrier from user selection of potential carriers.
		/// </summary>
		/// <returns>Returns true to continue, false to cancel current command.</returns>
		bool PromptForCarrierIfNeeded(QuotedBooking quotedBooking)
		{
			var oneOffQuote = quotedBooking.Quote?.CurrentOneOffQuote;
			if (oneOffQuote == null ||
				!oneOffQuote.TT_OH_Carrier.IsEmpty ||
				!oneOffQuote.PossibleCarriers.Any())
			{
				return true;
			}

			var possibleRateOneOffCarriers = oneOffQuote.PossibleCarriers.Cast<RateOneOffCarrier>();
			var carrierOrgs = possibleRateOneOffCarriers.Select(x => x.Carrier);
			var selectedCode = PromptToSelectSingleCarrier(carrierOrgs.Select(x => x.OH_Code.ToString()));
			if (selectedCode == null)
			{
				return false;
			}

			if (selectedCode.Length > 0)
			{
				var carrier = carrierOrgs.First(x => x.OH_Code == selectedCode);
				var correspondingCreditors = oneOffQuote.PossibleCarriers
							.Where(x => x.Carrier.PK == carrier.PK)
							.Select(x => x.Creditor)
							.Distinct()
							.FirstOrDefault();
				oneOffQuote.TT_OH_Carrier = carrier.PK;
				oneOffQuote.TT_OH_Creditor = correspondingCreditors?.PK ?? ZGuid.Empty;

				var possibleRateOneOffCarrier = possibleRateOneOffCarriers.FirstOrDefault(x => x.Carrier.PK == carrier.PK);
				if (possibleRateOneOffCarrier != null)
				{
					if (oneOffQuote.TT_TransitTime.IsEmpty)
					{
						oneOffQuote.TT_TransitTime = possibleRateOneOffCarrier.TTC_TransitTime;
					}

					if (oneOffQuote.TT_Frequency == 0 && oneOffQuote.TT_FrequencyUnit.IsEmpty)
					{
						oneOffQuote.TT_Frequency = possibleRateOneOffCarrier.TTC_Frequency;
						oneOffQuote.TT_FrequencyUnit = possibleRateOneOffCarrier.TTC_FrequencyUnit;
					}
				}

				oneOffQuote.Factory.Save();
			}

			return true;
		}

		internal virtual string PromptToSelectSingleCarrier(IEnumerable<string> carrierCodes)
		{
			string result;
			using (var form = new SingleValueSelectForm(
				carrierCodes.OrderBy(x => x),
				Res.GetString("8073957b-a017-406d-a73e-52551bec3252", "The Spot Quote has Potential Carriers but NO Carrier specified, would you like to choose the Carrier?"),
				Res.GetData("edd7371d-aed0-4f62-8346-53828a9b6c03", "Potential Carriers")))
			{
				form.ShowYesNoButtonsInsteadOfOkButton();
				var dialogResult = ZFormModaliser.ShowDialogWithoutDispose(form);
				if (dialogResult == DialogResult.Yes)
				{
					result = form.SelectedName;
				}
				else if (dialogResult == DialogResult.No)
				{
					result = string.Empty;
				}
				else
				{
					result = null;
				}
			}

			return result;
		}

		#endregion

		#region ApproveOneOff

		void ApproveOneOffButton_Click(object sender, EventArgs e)
		{
			QuotedBooking.Quote.InternalApproveQuote();
		}

		void Quote_ShowApprovalDialog(object sender, Quote.ApprovalDialogEventArgs e)
		{
			string message = "";
			string caption = Res.GetString("73401818-3d14-4d53-b105-d638cae4897e", "Approval");

			switch (e.Dialog)
			{
				case Quote.ApprovalDialog.WishToApprove:
					message = Res.GetString("b87b47e7-1c74-4518-ae96-c38d0813f1c3",
@"Marking a quotation as Approved means that it can be printed in Final mode, and can be used for Auto-Rating purposes.

Do you wish to Approve this quote?");
					break;

				case Quote.ApprovalDialog.WishToApproveWhenFinalizing:
					message = Res.GetString("1de69181-841a-409b-8fef-fb986e843407",
@"This Spot Quotation is not internally approved.
Marking a quotation as Approved means that it can be printed in Final mode, and can be used for Auto-Rating purposes.

Do you wish to Approve this quote for printing in Final mode? Otherwise this quote will be printed in Draft mode.");
					break;

				case Quote.ApprovalDialog.WishToApproveWhenSaving:
					message = Res.GetString("8e873566-2b63-443f-9b5b-adfa5f52cd3c",
@"This Spot Quotation is not internally approved.
Marking a quotation as Approved means that it can be printed in Final mode, and can be used for Auto-Rating purposes.

Do you wish to Approve this quote?");
					break;
			}

			if (!string.IsNullOrEmpty(message) && Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				e.Cancel = true;
			}
		}

		void Quote_ShowApprovalMessage(Quote.ApprovalMessage approvalMessage)
		{
			string message = "";
			switch (approvalMessage)
			{
				case Quote.ApprovalMessage.MissingLocalClientMessage:
					message = Res.GetString("b00dd8e1-92a9-435b-856e-de98cb8c07ea",
						"A local client or overseas agent is required to approve this Quotation.");
					Globals.Message.ShowError(message);
					return;

				case Quote.ApprovalMessage.MissingOverseasAgentMessage:
					message = Res.GetString("b3a86689-5df3-4ea0-af18-933764ac1ff5",
						"An overseas agent is required to approve this Quotation.");
					Globals.Message.ShowError(message);
					return;

				case Quote.ApprovalMessage.AlreadyApprovedMessage:
					message = Res.GetString("60ebaa2d-53ee-4928-ba6c-01d823007b40",
						"This Spot Quotation is already approved.");
					break;

				case Quote.ApprovalMessage.HaveNoRightsMessage:
					message = Res.GetString("ce11d1b8-7fed-4369-be2b-f70457376898",
						"The login details entered do not have security rights to approve Spot Quotations.");
					break;

				case Quote.ApprovalMessage.LoginFailedMessage:
					message = Res.GetString("0f528eb8-bf32-4323-a309-10753a09c71f",
						"The login details entered are incorrect.");
					break;
			}

			if (!string.IsNullOrEmpty(message))
			{
				Globals.Message.Show(message);
			}
		}

		void CurrentHeader_OneOffQuoteApprovalSecurityNotGranted(object sender, Quote.QuoteApprovalSecurityEventArgs e)
		{
			string message = Res.GetString("58ce1137-f60e-4561-a488-704740937fb4",
@"You do not have the appropriate security rights to mark this Spot Quotation as Approved.
If you do not mark this quotation as approved you cannot use it for Auto Rating Purposes and it can only be printed in Draft Mode.

You can either continue, or have a user with higher security rights enter their credentials.

Do you wish to have a user with higher rights enter their credentials?");

			string caption = Res.GetString("0096a2df-afe1-4e83-b21c-682755ee7b95", "Approval");

			DialogResult questionResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Hand);
			if (questionResult != DialogResult.Yes)
			{
				e.Cancel = true;
			}
			else
			{
				if (ZFormModaliser.ShowDialogAndDispose(new Rating.GUI.LoginForm(e.OverrideLogin)) != DialogResult.OK)
				{
					e.Cancel = true;
				}
			}
		}

		#endregion

		#region PrintQuoteButton

		void PrintQuoteButton_Click(object sender, EventArgs e)
		{
			if (QuotedBooking.HasChanges || !QuotedBooking.IsInDatabase)
			{
				string errorMessage = Res.GetString("b77e3d3f-5ac3-4a21-8669-f424672b6a60", "Please save your quotation before printing.");
				Globals.Message.ShowError(errorMessage, Res.GetString("7066ce27-690e-4c3d-8f89-872d930aabe4", "Print"));

				return;
			}

			var dialogResult = ShowDiscrepancyFormIfNecessary(true);
			if (dialogResult == DialogResult.Cancel)
			{
				return;
			}

			if (QuotedBooking.HasChanges)
			{
				var continueWithSave = FireSaveButton();
				if (continueWithSave == ContinueWithSave.No)
				{
					return;
				}
			}

			var documentSupporter = (Quote.QuoteDocumentSupporter)((IDocumentSupportable)QuotedBooking.Quote).DocumentSupporter;
			var documentCommand = DocumentCommand.GetDocumentCommand(QuotedBooking.Factory, QuotedBooking.Quote, Core.Constants.MenuNameConstantsForPrinting.QuotationPack);

			if (documentCommand == null)
			{
				return;
			}

			using (documentSupporter.InitialiseFetchStrategy())
			using (var guiManager = ObjectFactory.Get<IDocumentDeliveryRestrictionGUIManager>())
			{
				documentCommand.Parent = QuotedBooking;
				guiManager.Initialise(documentCommand.Parent as ICreditControlledBusinessObject);
				documentCommand.CreditControlledDocumentDeliveryGUIManager = guiManager;
				if (DocumentRunner.CheckDataState(documentCommand))
				{
					var task = documentSupporter.BuildPrintTask(documentCommand);
					if (task != null)
					{
						documentSupporter.RunTask(task);
						UpdateQuoteLockedReadOnlyState();
					}
				}
			}
		}

		protected void UpdateConsolidatedQuoteReadOnlyState()
		{
			var quote = QuotedBooking?.Quote;
			if (quote != null && QuotedBooking.IsForwardRegistered)
			{
				QuotedBooking.SetReadOnlyIncludingChildren(true);
				NotesTabPage.SetReadOnlyIncludingChildren(true);
				QuotedBooking.OneOffQuoteStatistics.ReadOnly = false;
				PrintQuoteButton.Enabled = false;
				NVOCCModeCheckBox.Enabled = false;
			}
		}

		protected void UpdateQuoteLockedReadOnlyState()
		{
			var quote = QuotedBooking?.Quote;

			if (quote != null
				&& QuotedBooking.ObjectState == QuotedBookingState.QuoteOnly
				&& (quote.TH_IsLocked || quote.IsClientAccepted))
			{
				QuotedBooking.DisableProcessTasks();
				QuotedBooking.SetReadOnlyIncludingChildren(true);
				NotesTabPage.SetReadOnlyIncludingChildren(true);
				QuotedBooking.OneOffQuoteStatistics.ReadOnly = false;
			}
		}

		#endregion

		#endregion

		#region SkipRecentItems

		public bool SkipRecentItems { get; set; }

		protected override void SaveToRecentItems()
		{
			if (!SkipRecentItems)
			{
				base.SaveToRecentItems();
			}
		}

		#endregion

		#region Saving

		protected override ContinueWithSave ValidateAndSave()
		{
			if (QuotedBooking == null)
			{
				return ContinueWithSave.No;
			}

			QuotedBooking.CheckJobDefaults();

			var booking = QuotedBooking.Booking;
			if (booking != null)
			{
				booking.CheckTotalsDiffer();

				bool cannotUpdateShipmentEntryNumber = booking.JS_UniqueConsignRef.IsEmpty
					&& booking.RequiresShipmentEntryNumberSelection
					&& Env.Registry.AllowManualShipmentEntry
					&& !ShipmentNumberEntryForm.ShowForm(booking);

				if (cannotUpdateShipmentEntryNumber)
				{
					return ContinueWithSave.No;
				}
			}

			QuotedBooking.CheckAndCopyBookingValuesToQuoteIfRequired();

			// TryOpenConvertedInNewForm enables SkipRecentItems to avoid wrong ControlerID in recent-item when user didn't save the form
			// Later on when user save the form, disable SkipRecentItems to create recent-items
			SkipRecentItems = false;

			return base.ValidateAndSave();
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			if (QuotedBooking.ObjectState == QuotedBookingState.UnacceptedBookingWithQuote)
			{
				ShowDiscrepancyFormIfNecessary(false);
			}

			if (QuotedBooking.BuyerSupplierLinksHelper.ShouldPromptToSaveSupplierBuyerRelationship)
			{
				if (ShowConfirmationForNewSupplierBuyerRelationship() == DialogResult.Yes)
				{
					QuotedBooking.BuyerSupplierLinksHelper.AddNewBuyerSupplierLink();
				}
			}

			var result = base.ShowPreSaveDialogs();

			if (QuotedBooking.Booking != null)
			{
				if (result == ContinueWithSave.Yes && QuotedBooking.ControllingAgentDocumentaryAddress != null)
				{
					result = ControllingPartySecurityHelper.RequestSaveWithEmptyControllingPartyAuthorization(QuotedBooking.Booking, DocAddressType.ControllingAgent, QuotedBooking.GetControllingAgentSecurityCheckPoint()).ContinueWithSave;
				}

				if (result == ContinueWithSave.Yes && QuotedBooking.ControllingCustomerDocumentaryAddress != null)
				{
					result = ControllingPartySecurityHelper.RequestSaveWithEmptyControllingPartyAuthorization(QuotedBooking.Booking, DocAddressType.ControllingCustomer, QuotedBooking.GetControllingCustomerSecurityCheckPoint()).ContinueWithSave;
				}
			}

			if (QuotedBooking.ShipmentStatus == ShipmentStatusList.Codes.EBookingCancellationRequest)
			{
				ShowCancelRequestActionFormIfNecessary();
			}

			return result;
		}

		protected override void HandleSaveException(Exception ex)
		{
			if (ex is ZSaveConcurrencyException && ForceFormCloseOnSaveConcurrencyException)
			{
				forceClose = true;
				NotifySaveFailed();
				Close();
			}
			else
			{
				base.HandleSaveException(ex);
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (!forceClose)
			{
				base.OnClosing(e);
			}
		}

		void NotifySaveFailed()
		{
			var caption = Res.GetString("d45d5d0c-0efe-46f5-8a42-1e74ce54abcb", "Unable to complete action");
			var message = Res.GetString("80b2ee4f-4f85-4771-af3f-b27781459587", "While you have been working with this form, another user has made changes which cannot be merged. This form would be closed.");

			Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		bool forceClose;
		public bool ForceFormCloseOnSaveConcurrencyException { get; set; }

		DialogResult ShowConfirmationForNewSupplierBuyerRelationship()
		{
			return Globals.Message.Show(Res.GetString("a1692709-0abe-401c-829f-c3a17978afbe", "Do you wish to save this Supplier-Consignor/Buyer-Consignee relationship?"), Res.GetString("bdff87b6-c3d6-4243-bfae-4277edf304d4", "Save"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
		}

		#endregion

		#region Menu items for using alternatively with booking function buttons

		ZMenuItem printMenuItem;
		public ZMenuItem PrintMenuItem => printMenuItem;

		ZMenuItem convertQuoteToBookingMenuItem;
		public ZMenuItem ConvertQuoteToBookingMenuItem => convertQuoteToBookingMenuItem;

		ZMenuItem approveOneOffMenuItem;
		public ZMenuItem ApproveOneOffMenuItem => approveOneOffMenuItem;

		#endregion

		#region Validation

		protected override void PerformValidation()
		{
			QuotedBooking.Booking?.MarkAsNeedingValidationWhenControllingCustomerOrAgentRequireDefaulting();

			base.PerformValidation();
		}

		#endregion

		#region Discrepancy

		DialogResult ShowDiscrepancyFormIfNecessary(bool withSaveOptions)
		{
			if (QuotedBooking.ObjectState != QuotedBookingState.UnacceptedBookingWithQuote)
			{
				return DialogResult.None;
			}

			bool convertedBookingHasAnyDiscrepancy = Enum.GetValues(typeof(QuotedBooking.DiscrepancyCheck))
					.Cast<QuotedBooking.DiscrepancyCheck>()
					.Any(check => QuotedBooking.HasDiscrepancyFor(check));

			if (!convertedBookingHasAnyDiscrepancy)
			{
				return DialogResult.None;
			}

			var dialogResult = ZFormModaliser.ShowDialogAndDispose(new DiscrepancyForm(QuotedBooking, withSaveOptions), this);
			switch (dialogResult)
			{
				case DialogResult.Yes:
					{
						QuotedBooking.CopyBookingValuesToQuote();

						InvoicingPluginToFreight invoicingPlugin = PlugIns.GetPlugIn(ControllerIDs.JobInvoicing) as InvoicingPluginToFreight;
						invoicingPlugin.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

						QuotedBooking.AcceptDiscrepancy();
						break;
					}

				case DialogResult.OK:
					QuotedBooking.CopyQuoteValuesToBooking();
					QuotedBooking.AcceptDiscrepancy();
					break;
			}

			return dialogResult;
		}

		#endregion

		#region CancelRequest

		void ShowCancelRequestActionFormIfNecessary()
		{
			if (QuotedBooking.ShipmentStatus == ShipmentStatusList.Codes.EBookingCancellationRequest)
			{
				using (var form = new CancelRequestActionForm())
				{
					var dialogResult = ZFormModaliser.ShowDialogWithoutDispose(form, this);
					switch (dialogResult)
					{
						case DialogResult.Yes:
							{
								QuotedBooking.ShipmentStatus = ShipmentStatusList.Codes.BookingCancelled;
								break;
							}
						case DialogResult.No:
							{
								var latestSTULog = QuotedBooking.Booking.Logs?.GetAllLogs().OfType<StmALog>()
									.OrderByDescending(log => log.SL_EventTime)
									.FirstOrDefault(log => !log.SL_IsCancelled &&
													log.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode &&
													log.Parameters[Params.New] == ShipmentStatusList.Codes.EBookingCancellationRequest);

								if (latestSTULog != null)
								{
									latestSTULog.Parameters.TryGetValue(Params.Old, out var previousShipmentStatus);
									QuotedBooking.LogStatusChangedEvent(previousShipmentStatus, form.Reason);
									QuotedBooking.ShipmentStatus = previousShipmentStatus;
								}
								break;
							}
					}
				}
			}
		}

		#endregion

		#region Consol Creation/Adding to Existing Consol

		#region Create New Consol

		void ConsolidateToNewConsol()
		{
			if (QuotedBooking.Booking != null && (QuotedBooking.HasChanges || !QuotedBooking.Booking.IsInDatabase))
			{
				Globals.Message.Show(Res.GetString("849e9c9a-ecba-4eca-ba47-35334a6f8c5d", "Please save the booking before consolidating."), Res.GetString("9ee8190f-8f75-454a-918b-c9c9bfaf2574", "Consolidate"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else
			{
				if (QuotedBooking.Booking != null && QuotedBooking.ShipmentStatus != ShipmentStatusList.Codes.Booked)
				{
					Globals.Message.Show(NotConfirmedWarningMessage, Res.GetString("9ee8190f-8f75-454a-918b-c9c9bfaf2574", "Consolidate"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				if (QuotedBooking.Job != null && QuotedBooking.Job.HasEmptyExchangeRates())
				{
					var message = Res.GetString("528AB6BA-2139-44E6-AD4D-B65DD6C8148B", "Exchange Rates could not be found in Maintain > Reference Files > Exchange Rates. Would you like to use the Exchange Rates on the One Off Quote/Booking with Quote? Click Yes to complete the consolidation operation using the Exchange Rates on the OOQ/BWQ. Click No to cancel the operation.");
					var confirmationResult = Globals.Message.Show(message, Res.GetString("9ee8190f-8f75-454a-918b-c9c9bfaf2574", "Consolidate"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
					if (confirmationResult == DialogResult.No)
					{
						return;
					}
				}

				if (QuotedBooking.Booking != null && !IsDeniedPartyOrComplianceRiskConditionsValidElseShowMessageIfNeeded(ActionsOnBooking.Consolidate))
				{
					return;
				}

				var consolController = ZControllerFactory.Create(ControllerIDs.JobConsol);

				ZGuid bookingPK;
				if (QuotedBooking.Booking == null)
				{
					if (CanConvertQuoteToQuotedBooking(QuotedBooking))
					{
						var vQuotedBooking = consolController.Factory.Load<ViewQuotedBooking>(QuotedBooking.Quote.PK);
						vQuotedBooking.QuotedBooking.ConvertQuoteToQuotedBooking();

						// Accept the quote
						if (vQuotedBooking.QuotedBooking.Quote != null)
						{
							vQuotedBooking.QuotedBooking.Quote.TH_IsLocked = true;
							vQuotedBooking.QuotedBooking.Quote.TH_Accepted = ZDateTime.Today;
							vQuotedBooking.QuotedBooking.Quote.Logs.AddNew(AutoEvents.QuotationAccepted);
						}

						bookingPK = vQuotedBooking.QuotedBooking.Booking.PK;
					}
					else
					{
						return;
					}
				}
				else
				{
					bookingPK = QuotedBooking.Booking.PK;
				}

				consolController.ShowNewForm();

				var consolForm = (ZForm)consolController.LastShownForm;
#if DEBUG
				PopupForm_ForTesting = consolForm;
#endif
				var forwardingConsol = consolForm != null ? consolForm.BusinessEntity as ForwardingConsol : null;

				if (forwardingConsol != null)
				{
					var buildConsolHelper = new BuildConsolHelper();
					buildConsolHelper.ShipmentCannotBeAttachedToConsol += (sender, args) => Globals.Message.ShowError(args.Message);
					buildConsolHelper.MakeConsolFromBookingOrStandaloneShipment(forwardingConsol, bookingPK, QuotedBooking.PK);

					if (forwardingConsol.Shipments.Count == 1)
					{
						var forwardingShipment = forwardingConsol.Shipments[0];

						if (forwardingShipment.IsInDatabase)
						{
							var converter = new QuotedBookingToShipmentConverter(QuotedBooking, BookingToShipmentConversionSource.Form);
							converter.CreateDeclarationIfNecessary(forwardingShipment);

							if (forwardingShipment.Job == null)
							{
								CleanupJob();

								var loader = new JobHeader.Loader(forwardingShipment);
								var jobHeader = loader.TryCreateWithMutex();

								if (jobHeader == null)
								{
									forwardingShipment.AddRowError(loader.GetJobCreationError().Message);
								}
							}
						}

						if (!forwardingShipment.JS_IsDirectBooking)
						{
							if (forwardingShipment.JS_HouseBill.IsEmpty && forwardingShipment.Origin?.Country != null && forwardingShipment.Origin.Country.Code.EqualsIgnoringCase(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
							{
								forwardingShipment.RegenerateHouseBillInSaving(true);
							}
						}
					}

					Close();
				}
			}
		}

		void CleanupJob()
		{
			if (QuotedBooking.Job != null &&
				!QuotedBooking.Job.IsInDatabase &&
				!QuotedBooking.Job.HasChanges)
			{
				QuotedBooking.Job.DisposeAndPreventSave();
			}
		}

		#endregion

		#region Test Only
#if DEBUG

		public ZForm PopupForm_ForTesting;
		internal AttachConsolModuleGridHelper LastSelectHelper_ForTesting;

#endif
		#endregion

		#endregion

		#region NVOCC Mode

		const string DisplayInNVOCC_Name = "DisplayBookingInNVOCC";

		internal ZBool RegistryDisplayInNVOCC
		{
			get { return (Env.Registry.GetFilterCriteria(DisplayInNVOCC_Name) == ZBool.True.ToString()); }
			set
			{
				if (RegistryDisplayInNVOCC != value)
				{
					Env.Registry.SetFilterCriteria(DisplayInNVOCC_Name, value.ToString());
				}
			}
		}

		void ChangeDisplayMode(bool displayInNVOCCMode, bool refresh)
		{
			QuotedBookingDetailsControl.Visible = QuotedBookingAdditionalDetailsControl.Visible = !displayInNVOCCMode;
			NVOCCQuotedBookingDetailsControl.Visible = NVOCCQuotedBookingAdditionalDetailsControl.Visible = displayInNVOCCMode;

			if (displayInNVOCCMode)
			{
				CustomFieldsTabPage.TabVisible = false;
			}
			else if (tabConfigurationManager != null)
			{
				tabConfigurationManager.RefreshTabVisibility();
			}

			if (refresh)
			{
				QuotedBooking.RefreshBindingIncludingChildren();
			}
		}

		#endregion

		#region IRequireInactivationPrompt Members

		public void PromptForInactivation()
		{
			using (QuotedBooking.SuspendAutomaticCreationOfStatusChangedLog())
			{
				if (QuotedBooking.ShipmentStatus == ShipmentStatusList.Codes.ElectronicBooking
					|| (QuotedBooking.ShipmentStatus != ShipmentStatusList.Codes.BookingRejected
					&& ParentContainsEBookingLog()))
				{
					var dialogContext = new DialogDefaultContext(
						new ZGuid("9ea5e46d-9415-b7b9-4b57-361453f4045e"),
						ResString.GetMultilingualString("194486da-0bf8-a688-4ed5-92cf2ffcae3d", "Booking Rejection Message"),
						ZMessageBoxButtons.YesNo,
						ZMessageBoxIcon.Question,
						null,
						showCheckboxOnly: true
					);

					var dialogResult = Globals.Message.ShowOrDefault(dialogContext,
						ResString.GetMultilingualString("94c8e2a7-1f0e-dba2-4956-34f2addd7a1c", "This Booking was created electronically, would you like to send Booking Rejection message to the Booking Party?"));

					if (dialogResult == ZDialogResult.Yes)
					{
						QuotedBooking.LogStatusChangedEvent(ShipmentStatusList.Codes.BookingRejected, (NoResString)"Booking Cancelled"); // Event Parameter Constant.
					}
				}

				QuotedBooking.ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
			}
		}

		bool ParentContainsEBookingLog()
		{
			return QuotedBooking.Logs.GetAllLogs().Cast<StmALog>().Any(log => log.Parameters.TryGetValue(Params.New, out var newParam) && newParam == ShipmentStatusList.Codes.ElectronicBooking)
				|| (QuotedBooking.Booking?.Logs.GetAllLogs().Cast<StmALog>().Any(log => log.Parameters.TryGetValue(Params.New, out var newParam) && newParam == ShipmentStatusList.Codes.ElectronicBooking) ?? false);
		}

		#endregion
		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (QuotedBooking != null)
				{
					if (QuotedBooking.Booking != null && QuotedBooking.Booking.Job != null)
					{
						QuotedBooking.Booking.Job.DisposeAndDeleteNew();
					}

					if (QuotedBooking.Job != null)
					{
						QuotedBooking.Job.DisposeAndDeleteNew();
					}
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		void ISupportSwitchTabPage.SwitchTabPage(string tabPageName)
		{
			var tabPage = MainTabControl.GetTabPage(tabPageName);
			if (tabPage != null)
			{
				MainTabControl.SelectedTab = tabPage;
			}
		}

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}

		#endregion

		#region ICustomerServiceMenuSectionCodeOverridable

		string ICustomerServiceMenuSectionCodeOverridable.SectionCode
			=> MainTabControl.SelectedTab.Name == ComplianceWiseConstants.ComplianceRiskTabPageName
			? ModuleTreeCustomerServiceMenuSectionList.Codes.ComplianceWise
			: ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding;

		#endregion ICustomerServiceMenuSectionCodeOverridable

		static ZString NotConfirmedWarningMessage
		{
			get => Res.GetString("dcc6e2ff-935a-4278-93d1-bacb8403eed4", "The Booking is not confirmed yet.  Please confirm the booking by changing the HBL Booking Status to BKD.");
		}
	}
}
