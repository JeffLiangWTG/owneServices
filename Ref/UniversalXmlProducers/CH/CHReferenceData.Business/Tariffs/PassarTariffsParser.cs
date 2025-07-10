using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.CHReferenceData.Business.ExportTariffs.MasterDataV3;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs
{
	public abstract class PassarTariffsParser : TariffsParser
	{
		protected PassarTariffsParser(DateTime actualDate) : base(actualDate)
		{
		}

		internal void GetPermitConditions(List<RefCusCondition> refCusConditionList, tariffMasterDataCommodityCodeControlCode controlCode, PermitInformation permitInformation, string commodityCodeValue)
		{
			if (controlCode.permit != null)
			{
				foreach (var permit in controlCode.permit.Where(x => x.validTo >= ActualDate))
				{
					List<RefCusConditionValue> valueList = new List<RefCusConditionValue>();
					List<RefCusApplicability> applicabilityList = new List<RefCusApplicability>();

					valueList.Add(new RefCusConditionValue()
					{
						ZX3_ZX4_NKValueType = "RST",
						ZX3_Value = permit.permitAuthority.ToString(CultureInfo.InvariantCulture)
					});

					GetRefCusConditionOptionalValue(valueList, permit.optional);
					GetPermitToleranceValue(valueList, permit.toleranceCode);
					GetRefCusConditionCountryGroupAssignments(applicabilityList, permit.countryGroupAssignment);

					RefCusCondition condition = new RefCusCondition()
					{
						ZX1_ZX2_NKConditionType = $"{PermitAuthorityConditionTypePrefix}{permit.permitAuthority}",
						ZX1_StartDate = permit.validFrom.Truncate(),
						ZX1_EndDate = permit.validTo.Truncate().EndOfDay(),
						RefCusConditionValues = valueList.ToArray(),
						RefCusApplicabilities = applicabilityList.ToArray(),
					};

					GetPermitComment(condition, permit.optional, permit.toleranceCode, permit.permitAuthority, permitInformation, commodityCodeValue, controlCode.value, permit.grpObligation);

					refCusConditionList.Add(condition);
				}
			}
		}

		protected override void AppendPermitGrpObligationComment(string language, StringBuilder comment, sbyte grpObligation)
		{
			if (grpObligation != 0)
			{
				comment.AppendFormat(CultureInfo.InvariantCulture, Appendix(), grpObligation);
			}

			string Appendix()
			{
				switch (language)
				{

					case "DE":
						return " / OBLIGATORISCH in Bewilligungsgruppe {0}";
					case "FR":
						return " / OBLIGATOIRE en groupe de permis {0}";
					case "IT":
						return " / OBBLIGATORIO nel gruppo di permesso {0}";
					default:
						return " / REQUIRED for Permit Group {0}";
				}
			}
		}

		protected void GetNonCustomsLawConditions(List<RefCusCondition> refCusConditionList, tariffMasterDataCommodityCodeControlCode controlCode, NonCustomsLawInformation nonCustomsLawInformation, string commodityCodeValue)
		{
			if (controlCode.nonCustomsLaw != null)
			{
				foreach (var nonCustomsLaw in controlCode.nonCustomsLaw.Where(x => x.validTo >= ActualDate))
				{
					List<RefCusConditionValue> valueList = new List<RefCusConditionValue>();
					List<RefCusApplicability> applicabilityList = new List<RefCusApplicability>();

					valueList.Add(new RefCusConditionValue()
					{
						ZX3_ZX4_NKValueType = "RST",
						ZX3_Value = nonCustomsLaw.code.ToString(CultureInfo.InvariantCulture)
					});

					GetRefCusConditionOptionalValue(valueList, nonCustomsLaw.optional);
					GetRefCusConditionCountryGroupAssignments(applicabilityList, nonCustomsLaw.countryGroupAssignment);

					RefCusCondition condition = new RefCusCondition()
					{
						ZX1_ZX2_NKConditionType = $"{NonCustomsLawConditionTypePrefix}{nonCustomsLaw.code}",
						ZX1_StartDate = nonCustomsLaw.validFrom.Truncate(),
						ZX1_EndDate = nonCustomsLaw.validTo.Truncate().EndOfDay(),
						RefCusConditionValues = valueList.ToArray(),
						RefCusApplicabilities = applicabilityList.ToArray(),
					};

					GetNonCustomsLawComment(condition, nonCustomsLaw.code, nonCustomsLaw.optional, nonCustomsLawInformation, commodityCodeValue, controlCode.value);

					refCusConditionList.Add(condition);
				}
			}
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

		protected static void GetStorageType(List<RefCusTariffAttribute> refCusTariffAttributesList, tariffMasterDataCommodityCodeControlCode controlCode)
		{
			if (controlCode?.storageType == "J")
			{
				refCusTariffAttributesList.Add(new RefCusTariffAttribute()
				{
					ZZ3_Name = "storageType",
					ZZ3_Value = "Y",
				});
			}
		}

		protected static void GetQuantityCode(List<RefCusTariffAttribute> refCusTariffAttributesList, tariffMasterDataCommodityCodeControlCode controlCode)
		{
			var code = (from declarationRule in controlCode.declarationRule
						where declarationRule.weight == DeclarationRuleCodes.DeadWght
						orderby declarationRule.validTo
						select declarationRule
						).LastOrDefault();

			if (code != null)
			{
				refCusTariffAttributesList.Add(new RefCusTariffAttribute()
				{
					ZZ3_Name = $"netMassOptional",
					ZZ3_Value = code.weightCalc == "J" ? "Y" : code.weightCalc
				});
				;
			}
		}

		protected static string GetAdditionalQuantityUOM(List<RefCusTariffUOM> tariffUOMList, tariffMasterDataCommodityCodeControlCode controlCode)
		{
			string additionalQuantityUOM = null;

			var quantityCodeValue = (from declartionRule in controlCode.declarationRule
									 where declartionRule.weight == DeclarationRuleCodes.AddVol
									 orderby declartionRule.validTo
									 select declartionRule.value
									).LastOrDefault();
			if (quantityCodeValue != 0)
			{
				additionalQuantityUOM = MapQuantityCode(quantityCodeValue);
				if (additionalQuantityUOM != null)
				{
					tariffUOMList.Add(new RefCusTariffUOM()
					{
						ZZ8_Type = "CU3",
						ZZ8_UOM = additionalQuantityUOM
					});
				}
			}

			return additionalQuantityUOM;
		}
		internal static string MapQuantityCode(int code)
		{
			switch (code)
			{
				case 200:
					return "KGM";
				case 201:
				case 202:
				case 225:
					return "NAR";
				case 203:
					return "MTR";
				case 204:
				case 211:
					return "LTR";
				case 205:
					return "MWH";
				case 206:
					return "KGMG";
				case 207:
					return "MTK";
				case 208:
				case 210:
					return "MTQ";
				case 209:
					return "NCR";
				case 212:
					return "NPR";
				case 223:
					return "NBR";
				default:
					return null;
			}
		}

		static class DeclarationRuleCodes
		{
			internal const string DeadWght = "DEAD_WGHT";
			internal const string AddVol = "ADD_VOL";
		}

		const string PermitAuthorityConditionTypePrefix = "PP";

		const string NonCustomsLawConditionTypePrefix = "PN";
	}
}
