using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Helpers
{
	public static class XmlHelper
	{
		public static string SerializeIDataObject(IDataObject dataObject)
		{
			using (var stream = new SubStreamableStream(EmptyDisposableLeakListener.Instance))
			using (var reader = new StreamReader(stream))
			{
				ObjectFactory.New<IXmlWriter>().WriteXML(dataObject, stream);
				stream.Flush();
				stream.Position = 0;
				return reader.ReadToEnd();
			}
		}

		public static string Serialize<T>(T target)
		{
			var serializer = new XmlSerializer(typeof(T));
			using var ms = new MemoryStream();
			using var writer = CreateFormattedXmlWriter(ms);
			serializer.Serialize(writer, target);

			return Encoding.UTF8.GetString(ms.ToArray());
		}

		public static XmlWriter CreateFormattedXmlWriter(Stream buffer)
		{
			var settings = new XmlWriterSettings
			{
				Indent = true,
				Encoding = new UTF8Encoding(false)
			};

			return System.Xml.XmlWriter.Create(buffer, settings);
		}

		public static (Shipment, string) DeserializeUniversalShipment(string xml)
		{
			var xmlResponse = XDocument.Parse(xml);
			var universalResponse = xmlResponse.Document?.Elements().FirstOrDefault(e => e.Name.LocalName == "UniversalResponse") ?? throw new ArgumentException("Cannot deserialize given XML");
			var status = xmlResponse.Descendants().FirstOrDefault(e => e.Name.LocalName == "Status")?.Value ?? throw new ArgumentException("Cannot deserialize given XML");
			var data = universalResponse.Descendants().FirstOrDefault(ur => ur.Name.LocalName == "Data")?.FirstNode?.ToString() ?? "";
			using var stream = (SubStreamableStream)new MemoryStream(Encoding.ASCII.GetBytes(data));
			return (stream.Parse<Shipment>(), status);
		}
	}
}
