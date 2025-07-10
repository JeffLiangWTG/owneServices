using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI.RateChooser;
using Enterprise.Rating.GUI.RateChooser.ViewModel;
using Enterprise.Rating.GUI.RateSelection;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Core;
using WiseRates.Api.Model;
using WiseRates.Constants;
using WiseRates.Tools;
using static Enterprise.Integration.Forwarding;
using Api = WiseRates.Api;
using NotificationType = CargoWise.ComponentModel.NotificationType;
using RateSelectorViewModels = Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI
{
	public class ChooserRateRow : ViewModelWithNotificationBase
	{
		[SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		public ChooserRateRow(ChooserContainerCommodity containerInfo, ChooserRateEntry rate, IRateChooserImages images, IEnumerable<ChooserContainerCommodityViewModel> tabs)
		{
			Rate = rate;
			ContainerInfo = containerInfo;
			this.images = images;
			this.tabs = tabs;
			notifications.ListChanged += Notifications_ListChanged;

			DisplayPrice = Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed || rate?.WiseRateEntry != null;

			if (rate == null)
			{
				return;
			}

			if (rate.WiseRateEntry != null)
			{
				var allChargesAreValid = rate.AllConvertedCharges.Where(x => !x.Key.IsOptional && x.Key.CustomCategory != WRConstants.ChargeCustomCategory.BOL)
					.All(x => x.Value.IsValidRate());

				if (!allChargesAreValid)
				{
					notifications.AddError(AreInvalidChargesError);
				}
			}

			if (rate.WiseRateEntry?.BookingInfo != null)
			{
				BookingInfo = RateSelectorViewModels.BookingInfoViewModel.PopulateFromRate(rate.WiseRateEntry);
			}

			selectedCalculatedResult = PopulateSelectCalculatedResult();
			UpdateTotalCalculatedCharges();
		}

		#region Contract and Annotation

		internal void PopupContractAndAllocationsAttachForm()
		{
			var effectiveDate = Rate.Filters.EffectiveDate;
			var origin = Rate.Filters.OriginCode;
			var destination = Rate.Filters.DestinationCode;
			var commodity = Rate.Commodity;

			// The rate may not have a service provider when the rate service
			// rate's carrier is not mapped to a CW1 carrier. 
			var serviceProviderPK = Rate.ServiceProvider?.PK ?? ZGuid.Empty;
			var simulator =
				new RateChooserContractsAndAllocationSimulation
				(
					ContractNumber,
					serviceProviderPK,
					origin,
					destination,
					effectiveDate,
					commodity
				);
			simulator.ShowPopup();
		}

		IForwardingConsol GetConsol()
		{
			return Rate.Filters?.OriginalCriteria.AutoRatedFor.OfType<IUniqueConsolProvider>().FirstOrDefault()?.UniqueConsol;
		}

		#endregion

		readonly IRateChooserImages images;

		public ChooserRateEntry Rate { get; }
		public ChooserContainerCommodity ContainerInfo { get; }

		Rate WiseRateEntry => Rate?.WiseRateEntry;
		IRateEntry RateEntry => Rate?.RateEntry;

		readonly IEnumerable<ChooserContainerCommodityViewModel> tabs;

		public string RoutingOrTransitTime
		{
			get
			{
				if (WiseRateEntry != null)
				{
					return Rate.Routing;
				}
				else
				{
					return RateEntry?.TI_TransitTime;
				}
			}
		}
		public bool CWTransitTimeVisibility => WiseRateEntry == null;
		public bool WiseRateRoutingVisibility => WiseRateEntry != null;

		public string ContainerType => ContainerInfo.ContainerRef.RC_Code;
		public string CommodityCode => ContainerInfo.CommodityCode;
		public string ContainerTypeWithCommodityCode => ContainerInfo.ContainerTypeWithCommodityCode;
		public string ContractNumber => Rate?.ContractNumber;
		public bool ContractNumberAllocationHyperlinkVisibility =>
			ObjectFactory.Get<IContractPermissions>().IsAllocationsVisible() &&
			(Rate?.IsFCL() ?? false) &&
			GetConsol() != null;
		public bool ContractNumberIsVisible => !string.IsNullOrEmpty(ContractNumber) && !ContractNumberAllocationHyperlinkVisibility;

		#region ServiceProvider

		public string ServiceProviderText
		{
			get
			{
				if (Rate?.ServiceProvider == null)
				{
					return string.Empty;
				}

				var truncatedName = TruncateWithUnicodeWhitespace(Rate.ServiceProvider.OH_FullName);
				var code = GetOrgCode(Rate.ServiceProvider);

				return truncatedName + " (" + code + ")";
			}
		}

		public string ServiceProviderToolTip =>
			Rate?.ServiceProvider != null
				? ResString.GetMultilingualString(
					"6DAF3788-1748-45ED-A701-6749D6B04C79",
					"Service Provider: {0}",
					Rate.ServiceProvider.NameAndCode)
				: string.Empty;

		string TruncateWithUnicodeWhitespace(string name, int maxLength = 20)
		{
			name = name.TrimWithUnicodeWhitespace();
			if (name.Length <= maxLength)
			{
				return name;
			}

			var whitespaceIndex = name.LastIndexOfAny(new[] { ' ', '\uFEFF', '\u200B' }, maxLength);

			return whitespaceIndex < 0
				? name
				: name.Substring(0, whitespaceIndex);
		}

		string GetOrgCode(OrgHeader org)
		{
			var shippingLine = org.ShippingLine;
			if (shippingLine == null)
			{
				// Organization/CW1 Code, 3rd priority
				return org.OH_Code;
			}

			// SCAC Code, 1st priority
			var code = shippingLine.RSL_StandardCarrierAlphaCode;

			if (code.IsEmpty)
			{
				// C1C Code - always not empty, 2nd priority
				code = shippingLine.RSL_CargoWiseOneCode;
			}

			return code;
		}

		#endregion

		public string TradeLane => tradeLane ?? (tradeLane = BuildTradeLane());
		string tradeLane;
		string BuildTradeLane()
		{
			if (WiseRateEntry != null)
			{
				return Rate.TradeLane;
			}
			else if (Rate != null)
			{
				var entry = RateEntry;
				return string.Join(" > ", new[] { entry.TI_OriginLRC, entry.TI_ViaLRC, entry.TI_DestinationLRC }.Where(x => !x.IsEmpty));
			}
			else
			{
				return string.Empty;
			}
		}

		public string Account => account ?? (account = BuildAccount());
		string account;
		string BuildAccount()
		{
			var wiseEntry = WiseRateEntry;
			if (wiseEntry != null)
			{
				return string.Join(", ", wiseEntry.NamedAccounts);
			}
			else
			{
				return RateEntry?.ControllingCustomer?.OH_Code ?? string.Empty;
			}
		}

		public string Commodities
		{
			get
			{
				var wiseEntry = WiseRateEntry;
				if (wiseEntry != null)
				{
					if (!RateEntry.TI_RH_NKCommodityCode.IsEmpty)
					{
						return RateEntry.TI_RH_NKCommodityCode;
					}
					else
					{
						return wiseEntry.CarrierCommodityInfo?.GroupName ?? string.Empty;
					}
				}
				else
				{
					return RateEntry?.TI_RH_NKCommodityCode ?? string.Empty;
				}
			}
		}

		public string CommodityName => WiseRateEntry?.CarrierCommodityInfo?.GroupName ?? string.Empty;
		public string CommodityType => WiseRateEntry?.CarrierCommodityInfo?.GroupType ?? string.Empty;

		public string CommodityIncluded => commodityIncluded ?? (commodityIncluded = string.Join(", ", WiseRateEntry?.CarrierCommodityInfo?.IncludedCommodities ?? Enumerable.Empty<string>()));
		string commodityIncluded;

		public string CommodityExcluded => commodityExcluded ?? (commodityExcluded = string.Join(", ", WiseRateEntry?.CarrierCommodityInfo?.ExcludedCommodities ?? Enumerable.Empty<string>()));
		string commodityExcluded;

		public bool CommodityInfoVisibility => Rate?.RateProvider == RateChooserViewModel.CargoSphereProviderCode;

		public string CarrierServiceLevel => Rate?.CarrierServiceLevel;
		public string CarrierServiceLevelError => Rate?.CarrierServiceLevelError;
		public bool CarrierServiceLevelErrorVisibility => !(Rate?.CarrierServiceLevelIsMapped ?? true);
		public string ServiceStringOrLevel
		{
			get
			{
				var wiseEntry = WiseRateEntry;
				if (wiseEntry != null)
				{
					return Rate.ServiceString;
				}
				else if (Rate != null)
				{
					return RateEntry.TI_RS_NKServiceLevel_NI;
				}
				else
				{
					return string.Empty;
				}
			}
		}

		public string RateType => Rate?.RateType;

		public string RateType2 => Rate?.RateType2;
		public string RateType2OrConsignor => WiseRateEntry != null ? Rate.RateType2 : RateEntry?.Consignor?.OH_Code.ToString() ?? string.Empty;

		public string LclUnit => Rate?.LclUnit ?? string.Empty;

		public string Vessel => Rate?.Vessel;
		public string VesselOrConsignee => WiseRateEntry != null ? Rate.Vessel : RateEntry?.Consignee?.OH_Code.ToString() ?? string.Empty;

		public string AddOn => Rate?.AddOn;
		public string AddOnOrCarrier => WiseRateEntry != null ? Rate.AddOn : RateEntry?.TransportProvider?.OH_Code.ToString() ?? string.Empty;

		public ZDate EffectiveDate => WiseRateEntry?.StartDate() ?? RateEntry?.TI_RateStartDate ?? ZDate.Empty;
		public ZDate ExpiryDate => WiseRateEntry?.ExpiryDate() ?? RateEntry?.TI_RateEndDate ?? ZDate.Empty;

		public Image ProviderIcon => Rate != null ? images?.GetProviderIcon(Rate.RateProvider) : BrandingFactory.Instance.ProductIcon.ToImage();

		public Image TotalIcon => notifications.HasErrors()
			? RateChooserImageRepository.ErrorIcon
			: notifications.HasWarnings()
				? RateChooserImageRepository.WarningIcon
				: null;

		public bool TotalIconVisibility => TotalIcon != null;

		public string TotalIconToolTip => string.Join(System.Environment.NewLine, notifications.GetUniqueNotifications().Select(x => x.Message));

		public string RateCategory => Rate?.RateCategory;

		public bool DisplayPrice { get; }

#if DEBUG
		// This property is going to be used only for test purposes.
		// So we can expand/collapse the "More Details"  Collapsible Panel on RateChooserCard.
		public bool ChargeDetailVisibility
		{
			get => chargeDetailVisibility;
			set
			{
				if (chargeDetailVisibility != value)
				{
					chargeDetailVisibility = value;
					OnPropertyChanged(nameof(ChargeDetailVisibility));
				}
			}
		}
		bool chargeDetailVisibility;
#endif

		bool IsSpotRate => BookingInfo?.IsSpotRate ?? false;
		public bool VisibleOnSpotRates => IsSpotRate;
		public bool NonVisibleOnSpotRates => !IsSpotRate;
		public bool RemoveVisibility => Rate != null;
		public bool SummaryCardVisibility => Rate != null;
		public bool EmptyCardVisibility => Rate == null;

		public bool SelectRelatedRatesApplyButtonVisibility
		{
			get
			{
				if (tabs == null)
				{
					return false;
				}

				var tabsWithThisRate = tabs.Where(t => t.ContainsRateRelatedTo(Rate)).ToList();
				if (tabsWithThisRate.Count == 1)
				{
					// This is the only tab with this rate. So, there are no tabs we can apply the same rate. So, nothing to apply.
					return false;
				}

				if (tabsWithThisRate.All(t => t.IsSelectedRateRelatedTo(Rate)))
				{
					// This rate is already selected in all possible tabs, so, nothing to apply.
					return false;
				}

				return true;
			}
		}

		public string SelectRelatedRatesText
		{
			get
			{
				if (Rate == null)
				{
					return null;
				}

				if (!Rate.IsFCL())
				{
					// Non FCL rates have no tabs, so, no related rates to select in other tabs
					return null;
				}

				var tabsWithRate = tabs
					.Where(t => t.ContainsRateRelatedTo(Rate))
					.OrderBy(t => t.Rates.Contains(this) ? 0 : 1)
					.ToList();

				if (tabsWithRate.Count <= 1)
				{
					// Only 1 tab has this rate, so, nothing to select in other tabs
					return null;
				}

				var unselectedTabs = tabsWithRate
					.Where(t => !t.IsSelectedRateRelatedTo(Rate))
					.ToList();

				if (unselectedTabs.Count == tabsWithRate.Count)
				{
					var tabNames = string.Join(" & ", unselectedTabs.Select(tab => tab.TabName));
					return Res.GetString("98A6ACCE-3EDD-4604-BF81-7F6A5BAB41FF", "Apply this rate to these containers: {0}.", tabNames);
				}

				if (unselectedTabs.Count == 0)
				{
					var tabNames = string.Join(" & ", tabsWithRate.Select(tab => tab.TabName));
					return Res.GetString("E3754345-5E5A-4359-967B-24A3771C916F", "This rate has been applied to these containers: {0}.", tabNames);
				}

				var selectedTabs = tabsWithRate.Except(unselectedTabs).ToList();
				var selectedTabNames = string.Join(" & ", selectedTabs.Select(tab => tab.TabName));
				var unselectedTabNames = string.Join(" & ", unselectedTabs.Select(tab => tab.TabName));
				return Res.GetString("114FECF8-916B-4237-B64E-208AD5AA80D8", "Applied to {0}. Also apply this rate to: {1}.", selectedTabNames, unselectedTabNames);
			}
		}

		public ChargesViewModel CW1BillOfLadingCharges
		{
			get
			{
				InitializeChargesIfNeeded();
				return cw1BillOfLadingCharges;
			}
		}
		ChargesViewModel cw1BillOfLadingCharges;

		public ChargesViewModel CW1FreightCharges
		{
			get
			{
				InitializeChargesIfNeeded();
				return cw1FreightCharges;
			}
		}
		ChargesViewModel cw1FreightCharges;

		public ChargesViewModel BOLCharges
		{
			get
			{
				InitializeChargesIfNeeded();
				return bolCharges;
			}
		}
		ChargesViewModel bolCharges;

		public LegChargesViewModel InlandCharges
		{
			get
			{
				InitializeChargesIfNeeded();
				return inlandCharges;
			}
		}
		LegChargesViewModel inlandCharges;

		public LegChargesViewModel OceanCharges
		{
			get
			{
				InitializeChargesIfNeeded();
				return oceanCharges;
			}
		}
		LegChargesViewModel oceanCharges;

		public LegChargesViewModel OutlandCharges
		{
			get
			{
				InitializeChargesIfNeeded();
				return outlandCharges;
			}
		}
		LegChargesViewModel outlandCharges;

		public void NotifyPropertyChanged(string propertyName)
		{
			OnPropertyChanged(propertyName);
		}

		public void RefreshBinding()
		{
			NotifyPropertyChanged(nameof(ChooserRateRow.SelectRelatedRatesApplyButtonVisibility));
			NotifyPropertyChanged(nameof(ChooserRateRow.SelectRelatedRatesText));
		}

		void InitializeChargesIfNeeded()
		{
			if (Rate == null || inlandCharges != null || oceanCharges != null || outlandCharges != null || bolCharges != null || cw1BillOfLadingCharges != null || cw1FreightCharges != null)
			{
				return;
			}

			if (Rate.WiseRateEntry != null)
			{
				InitializeWiseCharges();
			}
			else
			{
				InitializeCW1Charges();
			}
		}

		void InitializeCW1Charges()
		{
			var isLCL = Rate.RateEntry.IsLCL();
			var allRateLines = Rate.CalculatedResult.Select(r => r.Line).ToList();

			var bolRateLines = isLCL ? Array.Empty<IRateLine>() : allRateLines.Where(l => l.RateCalculatorType == CalculatorType.Flat).ToArray();
			var freightRateLines = allRateLines.Except(bolRateLines).ToArray();

			var bolChargeViewModels = bolRateLines.Select(l => new ChargeViewModel(l, Rate)).ToArray();
			var freightChargeViewModels = freightRateLines.Select(l => new ChargeViewModel(l, Rate)).ToArray();

			if (bolChargeViewModels.Any())
			{
				cw1BillOfLadingCharges = new ChargesViewModel(ChargesViewModel.ChargesGroup.BOL, Rate.ChooserServices, bolChargeViewModels, isLCL);
			}

			if (freightChargeViewModels.Any())
			{
				cw1FreightCharges = new ChargesViewModel(ChargesViewModel.ChargesGroup.Base, Rate.ChooserServices, freightChargeViewModels, isLCL);
			}
		}

		void InitializeWiseCharges()
		{
			var isLCL = Rate.RateEntry.IsLCL();

			foreach (var chargesByCategory in Rate.AllConvertedCharges.GroupBy(c => c.Key.CustomCategory))
			{
				if (chargesByCategory.Key == WRConstants.ChargeCustomCategory.BOL)
				{
					var charges = chargesByCategory
						.Select(pair => new ChargeViewModel(pair.Key, pair.Value, Rate, true))
						.ToList();

					bolCharges = new ChargesViewModel(ChargesViewModel.ChargesGroup.BOL, Rate.ChooserServices, charges, isLCL: isLCL);
					continue;
				}

				var legChargesViewModel = GetLegCharges(chargesByCategory.Key, chargesByCategory, isLCL);

				switch (chargesByCategory.Key)
				{
					case WRConstants.ChargeCustomCategory.Ocean:
						oceanCharges = legChargesViewModel;
						break;

					case WRConstants.ChargeCustomCategory.Inland:
						inlandCharges = legChargesViewModel;
						break;

					case WRConstants.ChargeCustomCategory.Outland:
						outlandCharges = legChargesViewModel;
						break;
				}
			}

			if (Rate.AllConvertedCharges.Any(c => c.Key.IsOptional))
			{
				Rate.ChargeIsActiveChanged += Rate_ChargeIsActiveChanged;
			}
		}

		LegChargesViewModel GetLegCharges(string chargeCustomCategroy, IEnumerable<KeyValuePair<Charge, WiseLine>> charges, bool isLCL)
		{
			// Charges of the same category must have the same routing as they come from the same leg
			var wiseCharge = charges.First().Key;
			var chargeRouting = wiseCharge.ProviderCustomFields.FirstOrDefault(f => f.Code == WiseRates.Api.Model.Rate.CustomFields.Common.Routing);

			// Charge routing is populated by URS directly from the tradelane the charge belongs to. It is much more reliable way.
			// Rates service populates category instead and then we are trying to find a routing for those category. This approach is more fragile, so,
			// it comes second as a fallback to the first one.
			var routing = chargeRouting != null && !string.IsNullOrEmpty(chargeRouting.Value.ToString())
				? chargeRouting.Value.ToString()
				: GetRouting(chargeCustomCategroy, Rate.WiseRateEntry);

			var baseCharges = new List<ChargeViewModel>();
			var additionalCharges = new List<ChargeViewModel>();

			foreach (var charge in charges)
			{
				var showIsActive = charge.Key.IsOptional;
				var chargeViewModel = new ChargeViewModel(charge.Key, charge.Value, Rate, showIsActive);

				if (showIsActive)
				{
					additionalCharges.Add(chargeViewModel);
				}
				else
				{
					baseCharges.Add(chargeViewModel);
				}
			}

			var chargesViewModels = new List<ChargesViewModel>();

			if (baseCharges.Any())
			{
				chargesViewModels.Add(new ChargesViewModel(ChargesViewModel.ChargesGroup.Base, Rate.ChooserServices, baseCharges, isLCL));
			}

			if (additionalCharges.Any())
			{
				chargesViewModels.Add(new ChargesViewModel(ChargesViewModel.ChargesGroup.Additional, Rate.ChooserServices, additionalCharges, isLCL));
			}

			return new LegChargesViewModel(routing, chargesViewModels);
		}

		string GetRouting(string chargeCategory, Rate rate)
		{
			string customFieldName;

			switch (chargeCategory)
			{
				case WRConstants.ChargeCustomCategory.Inland:
					customFieldName = Api.Model.Rate.CustomFields.CargoSphere.InlandRouting;
					break;

				case WRConstants.ChargeCustomCategory.Outland:
					customFieldName = Api.Model.Rate.CustomFields.CargoSphere.OutlandRouting;
					break;

				case WRConstants.ChargeCustomCategory.Ocean:
					customFieldName = Api.Model.Rate.CustomFields.CargoSphere.OceanRouting;
					break;

				case WRConstants.ChargeCustomCategory.BOL:
					return null;

				default:
					customFieldName = null;
					break;
			}

			var customField = rate.ProviderCustomFields.FirstOrDefault(f => f.Code == customFieldName);
			return customField != null ? customField.Value.ToString() : chargeCategory;
		}

		AutoRateInfoCollection selectedCalculatedResult;

		AutoRateInfoCollection PopulateSelectCalculatedResult()
		{
			var selectedCalculation = new AutoRateInfoCollection(Rate.Factory);

			if (Rate?.CalculatedResult == null)
			{
				return selectedCalculation;
			}

			if (Rate.RateProvider == RateChooserViewModel.CargoSphereProviderCode)
			{
				var wiseLines = Rate.ActiveConvertedCharges.Select(x => x.Value.WiseCharge).ToList();

				foreach (var result in Rate.CalculatedResult)
				{
					if (result.Line is WiseLine wiseLine)
					{
						if (wiseLines.Contains(wiseLine.WiseCharge))
						{
							selectedCalculation.Add(result);
						}
					}
					else
					{
						selectedCalculation.Add(result);
					}
				}

				return selectedCalculation;
			}

			//CW1 Rate
			return Rate.CalculatedResult;
		}

		string _totalCalculatedCharges;
		public string TotalCalculatedCharges
		{
			get
			{
				return _totalCalculatedCharges;
			}
			set
			{
				if (_totalCalculatedCharges != value)
				{
					_totalCalculatedCharges = value;
					OnPropertyChanged(nameof(TotalCalculatedCharges));
				}
			}
		}

		void UpdateTotalCalculatedCharges()
		{
			ZDecimal total = 0;

			if (Rate != null)
			{
				Rate.SelectedCalculatedResult = selectedCalculatedResult;

				if (Rate.SelectedCalculatedResult != null)
				{
					foreach (var result in Rate.SelectedCalculatedResult)
					{
						var rateAmount = Rate.ChooserServices.ConvertToDefaultCurrency(result.Amount, result.Currency);
						if (!rateAmount.IsValid)
						{
							notifications.UniqueAdd(new Notification(NotificationType.Error, MissingExchangeRateError(result.Currency)));
							TotalCalculatedCharges = RateChooserModel.DefaultCurrency;
							return;
						}
						total += rateAmount.Amount;
					}
				}
			}

			TotalCalculatedCharges = RateChooserModel.DefaultCurrency + " " + Rate.ChooserServices.ConvertToCurrentCompanyFormat(total);
		}

		public string TotalCalculatedChargesDescription => GetTotalCalculatedChargeDescription();

		string GetTotalCalculatedChargeDescription()
		{
			var sb = new StringBuilder();

			if (Rate != null)
			{
				Rate.SelectedCalculatedResult = selectedCalculatedResult;

				if (Rate.SelectedCalculatedResult != null)
				{
					foreach (var result in Rate.SelectedCalculatedResult)
					{
						sb.AppendLine($"{result.ChargeCode.AC_Code} {result.Amount} {result.Currency} - {result.SingleLineDescription}"); // not to be translated
					}
				}
			}

			return sb.ToString();
		}

		public string PlainLogs
		{
			get
			{
				var sb = new ZStringBuilder();

				if (Rate?.CalculatedResult != null && Rate.Logger is LoggerDecorator logger)
				{
					foreach (var message in logger.GetLogs().Where(x => !string.IsNullOrEmpty(x)))
					{
						sb.AppendLine(message.Trim());
					}
				}

				AddInformationFromChargesViewModels(sb);

				if (sb.IsEmpty)
				{
					sb.Append(NoWarningError);
				}

				return sb.ToString();
			}
		}

		void AddInformationFromChargesViewModels(ZStringBuilder sb)
		{
			var errors = new HashSet<string>();

			void Add(ChargesViewModel model)
			{
				if (model != null && !model.TotalPriceErrorString.IsNullOrEmpty())
				{
					errors.Add(model.TotalPriceErrorString);
				}
			}

			Add(cw1BillOfLadingCharges);
			Add(bolCharges);
			Add(cw1FreightCharges);
			Add(inlandCharges?.BaseCharges);
			Add(inlandCharges?.AdditionalCharges);
			Add(outlandCharges?.BaseCharges);
			Add(outlandCharges?.AdditionalCharges);
			Add(oceanCharges?.BaseCharges);
			Add(oceanCharges?.AdditionalCharges);

			errors.ForEach(e => sb.AppendLine(e));
		}

		void Rate_ChargeIsActiveChanged(object sender, ChooserRateEntry.ChargeIsActiveChangedEventArgs e)
		{
			var charges = new List<ChargesViewModel>();

			if (InlandCharges != null)
			{
				charges.AddRange(InlandCharges.Charges);
			}

			if (OutlandCharges != null)
			{
				charges.AddRange(OutlandCharges.Charges);
			}

			if (OceanCharges != null)
			{
				charges.AddRange(OceanCharges.Charges);
			}

			if (BOLCharges != null)
			{
				charges.Add(BOLCharges);
			}

			foreach (var chargesViewModel in charges)
			{
				foreach (var chargeViewModel in chargesViewModel.Charges)
				{
					if (object.ReferenceEquals(chargeViewModel.Charge, e.Charge))
					{
						if (chargeViewModel.IsActive != e.IsActive)
						{
							chargeViewModel.IsActive = e.IsActive;
						}

						break;
					}
				}

				chargesViewModel.RecalculateIsAnyActive();
			}

			charges.ForEach(sg => sg.Recalculate());

			UpdateSelectedCalculatedCharges();
		}

		void UpdateSelectedCalculatedCharges()
		{
			selectedCalculatedResult = PopulateSelectCalculatedResult();
			UpdateTotalCalculatedCharges();
			OnPropertyChanged(nameof(TotalCalculatedChargesDescription));
		}

		public MultilingualString TransitTimeLabel => ResString.GetMultilingualString("04D6839F-4D5C-472B-B799-FE25DA1E5E99", "Transit Time:");

		public MultilingualString ContainerTypeLabel => ResString.GetMultilingualString("CC6042CB-6844-463A-8B26-2238113DA910", "Container Type:");
		public MultilingualString CommodityLabel => ResString.GetMultilingualString("DF1E8880-D134-408D-9DF4-AD1BC3BD431A", "Commodity:");
		public MultilingualString CarrierServiceLevelLabel => ResString.GetMultilingualString("E2149E03-EFE8-42E6-B27A-84084BC5AD9B", "Carrier Service Level:");
		public MultilingualString EffectiveDateLabel => ResString.GetMultilingualString("4559E574-642B-451B-8752-5879ABFFCAAA", "Start Date:");
		public MultilingualString ServiceStringLabel => ResString.GetMultilingualString("181CD29C-9A59-4268-81C5-B51828EFBC8F", "Service String:");
		public MultilingualString ServiceLevelLabel => ResString.GetMultilingualString("0CB06C72-0C77-4BBF-A6D2-11F9032BB889", "Service Level:");
		public MultilingualString ServiceStringOrLevelLabel => WiseRateEntry != null ? ServiceStringLabel : ServiceLevelLabel;

		public MultilingualString VesselLabel => ResString.GetMultilingualString("DFF27DA1-DB2D-4F8A-B389-288ABB9DF02F", "Vessel:");
		public MultilingualString ConsigneeLabel => ResString.GetMultilingualString("AD690418-3810-4900-9C56-A0EFC07EFCB7", "Consignee:");
		public MultilingualString VesselOrConsigneeLabel => WiseRateEntry != null ? VesselLabel : ConsigneeLabel;

		public MultilingualString ExpiryDateLabel => ResString.GetMultilingualString("3356CE3A-ECC8-42E2-9B35-BE3BEBDB0ABE", "Expiry Date:");
		public MultilingualString RateTypeLabel => ResString.GetMultilingualString("182580BC-2E5D-4C03-AAC1-41FC9505C762", "Rate Type:");

		public MultilingualString RateTypeOrBlankLabel => WiseRateEntry != null ? RateTypeLabel : (NoResString)"";

		public MultilingualString RateType2Label => ResString.GetMultilingualString("6F1C528E-1543-4B95-A416-E1A8F365A140", "Rate Type 2:");
		public MultilingualString ConsignorLabel => ResString.GetMultilingualString("8292B0E2-9ED0-4D3E-86D3-ECFD56FA2E06", "Consignor:");
		public MultilingualString RateType2OrConsignorLabel => WiseRateEntry != null ? RateType2Label : ConsignorLabel;
		public MultilingualString AddonLabel => ResString.GetMultilingualString("A5439857-5028-4CAA-B2D8-E306889FD7BB", "Add-on:");
		public MultilingualString CarrierLabel => ResString.GetMultilingualString("036AED9A-9B7F-48AF-9F76-34881E0F93E2", "Carrier:");
		public MultilingualString AddonOrCarrierLabel => WiseRateEntry != null ? AddonLabel : CarrierLabel;

		public MultilingualString IncludedLabel => ResString.GetMultilingualString("RateSelection.Included", "Included");
		public MultilingualString ExcludedLabel => ResString.GetMultilingualString("RateSelection.Excluded", "Excluded");
		public MultilingualString NoRateSelectedLabel => ResString.GetMultilingualString("196153DD-8879-49A8-B6A7-DC1C51B4C6A5", "No rate selected.");
		public MultilingualString TotalLabel => ResString.GetMultilingualString("1A530CC6-9CAE-4B56-B53F-58E78DE0C11D", "Total");
		public MultilingualString WarningLabel => ResString.GetMultilingualString("4B7896B9-9A41-432E-B5C1-86E1180318AA", "Warnings");
		public MultilingualString NoWarningError => ResString.GetMultilingualString("5A65998B-DF0C-493B-833B-5CA0C9B38CCC", "There is no warning/error to show for the selected rate");

		public MultilingualString SpotRateTooltip => ResString.GetMultilingualString("196153DE-8879-49A8-B6A7-DC1C51B4C6A5", "Spot Rate");

		MultilingualString AreInvalidChargesError => ResString.GetMultilingualString("def54f44-129c-4afb-802c-9ec007755c7c", "There are charges that could not be converted.");
		MultilingualString MissingExchangeRateError(string fromCurrency) => ResString.GetMultilingualString("c12348ec-769f-4af3-9555-679897ff0ea2", "Missing exchange rate for {0} to {1}", fromCurrency, RateChooserModel.DefaultCurrency);

		public RateSelectorViewModels.BookingInfoViewModel BookingInfo { get; set; }
		public bool IsSelected
		{
			get
			{
				return isSelected;
			}

			set
			{
				if (isSelected != value)
				{
					isSelected = value;
					SelectionChanged?.Invoke(this, EventArgs.Empty);

					foreach (var item in tabs.SelectMany(tab => tab.Rates).Where(rate => Rate.IsRelatedTo(rate.Rate)))
					{
						item.RefreshBinding();
					}

					RefreshBinding();

					NotifyPropertyChanged(nameof(IsSelected));
				}
			}
		}

		bool isSelected;

		public event EventHandler SelectionChanged;

		#region Notifications

		readonly NotificationCollection notifications = new NotificationCollection();

		void Notifications_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
		{
			OnPropertyChanged(nameof(TotalIcon));
			OnPropertyChanged(nameof(TotalIconVisibility));
		}

		#endregion
	}
}
