using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.GUI;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	[TestedType(typeof(DpsMarkJobClearConfirmationForm))]
	public class DpsMarkJobClearConfirmationFormTest : ZFormBasherTest
	{
		public void TestSetTextAndImagesIfHasNoRelatedJobs()
		{
			using (var form = (DpsMarkJobClearConfirmationForm)GetFormToBashCore())
			{
				form.Show();
				var warnPic = form.Controls.Find("WarningPictureBox", true)[0] as KPictureBox;
				var reasonLabel = form.Controls.Find("ReasonLabel", true)[0] as ZLabel;
				var warnLabel = form.Controls.Find("WarningMessageLabel", true)[0] as ZLabel;
				CombineAssertions(() =>
				{
					AssertNotNull(warnPic.Image);
					AssertEquals("Please provide the clearing reason which will be stored in the audit logs", reasonLabel.Text);
					AssertEquals("Marking Job TEST_ID001 as JCL - Job Clear will allow the job to be progressed and override any document restrictions. This action should only be undertaken by a compliance officer who understands your companies compliance processes and your action will be recorded for auditing purposes.", warnLabel.Text);
				});
			}
		}

		public void TestSetTextAndImagesIfHasRelatedJobs()
		{
			var model = new DpsMarkJobClearConfirmationModel
			{
				JobID = "TEST_ID001",
				RelatedJobsIDNotJCLOrCLR = new string[] { "TEST_ID002", "TEST_ID003" }
			};
			using (var form = new DpsMarkJobClearConfirmationForm(model))
			{
				form.Show();
				var warnPic = form.Controls.Find("WarningPictureBox", true)[0] as KPictureBox;
				var reasonLabel = form.Controls.Find("ReasonLabel", true)[0] as ZLabel;
				var warnLabel = form.Controls.Find("WarningMessageLabel", true)[0] as ZLabel;
				CombineAssertions(() =>
				{
					AssertNotNull(warnPic.Image);
					AssertEquals("Please provide the clearing reason which will be stored in the audit logs", reasonLabel.Text);
					AssertEquals("Marking Job TEST_ID001 as JCL - Job Clear will allow the job to be progressed and override any document restrictions. The screening status of its related Job(s) TEST_ID002, TEST_ID003 will also be marked as Job Clear. This action should only be undertaken by a compliance officer who understands your companies compliance processes and your action will be recorded for auditing purposes.", warnLabel.Text);
				});
			}
		}

		public void TestOKButton()
		{
			var model = new DpsMarkJobClearConfirmationModel
			{
				JobID = "TEST_ID001"
			};

			using (var form = new DpsMarkJobClearConfirmationForm(model))
			{
				form.Show();
				var oKButton = form.Controls.Find("OKButton", true)[0] as ZButton;
				var confirmClearText = form.Controls.Find("ConfirmClearText", true)[0] as ZTextBox;
				var jobClearReasonTextBox = form.Controls.Find("JobClearReasonTextBox", true)[0] as ZTextBox;
				AssertEquals(false, oKButton.Enabled);

				confirmClearText.Text = "I AM AUTHORISED TO CLEAR THIS JOB";
				AssertEquals(true, oKButton.Enabled);
				AssertNotEquals(DialogResult.OK, form.DialogResult);

				jobClearReasonTextBox.Text = "ABCD ABCDEF";
				oKButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertEquals("ABCD ABCDEF", model.Reason);
			}
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control.Name == "ConfirmClearText")
			{
				return true;
			}

			return base.ShouldIgnoreMissingBindingMember(control);
		}

		protected override Form GetFormToBashCore()
		{
			var model = new DpsMarkJobClearConfirmationModel
			{
				JobID = "TEST_ID001"
			};
			var form = new DpsMarkJobClearConfirmationForm(model);
			MissingResourceStringChecker.ExcludeFromTest(form.Controls.Find("ConfirmClearText", true)[0]);
			MissingResourceStringChecker.ExcludeFromTest(form.Controls.Find("JobClearReasonTextBox", true)[0]);
			return form;
		}

		protected override bool AllowFormSizeFixed => true;
	}
}
