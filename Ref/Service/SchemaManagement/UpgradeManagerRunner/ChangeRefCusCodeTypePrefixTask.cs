using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class ChangeRefCusCodeTypePrefixTask : DataTransformation, IDataTransformationTask
	{
		public ChangeRefCusCodeTypePrefixTask(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = N'ZZK_PK' AND object_id = object_id('dbo.RefCusCodeType'))
BEGIN
	ALTER TABLE RefCusCodeType
	ADD ZZK_PK UNIQUEIDENTIFIER,
		ZZK_CodeType VARCHAR(5),
		ZZK_Description VARCHAR(500);

	ALTER TABLE RefCusCodeList
	ADD ZZD_ZZK_NKCodeType VARCHAR(5);

	DISABLE TRIGGER RefCusCodeType_Version_Update ON REfCusCodeType;
	DISABLE TRIGGER RefCusCodeList_Version_Update ON RefCusCodeList

	EXEC ('UPDATE RefCusCodeType SET ZZK_PK = ZZN_PK, ZZK_CodeType = ZZN_CodeType, ZZK_Description = ZZN_Description');
	EXEC ('UPDATE RefCusCodeList SET ZZD_ZZK_NKCodeType = ZZD_ZZN_NKCodeType');

	ENABLE TRIGGER RefCusCodeType_Version_Update ON REfCusCodeType;
	ENABLE TRIGGER RefCusCodeList_Version_Update ON RefCusCodeList
END
";
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
