using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;
using ResString = Enterprise.MasterFiles.Business.ResString;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceReportConfigurationSetting : RegistryBusinessObjectTemplate, IAccComplianceRule
	{
		public ComplianceReportConfigurationSetting()
			: base()
		{ }

		public ComplianceReportConfigurationSetting(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{ }

		#region Schema

		abstract class Schema
		{
			// Filtering criterias for Queue and Report Output
			public const string Country = "Country";
			public const string ComplianceSubType = "ComplianceSubType";
			public const string LedgerType = "LedgerType";
			public const string InvoiceType = "InvoiceType";
			public const string TaxInvoiceRule = "TaxInvoiceRule";
			public const string DisbursementRule = "DisbursementRule";
			public const string OriginalRule = "OriginalRule";
			public const string OrganisationCategory = "OrganisationCategory";
			public const string OrganisationLocation = "OrganisationLocation";
			public const string TaxRegistrationType = "TaxRegistrationType";
			public const string SelfBillingRule = "SelfBillingRule";
			public const string TaxRegistrationLocationRule = "TaxRegistrationLocationRule";
			public const string VATGroupRule = "VATGroupRule";
			public const string ReportingDate = "ReportingDate";
		}

		#endregion

		readonly MultilingualString DifferentTaxInvoiceRuleExists = ResString.GetMultilingualString("e3b95e9f-2be8-4914-8d1e-550f70ddef9f", "Different tax invoice rule already exists in another setting for the same Report");
		readonly MultilingualString IdenticalConfigurationExists = ResString.GetMultilingualString("74502567-ff20-471b-953d-820d81c3e4cf", "Setting with identical criteria already exists!");
		readonly MultilingualString OverlappedConfigurationExists = ResString.GetMultilingualString("b1cf84f4-08dc-4055-b950-b43049412df2", "Overlapped settings are detected, please check Original rule/Disbursement Rule is not overlapped");

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceReportConfigurationSetting(fallbackLevel, factory);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCountry();
			ValidateComplianceSubType();
			ValidateLedgerType();
			ValidateInvoiceType();
			ValidateOrganisationLocation();
			ValidateTaxInvoiceRule();
			ValidateOriginalRule();
			ValidateTaxRegistrationType();
			ValidateDisbursementRule();
			ValidateSelfBillingRule();
			ValidateTaxRegistrationLocationRule();
			ValidateVatGroupRule();
			ValidateReportingDate();
			ValidateOrganisationCategory();
		}

		public ComplianceReportConfigurationSettingCollection ParentCollection
		{
			get
			{
				var result = ((IBusinessObjectInternals)this).ParentCollections.FirstOrDefault(x => x is ComplianceReportConfigurationSettingCollection) as ComplianceReportConfigurationSettingCollection;
				return result ?? new ComplianceReportConfigurationSettingCollection(Factory, Country);
			}
		}

		#region Bound Properties

		#region Country

		[List("Lookups.CountryList")]
		[MaxLength(2)]
		public ZString Country
		{
			get { return country; }
			internal set
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

		protected bool Country_ReadOnly = true;

		public ZPropertyInfo CountryInfo
		{
			get { return GetZPropertyInfo(Schema.Country); }
		}

		public void ValidateCountry()
		{
			CountryInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CountryInfo, (IMultilingualString)ResString.GetMultilingualString("05f337bb-20e5-45e5-bbd8-5bbd526a2782", "Country/Region"));
			if (!CountryInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(CountryInfo, Lookups.CountryList);
			}
		}

		ZString country;

		#endregion

		#region ComplianceSubType

		ZString IAccComplianceRule.SubType
		{
			get { return ComplianceSubType; }
		}

		[List("Lookups.SubTypeList")]
		[MaxLength(3)]
		public ZString ComplianceSubType
		{
			get { return complianceSubType; }
			set
			{
				var isChanged = SetNonPersistentPropertyValue(ComplianceSubTypeInfo, ref complianceSubType, value);
				if (!IsValidationSuspended)
				{
					ValidateComplianceSubType();
				}
			}
		}

		public ZPropertyInfo ComplianceSubTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ComplianceSubType); }
		}

		public void ValidateComplianceSubType()
		{
			ComplianceSubTypeInfo.ClearAllNotifications();
			if (!ComplianceSubTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(ComplianceSubTypeInfo, Lookups.SubTypeList);
			}
			ValidateConfigurationSet(ComplianceSubTypeInfo);
		}

		ZString complianceSubType;

		#region ParentTransactionSubtype

		[MaxLength(3)]
		ZString IAccComplianceRule.ParentTransactionSubType
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Ledger

		[List("Lookups.LedgerTypeList")]
		[MaxLength(2)]
		public ZString LedgerType
		{
			get { return ledgerType; }
			set
			{
				var hasChanges = !value.Equals(ledgerType);
				SetNonPersistentPropertyValue(LedgerTypeInfo, ref ledgerType, value);
				if (!IsValidationSuspended)
				{
					ValidateLedgerType();
				}

				if (hasChanges)
				{
					ReportingDate = Lookups.ReportingDateList.ContainsCode(ComplianceSubTypeCodesAndLists.ReportingDateCodes.PostDate)
						? new ZString(ComplianceSubTypeCodesAndLists.ReportingDateCodes.PostDate)
						: ZString.Empty;
				}
			}
		}

		public ZPropertyInfo LedgerTypeInfo
		{
			get { return GetZPropertyInfo(Schema.LedgerType); }
		}

		public void ValidateLedgerType()
		{
			LedgerTypeInfo.ClearAllNotifications();
			if (ComplianceSubType.IsEmpty)
			{
				MandatoryValidation.CheckEntered(LedgerTypeInfo, (IMultilingualString)ResString.GetMultilingualString("7b8c46aa-4401-4530-95f1-f45be4700f42", "Ledger Type"));
			}
			if (!LedgerTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(LedgerTypeInfo, Lookups.LedgerTypeList);
			}
			ValidateConfigurationSet(LedgerTypeInfo);
		}

		ZString ledgerType;

		#endregion

		#region InvoiceType

		[List("Lookups.InvoiceTypeList")]
		[MaxLength(3)]
		public ZString InvoiceType
		{
			get { return invoiceType; }
			set
			{
				SetNonPersistentPropertyValue(InvoiceTypeInfo, ref invoiceType, value);
				if (!IsValidationSuspended)
				{
					ValidateInvoiceType();
				}
			}
		}

		public ZPropertyInfo InvoiceTypeInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceType); }
		}

		public void ValidateInvoiceType()
		{
			InvoiceTypeInfo.ClearAllNotifications();
			if (ComplianceSubType.IsEmpty)
			{
				MandatoryValidation.CheckEntered(InvoiceTypeInfo, (IMultilingualString)ResString.GetMultilingualString("e8c5ff46-fcf4-4633-9db9-29061e12cc40", "Invoice Type"));
			}
			if (!InvoiceTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(InvoiceTypeInfo, Lookups.InvoiceTypeList);
			}
			ValidateConfigurationSet(InvoiceTypeInfo);
		}

		ZString invoiceType;

		#endregion

		#region OrganisationCategory

		[List("Lookups.OrganisationCategoryList")]
		[MaxLength(3)]
		public ZString OrganisationCategory
		{
			get { return organisationCategory; }
			set
			{
				SetNonPersistentPropertyValue(OrganisationCategoryInfo, ref organisationCategory, value);
				if (!IsValidationSuspended)
				{
					ValidateOrganisationCategory();
				}
			}
		}

		public ZPropertyInfo OrganisationCategoryInfo
		{
			get { return GetZPropertyInfo(Schema.OrganisationCategory); }
		}

		public void ValidateOrganisationCategory()
		{
			OrganisationCategoryInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(OrganisationCategoryInfo, Lookups.OrganisationCategoryList);
			ValidateConfigurationSet(OrganisationCategoryInfo);
		}

		ZString organisationCategory;

		#endregion

		#region OrganisationLocation

		[List("Lookups.Locations")]
		[MaxLength(4)]
		public ZString OrganisationLocation
		{
			get { return organisationLocation; }
			set
			{
				SetNonPersistentPropertyValue(OrganisationLocationInfo, ref organisationLocation, value);
				if (!IsValidationSuspended)
				{
					ValidateOrganisationLocation();
				}
			}
		}

		public ZPropertyInfo OrganisationLocationInfo
		{
			get { return GetZPropertyInfo(Schema.OrganisationLocation); }
		}

		public void ValidateOrganisationLocation()
		{
			OrganisationLocationInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(OrganisationLocationInfo, Lookups.Locations);
			ValidateConfigurationSet(OrganisationLocationInfo);
		}

		ZString organisationLocation;

		#endregion

		#region TaxRegistrationType

		[List("Lookups.TaxRegistrationTypeList")]
		[MaxLength(3)]
		[ReadOnlyMember(nameof(TaxRegistrationTypeIsReadOnly))]
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

		bool TaxRegistrationTypeIsReadOnly
		{
			get { return Country != Core.Constants.CountryCodes.Argentina; }
		}

		public virtual ZPropertyInfo TaxRegistrationTypeInfo
		{
			get { return GetZPropertyInfo(Schema.TaxRegistrationType); }
		}

		public void ValidateTaxRegistrationType()
		{
			TaxRegistrationTypeInfo.ClearAllNotifications();

			if (Country == Core.Constants.CountryCodes.Argentina)
			{
				if (ComplianceSubType.IsEmpty)
				{
					MandatoryValidation.CheckEntered(TaxRegistrationTypeInfo, (IMultilingualString)ResString.GetMultilingualString("9d3a0811-c6ca-4470-955b-157493e49387", "Tax Registration Type"));
				}
				if (!TaxRegistrationTypeInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidCode(TaxRegistrationTypeInfo, Lookups.TaxRegistrationTypeList);
				}
				ValidateConfigurationSet(TaxRegistrationTypeInfo);
			}
		}

		ZString taxRegistrationType;

		#endregion

		#region TaxInvoiceRule

		[List("Lookups.TaxInvoiceRuleList")]
		[MaxLength(3)]
		public ZString TaxInvoiceRule
		{
			get { return taxInvoiceRule; }
			set
			{
				CheckMaximumLength(TaxInvoiceRuleInfo, value);
				SetNonPersistentPropertyValue(TaxInvoiceRuleInfo, ref taxInvoiceRule, value);

				if (!IsValidationSuspended)
				{
					ValidateTaxInvoiceRule();
				}
			}
		}

		public virtual ZPropertyInfo TaxInvoiceRuleInfo
		{
			get { return GetZPropertyInfo(Schema.TaxInvoiceRule); }
		}

		public void ValidateTaxInvoiceRule()
		{
			TaxInvoiceRuleInfo.ClearAllNotifications();
			if (ComplianceSubType.IsEmpty)
			{
				MandatoryValidation.CheckEntered(TaxInvoiceRuleInfo, (IMultilingualString)ResString.GetMultilingualString("a437060c-cddb-4f56-9c83-9138e1d32035", "Tax Invoice Rule"));
			}
			if (!TaxInvoiceRuleInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(TaxInvoiceRuleInfo, Lookups.TaxInvoiceRuleList);
			}
			ValidateConfigurationSet(TaxInvoiceRuleInfo);
		}

		ZString taxInvoiceRule;

		#endregion

		#region TaxIDCode

		IReadOnlyCollection<ZString> IAccComplianceRule.SeparatedTaxIDCodes { get; }

		#endregion

		#region OriginalRule

		[List("Lookups.OriginalRuleList")]
		[MaxLength(3)]
		public ZString OriginalRule
		{
			get { return originalRule; }
			set
			{
				CheckMaximumLength(OriginalRuleInfo, value);
				SetNonPersistentPropertyValue(OriginalRuleInfo, ref originalRule, value);

				if (!IsValidationSuspended)
				{
					ValidateOriginalRule();
				}
			}
		}

		public virtual ZPropertyInfo OriginalRuleInfo
		{
			get { return GetZPropertyInfo(Schema.OriginalRule); }
		}

		public void ValidateOriginalRule()
		{
			OriginalRuleInfo.ClearAllNotifications();
			if (ComplianceSubType.IsEmpty)
			{
				MandatoryValidation.CheckEntered(OriginalRuleInfo, (IMultilingualString)ResString.GetMultilingualString("ea8d6b53-ad23-40bd-bb8d-d9c92ffcf764", "Original Rule"));
			}
			if (!OriginalRuleInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(OriginalRuleInfo, Lookups.OriginalRuleList);
			}
			ValidateConfigurationSet(OriginalRuleInfo);
		}

		ZString originalRule;

		#endregion

		#region DisbursementRule

		[List("Lookups.DisbursementRuleList")]
		[MaxLength(3)]
		public ZString DisbursementRule
		{
			get { return disbursementRule; }
			set
			{
				CheckMaximumLength(DisbursementRuleInfo, value);
				SetNonPersistentPropertyValue(DisbursementRuleInfo, ref disbursementRule, value);
				if (!IsValidationSuspended)
				{
					ValidateDisbursementRule();
				}
			}
		}

		public virtual ZPropertyInfo DisbursementRuleInfo
		{
			get { return GetZPropertyInfo(Schema.DisbursementRule); }
		}

		public void ValidateDisbursementRule()
		{
			DisbursementRuleInfo.ClearAllNotifications();
			if (ComplianceSubType.IsEmpty)
			{
				MandatoryValidation.CheckEntered(DisbursementRuleInfo, (IMultilingualString)ResString.GetMultilingualString("adc5e239-2074-4487-97dd-c209e1f578c7", "Disbursement Rule"));
			}
			if (!DisbursementRuleInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(DisbursementRuleInfo, Lookups.DisbursementRuleList);
			}
			ValidateConfigurationSet(DisbursementRuleInfo);
		}

		ZString disbursementRule;

		#endregion

		#region Self-Billing Rule

		[List("Lookups.SelfBillingRuleList")]
		[MaxLength(3)]
		public ZString SelfBillingRule
		{
			get { return selfBillingRule; }
			set
			{
				CheckMaximumLength(SelfBillingRuleInfo, value);
				SetNonPersistentPropertyValue(SelfBillingRuleInfo, ref selfBillingRule, value);
				if (!IsValidationSuspended)
				{
					ValidateSelfBillingRule();
				}
			}
		}

		public virtual ZPropertyInfo SelfBillingRuleInfo
		{
			get { return GetZPropertyInfo(Schema.SelfBillingRule); }
		}

		public void ValidateSelfBillingRule()
		{
			SelfBillingRuleInfo.ClearAllNotifications();
			if (!SelfBillingRuleInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(SelfBillingRuleInfo, Lookups.SelfBillingRuleList);
			}
			if (!SelfBillingRuleInfo.HasErrors() && CheckDifferentTaxInvoiceRuleExist())
			{
				SelfBillingRuleInfo.AddError(DifferentTaxInvoiceRuleExists);
			}
			if (!SelfBillingRuleInfo.HasErrors() && CheckIdenticalConfigurationExists())
			{
				SelfBillingRuleInfo.AddError(IdenticalConfigurationExists);
			}
			if (!SelfBillingRuleInfo.HasErrors() && CheckOverlappedConfigurationExists())
			{
				SelfBillingRuleInfo.AddError(OverlappedConfigurationExists);
			}
		}

		ZString selfBillingRule;

		#endregion

		#region Exporter Exemption

		public ZString ExporterExemption { get; set; }

		#endregion

		#region Tax Registration Location

		[List("Lookups.TaxRegistrationLocationRuleList")]
		[MaxLength(3)]
		public ZString TaxRegistrationLocationRule
		{
			get { return taxRegistrationLocationRule; }
			set
			{
				CheckMaximumLength(TaxRegistrationLocationRuleInfo, value);
				SetNonPersistentPropertyValue(TaxRegistrationLocationRuleInfo, ref taxRegistrationLocationRule, value);
				if (!IsValidationSuspended)
				{
					ValidateTaxRegistrationLocationRule();
				}
			}
		}

		public virtual ZPropertyInfo TaxRegistrationLocationRuleInfo
		{
			get { return GetZPropertyInfo(Schema.TaxRegistrationLocationRule); }
		}

		public void ValidateTaxRegistrationLocationRule()
		{
			TaxRegistrationLocationRuleInfo.ClearAllNotifications();
			if (!TaxRegistrationLocationRuleInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(TaxRegistrationLocationRuleInfo, Lookups.TaxRegistrationLocationRuleList);
			}
			if (!TaxRegistrationLocationRuleInfo.HasErrors() && CheckDifferentTaxInvoiceRuleExist())
			{
				TaxRegistrationLocationRuleInfo.AddError(DifferentTaxInvoiceRuleExists);
			}
			if (!TaxRegistrationLocationRuleInfo.HasErrors() && CheckIdenticalConfigurationExists())
			{
				TaxRegistrationLocationRuleInfo.AddError(IdenticalConfigurationExists);
			}
			if (!TaxRegistrationLocationRuleInfo.HasErrors() && CheckOverlappedConfigurationExists())
			{
				TaxRegistrationLocationRuleInfo.AddError(OverlappedConfigurationExists);
			}
		}

		ZString taxRegistrationLocationRule;

		#endregion

		#region VAT Group Member

		[List("Lookups.VATGroupList")]
		[MaxLength(3)]
		public ZString VATGroupRule
		{
			get { return vatGroupRule; }
			set
			{
				CheckMaximumLength(VATGroupRuleInfo, value);
				SetNonPersistentPropertyValue(VATGroupRuleInfo, ref vatGroupRule, value);
				if (!IsValidationSuspended)
				{
					ValidateVatGroupRule();
				}
			}
		}
		ZString vatGroupRule;

		public virtual ZPropertyInfo VATGroupRuleInfo
		{
			get { return GetZPropertyInfo(Schema.VATGroupRule); }
		}

		public void ValidateVatGroupRule()
		{
			VATGroupRuleInfo.ClearAllNotifications();
			if (!VATGroupRuleInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(VATGroupRuleInfo, Lookups.VATGroupList);
			}
			if (!VATGroupRuleInfo.HasErrors() && CheckDifferentTaxInvoiceRuleExist())
			{
				VATGroupRuleInfo.AddError(DifferentTaxInvoiceRuleExists);
			}
			if (!VATGroupRuleInfo.HasErrors() && CheckIdenticalConfigurationExists())
			{
				VATGroupRuleInfo.AddError(IdenticalConfigurationExists);
			}
			if (!VATGroupRuleInfo.HasErrors() && CheckOverlappedConfigurationExists())
			{
				VATGroupRuleInfo.AddError(OverlappedConfigurationExists);
			}
		}

		#endregion

		#region Reporting Date

		[List("Lookups.ReportingDateList")]
		[ReadOnlyMember(nameof(ReportingDate_ReadOnly))]
		[MaxLength(3)]
		public ZString ReportingDate
		{
			get { return reportingDate; }
			set
			{
				CheckMaximumLength(ReportingDateInfo, value);
				SetNonPersistentPropertyValue(ReportingDateInfo, ref reportingDate, value);
				if (!IsValidationSuspended)
				{
					ValidateReportingDate();
				}
			}
		}
		ZString reportingDate;

		public virtual ZPropertyInfo ReportingDateInfo
		{
			get { return GetZPropertyInfo(Schema.ReportingDate); }
		}

		public void ValidateReportingDate()
		{
			ReportingDateInfo.ClearAllNotifications();
			if (!ReportingDateInfo.HasErrors() && !ReportingDateInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(ReportingDateInfo);
			}
			if (!ReportingDateInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(ReportingDateInfo, Lookups.ReportingDateList);
			}
		}

		bool ReportingDate_ReadOnly => Lookups.ReportingDateList.Count == 0;

		#endregion

		#region IAccComplianceRule Currently not used by this type of configuration

		ZString IAccComplianceRule.RequiredTaxSystem => ZString.Empty;

		ZString IAccComplianceRule.ExcludedTaxSystem => ZString.Empty;

		ZString IAccComplianceRule.RequiredRegistrationCode => ZString.Empty;

		ZString IAccComplianceRule.ExcludedRegistrationCode => ZString.Empty;

		ZBool IAccComplianceRule.ThresholdApplies => ZBool.False;

		ZString IAccComplianceRule.SubTypeThresholdNotMet => ZString.Empty;

		#endregion

		void ValidateConfigurationSet(ZPropertyInfo info)
		{
			if (!info.HasErrors() && CheckDifferentTaxInvoiceRuleExist())
			{
				info.AddError(DifferentTaxInvoiceRuleExists);
			}
			if (!info.HasErrors() && CheckIdenticalConfigurationExists())
			{
				info.AddError(IdenticalConfigurationExists);
			}
			if (!info.HasErrors() && CheckOverlappedConfigurationExists())
			{
				info.AddError(OverlappedConfigurationExists);
			}
		}

		ZBool CheckDifferentTaxInvoiceRuleExist()
		{
			ZBool result = ZBool.False;
			if (ParentCollections.Count > 0)
			{
				foreach (ComplianceReportConfigurationSetting item in ParentCollection)
				{
					if (PK != item.PK
						&& Country == item.Country
						&& ComplianceSubType == item.ComplianceSubType
						&& LedgerType == item.LedgerType
						&& InvoiceType == item.InvoiceType
						&& OriginalRule == item.OriginalRule
						&& OrganisationLocation == item.OrganisationLocation
						&& DisbursementRule == item.DisbursementRule
						&& (Country != Core.Constants.CountryCodes.Argentina || TaxRegistrationType == item.TaxRegistrationType)
						&& TaxInvoiceRule != item.TaxInvoiceRule && !TaxInvoiceRule.IsEmpty
						&& SelfBillingRule == item.SelfBillingRule
						&& TaxRegistrationLocationRule == item.TaxRegistrationLocationRule
						&& VATGroupRule == item.vatGroupRule
						&& OrganisationCategory == item.OrganisationCategory)
					{
						result = ZBool.True;
						break;
					}
				}
			}
			return result;
		}

		ZBool CheckIdenticalConfigurationExists()
		{
			ZBool result = ZBool.False;
			if (ParentCollections.Count > 0)
			{
				foreach (ComplianceReportConfigurationSetting item in ParentCollection)
				{
					if (PK != item.PK
						&& Country == item.Country
						&& ComplianceSubType == item.ComplianceSubType
						&& LedgerType == item.LedgerType
						&& InvoiceType == item.InvoiceType
						&& TaxInvoiceRule == item.TaxInvoiceRule
						&& DisbursementRule == item.DisbursementRule
						&& OriginalRule == item.OriginalRule
						&& OrganisationLocation == item.OrganisationLocation
						&& (Country != Core.Constants.CountryCodes.Argentina || TaxRegistrationType == item.TaxRegistrationType)
						&& (SelfBillingRule == item.SelfBillingRule)
						&& (TaxRegistrationLocationRule == item.TaxRegistrationLocationRule)
						&& (VATGroupRule == item.VATGroupRule)
						&& (OrganisationCategory == item.OrganisationCategory))
					{
						result = ZBool.True;
						break;
					}
				}
			}
			return result;
		}

		ZBool CheckOverlappedConfigurationExists()
		{
			ZBool result = ZBool.False;
			if (ParentCollections.Count > 0)
			{
				foreach (ComplianceReportConfigurationSetting item in ParentCollection)
				{
					if (PK != item.PK
						&& Country == item.Country
						&& ComplianceSubType == item.ComplianceSubType
						&& LedgerType == item.LedgerType
						&& InvoiceType == item.InvoiceType
						&& OrganisationLocation == item.OrganisationLocation
						&& OrganisationCategory == item.OrganisationCategory)
					{
						bool overlapDisbursementRuleDetected = false;
						bool overlapOriginalRuleDetected = false;
						bool overlapTaxInvoiceRuleDetected = false;
						bool overlapTaxRegistrationTypeDetected = false;
						bool overlapSelfBillingRuleDetected = false;
						bool overlapTaxRegistrationLocationDetected = false;
						bool overlapVATGroupDetected = false;

						overlapDisbursementRuleDetected = DisbursementRule == item.DisbursementRule ||
							DisbursementRule == DisbursementRuleCodes.AllTransactions ||
							item.DisbursementRule == DisbursementRuleCodes.AllTransactions;

						overlapOriginalRuleDetected = OriginalRule == item.OriginalRule ||
							OriginalRule == OriginalRuleCodes.AllTransactions ||
							item.OriginalRule == OriginalRuleCodes.AllTransactions;

						overlapTaxInvoiceRuleDetected = TaxInvoiceRule == item.TaxInvoiceRule;

						overlapTaxRegistrationTypeDetected = Country != Core.Constants.CountryCodes.Argentina || TaxRegistrationType == item.TaxRegistrationType;

						overlapSelfBillingRuleDetected = SelfBillingRule == item.SelfBillingRule;

						overlapTaxRegistrationLocationDetected = TaxRegistrationLocationRule == item.TaxRegistrationLocationRule;

						overlapVATGroupDetected = VATGroupRule == item.VATGroupRule;

						if (overlapDisbursementRuleDetected &&
							overlapOriginalRuleDetected &&
							overlapTaxInvoiceRuleDetected &&
							overlapTaxRegistrationTypeDetected &&
							overlapSelfBillingRuleDetected &&
							overlapTaxRegistrationLocationDetected &&
							overlapVATGroupDetected)
						{
							result = ZBool.True;
							break;
						}
					}
				}
			}
			return result;
		}

		#endregion

		#endregion

		void UpdateCountryRelatedData()
		{
			if (Country != Core.Constants.CountryCodes.Argentina)
			{
				TaxRegistrationType = ZString.Empty;
			}
		}

		#region ComplianceReportConfigurationSettingLookups

		public ComplianceReportConfigurationSettingLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = GetNewLookups();
				}
				return fLookups;
			}
		}

		protected virtual ComplianceReportConfigurationSettingLookups GetNewLookups()
		{
			return new ComplianceReportConfigurationSettingLookups(this);
		}

		ComplianceReportConfigurationSettingLookups fLookups;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Country, Country);
			writer.WriteElementString(Schema.ComplianceSubType, ComplianceSubType);
			writer.WriteElementString(Schema.LedgerType, LedgerType);
			writer.WriteElementString(Schema.InvoiceType, InvoiceType);
			writer.WriteElementString(Schema.TaxInvoiceRule, TaxInvoiceRule.ToString());
			writer.WriteElementString(Schema.DisbursementRule, DisbursementRule.ToString());
			writer.WriteElementString(Schema.OriginalRule, OriginalRule.ToString());
			writer.WriteElementString(Schema.OrganisationLocation, OrganisationLocation);
			writer.WriteElementString(Schema.TaxRegistrationType, TaxRegistrationType.ToString());
			writer.WriteElementString(Schema.ReportingDate, ReportingDate.ToString());
			writer.WriteElementString(Schema.OrganisationCategory, OrganisationCategory.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Country = reader.ReadElementString(Schema.Country);
			ComplianceSubType = reader.ReadElementString(Schema.ComplianceSubType);
			LedgerType = reader.ReadElementString(Schema.LedgerType);
			InvoiceType = reader.ReadElementString(Schema.InvoiceType);
			TaxInvoiceRule = reader.ReadElementString(Schema.TaxInvoiceRule);
			DisbursementRule = reader.ReadElementString(Schema.DisbursementRule);
			OriginalRule = reader.ReadElementString(Schema.OriginalRule);
			OrganisationLocation = reader.ReadElementString(Schema.OrganisationLocation);
			TaxRegistrationType = reader.ReadElementString(Schema.TaxRegistrationType);

			var reportingDate = reader.ReadElementString(Schema.ReportingDate);
			if (!string.IsNullOrWhiteSpace(reportingDate))
			{
				ReportingDate = reportingDate;
			}

			OrganisationCategory = reader.ReadElementString(Schema.OrganisationCategory);
		}

		#endregion
	}
}
