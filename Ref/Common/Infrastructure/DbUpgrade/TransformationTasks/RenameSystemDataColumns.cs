using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	public class RenameSystemDataColumns : RenameColumnsDataTransformation
	{
		public RenameSystemDataColumns(int version) : base(version)
		{
		}

		protected override IEnumerable<RenameColumnObject> GetColumnsToRename()
		{
			return new List<RenameColumnObject>()
			{
				new RenameColumnObject("Id","SD_PK"),
				new RenameColumnObject("Value","SD_Value"),
				new RenameColumnObject("Name","SD_Name"),
			};
		}

		protected override string GetTableName() => "SystemData";
	}
}
