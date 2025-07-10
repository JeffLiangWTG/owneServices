using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class WhsOrderExportToEmailMenuItemIFSTest : WhsDocketExportToEmailMenuItemTest<WhsOrder, Xsd.ConNote>
	{
		#region Implementation

		protected override WhsOrder GetNewDocket()
		{
			var order = Helper.CreateWhsOrder(Client, Warehouse);
			order.WD_FinalisedDate = ZDateTimeOffset.Now;
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order);

			return order;
		}

		protected override WhsValueObjectDataAdapter<WhsOrder, Xsd.ConNote> GetNewAdapter()
		{
			return new WhsOrderCartageValueObjectDataAdapterIFS();
		}

		protected override MenuItem GetNewMenuItem()
		{
			return new WhsOrderExportToEmailMenuItemIFS(Docket, (WhsOrderCartageValueObjectDataAdapterIFS)Adapter);
		}

		protected override Type GetExpectedExportDirectorType()
		{
			return typeof(WhsOrderCartageXmlExportToEmailDirectorIFS);
		}

		#endregion
	}
}
