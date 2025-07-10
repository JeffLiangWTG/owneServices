using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class WorkflowFilterStripsHelperCustoms : WorkflowFilterStripsHelper
	{
		public WorkflowFilterStripsHelperCustoms(Type businessObjectType, ZString templateCode, BusinessObjectFactory factory)
			: base(businessObjectType, templateCode, factory)
		{
		}

		protected override SchemaGuidColumn GetPrimaryKeyColumn() => JobDeclarationSchema.JE_ComputedParent;

		protected override WorkflowModuleFilter GetNewFilter(string description, WorkflowModuleFilterTypes filterType)
		{
			return new WorkflowModuleFilterCustoms(description, BusinessObjectType, filterType, TemplateCode);
		}

		protected override WorkflowModuleTextFilter GetNewTextFilter(string description, GetTextQuery queryDelegate, IList list)
		{
			return new WorkflowModuleTextFilterCustoms(description, queryDelegate, list, BusinessObjectType, TemplateCode);
		}

		public static ZDBOnlyQuery GetDeclarationAndShipmentQuery(ZDBOnlySubQuery milestoneSubQuery)
		{
			milestoneSubQuery.AddToFilter(ProcessTasksSchema.P9_ParentTableCode,
				new[]
				{
					JobDeclarationSchema.Constants.Prefix,
					JobShipmentSchema.Constants.Prefix
				});

			var query = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			query.AddSubQuery(JobDeclarationSchema.JE_ComputedParent, milestoneSubQuery, JoinCondition.And);

			return query;
		}
	}
}
