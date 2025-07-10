using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Core;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Environment;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.Shared;
using Country = Enterprise.ZArchitecture.Environment.Country;

namespace Enterprise.MasterFiles.Business
{
	[System.Diagnostics.DebuggerDisplay("Code = {RN_Code}")]
	[DescriptionProperty(AutoRefCountry.Schema.RN_Desc)]
	public class RefCountry : AutoRefCountry,
		ICountry,
		ILocationBiz,
		IRefCountry,
		IDocManagerSupport
	{
		#region Schema

		public new abstract class Schema : AutoRefCountry.Schema
		{
			public const string AWBCurrencyCode = "AWBCurrencyCode";
		}

		#endregion

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public RefCountry LoadForCountry(string countryCode)
			{
				return (RefCountry)Factory.LoadFromNaturalKey(typeof(RefCountry), RefCountrySchema.RN_Code, countryCode);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(RefCountry);
			}
		}

		public static ZString GetPrefixForTaxRegistrationCode(ZString countryCode)
		{
			ZString result;

			if (countryCode == Constants.CountryCodes.Greece)
			{
				result = "EL";
			}
			else
			{
				result = countryCode;
			}

			return result;
		}

		public static RefCountry LoadFromCountryCode(BusinessObjectFactory factory, ZString countryCode)
		{
			return new Loader(factory).LoadForCountry(countryCode);
		}

		public static RefCountry LoadFromCountryName(BusinessObjectFactory factory, ZString countryName)
		{
			ZQuery countryNameFilter = new ZQuery(RefCountrySchema.RN_Desc, countryName);
			RefCountry[] countries = (RefCountry[])factory.Load(typeof(RefCountry), countryNameFilter);

			return countries.Length == 1 ? countries[0] : null;
		}

		#endregion

		public RefCountry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Organisation Country

		public static RefCountry OrganisationCountry(OrgHeader org)
		{
			if (org != null && org.UNLOCO != null)
			{
				return org.UNLOCO.Country;
			}
			return null;
		}

		#endregion

		#region Business Objects Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			RN_IsSystem = false;
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#region Delete

		public override bool CanDelete => base.CanDelete && !RN_IsSystem && InUseByCompanies.Count == 0 && InUseByOrgAddresses.Count == 0;

		public List<GlbCompany> InUseByCompanies => Factory
			.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, SQLComparisonOperator.Equal, Code))
			.ToList();

		public List<OrgAddress> InUseByOrgAddresses => Factory
			.Load<OrgAddress>(new ZQuery(OrgAddressSchema.OA_RN_NKCountryCode, SQLComparisonOperator.Equal, Code)
				.AddToFilter(OrgAddressSchema.OA_IsActive, SQLComparisonOperator.Equal, true))
			.ToList();

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (RN_IsSystem)
				{
					return ResString.GetMultilingualString("{0F9B62EC-4254-40F3-B35C-77141DE6B07F}", "Cannot delete system defined countries/regions.");
				}

				var companies = InUseByCompanies;
				if (companies.Count > 0)
				{
					return ResString.GetMultilingualString("{563ADA67-B0E5-4AC0-9D85-EF75CD9F3546}", "Cannot delete country/region {0} because it is in use by companies: {1}.", Code, string.Join(", ", companies.Select(c => c.GC_Code)));
				}

				var addresses = InUseByOrgAddresses;
				if (addresses.Count > 0)
				{
					return ResString.GetMultilingualString("{3F227FA4-6266-43A9-9B04-90F3E5300C00}", "Cannot delete country/region {0} because it is in use by organizations: {1}.",
						Code, string.Join(", ", addresses.Select(a => Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.Equal, a.OA_OH)).OH_Code)));
				}
				return base.ReasonForNotAbleToDelete;
			}
		}

		#endregion

		#endregion

		#region Properties

		#region RN_Code

		[ReadOnlyMember(Schema.RN_IsSystem)]
		public override ZString RN_Code
		{
			get { return base.RN_Code; }
			set
			{
				if (Globals.IsTest && IsInDatabase)
				{
					ErrorReporter.ReportOnce("RefCountryRN_CodeSetInTest",
								"Attempted to set country code" + System.Environment.NewLine +
								"Original Value = '" + RN_Code + "'" + System.Environment.NewLine +
								"New Value = '" + value + "'");
				}
				base.RN_Code = value;
			}
		}

		#endregion

		#region RN_Desc

		[RefCountryTranslatableDataField(Schema.RN_Desc, MaxLength = Schema.RN_DescMaxLength, SecurityCheckpoint = "CountriesModify", Asmid = ResString.AssemblyId)]
		public override ZString RN_Desc
		{
			get { return base.RN_Desc; }
			set { base.RN_Desc = value; }
		}

		public MultilingualString RN_DescMultilingual
		{
			get { return GetMultilingual(RN_DescInfo); }
		}

		protected bool RN_Desc_ReadOnly
		{
			get { return RN_IsSystem && !GlbStaff.CurrentUser.GS_IsController; }
		}

		#endregion

		#region RN_RX_NKLocalCurrency

		[ReadOnlyMember(Schema.RN_IsSystem)]
		public override ZString RN_RX_NKLocalCurrency
		{
			get { return base.RN_RX_NKLocalCurrency; }
			set { base.RN_RX_NKLocalCurrency = value; }
		}

		#endregion

		#region RN_EconomicGroup

