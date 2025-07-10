using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	[TestedType(typeof(ManifestProcessor))]
	class ManifestProcessorTest : AMSProcessorTest
	{
		protected override ZString SendMode => "ESM";

		protected override void EndToEndCore()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "1";
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

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
			messageTransmit.EM_SystemCreateUser = staffZ2.GS_Code;

			Factory.Save();
			var manifestSequenceNumber = moveHeder.BM_ManifestSequenceNumber;
			var consolRef = consol.JK_UniqueConsignRef;

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			interchange.EI_From = "USC";
			interchange.EI_To = "OTT1";
			interchange.EI_HeaderText = "ACR8CWS      MR11050303221800085                                                ";
			interchange.EI_BodyText =
"M01OTT110AUAPL EMERALD            K34L 00001" + manifestSequenceNumber.PadLeft(6) + " 7819369                      " +
"M02HB1105031520_OTT14000086                                                     " +
"P01270405131100001    1518                                                      " +
"W02OTT11105030322150100100001000000000000000000000000100021                     ";
			interchange.EI_FooterText = "ZCR8CWS      MI                   00004";

			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			message.EM_MessageText = interchange.EI_InterchangeText;
			message.EM_MessageNum = messageTransmit.EM_MessageNum;
			Factory.Save();

			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			var expectedSubject = "Manifest Create Transmission Response for " + consolRef + " - HB1105031520 - OTT14000086";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == expectedSubject));
			AssertNotNull("Email with subject '" + expectedSubject + "'should exist", email);
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("The original sender should be notify", staffZ2.GS_EmailAddress, email.Recipients[0].Email);
			var expectedMainEmailBody = string.Format(@"
<br />
A response message has been received from Customs.<br />Shown below is a summary of relevant information received in the message.<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Transmission Summary</th></tr></thead><tr><td>Carrier Code</td><td>OTT1</td></tr><tr><td>Date of Transmission</td><td>03-May-11</td></tr><tr><td>Time of Transmission</td><td>032215</td></tr><tr><td>Total Manifests Read</td><td>1</td></tr><tr><td>Total Ports Read</td><td>1</td></tr><tr><td>Total Bills Read</td><td>1</td></tr><tr><td>Total Amendments Read</td><td>0</td></tr><tr><td>Total G01/H01 Records Input</td><td>0</td></tr><tr><td>Total Bills Rejected</td><td>0</td></tr><tr><td>Total Bills Accepted</td><td>1</td></tr><tr><td>Total Records Read</td><td>21</td></tr><tr><td>Total Export Transaction Data Read</td><td>0</td></tr><tr><td>Total Export Transaction Data Rejected</td><td>0</td></tr><tr><td>Total Export Transaction Data Accepted</td><td>0</td></tr></table>
<br />
", manifestSequenceNumber);

			AssertContains(@"<strong>Manifest Create Transmission Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=JobConsol&BusinessEntityPK=" + consol.PK.ToString(), email.Body);
			AssertContains(@""">" + consolRef + " - HB1105031520 - OTT14000086</a></strong><br />", email.Body);
			AssertContains(expectedMainEmailBody, email.Body);
		}

		public void TestChangeFromPendingToQue()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "1";
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			company.Branches.Add(branch2);
			Factory.Save();

			var consol = Factory.New<Integration.Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var header = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_CarrierSCAC = "OTT1";
			header.BH_GB = branch2.PK;
			var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, header.PK);
			query.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SubApplicationCodeList.Codes.AMS);
			query.FetchOnlyFromLocalCache = true;
			var moveHeder = Factory.LoadTop1<Integration.Customs.US.USAMS.ICusInBondMoveHeader>(query);

			var messageTransmit = Factory.New<AMSEDIMessage>();
			messageTransmit.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			messageTransmit.EM_LinkedObject = (BusinessObject)moveHeder;
			messageTransmit.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestAmendment;
			messageTransmit.EM_MessageSubType = AMSMessageSubTypeList.Codes.AmendingDelete;
			messageTransmit.EM_MessageOwner = Constants.ACE;
			messageTransmit.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			messageTransmit.EM_SystemCreateUser = staffZ2.GS_Code;
			messageTransmit.EM_Status = EDIMessage.Status.Sent;

			var pendingMessageTransmit = Factory.New<AMSEDIMessage>();
			pendingMessageTransmit.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			pendingMessageTransmit.EM_LinkedObject = (BusinessObject)moveHeder;
			pendingMessageTransmit.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestAmendment;
			pendingMessageTransmit.EM_MessageSubType = AMSMessageSubTypeList.Codes.AmendingAdd;
			pendingMessageTransmit.EM_MessageOwner = Constants.ACE;
			pendingMessageTransmit.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			pendingMessageTransmit.EM_SystemCreateUser = staffZ2.GS_Code;
			pendingMessageTransmit.EM_Status = EDIMessage.Status.Pending;

			Factory.Save();

			var manifestSequenceNumber = moveHeder.BM_ManifestSequenceNumber;
			var consolRef = consol.JK_UniqueConsignRef;

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse;
			interchange.EI_From = "USC";
			interchange.EI_To = "OTT1";
			interchange.EI_HeaderText = "ACR          AR11050303221800085                                                ";
			interchange.EI_BodyText =
"M01OTT110AUAPL EMERALD            K34L 00001" + manifestSequenceNumber.PadLeft(6) + " 7819369                      " +
"M02HB1105031520_OTT14000086                                                     " +
"P01270405131100001    1518                                                      " +
"W02OTT11105030322150100100001000000000000000000000000100021                     ";
			interchange.EI_FooterText = "ZCR          AR                   00004";
			interchange.EI_GB = branch2.PK;

			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse;
			message.EM_MessageText = interchange.EI_InterchangeText;
			message.EM_MessageNum = messageTransmit.EM_MessageNum;
			Factory.Save();

			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("Manifest Amendment")));
			AssertNotNull("An email with subject containing 'Manifest Amendment' should have been created.", email);
			Assert(email.Body.Contains("Please Note: There were pending messages waiting for this message to be acknowledged. Those messages have just been transmitted."));
		}

		public void TestEmailIsSentToRightGroup()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "1";
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var groupZZ2 = Factory.New<GlbGroup>();
			groupZZ2.GG_Code = "ZZ2";
			var staffZ3 = groupZZ2.Staff.AddNew();
			staffZ3.GS_Code = "Z3";
			staffZ3.GS_LoginName = "z3";
			staffZ3.GS_EmailAddress = "dong3@pretend.email.com";
			Factory.Save();

			FreightDataRegistry.Instance.USAMSGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", groupZZ2.PK, false));

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
			AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staffZ3.GS_EmailAddress, email.Recipients[0].Email);
			var expectedMainEmailBody = string.Format(@"
<br />
A response message has been received from Customs.<br />Shown below is a summary of relevant information received in the message.<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Transmission Summary</th></tr></thead><tr><td>Carrier Code</td><td>OTT1</td></tr><tr><td>Date of Transmission</td><td>03-May-11</td></tr><tr><td>Time of Transmission</td><td>032215</td></tr><tr><td>Total Manifests Read</td><td>1</td></tr><tr><td>Total Ports Read</td><td>1</td></tr><tr><td>Total Bills Read</td><td>1</td></tr><tr><td>Total Amendments Read</td><td>0</td></tr><tr><td>Total G01/H01 Records Input</td><td>0</td></tr><tr><td>Total Bills Rejected</td><td>0</td></tr><tr><td>Total Bills Accepted</td><td>1</td></tr><tr><td>Total Records Read</td><td>21</td></tr><tr><td>Total Export Transaction Data Read</td><td>0</td></tr><tr><td>Total Export Transaction Data Rejected</td><td>0</td></tr><tr><td>Total Export Transaction Data Accepted</td><td>0</td></tr></table>
<br />
", manifestSequenceNumber);

			AssertContains(@"<strong>Manifest Create Transmission Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=JobConsol&BusinessEntityPK=" + consol.PK.ToString(), email.Body);
			AssertContains(@""">" + consolRef + " - HB1105031520 - OTT14000086</a></strong><br />", email.Body);
			AssertContains(expectedMainEmailBody, email.Body);
		}

		public void TestACEMessage()
		{
			var consol = GetConsol("ACR          MI                                                                 M01OTT111IT23                     FR345     000096 9204790                      M02HB1203130301_OTT1410                                                         P013902032512                                                                   J01OTT1                                                                         B01HB1203130301015200000000100PKG  0000001000KGN                                B020000000001CMSMTH                         OTT1    60267     01520             B04OB OB1203130301                                                              S01AUEXP COMPANY NAME                 AUEXP ADDRESS 1 AUEXP ADDRESS 2 AUE       S02XPCITY NSW 4345 AUSTRALIA                                                    S03+61 (2) 9845-6579                                                            U01ACE TEST IMPORTER 3                1 TEST STR CHICAGO IL 60666               U03+1 (312) 555-1212                                                            C0143545345345   435                           200                    4210E     C02234567                                                                       D001001100010 000000000000001000KG                                              D010000000100GOODS                                                      PKG     D02MARKS                                                                        ZCR                               00000");

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

		public void TestManifestSequenceNumberIsUpdated()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "1";
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var consol = Factory.New<Integration.Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var header = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_CarrierSCAC = "OTT1";
			var bill = Factory.New<Integration.Customs.US.USAMS.ICusInBondBill>();
			bill.B0_BH = header.PK;
			bill.B0_MasterBillNumber = "MB23423";
			bill.B0_IssuerCode = "XXDZ";
			var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, header.PK);
			query.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SubApplicationCodeList.Codes.AMS);
			query.FetchOnlyFromLocalCache = true;
			var moveHeder = Factory.LoadTop1<Integration.Customs.US.USAMS.ICusInBondMoveHeader>(query);

			var messageTransmit = Factory.New<AMSEDIMessage>();
			messageTransmit.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			messageTransmit.EM_LinkedObject = (BusinessObject)moveHeder;
			messageTransmit.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			messageTransmit.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			messageTransmit.EM_SystemCreateUser = staffZ2.GS_Code;

			Factory.Save();
			var manifestSequenceNumber = moveHeder.BM_ManifestSequenceNumber;
			var consolRef = consol.JK_UniqueConsignRef;

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			interchange.EI_From = "USC";
			interchange.EI_To = "OTT1";
			interchange.EI_HeaderText = "ACR8CWS      MR11050303221800085                                                ";
			interchange.EI_BodyText =
