using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	sealed class AMSManifestEmailSendTest : TestCaseWithFactory
	{
		public void TestSendErrorMailForStaffMember()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "KNZ";
			var staffZ1 = group.Staff.AddNew();
			staffZ1.GS_Code = "K1";
			staffZ1.GS_LoginName = "K1";
			staffZ1.GS_EmailAddress = "kevin@pretend.email.com";

			var staffZ2 = group.Staff.AddNew();
			staffZ2.GS_Code = "K2";
			staffZ2.GS_LoginName = "K2";
			staffZ2.GS_EmailAddress = "kevin2@pretend.email.com";
			Factory.Save();

			FreightDataRegistry.Instance.USAMSGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, false));

			var consol = GetConsol(staffZ2, "ACR          MI                                                                 M01OTT111IT23                     FR345     000096 9204790                      M02HB1203130301_OTT1410                                                         P013902032512                                                                   J01OTT1                                                                         B01HB1203130301015200000000100PKG  0000001000KGN                                B020000000001CMSMTH                         OTT1    60267     01520             B04OB OB1203130301                                                              S01AUEXP COMPANY NAME                 AUEXP ADDRESS 1 AUEXP ADDRESS 2 AUE       S02XPCITY NSW 4345 AUSTRALIA                                                    S03+61 (2) 9845-6579                                                            U01ACE TEST IMPORTER 3                1 TEST STR CHICAGO IL 60666               U03+1 (312) 555-1212                                                            C0143545345345   435                           200                    4210E     C02234567                                                                       D001001100010 000000000000001000KG                                              D010000000100GOODS                                                      PKG     D02MARKS                                                                        ZCR                               00000");

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			interchange.EI_From = "USC";
			interchange.EI_To = "OTT1";
			interchange.EI_HeaderText = "ACR8CWS      MR11050303221800085                                                ";
			interchange.EI_BodyText = "ACR          MR12032621054409993                                                M01OTT111IT23                     FR345     000096 9204790                      W01                              000096151 INCORRECT VESSEL NAME                M02HB1203130301_OTT1410                                                         P013902032512                                                                   W01                          3902000096102 NO BILLS PROCESSD FOR PORT           W02OTT11203262105430100000000000000000000000000000000000017000000000000000      ZCR          MR                   00006                                         ";
			interchange.EI_FooterText = "ZCR8CWS      MI                   00004";

			messageTransmit.EM_MessageSubType = AMSMessageSubTypeList.Codes.Creating;
			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			message.EM_MessageText = interchange.EI_InterchangeText;
			message.EM_MessageNum = messageTransmit.EM_MessageNum;
			Factory.Save();

			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("Manifest Create Transmission Response (Failure) for")));
			AssertNotNull(email);
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("The original sender should be notify", staffZ2.GS_EmailAddress, email.Recipients[0].Email);
			message.Reload();
			AssertEquals(AMSMessageSubTypeList.Codes.Creating, message.EM_MessageSubType);
		}

		public void TestSendErrorMailForStaffMemberAndNominatedGroup()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "KNZ";
			var staffZ1 = group.Staff.AddNew();
			staffZ1.GS_Code = "K1";
			staffZ1.GS_LoginName = "K1";
			staffZ1.GS_EmailAddress = "kevin@pretend.email.com";

			var staffZ2 = group.Staff.AddNew();
			staffZ2.GS_Code = "K2";
			staffZ2.GS_LoginName = "K2";
			staffZ2.GS_EmailAddress = "kevin2@pretend.email.com";
			Factory.Save();

			FreightDataRegistry.Instance.USAMSGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));

			var consol = GetConsol(staffZ2, "ACR          MI                                                                 M01OTT111IT23                     FR345     000096 9204790                      M02HB1203130301_OTT1410                                                         P013902032512                                                                   J01OTT1                                                                         B01HB1203130301015200000000100PKG  0000001000KGN                                B020000000001CMSMTH                         OTT1    60267     01520             B04OB OB1203130301                                                              S01AUEXP COMPANY NAME                 AUEXP ADDRESS 1 AUEXP ADDRESS 2 AUE       S02XPCITY NSW 4345 AUSTRALIA                                                    S03+61 (2) 9845-6579                                                            U01ACE TEST IMPORTER 3                1 TEST STR CHICAGO IL 60666               U03+1 (312) 555-1212                                                            C0143545345345   435                           200                    4210E     C02234567                                                                       D001001100010 000000000000001000KG                                              D010000000100GOODS                                                      PKG     D02MARKS                                                                        ZCR                               00000");

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			interchange.EI_From = "USC";
			interchange.EI_To = "OTT1";
			interchange.EI_HeaderText = "ACR8CWS      MR11050303221800085                                                ";
			interchange.EI_BodyText = "ACR          MR12032621054409993                                                M01OTT111IT23                     FR345     000096 9204790                      W01                              000096151 INCORRECT VESSEL NAME                M02HB1203130301_OTT1410                                                         P013902032512                                                                   W01                          3902000096102 NO BILLS PROCESSD FOR PORT           W02OTT11203262105430100000000000000000000000000000000000017000000000000000      ZCR          MR                   00006                                         ";
			interchange.EI_FooterText = "ZCR8CWS      MI                   00004";

			messageTransmit.EM_MessageSubType = AMSMessageSubTypeList.Codes.Creating;
			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			message.EM_MessageText = interchange.EI_InterchangeText;
			message.EM_MessageNum = messageTransmit.EM_MessageNum;
			Factory.Save();

			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("Manifest Create Transmission Response (Failure) for")));
			AssertNotNull(email);
			AssertEquals(2, email.Recipients.Count);
			AssertEquals("The original sender should be notify", true, email.Recipients.Contains(staffZ2.GS_EmailAddress));
			AssertEquals("The original sender should be notify", true, email.Recipients.Contains(staffZ1.GS_EmailAddress));
			message.Reload();
			AssertEquals(AMSMessageSubTypeList.Codes.Creating, message.EM_MessageSubType);
		}

		string SetupJobForSendingAnEmail(ZString staffCode, bool isHVLV = false)
		{
			var consol = Factory.New<Integration.Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var header = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_CarrierSCAC = "OTT1";
			var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, header.PK);
			query.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SubApplicationCodeList.Codes.AMS);
			query.FetchOnlyFromLocalCache = true;
			var moveHeder = Factory.LoadTop1<Integration.Customs.US.USAMS.ICusInBondMoveHeader>(query);
			if (isHVLV)
			{
				((IStmALogProvider)header).Logs.AddNew(AutoEvents.Transferred, "|TYP=HVL");
			}

			var messageTransmit = Factory.New<AMSEDIMessage>();
			messageTransmit.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			messageTransmit.EM_LinkedObject = (BusinessObject)moveHeder;
			messageTransmit.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			messageTransmit.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			messageTransmit.EM_MessageNum = "~000000007";
			if (!staffCode.IsEmpty)
			{
				messageTransmit.EM_SystemCreateUser = staffCode;
			}
			var manifestSequenceNumber = "120001";
			moveHeder.BM_ManifestSequenceNumber = manifestSequenceNumber;
			var msnQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, moveHeder.PK);
			msnQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, "MSN");
			var mSNCusEntryNumber = (BusinessObject)Factory.LoadTop1<Integration.Customs.ICusEntryNumber>(msnQuery);
			mSNCusEntryNumber[CusEntryNumSchema.CE_EntryLineReference] = "OTT1";
			Factory.Save();

			var consolRef = consol.JK_UniqueConsignRef;

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			interchange.EI_From = "USC";
			interchange.EI_To = "OTT1";
			interchange.EI_HeaderText = "ACR8CWS      MR11050303221800085                                                ";
			interchange.EI_BodyText =
