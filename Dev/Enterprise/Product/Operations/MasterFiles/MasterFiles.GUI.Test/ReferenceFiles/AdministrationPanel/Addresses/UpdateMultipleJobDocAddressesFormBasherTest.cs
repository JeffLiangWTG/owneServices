using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(UpdateMultipleJobDocAddressesForm))]
	sealed class UpdateMultipleJobDocAddressesFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new UpdateMultipleJobDocAddressesForm(Res.GetData("b7611135-e150-43cb-977a-7dad57684253", "This is title.").ToString());
		}

		public void TestUpdateChoice()
		{
			using (var form = new UpdateMultipleJobDocAddressesFormForTest(string.Empty, false))
			{
				form.Show();
				form.UpdateAllRecordsButtonForTest.PerformClick();
				AssertEquals(UpdateChoice.AllRecords, form.Choice);
			}

			using (var form = new UpdateMultipleJobDocAddressesFormForTest(string.Empty, false))
			{
				form.Show();
				form.UpdateThisRecordButtonForTest.PerformClick();
				AssertEquals(UpdateChoice.ThisRecord, form.Choice);
			}

			using (var form = new UpdateMultipleJobDocAddressesFormForTest(string.Empty))
			{
				form.Show();
				form.CancelSelectButtonTest.PerformClick();
				AssertEquals(UpdateChoice.Cancel, form.Choice);
			}

			using (var form = new UpdateMultipleJobDocAddressesFormForTest(string.Empty))
			{
				form.Show();
				form.YesButtonTest.PerformClick();
				AssertEquals(UpdateChoice.Yes, form.Choice);
			}

			using (var form = new UpdateMultipleJobDocAddressesFormForTest(string.Empty))
			{
				form.Show();
				form.Close();
				AssertEquals(UpdateChoice.Cancel, form.Choice);
			}
		}

		public void TestDefaultButtons()
		{
			using (var form = new UpdateMultipleJobDocAddressesFormForTest(string.Empty))
			{
				form.Show();
				Assert(form.YesButtonTest.Visible);
				Assert(form.CancelSelectButtonTest.Visible);
				Assert(!form.UpdateThisRecordButtonForTest.Visible);
				Assert(!form.UpdateAllRecordsButtonForTest.Visible);
			}

			using (var form = new UpdateMultipleJobDocAddressesFormForTest(string.Empty, false))
			{
				form.Show();
				Assert(!form.YesButtonTest.Visible);
				Assert(form.CancelSelectButtonTest.Visible);
				Assert(form.UpdateThisRecordButtonForTest.Visible);
				Assert(form.UpdateAllRecordsButtonForTest.Visible);
			}
		}
	}
}
