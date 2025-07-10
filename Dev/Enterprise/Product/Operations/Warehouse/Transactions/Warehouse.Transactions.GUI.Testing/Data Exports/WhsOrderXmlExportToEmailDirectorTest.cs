using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

public class WhsOrderXmlExportToEmailDirectorTest : WhsDocketXmlExportToEmailDirectorTest<WhsOrder>
{
	#region Overrides

	protected override WhsOrder GetNewDocket() => Helper.CreateWhsOrder(Client, Warehouse);

	protected override WhsXmlExportDirector<WhsOrder, Xsd.WhsDocket> GetNewExportDirectorObject() => new WhsOrderXmlExportToEmailDirector((WhsOrderValueObjectDataAdapter)GetNewAdapter());

	protected override WhsValueObjectDataAdapter<WhsOrder, Xsd.WhsDocket> GetNewAdapter() => new WhsOrderValueObjectDataAdapter();

	protected override CargoWise.Types.ZString GetExpectedDocketTypeDescription() => "Order";

	protected override CargoWise.Types.ZString GetCommunicationsModeModule() => JobInvoicingConsumerTypes.WarehouseOutwards.Code;

	#endregion
}
