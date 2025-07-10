using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusStatementHeaderCollection))]
	sealed class CusStatementHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusStatementHeaderCollection>
	{
		public void TestGetUniquePaymentDetails()
		{
			var periodicStatement = Factory.New<CusStatementHeader>();

			AssertEquals(ZString.Empty, periodicStatement.DailyStatements.GetUniqueAccountNo());
			AssertEquals(ZString.Empty, periodicStatement.DailyStatements.GetUniquePaymentParty());

			var dailyStatement = Factory.New<CusStatementHeader>();
			dailyStatement.B2_B2_PeriodicStatement = periodicStatement.PK;
			dailyStatement.B2_AccountNo = "1";
			dailyStatement.B2_PaymentParty = PaymentPartyList.Codes.Broker;

			AssertEquals("1", periodicStatement.DailyStatements.GetUniqueAccountNo());
			AssertEquals(PaymentPartyList.Codes.Broker, periodicStatement.DailyStatements.GetUniquePaymentParty());

			dailyStatement = Factory.New<CusStatementHeader>();
			dailyStatement.B2_B2_PeriodicStatement = periodicStatement.PK;
			dailyStatement.B2_AccountNo = "2";
			dailyStatement.B2_PaymentParty = PaymentPartyList.Codes.Importer;

			AssertEquals(ZString.Empty, periodicStatement.DailyStatements.GetUniqueAccountNo());
			AssertEquals(ZString.Empty, periodicStatement.DailyStatements.GetUniquePaymentParty());

			dailyStatement.B2_AccountNo = "1";
			dailyStatement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			AssertEquals("1", periodicStatement.DailyStatements.GetUniqueAccountNo());
			AssertEquals(PaymentPartyList.Codes.Broker, periodicStatement.DailyStatements.GetUniquePaymentParty());
		}

		public void TestChildStatementHeaders()
		{
			var statementHeader1 = Factory.New<CusStatementHeader>();
			var periodicStatementHeader = Factory.New<CusStatementHeader>();

			statementHeader1.B2_B2_PeriodicStatement = periodicStatementHeader.PK;

			AssertEquals(1, periodicStatementHeader.DailyStatements.Count);
			Assert(periodicStatementHeader.DailyStatements.Contains(statementHeader1));
		}

		public void TestMatchesFilter()
		{
			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK));

			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			var declaration = CreateDeclaration("1");
			var declaration2 = CreateDeclaration("2");
			var declaration3 = CreateDeclaration("3");
			var declaration4 = CreateDeclaration("4");
			Factory.Save();

			CreateChargeForDeclaration(declaration, chargeCode.PK, 2m, false, 4m, true);
			CreateChargeForDeclaration(declaration2, chargeCode.PK, 10m, true, 80m, false);
			CreateChargeForDeclaration(declaration3, chargeCode.PK, 20m, true, 20m, true);
			CreateChargeForDeclaration(declaration4, chargeCode.PK, 30m, true, 20m, true);

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "1234";
			statement.B2_IsMonthlyStatement = false;
			statement.B2_StatementAmount = 2m;
			statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			var line1 = statement.StatementLines.AddNew();
			line1.B3_EntryNum = "1";
			line1.B3_EntryFilerCode = "XJ5";
			line1.B3_CustomsFeesTotal = 2m;
			var charge1 = line1.Charges.AddNew();
			charge1.B4_ChargeAmount = 2m;
			charge1.B4_ChargeType = "DTY";

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementNumber = "1235";
			statement.B2_IsMonthlyStatement = false;
			statement2.B2_StatementAmount = 80m;
			statement2.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			var line2 = statement2.StatementLines.AddNew();
			line2.B3_EntryNum = "2";
			line2.B3_EntryFilerCode = "XJ5";
			line2.B3_CustomsFeesTotal = 80m;
			charge1 = line2.Charges.AddNew();
			charge1.B4_ChargeAmount = 80m;
			charge1.B4_ChargeType = "DTY";

			var statement3 = Factory.New<CusStatementHeader>();
			statement3.B2_StatementNumber = "1236";
			statement.B2_IsMonthlyStatement = false;
			statement3.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			var line3 = statement3.StatementLines.AddNew();
			line3.B3_EntryNum = "3";
			line3.B3_EntryFilerCode = "XJ5";
			line3.B3_CustomsFeesTotal = 20m;
			line3.B3_Status = StatementLineStatusList.Codes.DeletionPending;
			charge1 = line3.Charges.AddNew();
			charge1.B4_ChargeAmount = 20m;
			charge1.B4_ChargeType = "DTY";

			var statement4 = Factory.New<CusStatementHeader>();
			statement4.B2_StatementNumber = "1237";
			statement.B2_IsMonthlyStatement = false;
			statement4.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			var line4 = statement4.StatementLines.AddNew();
			line4.B3_EntryNum = "4";
			line4.B3_EntryFilerCode = "XJ5";
			line4.B3_CustomsFeesTotal = 30m;
			line4.B3_Status = StatementLineStatusList.Codes.Deleted;
			charge1 = line4.Charges.AddNew();
			charge1.B4_ChargeAmount = 30m;
			charge1.B4_ChargeType = "DTY";

			var monthly = Factory.New<CusStatementHeader>();
			monthly.B2_StatementNumber = "1234P";
			monthly.B2_IsMonthlyStatement = true;
			monthly.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			statement.B2_B2_PeriodicStatement = monthly.PK;
			statement2.B2_B2_PeriodicStatement = monthly.PK;
			statement3.B2_B2_PeriodicStatement = monthly.PK;
			statement4.B2_B2_PeriodicStatement = monthly.PK;

			monthly.FilterStatementLinesBy = StatementLineFilterByOptionList.Codes.LinesWithAPDiscrepany;
			AssertEquals(2, monthly.DailyStatementsForAccountingRecon.Count);
			AssertEquals(true, monthly.DailyStatementsForAccountingRecon.Contains(statement2));

			monthly.FilterStatementLinesBy = StatementLineFilterByOptionList.Codes.LinesWithARDiscrepany;
			AssertEquals(2, monthly.DailyStatementsForAccountingRecon.Count);
			AssertEquals(true, monthly.DailyStatementsForAccountingRecon.Contains(statement));

			monthly.FilterStatementLinesBy = StatementLineFilterByOptionList.Codes.LinesWithDiscrepany;
			AssertEquals(3, monthly.DailyStatementsForAccountingRecon.Count);
			AssertEquals(true, monthly.DailyStatementsForAccountingRecon.Contains(statement));
			AssertEquals(true, monthly.DailyStatementsForAccountingRecon.Contains(statement2));
		}

		JobDeclaration CreateDeclaration(ZString entryNumber)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = entryNumber;
			return declaration;
		}

		void CreateChargeForDeclaration(JobDeclaration declaration, ZGuid chargeCodePK, ZDecimal apAmount, bool isCostPosted, ZDecimal aRAmount, bool isRevenuePosted)
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = declaration.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;

			var charge = Factory.New<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AC = chargeCodePK;
			charge.JR_LocalCostAmt = apAmount;
			charge.JR_LocalSellAmt = aRAmount;

			if (isCostPosted)
			{
				var line = Factory.New<AccTransactionLines>();
				line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
				charge.JR_AL_APLine = line.PK;
			}

			if (isRevenuePosted)
			{
				var line = Factory.New<AccTransactionLines>();
				line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
				charge.JR_AL_ARLine = line.PK;
			}

			job.LoadCharges_ForTestOnly();
		}

		protected override CusStatementHeaderCollection GetCollectionToTest()
		{
			return new CusStatementHeaderCollection(MonthlyStatement);
		}

		CusStatementHeader MonthlyStatement
		{
			get
			{
				if (monthlyStatement == null)
				{
					monthlyStatement = Factory.New<CusStatementHeader>();
					monthlyStatement.B2_StatementNumber = "1112P2222";
				}
				return monthlyStatement;
			}
		}
		CusStatementHeader monthlyStatement;
	}
}
