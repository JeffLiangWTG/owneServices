using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccComplianceSequenceBulkForm))]
	public class AccComplianceSequenceBulkFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var creator = new AccComplianceSequenceBulkCreator(Factory);
			var form = new AccComplianceSequenceBulkForm(creator);

			return form;
		}

		protected override bool AllowHasChangesOnFormOpen => true;
		protected override bool AllowSaveOnFormForTestHasChanges => false;

		public void TestFormCaptionAndVerb()
		{
			using (var form = (AccComplianceSequenceBulkForm)GetFormToBash())
			{
				AssertEquals("Compliance Sequence Bulk Create", form.CaptionResourceString.Caption);
				AssertEquals(string.Empty, form.FormVerb);
			}
		}

		public void TestSaveButtonAvailability()
		{
			using (var form = (AccComplianceSequenceBulkForm)GetFormToBash())
			{
				var control = typeof(AccComplianceSequenceBulkForm).GetField("PostingButtonsUserControl", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(form) as Core.Forms.ZPostingButtonsUserControl;
				AssertNull("Should only allow 'Save & Close'.", (form as IPostingButtonsProvider).CommandButtonApply);
			}
		}
	}
}
