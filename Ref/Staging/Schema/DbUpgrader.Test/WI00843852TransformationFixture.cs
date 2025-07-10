using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class WI00843852TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = "SELECT COUNT(1) FROM dbo.DataProcessingInformation";
				Assert.That(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), Is.EqualTo(1));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new WI00843852Transformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
ALTER TABLE dbo.DataProcessingInformation NOCHECK CONSTRAINT CK_DataProcessingInformation_DPI_ParentPk

INSERT INTO DataProcessingInformation (DPI_ID, DPI_Status, DPI_Message, DPI_SourceId, DPI_ParentTableCode, DPI_ParentPk)
VALUES
(NEWID(), 'ERR', '', '5D6F02B7-F2DE-46EE-AAB6-4D996FBFBF5A', 'ZZ1', '00000000-0000-0000-0000-000000000000'),
(NEWID(), 'ERR', '', '48F85121-E512-427C-8BF0-10EEB58AE473', 'ZZ1', '00000000-0000-0000-0000-000000000000'),
(NEWID(), 'ERR', '', 'A88B6EBD-7197-43FB-87A6-DBDD261C2CF9', 'ZZ1', '00000000-0000-0000-0000-000000000000'),
(NEWID(), 'ERR', '', '87CEEF77-847A-4B66-BBC1-B0E84F35A240', 'ZZ1', '00000000-0000-0000-0000-000000000000'),
(NEWID(), 'ERR', '', '702D2DC2-9942-4535-8FDB-72A6225251EE', 'ZZ1', '00000000-0000-0000-0000-000000000000'),
(NEWID(), 'ERR', '', 'A02469ED-6C53-4026-8327-5D75B22DC016', 'ZZ1', '00000000-0000-0000-0000-000000000000'),
(NEWID(), 'QUE', '', '1EEBB2F5-57E3-4BAC-A058-1C72B3BE216D', 'ZZ1', 'F8A91910-BA69-4D7A-8964-A25BAEB1AEC5');

ALTER TABLE dbo.DataProcessingInformation CHECK CONSTRAINT CK_DataProcessingInformation_DPI_ParentPk
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
