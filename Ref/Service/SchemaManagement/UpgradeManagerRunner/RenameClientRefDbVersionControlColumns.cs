using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RenameClientRefDbVersionControlColumns : RenameColumnsDataTransformation
	{
		public RenameClientRefDbVersionControlColumns(int version) : base(version) { }

		protected override IEnumerable<RenameColumnObject> GetColumnsToRename()
		{
			return new List<RenameColumnObject>()
			{
				new RenameColumnObject("Id","CVC_PK"),
				new RenameColumnObject("DataSet","CVC_DataSet"),
				new RenameColumnObject("ClientId","CVC_ClientId"),
				new RenameColumnObject("DataSetTimestamp","CVC_DataSetTimestamp"),
				new RenameColumnObject("DataSetCheckpoint","CVC_DataSetCheckpoint"),
				new RenameColumnObject("SystemType","CVC_SystemType"),
				new RenameColumnObject("LastUpdatedTimeUTC","CVC_LastUpdatedTimeUTC")
			};
		}

		protected override string GetTableName()
		{
			return "ClientRefDbVersionControl";
		}
	}
}
