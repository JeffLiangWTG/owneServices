using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.DataTransfer.Testing
{
	[TestedType(typeof(CusISFHeaderWorkflowDescriptor))]
	sealed class CusISFHeaderWorkflowDescriptorTest : WorkflowDescriptorTestCase<CusISFHeaderWorkflowDescriptor>
	{
		public void TestProcessThroughWorkFlow()
		{
			var header = Factory.New<CusISFHeader>();
			Factory.Save();

			var processor = new LogSubscriberForTest();
			using (new FactorySaveAlerterForTest("Log Walker"))
			{
				AssertNoExceptionThrown(() =>
				{
					processor.Process(new CusISFHeader[] { header });
				});
			}
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", JobInvoicingConsumerTypes.ImporterSecurityFiling.Code, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Description", JobInvoicingConsumerTypes.ImporterSecurityFiling.Description, WorkflowDescriptor.Description);
		}

		public void TestMilestoneTemplateHintCaption()
		{
			AssertEquals("", WorkflowDescriptor.MilestoneTemplateHintCaption);
		}

		public override void TestSubTypes()
		{
			AssertEquals("0 sub type", 0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public void TestConsigneeMessageRecipientParty()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			var mode = header.Importer.EDICommunicationsModes.AddNew();
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			mode.EK_Module = JobInvoicingConsumerTypes.ImporterSecurityFiling.Code;
			mode.EK_Destination = "dummy@dummy.com";
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			mode.EK_ServerAddressSubject = "DUMMY SUBJECT";
			var task = header.WorkflowItems.Triggers.AddNew();
			task.P9_Description = "DUMMY EMAIL TESTING";
			task.TriggerConditions.TriggerEventCode = Events.MessageStatusChange.Code;
			var notification = task.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignee;
			notification.OverrideEmail = true;
			notification.PQ_EmailText = "DUMMY TEST BODY";
			NotificationBuffer buffer = new NotificationBuffer();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory)).Process(buffer);
			Factory.Save();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("DUMMY TEST BODY", email.Body);
			AssertEquals("DUMMY SUBJECT", email.Subject);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		[TestDate(2009, 6, 10)]
		public void TestExtraDataSubstitution()
		{
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new EntryFiler() { EntryFilerCode = "XJ5" });
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "8888");
			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "MR IMPORTER OF RECORD";
			importer.OH_Code = "IMP123AU12";
			importer.MainAddress.OA_Address1 = "IMPORTER OF RECORD 1";

			var uSCompany = Factory.New<GlbCompany>();
			uSCompany.GC_Code = "Z1Z";
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			uSCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var uSBranch = uSCompany.Branches.AddNew();
			uSBranch.GB_Code = "Z1Z";
			uSBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_GB = uSBranch.PK;
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.CompliantTransaction;
			header.BF_CustomsReference = "B3232BD32343";
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFAdd;
			header.BF_OH_Importer = importer.PK;
			header.BF_ImporterCodeType = CodeTypeList.Codes.IRS;
			header.BF_ImporterCode = "91-013199000";
			header.BF_ConsigneeCodeType = CodeTypeList.Codes.EncryptedConsigneeNumber;
			header.BF_ConsigneeCode = "-45678912341";
			header.BF_JobReference = "652314569";
			header.BF_RL_NKPlaceOfDelivery = "USLAX";
			header.BF_RL_NKPortOfUnload = "USCHI";
			header.BF_SCAC = "CQTP";
			header.BF_ShipmentType = ShipmentTypeList.Codes.MilitaryAndGovernment;
			header.BF_HouseBill = "SCAC234323423";
			header.HouseBill.BB_MatchDate = new ZDateTime(2009, 2, 25);
			header.BF_FirstAcceptedDate = new ZDateTime(2009, 6, 5);
			header.BF_LastAcceptedDate = new ZDateTime(2009, 6, 9);

			CusISFBill bill1 = header.HouseBill;
			bill1.BB_CustomsStatus = DispositionCodeList.Codes.S4;
			CusISFBill bill2 = header.ReferenceDatas.AddNew();
			bill2.BB_BillType = Common.US.ISF.BillTypeList.Codes.MasterBillOfLading;
			bill2.BB_BillNum = "SCAC6864658445";
			bill2.BB_CustomsStatus = DispositionCodeList.Codes.S3;
			CusISFBill bill3 = header.ReferenceDatas.AddNew();
			bill3.BB_BillType = Common.US.ISF.BillTypeList.Codes.MasterBillOfLading;
			bill3.BB_BillNum = "MB6585466885";
			bill3.BB_CustomsStatus = DispositionCodeList.Codes.S2;
			CusISFBill bill4 = header.ReferenceDatas.AddNew();
			bill4.BB_BillType = Common.US.ISF.BillTypeList.Codes.HouseBillOfLading;
			bill4.BB_BillNum = "HB5344685544";
			bill4.BB_CustomsStatus = DispositionCodeList.Codes.S1;
			bill4.BB_MatchDate = new ZDateTime(2009, 2, 24);

			CusISFLine line1 = header.Lines.AddNew();
			line1.BL_HarmonisedNum = "10.10.8120";
			line1.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;

			US.Business.MQEDIMessage message1 = (US.Business.MQEDIMessage)header.Messages.AddNew(typeof(US.Business.MQEDIMessage));
			message1.EM_ApplicationCode = Enterprise.Customs.US.Business.EDIMessage.ApplicationCodes.USCustomsImport;
			message1.EM_ReceiveTransmit = Enterprise.Customs.US.Business.EDIMessage.Direction.Receive;
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			message1.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFAdd;
			message1.EM_MessageNum = "~150000";
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2009, 2, 23, 10, 30, 0);

			message1.EM_MessageText =
				"B018888XJ5SN                                               ~150000              " +
				"SF30MF MAINFREIGHT INTERNATIONAL             65007252333                        " +
				"SF401042334668US                                                                " +
				"SF90  404INVALID HTS CODE                                                       " +
				"SF9001   SECURITY FILING REJECTED                                               " +
				"Y  8888XJ5SN0004";

			US.Business.MQEDIMessage message2 = (US.Business.MQEDIMessage)header.Messages.AddNew(typeof(US.Business.MQEDIMessage));
			message2.EM_ApplicationCode = Enterprise.Customs.US.Business.EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = Enterprise.Customs.US.Business.EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			message2.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFAdd;
			message2.EM_MessageNum = "~150001";
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2009, 2, 23, 11, 30, 0);

			message2.EM_MessageText =
				"B018888XJ5SN                                               ~150001              " +
				"SF10101A  EI 91-013199000           10XJ5-20089367423    91-013199000        N  " +
				"SF15BMBM12356897                                                                " +
				"SF15BMBM56846559                                                                " +
				"SF20MB MB56846859                                                               " +
				"SF9002   ISF ACCEPTED                                                           " +
				"Y  8888XJ5SN00005";

			ProcessTask trigger = header.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "TEST TRIGGER " + TriggerActionCommunicationModeSubstitutor.ReplacementConstants.JobNumber + "-" + CusISFHeaderWorkflowDescriptor.ReplacementConstants.LastestMessageStatus;
			trigger.TriggerConditions.TriggerEventCode = Events.MessageStatusChange.Code;
			ProcessTaskNotification notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_EmailAddr = "dummy@where.com";
			notification.OverrideEmail = true;
			notification.PQ_EmailText = string.Format(@"Description:'{0}'
JobNumber:'{1}'
CBPResponse:'{2}'
ReferenceIDs:'{3}'
ValidationMessage:'{4}'
MessageStatuses:'{5}'
LastestMessageStatus:'{6}'
CustomsReference:'{7}'
LastABIStatus:'{8}'
ReferenceID:'{9}'
BillMatchedDate:'{14}'
ImporterOfRecord:'{10}'
ImporterOfRecordCode:'{11}'
First Accepted Date:'{12}'
Last Accepted Date:'{13}'",
						 TriggerActionCommunicationModeSubstitutor.ReplacementConstants.Description,
						 TriggerActionCommunicationModeSubstitutor.ReplacementConstants.JobNumber,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.CBPResponse,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.ReferenceIDs,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.ValidationMessage,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.MessageStatuses,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.LastestMessageStatus,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.CustomsReference,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.LastABIStatus,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.ReferenceID,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.ImporterOfRecord,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.ImporterOfRecordCode,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.FirstAcceptedDate,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.LastAcceptedDate,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.BillMatchedDate);
			NotificationBuffer buffer = new NotificationBuffer();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory)).Process(buffer);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			string expectedBody = string.Format(@"Description:'TEST TRIGGER 652314569-Clear ISF Add'
JobNumber:'652314569'
CBPResponse:'<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>CBP Response</th></tr></thead><tr><td>------------------APLB------------------<BR /> Block Number (2-3)                  :1<BR /> Processing District Port Code (4-7) :8888<BR /> Entry Filer Code (8-10)             :XJ5<BR /> Application Identifier (11-12)      :SN<BR /> User Data (60-80)                   :~150001<BR /><BR />----------------ISFSF10-----------------<BR /> I S F Submission Type (5-5)             :1<BR /> Shipment Type Code (6-7)                :01<BR /> Action Code (8-8)                       :A<BR /> I S F Importer Number Qualifier (11-13) :EI<BR /> I S F Importer Number (14-28)           :91-013199000<BR /> Mode Of Transportation Code (37-38)     :10<BR /> I S F Transaction Number (39-53)        :XJ5-20089367423<BR /> Bond Holder (58-72)                     :91-013199000<BR /><BR />----------------ISFSF15-----------------<BR /> Code Qualifier (5-6)                 :BM<BR /> Shipment Reference Identifier (7-56) :BM12356897<BR /><BR />----------------ISFSF15-----------------<BR /> Code Qualifier (5-6)                 :BM<BR /> Shipment Reference Identifier (7-56) :BM56846559<BR /><BR />----------------ISFSF20-----------------<BR /> Reference Identifier Qualifier (5-7) :MB<BR /> Reference Identifier (8-57)          :MB56846859<BR /><BR />----------------ISFSF90-----------------<BR /> Message Type Code (5-6)        :02<BR /> Narrative Message Text (10-49) :ISF ACCEPTED<BR /><BR />------------------APLY------------------<BR /> Processing District Port Code (4-7)                       :8888<BR /> Entry Filer Code (8-10)                                   :XJ5<BR /> Application Identifier (11-12)                            :SN<BR /> Number Of Transaction Detail Records In The Block (13-17) :5<BR /><BR /></td></tr></table>'
ReferenceIDs:'<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Of Lading</th><th>Status</th><th>Matched Date</th></tr></thead><tr><td>SCAC234323423</td><td>S4 - ISF bill is NOT on file in AMS 20 days after filing the ISF.</td><td>25-FEB-09</td></tr><tr><td>HB5344685544</td><td>S1 - ISF is matched to an AMS bill of lading.</td><td>24-FEB-09</td></tr></table>'
ValidationMessage:'<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Validation Message(s)</th></tr></thead><tr><td>Error - Customs Reference: The Customs Reference format must be FFF-NNNNNNNNNNN (FFF= Entry Filer Code and NNNNNNNNNNN= numeric sequence number).</td></tr><tr><td>Message Error - Buying Party: Organization: A Buying Party must be specified when Entry Type is &#39;1&#39;.</td></tr><tr><td>Message Error - Consolidator Address: Organization: A Consolidator Address must be specified when Entry Type is &#39;1&#39;.</td></tr><tr><td>Message Error - Container Stuffing Location: Organization: A Container Stuffing Location must be specified when Entry Type is &#39;1&#39;.</td></tr><tr><td>Message Error - Formatted Harmonised Num: Tariff &#39;10108120&#39; is too short; the minimum number of digits required is &#39;10&#39;.</td></tr><tr><td>Message Error - Manufacturer: Manufacturer is required when Entry Type is &#39;1&#39;, &#39;3&#39; or &#39;5&#39;.</td></tr><tr><td>Message Error - Selling Party: Organization: A Selling Party must be specified when Entry Type is &#39;1&#39;.</td></tr><tr><td>Message Error - Ship To Party: Organization: A Ship To Party must be specified when Entry Type is &#39;1&#39;.</td></tr><tr><td>Warning - Customs Reference: The Customs Reference should only be entered in the event that you are trying to modify a job originating from another system.</td></tr><tr><td>Warning - Importer Identification: Unable to verify validity of this EIN, since it does not belong to any organization in the system.</td></tr><tr><td>Warning - Importer: There is no Power of Attorney against this organization. Please place your cursor on the organization and press F3 to edit it. The document can be added to the eDocs tab.</td></tr></table>'
MessageStatuses:'<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Message Status</th><th>Received</th></tr></thead><tr><td>Error ISF Add</td><td>23-Feb-09 10:30:00</td></tr><tr><td>Clear ISF Add</td><td>23-Feb-09 11:30:00</td></tr></table>'
LastestMessageStatus:'Clear ISF Add'
CustomsReference:'B3232BD32343'
LastABIStatus:'<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>&nbsp;</td><td>ISF ACCEPTED</td></tr></table>'
ReferenceID:'SCAC234323423'
BillMatchedDate:'25-FEB-09'
ImporterOfRecord:'MR IMPORTER OF RECORD'
ImporterOfRecordCode:'IMP123AU12'
First Accepted Date:'05-JUN-09'
Last Accepted Date:'09-JUN-09'", DispositionCodeList.Descriptions.S4, DispositionCodeList.Descriptions.S1);
			AssertContains(expectedBody, email.Body);
			AssertEquals("Importer Security Filing - TEST TRIGGER 652314569-Clear ISF Add 652314569", email.Subject);

			trigger.P9_Description = "TEST TRIGGER -" + CusISFHeaderWorkflowDescriptor.ReplacementConstants.ReferenceID + "-" + CusISFHeaderWorkflowDescriptor.ReplacementConstants.ImporterOfRecordCode;
			buffer.Clear();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory)).Process(buffer);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			email = Env.OutgoingMailManager.EmailsCreated[0];
			expectedBody = string.Format(@"Description:'TEST TRIGGER -SCAC234323423-IMP123AU12'
JobNumber:'652314569'
CBPResponse:'<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>CBP Response</th></tr></thead><tr><td>------------------APLB------------------<BR /> Block Number (2-3)                  :1<BR /> Processing District Port Code (4-7) :8888<BR /> Entry Filer Code (8-10)             :XJ5<BR /> Application Identifier (11-12)      :SN<BR /> User Data (60-80)                   :~150001<BR /><BR />----------------ISFSF10-----------------<BR /> I S F Submission Type (5-5)             :1<BR /> Shipment Type Code (6-7)                :01<BR /> Action Code (8-8)                       :A<BR /> I S F Importer Number Qualifier (11-13) :EI<BR /> I S F Importer Number (14-28)           :91-013199000<BR /> Mode Of Transportation Code (37-38)     :10<BR /> I S F Transaction Number (39-53)        :XJ5-20089367423<BR /> Bond Holder (58-72)                     :91-013199000<BR /><BR />----------------ISFSF15-----------------<BR /> Code Qualifier (5-6)                 :BM<BR /> Shipment Reference Identifier (7-56) :BM12356897<BR /><BR />----------------ISFSF15-----------------<BR /> Code Qualifier (5-6)                 :BM<BR /> Shipment Reference Identifier (7-56) :BM56846559<BR /><BR />----------------ISFSF20-----------------<BR /> Reference Identifier Qualifier (5-7) :MB<BR /> Reference Identifier (8-57)          :MB56846859<BR /><BR />----------------ISFSF90-----------------<BR /> Message Type Code (5-6)        :02<BR /> Narrative Message Text (10-49) :ISF ACCEPTED<BR /><BR />------------------APLY------------------<BR /> Processing District Port Code (4-7)                       :8888<BR /> Entry Filer Code (8-10)                                   :XJ5<BR /> Application Identifier (11-12)                            :SN<BR /> Number Of Transaction Detail Records In The Block (13-17) :5<BR /><BR /></td></tr></table>'
ReferenceIDs:'<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Of Lading</th><th>Status</th><th>Matched Date</th></tr></thead><tr><td>SCAC234323423</td><td>S4 - ISF bill is NOT on file in AMS 20 days after filing the ISF.</td><td>25-FEB-09</td></tr><tr><td>HB5344685544</td><td>S1 - ISF is matched to an AMS bill of lading.</td><td>24-FEB-09</td></tr></table>'
ValidationMessage:'<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Validation Message(s)</th></tr></thead><tr><td>Error - Customs Reference: The Customs Reference format must be FFF-NNNNNNNNNNN (FFF= Entry Filer Code and NNNNNNNNNNN= numeric sequence number).</td></tr><tr><td>Message Error - Buying Party: Organization: A Buying Party must be specified when Entry Type is &#39;1&#39;.</td></tr><tr><td>Message Error - Consolidator Address: Organization: A Consolidator Address must be specified when Entry Type is &#39;1&#39;.</td></tr><tr><td>Message Error - Container Stuffing Location: Organization: A Container Stuffing Location must be specified when Entry Type is &#39;1&#39;.</td></tr><tr><td>Message Error - Formatted Harmonised Num: Tariff &#39;10108120&#39; is too short; the minimum number of digits required is &#39;10&#39;.</td></tr><tr><td>Message Error - Manufacturer: Manufacturer is required when Entry Type is &#39;1&#39;, &#39;3&#39; or &#39;5&#39;.</td></tr><tr><td>Message Error - Selling Party: Organization: A Selling Party must be specified when Entry Type is &#39;1&#39;.</td></tr><tr><td>Message Error - Ship To Party: Organization: A Ship To Party must be specified when Entry Type is &#39;1&#39;.</td></tr><tr><td>Warning - Customs Reference: The Customs Reference should only be entered in the event that you are trying to modify a job originating from another system.</td></tr><tr><td>Warning - Importer Identification: Unable to verify validity of this EIN, since it does not belong to any organization in the system.</td></tr><tr><td>Warning - Importer: There is no Power of Attorney against this organization. Please place your cursor on the organization and press F3 to edit it. The document can be added to the eDocs tab.</td></tr></table>'
MessageStatuses:'<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Message Status</th><th>Received</th></tr></thead><tr><td>Error ISF Add</td><td>23-Feb-09 10:30:00</td></tr><tr><td>Clear ISF Add</td><td>23-Feb-09 11:30:00</td></tr></table>'
LastestMessageStatus:'Clear ISF Add'
CustomsReference:'B3232BD32343'
LastABIStatus:'<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>&nbsp;</td><td>ISF ACCEPTED</td></tr></table>'
ReferenceID:'SCAC234323423'
BillMatchedDate:'25-FEB-09'
ImporterOfRecord:'MR IMPORTER OF RECORD'
ImporterOfRecordCode:'IMP123AU12'
First Accepted Date:'05-JUN-09'
Last Accepted Date:'09-JUN-09'", DispositionCodeList.Descriptions.S4, DispositionCodeList.Descriptions.S1);
			AssertContains(expectedBody, email.Body);
			AssertEquals("Importer Security Filing - TEST TRIGGER -SCAC234323423-IMP123AU12 652314569", email.Subject);
			AssertNoExceptionThrown(Factory.Save);
		}

		[TestDate(2009, 6, 10)]
		public void TestExportToXML()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_CustomsReference = "B3232BD32343";
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFAdd;
			header.BF_ImporterCode = "IMP32324";
			header.BF_ImporterCodeType = CodeTypeList.Codes.DUNSPlus4;
			header.BF_ConsigneeCode = "CGNEE123";
			header.BF_ConsigneeCodeType = CodeTypeList.Codes.DUNS;
			header.BF_JobReference = "BZZ23432";
			header.BF_RL_NKPlaceOfDelivery = "USLAX";
			header.BF_RL_NKPortOfUnload = "USCHI";
			header.BF_SCAC = "CQTP";
			header.BF_ShipmentType = ShipmentTypeList.Codes.MilitaryAndGovernment;

			CusISFBill bill1 = header.ReferenceDatas.AddNew();
			bill1.BB_BillType = Common.US.ISF.BillTypeList.Codes.HouseBillOfLading;
			bill1.BB_BillNum = "HB234323423";
			bill1.BB_CustomsStatus = DispositionCodeList.Codes.S4;
			CusISFBill bill2 = header.ReferenceDatas.AddNew();
			bill2.BB_BillType = Common.US.ISF.BillTypeList.Codes.MasterBillOfLading;
			bill2.BB_BillNum = "MB6864658445";
			bill2.BB_CustomsStatus = DispositionCodeList.Codes.S3;
			CusISFBill bill3 = header.ReferenceDatas.AddNew();
			bill3.BB_BillType = Common.US.ISF.BillTypeList.Codes.MasterBillOfLading;
			bill3.BB_BillNum = "MB6585466885";
			bill3.BB_CustomsStatus = DispositionCodeList.Codes.S2;
			CusISFBill bill4 = header.ReferenceDatas.AddNew();
			bill4.BB_BillType = Common.US.ISF.BillTypeList.Codes.HouseBillOfLading;
			bill4.BB_BillNum = "HB5344685544";
			bill4.BB_CustomsStatus = DispositionCodeList.Codes.S1;

			CusISFLine line1 = header.Lines.AddNew();
			line1.BL_HarmonisedNum = "10.10.8120";
			line1.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;

			US.Business.MQEDIMessage message1 = (US.Business.MQEDIMessage)header.Messages.AddNew(typeof(US.Business.MQEDIMessage));
			message1.EM_ApplicationCode = Enterprise.Customs.US.Business.EDIMessage.ApplicationCodes.USCustomsImport;
			message1.EM_ReceiveTransmit = Enterprise.Customs.US.Business.EDIMessage.Direction.Receive;
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			message1.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFAdd;
			message1.EM_MessageNum = "~150000";
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2009, 2, 23, 10, 30, 0);

			message1.EM_MessageText =
				"B018888XJ5SN                                               ~150000              " +
				"SF30MF MAINFREIGHT INTERNATIONAL             65007252333                        " +
				"SF401042334668US                                                                " +
				"SF90  404INVALID HTS CODE                                                       " +
				"SF9001   SECURITY FILING REJECTED                                               " +
				"Y  8888XJ5SN0004";

			US.Business.MQEDIMessage message2 = (US.Business.MQEDIMessage)header.Messages.AddNew(typeof(US.Business.MQEDIMessage));
			message2.EM_ApplicationCode = Enterprise.Customs.US.Business.EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = Enterprise.Customs.US.Business.EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			message2.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFAdd;
			message2.EM_MessageNum = "~150001";
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2009, 2, 23, 15, 30, 0);

			message2.EM_MessageText =
				"B018888XJ5SN                                               ~150001              " +
				"SF10101A  EI 91-013199000           10XJ5-20089367423    91-013199000        N  " +
				"SF15BMBM12356897                                                                " +
				"SF15BMBM56846559                                                                " +
				"SF20MB MB56846859                                                               " +
				"SF9002   ISF ACCEPTED                                                           " +
				"Y  8888XJ5SN00005";

			ProcessTask trigger = header.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "TEST TRIGGER " + TriggerActionCommunicationModeSubstitutor.ReplacementConstants.JobNumber + "-" + CusISFHeaderWorkflowDescriptor.ReplacementConstants.LastestMessageStatus;
			trigger.TriggerConditions.TriggerEventCode = Events.MessageStatusChange.Code;
			ProcessTaskNotification notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
			notification.PQ_EmailAddr = "dummy@where.com";
			notification.OverrideEmail = true;
			notification.PQ_EmailText = string.Format(@"Description:'{0}'
JobNumber:'{1}'
CBPResponse:'{2}'
ReferenceIDs:'{3}'
ValidationMessage:'{4}'
MessageStatuses:'{5}'
LastestMessageStatus:'{6}'
CustomsReference:'{7}'
LastABIStatus:'{8}'",
						 TriggerActionCommunicationModeSubstitutor.ReplacementConstants.Description,
						 TriggerActionCommunicationModeSubstitutor.ReplacementConstants.JobNumber,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.CBPResponse,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.ReferenceIDs,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.ValidationMessage,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.MessageStatuses,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.LastestMessageStatus,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.CustomsReference,
						 CusISFHeaderWorkflowDescriptor.ReplacementConstants.LastABIStatus);
			NotificationBuffer buffer = new NotificationBuffer();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			using (Factory.AddDisposableService())
			{
				WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory)).Process(buffer);
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Importer Security Filing - TEST TRIGGER BZZ23432-Clear ISF Add BZZ23432", email.Subject);
				AttachmentDef attachmentDef = null;
				foreach (AttachmentDef attachment in email.Attachments)
				{
					if (attachment.DisplayName == "BZZ23432_Response.xml")
					{
						attachmentDef = attachment;
						break;
					}
				}
				AssertNotNull(attachmentDef);
				string expectedEvent = @"          <Event>
            <Source>CusISFHeader</Source>
            <Code>MSC</Code>
            <CodeDescription>Message: Status Change</CodeDescription>
            <DateTime>2009-06-10T00:00:00+10:00</DateTime>
            <Information>CIO</Information>
            <User>" + GlbStaff.CurrentUser.GS_Code + @"</User>
            <UserName>" + GlbStaff.CurrentUser.GS_FullName + " (" + GlbStaff.CurrentUser.GS_Code + @")</UserName>
            <TriggeredBy>true</TriggeredBy>
            <IsEstimatedDate>false</IsEstimatedDate>
          </Event>
