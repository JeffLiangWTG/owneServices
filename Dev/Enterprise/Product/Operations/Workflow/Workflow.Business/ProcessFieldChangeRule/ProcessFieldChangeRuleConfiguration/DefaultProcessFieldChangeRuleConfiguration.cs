using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	public class DefaultProcessFieldChangeRuleConfiguration : IProcessFieldChangeRuleConfiguration
	{
		public DefaultProcessFieldChangeRuleConfiguration(ZString workflowType)
		{
			this.workflowType = workflowType;
		}

		public IEnumerable<ITableSchema> Schemas
		{
			get
			{
				if (WorkflowDescriptorNotSafe != null)
				{
					return new ITableSchema[] { BusinessObjectFactory.GetTableSchemaFromType(WorkflowDescriptorNotSafe.WorkflowProviderType) };
				}
				return System.Array.Empty<ITableSchema>();
			}
		}

		public HashSet<ZString> BlacklistedColumns => new HashSet<ZString>();

		public ZString GetFieldColumnDescription(SchemaColumn fieldColumn) => ProcessFieldChangeRuleColumnDescription.DefaultDescription(fieldColumn);

		WorkflowDescriptor WorkflowDescriptorNotSafe
		{
			get
			{
				if (workflowDescriptor == null && WorkflowDescriptors.Instance.TryGetValue(workflowType, out WorkflowDescriptor value))
				{
					workflowDescriptor = value;
				}
				return workflowDescriptor;
			}
		}
		WorkflowDescriptor workflowDescriptor;
		readonly ZString workflowType;
	}
}
