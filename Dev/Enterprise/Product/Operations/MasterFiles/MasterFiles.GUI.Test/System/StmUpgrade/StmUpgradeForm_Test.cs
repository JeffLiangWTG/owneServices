using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	sealed class StmUpgradeForm_Test : TestCaseWithFactory
	{
		public void TestCMRReferenceFilesUpdateForChange()
		{
			using (StmUpgradeForm form = new StmUpgradeForm(new StmUpgradeCollectionContainer(Factory)))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				UnitTestUserNotification userNotification = Globals.Message as UnitTestUserNotification;
				form.DataFilePathTextBox.Text = "P1-CHNG.tar.gz";
				form.UpdateFromFileButton_Click(null, null);
				Assert(userNotification.LastMessage.Text.IndexOf("Importing of the 'Change' file for the CMR Reference files is not supported. Please use the 'Main' file only") > -1);
			}
		}
	}
}
