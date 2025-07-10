using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;
using Res = Enterprise.MasterFiles.Business.Res;
using ResString = Enterprise.MasterFiles.Business.ResString;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceReportConfiguration : RegistryBusinessObjectTemplate
	{
		public ComplianceReportConfiguration(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public ComplianceReportConfiguration()
			: base()
		{ }

		public ComplianceReportConfiguration(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{ }

		#region Schema

		abstract class Schema
		{
			// Report Description
			public const string Country = "Country";
			public const string ReportCode = "ReportCode";
			public const string ReportTitle = "ReportTitle";
			public const string ReportBaseTablePrefix = "ReportBaseTablePrefix";
			public const string ReportPeriodicity = "ReportPeriodicity";
			// Tax Registration number
			public const string TaxRegistrationType = "TaxRegistrationType";
			// Country specific tax registration number used in the report
			public const string RepCountryRegistrationCode = "RepCountryRegistrationCode";
			// Grouping, Filtering and Rounding
			public const string ReportLineGrouping = "ReportLineGrouping";
			public const string ReportLineOrdering = "ReportLineOrdering";
			public const string GoodsServiceType = "GoodsServiceType";
			public const string ReportAmountsRoundingType = "ReportAmountsRoundingType";
			public const string ReportAmountsRoundingTruncating = "ReportAmountsRoundingTruncating";
			// Amount Thresholds
			public const string AmountThresholdLevel = "AmountThresholdLevel";
			public const string ExTaxAmountThreshold = "ExTaxAmountThreshold";
			public const string TaxAmountThreshold = "TaxAmountThreshold";
			// Recipient Org currently used for ReportSubCode mapping to Charge Codes
			public const string RecipientOrgPK = "RecipientOrgPK";
			// Report which needs to include queued records from the previous periods
			public const string IncludeQueuedForPreviousPeriod = "IncludeQueuedForPreviousPeriod";
			public const string IsDefaultReportType = "IsDefaultReportType";
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCountry();
			ValidateReportCode();
			ValidateReportTitle();
			ValidateReportBaseTablePrefix();
			ValidateReportPeriodicity();
			ValidateTaxRegistrationType();
			ValidateRepCountryRegistrationCode();
			ValidateReportLineGrouping();
			ValidateReportLineOrdering();
			ValidateReportAmountsRoundingTruncating();
			ValidateGoodsServiceType();
			ValidateAmountThresholdLevel();
			ValidateExTaxAmountThreshold();
			ValidateTaxAmountThreshold();
			ValidateRecipientOrgPK();
			ValidateIncludeQueuedForPreviousPeriod();
			ValidateIsDefaultReportType();
		}

		public ComplianceReportConfigurationCollection ParentCollection
		{
			get
			{
				var result = ((IBusinessObjectInternals)this).ParentCollections.FirstOrDefault(x => x is ComplianceReportConfigurationCollection) as ComplianceReportConfigurationCollection;
				return result ?? new ComplianceReportConfigurationCollection(CurrentFallbackLevel, CurrentFactory);
			}
		}

		[ChildEditable]
		public ComplianceReportConfigurationSettingCollection Settings
		{
			get
			{
				if (settings == null)
				{
					settings = new ComplianceReportConfigurationSettingCollection(CurrentFallbackLevel, CurrentFactory);
					settings.ParentConfiguration = this;
					settings.CountryCode = Country;
					RegisterEditableChildObject(settings);
				}
				return settings;
			}
		}
		ComplianceReportConfigurationSettingCollection settings;

		void SetNewSettings(ComplianceReportConfigurationSettingCollection newSettings)
		{
			if (settings != null)
			{
				settings.ParentConfiguration = null;
				UnRegisterEditableChildObject(settings);
			}
			settings = newSettings;
			if (settings != null)
			{
				settings.ParentConfiguration = this;
				settings.CountryCode = Country;
				RegisterEditableChildObject(settings);
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceReportConfiguration(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			((ComplianceReportConfiguration)clone).SetNewSettings((ComplianceReportConfigurationSettingCollection)Settings.Clone(CurrentFallbackLevel, CurrentFactory));
		}

		#region Bound Properties

		#region Country

		[List("Lookups.CountryList")]
		[MaxLength(2)]
		public ZString Country
		{
			get { return country; }
			set
			{
				CheckMaximumLength(CountryInfo, value);
				var isChanged = SetNonPersistentPropertyValue(CountryInfo, ref country, value);
				if (!IsValidationSuspended)
				{
					ValidateCountry();
				}
				if (isChanged)
				{
					UpdateCountryRelatedData();
				}
			}
		}

		public ZPropertyInfo CountryInfo
		{
			get { return GetZPropertyInfo(Schema.Country); }
		}

		public void ValidateCountry()
		{
			CountryInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CountryInfo, (IMultilingualString)ResString.GetMultilingualString("c7cf0082-d3f2-47dc-96ca-f42344cd3fbc", "Country/Region"));
			if (!CountryInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(CountryInfo, Lookups.CountryList);
			}
		}

		ZString country;

		#endregion

		#region ReportCode

		[MaxLength(3)]
		public ZString ReportCode
		{
			get { return reportCode; }
			set
			{
				CheckMaximumLength(ReportCodeInfo, value);
				SetNonPersistentPropertyValue(ReportCodeInfo, ref reportCode, value);
				if (!IsValidationSuspended)
				{
					ValidateReportCode();
				}
			}
		}

		public ZPropertyInfo ReportCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ReportCode); }
		}

		public void ValidateReportCode()
		{
			ReportCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ReportCodeInfo, (IMultilingualString)ResString.GetMultilingualString("339e94c5-676e-4541-9464-1b770457ee31", "Report Code"));
			if (!ReportCodeInfo.HasErrors() && CheckSameReportCodeExistsForCountry())
			{
				ReportCodeInfo.AddError(Res.GetString("18890ba1-fc8e-4cc5-9dc2-b2d06f1d1be9", "The same Report Code already exists for the '{0}' country/region.", Country));
			}
		}

		ZString reportCode;

		bool CheckSameReportCodeExistsForCountry()
		{
			return ParentCollection.Cast<ComplianceReportConfiguration>().Any(x => x.PK != this.PK && x.Country == this.Country && x.ReportCode == this.ReportCode);
		}

		#endregion

		#region ReportTitle

		[MaxLength(100)]
		public ZString ReportTitle
		{
			get { return reportTitle; }
			set
			{
				SetNonPersistentPropertyValue(ReportTitleInfo, ref reportTitle, value);
				if (!IsValidationSuspended)
				{
					ValidateReportTitle();
				}
			}
		}

		public ZPropertyInfo ReportTitleInfo
		{
			get { return GetZPropertyInfo(Schema.ReportTitle); }
		}

		public void ValidateReportTitle()
		{
			ReportTitleInfo.ClearAllNotifications();
		}

		ZString reportTitle;

		#endregion

		#region ReportBaseTablePrefix

		[List("Lookups.ReportBaseTablePrefixList")]
		[MaxLength(3)]
		public ZString ReportBaseTablePrefix
		{
			get { return reportBaseTablePrefix; }
			set
			{
				CheckMaximumLength(ReportBaseTablePrefixInfo, value);
				SetNonPersistentPropertyValue(ReportBaseTablePrefixInfo, ref reportBaseTablePrefix, value);

				if (!IsValidationSuspended)
				{
					ValidateReportBaseTablePrefix();
					ValidateIncludeQueuedForPreviousPeriod();
					ValidateIsDefaultReportType();
				}
			}
		}

		public virtual ZPropertyInfo ReportBaseTablePrefixInfo
		{
			get { return GetZPropertyInfo(Schema.ReportBaseTablePrefix); }
		}

		public void ValidateReportBaseTablePrefix()
		{
			ReportBaseTablePrefixInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ReportBaseTablePrefixInfo, (IMultilingualString)ResString.GetMultilingualString("b2655af6-cc06-4620-ad92-cfa8c8d877b3", "Report Base Table Prefix"));
			if (!ReportBaseTablePrefixInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(ReportBaseTablePrefixInfo, Lookups.ReportBaseTablePrefixList);
			}
			if (!ReportBaseTablePrefixInfo.HasErrors() && ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.GeneralLedgerData && CurrentFallbackLevel != null)
			{
				var generateJournalEntriesStartDate = ObjectFactory.Get<IAccounting>().GetGenerateJournalEntriesStartDate(CurrentFallbackLevel.CompanyPK(false));
				if (generateJournalEntriesStartDate == DateTime.MinValue)
				{
					ReportBaseTablePrefixInfo.AddError(Res.GetString("7a9fe8bc-32b4-4a75-b5f7-b1a7e4735e51", @"The ""GLD - General Ledger Data"" table prefix can only be selected when ""Generate Journal Entries for Posted Accounting Transaction"" is set to ""Yes"" and a ""Generate Journal Entries - Start Date"" value is specified."));
				}
			}
		}

		ZString reportBaseTablePrefix;

		#region ReportPeriodicity

		[List("Lookups.ReportPeriodicityList")]
		[MaxLength(3)]
		public ZString ReportPeriodicity
		{
			get { return reportPeriodicity; }
			set
			{
				CheckMaximumLength(ReportPeriodicityInfo, value);
				SetNonPersistentPropertyValue(ReportPeriodicityInfo, ref reportPeriodicity, value);

				if (!IsValidationSuspended)
				{
					ValidateReportPeriodicity();
				}
			}
		}

		public virtual ZPropertyInfo ReportPeriodicityInfo
		{
			get { return GetZPropertyInfo(Schema.ReportPeriodicity); }
		}

		public void ValidateReportPeriodicity()
		{
			ReportPeriodicityInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ReportPeriodicityInfo, (IMultilingualString)ResString.GetMultilingualString("ebabd7d5-1077-474b-92e0-611fe65049f0", "Report Periodicity"));
			if (!ReportPeriodicityInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(ReportPeriodicityInfo, Lookups.ReportPeriodicityList);
			}
		}

		ZString reportPeriodicity;

		#endregion

		#endregion

		#region TaxRegistrationType

		[List("Lookups.TaxRegistrationTypeList")]
		[MaxLength(3)]
		public ZString TaxRegistrationType
		{
			get { return taxRegistrationType; }
			set
			{
				CheckMaximumLength(TaxRegistrationTypeInfo, value);
				SetNonPersistentPropertyValue(TaxRegistrationTypeInfo, ref taxRegistrationType, value);

				if (!IsValidationSuspended)
				{
					ValidateTaxRegistrationType();
				}
			}
		}

		public virtual ZPropertyInfo TaxRegistrationTypeInfo
		{
			get { return GetZPropertyInfo(Schema.TaxRegistrationType); }
		}

		public void ValidateTaxRegistrationType()
		{
			TaxRegistrationTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TaxRegistrationTypeInfo, (IMultilingualString)ResString.GetMultilingualString("494cbe43-6270-4bd1-a619-ea8ee7f79373", "Tax Registration Type"));
			if (!TaxRegistrationTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(TaxRegistrationTypeInfo, Lookups.TaxRegistrationTypeList);
			}
		}

		ZString taxRegistrationType;

		#endregion

		#region RepCountryRegistrationCodeList

		[List("Lookups.RepCountryRegistrationCodeList")]
		[MaxLength(3)]
		public ZString RepCountryRegistrationCode
		{
			get { return repCountryRegistrationCode; }
			set
			{
				CheckMaximumLength(RepCountryRegistrationCodeInfo, value);
				SetNonPersistentPropertyValue(RepCountryRegistrationCodeInfo, ref repCountryRegistrationCode, value);

				if (!IsValidationSuspended)
				{
					ValidateRepCountryRegistrationCode();
				}
			}
		}

		public virtual ZPropertyInfo RepCountryRegistrationCodeInfo
		{
			get { return GetZPropertyInfo(Schema.RepCountryRegistrationCode); }
		}

		public void ValidateRepCountryRegistrationCode()
		{
			RepCountryRegistrationCodeInfo.ClearAllNotifications();
			if (!RepCountryRegistrationCode.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(RepCountryRegistrationCodeInfo, Lookups.RepCountryRegistrationCodeList);
			}
		}

		ZString repCountryRegistrationCode;

		#endregion

		#region ReportLineGrouping

		[List("Lookups.ReportLineGroupingList")]
		[MaxLength(3)]
		public ZString ReportLineGrouping
		{
			get { return reportLineGrouping; }
			set
			{
				CheckMaximumLength(ReportLineGroupingInfo, value);
				SetNonPersistentPropertyValue(ReportLineGroupingInfo, ref reportLineGrouping, value);

				if (!IsValidationSuspended)
				{
					ValidateReportLineGrouping();
				}
			}
		}

		public ZPropertyInfo ReportLineGroupingInfo
		{
			get { return GetZPropertyInfo(Schema.ReportLineGrouping); }
		}

		public void ValidateReportLineGrouping()
		{
			ReportLineGroupingInfo.ClearAllNotifications();
			if (!ReportLineGroupingInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(ReportLineGroupingInfo, Lookups.ReportLineGroupingList);
			}
		}

		ZString reportLineGrouping;

		#endregion

		#region ReportLineOrdering

		[List("Lookups.ReportLineOrderingList")]
		[MaxLength(3)]
		public ZString ReportLineOrdering
		{
			get { return reportLineOrdering; }
			set
			{
				CheckMaximumLength(ReportLineOrderingInfo, value);
				SetNonPersistentPropertyValue(ReportLineOrderingInfo, ref reportLineOrdering, value);

				if (!IsValidationSuspended)
				{
					ValidateReportLineOrdering();
				}
			}
		}

		public ZPropertyInfo ReportLineOrderingInfo
		{
			get { return GetZPropertyInfo(Schema.ReportLineOrdering); }
		}

		public void ValidateReportLineOrdering()
		{
			ReportLineOrderingInfo.ClearAllNotifications();
			if (!ReportLineOrderingInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(ReportLineOrderingInfo, Lookups.ReportLineOrderingList);
			}
		}

		ZString reportLineOrdering;

		#endregion

		#region ReportAmountsRoundingType

		[List("Lookups.ReportAmountsRoundingTypeList")]
		[MaxLength(3)]
		public ZString ReportAmountsRoundingType
		{
			get { return reportAmountsRoundingType; }
			set
			{
				CheckMaximumLength(ReportAmountsRoundingTypeInfo, value);
				SetNonPersistentPropertyValue(ReportAmountsRoundingTypeInfo, ref reportAmountsRoundingType, value);

				if (!IsValidationSuspended)
				{
					ValidateReportAmountsRoundingType();
				}
			}
		}

		public ZPropertyInfo ReportAmountsRoundingTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ReportAmountsRoundingType); }
		}

		public void ValidateReportAmountsRoundingType()
		{
			ReportAmountsRoundingTypeInfo.ClearAllNotifications();
			if (!ReportAmountsRoundingTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(ReportAmountsRoundingTypeInfo, Lookups.ReportAmountsRoundingTypeList);
			}
		}

		ZString reportAmountsRoundingType;

		bool NotUsingRoundingOrTruncating
		{
			get { return ReportAmountsRoundingType.IsEmpty; }
		}

		#endregion

		#region ReportAmountsRoundingTruncating

		[ReadOnlyMember(nameof(NotUsingRoundingOrTruncating))]
		public ZInt ReportAmountsRoundingTruncating
		{
			get { return reportAmountsRoundingTruncating; }
			set
			{
				SetNonPersistentPropertyValue(ReportAmountsRoundingTruncatingInfo, ref reportAmountsRoundingTruncating, value);

				if (!IsValidationSuspended)
				{
					ValidateReportAmountsRoundingTruncating();
				}
			}
		}

		public ZPropertyInfo ReportAmountsRoundingTruncatingInfo
		{
			get { return GetZPropertyInfo(Schema.ReportAmountsRoundingTruncating); }
		}

		public void ValidateReportAmountsRoundingTruncating()
		{
			ReportAmountsRoundingTruncatingInfo.ClearAllNotifications();
		}

		ZInt reportAmountsRoundingTruncating;

		#endregion

		#region GoodsServiceType

		[List("Lookups.GoodsServiceTypeList")]
		[MaxLength(3)]
		public ZString GoodsServiceType
		{
			get { return goodsServiceType; }
			set
			{
				CheckMaximumLength(GoodsServiceTypeInfo, value);
				SetNonPersistentPropertyValue(GoodsServiceTypeInfo, ref goodsServiceType, value);

				if (!IsValidationSuspended)
				{
					ValidateGoodsServiceType();
				}
			}
		}

		public virtual ZPropertyInfo GoodsServiceTypeInfo
		{
			get { return GetZPropertyInfo(Schema.GoodsServiceType); }
		}

		public void ValidateGoodsServiceType()
		{
			GoodsServiceTypeInfo.ClearAllNotifications();

			if (!GoodsServiceTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(GoodsServiceTypeInfo, Lookups.GoodsServiceTypeList);
			}
		}

		ZString goodsServiceType;

		#endregion

		#region AmountThresholdLevel

		[List("Lookups.AmountThresholdLevelList")]
		[MaxLength(3)]
		public ZString AmountThresholdLevel
		{
			get { return amountThresholdLevel; }
			set
			{
				CheckMaximumLength(TaxRegistrationTypeInfo, value);
				SetNonPersistentPropertyValue(AmountThresholdLevelInfo, ref amountThresholdLevel, value);

				if (AmountThresholdLevel.IsEmpty)
				{
					ExTaxAmountThreshold = ZDecimal.Zero;
					TaxAmountThreshold = ZDecimal.Zero;
				}

				if (!IsValidationSuspended)
				{
					ValidateAmountThresholdLevel();
				}
			}
		}

		public ZPropertyInfo AmountThresholdLevelInfo
		{
			get { return GetZPropertyInfo(Schema.AmountThresholdLevel); }
		}

		public void ValidateAmountThresholdLevel()
		{
			AmountThresholdLevelInfo.ClearAllNotifications();
			if (!AmountThresholdLevelInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(AmountThresholdLevelInfo, Lookups.AmountThresholdLevelList);
			}
		}

		ZString amountThresholdLevel;

		#endregion

		#region ExTaxAmountThreshold

		[ReadOnlyMember(nameof(NotUsingAmountThreshold))]
		[DecimalPlaces(nameof(CountryCurrencyDecimalPlaces))]
		public ZDecimal ExTaxAmountThreshold
		{
			get { return exTaxAmountThreshold; }
			set
			{
				SetNonPersistentPropertyValue(ExTaxAmountThresholdInfo, ref exTaxAmountThreshold, value);

				if (!IsValidationSuspended)
				{
					ValidateExTaxAmountThreshold();
				}
			}
		}

		public ZPropertyInfo ExTaxAmountThresholdInfo
		{
			get { return GetZPropertyInfo(Schema.ExTaxAmountThreshold); }
		}

		public void ValidateExTaxAmountThreshold()
		{
			ExTaxAmountThresholdInfo.ClearAllNotifications();
		}

		ZDecimal exTaxAmountThreshold;

		#endregion

		#region TaxAmountThreshold

		[ReadOnlyMember(nameof(NotUsingAmountThreshold))]
		[DecimalPlaces(nameof(CountryCurrencyDecimalPlaces))]
		public ZDecimal TaxAmountThreshold
		{
			get { return taxAmountThreshold; }
			set
			{
				SetNonPersistentPropertyValue(TaxAmountThresholdInfo, ref taxAmountThreshold, value);

				if (!IsValidationSuspended)
				{
					ValidateTaxAmountThreshold();
				}
			}
		}

		public ZPropertyInfo TaxAmountThresholdInfo
		{
			get { return GetZPropertyInfo(Schema.TaxAmountThreshold); }
		}

		public void ValidateTaxAmountThreshold()
		{
			TaxAmountThresholdInfo.ClearAllNotifications();
		}

		ZDecimal taxAmountThreshold;

		#endregion

		[List("Lookups.RecipientOrgs")]
		public ZGuid RecipientOrgPK
		{
			get { return recipientOrgPK; }
			set
			{
				SetNonPersistentPropertyValue(RecipientOrgPKInfo, ref recipientOrgPK, value);
				if (!IsValidationSuspended)
				{
					ValidateRecipientOrgPK();
				}
			}
		}

		public ZPropertyInfo RecipientOrgPKInfo
		{
			get { return GetZPropertyInfo(Schema.RecipientOrgPK); }
		}

		public void ValidateRecipientOrgPK()
		{
			RecipientOrgPKInfo.ClearAllNotifications();

			if (!RecipientOrgPKInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidPK(RecipientOrgPKInfo, Lookups.RecipientOrgs);
			}
		}

		ZGuid recipientOrgPK;

		public ZBool IncludeQueuedForPreviousPeriod
		{
			get { return includeQueuedForPreviousPeriod; }
			set
			{
				SetNonPersistentPropertyValue(IncludeQueuedForPreviousPeriodInfo, ref includeQueuedForPreviousPeriod, value);

				if (!IsValidationSuspended)
				{
					ValidateIncludeQueuedForPreviousPeriod();
				}
			}
		}

		public virtual ZPropertyInfo IncludeQueuedForPreviousPeriodInfo => GetZPropertyInfo(Schema.IncludeQueuedForPreviousPeriod);

		public void ValidateIncludeQueuedForPreviousPeriod()
		{
			IncludeQueuedForPreviousPeriodInfo.ClearAllNotifications();

			if (!IncludeQueuedForPreviousPeriodInfo.HasErrors() &&
				IncludeQueuedForPreviousPeriod)
			{
				if (ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.AllTransactions)
				{
					IncludeQueuedForPreviousPeriodInfo.AddError(Res.GetString("52987b65-f750-4af9-bdec-e21028a503cd", "Include previous queued records can't be true if the Table Prefix is set on All Transaction."));
				}
				if (ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.GeneralLedgerData)
				{
					IncludeQueuedForPreviousPeriodInfo.AddError(Res.GetString("b923e285-5468-4e5d-9571-516abced1735", "Include previous queued records can't be true if the Table Prefix is set on General Ledger Data."));
				}
			}
		}

		ZBool includeQueuedForPreviousPeriod;

		public ZBool IsDefaultReportType
		{
			get { return isDefaultReportType; }
			set
			{
				if (value != isDefaultReportType)
				{
					SetNonPersistentPropertyValue(IsDefaultReportTypeInfo, ref isDefaultReportType, value);

					if (!IsValidationSuspended)
					{
						ValidateIsDefaultReportType();
					}
				}
			}
		}

		public virtual ZPropertyInfo IsDefaultReportTypeInfo => GetZPropertyInfo(Schema.IsDefaultReportType);

		public void ValidateIsDefaultReportType()
		{
			IsDefaultReportTypeInfo.ClearAllNotifications();

			if (!IsDefaultReportTypeInfo.HasErrors() && IsDefaultReportType &&
				ParentCollection.Cast<ComplianceReportConfiguration>().Any(x => x.PK != PK && x.IsDefaultReportType))
			{
				IsDefaultReportTypeInfo.AddError(Res.GetString("50e26b0d-f873-4d87-96b9-8a5dcf2bc26a", "There must be only one default report."));
			}
		}

		ZBool isDefaultReportType;

		#endregion

		#region Implementation

		void UpdateCountryRelatedData()
		{
			TaxRegistrationType = ZString.Empty;
			RepCountryRegistrationCode = ZString.Empty;
			Settings.CountryCode = Country;
		}

		bool NotUsingAmountThreshold
		{
			get { return AmountThresholdLevel.IsEmpty; }
		}

		public int CountryCurrencyDecimalPlaces
		{
			get
			{
				var loadedCountry = RefCountry.LoadFromCountryCode((Factory ?? (helperFactory ?? (helperFactory = new BusinessObjectFactory()))), Country);
				return loadedCountry != null ? loadedCountry.LocalCurrency.Decimals : 0;
			}
		}

		BusinessObjectFactory helperFactory;

		#endregion

		#region ComplianceReportConfigurationLookups

		public ComplianceReportConfigurationLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = GetNewLookups();
				}
				return lookups;
			}
		}

		protected virtual ComplianceReportConfigurationLookups GetNewLookups()
		{
			return new ComplianceReportConfigurationLookups(this);
		}

		ComplianceReportConfigurationLookups lookups;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Country, Country);
			writer.WriteElementString(Schema.ReportCode, ReportCode);
			writer.WriteElementString(Schema.ReportTitle, ReportTitle);
			writer.WriteElementString(Schema.ReportBaseTablePrefix, ReportBaseTablePrefix);
			writer.WriteElementString(Schema.ReportPeriodicity, ReportPeriodicity);
			writer.WriteElementString(Schema.TaxRegistrationType, TaxRegistrationType);
			writer.WriteElementString(Schema.ReportLineGrouping, ReportLineGrouping);
			writer.WriteElementString(Schema.ReportLineOrdering, ReportLineOrdering);
			writer.WriteElementString(Schema.GoodsServiceType, GoodsServiceType);
			writer.WriteElementString(Schema.ReportAmountsRoundingType, ReportAmountsRoundingType);
			writer.WriteElementString(Schema.ReportAmountsRoundingTruncating, ReportAmountsRoundingTruncating.ToString());
			writer.WriteElementString(Schema.AmountThresholdLevel, AmountThresholdLevel);
			writer.WriteElementString(Schema.ExTaxAmountThreshold, ExTaxAmountThreshold.ToString());
			writer.WriteElementString(Schema.TaxAmountThreshold, TaxAmountThreshold.ToString());
			writer.WriteElementString(Schema.RecipientOrgPK, RecipientOrgPK.ToString());
			writer.WriteElementString(Schema.RepCountryRegistrationCode, RepCountryRegistrationCode);
			writer.WriteElementString(Schema.IncludeQueuedForPreviousPeriod, IncludeQueuedForPreviousPeriod.ToString());
			writer.WriteElementString(Schema.IsDefaultReportType, IsDefaultReportType.ToString());

			CollectionSerialiser.Serialize(writer, Settings);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Country = reader.ReadElementString(Schema.Country);
			ReportCode = reader.ReadElementString(Schema.ReportCode);
			ReportTitle = reader.ReadElementString(Schema.ReportTitle);
			ReportBaseTablePrefix = reader.ReadElementString(Schema.ReportBaseTablePrefix);
			ReportPeriodicity = reader.ReadElementString(Schema.ReportPeriodicity);
			TaxRegistrationType = reader.ReadElementString(Schema.TaxRegistrationType);
			ReportLineGrouping = reader.ReadElementString(Schema.ReportLineGrouping);
			ReportLineOrdering = reader.ReadElementString(Schema.ReportLineOrdering);
			GoodsServiceType = reader.ReadElementString(Schema.GoodsServiceType);
			ReportAmountsRoundingType = reader.ReadElementString(Schema.ReportAmountsRoundingType);
			ReportAmountsRoundingTruncating = ZInt.ParseSafe(reader.ReadElementString(Schema.ReportAmountsRoundingTruncating), ZInt.Zero);
			AmountThresholdLevel = reader.ReadElementString(Schema.AmountThresholdLevel);
			ExTaxAmountThreshold = ZDecimal.ParseSafe(reader.ReadElementString(Schema.ExTaxAmountThreshold), ZDecimal.Zero);
			TaxAmountThreshold = ZDecimal.ParseSafe(reader.ReadElementString(Schema.TaxAmountThreshold), ZDecimal.Zero);
			ZGuid org;
			RecipientOrgPK = ZGuid.TryParse(reader.ReadElementString(Schema.RecipientOrgPK), out org) ? org : ZGuid.Empty;
			RepCountryRegistrationCode = reader.ReadElementString(Schema.RepCountryRegistrationCode);
			IncludeQueuedForPreviousPeriod = reader.ReadElementStringAsZBool(Schema.IncludeQueuedForPreviousPeriod);
			IsDefaultReportType = reader.ReadElementStringAsZBool(Schema.IsDefaultReportType);

			SetNewSettings(((ComplianceReportConfigurationSettingCollection)CollectionSerialiser.Deserialize(reader)));
		}

		ZXmlSerializer CollectionSerialiser
		{
			get { return collectionSerialiser ?? (collectionSerialiser = ZXmlSerializer.New(typeof(ComplianceReportConfigurationSettingCollection))); }
		}

		ZXmlSerializer collectionSerialiser;

		#endregion
	}
}
