using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Module
{
	public class WorkflowFilterStripsHelperWithRoutingSupport : WorkflowFilterStripsHelper
	{
		public WorkflowFilterStripsHelperWithRoutingSupport(Type businessObjectType, ZString templateCode, SchemaColumn schemaColumnOverride, ZString parentTableCodeOverride, BusinessObjectFactory factory, params ZDBOnlySubQuery[] relatedParentJoiningQueries)
			: base(businessObjectType, templateCode, factory)
		{
			this.schemaColumnOverride = schemaColumnOverride;
			this.parentTableCodeOverride = parentTableCodeOverride;
			RelatedParentJoiningQueries.AddRange(relatedParentJoiningQueries);
		}

		public WorkflowFilterStripsHelperWithRoutingSupport(Type businessObjectType, ZString templateCode, BusinessObjectFactory factory, bool shouldFilterByCompanyForAnyOpenTask)
			: base(businessObjectType, templateCode, factory, shouldFilterByCompanyForAnyOpenTask)
		{
		}

		public WorkflowFilterStripsHelperWithRoutingSupport(Type businessObjectType, ZString templateCode, BusinessObjectFactory factory, params ZDBOnlySubQuery[] relatedParentJoiningQueries)
			: this(businessObjectType, templateCode, null, ZString.Empty, factory, relatedParentJoiningQueries)
		{
		}

		readonly SchemaColumn schemaColumnOverride;
		readonly ZString parentTableCodeOverride;

		protected override WorkflowModuleFilter GetNewFilter(string description, WorkflowModuleFilterTypes filterType)
		{
			return new WorkflowModuleFilterWithRoutingSupport(description, BusinessObjectType, filterType, parentTableCodeOverride, schemaColumnOverride, TemplateCode);
		}

		protected override WorkflowModuleTextFilter GetNewTextFilter(string description, GetTextQuery queryDelegate, IList list)
		{
			return new WorkflowModuleTextFilterWithRoutingSupport(description, queryDelegate, list, BusinessObjectType, parentTableCodeOverride, schemaColumnOverride, TemplateCode);
		}
	}
}
