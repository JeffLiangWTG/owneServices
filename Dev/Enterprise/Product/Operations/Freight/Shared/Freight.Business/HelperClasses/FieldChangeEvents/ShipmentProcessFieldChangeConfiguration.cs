using System.Collections.Generic;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ShipmentProcessFieldChangeConfiguration : IProcessFieldChangeRuleConfiguration
	{
		public IEnumerable<ITableSchema> Schemas => schemas;
		readonly IEnumerable<ITableSchema> schemas = new ITableSchema[] { JobShipmentSchema.Instance };

		public HashSet<ZString> BlacklistedColumns => blacklistedColumns;
		readonly HashSet<ZString> blacklistedColumns = new HashSet<ZString>() { };

		public ZString GetFieldColumnDescription(SchemaColumn fieldColumn) => ProcessFieldChangeRuleColumnDescription.DefaultDescription(fieldColumn);
	}
}
