using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class PermitFindBoxCollection : CusPermitHeaderCollection, IFilterModuleExtraNotificationProvider
	{
		public PermitFindBoxCollection(BusinessObjectFactory factory, ZString countryCode, OrgHeader permitHolder, ZString permitType, ZString permitNumber, Dictionary<CodeDescriptionPair, ZString> filterRules, ZDate assessmentDate, string qtyValIndicator = "", bool isCurrent = false)
			: this(factory, countryCode, permitHolder, new ZString[] { permitType }, permitNumber, filterRules, assessmentDate, qtyValIndicator, isCurrent)
		{
		}

		public PermitFindBoxCollection(BusinessObjectFactory factory, ZString countryCode, OrgHeader permitHolder, ZString[] permitTypes, ZString permitNumber, Dictionary<CodeDescriptionPair, ZString> filterRules, ZDate assessmentDate, string qtyValIndicator = "", bool isCurrent = false)
			: base(factory, countryCode, CusPermitHeaderApplicationCodeList.Codes.Permit)
		{
			Argument.NotNull(permitTypes, "permitTypes");

			this.countryCode = countryCode;
			this.permitHolder = permitHolder;
			this.permitTypes = permitTypes;
			this.permitNumber = permitNumber;
			this.filterRules = filterRules;
			this.assessmentDate = assessmentDate;
			this.qtyValIndicator = qtyValIndicator;
			this.isCurrent = isCurrent;
			AdditionalFilter = GetAdditionalFilter();
			SetFilterDefaults();
		}

		readonly ZString countryCode;
		readonly OrgHeader permitHolder;
		readonly ZString[] permitTypes;
		readonly ZString permitNumber;
		readonly Dictionary<CodeDescriptionPair, ZString> filterRules;
		readonly ZDate assessmentDate;
		readonly ZString qtyValIndicator;
		readonly ZBool isCurrent;

		public static PermitFindBoxCollection GetCachedCollection(BusinessObjectFactory factory, ZString countryCode, OrgHeader permitHolder, ZString permitType, ZString permitNumber, Dictionary<CodeDescriptionPair, ZString> filterRules, ZDate assessmentDate, string qtyValIndicator = "", bool isCurrent = false) =>
			GetCachedCollection(factory, countryCode, permitHolder, new ZString[] { permitType }, permitNumber, filterRules, assessmentDate, qtyValIndicator, isCurrent);

		public static PermitFindBoxCollection GetCachedCollection(BusinessObjectFactory factory, ZString countryCode, OrgHeader permitHolder, ZString[] permitTypes, ZString permitNumber, Dictionary<CodeDescriptionPair, ZString> filterRules, ZDate assessmentDate, string qtyValIndicator = "", bool isCurrent = false)
		{
			filterRules = filterRules ?? new Dictionary<CodeDescriptionPair, ZString>();
			var filterRulesSubKey = GetFilterRulesKey(filterRules);
			var key = string.Format(CultureInfo.InvariantCulture, "PermitFindBoxCollection_{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}", countryCode, permitHolder?.PK, string.Join("-", permitTypes), permitNumber, filterRulesSubKey, assessmentDate, qtyValIndicator, isCurrent);
			return factory.GetCachedValue(key, () =>
			{
				var result = new PermitFindBoxCollection(factory, countryCode, permitHolder, permitTypes, permitNumber, filterRules, assessmentDate, qtyValIndicator, isCurrent);
				return result;
			});
		}

		static ZString GetFilterRulesKey(Dictionary<CodeDescriptionPair, ZString> filterRules)
		{
			return filterRules.OrderBy(x => x.Key.Code).Aggregate(new ZString(), (accumulate, keyValuePair) => accumulate += ZString.Format("{0}-{1}", keyValuePair.Key, keyValuePair.Value));
		}

		protected override bool AllowNew => false;

		void SetFilterDefaults()
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterConstants.Country, "Property", countryCode, false));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterConstants.PermitHolder, "Property", permitHolder?.PK ?? ZGuid.Empty));

			if (permitTypes.Length == 1)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterConstants.PermitTypeSubType, "Property1", permitTypes[0]));
			}
			else
			{
				var i = 1;
				foreach (var permitType in permitTypes)
				{
					FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterConstants.PermitTypeSubType, "Property1", permitType, ZArchitecture.Business.FilterOrCategory.Red, i++));
				}
			}

			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterConstants.PermitNumber, "Property", permitNumber));

			if (!qtyValIndicator.IsEmpty)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterConstants.QtyValIndicator, "Property", qtyValIndicator));
			}
			if (isCurrent)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterConstants.IsCurrent, "Property0", isCurrent));
			}
			else
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterConstants.StartDate, "PropertySearch", new ZString((NoResString)"In the Past")));
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterConstants.EndDate, "PropertySearch", new ZString((NoResString)"In the Future")));
			}
		}

		public Dictionary<CodeDescriptionPair, ZString> GetFilterRules()
		{
			return filterRules;
		}

		ZQuery GetAdditionalFilter()
		{
			var result = new ZQuery();

			result.AddToFilter(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Permit);

			if (permitHolder != null)
			{
				result.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, permitHolder.PK);
			}

			var filteredPermitTypes = permitTypes.Where(x => !x.IsEmpty).Distinct();
			if (filteredPermitTypes.Any())
			{
				var permitTypeFilter = new ZQuery();
				permitTypeFilter.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_Type, filteredPermitTypes);
				result.AddToFilter(permitTypeFilter, JoinCondition.And);
			}

			foreach (var filterRule in filterRules)
			{
				var ruleCode = filterRule.Key.Code;
				if (!string.IsNullOrEmpty(ruleCode))
				{
					result.AddToFilter(GetRuleQueryForCodePartOnly(countryCode, ruleCode)); // Rule Value is matched in MatchesFilterCore use code.
				}
			}

			if (!assessmentDate.IsEmpty)
			{
				result.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.LessThanOrEqualTo, assessmentDate);
				result.AddToFilter(CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, assessmentDate);
			}

			if (!qtyValIndicator.IsEmpty)
			{
				result.AddToFilter(GetQtyValIndicatorQuery(qtyValIndicator));
			}

			if (isCurrent)
			{
				result.AddToFilter(GetIsCurrentQuery(isCurrent, assessmentDate.IsEmpty));
			}

			return result;
		}

		protected override object[] GetCollectionState()
		{
			var state = new List<object>();

			var baseState = base.GetCollectionState();
			if (baseState != null)
			{
				state.AddRange(baseState);
			}

			state.Add(GetFilterRulesKey(filterRules));
			state.Add(ShouldIgnoreAdditionalFilter);

			return state.ToArray();
		}

		protected override bool MatchesFilterCore(BaseCusPermitHeader permitHeader, bool fetchOnlyFromLocalCache)
		{
			var isMatch = base.MatchesFilterCore(permitHeader, fetchOnlyFromLocalCache);
			if (isMatch && filterRules.Any() && (ShouldIgnoreAdditionalFilter == null || !ShouldIgnoreAdditionalFilter()))
			{
				foreach (var filterRule in filterRules)
				{
					var ruleValue = filterRule.Value;
					if (!ruleValue.IsEmpty)
					{
						isMatch = permitHeader.CusPermitRules.Any(x => x.MatchesValue(ruleValue));
					}
				}
			}
			return isMatch;
		}

		public Func<bool> ShouldIgnoreAdditionalFilter { get; set; }

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			var selectedPermitHeader = (BaseCusPermitHeader)selectedBusinessObject;

			if (permitHolder != null && selectedPermitHeader.CPH_OH_PermitHolder != permitHolder.PK)
			{
				errors.Add(MustHavePermitHolder(permitHolder.OH_Code));
			}

			if (permitTypes.Any() && !permitTypes.Contains(selectedPermitHeader.CPH_Type))
			{
				errors.Add(MustHavePermitType(permitTypes));
			}

			foreach (var filterRule in filterRules)
			{
				var ruleCode = filterRule.Key.Code;
				if (!string.IsNullOrEmpty(ruleCode))
				{
					var firstPermitRule = selectedPermitHeader.CusPermitRules.FirstOrDefault(rule =>
					{
						return rule.CPR_RuleCode == ruleCode;
					});

					if (firstPermitRule == null)
					{
						errors.Add(MustMatchPermitRuleCode(filterRule.Key.Description));
					}
				}
			}

			if (!assessmentDate.IsEmpty && (selectedPermitHeader.CPH_StartDate > assessmentDate || selectedPermitHeader.CPH_EndDate < assessmentDate))
			{
				errors.Add(MustMatchPermitDate(assessmentDate));
			}

			if (!qtyValIndicator.IsEmpty && selectedPermitHeader.CPH_QtyValIndicator != qtyValIndicator)
			{
				errors.Add(MustHavePermitQtyValIndicator(qtyValIndicator));
			}

			if (isCurrent && (selectedPermitHeader.CPH_QtyValIndicator == PermitQtyValIndicatorList.Codes.QTY || selectedPermitHeader.CPH_QtyValIndicator == PermitQtyValIndicatorList.Codes.BTH) && selectedPermitHeader.QuantityBalance <= 0)
			{
				errors.Add(MustHaveQuantityBalance);
			}

			if (isCurrent && (selectedPermitHeader.CPH_QtyValIndicator == PermitQtyValIndicatorList.Codes.VAL || selectedPermitHeader.CPH_QtyValIndicator == PermitQtyValIndicatorList.Codes.BTH) && selectedPermitHeader.ValueBalance <= 0)
			{
				errors.Add(MustHaveValueBalance);
			}
		}

		public INotification GetExtraNotification(BusinessObject businessObject)
		{
			INotification result = null;
			var selectedPermitHeader = (BaseCusPermitHeader)businessObject;
			foreach (var filterRule in filterRules)
			{
				var ruleValue = filterRule.Value;
				if (!ruleValue.IsEmpty)
				{
					var firstPermitRule = selectedPermitHeader.CusPermitRules.FirstOrDefault(rule =>
					{
						return rule.CPR_RuleCode == filterRule.Key.Code && rule.MatchesValue(ruleValue);
					});

					if (firstPermitRule == null)
					{
						result = new Notification(CargoWise.ComponentModel.NotificationType.Error, MustMatchPermitRule(ruleValue, filterRule.Key.Description));
					}
				}
			}
			return result;
		}

		public static string MustHavePermitHolder(ZString permitHolderCode) => Res.GetString("FA66D9F4-A493-47C3-93C6-590D0109588A", "A Permit selected from here must have a permit holder of '{0}'", permitHolderCode);

		public static string MustHavePermitType(ZString[] permitTypes) => Res.GetString("4EC8554F-9EDA-42EE-917A-4AFFC47D02BF", "A Permit selected from here must have a permit type of '{0}'", string.Join("', '", permitTypes));

		public static string MustMatchPermitRule(ZString ruleValue, ZString ruleDescription) => Res.GetString("40D40366-7BE5-415F-BA16-43B774696A85", "A Permit selected from here must have a '{1}' permit rule matching '{0}'", ruleValue, ruleDescription);

		public static string MustMatchPermitRuleCode(ZString ruleDescription) => Res.GetString("7CAD7161-65FE-4523-B670-F48B32157FE7", "A Permit selected from here must have a '{0}' permit rule", ruleDescription);

		public static string MustMatchPermitDate(ZDate assessmentDate) => Res.GetString("69654C87-3FD7-4A4D-A293-A91DEE891929", "A Permit selected from here must have a validity period matching the assessment date '{0}'", assessmentDate);

		public static string MustHavePermitQtyValIndicator(ZString qtyValIndicator) => Res.GetString("4F919B19-E6B5-4C84-AF70-3E1ABDB5C525", "A Permit selected from here must have a permit Qty/Value Indicator of '{0}'", qtyValIndicator);

		public static string MustHaveQuantityBalance => Res.GetString("557784FD-711C-45D6-85A1-7F5CF904C71C", "A Permit selected from here must have a positive quantity balance.");

		public static string MustHaveValueBalance => Res.GetString("9BEC995F-E2AC-41B9-BA06-7FFD5E1D3E20", "A Permit selected from here must have a positive value balance.");

		public static ZQuery GetRuleQueryForCodePartOnly(ZString country, ZString ruleCode)
		{
			var result = new ZQuery();
			if (!ruleCode.IsEmpty)
			{
				var permitHeaderFilter = new ZDBOnlyQuery(typeof(BaseCusPermitHeader));
				permitHeaderFilter.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, country);

				var permitRuleSubQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitRule), CusPermitRuleSchema.CPR_CPH_PermitHeader);
				permitRuleSubQuery.AddToFilter(CusPermitRuleSchema.CPR_RuleCode, ruleCode);

				permitHeaderFilter.AddSubQuery(permitRuleSubQuery, JoinCondition.And);
				result = permitHeaderFilter;
			}
			return result;
		}

		public static ZQuery GetQtyValIndicatorQuery(ZString qtyValIndicator)
		{
			var result = new ZQuery();
			if (!qtyValIndicator.IsEmpty)
			{
				result.AddToFilter(CusPermitHeaderSchema.CPH_QtyValIndicator, qtyValIndicator);
				result.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_QtyValIndicator, PermitQtyValIndicatorList.Codes.BTH);
			}
			return result;
		}

		public static ZQuery GetIsCurrentQuery(ZBool flag)
		{
			return GetIsCurrentQuery(flag, true);
		}

		public static ZQuery GetIsCurrentQuery(ZBool flag, bool includingDate)
		{
			var result = new ZQuery();

			if (flag)
			{
				if (includingDate)
				{
					result.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.LessThanOrEqualTo, ZDate.Today);
					result.AddToFilter(CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDate.Today);
				}

				var sql = $@"({CusPermitHeaderSchema.Constants.CPH_QtyValIndicator} = '{PermitQtyValIndicatorList.Codes.QTY}' OR {CusPermitHeaderSchema.Constants.CPH_QtyValIndicator} = '{PermitQtyValIndicatorList.Codes.BTH}')
AND
(
SELECT SUM({CusPermitLineTransactionSchema.Constants.CPL_TranQty}) FROM {CusPermitLineTransactionSchema.Constants.SqlSchemaName}.{CusPermitLineTransactionSchema.Constants.TableName} WHERE {CusPermitLineTransactionSchema.Constants.CPL_CPH_PermitHeader} = {CusPermitHeaderSchema.Constants.PK}
	AND {CusPermitLineTransactionSchema.Constants.CPL_TransactionCategory} = '{PermitTransactionCategoryList.Codes.CUM}'
	AND {CusPermitLineTransactionSchema.Constants.CPL_TransactionStatus} <> '{PermitTransactionStatusList.Codes.Deleted}'
) > 0
OR
(({CusPermitHeaderSchema.Constants.CPH_QtyValIndicator} = '{PermitQtyValIndicatorList.Codes.VAL}' OR {CusPermitHeaderSchema.Constants.CPH_QtyValIndicator} = '{PermitQtyValIndicatorList.Codes.BTH}')
AND
(
SELECT SUM({CusPermitLineTransactionSchema.Constants.CPL_TranValue}) FROM {CusPermitLineTransactionSchema.Constants.SqlSchemaName}.{CusPermitLineTransactionSchema.Constants.TableName} WHERE {CusPermitLineTransactionSchema.Constants.CPL_CPH_PermitHeader} = {CusPermitHeaderSchema.Constants.PK}
	AND {CusPermitLineTransactionSchema.Constants.CPL_TransactionCategory} = '{PermitTransactionCategoryList.Codes.CUM}'
	AND {CusPermitLineTransactionSchema.Constants.CPL_TransactionStatus} <> '{PermitTransactionStatusList.Codes.Deleted}'
) > 0
)";

				var query = new ZDBOnlyQuery(typeof(BaseCusPermitHeader));
				query.AddFilterAndZSQLParameterCollection(sql, null);
				result.AddToFilter(query);
			}

			return result;
		}

		public static ZQuery GetAppliesToQuery(ZGuid appliesToOH)
		{
			var permitQuery = new ZDBOnlyQuery(typeof(BaseCusPermitHeader));
			var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), CusPermitHeaderSchema.CPH_OA_AppliesTo);
			addressQuery.AddToFilter(OrgAddressSchema.OA_OH, appliesToOH);
			permitQuery.AddSubQuery(addressQuery, JoinCondition.And);
			return permitQuery;
		}
	}
}
