using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.DataTransfer.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.GUI
{
	public partial class CostingForm : RatingForm
	{
		public CostingForm(Costing costing)
			: base(costing)
		{
			InitializeComponent();
			clientInformationUserControl1.SetOrgHeaderLabelToSupplier();

			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);

			InitializeRatesServiceMenuItems();

			Load += OnLoaded;
			Activated += SetupFilterValuesAndTab;
			ValidatingForSave += ValidateJustBeforeSaving;
		}

		protected override ZTabControl TopLevelTabControl
		{
			get { return costingTabControl1.TopLevelTabControl; }
		}

		protected override void SetReadOnlyIncludingChildren()
		{
			this.SetReadOnlyIncludingChildren(new[] { rateEntryFilterStripControl.Name }.ToList());
		}

		#region Data Transfer

		protected override IValueObjectDataAdapter ValueObjectDataAdapter
		{
			get { return new CostingValueObjectDataAdapter(); }
		}

		#endregion

		#region Support the setting of default filters.

		void OnLoaded(object sender, EventArgs e)
		{
			formLoaded = true;
			SetupFilterValuesAndTab(sender, e);
		}

		void SetupFilterValuesAndTab(object sender, EventArgs args)
		{
			if (!formLoaded)
			{
				return;
			}

			var filterValues = RateEntryFilterValueCache.Instance[CurrentHeader.PK];
			if (filterValues != null)
			{
				// Note: Changing selected tab resets filters. It needs to be done first
				costingTabControl1.SelectSummaryTabPage();
				rateEntryFilterStripControl.SetRateEntryDefaultFilters(filterValues);
			}
		}

		bool formLoaded;

		#endregion

		void ValidateJustBeforeSaving(object sender, ValidatingForSaveEventArgs e)
		{
			// By validating the TI_ContractNumberLinked we check all the
			// fields that should be correct so the contract linkeage is valid.
			// This is done on all loaded (see usages and tests for FetchAllEntriesFromLocalCache)
			// and changed RateEntries. It's done at saving time to avoid re-validating
			// each time a column value changes.
			//
			// This mainly ensures that the Contract's AllowHazardous and the
			// RateEntry's Commodity's IsHazardous remains consistent. It is
			// necessary as they could have changed in the background
			this.CurrentHeader?
				.FetchAllEntriesFromLocalCache()
				.Where(x => x.HasChanges)
				.ForEach(x => x.Validation.ValidateTI_ContractNumberLinked());
		}

		#region Dispose

		readonly System.ComponentModel.Container components;

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}

