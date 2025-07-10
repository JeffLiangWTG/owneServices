using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;
using Res = Enterprise.MasterFiles.Business.Res;
using ResString = Enterprise.MasterFiles.Business.ResString;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceSubTypeAttributionRuleConfiguration : RegistryBusinessObjectTemplate, IAccComplianceRule
	{
		#region Schema

		abstract class Schema
		{
			public const string NewElementsXMLNode = "NewElementsXMLNode";

			public const string Country = "Country";
			public const string SubType = "SubType";
			public const string DocumentTitle = "DocumentTitle";
			public const string LedgerType = "LedgerType";
			public const string InvoiceType = "InvoiceType";
			public const string TaxInvoiceRule = "TaxInvoiceRule";
			public const string TaxIDCode = "TaxIDCode";
			public const string DisbursementRule = "DisbursementRule";
			public const string OriginalRule = "OriginalRule";
			public const string OrganisationLocation = "OrganisationLocation";
			public const string SelfBillingRule = "SelfBillingRule";
			public const string ExporterExemption = "ExporterExemption";
			public const string TaxRegistrationType = "TaxRegistrationType";
			public const string TaxRegistrationLocationRule = "TaxRegistrationLocationRule";
			public const string VatGroupRule = "VATGroupRule";
			public const string RuleSetCode = "RuleSetCode";
			public const string RuleSetDescription = "RuleSetDescription";
			public const string ParentTransactionSubType = "ParentTransactionSubType";
			public const string RequiredTaxSystem = "RequiredTaxSystem";
			public const string ExcludedTaxSystem = "ExcludedTaxSystem";
			public const string RequiredRegistrationCode = "RequiredRegistrationCode";
			public const string ExcludedRegistrationCode = "ExcludedRegistrationCode";
			public const string ThresholdApplies = "ThresholdApplies";
			public const string SubTypeThresholdNotMet = "SubTypeThresholdNotMet";
			public const string OrganisationCategory = "OrganisationCategory";
		}

		readonly MultilingualString IdenticalConfigurationExists = ResString.GetMultilingualString("50B950F1-54B2-4EFF-AC97-A06CA8FB70C6", "configuration with identical criteria already exists!");
		readonly MultilingualString OverlappedConfigurationExists = ResString.GetMultilingualString("E35C9966-035A-486C-A390-209EB463AD4C", "overlapped configurations detected");

		public static class CalculatedFieldNames
		{
			public const string Description = nameof(Description);
		}

		#endregion

		public ComplianceSubTypeAttributionRuleConfiguration()
		{ }

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceSubTypeAttributionRuleConfiguration();
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCountry();
			ValidateSubType();
			ValidateDocumentTitle();
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
			ValidateParentTransactionComplianceSubType();
			ValidateRequiredTaxSystem();
			ValidateExcludedTaxSystem();
			ValidateExcludedRegistrationCode();
			ValidateRequiredRegistrationCode();
			ValidateSubTypeThresholdNotMet();
			ValidateOrganisationCategory();
		}

		public ComplianceSubTypeAttributionRuleConfigurationCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (ComplianceSubTypeAttributionRuleConfigurationCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return new ComplianceSubTypeAttributionRuleConfigurationCollection();
				}
			}
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
					UpdateDocumentTitle();
				}
				if (!IsTaxRegistrationTypeSupported)
				{
					TaxRegistrationType = ZString.Empty;
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
			MandatoryValidation.CheckEntered(CountryInfo, (IMultilingualString)ResString.GetMultilingualString("e9fabe62-a358-44c2-b186-2f1649e8b67d", "Country/Region"));
			if (!CountryInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(CountryInfo, Lookups.CountryList);
			}
			ValidateConfigurationSet(CountryInfo);
		}

		ZString country;

		#endregion

		#region SubType

		[List("Lookups.SubTypeList")]
		[MaxLength(3)]
		public ZString SubType
		{
			get { return subType; }
			set
			{
				var isChanged = SetNonPersistentPropertyValue(SubTypeInfo, ref subType, value);
				if (!IsValidationSuspended)
				{
					ValidateSubType();
				}
				if (isChanged)
				{
					UpdateDocumentTitle();
				}
			}
		}

		public ZPropertyInfo SubTypeInfo
		{
			get { return GetZPropertyInfo(Schema.SubType); }
		}

		public void ValidateSubType()
		{
			SubTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(SubTypeInfo, (IMultilingualString)ResString.GetMultilingualString("8c711933-369d-4bbe-b8f0-f4f05f11f446", "Subtype"));
			if (!SubTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(SubTypeInfo, Lookups.SubTypeList);
			}
		}

		ZString subType;

		[BusinessObjectTestExclude]
		public ZString Description
		{
			get { return Lookups.SubTypeList.GetDescriptionFromCode(SubType); }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(CalculatedFieldNames.Description, x => SubTypeInfo); }
		}

		#endregion

		#region ParentTransactionSubtype

		[List("Lookups.SubTypeList")]
		[MaxLength(3)]
		public ZString ParentTransactionSubType
		{
			get { return parentTransactionSubType; }
			set
			{
				SetNonPersistentPropertyValue(ParentTransactionSubTypeInfo, ref parentTransactionSubType, value);
				if (!IsValidationSuspended)
				{
					ValidateParentTransactionComplianceSubType();
				}
			}
		}

		public ZPropertyInfo ParentTransactionSubTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ParentTransactionSubType); }
		}

		public void ValidateParentTransactionComplianceSubType()
		{
			ParentTransactionSubTypeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ParentTransactionSubTypeInfo, Lookups.SubTypeList);
			ValidateConfigurationSet(ParentTransactionSubTypeInfo);
		}

		ZString parentTransactionSubType;

		#endregion

		#region DocumentTitle

		[MaxLength(60)]
		public ZString DocumentTitle
		{
			get { return documentTitle; }
			set
			{
				SetNonPersistentPropertyValue(DocumentTitleInfo, ref documentTitle, value);
				if (!IsValidationSuspended)
				{
					ValidateDocumentTitle();
				}
			}
		}

		public ZPropertyInfo DocumentTitleInfo
		{
			get { return GetZPropertyInfo(Schema.DocumentTitle); }
		}

		public void ValidateDocumentTitle()
		{
			DocumentTitleInfo.ClearAllNotifications();
		}

		ZString documentTitle;

		void UpdateDocumentTitle()
		{
			DocumentTitle = AccComplianceSequence.GetDocumentTitle(SubType, Country);
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
				SetNonPersistentPropertyValue(LedgerTypeInfo, ref ledgerType, value);
				if (!IsValidationSuspended)
				{
					ValidateLedgerType();
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
			MandatoryValidation.CheckEntered(LedgerTypeInfo, (IMultilingualString)ResString.GetMultilingualString("feb9d7bd-604e-42ca-bd4c-816800a3a1e8", "Ledger Type"));
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
			MandatoryValidation.CheckEntered(InvoiceTypeInfo, (IMultilingualString)ResString.GetMultilingualString("04f14cf7-91b5-4fb3-8b80-c47b537a0601", "Invoice Type"));
			if (!InvoiceTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(InvoiceTypeInfo, Lookups.InvoiceTypeList);
			}
			ValidateConfigurationSet(InvoiceTypeInfo);
		}

		ZString invoiceType;

		#endregion

		#region OrganisationLocation

		[List("Lookups.OrganisationLocationList")]
		[MaxLength(3)]
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
			ListValidation.ErrorIfInvalidCode(OrganisationLocationInfo, Lookups.OrganisationLocationList);
			ValidateConfigurationSet(OrganisationLocationInfo);
		}

		ZString organisationLocation;

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
			get { return !IsTaxRegistrationTypeSupported; }
		}

		public virtual ZPropertyInfo TaxRegistrationTypeInfo
		{
			get { return GetZPropertyInfo(Schema.TaxRegistrationType); }
		}

		public void ValidateTaxRegistrationType()
		{
			TaxRegistrationTypeInfo.ClearAllNotifications();

			if (IsTaxRegistrationTypeSupported)
			{
				if (IsTaxRegistrationTypeMandatory)
				{
					MandatoryValidation.CheckEntered(TaxRegistrationTypeInfo, (IMultilingualString)ResString.GetMultilingualString("8012799d-81d0-4dcc-a489-7215a745352c", "Tax Registration Type"));
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

				if (!IsSpecificTaxIDsTaxInvoiceRule)
				{
					TaxIDCode = ZString.Empty;
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
			MandatoryValidation.CheckEntered(TaxInvoiceRuleInfo, (IMultilingualString)ResString.GetMultilingualString("13bb431e-5f4d-417f-9967-4029c4b1cc51", "Tax Invoice Rule"));
			if (!TaxInvoiceRuleInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(TaxInvoiceRuleInfo, Lookups.TaxInvoiceRuleList);
			}
			ValidateConfigurationSet(TaxInvoiceRuleInfo);
		}

		ZString taxInvoiceRule;

		#endregion

		#region TaxIDCode

		public ZString TaxIDCode
		{
			get { return taxIDCode; }
			set
			{
				SetNonPersistentPropertyValue(TaxIDCodeInfo, ref taxIDCode, value);

				if (!IsValidationSuspended)
				{
					ValidateTaxIDCode();
				}
			}
		}
		ZString taxIDCode;

		public IReadOnlyCollection<ZString> SeparatedTaxIDCodes => TaxIDCode.Split(",")?.Select(s => s.Trim())?.ToArray();

		[ReadOnlyMember(nameof(TaxIDCodeIsReadOnly))]
		public virtual ZPropertyInfo TaxIDCodeInfo
		{
			get { return GetZPropertyInfo(Schema.TaxIDCode); }
		}

		bool TaxIDCodeIsReadOnly => !IsSpecificTaxIDsTaxInvoiceRule;

		bool IsSpecificTaxIDsTaxInvoiceRule => TaxInvoiceRule == TaxInvoiceRuleCodes.SpecificTaxIDs;

		public void ValidateTaxIDCode()
		{
			TaxIDCodeInfo.ClearAllNotifications();
			if (IsSpecificTaxIDsTaxInvoiceRule)
			{
				MandatoryValidation.CheckEntered(TaxIDCodeInfo, (IMultilingualString)ResString.GetMultilingualString("818f94ab-916f-4a3a-8787-2f550a3dba12", "Tax ID Code"));

				if (!TaxIDCodeInfo.HasErrors())
				{
					bool isInvalidTaxIDCodes = SeparatedTaxIDCodes == null;
					if (!isInvalidTaxIDCodes)
					{
						var taxIDs = CurrentFactory.Load<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, Country))?.Select(x => x.AT_Code);
						if (taxIDs != null && SeparatedTaxIDCodes.Any(x => !taxIDs.Contains(x)))
						{
							isInvalidTaxIDCodes = true;
						}
					}
					if (isInvalidTaxIDCodes)
					{
						TaxIDCodeInfo.AddError(ResString.GetMultilingualString("0ffcf739-66e7-474d-9013-033155dffba6", "Invalid Tax ID : {0}", TaxIDCode));
					}
				}
				ValidateConfigurationSet(TaxIDCodeInfo);
			}
		}

		bool TaxIDCodeOverlap(ComplianceSubTypeAttributionRuleConfiguration config) => SeparatedTaxIDCodes.Intersect(config.SeparatedTaxIDCodes).Any();

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
			MandatoryValidation.CheckEntered(OriginalRuleInfo, (IMultilingualString)ResString.GetMultilingualString("32b6dbb7-3017-4d24-91b0-139dbf315874", "Original Rule"));
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
			MandatoryValidation.CheckEntered(DisbursementRuleInfo, (IMultilingualString)ResString.GetMultilingualString("adc5e239-2074-4487-97dd-c209e1f578c7", "Disbursement Rule"));
			if (!DisbursementRuleInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(DisbursementRuleInfo, Lookups.DisbursementRuleList);
			}
			ValidateConfigurationSet(DisbursementRuleInfo);
		}

		ZString disbursementRule;

		#endregion

		#region Exporter Exemption

		[List("Lookups.ExporterExemptionList")]
		[MaxLength(3)]
		public ZString ExporterExemption
		{
			get { return exporterExemption; }
			set
			{
				CheckMaximumLength(ExporterExemptionInfo, value);
				SetNonPersistentPropertyValue(ExporterExemptionInfo, ref exporterExemption, value);
				if (!IsValidationSuspended)
				{
					ValidateExporterExemption();
				}
			}
		}

		public virtual ZPropertyInfo ExporterExemptionInfo => GetZPropertyInfo(Schema.ExporterExemption);

		public void ValidateExporterExemption()
		{
			ExporterExemptionInfo.ClearAllNotifications();
			if (!ExporterExemptionInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(ExporterExemptionInfo, Lookups.ExporterExemptionList);
			}

			ValidateConfigurationSet(ExporterExemptionInfo);
		}

		ZString exporterExemption;

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
			ValidateConfigurationSet(SelfBillingRuleInfo);
		}

		ZString selfBillingRule;

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
			ValidateConfigurationSet(TaxRegistrationLocationRuleInfo);
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
			get { return GetZPropertyInfo(Schema.VatGroupRule); }
		}

		public void ValidateVatGroupRule()
		{
			VATGroupRuleInfo.ClearAllNotifications();
			if (!VATGroupRuleInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(VATGroupRuleInfo, Lookups.VATGroupList);
			}
			ValidateConfigurationSet(VATGroupRuleInfo);
		}

		#endregion

		#region RuleSetCode

		[ReadOnly(true)]
		public ZString RuleSetCode
		{
			get
			{
				return ruleSetCode;
			}
			set
			{
				SetNonPersistentPropertyValue(RuleSetCodeInfo, ref ruleSetCode, value);
			}
		}

		public ZPropertyInfo RuleSetCodeInfo
		{
			get { return GetZPropertyInfo(Schema.RuleSetCode); }
		}

		ZString ruleSetCode;
		#endregion

		#region RuleSetDescription

		[ReadOnly(true)]
		public ZString RuleSetDescription
		{
			get
			{
				return ruleSetDescription;
			}
			set
			{
				SetNonPersistentPropertyValue(RuleSetDescriptionInfo, ref ruleSetDescription, value);
			}
		}
		ZString ruleSetDescription;

		public ZPropertyInfo RuleSetDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.RuleSetDescription); }
		}

		#endregion

		#region RequiredTaxSystem

		[List("Lookups.TaxSystemList")]
		public ZString RequiredTaxSystem
		{
			get { return requiredTaxSystem; }
			set
			{
				SetNonPersistentPropertyValue(RequiredTaxSystemInfo, ref requiredTaxSystem, value);

				if (!IsValidationSuspended)
				{
					ValidateRequiredTaxSystem();
				}
			}
		}
		ZString requiredTaxSystem;

		public virtual ZPropertyInfo RequiredTaxSystemInfo
		{
			get { return GetZPropertyInfo(Schema.RequiredTaxSystem); }
		}

		public void ValidateRequiredTaxSystem()
		{
			RequiredTaxSystemInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(RequiredTaxSystemInfo);
			if (!RequiredTaxSystemInfo.HasErrors()
				&& !RequiredTaxSystem.IsEmpty
				&& RequiredTaxSystem.EqualsIgnoringCase(ExcludedTaxSystem))
			{
				RequiredTaxSystemInfo.AddError(RequireAndExcludeSameTaxSystemError);
			}
			ValidateConfigurationSet(RequiredTaxSystemInfo);
		}

		string RequireAndExcludeSameTaxSystemError => Res.GetString("2367fcd4-89c2-4a92-8948-71c64b8fb89c", "Cannot require and exclude same Tax System");

		#endregion

		#region ExcludedTaxSystem

		[List("Lookups.TaxSystemList")]
		public ZString ExcludedTaxSystem
		{
			get { return excludedTaxSystem; }
			set
			{
				SetNonPersistentPropertyValue(ExcludedTaxSystemInfo, ref excludedTaxSystem, value);

				if (!IsValidationSuspended)
				{
					ValidateExcludedTaxSystem();
				}
			}
		}
		ZString excludedTaxSystem;

		public virtual ZPropertyInfo ExcludedTaxSystemInfo
		{
			get { return GetZPropertyInfo(Schema.ExcludedTaxSystem); }
		}

		public void ValidateExcludedTaxSystem()
		{
			ExcludedTaxSystemInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ExcludedTaxSystemInfo);
			if (!ExcludedTaxSystemInfo.HasErrors()
				&& !ExcludedTaxSystem.IsEmpty
				&& ExcludedTaxSystem.EqualsIgnoringCase(RequiredTaxSystem))
			{
				ExcludedTaxSystemInfo.AddError(RequireAndExcludeSameTaxSystemError);
			}
			ValidateConfigurationSet(ExcludedTaxSystemInfo);
		}

		#endregion

		#region RequiredRegistrationCode

		[List("Lookups.RegistrationCodeList")]
		public ZString RequiredRegistrationCode
		{
			get { return requiredRegistrationCode; }
			set
			{
				SetNonPersistentPropertyValue(RequiredRegistrationCodeInfo, ref requiredRegistrationCode, value);

				if (!IsValidationSuspended)
				{
					ValidateRequiredRegistrationCode();
				}
			}
		}
		ZString requiredRegistrationCode;

		public virtual ZPropertyInfo RequiredRegistrationCodeInfo
		{
			get { return GetZPropertyInfo(Schema.RequiredRegistrationCode); }
		}

		public void ValidateRequiredRegistrationCode()
		{
			RequiredRegistrationCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(RequiredRegistrationCodeInfo);
			if (!RequiredRegistrationCodeInfo.HasErrors()
				&& !RequiredRegistrationCode.IsEmpty
				&& RequiredRegistrationCode.EqualsIgnoringCase(ExcludedRegistrationCode))
			{
				RequiredRegistrationCodeInfo.AddError(RequireAndExcludeSameRequiredRegistrationError);
			}
			ValidateConfigurationSet(RequiredRegistrationCodeInfo);
		}

		string RequireAndExcludeSameRequiredRegistrationError => Res.GetString("D9EEA63C-AA60-4062-993B-C14331CC5067", "The same code cannot be selected for {0} and {1}", "RequiredRegistrationCode", "ExcludedRegistrationCode");

		#endregion

		#region ExcludedRegistration

		[List("Lookups.RegistrationCodeList")]
		public ZString ExcludedRegistrationCode
		{
			get { return excludedRegistrationCode; }
			set
			{
				SetNonPersistentPropertyValue(ExcludedRegistrationCodeInfo, ref excludedRegistrationCode, value);

				if (!IsValidationSuspended)
				{
					ValidateExcludedRegistrationCode();
				}
			}
		}

		ZString excludedRegistrationCode;

		public virtual ZPropertyInfo ExcludedRegistrationCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ExcludedRegistrationCode); }
		}

		public void ValidateExcludedRegistrationCode()
		{
			ExcludedRegistrationCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ExcludedRegistrationCodeInfo);
			if (!ExcludedRegistrationCodeInfo.HasErrors()
				&& !ExcludedRegistrationCode.IsEmpty
				&& ExcludedRegistrationCode.EqualsIgnoringCase(RequiredRegistrationCode))
			{
				ExcludedRegistrationCodeInfo.AddError(RequireAndExcludeSameRequiredRegistrationError);
			}
			ValidateConfigurationSet(ExcludedRegistrationCodeInfo);
		}

		#endregion

		#region ThresholdApplies

		public ZBool ThresholdApplies
		{
			get { return thresholdApplies; }
			set
			{
				SetNonPersistentPropertyValue(ThresholdAppliesInfo, ref thresholdApplies, value);

				if (!ThresholdApplies)
				{
					SubTypeThresholdNotMet = ZString.Empty;
				}
			}
		}
		ZBool thresholdApplies;

		public virtual ZPropertyInfo ThresholdAppliesInfo
		{
			get { return GetZPropertyInfo(Schema.ThresholdApplies); }
		}

		#endregion

		#region SubTypeThresholdNotMet

		[List("Lookups.SubTypeList")]
		[ReadOnlyMember(nameof(SubTypeThresholdNotMetIsReadOnly))]
		public ZString SubTypeThresholdNotMet
		{
			get { return subTypeThresholdNotMet; }
			set
			{
				SetNonPersistentPropertyValue(SubTypeThresholdNotMetInfo, ref subTypeThresholdNotMet, value);

				if (!IsValidationSuspended)
				{
					ValidateSubTypeThresholdNotMet();
				}
			}
		}
		ZString subTypeThresholdNotMet;

		bool SubTypeThresholdNotMetIsReadOnly
		{
			get { return !ThresholdApplies; }
		}

		public virtual ZPropertyInfo SubTypeThresholdNotMetInfo
		{
			get { return GetZPropertyInfo(Schema.SubTypeThresholdNotMet); }
		}

		public void ValidateSubTypeThresholdNotMet()
		{
			SubTypeThresholdNotMetInfo.ClearAllNotifications();
			if (ThresholdApplies)
			{
				MandatoryValidation.CheckEntered(SubTypeThresholdNotMetInfo);
				if (!SubTypeThresholdNotMetInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidCode(SubTypeThresholdNotMetInfo);
					if (SubTypeThresholdNotMet.EqualsIgnoringCase(SubType))
					{
						SubTypeThresholdNotMetInfo.AddError(RequireSubTypeThresholdNotMet);
					}
				}
			}
		}

		string RequireSubTypeThresholdNotMet => Res.GetString("C0EF9700-AC55-4DE6-A97B-ECAC07B89A87", "The same code cannot be selected for {0} and {1}", nameof(SubType), nameof(SubTypeThresholdNotMet));

		#endregion

		ZBool CheckIdenticalConfigurationExists()
		{
			ZBool result = ZBool.False;
			if (ParentCollections.Count > 0)
			{
				foreach (ComplianceSubTypeAttributionRuleConfiguration item in ParentCollection)
				{
					if (PK != item.PK
						&& Country == item.Country
						&& LedgerType == item.LedgerType
						&& InvoiceType == item.InvoiceType
						&& TaxInvoiceRule == item.TaxInvoiceRule
						&& TaxIDCode == item.TaxIDCode
						&& DisbursementRule == item.DisbursementRule
						&& OriginalRule == item.OriginalRule
						&& OrganisationLocation == item.OrganisationLocation
						&& (!IsTaxRegistrationTypeSupported || TaxRegistrationType == item.TaxRegistrationType)
						&& SelfBillingRule == item.SelfBillingRule
						&& TaxRegistrationLocationRule == item.taxRegistrationLocationRule
						&& VATGroupRule == item.VATGroupRule
						&& ParentTransactionSubType == item.ParentTransactionSubType
						&& ExporterExemption == item.ExporterExemption
						&& RequiredTaxSystem == item.RequiredTaxSystem
						&& ExcludedTaxSystem == item.ExcludedTaxSystem
						&& RequiredRegistrationCode == item.RequiredRegistrationCode
						&& ExcludedRegistrationCode == item.ExcludedRegistrationCode
						&& OrganisationCategory == item.OrganisationCategory
						)
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
			bool blankOrEqual(string s1, string s2) => string.IsNullOrWhiteSpace(s1) || string.IsNullOrWhiteSpace(s2) || s1 == s2;

			ZBool result = ZBool.False;
			if (ParentCollections.Count > 0)
			{
				foreach (ComplianceSubTypeAttributionRuleConfiguration item in ParentCollection)
				{
					if (PK != item.PK
						&& Country == item.Country
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
						bool overlapExporterExemptionDetected = false;
						bool overlapTaxRegistrationLocationRule = false;
						bool overlapVATGroupMemberRule = false;
						bool overlapParentTransactionSubTypeMemberRule = false;
						bool overlapSpecifiedTaxIDCodesDetected = false;
						bool overlapSpecifiedRequiredTaxSystemDetected = false;
						bool overlapSpecifiedExcludedTaxSystemDetected = false;
						bool overlapSpecifiedRequiredRegistrationDetected = false;
						bool overlapSpecifiedExcludedRegistrationDetected = false;

						overlapDisbursementRuleDetected = DisbursementRule == item.DisbursementRule ||
							DisbursementRule == DisbursementRuleCodes.AllTransactions ||
							item.DisbursementRule == DisbursementRuleCodes.AllTransactions;

						overlapOriginalRuleDetected = OriginalRule == item.OriginalRule ||
							OriginalRule == OriginalRuleCodes.AllTransactions ||
							item.OriginalRule == OriginalRuleCodes.AllTransactions;

						overlapTaxInvoiceRuleDetected = TaxInvoiceRule == item.TaxInvoiceRule;

						overlapTaxRegistrationTypeDetected = !IsTaxRegistrationTypeSupported || TaxRegistrationType == item.TaxRegistrationType;

						overlapSelfBillingRuleDetected = SelfBillingRule == item.SelfBillingRule;

						overlapExporterExemptionDetected = blankOrEqual(ExporterExemption, item.ExporterExemption);

						overlapTaxRegistrationLocationRule = TaxRegistrationLocationRule == item.TaxRegistrationLocationRule;

						overlapVATGroupMemberRule = VATGroupRule == item.VATGroupRule;

						overlapParentTransactionSubTypeMemberRule = ParentTransactionSubType == item.ParentTransactionSubType;

						overlapSpecifiedTaxIDCodesDetected = TaxIDCodeOverlap(item);

						overlapSpecifiedRequiredTaxSystemDetected = RequiredTaxSystem.EqualsIgnoringCase(item.RequiredTaxSystem);

						overlapSpecifiedExcludedTaxSystemDetected = ExcludedTaxSystem.EqualsIgnoringCase(item.ExcludedTaxSystem);

						overlapSpecifiedRequiredRegistrationDetected = RequiredRegistrationCode.EqualsIgnoringCase(item.RequiredRegistrationCode);

						overlapSpecifiedExcludedRegistrationDetected = ExcludedRegistrationCode.EqualsIgnoringCase(item.ExcludedRegistrationCode);

						if (overlapDisbursementRuleDetected &&
							overlapOriginalRuleDetected &&
							overlapTaxInvoiceRuleDetected &&
							overlapTaxRegistrationTypeDetected &&
							overlapSelfBillingRuleDetected &&
							overlapExporterExemptionDetected &&
							overlapTaxRegistrationLocationRule &&
							overlapVATGroupMemberRule &&
							overlapParentTransactionSubTypeMemberRule &&
							overlapSpecifiedTaxIDCodesDetected &&
							overlapSpecifiedRequiredTaxSystemDetected &&
							overlapSpecifiedExcludedTaxSystemDetected &&
							overlapSpecifiedRequiredRegistrationDetected &&
							overlapSpecifiedExcludedRegistrationDetected)
						{
							result = ZBool.True;
							break;
						}
					}
				}
			}
			return result;
		}

		void ValidateConfigurationSet(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.HasErrors() && CheckIdenticalConfigurationExists())
			{
				propertyInfo.AddError(IdenticalConfigurationExists);
			}
			if (!propertyInfo.HasErrors() && CheckOverlappedConfigurationExists())
			{
				propertyInfo.AddError(OverlappedConfigurationExists);
			}
		}

#if DEBUG
		public
#endif
		ZBool IsTaxRegistrationTypeSupported
		{
			get
			{
				return Country == Core.Constants.CountryCodes.Argentina
					|| Country == Core.Constants.CountryCodes.Peru
					|| Country == Core.Constants.CountryCodes.ElSalvador
					|| Country == Core.Constants.CountryCodes.China
					|| country == Core.Constants.CountryCodes.Turkey
					|| country == Core.Constants.CountryCodes.SriLanka
					|| country == Core.Constants.CountryCodes.DominicanRepublic;
			}
		}

		ZBool IsTaxRegistrationTypeMandatory
		{
			get
			{
				return Country == Core.Constants.CountryCodes.Argentina || Country == Core.Constants.CountryCodes.China;
			}
		}

		#endregion

		#region ComplianceSubTypeAttributionRuleConfigurationLookups

		public ComplianceSubTypeAttributionRuleConfigurationLookups Lookups
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

		protected virtual ComplianceSubTypeAttributionRuleConfigurationLookups GetNewLookups()
		{
			return new ComplianceSubTypeAttributionRuleConfigurationLookups(this);
		}

		ComplianceSubTypeAttributionRuleConfigurationLookups fLookups;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Country, Country);
			writer.WriteElementString(Schema.SubType, SubType);
			writer.WriteElementString(Schema.DocumentTitle, DocumentTitle);
			writer.WriteElementString(Schema.LedgerType, LedgerType);
			writer.WriteElementString(Schema.InvoiceType, InvoiceType);
			writer.WriteElementString(Schema.TaxInvoiceRule, TaxInvoiceRule.ToString());
			writer.WriteElementString(Schema.TaxIDCode, TaxIDCode);
			writer.WriteElementString(Schema.DisbursementRule, DisbursementRule.ToString());
			writer.WriteElementString(Schema.OriginalRule, OriginalRule.ToString());
			writer.WriteElementString(Schema.OrganisationLocation, OrganisationLocation);
			writer.WriteElementString(Schema.TaxRegistrationType, TaxRegistrationType.ToString());
			writer.WriteElementString(Schema.SelfBillingRule, SelfBillingRule.ToString());
			writer.WriteElementString(Schema.TaxRegistrationLocationRule, TaxRegistrationLocationRule.ToString());
			writer.WriteElementString(Schema.VatGroupRule, VATGroupRule.ToString());
			writer.WriteElementString(Schema.ExporterExemption, ExporterExemption.ToString());

			XElement newElementsNode = new XElement(Schema.NewElementsXMLNode);
			newElementsNode.Add(ParentTransactionSubType.IsEmpty ? null : new XElement(Schema.ParentTransactionSubType, ParentTransactionSubType.ToString()));
			newElementsNode.Add(RequiredTaxSystem.IsEmpty ? null : new XElement(Schema.RequiredTaxSystem, RequiredTaxSystem.ToString()));
			newElementsNode.Add(ExcludedTaxSystem.IsEmpty ? null : new XElement(Schema.ExcludedTaxSystem, ExcludedTaxSystem.ToString()));
			newElementsNode.Add(RequiredRegistrationCode.IsEmpty ? null : new XElement(Schema.RequiredRegistrationCode, RequiredRegistrationCode.ToString()));
			newElementsNode.Add(ExcludedRegistrationCode.IsEmpty ? null : new XElement(Schema.ExcludedRegistrationCode, ExcludedRegistrationCode.ToString()));
			newElementsNode.Add(ThresholdApplies.IsEmpty ? null : new XElement(Schema.ThresholdApplies, ThresholdApplies.ToString()));
			newElementsNode.Add(SubTypeThresholdNotMet.IsEmpty ? null : new XElement(Schema.SubTypeThresholdNotMet, SubTypeThresholdNotMet.ToString()));
			newElementsNode.Add(OrganisationCategory.IsEmpty ? null : new XElement(Schema.OrganisationCategory, OrganisationCategory.ToString()));

			if (newElementsNode.HasElements)
			{
				newElementsNode.WriteTo(writer);
			}
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Country = reader.ReadElementString(Schema.Country);
			SubType = reader.ReadElementString(Schema.SubType);
			DocumentTitle = reader.ReadElementString(Schema.DocumentTitle);
			LedgerType = reader.ReadElementString(Schema.LedgerType);
			InvoiceType = reader.ReadElementString(Schema.InvoiceType);
			TaxInvoiceRule = reader.ReadElementString(Schema.TaxInvoiceRule);
			TaxIDCode = reader.ReadElementString(Schema.TaxIDCode);
			DisbursementRule = reader.ReadElementString(Schema.DisbursementRule);
			OriginalRule = reader.ReadElementString(Schema.OriginalRule);
			OrganisationLocation = reader.ReadElementString(Schema.OrganisationLocation);
			TaxRegistrationType = reader.ReadElementString(Schema.TaxRegistrationType);
			SelfBillingRule = reader.ReadElementString(Schema.SelfBillingRule);
			TaxRegistrationLocationRule = reader.ReadElementString(Schema.TaxRegistrationLocationRule);
			VATGroupRule = reader.ReadElementString(Schema.VatGroupRule);
			ExporterExemption = reader.ReadElementString(Schema.ExporterExemption);

			var newElement = reader.Reader.Name != Schema.NewElementsXMLNode ? null : XNode.ReadFrom(reader) as XElement;
			SetValidValueHelper.SetStringIfValid(newElement?.GetNodeValue(Schema.ParentTransactionSubType), x => ParentTransactionSubType = x);
			SetValidValueHelper.SetStringIfValid(newElement?.GetNodeValue(Schema.RequiredTaxSystem), x => RequiredTaxSystem = x);
			SetValidValueHelper.SetStringIfValid(newElement?.GetNodeValue(Schema.ExcludedTaxSystem), x => ExcludedTaxSystem = x);
			SetValidValueHelper.SetStringIfValid(newElement?.GetNodeValue(Schema.RequiredRegistrationCode), x => RequiredRegistrationCode = x);
			SetValidValueHelper.SetStringIfValid(newElement?.GetNodeValue(Schema.ExcludedRegistrationCode), x => ExcludedRegistrationCode = x);
			SetValidValueHelper.SetBoolIfValid(newElement?.GetNodeValue(Schema.ThresholdApplies), x => ThresholdApplies = x);
			SetValidValueHelper.SetStringIfValid(newElement?.GetNodeValue(Schema.SubTypeThresholdNotMet), x => SubTypeThresholdNotMet = x);
			SetValidValueHelper.SetStringIfValid(newElement?.GetNodeValue(Schema.OrganisationCategory), x => OrganisationCategory = x);
		}
		#endregion
	}
}
