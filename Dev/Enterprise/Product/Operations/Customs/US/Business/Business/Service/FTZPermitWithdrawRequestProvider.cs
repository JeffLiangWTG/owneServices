using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using PermitCriteria = Enterprise.Customs.Business.PermitCriteria;

namespace Enterprise.Customs.US.Business.Service
{
	class FTZPermitWithdrawRequestProvider : Customs.Business.IPermitWithdrawRequestProvider
	{
		ZString Customs.Business.IPermitWithdrawRequestProvider.GetMatchingCriteria(PermitCriteria permitCriteria)
		{
			var result = new ZStringBuilder();
			result.Append("Type (FTZ)");
			var warehouse = permitCriteria.Warehouse;
			if (warehouse == null)
			{
				result.Append("Warehouse is null");
			}
			else
			{
				var firmsCode = warehouse.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FIRMSCode, Core.Constants.CountryCodes.UnitedStates);
				result.Append(string.Format(CultureInfo.InvariantCulture, "Warehouse ({0}:{1}, FIRMS:{2})", warehouse.Header.OH_Code, warehouse.OA_Code, firmsCode));
			}
			var owner = permitCriteria.Owner;
			var ownerHeader = owner?.Header;
			result.Append(ownerHeader == null ? "Owner is null" : string.Format(CultureInfo.InvariantCulture, "Owner ({0}:{1})", ownerHeader.OH_Code, owner.OA_Code));
			var isDetailedTracking = permitCriteria.IsDetailedTracking;
			if (isDetailedTracking)
			{
				var manufacturer = permitCriteria.Manufacturer;
				if (manufacturer == null)
				{
					result.Append("Manufacturer is null");
				}
				else
				{
					result.Append(string.Format(CultureInfo.InvariantCulture, "Manufacturer ({0}:{1})", manufacturer.Header.OH_Code, manufacturer.OA_Code));
				}
			}
			else
			{
				result.Append(string.Format(CultureInfo.InvariantCulture, "Product ({0})", permitCriteria.ProductCode));
			}
			result.Append(string.Format(CultureInfo.InvariantCulture, "Tariff ({0})", TariffFormatter.DisplayFormat(permitCriteria.Tariff)));
			result.Append(string.Format(CultureInfo.InvariantCulture, "UQ ({0})", permitCriteria.UQ));
			if (isDetailedTracking)
			{
				result.Append(string.Format(CultureInfo.InvariantCulture, "Country Of Origin ({0})", permitCriteria.CountryOfOrigin));
				result.Append(string.Format(CultureInfo.InvariantCulture, "Zone Status ({0})", permitCriteria.ZoneStatus));
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		TariffFormatter TariffFormatter => tariffFormatter ?? (tariffFormatter = new TariffFormatter());
		TariffFormatter tariffFormatter;

		Customs.Business.BaseCusPermitHeader[] Customs.Business.IPermitWithdrawRequestProvider.FindMatchingPermit(PermitCriteria permitCriteria)
		{
			Customs.Business.BaseCusPermitHeader[] result = null;
			var owner = permitCriteria.Owner;
			var warehouse = permitCriteria.Warehouse;
			var tariff = permitCriteria.Tariff;
			if (owner != null && warehouse != null && !tariff.IsEmpty)
			{
				var query = new ZQuery(CusPermitHeaderSchema.CPH_OH_PermitHolder, owner.OA_OH);
				query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
				query.AddToFilter(CusPermitHeaderSchema.CPH_Type, PermitTypeList.Codes.FTZ);
				query.AddToFilter(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Permit);
				var effectiveDate = ZDate.Today;
				query.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
				query.AddToFilter(CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
				query.AddToFilter(CusPermitHeaderSchema.CPH_UnitOfMeasure, permitCriteria.UQ);
				query.AddToFilter(CusPermitHeaderSchema.CPH_IsClosed, false);
				var zoneStatus = permitCriteria.ZoneStatus;
				var countryOfOrigin = permitCriteria.CountryOfOrigin;
				var isDetailedTracking = permitCriteria.IsDetailedTracking;
				if (isDetailedTracking)
				{
					var manufacturer = permitCriteria.Manufacturer;
					if (manufacturer != null)
					{
						query.AddToFilter(CusPermitHeaderSchema.CPH_OA_AppliesTo, manufacturer.PK);
					}
					query.IsNoResultQuery = countryOfOrigin.IsEmpty || manufacturer == null || zoneStatus.IsEmpty;
				}
				else
				{
					var queryAppliesTo = new ZQuery(CusPermitHeaderSchema.CPH_OA_AppliesTo, ZGuid.Empty);
					queryAppliesTo.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_OA_AppliesTo, null);
					query.AddToFilter(queryAppliesTo);
				}
				var factory = owner.Factory;
				var productCode = permitCriteria.ProductCode;
				var permits = GetPermitMatchingFirm(factory.Load<CusPermitHeader>(query), warehouse.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FIRMSCode, Core.Constants.CountryCodes.UnitedStates));
				result = isDetailedTracking ? MatchDetailPermit(permits, zoneStatus, countryOfOrigin, tariff) : MatchSimplePermit(permits, productCode, tariff);
			}
			return result;
		}

