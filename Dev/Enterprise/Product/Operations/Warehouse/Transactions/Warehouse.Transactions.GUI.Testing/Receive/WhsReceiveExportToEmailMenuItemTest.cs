using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class WhsReceiveExportToEmailMenuItemTest : WhsDocketExportToEmailMenuItemTest<WhsReceive, Xsd.WhsDocket>
	{
		protected override WhsReceive GetNewDocket()
		{
			var receive = Helper.CreateWhsReceive(Client, Warehouse);
			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals("Receive is not finalized", true, receive.IsFinalised);
			return receive;
		}

		protected override WhsValueObjectDataAdapter<WhsReceive, Xsd.WhsDocket> GetNewAdapter()
		{
			return new WhsReceiveValueObjectDataAdapter();
		}

		protected override MenuItem GetNewMenuItem()
		{
			return new WhsReceiveExportToEmailMenuItem(Docket, (WhsReceiveValueObjectDataAdapter)Adapter);
		}

		protected override Type GetExpectedExportDirectorType()
		{
			return typeof(WhsReceiveXmlExportToEmailDirector);
		}
	}
}
