using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(WhsInventoryHeldCodeEntryForm))]
	public class WhsInventoryHeldCodeEntryFormBasherTest : ZFormBasherTest
	{
		#region TestShowPreDeleteDialogs

		public void TestShowPreDeleteDialogs()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var code = helper.CreateInventoryHeldCode("AAA", "AAA");

			using (var form = new WhsInventoryHeldCodeEntryForm_ForTest(code))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var result = form.ShowPreDeleteDialogs_Exposed();
				AssertEquals("You are about to delete this record permanently from the system. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZForm.ContinueWithDelete.Yes, result);
			}

			var data = new TestDataSimpleEnvironment(Factory);
			var receivePK = helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "1", ZDateTimeOffset.Today, helper.Notify);
			helper.CreateWhsReceiveInventoryLine(receivePK, data.Part1.PK, 10m, "A-1-1", "ARV", code.WHC_Code);
			Factory.Save();

			using (var form = new WhsInventoryHeldCodeEntryForm_ForTest(code))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var result1 = form.ShowPreDeleteDialogs_Exposed();
				AssertEquals("This Hold Code is in use. Are you sure you want to delete it?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZForm.ContinueWithDelete.No, result1);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var result2 = form.ShowPreDeleteDialogs_Exposed();
				AssertEquals("This Hold Code is in use. Are you sure you want to delete it?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZForm.ContinueWithDelete.Yes, result2);
			}
		}

		#endregion

		#region Implementation

		class WhsInventoryHeldCodeEntryForm_ForTest : WhsInventoryHeldCodeEntryForm
		{
			public WhsInventoryHeldCodeEntryForm_ForTest(WhsInventoryHeldCode heldCode)
				: base(heldCode)
			{
			}

			public ContinueWithDelete ShowPreDeleteDialogs_Exposed()
			{
				return ShowPreDeleteDialogs();
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new WhsInventoryHeldCodeEntryForm(Factory.New<WhsInventoryHeldCode>());
		}

		#endregion
	}
}
