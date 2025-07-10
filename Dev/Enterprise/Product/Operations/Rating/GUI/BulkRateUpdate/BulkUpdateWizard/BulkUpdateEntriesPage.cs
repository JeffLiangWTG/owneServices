namespace Enterprise.Rating.GUI
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using CargoWise.Types;
	using Enterprise.Rating.Business;
	using Enterprise.ZArchitecture;
	using Enterprise.ZArchitecture.Environment;

	public partial class BulkUpdateEntriesPage : WizardPageStep
	{
		public BulkUpdateEntriesPage()
		{
			InitializeComponent();

			SetGridColumns();

			SelectAllButton.Text = Res.GetString("77db6c98-b0a0-4ec6-b65c-44619abde4e0", "Select All");
			DeSelectAllButton.Text = Res.GetString("461c0cf8-07e9-4f81-9009-7f4c00ecf013", "De-Select All");

			EntriesGrid.Click += new EventHandler(OnEntriesGridClick);
			EntriesGrid.KeyPress += new KeyPressEventHandler(OnEntriesGridKeyPress);
			EntriesGrid.ColourDeciding += new EventHandler<ColourDecidingEventArgs>(OnEntriesGridColourDeciding);
			SelectAllButton.Click += (s, e) => Updater.SelectAllEntries(true);
			DeSelectAllButton.Click += (s, e) => Updater.SelectAllEntries(false);
		}

		#region Fields

		List<ZGuid> selectedEntries;

		#endregion

		#region Implementation

		public override void NotifyActivated(WizardForm wizard)
		{
			base.NotifyActivated(wizard);

			wizard.PageHeaderTitle = Res.GetString("abd41e2b-3347-4cae-9a05-884e98832069", "Results");
			wizard.PageHeaderDescription = Res.GetString("809a7be6-6931-4178-a189-198fe96beb1f", "Review results matching the selected criteria");

			Updater.LoadEntries();

			if (selectedEntries != null)
			{
				foreach (RateEntry entry in Updater.Entries)
				{
					entry.IncludeInUpdate = selectedEntries.Contains(entry.PK);
				}
			}
		}

		public override void NotifyLeaving(WizardSteppingEventArgs args)
		{
			base.NotifyLeaving(args);

			if (args.MovementDirection == WizardSteppingEventArgs.Direction.Forward &&
				!Updater.Entries.Cast<RateEntry>().ToList().Exists((e) => e.IncludeInUpdate))
			{
				args.Cancel = true;
				Globals.Message.ShowError(Res.GetString("7d286764-a572-4230-bd03-28a848e538af", "You must select at least one entry for update"),
					UnableToProceedMessage);
			}

			selectedEntries = (from entry in Updater.Entries.Cast<RateEntry>()
							   where entry.IncludeInUpdate
							   select entry.PK).ToList();
		}

		void SetGridColumns()
		{
			var columns = new BulkUpdateGUIInfo().Columns;
			for (int i = 0; i < columns.Count; i++)
			{
				EntriesGrid.ColumnStyles.Add(columns[i]);
			}
		}

		void SetIncludeInUpdate(object sender)
		{
			if (sender == EntriesGrid)
			{
				if (EntriesGrid.CurrentRowIndex >= 0 && EntriesGrid.CurrentRowIndex < Updater.Entries.Count)
				{
					RateEntry entry = Updater.Entries[EntriesGrid.CurrentRowIndex];
					entry.IncludeInUpdate = !entry.IncludeInUpdate;
					Updater.ResetActionsLineChargeDescription(entry);
				}
			}
		}

		#endregion

		#region Event Handlers

		void OnEntriesGridClick(object sender, EventArgs e)
		{
			if (EntriesGrid.CurrentRowIndex >= 0)
			{
				int columnNumber = EntriesGrid.CurrentCell.ColumnNumber;
				if (columnNumber >= 0 && columnNumber < EntriesGrid.Columns.Count &&
					EntriesGrid.Columns[columnNumber].ColumnStyle.MappingName == "IncludeInUpdate" &&
					EntriesGrid.GetCurrentCellBounds().Contains(EntriesGrid.PointToClient(Control.MousePosition)))
				{
					SetIncludeInUpdate(sender);
				}
			}
		}

		void OnEntriesGridKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == ' ')
			{
				SetIncludeInUpdate(sender);
			}
		}

		void OnEntriesGridColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			RateEntry entry = e.ObjectAtRow as RateEntry;
			if (entry != null)
			{
				e.Colour = entry.IsExpired() ? Color.PaleGoldenrod : Color.Empty;
			}
		}

		#endregion
	}
}

