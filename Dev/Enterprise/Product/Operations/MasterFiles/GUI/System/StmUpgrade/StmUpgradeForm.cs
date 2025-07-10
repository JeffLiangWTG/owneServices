using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.AU.CMR;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	[SuppressBindingMemberBashingTest]
	public partial class StmUpgradeForm : ZChildForm
	{
		public StmUpgradeForm(StmUpgradeCollectionContainer dataSource)
			: base(dataSource)
		{
			InitializeComponent();
			SetShowDefaults();

			AvailableUpgradesGrid.AfterBind += new EventHandler(AvailableUpgradesGrid_AfterBind);
		}

		protected new StmUpgradeCollectionContainer DataSource
		{
			get { return (StmUpgradeCollectionContainer)base.DataSource; }
		}

		protected StmUpgradeCollectionView AvailableUpgrades
		{
			get { return DataSource.FullUpgradesView; }
		}

		#region Overrides

		public override string FormVerb
		{
			get
			{
				return "";
			}
		}

		void AvailableUpgradesGrid_AfterBind(object sender, EventArgs e)
		{
			SortGridByDateDescendingAndSelectLatestUpgrade();
			ChangeUpgradeButtonText();
			AvailableUpgradesGrid.ListManager.CurrentChanged += new EventHandler(AvailableUpgradesGrid_CurrentChanged);
		}

		void AvailableUpgradesGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			StmUpgrade upgrade = (StmUpgrade)e.ObjectAtRow;
			if (upgrade.IsCurrentVersion)
			{
				e.Colour = System.Drawing.Color.Red;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			IndustryTestingGroupBox.Visible = FileGroupBox.Visible = DataSource.IsCmrManualTestFileUpdateAllowed;
		}

		#endregion

		#region Implementation

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void ShowNoUpgradeSelectedErrorMessage(string action)
		{
			Globals.Message.ShowError(Res.GetString("784eb2ef-1fff-42da-9003-f59d27e67db0", "Please select an Upgrade in the grid to {0}.", action), Res.GetString("5d3828b3-611b-4fd3-b34e-56a3dd37d02b", "Please Select an Upgrade"));
		}

		void ShowDeletedOrObsoleteUpgradeSelectedErrorMessage(string action)
		{
			Globals.Message.ShowError(Res.GetString("135dfcd0-07f6-4842-a40b-32ec914be84d", "One or more of the the Upgrades you have selected has already been deleted or is obsolete.\r\nDeleted or obsolete Upgrades cannot be {0}.", action),
				Res.GetString("282007b7-6bc6-4b49-9822-fdd7b6d93cb0", "Deleted or Obsolete Upgrade(s) Selected"));
		}

		void ShowCannotDeleteCurrentVersionErrorMessage()
		{
			Globals.Message.ShowError(Res.GetString("4f81125e-efeb-4a8d-b28e-a4cb591c9b0d", "You cannot delete the current version upgrade package."), Res.GetString("d044dbaa-54c5-4c14-9965-22ebfcdb2bde", "Delete Upgrade(s)"));
		}
#if !WINZOR
		void ShowInvalidDirectorySelectedErrorMessage()
		{
			Globals.Message.ShowError(Res.GetString("9C8C1CD4-5B7F-43E1-9644-D3BE266A613E", "Selected directory is inaccessible or otherwise invalid. Please select another location or try again."), Res.GetString("35B286B9-E678-4BAD-A6E5-D8791AA8A83C", "Invalid Upgrade Directory"));
		}
#endif
		void ShowPackageNotFoundErrorMessage(string version)
		{
			Globals.Message.ShowError(Res.GetString("1E9099AA-B669-453F-B097-D7DE5B58E6BB", "Failed to download the upgrade package for version {0}, the server returned package not found.", version), Res.GetString("4C420C07-B3A5-47CC-B784-B70774208E7E", "Upgrade Package Not Found"));
		}

		#region Change Upgrade Button Text

		void AvailableUpgradesGrid_CurrentChanged(object sender, EventArgs e)
		{
			// This needs to be done after the grid handles the event, so we use BeginInvoke.
			BeginInvoke(new MethodInvoker(ChangeUpgradeButtonText));
		}

		protected void ChangeUpgradeButtonText()
		{
			if (CurrentUpgrade != null)
			{
				UpgradeButton.Text = (CurrentUpgrade.IsOlderThanCurrentVersion) ? Res.GetString("650c0c1a-ad62-4ed7-aaf8-227b3891b5c4", "Downgrade") : Res.GetString("112d3619-0a5e-4651-8d5a-f387df457473", "Upgrade");
			}
		}

		#endregion

		#region Currently Selected Upgrade(s)

		protected ZGrid CurrentGrid
		{
			get
			{
				if (StmUpgradeTabControl.SelectedTab == UpgradesTabPage)
				{
					return AvailableUpgradesGrid;
				}
				return null;
			}
		}

		protected StmUpgrade CurrentUpgrade
		{
			get
			{
				StmUpgrade result = null;

				if (CurrentGrid != null &&
					!IsMoreThanOneUpgradeSelected &&
					CurrentGrid.ListManager != null)
				{
					result = (StmUpgrade)CurrentGrid.ListManager.GetCurrent();
				}

				return result;
			}
		}

		protected StmUpgrade[] SelectedUpgrades
		{
			get
			{
				ArrayList result = new ArrayList();

				if (IsMoreThanOneUpgradeSelected)
				{
					foreach (StmUpgrade upgrade in CurrentGrid.SelectedElements)
					{
						result.Add(upgrade);
					}
				}
				else if (CurrentUpgrade != null)
				{
					result.Add(CurrentUpgrade);
				}

				return (StmUpgrade[])result.ToArray(typeof(StmUpgrade));
			}
		}

		protected bool IsMoreThanOneUpgradeSelected
		{
			get
			{
				if (CurrentGrid != null)
				{
					return CurrentGrid.SelectedElements.Length > 1;
				}
				return false;
			}
		}

		#endregion

		#region Perform Upgrade

		void UpgradeButton_Click(object sender, EventArgs e)
		{
			if (IsMoreThanOneUpgradeSelected)
			{
				Globals.Message.ShowError(Res.GetString("80da47c7-77bf-4872-b8ab-91b2e7b10d2c", "You have selected more than one Upgrade. Please select only one Upgrade to apply to your current system."),
					Res.GetString("fe14d2e7-21f1-471d-a4e7-31fa4e2608d2", "Please Select Only One Upgrade"));
			}
			else
			{
				if (CurrentUpgrade != null)
				{
					if (CurrentUpgrade.IsCurrentVersion)
					{
						Globals.Message.ShowError(Res.GetString("c91b7894-bf18-4bba-bf8a-4cb23a2536bc", "{0} is already on the same version of the Upgrade you have selected.", Core.Constants.ProductName), Res.GetString("5b1f2b88-b4eb-4d93-85b6-f0721a0d4f35", "Upgrade Version Is The Same"));
					}
					else if (CurrentUpgrade.IsNotDeployable)
					{
						Globals.Message.ShowError(Res.GetString("0d0f557c-41cd-48f5-ad07-fb1b248b87f0", "The selected Upgrade is deleted/obsolete and cannot be applied."), Res.GetString("139c1f55-cf31-4493-bc9a-5c327c99db89", "Upgrade is Invalid"));
					}
					else if (CurrentUpgrade.VersionNumber.ToVersion() < UpgradeManager.MinimumRunnableVersion)
					{
						Globals.Message.ShowError(Res.GetString("7272217a-27a2-4889-99e9-3f5325168d09", "The selected Upgrade uses an obsolete upgrade mechanism and cannot be applied."), Res.GetString("f0e41c23-83f1-4e81-a8e2-956e367176da", "Downgrade Not Supported"));
					}
					else if (GetUpgradeConfirmation() == DialogResult.OK)
					{
						try
						{
							PerformUpgrade();
						}
						catch (InvalidOperationException excep)
						{
							Globals.Message.ShowError(excep.Message, Res.GetString("25783774-dbcc-4600-b0c7-258c2aeb6f39", "Error Encountered During Upgrade"));
						}
						catch (UnauthorizedAccessException excep)
						{
							Globals.Message.ShowError(excep.Message, Res.GetString("25783774-dbcc-4600-b0c7-258c2aeb6f39", "Error Encountered During Upgrade"));
						}
					}
				}
				else
				{
					ShowNoUpgradeSelectedErrorMessage(Res.GetString("94cde47d-450b-4bcd-87fa-b894e8fb1f38", "apply to your current system. CargoWise will inform you when a new version has been sent to your system and ready for upgrade"));
				}
			}
		}

		ProgressFormManager progressForm;
		bool progressFormCancelled;

		void PerformUpgrade()
		{
			Upgrader upgrader = new Upgrader();
			var upgrade = CurrentUpgrade;
			using (progressForm = new ProgressFormManager())
			{
				StartProgressForm();
				try
				{
					using (upgrader.InstallPackage(upgrade, UpdateProgress))
					{
						upgrader.LaunchUpgrade(upgrade);
					}
				}
				catch (CancelledException)
				{ }
				catch (CorruptEdpException)
				{
					Globals.Message.ShowError(Res.GetString("4225AEE6-AB45-44e5-99E8-F1380117C34F", "The upgrade package file is corrupt. Check that the file has finished downloading, and that the file size is correct. If the problem persists, try downloading a new copy of the file."));
				}
				catch (InvalidPackageException ex)
				{
					Globals.Message.ShowError(Res.GetString("61280AA7-FAF6-41ef-BC4D-2123D0F51E22", "The upgrade package contents could not be validated.") + "\r\n" + ex.Message);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError(Res.GetString("E8BB29D6-EF20-4881-B75F-7975790B1F10", "The upgrade package could not be installed.") + "\r\n" + ex.Message);
				}
			}
		}

		bool UpdateProgress(string status, int percentComplete)
		{
			progressForm.UpdateStatus(status, percentComplete);
			return !progressFormCancelled;
		}

		DialogResult GetUpgradeConfirmation()
		{
			return Globals.Message.ShowConfirmation(Res.GetString("f1f45fac-50fe-4d05-b73e-0d8211baa1a1", "Do you want to apply the selected Upgrade to {0}?\r\nBefore you click yes, please make sure you close other unsaved forms as {1} will be restarted shortly.", Core.Constants.ProductName, Core.Constants.ProductName), Res.GetString("054432ae-a929-4cfd-90bf-52a6eabe0ac5", "Continue?"), (NoResString)"Yes", MessageBoxIcon.Question);
		}

		#endregion

		#region Delete Upgrade(s)

		void DeleteButton_Click(object sender, EventArgs e)
		{
			if (SelectedUpgrades != null &&
				SelectedUpgrades.Length > 0)
			{
				bool continueWithDelete = true;
				foreach (StmUpgrade upgrade in SelectedUpgrades)
				{
					if (upgrade.IsCurrentVersion)
					{
						ShowCannotDeleteCurrentVersionErrorMessage();
						continueWithDelete = false;
						break;
					}
					if (upgrade.IsNotDeployable)
					{
						ShowDeletedOrObsoleteUpgradeSelectedErrorMessage(Res.GetString("b5f6ebf5-0960-463f-8409-93b086c01797", "deleted"));
						continueWithDelete = false;
						break;
					}
				}

				if (continueWithDelete &&
					GetDeleteConfirmation() == DialogResult.OK)
				{
					DataSource.DeleteUpgrades(SelectedUpgrades);
				}
			}
			else
			{
				ShowNoUpgradeSelectedErrorMessage(Res.GetString("cdf74ad0-c937-48ef-b7c1-49b14f0de013", "delete"));
			}
		}

		protected virtual DialogResult GetDeleteConfirmation()
		{
			string question = Res.GetString("2eabeb1b-e363-4a67-9466-9a64810b01c4", "Do you really want to delete the selected Upgrade(s) from {0}?\r\nUpgrades are not recoverable once deleted.", Core.Constants.ProductName);
			return Globals.Message.ShowConfirmation(question, Res.GetString("054432ae-a929-4cfd-90bf-52a6eabe0ac5", "Continue?"), (NoResString)"Yes", MessageBoxIcon.Question);
		}

		#endregion

		#region Select Latest Upgrade

		void SortGridByDateDescendingAndSelectLatestUpgrade()
		{
			SortGridByDateDescending();

			if (AvailableUpgradesGrid.ListManager != null &&
				AvailableUpgradesGrid.ListManager.Count > 0)
			{
				SelectLatestUpgrade();
			}
		}

		void SortGridByDateDescending()
		{
			AvailableUpgrades.Sort(StmUpgradeSchema.SZ_ExeVersionDate.Name, System.ComponentModel.ListSortDirection.Descending);
			AvailableUpgradesGrid.RefreshTableStyles();
		}

		protected void SelectLatestUpgrade()
		{
			int latestUpgradeIndex = 0;
			StmUpgrade latestUpgrade = (StmUpgrade)AvailableUpgradesGrid.List[0];
			for (int x = 1; x < AvailableUpgradesGrid.ListManager.Count; ++x)
			{
				StmUpgrade upgrade = (StmUpgrade)AvailableUpgradesGrid.List[x];

				if (upgrade.SZ_ExeVersionDate >
					latestUpgrade.SZ_ExeVersionDate)
				{
					latestUpgradeIndex = x;
					latestUpgrade = upgrade;
				}
			}

			AvailableUpgradesGrid.ListManager.Position = latestUpgradeIndex;
			AvailableUpgradesGrid.Select(latestUpgradeIndex);
		}

		#endregion

		#region Save Selected Upgrade(s) To Disk

		protected virtual UpgradeManager NewUpgradeManager()
		{
			return Upgrader.NewUpgradeManager();
		}

		void SaveToDiskButton_Click(object sender, EventArgs e)
		{
			if (ShowManualUpgradeObsoleteMessage())
			{
				return;
			}

			if (SelectedUpgrades != null &&
			SelectedUpgrades.Length > 0)
			{
				bool continueWithSave = true;

				foreach (StmUpgrade upgrade1 in SelectedUpgrades)
				{
					if (upgrade1.IsNotDeployable)
					{
						ShowDeletedOrObsoleteUpgradeSelectedErrorMessage(Res.GetString("01adc591-9cad-4d9a-86b1-09a82cd99cd5", "saved to disk"));
						continueWithSave = false;
						break;
					}
				}

				if (continueWithSave)
				{
#if !WINZOR
					string pathToSave = "";

					DialogResult getPathToSaveDialogResult = DialogResult.Cancel;
					try
					{
						getPathToSaveDialogResult = GetPathToSave(out pathToSave);
					}
					catch (ArgumentException)
					{
						ShowInvalidDirectorySelectedErrorMessage();
					}

					if (getPathToSaveDialogResult == DialogResult.OK)
#endif
					{
						using (progressForm = new ProgressFormManager())
						{
							var versionNo = string.Empty;
							try
							{
								progressForm.UpdateStatus(Res.GetString("484b691b-9768-4c64-9264-2c6219c7020b", "Saving upgrade packages"), progressForm.ProgressValue);
								StartProgressForm();

								StmUpgrade[] selectedUpgrades = SelectedUpgrades;
								UpgradeManager mgr = NewUpgradeManager();
								saveToDiskTasks = selectedUpgrades.Length;
								for (saveToDiskTask = 0; saveToDiskTask < saveToDiskTasks && !progressFormCancelled; saveToDiskTask++)
								{
									StmUpgrade upgrade = selectedUpgrades[saveToDiskTask];
									var packagePk = upgrade.PK.ToGuid();
									versionNo = upgrade.FullVersionString;
#if !WINZOR
									var savePath = Path.Combine(pathToSave, upgrade.Filename);
									mgr.DownloadUpgradePackageFile(packagePk, savePath, UpdateSaveToDiskProgress);
#else
									
									if (CargoWiseClientServices != null)
									{
										var upgradeDownloadObject = new UpgradeDownloadObject(mgr, upgrade.Filename, packagePk);
										upgradeDownloadObject.Prepare();
										var objectId = CargoWiseClientServices?.FileService.AddDownloadObject(upgradeDownloadObject) ?? "";
										CargoWiseClientServices?.FileService.DownloadFileAsync(objectId);
									}
#endif
								}
							}
							catch (InvalidPackageException)
							{
								ShowPackageNotFoundErrorMessage(versionNo);
							}
							catch (CancelledException)
							{ }
						}
					}
				}
			}
			else
			{
				ShowNoUpgradeSelectedErrorMessage(Res.GetString("23d7ad52-50d7-4618-bbeb-13dd79d9608f", "save to disk"));
			}
		}

		int saveToDiskTasks;
		int saveToDiskTask;
#if !WINZOR
		bool UpdateSaveToDiskProgress(string status, int percentComplete)
		{
			var formPercent = (percentComplete / saveToDiskTasks) + saveToDiskTask * (100 / saveToDiskTasks);
			progressForm.UpdateStatus(progressForm.Status, formPercent);
			return !progressFormCancelled;
		}
#endif
		protected virtual DialogResult GetPathToSave(out string pathToSave)
		{
			pathToSave = "";

			var folderBrowserDialog = new ZFolderBrowserDialog();
			var result = folderBrowserDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				pathToSave = folderBrowserDialog.MappedSelectedPath;
			}

			return result;
		}

		#endregion

		#region Import Update from File on Disk

		void ImportFromFileButton_Click(object sender, EventArgs e)
		{
			if (!ShowManualUpgradeObsoleteMessage())
			{
				ImportFromFile();
			}
		}

		protected void ImportFromFile()
		{
			using (progressForm = new ProgressFormManager())
			{
				if (GetFileToImport(out string sourceFile) == DialogResult.OK)
				{
					try
					{
						using (ZOpenFileDialog.ForceLocalFile(ref sourceFile))
						{
#if !WINZOR
							StartProgressForm();
#endif
							new StmUpgradeImporter().ImportPackage(sourceFile, UpdateProgress);
						}
					}
					catch (CancelledException)
					{ }
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						Globals.Message.ShowError(Res.GetString("6bc1ecb5-0ab4-4d32-828c-5e8740bac6d1", "Error importing package from the file {0}.\r\n{1}", sourceFile, ex.Message));
					}

					if (DataSource != null)
					{
						DataSource.FullUpgrades.SwapFactoryAndRemoveAll(new BusinessObjectFactory());
						DataSource.FullUpgrades.Load();

						DataSource.UpdateFullUpgradesView(ReadyCheckBox.Checked, AppliedCheckBox.Checked, NotAppliedCheckBox.Checked, DeletedOrObsoleteCheckBox.Checked, DeletedOrObsoleteCheckBox.Checked);
					}
				}
			}
		}

		[SuppressMessage("Enterprise", "EDI012", Justification = "Path names are exempt")]
		protected virtual DialogResult GetFileToImport(out string filePathToImport)
		{
			filePathToImport = "";

			using (var openFileDialog = new ZOpenFileDialog
			{
				CheckFileExists = true,
				DefaultExt = ".edp",
				Filter = (NoResString)"CargoWise One upgrade packages|*.edp",
				AddExtension = true
			})
			{
				DialogResult result = openFileDialog.ShowDialog();

				if (result == DialogResult.OK)
				{
#if WINZOR
					StartProgressForm();
#endif
					filePathToImport = openFileDialog.UnmappedFileName;
				}

				return result;
			}
		}

