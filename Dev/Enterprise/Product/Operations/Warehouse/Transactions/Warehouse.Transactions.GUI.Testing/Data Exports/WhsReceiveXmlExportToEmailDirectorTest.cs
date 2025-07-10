using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

public class WhsReceiveXmlExportToEmailDirectorTest : WhsDocketXmlExportToEmailDirectorTest<WhsReceive>
{
	#region Overrides

	protected override WhsReceive GetNewDocket() => Helper.CreateWhsReceive(Client, Warehouse);

	protected override WhsXmlExportDirector<WhsReceive, Xsd.WhsDocket> GetNewExportDirectorObject()
		=> new WhsReceiveXmlExportToEmailDirector((WhsReceiveValueObjectDataAdapter)GetNewAdapter());

	protected override WhsValueObjectDataAdapter<WhsReceive, Xsd.WhsDocket> GetNewAdapter() => new WhsReceiveValueObjectDataAdapter();

	protected override CargoWise.Types.ZString GetExpectedDocketTypeDescription() => "Receipt";

	protected override CargoWise.Types.ZString GetCommunicationsModeModule() => JobInvoicingConsumerTypes.WarehouseInwards.Code;

	#endregion
}
