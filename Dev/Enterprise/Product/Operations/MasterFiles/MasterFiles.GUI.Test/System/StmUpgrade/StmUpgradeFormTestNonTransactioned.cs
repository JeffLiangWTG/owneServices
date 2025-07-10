using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class StmUpgradeFormTestNonTransactioned : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		[RequiresSTA]
		public void TestOpenUpgradeFormAndImportFile()
		{
			TestCaseHelper.ClearTable(StmUpgradeSchema.Constants.TableName);

			var timerTest = new System.Windows.Forms.Timer();
			timerTest.Interval = (int)new TimeSpan(0, 0, 0, 0, 10).TotalMilliseconds;
			timerTest.Tick += timerTest_Tick;

			using (var testForm = new StmUpgradeFormForTest(new StmUpgradeCollectionContainer(new BusinessObjectFactory())))
			{
				string tempPath = testForm.TempPath;
				if (!Directory.Exists(tempPath))
				{
					Directory.CreateDirectory(tempPath);
				}

				string upgradeFileName = tempPath + @"\Package20090629_101500_1_2_3_4.edp";
				File.Copy(TestEdpFileName, upgradeFileName);
				new FileInfo(upgradeFileName).IsReadOnly = false;

				try
				{
					timerTest.Start();
					Thread.Sleep(1000);

					testForm.Show();

					ZButton importFromFileButton = testForm.GetImportFromFileButton();

					testForm.FileToImport = upgradeFileName;
					testForm.GetFileToImportDialogResult = DialogResult.OK;

					AssertNoExceptionThrown(importFromFileButton.PerformClick);
					//AssertEquals("", UnitTestUserNotification.Instance.LastMessage.ToString()); // undo this line when checking original functionality since no error will occur otherwise
				}
				finally
				{
					timerTest.Stop();
					timerTest.Dispose();

					File.Delete(upgradeFileName);
					Directory.Delete(tempPath);
				}
			}
		}

		#region Implementation

		void timerTest_Tick(object sender, EventArgs e)
		{
			Db.Connection.ExecuteScalar("SELECT 1");
		}

		static string TestEdpFileName
		{
			get { return Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\System\StmUpgrade\testing.edp"); }
		}

		#endregion // Implementation
	}
}
