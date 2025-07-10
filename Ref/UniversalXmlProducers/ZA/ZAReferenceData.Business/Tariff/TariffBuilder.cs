using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.ZAReferenceData.Business.Tariff
{
	public class TariffBuilder : XmlBuilder<Header, RefCusTariff>
	{
		public TariffBuilder(Header sourceData, ILogger logger) : base(sourceData, logger)
		{
		}

		protected override string FilePrefix => "ZA_RefCusTariff";

		protected override string DataSource => "ZATariffs";

		protected override UpdateType UpdateType => UpdateType.Partial;

		protected override XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var deactivate = sourceData.TransactionType == Constants.TransactionType.Deletion;

			var writerConfig = new XmlWriterConfiguration();
			var tariffConfig = new EntityTypeConfiguration<RefCusTariff>(true);

			tariffConfig.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfig.IncludeColumn(x => x.ZZ1_ZZI_NKTariffType, true);
			tariffConfig.IncludeColumn(x => x.ZZ1_IAMUnique, true);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.ZADataGrouping);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.ZADataGrouping);
			tariffConfig.IncludeColumn(x => x.ZZ1_Description);
			tariffConfig.IncludeColumn(x => x.ZZ1_StartDate);
			tariffConfig.IncludeColumn(x => x.ZZ1_EndDate);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZF_NKTaxOrFeeCode, false, Constants.VatFeeCode);
			tariffConfig.IncludeColumn(x => x.RefCusTariffAttributes);
			tariffConfig.IncludeKeyWithConstantValue("RefCusTariffAttribute.ZZ3_Name", Constants.CheckDigit);
			tariffConfig.IncludeKey("RefCusTariffAttribute.ZZ3_Value");

			if (!deactivate)
			{
				tariffConfig.IncludeColumn(x => x.RefCusRates);
				tariffConfig.IncludeColumn(x => x.RefCusTariffUOMs);
				tariffConfig.IncludeColumn(x => x.RefCusTariffRelationships);
			}

			writerConfig.IncludeEntityTypeConfiguration(tariffConfig);

			var attribConfig = new EntityTypeConfiguration<RefCusTariffAttribute>(true);
			attribConfig.IncludeColumn(x => x.ZZ3_Name, true);
			attribConfig.IncludeColumn(x => x.ZZ3_Value);

			writerConfig.IncludeEntityTypeConfiguration(attribConfig);

			if (!deactivate)
			{
				var uomConfig = new EntityTypeConfiguration<RefCusTariffUOM>(true);
				uomConfig.IncludeColumn(x => x.ZZ8_Type, true);
				uomConfig.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, true, Constants.ZADataGrouping);
				uomConfig.IncludeColumn(x => x.ZZ8_UOM);

				writerConfig.IncludeEntityTypeConfiguration(uomConfig);

				var rateConfig = new EntityTypeConfiguration<RefCusRate>(true);
				rateConfig.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
				rateConfig.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, true);
				rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.ZADataGrouping);
				rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.ZADataGrouping);
				rateConfig.IncludeColumn(x => x.RefCusApplicabilities, true);
				rateConfig.IncludeColumn(x => x.RefCusRateUOMs);
				rateConfig.IncludeColumn(x => x.ZZ2_StartDate);
				rateConfig.IncludeColumn(x => x.ZZ2_EndDate);
				rateConfig.IncludeColumn(x => x.ZZ2_RateFormula);
				rateConfig.IncludeColumn(x => x.ZZ2_RateFormulaDerivedFrom);
				rateConfig.IncludeColumn(x => x.ZZ2_ZZS_NKPreference);
				rateConfig.IncludeColumn(x => x.ZZ2_ZZS_ZZZ_NKDataGrouping);

				writerConfig.IncludeEntityTypeConfiguration(rateConfig);

				var appConfig = new EntityTypeConfiguration<RefCusApplicability>(true);
				appConfig.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);
				appConfig.IncludeColumn(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true);
				appConfig.IncludeColumn(x => x.ZZT_StartDate);
				appConfig.IncludeColumn(x => x.ZZT_EndDate);

				writerConfig.IncludeEntityTypeConfiguration(appConfig);

				var rateUOMConfig = new EntityTypeConfiguration<RefCusRateUOM>(true);
				rateUOMConfig.IncludeColumn(x => x.ZXG_UOM, true);

				writerConfig.IncludeEntityTypeConfiguration(rateUOMConfig);

				var relConfig = new EntityTypeConfiguration<RefCusTariffRelationship>(true);
				relConfig.IncludeColumn(x => x.ZZH_TariffCode, true);
				relConfig.IncludeColumn(x => x.ZZH_ZZI_NKTariffType, true);
				relConfig.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_ZZZ_NKDataGrouping, true, Constants.ZADataGrouping);

				writerConfig.IncludeEntityTypeConfiguration(relConfig);
			}

			return writerConfig;
		}

		protected override List<RefCusTariff> ConvertToRefModels()
		{
			var results = new List<RefCusTariff>();

			var deactivate = sourceData.TransactionType == Constants.TransactionType.Deletion;

			var validTariffs = sourceData.Tariffs.Where(x => x.IsValidTariff(sourceData, logger)).ToList();

			foreach (var t in validTariffs
								.OrderBy(x => x.Schedule.Schedule)
								.ThenBy(x => x.Schedule.Part)
								.ThenBy(x => x.Schedule.Section)
								.ThenBy(x => x.TariffCode)
								.ThenBy(x => x.CheckDigit))
			{
				var tariff = CreateTariff(t, deactivate);

				if (!t.TariffKeyExists)
				{
					var maxLoaded = validTariffs.Any(x => x.TariffCode == t.TariffCode) ? validTariffs.Where(x => x.TariffCode == t.TariffCode).Max(x => x.UniqueId) : -1;
					var maxCalc = results.Any(x => x.ZZ1_TariffCode == t.TariffCode) ? results.Where(x => x.ZZ1_TariffCode == t.TariffCode).Max(x => x.ZZ1_IAMUnique) : -1;

					tariff.ZZ1_IAMUnique = (short)((maxLoaded > maxCalc ? maxLoaded : maxCalc) + 1);
				}

				results.Add(tariff);
			}

			return results;
		}

		static RefCusTariff CreateTariff(TariffData data, bool isDeactivated)
		{
			var tariff = new RefCusTariff()
			{
				ZZ1_TariffCode = data.TariffCode,
				ZZ1_Description = data.Description,
				ZZ1_StartDate = data.CalcStartDate(),
				ZZ1_EndDate = data.CalcEndDate(),
				ZZ1_ZZI_NKTariffType = data.Schedule.GetTariffType(),
				ZZ1_IAMUnique = data.UniqueId
			};

			tariff.RefCusTariffAttributes = CreateRefCusTariffAttributes(data, isDeactivated);

			if (!isDeactivated)
			{	
				tariff.RefCusTariffUOMs = CreateRefCusTariffUOM(data);
				tariff.RefCusRates = CreateRates(data);
				tariff.RefCusTariffRelationships = CreateRefCusTariffRelationship(data);
			}

			return tariff;
		}

		static RefCusTariffAttribute[] CreateRefCusTariffAttributes(TariffData data, bool isDeactivated)
		{
			var results = new List<RefCusTariffAttribute>();

			if (!string.IsNullOrWhiteSpace(data.CheckDigit))
			{
				results.Add(new RefCusTariffAttribute
				{
					ZZ3_Name = Constants.CheckDigit,
					ZZ3_Value = data.CheckDigit
				});
			}

			if (!isDeactivated)
			{
				foreach (var attrib in data.AdditionalAttributes)
				{
					results.Add(new RefCusTariffAttribute
					{
						ZZ3_Name = attrib.Key,
						ZZ3_Value = attrib.Value
					});
				}
			}

			return results.Any() ? results.ToArray() : null;
		}

		static RefCusTariffUOM[] CreateRefCusTariffUOM(TariffData data)
		{
			var results = new List<RefCusTariffUOM>();
			var i = 1;

			if (!string.IsNullOrWhiteSpace(data.StatisticalUnitConverted))
			{
				results.Add(new RefCusTariffUOM
				{
					ZZ8_Type = $"CU{i++}",
					ZZ8_UOM = data.StatisticalUnitConverted
				});
			}

			var rateUOM = GetRateUOM(data);
			if (!string.IsNullOrWhiteSpace(rateUOM) && rateUOM != data.StatisticalUnitConverted)
			{
				results.Add(new RefCusTariffUOM
				{
					ZZ8_Type = $"CU{i}",
					ZZ8_UOM = rateUOM
				});
			}

			foreach (var kvp in data.AdditionalUOMs)
			{
				if (!string.IsNullOrWhiteSpace(kvp.Value))
				{
					results.Add(new RefCusTariffUOM
					{
						ZZ8_Type = kvp.Key,
						ZZ8_UOM = kvp.Value
					});
				}
			}

			return results.Any() ? results.ToArray() : null;
		}

		static string GetRateUOM(TariffData data)
		{
			return data.Rates.Where(r => r.IsValid(data))
				.GroupBy(g => g.RateType)
				.Select(g => g.First())
				.Where(r => !string.IsNullOrWhiteSpace(r.UnitOfMeasureConverted) && r.UnitOfMeasureConverted != data.StatisticalUnitConverted)
				.Select(r => r.UnitOfMeasureConverted)
				.Distinct()
				.SingleOrDefault();
		}

		static RefCusRate[] CreateRates(TariffData data)
		{
			var results = new List<RefCusRate>();

			foreach (var r in data.Rates.Where(r => r.IsValid(data))
										.GroupBy(g => g.RateType)
										.Select(g => g.First()))
			{
				var rate = results.FirstOrDefault(x => x.ZZ2_RateFormula == r.Formula &&
													   x.ZZ2_ZZS_NKPreference == r.Preference);

				if (rate == null)
				{
					rate = new RefCusRate
					{
						ZZ2_StartDate = data.CalcStartDate(),
						ZZ2_EndDate = data.CalcEndDate(),
						ZZ2_RateFormula = r.Formula,
						ZZ2_ZZS_NKPreference = r.Preference,
						ZZ2_ZZS_ZZZ_NKDataGrouping = string.IsNullOrWhiteSpace(r.Preference) ? null : Constants.ZADataGrouping,
						ZZ2_ZY1_NKRateCode = data.Schedule.GetTariffType(),
						ZZ2_ZY1_ZZR_NKRateType = data.Schedule.RateType,
						ZZ2_RateFormulaDerivedFrom = r.Description,
						RefCusApplicabilities = new RefCusApplicability[0]
					};

					results.Add(rate);
				}

				if (r.StartDate != null)
				{
					rate.ZZ2_StartDate = CommonHelper.GetMinDate(r.StartDate.Value, rate.ZZ2_StartDate);
				}

				rate.RefCusApplicabilities = CreateRefCusApplicabalities(data, r, rate.RefCusApplicabilities);
			}

			return results.Any() ? results.ToArray() : null;
		}

		static RefCusApplicability[] CreateRefCusApplicabalities(TariffData data, Rate rate, RefCusApplicability[] existingApplicabilities)
		{
			var results = existingApplicabilities.ToList();

			foreach (var tradeGroup in rate.GetTradeGroups(data))
			{
				if (!results.Any(x => x.ZZT_ZZA_NKTradeGroup == tradeGroup))
				{
					results.Add(new RefCusApplicability
					{
						ZZT_ZZA_NKTradeGroup = tradeGroup,
						ZZT_ZZA_ZZZ_NKDataGrouping = string.IsNullOrWhiteSpace(tradeGroup) ? null : Constants.ZADataGrouping,
						ZZT_StartDate = rate.StartDate ?? data.CalcStartDate(),
						ZZT_EndDate = rate.EndDate ?? data.CalcEndDate()
					});
				}
			}

			return results.Any() ? results.ToArray() : null;
		}

		static RefCusTariffRelationship[] CreateRefCusTariffRelationship(TariffData data)
		{
			var results = new List<RefCusTariffRelationship>();

			if (data.Schedule.CreateRelationship)
			{
				results.Add(new RefCusTariffRelationship
				{
					ZZH_TariffCode = data.RelationshipTariffCode,
					ZZH_ZZI_NKTariffType = Constants.Schedules.S1P1
				});
			}

			return results.Any() ? results.ToArray() : null;
		}
	}
}
