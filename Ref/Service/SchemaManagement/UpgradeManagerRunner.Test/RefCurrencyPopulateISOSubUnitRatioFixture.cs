using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class RefCurrencyPopulateISOSubUnitRatioFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			Assert.AreEqual(1000, GetMinorUnitRatio(CurrencyCode1), "ISO Minor Unit ratio of ZZ1");
			Assert.AreEqual(100, GetMinorUnitRatio(CurrencyCode2), "ISO Minor Unit ratio of ZZ2");
			Assert.AreEqual(10, GetMinorUnitRatio(CurrencyCode3), "ISO Minor Unit ratio of ZZ3");
			Assert.AreEqual(1, GetMinorUnitRatio(CurrencyCode4), "ISO Minor Unit ratio of ZZ4");
			Assert.AreEqual(100, GetMinorUnitRatio(TWCurrency), "ISO Minor Unit ratio of TWD");
			Assert.AreEqual(100, GetMinorUnitRatio(HUCurrency), "ISO Minor Unit ratio of HUF");

			Assert.AreEqual(1000, GetMinorUnitRatio(CurrencyCode1, false), "Minor Unit ratio of ZZ1");
			Assert.AreEqual(100, GetMinorUnitRatio(CurrencyCode2, false), "Minor Unit ratio of ZZ2");
			Assert.AreEqual(10, GetMinorUnitRatio(CurrencyCode3, false), "Minor Unit ratio of ZZ3");
			Assert.AreEqual(1, GetMinorUnitRatio(CurrencyCode4, false), "Minor Unit ratio of ZZ4");
			Assert.AreEqual(1, GetMinorUnitRatio(TWCurrency, false), "Minor Unit ratio of TWD");
			Assert.AreEqual(1, GetMinorUnitRatio(HUCurrency, false), "Minor Unit ratio of HUF");
		}

		int GetMinorUnitRatio(string code, bool isISO = true)
		{
			using (var cmd = Connection.CreateCommand())
			{
				var querySql = FormattableString.Invariant($@"SELECT {(isISO ? "RX_ISOSubUnitRatio" : "RX_SubUnitRatio")} FROM RefCurrency WHERE RX_Code = '{code}'");
				cmd.CommandText = querySql;
				cmd.Transaction = Transaction;
				return (int)cmd.ExecuteScalar();
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new RefCurrencyPopulateISOSubUnitRatio(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = FormattableString.Invariant($@"
ALTER TABLE RefCurrency NOCHECK CONSTRAINT Constraint_RX_ISOSubUnitRatio;

{InsertCurrency(TWCurrency, 1)};
{InsertCurrency(HUCurrency, 1)};
{InsertCurrency(CurrencyCode1, 1000)};
{InsertCurrency(CurrencyCode2, 100)};
{InsertCurrency(CurrencyCode3, 10)};
{InsertCurrency(CurrencyCode4, 1)};
");
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}

		const string CurrencyCode1 = "ZZ1";
		const string CurrencyCode2 = "ZZ2";
		const string CurrencyCode3 = "ZZ3";
		const string CurrencyCode4 = "ZZ4";
		const string TWCurrency = "TWD";
		const string HUCurrency = "HUF";

		string InsertCurrency(string code, int minorUnitRatio)
		{
			return FormattableString.Invariant($@"
INSERT INTO [dbo].[RefCurrency]
			([RX_PK]
			,[RX_Code]
			,[RX_IsActive]
			,[RX_Symbol]
			,[RX_Desc]
			,[RX_UnitName]
			,[RX_SubUnitName]
			,[RX_SubUnitRatio]
			,[RX_ISOSubUnitRatio])
			VALUES
			('{Guid.NewGuid()}'
			, '{code}'
			, 1
			, '$'
			, '{code} Description'
			, 'Dollar'
			, 'Cents'
			, {minorUnitRatio}
			, 0)");
		}
	}
}
