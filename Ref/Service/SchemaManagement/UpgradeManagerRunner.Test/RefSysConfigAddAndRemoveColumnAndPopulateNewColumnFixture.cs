using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class RefSysConfigAddAndRemoveColumnAndPopulateNewColumnFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = "SELECT count(*) FROM RefSysConfig WHERE ZRC_ZRT_NKConfigCode = 'TEST'";
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, sql));
			sql = "SELECT count(*) FROM RefSysConfigHistory WHERE ZRC_ZRT_NKConfigCode = 'TEST'";
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, sql));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new RefSysConfigAddAndRemoveColumnAndPopulateNewColumnTask(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"
Alter Table dbo.RefSysConfig Add ZRC_ZRT_ConfigCode UNIQUEIDENTIFIER;
";
				cmd.ExecuteNonQuery();
			}

			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"
Declare @configType uniqueidentifier,@config uniqueidentifier
Select @configType = newid(), @config = newid()
insert into RefSysConfigType(ZRT_PK,ZRT_ConfigCode,ZRT_Description,ZRT_LongDescription)
values (@configType,'TEST','TEST Description','Test Long Description'),
(newid(),'TST','TEST Description','Test Long Description');

insert into RefSysConfig(ZRC_PK,ZRC_ZRT_ConfigCode,ZRC_StartDate,ZRC_EndDate,[ZRC_DecimalValue],[ZRC_StringValue],[ZRC_BitValue],ZRC_ZRT_NKConfigCode)
values(@config,@configType,'1900-01-01','2020-01-01',1,'',0,'TST');

update RefSysConfig set ZRC_DecimalValue=2,ZRC_StartDate='1901-01-01' where ZRC_PK = @config;
";
				cmd.ExecuteNonQuery();
			}
		}
	}
}
