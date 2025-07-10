using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader
{
	public class TariffHelper : ITariffHelper
	{
		readonly IRefDataLoader refDataLoader;
		readonly ILogger logger;
		public TariffHelper(IRefDataLoader dataLoader, ILogger logger)
		{
			refDataLoader = dataLoader;
			this.logger = logger;
		}

		public (short UniqueId, bool Exists) GetUniqueId(string tariffCode, string checkDigit, string relationshipTariffCode)
		{
			short uniqueId = -1;
			bool exists = false;

			if (refDataLoader != null)
			{
				var keys = GetTariffKeys(tariffCode);

				var key = keys.FirstOrDefault(x => x.CheckDigit == checkDigit && x.RelationshipTariffCodes.Any(r => r == relationshipTariffCode));
				if (key != null)
				{
					uniqueId = key.UniqueId;
					exists = true;
				}
				else if (keys.Any())
				{
					uniqueId = keys.Max(x => x.UniqueId);
				}
			}

			return (uniqueId, exists);
		}

		public List<RuleMapping> GetRules() => RuleMappings;

		public List<TariffData> GetTariffs(string tariffCode)
		{
			if (TariffCheckDigits == null)
			{
				_ = GetTariffKeys(tariffCode);
			}

			var tariffs = Get<RefCusTariff>(GetTariffQuery(tariffCode)).ToList();
			var results = new List<TariffData>();
			foreach (var t in tariffs)
			{
				var tariffTypeCode = t.ZZ1_ZZI_TariffType == null ? null : Get<RefCusTariffType>(GetTariffTypeQuery(t.ZZ1_ZZI_TariffType.Value))?.FirstOrDefault()?.ZZI_TariffType;
				var tariffData = new TariffData
				{
					SubHeading = t.ZZ1_TariffCode,
					TariffCode = t.ZZ1_TariffCode,
					Description = t.ZZ1_Description,
					Schedule = SARSSchedule.Get(tariffTypeCode, t.ZZ1_TariffCode),
					StartDate = t.ZZ1_StartDate.UtcDateTime,
					EndDate = t.ZZ1_EndDate.UtcDateTime,
					UniqueId = t.ZZ1_IAMUnique,
					TariffKeyExists = true
				};
				if (TariffCheckDigits.TryGetValue(t.ZZ1_PK, out var checkDigit))
				{
					tariffData.CheckDigit = checkDigit;
				}
				results.Add(tariffData);
			}
			return results;
		}

		public List<Rate> GetRates(string tariffCode, DateTime startDate, DateTime endDate)
		{
			var results = new List<Rate>();
			var tariff = Get<RefCusTariff>(GetExpandedTariffQuery(tariffCode, startDate, endDate)).FirstOrDefault();
			if (tariff != null)
			{
				var dutyRates = tariff.RefCusRates.Where(r => r.RefCusRateCode.ZY1_RateCode == Schedules.S1P1 && r.RefCusApplicabilities.Count > 0);
				foreach (var dutyRate in dutyRates)
				{
					var applicability = dutyRate.RefCusApplicabilities.First();
					var rate = new Rate
					{
						Formula = dutyRate.ZZ2_RateFormula,
						Description = dutyRate.ZZ2_RateFormulaDerivedFrom,
						Preference = dutyRate.RefCusPreference.ZZS_Preference,
						RateType = SARSMappingHelper.GetRateTypeFromTradeGroup(applicability.RefCusTradeGroup.ZZA_TradeGroup),
						StartDate = CommonHelper.GetMaxDate(dutyRate.ZZ2_StartDate.UtcDateTime, applicability.ZZT_StartDate.UtcDateTime),
						EndDate = CommonHelper.GetMinDate(dutyRate.ZZ2_EndDate.UtcDateTime, applicability.ZZT_EndDate.UtcDateTime)
					};
					if (string.IsNullOrEmpty(rate.RateType))
					{
						rate.CountryCodes.AddRange(dutyRate.RefCusApplicabilities.Select(a => a.RefCusTradeGroup.ZZA_TradeGroup).OrderBy(country => country));
					}
					results.Add(rate);
				}
			}
			return results;
		}

		List<TariffKey> GetTariffKeys(string tariffCode)
		{
			var keys = CreateKeysFromTariffCode(tariffCode);

			if (keys.Any())
			{
				PopulateCheckDigits(keys);
				PopulateRelationshipTariffCode(keys);
			}

			return keys;
		}

		List<TariffKey> CreateKeysFromTariffCode(string tariffCode)
		{
			return Get<RefCusTariff>(GetTariffQuery(tariffCode))
					.Select(x => new TariffKey { PK = x.ZZ1_PK, UniqueId = x.ZZ1_IAMUnique }).ToList();
		}

		void PopulateCheckDigits(List<TariffKey> keys)
		{
			if (TariffCheckDigits == null)
			{
				TariffCheckDigits = Get<RefCusTariffAttribute>(GetTariffAttributeQuery())
					.Select(x => new { x.ZZ3_ZZ1_Tariff, x.ZZ3_Value })
					.ToList()
					.Where(x => x.ZZ3_ZZ1_Tariff.HasValue)
					.GroupBy(x => new { x.ZZ3_ZZ1_Tariff, x.ZZ3_Value })
					.ToDictionary(x => x.Key.ZZ3_ZZ1_Tariff.Value, x => x.Key.ZZ3_Value);
			}

			foreach (var key in keys)
			{
				if (TariffCheckDigits.TryGetValue(key.PK, out var value))
				{
					key.CheckDigit = value;
				}
			}
		}

		void PopulateRelationshipTariffCode(List<TariffKey> keys)
		{
			if (TariffRelationships == null)
			{
				TariffRelationships = Get<RefCusTariffRelationship>(GetTariffRelationshipQuery())
					.Select(x => new { x.ZZH_ZZ1_Tariff, x.ZZH_TariffCode })
					.ToList()
					.GroupBy(x => x.ZZH_ZZ1_Tariff)
					.ToDictionary(x => x.Key.Value, x => x.Select(r => r.ZZH_TariffCode).ToList());
			}

			foreach (var key in keys)
			{
				if (TariffRelationships.TryGetValue(key.PK, out var values))
				{
					key.RelationshipTariffCodes = values;
				}
				else
				{
					key.RelationshipTariffCodes = new List<string> { string.Empty };
				}
			}
		}

		List<RuleMapping> RuleMappings => ruleMappings ?? (ruleMappings = GetRuleMappings());
		List<RuleMapping> GetRuleMappings()
		{
			var tariffRules = Get<RefCusTariffRule>(GetTariffRuleQuery()).ToList();

			var result = PopulateRuleMappings(tariffRules);

			return result;
		}

		List<RuleMapping> PopulateRuleMappings(List<RefCusTariffRule> rules)
		{
			var results = new List<RuleMapping>();

			foreach (var rule in rules)
			{
				var attributes = Get<RefCusTariffAttributeRule>(GetTariffAttributeRuleQuery(rule.ZZ1_PK)).ToList();

				var map = new RuleMapping
				{
					RuleId = rule.ZZ1_PK,
					DataGrouping = rule.ZZ1_ZZZ_NKDataGrouping,
					TariffCode = rule.ZZ1_TariffCode,
					TariffType = rule.ZZ1_ZZI_TariffType == null ? null : Get<RefCusTariffType>(GetTariffTypeQuery(rule.ZZ1_ZZI_TariffType.Value))?.FirstOrDefault()?.ZZI_TariffType,
					CheckDigit = attributes.Where(x => x.ZZ3_Name == Constants.CheckDigit)?.FirstOrDefault()?.ZZ3_Value,

					Attributes = attributes.Where(x => x.ZZ3_Name != Constants.CheckDigit)?
													.Select(x => new RuleTariffAttribute { AttributeName = x.ZZ3_Name, AttributeValue = x.ZZ3_Value })
													.ToList(),

					UnitsOfMeasure = Get<RefCusTariffUOMRule>(GetTariffUOMRuleQuery(rule.ZZ1_PK))?
													.Select(x => new RuleTariffUOM { UOMType = x.ZZ8_Type, UOMValue = x.ZZ8_UOM })
													.ToList(),

					Rates = Get<RefCusRateRule>(GetRateRuleQuery(rule.ZZ1_PK))?
													.Select(x => new RuleRate { RateFormula = x.ZZ2_RateFormula, SelectorFormula = x.ZZ2_SelectorFormula, Description = x.ZZ2_RateFormulaDerivedFrom })
													.ToList()
				};

				results.Add(map);
			}

			return results;
		}

		protected IEnumerable<T> Get<T>(string query)
		{
			try
			{
				return refDataLoader.LoadData<T>(query).Result;
			}
			catch (Exception ex)
			{
				var msg = $"Failed to load query [{query}] from RefDataLoader";
				logger.LogError(msg);
				throw new ReferenceDataException(msg, ex);
			}
		}

		protected static string GetTariffQuery(string tariffCode) => $"RefCusTariffUpdate?$filter=ZZ1_ZZZ_NKDataGrouping eq '{Constants.ZADataGrouping}' and ZZ1_TariffCode eq '{tariffCode}'";
#pragma warning disable CA1024 // Use properties where appropriate
		protected static string GetTariffAttributeQuery() => $"RefCusTariffAttributeUpdate?$filter=ZZ3_Name eq '{Constants.CheckDigit}'";
		protected static string GetTariffRelationshipQuery() => "RefCusTariffRelationshipUpdate";
		protected static string GetTariffRuleQuery() => $"RefCusTariffRuleUpdate?$filter=ZZ1_ZZZ_NKDataGrouping eq '{Constants.ZADataGrouping}'";
#pragma warning restore CA1024 // Use properties where appropriate
		protected static string GetTariffAttributeRuleQuery(Guid tariffPk) => $"RefCusTariffAttributeRuleUpdate?$filter=ZZ3_ZZ1_Tariff eq {tariffPk}";
		protected static string GetTariffTypeQuery(Guid tariffTypePk) => $"RefCusTariffTypeUpdate?$filter=ZZI_PK eq {tariffTypePk}";
		protected static string GetTariffUOMRuleQuery(Guid tariffPk) => $"RefCusTariffUOMRuleUpdate?$filter=ZZ8_ZZ1_Tariff eq {tariffPk}";
		protected static string GetRateRuleQuery(Guid tariffPk) => $"RefCusRateRuleUpdate?$filter=ZZ2_ZZ1_Tariff eq {tariffPk}";

		protected static string GetExpandedTariffQuery(string tariffCode, DateTime startDate, DateTime endDate)
		{
			var builder = new StringBuilder();
			builder.Append(GetTariffQuery(tariffCode));
			builder.Append(CultureInfo.InvariantCulture, $" and ZZ1_StartDate le {startDate:yyyy-MM-dd} and ZZ1_EndDate ge {endDate:yyyy-MM-dd}");
			builder.Append("&$expand=RefCusRates($expand=RefCusApplicabilities($expand=RefCusTradeGroup),RefCusPreference,RefCusRateCode)");
			return builder.ToString();
		}


		Dictionary<Guid, string> TariffCheckDigits;
		Dictionary<Guid, List<string>> TariffRelationships;
		List<RuleMapping> ruleMappings;
	}
}