		CusPermitHeader[] GetPermitMatchingFirm(CusPermitHeader[] permits, ZString firm)
		{
			foreach (var permit in permits)
			{
				permit.Factory.AddFetchHint(CusPermitRuleSchema.CPR_CPH_PermitHeader, permit.PK);
			}
			return permits.Where(x => x.CusPermitRules.Any(y => y.CPR_RuleCode == FirmRuleCode && y.CPR_ValueFrom == firm)).ToArray();
		}

		internal const string FirmRuleCode = "FRM"; //TODO: Replace with USPermitRuleCodeList.Codes.FRM

		CusPermitHeader[] MatchSimplePermit(CusPermitHeader[] permits, ZString productCode, ZString tariff)
		{
			return MatchPermitsCore(permits, productCode, Tuple.Create(USPermitRuleCodeList.Codes.TAR, tariff));
		}

		CusPermitHeader[] MatchDetailPermit(CusPermitHeader[] permits, ZString zoneStatus, ZString countryOfOrigin, ZString tariff)
		{
			return MatchPermitsCore(permits, ZString.Empty, Tuple.Create(USPermitRuleCodeList.Codes.TAR, tariff), Tuple.Create(USPermitRuleCodeList.Codes.COO, countryOfOrigin), Tuple.Create(USPermitRuleCodeList.Codes.ZST, zoneStatus));
		}

		CusPermitHeader[] MatchPermitsCore(CusPermitHeader[] permits, ZString productCode, params Tuple<string, ZString>[] permitRules)
		{
			var matchedWithProduct = new List<CusPermitHeader>();
			var matchedWithoutProduct = new List<CusPermitHeader>();
			var hasProductCode = !productCode.IsEmpty;

			foreach (var permit in permits)
			{
				var rulesDictionary = GatherRules(permit);

				ProcessRule(FirmRuleCode, rulesDictionary);//already matched

				var productRules = ProcessRule(USPermitRuleCodeList.Codes.PRD, rulesDictionary);

				if (Matched(permitRules, rulesDictionary))
				{
					if (HasMatchingRule(hasProductCode, productCode, productRules))
					{
						matchedWithProduct.Add(permit);
					}
					else if (productRules.Count == 0)
					{
						matchedWithoutProduct.Add(permit);
					}
				}
			}
			return matchedWithProduct.Count > 0 ? matchedWithProduct.ToArray() : matchedWithoutProduct.ToArray();
		}

		bool Matched(Tuple<string, ZString>[] permitRules, Dictionary<ZString, List<CusPermitRule>> rulesDictionary)
		{
			bool result = true;

			foreach (var permitRule in permitRules)
			{
				var rules = ProcessRule(permitRule.Item1, rulesDictionary);
				if (!HasMatchingRule(true, permitRule.Item2, rules))
				{
					result = false;
					break;
				}
			}

			return result && rulesDictionary.Count == 0;
		}

		List<CusPermitRule> ProcessRule(ZString rule, Dictionary<ZString, List<CusPermitRule>> rulesDictionary)
		{
			List<CusPermitRule> result = null;
			if (rulesDictionary.TryGetValue(rule, out result))
			{
				rulesDictionary.Remove(rule);
			}
			return result ?? new List<CusPermitRule>();
		}

		bool HasMatchingRule(bool hasValue, ZString value, List<CusPermitRule> rules)
		{
			return hasValue ? rules.Any(x => x.MatchesValue(value)) : rules.Count == 0;
		}

		Dictionary<ZString, List<CusPermitRule>> GatherRules(CusPermitHeader permit)
		{
			var rulesDictionary = new Dictionary<ZString, List<CusPermitRule>>();
			foreach (var group in permit.CusPermitRules.OfType<CusPermitRule>().Where(x => !x.CPR_ValueFrom.IsEmpty).GroupBy((x) => x.CPR_RuleCode))
			{
				rulesDictionary.Add(group.Key, new List<CusPermitRule>(group));
			}
			return rulesDictionary;
		}

		ZString Customs.Business.IPermitWithdrawRequestProvider.GetOutwardEntryNumber(Customs.Business.BaseCusPermitHeader permit)
		{
			return permit?.CusPermitLineTransactions.Where(x => x.CPL_TransactionType == PermitTransactionTypeList.Codes.OBL).OrderBy(x => x.CPL_TransactionDate).FirstOrDefault()?.CPL_Reference ?? ZString.Empty;
		}

		ZBool Customs.Business.IPermitWithdrawRequestProvider.IsExemptForMatchingPermit(MasterFiles.Integration.Customs.PermitService.IPermitTransactionDetail detail)
		{
			return detail.ZoneStatus == ZoneStatusList.Codes.Domestic;
		}
	}
}
