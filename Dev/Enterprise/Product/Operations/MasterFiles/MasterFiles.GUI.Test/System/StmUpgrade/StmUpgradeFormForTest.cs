using System.IO;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class StmUpgradeFormForTest : StmUpgradeForm
	{
		readonly Mock<UpgradeManager> upgradeManagerMock;
		public string TempPath;

		public StmUpgradeFormForTest(StmUpgradeCollectionContainer dataSource, Mock<UpgradeManager> upgradeManagerMock = null)
			: base(dataSource)
		{
			TempPath = Path.Combine(Env.TempPath, "StmUpgrade");
			this.upgradeManagerMock = upgradeManagerMock;
		}

		protected override UpgradeManager NewUpgradeManager()
		{
			if (upgradeManagerMock != null)
			{
				return upgradeManagerMock.Object;
			}

			return base.NewUpgradeManager();
		}

		public bool GetIsMoreThanUpgradeSelected()
		{
			return IsMoreThanOneUpgradeSelected;
		}

		public StmUpgradeCollectionView GetAvailableUpgrades()
		{
			return AvailableUpgrades;
		}

		public ZGroupBox GetUpgradesTabGroupBox()
		{
			return UpgradesTabGroupBox;
		}

		public ZLabel GetUpgradesTabNotDisplayedLabel()
		{
			return UpgradesTabNotDisplayedLabel;
		}

		public ZGroupBox GetCMRReferenceFilesTabPageGroupBox()
		{
			return CMRReferenceFilesTabPageGroupBox;
		}

		public ZLabel GetCMRUpgradesTabNotDisplayedLabel()
		{
			return CMRUpgradesTabNotDisplayedLabel;
		}

		public ZCheckBox GetReadyCheckBox()
		{
			return ReadyCheckBox;
		}

		public ZCheckBox GetAppliedCheckBox()
		{
			return AppliedCheckBox;
		}

		public ZCheckBox GetNotAppliedCheckBox()
		{
			return NotAppliedCheckBox;
		}

		public ZCheckBox GetDeletedOrObsoleteCheckBox()
		{
			return DeletedOrObsoleteCheckBox;
		}

		public ZGrid GetAvailableUpgradesGrid()
		{
			return AvailableUpgradesGrid;
		}

		public ZButton GetDeleteButton()
		{
			return DeleteButton;
		}

		public void SelectStmUpgradesTab()
		{
			StmUpgradeTabControl.SelectedIndex = 0;
		}

		public void SelectCMRUpgradesTab()
		{
			StmUpgradeTabControl.SelectedIndex = 1;
		}

		public ZButton GetUpgradeButton()
		{
			return UpgradeButton;
		}

		public ZButton GetSaveToDiskButton()
		{
			return SaveToDiskButton;
		}

		public ZButton GetImportFromFileButton()
		{
			return ImportFromFileButton;
		}

		public StmUpgrade GetCurrentUpgrade()
		{
			return CurrentUpgrade;
		}

		public StmUpgrade[] GetSelectedUpgrades()
		{
			return SelectedUpgrades;
		}

		protected override DialogResult GetPathToSave(out string pathToSave)
		{
			pathToSave = TempPath;
			return GetPathToSaveDialogResult;
		}
		public DialogResult GetPathToSaveDialogResult;

		protected override DialogResult GetDeleteConfirmation()
		{
			return GetDeleteConfirmationDialogResult;
		}
		public DialogResult GetDeleteConfirmationDialogResult;

		public void SelectLatestUpgradeForTest()
		{
			SelectLatestUpgrade();
		}

		protected override DialogResult GetFileToImport(out string filePathToImport)
		{
			filePathToImport = FileToImport;

#if WINZOR
			if (GetFileToImportDialogResult == DialogResult.OK)
			{
				StartProgressForm();
			}
#endif

			return GetFileToImportDialogResult;
		}
		public DialogResult GetFileToImportDialogResult;
		public string FileToImport;

		public StmUpgradeCollectionContainer FormDataSource
		{
			get { return DataSource; }
		}

		public void PerformShowCheckBoxesClickForTest()
		{
			PerformShowCheckBoxesClick();
		}

		public bool IsCMRReferenceTabPageShowing
		{
			get
			{
				foreach (TabPage page in StmUpgradeTabControl.TabPages)
				{
					if (page == CMRReferenceFilesTabPage)
					{
						return true;
					}
				}

				return false;
			}
		}

		public void UpdateCMRFilesMain()
		{
			base.UpdateCMRFiles(CMRUpdateMethod.WebMainFile);
		}

		public void UpdateCMRFilesTestingMain()
		{
			base.UpdateCMRFiles(CMRUpdateMethod.WebTestingMainFile);
		}

		protected override ICMRReferenceFileUpgrader GetNewReferenceFileUpdater()
		{
			return ReferenceFileUpgrader;
		}

		public FakeReferenceFileUpgrader ReferenceFileUpgrader
		{
			get
			{
				if (fReferenceFileUpgrader == null)
				{
					fReferenceFileUpgrader = new FakeReferenceFileUpgrader();
				}
				return fReferenceFileUpgrader;
			}
		}

		FakeReferenceFileUpgrader fReferenceFileUpgrader;

		public class FakeReferenceFileUpgrader : ICMRReferenceFileUpgrader
		{
			public void ImportData(ZBlob fileToUnzip)
			{
				CurrentData = fileToUnzip;
			}

			public ZBlob CurrentData;
		}

		protected override bool IsManualUpgradeAllowed
		{
			get { return isManualUpgradeAllowed; }
		}

		public bool isManualUpgradeAllowed = true;

		protected override void OpenNoteInWebBrowser(string destinationUrl)
		{
			this.destinationUrl = destinationUrl;
		}

		public string destinationUrl;
	}
}
