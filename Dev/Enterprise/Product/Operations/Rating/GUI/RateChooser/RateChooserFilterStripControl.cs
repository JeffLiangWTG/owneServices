using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.AutoRating.RatesLoad;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class RateChooserFilterStripControl : ZFilterStripBaseControl
	{
		public RateChooserFilterStripControl()
		{
			InitializeComponent();
		}

		public RateChooserFilterStripControl(FilterStripBusinessObject filterBusinessObject)
			: base(filterBusinessObject)
		{
			InitializeComponent();
		}

		public RateChooserFilterStripControl(RateChooserViewModel viewModel, FilterStripBusinessObject filterBusinessObject = null)
			: this(filterBusinessObject ?? new RateChooserFilterStripBusinessObject(viewModel.Model.Criteria))
		{
			ViewModel = viewModel;
		}

		/// <summary>
		/// Prevent the default saved layout from being automatically applied
		/// </summary>
		protected override bool ShouldInvokeFilterStrips => false;

		RateChooserViewModel ViewModel { get; }

		bool isDefaulted;

		protected override void AddAlwaysVisibleFilterStrips()
		{
			// Called whenever the filter strips are initialized/reset.
			// Not called if the user Loads an existing layout

			// Add the AlwaysVisible strips.
			// In our case, the always visible filters are those that have defaults.
			// See RateChooserFilterStripBusinessObject.ApplyDefaults.
			base.AddAlwaysVisibleFilterStrips();

			DefaultFilters();
		}

		#region public (only for testing)
#if DEBUG
		public
#endif
		#endregion
		void DefaultFilters()
		{
			var criteria = ViewModel?.Model?.Criteria;
			if (criteria == null || isDefaulted)
			{
				return;
			}

			isDefaulted = true;
			var filterBiz = (RateChooserFilterStripBusinessObject)FilterBusinessObject;

			filterBiz.LocationFilter.Property1 = criteria.DefaultFilterValueForOriginCode;
			filterBiz.LocationFilter.Property2 = criteria.DefaultFilterValueForDestinationCode;

			if (criteria.PossibleServiceProviders != null)
			{
				foreach (var serviceProviderOrg in criteria.PossibleServiceProviders)
				{
					var serviceProviderStrip = AddStrip(RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider, false);
					var serviceProviderFiler = (ModuleGuidFilter)serviceProviderStrip.CurrentModuleFilter;
					serviceProviderFiler.Property = serviceProviderOrg.PK;
				}

				var carrierServiceLevel = ViewModel.Model.GetUniversalCarrierServiceLevel(criteria.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier), criteria.PossibleServiceProviders);
				if (!carrierServiceLevel.IsEmpty)
				{
					filterBiz.CarrierServiceLevelFilter.Property = carrierServiceLevel;
					AddStrip(filterBiz.CarrierServiceLevelFilter.Description, false);
				}
			}

			AddTextStrips(filterBiz, RateEntryFilterUtility.Constants.Codes.CarrierContractNumber, criteria.CarrierContractNumbers.Where(x => !x.IsEmpty));

			if (!criteria.NamedAccount.IsEmpty)
			{
				AddTextStrips(filterBiz, RateEntryFilterUtility.Constants.Codes.NamedAccount, new ZString[] { criteria.NamedAccount });
			}

			// we introduced new logic in  [WI00592544 - [CS][CG][CW][RS] Autorating Date Filtering Enhancement] and use contract and carriers to calculate effective date
			// so we should do it after populating contract and carriers filters
			var effectiveDate = WiseRatesQueryBuilder.GetEffectiveDate(criteria, contractNumbersFromFilters: filterBiz.ContractNumbersFromFilters, carriersFromFilters: filterBiz.Carriers);
			filterBiz.EffectiveOnFilter.Property1 = effectiveDate.IsEmpty
				? ZDateTime.Today
				: effectiveDate;
		}

		void AddTextStrips(RateChooserFilterStripBusinessObject filterBiz, ZString description, IEnumerable<ZString> values)
		{
			foreach (var text in values)
			{
				var strip = filterBiz.FilterStrips.AddNew();
				strip.FilterDescription = description;
				var filter = ((ModuleTextFilter)strip.CurrentModuleFilter);
				filter.Property = text;
				AddFilterStrip(NewZFilterStrip(), strip);
			}
		}

		FilterStrip AddStrip(ZString description, bool isReadonly = true)
		{
			var strip = FilterBusinessObject.FilterStrips.AddNew();
			strip.FilterDescription = description;
			strip.ReadOnly = isReadonly;
			AddFilterStrip(NewZFilterStrip(), strip);
			return strip;
		}

		IEnumerable<string> ErrorsOrWarnings;
		string RawData;

		public void SetErrorsAndWarnings(IEnumerable<string> errorsOrWarnings, string rawData)
		{
			ErrorsOrWarnings = errorsOrWarnings;
			bool isVisible = errorsOrWarnings.Any(s => !string.IsNullOrEmpty(s));
			if (isVisible)
			{
#if !WINZOR
				RatesSearchErrorsOrWarningsLink.Text = WiseRatesSearchResultViewModel.GetMultilingualRatesSearchErrorsOrWarnings(errorsOrWarnings);
#endif
			}
			RatesSearchErrorsOrWarningsLink.Visible = isVisible;

			RawData = rawData;
			RawDataLinkLabel.Visible = !string.IsNullOrEmpty(rawData);
#if !WINZOR
			RawDataLinkLabel.Text = WiseRatesSearchResultViewModel.GetMultilingualRawData();
#endif
		}

		void RatesSearchErrorsOrWarningsLink_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			var text = new ZStringBuilder(ErrorsOrWarnings);
			Globals.Message.ShowInformation(text.ToStringWithNewLineBetweenAppends(), RatesSearchErrorsOrWarningsLink.Text);
		}

		void RawDataLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			WiseRatesGUIHelper.ViewJSONWithDeveloperAuthentication(RawData);
		}
	}
}
