using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI
{
	[TestedType(typeof(WorkflowEventForm))]
	sealed class WorkflowEventFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestFormCaption()
		{
			using (var form = new WorkflowEventForm(StmEvent))
			{
				AssertEquals(Res.GetData("e43f898a-ba12-11e3-a1b8-1c6f653fb9f5", "Workflow Event"), form.CaptionResourceString);
			}
		}

		public void TestAllowNew()
		{
			using (var form = new WorkflowEventForm(StmEvent))
			{
				form.Show();
				Assert("Should not be able to create new event", !form.AllowNew);
			}
		}

		public void TestSupportsEDocs()
		{
			using (var form = new WorkflowEventForm(StmEvent))
			{
				form.Show();
				Assert("EDocs should not be visible", !form.SupportsEDocs);
			}
		}

		public void TestMixedCaseForReferenceColumns()
		{
			StmEvent.SE_IsRefernceFormatOverridden = true;
			StmEvent.SE_OverriddenReferenceFormat = "<REF>ref";
			StmEvent.SE_ReferenceFormat = "<REF>ref";
			using (var form = new WorkflowEventForm(StmEvent))
			{
				form.Show();

				var referenceFormatTextBox = form.Controls.Find("referenceFormatTextBox", true)[0];
				AssertEquals("<REF>ref", referenceFormatTextBox.Text);

				var referenceFormatOverriddenTextBox = form.Controls.Find("referenceFormatOverriddenTextBox", true)[0];
				AssertEquals("<REF>ref", referenceFormatOverriddenTextBox.Text);
			}
		}

		#region Implementation

		StmEvent StmEvent
		{
			get { return stmEvent ?? (stmEvent = Factory.New<StmEvent>()); }
		}

		StmEvent stmEvent;

		protected override Form GetFormToBashCore()
		{
			return new WorkflowEventForm(Factory.New<StmEvent>());
		}

		#endregion
	}
}
