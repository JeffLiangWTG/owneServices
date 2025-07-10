using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.Messaging;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.GUI
{
	public class MessagingMenu : ZMenuItem
	{
		public MessagingMenu(Trip trip)
			: base(ResString.GetMultilingualString("92253241-514e-4c27-b968-77233b093d95", "Messaging"))
		{
			Argument.NotNull(trip, "trip");
			this.trip = trip;

			const string splitter = "-";
			MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("c6e3cf06-0d9e-450c-bb34-a8b3a617da7e", "Submit Complete e-Manifest w/ACE ID"), (s, e) => SendMessage(MessageTypes.Codes.eManifest)));
			MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("ff9fec4b-1dc3-45ad-a998-5cb7d4cd028c", "Change Complete e-Manifest w/ACE ID Header Only"), (s, e) => SendMessage(MessageTypes.Codes.eManifest, MessageSubTypes.ReplaceHeader)));

			MenuItems.Add(new ZMenuItem(splitter));

			MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("4D72267C-7545-4E61-94ED-752513A620C2", "Submit eManifest (No ACE ID)"), new EventHandler(SubmitEManifestNoACEID_Click)));
			MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("e8c4e90b-b938-4f01-b74d-1042b5a39d38", "Submit Unassociated Shipments"), (s, e) => SendMessage(MessageTypes.Codes.UnassociatedShipments)));
			MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("ae2c0457-a3ac-481f-adad-412e299f35f9", "Submit Preliminary Trip Details"), (s, e) => SendMessage(MessageTypes.Codes.PreliminaryTrip)));
			MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("6b7b6100-cc6a-4cdf-a058-c8430358fb06", "Submit Crew/Passengers Details"), (s, e) => SendMessage(MessageTypes.Codes.CrewAndPassenger)));
			MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("7dee37eb-d74e-47b1-86ea-c2f8f665b2fb", "Confirm Trip Details Completed"), (s, e) => SendMessage(MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Confirmation)));

			MenuItems.Add(new ZMenuItem(splitter));
			MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("6642cf93-e6e4-48fa-86cb-18637307a8ab", "Cancel Trip And Linked Shipments"), (s, e) => SendMessage(MessageTypes.Codes.eManifest, MessageSubTypes.Withdraw)));

			MenuItems.Add(new ZMenuItem(splitter));
			MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("df22d9e1-3df1-489b-847f-f6bf7fa3b68d", "Register Crew Information"), (s, e) => SendMessage(MessageTypes.Codes.CrewOrEquipmentRegistration)));
		}

		void SubmitEManifestNoACEID_Click(object sender, EventArgs e)
		{
			if (trip.Shipments.Count > 9)
			{
				Globals.Message.ShowError(GetTooManyShipmentsMessageError(string.Empty), MessageHasNotBeenSent);
			}
			else
			{
				SubmitEManifestNoACEID();
			}
		}

		void SubmitEManifestNoACEID()
		{
			var continueWithSend = false;
			if (PreSaveMessage())
			{
				continueWithSend = true;
			}

			if (continueWithSend)
			{
				var wrapper = new eManifestMessageSendingObjectParent(trip);
				wrapper.DefaultShouldSend();

				using (var form = new MessageSendingForm(wrapper))
				{
					continueWithSend = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
				}

				if (continueWithSend)
				{
					var manager = new MultiEManifestMessageManager(wrapper, new MessageNotificationCollector());
					manager.SendMessages(shouldSkipNotification: ShouldSkipShipmentValidation);
				}
			}
		}

		bool PreSaveMessage()
		{
			var result = true;
			if (trip.HasChanges)
			{
				var messageBoxResult = Globals.Message.Show(
					Res.GetString("DA936EBA-32D8-4CCE-A489-E4D449D3684D", "The Job has not yet been saved. Do you want to save and proceed?"),
					Res.GetString("EEBC5115-94EA-419A-AF33-74CAF1A8E6E3", "Save Job"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning,
					DialogResult.Yes);
				result = messageBoxResult == DialogResult.Yes && ((ZForm)GetMainMenu()?.GetForm()).FireSaveButton() == ContinueWithSave.Yes;
			}
			return result && !trip.HasChanges;
		}

		void SendMessage(string messageType, MessageSubTypes actionCode = MessageSubTypes.Undefined)
		{
			if ((messageType == MessageTypes.Codes.UnassociatedShipments || messageType == MessageTypes.Codes.PreliminaryTrip || messageType == MessageTypes.Codes.CrewAndPassenger) && trip.Shipments.Count > 9)
			{
				Globals.Message.ShowError(GetTooManyShipmentsMessageError(messageType), MessageHasNotBeenSent);
			}
			else
			{
				SendMessageCore(messageType, actionCode);
			}
		}

		void SendMessageCore(string messageType, MessageSubTypes actionCode = MessageSubTypes.Undefined)
		{
			var wrapper = new eManifestMessageWrapper(trip, messageType);
			if (actionCode == MessageSubTypes.Withdraw)
			{
				wrapper.CalculateMessageTypeForCancellation();
			}

			var manager = new eManifestMessageManager(wrapper, wrapper.MessageType, new UserNotification(), shouldSkipNotification: ShouldSkipShipmentValidation);
			manager.SendMessage(actionCode, runPreSaveValidation: !ShouldSkipShipmentValidation);
		}

		string GetTooManyShipmentsMessageError(string messageType)
		{
			var messageFriendlyName = messageType.IsNullOrEmpty() ? "eManifest (No ACE ID)" : new MessageTypes().GetDescriptionFromCode(messageType);
			return Res.GetString("AB3FABC1-CC11-4919-BE0B-C76968CD1430", "System cannot send {0} message as there are too many shipments for this type of message. Instead send the Completed Manifest with ACE ID.", messageFriendlyName);
		}

		string MessageHasNotBeenSent
		{
			get
			{
				return Res.GetString("AC352A16-00A3-42CA-A1D9-B82D2B9F48F7", "Message has not been sent");
			}
		}

		bool ShouldSkipShipmentValidation => trip.IsFromHVLV && HVLVDataRegistry.Instance.SkipeManifestShipmentValidation.Value;

		readonly Trip trip;
	}
}
