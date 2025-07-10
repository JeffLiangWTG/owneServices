using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public sealed class StrategicGoodsData : IStrategicGoodsData
	{
		public StrategicGoodsData(ZString requestedTariff, ZString tariff, ZString tariffTypeDescription, ZString source, ZString comment, ZString conditionType, ZString conditionTypeDescription, ZDateTime startDate, ZDateTime endDate)
		{
			RequestedTariff = requestedTariff;
			Tariff = tariff;
			TariffTypeDescription = tariffTypeDescription;
			Source = source;
			Comment = comment;
			ConditionType = conditionType;
			ConditionTypeDescription = conditionTypeDescription;
			StartDate = startDate;
			EndDate = endDate;
		}
		public ZString RequestedTariff { get; }
		public ZString Tariff { get; }
		public ZString TariffTypeDescription { get; }
		public ZString Source { get; }
		public ZString Comment { get; }
		public ZString ConditionType { get; }
		public ZString ConditionTypeDescription { get; }
		public ZDateTime StartDate { get; }
		public ZDateTime EndDate { get; }
	}

	public class StrategicGoods
	{
		public IReadOnlyList<IStrategicGoodsData> GetStrategicGoodsData(BusinessObjectFactory factory, HashSet<ZString> tariffCodes, ZString countrycode, ZDateTime effectiveDate)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(tariffCodes, nameof(tariffCodes));

			if (!effectiveDate.IsValid)
			{
				effectiveDate = ZDateTime.Today;
			}

			var result = new List<IStrategicGoodsData>();

			var codeLengths = GetCodeLengths(factory);
			if (codeLengths.AscendingTariffCodeLengths.Count > 0)
			{
				var minTariffCodeLength = codeLengths.AscendingTariffCodeLengths[0];
				foreach (var requestedTariff in tariffCodes.Where(tc => tc.Length >= minTariffCodeLength))
				{
					result.AddRange(GetStrategicGoodsDataForOnTariffCode(requestedTariff));
				}
			}

			return result;

			IEnumerable<IStrategicGoodsData> GetStrategicGoodsDataForOnTariffCode(ZString requestedTariff)
			{
				return factory.GetCachedValue($"MatchedTariffAndNomenclatureRecord_{requestedTariff}_{effectiveDate}", () =>
				{
					var possibleTariffCodes = codeLengths.AscendingTariffCodeLengths.Select(length => requestedTariff.SubstringSafe(0, length)).ToImmutableHashSet();
					var possibleNomenclatureCodes = codeLengths.AscendingFilteredNomenclatureLengths.Select(length => requestedTariff.SubstringSafe(0, length)).ToImmutableHashSet();
					var hasPossibleTariffCodes = possibleTariffCodes.Count > 0;
					var hasPossibleNomenclatureCodes = possibleNomenclatureCodes.Count > 0;
					if (hasPossibleTariffCodes || hasPossibleNomenclatureCodes)
					{
						var sql = new ZStringBuilder();
						var parameters = new List<ZSqlParameter>
						{
							ZSqlParameter.New("@startDateOfCondition", effectiveDate, RefCusConditionSchema.ZX1_StartDate),
							ZSqlParameter.New("@endDateOfCondition", effectiveDate, RefCusConditionSchema.ZX1_EndDate)
						};

						if (hasPossibleTariffCodes)
						{
							sql.Append(SqlTemplateToQueryRefCusTariff);
							parameters.AddRange(new[]
							{
								ZSqlParameter.New("@tariffCode", possibleTariffCodes, RefCusTariffSchema.ZZ1_TariffCode, true),
								ZSqlParameter.New("@startDateOfTariff", effectiveDate, RefCusTariffSchema.ZZ1_StartDate),
								ZSqlParameter.New("@endDateOfTariff", effectiveDate, RefCusTariffSchema.ZZ1_EndDate)
							});
						}

						if (hasPossibleNomenclatureCodes)
						{
							sql.Append(SqlTemplateToQueryRefCusNomenclature);
							parameters.AddRange(new[]
							{
								ZSqlParameter.New("@nomenclatureCode", possibleNomenclatureCodes, RefCusNomenclatureGroupSchema.ZZ5_Value, true),
								ZSqlParameter.New("@startDateOfNom", effectiveDate, RefCusNomenclatureGroupSchema.ZZ5_StartDate),
								ZSqlParameter.New("@endDateOfNom", effectiveDate, RefCusNomenclatureGroupSchema.ZZ5_EndDate)
							});
						}

						var collection = new DynamicBusinessObjectCollection(factory);
						collection.Load(sql.ToStringWithDelimiterBetweenAppends(SqlTemplateUnionKeyword), parameters.ToArray());

						return collection.Select(dynamicObject => new StrategicGoodsData(requestedTariff,
							(ZString)dynamicObject[nameof(IStrategicGoodsData.Tariff)],
							(ZString)dynamicObject[nameof(IStrategicGoodsData.TariffTypeDescription)],
							(ZString)dynamicObject[nameof(IStrategicGoodsData.Source)],
							(ZString)dynamicObject[nameof(IStrategicGoodsData.Comment)],
							(ZString)dynamicObject[nameof(IStrategicGoodsData.ConditionType)],
							(ZString)dynamicObject[nameof(IStrategicGoodsData.ConditionTypeDescription)],
							(ZDateTime)dynamicObject[nameof(IStrategicGoodsData.StartDate)],
							(ZDateTime)dynamicObject[nameof(IStrategicGoodsData.EndDate)])).ToList<IStrategicGoodsData>();
					}
					else
					{
						return Enumerable.Empty<IStrategicGoodsData>();
					}
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "string used in codes only, won't be shown on any GUI")]
		(IReadOnlyList<ZInt> AscendingTariffCodeLengths, IReadOnlyCollection<ZInt> AscendingFilteredNomenclatureLengths) GetCodeLengths(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("TariffAndNomenclatureRecordLength", () =>
			{
				var collection = new DynamicBusinessObjectCollection(factory);
				collection.Load(@"
					SELECT DISTINCT(DATALENGTH(ZZ5_Value)) [Length], 'Nomenclature' [LengthSource]
						FROM RefDatabase_RefCusNomenclatureGroup
						JOIN RefDatabase_RefCusTariffType ON ZZ5_ZZ9_NKNomenclatureGroupType = ZZI_ZZ9_NKNomenclatureGroupType AND ZZ5_ZZZ_NKDataGrouping = ZZI_ZZZ_NKDataGrouping
						WHERE ZZI_TariffType = 'HSN' AND ZZ5_ZZZ_NKDataGrouping = 'WCO'
					UNION ALL
					SELECT DISTINCT(DATALENGTH(ZZ1_TariffCode)), 'Tariff'
						FROM RefDatabase_RefCusTariff
						JOIN RefDatabase_RefCusTariffType ON ZZ1_ZZI_TariffType = ZZI_PK
						WHERE ZZI_TariffType = 'HSN' AND ZZ1_ZZZ_NKDataGrouping = 'WCO'");
				var splitSet = collection.Split(x => (ZString)x["LengthSource"] == "Tariff");
				var ascendingTariffCodeLengths = splitSet.MatchingSet.Select(c => (ZInt)c["Length"]).OrderBy(x => x).ToArray();
				var minTariffCodeLength = ascendingTariffCodeLengths.FirstOrDefault();
				var ascendingNomenclatureLengths = splitSet.NonMatchingSet.Select(c => (ZInt)c["Length"]).Where(x => x >= minTariffCodeLength).OrderBy(x => x).ToArray();
				return (ascendingTariffCodeLengths, ascendingNomenclatureLengths);
			});
		}

		const string SqlTemplateToQueryRefCusTariff = @"
	SELECT ZZ1_TariffCode [Tariff], ZZI_Description [TariffTypeDescription], ZX1_Source [Source], ZX1_Comment [Comment], ZX2_ConditionType [ConditionType], ZX2_Description [ConditionTypeDescription],
		IIF(ZZ1_StartDate <= ZX1_StartDate, ZX1_StartDate, ZZ1_StartDate) [StartDate],
		IIF(ZZ1_EndDate <= ZX1_EndDate, ZZ1_EndDate, ZX1_EndDate) [EndDate]
	FROM RefDatabase_RefCusTariff
	JOIN RefDatabase_RefCusTariffType ON ZZ1_ZZI_TariffType = ZZI_PK
	JOIN RefDatabase_RefCusCondition ON ZZ1_PK = ZX1_ZZ1_Tariff
	JOIN RefDatabase_RefCusConditionType ON ZX1_ZX2_ConditionType = ZX2_PK
	WHERE ZZI_TariffType = 'HSN' AND ZZ1_ZZZ_NKDataGrouping='WCO' AND ZX2_ConditionClass = 'RISK' AND ZZ1_StartDate <= @startDateOfTariff AND ZZ1_EndDate >= @endDateOfTariff AND ZX1_StartDate <= @startDateOfCondition AND ZX1_EndDate >= @endDateOfCondition AND ZZ1_TariffCode IN (SELECT Value FROM @tariffCode)";

		const string SqlTemplateUnionKeyword = @"
	UNION ALL
";

		const string SqlTemplateToQueryRefCusNomenclature = @"
	SELECT ZZ5_Value [Tariff], ZZI_Description [TariffTypeDescription], ZX1_Source [Source], ZX1_Comment [Comment], ZX2_ConditionType [ConditionType], ZX2_Description [ConditionTypeDescription],
		IIF(ZZ5_StartDate <= ZX1_StartDate, ZX1_StartDate, ZZ5_StartDate) [StartDate],
		IIF(ZZ5_EndDate <= ZX1_EndDate, ZZ5_EndDate, ZX1_EndDate) [EndDate]
	FROM RefDatabase_RefCusTariffType
	JOIN RefDatabase_RefCusNomenclatureGroup ON ZZI_ZZ9_NKNomenclatureGroupType = ZZ5_ZZ9_NKNomenclatureGroupType AND ZZI_ZZZ_NKDataGrouping = ZZ5_ZZZ_NKDataGrouping
	JOIN RefDatabase_RefCusCondition on ZX1_ZZ5_Nomenclature = ZZ5_PK
	JOIN RefDatabase_RefCusConditionType ON ZX1_ZX2_ConditionType = ZX2_PK
	WHERE ZZI_TariffType = 'HSN' AND ZZI_ZZZ_NKDataGrouping='WCO' AND ZX2_ConditionClass = 'RISK' AND ZZ5_StartDate <= @startDateOfNom AND ZZ5_EndDate >= @endDateOfNom AND ZX1_StartDate <= @startDateOfCondition AND ZX1_EndDate >= @endDateOfCondition AND ZZ5_Value IN (SELECT Value FROM @nomenclatureCode)";
	}
}
