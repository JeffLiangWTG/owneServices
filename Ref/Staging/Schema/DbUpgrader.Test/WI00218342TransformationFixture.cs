using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test;

[Property("DAT:CapabilityRequirements", "SQL2019+")]
class WI00218342TransformationFixture : TransformationFixture
{
	protected override void AssertTransformationResults()
	{
		using var cmd = Connection.CreateCommand();
		cmd.Transaction = Transaction;
		cmd.CommandText = @"SELECT COUNT(*) FROM NamedEntityClassification WHERE NEC_Name = 'Brunei Darussalam'";
		Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
	}

	protected override IDataTransformationTask GetTask()
	{
		return new WI00202963Transformation(0);
	}

	protected override void PrepareTestData()
	{
		using var cmd = Connection.CreateCommand();
		cmd.CommandText = @"
INSERT INTO NamedEntityClassification(NEC_PK, NEC_Name, NEC_Class, NEC_Language, NEC_Code)
VALUES
(newid(), 'Brunei Darussalam', 'COUNTRY', 'EN', 'BN')
";
		cmd.Transaction = Transaction;
		cmd.ExecuteNonQuery();
	}
}
