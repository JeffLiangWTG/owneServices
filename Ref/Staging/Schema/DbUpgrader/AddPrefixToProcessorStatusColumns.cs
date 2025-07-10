using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class AddPrefixToProcessorStatusColumns : RenameColumnsDataTransformation
	{
		public AddPrefixToProcessorStatusColumns(int version) : base(version) { }

		protected override IEnumerable<RenameColumnObject> GetColumnsToRename()
		{
			return new List<RenameColumnObject>()
			{
				new RenameColumnObject("Id","PRC_PK"),
				new RenameColumnObject("Processor","PRC_Processor"),
				new RenameColumnObject("Country","PRC_Country"),
				new RenameColumnObject("LastRunTime","PRC_LastRunTime"),
				new RenameColumnObject("LastSuccessRunTime","PRC_LastSuccessRunTime"),
				new RenameColumnObject("LastSuccessRecordUpdatedCount","PRC_LastSuccessRecordUpdatedCount"),
				new RenameColumnObject("LastDataSetUpdatedTime","PRC_LastDataSetUpdatedTime")
			};
		}

		protected override string GetTableName()
		{
			return "ProcessorStatus";
		}
	}
}
