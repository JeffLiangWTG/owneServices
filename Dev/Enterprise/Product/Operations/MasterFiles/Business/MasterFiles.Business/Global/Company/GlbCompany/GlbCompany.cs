using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.AddressCleansing.Common;

#if DEBUG
using Enterprise.MasterFiles.Business.Testing;
#endif

namespace Enterprise.MasterFiles.Business
{
	[AllowAllObjectsToBeLoaded]
	[DescriptionProperty(GlbCompany.Schema.GC_Name)]
	[DebuggerDisplay("Company ({" + GlbCompany.Schema.GC_Code + "})")]
	[SystemDefinedValues]
	public class GlbCompany : AutoGlbCompany
		, IDocManagerSupport
		, IGlbCompany
		, IAddressDetails
		, ICompany
		, ISupportWebAddressValidation
		, IDataVersionLoggingSupported
		, ICustomsNumberViewStmNumsParent
		, ICombinedEInvoicingCredentials
	{
		public GlbCompany(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static Guid FirstActiveBranchPK(ZString companyCode, BusinessObjectFactory factory = null)
		{
			var result = Guid.Empty;
			factory = factory ?? new BusinessObjectFactory();
			var company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);
			if (company != null)
			{
				var branch = company.FirstActiveBranch;
				if (branch != null)
				{
					result = branch.PK.ToGuid();
				}
			}
			return result;
		}

		public static GlbCompany GetDemoCompany(BusinessObjectFactory factory)
		{
			return factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, DemoCompanyCode);
		}

		#region Schema

		public new class Schema : AutoGlbCompany.Schema
		{
			public const string LicenceServerID = "LicenceServerID";
			public const string LicenceEnterpriseCode = "LicenceEnterpriseCode";
			public const string GC_IsWHTAccrualBasis = "GC_IsWHTAccrualBasis";
			public const string GC_Phone_Formatted = "GC_Phone_Formatted";
			public const string GC_Phone_FormattedLocalNumberIfLoggedInSameCountry = "GC_Phone_FormattedLocalNumberIfLoggedInSameCountry";
			public const string GC_Phone_IsManuallyVerified = "GC_Phone_IsManuallyVerified";
			public const string GC_Fax_Formatted = "GC_Fax_Formatted";
			public const string GC_Fax_IsManuallyVerified = "GC_Fax_IsManuallyVerified";
			public const string GC_Fax_FormattedLocalNumberIfLoggedInSameCountry = "GC_Fax_FormattedLocalNumberIfLoggedInSameCountry";
			public const int LanguageMaxLength = 3;
		}

		#endregion

		#region BusinessObjectsWithRelatedEventsCore

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(AccCFXConfigurations);
				result.AddRange(AccExchangeRateConfigurations.Where(x => x.Level == AccExRateConfigurationLevelEnum.Company));
				return result.ToArray();
			}
		}

		#endregion

		#region Related Business Objects

		#region Branches

		[ChildEditable(true)]
		public GlbBranchDependentCollection Branches
		{
			get
			{
				if (fBranches == null)
				{
					fBranches = new GlbBranchDependentCollection(this, Factory);
					RegisterEditableChildObject(fBranches);
				}
				return fBranches;
			}
		}
		protected GlbBranchDependentCollection fBranches;

		#endregion

		public GlbBranch FirstActiveBranch
		{
			get { return ActiveBranches.OrderBy(b => b.GB_Code).FirstOrDefault(); }
		}

		public IEnumerable<GlbBranch> ActiveBranches
		{
			get { return Branches.Where(branch => branch.GB_IsActive); }
		}

		public string FirstActiveBranchCode
		{
			get { return ActiveBranches.Select(b => b.GB_Code).OrderBy(b => b).FirstOrDefault(b => !string.IsNullOrEmpty(b)); }
		}

		public bool HasOnlyOneActiveBranch()
		{
			return ActiveBranches.IsCountEqualTo(1);
		}

		public bool IsBranchActive(string branchCode)
		{
			return ActiveBranches.Any(b => b.GB_Code == branchCode);
		}

		#region BatchProcessorLogs

		public StmALogCollection BatchProcessorLogs
		{
			get
			{
				if (fBatchProcessorLogs == null)
				{
					ZQuery filter = new ZQuery();
					filter.AddToFilter(StmALogSchema.SL_Parent, PK);
					filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.BatchProcessorLog.Code);

					var localBatchProcessorLogs = new StmALogCollection(Factory, filter);
					localBatchProcessorLogs.Load();
					fBatchProcessorLogs = localBatchProcessorLogs;
				}
				return fBatchProcessorLogs;
			}
		}

		StmALogCollection fBatchProcessorLogs;

		#endregion

		#endregion

		#region Properties

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => true;

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		#endregion

		#region CurrentCompany

#if DEBUG
		[TestExcludeDetectStaticBusinessObjectsCollectionsAndFactories]
