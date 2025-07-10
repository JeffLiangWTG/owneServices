using System;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class WhsOrderExportToFileMenuItemTestIFS : WhsDocketExportToFileMenuItemTest<WhsOrder, Xsd.ConNote>
	{
		#region Overrides

		protected override WhsOrder GetNewDocket()
		{
			var order = Helper.CreateWhsOrder(Client, Warehouse);
			order.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals("Order is not finalized", true, order.IsFinalised);
			return order;
		}

		protected override WhsValueObjectDataAdapter<WhsOrder, Xsd.ConNote> GetNewAdapter()
		{
			return new WhsOrderCartageValueObjectDataAdapterIFS();
		}

		protected override System.Windows.Forms.MenuItem GetNewMenuItem()
		{
			return new WhsOrderExportToFileMenuItemIFS(Docket, (WhsOrderCartageValueObjectDataAdapterIFS)Adapter);
		}

		protected override Type GetExpectedExportDirectorType()
		{
			return typeof(WhsOrderCartageXmlExportToFileDirectorIFS);
		}

		#endregion
	}
}
