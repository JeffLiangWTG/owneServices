using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors.Statement.Testing
{
	public abstract class CommonDailyStatementProcessorTest<T, ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : StatementProcessorTest<T, ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
			where T : StatementProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
			where ControlMessageBlockA : MessageBlock, IABIControlMessageBlockA, IControlMessageBlockA, new()
			where ControlMessageBlockB : MessageBlock, IABIControlMessageBlockB, IControlMessageBlockB, new()
			where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		public override void TestCreateNewCusStatementHeaderFilter()
		{
			AssertNotNull(loader.CreateNewCusStatementHeaderFilter("XJ5", "8804P04001", GlbCompany.CurrentCompany.PK));
		}

		public override void TestGetCusStatementHeader()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_EntryFilerCode = "XJ5";
			header.B2_ProcessPort = "8888";
			header.B2_StatementNumber = "8804P04001";
			Factory.Save();

			AssertEquals(header.B2_StatementNumber, loader.LoadWithStatementNumber("XJ5", "8804P04001", GlbCompany.CurrentCompany.PK).B2_StatementNumber);
		}

		public void TestPFStatementType()
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
				"Q24904SV9  20135357 0000000000000000000000           2Y     I453                " +
				"QA0149900000017320                                                              " +
				"Y  4904SV9PF00006                                                               ",
				"Z             051815                                                            ", "");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var processor = GetNewIncomingMessageProcessor();
			processor.ExecuteBatch();

			var statement = Factory.LoadTop1<CusStatementHeader>(new ZQuery(CusStatementHeaderSchema.B2_StatementNumber, "4915138000"));

			AssertEquals(StatementTypeList.Codes.ACE, statement.B2_StatementType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			USCustomsDataRegistry.Instance.DailyStatementsMessagesGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new GroupNotification(GroupNotification.StaffMemberOrNominatedGroup, groupZZ1.PK));
			Factory.Save();

			loader = new CusStatementHeader.Loader(Factory);
		}

		protected override IncomingMessageProcessor GetNewIncomingMessageProcessor() => new ABIIncomingMessageProcessor();

		CusStatementHeader.Loader loader;
	}
}
