using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business
{
	public class OrganisationTaxRateFileImportLine : NonPersistentBusinessObject
	{
		public OrganisationTaxRateFileImportLine(BusinessObjectFactory factory)
			: base(factory)
		{
			TaxRate = Factory.New<AccOrgTaxRate>();
		}
		AccOrgTaxRate TaxRate { get; }

		protected override void RunPreSaveValidationCore()
		{
			TaxRate.RunPreSaveValidation();
		}

		[ReadOnly(true)]
		[ResourceStringData("69509F05-B0CB-4065-9303-8F9999A154A7", Caption = "Organization Code", ShortCaption = "Org. Code")]
		public ZString OrganizationCode
		{
			get => organizationCode;
			set
			{
				organizationCode = value;
				OrganizationCodeInfo.RefreshBinding();
			}
		}
		ZString organizationCode;
		public ZPropertyInfo OrganizationCodeInfo => GetZPropertyInfo(nameof(OrganizationCode));

		[ReadOnly(true)]
		[ResourceStringData("DAEF10A9-CECE-4932-A2E7-7F85F2A8B5CA", Caption = "Organization Name", ShortCaption = "Org. Name")]
		public ZString OrganizationName
		{
			get => organizationName;
			set
			{
				organizationName = value;
				OrganizationNameInfo.RefreshBinding();
			}
		}
		ZString organizationName;
		public ZPropertyInfo OrganizationNameInfo => GetZPropertyInfo(nameof(OrganizationName));

		[ReadOnly(true)]
		[ResourceStringData("D9050DFE-AE9E-41D3-9FE5-586B79DBC243", Caption = "Registration Code", ShortCaption = "Reg. Code")]
		public ZString RegistrationCode
		{
			get => registrationCode;
			set => SetNonPersistentPropertyValue(RegistrationCodeInfo, ref registrationCode, value);
		}
		ZString registrationCode;
		public ZPropertyInfo RegistrationCodeInfo => GetZPropertyInfo(nameof(RegistrationCode));

		[ReadOnly(true)]
		[ResourceStringData("D9ACAA7D-AEBA-4984-A8B1-05ED00074B6B", Caption = "Rate Source")]
		public ZString RateSource
		{
			get => TaxRate.OTR_Source;
			set => TaxRate.OTR_Source = value;
		}
		public ZPropertyInfo RateSourceInfo => GetWrappedZPropertyInfo(nameof(RateSource), _ => TaxRate.OTR_SourceInfo);

		[ReadOnly(true)]
		[ResourceStringData("99FE9660-865D-48C1-9068-0E875EB40A0F", Caption = "Start Date")]
		public ZDate StartDate
		{
			get => TaxRate.OTR_StartDate;
			set => TaxRate.OTR_StartDate = value;
		}
		public ZPropertyInfo StartDateInfo => GetWrappedZPropertyInfo(nameof(StartDate), _ => TaxRate.OTR_StartDateInfo);

		[ReadOnly(true)]
		[ResourceStringData("0338DA2F-08A7-468D-A9A5-0E65938E8FC2", Caption = "End Date")]
		public ZDate EndDate
		{
			get => TaxRate.OTR_EndDate;
			set => TaxRate.OTR_EndDate = value;
		}
		public ZPropertyInfo EndDateInfo => GetWrappedZPropertyInfo(nameof(EndDate), _ => TaxRate.OTR_EndDateInfo);

		[ReadOnly(true)]
		[ResourceStringData("BD53E55D-89C8-4385-A7E2-5F3E81210BC0", Caption = "Rate Numerator", ShortCaption = "Rate Num.")]
		public ZInt RateNumerator
		{
			get => TaxRate.OTR_RateNumerator;
			set => TaxRate.OTR_RateNumerator = value;
		}
		public ZPropertyInfo RateNumeratorInfo => GetWrappedZPropertyInfo(nameof(RateNumerator), _ => TaxRate.OTR_RateNumeratorInfo);

		[ReadOnly(true)]
		[ResourceStringData("F7206E01-A562-4158-9904-B43B903BF744", Caption = "Rate Denominator", ShortCaption = "Rate Den.")]
		public ZInt RateDenominator
		{
			get => TaxRate.OTR_RateDenominator;
			set => TaxRate.OTR_RateDenominator = value;
		}
		public ZPropertyInfo RateDenominatorInfo => GetWrappedZPropertyInfo(nameof(RateDenominator), _ => TaxRate.OTR_RateDenominatorInfo);

		[ReadOnly(true)]
		public ZGuid TaxConfigurationPK
		{
			get => TaxRate.OTR_OTC;
			set => TaxRate.OTR_OTC = value;
		}
		public ZPropertyInfo TaxConfigurationPKInfo => GetWrappedZPropertyInfo(nameof(TaxConfigurationPK), _ => TaxRate.OTR_OTCInfo);

		public ZBool IsOrgTaxRateInDb => TaxRate.IsInDatabase;
	}
}
