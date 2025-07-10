using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
	public class ETradeExemptionCodesParser
	{
		public void GenerateUniversalReferenceData(string outputFolderPath)
		{
			try
			{
				var dataFilePath = Path.Combine(ApplicationConfig.ResPath, @"ETradeExemptionCodes.xlsx");
				var data = ETradeExemptionCodesLoader.LoadData(dataFilePath);
				Helper.ExportToXMLFile("TR ETrade Exemption Codes", outputFolderPath, GetETradeExemptionCodesWriterConfiguration(), PublicationDateTime, GetEntities(data));
			}
			catch (Exception ex)
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"GenerateUniversalReferenceData failed. Exception: {ex.GetBaseException().Message}");
			}
		}

		IEnumerable<RefDataRepoModelEntityType> GetEntities(IEnumerable<ETradeExemptionCodes> data)
		{
			var entities = new List<RefDataRepoModelEntityType>();
			entities.AddRange(CreateTariff(data));

			return entities;
		}

		protected virtual IEnumerable<RefCusTariff> CreateTariff(IEnumerable<ETradeExemptionCodes> tariffs)
		{
			return tariffs.Select(tariff => new RefCusTariff
			{
				ZZ1_TariffCode = tariff.ExemptionCode,
				ZZ1_Description = tariff.ExemptionDescEnglish,
				ZZ1_StartDate = tariff.StartDate,
				ZZ1_EndDate = tariff.EndDate,
				RefCusTariffLanguages = string.IsNullOrEmpty(tariff.ExemptionDescTurkish) ? null : new[]
				{
					new RefCusTariffLanguage
					{
						ZX7_ZX6_NKLanguage = Constants.CountryCodeTR,
						ZX7_Description = tariff.ExemptionDescTurkish
					}
				},
				RefCusRates = new[]
				{
					new RefCusRate
					{
						ZZ2_ZY1_ZZR_NKRateType = tariff.RateType,
						ZZ2_ZY1_NKRateCode = tariff.RateCode,
						ZZ2_RateFormula = tariff.DutyFormula,
						ZZ2_RateFormulaDerivedFrom = tariff.DutyPercent,
						ZZ2_StartDate = tariff.StartDate,
						ZZ2_EndDate = tariff.EndDate,
						RefCusApplicabilities = string.IsNullOrEmpty(tariff.ExemptionCode) ? null : new[]
						{
							new RefCusApplicability
							{
								ZZT_ZZA_NKTradeGroup = tariff.TradeGroup,
								ZZT_StartDate = tariff.StartDate,
								ZZT_EndDate = tariff.EndDate
							}
						}
					}
				}		
			});
		}

		protected static XmlWriterConfiguration GetETradeExemptionCodesWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var tariff = new EntityTypeConfiguration<RefCusTariff>(true);
			tariff.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariff.IncludeColumn(x => x.ZZ1_Description, true);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.TariffType.Code.ETR);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			tariff.IncludeColumn(x => x.RefCusRates, false);
			tariff.IncludeColumn(x => x.RefCusTariffLanguages, false);
			tariff.IncludeColumn(x => x.ZZ1_StartDate, false);
			tariff.IncludeColumn(x => x.ZZ1_EndDate, false);
			writerConfiguration.IncludeEntityTypeConfiguration(tariff);

			var languageConfiguration = new EntityTypeConfiguration<RefCusTariffLanguage>(true);
			languageConfiguration.IncludeColumn(x => x.ZX7_Description, false);
			languageConfiguration.IncludeColumnWithConstantValue(x => x.ZX7_ZX6_NKLanguage, true, Constants.CountryCodeTR);
			writerConfiguration.IncludeEntityTypeConfiguration(languageConfiguration);

			var rate = new EntityTypeConfiguration<RefCusRate>(true);
			rate.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			rate.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, true);
			rate.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			rate.IncludeColumn(x => x.ZZ2_RateFormula, false);
			rate.IncludeColumn(x => x.ZZ2_RateFormulaDerivedFrom, false);
			rate.IncludeColumn(x => x.RefCusApplicabilities, false);
			rate.IncludeColumn(x => x.ZZ2_StartDate, false);
			rate.IncludeColumn(x => x.ZZ2_EndDate, false);
			writerConfiguration.IncludeEntityTypeConfiguration(rate);

			var refCusApplicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
			refCusApplicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			refCusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);
			refCusApplicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_AdditionalCode, true, string.Empty);
			refCusApplicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_OrderNumber, true, string.Empty);
			refCusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_StartDate, false);
			refCusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_EndDate, false);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusApplicabilityConfiguration);

			return writerConfiguration;
		}

		protected virtual DateTime PublicationDateTime => DateTime.Now;

		public string ErrorMessage => ErrorBuilder.ToString();
		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;
	}
}
