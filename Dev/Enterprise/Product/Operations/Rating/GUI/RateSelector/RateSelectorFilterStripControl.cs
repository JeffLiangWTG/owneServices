using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.AutoRating.RatesLoad;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.GUI.RateSelector
{
	public partial class RateSelectorFilterStripControl : ZFilterStripBaseControl
	{
		public RateSelectorFilterStripControl(RateSelectorFilterStripBusinessObject filterStripBusinessObject)
			: base(filterStripBusinessObject)
		{
			InitializeComponent();

			SetDefaults();

			filterStripBusinessObject.ModuleFilterIsActive += OnModuleFilterIsActive;
		}

		protected override void UnhookEventsOnFilterBizO()
		{
			base.UnhookEventsOnFilterBizO();

			if (FilterBusinessObject != null)
			{
				FilterBusinessObject.ModuleFilterIsActive -= OnModuleFilterIsActive;
			}
		}

		public void StartSearching()
		{
			Enabled = false;
		}

		public void DoneSearching()
		{
			Enabled = true;
			IsSearchingWithFindDropButton = false;
		}

		protected override bool ShouldInvokeFilterStrips => false;

		protected new RateSelectorFilterStripBusinessObject FilterBusinessObject =>
			(RateSelectorFilterStripBusinessObject)base.FilterBusinessObject;

		void SetDefaults()
		{
			var defaults = new FilterBusinessObjectDefaults();
			var criteria = FilterBusinessObject.OriginalCriteria;
			IEnumerable<OrgWithSource> carriers = null;

			if (criteria.PossibleServiceProviders != null)
			{
				var possibleCarrierPKs = criteria.PossibleServiceProviders.Select(x => x.PK).ToList();
				carriers = FilterBusinessObject.Factory
					.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, possibleCarrierPKs))
					.OrderBy(x => possibleCarrierPKs.IndexOf(x.PK))
					.Select(x => OrgWithSource.New(x, new List<string> { FilterBusinessObject.OrgSourceText }))
					.ToList();
			}

			var effectiveDate = WiseRatesQueryBuilder.GetEffectiveDate(criteria, carriersFromFilters: carriers);

			// Effective Date
			defaults.Add(new FilterBusinessObjectDefault(
				RateEntryFilterUtility.Constants.Codes.EffectiveOn,
				"Property1",
				effectiveDate.IsEmpty ? ZDateTime.Today : effectiveDate));

			// Origin
			defaults.Add(new FilterBusinessObjectDefault(
				RateEntryFilterUtility.Constants.Codes.OriginDestination,
				"Property1",
				criteria.DefaultFilterValueForOriginCode));

			// Destination
			defaults.Add(new FilterBusinessObjectDefault(
				RateEntryFilterUtility.Constants.Codes.OriginDestination,
				"Property2",
				criteria.DefaultFilterValueForDestinationCode));

			FilterBusinessObject.SetExternalDefaults(defaults);
		}

		/// <summary>
		/// When searching with a saved template, the strips are loaded from XML. We need to mark it so that the ModuleFilter.IsActiveChanged
		/// event handler can skip creating strips from job's values.
		/// </summary>
		public override void ToolStripFindDropButton_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			base.ToolStripFindDropButton_DropDownItemClicked(sender, e);
			IsSearchingWithFindDropButton = true;
		}
		bool IsSearchingWithFindDropButton;

		protected override void AddAlwaysVisibleFilterStrips()
		{
			base.AddAlwaysVisibleFilterStrips();

			// just add the first strip, the event handler below does the rest, including filling the values
			var carrierFilterStrip = AddNewFilterStrip();
			carrierFilterStrip.CurrentDataItem.FilterDescription = RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider;

			var serviceLevelFilterStrip = AddNewFilterStrip();
			serviceLevelFilterStrip.CurrentDataItem.FilterDescription = RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel;

			if (!string.IsNullOrEmpty(FilterBusinessObject.PaymentTermsFromJob))
			{
				var paymentTermFilterStrip = AddNewFilterStrip();
				paymentTermFilterStrip.CurrentDataItem.FilterDescription = RateEntryFilterUtility.Constants.Codes.PaymentTerm;
			}

			if (FilterBusinessObject.UniversalCommodityGroupsFromJob.Any())
			{
				var universalCommodityGroupFilterStrip = AddNewFilterStrip();
				universalCommodityGroupFilterStrip.CurrentDataItem.FilterDescription = RateEntryFilterUtility.Constants.Codes.UniversalCommodityGroup;
			}

			if (FilterBusinessObject.ContractNumbersFromJob.Any())
			{
				var contractNumberFilterStrip = AddNewFilterStrip();
				contractNumberFilterStrip.CurrentDataItem.FilterDescription = RateEntryFilterUtility.Constants.Codes.CarrierContractNumber;
			}

			IsInitialized = true;
		}
		bool IsInitialized;

		/// <summary>
		/// Handle event IsActiveChanged propagated from module filters to RateSelectorFilterStripBusinessObject
		/// then, when it is active, to this RateSelectorFilterStripControl.
		/// </summary>
		/// <param name="sender">RateSelectorFilterStripBusinessObject</param>
		/// <param name="e">ModuleFilter.IsActiveChangedEventArgs containing the ModuleFilter initiated the event</param>
		void OnModuleFilterIsActive(object sender, ModuleFilter.IsActiveChangedEventArgs e)
		{
			var filter = e.ModuleFilter;
			if (filter == null || IsSearchingWithFindDropButton)
			{
				return;
			}

			SuspendLayout();

			if (TrySetDefaultValueAndReturnNumberOfRemainingOnesGeneric(filter) > 0)
			{
				// there are still more values to display? Add another strip for next value
				var strip = FilterBusinessObject.FilterStrips.AddNew();
				// setting FilterDescription will set IsActive of the new strip to true
				// then the IsActiveChanged event is propagated from new module filter
				// then we set the value for the strip with above method
				strip.FilterDescription = filter.OriginalCode;
				AddFilterStrip(NewZFilterStrip(), strip);
			}

			PropertyInfoRefreshBinding(filter);

			ResumeLayout();
			PerformLayout();
		}

		/// <summary>
		/// Set default value from job to a newly active strip associating with module filter and return the remaining number
		/// of values to add to strip control.
		/// </summary>
		/// <typeparam name="T">WiseRatesModuleTextFilter or ModuleGuidFilter</typeparam>
		/// <param name="filter">Module filter</param>
		int TrySetDefaultValueAndReturnNumberOfRemainingOnesGeneric<T>(T filter)
		{
			switch (filter)
			{
				case WiseRatesModuleTextFilter textFilter:
					return TrySetDefaultValueAndReturnNumberOfRemainingOnes(textFilter);
				case ModuleGuidFilter guidFilter:
					return TrySetDefaultValueAndReturnNumberOfRemainingOnes(guidFilter);
				default:
					return 0;
			}
		}

		int TrySetDefaultValueAndReturnNumberOfRemainingOnes(WiseRatesModuleTextFilter filter)
		{
			var moduleFilterCode = filter.OriginalCode;

			var displayingValues = Enumerable.Empty<string>();
			var initialValues = Enumerable.Empty<string>();

			if (moduleFilterCode == RateEntryFilterUtility.Constants.Codes.UniversalCommodityGroup && !IsInitialized)
			{
				displayingValues = FilterBusinessObject.UniversalCommodityGroupsFromFilters;
				initialValues = FilterBusinessObject.UniversalCommodityGroupsFromJobPlusGeneralAndNotClassified;
			}
			else if (moduleFilterCode == RateEntryFilterUtility.Constants.Codes.CommodityCode && !IsInitialized)
			{
				initialValues = Enumerable.Empty<string>();

				if (!string.IsNullOrEmpty(FilterBusinessObject.CommodityCodeFromFilter))
				{
					displayingValues = new[] { FilterBusinessObject.CommodityCodeFromFilter };
				}
			}
			else if (moduleFilterCode == RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel)
			{
				displayingValues = FilterBusinessObject.CarrierServiceLevelsFromFilters;
				initialValues = FilterBusinessObject.UniversalCarrierServiceLevelsFromJob;
			}
			else if (moduleFilterCode == RateEntryFilterUtility.Constants.Codes.CarrierContractNumber)
			{
				displayingValues = FilterBusinessObject.ContractNumbersFromFilters;
				initialValues = FilterBusinessObject.ContractNumbersFromJob;
			}
			else if (moduleFilterCode == RateEntryFilterUtility.Constants.Codes.PaymentTerm)
			{
				displayingValues = FilterBusinessObject.PaymentTermsFromFilters;
				initialValues = string.IsNullOrWhiteSpace(FilterBusinessObject.PaymentTermsFromJob)
					? Enumerable.Empty<string>()
					: new[] { FilterBusinessObject.PaymentTermsFromJob };
			}

			var valuesToDisplay = initialValues.Except(displayingValues).ToArray();
			var valueToDisplay = valuesToDisplay.FirstOrDefault();

			if (!string.IsNullOrWhiteSpace(valueToDisplay))
			{
				filter.Property = valueToDisplay;
			}

			return valuesToDisplay.Length - 1;
		}

		int TrySetDefaultValueAndReturnNumberOfRemainingOnes(ModuleGuidFilter filter)
		{
			if (filter.OriginalCode != RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider)
			{
				return 0;
			}

			var displayingValues = FilterBusinessObject.Carriers
				.Select(x => x.Org.PK);
			var initialValues = FilterBusinessObject.JobServiceProviders
				.Select(x => x.PK);

			var valuesToDisplay = initialValues.Except(displayingValues).ToArray();
			var valueToDisplay = valuesToDisplay.FirstOrDefault();

			if (!valueToDisplay.IsEmpty)
			{
				filter.Property = valueToDisplay;
			}

			return valuesToDisplay.Length - 1;
		}

		static void PropertyInfoRefreshBinding<T>(T filter)
		{
			switch (filter)
			{
				case WiseRatesModuleTextFilter textFilter:
					textFilter.PropertyInfo.RefreshBinding();
					break;

				case ModuleGuidFilter guidFilter:
					guidFilter.PropertyInfo.RefreshBinding();
					break;
			}
		}
	}
}
