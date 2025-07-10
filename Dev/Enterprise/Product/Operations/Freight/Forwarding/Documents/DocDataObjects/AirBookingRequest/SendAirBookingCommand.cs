using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class SendAirBookingCommand : AirBookingCommand
	{
		public SendAirBookingCommand()
		{
			isEnabled = true;
		}

		bool isEnabled;

		public override string Id => CommandIds.SendMessage;
		public override string Caption => CommandResources.Captions.SendMessage;
		public override bool IsEnabled => isEnabled;
		public override bool IsVisible => true;

		protected override bool OnInvokeCommand()
		{
			if (!CheckMessageValid())
			{
				return false;
			}

			var (airBookingRequest, uri) = TryGetValidAirBookingRequestAndUrl();
			if (airBookingRequest == null || uri == null)
			{
				return false;
			}

			var factory = GetFactory();

			var progressManager = ObjectFactory.Get<IAirBookingProgressManager>();

			ResponseProcessResult SendBookingRequest(UShipment uxml, out string userMessage)
			{
				userMessage = string.Empty;

				var tokenSource = new CancellationTokenSource();

				try
				{
					using (progressManager.Show(tokenSource.Cancel))
					{
						var client = new AirBookingRequestApiClient(uri);

						var requestProcessor = new AirBookingRequestProcessor(uxml, u => SendRequestUXml(u, client, tokenSource));

						var response = ProcessRequestAndResponse(requestProcessor, factory);

						if (response.ResponseType != ResponseType.Rates)
						{
							if (response.ResponseType == ResponseType.InterchangeSent)
							{
								FlightBookingStatusManager.UpdateBookingStatus(factory, airBookingRequest, Core.Constants.TransportStatus.Requested);
							}

							SaveDocumentToEDocs(airBookingRequest);
							ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);

							isEnabled = response.ResponseType == ResponseType.InterchangeRejected;

							Notify(new MessageSentEvent(documentInfo.Document));

							userMessage = response.UserMessage
								?? Res.GetString("DDEF44F3-BB51-4E6B-9F03-B673057F1462", "Message has been sent.");
						}

						return response;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (!tokenSource.IsCancellationRequested)
					{
						userMessage = AirBookingErrorMessageHelper.GetHumanReadableError(ex);
					}

					return new ResponseProcessResult(ResponseType.Invalid);
				}
			}

			var airBookingShipment = (UShipment)Document
				.ToUniversalXmlDataObject();

			var res = SendBookingRequest(airBookingShipment, out var message);

			if (res.ResponseType == ResponseType.Rates)
			{
				var airlinePrefix = airBookingRequest.MasterAirWaybillNumber.SubstringSafe(0, 3);
				var selectedRate = SelectRate(airlinePrefix, res.Shipment);

				if (selectedRate != null)
				{
					airBookingShipment.SetSubShipmentCollection(() => new DataObjectList<UShipment>
					{
						selectedRate
					});

					res = SendBookingRequest(airBookingShipment, out message);
				}
				else
				{
					message = string.Empty;
				}
			}

			ShowMessage(message);

			return res.ResponseType != ResponseType.Invalid;
		}

		bool CheckMessageValid()
		{
			var errorMessage = string.Empty;
			var check = CheckAllowSendMessage()
				&& CheckHasNoChanges(out errorMessage)
				&& CheckHasNoErrors(out errorMessage)
				&& CheckHasNoMessageErrors(out errorMessage);

			if (!check)
			{
				ShowMessage(errorMessage);
			}

			return check;
		}

		(AirBookingRequest airBookingRequest, Uri url) TryGetValidAirBookingRequestAndUrl()
		{
			var airBookingRequest = GetAirBookingRequest();

			if (airBookingRequest == null)
			{
				return (null, null);
			}

			if (!CheckCarrierIsSupported(airBookingRequest.MasterAirWaybillNumber, out var errorMessage))
			{
				ShowMessage(errorMessage);
				return (null, null);
			}

			var (uri, uriErrorMessage) = AirBookingUriProvider.GetBookingUri(airBookingRequest.MasterAirWaybillNumber);
			if (uri == null)
			{
				ShowMessage(uriErrorMessage);
				return (null, null);
			}

			return (airBookingRequest, uri);
		}

		UShipment SelectRate(string carrierPrefix, UShipment shipment)
		{
			if (shipment.SubShipmentCollection == null
				|| shipment.SubShipmentCollection.Count == 0)
			{
				return null;
			}

			var rateUXmlMap = new Dictionary<IBookingRate, UShipment>();

			foreach (var subShipment in shipment.SubShipmentCollection)
			{
				var rate = BookingRateBuilder.Build(carrierPrefix, subShipment);
				rateUXmlMap[rate] = subShipment;
			}

			var selector = ObjectFactory.Get<IBookingRateSelector>();
			var selectedRate = selector.SelectRate(rateUXmlMap.Keys);

			return selectedRate != null ? rateUXmlMap.GetValueSafe(selectedRate) : null;
		}

		string SendRequestUXml(UShipment uxml, AirBookingRequestApiClient client, CancellationTokenSource tokenSource)
		{
			var interchangeXml = uxml.ToUniversalXml().WrapInInterchange();

			var result = client.Post(interchangeXml, tokenSource.Token)
				.EnsureSuccessStatusCodeAsync().GetAwaiter().GetResult();

			AddLogs(interchangeXml);

			return result.Content;
		}

		void AddLogs(string messageSent)
		{
			if (documentInfo.DocumentData is IStmALogParent logParent)
			{
				logParent.CreateMessageSentLog(documentInfo.Document.Name);
				logParent.CreateDataExportEventLog(messageSent);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		void SaveDocumentToEDocs(AirBookingRequest airBookingRequest)
		{
			const string documentName = "Air Booking";

			var eDocDocumentName = airBookingRequest.HasTermsAndConditions()
				? FormattableString.Invariant($"{documentName} ({documentInfo.DocumentData.CalculateLogs(Events.MessageSentCode)})")
				: documentName;

			SaveDocumentToEDocs(airBookingRequest, eDocDocumentName);
		}
	}
}
