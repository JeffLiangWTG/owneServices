using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Business.ExportTariffs.MasterData;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs
{
	public abstract class EdecTariffsParser : TariffsParser
	{
		protected EdecTariffsParser(DateTime actualDate) : base(actualDate)
		{
		}

		protected static void GetQuantityCode(List<RefCusTariffAttribute> refCusTariffAttributesList, tariffMasterDataCommodityCodeStatisticalCode statisticalCode, byte type)
		{
			if (statisticalCode?.quantityCode?.Length >= 1)
			{
				var code = (from quantityCode in statisticalCode.quantityCode
							where quantityCode.type == type
							orderby quantityCode.validTo
							select quantityCode)
									.LastOrDefault();

				if (code != null)
				{
					refCusTariffAttributesList.Add(new RefCusTariffAttribute()
					{
						ZZ3_Name = $"quantityCode{type}",
						ZZ3_Value = code.value.ToString(CultureInfo.InvariantCulture)
					});
				}
			}
		}

		protected static void GetMeanValueLimits(List<RefCusCondition> refCusConditionList, tariffMasterDataCommodityCodeStatisticalCode statisticalCode, string uom)
		{
			if (statisticalCode.meanValueLimit?.Length >= 1)
			{
				var meanValueLimitList = statisticalCode.meanValueLimit.OrderBy(s => s.validFrom).Reverse();
				foreach (var meanValueLimit in meanValueLimitList)
				{
					if (!IsOverlappingRecord(meanValueLimitList, meanValueLimit))
					{
						var refCusConditionValuesList = new List<RefCusConditionValue>();
						if (meanValueLimit.assessmentCode == 51)
						{
							refCusConditionValuesList.Add(new RefCusConditionValue()
							{
								ZX3_ZX4_NKValueType = "FRM",
								ZX3_Value = $"VFS/[KGM] <= {meanValueLimit.upperValue.PadLeftMissingZero()} & VFS/[KGM] >= {meanValueLimit.lowerValue.PadLeftMissingZero()}"
							});
						}
						if (meanValueLimit.assessmentCode == 61)
						{
							refCusConditionValuesList.Add(new RefCusConditionValue()
							{
								ZX3_ZX4_NKValueType = "FRM",
								ZX3_Value = $"VFS/[{uom}] <= {meanValueLimit.upperValue.PadLeftMissingZero()} & VFS/[{uom}] >= {meanValueLimit.lowerValue.PadLeftMissingZero()}"
							});
						}
						refCusConditionList.Add(new RefCusCondition()
						{
							ZX1_ZX2_NKConditionType = "MVC",
							ZX1_ZX2_ZZZ_NKDataGrouping = "CH",
							ZX1_Comment = "Mean Value Check",
							ZX1_StartDate = Helper.Truncate(meanValueLimit.validFrom),
							ZX1_EndDate = Helper.EndOfDay(Helper.Truncate(meanValueLimit.validTo)),
							RefCusConditionValues = refCusConditionValuesList.ToArray()
						});
					}
					else
					{
						break;
					}
				}
			}
		}

		internal void GetPermitConditions(List<RefCusCondition> refCusConditionList, tariffMasterDataCommodityCodeStatisticalCode statisticalCode, PermitInformation permitInformation, string commodityCodeValue)
		{
			if (statisticalCode.permit != null)
			{

				var permits = statisticalCode.permit.GroupBy(o => o.permitAuthority)
					.SelectMany(p =>
					{
						var minTolerance = p.Min(o => o.toleranceCode);
						return p.Where(o => o.toleranceCode == minTolerance);
					});


				foreach (var permit in permits)
				{
					List<RefCusConditionValue> valueList = new List<RefCusConditionValue>();
					List<RefCusApplicability> applicabilityList = new List<RefCusApplicability>();

					valueList.Add(new RefCusConditionValue()
					{
						ZX3_ZX4_NKValueType = "PRM",
						ZX3_Value = permit.permitAuthority.ToString(CultureInfo.InvariantCulture)
					});

					GetRefCusConditionOptionalValue(valueList, permit.optional);

					GetPermitToleranceValue(valueList, permit.toleranceCode);
					GetRefCusConditionCountryGroupAssignments(applicabilityList, permit.countryGroupAssignment);

					RefCusCondition condition = new RefCusCondition()
					{
						ZX1_ZX2_NKConditionType = $"PA{permit.permitAuthority}",
						ZX1_StartDate = permit.validFrom.Truncate(),
						ZX1_EndDate = permit.validTo.Truncate().EndOfDay(),
						RefCusConditionValues = valueList.ToArray(),
						RefCusApplicabilities = applicabilityList.ToArray(),
					};

					GetPermitComment(condition, permit, permitInformation, commodityCodeValue, statisticalCode.value);

					refCusConditionList.Add(condition);
				}
			}
		}

		internal static void GetPermitOptionalAttribute(List<RefCusTariffAttribute> refCusTariffAttributesList, tariffMasterDataCommodityCodeStatisticalCode statisticalCode)
		{
			if (statisticalCode.permit != null && statisticalCode.permit.Any())
			{
				var value = statisticalCode.permit.Any(p => p.optional) ? "1" : "0";
				refCusTariffAttributesList.Add(new RefCusTariffAttribute() { ZZ3_Name = "hasOptionalPermit", ZZ3_Value = value });
			}
		}

		void GetPermitComment(RefCusCondition condition, tariffMasterDataCommodityCodeStatisticalCodePermit permit, PermitInformation permitInformation, string commodityCodeValue, short statisticalCodeValue)
		{
			GetPermitComment(condition, permit.optional, permit.toleranceCode, permit.permitAuthority, permitInformation, commodityCodeValue, statisticalCodeValue);
		}

		internal static void GetRefCusConditionCountryGroupAssignments(List<RefCusApplicability> applicabilityList, countryGroupAssignment[] countryGroupAssignments)
		{
			if (countryGroupAssignments != null)
			{
				List<RefCusExcludedTradeGroup> excludedTradeGroupList = new List<RefCusExcludedTradeGroup>();
				foreach (var countryGroupAssignment in countryGroupAssignments.Where(a => a.type == countryGroupAssignmentType.exclude))
				{
					excludedTradeGroupList.Add(new RefCusExcludedTradeGroup()
					{
						ZZC_ZZA_NKTradeGroup = countryGroupAssignment.grpNr.ToString(CultureInfo.InvariantCulture),
					});
				}
				foreach (var countryGroupAssignment in countryGroupAssignments.Where(a => a.type == countryGroupAssignmentType.include))
				{
					applicabilityList.Add(new RefCusApplicability()
					{
						ZZT_ZZA_NKTradeGroup = countryGroupAssignment.grpNr.ToString(CultureInfo.InvariantCulture),
						ZZT_StartDate = countryGroupAssignment.validFrom.Truncate(),
						ZZT_EndDate = countryGroupAssignment.validTo.Truncate().EndOfDay(),
						RefCusExcludedTradeGroups = excludedTradeGroupList.ToArray(),
					});
				}
			}
		}

		protected static void GetScaleWeightCode(List<RefCusCondition> refCusConditionList, tariffMasterDataCommodityCodeStatisticalCode statisticalCode, string uom)
		{
			GetScaleWeightCode(refCusConditionList, statisticalCode.scaleWeightCode, statisticalCode.lowerScaleWeight, statisticalCode.upperScaleWeight, statisticalCode.validFrom, statisticalCode.validTo, uom);
		}

		protected static string GetAdditionalQuantityUOM(List<RefCusTariffUOM> tariffUOMList, tariffMasterDataCommodityCodeStatisticalCode statisticalCode)
		{
			var quantityCodeValue = (from quantityCode in statisticalCode.quantityCode
									 where quantityCode.type == 2
									 orderby quantityCode.validTo
									 select quantityCode.value)
												.Last();
			var additionalQuantityUOM = MapQuantityCode(quantityCodeValue);
			if (additionalQuantityUOM != null)
			{
				tariffUOMList.Add(new RefCusTariffUOM()
				{
					ZZ8_Type = "CU3",
					ZZ8_UOM = additionalQuantityUOM
				});
			}
			return additionalQuantityUOM;
		}

		internal static string MapQuantityCode(int code)
		{
			switch (code)
			{
				case 10:
					return null;
				case 11:
					return "LTR";
				case 12:
					return "MTR";
				case 13:
					return "MTK";
				case 14:
					return "MTQ";
				case 15:
					return "MWH";
				case 16:
					return "NCR";
				case 17:
					return "NAR";
				case 18:
					return "NAR";
				case 19:
					return "NAR";
				case 20:
					return "NPR";
				case 22:
					return "NAR";
				case 24:
					return "LTR";
				case 25:
					return "MTQ";
				case 26:
					return "NAR";
				default:
					return null;
			}
		}

		internal static bool IsOverlappingRecord(IEnumerable<tariffMasterDataCommodityCodeStatisticalCodeMeanValueLimit> meanValueLimitList, tariffMasterDataCommodityCodeStatisticalCodeMeanValueLimit meanValueLimit)
		{
			foreach (var listEntry in meanValueLimitList)
			{
				if (listEntry != meanValueLimit)
				{
					if (listEntry.validFrom < meanValueLimit.validFrom && meanValueLimit.validFrom < listEntry.validTo)
					{
						return true;
					}
					else if (listEntry.validFrom < meanValueLimit.validTo && meanValueLimit.validTo < listEntry.validTo)
					{
						return true;
					}
					else if (listEntry.validFrom < meanValueLimit.validFrom && meanValueLimit.validTo < listEntry.validTo)
					{
						return true;
					}
				}
			}
			return false;
		}

		protected static void GetNonCustomsLawConditions(List<RefCusCondition> refCusConditionList, tariffMasterDataCommodityCodeStatisticalCode statisticalCode, NonCustomsLawInformation nonCustomsLawInformation, string commodityCodeValue)
		{
			if (statisticalCode.nonCustomsLaw != null)
			{
				foreach (var nonCustomsLaw in statisticalCode.nonCustomsLaw)
				{
					List<RefCusConditionValue> valueList = new List<RefCusConditionValue>();
					List<RefCusApplicability> applicabilityList = new List<RefCusApplicability>();

					valueList.Add(new RefCusConditionValue()
					{
						ZX3_ZX4_NKValueType = "NCL",
						ZX3_Value = nonCustomsLaw.code.ToString(CultureInfo.InvariantCulture)
					});

					GetRefCusConditionOptionalValue(valueList, nonCustomsLaw.optional);
					GetRefCusConditionCountryGroupAssignments(applicabilityList, nonCustomsLaw.countryGroupAssignment);

					RefCusCondition condition = new RefCusCondition()
					{
						ZX1_ZX2_NKConditionType = $"N{nonCustomsLaw.code}",
						ZX1_StartDate = nonCustomsLaw.validFrom.Truncate(),
						ZX1_EndDate = nonCustomsLaw.validTo.Truncate().EndOfDay(),
						RefCusConditionValues = valueList.ToArray(),
						RefCusApplicabilities = applicabilityList.ToArray(),
					};

					GetNonCustomsLawComment(condition, nonCustomsLaw, nonCustomsLawInformation, commodityCodeValue, statisticalCode.value);

					refCusConditionList.Add(condition);
				}
			}
		}

		protected static void GetNonCustomsLawOptionalAttribute(List<RefCusTariffAttribute> refCusTariffAttributesList, tariffMasterDataCommodityCodeStatisticalCode statisticalCode)
		{
			if (statisticalCode.nonCustomsLaw != null && statisticalCode.nonCustomsLaw.Any())
			{
				var value = statisticalCode.nonCustomsLaw.Any(p => p.optional) ? "1" : "0";
				refCusTariffAttributesList.Add(new RefCusTariffAttribute() { ZZ3_Name = "hasOptionalNCL", ZZ3_Value = value });
			}
		}

		static void GetNonCustomsLawComment(RefCusCondition condition, tariffMasterDataCommodityCodeStatisticalCodeNonCustomsLaw nonCustomsLaw, NonCustomsLawInformation nonCustomsLawInformation, string commodityCodeValue, short statisticalCodeValue)
		{
			GetNonCustomsLawComment(condition, nonCustomsLaw.code, nonCustomsLaw.optional, nonCustomsLawInformation, commodityCodeValue, statisticalCodeValue);
		}
	}
}
