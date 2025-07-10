using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public class ProcessTemplateValidationFilterStripBusinessObject : FilterStripBusinessObject
	{
		public ProcessTemplateValidationFilterStripBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = ProcessTemplateValidation.Schema.TableName;
		}

		public IReadOnlyList<ProcessTaskTemplate> FilterInProcessTaskTemplates { get; set; } = Array.Empty<ProcessTaskTemplate>();

		public static class Descriptions
		{
			public static ResourceString ActionSource => ResString.GetMultilingualString("Enterprise.MasterFiles.GUI.ProcessTemplateValidationFilterStripBusinessObject|ActionSource", "Action Source");
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddStatusAndFlagsFilters(filters);

			return filters;
		}

		#region StatusAndFlags

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			AddActionSourceFilter(filters);
		}

		void AddActionSourceFilter(ModuleFilterCollection filters)
		{
			var actionSourceFilter = filters.AddTextFilter(
				Descriptions.ActionSource.EnglishText,
				GetActionSourceFilterQuery,
				GetActionSourceList);
			actionSourceFilter.MultilingualDescription = Descriptions.ActionSource;
			actionSourceFilter.Category = FilterCategories.StatusAndFlags;

			ZQuery GetActionSourceFilterQuery(SQLComparisonOperator comparisonOperator, ZString value)
			{
				var actionQuery = new ZDBOnlySubQuery(typeof(ProcessTemplateValidationAction), ProcessTemplateValidationActionSchema.P0A_P0V_ValidationRule);
				actionQuery.AddToFilter(JoinCondition.And, ProcessTemplateValidationActionSchema.P0A_ActionSource, comparisonOperator, value);
				if (FilterInProcessTaskTemplates != null && FilterInProcessTaskTemplates.Count > 0)
				{
					actionQuery.AddToFilter(JoinCondition.And, ProcessTemplateValidationActionSchema.P0A_P0_WorkflowTemplate, SQLComparisonOperator.Equal, FilterInProcessTaskTemplates.Select(x => x.PK));
				}
				var result = new ZDBOnlyQuery(typeof(ProcessTemplateValidation));
				result.AddSubQuery(ProcessTemplateValidationSchema.PK, actionQuery, JoinCondition.And);

				return result;
			}

			System.Collections.IList GetActionSourceList()
			{
				CodeDescriptionPairList actionSourceList = null;

				if (FilterInProcessTaskTemplates != null && FilterInProcessTaskTemplates.Count > 0)
				{
					actionSourceList = new CodeDescriptionPairList();
					foreach(var filterInProcessTaskTemplate in FilterInProcessTaskTemplates)
					{
						var list = filterInProcessTaskTemplate.Lookups.ProcessTemplateValidationActionSourceList;
						actionSourceList.AddPairsIfNotExist(list.Cast<ICodeDescription>());
					}
				}

				return actionSourceList;
			}
		}

		#endregion
	}
}
