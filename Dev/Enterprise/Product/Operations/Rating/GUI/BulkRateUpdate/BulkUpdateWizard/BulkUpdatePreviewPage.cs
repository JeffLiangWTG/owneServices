namespace Enterprise.Rating.GUI
{
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.GUI;

	public partial class BulkUpdatePreviewPage : WizardPageStep
	{
		public BulkUpdatePreviewPage()
		{
			InitializeComponent();

			SetGridColumns();
		}

		#region Implementation

		public override void NotifyActivated(WizardForm wizard)
		{
			base.NotifyActivated(wizard);

			wizard.PageHeaderTitle = Res.GetString("2411d189-2958-4711-9575-607a781e4918", "Preview");
			wizard.PageHeaderDescription = Res.GetString("0429119a-08e7-4b02-8b82-92dcb65e5a9d", "Please note only the first 50 applicable rates and their resulting changes will be displayed.");

			Updater.UpdateFirstRateEntriesBatch();
			ShowWarningIfNecessaryAfterLoadingPreviewEntries();
		}

		void ShowWarningIfNecessaryAfterLoadingPreviewEntries()
		{
			var warning = ZString.Empty;
			var previewResultsCount = Updater.PreviewEntries.Count;
			if (previewResultsCount == 0)
			{
				warning = Res.GetString("a15eb190-a54b-41d8-a8ce-ab642ee08199", "No changes will be made as this action is not applicable to any of the selected {0} rate trade lane(s).", Updater.Entries.Count);
			}
			else if (Updater.HasAdditionalBatchesToProcess)
			{
				warning = Res.GetString("edd52201-40d6-4180-8ac0-79138cdc2d30", "Only {0} of {1} results will be shown for performance reasons.", previewResultsCount, Updater.AllEntriesToUpdateCount);
			}

			if (!warning.IsEmpty)
			{
				Globals.Message.ShowWarning(warning);
			}
		}

		public override void NotifyLeaving(WizardSteppingEventArgs args)
		{
			base.NotifyLeaving(args);

			if (args.MovementDirection != WizardSteppingEventArgs.Direction.Forward)
			{
				return;
			}

			var message = GetErrorMessageFromSaving();

			if (!string.IsNullOrEmpty(message))
			{
				args.Cancel = true;
				Globals.Message.ShowError(message, Res.GetString("06d22395-161f-40ab-9f56-e5a1e51e213b", "Update Status"));
			}
		}

		void SetGridColumns()
		{
			var columns = new BulkUpdateGUIInfo().Columns;
			for (int i = 1; i < columns.Count; i++)
			{
				PreviewEntriesGrid.ColumnStyles.Add(columns[i]);
			}
		}

		string GetErrorMessageFromSaving()
		{
			using (ProgressForm progressForm = new ProgressForm())
			{
				progressForm.ShowCancelButton = false;
				progressForm.Status = Res.GetString("e625c4f4-1314-4d3e-88f9-3014b653bec9", "Updating {0} Rate Entries...", Updater.AllEntriesToUpdateCount);
				Updater.ProcessingProgressed += (s, e) =>
					{
						if (e.PercentComplete < 100)
						{
							progressForm.PercentComplete = e.PercentComplete;
							progressForm.Show();
						}
					};

				var result = Updater.TrySaveRateEntryInBatches();

				progressForm.Hide();

				return result;
			}
		}

		#endregion
	}
}

