using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	///<summary>
	/// For internal use within RateEntryFilterStripControl only. Do not add to any other control.
	///</summary>
	[SuppressFormsLocalizedTest]
	public partial class RateEntryStripControl : ZFilterStripCommonControl
	{
		public RateEntryStripControl(RateEntryCollection collection)
			: base(new RateEntryFilterStripBusinessObject(collection))
		{
			Collection = collection;

			InitializeComponent();
			this.PerformSearch += RateEntryStripControl_PerformSearch;
		}

		protected override ZFilterStrip NewZFilterStrip()
			=> new RateEntryFilterStrip();

		void RateEntryStripControl_PerformSearch(object sender, EventArgs e)
		{
			// TODO: IsDataViewOptimisable should be renamed into IsDbOnly once available
			if (Collection.HasChanges && !FilterBusinessObject.Filter.IsDataViewOptimisable)
			{
				Globals.Message.ShowWarning(
					Res.GetString("CA055746-32CA-42BF-844F-ADBA5DD51138", "You have unsaved changes. Please save your changes and try again."));

				return;
			}

			Collection.LoadAndSortForGUI(includeUserFilter: true, allowOneMoreThanMaximumNumber: true);

			var registryMaxNumberOfRecords = SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids;
			int maxNumberOfRecords = registryMaxNumberOfRecords.Value;
			if (!hasMessageBeenShownMaxNumberOfRecords && Collection.Count > maxNumberOfRecords)
			{
				Collection.Remove(Collection[Collection.Count - 1]);
				hasMessageBeenShownMaxNumberOfRecords = true;
				Globals.Message.ShowInformation(
					Res.GetString(
						"92255480-DB56-44DA-BDA6-2BB93AFCA924",
						"Too many records to display. Only the first {0:G} records have been loaded. Configurable in registry {1}",
						maxNumberOfRecords,
						registryMaxNumberOfRecords.Location()));
			}
		}

		bool hasMessageBeenShownMaxNumberOfRecords;

		public RateEntryCollection Collection { get; }

		#region Overrides

		public override IBusinessObjectCollection GridCollection
		{
			get { return Collection; }
		}

		protected override bool CanSaveColumnLayouts
		{
			get
			{
				return false;
			}
		}

		protected override bool ShouldAddEmptyFilterStripOnReset => shouldAddEmptyFilterStripOnReset;
		bool shouldAddEmptyFilterStripOnReset = true;

		protected override void InitialiseGridCore()
		{
			this.Grid.Visible = false;
		}

		protected override ZString UpdateNoteURL
		{
			get { return "http://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20150116.pdf"; } // URL for help
		}

		protected override int MaxFilterStripPanelHeight => ControlDpiScalingHelper.ScaleToCurrentDpiY(92);

		#endregion

		/// <summary>
		/// Resets the filter strips without adding a blank one at the end.
		/// Used when a reset is ocurring so new defaults can be added without having
		/// an ugly empty filter strip at the top.
		/// </summary>
		internal void ResetFilterStripsForAddingDefaults()
		{
			shouldAddEmptyFilterStripOnReset = false;
			base.ResetFilterStrips();
			shouldAddEmptyFilterStripOnReset = true;
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (FilterBusinessObject != null && FilterBusinessObject.AreModuleFiltersLoaded)
				{
					FilterBusinessObject.RunPreSaveValidation();
					if (!FilterBusinessObject.HasErrors)
					{
						FilterBusinessObject.SaveLastUsedLayout();
					}
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

