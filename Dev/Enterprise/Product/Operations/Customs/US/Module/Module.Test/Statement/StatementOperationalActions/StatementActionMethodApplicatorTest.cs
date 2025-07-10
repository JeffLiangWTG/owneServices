using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(StatementActionMethodApplicator))]
	sealed class StatementActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestEndToEnd()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "~KNZ";
			var wrapper = OrgHeaderWrapper.New(importer);
			wrapper.ZO_AccountNo = "UN123";
			var header1 = Factory.New<CusStatementHeader>();
			header1.B2_OH_Importer = importer.PK;
			header1.B2_PaymentType = "3";
			header1.B2_EntryFilerCode = "F12";
			header1.B2_StatementNumber = "0912345678";
			var line1 = header1.StatementLines.AddNew();
			line1.B3_CustomsFeesTotal = 123.45m;
			var header2 = Factory.New<CusStatementHeader>();
			header2.B2_OH_Importer = importer.PK;
			header2.B2_PaymentType = "7";
			header2.B2_EntryFilerCode = "F12";
			header2.B2_StatementNumber = "8725874196";
			var line2 = header2.StatementLines.AddNew();
			line2.B3_CustomsFeesTotal = 258.12m;
			Factory.Save();
			var log = new DummyOperationalActionSectionLog();
			StatementApplicator.SkipValidationJustForTesting = true;
			StatementApplicator.Build(new ZGuid[] { header1.PK, header2.PK });
			StatementApplicator.InitialiseBeforeIndividiualBatchRun();
			StatementApplicator.Apply(log, new CusStatementHeader[] { header1, header2 });
			log.Verify();
			if (StatementApplicator.SupportsSummary)
			{
				StatementApplicator.SummaryLog(log);
			}

			var newFactory = new BusinessObjectFactory();
			var reloadHeader1 = newFactory.Load<CusStatementHeader>(header1.PK);
			AssertEquals(1, reloadHeader1.Messages.Count);
			AssertEquals("RM", reloadHeader1.Messages[0].EM_MessageType);
			AssertEquals("ACH", reloadHeader1.Messages[0].EM_MessageSubType);
			Assert(log.MessagesString().Contains("0912345678"));
			Assert(log.MessagesString().Contains("Message(s) Sent"));
			var reloadHeader2 = newFactory.Load<CusStatementHeader>(header2.PK);
			AssertEquals(1, reloadHeader2.Messages.Count);
			AssertEquals("RM", reloadHeader2.Messages[0].EM_MessageType);
			AssertEquals("ACH", reloadHeader2.Messages[0].EM_MessageSubType);
			Assert(log.MessagesString().Contains("8725874196"));
			Assert(log.MessagesString().Contains("Message(s) Sent"));
		}

		public void TestUpdateStatement()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "~KNZ";
			var wrapper = OrgHeaderWrapper.New(importer);
			wrapper.ZO_AccountNo = "UN123";
			var header1 = Factory.New<CusStatementHeader>();
			header1.B2_OH_Importer = importer.PK;
			header1.B2_PaymentType = "3";
			header1.B2_EntryFilerCode = "F12";
			header1.B2_StatementNumber = "0912345678";
			var line1 = header1.StatementLines.AddNew();
			line1.B3_CustomsFeesTotal = 123.45m;
			Factory.Save();
			var log = new DummyOperationalActionSectionLog();
			StatementApplicator.SkipValidationJustForTesting = true;
			StatementApplicator.Build(new ZGuid[] { header1.PK });
			AssertEquals(1, StatementApplicator.StatementPaymentActions.Count);
			var action = StatementApplicator.StatementPaymentActions[0];
			action.PayerUnitNo = "123456";
			StatementApplicator.InitialiseBeforeIndividiualBatchRun();
			StatementApplicator.Apply(log, new CusStatementHeader[] { header1 });
			var newFactory = new BusinessObjectFactory();
			var reloadHeader1 = newFactory.Load<CusStatementHeader>(header1.PK);
			AssertEquals(1, reloadHeader1.Messages.Count);
			AssertEquals(PaymentStatusList.Codes.PaymentInProgress, reloadHeader1.B2_PaymentStatus);
			AssertEquals("123456", reloadHeader1.B2_AccountNo);
		}

		public void TestBuildCore()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "~KNZ";
			var wrapper = OrgHeaderWrapper.New(importer);
			wrapper.ZO_AccountNo = "UN123";
			var header1 = Factory.New<CusStatementHeader>();
			header1.B2_OH_Importer = importer.PK;
			header1.B2_PaymentType = "3";
			header1.B2_EntryFilerCode = "F12";
			header1.B2_StatementNumber = "0912345678";
			var line1 = header1.StatementLines.AddNew();
			line1.B3_CustomsFeesTotal = 123.45m;
			var header2 = Factory.New<CusStatementHeader>();
			header2.B2_OH_Importer = importer.PK;
			header2.B2_PaymentType = "3";
			header2.B2_EntryFilerCode = "F12";
			header2.B2_StatementNumber = "8725874196";
			var line2 = header2.StatementLines.AddNew();
			line2.B3_CustomsFeesTotal = 258.12m;
			Factory.Save();
			StatementApplicator.Build(new ZGuid[] { header1.PK, header2.PK });
			var actions = StatementApplicator.StatementPaymentActions.Cast<StatementPaymentAction>();
			AssertEquals(2, actions.Count());
			Assert(actions.Any(x => x.StatementNumber == "0912345678"));
			Assert(actions.Any(x => x.StatementNumber == "8725874196"));
		}

		StatementActionMethodApplicator statementApplicator;
		StatementActionMethodApplicator StatementApplicator => statementApplicator ?? (statementApplicator = (StatementActionMethodApplicator)GetNewBusinessObject());
	}
}
