using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class WI00473541TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT COUNT(1) FROM DataSourceInformation WHERE DSI_SubSource = 'EUN Tariffs chapters 70-79'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 0);
				cmd.CommandText = @"SELECT COUNT(1) FROM DataSourceInformation WHERE DSI_SubSource = 'Import EUN Tariffs chapters 70-79'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"SELECT COUNT(1) FROM DataSourceInformation WHERE DSI_SubSource = 'Export EUN Tariffs chapters 70-79'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"SELECT COUNT(1) FROM DataProcessingResult WHERE DPR_PK = '5831207A-5599-456D-A69E-30DAB06A192A'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 0);
				cmd.CommandText = @"SELECT COUNT(1) FROM DataProcessingResult WHERE DPR_SubSource = 'EUN Tariffs chapters 70-79'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 0);
				cmd.CommandText = @"SELECT COUNT(1) FROM DataProcessingResult WHERE DPR_SubSource = 'Import EUN Tariffs chapters 70-79'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new WI00473541Transformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO DataSourceInformation (DSI_PK,DSI_SubSource,DSI_EnableAutoExpiration) VALUES
('3DEB7135-41A6-412B-AABE-3DCB6D1C8FC4','EUN Tariffs chapters 70-79','1');
INSERT INTO DataProcessingResult (DPR_PK,DPR_SubSource,DPR_PublicationTime,DPR_ParentTableCode,DPR_ParentPK,DPR_Status) VALUES
('99B75F4F-9780-41BE-AEA2-44FF49198B79','EUN Tariffs chapters 70-79','2022-01-28 22:27:00.0000000','ZZ1','A18218D2-BEB2-4C59-96A6-1DEC8F24399F','QUE'),
('5831207A-5599-456D-A69E-30DAB06A192A','EUN Tariffs chapters 10-19','2021-12-28 22:27:00.0000000','ZZ1','B4634D24-508F-4A5E-8CDB-6220BB8AE55D','QUE'),
('FDA30FC9-CCCE-49EA-8DCB-7F1BA9FFEB1F','Import EUN Tariffs chapters 10-19','2022-01-28 22:27:00.0000000','ZZ1','B4634D24-508F-4A5E-8CDB-6220BB8AE55D','QUE');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
