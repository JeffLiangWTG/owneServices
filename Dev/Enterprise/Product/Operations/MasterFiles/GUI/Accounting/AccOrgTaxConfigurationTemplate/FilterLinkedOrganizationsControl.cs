using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class FilterLinkedOrganizationsControl : ZUserControl
	{
		public FilterLinkedOrganizationsControl()
		{
			InitializeComponent();
			LinkedOrganizationsControl.ConfigureExportColumnsToExcelMenuItems(false);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (LinkedOrganizationsFilterControl == null && !DesignMode)
			{
				LinkedOrganizationsFilterControl = new LinkedOrganizationsFilterControl(LinkedOrganisationFilterBusinessObject)
				{
					Dock = System.Windows.Forms.DockStyle.Fill,
					BackColor = BackColor,
					Size = FilterPanel.ClientSize,
					Name = "LinkedOrganizationsFilterControl",
				};

				LinkedOrganizationsFilterControl.PerformSearch += LinkedOrganizationsFilter_PerformSearch;
				LinkedOrganizationsFilterControl.FiltersCleared += LinkedOrganizationsFilter_ClearButtonClicked;
				LinkedOrganizationsFilterControl.FilteredGrid.SizeChanged += LinkedOrganizationsFilter_BoundsChanged;
				LinkedOrganizationsFilterControl.FilteredGrid.LocationChanged += LinkedOrganizationsFilter_BoundsChanged;
				FilterPanel.Controls.Add(LinkedOrganizationsFilterControl);

				LinkedOrganizationsFilter_BoundsChanged(LinkedOrganizationsFilterControl.FilteredGrid, EventArgs.Empty);
				ResetStatus();

				LinkedOrganizationsControl.AllowOverlap(LinkedOrganizationsFilterControl);
			}
		}

		#region Events

		void LinkedOrganizationsFilter_PerformSearch(object sender, EventArgs e)
		{
			if (LinkedOrgHasChanges)
			{
				Globals.Message.Show(FilterWarningMessage);
			}
			else
			{
				LoadGrid(new ZQuery(LinkedOrganisationFilterBusinessObject.Filter));
			}
		}

		void LinkedOrganizationsFilter_ClearButtonClicked(object sender, EventArgs e)
		{
			if (LinkedOrgHasChanges)
			{
				Globals.Message.Show(FilterWarningMessage);
			}
			else
			{
				LoadGrid(LinkedOrganisationFilterBusinessObject.Filter);
			}
		}

		void LoadGrid(ZQuery zQuery)
		{
			var hasMaxRowsLoaded = false;
			LinkedOrganizationsControl.SuspendLayout();
			try
			{
				FilterRowCount = 0;
				LinkedOrganizationsControl.BoundedOrgCollection.Factory.RowsLoaded += CheckMaximumRowsLoaded;
				LinkedOrganizationsControl.BoundedOrgCollection.Load(zQuery);
			}
			catch (MaxRowsLoadedException)
			{
				hasMaxRowsLoaded = true;
			}
			finally
			{
				LinkedOrganizationsControl.BoundedOrgCollection.Factory.RowsLoaded -= CheckMaximumRowsLoaded;
				if (hasMaxRowsLoaded)
				{
					LinkedOrganizationsControl.BoundedOrgCollection.ClearResult();
					UpdateRecordsFoundLabelForTooManyRecords();
				}
				else
				{
					UpdateRecordsFoundLabel();
				}
				LinkedOrganizationsControl.ConfigureExportColumnsToExcelMenuItems(true);
				LinkedOrganizationsControl.ResumeLayout();
			}
		}

		void CheckMaximumRowsLoaded(object sender, RowsLoadedEventArgs e)
		{
			if (e.TableOrViewName == OrgHeaderSchema.Constants.TableName)
			{
				FilterRowCount = e.Rows.Length;
				if (FilterRowCount > MaxRowsToLoad)
				{
					throw new MaxRowsLoadedException();
				}
			}
		}

		void LinkedOrganizationsFilter_BoundsChanged(object sender, EventArgs e)
		{
			if (LinkedOrganizationsFilterControl != null)
			{
				LinkedOrganizationsControl.Bounds = LinkedOrganizationsFilterControl.FilteredGrid.Bounds;
			}
		}

		void LinkedOrganizationsControl_OnAttached(object sender, ModuleButtonGridOnAttachEventArgs e)
		{
			foreach (var bo in e.AttachedBusinessObjects.OfType<OrgHeader>().Where(LinkedOrganizationsControl.BoundedOrgCollection.CheckOrgLinkedHasChanged))
			{
				AttachedRows.Add(bo.PK);
			}

			FilterRowCount = LinkedOrganizationsControl.BoundedOrgCollection.Count;
			UpdateRecordsFoundLabel();

			LinkedOrganizationsControl.ConfigureExportColumnsToExcelMenuItems(false);
		}

		void LinkedOrganizationsControl_OnAttaching(object sender, ModuleButtonGridOperationCancelEventArgs args)
		{
			var maxRowsToLoad = MaxRowsToLoad;
			if (AttachedRows.Any() && ModifiedCount >= maxRowsToLoad)
			{
				Globals.Message.Show(Res.GetString("C5B6C336-3724-4464-BAD8-8CD0CA6D1579", "Please Save the form before attaching more organizations to this Template."));
				args.Cancel = true;
			}
		}

		void LinkedOrganizationsControl_OnDetached(object sender, ModuleButtonGridOnDetachedEventArgs e)
		{
			foreach (var bo in e.DetachedBusinessObjects.OfType<OrgHeader>().Where(LinkedOrganizationsControl.BoundedOrgCollection.CheckOrgLinkedHasChanged))
			{
				DetachedRows.Add(bo.PK);
			}

			FilterRowCount = LinkedOrganizationsControl.BoundedOrgCollection.Count;
			UpdateRecordsFoundLabel();

			LinkedOrganizationsControl.ConfigureExportColumnsToExcelMenuItems(false);
		}

		void LinkedOrganizationsControl_BeforeAttach(object sender, ModuleButtonGridOnAttachEventArgs args)
		{
			if (!AttachedRows.Any())
			{
				LinkedOrganizationsControl.BoundedOrgCollection.ClearResult();
			}
		}

		#endregion

		#region Record Counter

		internal void ResetStatus()
		{
			if (LinkedOrganizationsControl.Created)
			{
				AttachedRows.Clear();
				DetachedRows.Clear();
				totalRowsCount = null;
				UpdateRecordsFoundLabel();
			}
		}

		void UpdateRecordsFoundLabel()
		{
			var stringBuilder = new ZStringBuilder();
			var totalRows = TotalRowsCount + AttachedRows.Count - DetachedRows.Count;
			stringBuilder.AppendLine(Res.GetString("9CB6F932-0D13-450D-AFE5-D219B6A6CB35|Displayed", "Found record(s): {0} of {1}", FilterRowCount, totalRows));

			if (ModifiedCount > 0)
			{
				stringBuilder.AppendLine(Res.GetString("9CB6F932-0D13-450D-AFE5-D219B6A6CB35|Modified", "Modified: {0}", ModifiedCount));
			}

			LinkedOrganizationsFilterControl?.UpdateRecordsFoundLabel(stringBuilder.ToString(), false);
		}

		void UpdateRecordsFoundLabelForTooManyRecords()
		{
			var stringBuilder = new ZStringBuilder();
			stringBuilder.AppendLine(Res.GetString("9CB6F932-0D13-450D-AFE5-D219B6A6CB35|TooManyRecords", "Found {0} of {1} records. This is too many records to display", FilterRowCount, TotalRowsCount));

			LinkedOrganizationsFilterControl?.UpdateRecordsFoundLabel(stringBuilder.ToString(), true);
		}

		bool LinkedOrgHasChanges => DetachedRows.Any() || AttachedRows.Any();

		int ModifiedCount => AttachedRows.Union(DetachedRows).Distinct().Count();

		int FilterRowCount;

		HashSet<ZGuid> AttachedRows { get; } = new HashSet<ZGuid>();
		HashSet<ZGuid> DetachedRows { get; } = new HashSet<ZGuid>();

		int MaxRowsToLoad => SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;

		int TotalRowsCount => (totalRowsCount ?? (totalRowsCount = LinkedOrganizationsControl.BoundedOrgCollection.GetTotalRowsCount())).Value;
		int? totalRowsCount;

		#endregion

		#region Implementation

		LinkedOrganizationsFilterBusinessObject LinkedOrganisationFilterBusinessObject => linkedOrganisationFilterBusinessObject ?? (linkedOrganisationFilterBusinessObject = new LinkedOrganizationsFilterBusinessObject());
		LinkedOrganizationsFilterBusinessObject linkedOrganisationFilterBusinessObject;

		LinkedOrganizationsFilterControl LinkedOrganizationsFilterControl;

		readonly string FilterWarningMessage = ResString.GetMultilingualString("35CE88EE-FE9E-4F92-97BF-8966769B0C21", "Linked Organizations have been changed, Please Save the form before applying filter.");

		#endregion
	}
}
