using System;

namespace CargoWise.RefDbRepo.EdmxGen
{
	public class LogicalRelationship : ILogicalRelationship
	{
		public string Table { get; set; }
		public string ReferencedTable { get; set; }
		public string Column { get; set; }
		public string ReferencedColumn { get; set; }
		public bool IsNullable { get; set; }

		public string GetAssociationName()
		{
			var result = FormattableString.Invariant($"LFK_{Table}_{ReferencedTable}");
			return result;
		}
	}
}
