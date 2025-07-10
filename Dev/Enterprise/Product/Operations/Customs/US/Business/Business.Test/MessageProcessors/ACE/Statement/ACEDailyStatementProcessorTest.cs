using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors.Statement.Testing
{
	public class ACEDailyStatementProcessorTest : CommonDailyStatementProcessorTest<ACEDailyStatementProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override (EDIMessage message, ZGuid expectedBranchPK, ZGuid expectedLinkUniqueID, ZString expectedJobNumber, string[] expectedKeys) GenerateInfoToTestGetKeysForBlockingParallelProcessing()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "3901138000";
			statement.B2_EntryFilerCode = "XJ5";
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.DailyStatement;
			message.EM_MessageText =
"B018888XJ5PFF3901138000050207919-262203600                                      " +
"Q18887XJ5  00000360  13-1479270000518150000023500000000000000NB00001160      01 " +
"Q24904SV9  71009847 0001100003300002200000           2Y      453                " +
"QA010530000001732005400000000320055000000189200560000002912005700000087320      " +
"Q34915138000  051815SV9              0000023500000098000000000450000004904      " +
"Q4000013000030000310000033000252320           0000100000                        " +
"QE010790000003742009000000000740102000000149201030000002812010400000083320      " +
"Q54915138000  051815SV9112233        0000023500000002200000000780000004904      " +
"Q6000120000033000011000023000252320           0000100000                        " +
"QJ011050000003142010600000000710107000000141201080000002811010900000083120      " +
"Q78804091002XJ5  20135316ABIXJ5  20135530ABIXJ5  20135647CBP                    " +
"Y  4904SV9PF00006                                                               ";
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			return (message, GlbBranch.CurrentBranch.PK, statement.PK, "3901138000", new[] { $"Statement:XJ5-3901138000|{GlbCompany.CurrentCompany.PK}" });
		}

		public void TestEntryStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_DeclarationReference = "2013576AAA";
			declaration.ImportEntryNumber = "20135357";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var message = CreateInterchangeAndMessageResponse(ACEApplicationIdentifierCodeList.Codes.DailyStatement,
				"A             051815     PF                                                     ",
				"B018888XJ5PFP4915138000050207313-262203600                                      " +
				"Q18887XJ5  20135357  13-1479270000518150000023500000000000000NB00163388      01 " +
				"Q24904SV9  20135357 0000000000000000000000           2Y      453                " +
				"QA0149900000017320                                                              " +
				"Y  4904SV9PF00006                                                               ",
				"Z             051815                                                            ", "");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var processor = GetNewIncomingMessageProcessor();
			processor.ExecuteBatch();

			declaration.Reload();
			var statementLine = Factory.LoadTop1<CusStatementLine>(new ZQuery(CusStatementLineSchema.B3_EntryNum, "20135357"));

			statementLine.Reload();
			AssertEquals(StatementEntryStatus.Codes.Paperless, statementLine.B3_EntryStatus);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			message = CreateInterchangeAndMessageResponse(ACEApplicationIdentifierCodeList.Codes.DailyStatement,
				"A             051815     PF                                                     ",
				"B018888XJ5PFP4915138000050207313-262203600                                      " +
				"Q18887XJ5  20135357  13-1479270000518150000023500000000000000NB00163388      01 " +
				"Q24904SV9  20135357 0000000000000000000000           2Y      453                " +
				"QA0149900000017320                                                              " +
				"Y  4904SV9PF00006                                                               ",
				"Z             051815                                                            ", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor = GetNewIncomingMessageProcessor();
			processor.ExecuteBatch();

			declaration.Reload();
			statementLine = Factory.LoadTop1<CusStatementLine>(new ZQuery(CusStatementLineSchema.B3_EntryNum, "20135357"));

			statementLine.Reload();
			AssertEquals(StatementEntryStatus.Codes.Paperless, statementLine.B3_EntryStatus);
		}

		public void TestStatementLineCharges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_DeclarationReference = "2013576AAA";
			declaration.ImportEntryNumber = "20135357";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();

			var message = CreateInterchangeAndMessageResponse(ACEApplicationIdentifierCodeList.Codes.DailyStatement,
					"A             051815     PF                                                     ",
					"B018888XJ5PFP4915138000050207313-262203600                                      " +
					"Q18887XJ5  20135357  13-1479270000518150000023500000000000000NB00163388      01 " +
					"Q24904SV9  20135357 0000012340000000085600           2Y      453                " +
					"QA01499000000173200530000008510007900000102510                                  " +
					"Y  4904SV9PF00006                                                               ",
					"Z             051815                                                            ", "");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var processor = GetNewIncomingMessageProcessor();
			processor.ExecuteBatch();

			declaration.Reload();
			var statementLine = Factory.LoadTop1<CusStatementLine>(new ZQuery(CusStatementLineSchema.B3_EntryNum, "20135357"));

			statementLine.Reload();

			var lineCharge = statementLine.Charges.GetFirstCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			AssertEquals("Merchandise Processing Fee", 173.2m, lineCharge.B4_ChargeAmount);
			lineCharge = statementLine.Charges.GetFirstCharge(Core.Constants.USCustoms.FeeCodes.Beef);
			AssertEquals("Beef Fee", 851m, lineCharge.B4_ChargeAmount);
			lineCharge = statementLine.Charges.GetFirstCharge(Core.Constants.USCustoms.FeeCodes.Sugar);
			AssertEquals("Sugar Fee", 1025.1m, lineCharge.B4_ChargeAmount);
			lineCharge = statementLine.Charges.GetFirstCharge(Core.Constants.USCustoms.FeeCodes.CountervailingDuty);
			AssertEquals("Countervailing Duty", 856m, lineCharge.B4_ChargeAmount);
			lineCharge = statementLine.Charges.GetFirstCharge(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty);
			AssertEquals("Antidumping Duty", 1234m, lineCharge.B4_ChargeAmount);
		}

		public void TestACEDailyStatementProcessingQ7Block()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var message = CreateInterchangeAndMessageResponse(ACEApplicationIdentifierCodeList.Codes.DailyStatement,
				"A             051815     PF                                                     ",
				"B018888XJ5PFP4915138000050207313-262203600                                      " +
				"Q18887XJ5  20135357  13-1479270000518150000023500000000000000NB00163388      01 " +
				"Q24904SV9  20135357 0000000000000000000000           2Y      453                " +
				"Q78804091001XJ5  20135357ABIXJ5  20135358BKR                                    " +
				"Y  4904SV9PF00006                                                               ",
				"Z             051815                                                            ", "");

			var processor = GetNewIncomingMessageProcessor();
			processor.ExecuteBatch();

			declaration.Reload();
			var statementLine = Factory.LoadTop1<CusStatementLine>(new ZQuery(CusStatementLineSchema.B3_EntryNum, "20135357"));

			statementLine.Reload();
			AssertEquals("The statement line has been deleted", StatementLineStatusList.Codes.Deleted, statementLine.B3_Status);
			AssertEquals("Delete Source is ABI", "ABI", statementLine.B3_DeletedByParty);
		}

		public void TestACEDailyStatementMessageProcessing()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var message = CreateInterchangeAndMessageResponse(ACEApplicationIdentifierCodeList.Codes.DailyStatement,
				"A             051815     PF                                                     ",
			"B018888XJ5PFF3901138000050207919-262203600                                      " +
			"Q18887XJ5  00000360  13-1479270000518150000023500000000000000NB00001160      01 " +
			"Q24904SV9  71009847 0001100003300002200000           2Y      453                " +
			"QA010530000001732005400000000320055000000189200560000002912005700000087320      " +
			"Q34915138000  051815SV9              0000023500000098000000000450000004904      " +
			"Q4000013000030000310000033000252320           0000100000                        " +
			"QE010790000003742009000000000740102000000149201030000002812010400000083320      " +
			"Q54915138000  051815SV9112233        0000023500000002200000000780000004904      " +
			"Q6000120000033000011000023000252320           0000100000                        " +
			"QJ011050000003142010600000000710107000000141201080000002811010900000083120      " +
			"Q78804091002XJ5  20135316ABIXJ5  20135530ABIXJ5  20135647CBP                    " +
			"Y  4904SV9PF00006                                                               ",
			"Z             051815                                                            ", "");

			var processor = GetNewIncomingMessageProcessor();
			processor.ExecuteBatch();

			var statement = new BusinessObjectFactory().LoadTop1<CusStatementHeader>(new ZQuery(CusStatementHeaderSchema.B2_StatementNumber, "3901138000"));
			AssertEquals("Total Amount should be taken Q6 for a final statement", 230002523.20m, statement.B2_StatementAmount);
		}

		protected override void EndToEndCore()
		{
			var message = CreateInterchangeAndMessageResponse(ACEApplicationIdentifierCodeList.Codes.DailyStatement, "A             051815     PF                                                     ", "B018888XJ5PFP4915138000050207313-262203600                                      Q18887XXX  20135761  13-1479270000518150000023500000000000000NB00163388      01 Q24904SV9  71009847 0000000000000000000000           2Y      453                QA0149900000017320                                                              Q34915138000  051815SV9              0000023500000000000000000000000004904      Q4000000000000000000000000000252320           0000100000                        QE0149900000017320                                                              Y  4904SV9PF00006                                                               ", "Z             051815                                                            ", "");

			var processor = GetNewIncomingMessageProcessor();
			processor.ExecuteBatch();

			message.Reload();
			AssertEquals("PreCondition:Processed", EDIMessage.Status.Received, message.EM_Status);
		}

		protected override IncomingMessageProcessor GetNewIncomingMessageProcessor() => new USRIncomingMessageProcessor();

		protected override string ApplicationIdentifier
		{
			get { return ACEApplicationIdentifierCodeList.Codes.DailyStatement; }
		}

		protected override string MessageText
		{
			get { return "B018888XJ5PFP4915138000050207313-262203600                                      Q18887XXX  20135761  13-1479270000518150000023500000000000000NB00163388      01 Q24904SV9  71009847 0000000000000000000000           2Y      453                QA0149900000017320                                                              Q34915138000  051815SV9              0000023500000000000000000000000004904      Q4000000000000000000000000000252320           0000100000                        QE0149900000017320                                                              Y  4904SV9PF00006                                                               "; }
		}

		protected override string StatementNumber
		{
			get { return "4915138000"; }
		}
	}
}
