using System.Linq;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DeactivateUNDGSubstanceWithIncorrectVariantInformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT COUNT(*) FROM UNDGSubstance WHERE DG_IsActive = 0 and DG_Variant = 'a'";
				Assert.AreEqual(UNNOs.Length, cmd.ExecuteScalar());
				cmd.CommandText = @"SELECT COUNT(*) FROM UNDGSubstance WHERE DG_IsActive = 1 and DG_Variant = ''";
				Assert.AreEqual(UNNOs.Length, cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DeactivateUNDGSubstanceWithIncorrectVariantInformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				var unnosToFix = UNNOs.Select(x => $"'{x}'");
				var activeInserts = UNNOs.Select(x => $"(newid(), '{x}', 'a', 'IMO', 1)");
				var inactiveInserts = UNNOs.Select(x => $"(newid(), '{x}', '', 'IMO', 0)");

				cmd.CommandText = $@"
IF(SELECT COUNT(*) FROM UNDGSubstance WHERE DG_UNNO IN ({string.Join(",", unnosToFix)})) = 0
BEGIN
			INSERT INTO UNDGSubstance(DG_PK, DG_UNNO, DG_Variant, DG_Standard, DG_IsActive)
			VALUES
			";
				cmd.CommandText += string.Join(",", activeInserts) + ",";
				cmd.CommandText += string.Join(",", inactiveInserts);
				cmd.CommandText += @"
END";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}

		string[] UNNOs = new[]
		{
			"0124",
			"0248",
			"0249",
			"0276",
			"0323",
			"0337",
			"0494",
			"1005",
			"1075",
			"1230",
			"1350",
			"1365",
			"1395",
			"1733",
			"1738",
			"1754",
			"1786",
			"1829",
			"1839",
			"1911",
			"1941",
			"1954",
			"1961",
			"1967",
			"2036",
			"2073",
			"2212",
			"2315",
			"2442",
			"2448",
			"2672",
			"2692",
			"2743",
			"2826",
			"2845",
			"2908",
			"2910",
			"2919",
			"3077",
			"3082",
			"3318",
			"3331",
			"3332",
			"3333",
			"3334",
			"3356",
			"3369",
			"3432",
		};
	}
}
