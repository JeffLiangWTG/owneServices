using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	[TestedType(typeof(StatusNotificationProcessor))]
	sealed class StatusNotificationProcessorTest : AMSProcessorTest
	{
		protected override void EndToEndCore()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", dataGrouping.ZZZ_DataGrouping);
			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition, "IMakeBondCloseDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition6263, "IMakeBondCloseDisposition6263", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName3 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.INeutralInBondDisposition, "INeutralInBondDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName4 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsExamDisposition, "IsExamDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName5 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldDisposition, "IsHoldDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName6 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldExamRemovedDisposition, "IsHoldExamRemovedDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName7 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.HoldRemovedExamCompletedMapCode, "HoldRemovedExamCompletedMapCode", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var cusCode1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "55", "Carrier bill - add", startDate, endDate);
			Factory.Save();

			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);

			var groupZZ2 = Factory.New<GlbGroup>();
			groupZZ2.GG_Code = "ZZ2";
			var staffZ3 = groupZZ2.Staff.AddNew();
			staffZ3.GS_Code = "Z3";
			staffZ3.GS_LoginName = "z3";
			staffZ3.GS_EmailAddress = "dong3@pretend.email.com";
			Factory.Save();

			FreightDataRegistry.Instance.USAMSGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", groupZZ2.PK, false));

			var consol = Factory.New(ObjectFactory.GetType<Integration.Forwarding.IForwardingConsol>());
			consol[JobConsolSchema.JK_TransportMode] = Core.Constants.TransportModes.Sea;
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
			var consolRef = new ZString(consol[JobConsolSchema.JK_UniqueConsignRef]);

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.StatusNotification;
			interchange.EI_From = "USC";
			interchange.EI_To = "OTT1";
			interchange.EI_HeaderText = "ACR8CWS      RC11051022481100089                                                ";
			interchange.EI_BodyText =