";
				AssertContains(expectedEvent, Encoding.ASCII.GetString(attachmentDef.Data));
				AssertNoExceptionThrown(Factory.Save);
			}
		}

		public void TestSendISFMessage()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFAdd;
			header.BF_ImporterCode = "IMP32324";
			header.BF_ImporterCodeType = CodeTypeList.Codes.DUNSPlus4;
			header.BF_ConsigneeCode = "CGNEE123";
			header.BF_ConsigneeCodeType = CodeTypeList.Codes.DUNS;
			header.BF_JobReference = "BZZ23432";
			header.BF_RL_NKPlaceOfDelivery = "USLAX";
			header.BF_RL_NKPortOfUnload = "USCHI";
			header.BF_SCAC = "CQTP";
			header.BF_ShipmentType = ShipmentTypeList.Codes.MilitaryAndGovernment;

			var bill1 = header.ReferenceDatas.AddNew();
			bill1.BB_BillType = Common.US.ISF.BillTypeList.Codes.HouseBillOfLading;
			bill1.BB_BillNum = "HB234323423";
			bill1.BB_CustomsStatus = DispositionCodeList.Codes.S4;
			var bill2 = header.ReferenceDatas.AddNew();
			bill2.BB_BillType = Common.US.ISF.BillTypeList.Codes.MasterBillOfLading;
			bill2.BB_BillNum = "MB6864658445";
			bill2.BB_CustomsStatus = DispositionCodeList.Codes.S3;
			var bill3 = header.ReferenceDatas.AddNew();
			bill3.BB_BillType = Common.US.ISF.BillTypeList.Codes.MasterBillOfLading;
			bill3.BB_BillNum = "MB6585466885";
			bill3.BB_CustomsStatus = DispositionCodeList.Codes.S2;
			var bill4 = header.ReferenceDatas.AddNew();
			bill4.BB_BillType = Common.US.ISF.BillTypeList.Codes.HouseBillOfLading;
			bill4.BB_BillNum = "HB5344685544";
			bill4.BB_CustomsStatus = DispositionCodeList.Codes.S1;

			var line1 = header.Lines.AddNew();
			line1.BL_HarmonisedNum = "10.10.8120";
			line1.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;

			var trigger = header.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "TEST ISF SEND MESSAGE TRIGGER";
			trigger.TriggerConditions.TriggerEventCode = Events.Authorised.Code;
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendISFMessage;
			header.Logs.AddNew(Events.Authorised);
			Factory.Save();

			var buffer = new NotificationBuffer();
			var workflowProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
			AssertEquals("CusISFHeaderWorkflowDescriptor.GetWorkflowTriggerAction()", "Enterprise.Customs.Business.BatchProcessor.CustomsStmProcessQueueCreatorProcessor", workflowProcessor.GetType().FullName);

			AssertNoExceptionThrown(() =>
			{
				workflowProcessor.Process(buffer);
				Factory.Save();
			});
			AssertEquals(0, header.Messages.Count);
			AssertEquals(MessageStatusList.Codes.ClearISFAdd, header.BF_CustomsStatus);

			var logger = new BatchProcessor.LoggingInformation();
			AssertNoExceptionThrown(() =>
			{
				var autoSendMessageProcessor = new Customs.Business.BatchProcessor.AutoSendCustomsMessagingBatchProcessor(logger);
				autoSendMessageProcessor.ExecuteBatch();
			});
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			var logs = new ZStringBuilder();
			var enumerator = logger.UserLogStrings.GetEnumerator();
			while (enumerator.MoveNext())
			{
				logs.Append(enumerator.Current.Trim());
			}

			AssertContains("Processing: BZZ23432 (HouseBill='HB234323423')", logs.ToStringWithNewLineBetweenAppends());
			AssertContains("ISF message has been sent to customs for Job:BZZ23432", logs.ToStringWithNewLineBetweenAppends());
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest() => new IWorkflowProvider[] { GetISFWithOrganisations() };

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		{
			get
			{
				return new SchemaColumn[]
				{
					JobConsolTransportSchema.JW_RL_NKLoadPort,
					JobConsolTransportSchema.JW_RL_NKDiscPort,
					JobConsolTransportSchema.JW_Vessel,
					JobConsolTransportSchema.JW_VoyageFlight,
					JobConsolTransportSchema.JW_ETD,
					JobConsolTransportSchema.JW_ETA,
					JobConsolTransportSchema.JW_ATD,
					JobConsolTransportSchema.JW_ATA,
					CusISFHeaderSchema.BF_CustomsStatus,
					CusISFBillSchema.BB_CustomsStatus
				};
			}
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.Email |
					MessageRecipientPartyType.BillToParty |
					MessageRecipientPartyType.Consignee |
					MessageRecipientPartyType.OrgProxy;
			}
		}

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				List<CodeDescriptionPair> list = new List<CodeDescriptionPair>();
				list.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendXML, "Send XML Document"));
				list.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendISFMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendISFMessage));
				return list.ToArray();
			}
		}

		protected override string TestingCountry => Core.Constants.CountryCodes.UnitedStates;

		CusISFHeader GetISFWithOrganisations()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;

			JobHeader.Loader jobLoader = new JobHeader.Loader(header);
			JobHeader job = jobLoader.TryCreate();
			job.JH_OA_LocalChargesAddr = BillToPartyOrg.MainAddress.PK;

			header.BF_OH_Importer = ConsigneeOrg.PK;
			JobDocAddress manufacturerAddress = header.ManufacturerAddresses.AddNew();
			manufacturerAddress.E2_OA_Address = ConsignorOrg.MainAddress.PK;

			header.MainShipToParty.E2_OA_Address = DeliveryCartageOrg.MainAddress.PK;

			return header;
		}

		sealed class FactorySaveAlerterForTest : Disposable, ITransactionParticipantListener
		{
			public FactorySaveAlerterForTest(string context)
			{
				BusinessObjectFactory.RegisterListener(this);
				this.context = context;
			}

			readonly string context;

			void ITransactionParticipantListener.FactorySaveBeginning(ITransactionParticipant[] factories)
			{
				ErrorReporter.ReportOnce(FormattableString.Invariant($"Save in [{context}]"), FormattableString.Invariant($@"It is not valid to call Factory.Save in {context}.
It is never valid or safe to call Factory.Save within a LogSubscriber of the LWK service task. (And it never has been)
There reason for this is that we cannot guarantee that the same StmJobQueue will not be processed multiple times if something goes wrong after this save but before the NewsTransmitter calls save.
In order to fix this issue you should be deleting the instance of the save being reported here."));
			}

			void ITransactionParticipantListener.FactorySaveCompleted(ITransactionParticipant[] factories, bool successful)
			{
				// Don't care.
			}

			protected override void Dispose(bool isDisposing) => BusinessObjectFactory.UnRegisterListener(this);
		}

		[Serializable]
		sealed class LogSubscriberForTest : LogSubscriber
		{
			public override string Name => ProcessTask.WorkflowEventTriggerJobQueueName;

			public override string[] EventTypes => new string[] { Events.WorkflowTriggerEvent.Code };

			public override string[] TableNames => new[] { ProcessTasksSchema.Constants.TableName, ProcessJobTriggerLinkSchema.Constants.TableName };

			public void Process(CusISFHeader[] headers)
			{
				foreach (var header in headers)
				{
					var trigger = header.WorkflowItems.Triggers.AddNew();
					trigger.P9_Description = "TEST ISF SEND MESSAGE TRIGGER";
					trigger.TriggerConditions.TriggerEventCode = Events.Authorised.Code;
					var notification = trigger.ProcessTaskNotifications.AddNew();
					notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendISFMessage;
					header.Logs.AddNew(Events.Authorised);

					var buffer = new NotificationBuffer();
					new CusISFHeaderWorkflowDescriptor().GetWorkflowTriggerAction(notification, new QueuedLogForTesting(header.Factory)).Process(buffer);
				}
			}

			protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
			{
				throw new NotImplementedException();
			}

			public override bool EnableFactorySaveAlerterInTesting => true;
		}
	}
}
