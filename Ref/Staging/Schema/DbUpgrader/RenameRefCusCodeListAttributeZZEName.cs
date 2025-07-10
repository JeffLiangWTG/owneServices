using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class RenameRefCusCodeListAttributeZZEName : RenameColumnsDataTransformation
	{
		public RenameRefCusCodeListAttributeZZEName(int version) : base(version) { }

		protected override IEnumerable<RenameColumnObject> GetColumnsToRename()
		{
			return new List<RenameColumnObject>()
			{
				new RenameColumnObject("ZZE_Name", "ZZE_ZXE_NKName"),
			};
		}

		protected override string GetTableName()
		{
			return "RefCusCodeListAttribute";
		}
	}
}
