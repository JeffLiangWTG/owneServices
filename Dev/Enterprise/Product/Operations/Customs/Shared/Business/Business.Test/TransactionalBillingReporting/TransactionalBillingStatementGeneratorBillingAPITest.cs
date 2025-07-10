using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.Business.TransactionalBillingReporting.Testing
{
	public sealed class TransactionalBillingStatementGeneratorBillingAPITest : TestCaseWithFactory
	{
		[TestDate(1986, 3, 12)]
		public void TestRunForVeryFirstTimeAPI()
		{
			EnvProxy.SetHostedLocationForTest("Milton Keynes");
			try
			{
				// Due date is blank - should run for the very first time
				var logger = new TestServiceLogger();
				SetupTestRecords();
				var generator = GetGenerator(logger);
				generator.DoEverything();

				var results = GetBillingTransactions();
				var awbs = results.Where(t => t.PriceItemCode == "AWB");
				var entries = results.Where(t => t.PriceItemCode == "CUE");
				var genrals = results.Where(t => t.PriceItemCode == "GTM");

				AssertEquals(3, awbs.Count());
				AssertEquals(1, entries.Count());
				AssertEquals(5, genrals.Count());

				Assert(!entries.Any(t => t.HasAnyReferenceContaining("auDecECN")));
				Assert(!entries.Any(t => t.HasAnyReferenceContaining("auCehECN")));
				Assert(!entries.Any(t => t.HasAnyReferenceContaining("TooOld")));
				Assert(entries.Any(t => t.HasAnyReferenceContaining("gbDecEXP")));
				Assert(entries.Any(t => t.HasAnyReferenceContaining("gbCehEXP")));

				Assert(!entries.Any(t => t.HasAnyReferenceContaining("gbCehEXPMcp")));
				Assert(!entries.Any(t => t.HasAnyReferenceContaining("gbCehEXPGems")));
				AssertEquals(1, entries.Count(t => t.HasAnyReferenceContaining("gbDecEXP")));  // no duplicates caused by several messages

				Assert(!awbs.Any(t => t.HasAnyReferenceContaining("cmrOrphanHawb")));

				Assert(!awbs.Any(t => t.HasAnyReferenceContaining("cmrOrphanHawb") && t.HasAnyReferenceContaining("cmrHawb1")));
				Assert(awbs.Any(t => t.HasAnyReferenceContaining("22222222222") && t.HasAnyReferenceContaining("cukHawb1")));
				Assert(awbs.Any(t => t.HasAnyReferenceContaining("22222222222") && t.HasAnyReferenceContaining("cukHawb2")));
				Assert(awbs.Any(t => t.HasAnyReferenceContaining("33333333333")));
				
				Assert(!awbs.Any(t => t.Reference1 == "22222222222" && string.IsNullOrEmpty(t.Reference2) && t.Reference2 != null));
				Assert(!awbs.Any(t => t.Reference1 == "cmrOrphanHawbTooYoung" && string.IsNullOrEmpty(t.Reference2) && t.Reference2 != null));

				Assert(genrals.Any(t => t.HasAnyReferenceContaining("CUKCTM98AAABBB/CCC")));
				Assert(genrals.Any(t => t.HasAnyReferenceContaining("111")));
				Assert(genrals.Any(t => t.HasAnyReferenceContaining("222")));
				Assert(genrals.Any(t => t.HasAnyReferenceContaining("333")));
				Assert(genrals.Any(t => t.HasAnyReferenceContaining("444")));
				Assert(!genrals.Any(t => t.HasAnyReferenceContaining("555")));
				Assert(!genrals.Any(t => t.HasAnyReferenceContaining("666")));
				Assert(genrals.Any(t => t.HasAnyReferenceContaining("777")));  // Included because this is the first ever run

				var g = genrals.OrderBy(t => t.Reference5).First();
				AssertEquals(1, g.BillableCount);
				AssertEquals("MAN", g.Branch);
				AssertEquals("GBC", g.Category);
				AssertEquals("EDIGBxDAT", g.ClientID);
				AssertEquals(null, g.ClientStaffCode);
				AssertEquals("RCV", g.Reference1);
				AssertEquals("CUKSYS98COMMDB", g.Reference2);
				AssertEquals("CUKFFW98000DAN", g.Reference3);
				AssertContains("Fallback", g.Reference4);
				AssertEquals("Did not try to record more than 50 chars", "111I__Something very long that will run to more th", g.Reference5);

				AssertEquals("Recorded as running today", new ZDateTime(1986, 3, 12), CustomsDataRegistry.Instance.CustomsTransactionalBillingTaskLastRunDate_BillingAPI.Value);
			}
			finally
			{
				EnvProxy.SetHostedLocationForTest("");
			}
		}

		[TestDate(1986, 2, 3, 1, 2, 3)]
		public void TestRunSecondTimeAPI()
		{
			CustomsDataRegistry.Instance.CustomsTransactionalBillingTaskLastRunDate_BillingAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ZDateTime(1986, 2, 2).ToDateTime());
			EnvProxy.SetHostedLocationForTest("Milton Keynes");
			try
			{
				// Due date is blank - should run for the very first time
				var logger = new TestServiceLogger();
				SetupTestRecords();
				var generator = GetGenerator(logger);
				generator.DoEverything();

				var results = GetBillingTransactions();
				var genrals = results.Where(t => t.PriceItemCode == "GTM");

				AssertEquals(4, genrals.Count());

				Assert(genrals.Any(t => t.HasAnyReferenceContaining("111")));
				Assert(genrals.Any(t => t.HasAnyReferenceContaining("222")));
				Assert(genrals.Any(t => t.HasAnyReferenceContaining("333")));
				Assert(genrals.Any(t => t.HasAnyReferenceContaining("444")));
				Assert(!genrals.Any(t => t.HasAnyReferenceContaining("777")));  // Excluded - too old

				AssertEquals("Recorded as running today", new ZDateTime(1986, 2, 3, 1, 2, 3), CustomsDataRegistry.Instance.CustomsTransactionalBillingTaskLastRunDate_BillingAPI.Value);
			}
			finally
			{
				EnvProxy.SetHostedLocationForTest("");
			}
		}

		TransactionalBillingStatementGeneratorBillingAPI GetGenerator(TestServiceLogger logger)
		{
			return new TransactionalBillingStatementGeneratorBillingAPI(logger);
		}

		[TestDate(1986, 3, 12, 4, 0, 0)]
		public void TestRunDate_DoesNothingWhenNotWiseCloud()
		{
			var logger = new TestServiceLogger();
			SetupTestRecords();
			var generator = GetGenerator(logger);
			generator.DoEverything();
			AssertEquals(0, Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertContains("Information|No need to do anything - Not_WiseCloud", logger[0]);
		}

		List<BillingTransaction> GetBillingTransactions()
		{
			var billingTransactions = new List<BillingTransaction>();

			using (var cmd = TestConnection.Command("SELECT SUD_Data FROM dbo.StmUsageData"))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					billingTransactions.Add(BillingManager.DecryptTransaction(reader[0].ToString(), BillingManager.CurrentSchemaVersion));
				}
			}

			return billingTransactions;
		}

		void SetupTestRecords()
		{
			CreateCompaniesAndBranches();
			CreateAwbs();
			CreateDeclarationsWithEntries();
			CreateGenralTextMessages(manchester.PK.ToGuid(), Factory);
			Factory.Save();
		}

		public static void CreateGenralTextMessages(Guid branchPK, BusinessObjectFactory factory)
		{
			using (DisposableEnvironment.ForBranch(branchPK))
			{
				CreateInterchangeAndMessage(factory, "CUKSYS98COMMDB", "CUKFFW98000DAN", "FBK", "RCV", "111", new ZDateTime(1986, 2, 2)); // Include
				CreateInterchangeAndMessage(factory, "CUKCCS98XXXYYY", "CUKAIR98LHRDJC", "TXT", "RCV", "222", new ZDateTime(1986, 2, 2)); // Include
				CreateInterchangeAndMessage(factory, "CUKCCS98XXXYYY", "CUKAIR98LHRDJC", "BCM", "RCV", "333", new ZDateTime(1986, 2, 2)); // Include
				CreateInterchangeAndMessage(factory, "CUKCTM98AAABBB/CCC", "CUKAIR98LHRDJC", "TXT", "TRX", "444", new ZDateTime(1986, 2, 2)); // Include
				CreateInterchangeAndMessage(factory, "CUKSYS98COMMDB", "CUKAIR98LHRDJC", "FRI", "RCV", "555", new ZDateTime(1986, 2, 2), messageType: "CAR"); // Exclude, right application but not a GENRAL
				CreateInterchangeAndMessage(factory, "CUKSYS98COMMDB", "CUKFFW98000DAN", "FBK", "RCV", "666", new ZDateTime(1986, 2, 2), applicationCode: "CMR"); // Exclude, wrong application
				CreateInterchangeAndMessage(factory, "CUKFFW98000DAN", "CUKAIR98LHRDJC", "TXT", "TRX", "777", new ZDateTime(1986, 1, 2)); // Exclude, too old
			}
		}

		static void CreateInterchangeAndMessage(BusinessObjectFactory factory, string eiFrom, string eiTo, string messageSubTypePurpose, string receiveTransmit, string messageNumber, ZDateTime sendDate, string applicationCode = "CUK", string messageType = "GEN")
		{
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_From = eiFrom;
			interchange.EI_To = eiTo;
			interchange.EI_BodyText = "Anything";
			interchange.EI_InterchangeNum = messageNumber + "I";
			if (messageNumber == "111")
			{
				ZString longNumber = interchange.EI_InterchangeNum + "__Something very long that will run to more than 50 chars if we are not careful..." + System.Guid.NewGuid().ToString() + System.Guid.NewGuid().ToString();
				interchange.EI_InterchangeNum = longNumber.Left(EDIInterchange.Schema.EI_InterchangeNumMaxLength);
			}
			var message = factory.New<EdiMessageThatDoesNotCryAboutMessageNumbers>();
			interchange.ContainedMessages.Add(message);
			message.EM_ApplicationCode = applicationCode;
			message.EM_ReceiveTransmit = receiveTransmit;
			message.EM_SystemCreateTimeUtc = sendDate;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubTypePurpose;
			message.EM_MessageNum = messageNumber = "M";
		}

		class EdiMessageThatDoesNotCryAboutMessageNumbers : EDIMessage
		{
			public EdiMessageThatDoesNotCryAboutMessageNumbers(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			protected override string GetMessageReferenceNumber()
			{
				return "Shut Up" + EM_MessageNum;
			}
		}

		void CreateCompaniesAndBranches()
		{
			var companyAU = Factory.New<GlbCompany>();
			companyAU.GC_RN_NKCountryCode = "AU";
			companyAU.GC_Code = "AUx";
			perth = companyAU.Branches.AddNew();
			perth.GB_Code = "PER";

			var companyGB = Factory.New<GlbCompany>();
			companyGB.GC_RN_NKCountryCode = "GB";
			companyGB.GC_Code = "GBx";
			manchester = companyGB.Branches.AddNew();
			manchester.GB_Code = "MAN";

			Factory.Save();
		}

		void CreateDeclarationsWithEntries()
		{
			using (DisposableEnvironment.ForBranch(perth.PK.ToGuid()))
			{
				var auDecECN = Factory.New<BaseJobDeclaration>();
				auDecECN.JE_GB = perth.PK;
				auDecECN.JE_DeclarationReference = "auDecECN";
				var auCehECN = auDecECN.CustomsEntryHeaders.AddNew();
				auCehECN.EntryNumber = "AU ECN";
				auCehECN.CH_BGMReference = "auCehECN";
				var auCenECN = auCehECN.CusEntryNumber;
				auCenECN.CE_Category = "CUS";
				auCenECN.CE_EntryType = Common.CusEntryNumberTypes.Australia.ECN;
				auCenECN.CE_EntryIsSystemGenerated = true;
				auCenECN.CE_IssueDate = new ZDateTime(1986, 2, 1);
			}

			using (DisposableEnvironment.ForBranch(manchester.PK.ToGuid()))
			{
				// All good
				var gbDecEXP = Factory.New<BaseJobDeclaration>();
				gbDecEXP.JE_DeclarationReference = "gbDecEXP";
				gbDecEXP.JE_GB = manchester.PK;
				gbDecEXP.JE_AddInfo = "Gateway=CCSUK";
				var gbCehEXP = gbDecEXP.CustomsEntryHeaders.AddNew();
				gbCehEXP.EntryNumber = "GB EXP";
				gbCehEXP.CH_BGMReference = "gbCehEXP";
				var gbCenEXP = gbCehEXP.CusEntryNumber;
				gbCenEXP.CE_Category = "CUS";
				gbCenEXP.CE_EntryType = "EXP";
				gbCenEXP.CE_EntryIsSystemGenerated = true;
				gbCenEXP.CE_IssueDate = new ZDateTime(1986, 2, 2);
				MakeMessageandInterchange(gbCehEXP, "CUKCTM98CHFEXP");
				MakeMessageandInterchange(gbCehEXP, "CUKCTM98CHFEXP");  // makes sure we don't duplicate
				var gbMrn = Factory.New<CusEntryNumber>();
				gbMrn.CE_ParentID = gbCehEXP.PK;
				gbMrn.CE_ParentTable = CusEntryNumber.Schema.TableName;
				gbMrn.CE_EntryType = "MRN";
				gbMrn.CE_Category = "CUS";
				gbMrn.CE_EntryNum = "GB MRN Ignore";
				gbMrn.CE_EntryIsSystemGenerated = true;
				gbMrn.CE_IssueDate = new ZDateTime(1986, 2, 3);

				// Wrong date
				var gbDecTooOld = Factory.New<BaseJobDeclaration>();
				gbDecTooOld.JE_GB = manchester.PK;
				gbDecTooOld.JE_DeclarationReference = "gbDecTooOld";
				var gbCehTooOld = gbDecTooOld.CustomsEntryHeaders.AddNew();
				gbCehTooOld.EntryNumber = "GB Too Old";
				gbCehTooOld.CH_BGMReference = "gbCehTooOld";
				MakeMessageandInterchange(gbCehTooOld, "CUKCTM98CHFEXP");

				// Wrong country of issue
				var auCenTooOld = gbCehTooOld.CusEntryNumber;
				auCenTooOld.CE_Category = "CUS";
				auCenTooOld.CE_EntryType = "EXP";
				auCenTooOld.CE_EntryIsSystemGenerated = true;
				auCenTooOld.CE_IssueDate = ZDateTime.BrettsBirthday;

				// Wrong CSP
				var gbDecEXPWrongCsp = Factory.New<BaseJobDeclaration>();
				gbDecEXPWrongCsp.JE_DeclarationReference = "gbDecEXPMcp";
				gbDecEXPWrongCsp.JE_GB = manchester.PK;
				gbDecEXPWrongCsp.JE_AddInfo = "Gateway=MCP";
				var gbCehEXPWrongCsp = gbDecEXPWrongCsp.CustomsEntryHeaders.AddNew();
				gbCehEXPWrongCsp.EntryNumber = "GB EXP";
				gbCehEXPWrongCsp.CH_BGMReference = "gbCehEXPMcp";
				var gbCenEXPWrongCsp = gbCehEXPWrongCsp.CusEntryNumber;
				gbCenEXPWrongCsp.CE_Category = "CUS";
				gbCenEXPWrongCsp.CE_EntryType = "EXP";
				gbCenEXPWrongCsp.CE_EntryIsSystemGenerated = true;
				gbCenEXPWrongCsp.CE_IssueDate = new ZDateTime(1986, 2, 2);
				MakeMessageandInterchange(gbCehEXPWrongCsp, "CUKCTM98CHFEXP");

				// Not directly via CCSUK
				var gbDecEXPCcsukButViaGems = Factory.New<BaseJobDeclaration>();
				gbDecEXPCcsukButViaGems.JE_DeclarationReference = "gbDecEXPGems";
				gbDecEXPCcsukButViaGems.JE_GB = manchester.PK;
				gbDecEXPCcsukButViaGems.JE_AddInfo = "Gateway=CCSUK";
				var gbCehEXPCcsukButViaGems = gbDecEXPCcsukButViaGems.CustomsEntryHeaders.AddNew();
				gbCehEXPCcsukButViaGems.EntryNumber = "GB EXP";
				gbCehEXPCcsukButViaGems.CH_BGMReference = "gbCehEXPGems";
				var gbCenEXPCcsukButViaGems = gbCehEXPCcsukButViaGems.CusEntryNumber;
				gbCenEXPCcsukButViaGems.CE_Category = "CUS";
				gbCenEXPCcsukButViaGems.CE_EntryType = "EXP";
				gbCenEXPCcsukButViaGems.CE_EntryIsSystemGenerated = true;
				gbCenEXPCcsukButViaGems.CE_IssueDate = new ZDateTime(1986, 2, 2);
				MakeMessageandInterchange(gbCehEXPCcsukButViaGems, "GBG");
			}
		}

		void MakeMessageandInterchange(CusEntryHeader entry, string sender)
		{
			var message = entry.Messages.AddNew();  // Do not duplicate the results
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ReceiveTransmit = "RCV";
			message.EM_ReceiveTransmit = "RCV";
			interchange.EI_From = sender;
			interchange.EI_To = "X";
			interchange.EI_BodyText = "Anything";
			message.EM_EI = interchange.PK;
		}

		void CreateAwbs()
		{
			using (DisposableEnvironment.ForBranch(perth.PK.ToGuid()))
			{
				var cmrMawb = Factory.New<CusMAWB>();
				cmrMawb.CM_MAWB = "11111111111";
				cmrMawb.CM_ApplicationCode = "CMR";
				cmrMawb.CM_SystemCreateTimeUtc = new ZDateTime(1986, 2, 1);
				var cmrHawb1 = Factory.New<CusHAWB>();
				cmrHawb1.CS_CM = cmrMawb.PK;
				cmrHawb1.CS_HAWB = "cmrHawb1";
				cmrHawb1.CS_SystemCreateTimeUtc = cmrMawb.CM_SystemCreateTimeUtc;
				var cmrOrphanHawb = Factory.New<CusHAWB>();
				cmrOrphanHawb.CS_ApplicationCode = "CMR";
				cmrOrphanHawb.CS_HAWB = "cmrOrphanHawb";
				cmrOrphanHawb.CS_SystemCreateTimeUtc = new ZDateTime(1986, 2, 1);
				var cmrOrphanHawbTooYoung = Factory.New<CusHAWB>();
				cmrOrphanHawbTooYoung.CS_ApplicationCode = "CMR";
				cmrOrphanHawbTooYoung.CS_HAWB = "cmrOrphanHawbTooYoung";
			}

			using (DisposableEnvironment.ForBranch(manchester.PK.ToGuid()))
			{
				var cukMawb = Factory.New<CusMAWB>();
				cukMawb.CM_MAWB = "22222222222";
				cukMawb.CM_ApplicationCode = "CUK";
				cukMawb.CM_SystemCreateTimeUtc = new ZDateTime(1986, 2, 2);
				var cukHawb1 = Factory.New<CusHAWB>();
				cukHawb1.CS_CM = cukMawb.PK;
				cukHawb1.CS_HAWB = "cukHawb1";
				cukHawb1.CS_SystemCreateTimeUtc = new ZDateTime(1986, 2, 2);
				var cukHawb2 = Factory.New<CusHAWB>();
				cukHawb2.CS_CM = cukMawb.PK;
				cukHawb2.CS_HAWB = "cukHawb2";
				cukHawb2.CS_SystemCreateTimeUtc = new ZDateTime(1986, 2, 2, 1, 1, 1);
				var cukHawbWorkerForMawb = Factory.New<CusHAWB>();
				cukHawbWorkerForMawb.CS_CM = cukMawb.PK;
				cukHawbWorkerForMawb.CS_IsMasterHouse = true;
				cukHawbWorkerForMawb.CS_SystemCreateTimeUtc = new ZDateTime(1986, 2, 2);

				var cukBasic = Factory.New<CusMAWB>();
				cukBasic.CM_SystemCreateTimeUtc = new ZDateTime(1986, 2, 3);
				cukBasic.CM_MAWB = "33333333333";
				cukBasic.CM_ApplicationCode = "CUK";
				var cukHawbWorkerForBasic = Factory.New<CusHAWB>();
				cukHawbWorkerForBasic.CS_CM = cukBasic.PK;
				cukHawbWorkerForBasic.CS_IsMasterHouse = true;
				cukHawbWorkerForBasic.CS_SystemCreateTimeUtc = new ZDateTime(1986, 2, 3);
			}
		}

		GlbBranch perth;
		GlbBranch manchester;
	}
}
