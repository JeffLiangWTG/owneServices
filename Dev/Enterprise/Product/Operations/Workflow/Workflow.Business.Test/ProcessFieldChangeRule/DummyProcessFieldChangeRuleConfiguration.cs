using System.Collections.Generic;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business.Test
{
	public class DummyProcessFieldChangeRuleConfiguration : IProcessFieldChangeRuleConfiguration
	{
		public IEnumerable<ITableSchema> Schemas => schemas;

		readonly IEnumerable<ITableSchema> schemas = new ITableSchema[]
		{
			DummyBizoSchema.Instance,
			JobShipmentSchema.Instance,
			StmALogSchema.Instance
		};

		public HashSet<ZString> BlacklistedColumns => blacklistedColumns;

		readonly HashSet<ZString> blacklistedColumns = new HashSet<ZString>()
		{
			DummyBizoSchema.Z0_Code.Name
		};

		public ZString GetFieldColumnDescription(SchemaColumn fieldColumn)
		{
			if (fieldColumn == DummyBizoSchema.Z0_Date)
			{
				return "My Date Column";
			}
			else if (fieldColumn == DummyBizoSchema.Z0_AnotherDate)
			{
				return "Another - Date";
			}
			return ProcessFieldChangeRuleColumnDescription.DefaultDescription(fieldColumn);
		}
	}
}
