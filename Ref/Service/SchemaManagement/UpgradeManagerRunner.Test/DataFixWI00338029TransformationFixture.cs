using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixWI00338029TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"select count(1) from RefCusProcedure where ZZ6_IntoWarehouse not in ('Y','N','I');";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"select count(1) from RefCusProcedure where ZZ6_OutOfWarehouse not in ('Y','N','I');";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixWI00338029Transformation(133);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
IF EXISTS(SELECT * FROM sys.check_constraints WHERE name='CK_RefCusProcedure_ZZ6_IntoWarehouse' AND parent_object_id = OBJECT_ID(N'[dbo].[RefCusProcedure]'))
ALTER TABLE [dbo].[RefCusProcedure] DROP CONSTRAINT [CK_RefCusProcedure_ZZ6_IntoWarehouse];

IF EXISTS(SELECT * FROM sys.check_constraints WHERE name='CK_RefCusProcedure_ZZ6_OutOfWarehouse' AND parent_object_id = OBJECT_ID(N'[dbo].[RefCusProcedure]'))
ALTER TABLE [dbo].[RefCusProcedure] DROP CONSTRAINT [CK_RefCusProcedure_ZZ6_OutOfWarehouse];

INSERT RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES('B9AB8BED-0789-4852-836C-67912F96BE6B', 'ZA', 'South Africa');

INSERT INTO RefCusProcedure
(ZZ6_PK,ZZ6_Category,ZZ6_ProcedureCode,ZZ6_PreviousProcedureCode,ZZ6_Concession,ZZ6_Description,ZZ6_ZZZ_NKDataGrouping
,ZZ6_ShipmentType,ZZ6_CalculateDuty,ZZ6_Group,ZZ6_LandedCost,ZZ6_IntoWarehouse,ZZ6_OutOfWarehouse,ZZ6_StartDate,ZZ6_EndDate)
VALUES ('203644BE-8557-4CB8-9BC9-82DE2F781989','A','1','10','4','ZZZ','ZA','EXW',1,'',0,'1','Y','1900-01-01','2079-06-06 23:59:00');

INSERT INTO RefCusProcedure
(ZZ6_PK,ZZ6_Category,ZZ6_ProcedureCode,ZZ6_PreviousProcedureCode,ZZ6_Concession,ZZ6_Description,ZZ6_ZZZ_NKDataGrouping
,ZZ6_ShipmentType,ZZ6_CalculateDuty,ZZ6_Group,ZZ6_LandedCost,ZZ6_IntoWarehouse,ZZ6_OutOfWarehouse,ZZ6_StartDate,ZZ6_EndDate)
VALUES ('1BBB5305-E596-4DBE-A1E3-6360B2C2F063','B','2','20','4','ZZZ','ZA','EXW',1,'',0,'N','1','1900-01-01','2079-06-06 23:59:00');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