#endif
		public static GlbCompany CurrentCompany
		{
			get { return (GlbCompany)Env.CurrentCompany; }
		}

		public static GlbCompany GetCurrentCompany(BusinessObjectFactory factory)
		{
			var company = CurrentCompany;
			return company != null && factory != company.Factory ? factory.Load<GlbCompany>(company.PK) : company;
		}

		#endregion

		#region IsDemoCompany

		public const string DemoCompanyCode = "DEM";

		public bool IsDemoCompany
		{
			get { return (GC_Code == DemoCompanyCode); }
		}

		#endregion

		#region LicenceBusinessRegNo

		public static string LicenceBusinessRegNoType(RefCountry country)
		{
			if (country == null)
			{
				return "";
			}

			switch (country.Code)
			{
				case Core.Constants.CountryCodes.Australia:
				case Core.Constants.CountryCodes.Belgium:
				case Core.Constants.CountryCodes.BurkinaFaso:
				case Core.Constants.CountryCodes.Colombia:
				case Core.Constants.CountryCodes.Guatemala:
				case Core.Constants.CountryCodes.Ethiopia:
				case Core.Constants.CountryCodes.Uganda:
				case Core.Constants.CountryCodes.Zambia:
				case Core.Constants.CountryCodes.Nigeria:
				case Core.Constants.CountryCodes.Venezuela:
				case Core.Constants.CountryCodes.Latvia:
				case Core.Constants.CountryCodes.Lithuania:
				case Core.Constants.CountryCodes.Slovenia:
				case Core.Constants.CountryCodes.Ecuador:
				case Core.Constants.CountryCodes.ElSalvador:
				case Core.Constants.CountryCodes.Paraguay:
				case Core.Constants.CountryCodes.Uruguay:
				case Core.Constants.CountryCodes.Azerbaijan:
				case Core.Constants.CountryCodes.Bermuda:
				case Core.Constants.CountryCodes.Bolivia:
				case Core.Constants.CountryCodes.FrenchPolynesia:
				case Core.Constants.CountryCodes.Honduras:
				case Core.Constants.CountryCodes.Kenya:
				case Core.Constants.CountryCodes.NewCaledonia:
				case Core.Constants.CountryCodes.Nicaragua:
				case Core.Constants.CountryCodes.Portugal:
				case Core.Constants.CountryCodes.Kuwait:
				case Core.Constants.CountryCodes.Mongolia:
				case Core.Constants.CountryCodes.Slovakia:
				case Core.Constants.CountryCodes.Botswana:
				case Core.Constants.CountryCodes.Kazakhstan:
				case Core.Constants.CountryCodes.Tanzania:
				case Core.Constants.CountryCodes.Mali:
				case Core.Constants.CountryCodes.Jordan:
				case Core.Constants.CountryCodes.Brunei:
				case Core.Constants.CountryCodes.Iraq:
				case Core.Constants.CountryCodes.Macau:
				case Core.Constants.CountryCodes.Bahrain:
				case Core.Constants.CountryCodes.Mozambique:
				case Core.Constants.CountryCodes.EquatorialGuinea:
				case Core.Constants.CountryCodes.Somalia:
					return OrgCusCode.CodeTypes.CorporationCode;
				case Core.Constants.CountryCodes.NewZealand:
					return OrgCusCode.CodeTypes.CompanyNumber;
				case Core.Constants.CountryCodes.CostaRica:
					return CostaRicaOrgCusCodeInfo.OrgCusCodes.BusinessIdentificationNumber;
				case Core.Constants.CountryCodes.Mauritius:
					return OrgCusCode.CodeTypes.BusinessRegistrationNumber;
				case Core.Constants.CountryCodes.Zimbabwe:
					return ZimbabweOrgCusCodeInfo.OrgCusCodes.BPN;
				case Core.Constants.CountryCodes.Lebanon:
					return OrgCusCode.LebanonCodeTypes.CRN;
				case Core.Constants.CountryCodes.Senegal:
					return OrgCusCode.SenegalCodeTypes.NIN;
				case Core.Constants.CountryCodes.CoteDivoire:
					return OrgCusCode.CoteDivoireCodeTypes.NRC;
				case Core.Constants.CountryCodes.Cameroon:
					return OrgCusCode.CameroonCodeTypes.NRC;
				case Core.Constants.CountryCodes.Yemen:
					return OrgCusCode.CodeTypes.CorporationCode;
				case Core.Constants.CountryCodes.Panama:
					return PanamaOrgCusCodeInfo.OrgCusCodes.NAO;
				case Core.Constants.CountryCodes.Algeria:
					return AlgeriaOrgCusCodeInfo.OrgCusCodes.NRC;
				case Core.Constants.CountryCodes.Malawi:
					return OrgCusCode.MalawiCodeTypes.BRN;
				case Core.Constants.CountryCodes.Niger:
					return OrgCusCode.NigerCodeTypes.RCC;
				case Core.Constants.CountryCodes.Palau:
					return OrgCusCode.PalauCodeTypes.EIN;
				case Core.Constants.CountryCodes.Cuba:
					return OrgCusCode.CubaCodeTypes.GCR;
				case Core.Constants.CountryCodes.Ghana:
				case Core.Constants.CountryCodes.Belarus:
				case Core.Constants.CountryCodes.SierraLeone:
				case Core.Constants.CountryCodes.Kiribati:
					return OrgCusCode.GhanaCodeTypes.GCR;
				default:
					return country.LocalBusinessRegNoCodeType;
			}
		}

		#endregion

		#region LicenceKeyIdentifier

		Lazy<IProductRegistrationKey> registrationKey = new Lazy<IProductRegistrationKey>(GetRegistrationKey);
		IProductRegistrationKey RegistrationKey => registrationKey.Value;

		static IProductRegistrationKey GetRegistrationKey()
		{
			return ObjectFactory.Get<IProductRegistration>().Key;
		}

		public ZString LicenceKeyIdentifier
		{
			get
			{
				return GetLicenceKeyIdentifier("");
			}
		}

		public string GetLicenceKeyIdentifier(string separator)
		{
			return RegistrationKey.EnterpriseCode + separator + GC_Code + separator + RegistrationKey.ServerCode;
		}

		public void UpdateLicenceKeyIdentifier()
		{
			registrationKey = new Lazy<IProductRegistrationKey>(GetRegistrationKey);
		}

		public string DatabaseType
		{
			get
			{
				return RegistrationKey.DatabaseType;
			}
		}

		public ZPropertyInfo LicenceKeyIdentifierInfo
		{
			get { return GetZPropertyInfo(nameof(LicenceKeyIdentifier)); }
		}

		#endregion

		#region IsLicenceKeyIdentifierValid

		public bool IsLicenceKeyIdentifierValid
		{
			get
			{
				return (!string.IsNullOrEmpty(RegistrationKey.EnterpriseCode) && !GC_Code.IsEmpty && !string.IsNullOrEmpty(RegistrationKey.ServerCode));
			}
		}

		#endregion

		#region LicenceServerID

		public ZString LicenceServerID
		{
			get
			{
				return RegistrationKey.ServerCode;
			}
		}

		public ZPropertyInfo LicenceServerIDInfo
		{
			get { return GetZPropertyInfo(Schema.LicenceServerID); }
		}

		#endregion

		#region LicenceEnterpriseCode

		public ZString LicenceEnterpriseCode
		{
			get
			{
				return RegistrationKey.EnterpriseCode;
			}
		}
		protected ZString licenceEnterpriseCode;

		public ZPropertyInfo LicenceEnterpriseCodeInfo
		{
			get { return GetZPropertyInfo(Schema.LicenceEnterpriseCode); }
		}

		#endregion

		#region GC_IsWHTAccrualBasis

		public ZBool GC_IsWHTAccrualBasis
		{
			get { return (GC_IsWHTRegistered && !GC_IsWHTCashBasis); }
		}

		public ZPropertyInfo GC_IsWHTAccrualBasisInfo
		{
			get { return GetZPropertyInfo(Schema.GC_IsWHTAccrualBasis); }
		}

		#endregion

		#region GC_RN_NKCountryCode

		[List("Lookups.Countries")]
		public override ZString GC_RN_NKCountryCode
		{
			get
			{
				return base.GC_RN_NKCountryCode;
			}
			set
			{
				if (GC_RN_NKCountryCode != value)
				{
					CheckMaximumLength(GC_RN_NKCountryCodeInfo, value);
					base.GC_RN_NKCountryCode = value;
					GC_Phone_Wrapper.RefreshFormat();
					GC_Fax_Wrapper.RefreshFormat();
					if (!IsMarkingAsNeedingValidationSuspended)
					{
						Branches.MarkAsNeedingValidationIncludingChildren();
					}
					ResetValidationStatus(GC_RN_NKCountryCodeInfo);
				}
			}
		}

		[List("Lookups.Countries")]
		public ZString AccountingCountry
		{
			get { return GC_RN_NKCountryCode; }
			set
			{
				if (value != GC_RN_NKCountryCode)
				{
					GC_RN_NKCountryCode = value;

					var country = Country;
					if (country != null)
					{
						GC_RX_NKLocalCurrency = country.RN_RX_NKLocalCurrency;
						GC_IsGSTRegistered = ZArchitecture.Environment.Country.GetIsGSTRegistered(GC_RN_NKCountryCode);
						GC_IsGSTCashBasis = ZArchitecture.Environment.Country.IsGSTCashBasis(GC_RN_NKCountryCode);

						try
						{
							GC_IsReciprocal = ZArchitecture.Environment.Country.IsReciprocal(GC_RN_NKCountryCode);
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
						}
					}
				}
			}
		}

		public ZPropertyInfo AccountingCountryInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(AccountingCountry), x => GC_RN_NKCountryCodeInfo); }
		}

		[Obsolete("Please use Country property. This will be removed once references in client-defined macros (Documents, Workflows, Notes, etc) are transformed.")]
		[MacroIgnore]
		public RefCountry CountryCode => Country;

		#endregion

		#region GC_Email

		[EmailAddress]
		public override ZString GC_Email
		{
			get
			{
				return base.GC_Email;
			}

			set
			{
				base.GC_Email = value;
			}
		}

		#endregion

		#region Address

		public override ZString GC_Address1
		{
			get { return base.GC_Address1; }
			set
			{
				if (value != base.GC_Address1)
				{
					base.GC_Address1 = value;
					ResetValidationStatus(GC_Address1Info);
				}
			}
		}

		public override ZString GC_Address2
		{
			get { return base.GC_Address2; }
			set
			{
				if (value != base.GC_Address2)
				{
					base.GC_Address2 = value;
					ResetValidationStatus(GC_Address2Info);
				}
			}
		}

		public override ZString GC_City
		{
			get { return base.GC_City; }
			set
			{
				if (value != base.GC_City)
				{
					base.GC_City = value;
					ResetValidationStatus(GC_CityInfo);
				}
			}
		}

		[List("Lookups.StateList")]
		public override ZString GC_State
		{
			get { return base.GC_State; }
			set
			{
				if (value != base.GC_State)
				{
					base.GC_State = value;
					ResetValidationStatus(GC_StateInfo);
				}
			}
		}

		public override ZString GC_PostCode
		{
			get { return base.GC_PostCode; }
			set
			{
				if (value != base.GC_PostCode)
				{
					base.GC_PostCode = value;
					ResetValidationStatus(GC_PostCodeInfo);
				}
			}
		}

		public ZBool StateListHasMembers
		{
			get { return Lookups.StateList.Count > 0; }
		}

		#endregion

		#region GC_Active

		public override ZBool GC_IsActive
		{
			get
			{
				return base.GC_IsActive;
			}
			set
			{
				base.GC_IsActive = value;
				Validation.ValidateAll();
				GC_Phone_Wrapper.FormattedForBindingInfo.RefreshBinding();
				foreach (GlbBranch branch in Branches)
				{
					branch.Validation.ValidateGB_IsActive();
					branch.GB_IsActiveInfo.RefreshBinding();
				}
			}
		}

		public bool IgnoreValidationStatusError { get; set; }

		#endregion

		#region GC_ValidationStatus

		public override ZString GC_ValidationStatus
		{
			get => base.GC_ValidationStatus;

			set
			{
				base.GC_ValidationStatus = value;
				isManuallyVerifiedByUser = value == AddressValidationStatus.ManuallyVerified;
			}
		}

		#endregion

		#region Business Registration Number Captions

		public ZString BusinessRegistrationNumberOneCaption
		{
			get
			{
				ZString result;
				if (Country != null)
				{
					switch (Country.RN_Code)
					{
						case Core.Constants.CountryCodes.Australia:
							result = Res.GetString("GlbCompany|RegNoLabels|ABN", "Australian Business Number");
							break;
						case Constants.CountryCodes.Chad:
							result = Res.GetString("GlbCompany|RegNoLabels|TVARegNo", "TVA Reg No");
							break;
						default:
							var consumptionTaxRegistrationCode = Country.ConsumptionTaxRegistrationCode;
							var taxName = !string.IsNullOrEmpty(consumptionTaxRegistrationCode) ? consumptionTaxRegistrationCode : OrgCusCode.CodeTypes.GSTCode;
							result = Res.GetString("e0dae1e4-23de-4fe5-91d7-fe6bd0c3fcbd", "{0} Reg No", taxName);
							break;
					}
				}
				else
				{
					result = Res.GetString("9d60d887-e7d2-43e3-8d9e-4bae90519ba1", "Business Registration Number 1");
				}

				return result;
			}
		}

		public ZString BusinessRegistrationNumberTwoCaption
		{
			get
			{
				ZString result;
				if (Country != null)
				{
					switch (Country.RN_Code)
					{
						case Core.Constants.CountryCodes.Australia:
							result = Res.GetString("GlbCompany|RegNoLabels|ACN", "Australian Company Number");
							break;
						default:
							result = Res.GetString("GlbCompany|RegNoLabels|CompanyRegNo", "Company Reg No");
							break;
					}
				}
				else
				{
					result = Res.GetString("3b37b3ee-ad9a-44fd-90cf-6754a01a46ac", "Business Registration Number 2");
				}

				return result;
			}
		}

		#endregion

		#region Has GST Registered Changed To Unregistered

		public bool HasGSTRegisteredChangedToUnregistered
		{
			get { return IsInDatabase && GC_IsGSTRegisteredInfo.HasChanges && GC_IsGSTRegistered == ZBool.False; }
		}

		#endregion

		#region GSTRateIsAlreadyUsedByTransactions

		public bool GSTRateIsAlreadyUsedByTransactions
		{
			get
			{
				DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
				ZSqlParameterCollection parameters = new ZSqlParameterCollection();
				parameters.Add(ZSqlParameter.New("@CompanyPK", PK, GlbCompanySchema.PK));
				collection.Load(@"Select top 1 AL_PK from dbo.AccTransactionLines
							inner join dbo.GlbBranch on AL_GB = GB_PK
							inner join dbo.GlbCompany on GB_GC = GC_PK
							where AL_AT is not null
							AND GC_PK = @CompanyPK", parameters);
				return collection.Count > 0;
			}
		}

		#endregion

		#region GC_Code_ReadOnly
		public bool GC_Code_ReadOnly
		{
			get
			{
				if (GlbStaff.CurrentUser.IsSupportUser)
				{
					return false;
				}
				else if (IsInDatabase)
				{
					return true;
				}

				return false;
			}
		}
		#endregion

		#region Phone Numbers

		#region GC_Phone

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString GC_Phone_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(GC_PhoneInfo); }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(GC_PhoneInfo, GC_Phone_FormattedInfo, value, Validation.ValidateGC_Phone_Formatted, GC_Phone_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo GC_Phone_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.GC_Phone_Formatted); }
		}

		public ZString GC_Phone_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(GC_PhoneInfo); }
		}

		public ZPropertyInfo GC_Phone_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.GC_Phone_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		public ZBool GC_Phone_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, GlbCompanySchema.Constants.GC_Phone, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(GC_Phone_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, GlbCompanySchema.Constants.GC_Phone, Validation.ValidateGC_Phone_Formatted, GC_Phone_Wrapper.FormattedForBindingInfo); }
		}

		public ZPropertyInfo GC_Phone_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.GC_Phone_IsManuallyVerified); }
		}

		public PhoneNumber GC_Phone_Wrapper
		{
			get { return gcPhoneWrapper ?? (gcPhoneWrapper = new PhoneNumber(GC_Phone_FormattedInfo, null, GC_Phone_FormattedLocalNumberIfLoggedInSameCountryInfo, GC_Phone_IsManuallyVerifiedInfo)); }
		}

		PhoneNumber gcPhoneWrapper;

		#endregion

		#region GC_Fax

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString GC_Fax_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(GC_FaxInfo); }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(GC_FaxInfo, GC_Fax_FormattedInfo, value, Validation.ValidateGC_Fax_Formatted, GC_Fax_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo GC_Fax_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.GC_Fax_Formatted); }
		}

		public ZString GC_Fax_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(GC_FaxInfo); }
		}

		public ZPropertyInfo GC_Fax_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.GC_Fax_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		public ZBool GC_Fax_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, GlbCompanySchema.Constants.GC_Fax, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(GC_Fax_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, GlbCompanySchema.Constants.GC_Fax, Validation.ValidateGC_Fax_Formatted, GC_Fax_Wrapper.FormattedForBindingInfo); }
		}

		public ZPropertyInfo GC_Fax_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.GC_Fax_IsManuallyVerified); }
		}

		public PhoneNumber GC_Fax_Wrapper
		{
			get { return gcFaxWrapper ?? (gcFaxWrapper = new PhoneNumber(GC_Fax_FormattedInfo, null, GC_Fax_FormattedLocalNumberIfLoggedInSameCountryInfo, GC_Fax_IsManuallyVerifiedInfo)); }
		}

		PhoneNumber gcFaxWrapper;

		#endregion

		#region AddOnRuleAcks

		[ChildEditable(true)]
		public GenCustomAddOnRuleAckCollection AddOnRuleAcks
		{
			get
			{
				if (addOnRuleAcks == null)
				{
					addOnRuleAcks = new GenCustomAddOnRuleAckCollection(this);
					RegisterEditableChildObject(addOnRuleAcks);
				}
				return addOnRuleAcks;
			}
		}

		GenCustomAddOnRuleAckCollection addOnRuleAcks;

		#endregion

		#region DefaultCountryCodeForPhoneNumbers

		internal ZString DefaultCountryCodeForPhoneNumbers
		{
			get { return GC_RN_NKCountryCode; }
		}

		#endregion

		#region Implementations

		PhoneNumberPropertyHelper PhoneNumberPropertyHelper
		{
			get { return phoneNumberPropertyHelper ?? (phoneNumberPropertyHelper = new PhoneNumberPropertyHelper(() => DefaultCountryCodeForPhoneNumbers)); }
		}

		PhoneNumberPropertyHelper phoneNumberPropertyHelper;

		#endregion

		#endregion

		#region SupportElectronicInvoicing

		public bool SupportsElectronicInvoicing => ObjectFactory.Get<IGlobalEInvoicingObjectFactory>().DoesCountrySupportElectronicInvoicing(GC_RN_NKCountryCode);

		#endregion

		#endregion

		#region Accounting Configuration

		#region CashAdvanceConfiguration

		[ChildEditable(true)]
		public AccCashAdvanceDefaultingConfigurationCollection CashAdvanceConfigurations
		{
			get
			{
				if (cashAdvanceConfigurations == null)
				{
					var localCashAdvanceConfigurations = new AccCashAdvanceDefaultingConfigurationCollection(Factory, PK);
					localCashAdvanceConfigurations.Load();
					cashAdvanceConfigurations = localCashAdvanceConfigurations;
					RegisterEditableChildObject(cashAdvanceConfigurations);
				}
				return cashAdvanceConfigurations;
			}
		}
		AccCashAdvanceDefaultingConfigurationCollection cashAdvanceConfigurations;

		#endregion

		#region AccountFeeSettings

		public AccountFeeSettings AccountFeeSettings
		{
			get
			{
				if (fAccountFeeSettings == null)
				{
					fAccountFeeSettings = new AccountFeeSettings(this.PK, this);
					fAccountFeeSettings.PopulateFields();
				}
				RegisterEditableChildObject(fAccountFeeSettings);
				return fAccountFeeSettings;
			}
		}
		AccountFeeSettings fAccountFeeSettings;

		#endregion

		#region AccCFXConfigurations
		[ChildEditable(true)]
		public AccCFXUpliftConfigurationCollection AccCFXConfigurations
		{
			get
			{
				if (accCFXConfigurations == null)
				{
					var localAccCFXConfigurations = new AccCFXUpliftConfigurationCollection(Factory, PK);
					localAccCFXConfigurations.Load();
					accCFXConfigurations = localAccCFXConfigurations;
					RegisterEditableChildObject(accCFXConfigurations);
				}
				return accCFXConfigurations;
			}
		}
		AccCFXUpliftConfigurationCollection accCFXConfigurations;
		#endregion

		#region AccExchangeRateConfigurations
		[ChildEditable(true)]
		public AccExchangeRateConfigurationCollection AccExchangeRateConfigurations
		{
			get
			{
				if (accExchangeRateConfigurations == null)
				{
					var localAccExchangeRateConfigurations = new AccExchangeRateConfigurationCollection(Factory, PK);
					localAccExchangeRateConfigurations.Load();
					accExchangeRateConfigurations = localAccExchangeRateConfigurations;
					RegisterEditableChildObject(accExchangeRateConfigurations);
				}
				return accExchangeRateConfigurations;
			}
		}
		AccExchangeRateConfigurationCollection accExchangeRateConfigurations;
		#endregion

		#region AccPlaceOfSupplyConfigurations

		[ChildEditable(true)]
		public AccPOSConfigurationCollection AccPlaceOfSupplyConfigurations
		{
			get
			{
				if (AccPlaceOfSupplyConfigurations_Internal == null)
				{
					var localAccPlaceOfSupplyConfigurations_Internal = new AccPOSConfigurationCollection(this);
					localAccPlaceOfSupplyConfigurations_Internal.Load();
					AccPlaceOfSupplyConfigurations_Internal = localAccPlaceOfSupplyConfigurations_Internal;
					RegisterEditableChildObject(AccPlaceOfSupplyConfigurations_Internal);
				}
				return AccPlaceOfSupplyConfigurations_Internal;
			}
		}
		AccPOSConfigurationCollection AccPlaceOfSupplyConfigurations_Internal;

		#endregion

		#region AccSurchargeConfigurations

		[ChildEditable(true)]
		public AccSurchargeConfigurationCollection AccSurchargeConfigurations
		{
			get
			{
				if (accSurchargeConfigurations == null)
				{
					var localAccSurchargeConfigurations = new AccSurchargeConfigurationCollection(Factory, PK);
					localAccSurchargeConfigurations.Load();
					accSurchargeConfigurations = localAccSurchargeConfigurations;
					RegisterEditableChildObject(accSurchargeConfigurations);
				}
				return accSurchargeConfigurations;
			}
		}
		AccSurchargeConfigurationCollection accSurchargeConfigurations;

		#endregion

		#region AccTaxConfigurations
		[ChildEditable(true)]
		public AccTaxConfigurationCollection AccTaxConfigurations
		{
			get
			{
				if (accTaxConfigurations == null)
				{
					accTaxConfigurations = new AccTaxConfigurationCollectionForCompanyOrBranch(this);
					RegisterEditableChildObject(accTaxConfigurations);
				}

				return accTaxConfigurations;
			}
		}
		AccTaxConfigurationCollection accTaxConfigurations;

		#endregion

		#region AccSurchargeApplications
		[ChildEditable(true)]
		public AccSurchargeApplicationCollection AccSurchargeApplications
		{
			get
			{
				if (accSurchargeApplications == null)
				{
					var localAccSurchargeApplications = new AccSurchargeApplicationCollection(Factory, PK);
					localAccSurchargeApplications.Load();
					accSurchargeApplications = localAccSurchargeApplications;
					RegisterEditableChildObject(accSurchargeApplications);
				}
				return accSurchargeApplications;
			}
		}
		AccSurchargeApplicationCollection accSurchargeApplications;
		#endregion

		#region GlbSignatureCredentials

		[ChildEditable(true)]
		public GlbCompanySignatureCredentialCollection SignatureCredentials
		{
			get
			{
				if (signatureCredentials == null)
				{
					signatureCredentials = new GlbCompanySignatureCredentialCollection(this);
					RegisterEditableChildObject(signatureCredentials);

					if (signatureCredentials.IsEditAllowed)
					{
						signatureCredentials.Load();
					}
				}
				return signatureCredentials;
			}
		}
		GlbCompanySignatureCredentialCollection signatureCredentials;

		#endregion

		#region HungaryEInvoicingCredentials

		public GlbCompanyExternalPasswordHUI HungaryEInvoicingCredentials
		{
			get
			{
				if (hungaryEInvoicingCredentials == null && GlbCompanyExternalPasswordHUI.IsAllowed(this))
				{
					hungaryEInvoicingCredentials = GlbCompanyExternalPasswordHUI.LoadForCompanyOrNew(Factory, this);
					RegisterEditableChildObject(hungaryEInvoicingCredentials);
				}
				return hungaryEInvoicingCredentials;
			}
		}
		GlbCompanyExternalPasswordHUI hungaryEInvoicingCredentials;

		#endregion

		#region PhilippinesEInvoicingCredentials

		public GlbCompanyExternalPasswordForPhilippines PhilippinesEInvoicingCredentials
		{
			get
			{
				if (philippinesEInvoicingCredentials == null && GlbCompanyExternalPasswordForPhilippines.IsAllowed(this))
				{
					philippinesEInvoicingCredentials = GlbCompanyExternalPasswordForPhilippines.New(this);
					RegisterEditableChildObject(philippinesEInvoicingCredentials);
				}
				return philippinesEInvoicingCredentials;
			}
		}
		GlbCompanyExternalPasswordForPhilippines philippinesEInvoicingCredentials;

		#endregion

		#region EInvoicingCredentials

		IEInvoicingCredentialSettings EInvoicingCredentialSettings
		{
			get
			{
				if (eInvoicingCredentialSettingsValue == null)
				{
					var objectFactory = ObjectFactory.Get<IGlobalEInvoicingObjectFactory>();
					var settings = objectFactory.GetCountryEInvoicingObjectFactorySettings(GC_RN_NKCountryCode);
					eInvoicingCredentialSettingsValue = settings?.Credentials;
				}
				return eInvoicingCredentialSettingsValue;
			}
		}
		IEInvoicingCredentialSettings eInvoicingCredentialSettingsValue;

		[ChildEditable(true)]
		public CombinedEInvoicingCertificateCollection EInvoicingCertificateCredentials => GetLazyEInvoicingCertificateCredentials().Value;

		Lazy<CombinedEInvoicingCertificateCollection> GetLazyEInvoicingCertificateCredentials()
		{
			CombinedEInvoicingCertificateCollection GetCollection()
			{
				var credentialSettings = EInvoicingCredentialSettings as IEInvoicingCertificateCredentialSettings;
				var collection = new CombinedEInvoicingCertificateCollection(new CombinedEInvoicingMaster(this), credentialSettings);
				RegisterEditableChildObject(collection);
				if (collection.IsEnabled && collection.IsEditAllowed)
				{
					collection.Load();
				}
				return collection;
			}

			return lazyEInvoicingCertificateCredentials ?? (lazyEInvoicingCertificateCredentials = new Lazy<CombinedEInvoicingCertificateCollection>(GetCollection));
		}
		Lazy<CombinedEInvoicingCertificateCollection> lazyEInvoicingCertificateCredentials;

		[ChildEditable(true)]
		public CombinedEInvoicingPasswordCollection EInvoicingPasswordCredentials => GetLazyEInvoicingPasswordCredentials().Value;

		Lazy<CombinedEInvoicingPasswordCollection> GetLazyEInvoicingPasswordCredentials()
		{
			CombinedEInvoicingPasswordCollection GetCollection()
			{
				var credentialSettings = EInvoicingCredentialSettings as IEInvoicingPasswordCredentialSettings;
				var collection = new CombinedEInvoicingPasswordCollection(new CombinedEInvoicingMaster(this), credentialSettings);
				RegisterEditableChildObject(collection);
				if (collection.IsEnabled && collection.IsEditAllowed)
				{
					collection.Load();
				}
				return collection;
			}

			return lazyEInvoicingPasswordCredentials ?? (lazyEInvoicingPasswordCredentials = new Lazy<CombinedEInvoicingPasswordCollection>(GetCollection));
		}
		Lazy<CombinedEInvoicingPasswordCollection> lazyEInvoicingPasswordCredentials;

		#endregion

		#region TemplateFiles

		[ChildEditable(true)]
		public AccTemplateFileStorageCollection TemplateFiles
		{
			get
			{
				if (templateFiles == null)
				{
					templateFiles = new AccTemplateFileStorageCollection(this);
					RegisterEditableChildObject(templateFiles);
					templateFiles.Load();
				}
				return templateFiles;
			}
		}
		AccTemplateFileStorageCollection templateFiles;

		#endregion

		#region EInvoicingTemplateFileConfigurations

		[ChildEditable(true)]
		public AccEInvoicingTemplateFileViewCollection EInvoicingTemplateFileConfigurations
		{
			get
			{
				if (eInvoicingTemplateFileConfigurations == null)
				{
					var localEInvoicingTemplateFileConfigurations = new AccEInvoicingTemplateFileViewCollection(Factory, PK);
					RegisterEditableChildObject(localEInvoicingTemplateFileConfigurations);
					localEInvoicingTemplateFileConfigurations.Load();
					eInvoicingTemplateFileConfigurations = localEInvoicingTemplateFileConfigurations;
				}
				return eInvoicingTemplateFileConfigurations;
			}
		}
		AccEInvoicingTemplateFileViewCollection eInvoicingTemplateFileConfigurations;

		public bool IsTemplateFileConfigurationsEnabled => this.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Turkey
				&& AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeatures;

		#endregion

		#endregion

		#region Country / UNLOCO Helpers

		public GlbBranch FirstBranchForUnLoco(RefUNLOCO unloco)
		{
			if (unloco != null)
			{
				if (BranchContainsPort(GlbBranch.CurrentBranch, unloco))
				{
					return GlbBranch.CurrentBranch;
				}
				foreach (GlbBranch branch in Branches)
				{
					if (BranchContainsPort(branch, unloco))
					{
						return branch;
					}
				}
			}

			return null;
		}

		public GlbBranch FirstActiveBranchForUnLoco(RefUNLOCO unloco)
		{
			if (unloco != null)
			{
				if (BranchContainsPort(GlbBranch.CurrentBranch, unloco))
				{
					return GlbBranch.CurrentBranch;
				}
				foreach (GlbBranch branch in ActiveBranches)
				{
					if (BranchContainsPort(branch, unloco))
					{
						return branch;
					}
				}
			}

			return null;
		}

		bool BranchContainsPort(GlbBranch branch, RefUNLOCO unloco)
		{
			if (branch.GB_RL_NKHomePort == unloco.Code)
			{
				return true;
			}

			foreach (GlbBranchExtraPorts port in branch.ExtraPorts)
			{
				if (port.GY_RL_NKAdditionalBranchRelatedPort == unloco.Code)
				{
					return true;
				}
			}

			return false;
		}

		public string ConsumptionTaxDescriptionForCompanyForm
		{
			get { return (Country == null || string.IsNullOrEmpty(Country.ConsumptionTaxDescription)) ? "VAT" : Country.ConsumptionTaxDescription; }
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			var envCompany = GlbCompany.CurrentCompany;
			if (envCompany != null)
			{
				// Hundreds of unit tests set the country, but not the currency.
				// Before May 2015, the default country and currency was from the current company, usually AU/AUD.
				// Since then there is no default, but the unit tests still need a valid currency, so...
				GC_RN_NKCountryCode = envCompany.GC_RN_NKCountryCode;
				GC_RX_NKLocalCurrency = envCompany.GC_RX_NKLocalCurrency;
			}

			//Make sure to also set 'empty', as the row might still be null (as we read null as empty)
			if (!GC_GeoLocation.IsValid || GC_GeoLocation.IsEmpty)
			{
				GC_GeoLocation = ZGeography.Empty;
			}

#if DEBUG
			if (Globals.IsTest)
			{
				if (GC_Code.IsEmpty)
				{
					NewBusinessObjectTestDataHelper().PopulateUniqueString(GC_CodeInfo, Array.Empty<PropertyDescriptor>());
				}

				if (GC_Name.IsEmpty)
				{
					GC_Name = "Company";
				}
			}
#endif

		}

		#endregion

		#region Saving / Deleting

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (!saveSucceeded)
			{
				foreach (var log in Logs.LogsNotInDB)
				{
					if (log.SL_Reference.StartsWith(BusinessRegistrationNumberOneCaption) || log.SL_Reference.StartsWith(BusinessRegistrationNumberTwoCaption))
					{
						log.Delete();
					}
				}
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsInDatabase || AccoutingPropertyHasChanges)
			{
				CopyOrCreateAccountingRelatedData();
			}

			if (EnableConfigurationToSender && ShouldSendCredential())
			{
				RegisterConfigurationForSending();
			}

			if (IsInDatabase)
			{
				if (GC_BusinessRegNoInfo.HasChanges)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, ZString.Format("{0}: {1} to {2}", BusinessRegistrationNumberOneCaption, (ZString)GC_BusinessRegNoInfo.OriginalValue, GC_BusinessRegNo));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}

				if (GC_BusinessRegNo2Info.HasChanges)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, ZString.Format("{0}: {1} to {2}", BusinessRegistrationNumberTwoCaption, (ZString)GC_BusinessRegNo2Info.OriginalValue, GC_BusinessRegNo2));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}

				if (GC_RX_NKLocalCurrencyInfo.HasChanges)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, ZString.Format("{0}: {1} to {2}", "Company Currency", (ZString)GC_RX_NKLocalCurrencyInfo.OriginalValue, GC_RX_NKLocalCurrency));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			AccountingCountryInfo.RefreshBinding();
		}

		bool AccoutingPropertyHasChanges
		{
			get
			{
				return GC_RN_NKCountryCodeInfo.HasChanges
					|| GC_IsGSTRegisteredInfo.HasChanges
					|| GC_IsGSTCashBasisInfo.HasChanges
					|| GC_IsWHTRegisteredInfo.HasChanges
					|| GC_IsWHTCashBasisInfo.HasChanges
					|| GC_IsActiveInfo.HasChanges;
			}
		}

		public override void Delete()
		{
			if (PK != CurrentCompany.PK)
			{
				DeleteAccountingRelatedData();
				DeleteUnusedStmData();
				DeleteStmNums();
				AddOnRuleAcks.DeleteAll();
				DeleteStmLinks();

				if (EnableConfigurationToSender && IsInDatabase && ShouldSendDeleteCredential())
				{
					RegisterConfigurationForSending();
				}

				base.Delete();
			}
		}

		void DeleteUnusedStmData()
		{
			var datas = new StmDataCollection(Factory, new ZQuery(StmDataSchema.SD_DepartmentGuid, this.PK));
			datas.DeleteAll();
		}

		void DeleteStmNums()
		{
			var stmNumsProvider = CustomsNumberProvider;
			if (stmNumsProvider != null)
			{
				stmNumsProvider.CustomsNumbers.DeleteAll();
			}
		}

		void DeleteStmLinks()
		{
			var links = new ZArchitecture.Favorites.StmLinkCollection(Factory, new ZQuery(StmLinkSchema.STL_GC_LogonCompany, this.PK));
			links.DeleteAll();
		}

		protected virtual void RegisterConfigurationForSending() => GlbCompanyConfigurationToSender.RegisterForSending(Factory, CredentialData.ConfigurationName, this, CredentialData.InterchangeTypeForSending);

		protected bool ShouldSendCredential() => IsInDatabase ? CredentialData.CredentialApplicableInfos.Any(x => x.HasChanges) : CredentialData.CredentialApplicableInfos.Any(x => !x.Value.IsEmpty);

		protected bool ShouldSendDeleteCredential() => CredentialData.CredentialApplicableInfos.Any(x => !x.OriginalValue.IsEmpty);

		public Func<GlbCompanyCredentialData> CredentialDataCreator;

		public GlbCompanyCredentialData CredentialData => credentialData ??= CredentialDataCreator?.Invoke();
		GlbCompanyCredentialData credentialData;

		public bool EnableConfigurationToSender => CredentialData != null && !CredentialData.ConfigurationName.IsEmpty;

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new GlbCompanyFetchStrategy(this);
		}

		#endregion

		#region ExchangeRate

		public int ExchangeRateDecimalPlaces
		{
			get { return 6; }
		}

		#endregion

		#region Implementation

		#region Accounting Related Data

		#region Copy Accounting Related Data

		GlbCompany demoCompany;
		bool demoCompanyLoaded;

		GlbCompany DemoCompany
		{
			get
			{
				if (!demoCompanyLoaded)
				{
					demoCompany = GetDemoCompany(Factory);
					demoCompanyLoaded = true;
				}
				return demoCompany;
			}
		}

		/// <summary>
		/// Create company related data in accounting tables (AccTaxRate, AccWithholding and AccChargeCode).
		/// These records are copied from Demo Company information.
		/// </summary>
		public void CopyOrCreateAccountingRelatedData()
		{
			CreateMissingTaxRates();

			if (!AccountingChargeCodesCreated)
			{
				var globalChargeCodes = new AccGlobalChargeCodeCollection(Factory);
				bool createChargeCodesFromGlobal = globalChargeCodes.Any();

				if (createChargeCodesFromGlobal)
				{
					CreateChargeCodesFromGlobalChargeCodes(globalChargeCodes);
				}

				if (DemoCompany != null)
				{
					var demo2NewWithholding = CopyAccWithholding();
					if (!(globalChargeCodes.Any(x => x.PK != ObjectFactory.Get<IAccountingRegistryProvider>().ElectronicProcessingChargeCode)))
					{
						CopyDemoCompanyAccChargeCodes(demo2NewWithholding);
					}
					else
					{
						CopyDemoCompanyTaxFieldsToExistingChargeCodes(demo2NewWithholding);
					}
				}
			}
		}

		void CopyDemoCompanyTaxFieldsToExistingChargeCodes(Dictionary<ZString, ZGuid> demo2NewWithholding)
		{
			var chargeCodes = new AccChargeCodeCollection(Factory, this);
			chargeCodes.Load();
			foreach (AccChargeCode chargeCode in chargeCodes)
			{
				var demoChargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_GC, DemoCompany.PK);
				demoChargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode.AC_Code);
				var demoChargeCode = Factory.LoadTop1<AccChargeCode>(demoChargeCodeQuery);

				using (chargeCode.GetValidationSuspender())
				{
					if (demoChargeCode != null)
					{
						if (GC_IsWHTRegistered && demo2NewWithholding.ContainsKey(demoChargeCode.AC_Code))
						{
							chargeCode.AC_AW_WithholdingTaxRate = new ZGuid(demo2NewWithholding[chargeCode.AC_Code]);
						}
						if (GC_IsGSTRegistered)
						{
							chargeCode.AC_AT_GSTRate = GetNewAccTaxRatePK(demoChargeCode.AC_AT_GSTRate);
						}
					}
				}
			}
		}

		void CreateChargeCodesFromGlobalChargeCodes(AccGlobalChargeCodeCollection globalChargeCodes)
		{
			if (!GC_IsActive)
			{
				return;
			}

			Factory.SetContext(BusinessContext.SavingChargeCodeFromCompany);
			foreach (AccChargeCode globalChargeCode in globalChargeCodes)
			{
				globalChargeCode.CopyToCompany(this);
			}
			Factory.RemoveContext(BusinessContext.SavingChargeCodeFromCompany);
		}

		bool AccountingChargeCodesCreated => Factory.Exists(typeof(AccChargeCode), new ZQuery(AccChargeCodeSchema.AC_GC, PK));

		#endregion

		#region Copy AccTaxRate

		internal void CreateMissingTaxRates()
		{
			var importer = new TaxRateImporter(GC_RN_NKCountryCode, PK, Factory);
			importer.ImportFromXMLFile();
			SetupStampDutyRegistriesForItaly();
		}

		void SetupStampDutyRegistriesForItaly()
		{
			if (GC_IsGSTRegistered && GC_RN_NKCountryCode == Constants.CountryCodes.Italy)
			{
				var italianCompaniesSQL = string.Format(CultureInfo.InvariantCulture, "SELECT {0} FROM {1}", GlbCompanySchema.PK.Name, GlbCompanySchema.Constants.TableName);
				var query = new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Constants.CountryCodes.Italy);

				var companiesToChange = new DynamicBusinessObjectCollection(Factory);
				companiesToChange.Load(italianCompaniesSQL + query.GetAsWhereClause(false), query.Params);

				var companyGuids = new HashSet<Guid>(companiesToChange.Select(x => ((ZGuid)x[GlbCompanySchema.PK]).ToGuid()));
				companyGuids.Add(PK.ToGuid());

				if (companyGuids.Any())
				{
					var taxIDsAttractingStampDuty = new List<string>();
					taxIDsAttractingStampDuty.Add(AccTaxRate.FindExistingTaxRate(Factory, "DICH.INT", "RAT", Constants.CountryCodes.Italy).PK.ToString());
					taxIDsAttractingStampDuty.Add(AccTaxRate.FindExistingTaxRate(Factory, "ART10", "RAT", Constants.CountryCodes.Italy).PK.ToString());
					taxIDsAttractingStampDuty.Add(AccTaxRate.FindExistingTaxRate(Factory, "ART15", "RAT", Constants.CountryCodes.Italy).PK.ToString());
					taxIDsAttractingStampDuty.Add(AccTaxRate.FindExistingTaxRate(Factory, "ART7", "RAT", Constants.CountryCodes.Italy).PK.ToString());
					taxIDsAttractingStampDuty.Add(AccTaxRate.FindExistingTaxRate(Factory, "ART71", "RAT", Constants.CountryCodes.Italy).PK.ToString());
					taxIDsAttractingStampDuty.Add(AccTaxRate.FindExistingTaxRate(Factory, "ART2", "RAT", Constants.CountryCodes.Italy).PK.ToString());
					taxIDsAttractingStampDuty.Add(AccTaxRate.FindExistingTaxRate(Factory, "ART9", "RAT", Constants.CountryCodes.Italy).PK.ToString());
					taxIDsAttractingStampDuty.Add(AccTaxRate.FindExistingTaxRate(Factory, "ESCLUSEB", "EXL", Constants.CountryCodes.Italy).PK.ToString());
					taxIDsAttractingStampDuty.Add(AccTaxRate.FindExistingTaxRate(Factory, "IVAREVB", "RVS", Constants.CountryCodes.Italy).PK.ToString());
					taxIDsAttractingStampDuty.Add(AccTaxRate.FindExistingTaxRate(Factory, "FREEIVAB", "RAT", Constants.CountryCodes.Italy).PK.ToString());
					string taxIDs = string.Join(",", taxIDsAttractingStampDuty);
					companyGuids.ForEach(x => ObjectFactory.Get<IAccounting>().SetStampDutyConfiguration(x, taxIDs, 2.0m, 77.47m));
				}
			}
		}

		#endregion

		#region Copy AccWithholding

		protected Dictionary<ZString, ZGuid> CopyAccWithholding()
		{
			var demo2NewWithholding = new Dictionary<ZString, ZGuid>();

			if (DemoCompany != null && GC_IsWHTRegistered)
			{
				ZQuery filter = new ZQuery(AccWithholdingSchema.AW_GC, DemoCompany.PK);
				AccWithholding[] demoWithholdings = (AccWithholding[])Factory.Load(typeof(AccWithholding), filter);
				foreach (AccWithholding demoWithholding in demoWithholdings)
				{
					AccWithholding newWithholding = Factory.New<AccWithholding>();

					newWithholding.AW_Code = demoWithholding.AW_Code;
					newWithholding.AW_IsActive = demoWithholding.AW_IsActive;
					newWithholding.AW_Description = demoWithholding.AW_Description;
					newWithholding.AW_Rate = demoWithholding.AW_Rate;
					newWithholding.AW_GC = PK;

					demo2NewWithholding.Add(demoWithholding.AW_Code, newWithholding.PK);
				}
			}

			return demo2NewWithholding;
		}

		#endregion

		#region Copy AccChargeCode

		protected ZGuid GetNewAccTaxRatePK(ZGuid oldPK)
		{
			ZGuid result = ZGuid.Empty;
			AccTaxRate oldTaxRate = Factory.Load<AccTaxRate>(oldPK);
			if (oldTaxRate != null)
			{
				if (oldTaxRate.AT_Type == AccTaxRate.Types.Rated)
				{
					var registryID = oldTaxRate.AT_Code == "GST" ? AccTaxRate.Helper.MainGSTTaxRegistryID : AccTaxRate.Helper.MainFreeGSTTaxRegistryID;
					result = AccTaxRate.Helper.FindTaxRatePK(Factory, registryID, PK.ToGuid());
				}
				else
				{
					var newQuery = new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, GC_RN_NKCountryCode);
					newQuery.AddToFilter(AccTaxRateSchema.AT_Type, oldTaxRate.AT_Type);
					var newTaxRate = Factory.LoadTop1<AccTaxRate>(newQuery);
					if (newTaxRate != null)
					{
						result = newTaxRate.PK;
					}
				}
			}

			return result;
		}

		protected void CopyDemoCompanyAccChargeCodes(Dictionary<ZString, ZGuid> demo2NewWithholding)
		{
			var filter = new ZQuery(AccChargeCodeSchema.AC_GC, DemoCompany.PK);
			var demoChargeCodes = Factory.Load<AccChargeCode>(filter);
			foreach (var demoChargeCode in demoChargeCodes)
			{
				var newChargeCode = Factory.New<AccChargeCode>();
				using (newChargeCode.GetValidationSuspender())
				{
					newChargeCode.AC_Code = demoChargeCode.AC_Code;
					newChargeCode.AC_IsActive = demoChargeCode.AC_IsActive;
					newChargeCode.AC_Desc = demoChargeCode.AC_Desc;
					newChargeCode.AC_ChargeType = demoChargeCode.AC_ChargeType;
					newChargeCode.AC_MarginPercentage = demoChargeCode.AC_MarginPercentage;

					if (GC_IsWHTRegistered && demoChargeCode.WithholdingTaxRate != null && demo2NewWithholding.ContainsKey(demoChargeCode.WithholdingTaxRate.AW_Code))
					{
						newChargeCode.AC_AW_WithholdingTaxRate = new ZGuid(demo2NewWithholding[demoChargeCode.WithholdingTaxRate.AW_Code]);
					}

					newChargeCode.AC_AT_GSTRate = GetNewAccTaxRatePK(demoChargeCode.AC_AT_GSTRate);
					newChargeCode.AC_AG_RevenueAccount = demoChargeCode.AC_AG_RevenueAccount;
					newChargeCode.AC_AG_WIPAccount = demoChargeCode.AC_AG_WIPAccount;
					newChargeCode.AC_AG_CostAccount = demoChargeCode.AC_AG_CostAccount;
					newChargeCode.AC_AG_AccrualAccount = demoChargeCode.AC_AG_AccrualAccount;
					newChargeCode.AC_AR_SalesGroup = demoChargeCode.AC_AR_SalesGroup;
					newChargeCode.AC_AR_ExpenseGroup = demoChargeCode.AC_AR_ExpenseGroup;
					newChargeCode.AC_ChargeGroup = demoChargeCode.AC_ChargeGroup;
					newChargeCode.AC_ChargeSubGroup = demoChargeCode.AC_ChargeSubGroup;
					newChargeCode.AC_RateCalculator = demoChargeCode.AC_RateCalculator;
					newChargeCode.AC_ShowOnQuotation = demoChargeCode.AC_ShowOnQuotation;
					newChargeCode.AC_SuppressOnQuoteIfZero = demoChargeCode.AC_SuppressOnQuoteIfZero;
					newChargeCode.AC_DepartmentFilterList = demoChargeCode.AC_DepartmentFilterList;
					newChargeCode.AC_AllowDescriptionOvertype = demoChargeCode.AC_AllowDescriptionOvertype;
					newChargeCode.AC_GC = PK;
				}
			}
		}

		#endregion

		#region Delete Accounting Related Data

		/// <summary>
		/// Delete company related data from accounting tables (AccTaxRate, AccWithholding, AccJobConfig and AccChargeCode).
		/// </summary>
		protected void DeleteAccountingRelatedData()
		{
			ZQuery filter = new ZQuery(AccChargeCodeSchema.AC_GC, PK);
			BusinessObject[] accChargeCodes = Factory.Load(typeof(AccChargeCode), filter);
			foreach (AccChargeCode chargeCode in accChargeCodes)
			{
				chargeCode.Delete();
			}

			filter = new ZQuery(AccWithholdingSchema.AW_GC, PK);
			BusinessObject[] accWithholdings = Factory.Load(typeof(AccWithholding), filter);
			foreach (AccWithholding withholding in accWithholdings)
			{
				withholding.Delete();
			}

			var exRateConfigQuery = AccExchangeRateConfigurationsQueryProviderFactory.CreateCompanyLevelProvider(PK, new ZQuery()).GetQuery();
			AccExchangeRateConfigurationsHelper.LoadAndDelete(Factory, exRateConfigQuery);

			AccTaxConfigurations.DeleteAll();
		}

		#endregion

		#endregion

		#region Readonly Fields

		protected bool GC_RN_NKCountryCode_ReadOnly
		{
			get { return IsInDatabase && !Env.CurrentUser.IsSupportUser; }
		}

		protected bool GC_RX_NKLocalCurrency_ReadOnly
		{
			get { return !Env.CurrentUser.IsSupportUser; }
		}

		protected bool GC_IsReciprocal_ReadOnly
		{
			get { return !Env.CurrentUser.IsSupportUser; }
		}

		#endregion

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.Company);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IGlbCompany Members

		IRefCurrency IGlbCompany.Currency
		{
			get { return LocalCurrency; }
		}

		IRefCountry IGlbCompany.Country => Country;

		IEnumerable<IGlbBranch> IGlbCompany.GetActiveBranches()
		{
			return ActiveBranches;
		}

		#endregion

		#region IAddressDetails Members

		ZString IAddressDetails.CompanyName => ((IAddressDetails)OrgProxy)?.CompanyName ?? GC_Name;

		ZString IAddressDetails.ContactName => ((IAddressDetails)OrgProxy)?.ContactName ?? ZString.Empty;

		ZString IAddressDetails.Phone => ((IAddressDetails)OrgProxy)?.Phone ?? GC_Phone;

		ZString IAddressDetails.Fax => ((IAddressDetails)OrgProxy)?.Fax ?? GC_Fax;

		ZString IAddressDetails.Email => ((IAddressDetails)OrgProxy)?.Email ?? GC_Email;

		ZString IAddressDetails.AddressLine1 => ((IAddressDetails)OrgProxy)?.AddressLine1 ?? GC_Address1;

		ZString IAddressDetails.AddressLine2 => ((IAddressDetails)OrgProxy)?.AddressLine2 ?? GC_Address2;

		ZString IAddressDetails.City => ((IAddressDetails)OrgProxy)?.City ?? GC_City;

		ZString IAddressDetails.State => ((IAddressDetails)OrgProxy)?.State ?? GC_State;

		ZString IAddressDetails.PostCode => ((IAddressDetails)OrgProxy)?.PostCode ?? GC_PostCode;

		ZString IAddressDetails.Country => ((IAddressDetails)OrgProxy)?.Country ?? Country?.RN_Code ?? ZString.Empty;

		#endregion

		public override RefCurrency LocalCurrency
		{
			get
			{
				return localCurrencyCached != null && !localCurrencyCached.IsDeleted && localCurrencyCached.RX_Code == GC_RX_NKLocalCurrency ?
				localCurrencyCached : (localCurrencyCached = base.LocalCurrency);
			}
		}
		RefCurrency localCurrencyCached;

		public string GetLocalCurrencyCachedMessage
		{
			get
			{
				return localCurrencyCached == null ? null : $@"localCurrencyCached.IsDeleted = {localCurrencyCached.IsDeleted}
localCurrencyCached.RX_Code = {localCurrencyCached.RX_Code}";
			}
		}

		public RefCurrency CustomsCurrency => LocalCurrency;

		public bool HasBranchWithThisPK(ZGuid branchPk) => Branches.FindByPK(branchPk) != null;

		public static GlbCompany[] GetActiveCompanies(string countryCode = null, BusinessObjectFactory factory = null)
		{
			return GetActiveCompanies(c => true, countryCode, factory);
		}

		public static GlbCompany[] GetActiveCompanies(Func<GlbCompany, bool> selector, string countryCode = null, BusinessObjectFactory factory = null)
		{
			Argument.NotNull(selector, nameof(selector));

			return
				new Loader(factory ?? new BusinessObjectFactory())
					.LoadCompanies(countryCode, activeCompaniesOnly: true, excludeDemoCompany: true)
					.Where(c => selector(c) && c.HasActiveBranch)
					.ToArray();
		}

		public OrgHeader GetNewOrgProxy(BusinessObjectFactory factory)
		{
			var org = factory.New<OrgHeader>();

			org.OH_FullName = GC_Name;
			org.MainAddress.OA_Address1 = GC_Address1;
			org.MainAddress.OA_Address2 = GC_Address2;
			org.MainAddress.OA_City = GC_City;
			org.MainAddress.OA_PostCode = GC_PostCode;

			foreach (var branch in Branches)
			{
				if (HasSameAddress(branch))
				{
					org.OH_RL_NKClosestPort = branch.GB_RL_NKHomePort;
					break;
				}
			}

			org.MainAddress.OA_State = GC_State;
			org.MainAddress.OA_Phone = GC_Phone;
			org.MainAddress.OA_Fax = GC_Fax;
			org.MainAddress.OA_Email = GC_Email;
			org.OH_IsForwarder = true;
			org.MainWebURL.PU_URL = GC_WebAddress;
			AddBusinessRegNumbersToProxyOrg(org);
			org.GenerateProposedCode();
			if (org.OH_Code.IsEmpty)
			{
				org.OH_Code = Utilities.GetCodeFromNameAndUNLOCO(GC_Name, null, "");
			}

			return org;
		}

		void AddBusinessRegNumbersToProxyOrg(OrgHeader org)
		{
			var countryCode = org.CountryCode;
			string taxCodeType = null;
			if (!GC_BusinessRegNo.IsEmpty)
			{
				taxCodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(countryCode);
				if (!string.IsNullOrEmpty(taxCodeType) &&
						taxCodeType.Length <= OrgCusCodeSchema.OK_CodeType.MaxLength)
				{
					var cusCode = org.CustomsCodes.AddNew();
					cusCode.OK_RN_NKCodeCountry = countryCode;
					cusCode.OK_CodeType = new ZString(taxCodeType).Left(cusCode.OK_CodeTypeInfo.MaxLength);
					cusCode.OK_CustomsRegNo = GC_BusinessRegNo;
				}
				else
				{
					org.PrimaryRegistrationNumber.Number = GC_BusinessRegNo;
				}
			}

			if (!GC_BusinessRegNo2.IsEmpty)
			{
				var country = org.Country;
				if (country != null)
				{
					string bizCodeType = LicenceBusinessRegNoType(country);
					if (!string.IsNullOrEmpty(bizCodeType)
						&& bizCodeType != taxCodeType
						&& bizCodeType.Length <= OrgCusCodeSchema.OK_CodeType.MaxLength)
					{
						var cusCode = org.CustomsCodes.AddNew();
						cusCode.OK_RN_NKCodeCountry = countryCode;
						cusCode.OK_CodeType = bizCodeType;
						cusCode.OK_CustomsRegNo = GC_BusinessRegNo2;
					}
				}
			}
		}

		bool HasSameAddress(GlbBranch branch)
		{
			return branch.Address1 == GC_Address1
						 && branch.Address2 == GC_Address2
						 && branch.City == GC_City
						 && branch.Postcode == GC_PostCode
						 && branch.GB_State == GC_State;
		}

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{ }

			public GlbCompany[] LoadCompanies(
				string countryCode = null,
				bool activeCompaniesOnly = true,
				bool excludeDemoCompany = false)
			{
				var query = new ZQuery();

				if (!string.IsNullOrEmpty(countryCode))
				{
					query.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, countryCode);
				}

				if (activeCompaniesOnly)
				{
					query.AddToFilter(GlbCompanySchema.GC_IsActive, true);
				}

				if (excludeDemoCompany)
				{
					query.AddToFilter(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, DemoCompanyCode);
				}

				return Factory.Load<GlbCompany>(query);
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(GlbCompany);
		}

		public CustomsNumberViewStmNumsBusinessProvider CustomsNumberProvider
		{
			get
			{
				var providerKey = CustomsNumberProviderKey;
				if (customsNumberProvider == null || customsNumberProvider.ProviderKey != providerKey)
				{
					customsNumberProvider = (CustomsNumberViewStmNumsCompanyProvider)CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, providerKey, PK);
				}
				return customsNumberProvider;
			}
		}
		CustomsNumberViewStmNumsCompanyProvider customsNumberProvider;

		public string CustomsNumberProviderKey => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GC_RN_NKCountryCode);

		#region HumanReadableNameForRegistry

		public string HumanReadableNameForRegistry => Env.Registry.ShowCodeAtCompanyAndBranchName ? HumanReadableShortcutName : GC_Name;

		#endregion

		#region ICompany

		string ICompany.Address1
		{
			get { return this.GC_Address1; }
		}

		string ICompany.Address2
		{
			get { return this.GC_Address2; }
		}

		string ICompany.BusinessRegNo1
		{
			get { return this.GC_BusinessRegNo; }
		}

		string ICompany.BusinessRegNo2
		{
			get { return this.GC_BusinessRegNo2; }
		}

		string ICompany.City
		{
			get { return this.GC_City; }
		}

		string ICompany.Code
		{
			get { return this.GC_Code; }
		}

		ICountry ICompany.Country
		{
			get { return this.Country; }
		}

		IEnumerable<IBranch> ICompany.Branches
		{
			get { return Branches; }
		}

		IEnumerable<IBranch> ICompany.ActiveBranches
		{
			get { return ActiveBranches; }
		}

		CountryDateTimeFormat ICompany.DateTimeFormat
		{
			get
			{
				switch (this.GC_RN_NKCountryCode)
				{
					case Constants.CountryCodes.Philippines:
					case Constants.CountryCodes.UnitedStates:
						return CountryDateTimeFormat.US;

					case Constants.CountryCodes.Japan:
						return CountryDateTimeFormat.Japan;

					default:
						return CountryDateTimeFormat.Other;
				}
			}
		}

		ExchangeRate ICompany.ExchangeRate
		{
			get { return new ExchangeRate(GC_IsReciprocal, LocalCurrency.Decimals, PK.ToGuid()); }
		}

		string ICompany.Fax
		{
			get { return this.GC_Fax; }
		}

		int ICompany.GetPeriod(DateTime date)
		{
			return AccountingPeriodManager.GetPeriod(date, PK.ToGuid());
		}

		bool ICompany.IsGSTCashBasis
		{
			get { return this.GC_IsGSTCashBasis; }
		}

		bool ICompany.IsGSTRegistered
		{
			get { return this.GC_IsGSTRegistered; }
		}

		bool ICompany.IsReciprocal
		{
			get { return this.GC_IsReciprocal; }
		}

		bool ICompany.IsWHTCashBasis
		{
			get { return this.GC_IsWHTCashBasis; }
		}

		bool ICompany.IsWHTRegistered
		{
			get { return this.GC_IsWHTRegistered; }
		}

		Enterprise.Integration.ZArchitecture.ICurrency ICompany.LocalCurrency
		{
			get { return this.LocalCurrency; }
		}

		string ICompany.Name
		{
			get { return this.GC_Name; }
		}

		Guid ICompany.OrganisationPK
		{
			get { return this.GC_OH_OrgProxy.IsEmpty ? Guid.Empty : this.GC_OH_OrgProxy.ToGuid(); }
		}

		Guid ICompany.PK
		{
			get { return this.PK.ToGuid(); }
		}

		string ICompany.Postcode
		{
			get { return this.GC_PostCode; }
		}

		string ICompany.State
		{
			get { return this.GC_State; }
		}

		string ICompany.HumanReadableNameForRegistry => HumanReadableNameForRegistry;

