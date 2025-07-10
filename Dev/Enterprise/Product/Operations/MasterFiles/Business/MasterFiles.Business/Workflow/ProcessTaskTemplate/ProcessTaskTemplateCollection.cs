using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.ProcessTemplates)]
	public sealed class ProcessTaskTemplateCollection : BusinessObjectCollection<ProcessTaskTemplate>
	{
		public ProcessTaskTemplateCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ProcessTaskTemplateCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		#region Filter

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			var companyQuery = new ZQuery(ProcessTaskTemplateSchema.P0_GC, GlbCompany.CurrentCompany.PK);
			companyQuery.AddToFilter(JoinCondition.Or, ProcessTaskTemplateSchema.P0_GC, SQLComparisonOperator.Equal, null);

			query.AddToFilter(companyQuery);

			if (!DataRegistry.Instance.ProductivityWiseModeEnabled)
			{
				return query;
			}

			var productivityWiseWorkflowTypeCodes = WorkflowDescriptors.AllowedWorkflowDescriptorCodesForProductivityWise();

			var validPWiseWorkflowType = new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, productivityWiseWorkflowTypeCodes);

			return query.AddToFilter(validPWiseWorkflowType);
		}

		#endregion

		#region FindBoxListProvider

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new ProcessTaskTemplateFindBoxListProvider(this); }
		}

		class ProcessTaskTemplateFindBoxListProvider : FindBoxListProvider
		{
			internal ProcessTaskTemplateFindBoxListProvider(IBusinessObjectCollection collection)
				: base(collection)
			{
			}

			protected override void AddCodeEqualsFilter(ZQuery query, string code)
			{
				if (!string.IsNullOrEmpty(code))
				{
					query.AddToFilter(ProcessTaskTemplateSchema.P0_Name, code);
				}
				else
				{
					query.IsNoResultQuery = true;
				}
			}
		}

		#endregion
	}
}
