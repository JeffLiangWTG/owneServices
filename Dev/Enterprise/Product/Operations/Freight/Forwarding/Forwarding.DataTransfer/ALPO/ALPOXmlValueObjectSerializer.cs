using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1.ALPO;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ALPOXmlValueObjectSerializer : XmlValueObjectSerializer
	{
		public ALPOXmlValueObjectSerializer(Type valueObjectType)
			: base(valueObjectType)
		{
		}

		public override void ExportXmlData(Stream writer, IValueObjectDataAdapter dataAdapter, IList businessObjectsToExport, IValueObjectExportContext context, string senderID, string receiverID, string purpose)
		{
			XmlTextWriter sW = new XmlTextWriter(writer, Encoding.UTF8);
			IValueObject objectToExport = dataAdapter.ExportToValueObject((ForwardingShipment)businessObjectsToExport[0], context);
			ZXmlSerializer serialiser = ZXmlSerializer.New(typeof(Xsd.AdvantageEnterpriseVersion01));
			XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
			ns.Add((NoResString)"xsi", "http://www.w3.org/2001/XMLSchema-instance"); // Hard-coded constant
			ns.Add("noNamespaceSchemaLocation", "ALPO-AUFTRAG_V1_03.xsd");

			serialiser.Serialize(sW, objectToExport, ns);
		}
	}
}
