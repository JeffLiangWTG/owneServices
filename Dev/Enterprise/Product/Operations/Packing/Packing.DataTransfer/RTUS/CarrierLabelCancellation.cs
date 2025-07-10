using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Core;
using WTG.Foundation.Http;
using WTG.RTUS.Interface;

namespace Enterprise.Packing.DataTransfer
{
	public sealed class CarrierLabelCancellation : ICarrierLabelCancellation
	{
		public CarrierLabelCancellation(IHttpClientFactory httpClientFactory)
		{
			HttpClientFactory = httpClientFactory; // can be null, let the Carrier Label Manager figure it out
			RTUSPackageSubscriber = new RTUSPackageSubscriber();
		}

		IHttpClientFactory HttpClientFactory { get; }
		RTUSPackageSubscriber RTUSPackageSubscriber { get; }

		ReturnResult ICarrierLabelCancellation.SubscribePackageForCancellation(PkgPackage package)
		{
			Argument.NotNull(package, nameof(package));
			return RTUSPackageSubscriber.SubscribePackage(package, RequestType.Cancellation);
		}

		IEnumerable<CancellationResponse> ICarrierLabelCancellation.CancelPackages(IEnumerable<PkgPackage> packagesToCancel)
		{
			Argument.NotNull(packagesToCancel, nameof(packagesToCancel));

			var packages = packagesToCancel.ToArray();
			if (packages.Any(p => p == null))
			{
				throw new ArgumentException("Cannot have null Package elements in Collection.", nameof(packagesToCancel));
			}

			var responses = new List<CancellationResponse>();

			using (var manager = ObjectFactory.New<ICarrierLabelManager>(null, HttpClientFactory))
			{
				var registry = ObjectFactory.Get<ITransportRegistry>();
				var rtusTypeAndUrlByPackageJob = new Dictionary<ZGuid, (RTUSCBA Type, Uri Url, string ErrorMessage)>();

				foreach (var package in packages.Where(p => RTUSPackageSubscriber.IsPackageSubscribed(p)))
				{
					var rtusRequestXml = RTUSPackageSubscriber.GetRequestXML(package);

					if (rtusRequestXml.ShipmentDataObject != null && !string.IsNullOrEmpty(rtusRequestXml.PackageID))
					{
						if (!rtusTypeAndUrlByPackageJob.TryGetValue(rtusRequestXml.PackageJobPK, out var typeAndUrl))
						{
							var result = GetRTUSCBAAndUrl(registry, rtusRequestXml.PackingParent);
							rtusTypeAndUrlByPackageJob[rtusRequestXml.PackageJobPK] = typeAndUrl = result;
						}

						if (string.IsNullOrEmpty(typeAndUrl.ErrorMessage))
						{
							var response = manager.PushCarrierLabelRequest(RequestType.Cancellation, rtusRequestXml.ShipmentDataObject, typeAndUrl.Type, typeAndUrl.Url);
							responses.Add(new CancellationResponse(rtusRequestXml.PackageJobPK, rtusRequestXml.PackingParent, response, rtusRequestXml.PackageID));
						}
						else
						{
							var failResponse = RequestType.Cancellation.GetFailResponse(Res.GetString("e297b309-e403-405d-a9dd-09bfaddacef7", "Failed to cancel Package '{0}' because {1}.", rtusRequestXml.PackageID, typeAndUrl.ErrorMessage));
							responses.Add(new CancellationResponse(rtusRequestXml.PackageJobPK, rtusRequestXml.PackingParent, failResponse, rtusRequestXml.PackageID));
						}
					}
				}
			}

			return responses;
		}

		static (RTUSCBA Type, Uri Url, string ErrorMessage) GetRTUSCBAAndUrl(ITransportRegistry registry, IPackingParent packingParent)
		{
			(RTUSCBA Type, Uri Url, string ErrorMessage) result = default;

			var carrierBookingAgent = packingParent?.CarrierBookingAgent;
			if (carrierBookingAgent != null)
			{
				var rtusOption = registry.GetRTUSOption(carrierBookingAgent.PK.ToGuid());
				if (rtusOption != null)
				{
					result = (rtusOption.RTUSCBA, rtusOption.WrappedUrl, null);
				}
				else
				{
					result.ErrorMessage = Res.GetString("1a4726ce-511d-4026-a98c-9056b64db62b", "no RTUS URL was provided for Organization '{0}'", carrierBookingAgent.OH_Code);
				}
			}
			else
			{
				result.ErrorMessage = Res.GetString("565ac728-bf6b-44bf-b904-57e9b65a8413", "no Carrier Booking Agent is specified");
			}

			return result;
		}
	}
}