#if DEBUG
		string ICompany.LicenceKeyIdentifier => LicenceKeyIdentifier;
#endif

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (GC_RN_NKCountryCode.IsEmpty)
			{
				GC_RN_NKCountryCode = "AU";
			}

			if (GC_RX_NKLocalCurrency.IsEmpty)
			{
				GC_RX_NKLocalCurrency = "AUD";
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

		#region Country / UNLOCO Helpers

		public static IDisposable TemporaryLoginInNewCompanyForCountry(string countryCode)
		{
			var branch = new BusinessObjectFactory().NewWithValidTestData<GlbBranch>();
			branch.GB_Code = MasterFilesTestHelper.GetRandomString(branch.GB_CodeInfo.MaxLength);
			branch.SetCountry(countryCode);

			var company = branch.Company;
			company.GC_Code = MasterFilesTestHelper.GetRandomString(company.GC_CodeInfo.MaxLength);
			company.SetCountry(countryCode);

			branch.Factory.Save();

			return Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK);
		}

		public void SetCountry(string countryCode)
			=> SetCountryCore(countryCode, isInTemporarilySetCountry: false);

		void SetCountryCore(string countryCode, bool isInTemporarilySetCountry = false)
		{
			bool saveChanges = IsInDatabase && !NUnit.Framework.TransactionedTestCase.InReflectionTest &&
				(!Globals.IsTest || NUnit.Framework.TransactionedTestCase.InTransactionedTestCase || CargoWise.Data.Testing.UseSnapshotProtectionAttribute.IsProtected);
			var factory = saveChanges ? new BusinessObjectFactory() : Factory;

			var bizo = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode)
				?? throw new InvalidOperationException("Could not find country : " + countryCode);

			var company = factory.Load<GlbCompany>(PK);
			if (company.GC_RN_NKCountryCode == bizo.RN_Code)
			{
				return;
			}

			if (isInTemporarilySetCountry)
			{
				// PERF: Branches.MarkAsNeedingValidationIncludingChildren() is called in GC_RN_NKCountryCode setter.
				// PERF: This takes ~75% of time in TemporarilySetCountry(), but no unit test depends on this behavior.
				company.SetPropertyValue(GC_RN_NKCountryCodeInfo, bizo.RN_Code);
			}
			else
			{
				company.GC_RN_NKCountryCode = bizo.RN_Code;
			}
			company.GC_RX_NKLocalCurrency = bizo.RN_RX_NKLocalCurrency;

			if (string.IsNullOrEmpty(company.GC_RX_NKLocalCurrency))
			{
				company.GC_RX_NKLocalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}

			var isCurrentCompany = PK == Env.CurrentCompanyPK;
			if (isCurrentCompany)
			{
				var branch = factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
				branch.Reload(); //after db error and transaction rollback on saving in some tests, it somehow loads not latest version in new factory. This cause concurrency error on saving. Reload does required update though.
				branch.SetCountry(countryCode);
			}

			if (saveChanges)
			{
				factory.Save();

				if (isCurrentCompany && CurrentCompany.GC_RN_NKCountryCode != bizo.RN_Code)
				{
					CurrentCompany.Reload();
					if (CurrentCompany.GC_RN_NKCountryCode != bizo.RN_Code)
					{
						throw new InvalidOperationException("Env.CurrentCompany is not updated: " + countryCode);
					}
				}
				if (GC_RN_NKCountryCode != bizo.RN_Code)
				{
					if (HasChanges)
					{
						throw new InvalidOperationException("Current Factory company is not updated and can't be reloaded as has changes: " + GlbBranch.CurrentBranch.GB_RL_NKHomePort);
					}

					Reload();
					if (GC_RN_NKCountryCode != bizo.RN_Code)
					{
						throw new InvalidOperationException("Current Factory company is not updated: " + countryCode);
					}
				}

				if (isCurrentCompany)
				{
					var branch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, GlbBranch.CurrentBranch.PK) { FetchOnlyFromLocalCache = true });
					if (branch != null && branch.GB_RL_NKHomePort != GlbBranch.CurrentBranch.GB_RL_NKHomePort)
					{
						if (branch.HasChanges)
						{
							throw new InvalidOperationException("Current Factory branch is not updated and can't be reloaded as has changes. Home port: " + GlbBranch.CurrentBranch.GB_RL_NKHomePort);
						}

						branch.Reload();
						if (branch.GB_RL_NKHomePort != GlbBranch.CurrentBranch.GB_RL_NKHomePort)
						{
							throw new InvalidOperationException("Current Factory branch is not updated. Home port: " + GlbBranch.CurrentBranch.GB_RL_NKHomePort);
						}
					}
				}
			}
			else
			{
				if (isCurrentCompany)
				{
					CurrentCompany.GC_RN_NKCountryCode = bizo.RN_Code;
					CurrentCompany.GC_RX_NKLocalCurrency = bizo.RN_RX_NKLocalCurrency;
					GlbBranch.CurrentBranch.SetCountry(countryCode);
				}
			}
		}

		public void SetCurrency(string currencyCode)
		{
			bool saveChanges = IsInDatabase && (!Globals.IsTest || NUnit.Framework.TransactionedTestCase.InTransactionedTestCase || CargoWise.Data.Testing.UseSnapshotProtectionAttribute.IsProtected);
			var factory = saveChanges ? new BusinessObjectFactory() : Factory;

			var company = factory.Load<GlbCompany>(PK);
			if (company.GC_RX_NKLocalCurrency == currencyCode)
			{
				return;
			}
			company.GC_RX_NKLocalCurrency = currencyCode;

			var isCurrentCompany = PK == Env.CurrentCompanyPK;
			if (saveChanges)
			{
				factory.Save();
			}
			else
			{
				if (isCurrentCompany)
				{
					CurrentCompany.GC_RX_NKLocalCurrency = currencyCode;
				}
			}
		}

		public IDisposable TemporarilySetCountry(string countryCode)
		{
			var oldCountry = GC_RN_NKCountryCode;
			SetCountryCore(countryCode, isInTemporarilySetCountry: true);

			return new DisposableAction(() =>
			{
				SetCountryCore(oldCountry, isInTemporarilySetCountry: true);
			});
		}

		public IDisposable TemporarilySetCurrency(string currencyCode)
		{
			var oldCurrency = GC_RX_NKLocalCurrency;
			SetCurrency(currencyCode);

			return new DisposableAction(() => SetCurrency(oldCurrency));
		}

		#endregion

