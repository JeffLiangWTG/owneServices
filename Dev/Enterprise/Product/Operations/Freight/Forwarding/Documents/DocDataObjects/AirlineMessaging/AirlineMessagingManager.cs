using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirlineMessaging.Processors;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirlineMessaging
{
	public class AirlineMessagingManager : IAirlineMessagingManager
	{
		public bool Send<T>(T documentObject, KeyValuePair<string, string>[] additionalInfoCollection, out string failureReason)
		{
			failureReason = null;
			var success = true;

			try
			{
				var consol = documentObject as ForwardingConsol ?? throw new ArgumentException($"'{documentObject}' is not a valid {nameof(ForwardingConsol)} instance.");

				var consolDataObject = consol.ToDataObject().IncludeAddInfo(additionalInfoCollection);

				ProcessUniversalShipment(consolDataObject, consol);

				if (!string.IsNullOrEmpty(consolDataObject.EventBranchHomePort?.Code))
				{
					consolDataObject.EventBranchHomePort.Code = consolDataObject.EventBranchHomePort.Code.UnlocoToIata(consol.Factory);
				}

				var xus = consolDataObject.ToUniversalXml().WrapInInterchange(Constants.MessageTarget);
				var client = new PelicanApiClient(ComposeUrl(consol));
				var logger = new AirlineMessagingLogger();
				var cancelToken = new CancellationToken();

				logger.LogRequest(consol, xus);

				var responseProcessor = new AirlineMessagingProcessor();
				if (!responseProcessor.TryProcessResponse(client.Post(xus, cancelToken).Content, out var originalMessages, out var parsedEvents, out var exception))
				{
					success = false;
					failureReason = exception;
					if (parsedEvents == null)
					{
						logger.LogFailedMessageValidation(consol, failureReason);
				}
			}
				logger.LogResponse(consol, originalMessages, parsedEvents);
			}
			catch (Exception ex)
			{
				success = false;
				failureReason = ex.Message;
				ErrorReporter.ReportOnce($"Error when calling Airline Messaging Gateway: {ex.Message}.", ex);
			}

			return success;
		}

		void ProcessUniversalShipment(UniversalDataBuss.DataObjects.Universal.Shipment universalShipment, ForwardingConsol consol)
		{
			var processors = new List<IUniversalShipmentProcessor>()
			{
				new KnownConsignorProcessor(consol),
				new AESCollectionProcessor(consol),
				new ACASProcessor(consol),
				new CompanyIdAndNumberProcessor(consol)
			};
			processors.ForEach(x => x.Process(universalShipment));
		}

		Uri ComposeUrl(ForwardingConsol consol)
		{
			var baseUrl = FreightDataRegistry.Instance.PelicanApiUrl.Value;
			var relativePath = $"cargo-imp/carriers/{consol.MasterBillAirlinePrefix}/awbs/{consol.MasterBillMAWB}/process";
			return new Uri($"{baseUrl.TrimEnd('/')}/{relativePath}");
		}
	}
}
