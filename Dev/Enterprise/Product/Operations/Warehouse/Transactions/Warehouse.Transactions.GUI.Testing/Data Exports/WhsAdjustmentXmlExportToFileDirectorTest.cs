using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class WhsAdjustmentXmlExportToFileDirectorTest : WhsDocketXmlExportToFileDirectorTest<WhsAdjustment>
	{
		#region Implementation

		protected override WhsAdjustment GetNewDocket() => Helper.CreateWhsAdjustment(Client, Warehouse);

		protected override WhsXmlExportDirector<WhsAdjustment, Xsd.WhsDocket> GetNewExportDirectorObject()
			=> new WhsAdjustmentXmlExportToFileDirector((WhsAdjustmentValueObjectDataAdapter)GetNewAdapter());

		protected override WhsValueObjectDataAdapter<WhsAdjustment, Xsd.WhsDocket> GetNewAdapter() => new WhsAdjustmentValueObjectDataAdapter();

		protected override CargoWise.Types.ZString GetExpectedDocketTypeDescription() => "Adjustment";

		#endregion
	}
}
