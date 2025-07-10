using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.NLReferenceData.Business
{
	public class TariffsBuilder
	{
		public TariffsBuilder(StringBuilder errorCollector)
		{
			if (errorCollector == null)
			{
				throw new ArgumentNullException(nameof(errorCollector));
			}
			ErrorCollector = errorCollector;
		}
		StringBuilder ErrorCollector;

		public List<RefCusTariff> ConvertMeasuresToRefCusTariff(IEnumerable<Measure> data)
		{
			var refCusTariffs = ConvertToRefCusTariffs(data);
			var content = new List<RefCusTariff>();

			foreach (var refCusTariff in refCusTariffs)
			{
				if (IsValid(refCusTariff))
				{
					content.Add(refCusTariff);
				}
			}
			return content;
		}

		public static void GenerateUniversalReferenceDataXml(IEnumerable<RefCusTariff> content, DateTime publicationDate, string outputPath)
		{
			if (content.Any())
			{
				var dataSource = $"{XMLWriterDataSource}";
				Helper.ExportToXMLFile(dataSource, Path.Combine(outputPath, GetOutputFileName(dataSource, publicationDate)), GetRefCusTariffWriterConfiguration(), publicationDate, UpdateType.Full, content);
			}
		}

		protected bool IsValid(RefCusTariff refCusTariff)
		{
			bool valid = true;

			var validationErrors = new StringBuilder();

			if (string.IsNullOrWhiteSpace(refCusTariff.ZZ1_TariffCode))
			{
				validationErrors.Append("ZZ1_TariffCode is required. ");
				valid = false;
			}

			if (refCusTariff.RefCusVATApplicabilities != null)
			{
				foreach (var vatApplicability in refCusTariff.RefCusVATApplicabilities)
				{
					if (string.IsNullOrWhiteSpace(vatApplicability.ZX5_ZZF_NKTaxOrFeeCode))
					{
						validationErrors.Append("ZX5_ZZF_NKTaxOrFeeCode is required. ");
						valid = false;
					}
				}
			}

			if (refCusTariff.RefCusRates != null)
			{
				foreach (var rate in refCusTariff.RefCusRates)
				{
					if (string.IsNullOrWhiteSpace(rate.ZZ2_RateFormula))
					{
						validationErrors.Append("ZZ2_RateFormula is required. ");
						valid = false;
					}
					if (string.IsNullOrWhiteSpace(rate.ZZ2_ZY1_NKRateCode) || string.IsNullOrWhiteSpace(rate.ZZ2_ZY1_ZZR_NKRateType))
					{
						validationErrors.Append("ZZ2_ZY1_NKRateCode and ZZ2_ZY1_ZZR_NKRateType are required. Some mappings might be missing.");
						valid = false;
					}
				}
			}

			if (!valid)
			{
				var msg = Invariant($"RefCusTariff validation error: Key '{refCusTariff.ZZ1_TariffCode}_{refCusTariff.ZZ1_StartDate:yyyyMMdd}' Errors: {validationErrors}");
				ErrorCollector.AppendLine(msg);
			}

			return valid;
		}

		protected static IEnumerable<RefCusTariff> ConvertToRefCusTariffs(IEnumerable<Measure> data)
		{
			var results = new List<RefCusTariff>();
			foreach (var ad in data)
			{
				if (ServiceConstants.MeasureFilters.FilterMeasureTypeDuties.Contains(ad.MeasureType) && !string.IsNullOrWhiteSpace(ad.RateCode))
				{
					var refCusRateUOMs = new List<RefCusRateUOM>();
					if (ad.UnitOfMeasure != null)
					{
						foreach (var unitOfMeasure in ad.UnitOfMeasure)
						{
							var refCusRateUOM = new RefCusRateUOM();
							refCusRateUOM.ZXG_UOM = unitOfMeasure;
							refCusRateUOMs.Add(refCusRateUOM);
						}
					}

					var newRefCusTariff = new RefCusTariff()
					{
						ZZ1_TariffCode = ad.CleanId,
						RefCusRates = new[]
						{
							new RefCusRate
							{
								ZZ2_RateFormula = ad.Formula,
								ZZ2_StartDate = ad.DateStart ?? Constants.DefaultValues.MinimumDateTime,
								ZZ2_EndDate = ad.DateEnd ?? Constants.DefaultValues.MaximumDateTime,
								ZZ2_ZY1_NKRateCode = ad.RateCode,
								ZZ2_ZY1_ZZR_NKRateType = ad.RateType,
								RefCusRateUOMs = refCusRateUOMs.ToArray(),
								RefCusApplicabilities = new[]
								{
									new RefCusApplicability
									{
										ZZT_AdditionalCode = ad.AdditionalCode,
										ZZT_ZZA_NKTradeGroup = ad.GeographicalAreaId,
										ZZT_StartDate = ad.DateStart ?? Constants.DefaultValues.MinimumDateTime,
										ZZT_EndDate = ad.DateEnd?.MidnightToEndOfDay() ?? Constants.DefaultValues.MaximumDateTime,
									}
								}
							}
						}
					};
					results.Add(newRefCusTariff);
				}
				else if (ad.MeasureType == ServiceConstants.MeasureFilters.FilterMeasureTypeVAT)
				{
					var refCusTariff = results.Find(x => x.ZZ1_TariffCode == ad.CleanId);
					if (refCusTariff == default)
					{
						refCusTariff = new RefCusTariff()
						{
							ZZ1_TariffCode = ad.CleanId,
						};
						results.Add(refCusTariff);
					}

					var refCusVATApplacibility = new RefCusVATApplicability()
					{
						ZX5_AdditionalCode = ad.AdditionalCode,
						ZX5_ZZF_NKTaxOrFeeCode = ad.TaxOrFeeCode,
						ZX5_StartDate = ad.DateStart ?? Constants.DefaultValues.MinimumDateTime,
						ZX5_EndDate = ad.DateEnd ?? Constants.DefaultValues.MaximumDateTime,
					};

					if (refCusTariff.RefCusVATApplicabilities == null)
					{
						refCusTariff.RefCusVATApplicabilities = new List<RefCusVATApplicability>().ToArray();
					}

					refCusTariff.RefCusVATApplicabilities = refCusTariff.RefCusVATApplicabilities.Append(refCusVATApplacibility).ToArray();
				}
			}
			return results;
		}

		protected static XmlWriterConfiguration GetRefCusTariffWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var tariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(false);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, false, Constants.DefaultValues.TariffType);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.DefaultValues.EUNCountryCode);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.DefaultValues.EUNCountryCode);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);

			tariffConfiguration.IncludeColumn(x => x.RefCusRates);
			var rateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.DefaultValues.NLDataGrouping);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.DefaultValues.NLDataGrouping);
			rateConfiguration.IncludeColumn(x => x.ZZ2_EndDate);
			rateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula);
			rateConfiguration.IncludeColumn(x => x.ZZ2_StartDate);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, true);

			rateConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);
			var applicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
			applicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, Constants.DefaultValues.EUNCountryCode);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_AdditionalCode, true);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_EndDate);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_StartDate);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);

			rateConfiguration.IncludeColumn(x => x.RefCusRateUOMs, true);
			var rateUOMsConfiguration = new EntityTypeConfiguration<RefCusRateUOM>(true);
			rateUOMsConfiguration.IncludeColumn(x => x.ZXG_UOM, true);

			tariffConfiguration.IncludeColumn(x => x.RefCusVATApplicabilities);
			var vatApplicabilityConfiguration = new EntityTypeConfiguration<RefCusVATApplicability>(true);
			vatApplicabilityConfiguration.IncludeColumn(x => x.ZX5_ZZF_NKTaxOrFeeCode, true);
			vatApplicabilityConfiguration.IncludeColumn(x => x.ZX5_StartDate);
			vatApplicabilityConfiguration.IncludeColumn(x => x.ZX5_EndDate);
			vatApplicabilityConfiguration.IncludeColumn(x => x.ZX5_AdditionalCode, true);
			vatApplicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZX5_ZZZ_NKDataGrouping, true, Constants.DefaultValues.NLDataGrouping);

			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(vatApplicabilityConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(rateConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(applicabilityConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(rateUOMsConfiguration);

			return writerConfiguration;
		}

		static string FilePrefix => "RefCusTariff";
		static string XMLWriterDataSource => "NL Tariff";
		static string GetOutputFileName(string sourceName, DateTime publicationDate) => Invariant($"{FilePrefix}_{sourceName}_{publicationDate:HHmmssfff}.xml");
	}
}
