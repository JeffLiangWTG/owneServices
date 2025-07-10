using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class DeleteCodeTypesWithEmptyDataGroupingFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT COUNT(1) FROM RefCusCodeType WHERE ZZK_ZZZ_NKDataGrouping = ''";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DeleteCodeTypesWithEmptyDataGrouping(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
IF EXISTS (SELECT * FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[dbo].[CK_RefCusCodeType_ZZK_ZZZ_NKDataGrouping]') AND parent_object_id = OBJECT_ID(N'[dbo].[RefCusCodeType]'))
ALTER TABLE [dbo].[RefCusCodeType] DROP CONSTRAINT [CK_RefCusCodeType_ZZK_ZZZ_NKDataGrouping];

INSERT INTO RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'TEST', 'X', ''),
(NEWID(), 'TEST', 'X', 'ZA');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