"M01OTT110AUAPL EMERALD            K34L 00001" + manifestSequenceNumber + " 7819369                      " +
"M02HB1105031520_OTT14000086                                                     " +
"P01270405131100001    1518                                                      " +
"W02OTT11105030322150100100001000000000000000000000000100021                     ";
			interchange.EI_FooterText = "ZCR8CWS      MI                   00004";

			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			message.EM_MessageText = interchange.EI_InterchangeText;
			Factory.Save();

			return consolRef;
		}

		public void TestNoReceipientNoEmailSentOnManifest()
		{
			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_EmailAddress = "";

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "KNZ";

			var staffZ1 = group.Staff.AddNew();
			staffZ1.GS_Code = "K1";
			staffZ1.GS_LoginName = "K1";
			Factory.Save();

			FreightDataRegistry.Instance.USAMSGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, false));
			var consolRef = SetupJobForSendingAnEmail("");

			var processor = new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ExecuteBatch();
			var expectedSubject = "Manifest Create Transmission Response for " + consolRef + " - HB1105031520 - OTT14000086";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == expectedSubject));
			AssertNull("Email with subject did not send", email);
		}

		public void TestSendEmailToRecipientFromGroup()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "KNZ";

			var staffZ1 = group.Staff.AddNew();
			staffZ1.GS_Code = "K1";
			staffZ1.GS_LoginName = "K1";
			staffZ1.GS_EmailAddress = "k1@k1.email.com";
			Factory.Save();

			FreightDataRegistry.Instance.USAMSGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));
			var consolRef = SetupJobForSendingAnEmail("");

			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			var expectedSubject = "Manifest Create Transmission Response for " + consolRef + " - HB1105031520 - OTT14000086";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == expectedSubject));
			AssertNotNull("Email with subject sent.", email);
			AssertEquals("recipients from GroupRegistry", true, email.Recipients.Contains(staffZ1.GS_EmailAddress));
		}

		public void TestSendEmailToRecipientFromGroup_WhenHasHVLTRFLog_UseUSHVLVAMSGroupNotificationRegistryItem()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "KNZ";

			var staffZ1 = group.Staff.AddNew();
			staffZ1.GS_Code = "K1";
			staffZ1.GS_LoginName = "K1";
			staffZ1.GS_EmailAddress = "k1@k1.email.com";
			Factory.Save();

			FreightDataRegistry.Instance.USHVLVAMSGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));
			var consolRef = SetupJobForSendingAnEmail("", true);

			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			var expectedSubject = "Manifest Create Transmission Response for " + consolRef + " - HB1105031520 - OTT14000086";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == expectedSubject));
			AssertNotNull("Email with subject sent.", email);
			AssertEquals("recipients from GroupRegistry", true, email.Recipients.Contains(staffZ1.GS_EmailAddress));
		}

		public void TestSendEmailToRecipientFromJob()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "KNZ";

			var staffZ1 = group.Staff.AddNew();
			staffZ1.GS_Code = "K1";
			staffZ1.GS_LoginName = "K1";

			var staffZ2 = Factory.New<GlbStaff>();
			staffZ2.GS_Code = "K2";
			staffZ2.GS_LoginName = "K2";
			staffZ2.GS_EmailAddress = "k2@k2.email.com";
			Factory.Save();
			FreightDataRegistry.Instance.USAMSGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, false));
			var consolRef = SetupJobForSendingAnEmail(staffZ2.GS_Code);
			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			var expectedSubject = "Manifest Create Transmission Response for " + consolRef + " - HB1105031520 - OTT14000086";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == expectedSubject));
			AssertNotNull("Email with subject sent.", email);
			AssertEquals("recipients from the Job", true, email.Recipients.Contains(staffZ2.GS_EmailAddress));
		}

		public void TestSendMessageAcknowledgementsForStaffMember()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "KNZ";
			var staffZ1 = group.Staff.AddNew();
			staffZ1.GS_Code = "K1";
			staffZ1.GS_LoginName = "K1";
			staffZ1.GS_EmailAddress = "kevin@pretend.email.com";

			var staffZ2 = group.Staff.AddNew();
			staffZ2.GS_Code = "K2";
			staffZ2.GS_LoginName = "K2";
			staffZ2.GS_EmailAddress = "kevin2@pretend.email.com";

			Factory.Save();

			FreightDataRegistry.Instance.USAMSGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, false));

			var consol = Factory.New<Integration.Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var header = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_CarrierSCAC = "OTT1";
			var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, header.PK);
			query.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SubApplicationCodeList.Codes.AMS);
			query.FetchOnlyFromLocalCache = true;
			var moveHeder = Factory.LoadTop1<Integration.Customs.US.USAMS.ICusInBondMoveHeader>(query);

			var messageTransmit = Factory.New<AMSEDIMessage>();
			messageTransmit.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			messageTransmit.EM_LinkedObject = (BusinessObject)moveHeder;
			messageTransmit.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			messageTransmit.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			messageTransmit.EM_MessageNum = "~000000007";
			messageTransmit.EM_SystemCreateUser = staffZ1.GS_Code;
			var manifestSequenceNumber = "120001";
			moveHeder.BM_ManifestSequenceNumber = manifestSequenceNumber;
			var msnQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, moveHeder.PK);
			msnQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, "MSN");
			var mSNCusEntryNumber = (BusinessObject)Factory.LoadTop1<Integration.Customs.ICusEntryNumber>(msnQuery);
			mSNCusEntryNumber[CusEntryNumSchema.CE_EntryLineReference] = "OTT1";
			Factory.Save();

			var consolRef = consol.JK_UniqueConsignRef;

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			interchange.EI_From = "USC";
			interchange.EI_To = "OTT1";
			interchange.EI_HeaderText = "ACR8CWS      MR11050303221800085                                                ";
			interchange.EI_BodyText =
