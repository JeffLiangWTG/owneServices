using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

public class WhsAdjustmentXmlExportToEmailDirectorTest : WhsDocketXmlExportToEmailDirectorTest<WhsAdjustment>
{
	#region Overrides

	protected override WhsAdjustment GetNewDocket() => Helper.CreateWhsAdjustment(Client, Warehouse);

	protected override WhsXmlExportDirector<WhsAdjustment, Xsd.WhsDocket> GetNewExportDirectorObject()
		=> new WhsAdjustmentXmlExportToEmailDirector((WhsAdjustmentValueObjectDataAdapter)GetNewAdapter());

	protected override WhsValueObjectDataAdapter<WhsAdjustment, Xsd.WhsDocket> GetNewAdapter() => new WhsAdjustmentValueObjectDataAdapter();

	protected override CargoWise.Types.ZString GetExpectedDocketTypeDescription() => "Adjustment";

	protected override CargoWise.Types.ZString GetCommunicationsModeModule() => WorkflowDescriptors.WhsAdjustmentWorkflowDescriptorCode;

	#endregion
}
