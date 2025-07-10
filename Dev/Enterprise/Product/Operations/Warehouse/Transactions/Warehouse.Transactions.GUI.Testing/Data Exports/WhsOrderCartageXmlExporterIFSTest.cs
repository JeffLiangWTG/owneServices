using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

public class WhsOrderCartageXmlExporterIFSTest : WhsXmlExporterTest<WhsOrder, Xsd.ConNote>
{
	#region Overrides

	protected override WhsXmlExporter<WhsOrder, Xsd.ConNote> GetNewExporter()
		=> new WhsOrderCartageXmlExporterIFS(new WhsOrderCartageValueObjectDataAdapterIFS());

	protected override WhsOrder GetNewDocket() => Helper.CreateWhsOrder(Client, Warehouse);

	#endregion
}
