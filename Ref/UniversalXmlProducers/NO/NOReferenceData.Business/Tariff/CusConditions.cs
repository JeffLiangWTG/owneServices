using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NOReferenceData.Services.RefCusConditions;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Avgiftliste;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Tollsats;
using CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.LandGruppe;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.Tariff
{
	public static class CusConditions
	{
		public static RefCusCondition[] GetCusConditions(RefCusCodeConditionItems refCusConditions, AvgiftListe importFees, string tariffId)
		{
			var refCusCondition = new List<RefCusCondition>();

			if (importFees?.Goods is null || refCusConditions?.RefCusCodeConditionItem is null)
			{
				return refCusCondition.ToArray();
			}		

			var cusCode = (from goods in importFees.Goods
						 where goods.id == tariffId
						 from dutyRate in goods.DutyRates
						 from dutyTypes in dutyRate.DutyTypes
						 from dutyGroup in dutyTypes.DutyGroups
						 where dutyTypes.DutyType != Constants.Types.VAT
						 select new
						 {
							 dutyTypes.DutyType,
							 dutyGroup.DutyGroup
						 });
			foreach (var singleCode in cusCode.Distinct())
			{
				var feeType = singleCode.DutyType + singleCode.DutyGroup;
				var condition = (from RefCusCodeConditionItemsRefCusCodeConditionItem in refCusConditions?.RefCusCodeConditionItem ?? Array.Empty<RefCusCodeConditionItemsRefCusCodeConditionItem>()
								 where RefCusCodeConditionItemsRefCusCodeConditionItem.CusCode == feeType
								  select new
								 {
									 RefCusCodeConditionItemsRefCusCodeConditionItem.StartDate,
									 RefCusCodeConditionItemsRefCusCodeConditionItem.EndDate,
									 RefCusCodeConditionItemsRefCusCodeConditionItem.CusCode,
									 RefCusCodeConditionItemsRefCusCodeConditionItem.IsImport,
									 RefCusCodeConditionItemsRefCusCodeConditionItem.ConditionFormulas
								  });

				foreach (var cond in condition.Distinct())
				{
					var refCusConditionValueLocal = new List<RefCusConditionValue>();
					foreach (var singleConditionFormula in cond.ConditionFormulas)
					{
						var cusConditionValue = ConvertRefCusConditionValue(singleConditionFormula);
						if (cusConditionValue != null)
						{
							refCusConditionValueLocal.Add(cusConditionValue);
						}
					}

					var cusCondition = ConvertRefCusCondition(cond.IsImport, cond.StartDate, cond.EndDate, cond.CusCode, refCusConditionValueLocal.ToArray());

					if (cusCondition != null)
					{
						refCusCondition.Add(cusCondition);
					}
				}
			}
			return refCusCondition.ToArray();
		}
	
		public static RefCusCondition ConvertRefCusCondition(bool isImport, string startDate, string endDate, string feeType, RefCusConditionValue[] values)
		{
			var (startDateOk, startDateDtm) = startDate.TryParseDateTime(Constants.XmlDateTimeFormat);
			var (endDateOk, endDateDtm) = endDate.TryParseEndDateTime(Constants.XmlDateTimeFormat);

			if (startDateOk && endDateOk && startDateDtm < endDateDtm && !string.IsNullOrEmpty(feeType) && values.Length > 0)
			{
				return new RefCusCondition
				{
					ZX1_IsImport = isImport,
					ZX1_StartDate = startDateDtm,
					ZX1_EndDate = endDateDtm,
					ZX1_ZX2_NKConditionType = feeType,
					RefCusConditionValues = values,
					RefCusApplicabilities = new[]
					{
						new RefCusApplicability()
						{
							ZZT_EndDate = endDateDtm,
							ZZT_StartDate = startDateDtm,
							ZZT_AdditionalCode = feeType
						}
					}
				};
			}

			TariffParser.ErrorBuilder.AppendLine("Unable to parse RefCusCondition code due to empty code or invalid Dates.");
			return null;
		}

		static RefCusConditionValue ConvertRefCusConditionValue(string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				return new RefCusConditionValue
				{
					ZX3_Value = value
				};
			}

			TariffParser.ErrorBuilder.AppendLine("Unable to parse RefCusConditionValue code due to empty value.");
			return null;
		}
	}
}
