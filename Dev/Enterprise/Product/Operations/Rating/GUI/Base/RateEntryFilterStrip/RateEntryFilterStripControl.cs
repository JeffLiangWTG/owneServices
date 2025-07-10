using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Module;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using static Enterprise.MasterFiles.Module.RefCommodityCodeFilterBusinessObject;

namespace Enterprise.Rating.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class RateEntryFilterStripControl : ZUserControl, IReadOnlyToggleControl
	{
		public RateEntryFilterStripControl()
		{
			InitializeComponent();
			this.SkipSettingChildControlReadOnly = true;
		}

		const string FilterCategoryName = "FilterCategory";

		RateEntryStripControl stripControl;
		RatingHeader ratingHeader;
		readonly Dictionary<string, RateEntryStripControl> strips = new Dictionary<string, RateEntryStripControl>();

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			DataBindings.Clear();
			base.SetDataBinding(dataSource, dataMember);
			ratingHeader = dataSource as RatingHeader;

			if (ratingHeader != null)
			{
				DataBindings.Add(new KBinding(FilterCategoryName, ratingHeader, RatingHeader.Schema.SelectedFilterCategory));
			}
		}

		public void SetRateEntryDefaultFilters(RateEntryFilterValue filterValues)
		{
			var filterBizo = stripControl.FilterBusinessObject;

			stripControl.ResetFilterStripsForAddingDefaults();

			SetBasicDefaultFilters(filterValues, filterBizo);
			SetLocationDefaultFilters(filterValues, filterBizo);
			SetContainerDefaultFilters(filterValues, filterBizo);

			stripControl.FirePerformSearch();
		}

		void SetContainerDefaultFilters(RateEntryFilterValue filterValues, FilterStripBusinessObject filterBizo)
		{
			if (!filterValues.ContainerType.IsEmpty || !filterValues.ContainerCode.IsEmpty)
			{
				var stripContainerType = stripControl.AddNewFilterStrip();
				stripContainerType.CurrentDataItem.FilterDescription = RateEntryFilterUtility.Constants.Codes.ContainerType;
				var containerTypeFilter = filterBizo[RateEntryFilterUtility.Constants.Codes.ContainerType] as ModuleGuidFilter;

				if (!filterValues.ContainerType.IsEmpty)
				{
					containerTypeFilter.SelectedFilters.AddTextFilterStrip(RefContainerFilterBusinessObject.Constants.Codes.ContainerType, filterValues.ContainerType);
				}
				else
				{
					containerTypeFilter.SelectedFilters.AddTextFilterStrip(RefContainerFilterBusinessObject.Constants.Codes.ContainerCode, filterValues.ContainerCode);
				}

				containerTypeFilter.IsActive = true;
				containerTypeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.FiltersMatch;
			}
		}

		void SetLocationDefaultFilters(RateEntryFilterValue filterValues, FilterStripBusinessObject filterBizo)
		{
			if (!filterValues.Origin.IsEmpty || !filterValues.Destination.IsEmpty)
			{
				var stripLocation = stripControl.AddNewFilterStrip();
				stripLocation.CurrentDataItem.FilterDescription = RateEntryFilterUtility.Constants.Codes.OriginDestination;
				var locationFilter = filterBizo[RateEntryFilterUtility.Constants.Codes.OriginDestination] as ModuleLocationFilter;
				locationFilter.IsActive = true;
				locationFilter.Property1 = filterValues.Origin;
				locationFilter.Property2 = filterValues.Destination;
			}
		}

		void SetBasicDefaultFilters(RateEntryFilterValue filterValues, FilterStripBusinessObject filterBizo)
		{
			var stripCarrierContractNumber = stripControl.AddNewFilterStrip();
			stripCarrierContractNumber.CurrentDataItem.FilterDescription = RateEntryFilterUtility.Constants.Codes.CarrierContractNumber;
			var carrierContractNumberFilter = filterBizo[RateEntryFilterUtility.Constants.Codes.CarrierContractNumber] as ModuleTextFilter;
			carrierContractNumberFilter.IsActive = true;
			carrierContractNumberFilter.Property = filterValues.ContractNumber;

			var stripTransportMode = stripControl.AddNewFilterStrip();
			stripTransportMode.CurrentDataItem.FilterDescription = RateEntryFilterUtility.Constants.Codes.TransportMode;
			var transportModeFilter = filterBizo[RateEntryFilterUtility.Constants.Codes.TransportMode] as ModuleTextFilter;
			transportModeFilter.IsActive = true;
			transportModeFilter.Property = filterValues.TransportMode;

			var stripStartDate = stripControl.AddNewFilterStrip();
			stripStartDate.CurrentDataItem.FilterDescription = RateEntryFilterUtility.Constants.Codes.StartDate;
			var startDateFilter = filterBizo[RateEntryFilterUtility.Constants.Codes.StartDate] as ModuleDateFilter;
			startDateFilter.IsActive = true;
			startDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			startDateFilter.Property1 = filterValues.StartDate;

			var stripEndDate = stripControl.AddNewFilterStrip();
			stripEndDate.CurrentDataItem.FilterDescription = RateEntryFilterUtility.Constants.Codes.EndDate;
			var endDateFilter = filterBizo[RateEntryFilterUtility.Constants.Codes.EndDate] as ModuleDateFilter;
			endDateFilter.IsActive = true;
			endDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			endDateFilter.Property2 = filterValues.ExpiryDate;

			AddAllowHazardousFilterStrip(filterValues, filterBizo);
		}

		public void AddAllowHazardousFilterStrip(RateEntryFilterValue filterValues, FilterStripBusinessObject filterBizo)
		{
			if (!filterValues.AllowHazardousCommodity)
			{
				var stripCommodityType = stripControl.AddNewFilterStrip();
				stripCommodityType.CurrentDataItem.FilterDescription = RateEntryFilterUtility.Constants.Codes.CommodityCode;
				stripCommodityType.CurrentDataItem.OrCategory = FilterOrCategory.Green;
				var stripCommodityFilter = filterBizo[RateEntryFilterUtility.Constants.Codes.CommodityCode] as ModuleNkFilter;
				stripCommodityFilter.IsActive = true;
				stripCommodityFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

				var stripNonHazCommodity = stripControl.AddNewFilterStrip();
				stripNonHazCommodity.CurrentDataItem.FilterDescription = RateEntryFilterUtility.Constants.Codes.CommodityCode;
				stripNonHazCommodity.CurrentDataItem.OrCategory = FilterOrCategory.Green;
				var nonHazCommodityFilter = filterBizo[RateEntryFilterUtility.Constants.Codes.CommodityCode + " (1)"] as ModuleNkFilter;

				var isNotHaz = nonHazCommodityFilter.SelectedFilters.AddTextFilterStrip(RefCommodityCodeFilterBusinessObject.FilterCodes.IsHazardous, filterValues.CommodityCode);
				isNotHaz.IsActive = true;
				isNotHaz.Property = IsHazardousConstants.Code.STD;

				// The nonHazCommodityFilter.ComparisonOperator has to be set after
				// the nonHazCommodityFilter.SelectedFilters are done changing so
				// that the correct string appears in the UI.
				// So that it says: "1 filters applied"
				nonHazCommodityFilter.IsActive = true;
				nonHazCommodityFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.FiltersMatch;
			}
		}

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<RateEntryFilterStripControl>()
			.Property(FilterCategoryName, ZString.Empty, false)
			.Result;
		}

		// This is set by DataBinding
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		public ZString FilterCategory
		{
			get { return filterCategory; }
			set
			{
				if (filterCategory != value)
				{
					filterCategory = value;

					if (stripControl != null)
					{
						stripControl.Hide();
					}

					stripControl = FindOrCreateStrip();

					if (stripControl != null)
					{
						if (!Controls.Contains(stripControl))
						{
							Controls.Add(stripControl);
						}

						stripControl.HorizontalScroll.Maximum = 0;
						stripControl.AutoScroll = false;
						stripControl.VerticalScroll.Visible = false;
						stripControl.AutoScroll = true;
						stripControl.Show();
						var collection = stripControl.Collection;
						if (collection.Count == 0)
						{
							stripControl.FirePerformSearch();
						}

						if (filterCategory == RatingConstants.RateCategory.SummaryRatesCategory && ratingHeader != null)
						{
							ratingHeader.SummaryRateEntries.LoadAndSortForGUI();
						}
					}
				}
			}
		}
		string filterCategory;

		RateEntryStripControl FindOrCreateStrip()
		{
			RateEntryStripControl result = null;
			if (!string.IsNullOrWhiteSpace(FilterCategory) && ratingHeader != null && !strips.TryGetValue(FilterCategory, out result))
			{
				var collection = ratingHeader.EntryCollections[FilterCategory].LazyLoadingCollection;
				result = new RateEntryStripControl(collection);
				strips.Add(FilterCategory, result);
			}

			return result;
		}

		bool IReadOnlyToggleControl.ReadOnly { get; set; }
	}
}

