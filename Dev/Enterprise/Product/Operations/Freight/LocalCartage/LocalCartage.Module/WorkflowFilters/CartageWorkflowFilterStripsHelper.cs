using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.LocalCartage.Module
{
	public class CartageWorkflowFilterStripsHelper : WorkflowFilterStripsHelper
	{
		public CartageWorkflowFilterStripsHelper(Type businessObjectType, ZString templateCode, BusinessObjectFactory factory, SchemaColumn schemaColumnOverride = null)
			: base(businessObjectType, templateCode, factory)
		{
			this.schemaColumnOverride = schemaColumnOverride;
		}

		public CartageWorkflowFilterStripsHelper(Type businessObjectType, ZString templateCode, BusinessObjectFactory factory, SchemaColumn schemaColumnOverride, params ZDBOnlySubQuery[] relatedParentJoiningQueries)
			: this(businessObjectType, templateCode, factory, schemaColumnOverride)
		{
			RelatedParentJoiningQueries.AddRange(relatedParentJoiningQueries);
		}

		protected override WorkflowModuleFilter GetNewFilter(string description, WorkflowModuleFilterTypes filterType)
		{
			return new CartageWorkflowModuleDateFilter(description, BusinessObjectType, filterType, TemplateCode, schemaColumnOverride);
		}

		protected override WorkflowModuleTextFilter GetNewTextFilter(string description, GetTextQuery queryDelegate, IList list)
		{
			return new CartageWorkflowModuleTextFilter(description, queryDelegate, list, BusinessObjectType, TemplateCode, schemaColumnOverride);
		}

		readonly SchemaColumn schemaColumnOverride;
	}
}
