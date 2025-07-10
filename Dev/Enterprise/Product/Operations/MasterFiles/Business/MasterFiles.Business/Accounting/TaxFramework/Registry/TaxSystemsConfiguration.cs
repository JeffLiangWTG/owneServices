using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class TaxSystemsConfiguration : RegistryBusinessObjectTemplate
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TaxSystemsConfiguration();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(nameof(Code), Code);
			writer.WriteElementString(nameof(Name), Name);
			writer.WriteElementString(nameof(Country), Country);
			writer.WriteElementString(nameof(TaxAuthorityType), TaxAuthorityType);
			writer.WriteElementString(nameof(TaxSuperType), TaxSuperType);
			writer.WriteElementString(nameof(RegistrationLevel), RegistrationLevel);
			writer.WriteElementString(nameof(IncludeInInvoceTotal), IncludeInInvoceTotal.ToString());
			writer.WriteElementString(nameof(AdjustmentSign), AdjustmentSign);
			writer.WriteElementString(nameof(TaxBaseCalculationMethod), TaxBaseCalculationMethod);
			writer.WriteElementString(nameof(TaxAmountCalculationMethod), TaxAmountCalculationMethod);
			writer.WriteElementString(nameof(ThresholdRule), ThresholdRule);
			writer.WriteElementString(nameof(TaxRateSource), TaxRateSource);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(nameof(Code));
			Name = reader.ReadElementString(nameof(Name));
			Country = reader.ReadElementString(nameof(Country));
			TaxAuthorityType = reader.ReadElementString(nameof(TaxAuthorityType));
			TaxSuperType = reader.ReadElementString(nameof(TaxSuperType));
			RegistrationLevel = reader.ReadElementString(nameof(RegistrationLevel));
			IncludeInInvoceTotal = reader.ReadElementStringAsZBool(nameof(IncludeInInvoceTotal));
			AdjustmentSign = reader.ReadElementString(nameof(AdjustmentSign));
			TaxBaseCalculationMethod = reader.ReadElementString(nameof(TaxBaseCalculationMethod));
			TaxAmountCalculationMethod = reader.ReadElementString(nameof(TaxAmountCalculationMethod));
			ThresholdRule = reader.ReadElementString(nameof(ThresholdRule));
			TaxRateSource = reader.ReadElementString(nameof(TaxRateSource));
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCode();
			ValidateCountry();
			ValidateTaxAuthorityType();
			ValidateTaxSuperType();
			ValidateRegistrationLevel();
			ValidateAdjustmentSign();
			ValidateTaxBaseCalculationMethod();
			ValidateTaxAmountCalculationMethod();
			ValidateThresholdRule();
			ValidateTaxRateSource();
		}

		#region Country
		[List(nameof(CountryList))]
		[MaxLength(2)]
		[ResourceStringData("C5149302-DE85-412F-A383-24990848B749", Caption = "Country/Region")]
		public ZString Country
		{
			get
			{
				return country;
			}
			set
			{
				SetNonPersistentPropertyValue(CountryInfo, ref country, value);
			}
		}

		public ZPropertyInfo CountryInfo
		{
			get { return GetZPropertyInfo(nameof(Country)); }
		}

		public void ValidateCountry()
		{
			CountryInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CountryInfo);
			if (!CountryInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(CountryInfo);
			}
		}

		ZString country;

		public RefCountryCollection CountryList
		{
			get
			{
				if (countryList == null)
				{
					countryList = new RefCountryCollection(CurrentFactory);
				}
				return countryList;
			}
		}

		public RefCountryCollection countryList;
		#endregion

		#region Code
		[MaxLength(10)]
		[ResourceStringData("A1535B9F-7A85-492F-8BF8-0AB4D54745A3", Caption = "Code")]
		public ZString Code
		{
			get
			{
				return code;
			}
			set
			{
				SetNonPersistentPropertyValue(CodeInfo, ref code, value.ToUpperInvariant());
			}
		}
		ZString code;

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(nameof(Code)); }
		}

		public void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CodeInfo);
			if (!CodeInfo.HasErrors())
			{
				if (!Code.IsLettersAndNumbersOnlyOrEmpty)
				{
					CodeInfo.AddError(Res.GetString("051FD451-34E4-4702-AFF5-30574F1CC23A", "Please enter alphanumeric characters only."));
				}
				EnglishCharactersValidation.ErrorIfNotWesternEuropean(CodeInfo);
			}

			if (!CodeInfo.HasErrors())
			{
				if (ParentCollection != null)
				{
					var result = from TaxSystemsConfiguration taxConfiguration in ParentCollection
								 where taxConfiguration.Code == Code
								 select taxConfiguration;

					if (result.Count() > 1)
					{
						CodeInfo.AddError(Res.GetString("47D015DF-5F21-486E-86B1-998209FD8A15", "This Code is already in use. Please use a different Code."));
					}
				}
			}
		}
		#endregion

		#region Name
		[MaxLength(80)]
		[ResourceStringData("3AA01E49-71B5-47C2-BB4D-378BDDD884C9", Caption = "Name")]
		public ZString Name
		{
			get
			{
				return name;
			}
			set
			{
				SetNonPersistentPropertyValue(NameInfo, ref name, value);
			}
		}

		public ZPropertyInfo NameInfo
		{
			get { return GetZPropertyInfo(nameof(Name)); }
		}

		ZString name;
		#endregion

		#region Tax Super Type
		[List(nameof(TaxSuperTypeList))]
		[MaxLength(3)]
		[ResourceStringData("01B99C19-A56B-421D-A65F-97BD6B03889F", Caption = "Tax Super Type")]
		public ZString TaxSuperType
		{
			get
			{
				return taxSuperType;
			}
			set
			{
				SetNonPersistentPropertyValue(TaxSuperTypeInfo, ref taxSuperType, value);
			}
		}

		public ZPropertyInfo TaxSuperTypeInfo
		{
			get { return GetZPropertyInfo(nameof(TaxSuperType)); }
		}

		public void ValidateTaxSuperType()
		{
			TaxSuperTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TaxSuperTypeInfo);
			if (!TaxSuperTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(TaxSuperTypeInfo);
			}
		}

		ZString taxSuperType;

		public CodeDescriptionPairList TaxSuperTypeList => new AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList();
		#endregion

		#region Tax Authority Type
		[List(nameof(TaxAuthorityTypeList))]
		[MaxLength(3)]
		[ResourceStringData("8E91A6B2-1708-4C10-87C5-0F8CC53620C0", Caption = "Tax Authority Type")]
		public ZString TaxAuthorityType
		{
			get
			{
				return taxAuthorityType;
			}
			set
			{
				SetNonPersistentPropertyValue(TaxAuthorityTypeInfo, ref taxAuthorityType, value);
			}
		}

		public ZPropertyInfo TaxAuthorityTypeInfo
		{
			get { return GetZPropertyInfo(nameof(TaxAuthorityType)); }
		}

		public void ValidateTaxAuthorityType()
		{
			TaxAuthorityTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TaxAuthorityTypeInfo);
			if (!TaxAuthorityTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(TaxAuthorityTypeInfo);
			}
		}

		ZString taxAuthorityType;

		public CodeDescriptionPairList TaxAuthorityTypeList => new AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList();
		#endregion

		#region Registration Level
		[List(nameof(TaxSystemRegistrationLevels))]
		[MaxLength(3)]
		[ResourceStringData("399003D1-EBAD-47E2-84FC-4D6036BFA7A5", Caption = "Registration Level")]
		public ZString RegistrationLevel
		{
			get
			{
				return registrationLevel;
			}
			set
			{
				SetNonPersistentPropertyValue(RegistrationLevelInfo, ref registrationLevel, value);
			}
		}

		public ZPropertyInfo RegistrationLevelInfo
		{
			get { return GetZPropertyInfo(nameof(RegistrationLevel)); }
		}

		public void ValidateRegistrationLevel()
		{
			RegistrationLevelInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(RegistrationLevelInfo);
			if (!RegistrationLevelInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(RegistrationLevelInfo);
			}
		}

		ZString registrationLevel;

		public CodeDescriptionPairList TaxSystemRegistrationLevels => new AccountingMasterFilesTaxFrameworkConstants.TaxSystemRegistrationLevels();
		#endregion

		#region Include In Invoce Total
		[ResourceStringData("22CC296C-630A-4758-AD3A-A78D55F3FB42", Caption = "Include In Invoice Total")]
		public ZBool IncludeInInvoceTotal
		{
			get
			{
				return includeInInvoceTotal;
			}
			set
			{
				SetNonPersistentPropertyValue(IncludeInInvoceTotalInfo, ref includeInInvoceTotal, value);
			}
		}

		public ZPropertyInfo IncludeInInvoceTotalInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeInInvoceTotal)); }
		}

		ZBool includeInInvoceTotal;
		#endregion

		#region Adjustment Sign
		[List(nameof(TaxCalculationAdjustmentSigns))]
		[MaxLength(3)]
		[ResourceStringData("E25981FC-380F-473B-9CF3-BF88BF24FA5C", Caption = "Adjustment Sign")]
		public ZString AdjustmentSign
		{
			get
			{
				return adjustmentSign;
			}
			set
			{
				SetNonPersistentPropertyValue(AdjustmentSignInfo, ref adjustmentSign, value);
			}
		}

		public ZPropertyInfo AdjustmentSignInfo
		{
			get { return GetZPropertyInfo(nameof(AdjustmentSign)); }
		}

		public void ValidateAdjustmentSign()
		{
			AdjustmentSignInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AdjustmentSignInfo);
			if (!AdjustmentSignInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(AdjustmentSignInfo);
			}
		}

		ZString adjustmentSign;

		public CodeDescriptionPairList TaxCalculationAdjustmentSigns => new AccountingMasterFilesTaxFrameworkConstants.TaxCalculationAdjustmentSigns();
		#endregion

		#region Tax Base Calculation Method
		[List(nameof(TaxBaseCalculationMethods))]
		[MaxLength(3)]
		[ResourceStringData("633864C7-521A-48EB-AE6B-EE08F91CCA34", Caption = "Tax Base Calculation Method")]
		public ZString TaxBaseCalculationMethod
		{
			get
			{
				return taxBaseCalculationMethod;
			}
			set
			{
				SetNonPersistentPropertyValue(TaxBaseCalculationMethodInfo, ref taxBaseCalculationMethod, value);
			}
		}

		public ZPropertyInfo TaxBaseCalculationMethodInfo
		{
			get { return GetZPropertyInfo(nameof(TaxBaseCalculationMethod)); }
		}

		public void ValidateTaxBaseCalculationMethod()
		{
			TaxBaseCalculationMethodInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TaxBaseCalculationMethodInfo);
			if (!TaxBaseCalculationMethodInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(TaxBaseCalculationMethodInfo);
			}
		}

		ZString taxBaseCalculationMethod;

		public CodeDescriptionPairList TaxBaseCalculationMethods => new AccountingMasterFilesTaxFrameworkConstants.TaxBaseCalculationMethods();
		#endregion

		#region Tax Amount Calculation Method
		[List(nameof(TaxAmountCalculationMethods))]
		[MaxLength(3)]
		[ResourceStringData("63F27035-77DD-4645-B504-0A76B0323174", Caption = "Tax Amount Calculation Method")]
		public ZString TaxAmountCalculationMethod
		{
			get
			{
				return taxAmountCalculationMethod;
			}
			set
			{
				SetNonPersistentPropertyValue(TaxAmountCalculationMethodInfo, ref taxAmountCalculationMethod, value);
			}
		}

		public ZPropertyInfo TaxAmountCalculationMethodInfo
		{
			get { return GetZPropertyInfo(nameof(TaxAmountCalculationMethod)); }
		}

		public void ValidateTaxAmountCalculationMethod()
		{
			TaxAmountCalculationMethodInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TaxAmountCalculationMethodInfo);
			if (!TaxAmountCalculationMethodInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(TaxAmountCalculationMethodInfo);
			}
		}

		ZString taxAmountCalculationMethod;

		public CodeDescriptionPairList TaxAmountCalculationMethods => new AccountingMasterFilesTaxFrameworkConstants.TaxAmountCalculationMethods();
		#endregion

		#region Threshold Rules
		[List(nameof(ThresholdRules))]
		[MaxLength(3)]
		[ResourceStringData("1DF92746-5544-4614-B772-256C3BA81EDE", Caption = "Threshold Rule")]
		public ZString ThresholdRule
		{
			get
			{
				return thresholdRule;
			}
			set
			{
				SetNonPersistentPropertyValue(ThresholdRuleInfo, ref thresholdRule, value);
			}
		}

		public ZPropertyInfo ThresholdRuleInfo
		{
			get { return GetZPropertyInfo(nameof(ThresholdRule)); }
		}

		public void ValidateThresholdRule()
		{
			ThresholdRuleInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ThresholdRuleInfo);
			if (!ThresholdRuleInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(ThresholdRuleInfo);
			}
		}

		ZString thresholdRule;

		public CodeDescriptionPairList ThresholdRules => new AccountingMasterFilesTaxFrameworkConstants.ThresholdRulesForTaxCalculation();
		#endregion

		#region Tax Rate Source
		[List(nameof(TaxRateSources))]
		[MaxLength(3)]
		[ResourceStringData("832C95EC-ADF7-4C44-93A7-16F65B319DB8", Caption = "Tax Rate Source")]
		public ZString TaxRateSource
		{
			get
			{
				return taxRateSource;
			}
			set
			{
				SetNonPersistentPropertyValue(TaxRateSourceInfo, ref taxRateSource, value);
			}
		}

		public ZPropertyInfo TaxRateSourceInfo
		{
			get { return GetZPropertyInfo(nameof(TaxRateSource)); }
		}

		public void ValidateTaxRateSource()
		{
			TaxRateSourceInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TaxRateSourceInfo);
			if (!TaxRateSourceInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(TaxRateSourceInfo);
			}
		}

		ZString taxRateSource;

		public CodeDescriptionPairList TaxRateSources => new AccountingMasterFilesTaxFrameworkConstants.TaxRateSources();
		#endregion

		#region TEMPORARY additional properties that should be moved to DB or be removed in future when more countries or tax system codes use them
		//
		//DO NOT add more taxes without moving the property in db and make it non country or tax system code specific
		//

		public ZBool IsSingleRecordPerARTransactionRequired(ZString countryCode)
		{
			return Country == countryCode
				&& countryCode == Core.Constants.CountryCodes.Brazil
				&& Code == ISSCode;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Warning message string")]
		internal (ZString Separator, ZString WarningMessage)? GetServiceCodeSeparatorAndWarning(ZString countryCode)
		{
			if (Country == countryCode
				&& countryCode == Core.Constants.CountryCodes.Brazil
				&& Code == ISSCode)
			{
				return (" | ",
@"You have entered a separator (space + | + space). The system will split this value into two codes when a transaction that only contains ISS Tax is queued for E-Reporting.
The value before the separator will be treated as the City Code.
The value after the separator will be treated as the Federal Code.
When a transaction contains a PIS tax record the value after the separator will be ignored.");
			}

			return null;
		}

		const string ISSCode = "ISS";

		#endregion

		#region implementation

		TaxSystemsConfigurationCollection ParentCollection
		{
			get { return GetParentCollection(this, typeof(TaxSystemsConfigurationCollection)) as TaxSystemsConfigurationCollection; }
		}

		#endregion
	}
}
