using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

public class WhsReceiveXmlExporterTest : WhsDocketXmlExporterTest<WhsReceive>
{
	#region Overrides

	protected override WhsXmlExporter<WhsReceive, Xsd.WhsDocket> GetNewExporter() => new WhsReceiveXmlExporter(new WhsReceiveValueObjectDataAdapter());

	protected override WhsReceive GetNewDocket() => Helper.CreateWhsReceive(Client, Warehouse);

	#endregion
}
