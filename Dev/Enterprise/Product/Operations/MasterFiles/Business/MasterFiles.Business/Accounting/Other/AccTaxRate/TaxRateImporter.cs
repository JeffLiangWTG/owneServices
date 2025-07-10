using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business
{
	public class TaxRateImporter
	{
		public TaxRateImporter(ZString countryCode, ZGuid companyPK, BusinessObjectFactory factory)
		{
			this.countryCode = countryCode;
			this.companyPK = companyPK;
			this.factory = factory;
		}

		readonly ZString countryCode;
		readonly ZGuid companyPK;
		readonly BusinessObjectFactory factory;

		GlbCompany Company
		{
			get
			{
				if (company == null)
				{
					company = factory.Load<GlbCompany>(companyPK);
				}
				return company;
			}
		}
		GlbCompany company;

		bool ShouldImportExtraTaxForSpain(ZString extraType) =>
			countryCode != Enterprise.Core.Constants.CountryCodes.Spain ||
			(
				extraType != AccTaxRate.ExtraTypes.RegionalTax ||
				Company?.Branches.FirstOrDefault(x => x.HomePort?.CountryStates != null &&
					(x.HomePort.CountryStates.RW_Code == "TF" || x.HomePort.CountryStates.RW_Code == "GC")) != null
			);

		public void ImportFromXMLFile()
		{
			var xmlParser = new TaxRateXmlParser();
			var taxRatesForcountries = xmlParser.BuildTaxRatesDictionaryBasedOnCountry();
			if (taxRatesForcountries.ContainsKey(countryCode))
			{
				var existingTaxRates = new AccTaxRateCollection(factory, countryCode);
				existingTaxRates.Load();
				var existingTaxRatesByCode = existingTaxRates.Cast<AccTaxRate>().ToDictionary(x => x.AT_Code);

				var existingTaxConfigurations = new AccTaxConfigurationCollection(factory, countryCode);
				var existingTaxSystems = existingTaxConfigurations.Cast<AccTaxConfiguration>()
					.Select(x => x.ETC_TaxSystemCode).Distinct().ToHashSet();

				var taxRatesList = taxRatesForcountries[countryCode];
				foreach (TaxRateConfiguration taxRate in taxRatesList)
				{
					if (taxRate.TaxSystem.IsEmpty
						? (Company?.GC_IsGSTRegistered ?? true) && ShouldImportExtraTaxForSpain(taxRate.ExtraType)
						: existingTaxSystems.Contains(taxRate.TaxSystem))
					{
						ZGuid ratePK = ZGuid.Empty;
						if (existingTaxRatesByCode.TryGetValue(taxRate.TaxID, out var existingTaxRate))
						{
							ratePK = existingTaxRate.PK;
						}
						else
						{
							ratePK = CreateTaxRate(factory, countryCode, taxRate.TaxID, taxRate.Description, taxRate.Type, taxRate.ExtraType, taxRate.ReferenceRateType, taxRate.ReferenceExtraRateType, taxRate.TaxSystem, taxRate.RateSource).PK;
						}

						IAccounting accounting = ObjectFactory.Get<IAccounting>();
						if (taxRate.IsDefaultGST)
						{
							accounting.SetMainGSTTaxIDConfiguration(companyPK.ToGuid(), ratePK.ToGuid());
						}
						if (taxRate.IsDefaultFreeGST)
						{
							accounting.SetMainFreeGSTTaxIDConfiguration(companyPK.ToGuid(), ratePK.ToGuid());
						}
						if (taxRate.IsDefaultGSTReverse)
						{
							accounting.SetMainGSTReverseTaxIDConfiguration(companyPK.ToGuid(), ratePK.ToGuid());
						}
						if (taxRate.IsDefaultFreeGSTReverse)
						{
							accounting.SetMainFreeGSTReverseTaxIDConfiguration(companyPK.ToGuid(), ratePK.ToGuid());
						}
						if (taxRate.IsDefaultNotReport)
						{
							accounting.SetMainNotReportableTaxIDConfiguration(companyPK.ToGuid(), ratePK.ToGuid());
						}
					}
				}
			}
		}

		public int UpdatePostingGroupsFromXMLFile()
		{
			int resultCount = 0;
			var xmlParser = new TaxRateXmlParser();
			var taxRatesForcountries = xmlParser.BuildTaxRatesDictionaryBasedOnCountry();
			if (taxRatesForcountries.ContainsKey(countryCode))
			{
				var existingTaxRates = new AccTaxRateCollection(factory, countryCode);
				existingTaxRates.Load();
				var existingTaxRatesByCode = existingTaxRates.Cast<AccTaxRate>().ToDictionary(x => x.AT_Code);

				var taxRatesList = taxRatesForcountries[countryCode];
				foreach (var taxRate in taxRatesList)
				{
					if (existingTaxRatesByCode.TryGetValue(taxRate.TaxID, out var existingTaxRate) && existingTaxRate.AT_PostingGroupId != taxRate.PostingGroup)
					{
						existingTaxRate.AT_PostingGroupId = taxRate.PostingGroup;
						resultCount++;
					}
				}
			}

			return resultCount;
		}

		static AccTaxRate CreateTaxRate(BusinessObjectFactory factory,
			ZString countryCode,
			ZString taxCode,
			ZString description,
			ZString taxRateType,
			ZString extraTaxRateType,
			ZString referenceRateType,
			ZString referenceExraRateType,
			ZString taxSystem,
			ZString rateSource,
			bool isActive = true
			)
		{
			AccTaxRate result = factory.New<AccTaxRate>();

			result.AT_Code = taxCode;
			result.AT_IsActive = isActive;
			result.AT_Description = description;
			result.AT_Type = taxRateType;
			result.AT_ExtraTaxRateType = extraTaxRateType;
			result.AT_RN_NKCountry = countryCode;
			result.AT_PostingGroupId = 0;
			result.AT_ReferenceRateType = referenceRateType;
			result.AT_ReferenceExtraRateType = referenceExraRateType;
			result.AT_TaxSystemCode = taxSystem;
			result.AT_RateSource = rateSource;

			return result;
		}

#if DEBUG

		static internal AccTaxRate CreateTaxRate_ForTestOnly(BusinessObjectFactory factory,
			ZString countryCode,
			ZString taxCode,
			ZString description,
			ZString taxRateType,
			ZString extraTaxRateType,
			ZString referenceRateType,
			ZString referenceExraRateType,
			ZString taxSystem,
			ZString rateSource,
			bool isActive = true)
		{
			return CreateTaxRate(factory, countryCode, taxCode, description, taxRateType, extraTaxRateType, referenceRateType, referenceExraRateType, taxSystem, rateSource, isActive);
		}

#endif

	}
}
