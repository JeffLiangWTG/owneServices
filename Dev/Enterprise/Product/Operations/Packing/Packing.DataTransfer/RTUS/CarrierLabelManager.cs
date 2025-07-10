using System;
using System.IO;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.IO;
using Enterprise.Packing.Business;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.RTUS.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using WTG.Foundation.Http;
using WTG.RTUS.Interface;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer
{
	public sealed class CarrierLabelManager : ICarrierLabelManager
	{
		public CarrierLabelManager(Action<Exception> onError, IHttpClientFactory httpClientFactory)
		{
			HttpClientFactory = httpClientFactory ?? new HttpClientFactory(HttpMessageHandlerFactory.GetHandler);
			XmlWriter = ObjectFactory.New<IXmlWriter>();
			OnError = onError;

			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		IHttpClientFactory HttpClientFactory { get; }
		IXmlWriter XmlWriter { get; }
		Action<Exception> OnError { get; }

		public T PushCarrierLabelRequest<T>(RequestType<T> requestType, ITopLevelDataObject packageUniversalShipment, RTUSCBA type, Uri url)
			where T : IRTUSResponse
		{
			Argument.NotNull(packageUniversalShipment, nameof(packageUniversalShipment));

			T result;

			using (var uxmlStream = (SubStreamableStream)new MemoryStream())
			{
				XmlWriter.WriteXML(packageUniversalShipment, uxmlStream);

				result = ObjectFactory.Get<IRTUSProcessor>().PushMessage(requestType, HttpClientFactory, uxmlStream, type, url);
			}

			return result;
		}

		T ICarrierLabelManager.PushCarrierLabelRequest<T>(RequestType<T> requestType, PkgPackage package, RTUSCBA type, Uri url)
		{
			Argument.NotNull(package, nameof(package));

			T result;

			var packageUniversalShipment = GetPackageUniversalShipment(package, requestType);
			if (packageUniversalShipment != null)
			{
				result = PushCarrierLabelRequest(requestType, packageUniversalShipment, type, url);
			}
			else
			{
				var message = Res.GetString("95bace2b-c113-4e0b-bd4e-92452aba77ec", "Failed to generate Universal Shipment from Package '{0}'.", package.KP_PackageID);
				result = requestType.GetFailResponse(message);
			}

			return result;
		}

		UniversalShipment GetPackageUniversalShipment(PkgPackage package, RequestType requestType)
		{
			var writeManager = new DataWritingManager(new ActionInfo(null, package));

			try
			{
				return new PkgPackageUniversalShipmentDataObjectWriter(writeManager, requestType).GetDataObject(package);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (OnError != null)
				{
					OnError(ex);
				}
				else
				{
					throw;
				}
			}

			return null;
		}

		public void Dispose()
		{
			HttpClientFactory.Dispose();

			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}
	}
}
