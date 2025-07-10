using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class WI00616785TransformationFixture : TransformationFixture
	{
		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO DataProcessingResult (DPR_PK, DPR_SubSource, DPR_PublicationTime, DPR_ParentTableCode, DPR_ParentPK, DPR_ExpirableAncestorPK, DPR_Status) VALUES
('A417E220-E5F0-447B-B3D1-0AED248BAB4F', 'EUN Tariffs', '2022-01-28 10:27:00.0000000', 'ZZ1', 'D501138F-4C5B-42C1-82DA-6E9B09E5D680', null, 'QUE'),
('9E1B85F9-50F1-461C-AE73-A74B1AA72F8B', 'EUN Tariffs', '2021-12-28 22:27:00.0000000', 'ZZ1', 'D501138F-4C5B-42C1-82DA-6E9B09E5D680', 'C2A66419-CD2A-46B8-8FA6-8BB5570ADDA2', 'QUE'),
('3F1AE638-D283-4C27-8B24-28850E5C2981', 'Import EUN', '2022-01-28 22:27:00.0000000', 'ZZ1', '856E1329-A267-4752-9D33-D5A1D1861196', '46CA4F96-FD86-4428-8047-8CC246D0D041', 'QUE'),
('0DF8056C-622F-4974-A719-9C0791EFC32A', 'EUN Tariffs 01', '2022-01-28 23:27:00.0000000', 'ZZ1', '0DB8135C-9693-4AB5-9455-E5DCAABA83FD', '74DF5E7C-3ADC-4704-9E95-D6F31328C1AD', 'QUE');
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
				cmd.CommandText = @"SELECT COUNT(1) FROM DataProcessingResult WHERE DPR_SubSource = 'EUN Tariffs'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"SELECT COUNT(1) FROM DataProcessingResult WHERE DPR_SubSource = 'Import EUN'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"SELECT COUNT(1) FROM DataProcessingResult WHERE DPR_SubSource = 'EUN Tariffs 01'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new WI00616785Transformation(0);
		}
	}
}
