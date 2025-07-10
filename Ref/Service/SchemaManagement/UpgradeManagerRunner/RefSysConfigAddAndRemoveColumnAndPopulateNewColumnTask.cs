using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefSysConfigAddAndRemoveColumnAndPopulateNewColumnTask : DataTransformation, IDataTransformationTask
	{
		public RefSysConfigAddAndRemoveColumnAndPopulateNewColumnTask(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = @"IF not EXISTS(SELECT 1 FROM sys.columns 
		  WHERE Name = N'ZRC_ZRT_NKConfigCode'
		  AND Object_ID = Object_ID(N'dbo.RefSysConfig'))
BEGIN
	Alter TABLE dbo.RefSysConfig add ZRC_ZRT_NKConfigCode varchar(10);
END";
				cmd.ExecuteNonQuery();
			}

			DbHelper.SetSystemVersioningOff(trans, "RefSysConfig");

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = @"update history set ZRC_ZRT_NKConfigCode = configType.ZRT_ConfigCode
	from refsysconfighistory history
	join refsysconfigtype configType on history.ZRC_ZRT_ConfigCode = configType.ZRT_PK;

	update config set ZRC_ZRT_NKConfigCode = configType.ZRT_ConfigCode
	from refsysconfig config
	join refsysconfigtype configType on config.ZRC_ZRT_ConfigCode = configType.ZRT_PK; ";
				cmd.ExecuteNonQuery();
			}

			DbHelper.SetSystemVersioningOn(trans, "RefSysConfig");
		}
	}
}