"M01OTT110AUAPL EMERALD            K34L 00001" + manifestSequenceNumber + " 7819369                      " +
"M02HB1105031520_OTT14000086                                                     " +
"P01270405131100001    1518                                                      " +
"W02OTT11105030322150100100001000000000000000000000000100021                     ";
			interchange.EI_FooterText = "ZCR8CWS      MI                   00004";

			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			message.EM_MessageText = interchange.EI_InterchangeText;
			Factory.Save();

			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			var expectedSubject = "Manifest Create Transmission Response for " + consolRef + " - HB1105031520 - OTT14000086";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == expectedSubject));
			AssertNotNull("Email with subject '" + expectedSubject + "'should exist", email);

			AssertEquals(1, email.Recipients.Count);
			AssertEquals(staffZ1.GS_EmailAddress, email.Recipients[0].Email);
		}

		public void TestSendMessageAcknowledgementsStaffMemberAndNominatedGroup()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "KNZ";
			var staffZ1 = group.Staff.AddNew();
			staffZ1.GS_Code = "K1";
			staffZ1.GS_LoginName = "K1";
			staffZ1.GS_EmailAddress = "kevin@pretend.email.com";

			var staffZ2 = group.Staff.AddNew();
			staffZ2.GS_Code = "K2";
			staffZ2.GS_LoginName = "K2";
			staffZ2.GS_EmailAddress = "kevin2@pretend.email.com";

			Factory.Save();

			FreightDataRegistry.Instance.USAMSGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));

			var consol = Factory.New<Integration.Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var header = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_CarrierSCAC = "OTT1";
			var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, header.PK);
			query.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SubApplicationCodeList.Codes.AMS);
			query.FetchOnlyFromLocalCache = true;
			var moveHeder = Factory.LoadTop1<Integration.Customs.US.USAMS.ICusInBondMoveHeader>(query);

			var messageTransmit = Factory.New<AMSEDIMessage>();
			messageTransmit.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			messageTransmit.EM_LinkedObject = (BusinessObject)moveHeder;
			messageTransmit.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			messageTransmit.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			messageTransmit.EM_MessageNum = "~000000007";
			messageTransmit.EM_SystemCreateUser = staffZ1.GS_Code;
			var manifestSequenceNumber = "120001";
			moveHeder.BM_ManifestSequenceNumber = manifestSequenceNumber;
			var msnQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, moveHeder.PK);
			msnQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, "MSN");
			var mSNCusEntryNumber = (BusinessObject)Factory.LoadTop1<Integration.Customs.ICusEntryNumber>(msnQuery);
			mSNCusEntryNumber[CusEntryNumSchema.CE_EntryLineReference] = "OTT1";
			Factory.Save();

			var consolRef = consol.JK_UniqueConsignRef;

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			interchange.EI_From = "USC";
			interchange.EI_To = "OTT1";
			interchange.EI_HeaderText = "ACR8CWS      MR11050303221800085                                                ";
			interchange.EI_BodyText =
