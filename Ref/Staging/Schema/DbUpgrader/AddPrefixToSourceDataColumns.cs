using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class AddPrefixToSourceDataColumns : RenameColumnsDataTransformation
	{
		public AddPrefixToSourceDataColumns(int version) : base(version) { }

		protected override IEnumerable<RenameColumnObject> GetColumnsToRename()
		{
			return new List<RenameColumnObject>()
			{
				new RenameColumnObject("Id","SDA_PK"),
				new RenameColumnObject("Source","SDA_Source"),
				new RenameColumnObject("Filename","SDA_Filename"),
				new RenameColumnObject("Filetype","SDA_Filetype"),
				new RenameColumnObject("Content","SDA_Content"),
				new RenameColumnObject("ContentText","SDA_ContentText"),
				new RenameColumnObject("Status","SDA_Status"),
				new RenameColumnObject("CreatedTime","SDA_CreatedTime"),
				new RenameColumnObject("ContentType","SDA_ContentType"),
				new RenameColumnObject("SourceTime","SDA_SourceTime"),
				new RenameColumnObject("SubSource","SDA_SubSource"),
				new RenameColumnObject("NotProcessedUntil","SDA_NotProcessedUntil")
			};
		}

		protected override string GetTableName()
		{
			return "SourceData";
		}
	}
}
