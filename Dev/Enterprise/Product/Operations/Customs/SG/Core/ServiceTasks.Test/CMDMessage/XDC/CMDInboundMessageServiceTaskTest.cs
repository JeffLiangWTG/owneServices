using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.SG.V4.ServiceTasks.Testing
{
	[TestedType(typeof(CMDInboundMessageServiceTask))]
	sealed class CMDInboundMessageServiceTaskTest : ServiceTaskTestCase<CMDInboundMessageServiceTask>
	{
		[TestDate(2021, 1, 1, 0, 0, 0)]
		public void TestProcessMessage()
		{
			const string fnaMessage = @"<CMD>FNA
ACK/RECIPIENT NOT CONFIGURED TO RECEIVE THIS MESSAGE TYPE/VERSION
CMD/2
A/N/Y
MWB/081-12345675SYDSIN/T11K1001
/AS PER MANIFEST
FLT/QF410/09MAR
HWB/46546351/6/K501
/JOUMAAAAAA/76031000
SHP/YOUR AUSTRALIA COMPANY
/105 O RIORDAN STREET
CNE/CHIP FORWARDING
/1 STREET
TPT/TDB/IM0C108288R
DUI/N
SND/CargoWise Support/YOUR SINGAPORE CORP
/198801949D
/234234/23432
</CMD>
";
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "088-99898989";
			consol.JK_MasterBillNum = "08112345675";
			consol.JK_MasterBillIssueDate = ZDateTime.Now;
			var txMessage = Factory.New<CMDEDIMessage>();
			txMessage.EM_ApplicationReference = "088-99898989";
			txMessage.EM_LinkedObject = shipment;
			txMessage.EM_MessageType = EDIMessage.ApplicationCodes.SingaporeCMD;
			txMessage.EM_MessageSubType = CMDGenerator.Constants.GHA;
			txMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			txMessage.EM_IsActive = true;
			var rxInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			rxInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			rxInterchange.EI_Status = EDIInterchange.Status.Queued;
			rxInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.SingaporeCMD;
			rxInterchange.EI_IsActive = true;
			var company = new GlbCompany.Loader(new BusinessObjectFactory()).LoadCompanies(Core.Constants.CountryCodes.Singapore)[0];
			rxInterchange.EI_GB = company.FirstActiveBranch.PK;
			var rxMessage = Factory.New<CMDEDIMessage>();
			rxMessage.EM_EI = rxInterchange.PK;
			rxMessage.EM_GB = rxInterchange.EI_GB;
			rxMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			rxMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			rxMessage.EM_Status = EDIMessage.Status.Queued;
			rxMessage.EM_MessageText = fnaMessage;
			SetupNotificationGroupEmails();
			Factory.Save();
			var logger = new TestServiceLogger();
			var task = new CMDInboundMessageServiceTask()
			{ ServiceLogger = logger };
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			string expectedLog = $@"Information|SCP Processing for Company {company.GC_Code}.
Information|	Processing Message #
Information|	Successfully processed inbound FNA message for MAWB '081-12345675' and HAWB '46546351'
Information|	Saving...
Information|	1 message processed
Information|SCP Processing for Company EDI.";
			AssertMultilineASCIIEquals("Logs", expectedLog, logger.ToString());
			rxMessage.Reload();
			AssertEquals("EM_Status", EDIMessage.Status.Received, rxMessage.EM_Status);
			var generatedMessage = LoadGeneratedMessage(rxInterchange, rxMessage);
			AssertEquals(EDIMessage.Status.Received, generatedMessage.EM_Status);
			AssertEquals(CMDInbound.Constants.FNA, generatedMessage.EM_MessageType);
			AssertEquals(txMessage.PK, generatedMessage.EM_LinkUniqueID);
		}

		[TestDate(2021, 1, 1, 0, 0, 0)]
		public void TestProcessMessage_OriginatingMessageNotFound()
		{
			const string ackMessage = @"
<CMD>CMA
ACK/MESSAGE RECEIVED SUCCESSFULLY
CMD/2
A/N/Y
</CMD>";
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.SingaporeCMD;
			interchange.EI_IsActive = true;
			var company = new GlbCompany.Loader(new BusinessObjectFactory()).LoadCompanies(Core.Constants.CountryCodes.Singapore)[0];
			interchange.EI_GB = company.FirstActiveBranch.PK;
			var message = Factory.New<CMDEDIMessage>();
			message.EM_EI = interchange.PK;
			message.EM_GB = interchange.EI_GB;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = ackMessage;
			SetupNotificationGroupEmails();
			Factory.Save();
			var logger = new TestServiceLogger();
			var task = new CMDInboundMessageServiceTask()
			{ ServiceLogger = logger };
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			string expectedLog = $@"Information|SCP Processing for Company {company.GC_Code}.
Information|	Processing Message #
Warning|	Did not find exactly one consol dated within the MAWB recycle period. Found 0. MAWB# was .
Warning|	Could not find originating message for inbound 'CMA' message for MAWB '' and HAWB ''. Ignoring.
Information|	Saving...
Information|	1 message processed
Information|SCP Processing for Company EDI.";
			AssertMultilineASCIIEquals("Logs", expectedLog, logger.ToString());
			message.Reload();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			var generatedMessage = LoadGeneratedMessage(interchange, message);
			AssertEquals(EDIMessage.Status.Failed, generatedMessage.EM_Status);
			AssertEquals(CMDInbound.Constants.CMA, generatedMessage.EM_MessageType);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						CMDInboundMessageServiceTask.CMDInboundXDCMessageServiceName,
						EDIMessageSchema.Constants.EM_IsActive        + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.SingaporeCMD,
						EDIMessageSchema.Constants.EM_MessageType     + "=" + EDIMessageTypeList.Codes.XDC),
				};
			}
		}

		EDIMessage LoadGeneratedMessage(EDIInterchange interchange, EDIMessage xdcMessage)
		{
			interchange.Reload();
			var generatedMessages = interchange.ContainedMessages.Where(x => x.PK != xdcMessage.PK);
			AssertEquals(1, generatedMessages.Count());
			return generatedMessages.First();
		}

		void SetupNotificationGroupEmails()
		{
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_EmailAddress = "test@test.edi.com.au";
			var postMaster = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, User.PostMasterUserName);
			postMaster.GS_EmailAddress = "postnotify@test.edi.com.au";
		}
	}
}
