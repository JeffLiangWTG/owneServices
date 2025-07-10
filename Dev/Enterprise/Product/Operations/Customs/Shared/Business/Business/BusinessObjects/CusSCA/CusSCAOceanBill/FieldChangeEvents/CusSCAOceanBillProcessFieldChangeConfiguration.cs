
using System.Collections.Generic;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	sealed class CusSCAOceanBillProcessFieldChangeConfiguration : IProcessFieldChangeRuleConfiguration
	{
		public IEnumerable<ITableSchema> Schemas => schemas;
		readonly IEnumerable<ITableSchema> schemas = new ITableSchema[] { CusSCAOceanBillSchema.Instance };

		public HashSet<ZString> BlacklistedColumns => blacklistedColumns;
		readonly HashSet<ZString> blacklistedColumns = new HashSet<ZString>() { CusSCAOceanBillSchema.CB_OceanBill.Name };

		public ZString GetFieldColumnDescription(SchemaColumn fieldColumn) => ProcessFieldChangeRuleColumnDescription.DefaultDescription(fieldColumn);
	}
}
