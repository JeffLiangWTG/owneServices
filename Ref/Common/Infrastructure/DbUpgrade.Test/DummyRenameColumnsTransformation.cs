using System.Collections.Generic;
using System.Data;

namespace CargoWise.RefDbRepo.Common.DbUpgrade.Test
{
	public class DummyRenameColumnsTransformation : RenameColumnsDataTransformation
	{
		public DummyRenameColumnsTransformation(int version) : base(version)
		{
		}

		protected override IEnumerable<RenameColumnObject> GetColumnsToRename()
		{
			return new List<RenameColumnObject>()
				{
					new RenameColumnObject("DMY_Old1", "DMY_New1"),
					new RenameColumnObject("DMY_Old2", "DMY_New2"),
					new RenameColumnObject("DMY_Old3", "DMY_New3")
				};
		}

		public override void ExecuteCommandBeforeRename(IDbTransaction trans)
		{
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.CommandText = "SELECT DMY_Old1, DMY_Old2, DMY_Old3 FROM DummyRenameObject";
				cmd.Transaction = trans;

				cmd.ExecuteNonQuery();
			}
		}

		public override void ExecuteCommandAfterRename(IDbTransaction trans)
		{
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.CommandText = "SELECT DMY_New1, DMY_New2, DMY_New3 FROM DummyRenameObject";
				cmd.Transaction = trans;

				cmd.ExecuteNonQuery();
			}
		}

		protected override string GetTableName()
		{
			return "DummyRenameObject";
		}
	}
}
