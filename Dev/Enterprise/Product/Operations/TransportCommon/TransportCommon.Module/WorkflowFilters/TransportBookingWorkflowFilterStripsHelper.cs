using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportCommon.Module
{
	public class TransportWorkflowFilterStripsHelper : WorkflowFilterStripsHelper
	{
		public TransportWorkflowFilterStripsHelper(Type businessObjectType, ZString templateCode, BusinessObjectFactory factory, SchemaColumn schemaColumnOverride = null)
			: base(businessObjectType, templateCode, factory)
		{
			this.schemaColumnOverride = schemaColumnOverride;
		}

		public TransportWorkflowFilterStripsHelper(Type businessObjectType, ZString templateCode, BusinessObjectFactory factory, SchemaColumn schemaColumnOverride, params ZDBOnlySubQuery[] relatedParentJoiningQueries)
			: this(businessObjectType, templateCode, factory, schemaColumnOverride)
		{
			RelatedParentJoiningQueries.AddRange(relatedParentJoiningQueries);
		}

		#region Filters

		protected override WorkflowModuleFilter GetNewFilter(string description, WorkflowModuleFilterTypes filterType)
		{
			return new TransportBookingWorkflowModuleDateFilter(description, BusinessObjectType, filterType, TemplateCode, schemaColumnOverride);
		}

		protected override WorkflowModuleTextFilter GetNewTextFilter(string description, GetTextQuery queryDelegate, IList list)
		{
			return new TransportBookingWorkflowModuleTextFilter(description, queryDelegate, list, BusinessObjectType, TemplateCode, schemaColumnOverride);
		}

		#endregion

		readonly SchemaColumn schemaColumnOverride;
	}
}