#if DEBUG
		protected virtual
#endif
 bool IsManualUpgradeAllowed
		{
			get { return Env.CurrentUser.IsDeveloper; }
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "developer only string")]
		bool ShowManualUpgradeObsoleteMessage()
		{
			bool allowed = IsManualUpgradeAllowed;

			if (!allowed)
			{
				string url = @"http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20091106.pdf";
				string msg = Res.GetString("10eaf603-c5fb-40fa-8bdc-eae56a4fb467", "This function is no longer available.\r\nManual handling of upgrade packages has been removed.\r\n\r\nPress OK to open the Update Note for this change in your web browser.");
				if (DialogResult.OK == Globals.Message.Show(msg, Res.GetString("c9f933e1-f05b-4d5f-a785-34d66b6f2b6a", "Information"), MessageBoxButtons.OKCancel, MessageBoxIcon.Information))
				{
					OpenNoteInWebBrowser(url);
				}
			}
			else
			{
				Globals.Message.ShowWarning("This function is no longer available to ordinary users.\r\nHowever it is still available for CWSupport login.");
			}

			return !allowed;
		}

#if DEBUG
		protected virtual
#endif
 void OpenNoteInWebBrowser(string destinationUrl)
		{
			WebUrlLauncher.Launch(destinationUrl);
		}

