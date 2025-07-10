using System.Collections.Generic;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Workflow.Integration
{
	public interface IProcessFieldChangeRuleConfiguration
	{
		IEnumerable<ITableSchema> Schemas { get; }

		HashSet<ZString> BlacklistedColumns { get; }

		ZString GetFieldColumnDescription(SchemaColumn fieldColumn);
	}
}
