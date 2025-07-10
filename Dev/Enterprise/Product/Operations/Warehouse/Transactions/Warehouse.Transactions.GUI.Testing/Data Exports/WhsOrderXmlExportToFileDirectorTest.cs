using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class WhsOrderXmlExportToFileDirectorTest : WhsDocketXmlExportToFileDirectorTest<WhsOrder>
	{
		#region Overrides

		protected override WhsOrder GetNewDocket() => Helper.CreateWhsOrder(Client, Warehouse);

		protected override WhsXmlExportDirector<WhsOrder, Xsd.WhsDocket> GetNewExportDirectorObject() => new WhsOrderXmlExportToFileDirector((WhsOrderValueObjectDataAdapter)GetNewAdapter());

		protected override WhsValueObjectDataAdapter<WhsOrder, Xsd.WhsDocket> GetNewAdapter() => new WhsOrderValueObjectDataAdapter();

		protected override ZString GetExpectedDocketTypeDescription() => "Order";

		#endregion
	}
}
