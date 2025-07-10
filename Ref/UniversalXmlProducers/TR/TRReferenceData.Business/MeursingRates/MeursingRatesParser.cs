using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
	public class MeursingRatesParser
	{
		public void GenerateUniversalReferenceData(string outputFolderPath)
		{
			try
			{
				var dataFilePath = Path.Combine(ApplicationConfig.ResPath, @"MeursingRates.docx");
				var data = MeursingRatesLoader.LoadData(dataFilePath);
				Helper.ExportToXMLFile("TR Meursing Rates", outputFolderPath, GetMeursingRateWriterConfiguration(), PublicationDateTime, GetEntities(data));
			}
			catch (Exception ex)
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"GenerateUniversalReferenceData failed. Exception: {ex.GetBaseException().Message}");
			}
		}

		IEnumerable<RefDataRepoModelEntityType> GetEntities(IEnumerable<MeursingRates> data)
		{
			var entities = new List<RefDataRepoModelEntityType>();
			entities.AddRange(CreateTariff(data));

			return entities;
		}

		protected virtual IEnumerable<RefCusTariff> CreateTariff(IEnumerable<MeursingRates> tariffs)
		{
			var refCusTariffs = new List<RefCusTariff>();

			foreach (var tariff in tariffs)
			{
				var refCusRates = new List<RefCusRate>();

				if (!string.IsNullOrEmpty(tariff.T1))
				{
					refCusRates.Add(new RefCusRate
					{
						ZZ2_RateFormula = tariff.T1.Replace(",", ".") + " * [DTN]",
						ZZ2_ZY1_NKRateCode = Constants.TariffRateCode.Code._39,
						ZZ2_RateFormulaDerivedFrom = "T1",
						RefCusApplicabilities = new[]
						{
							new RefCusApplicability
							{
								ZZT_AdditionalCode = "T1"
							}
						}
					});
				}

				if (!string.IsNullOrEmpty(tariff.T2))
				{
					refCusRates.Add(new RefCusRate
					{
						ZZ2_RateFormula = tariff.T2.Replace(",", ".") + " * [DTN]",
						ZZ2_ZY1_NKRateCode = Constants.TariffRateCode.Code._39,
						ZZ2_RateFormulaDerivedFrom = "T2",
						RefCusApplicabilities = new[]
						{
							new RefCusApplicability
							{
								ZZT_AdditionalCode = "T2"
							}
						}
					});
				}

				if (refCusRates.Count > 0)
				{
					refCusTariffs.Add(new RefCusTariff
					{
						ZZ1_TariffCode = tariff.CodeNumber,
						ZZ1_Description = tariff.CodeNumber + " Meursing Additional Code",
						RefCusRates = refCusRates.ToArray()
					});
				}
			}

			return refCusTariffs;
		}

		protected static XmlWriterConfiguration GetMeursingRateWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var tariff = new EntityTypeConfiguration<RefCusTariff>(true);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.TariffType.Code.MEU);
			tariff.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariff.IncludeColumn(x => x.ZZ1_Description, false);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_StartDate, false, Constants.MeursingRateStartDate);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_EndDate, false, Constants.MaximumSmallDateTime);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			tariff.IncludeColumn(x => x.RefCusRates, false);
			writerConfiguration.IncludeEntityTypeConfiguration(tariff);

			var rate = new EntityTypeConfiguration<RefCusRate>(true);
			rate.IncludeColumnWithConstantValue(x => x.ZZ2_StartDate, false, Constants.MeursingRateStartDate);
			rate.IncludeColumnWithConstantValue(x => x.ZZ2_EndDate, false, Constants.MaximumSmallDateTime);
			rate.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			rate.IncludeColumn(x => x.ZZ2_RateFormula, false);
			rate.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, true);
			rate.IncludeColumn(x => x.ZZ2_RateFormulaDerivedFrom, true);
			rate.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			rate.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			rate.IncludeColumnWithConstantValue(x => x.ZZ2_RX_NKCurrencyOverride, false, "EUR");
			rate.IncludeColumn(x => x.RefCusApplicabilities, false);
			writerConfiguration.IncludeEntityTypeConfiguration(rate);

			var refCusApplicability = new EntityTypeConfiguration<RefCusApplicability>(true);
			refCusApplicability.IncludeColumnWithConstantValue(x => x.ZZT_StartDate, false, Constants.MeursingRateStartDate);
			refCusApplicability.IncludeColumnWithConstantValue(x => x.ZZT_EndDate, false, Constants.MaximumSmallDateTime);
			refCusApplicability.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_NKTradeGroup, true, Constants.TradeGroup.AllCountries);
			refCusApplicability.IncludeColumn(x => x.ZZT_AdditionalCode, true);
			refCusApplicability.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusApplicability);

			return writerConfiguration;
		}

		protected virtual DateTime PublicationDateTime => DateTime.Now;

		public string ErrorMessage => ErrorBuilder.ToString();
		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;
	}
}
