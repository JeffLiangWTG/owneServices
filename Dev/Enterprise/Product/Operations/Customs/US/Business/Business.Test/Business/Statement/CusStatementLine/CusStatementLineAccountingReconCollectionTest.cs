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
	[TestedType(typeof(CusStatementLineAccountingReconCollection))]
	sealed class CusStatementLineAccountingReconCollectionTest : ActiveBusinessObjectCollectionTestCase<CusStatementLineAccountingReconCollection>
	{
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
			statement.B2_StatementNumber = "12345";
			statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			var line1 = statement.StatementLines.AddNew();
			line1.B3_EntryNum = "1";
			line1.B3_EntryFilerCode = "XJ5";
			line1.B3_CustomsFeesTotal = 2m;

			var line2 = statement.StatementLines.AddNew();
			line2.B3_EntryNum = "2";
			line2.B3_EntryFilerCode = "XJ5";
			line2.B3_CustomsFeesTotal = 80m;

			var line3 = statement.StatementLines.AddNew();
			line3.B3_EntryNum = "3";
			line3.B3_EntryFilerCode = "XJ5";
			line3.B3_CustomsFeesTotal = 20m;
			line3.B3_Status = StatementLineStatusList.Codes.DeletionPending;

			var line4 = statement.StatementLines.AddNew();
			line4.B3_EntryNum = "4";
			line4.B3_EntryFilerCode = "XJ5";
			line4.B3_CustomsFeesTotal = 30m;
			line4.B3_Status = StatementLineStatusList.Codes.Deleted;

			AssertEquals(3, statement.StatementLinesForAccountingRecon.Count);

			statement.FilterStatementLinesBy = StatementLineFilterByOptionList.Codes.LinesWithAPDiscrepany;
			AssertEquals(1, statement.StatementLinesForAccountingRecon.Count);
			AssertEquals(true, statement.StatementLinesForAccountingRecon.Contains(line2));

			statement.FilterStatementLinesBy = StatementLineFilterByOptionList.Codes.LinesWithARDiscrepany;
			AssertEquals(1, statement.StatementLinesForAccountingRecon.Count);
			AssertEquals(true, statement.StatementLinesForAccountingRecon.Contains(line1));

			statement.FilterStatementLinesBy = StatementLineFilterByOptionList.Codes.LinesWithDiscrepany;
			AssertEquals(2, statement.StatementLinesForAccountingRecon.Count);
			AssertEquals(true, statement.StatementLinesForAccountingRecon.Contains(line1));
			AssertEquals(true, statement.StatementLinesForAccountingRecon.Contains(line2));
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
				AccTransactionLines line = Factory.New<AccTransactionLines>();
				line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
				charge.JR_AL_APLine = line.PK;
			}

			if (isRevenuePosted)
			{
				AccTransactionLines line = Factory.New<AccTransactionLines>();
				line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
				charge.JR_AL_ARLine = line.PK;
			}

			job.LoadCharges_ForTestOnly();
		}

		protected override CusStatementLineAccountingReconCollection GetCollectionToTest()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			return new CusStatementLineAccountingReconCollection(statement);
		}
	}
}
