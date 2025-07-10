using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	sealed class StowPlanIncomingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestPreProcessing()
		{
			var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "bob@where.com";
			var group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			group.Staff.Add(currentStaff);
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "Z!Z";
			branch1.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "Z!2";
			branch2.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(2);
			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "USCHI";
			destination1.JB_E_ARV = ZDateTime.Today.AddDays(10);
			var destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "USLAX";
			destination2.JB_E_ARV = ZDateTime.Today.AddDays(2);
			voyage.GenerateSailings();
			var originalMessage = Factory.New<StowPlanMessageTestClass>();
			originalMessage.EM_GB = branch1.PK;
			originalMessage.EM_MessageText = sentMessageText;
			originalMessage.EM_ReceiveTransmit = StowPlanMessage.Direction.Transmit;
			originalMessage.EM_LinkedObject = destination1;
			var originalMessage2 = Factory.New<StowPlanMessageTestClass>();
			originalMessage2.EM_GB = branch2.PK;
			originalMessage2.EM_MessageText = sentMessageText;
			originalMessage2.EM_ReceiveTransmit = StowPlanMessage.Direction.Transmit;
			originalMessage2.EM_LinkedObject = destination2;
			originalMessage2.EM_ApplicationCode = "K@K";
			var originalMessage3 = Factory.New<StowPlanMessageTestClass>();
			originalMessage3.EM_GB = branch2.PK;
			originalMessage3.EM_MessageText = sentMessageText;
			originalMessage3.EM_ReceiveTransmit = StowPlanMessage.Direction.Transmit;
			originalMessage3.EM_LinkedObject = destination2;
			Factory.Save();
			originalMessage.EM_MessageNum = "DN1234567890";
			originalMessage2.EM_MessageNum = "DN1234567890";
			originalMessage3.EM_MessageNum = "DN1234567891";
			Factory.Save();
			var receivedMsgText = "UNH+4+CUSRES:D:05B:UN'BGM+294+{0}'TDT+20+VOY444+1++8CAR:172:ZZZ:MAERSK ENFIELD+++9463047:146:11:MAERSK ENFIELD'RFF+AAA'DTM+133:2109210000'LOC+5+CAAAB'RFF+AAA'DTM+132:2109290000'LOC+61+USCHI'ERP+1'ERC+S02'FTX+AAH+++ACCEPTED'UNT+31+4";
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.StowPlan;
			interchange.EI_From = "USCBPTST";
			interchange.EI_To = "8CAR";
			interchange.EI_HeaderText = "UNB+UNOA:4+USCBPTST:ZZZ+8CAR+130128:2239+100000114586++++0'UNG+CUSRES+USCBPTST+8CAR+130128:2239+1086431+UN+D:05B'";
			interchange.EI_BodyText = string.Format(receivedMsgText, "");
			interchange.EI_FooterText = "UNE+1+1086431'UNZ+1+100000114586'";
			var message = Factory.New<StowPlanMessageTestClass>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_EI = interchange.PK;
			message.EM_MessageNum = "";
			message.EM_MessageText = string.Format(receivedMsgText, message.EM_MessageNum);
			Factory.Save();
			var stowPlanProcessor = new StowPlanIncomingMessageProcessor(new LoggingInformation());
			stowPlanProcessor.ExecuteBatch();
			message.Reload();
			AssertEquals("EM_GB should not changed as message number is empty", GlbBranch.CurrentBranch.PK, message.EM_GB);
			AssertNull("EM_LinkedObject should not changed as message number is empty", message.EM_LinkedObject);
			AssertEquals("EM_LinkTable should not changed as message number is empty", ZString.Empty, message.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID should not changed as message number is empty", ZGuid.Empty, message.EM_LinkUniqueID);
			AssertEquals("AfterNewObjectIsLinkedCalledCountForTesting should not changed as message number is empty", 0, message.AfterNewObjectIsLinkedCalledCountForTesting);
			message.EM_MessageNum = "DN1234567892";
			message.EM_MessageText = string.Format(receivedMsgText, message.EM_MessageNum);
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.Save();
			stowPlanProcessor.ExecuteBatch();
			message.Reload();
			AssertEquals("EM_GB should not changed as no original message match", GlbBranch.CurrentBranch.PK, message.EM_GB);
			AssertNull("EM_LinkedObject should not changed as no original message match", message.EM_LinkedObject);
			AssertEquals("EM_LinkTable should not changed as no original message match", ZString.Empty, message.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID should not changed as no original message match", ZGuid.Empty, message.EM_LinkUniqueID);
			AssertEquals("AfterNewObjectIsLinkedCalledCountForTesting should not changed as no original message match", 0, message.AfterNewObjectIsLinkedCalledCountForTesting);
			var message2 = Factory.New<StowPlanMessageTestClass>();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_Status = EDIMessageStatusList.Codes.Queued;
			message2.EM_EI = interchange.PK;
			message2.EM_MessageNum = "DN1234567890";
			message2.EM_MessageText = string.Format(receivedMsgText, message2.EM_MessageNum);
			Factory.Save();
			stowPlanProcessor.ExecuteBatch();
			message2.Reload();
			AssertEquals("EM_GB should have changed as OriginalMessage was matched", branch1.PK, message2.EM_GB);
			AssertSame("EM_LinkedObject should have changed as OriginalMessage was matched", destination1, message2.EM_LinkedObject);
			AssertEquals("EM_LinkTable should have changed as OriginalMessage was matched", destination1.TableName, message2.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID should have changed as OriginalMessage was matched", destination1.PK, message2.EM_LinkUniqueID);
			AssertEquals("AfterNewObjectIsLinkedCalledCountForTesting should have changed as OriginalMessage was matched", 1, message2.AfterNewObjectIsLinkedCalledCountForTesting);
			message2.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.Save();
			stowPlanProcessor.ExecuteBatch();
			message2.Reload();
			AssertEquals("EM_GB", branch1.PK, message2.EM_GB);
			AssertSame("EM_LinkedObject", destination1, message2.EM_LinkedObject);
			AssertEquals("EM_LinkTable", destination1.TableName, message2.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", destination1.PK, message2.EM_LinkUniqueID);
			AssertEquals("AfterNewObjectIsLinkedCalledCountForTesting should not have changed EM_LinkedObject was not changed", 1, message2.AfterNewObjectIsLinkedCalledCountForTesting);
		}

		public void TestEndToEnd()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ZZ1";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "Z1";
			staff.GS_LoginName = "z1";
			staff.GS_EmailAddress = "tv@pretend.email.com";

			var user = Factory.New<GlbStaff>();
			user.GS_EmailAddress = "tim@yahoo.com";
			user.GS_Code = "TV";
			Factory.Save();

			FreightDataRegistry.Instance.USAMSGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));

			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(2);
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(10);
			voyage.GenerateSailings();

			var messageTransmit = Factory.New<StowPlanMessage>();
			messageTransmit.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			messageTransmit.EM_LinkedObject = destination;
			messageTransmit.EM_MessageText = "A " + EDIMessage.MessageNumberPlaceHolder + " B";
			messageTransmit.EM_SystemCreateUser = user.GS_Code;

			Factory.Save();

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.StowPlan;
			interchange.EI_From = "USCBPTST";
			interchange.EI_To = "8CAR";
			interchange.EI_HeaderText = "UNB+UNOA:4+USCBPTST:ZZZ+8CAR+130128:2239+100000114586++++0'UNG+CUSRES+USCBPTST+8CAR+130128:2239+1086431+UN+D:05B'";
			interchange.EI_BodyText = "UNH+1086431+CUSRES:D:05B:UN'BGM+294+" + messageTransmit.EM_MessageNum + "'TDT+20+1299+1++APLU:172:ZZZ:APL BAHRAIN+++9395927:146:11:APL BAHRAIN'RFF+AAA'DTM+133:20120420'LOC+5+USBAL'RFF+AAA'DTM+132:20120430'LOC+61'ERP+1'ERC+SB4'FTX+AAH+++VESSEL IMO NOT ON FILE'ERP+1'ERC+SBB'FTX+AAH+++VESSEL ARRIVAL DATE IN THE PAST'ERP+1'ERC+SB8'FTX+AAH+++MISSING VESSEL ARRIVAL PORT'ERP+1'ERC+SBT'FTX+AAH+++VESSEL DEPARTURE DATE MORE THAN 30 DAYS IN THE PAST'ERP+1'ERC+S01'FTX+AAH+++REJECTED'UNT+25+1086431'";
			interchange.EI_FooterText = "UNE+1+1086431'UNZ+1+100000114586'";

			var message = Factory.New<StowPlanMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageText = interchange.EI_BodyText;

			Factory.Save();

			new StowPlanIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			Assert(email.Recipients.Contains("tim@yahoo.com"));
			Assert(email.CCRecipients.Contains("tv@pretend.email.com"));
			Assert(email.Body.Contains("VESSEL IMO NOT ON FILE"));
			Assert(email.Body.Contains("VESSEL ARRIVAL DATE IN THE PAST"));
			Assert(email.Body.Contains("MISSING VESSEL ARRIVAL PORT"));
			Assert(email.Body.Contains("VESSEL DEPARTURE DATE MORE THAN 30 DAYS IN THE PAST"));
			AssertEquals(MessageStatusListSTW.Codes.Rejection, destination.StowPlanMessageStatus);
		}

		readonly string sentMessageText = @"UNH+8074+BAPLIE:D:95B:UN:SMDG20
BGM++8074+9
DTM+137:130812:101
TDT+20+1299+++ALPU:172:166+++2111111:146:11:NVO STEP 1
LOC+5+AUMEL:139:6
LOC+61+USLAX:139:6
DTM+133:130613:101
DTM+132:130616:101
LOC+147+1112233::5
MEA+WT++KG:3830
LOC+9+DEHAM
LOC+11+NZABY
EQD+CN+ANLU8463790+:::42G0
NAD+CA+0UAFORLON:172:166
DGS+IMD+0004A
LOC+147+3331122::5
MEA+WT++KG:3832
LOC+9+DEHAM
LOC+11+NZABY
EQD+CN+MOLU0383939+:::42G0
NAD+CA+0UAFORLON:172:166
DGS+IMD+0004A
UNT+23+8074".Replace("\r\n", "'");
		sealed class StowPlanMessageTestClass : StowPlanMessage
		{
			public StowPlanMessageTestClass(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public int AfterNewObjectIsLinkedCalledCountForTesting;
			protected override void AfterNewObjectIsLinked(BusinessObject newBizObj)
			{
				base.AfterNewObjectIsLinked(newBizObj);
				AfterNewObjectIsLinkedCalledCountForTesting++;
			}
		}
	}
}
