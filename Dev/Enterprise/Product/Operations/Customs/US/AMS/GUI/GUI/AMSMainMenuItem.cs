using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.GUI
{
	public class AMSMainMenuItem : ZMenuItem
	{
		public AMSMainMenuItem(CusInBondHeader header)
			: base(ResString.GetMultilingualString("USAMSPlugIn|MainMenuItem", "A&MS"))
		{
			this.Header = header;

			AddSendManifestMenuItem();
			AddAmendmentManifestMenuItem();
			AddOtherVesselMenuItems();

			pTTMenuItem = new ZMenuItem(ResString.GetMultilingualString("USAMSPlugIn|PTTMenuItem", "&Permit To Transfer"));
			AddPTTMenuItems();
			this.MenuItems.Add(pTTMenuItem);

			AddInBondMenuItems();
		}
		public CusInBondHeader Header
		{
			get;
			internal set;
		}

		readonly ZMenuItem pTTMenuItem;

		void AddInBondMenuItems()
		{
			var inBondMenuItem = new ZMenuItem(ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem", "&In-Bond"));
			inBondMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|SubsequentOriginal", "Send Subsequent &Original"), SendSubsequentOriginalMessageClick));
			inBondMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|SubsequentAmendment", "Send Subsequent &Amendment"), SendSubsequentAmendMessageClick));
			inBondMenuItem.MenuItems.Add("-");
			inBondMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|Diversion", "Send D&iversion"), SendInBondDiversionMessageClick));
			inBondMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|Arrival", "Send Arri&val"), SendInBondArrivalMessageClick));
			inBondMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|Exportation", "Send E&xportation"), SendInBondExportationMessageClick));
			inBondMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|TransferOfLiability", "Send &Transfer Of Liability"), SendInBondTransferOfLiabilityMessageClick));
			this.MenuItems.Add(inBondMenuItem);
		}

		void AddSendManifestMenuItem()
		{
			var item = new ZMenuItem(ResString.GetMultilingualString("USAMSPlugIn|SendManifest", "Send &Manifest"));
			item.Click += delegate
			{ ValidateAndDeclareAMS(ActionCode.Creating); };
			this.MenuItems.Add(item);
		}

		void AddAmendmentManifestMenuItem()
		{
			var item = new ZMenuItem(ResString.GetMultilingualString("USAMSPlugIn|AmendmentManifest", "&Amendment Manifest"));
			item.Click += delegate
			{ ValidateAndDeclareAMS(ActionCode.AmendingAdd); };
			this.MenuItems.Add(item);
		}

		void AddPTTMenuItems()
		{
			var item = new ZMenuItem(Constants.Caption.PTTMenuItemText);
			item.Click += delegate
			{ ValidateAndDeclareAMS(ActionCode.PermitToTransfer); };
			pTTMenuItem.MenuItems.Add(item);

			item = new ZMenuItem(Constants.Caption.CancelPTTByBillOfLading);
			item.Click += delegate
			{ ValidateAndDeclareAMS(ActionCode.CancelPermitToTransfer); };
			pTTMenuItem.MenuItems.Add(item);
		}

		void AddOtherVesselMenuItems()
		{
			var vesselMenuItem = new ZMenuItem(ResString.GetMultilingualString("USAMSPlugIn|VesselMenuItem", "&Vessel"));
			this.MenuItems.Add(vesselMenuItem);
			var item = new ZMenuItem(Constants.Caption.VesselArrivalMenuItemText);
			item.Click += delegate
			{ ValidateAndDeclareAMS(ActionCode.VesselArrival); };
			vesselMenuItem.MenuItems.Add(item);

			item = new ZMenuItem(Constants.Caption.VesselDepartureMenuItemText);
			item.Click += delegate
			{ ValidateAndDeclareAMS(ActionCode.VesselDeparture); };
			vesselMenuItem.MenuItems.Add(item);

			item = new ZMenuItem(Constants.Caption.ChangeEstDateOfArrival);
			item.Click += delegate
			{ ValidateAndDeclareAMS(ActionCode.ChangeEstDateOfArrival); };
			vesselMenuItem.MenuItems.Add(item);
		}

		void SendInBondDiversionMessageClick(object sender, EventArgs e)
		{
			ValidateAndDeclareAMS(ActionCode.InBondDiversion);
		}

		void SendInBondArrivalMessageClick(object sender, EventArgs e)
		{
			ValidateAndDeclareAMS(ActionCode.InBondArrival);
		}

		void SendInBondExportationMessageClick(object sender, EventArgs e)
		{
			ValidateAndDeclareAMS(ActionCode.InBondExportation);
		}

		void SendInBondTransferOfLiabilityMessageClick(object sender, EventArgs e)
		{
			ValidateAndDeclareAMS(ActionCode.InBondTransferOfLiability);
		}

		void SendSubsequentOriginalMessageClick(object sender, EventArgs e)
		{
			ValidateAndDeclareAMS(ActionCode.SubsequentInBondOriginal);
		}

		void SendSubsequentAmendMessageClick(object sender, EventArgs e)
		{
			ValidateAndDeclareAMS(ActionCode.SubsequentInBondAmendment);
		}

		void ValidateAndDeclareAMS(ActionCode actionCode)
		{
			if (Header != null)
			{
				var parent = (BusinessObject)Header.Consol ?? Header;
				if (SaveData(parent))
				{
					if (Header.SCACInCarrier.IsEmpty)
					{
						Globals.Message.ShowError(MessageSender.Constants.Message.NoOrgProxySCACNotification(Header), MessageSender.Constants.Caption.NoOrgProxySCACCaption);
					}
					else
					{
						DeclareAMS(actionCode);
					}
				}
			}
			else
			{
				Globals.Message.ShowError(MessageSender.Constants.Message.NoHeaderNotification, MessageSender.Constants.Caption.NoHeaderCaption);
			}
		}

		void DeclareAMS(ActionCode actionCode)
		{
			var header = Header;
			MessageSendingAction messageSendingAction = null;

			using (var progressForm = new ProgressForm())
			{
				progressForm.ShowCancelButton = false;
				progressForm.ShowProgressBar = false;
				progressForm.CaptionResourceString = new ResourceStringData("a293db9c-720e-4d9e-9182-bae05ad4ce22", "Loading data");
				progressForm.ShowModalTo(MainForm);

				messageSendingAction = new MessageSendingAction(header, actionCode);
			}

			using (var form = new USAMSMessageSendingActionForm(messageSendingAction))
			{
				var isInBondAction = ActionCodeTool.IsInBondType(actionCode);
				if (isInBondAction ? form.BusinessEntity.Movements.Count == 0 : form.BusinessEntity.MessageSendingObjects.Count == 0)
				{
					ShowNoDataToSend(actionCode);
				}
				else if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					var warningMessage = form.BusinessEntity.GetWarningForEntitiesToSendThatAreWaitingForResponse();
					if (warningMessage.IsEmpty || Globals.Message.Show(warningMessage, Constants.Caption.EntitiesThatAreWaitingForResponseCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes)
					{
						try
						{
							var totalMessages = 0;
							using (var progressForm = new ProgressForm())
							{
								progressForm.ShowCancelButton = false;
								progressForm.ShowProgressBar = false;
								progressForm.CaptionResourceString = new ResourceStringData("ce3ec319-6aa3-4195-b23a-3d0c6645d8d1", $"Sending {form.FormCaption}");
								progressForm.ShowModalTo(form);

								totalMessages = form.BusinessEntity.CreateAMSMessages();
							}

							header.Factory.SuspendValidation();

							if (MainForm.FireSaveButton() == ContinueWithSave.Yes)
							{
								header.UpdateATDAfterSendingMessage(actionCode, form.BusinessEntity.MessageSendingObjects);
								Globals.Message.ShowInformation(MessageSender.Constants.Message.MessageSentNotification(totalMessages));
							}
							else
							{
								Globals.Message.ShowInformation(Constants.Message.MessageWillBeSentOnSavedNotification(totalMessages));
							}
						}
						finally
						{
							if (header.Factory.IsValidationSuspended)
							{
								header.Factory.ResumeValidation();
							}
						}
					}
				}
			}
		}

		bool SaveData(BusinessObject parent)
		{
			var result = true;

			if (parent.HasChanges)
			{
				if (Globals.Message.Show(Constants.Message.DataNotSavedNotification, Constants.Caption.SaveDataCaption, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					result = MainForm.FireSaveButton() == ContinueWithSave.Yes;
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		ZForm MainForm
		{
			get { return (ZForm)this.GetMainMenu().GetForm(); }
		}

		void ShowNoDataToSend(ActionCode actionCode)
		{
			switch (actionCode)
			{
				case ActionCode.SubsequentInBondOriginal:
					Globals.Message.ShowInformation(Constants.Message.NoInBondForDepartureOriginalNotification, Constants.Caption.NoInBondForSubsequentOriginalCaption);
					break;
				case ActionCode.SubsequentInBondAmendment:
					Globals.Message.ShowInformation(Constants.Message.NoInBondForDepartureAmendmentNotification, Constants.Caption.NoInBondForSubsequentAmendmentCaption);
					break;
				case ActionCode.InBondArrival:
					Globals.Message.ShowInformation(Constants.Message.NoInBondForArrivalNotification, Constants.Caption.NoInBondForArrivalCaption);
					break;
				case ActionCode.InBondExportation:
					Globals.Message.ShowInformation(Constants.Message.NoInBondForExportationNotification, Constants.Caption.NoInBondForExportationCaption);
					break;
				case ActionCode.InBondTransferOfLiability:
					Globals.Message.ShowInformation(Constants.Message.NoInBondForTransferOfLiabilityNotification, Constants.Caption.NoInBondForTransferOfLiabilityCaption);
					break;
				case ActionCode.PermitToTransfer:
					Globals.Message.ShowInformation(Constants.Message.NoPTTMovementsNotification, Constants.Caption.NoPTTCaption);
					break;
				case ActionCode.CancelPermitToTransfer:
					Globals.Message.ShowInformation(Constants.Message.NoCancelPTTNotification, Constants.Caption.NoPTTCaption);
					break;
				case ActionCode.InBondDiversion:
					Globals.Message.ShowInformation(Constants.Message.NoDiversionNotification, Constants.Caption.NoDiversionCaption);
					break;
				default:
					Globals.Message.ShowError(MessageSender.Constants.Message.NoBillsNotification, MessageSender.Constants.Caption.NoBillsCaption);
					break;
			}
		}

		public static class Constants
		{
			public static class Caption
			{
				public static string EntitiesThatAreWaitingForResponseCaption
				{
					get { return Res.GetString("USAMSPlugIn|5C9FF9AE-4642-4E5A-9222-6B01F1F32B3D", "Pending Messages"); }
				}

				public static string InvalidRegistrySettingCaption
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|4111138D-B839-4AF6-900D-CA4644907E06", "Invalid Registry Setting"); }
				}

				public static string SaveDataCaption
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|6C0FBCB9-E19C-426C-A2AB-1CB2296EC17B", "Save Data"); }
				}

				public static string NoPTTCaption
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|PTTMenuItem|DD104860-0323-4383-887F-B261A81D9C1E", "No Permit To Transfer Messaging"); }
				}

				public static MultilingualString PTTMenuItemText
				{
					get { return ResString.GetMultilingualString("3D0BB344-B249-43C9-9451-352A310C07BA", "Send &Permit To Transfer Messages"); }
				}

				public static MultilingualString CancelPTTByBillOfLading
				{
					get { return ResString.GetMultilingualString("98476D68-1596-461C-8C36-C47E6359900B", "Cancel &Permits To Transfer By Bill Of Lading"); }
				}

				public static MultilingualString VesselArrivalMenuItemText
				{
					get { return ResString.GetMultilingualString("53A10BCE-14B3-41CD-8CD8-646D79622686", "Send &Arrival Messages"); }
				}

				public static MultilingualString VesselDepartureMenuItemText
				{
					get { return ResString.GetMultilingualString("5061A47C-278F-408C-AA26-5B2C6E338252", "Send &Departure Messages"); }
				}

				public static MultilingualString ChangeEstDateOfArrival
				{
					get { return ResString.GetMultilingualString("D61FA9C5-C11C-4972-B8F9-573F3C712068", "&Change in the Estimated Date of Arrival"); }
				}

				public static string RequestForInBondDiversionMenuItemText
				{
					get { return ResString.GetMultilingualString("DCBEA9E3-CB03-4552-ADE6-559C8BA0BAE6", "Send &Request For In-Bond Diversion Messages"); }
				}

				public static string NoInBondForSubsequentOriginalCaption
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|367BBE2A-A75B-49B9-8578-F73E55ED2146", "No In-Bond Movement For Departure Subsequent Messaging"); }
				}

				public static string NoInBondForSubsequentAmendmentCaption
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|FBF599E4-68D6-47E8-8930-BA03C27D7A26", "No In-Bond Movement For Subsequent Amendment Messaging"); }
				}

				public static string NoInBondForArrivalCaption
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|5CC2D6B4-45CF-4792-ACC3-139ED148DFD7", "No In-Bond Movement For Arrival Messaging"); }
				}

				public static string NoInBondForExportationCaption
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|9E466A11-2C55-4670-AB70-917E1B600913", "No In-Bond Movement For Exportation Messaging"); }
				}

				public static string NoInBondForTransferOfLiabilityCaption
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|25BD8077-C96E-4fe2-BDD9-0D9DB978B857", "No In-Bond Movement For Transfer of Liability Messaging"); }
				}

				public static string NoDiversionCaption
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|41000158-C47D-4604-8EE2-2ED37E3C7384", "No Movement For Request for In-Bond Diversion Messaging"); }
				}
			}

			public static class Message
			{
				public static string DataNotSavedNotification
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|F62959E1-93B2-4ACA-9B42-5F2195A82659", "The data has not yet been saved. Do you want to save and proceed?"); }
				}

				public static string NoPTTMovementsNotification
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|PTTMenuItem|F8EB4A05-2BB5-4670-9EDA-7A5AB07CAD09", "There is no Permit To Transfer Movements available for sending 'Permit To Transfer' message to Customs.\r\nPossibly all PTT Movements are either pending Customs response or have already been accepted by Customs."); }
				}

				public static string NoCancelPTTNotification
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|PTTMenuItem|2923B92E-33C8-4D8B-A88A-5BBB4E5C0226", "There is no Permit To Transfer Movements available for sending 'Cancel Permit To Transfer' message to Customs."); }
				}

				public static string MessageWillBeSentOnSavedNotification(int totalMessages)
				{
					return totalMessages == 1
						? ResString.GetMultilingualString("0a300851-a5bc-4584-9273-bee73fa0057b", "1 message will be sent when the consol is saved.")
						: ResString.GetMultilingualString("c1d7df31-0a3b-418d-b1e6-e52772dca358", "{0} messages will be sent when the consol is saved.", totalMessages);
				}

				public static string NoInBondForTransferOfLiabilityNotification
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|1653744B-2EE8-4be5-B3AA-467716889595", "There is no In-Bond Movement available for sending a 'Transfer of Liability' message to Customs.\r\nPossibly all In-Bond Movements are pending Customs response."); }
				}

				public static string NoDiversionNotification
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|9742034F-A207-47BC-9F8A-7DD84A817FA8", "There is no In-Bond Movement available for sending a 'Request for In-Bond Diversion' message to Customs."); }
				}

				public static string NoInBondForExportationNotification
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|2117DE0D-1D94-4e50-A4BF-60046FE280E8", "There is no In-Bond Movement available for sending a 'Exportation' message to Customs.\r\nPossibly all In-Bond Movements are either pending Customs response or do not have Entry Type '62' or '63'."); }
				}

				public static string NoInBondForArrivalNotification
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|67C723FE-B9BF-4edb-8984-CA6F1A0C78C9", "There is no In-Bond Movement available for sending a 'Arrival' message to Customs.\r\nPossibly all In-Bond Movements are pending Customs response."); }
				}

				public static string NoInBondForDepartureAmendmentNotification
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|A127D0CC-B4C7-476E-8B4A-1EF1EF2A688A", "Departure Amend message(s) cannot be sent to Customs.\r\nAt least one In-Bond Movement should have a cleared status."); }
				}

				public static string NoInBondForDepartureOriginalNotification
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|37BEB192-0C14-45d7-A773-2C16F5797383", "There is no In-Bond Movement available for sending a 'Departure Original' message to Customs.\r\nPossibly all In-Bond Movements are either pending Customs response or have already been accepted by Customs."); }
				}

				public static string InvalidRegistrySettingNotification(string error)
				{
					return ResString.GetMultilingualString("USAMSPlugIn|InBondMenuItem|41313018-D69E-4BC2-A4B4-E61A35220283", "Cannot create AMS message as the following settings are invalid:\r\n{0}", error);
				}
			}
		}
	}
}