"M01OTT110AUAPL EMERALD            K34L 00001" + manifestSequenceNumber + " 7819369                      " +
"M02HB1105031520_OTT14000086                                                     " +
"P01270405131100001    1518                                                      " +
"W02OTT11105030322150100100001000000000000000000000000100021                     ";
			interchange.EI_FooterText = "ZCR8CWS      MI                   00004";

			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			message.EM_MessageText = interchange.EI_InterchangeText;
			Factory.Save();

			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			var expectedSubject = "Manifest Create Transmission Response for " + consolRef + " - HB1105031520 - OTT14000086";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == expectedSubject));
			AssertNotNull("Email with subject '" + expectedSubject + "'should exist", email);

			AssertEquals(2, email.Recipients.Count);
			Assert(email.Recipients.Contains(staffZ1.GS_EmailAddress));
			Assert(email.Recipients.Contains(staffZ2.GS_EmailAddress));
		}

		Integration.Forwarding.IForwardingConsol GetConsol(GlbStaff messageCreateUser, ZString messageText)
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "1";
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var consol = Factory.New<Integration.Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			header = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_CarrierSCAC = "OTT1";
			var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, header.PK);
			query.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SubApplicationCodeList.Codes.AMS);
			query.FetchOnlyFromLocalCache = true;
			var moveHeder = Factory.LoadTop1<Integration.Customs.US.USAMS.ICusInBondMoveHeader>(query);

			var mock = Factory.NewMoq<AMSEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			messageTransmit = mock.Object;
			messageTransmit.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			messageTransmit.EM_LinkedObject = (BusinessObject)moveHeder;
			messageTransmit.EM_MessageNum = "0000000096";
			messageTransmit.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			messageTransmit.EM_MessageText = messageText;
			messageTransmit.EM_SystemCreateUser = messageCreateUser.GS_Code;

			Factory.Save();
			return consol;
		}
		AMSEDIMessage messageTransmit;
		Integration.Customs.US.USAMS.ICusInBondHeader header;
	}
}
