using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixWI00203641TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefCountryStates Where RW_Code = 'KNR' and RW_RN_NKCountryCode = 'AF' and RW_Description=N'Kuna?'";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"Select Count(1) from RefCountryStates Where RW_Code = 'KNR' and RW_RN_NKCountryCode = 'AF' and RW_Description=N'Kunaṟ'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"Select Count(1) from RefCountryStates Where RW_Code = 'NSW' and RW_RN_NKCountryCode = 'AU' and RW_Description=N'New South Wales'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"Select Count(1) from RefCountryStates Where RW_Code = '18' and RW_RN_NKCountryCode = 'IR' and RW_Description=N'Kohgiluyeh va Bowyer A?mad'";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"Select Count(1) from RefCountryStates Where RW_Code = '18' and RW_RN_NKCountryCode = 'IR' and RW_Description=N'Kohgīlūyeh va Bowyer Aḩmad'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixWI00203641Transformation(25);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefCountryStates (RW_PK, RW_Description, RW_RegionName, RW_IsActive, RW_Code, RW_RN_NKCountryCode)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','Kuna?','',1,'KNR','AF');

INSERT INTO RefCountryStates (RW_PK, RW_Description, RW_RegionName, RW_IsActive, RW_Code, RW_RN_NKCountryCode)
VALUES ('48E245C1-9989-4C19-9F97-BBF7EFE4FF9C','New South Wales','',1,'NSW','AU');

INSERT INTO RefCountryStates (RW_PK, RW_Description, RW_RegionName, RW_IsActive, RW_Code, RW_RN_NKCountryCode)
VALUES ('64D9A741-205B-4371-B8CB-314EE3177F4D','Kohgiluyeh va Bowyer A?mad','',1,'18','IR');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
