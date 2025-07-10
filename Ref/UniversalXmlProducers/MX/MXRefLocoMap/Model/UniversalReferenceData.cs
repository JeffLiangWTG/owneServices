using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.MXRefLocoMap.Business
{
	public class UniversalReferenceData : IXmlSerializable
	{
		public string DataSource { get; set; }

		public DateTime PublicationTime { get; set; }

		public string UpdateType { get; set; }

		public string Schemas { get; set; }

		public List<RefLocoMap> RefCusCodeLists { get; } = new List<RefLocoMap>();

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			throw new NotImplementedException();
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement(nameof(DataSource));
			writer.WriteRaw(DataSource);
			writer.WriteEndElement();

			writer.WriteStartElement(nameof(PublicationTime));
			writer.WriteRaw(PublicationTime.ToString("s"));
			writer.WriteEndElement();

			writer.WriteStartElement(nameof(UpdateType));
			writer.WriteRaw(UpdateType);
			writer.WriteEndElement();

			writer.WriteStartElement(nameof(Schemas));
			writer.WriteRaw(Schemas);
			writer.WriteEndElement();

			var inner = new XmlSerializer(typeof(RefLocoMap));
			var ns = new XmlSerializerNamespaces();
			ns.Add("", "");
			RefCusCodeLists.ForEach(x => inner.Serialize(writer, x, ns));
		}
	}
}
