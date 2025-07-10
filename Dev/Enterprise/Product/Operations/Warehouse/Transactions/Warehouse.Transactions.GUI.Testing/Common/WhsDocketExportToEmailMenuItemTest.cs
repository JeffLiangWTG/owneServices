using Enterprise.DataTransfer.Xml;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public abstract class WhsDocketExportToEmailMenuItemTest<TDocket, TValueObject> : WhsDocketExportMenuItemTest<TDocket, TValueObject>
			where TDocket : WhsDocket
			where TValueObject : IValueObject
	{
		#region TestMenuText

		public void TestMenuText()
		{
			AssertEquals("Send Email", MenuItem.Text);
		}

		#endregion

	}
}
