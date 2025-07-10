using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ExternalValidationResultForm))]
	sealed class ExternalValidationResultFormTest : ZFormBasherTest
	{
		OrgHeader testOrganization;

		public OrgHeader TestOrganization
		{
			get
			{
				if (testOrganization == null)
				{
					testOrganization = Factory.NewWithValidTestData<OrgHeader>();
					Factory.Save();
				}
				Assert(!testOrganization.HasChanges);
				return testOrganization;
			}
		}
		public void TestMessagesAreShown()
		{
			using (var testForm = (ExternalValidationResultForm)GetFormToBash())
			{
				testForm.Show();
				var entries = testForm.resultGrid.ListManager.List.Cast<ExternalValidationResultMessage>();
				Assert(entries.Any(m => m.MessageType == ExternalValidationResultWrapper.ErrorStatus.ToString() && m.MessageContent == "Error1"));
				Assert(entries.Any(m => m.MessageType == ExternalValidationResultWrapper.ErrorStatus.ToString() && m.MessageContent == "Error2"));
				Assert(entries.Any(m => m.MessageType == ExternalValidationResultWrapper.WarningStatus.ToString() && m.MessageContent == "Warning1"));
				Assert(entries.Any(m => m.MessageType == ExternalValidationResultWrapper.WarningStatus.ToString() && m.MessageContent == "Warning2"));
			}
		}

		protected override Form GetFormToBashCore()
		{
			var testValidationResult = new ExternalValidationResult { Value = "Invalid", Errors = new[] { "Error1", "Error2" }, Warnings = new[] { "Warning1", "Warning2" } };
			return new ExternalValidationResultForm(TestOrganization, testValidationResult);
		}
	}
}
