using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class AgencyBookingForm : ZTemplateForm, IRequireInactivationPrompt, ISupportSwitchTabPage, ICustomerServiceMenuSectionCodeOverridable
	{
		public AgencyBookingForm(AgencyBooking shipment)
			: base(shipment)
		{
			InitializeComponent();

			if (!this.IsDesignMode())
			{
				PlugIns.Add(ControllerIDs.Routing);
				PlugIns.Add(ControllerIDs.DocAddresses);
				PlugIns.AddJobInvoicing(shipment.InvoicingSupporter);
				PlugIns.Add(ControllerIDs.DocDataPlugIn);
				PlugIns.Add(ControllerIDs.DtbBooking);

				HookEvents();
			}

			WorkflowTabPage.Initialize(shipment);
			InitialiseActionsMenu();

			ShipmentDocumentSupporterGuiQueryProvider.Register(shipment.Factory);
			ServicesSelectionGuiProvider.Register(shipment.Factory);

			if (ComplianceRiskHelper.IsLinerAgencyEnabledComplianceWise)
			{
				PlugIns.Add(ControllerIDs.ComplianceRiskPlugin);
			}
		}

		#region Implementation

		public override string FormCaption
		{
			get
			{
				if (Shipment == null)
				{
					return base.FormCaption;
				}
				else if (Shipment.JS_IsCancelled)
				{
					return Res.GetString("17333349-537b-43d5-afe7-1b6a0913535f", "Booking {0} - Canceled", Shipment.JS_UniqueConsignRef);
				}
				else
				{
					return Res.GetString("8962efef-1140-4301-b387-76f83189bc09", "Booking {0}", Shipment.JS_UniqueConsignRef);
				}
			}
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			if (DisplayMode == ODisplayMode.Edit)
			{
				Array.Resize(ref factories, factories.Length + 1);
				factories[factories.Length - 1] = new PackingTransactionParticipant(Shipment);
			}

			base.Save(factories);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (FormVerb == FormVerbs.Deactivate)
			{
				PromptForInactivation();
			}

			ShowCancelRequestActionFormIfNecessary();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Shipment != null)
				{
					UnHookEvents();
				}
			}
			base.Dispose(disposing);
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			if (Shipment == null)
			{
				return ContinueWithSave.No;
			}

			try
			{
				Shipment.DefaultWeightAndVolumeUnits();
				Shipment.CheckTotalsDiffer();

				var result = ContinueWithSave.No;

				if (Shipment.Sailing == null || Shipment.Sailing.Voyage == null)
				{
					result = base.ValidateAndSave();
				}
				else
				{
					using (var mutex = new AgencyAllocationMutex(Shipment.Sailing.Voyage))
					{
						mutex.Lock();
						if (mutex.HasLock)
						{
							try
							{
								result = base.ValidateAndSave();
							}
							finally
							{
								mutex.Unlock();
							}
						}
						else
						{
							var lockInfo = mutex.GetLockInfo();
							var (caption, message, allowRelease) = mutex.GetFriendlyMessage(lockInfo);
							var shouldForceUnlock = Globals.Message.Show(message, caption, allowRelease ? MessageBoxButtons.YesNo : MessageBoxButtons.OK, MessageBoxIcon.Error);

							if (shouldForceUnlock == DialogResult.Yes)
							{
								mutex.ReleaseLocks(lockInfo);
								result = ValidateAndSave();
							}
						}
					}
				}
				return result;
			}
			catch (PackedIntoMultipleContainersException)
			{
				PackedIntoMultipleContainersHandler.Handle(Shipment);
				return ContinueWithSave.No;
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			if (Shipment.BuyerSupplierLinksHelper.ShouldPromptToSaveSupplierBuyerRelationship && QueryForNewSupplierBuyerRelationship())
			{
				Shipment.BuyerSupplierLinksHelper.AddNewBuyerSupplierLink();
			}

			ContinueWithSave result = base.ShowPreSaveDialogs();

			if (result == ContinueWithSave.Yes && Shipment.ShouldEnforceAllocations)
			{
				AllocationUsage usageBeforeChange = Shipment.LastSavedUsage;
				AllocationUsage usageAfterChange = AllocationUsage.LoadFromShipment(Shipment);

				if (Shipment.JS_JX != Shipment.JS_JX_LastSavedSailing || usageAfterChange.HasAspectExceedingThatFor(usageBeforeChange))
				{
					AllocationUsageSet set = Shipment.LoadAllocationUsageSet();

					if (set == null)
					{
						Globals.Message.ShowError(Res.GetString("20b088f9-69d3-4f80-a61d-010366621a85", "Allocations are not configured for the current sailing."));
						result = ContinueWithSave.No;
					}
					else if (!set.CanFit(usageAfterChange))
					{
						result = AllocationAdjustmentDialog.ConfirmAdjustAllocations(set, usageAfterChange) ? ContinueWithSave.Yes : ContinueWithSave.No;
					}
				}
			}

			ShowCancelRequestActionFormIfNecessary();

			return result;
		}

		public override bool IsResizableByTabPageAllowed => true;

		void InitialiseActionsMenu()
		{
			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("e5563593-304d-4d68-ada1-f2fbd3931133", "Container Release Wizard"), delegate
			{ ContainerReleaseForm.Show(this, Shipment, false); });
			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("c67170a3-f1e5-4b72-918b-d4c7c29bba4b", "Container Release Replacement Wizard"), delegate
			{ ContainerReleaseForm.Show(this, Shipment, true); });

			ActionsMenuItem.Popup += (s, e) => { ActionsMenuItemsHelper.DisableActionMenuItemsExcludingDefaultsInViewMode(this); };
		}

		void QueryAndConfirmShipment()
		{
			if (Shipment.HasChanges || !Shipment.IsInDatabase)
			{
				Globals.Message.ShowError(Res.GetString("881f42a3-3fb9-49be-96c4-53f35cbcc6b3", "You must save this booking first."));
			}
			else if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value && Shipment.JS_ShipmentStatus != ShipmentStatusList.Codes.Booked)
			{
				Globals.Message.Show(
					Res.GetString("9e94d2e4-0bd0-42df-b77e-51b0e35203a1", "The Booking is not confirmed yet.  Please confirm the booking by changing the Status to BKD."),
					Res.GetString("2f5d66cd-7a17-4e15-bde7-a6465ce077f0", "Confirm booking {0}", Shipment.JS_UniqueConsignRef),
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning
				);
			}
			else if (Globals.Message.Show(Res.GetString("ed12fc8f-38b2-44eb-9620-3047d9e07be2", "Are you sure you want to confirm this booking?"), Res.GetString("ed04d781-a181-4278-bd6e-76c1f319b124", "Confirm booking {0}", Shipment.JS_UniqueConsignRef), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
			{
				var agencyBillOfLadingController = ZControllerFactory.Create(ControllerIDs.AgencyBillOfLading);
				var bill = agencyBillOfLadingController.Factory.Load<BillOfLading>(Shipment.PK);
				bill.Confirm();
				bill.Factory.NameForDebugging = "Confirmed Bill Of Lading";

				Shipment.Numbers.IsManagedForDataRefresh = false;
				Shipment.CusEntryNumbers.IsManagedForDataRefresh = false;
				Shipment.BookedContainers.IsManagedForDataRefresh = false;
				Shipment.RealContainers.IsManagedForDataRefresh = false;
				Shipment.Factory.NameForDebugging = "Confirmed Agency Booking";
				((IBusinessObjectFactoryInternals)Shipment.Factory).CanSave = false;
				DisposePendingUserAction();
				Shipment.Factory.CleanUp();

				Close();

				var newForm = (ZForm)agencyBillOfLadingController.ShowEditForm(bill);

#if DEBUG
				PopupForm_ForTesting = newForm;
#endif
			}
		}

#if DEBUG
		public ZForm PopupForm_ForTesting;
#endif

		bool QueryForNewSupplierBuyerRelationship()
		{
			return Globals.Message.Show(
				Res.GetString("0035b36f-36b0-4550-aead-b230f83385da", "Do you wish to save this Supplier-Consignor/Buyer-Consignee relationship?"),
				Res.GetString("41b074ac-581f-4bb9-aeb6-5c916fd45464", "Save"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				DialogResult.Yes
				) == DialogResult.Yes;
		}

		void HookEvents()
		{
			Shipment.PackingModeChanging += Shipment_PackingModeChanging;
			Shipment.UpdateShipmentTotalsPackQuantityVariation += new CancelEventHandler(Shipment_CheckUpdateShipmentTotals);
			if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value)
			{
				Shipment.OnShipmentStatusUpdate += new EventHandler<ShipmentStatusEventArgs>(UpdateShipmentStatus);
			}
		}

		void UnHookEvents()
		{
			Shipment.PackingModeChanging -= Shipment_PackingModeChanging;
			Shipment.UpdateShipmentTotalsPackQuantityVariation -= new CancelEventHandler(Shipment_CheckUpdateShipmentTotals);
			Shipment.OnShipmentStatusUpdate -= new EventHandler<ShipmentStatusEventArgs>(UpdateShipmentStatus);
		}

		void BookingDetails_Confirm(object sender, EventArgs e)
		{
			QueryAndConfirmShipment();
		}

		void Shipment_PackingModeChanging(object sender, CancelEventArgsWithMessage e)
		{
			if (!e.Cancel)
			{
				e.Cancel = Globals.Message.Show(e.Message, e.Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No;
			}
		}

		void Shipment_CheckUpdateShipmentTotals(object sender, CancelEventArgs e)
		{
			if (!e.Cancel && Shipment.JS_PackingMode != Core.Constants.ContainerModes.FCL)
			{
				string caption = Res.GetString("b133886e-82cc-4edc-8fd5-07854ddc63bd", "Totals do not match");
				string message = Res.GetString("a5dbce15-e8b3-4327-bcf0-d43f58bf8603", "Total weight and volume do not match the booking total. Would you like to update the booking to match the totals?");
				DialogResult result = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				e.Cancel = result == DialogResult.No;
			}
		}

		void UpdateShipmentStatus(object sender, ShipmentStatusEventArgs e)
		{
			if (e.ShipmentStatus == ShipmentStatusList.Codes.BookingRejected)
			{
				var userResponseArgs = new UserResponseArgument
				{
					Caption = Res.GetString("af48afa8-2998-4772-8822-fe78a1c51541", "Rejection Reason"),
					Message = Res.GetString("0435a4a3-10a5-44fc-974c-aec6286a95eb", "Please enter the reason of rejection."),
					Buttons = ZMessageBoxButtons.OKCancel,
					DefaultButton = ZMessageBoxDefaultButton.Button1,
					Icon = ZMessageBoxIcon.Information,
					MinimumResponseLength = 1
				};

				e.StatusUpdatedReason = Globals.Message.QueryUserResponse(userResponseArgs);
			}
		}

		AgencyBooking Shipment
		{
			get { return (AgencyBooking)this.BusinessEntity; }
		}

		#endregion

		#region IRequireInactivationPrompt Members

		public void PromptForInactivation()
		{
			if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value)
			{
				if (Shipment.JS_ShipmentStatus == ShipmentStatusList.Codes.ElectronicBooking
					|| (Shipment.JS_ShipmentStatus != ShipmentStatusList.Codes.BookingRejected && Shipment.IsReceivedElectronicBooking()))
				{
					var dialogContext = new DialogDefaultContext(
						new ZGuid("7908746a-260f-49c6-9535-fd6cbcc82db8"),
						ResString.GetMultilingualString("6a703c1e-4b27-4cfd-8fae-e744483f5898", "Booking Rejection Message"),
						ZMessageBoxButtons.YesNo,
						ZMessageBoxIcon.Question,
						null,
						showCheckboxOnly: true
					);

					var dialogResult = Globals.Message.ShowOrDefault(dialogContext,
						ResString.GetMultilingualString("84b06339-1d91-4d00-9825-dcc975a39c14", "This Booking was created electronically, would you like to send Booking Rejection message to the Booking Party?"));

					if (dialogResult == ZDialogResult.Yes)
					{
						Shipment.LogStatusChangedEvent(ShipmentStatusList.Codes.BookingRejected, (NoResString)"Booking Cancelled"); // Event Parameter Constant.
					}
				}
				else if (Shipment.JS_ShipmentStatus != ShipmentStatusList.Codes.BookingRejected)
				{
					Shipment.LogStatusChangedEvent(ShipmentStatusList.Codes.BookingRejected, (NoResString)"Booking Cancelled"); // Event Parameter Constant.
				}

				using (Shipment.SuspendAutomaticCreationOfStatusChangedLog())
				{
					Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
				}

				DeleteUselessShipmentStatusUpdateLogsForInactivation();
			}
		}

		void DeleteUselessShipmentStatusUpdateLogsForInactivation()
		{
			var existingNonBookingRejectedShipmentStatusUpdateLogsNotInDB = Shipment.Logs.LogsNotInDB.OfType<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode && !x.SL_IsCancelled
				&& x.Parameters.TryGetValue(Params.Type, out var type) && type == Core.Constants.EventReferenceMessageTypes.ShipmentStatus
				&& x.Parameters.TryGetValue(Params.New, out var newShipmentStatus) && newShipmentStatus != ShipmentStatusList.Codes.BookingRejected);

			if (existingNonBookingRejectedShipmentStatusUpdateLogsNotInDB.Any())
			{
				existingNonBookingRejectedShipmentStatusUpdateLogsNotInDB.DeleteAll();
			}
		}

		#endregion

		#region CancelRequest

		void ShowCancelRequestActionFormIfNecessary()
		{
			if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value && Shipment.JS_ShipmentStatus == ShipmentStatusList.Codes.EBookingCancellationRequest)
			{
				using (var form = new CancelRequestActionForm())
				{
					var dialogResult = ZFormModaliser.ShowDialogWithoutDispose(form, this);
					switch (dialogResult)
					{
						case DialogResult.Yes:
							{
								Shipment.LogStatusChangedEvent(ShipmentStatusList.Codes.BookingCancelled, (NoResString)"Booking Cancelled by Booking Party");

								using (Shipment.SuspendAutomaticCreationOfStatusChangedLog())
								{
									Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingCancelled;
								}
								break;
							}
						case DialogResult.No:
							{
								var previousShipmentStatus = Shipment.GetShipmentStatusBeforeLastestEBookingCancellationRequest();

								if (!previousShipmentStatus.IsEmpty)
								{
									Shipment.LogStatusChangedEvent(previousShipmentStatus, (NoResString)"Booking Withdrawal/Cancellation Request Rejected" + (form.Reason.IsNullOrEmpty() ? string.Empty : ", " + form.Reason));

									using (Shipment.SuspendAutomaticCreationOfStatusChangedLog())
									{
										Shipment.JS_ShipmentStatus = previousShipmentStatus;
									}
								}
								break;
							}
					}
				}
			}
		}

		#endregion

		#region ISupportSwitchTabPage

		public void SwitchTabPage(string tabPageName)
		{
			var tabPage = MainTabControl.GetTabPage(tabPageName);
			if (tabPage != null)
			{
				MainTabControl.SelectedTab = tabPage;
			}
		}

		#endregion

		#region ICustomerServiceMenuSectionCodeOverridable

		string ICustomerServiceMenuSectionCodeOverridable.SectionCode
			=> MainTabControl.SelectedTab.Name == ComplianceWiseConstants.ComplianceRiskTabPageName
			? ModuleTreeCustomerServiceMenuSectionList.Codes.ComplianceWise
			: ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency;

		#endregion ICustomerServiceMenuSectionCodeOverridable
	}
}



