using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Helper = Enterprise.Workflow.Business.Test.WorkflowEdiMessageTestHelper;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(EDIMessageContentFilter))]
	class EDIMessageContentFilterTest : EnterpriseBusinessObjectTestCase
	{
		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://");
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return nameof(EDIMessageContentFilter.Description);
				yield return nameof(EDIMessageContentFilter.UniversalEvent);
				yield return nameof(EDIMessageContentFilter.UniversalShipment);
				yield return nameof(EDIMessageContentFilter.UniversalTransaction);
				yield return nameof(EDIMessageContentFilter.Config);
			}
		}

		OrgHeader MakeOrgWithCommunicationModes(string module, string fileFormat, EDIMessagePurpose purpose)
		{
			var orgToUse = Factory.NewWithValidTestData<OrgHeader>();
			AddCommunicationModeToOrg(orgToUse, module, fileFormat, purpose);
			return orgToUse;
		}

		void MakeOrgProxyCommunicationMode(string module)
		{
			var communicationsMode = GlbCompany.GetCurrentCompany(Factory).OrgProxy.EDICommunicationsModes.AddNew();
			communicationsMode.EK_Module = module;
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_Destination = "VASILIY";
		}

		void AddCommunicationModeToOrg(OrgHeader orgToUse, string module, string fileFormat, EDIMessagePurpose purpose)
		{
			var communicationsMode = orgToUse.EDICommunicationsModes.AddNew();
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_FileFormat = fileFormat;
			communicationsMode.EK_Destination = "SOMEONE";
			communicationsMode.EK_Module = module;
			if (purpose != null)
			{
				communicationsMode.EK_MessagePurpose = purpose.EMP_Code;
			}
		}

		Forwarding.IForwardingShipment MakeTriggeredShipment(EDIMessagePurpose purpose, OrgHeader orgToUse, string triggerType)
		{
			var shipment = MakeShipment(purpose, orgToUse, triggerType);
			((IStmALogProvider)shipment).Logs.AddNew(AutoEvents.CustomisableEvent00);
			return shipment;
		}

		Forwarding.IForwardingConsol MakeTriggeredConsol(EDIMessagePurpose purpose, string triggerType)
		{
			var job = MakeConsol(purpose, triggerType);
			((IStmALogProvider)job).Logs.AddNew(AutoEvents.CustomisableEvent00);
			return job;
		}

		Enterprise.Integration.Customs.IBaseJobDeclaration MakeTriggeredDeclaration(EDIMessagePurpose purpose, string triggerType)
		{
			var job = MakeDeclaration(purpose, triggerType);
			((IStmALogProvider)job).Logs.AddNew(AutoEvents.CustomisableEvent00);
			return job;
		}

		void MakeTriggeredShipmentWithEDoc(EDIMessagePurpose purpose, OrgHeader orgToUse, string triggerType)
		{
			var shipment = MakeShipment(purpose, orgToUse, triggerType);

			var docSource = Factory.NewWithValidTestData<RefDocSource>();
			docSource.RDS_Code = "AZA";
			docSource.RDS_Desc = "Aaaaa!";

			var eDoc = ((IDocManagerSupport)shipment).DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "TestCartageAdviceFile", "CAD", false, Env.CurrentCompanyPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK, documentSource: "AZA");
			var reference = "CAD|" + eDoc.UniqueKey.ToString();
			((IDocManagerSupport)shipment).DocManagerInfo.MasterFactory.Save();

			((IStmALogProvider)shipment).Logs.AddNew(AutoEvents.CustomisableEvent00, reference);
		}

		IeDoc AddEDoc(IDocManagerSupport docManager, string docType, string fileName, string contents)
		{
			var eDoc = docManager.DocManagerInfo.AddFileOrDocument(Encoding.UTF8.GetBytes(contents), fileName, docType, false, Env.CurrentCompanyPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK, documentSource: "AZA");
			docManager.DocManagerInfo.MasterFactory.Save();
			return eDoc;
		}

		Forwarding.IForwardingShipment MakeShipment(EDIMessagePurpose purpose, OrgHeader orgToUse, string triggerType)
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = orgToUse.PK;

			((BusinessObject)shipment).FillWithValidTestData();
			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.P9_Description = "Hide the shed-mole";
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = triggerType;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignor;
			notification.PQ_MessagePurpose = purpose.EMP_Code;
			return shipment;
		}

		Forwarding.IForwardingConsol MakeConsol(EDIMessagePurpose purpose, string triggerType)
		{
			var job = Factory.New<Forwarding.IForwardingConsol>();

			((BusinessObject)job).FillWithValidTestData();
			var trigger = ((IWorkflowProvider)job).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.P9_Description = "Hide the shed-mole";
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = triggerType;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			notification.PQ_MessagePurpose = purpose.EMP_Code;
			return job;
		}

		Enterprise.Integration.Customs.IBaseJobDeclaration MakeDeclaration(EDIMessagePurpose purpose, string triggerType)
		{
			var job = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			((BusinessObject)job).FillWithValidTestData();
			var trigger = ((IWorkflowProvider)job).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.P9_Description = "Hide the shed-mole";
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = triggerType;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			notification.PQ_MessagePurpose = purpose.EMP_Code;
			return job;
		}

		IARInvoice MakeTriggeredInvoice(EDIMessagePurpose purpose)
		{
			var invoice1 = Factory.New(ObjectFactory.GetType<IARInvoice>());
			invoice1.FillWithValidTestData();
			invoice1[AccTransactionHeaderSchema.AH_TransactionNum] = "00001000";

			var invoice2 = Factory.New(ObjectFactory.GetType<IARInvoice>());
			invoice2.FillWithValidTestData();
			invoice2[AccTransactionHeaderSchema.AH_TransactionNum] = "00001001";

			var invoice3 = Factory.New(ObjectFactory.GetType<IARInvoice>());
			invoice3.FillWithValidTestData();
			invoice3[AccTransactionHeaderSchema.AH_TransactionNum] = "00001002";

			var trigger = ((IWorkflowProvider)invoice1).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.P9_Description = "Hide the shed-mole";
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			notification.PQ_MessagePurpose = purpose.EMP_Code;

			((IStmALogProvider)invoice1).Logs.AddNew(AutoEvents.CustomisableEvent00);
			return (IARInvoice)invoice1;
		}

		void DoAccountingSetupBoilerplate()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			AccGLHeader aRSuspenseControlAccount = testObjectCreator.CreateARSuspenseControlAccount();
			AccGLHeader aPSuspenseControlAccount = testObjectCreator.CreateAPSuspenseControlAccount();
			AccGLHeader jobRevenueJournalControlAccount = testObjectCreator.CreateJobRevenueJournalControlAccount();
			AccGLHeader cFXAccount = testObjectCreator.CreateCFXAccount();
			AccGLHeader pendingInputTaxAccount = testObjectCreator.CreateInputTaxReceivablePendingAccount();
			AccGLHeader pendingOutputTaxAccount = testObjectCreator.CreateOutputTaxPayablePendingAccount();
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aRSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobRevenueJournalControlAccount.PK.ToGuid());
			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, department.PK.ToGuid(), cFXAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingInputTaxAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingOutputTaxAccount.PK.ToGuid());
		}
		#endregion

		public void TestSupportsDocuments()
		{
			var filter = Factory.New<EDIMessageContentFilter>();

			AssertEquals(false, EDIMessageContentFilterSpec.SupportsDocuments(filter.UniversalEvent.Schema));
			AssertEquals(true, EDIMessageContentFilterSpec.SupportsDocuments(filter.UniversalShipment.Schema));
			AssertEquals(false, EDIMessageContentFilterSpec.SupportsDocuments(filter.UniversalTransaction.Schema));
		}

		public void TestIncludeMessageContent_Shipment()
		{
			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Include);
			Helper.AddLine(filter.UniversalShipment, "RecipientRoleCollection");
			var purpose = Helper.MakePurpose(Factory, "MOL", "Dont let them see", filter);
			var orgToUse = MakeOrgWithCommunicationModes("SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, purpose);
			MakeTriggeredShipment(purpose, orgToUse, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);

			Factory.Save();

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertContains("RecipientRoleCollection", messages[0].EM_MessageText);
			AssertNotContains("DataSourceCollection", messages[0].EM_MessageText);
			AssertNotContains("DateCollection", messages[0].EM_MessageText);
		}

		public void TestIncludeMessageContent_Shipment2()
		{
			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Include);
			Helper.AddLine(filter.UniversalShipment, "DataSourceCollection");
			var purpose = Helper.MakePurpose(Factory, "MOL", "Dont let them see", filter);
			var orgToUse = MakeOrgWithCommunicationModes("SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, purpose);
			MakeTriggeredShipment(purpose, orgToUse, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);

			Factory.Save();

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertContains("DataSourceCollection", messages[0].EM_MessageText);
			AssertNotContains("RecipientRoleCollection", messages[0].EM_MessageText);
			AssertNotContains("DateCollection", messages[0].EM_MessageText);
		}

		public void TestExcludeMessageContent_Shipment()
		{
			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Exclude);
			Helper.AddLine(filter.UniversalShipment, "RecipientRoleCollection");
			var purpose = Helper.MakePurpose(Factory, "MOL", "Dont let them see", filter);
			var orgToUse = MakeOrgWithCommunicationModes("SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, purpose);
			MakeTriggeredShipment(purpose, orgToUse, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);
			Factory.Save();

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertNotContains("RecipientRoleCollection", messages[0].EM_MessageText);
			AssertContains("DataSourceCollection", messages[0].EM_MessageText);
			AssertContains("DateCollection", messages[0].EM_MessageText);
		}

		public void TestExcludeMessageContent_Shipment_WhiteSpaceAtEndFilter()
		{
			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Exclude);
			Helper.AddLine(filter.UniversalShipment, "RecipientRoleCollection ");
			var purpose = Helper.MakePurpose(Factory, "MOL", "Dont let them see", filter);
			var orgToUse = MakeOrgWithCommunicationModes("SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, purpose);
			MakeTriggeredShipment(purpose, orgToUse, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);
			Factory.Save();

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertNotContains("RecipientRoleCollection", messages[0].EM_MessageText);
			AssertContains("DataSourceCollection", messages[0].EM_MessageText);
			AssertContains("DateCollection", messages[0].EM_MessageText);
		}

		public void TestExcludeMessageContent_Shipment2()
		{
			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Exclude);
			Helper.AddLine(filter.UniversalShipment, "DataSourceCollection");
			var purpose = Helper.MakePurpose(Factory, "MOL", "Dont let them see", filter);
			var orgToUse = MakeOrgWithCommunicationModes("SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, purpose);
			MakeTriggeredShipment(purpose, orgToUse, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);
			Factory.Save();

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertNotContains("DataSourceCollection", messages[0].EM_MessageText);
			AssertContains("RecipientRoleCollection", messages[0].EM_MessageText);
			AssertContains("DateCollection", messages[0].EM_MessageText);
		}

		public void TestExcludeMessageContent_AllShipmentTags()
		{
			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Exclude);
			var line = Helper.AddLine(filter.UniversalShipment, "DataSourceCollection");

			var purpose = Helper.MakePurpose(Factory, "MOL", "Dont let them see", filter);
			var orgToUse = MakeOrgWithCommunicationModes("SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, purpose);

			var universalShipmentCollectionElements = line.Lookups.SchemaElements.GetAllCodes();

			var maximumProcessableElements = 100;

			var processIterations = (universalShipmentCollectionElements.Length / maximumProcessableElements) + 1;

			for (var i = 0; i < processIterations; i++)
			{
				var chunkedCollection = universalShipmentCollectionElements.Skip(i * maximumProcessableElements).Take((i + 1) * maximumProcessableElements).ToList();

				var shipment = MakeShipment(purpose, orgToUse, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);
				Factory.Save();

				foreach (var element in chunkedCollection)
				{
					line.SchemaElement = element;
					((IStmALogProvider)shipment).Logs.AddNew(AutoEvents.CustomisableEvent00);
					Factory.Save();
					ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();
					var messages = Factory.Load<EDIMessage>(new ZQuery());
					AssertEquals(1, messages.Length);
					AssertNotContains(element, messages[0].EM_MessageText);
					messages[0].Delete();
				}
			}

			ErrorReporter.Instance.Clear();
		}

		public void TestIncludeMessageContent_Event()
		{
			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Include);
			Helper.AddLine(filter.UniversalEvent, "ContextCollection");
			var purpose = Helper.MakePurpose(Factory, "MOL", "Dont let them see", filter);
			var orgToUse = MakeOrgWithCommunicationModes("SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, purpose);
			MakeTriggeredShipment(purpose, orgToUse, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent);

			Factory.Save();

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertContains("ContextCollection", messages[0].EM_MessageText);
			AssertNotContains("DataSourceCollection", messages[0].EM_MessageText);
		}

		public void TestIncludeMessageContent_EventWithEDoc()
		{
			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Include);
			Helper.AddLine(filter.UniversalEvent, "AttachedDocumentCollection");
			var purpose = Helper.MakePurpose(Factory, "MOL", "Dont let them see", filter);
			var orgToUse = MakeOrgWithCommunicationModes("SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, purpose);
			MakeTriggeredShipmentWithEDoc(purpose, orgToUse, WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc);

			Factory.Save();

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertContains("AttachedDocumentCollection", messages[0].EM_MessageText);
			AssertNotContains("DataSourceCollection", messages[0].EM_MessageText);
		}

		[TestDate(2020, 10, 10)]
		public void TestMessageContent_ShipmentWithEDocs()
		{
			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Exclude);

			Helper.AddDocument(filter.UniversalShipment, Core.Constants.RefDocTypes.CartageAdvice);
			Helper.AddDocument(filter.UniversalShipment, Core.Constants.RefDocTypes.AuthorityToDeal);
			Helper.AddDocument(filter.UniversalShipment, Core.Constants.RefDocTypes.BookingConfirmation);

			var purpose = Helper.MakePurpose(Factory, "MOL", "Dont let them see", filter);
			var orgToUse = MakeOrgWithCommunicationModes("SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, purpose);
			var shipment = MakeTriggeredShipment(purpose, orgToUse, WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML);

			var eDoc1 = AddEDoc((IDocManagerSupport)shipment, Core.Constants.RefDocTypes.CartageAdvice, "CartageDocument", "Blah blah blah");
			var eDoc2 = AddEDoc((IDocManagerSupport)shipment, Core.Constants.RefDocTypes.CartageAdvice, "CartageDocument", "Blah blah blah");
			var eDoc3 = AddEDoc((IDocManagerSupport)shipment, Core.Constants.RefDocTypes.AgentsInvoice, "AgentsInvoice", "I should be ignored");
			var eDoc4 = AddEDoc((IDocManagerSupport)shipment, Core.Constants.RefDocTypes.BookingConfirmation, "BookingConfirmation", "efkhjksadgfjkgsdhkafgjasdgfsdagjkwghfsdafgjksdagfjgsdhagfjkhsdjkfgisadghfj");

			Factory.Save();

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertContains("eDocs 1, 2 and 4 should be included, eDoc 3 should be ignored", GetExpectedAttachedDocumentXml(eDoc1, eDoc2, eDoc4), messages[0].EM_MessageText);
			AssertNotContains("AgentsInvoice doc types should not be included", "AgentsInvoice", messages[0].EM_MessageText);
		}

		[TestDate(2020, 10, 10)]
		public void TestMessageContent_ConsolWithEDocs()
		{
			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Exclude);

			Helper.AddDocument(filter.UniversalShipment, Core.Constants.RefDocTypes.CartageAdvice);
			Helper.AddDocument(filter.UniversalShipment, Core.Constants.RefDocTypes.AuthorityToDeal);
			Helper.AddDocument(filter.UniversalShipment, Core.Constants.RefDocTypes.BookingConfirmation);

			var purpose = Helper.MakePurpose(Factory, "MOL", "Dont let them see", filter);
			MakeOrgProxyCommunicationMode("CON");
			var job = MakeTriggeredConsol(purpose, WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML);

			var eDoc1 = AddEDoc((IDocManagerSupport)job, Core.Constants.RefDocTypes.CartageAdvice, "CartageDocument", "Blah blah blah");
			var eDoc2 = AddEDoc((IDocManagerSupport)job, Core.Constants.RefDocTypes.CartageAdvice, "CartageDocument", "Blah blah blah");
			var eDoc3 = AddEDoc((IDocManagerSupport)job, Core.Constants.RefDocTypes.AgentsInvoice, "AgentsInvoice", "I should be ignored");
			var eDoc4 = AddEDoc((IDocManagerSupport)job, Core.Constants.RefDocTypes.BookingConfirmation, "BookingConfirmation", "efkhjksadgfjkgsdhkafgjasdgfsdagjkwghfsdafgjksdagfjgsdhagfjkhsdjkfgisadghfj");

			Factory.Save();

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertContains("eDocs 1, 2 and 4 should be included, eDoc 3 should be ignored", GetExpectedAttachedDocumentXml(eDoc1, eDoc2, eDoc4), messages[0].EM_MessageText);
			AssertNotContains("AgentsInvoice doc types should not be included", "AgentsInvoice", messages[0].EM_MessageText);
		}

		[TestDate(2020, 10, 10)]
		public void TestMessageContent_DeclarationWithEDocs()
		{
			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Exclude);

			Helper.AddDocument(filter.UniversalShipment, Core.Constants.RefDocTypes.CartageAdvice);
			Helper.AddDocument(filter.UniversalShipment, Core.Constants.RefDocTypes.AuthorityToDeal);
			Helper.AddDocument(filter.UniversalShipment, Core.Constants.RefDocTypes.BookingConfirmation);

			var purpose = Helper.MakePurpose(Factory, "MOL", "Dont let them see", filter);
			MakeOrgProxyCommunicationMode("BRK");
			var job = MakeTriggeredDeclaration(purpose, WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML);

			var eDoc1 = AddEDoc((IDocManagerSupport)job, Core.Constants.RefDocTypes.CartageAdvice, "CartageDocument", "Blah blah blah");
			var eDoc2 = AddEDoc((IDocManagerSupport)job, Core.Constants.RefDocTypes.CartageAdvice, "CartageDocument", "Blah blah blah");
			var eDoc3 = AddEDoc((IDocManagerSupport)job, Core.Constants.RefDocTypes.AgentsInvoice, "AgentsInvoice", "I should be ignored");
			var eDoc4 = AddEDoc((IDocManagerSupport)job, Core.Constants.RefDocTypes.BookingConfirmation, "BookingConfirmation", "efkhjksadgfjkgsdhkafgjasdgfsdagjkwghfsdafgjksdagfjgsdhagfjkhsdjkfgisadghfj");

			Factory.Save();

			var logs = ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertContains("eDocs 1, 2 and 4 should be included, eDoc 3 should be ignored", GetExpectedAttachedDocumentXml(eDoc1, eDoc2, eDoc4), messages[0].EM_MessageText);
			AssertNotContains("AgentsInvoice doc types should not be included", "AgentsInvoice", messages[0].EM_MessageText);
		}

		string GetExpectedAttachedDocumentXml(IeDoc eDoc1, IeDoc eDoc2, IeDoc eDoc3)
		{
			return $@"<AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>CartageDocument</FileName>
        <ImageData>QmxhaCBibGFoIGJsYWg=</ImageData>
        <Type>
          <Code>CAD</Code>
          <Description>Cartage Advice</Description>
        </Type>
        <DocumentID>{eDoc1.UniqueKey}</DocumentID>
        <FileSizeInBytes>14</FileSizeInBytes>
        <IsPublished>true</IsPublished>
        <SaveDateUTC>2020-10-10T00:00:00</SaveDateUTC>
        <SavedBy>
          <Code>E</Code>
          <Name>CargoWise Support</Name>
        </SavedBy>
        <Source>
          <Code>{eDoc1.DocSource}</Code>
          <Description>{eDoc1.DocSourceDescription}</Description>
        </Source>
        <VisibleBranchCode>{eDoc1.VisibleBranchCode}</VisibleBranchCode>
        <VisibleCompanyCode>{eDoc1.VisibleCompanyCode}</VisibleCompanyCode>
        <VisibleDepartmentCode>{eDoc1.VisibleDepartmentCode}</VisibleDepartmentCode>
      </AttachedDocument>
      <AttachedDocument>
        <FileName>{eDoc2.FileName}</FileName>
        <ImageData>QmxhaCBibGFoIGJsYWg=</ImageData>
        <Type>
          <Code>CAD</Code>
          <Description>Cartage Advice</Description>
        </Type>
        <DocumentID>{eDoc2.UniqueKey}</DocumentID>
        <FileSizeInBytes>14</FileSizeInBytes>
        <IsPublished>true</IsPublished>
        <SaveDateUTC>2020-10-10T00:00:00</SaveDateUTC>
        <SavedBy>
          <Code>E</Code>
          <Name>CargoWise Support</Name>
        </SavedBy>
        <Source>
          <Code>{eDoc2.DocSource}</Code>
          <Description>{eDoc2.DocSourceDescription}</Description>
        </Source>
        <VisibleBranchCode>{eDoc2.VisibleBranchCode}</VisibleBranchCode>
        <VisibleCompanyCode>{eDoc2.VisibleCompanyCode}</VisibleCompanyCode>
        <VisibleDepartmentCode>{eDoc2.VisibleDepartmentCode}</VisibleDepartmentCode>
      </AttachedDocument>
      <AttachedDocument>
        <FileName>{eDoc3.FileName}</FileName>
        <ImageData>ZWZraGprc2FkZ2Zqa2dzZGhrYWZnamFzZGdmc2RhZ2prd2doZnNkYWZnamtzZGFnZmpnc2RoYWdmamtoc2Rqa2ZnaXNhZGdoZmo=</ImageData>
        <Type>
          <Code>{eDoc3.DocType}</Code>
          <Description>{eDoc3.DocType_List.GetDescriptionFromCode(eDoc3.DocType)}</Description>
        </Type>
        <DocumentID>{eDoc3.UniqueKey}</DocumentID>
        <FileSizeInBytes>74</FileSizeInBytes>
        <IsPublished>true</IsPublished>
        <SaveDateUTC>2020-10-10T00:00:00</SaveDateUTC>
        <SavedBy>
          <Code>E</Code>
          <Name>CargoWise Support</Name>
        </SavedBy>
        <Source>
          <Code>{eDoc3.DocSource}</Code>
          <Description>{eDoc3.DocSourceDescription}</Description>
        </Source>
        <VisibleBranchCode>{eDoc3.VisibleBranchCode}</VisibleBranchCode>
        <VisibleCompanyCode>{eDoc3.VisibleCompanyCode}</VisibleCompanyCode>
        <VisibleDepartmentCode>{eDoc3.VisibleDepartmentCode}</VisibleDepartmentCode>
      </AttachedDocument>
    </AttachedDocumentCollection>";
		}

		public void TestMessageContent_ShipmentWithEDoc_AttachedDocumentCollectionNotIncluded()
		{
			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Include);
			Helper.AddLine(filter.UniversalShipment, "ContextCollection");
			Helper.AddDocument(filter.UniversalShipment, Core.Constants.RefDocTypes.CartageAdvice);

			var purpose = Helper.MakePurpose(Factory, "MOL", "Dont let them see", filter);
			var orgToUse = MakeOrgWithCommunicationModes("SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, purpose);
			var shipment = MakeTriggeredShipment(purpose, orgToUse, WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML);

			AddEDoc((IDocManagerSupport)shipment, Core.Constants.RefDocTypes.CartageAdvice, "CartageDocument", "Blah blah blah");

			Factory.Save();

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertNotContains("This EDIMessageContentFilter is set up to only include ContextCollection", "AttachedDocumentCollection", messages[0].EM_MessageText);
		}

		public void TestMessageContent_ShipmentWithEDoc_AttachedDocumentCollectionExcluded()
		{
			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Exclude);
			Helper.AddLine(filter.UniversalShipment, "AttachedDocumentCollection");
			Helper.AddDocument(filter.UniversalShipment, Core.Constants.RefDocTypes.CartageAdvice);

			var purpose = Helper.MakePurpose(Factory, "MOL", "Dont let them see", filter);
			var orgToUse = MakeOrgWithCommunicationModes("SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, purpose);
			var shipment = MakeTriggeredShipment(purpose, orgToUse, WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML);

			AddEDoc((IDocManagerSupport)shipment, Core.Constants.RefDocTypes.CartageAdvice, "CartageDocument", "Blah blah blah");

			Factory.Save();

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertNotContains("This EDIMessageContentFilter is set up to exclude AttachedDocumentCollection", "AttachedDocumentCollection", messages[0].EM_MessageText);
		}

		public void TestMessageContent_ShipmentWithEDocs_MultipleCommunicationModes()
		{
			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Exclude);
			Helper.AddDocument(filter.UniversalShipment, Core.Constants.RefDocTypes.CartageAdvice);
			var purpose = Helper.MakePurpose(Factory, "MOL", "Purpose1", filter);

			var filter2 = Helper.MakeFilter(Factory, "Filter2", EDIMessageContentFilterTypes.Codes.Exclude);
			var purpose2 = Helper.MakePurpose(Factory, "ABC", "Purpose2", filter2);

			var orgToUse = MakeOrgWithCommunicationModes("SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, purpose);

			var shipment = MakeTriggeredShipment(purpose, orgToUse, WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML);

			var consigneeOrg = MakeOrgWithCommunicationModes("SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, purpose2);
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consigneeOrg.PK;
			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.P9_Description = "Hide the shed-mole";
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignee;
			notification.PQ_MessagePurpose = purpose2.EMP_Code;

			AddEDoc((IDocManagerSupport)shipment, Core.Constants.RefDocTypes.CartageAdvice, "CartageDocument", "Blah blah blah");

			Factory.Save();

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());

			AssertEquals(2, messages.Length);
			AssertEquals("Only one EDI Message Purpose was configured to send cartage documents", 1, messages.Where(m => m.EM_MessageText.Contains("CartageDocument")).Count());
		}

		public void TestExcludeMessageContent_Event()
		{
			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Exclude);
			Helper.AddLine(filter.UniversalEvent, "ContextCollection");
			var purpose = Helper.MakePurpose(Factory, "MOL", "Dont let them see", filter);
			var orgToUse = MakeOrgWithCommunicationModes("SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, purpose);
			MakeTriggeredShipment(purpose, orgToUse, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent);
			Factory.Save();

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertNotContains("ContextCollection", messages[0].EM_MessageText);
			AssertContains("DataSourceCollection", messages[0].EM_MessageText);
		}

		public void TestIncludeMessageContent_XUT()
		{
			DoAccountingSetupBoilerplate();

			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Include);
			Helper.AddLine(filter.UniversalTransaction, "RecipientRoleCollection");
			var purpose = Helper.MakePurpose(Factory, "MOL", "Dont let them see", filter);
			var org = MakeOrgWithCommunicationModes("RNV", EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction, purpose);
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			company.GC_OH_OrgProxy = org.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var invoice = MakeTriggeredInvoice(purpose);

				GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;

				Factory.Save();
				ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

				var messages = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals(1, messages.Length);
				AssertContains("RecipientRoleCollection", messages[0].EM_MessageText);
				AssertNotContains("DataSourceCollection", messages[0].EM_MessageText);
			}
		}

		public void TestExcludeMessageContext_XUT()
		{
			DoAccountingSetupBoilerplate();

			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Exclude);
			Helper.AddLine(filter.UniversalTransaction, "RecipientRoleCollection");
			var purpose = Helper.MakePurpose(Factory, "MOL", "Dont let them see", filter);
			var org = MakeOrgWithCommunicationModes("RNV", EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction, purpose);
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			company.GC_OH_OrgProxy = org.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var invoice = MakeTriggeredInvoice(purpose);

				GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;

				Factory.Save();
				ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

				var messages = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals(1, messages.Length);
				AssertNotContains("RecipientRoleCollection", messages[0].EM_MessageText);
				AssertContains("DataSourceCollection", messages[0].EM_MessageText);
			}
		}

		public void TestExclude_DataContextContainsHVLVConsignment_WhenSchemaElementsIsSubShipmentCollection()
		{
			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Exclude);
			Helper.AddLine(filter.UniversalShipment, "SubShipmentCollection", "HVLVConsignment");
			var purpose = Helper.MakePurpose(Factory, "MOL", "Exclude SubShipmentCollection", filter);
			var orgToUse = MakeOrgWithCommunicationModes("SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, purpose);
			var shipment = MakeTriggeredShipment(purpose, orgToUse, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);
			CreateConsignmentAndItemsForHVLShipment(shipment);
			Factory.Save();

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("Expect one message to be created", 1, messages.Length);
			AssertEquals("The SubShipmentCollection element should exclude the HVLVConsignment", false, DoesXmlContainElementWithSearchString(messages[0].EM_MessageText, "SubShipmentCollection", "HVLVConsignment"));
		}

		public void TestInclude_DataContextContainsHVLVConsignment_WhenSchemaElementsIsSubShipmentCollection()
		{
			var filter = Helper.MakeFilter(Factory, "The shed", EDIMessageContentFilterTypes.Codes.Include);
			Helper.AddLine(filter.UniversalShipment, "SubShipmentCollection", "HVLVConsignment");
			var purpose = Helper.MakePurpose(Factory, "MOL", "Include SubShipmentCollection", filter);
			var orgToUse = MakeOrgWithCommunicationModes("SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, purpose);
			var shipment = MakeTriggeredShipment(purpose, orgToUse, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);
			CreateConsignmentAndItemsForHVLShipment(shipment);
			Factory.Save();

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("Expect one message to be created", 1, messages.Length);
			AssertContains("SubShipmentCollection", messages[0].EM_MessageText);
		}

		void CreateConsignmentAndItemsForHVLShipment(Forwarding.IForwardingShipment shipment)
		{
			shipment.JS_ShipmentType = "HVL";
			var consignmentHeader = Factory.LoadTop1(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(HVLVConsignmentHeaderSchema.Constants.Prefix), new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipment.PK));
			var consignment = Factory.New(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(HVLVConsignmentSchema.Constants.Prefix));
			consignment[HVLVConsignmentSchema.HVC_HCH_Header] = consignmentHeader.PK;
			var item = Factory.New(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(HVLVItemSchema.Constants.Prefix));
			item[HVLVItemSchema.HVI_HVC_Consignment] = consignment.PK;
			var conShipmentLink = Factory.New(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(JobConShipLinkSchema.Constants.Prefix));
			var consol = Factory.New<Forwarding.IForwardingConsol>();
			((BusinessObject)consol).FillWithValidTestData();
			conShipmentLink[JobConShipLinkSchema.JN_JK] = consol.PK;
			conShipmentLink[JobConShipLinkSchema.JN_JS] = shipment.PK;
		}

		bool DoesXmlContainElementWithSearchString(string xml, string elementName, string search)
		{
			return XDocument.Parse(xml).Descendants().Any(node => node.Name.LocalName.Equals(elementName) && node.Value.Contains(search));
		}

		public void TestOldXmlStillValidAfterAddingDocuments()
		{
			var pk = InsertBadFilterContent(@"
				<ECF_FilterContent>
					<UniversalTransaction>
						<FilterType />
						<Lines />
					</UniversalTransaction>
				</ECF_FilterContent>");

			var filter = Factory.Load<EDIMessageContentFilter>(pk);

			AssertEquals(0, filter.UniversalTransaction.Lines.Count);
			AssertEquals(0, filter.UniversalTransaction.Documents.Count);

			filter.HasChanges = true;
			Factory.Save();

			var reloadedFilter = Factory.Load<EDIMessageContentFilter>(pk);
			AssertGoodFilterContent(reloadedFilter.ECF_FilterContent);

			AssertEquals(EDIMessageContentFilterTypes.Codes.Exclude, filter.UniversalTransaction.FilterType);
			AssertEquals(EDIMessageContentFilterTypes.Codes.Exclude, reloadedFilter.UniversalTransaction.FilterType);
		}

		public void TestAppliesDefaultFilterTypeForNullSpec()
		{
			var pk = InsertBadFilterContent("<ECF_FilterContent />");

			var filter = Factory.Load<EDIMessageContentFilter>(pk);
			filter.HasChanges = true;
			Factory.Save();

			var reloadedFilter = Factory.Load<EDIMessageContentFilter>(pk);
			AssertGoodFilterContent(reloadedFilter.ECF_FilterContent);

			AssertEquals(EDIMessageContentFilterTypes.Codes.Exclude, filter.UniversalTransaction.FilterType);
			AssertEquals(EDIMessageContentFilterTypes.Codes.Exclude, reloadedFilter.UniversalTransaction.FilterType);
		}

		public void TestAppliesDefaultFilterTypeForInvalidSpec()
		{
			var pk = InsertBadFilterContent(@"
				<ECF_FilterContent>
					<UniversalTransaction>
						<FilterType />
						<Lines />
					</UniversalTransaction>
				</ECF_FilterContent>");

			var filter = Factory.Load<EDIMessageContentFilter>(pk);
			filter.HasChanges = true;
			Factory.Save();

			var reloadedFilter = Factory.Load<EDIMessageContentFilter>(pk);
			AssertGoodFilterContent(reloadedFilter.ECF_FilterContent);

			AssertEquals(EDIMessageContentFilterTypes.Codes.Exclude, filter.UniversalTransaction.FilterType);
			AssertEquals(EDIMessageContentFilterTypes.Codes.Exclude, reloadedFilter.UniversalTransaction.FilterType);
		}

		ZGuid InsertBadFilterContent(string filterContent)
		{
			var pk = ZGuid.NewZGuid();
			var sql = string.Format(@"
				INSERT INTO {0} (
					{1}, {3}, {5}, {7}, {9}, {11}, {13}, {15}
				) VALUES (
					'{2}', '{4}', '{6}', '{8}', '{10}', '{12}', '{14}', '{16}'
				)
				",
				EDIMessageContentFilterSchema.Constants.TableName,
				EDIMessageContentFilterSchema.Constants.PK, pk,
				EDIMessageContentFilterSchema.Constants.ECF_Name, "Bad data",
				EDIMessageContentFilterSchema.Constants.ECF_FilterType, EDIMessageContentFilterTypes.Codes.Exclude,
				EDIMessageContentFilterSchema.Constants.ECF_FilterContent, filterContent,
				EDIMessageContentFilterSchema.Constants.ECF_SystemCreateUser, "ABC",
				EDIMessageContentFilterSchema.Constants.ECF_SystemCreateTimeUtc, new ZDateTime(DateTime.Now),
				EDIMessageContentFilterSchema.Constants.ECF_SystemLastEditUser, "ABC",
				EDIMessageContentFilterSchema.Constants.ECF_SystemLastEditTimeUtc, new ZDateTime(DateTime.Now));

			Db.Connection.ExecuteNonQuery(sql);

			return pk;
		}

		void AssertGoodFilterContent(string filterContent)
		{
			AssertEquals(@"<ECF_FilterContent>
  <UniversalEvent>
    <Lines />
    <Documents />
  </UniversalEvent>
  <UniversalShipment>
    <AdditionalConfiguration />
    <Lines />
    <Documents />
  </UniversalShipment>
  <UniversalTransaction>
    <Lines />
    <Documents />
  </UniversalTransaction>
  <Config />
</ECF_FilterContent>", filterContent);
		}
	}
}
