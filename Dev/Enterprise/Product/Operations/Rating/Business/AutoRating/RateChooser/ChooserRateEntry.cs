using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Api.Model;
using WiseRates.Constants;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// A rate (CW1 or CargoSphere) in the chooser model.
	/// Supports activating optional charges and exposing CargoSphere specific rate fields.
	/// </summary>
	public class ChooserRateEntry : NonPersistentBusinessObject
	{
		/// <summary>
		/// Constructor for CW1 rate
		/// </summary>
		public ChooserRateEntry(BusinessObjectFactory factory, ILogger logger, IRateEntry cw1Rate, IRateChooserServices services, AutoRateInfoCollection calculatedResult, IRateSelectorFilterValueProvider filters = null)
			: base(factory)
		{
			Logger = logger;
			ServiceProvider = cw1Rate.ParentRatingHeader.Header;
			RateEntry = cw1Rate;
			CarrierServiceLevel = cw1Rate.TI_PL_NKCarrierServiceLevel;
			CarrierServiceLevelIsMapped = true;
			CarrierIsMapped = true;
			ChooserServices = services;
			CalculatedResult = calculatedResult;
			SelectedCalculatedResult = calculatedResult;
			Filters = filters;
		}

		public AutoRateInfoCollection CalculatedResult { get; }
		public AutoRateInfoCollection SelectedCalculatedResult;

		public IRateSelectorFilterValueProvider Filters { get; }

		public ILogger Logger;

		public IRateChooserServices ChooserServices { get; }

		/// <summary>
		/// Constructor for wise rate
		/// </summary>
		public ChooserRateEntry(BusinessObjectFactory factory, ILogger logger, OrgHeader serviceProvider, Rate apiRate, IEnumerable<IRateEntry> convertedRates, IRateChooserServices services, AutoRateInfoCollection calculatedResult, IRateSelectorFilterValueProvider filters = null)
			: base(factory)
		{
			Logger = logger;
			ServiceProvider = serviceProvider;
			WiseRateEntry = apiRate;
			ChooserServices = services;
			CalculatedResult = calculatedResult;
			SelectedCalculatedResult = calculatedResult;

			foreach (var customField in apiRate.ProviderCustomFields)
			{
				switch (customField.Code)
				{
					case Rate.CustomFields.CargoSphere.RateType:
						RateType = customField.Value.ToString();
						break;
					case Rate.CustomFields.CargoSphere.RateType2:
						RateType2 = customField.Value.ToString();
						break;
					case Rate.CustomFields.CargoSphere.Vessel:
						Vessel = customField.Value.ToString();
						break;
					case Rate.CustomFields.CargoSphere.ArbitraryPermission:
						AddOn = customField.Value.ToString();
						break;
					case Rate.CustomFields.CargoSphere.TradeLane:
						TradeLane = customField.Value.ToString();
						break;
					case Rate.CustomFields.Common.Routing:
						Routing = customField.Value.ToString();
						break;
					case Rate.CustomFields.CargoSphere.ServiceString:
						ServiceString = customField.Value.ToString();
						break;
					case Rate.CustomFields.CargoSphere.LclUnit:
						LclUnit = customField.Value.ToString();
						break;
				}
			}

			activeChargeSet = new HashSet<Charge>(apiRate.Charges.Where(x => !x.IsOptional));
			chargeToWiseLine = new Dictionary<Charge, WiseLine>(apiRate.Charges.Count);
			Filters = filters;

			UpdateConvertedRates(convertedRates);
		}

#if DEBUG
		/// <summary>
		/// Constructor for CW1 rate with no logger - for test only
		/// </summary>
		public ChooserRateEntry(BusinessObjectFactory factory, IRateEntry cw1Rate, IRateChooserServices services, AutoRateInfoCollection calculatedResult)
			: this(factory, null, cw1Rate, services, calculatedResult) { }

		/// <summary>
		/// Constructor for wise rate with no logger - for test only
		/// </summary>
		public ChooserRateEntry(BusinessObjectFactory factory, OrgHeader serviceProvider, Rate apiRate, IEnumerable<IRateEntry> convertedRates, IRateChooserServices services, AutoRateInfoCollection calculatedResult)
			: this(factory, null, serviceProvider, apiRate, convertedRates, services, calculatedResult) { }
#endif

		/// <summary>
		/// Called on construction and after the user quick-maps something that was unmapped.
		/// </summary>
		public void UpdateConvertedRates(IEnumerable<IRateEntry> convertedRates)
		{
			ConvertedRates = convertedRates;
			RateEntry = convertedRates.FirstOrDefault();

			SetCarrierIsMapped();
			SetCarrierServiceLevelIsMapped();

			chargeToWiseLine.Clear();
			foreach (WiseEntry entry in convertedRates)
			{
				foreach (WiseLine line in entry.ChildRateLines)
				{
					chargeToWiseLine[line.WiseCharge] = line;
				}
			}
		}

		void SetCarrierServiceLevelIsMapped()
		{
			if (!string.IsNullOrWhiteSpace(WiseRateEntry.ServiceLevel))
			{
				var entry = (WiseEntry)RateEntry;
				if (entry.Errors.TryGetValue(RateEntrySchema.TI_PL_NKCarrierServiceLevel, out var error))
				{
					CarrierServiceLevel = WiseRateEntry.ServiceLevel;
					CarrierServiceLevelError = error;
					CarrierServiceLevelIsMapped = false;
				}
				else
				{
					CarrierServiceLevel = RateEntry?.TI_PL_NKCarrierServiceLevel;
					CarrierServiceLevelError = string.Empty;
					CarrierServiceLevelIsMapped = true;
				}
			}
			else
			{
				CarrierServiceLevel = string.Empty;
				CarrierServiceLevelError = string.Empty;
				CarrierServiceLevelIsMapped = true;
			}
		}

		public new bool HasErrors
		{
			get
			{
				if (WiseRateEntry == null)
				{
					// CW1 rate has no errors or warnings
					return false;
				}

				var entry = (WiseEntry)RateEntry;
				if (entry.Errors.Keys.Except(new[] { RateEntrySchema.TI_PL_NKCarrierServiceLevel }).Any())
				{
					return true;
				}

				foreach (var charge in AllConvertedCharges)
				{
					if (string.IsNullOrEmpty(charge.Key.ChargeCode))
					{
						return true;
					}

					if (charge.Value.Errors.Keys.Except(new[] { RateLinesSchema.TL_AC }).Any())
					{
						return true;
					}
				}

				return false;
			}
		}

		public new bool HasWarnings
		{
			get
			{
				if (WiseRateEntry == null)
				{
					// CW1 rate has no errors or warnings
					return false;
				}

				if (!CarrierIsMapped || !CarrierServiceLevelIsMapped)
				{
					return true;
				}

				foreach (var charge in AllConvertedCharges)
				{
					if (!string.IsNullOrEmpty(charge.Key.ChargeCode) && charge.Value.ChargeCode == null)
					{
						return true;
					}
				}

				return false;
			}
		}

		public RefCommodityCode Commodity
		{
			get
			{
				return Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, RateEntry.TI_RH_NKCommodityCode);
			}
		}

		void SetCarrierIsMapped() => CarrierIsMapped = string.IsNullOrWhiteSpace(WiseRateEntry.Carrier) || ServiceProvider != null;

		readonly Dictionary<Charge, WiseLine> chargeToWiseLine;

		bool IsSpotRate => WiseRateEntry?.BookingInfo != null && WiseRateEntry?.BookingInfo?.Schedule != null;

		public string ContractNumber => WiseRateEntry != null ? WiseRateEntry.ContractNumber : (string)RateEntry.TI_ContractNumber;

		public string CarrierQuoteNumber => IsSpotRate ? WiseRateEntry.ProviderRateId : string.Empty;

		public string CarrierServiceLevel { get; private set; }
		public string CarrierServiceLevelError { get; private set; }
		public bool CarrierServiceLevelIsMapped { get; private set; }
		public bool CarrierIsMapped { get; private set; }
		public string RateType { get; }
		public string RateType2 { get; }
		public string LclUnit { get; }
		public string Vessel { get; }
		public string AddOn { get; }
		public string TradeLane { get; }
		public string Routing { get; }
		public string ServiceString { get; }

		public string RateCategory => WiseRateEntry != null ? "" : (string)RateEntry.TI_RateCategory;

		public string RateProvider =>
			WiseRateEntry switch
			{
				null => string.Empty,
				var entry when entry.Provider == UniversalToWiseRateConverter.UrsProviderCode && entry.TransportMode == WRConstants.TransportModes.SEA => WRConstants.RateProviders.CargoSphere,
				var entry => entry.Provider
			};

		public OrgHeader ServiceProvider { get; set; }

		public string RateId => RateEntry.RateId;

		/// <summary>
		/// CW1 rate or first converted rate if this is a WiseRate
		/// </summary>
		public IRateEntry RateEntry { get; private set; }

		/// <summary>
		/// WiseRate - null if this is a CW1 rate.
		/// </summary>
		public Rate WiseRateEntry { get; }

		[BusinessObjectTestExclude()] // Since this is null for a CW1 rate
		public IEnumerable<IRateEntry> ConvertedRates { get; private set; }

		public bool IsActive(Charge charge) => activeChargeSet.Contains(charge);

		[BusinessObjectTestExclude()] // Since this is null for a CW1 rate
		[SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		public IEnumerable<KeyValuePair<Charge, WiseLine>> ActiveConvertedCharges => chargeToWiseLine.Where(x => activeChargeSet.Contains(x.Key));

		[BusinessObjectTestExclude()] // Since this is null for a CW1 rate
		[SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		public IEnumerable<KeyValuePair<Charge, WiseLine>> AllConvertedCharges => chargeToWiseLine;

		[BusinessObjectTestExclude()] // Since this is null for a CW1 rate
		public IEnumerable<Charge> ActiveCharges => activeChargeSet;
		readonly HashSet<Charge> activeChargeSet;

		public void SetActive(Charge optionalCharge, bool isActive)
		{
			bool isModified = isActive
				? activeChargeSet.Add(optionalCharge)
				: activeChargeSet.Remove(optionalCharge);

			if (isModified)
			{
				OnChargeIsActiveChanged(optionalCharge, isActive);
			}
		}

		public class ChargeIsActiveChangedEventArgs : EventArgs
		{
			public ChargeIsActiveChangedEventArgs(Charge charge, bool isActive)
			{
				Charge = charge;
				IsActive = isActive;
			}

			public Charge Charge { get; }
			public bool IsActive { get; }
		}

		public event EventHandler<ChargeIsActiveChangedEventArgs> ChargeIsActiveChanged;
		void OnChargeIsActiveChanged(Charge charge, bool isActive)
			=> ChargeIsActiveChanged?.Invoke(this, new ChargeIsActiveChangedEventArgs(charge, isActive));

		public bool IsRelatedTo(ChooserRateEntry other)
		{
			if (WiseRateEntry != null && other?.WiseRateEntry != null)
			{
				return WiseRateEntry.RatesServiceProvider == other.WiseRateEntry.RatesServiceProvider
					&& WiseRateEntry.ServiceGroupId == other.WiseRateEntry.ServiceGroupId;
			}

			return RateProvider == other?.RateProvider
				&& ContractNumber == other?.ContractNumber
				&& CarrierServiceLevel == other?.CarrierServiceLevel
				&& RateEntry.NamedAccounts.Count() == other?.RateEntry.NamedAccounts.Count()
				&& RateEntry.NamedAccounts.All(other.RateEntry.NamedAccounts.Contains);
		}

		public bool IsFCL()
		{
			if (RateEntry == null)
			{
				//default value for rate selector,although should not happen
				//because if Rate was null, we should not building rate card for it
				return true;
			}

			if (WiseRateEntry == null)
			{
				//for CW1 rates, always use IsFCL
				return RateEntry.IsFCL();
			}

			//there is a possibility that when we convert CS rates to CW1 rates,
			// TI_RateCategory was null and we don't reject that, so we can show it in Rates selector
			//and show calculated rates for possible charges. So if it was not empty, we use IsFCL
			//otherwise we use container mode for CS rates to see if it is FCL
			if (!string.IsNullOrEmpty(RateEntry.TI_RateCategory))
			{
				return RateEntry.IsFCL();
			}

			return WiseRateEntry.ContainerMode == RatingConstants.RateCategory.FCL;
		}
	}
}
