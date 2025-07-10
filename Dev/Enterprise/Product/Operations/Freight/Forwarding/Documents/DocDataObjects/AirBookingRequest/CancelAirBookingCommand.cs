using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class CancelAirBookingCommand : AirBookingCommand
	{
		public CancelAirBookingCommand()
		{
			isEnabled = true;
		}

		bool isEnabled;

		public override string Id => CommandIds.SendWithdrawal;
		public override string Caption => CommandResources.Captions.SendWithdrawal;
		public override bool IsEnabled => isEnabled;
		public override bool IsVisible => true;

		protected override bool OnInvokeCommand()
		{
			var errorMessage = string.Empty;
			var check = CheckAllowSendMessage()
				&& CheckHasNoChanges(out errorMessage)
				&& CheckHasNoErrors(out errorMessage)
				&& CheckHasNoMessageErrors(out errorMessage);

			if (!check)
			{
				ShowMessage(errorMessage);
				return false;
			}

			var withdrawalReason = GetWithdrawalReason();

			if (string.IsNullOrWhiteSpace(withdrawalReason))
			{
				return false;
			}

			var airBookingRequest = GetAirBookingRequest();

			if (airBookingRequest == null)
			{
				return false;
			}

			var (uri, uriErrorMessage) = AirBookingUriProvider.GetCancellationUri(airBookingRequest.MasterAirWaybillNumber);

			if (uri == null)
			{
				ShowMessage(uriErrorMessage);
				return false;
			}

			var factory = GetFactory();

			var uxml = (UShipment)Document.ToUniversalXmlDataObject(MessageType.Withdrawal);

			var tokenSource = new CancellationTokenSource();
			void RequestCancel() => tokenSource.Cancel();

			try
			{
				var userMessage = string.Empty;

				var progressManager = ObjectFactory.Get<IAirBookingProgressManager>();

				using (progressManager.Show(RequestCancel))
				{
					var client = new AirBookingRequestApiClient(uri);

					var requestProcessor = new AirBookingRequestProcessor(uxml, u => SendRequestUXml(u, client, tokenSource, withdrawalReason));

					var response = ProcessRequestAndResponse(requestProcessor, factory);

					if (response.ResponseType == ResponseType.InterchangeSent)
					{
						FlightBookingStatusManager.UpdateBookingStatus(factory, airBookingRequest, Core.Constants.TransportStatus.CancellationRequested);
					}
					else if (response.ResponseType == ResponseType.BookingCancelled)
					{
						FlightBookingStatusManager.UpdateBookingStatus(factory, airBookingRequest, Core.Constants.TransportStatus.Cancelled);
					}

					SaveDocumentToEDocs(airBookingRequest);
					ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);

					isEnabled = response.ResponseType == ResponseType.InterchangeRejected;

					Notify(new MessageSentEvent(documentInfo.Document));

					userMessage = response.UserMessage
						?? Res.GetString("87ef9678-dfa4-49e3-ab2f-84e9a1842496", "Message withdrawal has been sent.");
				}

				ShowMessage(userMessage);

				return true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (!tokenSource.IsCancellationRequested)
				{
					var error = AirBookingErrorMessageHelper.GetHumanReadableError(ex);
					ShowMessage(error);

					return false;
				}

				return true;
			}
		}

		string SendRequestUXml(UShipment uxml, AirBookingRequestApiClient client, CancellationTokenSource tokenSource, string withdrawalReason)
		{
			var interchangeXml = uxml.ToUniversalXml().AddCancellationNote(withdrawalReason).WrapInInterchange();

			var response = client.Post(interchangeXml, tokenSource.Token)
				.EnsureSuccessStatusCodeAsync().GetAwaiter().GetResult();

			AddLogs(withdrawalReason, interchangeXml);

			return response.Content;
		}

		string GetWithdrawalReason()
		{
			var message = Res.GetString("163dada3-a451-4c4a-9434-758cac29e900", "You are sending a {0} Cancellation message. Please enter a reason for cancellation:", Document?.Name);
			var caption = Res.GetString("5fe9cb4c-216e-40c5-8bfe-f41178e39c62", "Cancellation Reason");

			return QueryUserResponse(message, caption);
		}

		void AddLogs(string withdrawalReason, string messageSent)
		{
			if (documentInfo.DocumentData is IStmALogParent logParent)
			{
				logParent.CreateMessageWithdrawalLog(documentInfo.Document.Name, withdrawalReason);
				logParent.CreateDataExportEventLog(messageSent);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		void SaveDocumentToEDocs(AirBookingRequest airBookingRequest)
		{
			const string documentName = "Air Booking Cancellation";

			var eDocDocumentName = airBookingRequest.HasTermsAndConditions()
				? FormattableString.Invariant($"{documentName} ({documentInfo.DocumentData.CalculateLogs(Events.MessageWithdrawCancelRequestCode)})")
				: documentName;

			SaveDocumentToEDocs(airBookingRequest, eDocDocumentName);
		}
	}
}
