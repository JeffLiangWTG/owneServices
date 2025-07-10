using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Services;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
	public class BanderolTariffDataParser
	{
		List<RefDataRepoModelEntityType> GetTariffEntities()
		{
			var records = new List<RefDataRepoModelEntityType>();
			try
			{
				records.AddRange(GetTariffsWithRateWithRateUOM());
			}
			catch (Exception ex)
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"GetTariffCodeList failure, Exception: {ex.GetBaseException().Message}");
			}
			return records;
		}

		public void GenerateTariffUniversalReferenceData(string outputFilePath)
		{
			try
			{
				var writerConfiguration = GetRefTariffWriterConfiguration();
				Helper.ExportToXMLFile("TR ETRBN Tariff", outputFilePath, writerConfiguration, PublicationDateTime, GetTariffEntities());
			}
			catch (Exception ex)
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"GenerateTariffUniversalReferenceData failure, Exception: {ex.GetBaseException().Message}");
			}
		}

		protected virtual DateTime PublicationDateTime => DateTime.Now;

		protected IEnumerable<string> GetValidTariffCodes(IEnumerable<string> tariffCodes)
		{
			var validationErrors = new StringBuilder();

			var duplicatedGroups = tariffCodes.GroupBy(c => c, (key, list) => new { Key = key, Count = list.Count() }).Where(x => x.Count > 1);

			if (duplicatedGroups.Any())
			{
				validationErrors.Append(CultureInfo.InvariantCulture, $"Duplicated code exists. Code: {string.Join(",", duplicatedGroups.Select(g => g.Key))}");
			}

			if (tariffCodes.Any(c => string.IsNullOrWhiteSpace(c)))
			{
				validationErrors.Append($"Empty code exists.");
			}

			ErrorBuilder.AppendLine(validationErrors.ToString());

			return tariffCodes.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct();
		}

		protected virtual IEnumerable<RefCusTariff> GetTariffsWithRateWithRateUOM()
		{
			var tariffRates = CsvLoader.GetBanderolTariffRates();
			var validTariffCodes = GetValidTariffCodes(tariffRates.Select(r => r.TariffCode));
			var validTariffRates = tariffRates.Where(r => validTariffCodes.Contains(r.TariffCode));
			var applicability = new RefCusApplicability { ZZT_StartDate = Constants.MinimumDateTime, ZZT_EndDate = Constants.MaximumSmallDateTime };

			foreach (var tariffRate in validTariffRates)
			{
				var tariffCode = tariffRate.TariffCode;
				yield return new RefCusTariff
				{
					ZZ1_TariffCode = tariffCode,
					ZZ1_Description = tariffRate.Description,
					ZZ1_ZZI_NKTariffType = Constants.TariffType.Code.ETRBN,
					ZZ1_StartDate = Constants.MinimumDateTime,
					ZZ1_EndDate = Constants.MaximumDateTime,
					RefCusRates = new[]
					{
						new RefCusRate
						{
							ZZ2_RateFormula = tariffRate.RateFormula,
							ZZ2_StartDate = Constants.MinimumDateTime,
							ZZ2_EndDate = Constants.MaximumDateTime,
							ZZ2_ZY1_NKRateCode = Constants.TariffRateCode.Code._75,
							ZZ2_ZY1_ZZR_NKRateType = Constants.TariffRateType.Code.BAN,
							ZZ2_RX_NKCurrencyOverride = tariffRate.Currency,
							RefCusRateUOMs = new[]
							{
								new RefCusRateUOM { ZXG_UOM = tariffRate.UOM }
							},
							RefCusApplicabilities = new[]
							{
								applicability
							}
						}
					}
				};
			}
		}
		protected static XmlWriterConfiguration GetRefTariffWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var tariff = new EntityTypeConfiguration<RefCusTariff>(true);
			tariff.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariff.IncludeColumn(x => x.ZZ1_ZZI_NKTariffType, true);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_IAMUnique, true, 0);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			tariff.IncludeColumn(x => x.ZZ1_StartDate, false);
			tariff.IncludeColumn(x => x.ZZ1_EndDate, false);
			tariff.IncludeColumn(x => x.ZZ1_Description, false);
			tariff.IncludeColumn(x => x.RefCusRates, false);
			writerConfiguration.IncludeEntityTypeConfiguration(tariff);

			var rate = new EntityTypeConfiguration<RefCusRate>(true);
			rate.IncludeColumn(x => x.ZZ2_EndDate, false);
			rate.IncludeColumn(x => x.ZZ2_StartDate, false);
			rate.IncludeColumn(x => x.ZZ2_RateFormula, false);
			rate.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			rate.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			rate.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			rate.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, true);
			rate.IncludeColumn(x => x.ZZ2_RX_NKCurrencyOverride, false);
			rate.IncludeColumn(x => x.RefCusRateUOMs, false);
			rate.IncludeColumn(x => x.RefCusApplicabilities, false);
			writerConfiguration.IncludeEntityTypeConfiguration(rate);

			var rateCode = new EntityTypeConfiguration<RefCusRateCode>(true);
			rateCode.IncludeColumn(x => x.ZY1_RateCode, true);
			rateCode.IncludeColumn(x => x.ZY1_Description, false);
			writerConfiguration.IncludeEntityTypeConfiguration(rateCode);

			var applicability = new EntityTypeConfiguration<RefCusApplicability>(true);
			applicability.IncludeColumn(x => x.ZZT_StartDate);
			applicability.IncludeColumn(x => x.ZZT_EndDate);
			applicability.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_NKTradeGroup, true, Constants.TradeGroup.AllCountries);
			applicability.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			writerConfiguration.IncludeEntityTypeConfiguration(applicability);

			var rateUOM = new EntityTypeConfiguration<RefCusRateUOM>(true);
			rateUOM.IncludeColumn(x => x.ZXG_UOM, true);
			writerConfiguration.IncludeEntityTypeConfiguration(rateUOM);

			return writerConfiguration;
		}

		public string ErrorMessage => ErrorBuilder.ToString();
		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;
	}
}
