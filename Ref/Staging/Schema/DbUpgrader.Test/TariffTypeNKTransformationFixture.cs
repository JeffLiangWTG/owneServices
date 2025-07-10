using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class TariffTypeNKTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusTariff WHERE ZZ1_ZZI_NKTariffType <> ''";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusTariffRelationship WHERE ZZH_ZZI_NKTariffType <> ''";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new TariffTypeNKTransformationTask(14);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
ALTER TABLE RefCusTariff
ADD ZZ1_ZZI_TariffType uniqueidentifier

ALTER TABLE RefCusTariffRelationship
ADD ZZH_ZZI_TariffType uniqueidentifier
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();

				cmd.CommandText = @"
INSERT INTO RefCusTariffType (ZZI_PK,ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping)
VALUES
	('B8DA230C-BDDE-4B49-A17B-C42055D3769D','1P1', '1P1', 'ZA'),
	('583E9001-2824-49DA-AB9D-543835B943E8','12A', '12A', 'ZA')

INSERT INTO RefCusTariff (ZZ1_PK,ZZ1_ZZI_NKTariffType,ZZ1_ZZI_TariffType, ZZ1_StartDate, ZZ1_EndDate, ZZ1_TariffCode)
 VALUES ('15F534A1-E364-46A3-8ADC-952D685EDA1A','X','583E9001-2824-49DA-AB9D-543835B943E8', '1900-01-01', '2079-06-06', 'Test');

INSERT INTO RefCusTariffRelationship (ZZH_PK,ZZH_ZZ1_Tariff,ZZH_ZZI_NKTariffType,ZZH_ZZI_TariffType) 
VALUES ('BA946F9E-AA58-428F-9D73-BEA1E6329F62','15F534A1-E364-46A3-8ADC-952D685EDA1A','X','B8DA230C-BDDE-4B49-A17B-C42055D3769D');
";
				cmd.ExecuteNonQuery();
			}
		}
	}
}
