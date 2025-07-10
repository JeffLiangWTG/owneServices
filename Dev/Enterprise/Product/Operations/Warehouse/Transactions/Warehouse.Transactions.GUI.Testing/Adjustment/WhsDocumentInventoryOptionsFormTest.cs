using System.Windows.Forms;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(WhsDocumentInventoryOptionsForm))]
	public class WhsDocumentInventoryOptionsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var receive = Factory.New<WhsReceive>();
			receive.Inventory.AddNew();
			receive.Inventory.AddNew();
			receive.Inventory.AddNew();

			var options = new WhsDocumentInventoryOptions(receive.Inventory);
			return new WhsDocumentInventoryOptionsForm(options);
		}
	}
}
