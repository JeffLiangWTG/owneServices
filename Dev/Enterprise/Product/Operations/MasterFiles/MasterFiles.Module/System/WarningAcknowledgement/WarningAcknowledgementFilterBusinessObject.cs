using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class WarningAcknowledgementFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddFlagFilters(filters);

			return filters;
		}

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Rule Name", GetRuleNames, RuleNames_List).MultilingualDescription = ResString.GetMultilingualString("MasterFile|WarningAcknowledgementFilter|RuleName", "Rule Name");
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Parent Table Code", GenCustomAddOnRuleAckSchema.XK_ParentTableCode).MultilingualDescription = ResString.GetMultilingualString("MasterFile|WarningAcknowledgementFilter|ParentTable", "Parent Table Code");
		}

		public CodeDescriptionPairList RuleNames_List
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				foreach (var value in Enterprise.Core.Constants.CargoWiseOneGenCustomAddOnRuleIDsList.Values)
				{
					list.AddPair(value);
				}
				return list;
			}
		}

		protected ZQuery GetRuleNames(ZString value)
		{
			ZString ruleName = value;
			ZQuery query = new ZQuery();
			foreach (KeyValuePair<Guid, ResourceString> kvp in Enterprise.Core.Constants.CargoWiseOneGenCustomAddOnRuleIDsList)
			{
				if (kvp.Value.GetUnresolvedString() == ruleName)
				{
					query.AddToFilter(JoinCondition.And, GenCustomAddOnRuleAckSchema.XK_RuleID, SQLComparisonOperator.Equal, kvp.Key);
					break;
				}
			}
			return query;
		}
	}
}
