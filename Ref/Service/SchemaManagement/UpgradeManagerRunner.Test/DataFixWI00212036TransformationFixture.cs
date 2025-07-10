using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixWI00212036TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefCusTradeGroup tg join RefCusTradeGroupCountry tgc on tg.ZZA_PK = tgc.ZZB_ZZA_TradeGroup Where zza_tradegroup = '2014' and ZZA_ZZZ_NKDataGrouping='EUN' ";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusTradeGroup tg join RefCusTradeGroupCountry tgc on tg.ZZA_PK = tgc.ZZB_ZZA_TradeGroup Where zza_tradegroup = '5001' and ZZA_ZZZ_NKDataGrouping='EUN' ";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 106);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixWI00212036Transformation(28);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','EUN','EUN','EUN');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
