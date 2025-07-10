using System;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.US
{
	sealed class AcasMessagingExtensions : BaseMessagingExtensions
	{
		public AcasMessagingExtensions(IDocument document)
		{
			this.document = document ?? throw new ArgumentNullException(nameof(document));
		}

		readonly IDocument document;

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications)
		{
			if (document.Data is IDynamicData dynamicData
				&& dynamicData.Value is AirCargoAdvanceScreening acas
				&& acas.CanSendMessage)
			{
				return true;
			}

			return null;
		}

		// message withdrawal is not allowed
		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications) => false;

		// reset to original is not allowed
		public override bool? ContinueWithResetToOriginal(IUserNotifications notifications) => false;

		public override string GetXmlNamespace()
		{
			if (document.Data is IDynamicData dynamicData
				&& dynamicData.Value is AirCargoAdvanceScreening acas)
			{
				return acas.State == AcasState.AcknowledgementRequired
					? "/AcknowledgementOfHold/1"
					: "/AirCargoAdvanceScreening/1";
			}

			return null;
		}

		public override string GetMessageStatus()
		{
			if (document.Data is IDynamicData dynamicData
				&& dynamicData.Value is AirCargoAdvanceScreening acas)
			{
				return GetDisplayInformation(acas.State);
			}

			return null;
		}

		string GetDisplayInformation(AcasState state)
		{
			switch (state)
			{
				case AcasState.OriginalSent:
					return Res.GetString("62994131-DDAF-4F66-AF91-745DA5978DD4", @"The message previously sent has not yet received a response.
Please wait for a response before resending.");

				case AcasState.AmendmentSent:
					return Res.GetString("0ACE34F4-7002-44AA-A263-C0CBA5DBC6B9", @"An amendment message in response to Selectee Data Issues has been sent.
Please wait for a response before further action.");

				case AcasState.AcknowledgementSent:
					return Res.GetString("F6476EC7-5A44-4553-A306-67341D4E28B3", @"An Acknowledgement message has been sent and has not received a response.
Please wait for a response before further action.");

				case AcasState.AcknowledgementRequired:
					return Res.GetString("D53B19C7-1EF4-4F1C-A623-2037621F4008", @"The latest response from CBP is ""On Hold"" (6H, 7H or 8H).
Use the 'Send Message' option to send an Acknowledgement message.");

				case AcasState.HoldInPlace:
					return Res.GetString("E31630D8-D4AA-47B8-B75A-9163D3B68F4C", @"This Shipment is currently on ""Hold Currently in Place"" with CBP, per the latest response.
Wait for a 6I, 7I or 8I ""Hold Removed"" response before resending the message.");

				case AcasState.AmendmentRequired:
					return Res.GetString("E1FAA7F2-4529-4846-9295-5155B3DF6446", @"The latest status received from US Customs is Selectee Data Issue Hold.
Amend the data and resend the message to resolve the hold.");

				case AcasState.AssessmentOngoing:
					return Res.GetString("C2162597-E184-40C7-A6EF-A3D00D282B25", @"A risk assessment is currently being conducted by CBP.
Please wait for an additional response before further action.");

				case AcasState.HoldRemoved:
					return Res.GetString("68E411A3-8BE9-49B8-859E-0CFD72D41C64", @"The ""Hold"" status of this Shipment has been removed by CBP.
This Shipment has been approved to be uplifted/loaded on a flight to United States.");

				case AcasState.AssessmentComplete:
					return Res.GetString("2BC4E476-F116-459F-9B47-5362A6A830E5", "This Shipment has been approved to be uplifted/loaded on a flight to United States.");

				case AcasState.None:
					return Res.GetString("838B5410-0516-47FA-AE55-276AD08D99F0", "No ACAS Shipment Report Messages Have Been Sent.");

				default:
					return null;
			}
		}
	}
}
