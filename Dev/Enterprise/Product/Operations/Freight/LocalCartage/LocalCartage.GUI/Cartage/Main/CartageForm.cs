using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class CartageForm : ZTemplateForm, INotifications, INotificationSubscriberQueryUser
	{
		public CartageForm(CommonCartage cartageWrapper)
			: base(cartageWrapper)
		{
			AddPlugIns();

			WorkflowTabPage.Initialize(Cartage);
			SetupForm();
			Cartage.SetNotificationSubscriber(this);
			RunSheetSecurityGUIProvider.Register(cartageWrapper.Factory);
			Cartage.SetAutoLogOverride();
		}

		void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedTab == this.MainTabPage && Cartage.ShouldReorderAddresses)
			{
				CartageControl.SetupAddressesCaption(Cartage);
			}
		}

		public override string FormCaption
		{
			get
			{
				ZString caption = Res.GetString("c23c691b-1e5a-43e6-a072-6c20842be2a0", "Port Transport");
				if (Cartage != null && !Cartage.IsDeleted && !Cartage.JJ_ConsignmentID.IsEmpty)
				{
					caption += " - " + Cartage.JJ_ConsignmentID;
				}
				return caption;
			}
		}

		void AddPlugIns()
		{
			PlugIns.AddJobInvoicing(Cartage.InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocAddresses);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
		}

		protected override IBusiness GetTopLevelBusinessEntityForPlugIn()
		{
			return Cartage;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			JobTypeChanged();
			TransportModeChanged();
			ContainerModeChanged();
			DirectionChanged();
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			if (Cartage.IsDeleted)
			{
				Globals.Message.Show(Res.GetString("54c0daf5-ebb9-40e8-a5ed-bfeb30e49dae", "This Port Transport Job has been Deleted by another user. You will not be able to save the changes you made to this Job. Please Close and Reopen."));
				return ContinueWithSave.No;
			}
			else
			{
				var cartageController = (ICartageController)ZControllerFactory.Create(ControllerIDs.Cartage);
				if (CartageToBeDeactivated != null && cartageController.IsFormOpen(CartageToBeDeactivated))
				{
					Globals.Message.Show(Res.GetString("CartageForm|ValidateAndSave|JobScreenOpen", "The existing {0} Local Transport Job is open on another screen. Close this screen before attempting to overwrite it.", Cartage.JJ_ConsignmentID));
					return ContinueWithSave.No;
				}
			}

			return base.ValidateAndSave();
		}

		public CommonCartage CartageToBeDeactivated { get; set; }

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			Cartage.CheckTotalsDiffer();

			if (ShouldDeliverTimeSlotConfirmation())
			{
				DeliverTimeSlotConfirmation();
			}

			return base.ShowPreSaveDialogs();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				UnHookEvents();
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected override void HandleSaveException(Exception ex)
		{
			if (!PortTransportFormExceptionHandler.HandleSaveExceptionForTriggers(ex))
			{
				base.HandleSaveException(ex);
			}
		}

		void SetupForm()
		{
			ZFormMenuStrategy.AddInterfaceConnectorMenuItem(this, new ZMenuItem(ResString.GetMultilingualString("15E634FC-6818-4F2E-A58C-6918AE5AFCD0", "Export Port Transport Booking Status To XML"), delegate
			{ ExportCartageStatus(CancellationToken.None); }));
			ZFormMenuStrategy.AddInterfaceConnectorMenuItem(this, new ZMenuItem(ResString.GetMultilingualString("3ABA6109-42B4-486E-9A3B-BC7D3D77FC75", "Export Port Transport Booking Status To XML: Store As File"), delegate
			{ ExportCartageStatusAsFile(CancellationToken.None); }));

			HookEvents();

			FixContainerizedBookedMoves(); //To fix original Cartages not fixed during transform - Only do this on form open - not in business layer!

			looseMovesControl1.Visible = true;
			looseMovesControl1.Dock = DockStyle.Fill;
		}

		void TransportModeChanged()
		{
			if (Cartage.IsAir)
			{
				CartageControl.SetupSailingForAir();
			}
			else
			{
				CartageControl.SetupSailingForSea();
			}
		}

		void ContainerModeChanged()
		{
			var plugin = PlugIns.GetPlugIn(ControllerIDs.PackingPlugIn);
			if (plugin != null)
			{
				plugin.TabPage.TabVisible = Cartage.IsLoose;
			}

			LooseMovesTabPage.TabVisible = Cartage.IsLoose;
			ContainersTabPage.TabVisible = Cartage.IsContainerised;

			CartageControl.ContainerModeChanged(Cartage);
		}

		void DirectionChanged()
		{
			CartageControl.SetupSailingDates(Cartage.IsExportOrOrigin);
		}

		void JobTypeChanged()
		{
			CartageControl.SetupAddressesCaption(Cartage);
		}

		void ExportCartageStatus(CancellationToken token)
		{
			new StandaloneCartageManager(Cartage).ExportCartageStatus(this, token);
		}

		void ExportCartageStatusAsFile(CancellationToken token)
		{
			new StandaloneCartageManager(Cartage).ExportCartageStatusAsFile(this, token);
		}

		void FixContainerizedBookedMoves()
		{
			var cartageType = Cartage.CartageType;
			if (cartageType != null && cartageType.IsContainerised)
			{
				foreach (CommonBookedCtgMove move in Cartage.BookedMovesCollection)
				{
					if (move.IsContainerised && move.EW_E2PickupAddressID.IsEmpty && move.EW_E2WaitPointAddressID.IsEmpty && move.EW_E2DeliveryAddressID.IsEmpty)
					{
						if (cartageType.ContainerizedBooking == null)
						{
							var messageBuilder = new StringBuilder();
							messageBuilder.Append((NoResString)"In CartageForm.FixContainerizedBookedMoves(), cartage.CartageType.ContainerizedBooking was null (which had been causing CommonCartageAddressHelper.GetCartageXXXXXXAddress() to throw a null reference exception before it was fixed).");
							messageBuilder.AppendLine();
							messageBuilder.Append("Port Transport Consignment ID: " + Cartage?.JJ_ConsignmentID ?? (NoResString)"(null record)");
							messageBuilder.AppendLine();
							messageBuilder.Append((NoResString)"List of ContainerizedBookedMoveTypes:");
							messageBuilder.AppendLine();
							foreach (var type in cartageType.ContainerizedBookedMoveTypes)
							{
								messageBuilder.Append("(");
								if (type == null)
								{
									messageBuilder.Append("null");
								}
								else if (type.IsNull)
								{
									messageBuilder.Append("IsNull");
								}
								else if (type.IsDeleted)
								{
									messageBuilder.Append((NoResString)"Row Deleted");
								}
								else if (type.IsRowDeletedOrDetachedOrNull)
								{
									messageBuilder.Append((NoResString)"Row Detached");
								}
								else
								{
									messageBuilder.Append((NoResString)"exists");
								}
								messageBuilder.Append(")");
								messageBuilder.AppendLine();
							}
							messageBuilder.AppendLine();
							ErrorReporter.ReportOnce(ContainerizedBookingNullExceptionKey, messageBuilder.ToString());
						}

						JobDocAddress picAddress = CommonCartageAddressHelper.GetCartagePickupAddress(Cartage, cartageType.ContainerizedBooking);
						JobDocAddress waitAddress = CommonCartageAddressHelper.GetCartageWaitPointAddress(Cartage, cartageType.ContainerizedBooking);
						JobDocAddress dlvAddress = CommonCartageAddressHelper.GetCartageDeliveryAddress(Cartage, cartageType.ContainerizedBooking);
						if (picAddress != null)
						{
							move.EW_E2PickupAddressID = picAddress.PK;
						}

						if (waitAddress != null)
						{
							move.EW_E2WaitPointAddressID = waitAddress.PK;
						}

						if (dlvAddress != null)
						{
							move.EW_E2DeliveryAddressID = dlvAddress.PK;
						}
					}
				}
			}
		}

		void HookEvents()
		{
			Cartage.JJ_E3_NKJobTypeInfo.ValueChanged += new EventHandler(JJ_E3_NKJobTypeInfo_ValueChanged);
			Cartage.JJ_ContainerModeInfo.ValueChanged += JJ_ContainerModeInfo_ValueChanged;
			Cartage.JJ_ShippingTransportModeInfo.ValueChanged += JJ_ShippingTransportModeInfo_ValueChanged;
			Cartage.JJ_DirectionInfo.ValueChanged += JJ_DirectionInfo_ValueChanged;
			Cartage.OnGetCartageLegsToPrint += new EventHandler<DocumentCartageLegEventArgs>(Cartage_OnGetCartageLegsToPrint);
			Cartage.OnGetContainersToPrint += new EventHandler<DocumentContainerEventArgs>(Cartage_OnGetContainersToPrint);
			Cartage.UpdateCartageTotalsPackQuantityVariation += new CancelEventHandler(Cartage_CheckUpdateCartageTotals);
			Cartage.OnJobTypeChanging += new CancelEventHandler(Cartage_OnJobTypeChanging);
			Cartage.OnGetNonContainerisedCartageLegsToPrint += new EventHandler<DocumentCartageLegEventArgs>(Cartage_OnGetNonContainerisedCartageLegsToPrint);

			MainTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;
		}

		void UnHookEvents()
		{
			Cartage.JJ_E3_NKJobTypeInfo.ValueChanged -= new EventHandler(JJ_E3_NKJobTypeInfo_ValueChanged);
			Cartage.OnGetCartageLegsToPrint -= new EventHandler<DocumentCartageLegEventArgs>(Cartage_OnGetCartageLegsToPrint);
			Cartage.OnGetContainersToPrint -= new EventHandler<DocumentContainerEventArgs>(Cartage_OnGetContainersToPrint);
			Cartage.UpdateCartageTotalsPackQuantityVariation -= new CancelEventHandler(Cartage_CheckUpdateCartageTotals);
			Cartage.OnJobTypeChanging -= new CancelEventHandler(Cartage_OnJobTypeChanging);
			Cartage.OnGetNonContainerisedCartageLegsToPrint -= new EventHandler<DocumentCartageLegEventArgs>(Cartage_OnGetNonContainerisedCartageLegsToPrint);
			Cartage.JJ_ContainerModeInfo.ValueChanged -= JJ_ContainerModeInfo_ValueChanged;
			Cartage.JJ_ShippingTransportModeInfo.ValueChanged -= JJ_ShippingTransportModeInfo_ValueChanged;
			Cartage.JJ_DirectionInfo.ValueChanged -= JJ_DirectionInfo_ValueChanged;

			MainTabControl.SelectedIndexChanged -= MainTabControl_SelectedIndexChanged;
		}

		void JJ_E3_NKJobTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			JobTypeChanged();
		}

		void JJ_DirectionInfo_ValueChanged(object sender, EventArgs e)
		{
			DirectionChanged();
		}

		void JJ_ShippingTransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			TransportModeChanged();
		}

		void JJ_ContainerModeInfo_ValueChanged(object sender, EventArgs e)
		{
			ContainerModeChanged();
		}

		void Cartage_OnGetContainersToPrint(object sender, DocumentContainerEventArgs e)
		{
			if (e.DocumentContainerOptions.Containers.Count == 0)
			{
				e.ContinueToPrint = false;
				Globals.Message.ShowInformation
				(
					Res.GetString("78093635-cafd-48e0-ade5-754455e56754", "There are no containers to print. Please use the Cover Sheet."),
					Res.GetString("963ea00b-7af2-4508-bd65-c184970802ed", "No Containers")
				);
			}
			else
			{
				using (var form = new DocumentContainerForm(e.DocumentContainerOptions))
				{
					form.ShowDialog();
					e.ContinueToPrint = form.DialogResult == DialogResult.Yes;
				}
			}
		}

		void Cartage_OnGetCartageLegsToPrint(object sender, DocumentCartageLegEventArgs e)
		{
			if (e.DocumentCartageLegOptions.CartageLegs.Count == 0)
			{
				e.ContinueToPrint = false;
				Globals.Message.ShowInformation(Res.GetString("6088827b-bfca-4a79-b93e-4aa7dbd8880e", "There are no Port Transport Legs to print."), Res.GetString("032cb9eb-3411-4b36-93d3-20d8e274cfe2", "No Port Transport Legs"));
			}
			else
			{
				using (var form = new DocumentCartageLegsForm(e.DocumentCartageLegOptions))
				{
					form.ShowDialog();
					e.ContinueToPrint = form.DialogResult == DialogResult.Yes;
				}
			}
		}

		void Cartage_OnGetNonContainerisedCartageLegsToPrint(object sender, DocumentCartageLegEventArgs eventArg)
		{
			if (eventArg.DocumentCartageLegOptions.CartageLegs.Count == 0)
			{
				eventArg.ContinueToPrint = false;
				Globals.Message.ShowInformation(Res.GetString("47B5A76B-6ABB-4CE7-888E-83A2424DC18D", "There are no Port Transport Legs to print."), Res.GetString("C7BA518F-97FD-4D93-9D89-27E320185F70", "No Port Transport Legs"));
			}
			else
			{
				eventArg.ContinueToPrint = ZFormModaliser.ShowDialogAndDispose(new DocumentLooseJobsForm(eventArg.DocumentCartageLegOptions)) == DialogResult.Yes;
			}
		}

		void Cartage_OnJobTypeChanging(object sender, CancelEventArgs e)
		{
			if (Cartage.HasJobAlreadyCommenced)
			{
				string msg = Res.GetString("e44b367d-e9e6-49d6-9e67-124b7f907051", @"The Job Type cannot be changed if the Port Transport Job has already commenced.

The Job Type will revert back to its previous value.");
				Globals.Message.Show(msg, Res.GetString("904f3667-900b-48dc-9422-5363347bea91", "Job Type cannot be changed"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				e.Cancel = true;
			}
			else if (Cartage.HasParent)
			{
				string msg = Res.GetString("9992a402-4d85-4c53-8e85-385635911250", @"Changing the Job Type requires a re-population of the Port Transport Job from the linked Job '{0}'.

The following data will be cleared and repopulated:

 - Addresses
 - Containers / Port Transport Legs
 - Loose Bookings / Port Transport Legs

Are you sure you want to continue?", Cartage.CartageParent.UniqueConsignmentID);
				e.Cancel = Globals.Message.Show(msg, Res.GetString("bb999b64-73c1-4e48-9cfc-ad5d1121c49b", "Warning! Port Transport Re-population"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel;
			}
			else if (Cartage.Containers.Any() || Cartage.LooseBookedMoves.Any())
			{
				string msg = Res.GetString("A6FBC18D-BEA3-4BDA-9F68-38B920CC6C09", @"Changing the Job Type requires a re-population of the Port Transport Legs.

Are you sure you want to continue?");
				e.Cancel = Globals.Message.Show(msg, Res.GetString("56D80B0B-859E-41AD-9159-485A60EE4EC2", "Warning! Port Transport Re-population"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel;
			}
		}

		void Cartage_CheckUpdateCartageTotals(object sender, CancelEventArgs e)
		{
			if (!e.Cancel)
			{
				string caption = Res.GetString("bff00478-e2c4-46be-a8e9-3afb421e0aeb", "Totals do not match");
				string message = Res.GetString("1039b193-85da-4139-bcb1-8825e5a8e52e", "Total packs, weight and volume do not match the Port Transport total. Would you like to update the Port Transport to match the packline totals?");
				DialogResult result = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				e.Cancel = result != DialogResult.Yes;
			}
		}

		bool ShouldDeliverTimeSlotConfirmation()
		{
			return Cartage.Containers.Any(
				container =>
					container.JC_DepartureSlotReferenceInfo.HasChanges ||
					container.JC_DepartureSlotDateTimeInfo.HasChanges ||
					container.JC_ArrivalSlotReferenceInfo.HasChanges ||
					container.JC_ArrivalSlotDateTimeInfo.HasChanges
			);
		}

		void DeliverTimeSlotConfirmation()
		{
			ZString menuName = Res.GetString("03b20f0c-cacf-4d66-bed1-d2f5349895b4", "Time Slot Confirmation");
			var menuFilter = new ZQuery(StmMenuItemSchema.SU_MenuName, menuName);
			menuFilter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, BusinessContext.Cartage);
			var menuItem = Cartage.Factory.LoadTop1<DocumentCommand>(menuFilter);

			if (menuItem != null)
			{
				menuItem.Parent = Cartage;
				menuItem.Documents.Load();

				if (TransportRegistry.Instance.GenerateSlotBookingOption.Value == TransportRegistry.GenerateSlotBookingCodes.Verify)
				{
					using (var set = new DocumentPrintSet(menuItem, null))
					{
						set.RunWithPartialInstructions(AllowedDeliveryOptions.All, null, Env.Security.Transport);
					}
				}
				else
				{
					DeliveryInstructions instructions = new DeliveryInstructions
					{
						AllowAutoDelivery = true,
						Destination = DeliveryInstructionDestination.TakenFromContact
					};

					var autoDelivery = new DocAutoDelivery();
					var recipients = autoDelivery.GetDeliveryContacts(menuItem, Cartage.DocumentSupporter);
					foreach (DocDeliveryContact recipient in recipients)
					{
						if ((recipient.DeliveryMethod == Core.Constants.ContactNotifyModes.Email && !string.IsNullOrEmpty(recipient.Email)) ||
							(recipient.DeliveryMethod == Core.Constants.ContactNotifyModes.Fax && !string.IsNullOrEmpty(recipient.Fax)))
						{
							instructions.Recipients.Add(recipient);
						}
					}

					if (instructions.Recipients.Count > 0)
					{
						using (var set = new DocumentPrintSet(menuItem, null))
						{
							if (TransportRegistry.Instance.GenerateSlotBookingOption.Value == TransportRegistry.GenerateSlotBookingCodes.Auto)
							{
								set.Run(instructions);
							}
						}
					}
				}
			}
			else
			{
				ErrorReporter.ReportOnce("{81703D4D-0333-41d7-9A49-EBF19DC11AE2}" + menuName, "Unable to find the correct menu (\"" + menuName + "\")");
			}
		}

		public void SelectCartageLeg(CommonCartageLeg leg)
		{
			if (!Visible)
			{
				Shown += (s, e) =>
					{
						SelectCartageLegCore(leg);
					};
			}
			else
			{
				SelectCartageLegCore(leg);
			}
		}

		void SelectCartageLegCore(CommonCartageLeg leg)
		{
			if (leg.IsContainerised)
			{
				if (ContainersTabPage.TabVisible)
				{
					MainTabControl.SelectTab(ContainersTabPage.Name);
					containerDetailsControl1.SelectCartageLeg(leg);
				}
			}
			else if (LooseMovesTabPage.TabVisible)
			{
				MainTabControl.SelectTab(LooseMovesTabPage.Name);
				looseMovesControl1.SelectCartageLeg(leg);
			}
		}

		CommonCartage Cartage
		{
			get { return (CommonCartage)BusinessEntity; }
		}

		const string ContainerizedBookingNullExceptionKey = "CartageForm_FixContainerizedBookedMoves_ContainerizedBookingIsNull";

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification.Message);
		}

		void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
		{
			var msgBoxArgs = e as QueryUserMsgBoxEventArgs;
			if (msgBoxArgs != null)
			{
				msgBoxArgs.Response = Globals.Message.Show(msgBoxArgs.Message, msgBoxArgs.Caption, MessageBoxButtons.YesNo, (msgBoxArgs.Response ? DialogResult.Yes : DialogResult.No)) == DialogResult.Yes;
			}

			var dropModeArgs = e as QueryUserCartageTypeDropModeEventArgs;
			if (dropModeArgs != null)
			{
				dropModeArgs.Response = CartageTypeDropModeMessageBox.Show(Cartage, dropModeArgs);
			}
		}
	}
}
