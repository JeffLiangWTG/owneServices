using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class WhsOrderCartageXmlExportToFileDirectorIFSTest : WhsXmlExportToFileDirectorTest<WhsOrder, Xsd.ConNote>
	{
		#region TestXMLExporterType

		public void TestXMLExporterType() => AssertEquals(typeof(WhsOrderCartageXmlExporterIFS), ExportDirector.XmlExporter.GetType());

		#endregion

		#region Implementation

		protected override WhsOrder GetNewDocket() => Helper.CreateWhsOrder(Client, Warehouse);

		protected override WhsXmlExportDirector<WhsOrder, Xsd.ConNote> GetNewExportDirectorObject()
			=> new WhsOrderCartageXmlExportToFileDirectorIFS((WhsOrderCartageValueObjectDataAdapterIFS)GetNewAdapter());

		protected override WhsValueObjectDataAdapter<WhsOrder, Xsd.ConNote> GetNewAdapter() => new WhsOrderCartageValueObjectDataAdapterIFS();

		protected override ZString GetExpectedDocketTypeDescription() => "Order";

		protected override ZString GetExpectedSuccessNotification() => GetExpectedDocketTypeDescription() + " successfully exported to IFS XML.";

		#endregion
	}
}