#endif

		public bool HasActiveBranch
		{
			get
			{
				var query = new ZQuery(GlbBranchSchema.GB_IsActive, true);
				query.AddToFilter(GlbBranchSchema.GB_GC, PK);
				return Factory.LoadTop1<GlbBranch>(query) != null;
			}
		}

		public bool HasInactiveBranch
		{
			get
			{
				var query = new ZQuery(GlbBranchSchema.GB_IsActive, false);
				query.AddToFilter(GlbBranchSchema.GB_GC, PK);
				return Factory.LoadTop1<GlbBranch>(query) != null;
			}
		}

		#region ISupportWebAddressValidation

		public bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return false;
		}

		public async Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest)
		{
			return await AddressValidationService.ValidateAddressAsync(this, cancellationToken, false, cleanseAction);
		}

		public async Task<CandidateCityTown[]> GetCityTownAsync(CancellationTokenSource cancellationToken)
		{
			return await AddressValidationService.GetCityTownAsync(this, cancellationToken);
		}

		bool isManuallyVerifiedByUser;

		public AddressValidationSection ValidationSection { get; } = AddressValidationSection.Company;

		public void ResetValidationStatus(ZPropertyInfo propertyInfo)
		{
			if (isManuallyVerifiedByUser)
			{
				return;
			}

			if (Env.Registry.EnableAddressValidationWebService)
			{
				if (ValidationStatus != AddressValidationStatus.CountryNotAvailable && Country != null && OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(Country.PK.ToGuid(), ValidationSection))
				{
					ValidationStatus = AddressValidationStatus.ToBeVerified;
					AddressMap = string.Empty;
					if (!string.IsNullOrEmpty(RegistrationKey.SystemId))
					{
						RaiseWebServices(propertyInfo);
					}
				}
				else if (propertyInfo.Name == nameof(GC_RN_NKCountryCode))
				{
					ValidationStatus = AddressValidationStatus.ToBeVerified;
					AddressMap = string.Empty;
				}
			}
		}

		void RaiseWebServices(ZPropertyInfo propertyInfo)
		{
			if (!string.IsNullOrEmpty(GC_RN_NKCountryCode))
			{
				if ((string.IsNullOrEmpty(GC_City) || string.IsNullOrEmpty(GC_State) || string.IsNullOrEmpty(GC_PostCode)) && (propertyInfo == GC_CityInfo || propertyInfo == GC_StateInfo || propertyInfo == GC_PostCodeInfo))
				{
					RaiseTriggerWebGetCityTown(propertyInfo);
				}
				else
				{
					RaiseTriggerWebAddressValidation(propertyInfo);
				}
			}
		}

		void RaiseTriggerWebGetCityTown(ZPropertyInfo propertyInfo)
		{
			if (TriggerWebGetCityTown != null)
			{
				TriggerWebGetCityTown(this, new InfoEventArgs(propertyInfo));
			}
		}

		void RaiseTriggerWebAddressValidation(ZPropertyInfo propertyInfo)
		{
			if (TriggerWebAddressValidation != null)
			{
				TriggerWebAddressValidation(this, new InfoEventArgs(propertyInfo));
			}
		}

		public void PreValidationForAddressValidationService()
		{
			Validation.ValidateGC_Address1();
			Validation.ValidateGC_City();
			Validation.ValidateGC_RN_NKCountryCode();
			ValidatePostcodeAndStateForAddress();
		}

		public void ValidatePostcodeAndStateForAddress()
		{
			Validation.ValidateGC_PostCode();
			Validation.ValidateGC_State();
		}

		public void ClearWebAddressValidationHandler()
		{
			if (TriggerWebAddressValidation != null)
			{
				foreach (EventHandler item in TriggerWebAddressValidation.GetInvocationList())
				{
					TriggerWebAddressValidation -= item;
				}
			}
		}

		public void ClearWebGetCityTownHandler()
		{
			if (TriggerWebGetCityTown != null)
			{
				foreach (EventHandler item in TriggerWebGetCityTown.GetInvocationList())
				{
					TriggerWebGetCityTown -= item;
				}
			}
		}

		public bool IsErrorSuppressed => false;
		public bool IsJobDocAddress => false;

		public event EventHandler TriggerWebAddressValidation;
		public event EventHandler AddressValidationStatusChanged;
		public event EventHandler TriggerWebGetCityTown;

		[BusinessObjectTestExclude] // Language is English only
		public ZString Language
		{
			get { return Constants.Languages.English; }
			set { }
		}

		public ZPropertyInfo LanguageInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Language));
			}
		}

		public int Language_MaxLength
		{
			get
			{
				return Schema.LanguageMaxLength;
			}
		}

		public CodeDescriptionPairList LanguageList { get { return null; } }

		public ZString Address1
		{
			get { return GC_Address1; }
			set { GC_Address1 = value; }
		}

		public ZPropertyInfo Address1Info
		{
			get { return GC_Address1Info; }
		}

		public int Address1_MaxLength
		{
			get { return AutoGlbCompany.Schema.GC_Address1MaxLength; }
		}

		public ZString Address2
		{
			get { return GC_Address2; }
			set { GC_Address2 = value; }
		}

		public ZPropertyInfo Address2Info
		{
			get { return GC_Address2Info; }
		}

		public int Address2_MaxLength
		{
			get { return AutoGlbCompany.Schema.GC_Address2MaxLength; }
		}

		public ZString City
		{
			get { return GC_City; }
			set { GC_City = value; }
		}

		public ZPropertyInfo CityInfo
		{
			get { return GC_CityInfo; }
		}

		public int City_MaxLength
		{
			get { return AutoGlbCompany.Schema.GC_CityMaxLength; }
		}

		public ZString Postcode
		{
			get { return GC_PostCode; }
			set { GC_PostCode = value; }
		}

		public ZPropertyInfo PostcodeInfo
		{
			get { return GC_PostCodeInfo; }
		}

		public int Postcode_MaxLength
		{
			get { return AutoGlbCompany.Schema.GC_PostCodeMaxLength; }
		}

		public ZString CompanyName
		{
			get { return GC_Name; }
			set
			{
				GC_Name = value;
				CompanyNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CompanyNameInfo
		{
			get { return GetZPropertyInfo(nameof(CompanyName)); }
		}

		public int CompanyName_MaxLength
		{
			get { return AutoGlbCompany.Schema.GC_NameMaxLength; }
		}

		public ZString StateCode
		{
			get { return GC_State; }
			set
			{
				GC_State = value;
				StateCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StateCodeInfo
		{
			get { return GetZPropertyInfo(nameof(StateCode)); }
		}

		public int StateCode_MaxLength
		{
			get { return AutoGlbCompany.Schema.GC_StateMaxLength; }
		}

		public CodeDescriptionPairList StateCodeList
		{
			get
			{
				return Lookups.StateList;
			}
		}

		[BusinessObjectTestExclude] // State has to be valid
		public ZString State
		{
			get { return StateCodeList.GetDescriptionFromCode(GC_State); }
			set
			{
				var code = (ZString)StateCodeList.GetCodeFromDescription(value);
				GC_State = string.IsNullOrEmpty(code) ? value : code;
				StateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StateInfo
		{
			get { return GetZPropertyInfo(nameof(State)); }
		}

		public int State_MaxLength
		{
			get { return AutoGlbCompany.Schema.GC_StateMaxLength; }
		}

		ZString ISupportWebAddressValidation.CountryCodeISO2
		{
			get { return GC_RN_NKCountryCode; }
			set { GC_RN_NKCountryCode = value; }
		}

		public int CountryCodeISO2_MaxLength => Schema.GC_RN_NKCountryCodeMaxLength;
		public RefCountryCollection CountryCodeList => Lookups.Countries;

		public ZString DisplayText
		{
			get { return GC_Code; }
			set { GC_Code = value; }
		}

		public ZGuid EntityPK
		{
			get { return PK; }
		}

		public ZString AddressRecordGUID
		{
			get { return PK.ToString(); }
		}

		public ZString AddressSourceTable
		{
			get { return GlbCompanySchema.Constants.Prefix; }
		}

		public ZString ValidationStatus
		{
			get
			{
				return GC_ValidationStatus;
			}
			set
			{
				GC_ValidationStatus = value;
				RaiseAddressValidationStatusChanged();
			}
		}

		void RaiseAddressValidationStatusChanged()
		{
			if (AddressValidationStatusChanged != null)
			{
				AddressValidationStatusChanged(this, EventArgs.Empty);
			}
		}

		public ZString AddressMap
		{
			get { return GC_AddressMap; }
			set { GC_AddressMap = value; }
		}

		public ZString Addressee
		{
			get { return GC_Name; }
		}

		public ZGeography GeoLocation
		{
			get { return GC_GeoLocation; }
			set { GC_GeoLocation = value; }
		}

		public bool NeedValidation
		{
			get
			{
				var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, GC_RN_NKCountryCode);
				if (country != null)
				{
					if (!GC_Address1.IsEmpty && !GC_PostCode.IsEmpty && !GC_City.IsEmpty && !GC_State.IsEmpty)
					{
						if (!IsInDatabase || (GC_Address1Info.HasChanges || GC_Address2Info.HasChanges || GC_PostCodeInfo.HasChanges ||
												GC_CityInfo.HasChanges || GC_StateInfo.HasChanges || GC_RN_NKCountryCodeInfo.HasChanges))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public bool IsUpdatingCityTown { get; set; }
		public bool IsValidatingAddress { get; set; }
		public bool IsExactPointFound { get; set; }
		public bool IsValidatedByBackgroundService { get; set; }

		public bool IsTSAKnownAddress => false;
		public bool IsMIDAddress => false;

		[DocumentFieldExcludeFromMap]
		[BusinessObjectTestExclude]
		public ZString UnrestrictedAdditionalAddressInformation { get; set; }

		public ZPropertyInfo UnrestrictedAdditionalAddressInformationInfo => GetZPropertyInfo(nameof(UnrestrictedAdditionalAddressInformation));

		[BusinessObjectTestExclude]
		public CodeDescriptionPairList AdditionalAddressInfoList { get; }

		public ZString AddressCode { get; set; }

		public ZString ClosestPort { get; set; }

		#endregion
	}
}
