using System.IO;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsOrderCartageXmlExporterIFS : WhsXmlExporter<WhsOrder, Xsd.ConNote>
	{
		public WhsOrderCartageXmlExporterIFS(WhsOrderCartageValueObjectDataAdapterIFS adapter)
			: base(adapter)
		{
		}

		#region Overrides

		protected override void ExportToXml(WhsOrder order, TextWriter writer, NotificationBuffer notify)
		{
			// this is effectively tested by WhsOrderCartageValueObjectAdapterIFSTest
			XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(typeof(Xsd.ConNoteObject));

			var conNote = Adapter.ExportToValueObject(order, new ValueObjectExportContext(notify));

			var conNoteObject = new Xsd.ConNoteObject();
			conNoteObject.ConNote.Add(conNote);

			serializer.Serialize(writer, conNoteObject);
		}

		#endregion
	}
}
