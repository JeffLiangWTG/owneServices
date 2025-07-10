using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Business.MessageProcessors.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.Processor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors.Statement.Testing
{
	public abstract class StatementProcessorTest<T, ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIProcessorTest<T, ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
			where T : StatementProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
			where ControlMessageBlockA : MessageBlock, IABIControlMessageBlockA, IControlMessageBlockA, new()
			where ControlMessageBlockB : MessageBlock, IABIControlMessageBlockB, IControlMessageBlockB, new()
			where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		public abstract void TestCreateNewCusStatementHeaderFilter();
		public abstract void TestGetCusStatementHeader();
		protected abstract string ApplicationIdentifier { get; }
		protected abstract string MessageText { get; }
		protected abstract string StatementNumber { get; }

		public void TestGetKeysForBlockingParallelProcessing()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.DailyStatement;
			message.EM_MessageText =
"B018888XJ5PFF3901138000050207919-262203600                                      " +
"Y  4904SV9PF00006                                                               ";
			message.EM_Status = EDIMessage.Status.Queued;
			var logger = new LoggingInformation();
			var statementProcessor = Activator.CreateInstance<T>();
			var provider = statementProcessor as IKeysForBlockingParallelProcessingProvider;
			var actualMetaData = provider.GetLinkedBusinessObjectMetaData(message, logger);
			var actualKeys = provider.GetSerializationKeysResult(message, logger, actualMetaData.ReturnValue);
			CombineAssertions("Message Can not find statement", () =>
			{
				AssertEquals("Meta data", ProcessingResult.New(LinkedBusinessObjectMetaData.New(ZString.Empty, ZGuid.Empty, GlbBranch.CurrentBranch.PK, "3901138000")), actualMetaData);
				AssertArrayEqualsByElements("Keys", new[] { $"Statement:XJ5-3901138000|{GlbCompany.CurrentCompany.PK}" }, actualKeys.ReturnValue.Keys.ToArray());
			});

			var data = GenerateInfoToTestGetKeysForBlockingParallelProcessing();
			actualMetaData = provider.GetLinkedBusinessObjectMetaData(data.message, logger);
			actualKeys = provider.GetSerializationKeysResult(data.message, logger, actualMetaData.ReturnValue);
			CombineAssertions("Message link to statement", () =>
			{
				AssertEquals("Meta data", ProcessingResult.New(LinkedBusinessObjectMetaData.New(CusStatementHeader.Schema.TableName, data.expectedLinkUniqueID, data.expectedBranchPK, data.expectedJobNumber)), actualMetaData);
				AssertArrayEqualsByElements("Keys", data.expectedKeys, actualKeys.ReturnValue.Keys.ToArray());
			});
		}

		protected abstract (EDIMessage message, ZGuid expectedBranchPK, ZGuid expectedLinkUniqueID, ZString expectedJobNumber, string[] expectedKeys) GenerateInfoToTestGetKeysForBlockingParallelProcessing();

		public void TestGetImporterPKWhenDuplicateOrganisationExists()
		{
			SetUpTestData();
			Factory.Save();

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifier, "A3901SV9      05140701   051407014539                                00000000039", MessageText, "Z3901SV9      05140701   051407014539                                00000000039", "");
			processor.ExecuteBatch();

			message.Reload();
			AssertEquals("PreCondition:Processed", EDIMessage.Status.Received, message.EM_Status);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var statement = factory2.LoadTop1<CusStatementHeader>(new ZQuery(CusStatementHeaderSchema.B2_StatementNumber, StatementNumber));

			AssertEquals("Right IOR is hooked up", importer2.PK, statement.B2_OH_Importer);
		}

		protected abstract IncomingMessageProcessor GetNewIncomingMessageProcessor();

		protected virtual void SetUpTestData()
		{
			var validEINNumber = "13-262203600";
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, validEINNumber);

			importer2 = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer2.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_SchDEntry = "8887";
			declaration.US_EntryFilerCode = "XXX";
			declaration.ShouldDeclarationReferenceBePopulatedFromXml = true;
			declaration.JE_DeclarationReference = "AAA2013576";//10 length
			declaration.ImportEntryNumber = "20135761";
			declaration.IOROrgPK = importer2.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_EntryFilerCode = "XXX";
			reconDeclaration.ReconEntry.GetEntry().EntryNumber = "20135748";
			reconDeclaration.IOROrgPK = importer2.PK;
		}
		OrgHeader importer2;

		protected override void SetUp()
		{
			base.SetUp();
			processor = GetNewIncomingMessageProcessor();
		}

		protected IncomingMessageProcessor processor { get; private set; }
	}
}
