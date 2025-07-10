using System.Data;
using CargoWise.Common;

namespace CargoWise.RefDataRepo.Ent.Client.Exporter
{
	class DataRowAdapter : IDataRow
	{
		public DataRowAdapter(DataRow dataRow)
		{
			Argument.NotNull(dataRow, nameof(dataRow));
			this.dataRow = dataRow;
		}
		readonly DataRow dataRow;

		public object this[string propertyName] => dataRow[propertyName];
	}
}
