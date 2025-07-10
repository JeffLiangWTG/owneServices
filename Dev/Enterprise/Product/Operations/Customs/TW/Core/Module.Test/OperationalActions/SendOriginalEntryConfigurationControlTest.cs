using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.TW.Module.OperationalActions;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.Module.Testing
{
	internal sealed class SendOriginalEntryConfigurationControlTest : TestCaseWithFactory
	{
		public void TestReadOnlyWhenSetBeforeBinding()
		{
			var applicator = new DeclarationMessageOperationalActionMethodApplicator();
			using (var testForm = new ZChildForm(applicator))
			{
				var testControl = new SendOriginalEntryConfigurationControl();
				((ICompositeControlBindingSourceProvider)testForm).BindingSource.SetBindingMember(testControl, ".");
				testForm.Controls.Add(testControl);
				testForm.Show();
				Application.DoEvents();
				var useDaysOfDelayedCheckBox = testControl.FindSingleOrDefault<ZCheckBox>(x => x.Name == "UseDaysOfDelayedCheckBox");
				var reMergeAndCalculateCheckBox = testControl.FindSingleOrDefault<ZCheckBox>(x => x.Name == "ReMergeAndCalculateCheckBox");
				var suppressNotificationDialogsCheckBox = testControl.FindSingleOrDefault<ZCheckBox>(x => x.Name == "SuppressNotificationDialogsCheckBox");
				var ignoreMessegeWarningsCheckBox = testControl.FindSingleOrDefault<ZCheckBox>(x => x.Name == "IgnoreMessageWarningsCheckBox");
				AssertEquals(true, useDaysOfDelayedCheckBox.Checked);
				AssertEquals(false, reMergeAndCalculateCheckBox.Checked);
				AssertEquals(true, suppressNotificationDialogsCheckBox.Checked);
				AssertEquals(false, ignoreMessegeWarningsCheckBox.Checked);
				useDaysOfDelayedCheckBox.Checked = false;
				reMergeAndCalculateCheckBox.Checked = true;
				suppressNotificationDialogsCheckBox.Checked = false;
				ignoreMessegeWarningsCheckBox.Checked = true;
				AssertEquals(false, applicator.UseDaysOfDelayed);
				AssertEquals(true, applicator.ReMergeAndCalculate);
				AssertEquals(false, applicator.SuppressNotificationPopout);
				AssertEquals(true, applicator.IgnoreMessageWarnings);
			}
		}
	}
}
