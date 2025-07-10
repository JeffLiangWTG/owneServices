using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusStatementLineChargeCollection))]
	sealed class CusStatementLineChargeCollectionTest : ActiveBusinessObjectCollectionTestCase<CusStatementLineChargeCollection>
	{
		public void TestUpdateLineChargeFor()
		{
			var statement = Factory.New<CusStatementHeader>();
			var statementLine = statement.StatementLines.AddNew();
			statementLine.Charges.UpdateLineChargeFor("AAA", 100.25m);
			AssertEquals("one charge created", 1, statementLine.Charges.Count);
			AssertEquals("one charge created", 100.25m, statementLine.Charges[0].B4_ChargeAmount);
			statementLine.Charges.UpdateLineChargeFor("AAA", -100.25m);
			AssertEquals("one charge created", -100.25m, statementLine.Charges[0].B4_ChargeAmount);
			var charge = statementLine.Charges[0];
			statementLine.Charges.UpdateLineChargeFor("AAA", 0m);
			AssertEquals("AAA charge deleted", 0, statementLine.Charges.Count);
			AssertEquals("IsDeleted", true, charge.IsDeleted);
		}

		public void TestGetAmountFor()
		{
			var statement = Factory.New<CusStatementHeader>();
			var statementLine = statement.StatementLines.AddNew();
			statementLine.Charges.UpdateLineChargeFor("AAA", 100.25m);
			AssertEquals("Amount for valid code", 100.25m, statementLine.Charges.GetAmountFor("AAA"));
			AssertEquals("Amount for invalid code", 0m, statementLine.Charges.GetAmountFor("BBB"));
		}

		public void TestGetChargeLineFor()
		{
			var statement = Factory.New<CusStatementHeader>();
			var statementLine = statement.StatementLines.AddNew();
			statementLine.Charges.UpdateLineChargeFor("AAA", 100.25m);
			AssertEquals("Valid key", 100.25m, statementLine.Charges.GetChargeLineFor("AAA", 100.25m).B4_ChargeAmount);
			AssertNull("Invalid key", statementLine.Charges.GetChargeLineFor("BBB", 100.25m));
		}

		public void TestGetFirstCharge()
		{
			var statement = Factory.New<CusStatementHeader>();
			var statementLine = statement.StatementLines.AddNew();
			statementLine.Charges.UpdateLineChargeFor("AAA", 100.25m);
			AssertEquals("First Charge", 100.25m, statementLine.Charges.GetFirstCharge("AAA").B4_ChargeAmount);
		}

		protected override CusStatementLineChargeCollection GetCollectionToTest()
		{
			return new CusStatementLineChargeCollection(Factory.New<CusStatementHeader>().StatementLines.AddNew());
		}
	}
}
