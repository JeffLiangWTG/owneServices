using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

public class WhsAdjustmentXmlExporterTest : WhsDocketXmlExporterTest<WhsAdjustment>
{
	#region Overrides

	protected override WhsXmlExporter<WhsAdjustment, Xsd.WhsDocket> GetNewExporter()
		=> new WhsAdjustmentXmlExporter(new WhsAdjustmentValueObjectDataAdapter());

	protected override WhsAdjustment GetNewDocket() => Helper.CreateWhsAdjustment(Client, Warehouse);

	#endregion
}
