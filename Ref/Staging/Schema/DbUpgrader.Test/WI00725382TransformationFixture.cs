using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class WI00725382TransformationFixture : TransformationFixture
	{
		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO dbo.SourceData (SDA_PK, SDA_Source, SDA_FileType, SDA_ContentText, SDA_Status, SDA_SubSource, SDA_ContentType, SDA_SourceTime)
VALUES
(NEWID(), 'ZAA', 'TXT', '', 'ERR', 'UNKNOWN', 'PRO', '2018-11-12'),
(NEWID(), 'ZAB', 'TXT', '', 'ERR', 'UNKNOWN', 'PRO', '2018-11-12'),
(NEWID(), 'ZAC', 'TXT', '', 'ERR', 'UNKNOWN', 'PRO', '2018-11-12'),
(NEWID(), 'ZAD', 'TXT', '', 'ERR', 'UNKNOWN', 'PRO', '2018-11-12'),
(NEWID(), 'ZAE', 'TXT', '', 'ERR', 'UNKNOWN', 'PRO', '2018-11-12'),
(NEWID(), 'ZAF', 'TXT', '', 'QUE', 'ZA Tariffs', 'PRO', '2018-11-12')
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}

		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = "SELECT COUNT(1) FROM dbo.SourceData";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = "SELECT * FROM dbo.SourceData";
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						Assert.That(reader["SDA_Source"].ToString(), Is.EqualTo("ZAF"));
						Assert.That(reader["SDA_FileType"].ToString(), Is.EqualTo("TXT"));
						Assert.That(reader["SDA_ContentText"].ToString(), Is.EqualTo(string.Empty));
						Assert.That(reader["SDA_Status"].ToString(), Is.EqualTo("QUE"));
						Assert.That(reader["SDA_SubSource"].ToString(), Is.EqualTo("ZA Tariffs"));
						Assert.That(reader["SDA_ContentType"].ToString(), Is.EqualTo("PRO"));
						Assert.That((DateTime)reader["SDA_SourceTime"], Is.EqualTo(new DateTime(2018, 11, 12)));
					}
				}
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new WI00725382Transformation(0);
		}
	}
}