#endregion

		protected enum CMRUpdateMethod
		{
			WebChangesFile,
			WebMainFile,
			WebTestingChangesFile,
			WebTestingMainFile,
			File
		}

#endregion

		#region Update Show CheckBoxes

		void SetShowDefaults()
		{
			ReadyCheckBox.Checked = true;
			AppliedCheckBox.Checked = false;
			NotAppliedCheckBox.Checked = false;
			DeletedOrObsoleteCheckBox.Checked = false;
			SetTabsVisibility();
			if (GlbCompany.CurrentCompany == null || GlbCompany.CurrentCompany.GC_RN_NKCountryCode != "AU")
			{
				StmUpgradeTabControl.TabPages.Remove(CMRReferenceFilesTabPage);
				CMRReferenceFilesTabPage.Dispose();
			}
		}

		void SetTabsVisibility()
		{
			if (Env.Security.SystemUpgrade.IsAllowed)
			{
				UpgradesTabGroupBox.Visible = true;
				UpgradesTabNotDisplayedLabel.Visible = false;
			}
			else
			{
				UpgradesTabGroupBox.Visible = false;
				UpgradesTabNotDisplayedLabel.Visible = true;
				UpgradesTabNotDisplayedLabel.Text = Env.Security.SystemUpgrade.ErrorMessageForNotAllowed;
				UpgradesTabNotDisplayedLabel.BringToFront();
			}

			if (Env.Security.CMRReferenceFilesUpgrade.IsAllowed)
			{
				CMRReferenceFilesTabPageGroupBox.Visible = true;
				CMRUpgradesTabNotDisplayedLabel.Visible = false;
			}
			else
			{
				CMRReferenceFilesTabPageGroupBox.Visible = false;
				CMRUpgradesTabNotDisplayedLabel.Visible = true;
				CMRUpgradesTabNotDisplayedLabel.Text = Env.Security.CMRReferenceFilesUpgrade.ErrorMessageForNotAllowed;
				CMRUpgradesTabNotDisplayedLabel.BringToFront();
			}
		}

		void ReadyCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			PerformShowCheckBoxesClick();
		}

		void AppliedCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			PerformShowCheckBoxesClick();
		}

		void NotAppliedCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			PerformShowCheckBoxesClick();
		}

		void DeletedOrObsoleteCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			PerformShowCheckBoxesClick();
		}

		protected void PerformShowCheckBoxesClick()
		{
			DataSource.UpdateFullUpgradesView(ReadyCheckBox.Checked, AppliedCheckBox.Checked, NotAppliedCheckBox.Checked,
				DeletedOrObsoleteCheckBox.Checked, DeletedOrObsoleteCheckBox.Checked);
		}

		#endregion

		#region CMR Reference Files Updater

		#region Update Buttons

		#region Web

		void UpdateFromWebFullButton_Click(object sender, EventArgs e)
		{
			if (UserConfirms(GetPreviousUpdateWarning() + Res.GetString("c0f46c18-11a3-46f9-9408-b749c51c9913", "You have selected a Full Update, which will take at least 5 minutes.")))
			{
				UpdateCMRFiles(CMRUpdateMethod.WebMainFile);
			}
		}

		void UpdateFromWebMainTestingButton_Click(object sender, EventArgs e)
		{
			if (UserConfirms(Res.GetString("937bc302-890b-49ae-8906-7466d44b97b5", "These updates are for industry testing only. Additionally, as this is a Full Update, it will take at least 5 minutes.")))
			{
				UpdateCMRFiles(CMRUpdateMethod.WebTestingMainFile);
			}
		}

		#endregion

		#region File

		internal void UpdateFromFileButton_Click(object sender, EventArgs e)
		{
			UpdateCMRFiles(CMRUpdateMethod.File);
		}

		void BrowseButtonForData_Click(object sender, EventArgs e)
		{
			using (var openFileDialog = new ZOpenFileDialog())
			{
				openFileDialog.Filter = Res.GetString("16bc20c0-b40f-4244-b235-f8a41ef84762", "Update files|*.tar.gz");
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					DataFilePathTextBox.Text = openFileDialog.UnmappedFileName;
				}
			}
		}

		#endregion

		#endregion

		protected void UpdateCMRFiles(CMRUpdateMethod method)
		{
			using (var mutexConnection = CargoWise.Data.Db.NewExtraConnectionToMainDb())
			using (var transactionManager = mutexConnection.BeginTransactionWithManager())
			{
				try
				{
					this.Cursor = Cursors.WaitCursor;

					if (CmrAuReferenceFileUpdateMutex.AcquireUpdateLock(mutexConnection, TimeSpan.Zero))
					{
						DoUpdateCmrFiles(method);
					}
					else
					{
						ShowErrorDialog(Res.GetString("256f7f71-8cf8-49e7-8930-74db9c615100", "Unable to perform the CMR Reference Files update simultaneously with other user or batch processor."));
					}
				}
				finally
				{
					this.Cursor = Cursors.Default;
				}
			}
		}

		void DoUpdateCmrFiles(CMRUpdateMethod method)
		{
			ZBlob file = null;

			try
			{
				switch (method)
				{
					case CMRUpdateMethod.File:
						if (DataFilePathTextBox.Text.LastIndexOf("CHNG") > -1)
						{
							this.Cursor = Cursors.Default;
							Globals.Message.ShowError(Res.GetString("ec80599a-6c1e-408c-8aa8-19f02513d962", "Importing of the 'Change' file for the CMR Reference files is not supported. Please use the 'Main' file only"), Res.GetString("5efe913a-df5f-498b-a71b-d4b2596e8e8a", "Updating of CMR Reference Files"));
						}
						else
						{
							using (var stream = ZOpenFileDialog.OpenFile(DataFilePathTextBox.Text))
							{
								file = stream.ToByteArray();
							}
						}
						break;
					case CMRUpdateMethod.WebMainFile:
						file = new ReferenceFileDownloader(DataSource.Factory).DownloadMainFile().Data;
						break;
					case CMRUpdateMethod.WebTestingMainFile:
						file = new ReferenceFileDownloader(DataSource.Factory).DownloadTestingMainFile().Data;
						break;
				}
				try
				{
					if (file != null)
					{
						ICMRReferenceFileUpgrader upgrader = GetNewReferenceFileUpdater();
						upgrader.ImportData(file);
						new CMRReferenceFileUpdateLog(DataSource.Factory).LogUpdateSuccess();
						ShowHaveBeenUpdatedMessage();
					}

					return;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ShowErrorDialog(ex.Message);
					if (!(ex is Enterprise.Integration.Customs.AU.IInvalidCompressedFileExceptionProvider))
					{
						ErrorReporter.ReportOnce("Error updating Reference Files manually from GUI. CMRUpdateMethod = " + method, ex);
					}
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				ShowErrorDialog(Res.GetString("82c4dd4d-5636-4eee-a694-15a788c008ec", "Access denied reading file: {0}", ex.Message));
			}
			catch (IOException ex)
			{
				ShowErrorDialog(Res.GetString("fdc9be31-9e7a-4cb0-b1a2-94769c8264f9", "Error reading file from disk: {0}", ex.Message));
			}
			catch (NotSupportedException)
			{
				ShowErrorDialog(Res.GetString("d7b88a4f-4a42-4c4d-a0d7-adc563bd5bce", "Error in file name. The file name entered in this box must be on your local computer, not a web address. See the CMR FAQ on CargoWise's web site, {1}, for step-by-step instructions.", "www.cargowise.com"));
			}
			catch (ArgumentException)
			{
				ShowErrorDialog(Res.GetString("46efcf4c-a205-438f-91a1-04f074da9849", "Error in file name. Try the 'Update From Web' buttons before trying to update from a file. See the CMR FAQ on CargoWise's web site, {0}, for step-by-step instructions.", "www.cargowise.com"));
			}
			catch (ReferenceFileDownloaderException ex)
			{
				ShowErrorDialog(ex.Message);
			}

			new CMRReferenceFileUpdateLog(DataSource.Factory).LogUpdateFailure();
		}

		#region Implementation

		protected virtual ICMRReferenceFileUpgrader GetNewReferenceFileUpdater()
		{
			return (ICMRReferenceFileUpgrader)ObjectFactory.Get<Enterprise.Integration.Customs.AU.IImportAndUpdateDataReferenceFileData>();
		}

		void ShowHaveBeenUpdatedMessage()
		{
			Globals.Message.ShowInformation(Res.GetString("8646fbb5-fffe-4521-be78-f1293648faf7", "Reference files have been updated."));
		}

		void ShowErrorDialog(string message)
		{
			Globals.Message.ShowError(message, Res.GetString("bee1bc12-9265-4162-8189-b1ec77b0aaf5", "Error updating Reference Files."));
		}

		bool UserConfirms(string message)
		{
			return Globals.Message.Show(Res.GetString("2abf9a8b-3aee-4b29-b055-89b4da4e4b2b", "{0} Are you sure you wish to proceed?", message), Res.GetString("6fa76b44-41c5-4eca-8e77-d3bdf84d7a60", "Reference Files Update Confirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
		}

		string GetPreviousUpdateWarning()
		{
			CMRReferenceFileUpdateLog log = new CMRReferenceFileUpdateLog(DataSource.Factory);
			log.LoadLastSuccessfulUpdate();
			var lastUpdated = log.SuccessfulUpdateFileTimeStamp;
			if (lastUpdated.ToZDateTime() > ZDateTimeOffset.Now.Date)
			{
				return Res.GetString("be50efca-ba9c-43b1-b803-35aa1d3f60f1", "The Reference Files were already updated today {0}.", lastUpdated) + "\r\n";
			}
			else
			{
				return "";
			}
		}
		#endregion

		#endregion

		protected virtual void StartProgressForm()
		{
			progressFormCancelled = false;
			progressForm.Cancelled += new EventHandler(delegate
			{ progressFormCancelled = true; });
			progressForm.Start();
		}
	}
}
