using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

public class WhsOrderXmlExporterTest : WhsDocketXmlExporterTest<WhsOrder>
{
	#region Overrides

	protected override WhsXmlExporter<WhsOrder, Xsd.WhsDocket> GetNewExporter() => new WhsOrderXmlExporter(new WhsOrderValueObjectDataAdapter());

	protected override WhsOrder GetNewDocket() => Helper.CreateWhsOrder(Client, Warehouse);

	#endregion
}