"M01OTT110AUAPL EMERALD            K34L 00001123456 7819369                      " +
"M02HB1105031520_OTT14000086                                                     " +
"P01270405131100001    1518                                                      " +
"W02OTT11105030322150100100001000000000000000000000000100021                     ";
			interchange.EI_FooterText = "ZCR8CWS      MI                   00004";

			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			message.EM_MessageText = interchange.EI_InterchangeText;
			message.EM_MessageNum = messageTransmit.EM_MessageNum;
			Factory.Save();
			AssertNotEquals("moveHeder.BM_ManifestSequenceNumber", "123456", moveHeder.BM_ManifestSequenceNumber);
			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			var newFactory = new BusinessObjectFactory();
			moveHeder = newFactory.Load<Integration.Customs.US.USAMS.ICusInBondMoveHeader>(moveHeder.PK);
			AssertEquals("moveHeder.BM_ManifestSequenceNumber", "123456", moveHeder.BM_ManifestSequenceNumber);
			var expectedSubject = "Manifest Create Transmission Response for " + consolRef + " - HB1105031520 - OTT14000086";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == expectedSubject));
			AssertNotNull("Email with subject '" + expectedSubject + "'should exist", email);
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("The original sender should be notify", staffZ2.GS_EmailAddress, email.Recipients[0].Email);
			var expectedMainEmailBody = string.Format(@"
<br />
A response message has been received from Customs.<br />Shown below is a summary of relevant information received in the message.<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Transmission Summary</th></tr></thead><tr><td>Carrier Code</td><td>OTT1</td></tr><tr><td>Date of Transmission</td><td>03-May-11</td></tr><tr><td>Time of Transmission</td><td>032215</td></tr><tr><td>Total Manifests Read</td><td>1</td></tr><tr><td>Total Ports Read</td><td>1</td></tr><tr><td>Total Bills Read</td><td>1</td></tr><tr><td>Total Amendments Read</td><td>0</td></tr><tr><td>Total G01/H01 Records Input</td><td>0</td></tr><tr><td>Total Bills Rejected</td><td>0</td></tr><tr><td>Total Bills Accepted</td><td>1</td></tr><tr><td>Total Records Read</td><td>21</td></tr><tr><td>Total Export Transaction Data Read</td><td>0</td></tr><tr><td>Total Export Transaction Data Rejected</td><td>0</td></tr><tr><td>Total Export Transaction Data Accepted</td><td>0</td></tr></table>
<br />
", manifestSequenceNumber);

			AssertContains(@"<strong>Manifest Create Transmission Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=JobConsol&BusinessEntityPK=" + consol.PK.ToString(), email.Body);
			AssertContains(@""">" + consolRef + " - HB1105031520 - OTT14000086</a></strong><br />", email.Body);
			AssertContains(expectedMainEmailBody, email.Body);
		}

		public void TestProcessSubsequentInBondResponse()
		{
			var consol = GetConsol("ACR8CWSCAREDIII                                                                 M01OTT110AUAPL EMERALD            1106      000016 7819369                      M02201204201106_OTT1720000                                                      P012704050912                                                                   J01OTT1                                                                         B03201204201106                                      0000000001    OTT1         I01  N 040000273KLWC3902     0000100093-095632700                               ZCR8CWS      II                   00000                                         ");
			messageTransmit.EM_MessageNum = "OTT1720000";
			messageTransmit.EM_MessageOwner = Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Constants.ACE;
			messageTransmit.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.SubsequentInBond;

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ReceiveTransmit = CBPEDIInterchange.Direction.Receive;
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.SubsequentInBondResponse;
			interchange.EI_From = "USC";
			interchange.EI_To = "OTT1";
			interchange.EI_HeaderText = "ACR8CWS      IR12042305043700207                                                ";
			interchange.EI_BodyText = "ACR          IR12042305043700207                                                M01OTT110AUAPL EMERALD            1106      000016 7819369                      M02201204201106_OTT1720000                                                      P012704050912                                                                   J01OTT1                                                                         B03201204201106                                      0000000001    OTT1         I01  N 040000273KLWC3902     0000100093-095632700                               W01                                    040 BL NOT ON FILE                       W01                                    033 INVALID DDPP                         W01                                    036 INVALID STATUS CODE                  W01                                    030 INVALID IB ENTRY TYPE                W01                                    030 INVALID IB ENTRY TYPE                W01                                    113 B01/M11,I01/M12 DEST NOT=            W02OTT11204230504370100100000000000000000000000010000000008                     ZCR8CWS      IR                   00013                                         ";
			interchange.EI_FooterText = "ZCR8CWS      IR                   00013";
			interchange.EI_Status = CBPEDIInterchange.Status.Queued;

			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.SubsequentInBondResponse;
			message.EM_MessageText = interchange.EI_InterchangeText;
			message.EM_MessageNum = messageTransmit.EM_MessageNum;

			Factory.Save();

			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("Subsequent In-bond Response (Failure) for")));
			AssertNotNull(email);
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("The original sender should be notify", staffZ2.GS_EmailAddress, email.Recipients[0].Email);

			interchange.Reload();
			interchange.ContainedMessages.Load();
			AssertEquals(1, interchange.ContainedMessages.Count);
			messageTransmit.Reload();
			var responseMessage = messageTransmit.ResponseMessage;
			AssertEquals(responseMessage, interchange.ContainedMessages[0]);
		}

		public void TestProcessPermitToTransferResponse()
		{
			var consol = GetConsol("ACR          TI                                                                 M01OTT110ITAPL GARNET             1256      000099                              M02365_OTT1439000                                                               P012704043012                                                                   J01AAAD                                                                         T01365                       W03420-064032900                                   T020000000004                                                                   ZCR                               00000");
			messageTransmit.EM_MessageNum = "OTT1439000";
			messageTransmit.EM_MessageOwner = Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Constants.ACE;
			messageTransmit.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.PermitToTransfer;

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ReceiveTransmit = CBPEDIInterchange.Direction.Receive;
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.PermitToTransferResponse;
			interchange.EI_From = "USC";
			interchange.EI_To = "OTT1";
			interchange.EI_HeaderText = "ACR          TR12042321024811255                                                ";
			interchange.EI_BodyText = "M01OTT110ITAPL GARNET             1256      000099                              M02365_OTT1439000                                                               P012704043012                                                                   W02OTT11204232102470100100001000000000000000000000000100006                     ";
			interchange.EI_FooterText = "ZCR          TR                   00004                                         ";
			interchange.EI_Status = CBPEDIInterchange.Status.Queued;

			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.PermitToTransferResponse;
			message.EM_MessageText = interchange.EI_InterchangeText;
			message.EM_MessageNum = messageTransmit.EM_MessageNum;
			Factory.Save();

			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("Permit to Transfer")));
			AssertNotNull(email);
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("The original sender should be notify", staffZ2.GS_EmailAddress, email.Recipients[0].Email);

			interchange.Reload();
			interchange.ContainedMessages.Load();
			AssertEquals(1, interchange.ContainedMessages.Count);
			messageTransmit.Reload();
			var responseMessage = messageTransmit.ResponseMessage;
			AssertEquals(responseMessage, interchange.ContainedMessages[0]);
		}

		public void TestProcessVesselDepartureArrivalResponse()
		{
			var consol = GetConsol("ACR          HI                                                                 M01XXXW11ITAPL GARNET             A001      000004                              M02005_XXXW320000                                                               P012704043012                                                                   H014              1204302704APLU1906                                            H02                  APL GARNET             S41352A001                          ZCR                               00000");
			messageTransmit.EM_MessageNum = "XXXW320000";
			messageTransmit.EM_MessageOwner = Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Constants.ACE;
			messageTransmit.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival;

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ReceiveTransmit = CBPEDIInterchange.Direction.Receive;
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse;
			interchange.EI_From = "USC";
			interchange.EI_To = "XXXW";
			interchange.EI_HeaderText = "ACR          HR12050719190911967                                                ";
			interchange.EI_BodyText = "M01XXXW11ITAPL GARNET             A001      000004                              M02005_XXXW320000                                                               P012704043012                                                                   W02XXXW1205071919080100100000000000000000001000000000100005                     ";
			interchange.EI_FooterText = "ZCR          HR                   00004                                         ";
			interchange.EI_Status = CBPEDIInterchange.Status.Queued;

			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			message.EM_MessageText = interchange.EI_InterchangeText;
			message.EM_MessageNum = messageTransmit.EM_MessageNum;
			Factory.Save();

			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("Vessel arrival Response for " + consol.JK_UniqueConsignRef + " - 005 - XXXW32")));
			AssertNotNull(email);
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("The original sender should be notify", staffZ2.GS_EmailAddress, email.Recipients[0].Email);

			interchange.Reload();
			interchange.ContainedMessages.Load();
			AssertEquals(1, interchange.ContainedMessages.Count);
			messageTransmit.Reload();
			var responseMessage = messageTransmit.ResponseMessage;
			AssertEquals(responseMessage, interchange.ContainedMessages[0]);
		}

		[TestDate(2012, 05, 07)]
		public void TestProcessEstimatedDateOfArrivalEventAccepted()
		{
			var consol = GetConsol("ACR          HI                                                                 M01XXXW11ITAPL GARNET             A001      000004                              M02005_0000XXXW35                                                               P012704043012                                                                   H01Y              120513    APLU1842                                            H02                  APL GARNET             S41352A001                          ZCR                               00000");
			header.BH_ETA = ZDateTime.Today.AddDays(-4);
			messageTransmit.EM_MessageNum = "XXXW000035";
			messageTransmit.EM_MessageOwner = Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Constants.ACE;
			messageTransmit.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival;
			messageTransmit.EM_MessageSubType = AMSMessageSubTypeList.Codes.ChangeEstimatedDateOfArrival;

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ReceiveTransmit = CBPEDIInterchange.Direction.Receive;
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse;
			interchange.EI_From = "USC";
			interchange.EI_To = "OTT1";
			interchange.EI_HeaderText = "ACR          HR12050719424811973                                                ";
			interchange.EI_BodyText = "M01XXXW11ITAPL GARNET             A001      000004                              M02005_XXXW000035                                                               P012704043012                                                                   W02XXXW1205071942480100100000000000000000001000000000100005                     ";
			interchange.EI_FooterText = "ZCR          HR                   00004                                         ";
			interchange.EI_Status = CBPEDIInterchange.Status.Queued;

			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse;
			message.EM_MessageText = interchange.EI_InterchangeText;
			message.EM_MessageSubType = AMSMessageSubTypeList.Codes.VesselArrival;
			message.EM_MessageNum = messageTransmit.EM_MessageNum;
			Factory.Save();

			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			var factory2 = new BusinessObjectFactory();
			var reloadedHeader = factory2.Load<Integration.Customs.US.USAMS.ICusInBondHeader>(header.PK);
			AssertEquals(new ZDateTime(2012, 05, 13), reloadedHeader.BH_ETA.Date);
		}

		public void TestAddCommentsForSpecificError()
		{
			var consol = GetConsol("ACR          MI                                                                 M01CCLL11DEHOUSTON EXPRESS        064W             9294991                      M02VEJS14006824_CCLL4482                                                        P011401011515                                                                   J01CCLL                                                                         B01VEJS14006824428790000000066PKG  0000008584KGN                                B020000000035CMHAMBURG                      NYKSCCLL41374     42879             B04OB NYKS5050081020                                                            N00SH NEXANS DENMARK A/S                                                        N02SAVVAERKSVEJ 18                                                              N03JUELSMINDE                    DK                                             N00CN AVANTI WIND SYSTEMS INC.                                                  N025150 SOUTH TOWN DRIVE                                                        N03NEW BERLIN         WI53151    US                                             C01NYKU6148648   012894                        4B0                    42G0LCY   D00           000000000000000053KG                                              D010000000001PARTS OF WINDMILLS                                         PKG     D02AVANTI WIND SYSTEMS INC. 5150 SOUTH TOWN DRIV                                D02E NEW BERLIN WI 53151 UNITED STATES                                          D00           000000000000008531KG                                              D010000000065PARTS OF WINDMILLS                                         PKG     D02AVANTI WIND SYSTEMS INC. 5150 SOUTH TOWN DRIV                                D02E NEW BERLIN WI 53151 UNITED STATES                                          ZCR                               00000");

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			interchange.EI_From = "USC";
			interchange.EI_To = "OTT1";
			interchange.EI_HeaderText = "ACR8CWS      MR11050303221800085                                                ";
			interchange.EI_BodyText = "ACR          MR15012114503073208                                                M01CCLL11DEHOUSTON EXPRESS        FR345     000096 9204790                      M02VEJS14006824_CCLL4482                                                        P011401011515                                                                   W01                          1401000001059 A01/M13 REQ AS OF ACT/ARR            W02OTT11203262105430100000000000000000000000000000000000017000000000000000      ZCR          MR                   00006                                         ";
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
			AssertContains("In the case that a bill is being added after vessel arrival, a Manifest Amendment should be filed.", email.Body);
		}

		[TestDate(2016, 07, 14)]
		public void TestUpdateVesselArrivalActualArrivalDate()
		{
			var consol = GetConsol("ACR          HI                                                                 M01XXXW11ITAPL GARNET             A001      000004                              M02005_XXXW000035                                                               P012704043012                                                                   H014              1606241001    0000                                            ZCR                               00000");
			var bill0 = Factory.New<Integration.Customs.US.USAMS.ICusInBondBill>();
			bill0.B0_BH = header.PK;
			bill0.B0_MasterBillNumber = "005";
			bill0.B0_InBondPortOfDestDCode = "2704";
			var moveDetail0 = Factory.Load<Integration.Customs.US.USAMS.ICusInBondMoveDetail>(new ZQuery(CusInBondMoveDetailSchema.B9_B0, bill0.PK))[0];
			moveDetail0.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;

			var bill1 = Factory.New<Integration.Customs.US.USAMS.ICusInBondBill>();
			bill1.B0_BH = header.PK;
			bill1.B0_MasterBillNumber = "006";
			bill1.B0_InBondPortOfDestDCode = "2704";

			header.BH_ETA = ZDateTime.Today.AddDays(-4);
			messageTransmit.EM_MessageNum = "XXXW000035";
			messageTransmit.EM_MessageOwner = Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Constants.ACE;
			messageTransmit.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival;
			messageTransmit.EM_MessageSubType = AMSMessageSubTypeList.Codes.VesselArrival;

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ReceiveTransmit = CBPEDIInterchange.Direction.Receive;
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse;
			interchange.EI_From = "USC";
			interchange.EI_To = "OTT1";
			interchange.EI_HeaderText = "ACR          HR12050719424811973                                                ";
			interchange.EI_BodyText = "M01XXXW11ITAPL GARNET             A001      000004                              M02005_XXXW000035                                                               P012704043012                                                                   W02XXXW1205071942480100100000000000000000001000000000100005                     ";
			interchange.EI_FooterText = "ZCR          HR                   00004                                         ";
			interchange.EI_Status = CBPEDIInterchange.Status.Queued;

			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse;
			message.EM_MessageText = interchange.EI_InterchangeText;
			message.EM_MessageSubType = AMSMessageSubTypeList.Codes.VesselArrival;
			message.EM_MessageNum = messageTransmit.EM_MessageNum;
			Factory.Save();

			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			var factory2 = new BusinessObjectFactory();
			var reloadedBill0 = factory2.Load<Integration.Customs.US.USAMS.ICusInBondBill>(bill0.PK);
			AssertEquals(new ZDateTime(2016, 06, 24), reloadedBill0.B0_A_ARV);
			var reloadedBill1 = factory2.Load<Integration.Customs.US.USAMS.ICusInBondBill>(bill1.PK);
			AssertEquals(ZDateTime.Empty, reloadedBill1.B0_A_ARV);
		}

		public void TestMessageDiscardedWhenNoMatchedJobFound()
		{
			var consol = GetConsol("ACR          MI                                                                 M01OTT111IT23                     FR345     000096 9204790                      M02HB1203130301_OTT1410                                                         P013902032512                                                                   J01OTT1                                                                         B01HB1203130301015200000000100PKG  0000001000KGN                                B020000000001CMSMTH                         OTT1    60267     01520             B04OB OB1203130301                                                              S01AUEXP COMPANY NAME                 AUEXP ADDRESS 1 AUEXP ADDRESS 2 AUE       S02XPCITY NSW 4345 AUSTRALIA                                                    S03+61 (2) 9845-6579                                                            U01ACE TEST IMPORTER 3                1 TEST STR CHICAGO IL 60666               U03+1 (312) 555-1212                                                            C0143545345345   435                           200                    4210E     C02234567                                                                       D001001100010 000000000000001000KG                                              D010000000100GOODS                                                      PKG     D02MARKS                                                                        ZCR                               00000");

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
			message.EM_MessageNum = "12345678";
			Factory.Save();
			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var factory2 = new BusinessObjectFactory();
			var messageReloaded = factory2.Load<EDIMessage>(message.PK);
			AssertNotNull(messageReloaded);
			AssertEquals(EDIMessage.Status.Discarded, messageReloaded.EM_Status);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		Integration.Forwarding.IForwardingConsol GetConsol(ZString messageText)
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
			messageTransmit.EM_SystemCreateUser = staffZ2.GS_Code;

			Factory.Save();
			return consol;
		}
		AMSEDIMessage messageTransmit;
		Integration.Customs.US.USAMS.ICusInBondHeader header;

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest => 0;
	}
}
