using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Newtonsoft.Json;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public class EmissionSerializer : IHttpContentSerializer
	{
		const string JsonContentType = "application/json";
		const string XmlContentType = "application/xml";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Body string")]
		const string Body = "Body";
		const string UniversalShipment = "UniversalShipment";
		const string UniversalEvent = "UniversalEvent";

		public async Task<T> FromHttpContentAsync<T>(HttpContent content, CancellationToken cancellationToken = default)
		{
			if (typeof(T) == typeof(EmissionResult))
			{
				var emissionResult = new EmissionResult();
				var mediaType = content.Headers.ContentType?.MediaType;
				var data = await content.ReadAsStringAsync().ConfigureAwait(false);
				if (mediaType == XmlContentType)
				{
					emissionResult.InterchangeString = data;
					var (ushipment, shipmentString) = DeserializeUShipment(data);
					if (!string.IsNullOrEmpty(shipmentString))
					{
						emissionResult.UXml = ushipment;
						emissionResult.UXmlString = shipmentString;
					}
					else
					{
						var (uevent, eventString) = DeserializeUEvent(data);
						emissionResult.UXml = uevent;
						emissionResult.UXmlString = eventString;
					}
				}
				else if (mediaType == JsonContentType)
				{
					emissionResult.JsonContent = DeserializeJson(data);
				}

				return (T)(object)emissionResult;
			}

			throw new NotSupportedException($"Deserialization of type '{typeof(T).Name}' is not supported.");
		}

		public (Shipment, string) DeserializeUShipment(string data)
		{
			XDocument doc = null;
			try
			{
				doc = XDocument.Parse(data);
			}
			catch (XmlException)
			{
				return default;
			}

			var body = doc.Root?.Elements().FirstOrDefault(elem => elem.Name.LocalName == Body);

			if (body == null || body.IsEmpty)
			{
				return default;
			}

			var universalShipment = body.Elements().FirstOrDefault(elem => elem.Name.LocalName == UniversalShipment);

			if (universalShipment == null)
			{
				return default;
			}

			var shipmentString = universalShipment.ToString();
			return (new StringReader(shipmentString).Parse<Shipment>(), shipmentString);
		}

		public (Event, string) DeserializeUEvent(string data)
		{
			XDocument doc = null;
			try
			{
				doc = XDocument.Parse(data);
			}
			catch (XmlException)
			{
				return default;
			}

			var body = doc.Root?.Elements().FirstOrDefault(elem => elem.Name.LocalName == Body);

			if (body == null || body.IsEmpty)
			{
				return default;
			}

			var universalEvent = body.Elements().FirstOrDefault(elem => elem.Name.LocalName == UniversalEvent);

			if (universalEvent == null)
			{
				return default;
			}

			var eventString = universalEvent.ToString();
			return (new StringReader(eventString).Parse<Event>(), eventString);
		}

		EmissionResult.JsonResult DeserializeJson(string data)
		{
			return JsonConvert.DeserializeObject<EmissionResult.JsonResult>(data);
		}

		public async Task<Exception> SerializerErrorHandler(Exception e, HttpResponseMessage message)
		{
			var data = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
			var errorMessage = ResString.GetMultilingualString("229b9358-d567-4024-ab0d-a80fdf78a555", @"{0}
Data: {1}", e.Message, data);
			return new System.Runtime.Serialization.SerializationException(errorMessage);
		}
	}
}
