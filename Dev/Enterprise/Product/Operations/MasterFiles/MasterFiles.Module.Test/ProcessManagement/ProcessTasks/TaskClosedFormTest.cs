using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(TaskClosedForm))]
	sealed class TaskClosedFormTest : ZFormBasherTest
	{
		public void TestButtons_Yes()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_Type = task.Lookups.Types[0].Code;
			task.P9_Description = "Hello";

			using (TaskClosedForm form = new TaskClosedForm(task))
			{
				form.Show();

				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals(true, form.Visible);

				form.OKButton.PerformClick();
				AssertEquals(DialogResult.Yes, form.DialogResult);
				AssertEquals(false, form.Visible);
			}
		}

		public void TestButtons_Yes_Errors()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			task.AddRowError("Some crap");
			using (TaskClosedForm form = new TaskClosedForm(task))
			{
				form.Show();

				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals(true, form.Visible);

				form.OKButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals(true, form.Visible);
			}
		}

		public void TestButtons_No()
		{
			using (TaskClosedForm form = new TaskClosedForm(Factory.New<ProcessTask>()))
			{
				form.Show();

				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals(true, form.Visible);

				form.NoButton.PerformClick();
				AssertEquals(DialogResult.No, form.DialogResult);
				AssertEquals(false, form.Visible);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new TaskClosedForm(Factory.New<ProcessTask>());
		}

		#endregion
	}
}
