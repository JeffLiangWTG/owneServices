using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusStatementLineChargeCollection))]
	sealed class CusStatementLineChargeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestUpdateLineChargeFor()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			CusStatementLine statementLine = statement.StatementLines.AddNew();
			statementLine.Charges.UpdateLineChargeFor("AAA", 100.25m);
			AssertEquals("one charge created", 1, statementLine.Charges.Count);
			AssertEquals("one charge created", 100.25m, statementLine.Charges[0].B4_ChargeAmount);

			CusStatementLineCharge charge = statementLine.Charges[0];
			statementLine.Charges.UpdateLineChargeFor("AAA", 0m);
			AssertEquals("AAA charge deleted", 0, statementLine.Charges.Count);
			AssertEquals("IsDeleted", true, charge.IsDeleted);
		}

		public void TestGetTotalPayableAmount()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			CusStatementLine statementLine = statement.StatementLines.AddNew();
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100.25m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.HMF, 25.25m);

			AssertEquals(125.50m, statementLine.Charges.GetTotalPayableAmount());

			AssertEquals(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, statementLine.Charges.GetFirstCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).B4_ChargeType);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.HMF, statementLine.Charges.GetFirstCharge(Core.Constants.USCustoms.FeeCodes.HMF).B4_ChargeType);

			statementLine.B3_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(25.25m, statementLine.Charges.GetTotalPayableAmount());
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CusStatementLine statementLine = Factory.New<CusStatementLine>();
			return new CusStatementLineChargeCollection(statementLine);
		}
	}
}
