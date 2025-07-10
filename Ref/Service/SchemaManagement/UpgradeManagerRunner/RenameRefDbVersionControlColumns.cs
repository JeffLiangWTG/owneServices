using System.Collections.Generic;
using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RenameRefDbVersionControlColumns : RenameColumnsDataTransformation
	{
		public RenameRefDbVersionControlColumns(int version) : base(version) { }

		protected override IEnumerable<RenameColumnObject> GetColumnsToRename()
		{
			return new List<RenameColumnObject>()
			{
				new RenameColumnObject("ParentPK","RVC_ParentPK"),
				new RenameColumnObject("ParentCode","RVC_ParentCode"),
				new RenameColumnObject("LastUpdatedUTC","RVC_LastUpdatedUTC"),
				new RenameColumnObject("Deleted","RVC_Deleted"),
				new RenameColumnObject("CreatedTimeUTC","RVC_CreatedTimeUTC"),
				new RenameColumnObject("LastEditedUser","RVC_LastEditedUser")
			};
		}

		public override void ExecuteCommandBeforeRename(IDbTransaction trans)
		{
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.CommandText = Helper.TurnOffRefCusTariffViewSchemaBinding();
				cmd.Transaction = trans;

				cmd.ExecuteNonQuery();
			}
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.CommandText = Helper.TurnOffRefCusTariffRelationshipViewSchemaBinding();
				cmd.Transaction = trans;

				cmd.ExecuteNonQuery();
			}
		}

		protected override string GetTableName()
		{
			return "RefDbVersionControl";
		}

		static class Helper
		{
			internal static string TurnOffRefCusTariffRelationshipViewSchemaBinding()
			{
				return @"ALTER VIEW [dbo].[RefCusTariffRelationshipView]
AS
SELECT [ZZH_PK], [ZZH_ZZ1_Tariff], [ZZH_ZZI_TariffType], [ZZH_TariffCode], ParentPK, LastUpdatedUTC
FROM [dbo].[RefCusTariffRelationship]
JOIN [dbo].[RefDbVersionControl] ON ZZH_ZZ1_Tariff = ParentPK AND ParentCode = 'ZZ1';
";
			}

			internal static string TurnOffRefCusTariffViewSchemaBinding()
			{
				return @"ALTER VIEW [dbo].[RefCusTariffView]
AS
SELECT [ZZ1_PK], [ZZ1_ZZI_TariffType], [ZZ1_TariffCode], [ZZ1_IAMUnique], [ZZ1_Description], [ZZ1_StartDate], [ZZ1_EndDate],
	[ZZ1_ZZF_NKTaxOrFeeCode], [ZZ1_ZZZ_NKDataGrouping], [ZZ1_CompositeKeyOnZZ5], ParentPK, LastUpdatedUTC, Deleted
FROM [dbo].[RefCusTariff]
JOIN [dbo].[RefDbVersionControl] ON ZZ1_PK = ParentPK AND ParentCode = 'ZZ1'
";
			}
		}
	}
}
