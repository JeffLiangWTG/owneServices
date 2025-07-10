using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	//
	// !!! This class is used in web service, which means that Env.CurrentCompany can be null. !!!
	//
	[RestrictedFilteredItem]
	public class AccTaxRate : AutoAccTaxRate, ITemplateCopyable, Integration.IAccTaxRate
	{
		public AccTaxRate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static class Types
		{
			public const string Rated = "RAT";
			public const string ReverseRated = "RVS";
			public const string Exempt = "EXT";
			public const string CapitalRated = "CAP";
			public const string NotReportable = "NOT";
			public const string Suspended = "SUS";
			public const string RatedInAnotherCountry = "RAX";
			public const string ReportableUnderBusinessTax = "BST";
			public const string ExcludedFromTheTaxBase = "EXL";
			public const string IntegratedGST = "INT";
			public const string ServiceTax = "SER";
		}

		public static class ExtraTypes
		{
			public const string IndiaPrimaryAndSecondaryEducationTax = "EDU";
			public const string QuebecQST = "QST";
			public const string QuebecQSTExcludingGSTInQSTBase = "QCT";
			public const string ChinaInputVATClaimed = "INP";
			public const string ChinaInputVATOffsetAgainstOutputTax = "OTO";
			public const string VATRetention = "RET";
			public const string VATRetentionFraction = "REF";
			public const string ServiceTax = "SER";
			public const string RegionalTax = "REG";
			public const string VATRemittedByCustomer = "SPV";
			public const string StateGST = "STA";
		}

		public static class Helper
		{
			public const string MainGSTTaxRegistryID = "MainGST";
			public const string MainGSTReverseTaxRegistryID = "MainGSTRev";
			public const string MainNotReportableTaxRegistryID = "MainNotReport";
			public const string MainFreeGSTTaxRegistryID = "MainFreeGST";
			public const string MainFreeGSTReverseTaxRegistryID = "MainFreeGSTRev";

			public static string[] TaxConfigurationRegistryIDs
			{
				get
				{
					return new string[] {
						AccTaxRate.Helper.MainGSTTaxRegistryID,
						AccTaxRate.Helper.MainGSTReverseTaxRegistryID,
						AccTaxRate.Helper.MainNotReportableTaxRegistryID,
						AccTaxRate.Helper.MainFreeGSTTaxRegistryID,
						AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID
					};
				}
			}

			public static ZGuid FindTaxRatePK(BusinessObjectFactory factory, string registryID, Guid company)
			{
				ZQuery query = new ZQuery(StmDataSchema.SD_Name, registryID);
				query.AddToFilter(StmDataSchema.SD_Owner, company);
				StmData item = factory.LoadTop1<StmData>(query);

				return item == null ? ZGuid.Empty : new ZGuid(item.SD_GuidValue);
			}

			public static AccTaxRate FindTaxRate(BusinessObjectFactory factory, string registryID, Guid company)
			{
				var taxRatePK = FindTaxRatePK(factory, registryID, company);
				return taxRatePK.IsValid ? factory.Load<AccTaxRate>(taxRatePK) : null;
			}
		}

		public int TaxRateDecimals => (CountriesWith3DecimalPlaces.Any(x => x == AT_RN_NKCountry)) ? 3 : 2;
		readonly string[] CountriesWith3DecimalPlaces = { Constants.CountryCodes.Bolivia, Constants.CountryCodes.VietNam };

		public static AccTaxRate GetNOTREPORTTaxID(BusinessObjectFactory factory, GlbCompany glbCompany)
		{
			return glbCompany != null ? AccTaxRate.Helper.FindTaxRate(factory, AccTaxRate.Helper.MainNotReportableTaxRegistryID, glbCompany.PK.ToGuid()) : null;
		}

		#region Properties

		[ReadOnly(true)]
		public override ZString AT_Code
		{
			get { return base.AT_Code; }
			set { base.AT_Code = value; }
		}

		[DecimalPlaces(nameof(TaxRateDecimals))]
		public ZDecimal ExtraRateForToday => GetExtraRate(ZDate.Today);

		ZDecimal GetExtraRate(ZDate taxDate)
		{
			var rate = GetExtraRateComponents(taxDate);

			return GetExtraRate(GetRate(taxDate), rate.numerator, rate.denominator);
		}

		public (ZInt numerator, ZInt denominator) GetExtraRateComponents(ZDate taxDate)
		{
			var refRate = AllExtraRatesCache.GetRateComponents(taxDate);
			return refRate == null ? defaultValue : (numerator: refRate.ZAT_RateNumerator, denominator: refRate.ZAT_RateDenominator);
		}

		internal static ZDecimal GetExtraRate(AccTransactionLines line)
		{
			return line.TaxRate?.GetExtraRate(line.AL_TaxRateCalc, line.AL_TaxExtraRateNumerator, line.AL_TaxExtraRateDenominator) ?? ZDecimal.Zero;
		}

		ZDecimal GetExtraRate(ZDecimal taxRate, ZInt taxExtraRateNumerator, ZInt taxExtraRateDenominator) => GetExtraRate(taxRate, AT_ExtraTaxRateType, taxExtraRateNumerator, taxExtraRateDenominator);

		public static ZDecimal GetExtraRate(ZDecimal taxRate, ZString extraRateType, ZInt taxExtraRateNumerator, ZInt taxExtraRateDenominator) => GetExtraRate(taxRate, extraRateType, (ZDecimal)taxExtraRateNumerator / taxExtraRateDenominator);

		public static ZDecimal GetExtraRate(ZDecimal taxRate, ZString extraRateType, ZDecimal extraTaxRate)
		{
			var result = extraTaxRate;
			if (extraRateType == ExtraTypes.VATRetentionFraction)
			{
				result = taxRate * result;
			}

			return result;
		}

		public static ZString LocaliseTaxCode(ZString taxCode, RefCountry country)
		{
			if (country == null)
			{
				throw new ArgumentNullException(nameof(country));
			}

			string consumptionTaxDescription = ZArchitecture.Environment.Country.GetConsumptionTaxDescription(country.RN_Code);
			return string.IsNullOrEmpty(consumptionTaxDescription) ? taxCode : taxCode.Replace("GST", consumptionTaxDescription);
		}

		#region Ref DB properties for UI binding only

		[DecimalPlaces(nameof(TaxRateDecimals))]
		public ZDecimal RateForTodayForUIBinding
		{
			get
			{
				return SystemTaxRate
					? (RefTaxRateDataForUIBinding.FirstOrDefault(x => ZDate.Today >= x.ZAT_StartDate && ZDate.Today <= x.ZAT_EndDate)?.ZAT_Rate ?? 0M)
					: RateObsoleteForUIBinding;
			}
		}

		public ZDecimal ExtraRateForTodayForUIBinding
		{
			get
			{
				var rate = 0M;
				if (SystemTaxRate)
				{
					var todaysExtraRate = RefExtraTaxRateDataForUIBinding.FirstOrDefault(x => ZDate.Today >= x.ZAT_StartDate && ZDate.Today <= x.ZAT_EndDate)?.ZAT_Rate ?? 0M;
					rate = AT_ExtraTaxRateType == ExtraTypes.VATRetentionFraction ? (ZDecimal)(RateForTodayForUIBinding * todaysExtraRate) : todaysExtraRate;
				}
				else
				{
					rate = GetExtraRate(RateObsoleteForUIBinding, ExtraTaxRateNumeratorObsoleteForUIBinding, ExtraTaxRateDenominatorObsoleteForUIBinding);
				}
				return rate;
			}
		}

		public RefAccTaxRateCollection RefTaxRateDataForUIBinding => refTaxRateDataForUIBinding ??
																	(refTaxRateDataForUIBinding = new RefAccTaxRateCollection(Factory, GetFilter(AT_ReferenceRateType)));

		RefAccTaxRateCollection refTaxRateDataForUIBinding;

		public RefAccTaxRateCollection RefExtraTaxRateDataForUIBinding => refExtraTaxRateDataForUIBinding ??
																			(refExtraTaxRateDataForUIBinding = new RefAccTaxRateCollection(Factory, GetFilter(AT_ReferenceExtraRateType)));

		RefAccTaxRateCollection refExtraTaxRateDataForUIBinding;

		ZQuery GetFilter(string rateType)
		{
			var getRefTaxRateFilter = new ZQuery(RefAccTaxRateSchema.ZAT_ReferenceRateType, rateType);
			getRefTaxRateFilter.AddToFilter(RefAccTaxRateSchema.ZAT_RN_NKCountry, AT_RN_NKCountry);
			return getRefTaxRateFilter;
		}

		#endregion

		protected override ZString HumanReadableNameCore => Res.GetString("615F208E-67A5-4BC7-92AF-B74B10E34809", "Tax ID - {0}", CalculateShortcutName());

		[ReadOnly(true)]
		public override ZString AT_ReferenceExtraRateType { get => base.AT_ReferenceExtraRateType; set => base.AT_ReferenceExtraRateType = value; }

		[ReadOnly(true)]
		public override ZString AT_ReferenceRateType { get => base.AT_ReferenceRateType; set => base.AT_ReferenceRateType = value; }

		[ReadOnly(true)]
		public override ZString AT_RN_NKCountry { get => base.AT_RN_NKCountry; set => base.AT_RN_NKCountry = value; }

		public ZString AT_TaxSystemCode_ForDisplay => !AT_TaxSystemCode.IsEmpty ? AT_TaxSystemCode : (ZString)GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription;

		#region Tax Rates users may have on their old Tax IDs

		ZDecimal RateObsoleteForUIBinding
		{
			get
			{
				var addOn = AddOnColumns.Find(AddOnColumnNames.RateObsolete);
				return addOn != null && !addOn.XA_Data.IsEmpty ? new ZDecimal(addOn.XA_Data) : ZDecimal.Zero;
			}
		}

		ZInt ExtraTaxRateNumeratorObsoleteForUIBinding
		{
			get
			{
				var addOn = AddOnColumns.Find(AddOnColumnNames.ExtraTaxRateNumeratorObsolete);
				return addOn != null && !addOn.XA_Data.IsEmpty ? new ZInt(addOn.XA_Data) : ZInt.Zero;
			}
		}

		ZInt ExtraTaxRateDenominatorObsoleteForUIBinding
		{
			get
			{
				var addOn = AddOnColumns.Find(AddOnColumnNames.ExtraTaxRateDenominatorObsolete);
				return addOn != null && !addOn.XA_Data.IsEmpty ? new ZInt(addOn.XA_Data) : (ZInt)1;
			}
		}

		GenAddOnColumnCollection AddOnColumns => addOnColumns ?? (addOnColumns = new GenAddOnColumnCollection(this));
		GenAddOnColumnCollection addOnColumns;

		static class AddOnColumnNames
		{
			public const string RateObsolete = "RateObsolete";
			public const string ExtraTaxRateNumeratorObsolete = "ExtraTaxRateNumeratorObsolete";
			public const string ExtraTaxRateDenominatorObsolete = "ExtraTaxRateDenominatorObsolete";
		}

		#endregion

		[ReadOnly(true)]
		[List("Lookups.Types")]
		public override ZString AT_Type
		{
			get { return base.AT_Type; }
			set { base.AT_Type = value; }
		}

		[ReadOnly(true)]
		[List("Lookups.ExtraTypes")]
		public override ZString AT_ExtraTaxRateType
		{
			get { return base.AT_ExtraTaxRateType; }
			set { base.AT_ExtraTaxRateType = value; }
		}

		public ZDecimal GetRate(ZDate taxDate)
		{
			return IsReverseOrSuspendedCharge ? ZDecimal.Zero : GetRateRaw(taxDate);
		}

		public ZDecimal GetRateRaw(ZDate taxDate)
		{
			var rateComponents = GetRateComponents(taxDate);
			return GetRateRaw(rateComponents.numerator, rateComponents.denominator);
		}

		public static ZDecimal GetRate(ZString rateType, ZDecimal rate) => GetIsReverseOrSuspendedCharge(rateType) ? 0 : rate;

		internal static ZDecimal GetRateRaw(AccTransactionLines line) => GetRateRaw(line.AL_TaxRateNumerator, line.AL_TaxRateDenominator);

		static ZDecimal GetRateRaw(ZInt numerator, ZInt denominator) => (ZDecimal)numerator / denominator;

		RefAccTaxRate GetRateComponentsUnsafe(ZDate taxDate) => AllRatesCache.GetRateComponents(taxDate);

		public bool DoesRateExists(ZDate taxDate) => GetRateComponentsUnsafe(taxDate) != null;

		public (ZInt numerator, ZInt denominator) GetRateComponents(ZDate taxDate)
		{
			var refRate = GetRateComponentsUnsafe(taxDate);
			return refRate == null ? defaultValue : (numerator: refRate.ZAT_RateNumerator, denominator: refRate.ZAT_RateDenominator);
		}

		public ZDecimal GetEffectiveExtraRate(ZDate taxDate) => GetEffectiveExtraRate(AT_Type, GetRate(taxDate), AT_ExtraTaxRateType, GetExtraRate(taxDate));

		public static ZDecimal GetEffectiveExtraRate(ZString rateType, ZDecimal rate, ZString extraRateType, ZDecimal extraRate)
		{
			if (extraRateType == ExtraTypes.IndiaPrimaryAndSecondaryEducationTax)
			{
				return rate * extraRate / 100;
			}
			else if (extraRateType == ExtraTypes.ChinaInputVATClaimed)
			{
				return 100 * (extraRate / (100 - extraRate));
			}
			else if (extraRateType == ExtraTypes.VATRetention || extraRateType == ExtraTypes.VATRetentionFraction)
			{
				return -extraRate;
			}
			else if (extraRateType == ExtraTypes.VATRemittedByCustomer)
			{
				return -rate;
			}
			else if (GetIsReverseCharge(rateType) && extraRateType == ExtraTypes.StateGST) // special case for Indian C&SGST RVS type
			{
				return 0;
			}
			else if (extraRateType == ExtraTypes.QuebecQSTExcludingGSTInQSTBase || extraRateType == ExtraTypes.ChinaInputVATOffsetAgainstOutputTax || extraRateType == ExtraTypes.StateGST)
			{
				return extraRate;
			}

			return (rate + 100) * (extraRate / 100);
		}

		public static ZDecimal GetExtraTaxBaseAmount(ZString extraTaxRateType, ZDecimal extraTaxRate, ZDecimal extraTotalTaxAmount, ZDecimal totalPrimaryTaxAmount, ZDecimal totalExclTaxAmount)
		{
			if (extraTaxRateType == ExtraTypes.IndiaPrimaryAndSecondaryEducationTax)
			{
				return totalPrimaryTaxAmount;
			}
			else if (extraTaxRateType == ExtraTypes.ChinaInputVATClaimed)
			{
				return (extraTotalTaxAmount * 100) / extraTaxRate;
			}
			else if (extraTaxRateType == ExtraTypes.VATRetention || extraTaxRateType == ExtraTypes.VATRetentionFraction)
			{
				return -totalExclTaxAmount;
			}
			else if (extraTaxRateType == ExtraTypes.QuebecQST)
			{
				return totalExclTaxAmount + totalPrimaryTaxAmount;
			}
			return totalExclTaxAmount;
		}

		public bool IsNonReportable => AT_Type.IsEmpty || NonReportableTypes.Contains(AT_Type);

		ZString[] NonReportableTypes => new ZString[] { AccTaxRate.Types.NotReportable, AccTaxRate.Types.ExcludedFromTheTaxBase };

		public bool IsReverseCharge => GetIsReverseCharge(AT_Type);

		static bool GetIsReverseCharge(ZString rateType) => rateType == Types.ReverseRated;

		public bool IsSuspendedCharge => GetIsSuspendedCharge(AT_Type);

		static bool GetIsSuspendedCharge(ZString rateType) => rateType == Types.Suspended;

		public bool IsReverseOrSuspendedCharge => GetIsReverseOrSuspendedCharge(AT_Type);

		static bool GetIsReverseOrSuspendedCharge(ZString rateType) => GetIsReverseCharge(rateType) || GetIsSuspendedCharge(rateType);

		public bool IsVATWithholding
		{
			get { return AT_ExtraTaxRateType == ExtraTypes.VATRetention || AT_ExtraTaxRateType == ExtraTypes.VATRetentionFraction; }
		}

		public bool IsVATRemittedByCustomer => GetIsVATRemittedByCustomer(AT_RN_NKCountry, AT_Type, AT_ExtraTaxRateType);

		public static bool GetIsVATRemittedByCustomer(ZString country, ZString rateType, ZString extraRateType)
		{
			return (country == Core.Constants.CountryCodes.Italy || country == Core.Constants.CountryCodes.CostaRica)
					  && rateType == Types.Rated
						  && extraRateType == ExtraTypes.VATRemittedByCustomer;
		}

		public GlbCompanyCollection Companies
		{
			get
			{
				if (fCompanies == null)
				{
					fCompanies = new GlbCompanyCollection(Factory);
				}
				return fCompanies;
			}
		}

		public bool IsMexicoNeedExtraType => GetIsMexicoNeedExtraType(AT_RN_NKCountry, AT_ExtraTaxRateType);

		public static bool GetIsMexicoNeedExtraType(ZString country, ZString extraRateType)
			=> country == Constants.CountryCodes.Mexico && (extraRateType == ExtraTypes.VATRetentionFraction || extraRateType == ExtraTypes.VATRetention);

		public bool IsLocalExtraTaxAmountValuePersistent => GetIsLocalExtraTaxAmountValuePersistent(AT_RN_NKCountry, AT_ExtraTaxRateType);

		public static bool GetIsLocalExtraTaxAmountValuePersistent(ZString country, ZString extraRateType)
		{
			return ((country == Constants.CountryCodes.India && extraRateType == ExtraTypes.StateGST) ||
					(country == Constants.CountryCodes.Italy && extraRateType == ExtraTypes.VATRemittedByCustomer) ||
					(GetIsMexicoNeedExtraType(country, extraRateType)));
		}

		public bool IsIndiaStateTax => GetIsIndiaStateTax(AT_RN_NKCountry, AT_Type, AT_ExtraTaxRateType);

		public static bool GetIsIndiaStateTax(ZString country, ZString rateType, ZString extraRateType)
		{
			return rateType == Types.Rated && extraRateType == ExtraTypes.StateGST && country == Constants.CountryCodes.India;
		}

		public bool IsIndiaServiceTax
		{
			get
			{
				return (AT_Type == Types.ServiceTax || AT_ExtraTaxRateType == ExtraTypes.ServiceTax) && AT_RN_NKCountry == Constants.CountryCodes.India;
			}
		}

		public bool SystemTaxRate => !string.IsNullOrWhiteSpace(AT_ReferenceRateType);

		public bool IsZeroTaxRate => ZeroTaxReferenceRateTypeList.Contains(AT_ReferenceRateType) && AT_ReferenceExtraRateType.IsEmpty;

		IEnumerable<ZString> ZeroTaxReferenceRateTypeList
		{
			get
			{
				yield return Constants.ZeroTaxReferenceRateType.Zero;
				yield return Constants.ZeroTaxReferenceRateType.Empty;
			}
		}

		#endregion

		#region Implementation

		protected GlbCompanyCollection fCompanies;

		RefAccTaxRateCacheManager AllRatesCache => allRatesCache ?? (allRatesCache = new RefAccTaxRateCacheManager(Factory, AT_RN_NKCountryInfo, AT_ReferenceRateTypeInfo));
		RefAccTaxRateCacheManager allRatesCache;

		RefAccTaxRateCacheManager AllExtraRatesCache => allExtraRatesCache ?? (allExtraRatesCache = new RefAccTaxRateCacheManager(Factory, AT_RN_NKCountryInfo, AT_ReferenceExtraRateTypeInfo));
		RefAccTaxRateCacheManager allExtraRatesCache;

		#endregion

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			AccTaxRate result = Factory.New<AccTaxRate>();
			result.CopyPersistentValuesFrom(this);
			return result;
		}

		#endregion

		#region Debug only
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (AT_Type.IsEmpty)
			{
				AT_Type = Types.Rated;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

		public ZDecimal GetRateRaw_ForTestOnly() => GetRateRaw(ZDate.Today);

		public ZDecimal GetRate_ForTestOnly() => GetRate(ZDate.Today);

		public ZDecimal GetExtraRate_ForTestOnly() => GetExtraRate(ZDate.Today);

		public static AccTaxRate CreateTaxRate_ForTestOnly(BusinessObjectFactory factory)
		{
			var rate = factory.NewWithValidTestData<AccTaxRate>();
			rate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			return rate;
		}

		public RefAccTaxRate SetRateNumerator_ForTestOnly(ZInt value)
		{
			var rate = new RefTaxRateTestHelper(this).LoadOrCreateRefTaxRate(null, null);
			rate.ZAT_RateNumerator = value;

			return rate;
		}

		public RefAccTaxRate SetRate_ForTestOnly(ZInt numerator, ZInt denominator, ZDate? startTaxDate = null, ZDate? endTaxDate = null)
		{
			var rate = new RefTaxRateTestHelper(this).LoadOrCreateRefTaxRate(startTaxDate, endTaxDate);
			rate.ZAT_RateNumerator = numerator;
			rate.ZAT_RateDenominator = denominator;

			return rate;
		}

		public RefAccTaxRate SetExtraRate_ForTestOnly(ZInt numerator, ZInt denominator, ZDate? startTaxDate = null, ZDate? endTaxDate = null)
		{
			var rate = new RefTaxRateTestHelper(this).LoadOrCreateRefTaxExtraRate(startTaxDate, endTaxDate);
			rate.ZAT_RateNumerator = numerator;
			rate.ZAT_RateDenominator = denominator;

			return rate;
		}

		class RefTaxRateTestHelper
		{
			public RefTaxRateTestHelper(AccTaxRate rate)
			{
				this.Rate = rate;
			}

			public RefAccTaxRate LoadOrCreateRefTaxRate(ZDate? startDate, ZDate? endDate)
			{
				if (Rate.AT_ReferenceRateType.IsEmpty)
				{
					Rate.AT_ReferenceRateType = Rate.AT_Code;
				}
				var rate = LoadOrCreateRefTaxRate(Rate.AT_ReferenceRateType, startDate, endDate);

				return rate;
			}

			public RefAccTaxRate LoadOrCreateRefTaxExtraRate(ZDate? startDate, ZDate? endDate)
			{
				if (Rate.AT_ReferenceExtraRateType.IsEmpty)
				{
					var prefix = "X";
					var code = Rate.AT_Code;
					if (code.Length + prefix.Length > AccTaxRateSchema.AT_ReferenceExtraRateType.MaxLength)
					{
						code = code.Substring(prefix.Length, code.Length - prefix.Length);
					}
					Rate.AT_ReferenceExtraRateType = prefix + code;
				}
				var rate = LoadOrCreateRefTaxRate(Rate.AT_ReferenceExtraRateType, startDate, endDate);

				return rate;
			}

			RefAccTaxRate LoadOrCreateRefTaxRate(ZString refType, ZDate? startDate, ZDate? endDate)
			{
				var query = new ZQuery(RefAccTaxRateSchema.ZAT_RN_NKCountry, Rate.AT_RN_NKCountry);
				query.AddToFilter(RefAccTaxRateSchema.ZAT_ReferenceRateType, refType);

				if (!startDate.HasValue)
				{
					startDate = new ZDate(ZDateTime.MinSmallDateTimeValue);
				}

				if (!endDate.HasValue)
				{
					endDate = new ZDate(DateTime.MaxValue);
				}

				query.AddToFilter(RefAccTaxRateSchema.ZAT_StartDate, startDate);
				query.AddToFilter(RefAccTaxRateSchema.ZAT_EndDate, endDate);

				var rate = Rate.Factory.LoadTop1<RefAccTaxRate>(query);
				if (rate == null)
				{
					rate = Rate.Factory.New<RefAccTaxRate>();
					rate.ZAT_RN_NKCountry = Rate.AT_RN_NKCountry;
					rate.ZAT_ReferenceRateType = refType;
					rate.ZAT_StartDate = startDate.Value;
					rate.ZAT_EndDate = endDate.Value;
				}

				return rate;
			}

			AccTaxRate Rate { get; }
		}

		public static AccTaxRate LoadExistingOrCreateNewTaxRate(BusinessObjectFactory factory, ZString aT_Code, ZString aT_Type, int rateNumerator, int rateDenominator = 1)
		{
			var taxRate = FindExistingTaxRate(factory, aT_Code, aT_Type, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			if (taxRate == null)
			{
				taxRate = factory.New<AccTaxRate>();
				taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxRate.AT_Code = aT_Code;
				taxRate.AT_Type = aT_Type;
			}

			taxRate.SetRate_ForTestOnly(rateNumerator, rateDenominator);

			return taxRate;
		}

#endif
		#endregion

		public static AccTaxRate FindExistingTaxRate(BusinessObjectFactory factory, ZString aT_Code, ZString aT_Type, ZString countryCode)
		{
			ZQuery findTaxRateQuery = new ZQuery(AccTaxRateSchema.AT_Code, aT_Code);
			findTaxRateQuery.AddToFilter(AccTaxRateSchema.AT_Type, aT_Type);
			findTaxRateQuery.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, countryCode);
			AccTaxRate taxRate = factory.LoadTop1<AccTaxRate>(findTaxRateQuery);
			return taxRate;
		}

		public static string CreateApplicableTaxIds(BusinessObjectFactory factory)
		{
			ZString result = "";

			var message = Res.GetString("cd689831-eee6-45bd-8088-84e98eeae9f4", "No changes required. The current Login Country/Region's Tax ID Set is already up to date.");
			var beforeCreatingTaxIdsCount = factory.Load<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).Length;

			var currentCompany = factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.CreateMissingTaxRates();

			var afterCreatingTaxIdsCount = factory.Load<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).Length;

			if (beforeCreatingTaxIdsCount == afterCreatingTaxIdsCount)
			{
				result = message;
			}

			return result;
		}

		public static bool IsPostingGroupsEnabled(ZString countryCode)
		{
			var applicableCountries = new HashSet<string> {
				Constants.CountryCodes.Chile,
				Constants.CountryCodes.China,
				Constants.CountryCodes.Colombia,
				Constants.CountryCodes.Spain,
				Constants.CountryCodes.Indonesia,
				Constants.CountryCodes.KoreaSouth,
				Constants.CountryCodes.Peru,
				Constants.CountryCodes.Philippines,
				Constants.CountryCodes.SriLanka,
				Constants.CountryCodes.Taiwan,
				Constants.CountryCodes.VietNam,
				Constants.CountryCodes.Argentina,
				Constants.CountryCodes.Brazil,
				Constants.CountryCodes.Mexico,
				Constants.CountryCodes.WesternSamoa,
				Constants.CountryCodes.India,
				Constants.CountryCodes.Malaysia,
				Constants.CountryCodes.Turkey,
				Constants.CountryCodes.Fiji,
				Constants.CountryCodes.Romania,
				Constants.CountryCodes.Germany
			};

			return applicableCountries.Contains(countryCode.ToString());
		}

		public const short DefaultPostingGroupID = 0;

		(ZInt numerator, ZInt denominator) defaultValue = (numerator: 0, denominator: 1);
	}
}
