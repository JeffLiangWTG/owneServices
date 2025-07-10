using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccComplianceSequenceSplitForm))]
	sealed class AccComplianceSequenceSplitFormTest : ZFormBasherTest
	{
		protected override bool AllowHasChangesOnFormOpen => true;
		protected override bool AllowSaveOnFormForTestHasChanges => false;

		protected override Form GetFormToBashCore()
		{
			var sequence = Factory.New<AccComplianceSequence>();
			var model = new AccComplianceSequenceSplitViewModel(Factory, sequence.PK);
			var form = new AccComplianceSequenceSplitForm(model);
			return form;
		}

		public void TestFormCaptionAndVerb()
		{
			using (var form = (AccComplianceSequenceSplitForm)GetFormToBash())
			{
				AssertEquals("Split Compliance Sequence Book", form.CaptionResourceString.Caption);
				AssertEquals(string.Empty, form.FormVerb);
			}
		}

		public void TestSaveButtonAvailability()
		{
			using (var form = (AccComplianceSequenceSplitForm)GetFormToBash())
			{
				var control = typeof(AccComplianceSequenceSplitForm).GetField("PostingButtonsUserControl", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(form) as Core.Forms.ZPostingButtonsUserControl;
				AssertNull("Should only allow 'Save & Close'. Otherwise client may keep re-split on the same pair of exist / new books.", (form as IPostingButtonsProvider).CommandButtonApply);
			}
		}

		public void TestSaveAsksPrintingConfirmtation()
		{
			var model = new AccComplianceSequenceSplitViewModel(Factory, ZGuid.Empty);

			using (var testForm = new AccComplianceSequenceSplitForm(model))
			{
				testForm.Show();
				testForm.FireSaveButton();
				AssertEquals("Compliance Sequence book split successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
