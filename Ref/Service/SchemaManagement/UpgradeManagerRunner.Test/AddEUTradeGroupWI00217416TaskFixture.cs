using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class AddEUTradeGroupWI00217416TaskFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefCusTradeGroup Where ZZA_TradeGroup = 'EU' And ZZA_ZZZ_NkDataGrouping = 'ZA'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefCusTradeGroup tg Join RefCusTradeGroupCountry tgc on tgc.ZZB_ZZA_TradeGroup = tg.ZZA_PK Where ZZA_TradeGroup = 'EU' And ZZA_ZZZ_NkDataGrouping = 'ZA'";
				Assert.AreEqual(25, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new AddEUTradeGroupWI00217416Task(34);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','ZA','South Africa','ZA');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}

