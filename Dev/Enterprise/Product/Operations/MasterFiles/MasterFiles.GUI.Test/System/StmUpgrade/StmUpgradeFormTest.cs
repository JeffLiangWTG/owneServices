using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(StmUpgradeForm))]
	sealed class StmUpgradeFormTest : ZFormBasherTest
	{
		//#warning pop in an end to end test
		//#warning test message confirmations

		protected override Form GetFormToBashCore()
		{
			return new StmUpgradeForm(new StmUpgradeCollectionContainer(Factory));
		}

		StmUpgradeFormForTest GetFormToTest()
		{
			return new StmUpgradeFormForTest(new StmUpgradeCollectionContainer(Factory));
		}

		StmUpgradeFormForTest GetFormWithSaveToDiskThrowingExceptionToTest()
		{
			return new StmUpgradeFormWithGetPathToSaveThrowingExceptionForTest(new StmUpgradeCollectionContainer(Factory));
		}

		public void TestCMRReferenceFilesTabAU()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			using (StmUpgradeFormForTest form = GetFormToTest())
			{
				AssertEquals(true, form.IsCMRReferenceTabPageShowing);
			}
		}

		[RequiresSTA]
		public void TestInvalidCompressedFileException()
		{
			using (var form = new StmUpgradeFormForAUTest(new StmUpgradeCollectionContainer(Factory)))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.Show();
				form.UpdateCMRFilesInvalidCompressedTesting();
				AssertEquals("Unable to decompress file.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestTabsVisibility()
		{
			Env.Security.SystemUpgrade.IsAllowed = false;
			Env.Security.CMRReferenceFilesUpgrade.IsAllowed = false;
			GlbCompany.CurrentCompany.SetCountry("AU");
			using (StmUpgradeFormForTest form = GetFormToTest())
			{
				form.Show();
				form.SelectStmUpgradesTab();
				Assert("UpgradesTabGroupBox not visible", !form.GetUpgradesTabGroupBox().Visible);
				Assert("UpgradesTabNotDisplayedLabel is visible", form.GetUpgradesTabNotDisplayedLabel().Visible);
				form.SelectCMRUpgradesTab();
				Assert("CMRReferenceFilesTabPageGroupBox not visible", !form.GetCMRReferenceFilesTabPageGroupBox().Visible);
				Assert("CMRUpgradesTabNotDisplayedLabel is visible", form.GetCMRUpgradesTabNotDisplayedLabel().Visible);
			}

			Env.Security.SystemUpgrade.IsAllowed = true;
			Env.Security.CMRReferenceFilesUpgrade.IsAllowed = false;
			using (StmUpgradeFormForTest form = GetFormToTest())
			{
				form.Show();
				form.SelectStmUpgradesTab();
				Assert("UpgradesTabGroupBox is visible", form.GetUpgradesTabGroupBox().Visible);
				Assert("UpgradesTabNotDisplayedLabel not visible", !form.GetUpgradesTabNotDisplayedLabel().Visible);
				form.SelectCMRUpgradesTab();
				Assert("CMRReferenceFilesTabPageGroupBox not visible", !form.GetCMRReferenceFilesTabPageGroupBox().Visible);
				Assert("CMRUpgradesTabNotDisplayedLabel is visible", form.GetCMRUpgradesTabNotDisplayedLabel().Visible);
			}

			Env.Security.SystemUpgrade.IsAllowed = false;
			Env.Security.CMRReferenceFilesUpgrade.IsAllowed = true;
			using (StmUpgradeFormForTest form = GetFormToTest())
			{
				form.Show();
				form.SelectStmUpgradesTab();
				Assert("UpgradesTabGroupBox not visible", !form.GetUpgradesTabGroupBox().Visible);
				Assert("UpgradesTabNotDisplayedLabel is visible", form.GetUpgradesTabNotDisplayedLabel().Visible);
				form.SelectCMRUpgradesTab();
				Assert("CMRReferenceFilesTabPageGroupBox is visible", form.GetCMRReferenceFilesTabPageGroupBox().Visible);
				Assert("CMRUpgradesTabNotDisplayedLabel not visible", !form.GetCMRUpgradesTabNotDisplayedLabel().Visible);
			}

			Env.Security.SystemUpgrade.IsAllowed = true;
			Env.Security.CMRReferenceFilesUpgrade.IsAllowed = true;
			using (StmUpgradeFormForTest form = GetFormToTest())
			{
				form.Show();
				form.SelectStmUpgradesTab();
				Assert("UpgradesTabGroupBox is visible", form.GetUpgradesTabGroupBox().Visible);
				Assert("UpgradesTabNotDisplayedLabel not visible", !form.GetUpgradesTabNotDisplayedLabel().Visible);
				form.SelectCMRUpgradesTab();
				Assert("CMRReferenceFilesTabPageGroupBox is visible", form.GetCMRReferenceFilesTabPageGroupBox().Visible);
				Assert("CMRUpgradesTabNotDisplayedLabel not visible", !form.GetCMRUpgradesTabNotDisplayedLabel().Visible);
			}
		}

		public void TestCMRReferenceFilesTabNonAU()
		{
			GlbCompany.CurrentCompany.SetCountry("NZ");
			using (StmUpgradeFormForTest form = GetFormToTest())
			{
				AssertEquals(false, form.IsCMRReferenceTabPageShowing);
			}
		}

		[TestDate(2005, 08, 07)]
		public void TestCMRReferenceFilesUpdateFromWeb()
		{
			using (StmUpgradeFormForTest form = GetFormToTest())
			{
				form.UpdateCMRFilesMain();
				AssertEquals("P1-MAIN.tar.gz-2005-08-07", form.ReferenceFileUpgrader.CurrentData.ToAscii());

				form.UpdateCMRFilesTestingMain();
				AssertEquals("Q1-MAIN.tar.gz-2005-08-07", form.ReferenceFileUpgrader.CurrentData.ToAscii());
			}
		}

		public void TestCMRReferenceFilesUpgradeLock()
		{
			using (var otherConnection = CargoWise.Data.Db.NewExtraConnectionToMainDb())
			{
				otherConnection.BeginTransaction();

				try
				{
					CmrAuReferenceFileUpdateMutex.AcquireUpdateLock(otherConnection, TimeSpan.Zero);

					using (var form = GetFormToTest())
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

						form.UpdateCMRFilesMain();

						Assert("Error Message should be shown", UnitTestUserNotification.Instance.LastMessage.WasError);
						AssertEquals("Error Message Text", "Unable to perform the CMR Reference Files update simultaneously with other user or batch processor.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
				finally
				{
					otherConnection.RollbackTransaction();
				}
			}
		}

		[RequiresSTA]
		public void TestCurrentUpgrade()
		{
			TestCaseHelper.ClearTable(StmUpgradeSchema.Constants.TableName);

			StmUpgrade upgrade1 = Factory.NewWithValidTestData<StmUpgrade>();
			StmUpgrade upgrade2 = Factory.NewWithValidTestData<StmUpgrade>();

			upgrade1.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
			upgrade2.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;

			upgrade1.SZ_ExeVersionDate = new ZDateTime(2005, 1, 12, 12, 12, 12);
			upgrade2.SZ_ExeVersionDate = new ZDateTime(2004, 12, 13, 15, 12, 11);

			using (StmUpgradeFormForTest testForm = GetFormToTest())
			{
				testForm.Show();

				ZGrid availableUpgradesGrid = testForm.GetAvailableUpgradesGrid();

				availableUpgradesGrid.ListManager.Position = 0;
				AssertEquals("CurrentUpgrade", upgrade1, testForm.GetCurrentUpgrade());

				availableUpgradesGrid.ListManager.Position = 1;
				AssertEquals("CurrentUpgrade", upgrade2, testForm.GetCurrentUpgrade());
			}
		}

		[RequiresSTA]
		public void TestSelectedUpgrades()
		{
			TestCaseHelper.ClearTable(StmUpgradeSchema.Constants.TableName);

			StmUpgrade upgrade1 = Factory.NewWithValidTestData<StmUpgrade>();
			StmUpgrade upgrade2 = Factory.NewWithValidTestData<StmUpgrade>();

			upgrade1.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
			upgrade2.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;

			upgrade1.SZ_ExeVersionDate = new ZDateTime(2005, 1, 12, 12, 12, 12);
			upgrade2.SZ_ExeVersionDate = new ZDateTime(2004, 12, 13, 15, 12, 11);

			using (StmUpgradeFormForTest testForm = GetFormToTest())
			{
				testForm.Show();

				ZGrid availableUpgradesGrid = testForm.GetAvailableUpgradesGrid();

				availableUpgradesGrid.ListManager.Position = 0;
				AssertEquals("SelectedUpgrades.Length", 1, testForm.GetSelectedUpgrades().Length);
				AssertEquals("SelectedUpgrades[0]", upgrade1, testForm.GetSelectedUpgrades()[0]);

				availableUpgradesGrid.ListManager.Position = 1;
				AssertEquals("SelectedUpgrades.Length", 1, testForm.GetSelectedUpgrades().Length);
				AssertEquals("SelectedUpgrades[0]", upgrade2, testForm.GetSelectedUpgrades()[0]);

				availableUpgradesGrid.Select(0);
				availableUpgradesGrid.Select(1);
				AssertEquals("SelectedUpgrades.Length", 2, testForm.GetSelectedUpgrades().Length);
				AssertEquals("SelectedUpgrades[0]", upgrade1, testForm.GetSelectedUpgrades()[0]);
				AssertEquals("SelectedUpgrades[1]", upgrade2, testForm.GetSelectedUpgrades()[1]);
			}
		}

		[RequiresSTA]
		public void TestIsMoreThanOneUpgradeSelected()
		{
			TestCaseHelper.ClearTable(StmUpgradeSchema.Constants.TableName);

			StmUpgrade upgrade1 = Factory.NewWithValidTestData<StmUpgrade>();
			StmUpgrade upgrade2 = Factory.NewWithValidTestData<StmUpgrade>();

			upgrade1.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
			upgrade2.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;

			using (StmUpgradeFormForTest testForm = GetFormToTest())
			{
				testForm.Show();

				ZGrid availableUpgradesGrid = testForm.GetAvailableUpgradesGrid();

				availableUpgradesGrid.UnSelect(0);
				availableUpgradesGrid.UnSelect(1);
				Assert("IsMoreThanUpgradeSelected should be false", !testForm.GetIsMoreThanUpgradeSelected());

				availableUpgradesGrid.Select(0);
				Assert("IsMoreThanUpgradeSelected should be false", !testForm.GetIsMoreThanUpgradeSelected());

				availableUpgradesGrid.Select(1);
				Assert("IsMoreThanUpgradeSelected should be true", testForm.GetIsMoreThanUpgradeSelected());
			}
		}

		[RequiresSTA]
		public void TestGridSortByUpgradeDateDescendingAndLatestUpgradeSelected()
		{
			TestCaseHelper.ClearTable(StmUpgradeSchema.Constants.TableName);

			StmUpgrade upgrade1 = Factory.NewWithValidTestData<StmUpgrade>();
			StmUpgrade upgrade2 = Factory.NewWithValidTestData<StmUpgrade>();
			StmUpgrade upgrade3 = Factory.NewWithValidTestData<StmUpgrade>();
			StmUpgrade upgrade4 = Factory.NewWithValidTestData<StmUpgrade>();

			upgrade1.SZ_ExeVersionDate = new ZDateTime(2005, 1, 12, 12, 12, 12);
			upgrade2.SZ_ExeVersionDate = new ZDateTime(2004, 12, 13, 15, 12, 11);
			upgrade3.SZ_ExeVersionDate = new ZDateTime(2005, 1, 12, 12, 12, 15);
			upgrade4.SZ_ExeVersionDate = new ZDateTime(2005, 1, 13, 12, 12, 12);

			upgrade1.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
			upgrade2.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
			upgrade3.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
			upgrade4.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;

			upgrade1.SZ_MajorVersion = 1;
			upgrade2.SZ_MajorVersion = 2;
			upgrade3.SZ_MajorVersion = 3;
			upgrade4.SZ_MajorVersion = 4;

			using (StmUpgradeFormForTest testForm = GetFormToTest())
			{
				testForm.Show();

				ZGrid availableUpgradesGrid = testForm.GetAvailableUpgradesGrid();

				AssertEquals("Upgrade4 should be the first row", upgrade4, availableUpgradesGrid.List[0]);
				AssertEquals("Upgrade3 should be the second row", upgrade3, availableUpgradesGrid.List[1]);
				AssertEquals("Upgrade1 should be the third row", upgrade1, availableUpgradesGrid.List[2]);
				AssertEquals("Upgrade2 should be the fourth row", upgrade2, availableUpgradesGrid.List[3]);
				Assert("Only one upgrade should be selected", !testForm.GetIsMoreThanUpgradeSelected());
				AssertEquals("ListManager.Position", 0, availableUpgradesGrid.ListManager.Position);
				AssertEquals("Upgrade4 should be selected", upgrade4, testForm.GetCurrentUpgrade());
			}
		}

		[RequiresSTA]
		public void TestSelectLatestUpgrade()
		{
			TestCaseHelper.ClearTable(StmUpgradeSchema.Constants.TableName);

			StmUpgrade upgrade1 = Factory.NewWithValidTestData<StmUpgrade>();
			StmUpgrade upgrade2 = Factory.NewWithValidTestData<StmUpgrade>();

			upgrade1.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
			upgrade2.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;

			upgrade1.SZ_ExeVersionDate = new ZDateTime(2004, 1, 12, 12, 12, 12);
			upgrade2.SZ_ExeVersionDate = new ZDateTime(2005, 12, 13, 15, 12, 11);

			using (StmUpgradeFormForTest testForm = GetFormToTest())
			{
				testForm.Show();

				ZGrid availableUpgradesGrid = testForm.GetAvailableUpgradesGrid();

				testForm.GetAvailableUpgrades().Sort(StmUpgradeSchema.SZ_ExeVersionDate.Name, System.ComponentModel.ListSortDirection.Ascending);
				AssertEquals("AvailableUpgradesGrid.ListManager.Position", 0, availableUpgradesGrid.ListManager.Position);

				testForm.SelectLatestUpgradeForTest();
				AssertEquals("AvailableUpgradesGrid.ListManager.Position", 1, availableUpgradesGrid.ListManager.Position);
			}
		}

		[RequiresSTA]
		public void TestChangeUpgradeButtonText()
		{
			StmUpgrade upgrade1 = CreateUpgrade(StmUpgrade.StmUpgradeStatus.Ready, new ZDateTime(2004, 1, 12, 12, 12, 12), -1);
			StmUpgrade upgrade2 = CreateUpgrade(StmUpgrade.StmUpgradeStatus.Ready, new ZDateTime(2005, 1, 1, 15, 12, 11), 0);
			StmUpgrade upgrade3 = CreateUpgrade(StmUpgrade.StmUpgradeStatus.Ready, new ZDateTime(2005, 1, 12, 12, 12, 15), 1);
			StmUpgrade upgrade4 = CreateUpgrade(StmUpgrade.StmUpgradeStatus.Ready, new ZDateTime(2005, 1, 13, 12, 12, 12), -50);

			using (StmUpgradeFormForTest form = GetFormToTest())
			{
				form.Show();
				ZGrid availableUpgradesGrid = form.GetAvailableUpgradesGrid();
				ZButton upgradeButton = form.GetUpgradeButton();

				availableUpgradesGrid.ListManager.Position = 0;
				Application.DoEvents();
				AssertEquals("UpgradeButton.Text", "Downgrade", upgradeButton.Text);

				availableUpgradesGrid.ListManager.Position = 1;
				Application.DoEvents();
				AssertEquals("UpgradeButton.Text", "Upgrade", upgradeButton.Text);

				availableUpgradesGrid.ListManager.Position = 2;
				Application.DoEvents();
				AssertEquals("UpgradeButton.Text", "Upgrade", upgradeButton.Text);

				availableUpgradesGrid.ListManager.Position = 3;
				Application.DoEvents();
				AssertEquals("UpgradeButton.Text", "Downgrade", upgradeButton.Text);
			}
		}

		[RequiresSTA]
		public void TestDeleteUpgrade()
		{
			TestCaseHelper.ClearTable(StmUpgradeSchema.Constants.TableName);

			using (StmUpgradeFormForTest testForm = GetFormToTest())
			{
				testForm.Show();

				ZGrid availableUpgradesGrid = testForm.GetAvailableUpgradesGrid();
				ZButton deleteButton = testForm.GetDeleteButton();

				AssertEquals("AvailableUpgradesGrid.List.Count", 0, availableUpgradesGrid.List.Count);
				deleteButton.PerformClick();

				Assert("Error message should have been shown",
					Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Contains("Please select an Upgrade in the grid to delete."));
			}

			StmUpgrade upgrade1 = Factory.NewWithValidTestData<StmUpgrade>();
			StmUpgrade upgrade2 = Factory.NewWithValidTestData<StmUpgrade>();

			upgrade1.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
			upgrade2.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;

			upgrade1.SZ_MajorVersion = 1;
			upgrade2.SZ_MajorVersion = 2;

			using (StmUpgradeFormForTest testForm = GetFormToTest())
			{
				testForm.Show();

				ZGrid availableUpgradesGrid = testForm.GetAvailableUpgradesGrid();
				ZButton deleteButton = testForm.GetDeleteButton();

				AssertEquals("AvailableUpgradesGrid.List.Count", 2, availableUpgradesGrid.List.Count);

				availableUpgradesGrid.Select(0);
				availableUpgradesGrid.Select(1);

				testForm.GetDeleteConfirmationDialogResult = DialogResult.Cancel;
				deleteButton.PerformClick();
				Assert("Upgrade1 should not have been deleted", upgrade1.SZ_Status != StmUpgrade.StmUpgradeStatus.Deleted);
				Assert("Upgrade2 should not have been deleted", upgrade2.SZ_Status != StmUpgrade.StmUpgradeStatus.Deleted);

				testForm.GetDeleteConfirmationDialogResult = DialogResult.OK;
				deleteButton.PerformClick();
				Assert("Upgrade1 should have been deleted", upgrade1.SZ_Status == StmUpgrade.StmUpgradeStatus.Deleted);
				Assert("Upgrade2 should have been deleted", upgrade2.SZ_Status == StmUpgrade.StmUpgradeStatus.Deleted);
			}
		}

		[RequiresSTA]
		public void TestCannotDeleteCurrentVersion()
		{
			TestCaseHelper.ClearTable(StmUpgradeSchema.Constants.TableName);

			StmUpgrade upgrade = Factory.NewWithValidTestData<StmUpgrade>();

			upgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.CurrentVersion;

			using (StmUpgradeFormForTest testForm = GetFormToTest())
			{
				testForm.Show();

				ZGrid availableUpgradesGrid = testForm.GetAvailableUpgradesGrid();
				ZButton deleteButton = testForm.GetDeleteButton();

				AssertEquals("AvailableUpgradesGrid.List.Count", 1, availableUpgradesGrid.List.Count);

				availableUpgradesGrid.Select(0);
				deleteButton.PerformClick();

				Assert("Error message should have been shown",
					Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Contains("You cannot delete the current version upgrade package"));
				Assert("Upgrade should not have been deleted", upgrade.SZ_Status != StmUpgrade.StmUpgradeStatus.Deleted);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[RequiresSTA]
		public void TestImportFromFileWhenDispose()
		{
			TestCaseHelper.ClearTable(StmUpgradeSchema.Constants.TableName);

			using (StmUpgradeFormForTest testForm = GetFormToTest())
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
					testForm.Show();

					ZButton importFromFileButton = testForm.GetImportFromFileButton();

					testForm.FileToImport = upgradeFileName;
					testForm.GetFileToImportDialogResult = DialogResult.OK;

					testForm.Dispose();

					AssertNull(testForm.FormDataSource);
					AssertNoExceptionThrown(importFromFileButton.PerformClick);
				}
				finally
				{
					File.Delete(upgradeFileName);
					Directory.Delete(tempPath);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[RequiresSTA]
		public void TestImportFromFile()
		{
			TestCaseHelper.ClearTable(StmUpgradeSchema.Constants.TableName);

			using (StmUpgradeFormForTest testForm = GetFormToTest())
			{
				string tempPath = testForm.TempPath;
				if (!Directory.Exists(tempPath))
				{
					Directory.CreateDirectory(tempPath);
				}

				string upgradeFileName = tempPath + @"\Package20090629_101500_1_2_3_4.edp";
				File.Copy(TestEdpFileName, upgradeFileName);
				new FileInfo(upgradeFileName).IsReadOnly = false;

				string badFileName = tempPath + @"\BadFileName.dat";

				using (FileStream upgradeFile = File.OpenWrite(badFileName))
				{
					upgradeFile.Write(new byte[] { 0, 1, 2, 3, 4 }, 0, 5);
				}

				try
				{
					testForm.Show();

					ZGrid availableUpgradesGrid = testForm.GetAvailableUpgradesGrid();

					ZButton importFromFileButton = testForm.GetImportFromFileButton();

					testForm.FileToImport = upgradeFileName;
					testForm.GetFileToImportDialogResult = DialogResult.Cancel;

					importFromFileButton.PerformClick();
					Assert(UnitTestUserNotification.Instance.LastMessage.Contains("This function is no longer available to ordinary users."));

					AssertEquals("No new upgrades", 0, availableUpgradesGrid.ListManager.Count);

					testForm.FileToImport = badFileName;
					testForm.GetFileToImportDialogResult = DialogResult.OK;

					importFromFileButton.PerformClick();

					AssertEquals("No new upgrades", 0, availableUpgradesGrid.ListManager.Count);

					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("Error importing package from the file " + badFileName + ".\r\nPackage " + badFileName + " is corrupted.", UnitTestUserNotification.Instance.LastMessage.Text);

					testForm.FileToImport = upgradeFileName;
					testForm.GetFileToImportDialogResult = DialogResult.OK;

					importFromFileButton.PerformClick();
					AssertEquals("New upgrade imported", 1, availableUpgradesGrid.ListManager.Count);

					AssertEquals("One New Upgrade", 1, testForm.FormDataSource.FullUpgrades.Count);
					StmUpgrade upgrade = testForm.FormDataSource.FullUpgrades[0];

					AssertEquals(Path.GetFileName(upgradeFileName), upgrade.Filename);
					Assert("Original file for Upgrade should be the same as Upgrade.SZ_UpgradeData_Compressed",
						Utilities.IsByteArrayEqual(File.ReadAllBytes(TestEdpFileName), File.ReadAllBytes(upgradeFileName)));
					ZGuid oldPK = upgrade.PK;

					importFromFileButton.PerformClick();
					AssertEquals("Still one upgrade imported", 1, availableUpgradesGrid.ListManager.Count);

					AssertEquals("Still One Upgrade", 1, testForm.FormDataSource.FullUpgrades.Count);
					upgrade = testForm.FormDataSource.FullUpgrades[0];

					AssertEquals(Path.GetFileName(upgradeFileName), upgrade.Filename);
					Assert("Original file for Upgrade should be the same as Upgrade.SZ_UpgradeData_Compressed",
						Utilities.IsByteArrayEqual(File.ReadAllBytes(TestEdpFileName), File.ReadAllBytes(upgradeFileName)));
				}
				finally
				{
					if (File.Exists(upgradeFileName))
					{
						File.Delete(upgradeFileName);
					}
					if (File.Exists(badFileName))
					{
						File.Delete(badFileName);
					}
					if (Directory.Exists(tempPath))
					{
						Directory.Delete(tempPath);
					}
				}
			}
		}

		[RequiresSTA]
		public void TestImportFromFileDisallowed()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			using (StmUpgradeFormForTest form = GetFormToTest())
			{
				form.isManualUpgradeAllowed = false;
				form.Show();
				ZButton importFromFileButton = form.GetImportFromFileButton();

				UnitTestUserNotification.Instance.AddOKAnswer();
				importFromFileButton.PerformClick();
				AssertEquals(@"http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20091106.pdf", form.destinationUrl);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains("This function is no longer available."));
			}
		}

		[RequiresSTA]
		public void TestSaveToDisk()
		{
			TestCaseHelper.ClearTable(StmUpgradeSchema.Constants.TableName);

			StmUpgrade upgrade1 = Factory.NewWithValidTestData<StmUpgrade>();
			StmUpgrade upgrade2 = Factory.NewWithValidTestData<StmUpgrade>();

			upgrade1.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
			upgrade2.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;

			upgrade1.SZ_ExeVersionDate = new ZDateTime(2005, 1, 12, 12, 12, 12);
			upgrade2.SZ_ExeVersionDate = new ZDateTime(2004, 1, 1, 15, 12, 11);

			upgrade1.SZ_MajorVersion = 1;
			upgrade1.SZ_MinorVersion = 2;
			upgrade1.SZ_Release = 3;
			upgrade1.SZ_Patch = 4;

			upgrade2.SZ_MajorVersion = 5;
			upgrade2.SZ_MinorVersion = 6;
			upgrade2.SZ_Release = 7;
			upgrade2.SZ_Patch = 8;

			upgrade1.SZ_UpgradeData_Compressed = new byte[] { 1, 2, 3, 4 };
			upgrade2.SZ_UpgradeData_Compressed = new byte[] { 5, 6, 7, 8 };

			Factory.Save();

			using (StmUpgradeFormForTest testForm = GetFormToTest())
			{
				testForm.Show();

				ZGrid availableUpgradesGrid = testForm.GetAvailableUpgradesGrid();
				ZButton saveToDiskButton = testForm.GetSaveToDiskButton();

				string tempPath = testForm.TempPath;
				string upgrade1FileName = tempPath + @"\Package20050112_121212_1_2_3_4.edp";
				string upgrade2FileName = tempPath + @"\Package20040101_151211_5_6_7_8.edp";

				if (!Directory.Exists(tempPath))
				{
					Directory.CreateDirectory(tempPath);
				}
				else
				{
					if (File.Exists(upgrade1FileName))
					{
						File.Delete(upgrade1FileName);
					}
					if (File.Exists(upgrade2FileName))
					{
						File.Delete(upgrade2FileName);
					}
				}

				testForm.GetPathToSaveDialogResult = DialogResult.Cancel;

				availableUpgradesGrid.ListManager.Position = 0;
				saveToDiskButton.PerformClick();
				Assert("No file should have been created", !File.Exists(upgrade1FileName));
				Assert("No file should have been created", !File.Exists(upgrade2FileName));

				testForm.GetPathToSaveDialogResult = DialogResult.OK;

				saveToDiskButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains("This function is no longer available to ordinary users."));
				Assert("Upgrade1 should have been saved to disk", File.Exists(upgrade1FileName));
				Assert("Saved file for Upgrade1 should be the same as Upgrade1.SZ_UpgradeData_Compressed",
					Utilities.IsByteArrayEqual(File.ReadAllBytes(upgrade1FileName), new byte[] { 1, 2, 3, 4 }));
				Assert("Upgrade2 should not have been saved to disk", !File.Exists(upgrade2FileName));

				availableUpgradesGrid.ListManager.Position = 1;
				saveToDiskButton.PerformClick();
				Assert("Upgrade1 should still exist on disk", File.Exists(upgrade1FileName));
				Assert("Saved file for Upgrade1 should be the same as Upgrade1.SZ_UpgradeData_Compressed",
					Utilities.IsByteArrayEqual(File.ReadAllBytes(upgrade1FileName), new byte[] { 1, 2, 3, 4 }));
				Assert("Upgrade2 should have been saved to disk", File.Exists(upgrade2FileName));
				Assert("Saved file for Upgrade2 should be the same as Upgrade2.SZ_UpgradeData_Compressed",
					Utilities.IsByteArrayEqual(File.ReadAllBytes(upgrade2FileName), new byte[] { 5, 6, 7, 8 }));

				File.Delete(upgrade1FileName);
				File.Delete(upgrade2FileName);

				availableUpgradesGrid.Select(0);
				availableUpgradesGrid.Select(1);
				saveToDiskButton.PerformClick();
				Assert("Upgrade1 should have been saved to disk", File.Exists(upgrade1FileName));
				Assert("Saved file for Upgrade1 should be the same as Upgrade1.SZ_UpgradeData_Compressed",
					Utilities.IsByteArrayEqual(File.ReadAllBytes(upgrade1FileName), new byte[] { 1, 2, 3, 4 }));
				Assert("Upgrade2 should have been saved to disk", File.Exists(upgrade2FileName));
				Assert("Saved file for Upgrade2 should be the same as Upgrade2.SZ_UpgradeData_Compressed",
					Utilities.IsByteArrayEqual(File.ReadAllBytes(upgrade2FileName), new byte[] { 5, 6, 7, 8 }));

				File.Delete(upgrade1FileName);
				File.Delete(upgrade2FileName);
				Directory.Delete(tempPath);
			}
		}

		[RequiresSTA]
		public void TestSaveToDiskWithInvalidPathGotFromFolderBrowserDialog()
		{
			TestCaseHelper.ClearTable(StmUpgradeSchema.Constants.TableName);

			StmUpgrade upgrade = Factory.NewWithValidTestData<StmUpgrade>();
			upgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
			upgrade.SZ_ExeVersionDate = new ZDateTime(2005, 1, 12, 12, 12, 12);
			upgrade.SZ_MajorVersion = 1;
			upgrade.SZ_MinorVersion = 2;
			upgrade.SZ_Release = 3;
			upgrade.SZ_Patch = 4;
			upgrade.SZ_UpgradeData_Compressed = new byte[] { 1, 2, 3, 4 };
			Factory.Save();

			using (StmUpgradeFormForTest testForm = GetFormWithSaveToDiskThrowingExceptionToTest())
			{
				testForm.Show();
				ZGrid availableUpgradesGrid = testForm.GetAvailableUpgradesGrid();
				ZButton saveToDiskButton = testForm.GetSaveToDiskButton();
				string tempPath = testForm.TempPath;
				string upgradeFileName = tempPath + @"\Package20050112_121212_1_2_3_4.edp";
				if (!Directory.Exists(tempPath))
				{
					Directory.CreateDirectory(tempPath);
				}
				else
				{
					if (File.Exists(upgradeFileName))
					{
						File.Delete(upgradeFileName);
					}
				}
				availableUpgradesGrid.ListManager.Position = 1;

				try
				{
					saveToDiskButton.PerformClick();
				}
				finally
				{
					File.Delete(upgradeFileName);
					Directory.Delete(tempPath);
				}

				Assert(UnitTestUserNotification.Instance.LastMessage.Contains("Selected directory is inaccessible or otherwise invalid. Please select another location or try again."));
			}
		}

		[RequiresSTA]
		public void TestSaveToDiskDisallowed()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			using (StmUpgradeFormForTest form = GetFormToTest())
			{
				form.isManualUpgradeAllowed = false;
				form.Show();
				ZButton saveToDiskButton = form.GetSaveToDiskButton();

				UnitTestUserNotification.Instance.AddOKAnswer();
				saveToDiskButton.PerformClick();
				AssertEquals(@"http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20091106.pdf", form.destinationUrl);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains("This function is no longer available"));
			}
		}

		[RequiresSTA]
		public void TestPerformShowCheckBoxesClick()
		{
			StmUpgrade upgrade1 = CreateUpgrade(StmUpgrade.StmUpgradeStatus.Ready, new ZDateTime(2002, 1, 12, 12, 12, 12), -1);
			StmUpgrade upgrade2 = CreateUpgrade(StmUpgrade.StmUpgradeStatus.CurrentVersion, new ZDateTime(2003, 1, 1, 15, 12, 11), 0);
			StmUpgrade upgrade3 = CreateUpgrade(StmUpgrade.StmUpgradeStatus.NotApplied, new ZDateTime(2004, 1, 12, 12, 12, 12), 1);
			StmUpgrade upgrade4 = CreateUpgrade(StmUpgrade.StmUpgradeStatus.Deleted, new ZDateTime(2005, 1, 1, 15, 12, 11), -2);
			StmUpgrade upgrade5 = CreateUpgrade(StmUpgrade.StmUpgradeStatus.Obsolete, new ZDateTime(2006, 1, 12, 12, 12, 12), -3);

			using (StmUpgradeFormForTest form = GetFormToTest())
			{
				form.Show();
				StmUpgradeCollectionView availableUpgrades = form.GetAvailableUpgrades();
				ZButton upgradeButton = form.GetUpgradeButton();
				ZCheckBox readyCheckBox = form.GetReadyCheckBox();
				ZCheckBox appliedCheckBox = form.GetAppliedCheckBox();
				ZCheckBox notAppliedCheckBox = form.GetNotAppliedCheckBox();
				ZCheckBox deletedOrObsoleteCheckBox = form.GetDeletedOrObsoleteCheckBox();

				AssertEquals("ReadyCheckBox.Checked", true, readyCheckBox.Checked);
				AssertEquals("AppliedCheckBox.Checked.", false, appliedCheckBox.Checked);
				AssertEquals("NotAppliedCheckBox.Checked", false, notAppliedCheckBox.Checked);
				AssertEquals("DeletedOrObsoleteCheckBox.Checked", false, deletedOrObsoleteCheckBox.Checked);

				AssertEquals("UpgradeButton.Text", "Upgrade", upgradeButton.Text);
				AssertEquals("AvailableUpgrades.Contains(upgrade1)", true, availableUpgrades.Contains(upgrade1));
				AssertEquals("AvailableUpgrades.Contains(upgrade2)", true, availableUpgrades.Contains(upgrade2));
				AssertEquals("AvailableUpgrades.Contains(upgrade3)", false, availableUpgrades.Contains(upgrade3));
				AssertEquals("AvailableUpgrades.Contains(upgrade4)", false, availableUpgrades.Contains(upgrade4));
				AssertEquals("AvailableUpgrades.Contains(upgrade5)", false, availableUpgrades.Contains(upgrade5));

				appliedCheckBox.Checked = true;
				Application.DoEvents();
				AssertEquals("UpgradeButton.Text", "Upgrade", upgradeButton.Text);
				AssertEquals("AvailableUpgrades.Contains(upgrade1)", true, availableUpgrades.Contains(upgrade1));
				AssertEquals("AvailableUpgrades.Contains(upgrade2)", true, availableUpgrades.Contains(upgrade2));
				AssertEquals("AvailableUpgrades.Contains(upgrade3)", false, availableUpgrades.Contains(upgrade3));
				AssertEquals("AvailableUpgrades.Contains(upgrade4)", false, availableUpgrades.Contains(upgrade4));
				AssertEquals("AvailableUpgrades.Contains(upgrade5)", false, availableUpgrades.Contains(upgrade5));

				notAppliedCheckBox.Checked = true;
				Application.DoEvents();
				AssertEquals("UpgradeButton.Text", "Upgrade", upgradeButton.Text);
				AssertEquals("AvailableUpgrades.Contains(upgrade1)", true, availableUpgrades.Contains(upgrade1));
				AssertEquals("AvailableUpgrades.Contains(upgrade2)", true, availableUpgrades.Contains(upgrade2));
				AssertEquals("AvailableUpgrades.Contains(upgrade3)", true, availableUpgrades.Contains(upgrade3));
				AssertEquals("AvailableUpgrades.Contains(upgrade4)", false, availableUpgrades.Contains(upgrade4));
				AssertEquals("AvailableUpgrades.Contains(upgrade5)", false, availableUpgrades.Contains(upgrade5));

				deletedOrObsoleteCheckBox.Checked = true;
				Application.DoEvents();
				AssertEquals("UpgradeButton.Text", "Downgrade", upgradeButton.Text);
				AssertEquals("AvailableUpgrades.Contains(upgrade1)", true, availableUpgrades.Contains(upgrade1));
				AssertEquals("AvailableUpgrades.Contains(upgrade2)", true, availableUpgrades.Contains(upgrade2));
				AssertEquals("AvailableUpgrades.Contains(upgrade3)", true, availableUpgrades.Contains(upgrade3));
				AssertEquals("AvailableUpgrades.Contains(upgrade4)", true, availableUpgrades.Contains(upgrade4));
				AssertEquals("AvailableUpgrades.Contains(upgrade5)", true, availableUpgrades.Contains(upgrade5));

				readyCheckBox.Checked = false;
				appliedCheckBox.Checked = false;
				notAppliedCheckBox.Checked = false;
				deletedOrObsoleteCheckBox.Checked = false;
				Application.DoEvents();
				AssertEquals("UpgradeButton.Text", "Upgrade", upgradeButton.Text);
				AssertEquals("AvailableUpgrades.Contains(upgrade1)", false, availableUpgrades.Contains(upgrade1));
				AssertEquals("AvailableUpgrades.Contains(upgrade2)", true, availableUpgrades.Contains(upgrade2));
				AssertEquals("AvailableUpgrades.Contains(upgrade3)", false, availableUpgrades.Contains(upgrade3));
				AssertEquals("AvailableUpgrades.Contains(upgrade4)", false, availableUpgrades.Contains(upgrade4));
				AssertEquals("AvailableUpgrades.Contains(upgrade5)", false, availableUpgrades.Contains(upgrade5));
			}
		}

		static string TestEdpFileName
		{
			get { return Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\System\StmUpgrade\testing.edp"); }
		}

		[RequiresSTA]
		public void TestSaveToDiskClickRequestResponseOnInvalidPackageException()
		{
			TestCaseHelper.ClearTable(StmUpgradeSchema.Constants.TableName);

			StmUpgrade upgrade1 = Factory.NewWithValidTestData<StmUpgrade>();
			upgrade1.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
			Factory.Save();

			var guid = Guid.NewGuid();
			var upgradeManagerMock = new Mock<UpgradeManager>(It.IsAny<string>(), It.IsAny<string>());
			upgradeManagerMock
				.Setup(x => x.DownloadUpgradePackageFile(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Upgrades.Progress>()))
				.Callback((Guid pk, string fileName, Upgrades.Progress progress) =>
				{
					throw new InvalidPackageException(Invariant($"Package {pk} not available to download"));
				});

			using (StmUpgradeFormForTest testForm = new StmUpgradeFormForTest(new StmUpgradeCollectionContainer(Factory), upgradeManagerMock))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.Show();

				ZGrid availableUpgradesGrid = testForm.GetAvailableUpgradesGrid();

				availableUpgradesGrid.ListManager.Position = 0;
				availableUpgradesGrid.Select(0);
				testForm.GetPathToSaveDialogResult = DialogResult.OK;
				ZButton saveToDiskButton = testForm.GetSaveToDiskButton();
				saveToDiskButton.PerformClick();
			}

			AssertEquals("Package download failure message caption expected.", "Upgrade Package Not Found", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals("Package download failure message text expected.", $"Failed to download the upgrade package for version {upgrade1.VersionNumber}, the server returned package not found.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		StmUpgrade CreateUpgrade(string status, ZDateTime exeVersionDate, int addRelease)
		{
			StmUpgrade result = Factory.NewWithValidTestData<StmUpgrade>();
			result.SZ_Status = status;
			result.SZ_ExeVersionDate = exeVersionDate;

			var ver = ReleaseInfo.Instance.VersionNumber;
			int[] version = { ver.Patch, ver.Release, ver.Minor, ver.Major };

			//let the negative bubble up
			version[1] += addRelease;
			for (int j = 1; j < version.Length - 1 && version[j] < 0; j++)
			{
				version[j] *= -1;
				version[j + 1]--;
			}
			version[3] = Math.Max(0, version[3]); //Major set to atleast 0
			result.VersionNumber = new VersionNumber(version[3], version[2], version[1], version[0]);

			return result;
		}
	}
}
