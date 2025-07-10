using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(PromtAmendmentForm))]
	sealed class PromtAmendmentFormTest : ZFormBasherTest
	{
		public void TestIsExtendingAmendmentReasonVisible()
		{
			using (var frm = new PromtAmendmentForm())
			{
				frm.Show();
				Application.DoEvents();
				var textBox = frm.zGroupBox3.FindSingle<ZTextBox>(c => c.Name == "ReasonForExtendingTemporaryImportPeriodInfoTextBox");
				Assert("Default to true.", textBox.Visible);
				var label = frm.zGroupBox3.FindSingle<ZLabel>(c => c.Name == "ReasonForExtendingTemporaryImportPeriodInfoLabel");
				Assert("Default to true.", label.Visible);
			}

			using (var frm = new PromtAmendmentForm(new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory), false))
			{
				frm.Show();
				Application.DoEvents();
				var textBox = frm.zGroupBox3.FindSingle<ZTextBox>(c => c.Name == "ReasonForExtendingTemporaryImportPeriodInfoTextBox");
				Assert("Should be false as the value of isReasonForExtendingEnabled is false.", !textBox.Visible);
				var label = frm.zGroupBox3.FindSingle<ZLabel>(c => c.Name == "ReasonForExtendingTemporaryImportPeriodInfoLabel");
				Assert("Should be false as the value of isReasonForExtendingEnabled is false.", !label.Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var formToBash = new PromtAmendmentForm(new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory), true);
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("zTextBox1", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("ReasonForExtendingTemporaryImportPeriodInfoTextBox", true).Single());
			return formToBash;
		}
	}
}
