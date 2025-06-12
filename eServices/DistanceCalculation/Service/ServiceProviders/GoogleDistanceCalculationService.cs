using System;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.Freight.DistanceCalculation.Service.Extensions;

namespace Enterprise.Freight.DistanceCalculation.Service
{
	public class GoogleDistanceCalculationService
	{
		public DistanceCalculationResult Process(DistanceCalculationAddress OriginAddress, DistanceCalculationAddress DestinationAddress)
		{
			var url = GetSignedUrl(OriginAddress, DestinationAddress);
			using (var dataProvider = new GoogleDataProvider(url))
			{
				using (var dataStream = dataProvider.GetData())
				{
					return ParseData(dataStream);
				}
			}
		}

		protected string GetSignedUrl(DistanceCalculationAddress OriginAddress, DistanceCalculationAddress DestinationAddress)
		{
			var clientId = ConfigurationManager.AppSettings["GoogleDistanceAPI_ClientID"];
			var key = ConfigurationManager.AppSettings["GoogleDistanceAPI_Key"];
			var url = $"http://maps.googleapis.com/maps/api/distancematrix/json?origins={Uri.EscapeDataString(OriginAddress.ToString())}&destinations={Uri.EscapeDataString(DestinationAddress.ToString())}&client={clientId}&mode=driving&language=en-EN&sensor=false";

			// Code for URL signing
			// From: https://developers.google.com/maps/documentation/distance-matrix/get-api-key
			var encoding = new ASCIIEncoding();

			// converting key to bytes will throw an exception, need to replace '-' and '_' characters first.
			var usablePrivateKey = key.Replace("-", "+").Replace("_", "/");
			var privateKeyBytes = Convert.FromBase64String(usablePrivateKey);

			var uri = new Uri(url);
			var encodedPathAndQueryBytes = encoding.GetBytes(uri.LocalPath + uri.Query);

			// compute the hash
			var algorithm = new HMACSHA1(privateKeyBytes);
			var hash = algorithm.ComputeHash(encodedPathAndQueryBytes);

			// convert the bytes to string and make url-safe by replacing '+' and '/' characters
			var signature = Convert.ToBase64String(hash).Replace("+", "-").Replace("/", "_");

			// Add the signature to the existing URI.
			return uri.Scheme + "://" + uri.Host + uri.LocalPath + uri.Query + "&signature=" + signature;
		}

		public static DistanceCalculationResult ParseData(Stream dataStream)
		{
			DistanceCalculationResult result = new DistanceCalculationResult();

			try
			{

				var reader = JsonReaderWriterFactory.CreateJsonReader(dataStream, new XmlDictionaryReaderQuotas());
				var root = XElement.Load(reader);
				var status = root.XPathSelectElement("//status").Value;
				if (status == "OK")
				{
					var distanceElement = root.XPathSelectElement("//rows/item/elements/item/distance/value");
					if (distanceElement == null) throw new Exception("Distance element is missing");
					result.Distance = double.Parse(distanceElement.Value) / 1000;

					result.DistanceUnit = DistanceCalculationConstants.UnitsForCalculation.Kilometres;

					var durationElement = root.XPathSelectElement("//rows/item/elements/item/duration/value");
					if (durationElement == null) throw new Exception("Duration element is missing");

					result.TravelTime = new TimeSpan(0, 0, int.Parse(durationElement.Value)).TotalHours;
				}
				else
				{
					result.StatusMessage = typeof(StatusDescription).GetDescription(status);
				}
			}
			catch (Exception ex)
			{
				result.StatusMessage = ex.Message;
			}

			return result;
		}
	}
}
