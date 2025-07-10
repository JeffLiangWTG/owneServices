using System.Windows.Forms;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	[TestedType(typeof(WhsCartonSizeEntryForm))]
	public class WhsCartonSizeEntryFormFormBasherTest : ZFormBasherTest
	{
		#region TestShowPreDeleteDialogs

		public void TestShowPreDeleteDialogs()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var size1 = helper.CreateWhsCartonSize("S1");
			var size2 = helper.CreateWhsCartonSize("S2");

			var group = helper.CreateWhsCartonGroup("G1", "Group");
			group.CartonSizes.Add(size2);
			Factory.Save();

			using (var form = new WhsCartonSizeEntryForm_ForTest(size1))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var result = form.ShowPreDeleteDialogs_Exposed();
				AssertEquals("You are about to delete this record permanently from the system. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZForm.ContinueWithDelete.Yes, result);
			}

			using (var form = new WhsCartonSizeEntryForm_ForTest(size2))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var result1 = form.ShowPreDeleteDialogs_Exposed();
				AssertEquals("This Carton Size is attached to one or more Carton Groups. Are you sure you want to delete it?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZForm.ContinueWithDelete.No, result1);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var result2 = form.ShowPreDeleteDialogs_Exposed();
				AssertEquals("This Carton Size is attached to one or more Carton Groups. Are you sure you want to delete it?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZForm.ContinueWithDelete.Yes, result2);
			}
		}

		#endregion

		#region Implementation

		class WhsCartonSizeEntryForm_ForTest : WhsCartonSizeEntryForm
		{
			public WhsCartonSizeEntryForm_ForTest(WhsCartonSize cartonSize)
				: base(cartonSize)
			{
			}

			public ContinueWithDelete ShowPreDeleteDialogs_Exposed()
			{
				return ShowPreDeleteDialogs();
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new WhsCartonSizeEntryForm(Factory.New<WhsCartonSize>());
		}

		#endregion
	}
}
