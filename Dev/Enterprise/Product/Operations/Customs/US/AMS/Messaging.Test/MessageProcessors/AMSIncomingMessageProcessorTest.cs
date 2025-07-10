using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	sealed class AMSIncomingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageOrderAndMessageShouldBeProcessedInASeparateFactory()
		{
			var processor = new AMSIncomingMessageProcessor_MessageOrder();
			var query = processor.GetMessageProcessorQueryExposed();
			var hint = query.TableIndexHints.Single();
			AssertEquals("EM_SystemCreateTimeUtc, EM_MessageNum", processor.SortOrder);
			AssertEquals("NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_SystemCreateTimeUtc_EM_MessageNum", hint.IndexName);
			Assert(!processor.MessageShouldBeProcessedInASeparateFactory_Exposed);

			var message1 = Factory.New<AMSEDIMessage>();
			message1.EM_GB = GlbBranch.CurrentBranch.PK;
			message1.EM_MessageNum = "10002";
			message1.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message1.EM_Status = AMSEDIMessage.Status.Queued;
			Factory.Save();
			System.Threading.Thread.Sleep(1000);
			var message2 = Factory.New<AMSEDIMessage>();
			message2.EM_GB = GlbBranch.CurrentBranch.PK;
			message2.EM_MessageNum = "10001";
			message2.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message2.EM_Status = AMSEDIMessage.Status.Queued;
			Factory.Save();
			using (var messages = processor.GetEDIMessages())
			{
				AssertEquals(2, messages.ItemsInBatchCount);
				AssertEquals(message1.PK, messages[0].PK);
				AssertEquals(message2.PK, messages[1].PK);
			}
		}

		public void TestMessageProcessorProcessesMessage()
		{
			var omgCompany = Factory.New<GlbCompany>();
			omgCompany.GC_Code = "OMg";
			omgCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var orgBranch = omgCompany.Branches.AddNew();
			orgBranch.GB_Code = "O#G";
			orgBranch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var consol = Factory.New(ObjectFactory.GetType<Integration.Forwarding.IForwardingConsol>());
			consol[JobConsolSchema.JK_TransportMode] = Core.Constants.TransportModes.Sea;
			var header = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_CarrierSCAC = "OTT1";
			header.BH_GB = orgBranch.PK;
			var bill = Factory.New<Integration.Customs.US.USAMS.ICusInBondBill>();
			bill.B0_BH = header.PK;
			var moveHeader = Factory.New<Integration.Customs.US.USAMS.ICusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			var sentInterchange = Factory.New<EDIInterchange>();
			sentInterchange.EI_SessionGUID = ZGuid.NewZGuid();
			sentInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.USAMS;
			sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			sentInterchange.EI_From = "OTT1";
			sentInterchange.EI_To = "HYEDUSCMT";
			var sentMessage = Factory.New<AMSEDIMessage>();
			sentMessage.EM_LinkUniqueID = moveHeader.PK;
			sentMessage.EM_LinkTable = CusInBondMoveHeaderSchema.Constants.TableName;
			sentMessage.EM_GB = orgBranch.PK;
			sentMessage.EM_MessageNum = "103";
			sentMessage.EM_ApplicationReference = bill.PK.ToString();
			sentInterchange.ContainedMessages.Add(sentMessage);
			var otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "Z!Z";
			otherCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var otherBranch = otherCompany.Branches.AddNew();
			otherBranch.GB_Code = "Z!Z";
			otherBranch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var currentStaff = Factory.Load<GlbStaff>(MasterFiles.Business.GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "bob@where.com";
			var group = Factory.Load<GlbGroup>(Enterprise.Environment.Env.Registry.PostMasterGroup);
			group.Staff.Add(currentStaff);
			var otherCompanyMessage = CreateTestMessage("103");
			otherCompanyMessage.EM_MessageType = "Z@";
			otherCompanyMessage.EM_MessageText = "ZZ";
			otherCompanyMessage.EM_GB = otherBranch.PK;
			Factory.Save();
			ErrorReporter.Clear();
			new AMSIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch();
			var otherFactory = new BusinessObjectFactory();
			var message2 = otherFactory.Load<AMSEDIMessage>(otherCompanyMessage.PK);
			AssertEquals("Should process as part of this company", AMSEDIMessage.Status.Failed, message2.EM_Status);
			AssertEquals("Should have branch from the original message", message2.EM_GB, orgBranch.PK);
			AssertEquals("Should have ApplicationReference from the original message", message2.EM_ApplicationReference, bill.PK.ToString());
		}

		AMSEDIMessage CreateTestMessage(string messageNum)
		{
			var testMessage = Factory.New<AMSEDIMessage>();
			testMessage.EM_Status = AMSEDIMessage.Status.Queued;
			testMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			testMessage.EM_MessageNum = messageNum;
			return testMessage;
		}

		class AMSIncomingMessageProcessor_MessageOrder : AMSIncomingMessageProcessor
		{
			public AMSIncomingMessageProcessor_MessageOrder() : base(new LoggingInformation())
			{
			}

			public string SortOrder => GetProcessableMessagesOrder();

			public bool MessageShouldBeProcessedInASeparateFactory_Exposed => base.MessageShouldBeProcessedInASeparateFactory;

			public ZQuery GetMessageProcessorQueryExposed() => GetMessageProcessorQuery();

			public DisposableBatch GetEDIMessages()
			{
				return RetrieveNextProcessableMessages(base.DequeueMessages, new FailedMessagesManager(RetryType.NextExecution, ShouldRetryOnException));
			}
		}
	}
}
