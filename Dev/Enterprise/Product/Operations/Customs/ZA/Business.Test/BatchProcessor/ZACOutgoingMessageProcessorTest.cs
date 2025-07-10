using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.BatchProcessor.Testing
{
	sealed class ZACOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestDeferredMessagesAreBeingDeferred()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.Branch.Company.OrgProxy.SetAgentCode(declaration.Branch.Company.Country, "1234");
			AssertEquals("1234", declaration.AgentCode);
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.FillWithValidTestData();
			var message1 = entryHeader.Messages.AddNew(typeof(CUSDECEDIMessage));
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message1.EM_GB = GlbBranch.CurrentBranch.PK;
			message1.EM_MessageText = "UNH+1+CUSDEC:D:96B:UN:ZZZ01'";
			message1.EM_MessageOwner = "";
			message1.EM_IsTestMessage = true;
			message1.EM_MessageNum = "1";
			message1.EM_HeldUntilDate = ZDate.Today.AddDays(1);
			Factory.Save();
			var logger = new LoggingInformation();
			var processor = new ZACOutgoingMessageProcessor(logger);
			processor.ProcessMessage(CancellationToken.None);
			var interchangesWhenDeferred = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("Number of Interchanges when deferred", 0, interchangesWhenDeferred.Length);
			message1.EM_HeldUntilDate = ZDate.Today;
			Factory.Save();
			processor.ProcessMessage(CancellationToken.None);
			var interchangesNotDeferred = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("Number of Interchanges when not deferred", 1, interchangesNotDeferred.Length);
		}

		[TestDate(2015, 10, 30, 09, 36, 0)]
		public void TestCreateNewInterchangeProvider()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.Branch.Company.OrgProxy.SetAgentCode(declaration.Branch.Company.Country, "1234");
			AssertEquals("1234", declaration.AgentCode);
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.FillWithValidTestData();
			var message1 = entryHeader.Messages.AddNew(typeof(CUSDECEDIMessage));
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message1.EM_GB = GlbBranch.CurrentBranch.PK;
			message1.EM_MessageText = "UNH+1+CUSDEC:D:96B:UN:ZZZ01'";
			message1.EM_MessageOwner = "";
			message1.EM_IsTestMessage = true;
			message1.EM_MessageNum = "1";
			Factory.Save();
			var logger = new LoggingInformation();
			var processor = new ZACOutgoingMessageProcessor(logger);
			processor.ProcessMessage(CancellationToken.None);
			CombineAssertions(() =>
			{
				var interchangesCreated = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("NumberOfInterchanges", 1, interchangesCreated.Length);
				var interchange = interchangesCreated[0];
				AssertEquals("Header", "UNB+UNOB:4+1234::SENDERID:SENDERSUBID+SARSDECT+20151030:0936+1++CUSDEC++1+TRADINGPARTNER+1'\n", interchange.EI_HeaderText);
				AssertEquals("Footer", "UNZ+1+1'", interchange.EI_FooterText);
				AssertEquals("Body", "UNH+1+CUSDEC:D:96B:UN:ZZZ01'\n", interchange.EI_BodyText);
				AssertEquals("Status", "HQU", interchange.EI_Status);
			});
		}

		[TestDate(2020, 05, 20, 09, 18, 0)]
		public void TestCreateNewInterchange_for_CALINF_EdiMessage()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.Branch.Company.OrgProxy.SetAgentCode(declaration.Branch.Company.Country, "1234");
			AssertEquals("1234", declaration.AgentCode);
			var voyage = Factory.NewWithValidTestData<Freight.Business.JobVoyage>();
			var message1 = voyage.Messages.AddNew(typeof(CALINFEDIMessage));
			AssertEquals("Prerequisite: EM_MessageType", SARSEDIMessage.MessageTypes.CALINF, message1.EM_MessageType);
			AssertEquals("Prerequisite: EdiMessage Direction", EDIInterchange.Direction.Transmit, message1.EM_ReceiveTransmit);
			AssertEquals("Prerequisite: EM_LinkTable", Freight.Business.JobVoyage.Schema.TableName, message1.EM_LinkTable);
			AssertEquals("Prerequisite: EM_LinkUniqueID", voyage.PK, message1.EM_LinkUniqueID);
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message1.EM_GB = GlbBranch.CurrentBranch.PK;
			message1.EM_MessageText = "UNH+1+CALINF:D:16A:UN:RCG001'";
			message1.EM_MessageOwner = "";
			message1.EM_IsTestMessage = true;
			message1.EM_MessageNum = "1";
			Factory.Save();
			var logger = new LoggingInformation();
			var processor = new ZACOutgoingMessageProcessor(logger);
			processor.ProcessMessage(CancellationToken.None);
			Factory.Save();
			var interchangesCreated = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("NumberOfInterchanges", 1, interchangesCreated.Length);
			var interchange = interchangesCreated[0];
			AssertEquals("Interchange Footer", "UNZ+1+1'", interchange.EI_FooterText);
			AssertEquals("Interchange Body", "UNH+1+CALINF:D:16A:UN:RCG001'\n", interchange.EI_BodyText);
			AssertEquals("Interchange Status", "HQU", interchange.EI_Status);
			AssertEquals("Interchange Type", SARSEDIMessage.MessageTypes.CALINF, interchange.EI_InterchangeType);
			AssertEquals("Interchange Direction", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals("Interchange Application Code", "ZAC", interchange.EI_ApplicationCode);
			AssertEquals("Interchange Transport Type", "HUB", interchange.EI_TransportType);
		}

		protected override void SetUp()
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);
			base.SetUp();
		}
	}
}
