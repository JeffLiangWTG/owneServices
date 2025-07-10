using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusStatementLineCharge))]
	sealed class CusStatementLineChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLine()
		{
			AssertEquals(statementLine, statementLineCharge.Line);
		}

		public void TestAccountNo()
		{
			statementHeader.B2_AccountNo = "1111111";
			AssertEquals(statementHeader.B2_AccountNo, statementLineCharge.AccountNo);
		}

		public void TestProcessDate()
		{
			statementHeader.B2_ProcessDate = ZDateTime.Now;
			AssertEquals(statementHeader.B2_ProcessDate, statementLineCharge.ProcessDate);
		}

		public void TestEntryNum()
		{
			statementLine.B3_EntryNum = "DNB201608181234567";
			AssertEquals(statementLine.B3_EntryNum, statementLineCharge.EntryNum);
		}

		public void TestChargeTypeDescription()
		{
			statementLineCharge.B4_ChargeType = "V";
			AssertEquals("VAT", statementLineCharge.ChargeTypeDescription);
			statementLineCharge.B4_ChargeType = "I";
			AssertEquals("PAYMENT", statementLineCharge.ChargeTypeDescription);
			statementLineCharge.B4_ChargeType = "P";
			AssertEquals("PAYMENT", statementLineCharge.ChargeTypeDescription);
		}

		public void TestChargeTypes()
		{
			AssertType(typeof(CusStatementChargeTypeList), statementLineCharge.ChargeTypes);
		}

		public void TestFetchStrategy()
		{
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var lineCharge = newFactory.Load<CusStatementLineCharge>(statementLineCharge.PK);
			AssertEquals(1, newFactory.ActiveFetchHintsForTable(CusStatementLineSchema.Constants.TableName));
			AssertEquals(1, newFactory.ActiveFetchHintsForTable(CusStatementHeaderSchema.Constants.TableName));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return statementLineCharge;
		}

		protected override void SetUp()
		{
			base.SetUp();
			statementHeader = Factory.New<CusStatementHeader>();
			statementLine = statementHeader.StatementLines.AddNew();
			statementLineCharge = statementLine.Charges.AddNew();
		}

		CusStatementHeader statementHeader;
		CusStatementLine statementLine;
		CusStatementLineCharge statementLineCharge;
	}
}
