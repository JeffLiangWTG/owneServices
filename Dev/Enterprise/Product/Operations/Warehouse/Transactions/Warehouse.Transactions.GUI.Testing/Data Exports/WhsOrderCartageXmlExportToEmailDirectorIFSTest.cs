using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

public class WhsOrderCartageXmlExportToEmailDirectorIFSTest : WhsXmlExportToEmailDirectorTest<WhsOrder, Xsd.ConNote>
{
	#region Implementation

	protected override WhsOrder GetNewDocket() => Helper.CreateWhsOrder(Client, Warehouse);

	protected override WhsXmlExportDirector<WhsOrder, Xsd.ConNote> GetNewExportDirectorObject()
		=> new WhsOrderCartageXmlExportToEmailDirectorIFS((WhsOrderCartageValueObjectDataAdapterIFS)GetNewAdapter());

	protected override WhsValueObjectDataAdapter<WhsOrder, Xsd.ConNote> GetNewAdapter() => new WhsOrderCartageValueObjectDataAdapterIFS();

	protected override ZString GetExpectedDocketTypeDescription() => "Order";

	protected override ZString GetCommunicationsModeModule() => JobInvoicingConsumerTypes.WarehouseOutwards.Code;

	protected override string ExpectedEDICommunicationsModeFileFormat => EDICommunicationsModeFileFormatList.Codes.IFS;

	protected override ZString GetExpectedSuccessNotification()
		=> "Email with " + GetExpectedDocketTypeDescription() + " IFS XML attachment successfully sent.";

	#endregion
}
