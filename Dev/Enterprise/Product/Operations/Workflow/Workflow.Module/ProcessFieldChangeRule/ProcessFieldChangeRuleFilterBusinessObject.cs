using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Module
{
	public class ProcessFieldChangeRuleFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filter = new ModuleFilterCollection();

			AddProcessTypeFilter(filter);
			AddGroupNameFilter(filter);
			AddDescriptionFilter(filter);
			AddEventFilter(filter);
			AddDateFilter(filter);
			AddFieldsFilter(filter);
			AddReferenceFilter(filter);

			return filter;
		}

		void AddFieldsFilter(ModuleFilterCollection filter)
		{
			var result = filter.AddTextFilter("FieldName", (comparisonOperator, value) => GetFieldsQuery(comparisonOperator, value), FieldList);
			result.MultilingualDescription = ResString.GetMultilingualString("PFRFilter|FieldName", "Field Name");
		}

		void AddGroupNameFilter(ModuleFilterCollection filter)
		{
			var result = filter.AddTextFilter(ProcessFieldChangeRuleSchema.Constants.PFR_GroupName, ProcessFieldChangeRuleSchema.PFR_GroupName);
			result.MultilingualDescription = ResString.GetMultilingualString("PFRFilter|PFR_GroupName", "Name");
		}

		void AddDescriptionFilter(ModuleFilterCollection filter)
		{
			var result = filter.AddTextFilter(ProcessFieldChangeRuleSchema.Constants.PFR_Description, ProcessFieldChangeRuleSchema.PFR_Description);
			result.MultilingualDescription = ResString.GetMultilingualString("PFRFilter|PFR_Description", "Description");
		}

		void AddReferenceFilter(ModuleFilterCollection filter)
		{
			var result = filter.AddTextFilter(ProcessFieldChangeRuleSchema.Constants.PFR_Reference, ProcessFieldChangeRuleSchema.PFR_Reference);
			result.MultilingualDescription = ResString.GetMultilingualString("PFRFilter|PFR_Reference", "Reference");
		}

		void AddProcessTypeFilter(ModuleFilterCollection filter)
		{
			var result = filter.AddTextFilter(ProcessFieldChangeRuleSchema.Constants.PFR_ProcessType, ProcessFieldChangeRuleSchema.PFR_ProcessType, () => new WorkflowDescriptorList());
			result.MultilingualDescription = ResString.GetMultilingualString("PFRFilter|PFR_ProcessType", "Workflow Type");
		}

		void AddEventFilter(ModuleFilterCollection filter)
		{
			var result = filter.AddTextFilter(ProcessFieldChangeRuleSchema.Constants.PFR_SE_NKEvent, ProcessFieldChangeRuleSchema.PFR_SE_NKEvent, Factory.GetCachedValue("StmCustomizableEventCodeDescriptionPairList", () => new StmCustomizableEventCodeDescriptionPairList(Factory)));
			result.MultilingualDescription = ResString.GetMultilingualString("PFRFilter|PFR_SE_NKEvent", "Event Code");
		}

		void AddDateFilter(ModuleFilterCollection filter)
		{
			filter.AddDateFilter(ProcessFieldChangeRuleSchema.Constants.PFR_SystemLastEditTimeUtc, ProcessFieldChangeRuleSchema.PFR_SystemLastEditTimeUtc, true).MultilingualDescription = ResString.GetMultilingualString("PFRFilter|PFR_SystemLastEditTime", "System Last Edit Time");
			filter.AddDateFilter(ProcessFieldChangeRuleSchema.Constants.PFR_SystemCreateTimeUtc, ProcessFieldChangeRuleSchema.PFR_SystemCreateTimeUtc, true).MultilingualDescription = ResString.GetMultilingualString("PFRFilter|PFR_SystemCreateTime", "System Create Time");
		}

		ZDBOnlyQuery GetFieldsQuery(SQLComparisonOperator @operator, IZType value)
		{
			bool isNotQuery = @operator.IsNegativeSQLOperator();

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ProcessFieldChangeRule));
			ZDBOnlySubQuery fieldSubQuery = new ZDBOnlySubQuery(typeof(ProcessFieldChangeRuleField), ProcessFieldChangeRuleFieldSchema.PFL_PFR, isNotQuery);

			var operatorForSubQuery = isNotQuery ? @operator.GetNegatingSQLOperatorIfNotInSubquery() : @operator;
			fieldSubQuery.AddToFilter(ProcessFieldChangeRuleFieldSchema.PFL_FieldName, operatorForSubQuery, value);
			result.AddSubQuery(fieldSubQuery, JoinCondition.And);

			return result;
		}

		IList FieldList()
		{
			//this table is cached in memory
			var fields = Factory.Load<ProcessFieldChangeRuleField>(new ZQuery());

			var list = new CodeDescriptionPairList();
			foreach (var field in fields)
			{
				if (!list.ContainsCode(field.PFL_FieldName))
				{
					list.AddPair(field.PFL_FieldName, field.FieldDisplayName);
				}
			}

			return list;
		}
	}
}
