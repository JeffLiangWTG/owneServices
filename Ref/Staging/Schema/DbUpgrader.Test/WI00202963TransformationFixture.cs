using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class WI00202963TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusTariff WHERE ZZ1_ZZI_ZZZ_NKDataGrouping = ''";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 0);
				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusTariff WHERE ZZ1_ZZI_ZZZ_NKDataGrouping = 'ZA'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 2);
				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusTariff WHERE ZZ1_ZZI_ZZZ_NKDataGrouping = 'EUN'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new WI00202963Transformation(47);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_ZZZ_NKDataGrouping, ZZI_Description)
VALUES
	('B8DA230C-BDDE-4B49-A17B-C42055D3769D','1P1', 'ZA','1P1'),
	('583E9001-2824-49DA-AB9D-543835B943E8','12A', 'ZA','12A');

INSERT INTO RefCusTariff(ZZ1_PK, ZZ1_ZZI_NKTariffType, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_TariffCode)
VALUES
	('F7F679C0-B755-430E-B1EE-2547F64CE2BE', '1P1', 'ZA', '1900-01-01', '2079-06-06', 'T1'),
	('2AAB6B03-1A78-42AF-90F5-7309F08DF83F', '1P1', 'EUN', '1900-01-01', '2079-06-06', 'T2'),
	('18DD1A85-CA12-4E8B-A1DC-CF25F186F031', '12A', 'ZA', '1900-01-01', '2079-06-06', 'T3')
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
