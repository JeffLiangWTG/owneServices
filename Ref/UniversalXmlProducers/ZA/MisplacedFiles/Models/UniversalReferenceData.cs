using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ZAReferenceData.Models
{
	public class UniversalReferenceData : IXmlSerializable
	{
		public string DataSource { get; set; }

		public DateTime PublicationTime { get; set; }

		public string UpdateType { get; set; }

		public string Schema { get; set; }

		public List<RefCusCodeList> RefCusCodeLists { get; } = new List<RefCusCodeList>();

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

			writer.WriteStartElement(nameof(Schema));
			writer.WriteRaw(Schema);
			writer.WriteEndElement();

			var inner = new XmlSerializer(typeof(RefCusCodeList));
			var ns = new XmlSerializerNamespaces();
			ns.Add("", "");
			RefCusCodeLists.ForEach(x => inner.Serialize(writer, x, ns));
		}
	}
}
