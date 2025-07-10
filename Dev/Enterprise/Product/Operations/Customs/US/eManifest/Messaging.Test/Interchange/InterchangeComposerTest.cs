using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.Messaging.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using EDIMessage = Enterprise.Customs.US.eManifest.Business.EDIMessage;

namespace Enterprise.Customs.US.eManifest.Messaging.Interchange.Testing
{
	sealed class InterchangeComposerTest : TestCaseWithFactory
	{
		[TestDate(2012, 01, 17, 17, 20, 0)]
		public void TestComposeInterchangeProductionViaEHub()
		{
			const string expectedInterchangeText1 = "UNB+UNOA:4+MAN:ZZ+USC:ZZ+20120117:1720+1++ACE'UNG+CUSCAR+MAN:ZZ+USC:ZZ+20120117:1720+1+UN+D:03B'Message Text 1UNE+1+1'UNZ+1+1'";
			const string expectedInterchangeText2 = "UNB+UNOA:4+MAN:ZZ+USC:ZZ+20120117:1720+2++ACE'UNG+CUSCAR+MAN:ZZ+USC:ZZ+20120117:1720+2+UN+D:03B'Message Text 2UNE+1+2'UNZ+1+2'";
			var composer = new InterchangeComposer();
			composer.ExecuteBatch();
			var sendableInterchanges = GetSendableInterchanges();
			AssertEquals("1 interchange created", 2, sendableInterchanges.Length);
			AssertInterchange(sendableInterchanges[0], MessageTypes.Codes.eManifest, EDIInterchange.Status.eHubQueued, EDIInterchange.TransportType.eHub, GlbBranch.CurrentBranch.PK, SenderID, RecipientTestID, expectedInterchangeText1);
			AssertInterchange(sendableInterchanges[1], MessageTypes.Codes.eManifest, EDIInterchange.Status.eHubQueued, EDIInterchange.TransportType.eHub, GlbBranch.CurrentBranch.PK, SenderID, RecipientTestID, expectedInterchangeText2);
		}

		[TestDate(2012, 01, 17, 17, 20, 0)]
		public void TestComposeInterchangeTest()
		{
			var branch1 = GetNewBranch("0F18A929-C9B9-4049-9DAB-74D824C0D4D4");
			var branch2 = GetNewBranch("6C1090B7-DC5A-48FD-AA47-70B694F21335");
			message1.EM_GB = branch1.PK;
			message2.EM_GB = branch2.PK;
			var message3 = MessagingTestHelper.GetTransmitEDIMessage(Factory, MessageTypes.Codes.CrewAndPassenger);
			message3.EM_GB = branch2.PK;
			var message4 = MessagingTestHelper.GetTransmitEDIMessage(Factory, MessageTypes.Codes.UnassociatedShipments);
			message4.EM_GB = branch2.PK;
			var message5 = MessagingTestHelper.GetTransmitEDIMessage(Factory, MessageTypes.Codes.CompleteTrip);
			message5.EM_GB = branch2.PK;
			var message6 = MessagingTestHelper.GetTransmitEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip);
			message6.EM_GB = branch2.PK;
			var message7 = MessagingTestHelper.GetTransmitEDIMessage(Factory, MessageTypes.Codes.CrewOrEquipmentRegistration);
			message7.EM_GB = branch2.PK;
			Factory.Save();
			const string expectedInterchangeText1 = "UNB+UNOA:4+MAN:ZZ+USC:ZZ+20120117:1720+1++ACE'UNG+CUSCAR+MAN:ZZ+USC:ZZ+20120117:1720+1+UN+D:03B'Message Text 1UNE+1+1'UNZ+1+1'";
			const string expectedInterchangeText2 = "UNB+UNOA:4+MAN:ZZ+USC:ZZ+20120117:1720+2++ACE'UNG+CUSCAR+MAN:ZZ+USC:ZZ+20120117:1720+2+UN+D:03B'Message Text 2UNE+1+2'UNZ+1+2'";
			const string expectedInterchangeText3 = "UNB+UNOA:4+MAN:ZZ+USC:ZZ+20120117:1720+3++ACE'UNG+PAXLST+MAN:ZZ+USC:ZZ+20120117:1720+3+UN+D:03B'Message Text 3UNE+1+3'UNZ+1+3'";
			const string expectedInterchangeText4 = "UNB+UNOA:4+MAN:ZZ+USC:ZZ+20120117:1720+4++ACE'UNG+CUSCAR+MAN:ZZ+USC:ZZ+20120117:1720+4+UN+D:03B'Message Text 4UNE+1+4'UNZ+1+4'";
			const string expectedInterchangeText5 = "UNB+UNOA:4+MAN:ZZ+USC:ZZ+20120117:1720+5++ACE'UNG+CUSREP+MAN:ZZ+USC:ZZ+20120117:1720+5+UN+D:03B'Message Text 5UNE+1+5'UNZ+1+5'";
			const string expectedInterchangeText6 = "UNB+UNOA:4+MAN:ZZ+USC:ZZ+20120117:1720+6++ACE'UNG+CUSREP+MAN:ZZ+USC:ZZ+20120117:1720+6+UN+D:03B'Message Text 6UNE+1+6'UNZ+1+6'";
			const string expectedInterchangeText7 = "UNB+UNOA:4+MAN:ZZ+USC:ZZ+20120117:1720+7++ACE'UNG+MEDPID+MAN:ZZ+USC:ZZ+20120117:1720+7+UN+D:02A'Message Text 7UNE+1+7'UNZ+1+7'";
			var sender = new InterchangeComposer();
			sender.ExecuteBatch();
			var sendableInterchanges = GetSendableInterchanges();
			AssertEquals("7 interchange created", 7, sendableInterchanges.Length);
			AssertInterchange(sendableInterchanges[0], MessageTypes.Codes.eManifest, EDIInterchange.Status.eHubQueued, EDIInterchange.TransportType.eHub, branch1.PK, SenderID, RecipientTestID, expectedInterchangeText1);
			AssertInterchange(sendableInterchanges[1], MessageTypes.Codes.eManifest, EDIInterchange.Status.eHubQueued, EDIInterchange.TransportType.eHub, branch2.PK, SenderID, RecipientTestID, expectedInterchangeText2);
			AssertInterchange(sendableInterchanges[2], MessageTypes.Codes.CrewAndPassenger, EDIInterchange.Status.eHubQueued, EDIInterchange.TransportType.eHub, branch2.PK, SenderID, RecipientTestID, expectedInterchangeText3);
			AssertInterchange(sendableInterchanges[3], MessageTypes.Codes.UnassociatedShipments, EDIInterchange.Status.eHubQueued, EDIInterchange.TransportType.eHub, branch2.PK, SenderID, RecipientTestID, expectedInterchangeText4);
			AssertInterchange(sendableInterchanges[4], MessageTypes.Codes.CompleteTrip, EDIInterchange.Status.eHubQueued, EDIInterchange.TransportType.eHub, branch2.PK, SenderID, RecipientTestID, expectedInterchangeText5);
			AssertInterchange(sendableInterchanges[5], MessageTypes.Codes.PreliminaryTrip, EDIInterchange.Status.eHubQueued, EDIInterchange.TransportType.eHub, branch2.PK, SenderID, RecipientTestID, expectedInterchangeText6);
			AssertInterchange(sendableInterchanges[6], MessageTypes.Codes.CrewOrEquipmentRegistration, EDIInterchange.Status.eHubQueued, EDIInterchange.TransportType.eHub, branch2.PK, SenderID, RecipientTestID, expectedInterchangeText7);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var auOrg = Factory.NewWithValidTestData<OrgHeader>();
			auOrg.OH_Code = "AUTESTCOMP";
			auCompany = Factory.NewWithValidTestData<GlbCompany>();
			auCompany.GC_OH_OrgProxy = auOrg.PK;
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			auCompany.Branches.Add(Factory.NewWithValidTestData<GlbBranch>());
			var usOrg = Factory.NewWithValidTestData<OrgHeader>();
			usOrg.OH_Code = "USTESTCOMP1";
			usCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			usCompany1.GC_OH_OrgProxy = usOrg.PK;
			usCompany1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			usCompany1.Branches.Add(Factory.NewWithValidTestData<GlbBranch>());
			usOrg = Factory.NewWithValidTestData<OrgHeader>();
			usOrg.OH_Code = "USTESTCOMP2";
			usCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			usCompany2.GC_OH_OrgProxy = usOrg.PK;
			usCompany2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			usCompany2.Branches.Add(Factory.NewWithValidTestData<GlbBranch>());
			MessagingTestHelper.SetupPostMasterEmailGroup(Factory);
			message1 = MessagingTestHelper.GetTransmitEDIMessage(Factory, MessageTypes.Codes.eManifest);
			message2 = MessagingTestHelper.GetTransmitEDIMessage(Factory, MessageTypes.Codes.eManifest);
			Factory.Save();
		}