#if DEBUG
		public IDisposable TemporarilySetIsPartOfEuropeanUnion(bool isPartOfEU)
		{
			var currentEconomicGrouping = RN_EconomicGrouping;

			RN_EconomicGrouping = isPartOfEU
				? EconomicGroupList.Codes.EuropeanUnion
				: string.Empty;

			return new DisposableAction(() => RN_EconomicGrouping = currentEconomicGrouping);
		}
#endif

		public bool IsPartOfEuropeanUnion
		{
			get { return RN_EconomicGrouping == EconomicGroupList.Codes.EuropeanUnion; }
		}

		public bool IsEuOrFriend
		{
			get
			{
				return IsPartOfEuropeanUnion || IsRecognisedByEuropeanUnionAsIssuerOfThirdCountryUniqueIdentificationNumbersOrAEO || IsUnitedKingdom || IsNorthernIreland;
			}
		}

		public bool IsIcs2Member
		{
			get
			{
				return Code.EqualsIgnoringCase(Core.Constants.CountryCodes.UnitedKingdom) ||
					Code.EqualsIgnoringCase(Core.Constants.CountryCodes.Norway) ||
					Code.EqualsIgnoringCase(Core.Constants.CountryCodes.Switzerland) ||
					Code.EqualsIgnoringCase(Core.Constants.CountryCodes.SanMarino) ||
					Code.EqualsIgnoringCase(Core.Constants.CountryCodes.Andorra) ||
					Code.EqualsIgnoringCase(Core.Constants.CountryCodes.Liechtenstein) ||
					Code.EqualsIgnoringCase(Core.Constants.CountryCodes.Vatican) ||
					IsPartOfEuropeanUnion;
			}
		}

		public bool IsUnitedKingdom => RN_Code == Constants.CountryCodes.UnitedKingdom;

		public bool IsNorthernIreland => RN_Code == Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes;

		public bool IsRecognisedByEuropeanUnionAsIssuerOfThirdCountryUniqueIdentificationNumbersOrAEO
		{
			get
			{
				// http://customs.hmrc.gov.uk/channelsPortalWebApp/channelsPortalWebApp.portal?_nfpb=true&_pageLabel=pageImport_ShowContent&propertyType=document&resetCT=true&id=HMCE_PROD_008051
				return RN_Code == Constants.CountryCodes.Japan ||
							RN_Code == Constants.CountryCodes.UnitedStates ||
							RN_Code == Constants.CountryCodes.Switzerland ||
							RN_Code == Constants.CountryCodes.Norway ||
							RN_Code == Constants.CountryCodes.SanMarino ||
							RN_Code == Constants.CountryCodes.Liechtenstein ||
							RN_Code == Constants.CountryCodes.Vatican ||
							RN_Code == Constants.CountryCodes.Andorra ||
							RN_Code == Constants.CountryCodes.Canada ||
							RN_Code == Constants.CountryCodes.China;
			}
		}

		public bool IsFranceOrTerritory
		{
			get
			{
				return Constants.CountryCodes.IsFranceOrTerritory(RN_Code);
			}
		}

		[ReadOnly(true)]
		[List("Lookups.ListOfEconomicGroups")]
		public override ZString RN_EconomicGrouping
		{
			get { return base.RN_EconomicGrouping == EconomicGroupList.Codes.EuropeanUnion && RN_Code == Constants.CountryCodes.UnitedKingdom && AccountingMasterFilesRegistry.Instance.SimulateGBOutOfEU.Value ? ZString.Empty : base.RN_EconomicGrouping; }
			set { base.RN_EconomicGrouping = value; }
		}

		public bool UseEUVatDescription
		{
			get
			{
				return IsPartOfEuropeanUnion ||
					RN_Code == Constants.CountryCodes.Switzerland ||
					RN_Code == Constants.CountryCodes.Norway ||
					RN_Code == Constants.CountryCodes.UnitedKingdom ||
					RN_Code == Constants.CountryCodes.Turkey ||
					RN_Code == Constants.CountryCodes.Ghana;
			}
		}

		public bool UseEUAirCargoSecurityStandards
		{
			get
			{
				return IsPartOfEuropeanUnion ||
					RN_Code == Constants.CountryCodes.UnitedKingdom ||
					RN_Code == Constants.CountryCodes.Switzerland ||
					RN_Code == Constants.CountryCodes.Iceland ||
					RN_Code == Constants.CountryCodes.Liechtenstein ||
					RN_Code == Constants.CountryCodes.Norway;
			}
		}

		public bool IsInEFTA
		{
			get
			{
				return RN_Code == Constants.CountryCodes.Switzerland ||
					RN_Code == Constants.CountryCodes.Iceland ||
					RN_Code == Constants.CountryCodes.Liechtenstein ||
					RN_Code == Constants.CountryCodes.Norway;
			}
		}

		public bool IsBLNS
		{
			get
			{
				return RN_EconomicGrouping == EconomicGroupList.Codes.BLNS;
			}
		}

		public bool IsPartOfEuOrGspPlusOCT
		{
			get
			{
				if (!isPartOfEuOrGspPlusOCT.HasValue)
				{
					if (IsPartOfEuropeanUnion)
					{
						isPartOfEuOrGspPlusOCT = true;
					}
					else
					{
						var loader = CargoWise.Application.ObjectFactory.Get<ICusRefTradeGroupViewLoader>("ICusRefTradeGroupViewLoader", new object[] { Factory });
						isPartOfEuOrGspPlusOCT = loader.IsCountryPartOfTradeGroupAny(RN_Code, new ZString[] { Constants.Customs.Universal.RefCusTradeGroup.Codes.EuropeanUnionGsp, Constants.Customs.Universal.RefCusTradeGroup.Codes.EuropeanUnionGspPlus }, Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDateTime.Now);
					}
				}
				return isPartOfEuOrGspPlusOCT.Value;
			}
		}
		bool? isPartOfEuOrGspPlusOCT;
		#endregion

		#region RN_PostcodeValidationRule
		[List("Lookups.PostCodeValidationRules")]
		public override ZString RN_PostcodeValidationRule
		{
			get
			{
				return base.RN_PostcodeValidationRule;
			}
			set
			{
				base.RN_PostcodeValidationRule = value;
			}
		}

		public PostcodeFormattingRule PostcodeFormattingRule => ObjectFactory.Get<IPostcodeFormattingRulesProvider>().GetRuleFromIso(Code);

		#endregion

		#region RN_StateProvinceValidationRule
		[List("Lookups.StateAndProvinceValidationRules")]
		public override ZString RN_StateProvinceValidationRule
		{
			get
			{
				return base.RN_StateProvinceValidationRule;
			}
			set
			{
				base.RN_StateProvinceValidationRule = value;
			}
		}
		#endregion

		#region RN_AddressFormattingRule
		[List("Lookups.CountryAddressFormattingRules")]
		public override ZString RN_AddressFormattingRule
		{
			get
			{
				return base.RN_AddressFormattingRule;
			}
			set
			{
				base.RN_AddressFormattingRule = value;
			}
		}
		#endregion

		#region RN_RX_NKAirWaybillCurrency
		[ReadOnly(true)]
		public override ZString RN_RX_NKAirWaybillCurrency
		{
			get
			{
				return base.RN_RX_NKAirWaybillCurrency;
			}
			set
			{
				base.RN_RX_NKAirWaybillCurrency = value;
			}
		}
		#endregion

		#region RN_IsSystem

		public override ZBool RN_IsSystem
		{
			get { return base.RN_IsSystem; }
			set
			{
				if (base.RN_IsSystem != value)
				{
					base.RN_IsSystem = value;
					RN_CodeInfo.RefreshBinding();
					RN_RX_NKLocalCurrencyInfo.RefreshBinding();
					RN_DescInfo.RefreshBinding();
					RN_IsoAlpha3CodeInfo.RefreshBinding();
					RN_IsoNumericUNM49CodeInfo.RefreshBinding();
				}
			}
		}

		public bool RN_IsSystem_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region RN_IsoAlpha3Code

		[ReadOnlyMember(Schema.RN_IsSystem)]
		public override ZString RN_IsoAlpha3Code
		{
			get
			{
				return base.RN_IsoAlpha3Code;
			}
			set
			{
				base.RN_IsoAlpha3Code = value;
			}
		}

		#endregion

		#region RN_IsoNumericUNM49Code

		[ReadOnlyMember(Schema.RN_IsSystem)]
		public override ZString RN_IsoNumericUNM49Code
		{
			get
			{
				return base.RN_IsoNumericUNM49Code;
			}
			set
			{
				base.RN_IsoNumericUNM49Code = value;
			}
		}

		#endregion

		#region IsSpanishSpeakingCountry

		public ZBool IsSpanishSpeakingCountry
		{
			get { return SpanishSpeakingCountries.Any(code => code == RN_Code); }
		}

		ZString[] SpanishSpeakingCountries
		{
			get
			{
				return spanishSpeakingCountries ?? (spanishSpeakingCountries = new ZString[]
					{
						"ES", "AR", "BO", "CO", "CL", "CR", "CU",
						"DO", "EC", "SV", "GQ", "GT", "HN", "MX",
						"NI", "PE", "PA", "PR", "UY"
					});
			}
		}

		ZString[] spanishSpeakingCountries;

		#endregion

		#region RN_ValidationStatus

		[List("Lookups.ExternalAddressValidationRules")]
		public override ZString RN_ValidationStatus
		{
			get
			{
				return base.RN_ValidationStatus;
			}
			set
			{
				base.RN_ValidationStatus = value;
			}
		}

		public bool RN_ValidationStatus_ReadOnly
		{
			get { return true; }
		}

		public bool RN_ValidationStatus_HasAvailableData
		{
			get { return RN_ValidationStatus != ExternalAddressValidationRulesList.Codes.NotAvailable; }
		}

		#endregion

		#region IsStateMustNotBeEntered

		public ZBool IsStateMustNotBeEntered => RN_StateProvinceValidationRule == CountryAddressValidationRuleList.Codes.MustNotBeEntered;

		#endregion

		#endregion

		#region Related Business Objects

		#region Exchange Rate

		ExchangeRate fExchangeRate;

		public ExchangeRate ExchangeRate
		{
			get
			{
				if (fExchangeRate == null)
				{
					fExchangeRate = new ExchangeRate(GlbCompany.CurrentCompany.GC_IsReciprocal, GlbCompany.CurrentCompany.LocalCurrency.Decimals, GlbCompany.CurrentCompany.PK.ToGuid());
				}
				return fExchangeRate;
			}
		}

		#endregion

		#region States

		[ChildEditable(true)]
		public RefCountryStatesDependentCollection States
		{
			get
			{
				if (fStates == null)
				{
					var localStates = new RefCountryStatesDependentCollection(this, Factory);
					localStates.Load();
					fStates = localStates;
					RegisterEditableChildObject(fStates);
				}
				return fStates;
			}
		}
		RefCountryStatesDependentCollection fStates;

		#endregion

		#region Rules

		[ChildEditable(true)]
		public RefCountryRulesCollection Rules
		{
			get
			{
				if (fRules == null)
				{
					fRules = new RefCountryRulesCollection(Factory);
					fRules.LoadCountry(this);
					RegisterEditableChildObject(fRules);
				}

				return fRules;
			}
		}
		RefCountryRulesCollection fRules;

		#endregion

		#region Compliance Rules

		public IComplianceRuleCollection ComplianceRules
		{
			get
			{
				if (fComplianceRules == null)
				{
					fComplianceRules = Activator.CreateInstance(ObjectFactory.GetType<IComplianceRuleCollection>(), Factory) as IComplianceRuleCollection;
					fComplianceRules.LoadComplianceRules(RN_Code);
					RegisterEditableChildObject(fComplianceRules as IBusiness);
					if (!Env.Security.CountriesManageSanctions.IsAllowed)
					{
						fComplianceRules.SetReadOnlyIncludingChildren(true);
					}
				}

				return fComplianceRules;
			}
		}

		IComplianceRuleCollection fComplianceRules;

		#endregion

		#region RequiredDocuments

		[ChildEditable(true)]
		public RefCountryRequiredDocumentCollection RequiredDocuments
		{
			get
			{
				if (requiredDocuments == null)
				{
					requiredDocuments = new RefCountryRequiredDocumentCollection(Factory);
					requiredDocuments.LoadCountry(this);
					RegisterEditableChildObject(requiredDocuments);
				}

				return requiredDocuments;
			}
		}
		RefCountryRequiredDocumentCollection requiredDocuments;

		#endregion

		#endregion

		#region Contains UNLOCO

		public bool ContainsUNLOCO(RefUNLOCO uNLOCO)
		{
			return uNLOCO != null && uNLOCO.RL_RN_NKCountryCode == RN_Code;
		}

		public bool ContainsUNLOCO(ZString uNLOCOCode)
		{
			return uNLOCOCode.Length >= 2 && RN_Code.Length == 2 && uNLOCOCode.Left(2) == RN_Code;
		}

		#endregion

		#region ICountry Members

		string ICountry.Code
		{
			get { return this.RN_Code; }
		}

		CultureInfo ICountry.Culture
		{
			get
			{
				return culture ?? (culture = Culture.GetCulture(RN_Code));
			}
		}
		CultureInfo culture;

