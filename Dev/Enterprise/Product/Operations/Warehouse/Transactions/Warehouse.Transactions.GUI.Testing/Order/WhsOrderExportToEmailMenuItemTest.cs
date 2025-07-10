using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class WhsOrderExportToEmailMenuItemTest : WhsDocketExportToEmailMenuItemTest<WhsOrder, Xsd.WhsDocket>
	{
		#region Overrides

		protected override WhsOrder GetNewDocket()
		{
			var order = Helper.CreateWhsOrder(Client, Warehouse);
			order.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals("Order should be finalized", true, order.IsFinalised);
			return order;
		}

		protected override WhsValueObjectDataAdapter<WhsOrder, Xsd.WhsDocket> GetNewAdapter()
		{
			return new WhsOrderValueObjectDataAdapter();
		}

		protected override MenuItem GetNewMenuItem()
		{
			return new WhsOrderExportToEmailMenuItem(Docket, (WhsOrderValueObjectDataAdapter)Adapter);
		}

		protected override Type GetExpectedExportDirectorType()
		{
			return typeof(WhsOrderXmlExportToEmailDirector);
		}

		#endregion
	}
}