		EDIMessage message1;
		EDIMessage message2;
		GlbCompany auCompany;
		GlbCompany usCompany1;
		GlbCompany usCompany2;
		const string SenderID = "MAN";
		const string RecipientTestID = "USC";

		void AssertInterchange(CBPEDIInterchange interchange, string type, string status, string transportType, ZGuid branch, string sender, string recipient, string expectedInterchangeText)
		{
			AssertEquals("EI_InterchangeType", type, interchange.EI_InterchangeType);
			AssertEquals("EI_From", sender, interchange.EI_From);
			AssertEquals("EI_To", recipient, interchange.EI_To);
			AssertEquals("EI_InterchangeText", expectedInterchangeText, interchange.EI_InterchangeText);
			AssertEquals("EI_GB", branch, interchange.EI_GB);
			AssertEquals("EI_Status", status, interchange.EI_Status);
			AssertEquals("EI_TransportType", transportType, interchange.EI_TransportType);
			AssertEquals("EI_SessionGUID will never be empty for eHub messages.", true, !interchange.EI_SessionGUID.IsEmpty);
			AssertNotEquals("EI_SessionGUID will never be null for eHub messages.", null, interchange.EI_SessionGUID);
			foreach (EDIMessage message in interchange.ContainedMessages)
			{
				AssertEquals("EM_Status", Enterprise.Messaging.Business.EDIMessage.Status.Sent, message.EM_Status);
			}
		}

		CBPEDIInterchange[] GetSendableInterchanges()
		{
			var filter = new ZQuery();
			filter.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
			filter.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			filter.IncludeBlob(EDIInterchange.AllBlobFields);
			filter.OrderBy = EDIInterchangeSchema.EI_InterchangeNum.Name + " ASC";
			return Factory.Load<CBPEDIInterchange>(filter);
		}

		GlbBranch GetNewBranch(string guid)
		{
			var branch = Factory.NewWithPrimaryKey<GlbBranch>(new Guid(guid));
			branch.FillWithValidTestData();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			return branch;
		}
	}
}