#if DEBUG
		IDisposable ICountry.SetCultureForTest(CultureInfo culture)
		{
			if (!Globals.IsTest)
			{
				throw new NotSupportedException("Setting of culture directly is only supported in test cases.");
			}

			this.culture = culture;
			return new DisposableAction(delegate
			{ this.culture = null; });
		}
#endif

		ICurrency ICountry.Currency
		{
			get { return this.LocalCurrency; }
		}

		string ICountry.Description
		{
			get { return RN_DescMultilingual; }
		}

		Guid ICountry.PK
		{
			get { return PK.ToGuid(); }
		}

		string ICountry.ConsumptionTaxDescription
		{
			get { return this.ConsumptionTaxDescription; }
		}

		#endregion

		#region ILocationBiz Members

		public ZString Code
		{
			get { return RN_Code; }
			set { RN_Code = value; }
		}

		public ZPropertyInfo CodeInfo
		{
			get { return RN_CodeInfo; }
		}

		public ZString Description
		{
			get { return RN_DescMultilingual; }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return RN_DescInfo; }
		}

		public ZBool IsActive
		{
			get { return RN_IsActive; }
		}

		public ZPropertyInfo IsActiveInfo
		{
			get { return RN_IsActiveInfo; }
		}

		RefCityTown ILocation.CityTown
		{
			get { return null; }
		}

		RefCountry ILocation.Country
		{
			get { return this; }
		}

		RefCountryStates ILocation.State
		{
			get { return null; }
		}

		[MaxLength(100)]
		ZString ILocationBiz.StateDescription
		{
			get { return ZString.Empty; }
		}

		ZPropertyInfo ILocationBiz.StateDescriptionInfo
		{
			get { return GetZPropertyInfo("StateDescription"); }
		}

		RefUNLOCO ILocation.UNLOCO
		{
			get { return null; }
		}

		IATACityCode ILocation.IATACityCode => null;

		RefZoneHeader[] ILocation.Zones
		{
			get
			{
				RefCountryZoneCollection loadedZones = new RefCountryZoneCollection(this);
				loadedZones.Load();
				zones = (RefZoneHeader[])loadedZones.ToArray(typeof(RefZoneHeader));
				return zones;
			}
		}

		RefZoneHeader[] zones;

		bool ILocationReference.IsLocalInRelationTo(ZString code)
		{
			return code.Length > 1 && code.SubstringSafe(0, 2) == RN_Code;
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.Country);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		public bool SupportDeclarationOfIntent
		{
			get
			{
				return Code == Core.Constants.CountryCodes.Italy;
			}
		}

		#region Government Tax Invoice

		public bool HasGovtTaxInvoice
		{
			get
			{
				return (Code == Core.Constants.CountryCodes.Taiwan) || SupportComplianceSubType;
			}
		}

		public bool SupportDocumentSigning
		{
			get
			{
				var supportedCountries = new string[] { Core.Constants.CountryCodes.Portugal };
				return supportedCountries.Contains(GlbCompany.CurrentCompany.Country.Code.ToString());
			}
		}

		public bool SupportComplianceSubType
		{
			get
			{
				bool result = false;
				foreach (ComplianceSubTypeAttributionRuleConfiguration config in ComplianceSubTypeRules)
				{
					if (Code == config.Country)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public bool HasAccComplianceSequence
		{
			get
			{
				return SupportComplianceSubType && Code != Constants.CountryCodes.China;
			}
		}

		public bool ComplianceSubTypeIncludeAPLedger
		{
			get
			{
				bool result = false;
				foreach (ComplianceSubTypeAttributionRuleConfiguration config in ComplianceSubTypeRules)
				{
					if ((config.LedgerType == LedgerTypes.AccountsPayable) && (Code == config.Country))
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		ComplianceSubTypeAttributionRuleConfigurationCollection ComplianceSubTypeRules
		{
			get
			{
				if (complianceSubTypeRules == null)
				{
					complianceSubTypeRules = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration_ByTransactionHeaderBranch.Value;
					if (!complianceSubTypeRules.Any())
					{
						complianceSubTypeRules = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.Value;
					}
				}
				return complianceSubTypeRules;
			}
		}
		ComplianceSubTypeAttributionRuleConfigurationCollection complianceSubTypeRules;

		public ComplianceSubTypeAttributionRuleConfigurationCollection ComplianceSubTypeRules_ForTestOnly => ComplianceSubTypeRules;

		#endregion

		#region Local Business Registration Number Type

		public string LocalBusinessRegNoCodeType
		{
			get { return CountryComplianceInfo.GetLocalBusinessRegNoCodeType(Code); }
		}

		#endregion

		#region Tax / Licence Builder Logic

		public virtual string ConsumptionTaxDescription
		{
			get { return Country.GetConsumptionTaxDescription(RN_Code); }
		}

		public virtual string ConsumptionTaxRegistrationCode
		{
			get { return Country.GetConsumptionTaxRegistrationOrgCusCode(RN_Code); }
		}

		public bool IsGSTRegistered
		{
			get { return Country.GetIsGSTRegistered(RN_Code); }
		}

		public static bool IsGSTCashBasis(string countryCode)
		{
			return Country.IsGSTCashBasis(countryCode);
		}

		#endregion

		#region Licence Key Builder

		public static bool IsSupportedForLicenceBuilder(string countryCode)
		{
			return Country.IsSupportedForLicenceBuilder(countryCode);
		}

		#endregion

		#region Screening Log Collection

		[ChildEditable]
		[ChildEditableTestExclude]
		public IStmEntityScreeningLogCollection ScreeningLogCollection
		{
			get
			{
				if (screeningLogCollection == null)
				{
					screeningLogCollection = (IStmEntityScreeningLogCollection)Activator.CreateInstance(ObjectFactory.GetType<IStmEntityScreeningLogCollection>(), this);
					RegisterEditableChildObject((IBusiness)screeningLogCollection);
				}
				return screeningLogCollection;
			}
		}
		IStmEntityScreeningLogCollection screeningLogCollection;

		#endregion

		#region Holidays

		public DayOfWeekCodeList WeekDays => Factory.GetCachedValue<DayOfWeekCodeList>();

		[ChildEditable(true)]
		public GlbHolidayCountryStatesCollection Holidays
		{
			get
			{
				if (fHolidays == null)
				{
					fHolidays = new GlbHolidayCountryStatesCollection(this, Factory, GlbHolidayCountryStateCollectionTypes.Holiday);
					RegisterEditableChildObject(fHolidays);
				}
				return fHolidays;
			}
		}
		GlbHolidayCountryStatesCollection fHolidays;

		[ChildEditable(true)]
		public GlbHolidayCountryStatesCollection Weekends
		{
			get
			{
				if (fWeekends == null)
				{
					fWeekends = new GlbHolidayCountryStatesCollection(this, Factory, GlbHolidayCountryStateCollectionTypes.Weekend);
					RegisterEditableChildObject(fWeekends);
				}
				return fWeekends;
			}
		}
		GlbHolidayCountryStatesCollection fWeekends;

		void AddOrEditWeekendDay(string dayOfWeek, bool isNonWorkingDay)
		{
			if (!Weekends.Any())
			{
				foreach (var weekDay in WeekDays.GetAllCodes())
				{
					var holiday = Weekends.AddNew();
					holiday.GH_RecurrDay = weekDay;
					holiday.GH_ParentID = PK;
					holiday.GH_ParentTableCode = TablePrefix;
					holiday.GH_Recurring = true;
					holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
					holiday.GH_IsWorkingDay = true;
					holiday.GH_HolidayName = WeekDays.GetDescriptionFromCode(weekDay);
				}
			}
			var weekendRecord = Weekends.FirstOrDefault(x => x.GH_RecurrDay == dayOfWeek);
			weekendRecord.GH_IsWorkingDay = !isNonWorkingDay;
		}

		ZBool GetIsNonWorkingDayByWeekday(string weekDay)
		{
			var weekend = Weekends.FirstOrDefault(x => x.GH_RecurrDay == weekDay);
			return weekend != null && !weekend.GH_IsWorkingDay;
		}

		public ZBool AreAnyWeekendsDefined => WeekDays.GetAllCodes().Any(day => GetIsNonWorkingDayByWeekday(day));

		public ZBool IsWorkingDay(DayOfWeek dayOfWeek)
		{
			return !GetIsNonWorkingDayByWeekday(HolidayCodes.GetCodeFromDay(dayOfWeek));
		}

		public ZBool IsMondayNonWorkingDay
		{
			get
			{
				if (!fIsMondayNonWorkingDay.HasValue)
				{
					fIsMondayNonWorkingDay = GetIsNonWorkingDayByWeekday(AutoDayOfWeekCodeList.Codes.Monday);
				}

				return fIsMondayNonWorkingDay.Value;
			}
			set
			{
				if (fIsMondayNonWorkingDay != value)
				{
					fIsMondayNonWorkingDay = value;
					AddOrEditWeekendDay(AutoDayOfWeekCodeList.Codes.Monday, value);
					SetPropertyValue(IsMondayNonWorkingDayInfo, value);
				}
			}
		}
		ZBool? fIsMondayNonWorkingDay;
		ZPropertyInfo IsMondayNonWorkingDayInfo
		{
			get { return GetZPropertyInfo(nameof(IsMondayNonWorkingDay)); }
		}

		public ZBool IsTuesdayNonWorkingDay
		{
			get
			{
				if (!fIsTuesdayNonWorkingDay.HasValue)
				{
					fIsTuesdayNonWorkingDay = GetIsNonWorkingDayByWeekday(AutoDayOfWeekCodeList.Codes.Tuesday);
				}

				return fIsTuesdayNonWorkingDay.Value;
			}
			set
			{
				if (fIsTuesdayNonWorkingDay != value)
				{
					fIsTuesdayNonWorkingDay = value;
					AddOrEditWeekendDay(AutoDayOfWeekCodeList.Codes.Tuesday, value);
					SetPropertyValue(IsTuesdayNonWorkingDayInfo, value);
				}
			}
		}
		ZBool? fIsTuesdayNonWorkingDay;
		ZPropertyInfo IsTuesdayNonWorkingDayInfo
		{
			get { return GetZPropertyInfo(nameof(IsTuesdayNonWorkingDay)); }
		}

		public ZBool IsWednesdayNonWorkingDay
		{
			get
			{
				if (!fIsWednesdayNonWorkingDay.HasValue)
				{
					fIsWednesdayNonWorkingDay = GetIsNonWorkingDayByWeekday(AutoDayOfWeekCodeList.Codes.Wednesday);
				}

				return fIsWednesdayNonWorkingDay.Value;
			}
			set
			{
				if (fIsWednesdayNonWorkingDay != value)
				{
					fIsWednesdayNonWorkingDay = value;
					AddOrEditWeekendDay(AutoDayOfWeekCodeList.Codes.Wednesday, value);
					SetPropertyValue(IsWednesdayNonWorkingDayInfo, value);
				}
			}
		}
		ZBool? fIsWednesdayNonWorkingDay;
		ZPropertyInfo IsWednesdayNonWorkingDayInfo
		{
			get { return GetZPropertyInfo(nameof(IsWednesdayNonWorkingDay)); }
		}

		public ZBool IsThursdayNonWorkingDay
		{
			get
			{
				if (!fIsThursdayNonWorkingDay.HasValue)
				{
					fIsThursdayNonWorkingDay = GetIsNonWorkingDayByWeekday(AutoDayOfWeekCodeList.Codes.Thursday);
				}

				return fIsThursdayNonWorkingDay.Value;
			}
			set
			{
				if (fIsThursdayNonWorkingDay != value)
				{
					fIsThursdayNonWorkingDay = value;
					AddOrEditWeekendDay(AutoDayOfWeekCodeList.Codes.Thursday, value);
					SetPropertyValue(IsThursdayNonWorkingDayInfo, value);
				}
			}
		}
		ZBool? fIsThursdayNonWorkingDay;
		ZPropertyInfo IsThursdayNonWorkingDayInfo
		{
			get { return GetZPropertyInfo(nameof(IsThursdayNonWorkingDay)); }
		}

		public ZBool IsFridayNonWorkingDay
		{
			get
			{
				if (!fIsFridayNonWorkingDay.HasValue)
				{
					fIsFridayNonWorkingDay = GetIsNonWorkingDayByWeekday(AutoDayOfWeekCodeList.Codes.Friday);
				}
				return fIsFridayNonWorkingDay.Value;
			}
			set
			{
				if (fIsFridayNonWorkingDay != value)
				{
					fIsFridayNonWorkingDay = value;
					AddOrEditWeekendDay(AutoDayOfWeekCodeList.Codes.Friday, value);
					SetPropertyValue(IsFridayNonWorkingDayInfo, value);
				}
			}
		}
		ZBool? fIsFridayNonWorkingDay;
		ZPropertyInfo IsFridayNonWorkingDayInfo
		{
			get { return GetZPropertyInfo(nameof(IsFridayNonWorkingDay)); }
		}

		public ZBool IsSaturdayNonWorkingDay
		{
			get
			{
				if (!fIsSaturdayNonWorkingDay.HasValue)
				{
					fIsSaturdayNonWorkingDay = GetIsNonWorkingDayByWeekday(AutoDayOfWeekCodeList.Codes.Saturday);
				}

				return fIsSaturdayNonWorkingDay.Value;
			}
			set
			{
				if (fIsSaturdayNonWorkingDay != value)
				{
					fIsSaturdayNonWorkingDay = value;
					AddOrEditWeekendDay(AutoDayOfWeekCodeList.Codes.Saturday, value);
					SetPropertyValue(IsSaturdayNonWorkingDayInfo, value);
				}
			}
		}
		ZBool? fIsSaturdayNonWorkingDay;
		ZPropertyInfo IsSaturdayNonWorkingDayInfo
		{
			get { return GetZPropertyInfo(nameof(IsSaturdayNonWorkingDay)); }
		}

		public ZBool IsSundayNonWorkingDay
		{
			get
			{
				if (!fIsSundayNonWorkingDay.HasValue)
				{
					fIsSundayNonWorkingDay = GetIsNonWorkingDayByWeekday(AutoDayOfWeekCodeList.Codes.Sunday);
				}

				return fIsSundayNonWorkingDay.Value;
			}
			set
			{
				if (fIsSundayNonWorkingDay != value)
				{
					fIsSundayNonWorkingDay = value;
					AddOrEditWeekendDay(AutoDayOfWeekCodeList.Codes.Sunday, value);
					SetPropertyValue(IsSundayNonWorkingDayInfo, value);
				}
			}
		}
		ZBool? fIsSundayNonWorkingDay;
		ZPropertyInfo IsSundayNonWorkingDayInfo
		{
			get { return GetZPropertyInfo(nameof(IsSundayNonWorkingDay)); }
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);  // The default handler "randomly" generates code "AX" every time, which is a real country
			RN_Code = "ZZ";
		}
#endif
	}
}