"M01OTT110AUAPL EMERALD            K34L      " + manifestSequenceNumber + " 7819369                      " +
"R01OTT12704APL EMERALD            K34L " + manifestSequenceNumber + "110513                             " +
"J01OTT1                                                                         " +
"R02HB1105031521550000000006000000000000000001105102248             1            " +
"R022704                              OB OTT1OB1105031518                        " +
"R03CARR AMEND ADD                                                               " +
"R05NC                                                                           ";
			interchange.EI_FooterText = "ZCR8CWS      RC                   00007";

			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.StatusNotification;
			message.EM_MessageText = interchange.EI_InterchangeText;

			Factory.Save();
			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			var expectedSubject = "Status Notification Response for " + consolRef + " - HB1105031521";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == expectedSubject));
			AssertNotNull("Email with subject '" + expectedSubject + "'should exist", email);
			AssertEquals(1, email.Recipients.Count);
			AssertEquals(staffZ3.GS_EmailAddress, email.Recipients[0].Email);
			var expectedMainEmailBody = string.Format(@"
<br />
A response message has been received from Customs.<br />Shown below is a summary of relevant information received in the message.<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Manifest Details</th></tr></thead><tr><td>Carrier Code</td><td>OTT1</td></tr><tr><td>CBP District/Port</td><td>2704</td></tr><tr><td>Mode of Transportation</td><td>10</td></tr><tr><td>Vessel Country</td><td>AU</td></tr><tr><td>Vessel Name</td><td>APL EMERALD</td></tr><tr><td>Voyage Number</td><td>K34L</td></tr><tr><td>Manifest Sequence Number</td><td>" + manifestSequenceNumber + @"</td></tr><tr><td>Estimated Date</td><td>13-May-11</td></tr></table><br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Bill of Lading Status Notification</th></tr></thead><tr><td>Issuer Code</td><td>OTT1</td></tr><tr><td>Bill of Lading Number</td><td>HB1105031521</td></tr><tr><td>Disposition Code</td><td>55 = Carrier bill - add</td></tr><tr><td>Quantity</td><td>6</td></tr><tr><td>Entry Type</td><td>00</td></tr><tr><td>Entry Number</td><td>000000000000000</td></tr><tr><td>Action Date</td><td>10-May-11</td></tr><tr><td>Action Time</td><td>2248</td></tr><tr><td>Negative Indicator</td><td>&nbsp;</td></tr><tr><td>Resend Indicator</td><td>&nbsp;</td></tr><tr><td>District/Port of Transaction</td><td>2704</td></tr><tr><td>FIRMS Code</td><td>&nbsp;</td></tr><tr><td>U.S. Port of Destination/ Intermediate Destination</td><td>&nbsp;</td></tr><tr><td>Foreign Destination</td><td>&nbsp;</td></tr><tr><td>Container Number</td><td>&nbsp;</td></tr><tr><td>Reference Number Qualifier</td><td>OB</td></tr><tr><td>Reference Number</td><td>OTT1OB1105031518</td></tr></table><br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Remarks</th></tr></thead><tr><td>CARR AMEND ADD</td></tr></table><br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Container Number</th><th>Seal Number 1</th><th>Seal Number 2</th></tr></thead><tr><td>NC</td><td>&nbsp;</td><td>&nbsp;</td></tr></table><br>
<br />
", manifestSequenceNumber);

			AssertContains(@"<strong>Status Notification Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=JobConsol&BusinessEntityPK=" + consol.PK.ToString(), email.Body);
			AssertContains(@""">" + consol[JobConsolSchema.JK_UniqueConsignRef].ToString() + " - HB1105031521</a></strong><br />", email.Body);
			AssertContains(expectedMainEmailBody, email.Body);
		}

		public void TestMatchingToCorrectConsol()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", dataGrouping.ZZZ_DataGrouping);
			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition, "IMakeBondCloseDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition6263, "IMakeBondCloseDisposition6263", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName3 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.INeutralInBondDisposition, "INeutralInBondDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName4 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsExamDisposition, "IsExamDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName5 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldDisposition, "IsHoldDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName6 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldExamRemovedDisposition, "IsHoldExamRemovedDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName7 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.HoldRemovedExamCompletedMapCode, "HoldRemovedExamCompletedMapCode", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var cusCode1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "55", "Carrier bill - add", startDate, endDate);
			Factory.Save();

			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			company2.Branches.Add(branch2);

			var groupZZ2 = Factory.New<GlbGroup>();
			groupZZ2.GG_Code = "ZZ2";
			var staffZ3 = groupZZ2.Staff.AddNew();
			staffZ3.GS_Code = "Z3";
			staffZ3.GS_LoginName = "z3";
			staffZ3.GS_EmailAddress = "dong3@pretend.email.com";
			Factory.Save();

			var consol1 = GetConsol();
			var header1 = GetCusInBondHeader(branch2, consol1) as Customs.Business.CusInBondHeader;
			GetCusInBondBil(header1, "OTT1", "MB1234");

			var consol2 = GetConsol();
			var header2 = GetCusInBondHeader(branch2, consol2) as Customs.Business.CusInBondHeader;
			header2.BH_IsActive = false;
			GetCusInBondBil(header2, "OTT1", "HB1105031521");

			var consol3 = GetConsol();
			var header3 = GetCusInBondHeader(branch2, consol3) as Customs.Business.CusInBondHeader;
			GetCusInBondBil(header3, "OTT1", "HB1105031521");

			FreightDataRegistry.Instance.USAMSGroupNotification.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", groupZZ2.PK, false));
			Factory.Save();

			var manifestSequenceNumber = "000001";
			var consol2Ref = new ZString(consol2[JobConsolSchema.JK_UniqueConsignRef]);
			var consol3Ref = new ZString(consol3[JobConsolSchema.JK_UniqueConsignRef]);

			GetEdiMessage(manifestSequenceNumber);

			Factory.Save();
			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			var expectedSubject2 = "Status Notification Response for " + consol2Ref + " - HB1105031521";
			var email2 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == expectedSubject2));
			AssertNull("Email with subject '" + expectedSubject2 + "'should not exist", email2);

			var expectedSubject3 = "Status Notification Response for " + consol3Ref + " - HB1105031521";
			var email3 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == expectedSubject3));
			AssertNotNull("Email with subject '" + expectedSubject3 + "'should exist", email3);

			GetEdiMessage(manifestSequenceNumber);
			header2.BH_IsActive = true;
			header3.BH_IsActive = false;
			Factory.Save();

			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			email2 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == expectedSubject2));
			AssertNotNull("Email with subject '" + expectedSubject2 + "'should exist", email2);

			AssertEquals(1, email2.Recipients.Count);
			AssertEquals(staffZ3.GS_EmailAddress, email2.Recipients[0].Email);
			var expectedMainEmailBody = string.Format(@"
<br />
A response message has been received from Customs.<br />Shown below is a summary of relevant information received in the message.<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Manifest Details</th></tr></thead><tr><td>Carrier Code</td><td>OTT1</td></tr><tr><td>CBP District/Port</td><td>2704</td></tr><tr><td>Mode of Transportation</td><td>10</td></tr><tr><td>Vessel Country</td><td>AU</td></tr><tr><td>Vessel Name</td><td>APL EMERALD</td></tr><tr><td>Voyage Number</td><td>K34L</td></tr><tr><td>Manifest Sequence Number</td><td>{0}</td></tr><tr><td>Estimated Date</td><td>13-May-11</td></tr></table><br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Bill of Lading Status Notification</th></tr></thead><tr><td>Issuer Code</td><td>OTT1</td></tr><tr><td>Bill of Lading Number</td><td>HB1105031521</td></tr><tr><td>Disposition Code</td><td>55 = Carrier bill - add</td></tr><tr><td>Quantity</td><td>6</td></tr><tr><td>Entry Type</td><td>00</td></tr><tr><td>Entry Number</td><td>000000000000000</td></tr><tr><td>Action Date</td><td>10-May-11</td></tr><tr><td>Action Time</td><td>2248</td></tr><tr><td>Negative Indicator</td><td>&nbsp;</td></tr><tr><td>Resend Indicator</td><td>&nbsp;</td></tr><tr><td>District/Port of Transaction</td><td>2704</td></tr><tr><td>FIRMS Code</td><td>&nbsp;</td></tr><tr><td>U.S. Port of Destination/ Intermediate Destination</td><td>&nbsp;</td></tr><tr><td>Foreign Destination</td><td>&nbsp;</td></tr><tr><td>Container Number</td><td>&nbsp;</td></tr><tr><td>Reference Number Qualifier</td><td>OB</td></tr><tr><td>Reference Number</td><td>OTT1OB1105031518</td></tr></table><br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Remarks</th></tr></thead><tr><td>CARR AMEND ADD</td></tr></table><br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Container Number</th><th>Seal Number 1</th><th>Seal Number 2</th></tr></thead><tr><td>NC</td><td>&nbsp;</td><td>&nbsp;</td></tr></table><br>
<br />
", manifestSequenceNumber);

			AssertContains(@"<strong>Status Notification Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIDANDAT&ControllerID=JobConsol&BusinessEntityPK=" + consol2.PK.ToString(), email2.Body);
			AssertContains(@""">" + consol2[JobConsolSchema.JK_UniqueConsignRef].ToString() + " - HB1105031521</a></strong><br />", email2.Body);
			AssertContains(expectedMainEmailBody, email2.Body);
		}

		BusinessObject GetConsol()
		{
			var consol = Factory.New(ObjectFactory.GetType<Integration.Forwarding.IForwardingConsol>());
			consol[JobConsolSchema.JK_TransportMode] = Core.Constants.TransportModes.Sea;
			return consol;
		}

		Integration.Customs.US.USAMS.ICusInBondHeader GetCusInBondHeader(GlbBranch branch, BusinessObject consol)
		{
			var header = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_CarrierSCAC = "OTT1";
			header.BH_ImportConveyanceName = "APL EMERALD";
			header.BH_VoyageNumber = "K34L";
			header.BH_PortUnladingDCode = "2704";
			header.BH_ETA = new ZDate(2011, 5, 13);
			header.BH_GB = branch.PK;
			return header;
		}

		Integration.Customs.US.USAMS.ICusInBondBill GetCusInBondBil(Customs.Business.CusInBondHeader header, string issuerCode, string masterBillNumber)
		{
			var bill = Factory.New<Integration.Customs.US.USAMS.ICusInBondBill>();
			bill.B0_BH = header.PK;
			bill.B0_IssuerCode = issuerCode;
			bill.B0_MasterBillNumber = masterBillNumber;
			return bill;
		}

		void GetEdiMessage(string manifestSequenceNumber)
		{
			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.StatusNotification;
			interchange.EI_From = "USC";
			interchange.EI_To = "OTT1";
			interchange.EI_HeaderText = "ACR8CWS      RC11051022481100089                                                ";
			interchange.EI_BodyText =
"M01OTT110AUAPL EMERALD            K34L      " + manifestSequenceNumber + " 7819369                      " +
"R01OTT12704APL EMERALD            K34L " + manifestSequenceNumber + "110513                             " +
"J01OTT1                                                                         " +
"R02HB1105031521550000000006000000000000000001105102248             1            " +
"R022704                              OB OTT1OB1105031518                        " +
"R03CARR AMEND ADD                                                               " +
"R05NC                                                                           ";
			interchange.EI_FooterText = "ZCR8CWS      RC                   00007";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;

			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.StatusNotification;
			message.EM_MessageText = interchange.EI_InterchangeText;
		}

		public void TestVesselEventCode()
		{
			var consol = Factory.New(ObjectFactory.GetType<Integration.Forwarding.IForwardingConsol>());
			consol[JobConsolSchema.JK_TransportMode] = Core.Constants.TransportModes.Sea;
			var header = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_CarrierSCAC = "OTT1";
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			header.BH_ImportConveyanceName = "APL EMERALD";
			header.BH_VoyageNumber = "897";
			header.BH_PortUnladingDCode = "2704";
			header.BH_ETA = new ZDateTime(2011, 7, 31, 11, 4, 42);
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
			var manifestSequenceNumber = "100101";
			moveHeder.BM_ManifestSequenceNumber = manifestSequenceNumber;
			var msnQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, moveHeder.PK);
			msnQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, "MSN");
			var mSNCusEntryNumber = (BusinessObject)Factory.LoadTop1<Integration.Customs.ICusEntryNumber>(msnQuery);
			mSNCusEntryNumber[CusEntryNumSchema.CE_EntryLineReference] = "OTT1";
			Factory.Save();

			var consolRef = new ZString(consol[JobConsolSchema.JK_UniqueConsignRef]);

			var messageReceive = CreateInterchangeAndMessageResponse(AMSApplicationIdentifierCodeList.Codes.StatusNotification, "ACR          RC12052101013112729                                                ",
				"M01OTT111AUAPL EMERALD            897       " + manifestSequenceNumber + " 7819369                      " +
				"R01OTT12704APL EMERALD            897  " + manifestSequenceNumber + "110731                             " +
				"R06OCA1108150100                                                                " +
				"R03VESSEL ARRIVAL OVERDUE                                                       ", "ZCR          RC                   00004                                         ", "~012121");
			Factory.Save();

			var vesselEventFilter = new ZQuery(CusAddInfoSchema.B7_ParentID, header.PK);
			vesselEventFilter.AddToFilter(CusAddInfoSchema.B7_Type, "UDP");
			var vesselEvents = Factory.Load<Integration.Customs.US.IDispositionData>(vesselEventFilter);
			AssertEquals(0, vesselEvents.Length);
			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			vesselEvents = Factory.Load<Integration.Customs.US.IDispositionData>(vesselEventFilter);
			AssertEquals(1, vesselEvents.Length);
			var vesselEvent = vesselEvents[0];
			AssertEquals("Code", "OCA", vesselEvent.US_Code);
			AssertEquals("DispositionDate", new ZDateTime(2011, 8, 15, 1, 0, 0), vesselEvent.US_DispositionDate);

			var expectedSubject = "Status Notification Response for " + consolRef;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == expectedSubject));
			AssertNotNull("Email with subject '" + expectedSubject + "'should exist", email);
			AssertEquals(1, email.Recipients.Count);
			AssertEquals(amsMessageStaff.GS_EmailAddress, email.Recipients[0].Email);
			var expectedMainEmailBody = string.Format(@"
<br />
A response message has been received from Customs.<br />Shown below is a summary of relevant information received in the message.<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Manifest Details</th></tr></thead><tr><td>Carrier Code</td><td>OTT1</td></tr><tr><td>CBP District/Port</td><td>2704</td></tr><tr><td>Mode of Transportation</td><td>11</td></tr><tr><td>Vessel Country</td><td>AU</td></tr><tr><td>Vessel Name</td><td>APL EMERALD</td></tr><tr><td>Voyage Number</td><td>897</td></tr><tr><td>Manifest Sequence Number</td><td>" + manifestSequenceNumber + @"</td></tr><tr><td>Estimated Date</td><td>31-Jul-11</td></tr></table><br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Conveyance Event</th></tr></thead><tr><td>Event Code</td><td>OCA = Arrival overdue</td></tr><tr><td>Action Date</td><td>15-Aug-11</td></tr><tr><td>Action Time</td><td>0100</td></tr></table><br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Remarks</th></tr></thead><tr><td>VESSEL ARRIVAL OVERDUE</td></tr></table><br>
<br />
", manifestSequenceNumber);

			AssertContains(@"<strong>Status Notification Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=JobConsol&BusinessEntityPK=" + consol.PK.ToString(), email.Body);
			AssertContains(@""">" + consolRef + "</a></strong><br />", email.Body);
			AssertContains(expectedMainEmailBody, email.Body);
		}

		public void TestBillDispositionCode()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", dataGrouping.ZZZ_DataGrouping);
			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition, "IMakeBondCloseDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition6263, "IMakeBondCloseDisposition6263", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName3 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.INeutralInBondDisposition, "INeutralInBondDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName4 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsExamDisposition, "IsExamDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName5 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldDisposition, "IsHoldDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName6 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldExamRemovedDisposition, "IsHoldExamRemovedDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName7 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.HoldRemovedExamCompletedMapCode, "HoldRemovedExamCompletedMapCode", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var cusCode1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "69", "Bill on File", startDate, endDate);
			Factory.Save();

			var consol = Factory.New(ObjectFactory.GetType<Integration.Forwarding.IForwardingConsol>());
			consol[JobConsolSchema.JK_TransportMode] = Core.Constants.TransportModes.Sea;
			var header = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_CarrierSCAC = "XXXW";
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			header.BH_ImportConveyanceName = "TEST STEP 1";
			header.BH_VoyageNumber = "10000";
			header.BH_PortUnladingDCode = "2704";
			header.BH_ETA = new ZDateTime(2012, 5, 22, 11, 4, 42);
			var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, header.PK);
			query.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SubApplicationCodeList.Codes.AMS);
			query.FetchOnlyFromLocalCache = true;
			var moveHeder = Factory.LoadTop1<Integration.Customs.US.USAMS.ICusInBondMoveHeader>(query);
			moveHeder.BM_ManifestSequenceNumber = "000001";
			var bill1 = Factory.New<Integration.Customs.US.USAMS.ICusInBondBill>();
			bill1.B0_BH = header.PK;
			bill1.B0_IssuerCode = "XXXZ";
			bill1.B0_MasterBillNumber = "HB1190002222";
			var bill2 = Factory.New<Integration.Customs.US.USAMS.ICusInBondBill>();
			bill2.B0_BH = header.PK;
			bill2.B0_IssuerCode = "XXXW";
			bill2.B0_MasterBillNumber = "HB1190001111";
			var bill3 = Factory.New<Integration.Customs.US.USAMS.ICusInBondBill>();
			bill3.B0_BH = header.PK;
			bill3.B0_IssuerCode = "XXXZ";
			bill3.B0_MasterBillNumber = "HB1190001111";
			query = new ZQuery(CusInBondMoveDetailSchema.B9_BM, moveHeder.PK);
			query.AddToFilter(CusInBondMoveDetailSchema.B9_B0, bill1.PK);
			query.FetchOnlyFromLocalCache = true;
			var moveDetail1 = Factory.LoadTop1<Integration.Customs.US.USAMS.ICusInBondMoveDetail>(query);
			query = new ZQuery(CusInBondMoveDetailSchema.B9_BM, moveHeder.PK);
			query.AddToFilter(CusInBondMoveDetailSchema.B9_B0, bill2.PK);
			query.FetchOnlyFromLocalCache = true;
			var moveDetail2 = Factory.LoadTop1<Integration.Customs.US.USAMS.ICusInBondMoveDetail>(query);
			query = new ZQuery(CusInBondMoveDetailSchema.B9_BM, moveHeder.PK);
			query.AddToFilter(CusInBondMoveDetailSchema.B9_B0, bill3.PK);
			query.FetchOnlyFromLocalCache = true;
			var moveDetail3 = Factory.LoadTop1<Integration.Customs.US.USAMS.ICusInBondMoveDetail>(query);

			var messageTransmit = Factory.New<AMSEDIMessage>();
			messageTransmit.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			messageTransmit.EM_LinkedObject = (BusinessObject)moveHeder;
			messageTransmit.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			messageTransmit.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			messageTransmit.EM_MessageNum = "~000000007";
			Factory.Save();

			var manifestSequenceNumber = moveHeder.BM_ManifestSequenceNumber.PadRight(6);
			var consolRef = new ZString(consol[JobConsolSchema.JK_UniqueConsignRef]);

			var messageReceive = CreateInterchangeAndMessageResponse(AMSApplicationIdentifierCodeList.Codes.StatusNotification,
				"ACR          RC12052101143512794                                                ",
				"M01XXXW10ATTEST STEP 1            10000     " + manifestSequenceNumber + " 1111111                      " +
				"R01XXXW2704TEST STEP 1            10000" + manifestSequenceNumber + "120522                             " +
				"J01XXXZ                                                                         " +
				"R02HB1190001111690000001111                 1205210103             1            " +
				"R022704                              OB XXXWHB1190001101                        " +
				"B04OB XXXWHB1190001101                                                          " +
				"B04SNPXXXW                                                                      " +
				"R03BILL ON FILE                                                                 " +
				"R05NC                                                                           ",
				"ZCR          RC                   00009                                         ", "~012121");
			Factory.Save();

			var bill1DispositionFilter = new ZQuery(CusAddInfoSchema.B7_ParentID, bill1.PK);
			bill1DispositionFilter.AddToFilter(CusAddInfoSchema.B7_Type, "UDP");
			var bill2DispositionFilter = new ZQuery(CusAddInfoSchema.B7_ParentID, bill2.PK);
			bill2DispositionFilter.AddToFilter(CusAddInfoSchema.B7_Type, "UDP");
			var bill3DispositionFilter = new ZQuery(CusAddInfoSchema.B7_ParentID, bill3.PK);
			bill3DispositionFilter.AddToFilter(CusAddInfoSchema.B7_Type, "UDP");
			AssertEquals("Bill1 Disposition", 0, Factory.Load<Integration.Customs.US.IDispositionData>(bill1DispositionFilter).Length);
			AssertEquals("Bill2 Disposition", 0, Factory.Load<Integration.Customs.US.IDispositionData>(bill2DispositionFilter).Length);
			AssertEquals("Bill3 Disposition", 0, Factory.Load<Integration.Customs.US.IDispositionData>(bill3DispositionFilter).Length);
			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			AssertEquals("Bill1 Disposition", 0, Factory.Load<Integration.Customs.US.IDispositionData>(bill1DispositionFilter).Length);
			AssertEquals("Bill2 Disposition", 0, Factory.Load<Integration.Customs.US.IDispositionData>(bill2DispositionFilter).Length);
			var bill3Dispositions = Factory.Load<Integration.Customs.US.IDispositionData>(bill3DispositionFilter);
			AssertEquals(1, bill3Dispositions.Length);
			var bill3Disposition = bill3Dispositions[0];
			AssertEquals("Code", "69", bill3Disposition.US_Code);
			AssertEquals("DispositionDate", new ZDateTime(2012, 5, 21, 1, 3, 0), bill3Disposition.US_DispositionDate);
			messageReceive.Reload();
			AssertEquals(messageReceive.EM_ApplicationReference, bill3.PK.ToString());

			var expectedSubject = "Status Notification Response for " + consolRef + " - HB1190001111";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == expectedSubject));
			AssertNotNull("Email with subject '" + expectedSubject + "'should exist", email);
			AssertEquals(1, email.Recipients.Count);
			AssertEquals(amsMessageStaff.GS_EmailAddress, email.Recipients[0].Email);
			var expectedMainEmailBody = string.Format(@"
<br />
A response message has been received from Customs.<br />Shown below is a summary of relevant information received in the message.<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Manifest Details</th></tr></thead><tr><td>Carrier Code</td><td>XXXW</td></tr><tr><td>CBP District/Port</td><td>2704</td></tr><tr><td>Mode of Transportation</td><td>10</td></tr><tr><td>Vessel Country</td><td>AT</td></tr><tr><td>Vessel Name</td><td>TEST STEP 1</td></tr><tr><td>Voyage Number</td><td>10000</td></tr><tr><td>Manifest Sequence Number</td><td>000001</td></tr><tr><td>Estimated Date</td><td>22-May-12</td></tr></table><br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Bill of Lading Status Notification</th></tr></thead><tr><td>Issuer Code</td><td>XXXZ</td></tr><tr><td>Bill of Lading Number</td><td>HB1190001111</td></tr><tr><td>Disposition Code</td><td>69 = Bill on File</td></tr><tr><td>Quantity</td><td>1111</td></tr><tr><td>Entry Type</td><td>&nbsp;</td></tr><tr><td>Entry Number</td><td>&nbsp;</td></tr><tr><td>Action Date</td><td>21-May-12</td></tr><tr><td>Action Time</td><td>0103</td></tr><tr><td>Negative Indicator</td><td>&nbsp;</td></tr><tr><td>Resend Indicator</td><td>&nbsp;</td></tr><tr><td>District/Port of Transaction</td><td>2704</td></tr><tr><td>FIRMS Code</td><td>&nbsp;</td></tr><tr><td>U.S. Port of Destination/ Intermediate Destination</td><td>&nbsp;</td></tr><tr><td>Foreign Destination</td><td>&nbsp;</td></tr><tr><td>Container Number</td><td>&nbsp;</td></tr><tr><td>Reference Number Qualifier</td><td>OB</td></tr><tr><td>Reference Number</td><td>XXXWHB1190001101</td></tr></table><br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Remarks</th></tr></thead><tr><td>BILL ON FILE</td></tr></table><br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Container Number</th><th>Seal Number 1</th><th>Seal Number 2</th></tr></thead><tr><td>NC</td><td>&nbsp;</td><td>&nbsp;</td></tr></table><br>
<br />
", manifestSequenceNumber);

			AssertContains(@"<strong>Status Notification Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=JobConsol&BusinessEntityPK=" + consol.PK.ToString(), email.Body);
			AssertContains(@""">" + consolRef + " - HB1190001111</a></strong><br />", email.Body);
			AssertContains(expectedMainEmailBody, email.Body);
		}

		public void TestNoRecipientNoEmailNotificationSent()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			company2.Branches.Add(branch2);

			var groupZZ2 = Factory.New<GlbGroup>();
			groupZZ2.GG_Code = "ZZ2";
			var staffZ3 = groupZZ2.Staff.AddNew();
			staffZ3.GS_Code = "Z3";
			staffZ3.GS_LoginName = "z3";

			Factory.Save();

			var consol1 = GetConsol();
			var header1 = GetCusInBondHeader(branch2, consol1) as Customs.Business.CusInBondHeader;
			GetCusInBondBil(header1, "OTT1", "MB1234");

			var consol2 = GetConsol();
			var header2 = GetCusInBondHeader(branch2, consol2) as Customs.Business.CusInBondHeader;
			header2.BH_IsActive = false;
			GetCusInBondBil(header2, "OTT1", "HB1105031521");

			var consol3 = GetConsol();
			var header3 = GetCusInBondHeader(branch2, consol3) as Customs.Business.CusInBondHeader;
			GetCusInBondBil(header3, "OTT1", "HB1105031521");
			var query3 = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, header2.PK);
			query3.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SubApplicationCodeList.Codes.AMS);
			query3.FetchOnlyFromLocalCache = true;
			var moveHeder3 = Factory.LoadTop1<Integration.Customs.US.USAMS.ICusInBondMoveHeader>(query3);
			var messageTransmit3 = Factory.New<AMSEDIMessage>();
			messageTransmit3.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			messageTransmit3.EM_LinkedObject = header3;
			messageTransmit3.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			messageTransmit3.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			messageTransmit3.EM_MessageNum = "0000000007";

			FreightDataRegistry.Instance.USAMSGroupNotification.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, groupZZ2.PK, false));
			var staffUser2 = Factory.New<GlbStaff>();
			staffUser2.GS_Code = "U2";
			staffUser2.GS_LoginName = "U2";
			staffUser2.GS_EmailAddress = "U2@U2.email.com";
			Factory.Save();

			var manifestSequenceNumber = "000001";
			var consol2Ref = new ZString(consol2[JobConsolSchema.JK_UniqueConsignRef]);
			var consol3Ref = new ZString(consol3[JobConsolSchema.JK_UniqueConsignRef]);
			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			var expectedSubject2 = "Status Notification Response for " + consol2Ref + " - HB1105031521";
			var email2 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == expectedSubject2));
			AssertNull("Email of Job2 should not exist", email2);

			var expectedSubject3 = "Status Notification Response for " + consol3Ref + " - HB1105031521";
			var email3 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == expectedSubject3));
			AssertNull("Email of Job3 should not exist", email3);

			GetEdiMessage(manifestSequenceNumber);
			header2.BH_IsActive = true;
			header3.BH_IsActive = false;
			staffZ3.GS_EmailAddress = "dong3@pretend.email.com";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			email2 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == expectedSubject2));
			AssertNotNull("Email with subject '" + expectedSubject2 + "'should exist", email2);
			AssertEquals(1, email2.Recipients.Count);
			AssertEquals(staffZ3.GS_EmailAddress, email2.Recipients[0].Email);

			var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, header2.PK);
			query.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SubApplicationCodeList.Codes.AMS);
			query.FetchOnlyFromLocalCache = true;
			var moveHeder2 = Factory.LoadTop1<Integration.Customs.US.USAMS.ICusInBondMoveHeader>(query);
			var messageTransmit2 = Factory.New<AMSEDIMessage>();
			messageTransmit2.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			messageTransmit2.EM_LinkedObject = (BusinessObject)moveHeder2;
			messageTransmit2.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			messageTransmit2.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			messageTransmit2.EM_MessageNum = "0000000007";
			messageTransmit2.EM_SystemCreateUser = staffUser2.GS_Code;
			GetEdiMessage(manifestSequenceNumber);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			expectedSubject2 = "Status Notification Response for " + consol2Ref + " - HB1105031521";
			email2 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == expectedSubject2));
			AssertEquals(2, email2.Recipients.Count);
			AssertEquals("email from Job", true, email2.Recipients.Contains(staffUser2.GS_EmailAddress));
			AssertEquals("email from Group Registry", true, email2.Recipients.Contains(staffZ3.GS_EmailAddress));
		}

		[TestDate(2012, 06, 19)]
		public void TestAttachToInBondHeader()
		{
			var header = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_ApplicationCode = "INB";
			header.BH_CarrierSCAC = "CARL";
			header.BH_GB = Env.CurrentBranch.PK;
			header.BH_VoyageNumber = "ST013";
			header.BH_ImportConveyanceName = "HYUNDAI SINGAPORE";
			header.BH_PortUnladingDCode = "0901";
			header.BH_ETA = new ZDateTime(2012, 06, 19);

			var moveHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			moveHeader.InBondNumber = "333210146";

			var headerBill = Factory.New<Integration.Customs.US.InBond.ICusInBondBill>();
			headerBill.B0_BH = header.PK;
			headerBill.B0_IssuerCode = "CARL";
			headerBill.B0_MasterBillNumber = "CBP01307";

			var moveHederDetail = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveDetail>();
			moveHederDetail.B9_BM = moveHeader.PK;
			moveHederDetail.B9_B0 = headerBill.PK;
			Factory.Save();

			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.StatusNotification;
			interchange.EI_From = "USC";
			interchange.EI_To = "HYEDUKCMT";
			interchange.EI_HeaderText = "ACR          RC12061920375415774                                                ";
			interchange.EI_BodyText = "M01CARL30ITHYUNDAI SINGAPORE      ST013     000001                              R01CARL0901HYUNDAI SINGAPORE      ST013000001120619                             J01CARL                                                                         R02CBP01307    69000000000462333210146      1206192037             1            R020901    390262200                                                            B04SNPOTT1                                                                      R03BILL ON FILE                                                                 R05NC                                                                           ";
			interchange.EI_FooterText = "ZCR          RC                   00008                                         ";

			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.StatusNotification;
			message.EM_MessageText = interchange.EI_InterchangeText;

			Factory.Save();
			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			message.Reload();
			AssertEquals("CusInBondMoveHeader", message.EM_LinkTable);
			AssertEquals(moveHeader.PK, message.EM_LinkUniqueID);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest => 0;
	}
}
