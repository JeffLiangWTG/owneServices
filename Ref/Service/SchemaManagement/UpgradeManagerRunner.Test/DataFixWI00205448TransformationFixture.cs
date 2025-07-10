using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixWI00205448TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;

				cmd.CommandText = @"select COUNT(*) FROM RefCountryStates where RW_RN_NKCountryCode = 'ES' AND RW_Code = 'EX'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"select COUNT(*) FROM RefCountryStates where RW_RN_NKCountryCode = 'ES' AND RW_Code = 'CL'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"select COUNT(*) FROM RefCountryStates where RW_RN_NKCountryCode = 'ES' AND RW_Code = 'CN'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"select COUNT(*) FROM RefCountryStates where RW_RN_NKCountryCode = 'FJ' AND RW_Code = 'W'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"select COUNT(*) FROM RefCountryStates where RW_RN_NKCountryCode = 'FJ' AND RW_Code = 'E'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"select COUNT(*) FROM RefCountryStates where RW_RN_NKCountryCode = 'GR' AND RW_Code = '69'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixWI00205448Transformation(47);
		}

		protected override void PrepareTestData()
		{
		}
	}
}
