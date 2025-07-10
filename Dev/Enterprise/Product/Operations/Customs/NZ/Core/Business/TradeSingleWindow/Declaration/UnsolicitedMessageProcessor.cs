using System.Globalization;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Registry;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	class UnsolicitedMessageProcessor : DeclarationMessageProcessor<NZCResponse>
	{
		public UnsolicitedMessageProcessor(LoggingInformation logger)
			: base(logger, "Import")
		{
		}

		#region Overrides

		protected override bool IsWarningAboutTotalAmountInconsistencyRequired
		{
			get { return false; }
		}

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return ZGuid.Empty; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return ZString.Empty; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return ZGuid.Empty; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return ZString.Empty; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return ZGuid.Empty; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return ZString.Empty; }
		}

		protected string ResponseIsFor
		{
			get { return "The message is for the: " + Response.Recipient + "."; }
		}

		protected override void ProcessCore()
		{
			Report.AddLine("An unsolicited Delivery Order message has been received from New Zealand Customs.");
			Report.AddLine(ResponseIsFor);
			Report.AddLine("Below are the details contained in the message:");
			Report.AddLine();
			Report.AddLine(Response.MessageTypeDescription);
			Report.AddSeparator();
			Report.AddLine("Entry Number", Response.DeclarationID);
			Report.AddLine("Message No", Response.MessageNumber);
			Report.AddLine();
			OutputResponseStatus();

			if (Response.IsCustomsResponse)
			{
				if (Response.TotalAmount != 0 || !string.IsNullOrEmpty(Response.PaymentMethod))
				{
					Report.AddLine();
					Report.AddLine("Amount Returned", Response.TotalAmount != 0
													  ? Response.TotalAmount.ToString("C", CultureInfo.CurrentCulture)
													  : "Amount Not Included in Response.");
					Report.AddLine("Terms", Response.PaymentMethodDescription);
				}
			}

			ProcessDeliveryInstruction();
			ProcessCustomsInstructions();
		}

		protected override void SendEmail(ZArchitecture.Environment.EmailDef email)
		{
			SendReport(email, null, UnsolicitedDOMode, UnsolicitedDOGroup);
		}

		protected override void WriteEntryStatus()
		{
			// This is an usolicited message. There is not CW1 entry
		}

		protected override DeclarationResponse PreviousResponsesWithSameMessageType
		{
			get { return null; }
		}

		#endregion // Overrides

		protected ZGuid UnsolicitedDOGroup
		{
			get { return NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponsesSendToGroup.Value; }
		}

		protected ZString UnsolicitedDOMode
		{
			get { return NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponses.Value; }
		}
	}
}
