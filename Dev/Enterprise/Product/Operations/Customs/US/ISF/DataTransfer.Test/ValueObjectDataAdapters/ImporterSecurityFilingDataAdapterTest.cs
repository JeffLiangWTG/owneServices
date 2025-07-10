using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CusBondDetail = Enterprise.MasterFiles.Business.CusBondDetail;
using CusBondDetailCollection = Enterprise.MasterFiles.Business.CusBondDetailCollection;
using OrgSupplierPart = Enterprise.MasterFiles.Business.OrgSupplierPart;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.ISF.DataTransfer.Testing
{
	[TestedType(typeof(ImporterSecurityFilingDataAdapter))]
	sealed class ImporterSecurityFilingDataAdapterTest : ValueObjectDataAdapterTest<CusISFHeader, Xsd.ISF>
	{
		public void TestDataImportEventIsAdded()
		{
			ImporterSecurityFilingDataAdapter adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			Xsd.ISF xsdISF = new Xsd.ISF();
			xsdISF.CarrierSCAC = "SCAC";
			xsdISF.Action.IsSpecified = false;
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CusISFHeader bizObj = adapter.CreateOrUpdateFromValueObject(xsdISF, context);
			AssertNotNull("BizO should have a DataImport event", bizObj.Logs.MostRecentLogByEventTime(Events.DataImport, "Add"));
			xsdISF.Action.IsSpecified = true;
			xsdISF.Action.TypeSpecified = true;
			xsdISF.Action.Type = Xsd.ISFActionType.Replace;
			bizObj.BF_JobReference = "ISF23BF32343";
			xsdISF.JobReference = "ISF23BF32343";
			bizObj = adapter.CreateOrUpdateFromValueObject(xsdISF, context);
			bizObj.Logs.Delete();
			AssertNotNull("BizO should have a DataImport event", bizObj.Logs.MostRecentLogByEventTime(Events.DataImport, "Replace"));
			xsdISF.Action.Messaging.IsSpecified = true;
			xsdISF.Action.Messaging.SendToCustomsSpecified = true;
			xsdISF.Action.Messaging.SendToCustoms = true;
			adapter.AutoSendToCustomsList = null;
			bizObj = adapter.CreateOrUpdateFromValueObject(xsdISF, context);
			bizObj.Logs.Delete();
			AssertNull("BizO should have a DataImport event", bizObj.Logs.MostRecentLogByEventTime(Events.DataImport, "Replace With Send To Customs"));
			AssertNotNull("BizO should have a DataImport event", bizObj.Logs.MostRecentLogByEventTime(Events.DataImport, "Replace"));
			adapter.AutoSendToCustomsList = new Dictionary<ZGuid, Xsd.ISFActionType>();
			bizObj = adapter.CreateOrUpdateFromValueObject(xsdISF, context);
			AssertNotNull("BizO should have a DataImport event", bizObj.Logs.MostRecentLogByEventTime(Events.DataImport, "Replace With Send To Customs"));
		}

		public void TestDataExportEventIsAdded()
		{
			ImporterSecurityFilingDataAdapter adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_SCAC = "SCAC";
			Xsd.ISF xsdISF = adapter.ExportToValueObject(header, new ValueObjectExportContext(new NotificationBuffer()));
			AssertNotNull("BizO should have a DataImport event", header.Logs.MostRecentLogByEventTime(Events.DataExport));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSchemaValidation()
		{
			var notification = new NotificationBuffer();
			var adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			var dataImporter = new XmlDataImporter(adapter);
			var noOfISFs = Factory.GetDatabaseCount(typeof(CusISFHeader));
			var fileName = BaseTestFilePath + "ISFInvalidSchema.xml";
			dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			var allowNumericCharactersInCodeGeneration = Registry.Business.OrganisationsDataRegistry.Instance.AllowNumericCharactersInCodeGeneration.Value;
			var useUnmatchedOrganisationForMatching = Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.IsEnabled;
			var threshold = Registry.Business.OrganisationsDataRegistry.Instance.OrgMatchThreshold.Value;
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy.OH_Code;
			var companyCode = GlbCompany.CurrentCompany.GC_Code;
			var branchCode = GlbBranch.CurrentBranch.GB_Code;
			var schemaErrorMessage = "Registry value for Organizations -> Allow Numeric Characters In Code Generation is currently " + (allowNumericCharactersInCodeGeneration ? "Enabled" : "Disabled") + ".\r\n" + "Registry value for Organizations -> Use Default Organization for Matching is currently " + (useUnmatchedOrganisationForMatching ? "Enabled" : "Disabled") + ".\r\n" + "Current Organization Match Threshold - " + threshold + ".\r\n" + "Current Organization Proxy - " + orgProxy + ".\r\n" + "Current Company - " + companyCode + ".\r\n" + "Current Branch - " + branchCode + ".\r\n\r\n" + "Action should be Replace if the Transaction Number is known.";
			AssertMultilineASCIIEquals("Schema Error", schemaErrorMessage, notification.AsString);
			AssertEquals("No new ISF created", noOfISFs, Factory.GetDatabaseCount(typeof(CusISFHeader)));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTransportModeElementNotSpecified()
		{
			var adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			var dataImporter = new XmlDataImporter(adapter);
			var fileName = BaseTestFilePath + "EmptyTransportMode.xml";
			dataImporter.ImportData(fileName, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			Factory.Save();
			var header = Factory.LoadTop1<CusISFHeader>(new ZQuery(CusISFHeaderSchema.BF_ImporterCode, "91-999999999"));
			AssertEquals(Business.TransportModeCodes.Codes.OceanVesselContainerized, header.BF_TransportMode);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTwoManufacturers()
		{
			var importer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
			var part = Factory.New<Customs.Business.OrgSupplierPart>();
			part.OP_PartNum = "7BO7239423";
			part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "6110303059";
			var pivot1Child1 = pivot1.Children.AddNew();
			pivot1Child1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot1Child1.CI_ChildListOrder = 1;
			pivot1Child1.CI_TariffNum = "6108910030";
			var pivot1Child2 = pivot1.Children.AddNew();
			pivot1Child2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot1Child2.CI_TariffNum = "6205202016";
			pivot1Child2.CI_ChildListOrder = 2;
			var pivot1Child3 = pivot1.Children.AddNew();
			pivot1Child3.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			pivot1Child3.CI_TariffNum = "6205202000";
			pivot1Child3.CI_ChildListOrder = 2;
			Factory.Save();
			var adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			var dataImporter = new XmlDataImporter(adapter);
			var fileName = BaseTestFilePath + "TwoManufacturers.xml";
			dataImporter.ImportData(fileName, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			Factory.Save();
			var header = Factory.LoadTop1<CusISFHeader>(new ZQuery(CusISFHeaderSchema.BF_OH_Importer, importer.PK));
			var line = header.Lines.FirstOrDefault(x => x.BL_HarmonisedNum == "6110303059");
			foreach (var child in line.ChildLines)
			{
				AssertEquals("Child line should have manufacturer Info.", child.ManufacturerDocAddress.PK, line.ManufacturerDocAddress.PK);
			}
		}

		// CS00077140
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportMappedCountryOfOrigin()
		{
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			var orgPatternMatch = orgProxy.CreatePatternMatchOverrideForTest();
			orgPatternMatch.OO_ForeignCode = "DING INDIA DONG";
			orgPatternMatch.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Country;
			orgPatternMatch.OO_LocalGuid = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.India).PK;
			Factory.Save();
			var adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			var dataImporter = new XmlDataImporter(adapter);
			var fileName = BaseTestFilePath + "CS00077140Sample.xml";
			var buffer = new NotificationBuffer();
			dataImporter.ImportData(fileName, buffer, SourceInfo.EmptySourceInfo);
			Factory.Save();
			var query = new ZQuery(CusISFHeaderSchema.BF_ImporterCode, "91-999999999");
			query.AddToFilter(CusISFHeaderSchema.BF_OwnerReference, "4500213900");
			var header = Factory.LoadTop1<CusISFHeader>(query);
			AssertEquals(2, header.Lines.Count);
			AssertEquals(Core.Constants.CountryCodes.India, header.Lines[0].BL_RN_NKGoodsOrigin);
			AssertEquals(Core.Constants.CountryCodes.India, header.Lines[1].BL_RN_NKGoodsOrigin);
		}

		public void TestImportFromImporterCodeSetBranchFromOrganisation()
		{
			GlbCompany company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			GlbBranch branch2 = company.Branches.AddNew();
			branch2.GB_Code = "ZZ2";
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER COMPANY";
			importer.OH_RL_NKClosestPort = "USCHI";
			importer.MainAddress.OA_Address1 = "IMPORTER ADDRESS 1";
			importer.MainAddress.OA_Address2 = "IMPORTER ADDRESS 2";
			importer.MainAddress.OA_City = "CHICAGO";
			importer.MainAddress.OA_State = "IL";
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");
			importer.CompanyData.OB_GB_ControllingBranch = branch2.PK;
			ImporterSecurityFilingDataAdapter adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			Xsd.ISF xsdISF = new Xsd.ISF();
			xsdISF.ImporterOfRecord.IsSpecified = true;
			Xsd.RegistrationNumber number = new Xsd.RegistrationNumber();
			number.IsSpecified = true;
			number.Number = "91-013199000";
			number.NumberType = Xsd.RegistrationNumberTypes.EIN;
			xsdISF.ImporterOfRecord.Item = number;
			Xsd.ISFReferenceID oceanBill = xsdISF.ReferenceIDs.AddNew();
			oceanBill.IsSpecified = true;
			oceanBill.Number = "OB23423223";
			oceanBill.Type = Xsd.ISFReferenceIDType.OB;
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CusISFHeader bizObj = adapter.CreateOrUpdateFromValueObject(xsdISF, context);
			AssertEquals("91-013199000", bizObj.BF_ImporterCode);
			AssertEquals(ImporterCodeTypeList.Codes.IRS, bizObj.BF_ImporterCodeType);
			AssertEquals(branch2.PK, bizObj.BF_GB);
			AssertEquals("OB23423223", bizObj.BF_OceanBill);
			xsdISF.Action.IsSpecified = true;
			xsdISF.Action.Type = Xsd.ISFActionType.Replace;
			xsdISF.Action.TypeSpecified = true;
			bizObj.BF_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			CusISFHeader bizObj2 = adapter.CreateOrUpdateFromValueObject(xsdISF, context);
			AssertEquals(bizObj2, bizObj);
			AssertEquals("91-013199000", bizObj.BF_ImporterCode);
			AssertEquals(ImporterCodeTypeList.Codes.IRS, bizObj.BF_ImporterCodeType);
			AssertEquals(GlbBranch.CurrentBranch.PK, bizObj.BF_GB);
			AssertEquals("OB23423223", bizObj.BF_OceanBill);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdatingViaImporterAndBills()
		{
			var adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			var dataImporter = new XmlDataImporter(adapter);
			var notification = new NotificationBuffer();
			var noOfISFs = Factory.GetDatabaseCount(typeof(CusISFHeader));
			var fileName = BaseTestFilePath + "UpdatingViaImporterAndBillsImporterSecurityFiling.xml";
			dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			var noISFMatchingEitherHouseBillOrOreanBill = string.Format(ImporterSecurityFilingDataAdapter.NoISFMatchingEitherHouseBillOrOreanBill, "Replace");
			AssertContains("No data imported as invalid match", noISFMatchingEitherHouseBillOrOreanBill, notification.AsString);
			AssertEquals("No new ISF created", noOfISFs, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			var headerNoMatched = Factory.New<CusISFHeader>();
			headerNoMatched.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			headerNoMatched.BF_ImporterCode = "91-013199000";
			var billNoMatched = headerNoMatched.ReferenceDatas.AddNew();
			billNoMatched.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			billNoMatched.BB_BillNum = "BM864845654";
			var header1 = Factory.New<CusISFHeader>();
			header1.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			header1.BF_ImporterCode = "91-013199000";
			var bill1 = header1.ReferenceDatas.AddNew();
			bill1.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill1.BB_BillNum = "BM12356897";
			var header2 = Factory.New<CusISFHeader>();
			header2.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			header2.BF_ImporterCode = "91-013199000";
			header2.BF_CustomsReference = "XJ5-20089367423";
			var bill2 = header2.ReferenceDatas.AddNew();
			bill2.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill2.BB_BillNum = "BM56846559";
			Factory.Save();
			notification.Clear();
			dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			var multipleISFMatchingEitherHouseBillOrOreanBill = string.Format(ImporterSecurityFilingDataAdapter.MultipleISFMatchingEitherHouseBillOrOreanBill, "Replace the existing data");
			AssertContains("No data imported as multiple transaction number matched", multipleISFMatchingEitherHouseBillOrOreanBill, notification.AsString);
			AssertEquals("No new ISF created", noOfISFs + 3, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			headerNoMatched.Reload();
			AssertEquals(0, headerNoMatched.Lines.Count);
			AssertEquals(1, headerNoMatched.ReferenceDatas.Count);
			header1.Reload();
			AssertEquals(0, header1.Lines.Count);
			AssertEquals(1, header1.ReferenceDatas.Count);
			header2.Reload();
			AssertEquals(0, header2.Lines.Count);
			AssertEquals(1, header2.ReferenceDatas.Count);
			header2.Delete();
			Factory.Save();
			notification.Clear();
			dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			AssertEquals("No new ISF created", noOfISFs + 2, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			AssertNotContains("Data was imported successfully", noISFMatchingEitherHouseBillOrOreanBill, notification.AsString);
			AssertNotContains("Data was imported successfully", multipleISFMatchingEitherHouseBillOrOreanBill, notification.AsString);
			headerNoMatched.Reload();
			AssertEquals(0, headerNoMatched.Lines.Count);
			AssertEquals(1, headerNoMatched.ReferenceDatas.Count);
			header1.Reload();
			AssertEquals(3, header1.Lines.Count);
			AssertNotNull(header1.ReferenceDatas[Common.US.ISF.BillTypeList.Codes.HouseBillOfLading, "BM12356897"]);
			AssertNotNull(header1.ReferenceDatas[Common.US.ISF.BillTypeList.Codes.HouseBillOfLading, "BM56846559"]);
			AssertEquals(SubmissionTypeList.Codes.ISF10, header1.BF_EntryType);
			AssertEquals(ShipmentTypeList.Codes.StandardOrRegularFilings, header1.BF_ShipmentType);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdatingViaTransactionNumber()
		{
			var adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			var dataImporter = new XmlDataImporter(adapter);
			var notification = new NotificationBuffer();
			var noOfISFs = Factory.GetDatabaseCount(typeof(CusISFHeader));
			var fileName = BaseTestFilePath + "UpdatingViaTransactionNumberImporterSecurityFiling.xml";
			dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			var noISFMatchingTransactionNumber = string.Format(ImporterSecurityFilingDataAdapter.NoISFMatchingTransactionNumber, "Replace");
			AssertContains("No data imported as invalid match", noISFMatchingTransactionNumber, notification.AsString);
			AssertEquals("No new ISF created", noOfISFs, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			var header1 = Factory.New<CusISFHeader>();
			header1.BF_CustomsReference = "XJ5-20089367423";
			var header2 = Factory.New<CusISFHeader>();
			header2.BF_CustomsReference = "XJ5-20089367423";
			Factory.Save();
			notification.Clear();
			dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			var multipleISFMatchingTransactionNumber = string.Format(ImporterSecurityFilingDataAdapter.MultipleISFMatchingTransactionNumber, "Replace");
			AssertEquals(notification.AsString, true, notification.HasErrors);
			AssertContains("No data imported as multiple transaction number matched", multipleISFMatchingTransactionNumber, notification.AsString);
			AssertEquals("No new ISF created", noOfISFs + 2, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			header1.Reload();
			AssertEquals(0, header1.Lines.Count);
			header2.Reload();
			AssertEquals(0, header2.Lines.Count);
			header2.Delete();
			Factory.Save();
			notification.Clear();
			dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			AssertEquals(notification.AsString, false, notification.HasErrors);
			AssertEquals("No new ISF created", noOfISFs + 1, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			AssertNotContains("Data was imported successfully", noISFMatchingTransactionNumber, notification.AsString);
			AssertNotContains("Data was imported successfully", multipleISFMatchingTransactionNumber, notification.AsString);
			header1.Reload();
			AssertEquals(3, header1.Lines.Count);
			AssertEquals(ImporterCodeTypeList.Codes.IRS, header1.BF_ImporterCodeType);
			AssertEquals("91-013199000", header1.BF_ImporterCode);
			AssertEquals(SubmissionTypeList.Codes.ISF10, header1.BF_EntryType);
			AssertEquals(ShipmentTypeList.Codes.StandardOrRegularFilings, header1.BF_ShipmentType);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdatingDoesNotReTriggerExistingEvent_XmlContainsWorkflowTrigger()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "ISF";
			template.P0_OH_Client = client.PK;
			var trigger1 = template.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "CLEAR ISF ADD";
			trigger1.TriggerConditions.TriggerEventCode = Events.MessageStatusChangeCode;
			trigger1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			trigger1.TriggerConditions.TriggerConditionValue = MessageStatusList.Codes.ClearISFAdd;
			var notification1 = trigger1.ProcessTaskNotifications.AddNew();
			notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification1.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification1.PQ_EmailAddr = "dummy1@where.com";
			var trigger2 = template.WorkflowItems.Triggers.AddNew();
			trigger2.P9_Description = "BILL STATUS";
			trigger2.TriggerConditions.TriggerEventCode = Events.MessageStatusChangeCode;
			trigger2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			trigger2.TriggerConditions.TriggerConditionValue = "S2";
			var notification2 = trigger2.ProcessTaskNotifications.AddNew();
			notification2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification2.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification2.PQ_EmailAddr = "dummy2@where.com";
			var trigger3 = template.WorkflowItems.Triggers.AddNew();
			trigger3.P9_Description = "Send To Customs";
			trigger3.TriggerConditions.TriggerEventCode = Events.DataImportCode;
			var notification3 = trigger3.ProcessTaskNotifications.AddNew();
			notification3.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification3.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification3.PQ_EmailAddr = "dummy3@where.com";
			var milestones1 = template.WorkflowItems.Milestones.AddNew();
			milestones1.P9_Description = "MILESTONE";
			milestones1.TriggerConditions.TriggerEventCode = Events.DataImportCode;
			var milestones1Notification = milestones1.ProcessTaskNotifications.AddNew();
			milestones1Notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			milestones1Notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			milestones1Notification.PQ_EmailAddr = "dummy5@where.com";
			Factory.Save();
			var header = Factory.New<CusISFHeader>();
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.FlexibleRange;
			header.BF_JobReference = "ISF1234567";
			header.BF_OH_Importer = client.PK;
			Factory.Save();
			var milestones = header.WorkflowItems.Milestones.Cast<ProcessTask>().ToArray();
			AssertEquals(1, milestones.Length);
			var headerMilestone1 = milestones[0];
			AssertEquals("headerMilestone1.P9_ParentTemplateID", milestones1.PK, headerMilestone1.P9_ParentTemplateID);
			AssertEquals("headerMilestone1.P9_ActualDate", ZDateTime.Empty, headerMilestone1.P9_ActualDate);
			var triggers = header.WorkflowItems.Triggers.Cast<ProcessTask>().ToArray();
			AssertEquals(3, triggers.Length);
			var headerTrigger1 = triggers.FirstOrDefault(x => x.P9_Description == "CLEAR ISF ADD");
			var headerTrigger2 = triggers.FirstOrDefault(x => x.P9_Description == "BILL STATUS");
			var headerTrigger3 = triggers.FirstOrDefault(x => x.P9_Description == "Send To Customs");
			AssertEquals("headerTrigger1.P9_ParentTemplateID", trigger1.PK, headerTrigger1.P9_ParentTemplateID);
			AssertEquals("headerTrigger1.P9_ActualDate", ZDateTime.Empty, headerTrigger1.P9_ActualDate);
			AssertEquals("headerTrigger2.P9_ParentTemplateID", trigger2.PK, headerTrigger2.P9_ParentTemplateID);
			AssertEquals("headerTrigger2.P9_ActualDate", ZDateTime.Empty, headerTrigger2.P9_ActualDate);
			AssertEquals("headerTrigger3.P9_ParentTemplateID", trigger3.PK, headerTrigger3.P9_ParentTemplateID);
			AssertEquals("headerTrigger3.P9_ActualDate", ZDateTime.Empty, headerTrigger3.P9_ActualDate);
			AssertNull("No WTE", headerTrigger1.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));
			AssertNull("No WTE", headerTrigger2.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));
			AssertNull("No WTE", headerTrigger3.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));
			var headerTrigger4 = header.WorkflowItems.Triggers.AddNew();
			headerTrigger4.P9_Description = "Send To Customs";
			headerTrigger4.TriggerConditions.TriggerEventCode = Events.DataImportCode;
			var notification4 = headerTrigger4.ProcessTaskNotifications.AddNew();
			notification4.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification4.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification4.PQ_EmailAddr = "dummy4@where.com";
			var headerMilestone2 = header.WorkflowItems.Milestones.AddNew();
			headerMilestone2.P9_Description = "MILESTONE 2";
			headerMilestone2.TriggerConditions.TriggerEventCode = Events.MessageStatusChangeCode;
			var headerMilestone2Notification = headerMilestone2.ProcessTaskNotifications.AddNew();
			headerMilestone2Notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			headerMilestone2Notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			headerMilestone2Notification.PQ_EmailAddr = "dummy6@where.com";
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFAdd;
			Factory.Save();
			var headerTrigger1ActualDate = headerTrigger1.P9_ActualDate;
			AssertNotEquals("headerTrigger1.P9_ActualDate", ZDateTime.Empty, headerTrigger1ActualDate);
			AssertEquals("headerTrigger2.P9_ActualDate", ZDateTime.Empty, headerTrigger2.P9_ActualDate);
			AssertEquals("headerTrigger3.P9_ActualDate", ZDateTime.Empty, headerTrigger3.P9_ActualDate);
			AssertEquals("headerTrigger4.P9_ActualDate", ZDateTime.Empty, headerTrigger4.P9_ActualDate);
			var headerTrigger1WTELogs = headerTrigger1.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code));
			AssertEquals("Has WTE", 1, headerTrigger1WTELogs.Length);
			var headerTrigger1WTELog = headerTrigger1WTELogs[0];
			AssertNull("No WTE", headerTrigger2.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));
			AssertNull("No WTE", headerTrigger3.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));
			AssertNull("No WTE", headerTrigger4.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			var dataImporter = new XmlDataImporter(adapter);
			var notification = new NotificationBuffer();
			var fileName = BaseTestFilePath + "UpdatingViaJobReferenceImporterSecurityFiling.xml";
			dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			var newFactory = new BusinessObjectFactory();
			var headerLoaded = newFactory.Load<CusISFHeader>(header.PK);
			triggers = headerLoaded.WorkflowItems.Triggers.Cast<ProcessTask>().ToArray();
			AssertEquals(1, triggers.Length);
			var headerLoadedTrigger = triggers[0];
			AssertNotEquals(headerTrigger1.PK, headerLoadedTrigger.PK);
			AssertNotEquals(headerTrigger2.PK, headerLoadedTrigger.PK);
			AssertNotEquals(headerTrigger3.PK, headerLoadedTrigger.PK);
			AssertNotEquals(headerTrigger4.PK, headerLoadedTrigger.PK);
			AssertEquals("headerLoadedTrigger.P9_ParentTemplateID", ZGuid.Empty, headerLoadedTrigger.P9_ParentTemplateID);
			AssertEquals("headerLoadedTrigger.P9_ActualDate", ZDateTime.Empty, headerLoadedTrigger.P9_ActualDate);
			AssertNull("No WTE", headerLoadedTrigger.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));
			AssertEquals("headerLoadedTrigger.P9_Description", "Send To Customs", headerLoadedTrigger.P9_Description);
			AssertEquals("headerLoadedTrigger.TriggerConditions.TriggerEventCode", Events.DataImportCode, headerLoadedTrigger.TriggerConditions.TriggerEventCode);
			AssertEquals("headerLoadedTrigger.TriggerConditions.TriggerCondition", EventReferenceConditionList.Codes.EventReference, headerLoadedTrigger.TriggerConditions.TriggerCondition);
			AssertEquals("headerLoadedTrigger.ProcessTaskNotifications.Count", 1, headerLoadedTrigger.ProcessTaskNotifications.Count);
			var headerLoadedTriggerNotification = headerLoadedTrigger.ProcessTaskNotifications[0];
			AssertEquals("headerLoadedTriggerNotification.PQ_TriggerType", WorkflowTriggerActionTypeConstants.Codes.SendXML, headerLoadedTriggerNotification.PQ_TriggerType);
			AssertEquals("headerLoadedTriggerNotification.PQ_TriggerParty", MessageRecipientPartyTypeList.Codes.Email, headerLoadedTriggerNotification.PQ_TriggerParty);
			AssertEquals("headerLoadedTriggerNotification.PQ_EmailAddr", "dummy@emailTesting.com", headerLoadedTriggerNotification.PQ_EmailAddr);
			AssertEquals("headerLoaded.WorkflowItems.Milestones.Count", 2, headerLoaded.WorkflowItems.Milestones.Count);
			AssertNotNull(headerLoaded.WorkflowItems.Milestones.FindByPK(headerMilestone1.PK));
			AssertNotNull(headerLoaded.WorkflowItems.Milestones.FindByPK(headerMilestone2.PK));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdatingDoesNotReTriggerExistingEvent_XmlDoesNotContainsWorkflowTrigger()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "ISF";
			template.P0_OH_Client = client.PK;
			var trigger1 = template.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "CLEAR ISF ADD";
			trigger1.TriggerConditions.TriggerEventCode = Events.MessageStatusChangeCode;
			trigger1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			trigger1.TriggerConditions.TriggerConditionValue = MessageStatusList.Codes.ClearISFAdd;
			var notification1 = trigger1.ProcessTaskNotifications.AddNew();
			notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification1.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification1.PQ_EmailAddr = "dummy1@where.com";
			var trigger2 = template.WorkflowItems.Triggers.AddNew();
			trigger2.P9_Description = "BILL STATUS";
			trigger2.TriggerConditions.TriggerEventCode = Events.MessageStatusChangeCode;
			trigger2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			trigger2.TriggerConditions.TriggerConditionValue = "S2";
			var notification2 = trigger2.ProcessTaskNotifications.AddNew();
			notification2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification2.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification2.PQ_EmailAddr = "dummy2@where.com";
			var trigger3 = template.WorkflowItems.Triggers.AddNew();
			trigger3.P9_Description = "Send To Customs";
			trigger3.TriggerConditions.TriggerEventCode = Events.DataImportCode;
			var notification3 = trigger3.ProcessTaskNotifications.AddNew();
			notification3.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification3.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification3.PQ_EmailAddr = "dummy3@where.com";
			var milestones1 = template.WorkflowItems.Milestones.AddNew();
			milestones1.P9_Description = "MILESTONE";
			milestones1.TriggerConditions.TriggerEventCode = Events.DataImportCode;
			var milestones1Notification = milestones1.ProcessTaskNotifications.AddNew();
			milestones1Notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			milestones1Notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			milestones1Notification.PQ_EmailAddr = "dummy5@where.com";
			Factory.Save();
			var header = Factory.New<CusISFHeader>();
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.FlexibleRange;
			header.BF_JobReference = "ISF1234567";
			header.BF_OH_Importer = client.PK;
			Factory.Save();
			var milestones = header.WorkflowItems.Milestones.Cast<ProcessTask>().ToArray();
			AssertEquals(1, milestones.Length);
			var headerMilestone1 = milestones[0];
			AssertEquals("headerMilestone1.P9_ParentTemplateID", milestones1.PK, headerMilestone1.P9_ParentTemplateID);
			AssertEquals("headerMilestone1.P9_ActualDate", ZDateTime.Empty, headerMilestone1.P9_ActualDate);
			var triggers = header.WorkflowItems.Triggers.Cast<ProcessTask>().ToArray();
			AssertEquals(3, triggers.Length);
			var headerTrigger1 = triggers.FirstOrDefault(x => x.P9_Description == "CLEAR ISF ADD");
			var headerTrigger2 = triggers.FirstOrDefault(x => x.P9_Description == "BILL STATUS");
			var headerTrigger3 = triggers.FirstOrDefault(x => x.P9_Description == "Send To Customs");
			AssertEquals("headerTrigger1.P9_ParentTemplateID", trigger1.PK, headerTrigger1.P9_ParentTemplateID);
			AssertEquals("headerTrigger1.P9_ActualDate", ZDateTime.Empty, headerTrigger1.P9_ActualDate);
			AssertEquals("headerTrigger2.P9_ParentTemplateID", trigger2.PK, headerTrigger2.P9_ParentTemplateID);
			AssertEquals("headerTrigger2.P9_ActualDate", ZDateTime.Empty, headerTrigger2.P9_ActualDate);
			AssertEquals("headerTrigger3.P9_ParentTemplateID", trigger3.PK, headerTrigger3.P9_ParentTemplateID);
			AssertEquals("headerTrigger3.P9_ActualDate", ZDateTime.Empty, headerTrigger3.P9_ActualDate);
			AssertNull("No WTE", headerTrigger1.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));
			AssertNull("No WTE", headerTrigger2.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));
			AssertNull("No WTE", headerTrigger3.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));
			var headerTrigger4 = header.WorkflowItems.Triggers.AddNew();
			headerTrigger4.P9_Description = "Send To Customs";
			headerTrigger4.TriggerConditions.TriggerEventCode = Events.DataImportCode;
			var notification4 = headerTrigger4.ProcessTaskNotifications.AddNew();
			notification4.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification4.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification4.PQ_EmailAddr = "dummy4@where.com";
			var headerMilestone2 = header.WorkflowItems.Milestones.AddNew();
			headerMilestone2.P9_Description = "MILESTONE 2";
			headerMilestone2.TriggerConditions.TriggerEventCode = Events.MessageStatusChangeCode;
			var headerMilestone2Notification = headerMilestone2.ProcessTaskNotifications.AddNew();
			headerMilestone2Notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			headerMilestone2Notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			headerMilestone2Notification.PQ_EmailAddr = "dummy6@where.com";
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFAdd;
			Factory.Save();
			var headerTrigger1ActualDate = headerTrigger1.P9_ActualDate;
			AssertNotEquals("headerTrigger1.P9_ActualDate", ZDateTime.Empty, headerTrigger1ActualDate);
			AssertEquals("headerTrigger2.P9_ActualDate", ZDateTime.Empty, headerTrigger2.P9_ActualDate);
			AssertEquals("headerTrigger3.P9_ActualDate", ZDateTime.Empty, headerTrigger3.P9_ActualDate);
			AssertEquals("headerTrigger4.P9_ActualDate", ZDateTime.Empty, headerTrigger4.P9_ActualDate);
			var headerTrigger1WTELogs = headerTrigger1.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code));
			AssertEquals("Has WTE", 1, headerTrigger1WTELogs.Length);
			var headerTrigger1WTELog = headerTrigger1WTELogs[0];
			AssertNull("No WTE", headerTrigger2.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));
			AssertNull("No WTE", headerTrigger3.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));
			AssertNull("No WTE", headerTrigger4.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			var dataImporter = new XmlDataImporter(adapter);
			var notification = new NotificationBuffer();
			var fileName = BaseTestFilePath + "UpdatingViaJobReferenceWithoutTriggers.xml";
			dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			var newFactory = new BusinessObjectFactory();
			var headerLoaded = newFactory.Load<CusISFHeader>(header.PK);
			AssertEquals(4, headerLoaded.WorkflowItems.Triggers.Count);
			var headerLoadedTrigger1 = (ProcessTask)headerLoaded.WorkflowItems.Triggers.FindByPK(headerTrigger1.PK);
			var headerLoadedTrigger2 = (ProcessTask)headerLoaded.WorkflowItems.Triggers.FindByPK(headerTrigger2.PK);
			var headerLoadedTrigger3 = (ProcessTask)headerLoaded.WorkflowItems.Triggers.FindByPK(headerTrigger3.PK);
			var headerLoadedTrigger4 = (ProcessTask)headerLoaded.WorkflowItems.Triggers.FindByPK(headerTrigger4.PK);
			AssertEquals("headerLoadedTrigger1.P9_ParentTemplateID", trigger1.PK, headerLoadedTrigger1.P9_ParentTemplateID);
			AssertEquals("headerLoadedTrigger1.P9_ActualDate", headerTrigger1ActualDate, headerLoadedTrigger1.P9_ActualDate);
			AssertEquals("headerLoadedTrigger2.P9_ParentTemplateID", trigger2.PK, headerLoadedTrigger2.P9_ParentTemplateID);
			AssertEquals("headerLoadedTrigger2.P9_ActualDate", ZDateTime.Empty, headerLoadedTrigger2.P9_ActualDate);
			AssertEquals("headerLoadedTrigger3.P9_ParentTemplateID", trigger3.PK, headerLoadedTrigger3.P9_ParentTemplateID);
			AssertNotEquals("headerLoadedTrigger3.P9_ActualDate", ZDateTime.Empty, headerLoadedTrigger3.P9_ActualDate);
			AssertEquals("headerLoadedTrigger4.P9_ParentTemplateID", ZGuid.Empty, headerLoadedTrigger4.P9_ParentTemplateID);
			AssertNotEquals("headerLoadedTrigger4.P9_ActualDate", ZDateTime.Empty, headerLoadedTrigger4.P9_ActualDate);
			headerTrigger1WTELogs = headerLoadedTrigger1.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code));
			AssertEquals("Has WTE", 1, headerTrigger1WTELogs.Length);
			AssertEquals(headerTrigger1WTELog.PK, headerTrigger1WTELogs[0].PK);
			AssertNull("No WTE", headerLoadedTrigger2.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));
			AssertNotNull(headerLoadedTrigger3.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));
			AssertNotNull(headerLoadedTrigger4.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));
			AssertEquals("headerLoaded.WorkflowItems.Milestones.Count", 2, headerLoaded.WorkflowItems.Milestones.Count);
			AssertNotNull(headerLoaded.WorkflowItems.Milestones.FindByPK(headerMilestone1.PK));
			AssertNotNull(headerLoaded.WorkflowItems.Milestones.FindByPK(headerMilestone2.PK));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdatingViaJobReference()
		{
			var adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			var dataImporter = new XmlDataImporter(adapter);
			var notification = new NotificationBuffer();
			var noOfISFs = Factory.GetDatabaseCount(typeof(CusISFHeader));
			var fileName = BaseTestFilePath + "UpdatingViaJobReferenceImporterSecurityFiling.xml";
			dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			var noISFMatchingJobReference = string.Format(ImporterSecurityFilingDataAdapter.NoISFMatchingJobReference, "Replace");
			AssertContains("No data imported as invalid match", noISFMatchingJobReference, notification.AsString);
			AssertEquals("No new ISF created", noOfISFs, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			var header1 = Factory.New<CusISFHeader>();
			header1.BF_ActionReasonCode = ActionReasonCodeList.Codes.FlexibleRange;
			header1.BF_JobReference = "ISF1234567";
			Factory.Save();
			notification.Clear();
			dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			AssertEquals("No new ISF created", noOfISFs + 1, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			AssertNotContains("Data was imported successfully", noISFMatchingJobReference, notification.AsString);
			header1.Reload();
			AssertEquals(3, header1.Lines.Count);
			AssertEquals(ImporterCodeTypeList.Codes.IRS, header1.BF_ImporterCodeType);
			AssertEquals("91-013199000", header1.BF_ImporterCode);
			AssertEquals(SubmissionTypeList.Codes.ISF10, header1.BF_EntryType);
			AssertEquals(ShipmentTypeList.Codes.StandardOrRegularFilings, header1.BF_ShipmentType);
			AssertEquals(ActionReasonCodeList.Codes.FlexibleRange, header1.BF_ActionReasonCode);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdatingViaImporterAndOwnerReference()
		{
			var adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			var dataImporter = new XmlDataImporter(adapter);
			var notification = new NotificationBuffer();
			var noOfISFs = Factory.GetDatabaseCount(typeof(CusISFHeader));
			var fileName = BaseTestFilePath + "UpdatingViaImporterAndOwnerReferenceImporterSecurityFiling.xml";
			dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			var noISFMatchingEitherHouseBillOrOreanBillOrOwnerReference = string.Format(ImporterSecurityFilingDataAdapter.NoISFMatchingEitherHouseBillOrOreanBillOrOwnerReference, "Replace");
			AssertContains("No data imported as invalid match", noISFMatchingEitherHouseBillOrOreanBillOrOwnerReference, notification.AsString);
			AssertEquals("No new ISF created", noOfISFs, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			var header1 = Factory.New<CusISFHeader>();
			header1.BF_ActionReasonCode = ActionReasonCodeList.Codes.FlexibleRange;
			header1.BF_OwnerReference = "OF4343";
			header1.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			header1.BF_ImporterCode = "91-013199000";
			var header2 = Factory.New<CusISFHeader>();
			header2.BF_ActionReasonCode = ActionReasonCodeList.Codes.FlexibleRange;
			header2.BF_OwnerReference = "OF4343";
			header2.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			header2.BF_ImporterCode = "91-013199000";
			Factory.Save();
			notification.Clear();
			dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			var noISFMatchingOwnerReference = string.Format(ImporterSecurityFilingDataAdapter.NoISFMatchingOwnerReference, "Replace");
			AssertNotContains("Data imported as multiple owner reference matched", noISFMatchingOwnerReference, notification.AsString);
			AssertNotContains("No data imported as multiple owner reference matched", noISFMatchingEitherHouseBillOrOreanBillOrOwnerReference, notification.AsString);
			AssertEquals("No new ISF created", noOfISFs + 2, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			header1.Reload();
			AssertEquals(0, header1.Lines.Count);
			header2.Reload();
			AssertEquals(0, header2.Lines.Count);
			header2.Delete();
			Factory.Save();
			notification.Clear();
			dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			AssertEquals("No new ISF created", noOfISFs + 1, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			AssertNotContains("Data was imported successfully", noISFMatchingEitherHouseBillOrOreanBillOrOwnerReference, notification.AsString);
			header1.Reload();
			AssertEquals(3, header1.Lines.Count);
			AssertEquals(ImporterCodeTypeList.Codes.IRS, header1.BF_ImporterCodeType);
			AssertEquals("91-013199000", header1.BF_ImporterCode);
			AssertEquals(SubmissionTypeList.Codes.ISF10, header1.BF_EntryType);
			AssertEquals(ShipmentTypeList.Codes.StandardOrRegularFilings, header1.BF_ShipmentType);
		}

		public void TestImportAddActionType()
		{
			Xsd.ISF xmlISF = new Xsd.ISF();
			xmlISF.Action.Type = Xsd.ISFActionType.Add;
			xmlISF.Action.TypeSpecified = true;
			Xsd.RegistrationNumber importerOfRecord = new Xsd.RegistrationNumber();
			importerOfRecord.IsSpecified = true;
			importerOfRecord.Number = "EIN2342342";
			importerOfRecord.NumberType = Xsd.RegistrationNumberTypes.EIN;
			xmlISF.ImporterOfRecord.Item = importerOfRecord;
			xmlISF.TransactionNumber = "TRN123456789";
			xmlISF.JobReference = "ISFB2324232";
			Xsd.ISFReferenceID xmlBill1 = xmlISF.ReferenceIDs.AddNew();
			xmlBill1.IsSpecified = true;
			xmlBill1.Number = "BM12345678";
			xmlBill1.Type = Xsd.ISFReferenceIDType.BM;
			Xsd.ISFReferenceID xmlBill2 = xmlISF.ReferenceIDs.AddNew();
			xmlBill2.IsSpecified = true;
			xmlBill2.Number = "BM90123456";
			xmlBill2.Type = Xsd.ISFReferenceIDType.BM;
			ImporterSecurityFilingDataAdapter adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			CusISFHeader header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header);
			AssertContains(ImporterSecurityFilingDataAdapter.CannotAddNewIfTransactionNumberIsKnown, notify.AsString);
			xmlISF.TransactionNumber = ZString.Empty;
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header);
			AssertContains(ImporterSecurityFilingDataAdapter.CannotAddNewIfJobReferenceIsKnown, notify.AsString);
			xmlISF.JobReference = ZString.Empty;
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNotNull(header);
			ZString notificationMessage = notify.AsString;
			AssertNotContains(ImporterSecurityFilingDataAdapter.CannotAddNewIfTransactionNumberIsKnown, notificationMessage);
			AssertNotContains(ImporterSecurityFilingDataAdapter.CannotAddNewIfJobReferenceIsKnown, notificationMessage);
			AssertNotContains(ImporterSecurityFilingDataAdapter.ActiveISFMatchingEitherHouseBillOrOreanBillExist, notificationMessage);
			header.BF_CustomsReference = "BSD223423";
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFAdd;
			header.BF_SystemCreateTimeUtc = ZDateTime.Today;
			Factory.Save();
			notify.Clear();
			CusISFHeader header2 = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header2);
			notificationMessage = notify.AsString;
			AssertContains(ImporterSecurityFilingDataAdapter.ActiveISFMatchingEitherHouseBillOrOreanBillExist, notificationMessage);
			AssertEquals("BSD223423", header.BF_CustomsReference);
			AssertEquals(MessageStatusList.Codes.ClearISFAdd, header.BF_CustomsStatus);
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearWithWarningISFDelete;
			Factory.Save();
			notify.Clear();
			header2 = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNotNull(header2);
			AssertEquals(header, header2);
			notificationMessage = notify.AsString;
			AssertNotContains(ImporterSecurityFilingDataAdapter.ActiveISFMatchingEitherHouseBillOrOreanBillExist, notificationMessage);
			AssertEquals(ZString.Empty, header.BF_CustomsReference);
			AssertEquals(MessageStatusList.Codes.NotSentISF, header.BF_CustomsStatus);
			header.BF_CustomsReference = "BSD223423";
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFAdd;
			header.BF_SystemCreateTimeUtc = ZDateTime.Today.AddDays(ImporterSecurityFilingDataAdapter.NoOfDaysOldAllowedInMatchingBill - 1);
			Factory.Save();
			notify.Clear();
			header2 = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNotNull(header2);
			AssertNotEquals(header, header2);
			notificationMessage = notify.AsString;
			AssertNotContains(ImporterSecurityFilingDataAdapter.ActiveISFMatchingEitherHouseBillOrOreanBillExist, notificationMessage);
			header.BF_SystemCreateTimeUtc = ZDateTime.Today.AddDays(ImporterSecurityFilingDataAdapter.NoOfDaysOldAllowedInMatchingBill + 1);
			Factory.Save();
			notify.Clear();
			CusISFHeader header3 = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header3);
			notificationMessage = notify.AsString;
			AssertContains(string.Format(ImporterSecurityFilingDataAdapter.MultipleISFMatchingEitherHouseBillOrOreanBill, "add a new ISF Job"), notificationMessage);
		}

		public void TestImportReplaceActionType()
		{
			CusISFHeader headerMatch1 = Factory.New<CusISFHeader>();
			headerMatch1.BF_CustomsReference = "MTC123456789";
			headerMatch1.BF_JobReference = "ISFM1234567";
			headerMatch1.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			headerMatch1.BF_ImporterCode = "EIN2342342";
			headerMatch1.BF_HouseBill = "BMM1234567";
			CusISFHeader headerMatch2 = Factory.New<CusISFHeader>();
			headerMatch2.BF_CustomsReference = "MTC123456789";
			headerMatch2.BF_JobReference = "ISFM1234568";
			headerMatch2.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			headerMatch2.BF_ImporterCode = "EIN2342342";
			headerMatch2.BF_HouseBill = "BMM1234567";
			Factory.Save();
			CusISFHeader headerMatch3 = Factory.New<CusISFHeader>();
			headerMatch3.BF_CustomsReference = "MTC323456789";
			Factory.Save();
			Xsd.ISF xmlISF = new Xsd.ISF();
			xmlISF.Action.Type = Xsd.ISFActionType.Replace;
			xmlISF.Action.TypeSpecified = true;
			ImporterSecurityFilingDataAdapter adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			CusISFHeader header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header);
			string noMatchingKeysSpecified = string.Format(ImporterSecurityFilingDataAdapter.NoMatchingKeysSpecified, "Replace");
			AssertContains(noMatchingKeysSpecified, notify.AsString);
			Xsd.RegistrationNumber importerOfRecord = new Xsd.RegistrationNumber();
			importerOfRecord.IsSpecified = true;
			importerOfRecord.Number = "EIN2342342";
			importerOfRecord.NumberType = Xsd.RegistrationNumberTypes.EIN;
			xmlISF.ImporterOfRecord.Item = importerOfRecord;
			xmlISF.TransactionNumber = "TRN123456789";
			xmlISF.JobReference = "ISFB2324232";
			Xsd.ISFReferenceID xmlBill1 = xmlISF.ReferenceIDs.AddNew();
			xmlBill1.IsSpecified = true;
			xmlBill1.Number = "BM12345678";
			xmlBill1.Type = Xsd.ISFReferenceIDType.BM;
			Xsd.ISFReferenceID xmlBill2 = xmlISF.ReferenceIDs.AddNew();
			xmlBill2.IsSpecified = true;
			xmlBill2.Number = "BMM1234567";
			xmlBill2.Type = Xsd.ISFReferenceIDType.BM;
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header);
			string notificationMessage = notify.AsString;
			AssertNotContains(noMatchingKeysSpecified, notificationMessage);
			string noISFMatchingTransactionNumber = string.Format(ImporterSecurityFilingDataAdapter.NoISFMatchingTransactionNumber, "Replace");
			AssertContains(noISFMatchingTransactionNumber, notificationMessage);
			xmlISF.TransactionNumber = "MTC123456789";
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header);
			notificationMessage = notify.AsString;
			AssertNotContains(noISFMatchingTransactionNumber, notificationMessage);
			string multipleISFMatchingTransactionNumber = string.Format(ImporterSecurityFilingDataAdapter.MultipleISFMatchingTransactionNumber, "Replace");
			AssertContains(multipleISFMatchingTransactionNumber, notificationMessage);
			xmlISF.TransactionNumber = "MTC323456789";
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNotNull(header);
			AssertEquals(headerMatch3, header);
			notificationMessage = notify.AsString;
			AssertNotContains(multipleISFMatchingTransactionNumber, notificationMessage);
			AssertEquals("BM12345678", headerMatch3.BF_HouseBill);
			xmlISF.TransactionNumber = ZString.Empty;
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header);
			notificationMessage = notify.AsString;
			string noISFMatchingJobReference = string.Format(ImporterSecurityFilingDataAdapter.NoISFMatchingJobReference, "Replace");
			AssertContains(noISFMatchingJobReference, notificationMessage);
			xmlISF.JobReference = headerMatch3.BF_JobReference;
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNotNull(header);
			AssertEquals(headerMatch3, header);
			notificationMessage = notify.AsString;
			xmlISF.JobReference = ZString.Empty;
			importerOfRecord.Number = "EIN6688544";
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header);
			notificationMessage = notify.AsString;
			string noISFMatchingEitherHouseBillOrOreanBill = string.Format(ImporterSecurityFilingDataAdapter.NoISFMatchingEitherHouseBillOrOreanBill, "Replace");
			AssertContains(noISFMatchingEitherHouseBillOrOreanBill, notificationMessage);
			importerOfRecord.Number = "EIN2342342";
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header);
			notificationMessage = notify.AsString;
			AssertNotContains(noISFMatchingEitherHouseBillOrOreanBill, notificationMessage);
			string multipleISFMatchingEitherHouseBillOrOreanBill = string.Format(ImporterSecurityFilingDataAdapter.MultipleISFMatchingEitherHouseBillOrOreanBill, "Replace the existing data");
			AssertContains(multipleISFMatchingEitherHouseBillOrOreanBill, notificationMessage);
			headerMatch1.BF_SystemCreateTimeUtc = ZDateTime.Today.AddDays(ImporterSecurityFilingDataAdapter.NoOfDaysOldAllowedInMatchingBill - 1);
			headerMatch2.BF_SystemCreateTimeUtc = ZDateTime.Today.AddDays(ImporterSecurityFilingDataAdapter.NoOfDaysOldAllowedInMatchingBill - 1);
			Factory.Save();
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNotNull(header);
			AssertEquals(headerMatch3, header);
			notificationMessage = notify.AsString;
			AssertNotContains(multipleISFMatchingEitherHouseBillOrOreanBill, notificationMessage);
			headerMatch3.BF_CustomsStatus = MessageStatusList.Codes.AwaitingISFAdd;
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header);
			notificationMessage = notify.AsString;
			string pendingCustomsResponseJobExist = string.Format(ImporterSecurityFilingDataAdapter.PendingCustomsResponseJobExist, "Replace");
			AssertContains(pendingCustomsResponseJobExist, notificationMessage);
			headerMatch3.BF_CustomsStatus = MessageStatusList.Codes.ClearISFDelete;
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header);
			notificationMessage = notify.AsString;
			AssertNotContains(pendingCustomsResponseJobExist, notificationMessage);
			string customsDeletedJobExist = string.Format(ImporterSecurityFilingDataAdapter.CustomsDeletedJobExist, "Replace");
			AssertContains(customsDeletedJobExist, notificationMessage);
		}

		public void TestImportDeleteActionType()
		{
			CusISFHeader headerMatch1 = Factory.New<CusISFHeader>();
			headerMatch1.BF_CustomsReference = "MTC123456789";
			headerMatch1.BF_JobReference = "ISFM1234567";
			headerMatch1.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			headerMatch1.BF_ImporterCode = "EIN2342342";
			headerMatch1.BF_HouseBill = "BMM1234567";
			CusISFHeader headerMatch2 = Factory.New<CusISFHeader>();
			headerMatch2.BF_CustomsReference = "MTC123456789";
			headerMatch2.BF_JobReference = "ISFM1234568";
			headerMatch2.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			headerMatch2.BF_ImporterCode = "EIN2342342";
			headerMatch2.BF_HouseBill = "BMM1234567";
			Factory.Save();
			CusISFHeader headerMatch3 = Factory.New<CusISFHeader>();
			headerMatch3.BF_CustomsReference = "MTC323456789";
			headerMatch3.BF_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			headerMatch3.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			headerMatch3.BF_ImporterCode = "EIN2342342";
			headerMatch3.BF_HouseBill = "BM12345678";
			GlbGroup groupZZ1 = Factory.New<GlbGroup>();
			groupZZ1.GG_Code = "ZZ1";
			GlbStaff staffZ1 = groupZZ1.Staff.AddNew();
			staffZ1.GS_Code = "Z1";
			staffZ1.GS_LoginName = "z1";
			staffZ1.GS_EmailAddress = "dong@pretend.email.com";
			Factory.Save();
			ISFRegistry.Instance.ImporterSecurityFilingXMLImportNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupZZ1.PK.ToGuid());
			Xsd.ISF xmlISF = new Xsd.ISF();
			xmlISF.Action.Type = Xsd.ISFActionType.Delete;
			xmlISF.Action.TypeSpecified = true;
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			ImporterSecurityFilingDataAdapter adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			CusISFHeader header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header);
			string noMatchingKeysSpecified = string.Format(ImporterSecurityFilingDataAdapter.NoMatchingKeysSpecified, "Delete");
			AssertContains(noMatchingKeysSpecified, notify.AsString);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			Xsd.RegistrationNumber importerOfRecord = new Xsd.RegistrationNumber();
			importerOfRecord.IsSpecified = true;
			importerOfRecord.Number = "EIN2342342";
			importerOfRecord.NumberType = Xsd.RegistrationNumberTypes.EIN;
			xmlISF.ImporterOfRecord.Item = importerOfRecord;
			xmlISF.TransactionNumber = "TRN123456789";
			xmlISF.JobReference = "ISFB2324232";
			Xsd.ISFReferenceID xmlBill1 = xmlISF.ReferenceIDs.AddNew();
			xmlBill1.IsSpecified = true;
			xmlBill1.Number = "BM12345678";
			xmlBill1.Type = Xsd.ISFReferenceIDType.BM;
			Xsd.ISFReferenceID xmlBill2 = xmlISF.ReferenceIDs.AddNew();
			xmlBill2.IsSpecified = true;
			xmlBill2.Number = "BMM1234567";
			xmlBill2.Type = Xsd.ISFReferenceIDType.BM;
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header);
			string notificationMessage = notify.AsString;
			AssertNotContains(noMatchingKeysSpecified, notificationMessage);
			string noISFMatchingTransactionNumber = string.Format(ImporterSecurityFilingDataAdapter.NoISFMatchingTransactionNumber, "Delete");
			AssertContains(noISFMatchingTransactionNumber, notificationMessage);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			xmlISF.TransactionNumber = "MTC123456789";
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header);
			notificationMessage = notify.AsString;
			AssertNotContains(noISFMatchingTransactionNumber, notificationMessage);
			string multipleISFMatchingTransactionNumber = string.Format(ImporterSecurityFilingDataAdapter.MultipleISFMatchingTransactionNumber, "Delete");
			AssertContains(multipleISFMatchingTransactionNumber, notificationMessage);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			xmlISF.TransactionNumber = "MTC323456789";
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNotNull(header);
			AssertEquals(headerMatch3, header);
			notificationMessage = notify.AsString;
			AssertNotContains(multipleISFMatchingTransactionNumber, notificationMessage);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("ISF XML Import with Delete Action: " + header.HumanReadableName, email.Subject);
			string expectedBody = string.Format(@"<br />
<strong>Delete Request for <a href=""{0}"">{1}</a></strong><br />
<br />
This ISF Job has been deleted by the client.<br />Please send a 'Delete' message to Customs if needed.
<br />", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.ImporterSecurityFiling, headerMatch3.PK.ToGuid()), headerMatch3.HumanReadableName);
			AssertContains(expectedBody, email.Body);
			AssertEquals(1, email.Recipients.Count);
			AssertEquals(staffZ1.GS_EmailAddress, email.Recipients[0].Email);
			xmlISF.TransactionNumber = ZString.Empty;
			notify.Clear();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header);
			notificationMessage = notify.AsString;
			string noISFMatchingJobReference = string.Format(ImporterSecurityFilingDataAdapter.NoISFMatchingJobReference, "Delete");
			AssertContains(noISFMatchingJobReference, notificationMessage);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			xmlISF.JobReference = headerMatch3.BF_JobReference;
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNotNull(header);
			AssertEquals(headerMatch3, header);
			notificationMessage = notify.AsString;
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("ISF XML Import with Delete Action: " + header.HumanReadableName, email.Subject);
			AssertContains(expectedBody, email.Body);
			AssertEquals(1, email.Recipients.Count);
			AssertEquals(staffZ1.GS_EmailAddress, email.Recipients[0].Email);
			xmlISF.JobReference = ZString.Empty;
			importerOfRecord.Number = "EIN6688544";
			notify.Clear();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header);
			notificationMessage = notify.AsString;
			string noISFMatchingEitherHouseBillOrOreanBill = string.Format(ImporterSecurityFilingDataAdapter.NoISFMatchingEitherHouseBillOrOreanBill, "Delete");
			AssertContains(noISFMatchingEitherHouseBillOrOreanBill, notificationMessage);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			importerOfRecord.Number = "EIN2342342";
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header);
			notificationMessage = notify.AsString;
			AssertNotContains(noISFMatchingEitherHouseBillOrOreanBill, notificationMessage);
			string multipleISFMatchingEitherHouseBillOrOreanBill = string.Format(ImporterSecurityFilingDataAdapter.MultipleISFMatchingEitherHouseBillOrOreanBill, "Delete the existing data");
			AssertContains(multipleISFMatchingEitherHouseBillOrOreanBill, notificationMessage);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			headerMatch1.BF_SystemCreateTimeUtc = ZDateTime.Today.AddDays(ImporterSecurityFilingDataAdapter.NoOfDaysOldAllowedInMatchingBill - 1);
			headerMatch2.BF_SystemCreateTimeUtc = ZDateTime.Today.AddDays(ImporterSecurityFilingDataAdapter.NoOfDaysOldAllowedInMatchingBill - 1);
			Factory.Save();
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNotNull(header);
			AssertEquals(headerMatch3, header);
			notificationMessage = notify.AsString;
			AssertNotContains(multipleISFMatchingEitherHouseBillOrOreanBill, notificationMessage);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("ISF XML Import with Delete Action: " + header.HumanReadableName, email.Subject);
			AssertContains(expectedBody, email.Body);
			AssertEquals(1, email.Recipients.Count);
			AssertEquals(staffZ1.GS_EmailAddress, email.Recipients[0].Email);
			headerMatch3.BF_CustomsStatus = MessageStatusList.Codes.AwaitingISFAdd;
			notify.Clear();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header);
			notificationMessage = notify.AsString;
			string pendingCustomsResponseJobExist = string.Format(ImporterSecurityFilingDataAdapter.PendingCustomsResponseJobExist, "Delete");
			AssertContains(pendingCustomsResponseJobExist, notificationMessage);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			headerMatch3.BF_CustomsStatus = MessageStatusList.Codes.ClearISFDelete;
			notify.Clear();
			header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNull(header);
			notificationMessage = notify.AsString;
			AssertNotContains(pendingCustomsResponseJobExist, notificationMessage);
			string customsDeletedJobExist = string.Format(ImporterSecurityFilingDataAdapter.CustomsDeletedJobExist, "Delete");
			AssertContains(customsDeletedJobExist, notificationMessage);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		public void TestImportDeleteActionTypeUseHeaderRegistryDetails()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "Z!1";
			company1.GC_Name = "DUMMY COMPANY";
			company1.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "Z!1";
			branch1.GB_BranchName = "DUMMY BRANCH";
			branch1.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			CusISFHeader headerMatch = Factory.New<CusISFHeader>();
			headerMatch.BF_CustomsReference = "MTC323456789";
			headerMatch.BF_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			headerMatch.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			headerMatch.BF_ImporterCode = "EIN2342342";
			headerMatch.BF_HouseBill = "BM12345678";
			headerMatch.BF_GB = branch1.PK;
			GlbGroup groupZZ1 = Factory.New<GlbGroup>();
			groupZZ1.GG_Code = "ZZ1";
			GlbStaff staffZ1 = groupZZ1.Staff.AddNew();
			staffZ1.GS_Code = "Z1";
			staffZ1.GS_LoginName = "z1";
			staffZ1.GS_EmailAddress = "dong@pretend.email.com";
			Factory.Save();
			ISFRegistry.Instance.ImporterSecurityFilingXMLImportNotificationGroup.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, groupZZ1.PK.ToGuid());
			Xsd.ISF xmlISF = new Xsd.ISF();
			xmlISF.Action.Type = Xsd.ISFActionType.Delete;
			xmlISF.Action.TypeSpecified = true;
			Xsd.RegistrationNumber importerOfRecord = new Xsd.RegistrationNumber();
			importerOfRecord.IsSpecified = true;
			importerOfRecord.Number = "EIN2342342";
			importerOfRecord.NumberType = Xsd.RegistrationNumberTypes.EIN;
			xmlISF.ImporterOfRecord.Item = importerOfRecord;
			xmlISF.TransactionNumber = "MTC323456789";
			xmlISF.JobReference = "ISFB2324232";
			Xsd.ISFReferenceID xmlBill1 = xmlISF.ReferenceIDs.AddNew();
			xmlBill1.IsSpecified = true;
			xmlBill1.Number = "BM12345678";
			xmlBill1.Type = Xsd.ISFReferenceIDType.BM;
			Xsd.ISFReferenceID xmlBill2 = xmlISF.ReferenceIDs.AddNew();
			xmlBill2.IsSpecified = true;
			xmlBill2.Number = "BMM1234567";
			xmlBill2.Type = Xsd.ISFReferenceIDType.BM;
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			ImporterSecurityFilingDataAdapter adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			var header = adapter.CreateOrUpdateFromValueObject(xmlISF, context);
			AssertNotNull(header);
			AssertEquals(headerMatch, header);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("ISF XML Import with Delete Action: " + header.HumanReadableName, email.Subject);
			string expectedBody = string.Format(@"<br />
<strong>Delete Request for <a href=""{0}"">{1}</a></strong><br />
<br />
This ISF Job has been deleted by the client.<br />Please send a 'Delete' message to Customs if needed.
<br />", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.ImporterSecurityFiling, headerMatch.PK.ToGuid()), headerMatch.HumanReadableName);
			AssertContains(expectedBody, email.Body);
			AssertEquals(1, email.Recipients.Count);
			AssertEquals(staffZ1.GS_EmailAddress, email.Recipients[0].Email);
		}

		public void TestExportAsAXmlAttachmentForAfterCSMS09000148Changes()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_JobReference = "ISF2342343";
			header.BF_CustomsReference = "XS232342";
			MQEDIMessage message = (MQEDIMessage)header.Messages.AddNew(typeof(MQEDIMessage));
			message.EM_ApplicationCode = US.Business.MQEDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = US.Business.MQEDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			message.EM_MessageNum = "~150000";
			message.EM_MessageText = "B018888XJ5SN                                               ~150000              " +
				"SF10103A  DUN4684446465             11               SCASBONDNUMBER0123456789Y  " +
				"SF90  308CONSOLIDATOR NAME/ADDRESS REQUIRED                                     " +
				"SF90  303CONSIGNEE NUMBER REQUIRED                                              " +
				"SF15BMBM123456789                                                               " +
				"SF15OBOB123456789                                                               " +
				"SF20SBNSB123456789                                                              " +
				"SF20MB MB123456789                                                              " +
				"SF20V1 V1123456789                                                              " +
				"SF206B 6B123456789                                                              " +
				"SF2543TURE234323         14050                                                  " +
				"SF90  252INVALID EQUIPMENT DESCRIPTION CODE                                     " +
				"SF30BY IMPORTER AUSTRALIAN COMPANY        EI                                    " +
				"SF90  315INVALID ENTITY IDENTIFIER                                              " +
				"SF30MF MANACCOM PTY LTD                   EI                                    " +
				"SF90  315INVALID ENTITY IDENTIFIER                                              " +
				"SF401234568844AU                                                                " +
				"SF90  404INVALID HTS CODE                                                       " +
				"SF30MF MAINFREIGHT INTERNATIONAL             65007252333                        " +
				"SF401042334668US                                                                " +
				"SF90  404INVALID HTS CODE                                                       " +
				"SF9001   SECURITY FILING REJECTED                                               " +
				"Y  8888XJ5SN00021";
			ImporterSecurityFilingDataAdapter adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			NotificationBuffer notify = new NotificationBuffer();
			AttachmentDef attachment = adapter.ExportAsAXmlAttachment(header, "ISF_VALIDATION.XML", new ValueObjectExportContext(notify));
			AssertEquals("ISF_VALIDATION.XML", attachment.DisplayName);
			ZString xmlData = Encoding.ASCII.GetString(attachment.Data);
			AssertContains("<JobReference>ISF2342343</JobReference>", xmlData);
			AssertContains("<ValidationResponse>", xmlData);
			AssertContains("Ocean Bill: Either a House Bill (with optional Master Bill) Or an Ocean Bill must be specified but not both.", xmlData);
			attachment = adapter.ExportAsAXmlAttachment(header, "ISF_CUSTOMS.XML", new ValueObjectExportContext(notify));
			AssertEquals("ISF_CUSTOMS.XML", attachment.DisplayName);
			xmlData = Encoding.ASCII.GetString(attachment.Data);
			string expectedXmlMessage = @"<CustomsResponse>
            <MessageTypeCode>01</MessageTypeCode>
            <RawFormat>B018888XJ5SN                                               ~150000              SF10103A  DUN4684446465             11               SCASBONDNUMBER0123456789Y  SF90  308CONSOLIDATOR NAME/ADDRESS REQUIRED                                     SF90  303CONSIGNEE NUMBER REQUIRED                                              SF15BMBM123456789                                                               SF15OBOB123456789                                                               SF20SBNSB123456789                                                              SF20MB MB123456789                                                              SF20V1 V1123456789                                                              SF206B 6B123456789                                                              SF2543TURE234323         14050                                                  SF90  252INVALID EQUIPMENT DESCRIPTION CODE                                     SF30BY IMPORTER AUSTRALIAN COMPANY        EI                                    SF90  315INVALID ENTITY IDENTIFIER                                              SF30MF MANACCOM PTY LTD                   EI                                    SF90  315INVALID ENTITY IDENTIFIER                                              SF401234568844AU                                                                SF90  404INVALID HTS CODE                                                       SF30MF MAINFREIGHT INTERNATIONAL             65007252333                        SF401042334668US                                                                SF90  404INVALID HTS CODE                                                       SF9001   SECURITY FILING REJECTED                                               Y  8888XJ5SN00021</RawFormat>
            <InterpretedFormat>
              <MessageBlock Name=""ISFSF10"">
                <MessageField Name=""ISFSubmissionType"">1</MessageField>
                <MessageField Name=""ShipmentTypeCode"">03</MessageField>
                <MessageField Name=""ActionCode"">A</MessageField>
                <MessageField Name=""ActionReasonCode"" />
                <MessageField Name=""ISFImporterNumberQualifier"">DUN</MessageField>
                <MessageField Name=""ISFImporterNumber"">4684446465</MessageField>
                <MessageField Name=""DateOfBirth"" />
                <MessageField Name=""ModeOfTransportationCode"">11</MessageField>
                <MessageField Name=""ISFTransactionNumber"" />
                <MessageField Name=""SCACIdentifier"">SCAS</MessageField>
                <MessageField Name=""BondHolder"">BONDNUMBER01234</MessageField>
                <MessageField Name=""BondActivityCode"">56</MessageField>
                <MessageField Name=""BondType"">7</MessageField>
                <MessageField Name=""CountryOfIssuance"" />
              </MessageBlock>
              <MessageBlock Name=""ISFSF90"">
                <MessageField Name=""MessageTypeCode"" />
                <MessageField Name=""ErrorCode"">308</MessageField>
                <MessageField Name=""NarrativeMessageText"">CONSOLIDATOR NAME/ADDRESS REQUIRED</MessageField>
              </MessageBlock>
              <MessageBlock Name=""ISFSF90"">
                <MessageField Name=""MessageTypeCode"" />
                <MessageField Name=""ErrorCode"">303</MessageField>
                <MessageField Name=""NarrativeMessageText"">CONSIGNEE NUMBER REQUIRED</MessageField>
              </MessageBlock>
              <MessageBlock Name=""ISFSF15"">
                <MessageField Name=""CodeQualifier"">BM</MessageField>
                <MessageField Name=""ShipmentReferenceIdentifier"">BM123456789</MessageField>
              </MessageBlock>
              <MessageBlock Name=""ISFSF15"">
                <MessageField Name=""CodeQualifier"">OB</MessageField>
                <MessageField Name=""ShipmentReferenceIdentifier"">OB123456789</MessageField>
              </MessageBlock>
              <MessageBlock Name=""ISFSF20"">
                <MessageField Name=""ReferenceIdentifierQualifier"">SBN</MessageField>
                <MessageField Name=""ReferenceIdentifier"">SB123456789</MessageField>
              </MessageBlock>
              <MessageBlock Name=""ISFSF20"">
                <MessageField Name=""ReferenceIdentifierQualifier"">MB</MessageField>
                <MessageField Name=""ReferenceIdentifier"">MB123456789</MessageField>
              </MessageBlock>
              <MessageBlock Name=""ISFSF20"">
                <MessageField Name=""ReferenceIdentifierQualifier"">V1</MessageField>
                <MessageField Name=""ReferenceIdentifier"">V1123456789</MessageField>
              </MessageBlock>
              <MessageBlock Name=""ISFSF20"">
                <MessageField Name=""ReferenceIdentifierQualifier"">6B</MessageField>
                <MessageField Name=""ReferenceIdentifier"">6B123456789</MessageField>
              </MessageBlock>
              <MessageBlock Name=""ISFSF25"">
                <MessageField Name=""EquipmentDescriptionCode"">43</MessageField>
                <MessageField Name=""EquipmentInitial"">TURE</MessageField>
                <MessageField Name=""EquipmentNumber"">234323</MessageField>
                <MessageField Name=""EquipmentNumberCheckDigit"">1</MessageField>
                <MessageField Name=""EquipmentSizeTypeCode"">4050</MessageField>
              </MessageBlock>
              <MessageBlock Name=""ISFSF90"">
                <MessageField Name=""MessageTypeCode"" />
                <MessageField Name=""ErrorCode"">252</MessageField>
                <MessageField Name=""NarrativeMessageText"">INVALID EQUIPMENT DESCRIPTION CODE</MessageField>
              </MessageBlock>
              <MessageBlock Name=""ISFSF30"">
                <MessageField Name=""EntityCode"">BY</MessageField>
                <MessageField Name=""EntityName"">IMPORTER AUSTRALIAN COMPANY</MessageField>
                <MessageField Name=""EntityIdentifierQualifier"">EI</MessageField>
                <MessageField Name=""EntityIdentifier"" />
                <MessageField Name=""CountryCode"" />
                <MessageField Name=""DOB"" />
              </MessageBlock>
              <MessageBlock Name=""ISFSF90"">
                <MessageField Name=""MessageTypeCode"" />
                <MessageField Name=""ErrorCode"">315</MessageField>
                <MessageField Name=""NarrativeMessageText"">INVALID ENTITY IDENTIFIER</MessageField>
              </MessageBlock>
              <MessageBlock Name=""ISFSF30"">
                <MessageField Name=""EntityCode"">MF</MessageField>
                <MessageField Name=""EntityName"">MANACCOM PTY LTD</MessageField>
                <MessageField Name=""EntityIdentifierQualifier"">EI</MessageField>
                <MessageField Name=""EntityIdentifier"" />
                <MessageField Name=""CountryCode"" />
                <MessageField Name=""DOB"" />
              </MessageBlock>
              <MessageBlock Name=""ISFSF90"">
                <MessageField Name=""MessageTypeCode"" />
                <MessageField Name=""ErrorCode"">315</MessageField>
                <MessageField Name=""NarrativeMessageText"">INVALID ENTITY IDENTIFIER</MessageField>
              </MessageBlock>
              <MessageBlock Name=""ISFSF40"">
                <MessageField Name=""HarmonizedNumber"">1234568844</MessageField>
                <MessageField Name=""CountryOfOrigin"">AU</MessageField>
              </MessageBlock>
              <MessageBlock Name=""ISFSF90"">
                <MessageField Name=""MessageTypeCode"" />
                <MessageField Name=""ErrorCode"">404</MessageField>
                <MessageField Name=""NarrativeMessageText"">INVALID HTS CODE</MessageField>
              </MessageBlock>
              <MessageBlock Name=""ISFSF30"">
                <MessageField Name=""EntityCode"">MF</MessageField>
                <MessageField Name=""EntityName"">MAINFREIGHT INTERNATIONAL</MessageField>
                <MessageField Name=""EntityIdentifierQualifier"" />
                <MessageField Name=""EntityIdentifier"">65007252333</MessageField>
                <MessageField Name=""CountryCode"" />
                <MessageField Name=""DOB"" />
              </MessageBlock>
              <MessageBlock Name=""ISFSF40"">
                <MessageField Name=""HarmonizedNumber"">1042334668</MessageField>
                <MessageField Name=""CountryOfOrigin"">US</MessageField>
              </MessageBlock>
              <MessageBlock Name=""ISFSF90"">
                <MessageField Name=""MessageTypeCode"" />
                <MessageField Name=""ErrorCode"">404</MessageField>
                <MessageField Name=""NarrativeMessageText"">INVALID HTS CODE</MessageField>
              </MessageBlock>
              <MessageBlock Name=""ISFSF90"">
                <MessageField Name=""MessageTypeCode"">01</MessageField>
                <MessageField Name=""ErrorCode"" />
                <MessageField Name=""NarrativeMessageText"">SECURITY FILING REJECTED</MessageField>
              </MessageBlock>
            </InterpretedFormat>
          </CustomsResponse>";
			AssertContains(expectedXmlMessage, xmlData);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWithMissingProductCode()
		{
			var adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			var dataImporter = new XmlDataImporter(adapter);
			var notification = new NotificationBuffer();
			var query = new ZQuery(CusISFHeaderSchema.BF_ImporterCodeType, OrgCusCode.CodeTypes.PassportID);
			query.AddToFilter(CusISFHeaderSchema.BF_ImporterCode, "PASZZZ23432ZZZ");
			AssertNull(Factory.LoadTop1<CusISFHeader>(query));
			var fileName = BaseTestFilePath + "ImportWithMissingProductImporterSecurityFiling.xml";
			dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			AssertContains("No data imported as invalid match", "Product Code not found for the Importer: DUMMYTESTZZZPART", notification.AsString);
			var header = Factory.LoadTop1<CusISFHeader>(query);
			var line = new List<CusISFLine>(header.Lines.Find(new ZQuery(CusISFLineSchema.BL_HarmonisedNum, "30308320")))[0];
			AssertEquals("AU", line.BL_RN_NKGoodsOrigin);
			AssertEquals(ZGuid.Empty, line.BL_OP);
			AssertEquals("DUMMYTESTZZZPART", line.BL_TextProductCode);
		}

		[TestDate(2009, 6, 12)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportISF5Type()
		{
			var bizObjSample = GetFullyPopulatedBizObjSample();
			var header = bizObjSample.BizObj;
			header.BF_JobReference = "ISF2342343";
			header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			header.BF_ShipmentType = ShipmentTypeList.Codes.StandardOrRegularFilings;
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.CompliantTransaction;
			header.BF_BondNumberOrHolder = "123-12-1234";
			header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ISFBond16;
			header.BF_BondType = Enterprise.Customs.US.Business.ImporterBondTypeList.Codes.SingleTransactionBond;
			header.BF_SuretyCode = "791";
			header.BF_BondReferenceNumber = "BD323423";
			Factory.Save();
			var adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			var notify = new NotificationBuffer();
			var attachment = adapter.ExportAsAXmlAttachment(header, "ISF_VALIDATION.XML", new ValueObjectExportContext(notify));
			AssertEquals("ISF_VALIDATION.XML", attachment.DisplayName);
			var xmlData = Encoding.ASCII.GetString(attachment.Data);
			var expectedXML = @"
      <ISF>
        <Action ReasonCode=""CT"">
          <Messaging />
        </Action>
        <ImporterOfRecord>
          <RegistrationNumber>
            <CountryOfRegistration>US</CountryOfRegistration>
            <NumberType>PAS</NumberType>
            <Number>PAS1233343</Number>
          </RegistrationNumber>
          <DateOfBirth>1980-02-03</DateOfBirth>
          <CountryOfIssue>US</CountryOfIssue>
        </ImporterOfRecord>
        <SubmissionType>ISF5</SubmissionType>
        <ShipmentType>01</ShipmentType>
        <TransportMode>10</TransportMode>
        <CarrierSCAC>SVSM</CarrierSCAC>
        <OwnerReference>MYREF</OwnerReference>
        <BondActivityCode>16</BondActivityCode>
        <BondType>9</BondType>
        <BondHolder>123-12-1234</BondHolder>
        <ForeignPortOfUnlading Country=""United States"" City=""Los Angeles"">USLAX</ForeignPortOfUnlading>
        <PlaceOfDelivery Country=""United States"" City=""Los Angeles"">USLAX</PlaceOfDelivery>
        <Parties>
          <ISF5>
            <BookingParties>
              <BookingParty AddressType=""BKD"">
                <AddressLine1>BOOKING PARTY ADDRESS 1</AddressLine1>
                <AddressLine2>BOOKING PARTY ADDRESS 2</AddressLine2>
                <CityOrSuburb>MELBOURNE</CityOrSuburb>
                <StateOrProvince>VIC</StateOrProvince>
                <PostCode>3014</PostCode>
                <TelephoneNumbers>
                  <TelephoneNumber NumberType=""Business"">+61 (3) 8456 6855</TelephoneNumber>
                  <TelephoneNumber NumberType=""Fax"">+61 (3) 8456 6846</TelephoneNumber>
                </TelephoneNumbers>
                <Email>WENDY@BUILDER.COM</Email>
                <CompanyName>BOOKING PARTY COMPANY</CompanyName>
                <CountryCode>AU</CountryCode>
                <ContactName>WENDY THE BUILDER</ContactName>
              </BookingParty>
            </BookingParties>
            <ShipToParties>
              <ShipTo AddressType=""STP"">
                <AddressLine1>SHIP TO PARTY ADDRESS 1</AddressLine1>
                <AddressLine2>SHIP TO PARTY ADDRESS 2</AddressLine2>
                <CityOrSuburb>MELBOURNE</CityOrSuburb>
                <StateOrProvince>VIC</StateOrProvince>
                <PostCode>3014</PostCode>
                <TelephoneNumbers>
                  <TelephoneNumber NumberType=""Business"">+61 (3) 8456 6855</TelephoneNumber>
                  <TelephoneNumber NumberType=""Fax"">+61 (3) 8456 6846</TelephoneNumber>
                </TelephoneNumbers>
                <Email>SHIPTO@BUILDER.COM</Email>
                <CompanyName>SHIP TO PARTY COMPANY</CompanyName>
                <CountryCode>AU</CountryCode>
                <ContactName>SHIP TO THE BUILDER</ContactName>
              </ShipTo>
              <ShipTo AddressType=""STP"">
                <AddressLine1>SHIP2 TO PARTY ADDRESS 1</AddressLine1>
                <AddressLine2>SHIP2 TO PARTY ADDRESS 2</AddressLine2>
                <CityOrSuburb>MELBOURNE</CityOrSuburb>
                <StateOrProvince>VIC</StateOrProvince>
                <PostCode>3014</PostCode>
                <TelephoneNumbers>
                  <TelephoneNumber NumberType=""Business"">+61 (3) 8456 6855</TelephoneNumber>
                  <TelephoneNumber NumberType=""Fax"">+61 (3) 8456 6846</TelephoneNumber>
                </TelephoneNumbers>
                <Email>SHIP2TO@BUILDER.COM</Email>
                <CompanyName>SHIP2 TO PARTY COMPANY</CompanyName>
                <CountryCode>AU</CountryCode>
                <ContactName>SHIP2 TO THE BUILDER</ContactName>
              </ShipTo>
            </ShipToParties>
          </ISF5>
        </Parties>
        <ReferenceIDs>
          <ReferenceID>
            <Type>BM</Type>
            <Number>HB1232112</Number>
          </ReferenceID>
          <ReferenceID>
            <Type>MB</Type>
            <Number>MB1232112</Number>
          </ReferenceID>
          <ReferenceID>
            <Type>6B</Type>
            <Number>XJF69783256</Number>
          </ReferenceID>
          <ReferenceID>
            <Type>V1</Type>
            <Number>791</Number>
          </ReferenceID>
          <ReferenceID>
            <Type>BRN</Type>
            <Number>BD323423</Number>
          </ReferenceID>
        </ReferenceIDs>
        <Containers>
          <Container>
            <DescriptionCode>20</DescriptionCode>
            <ContainerNumber>TURE2323333</ContainerNumber>
            <ISOType>20FR</ISOType>
          </Container>
          <Container>
            <DescriptionCode>40</DescriptionCode>
            <ContainerNumber>TURE1110111</ContainerNumber>
            <ISOType>40FR</ISOType>
          </Container>
        </Containers>
        <Lines>
          <Line>
            <HTSNumber>10108120</HTSNumber>
            <CountryOfOrigin>AU</CountryOfOrigin>
            <CustomValues>
              <CustomValue Name=""CustomAttribute1"" Type=""String"">LNEATTRIB1</CustomValue>
              <CustomValue Name=""CustomAttribute2"" Type=""String"">LNEATTRIB2</CustomValue>
            </CustomValues>
          </Line>
          <Line>
            <HTSNumber>20208220</HTSNumber>
            <CountryOfOrigin>AU</CountryOfOrigin>
          </Line>
          <Line>
            <ProductCode>DUMMYTEST1PART</ProductCode>
            <HTSNumber>30308320</HTSNumber>
            <CountryOfOrigin>AU</CountryOfOrigin>
          </Line>
          <Line>
            <HTSNumber>40408420</HTSNumber>
            <CountryOfOrigin>NZ</CountryOfOrigin>
          </Line>
        </Lines>
        <JobReference>ISF2342343</JobReference>
        <Status>
          <MessageStatus Description=""Not Sent"">NOT</MessageStatus>
          <ValidationResponse></ValidationResponse>
        </Status>
        <CustomValues>
          <CustomValue Name=""CustomAttribute1"" Type=""String"">HDRATTRIB1</CustomValue>
          <CustomValue Name=""CustomAttribute2"" Type=""String"">HDRATTRIB2</CustomValue>
        </CustomValues>
        <Routings>
          <Routing>
            <TransportMode>SEA</TransportMode>
            <PortOfLoading>
              <Port Country=""Australia"" City=""Sydney"">AUSYD</Port>
              <EstimatedDateTime>2009-03-10T00:00:00</EstimatedDateTime>
              <ActualDateTime>2009-03-11T00:00:00</ActualDateTime>
            </PortOfLoading>
            <PortOfDischarge>
              <Port Country=""United States"" City=""Chicago"">USCHI</Port>
              <EstimatedDateTime>2009-04-10T00:00:00</EstimatedDateTime>
              <ActualDateTime>2009-04-11T00:00:00</ActualDateTime>
            </PortOfDischarge>
            <TransportType>MainVessel</TransportType>
            <LegOrderNumber>1</LegOrderNumber>
            <Vessel>
              <ETD>2009-03-10T00:00:00</ETD>
              <ETA>2009-04-10T00:00:00</ETA>
              <ATD>2009-03-11T00:00:00</ATD>
              <ATA>2009-04-11T00:00:00</ATA>
              <VesselName>VESSEL WITH COUNTRY</VesselName>
              <LloydsNo>8181812</LloydsNo>
              <VoyageNo>434</VoyageNo>
            </Vessel>
          </Routing>
          <Routing>
            <TransportMode>RAI</TransportMode>
            <PortOfLoading>
              <Port Country=""United States"" City=""Chicago"">USCHI</Port>
              <EstimatedDateTime>2009-04-10T00:00:00</EstimatedDateTime>
              <ActualDateTime>2009-04-11T00:00:00</ActualDateTime>
            </PortOfLoading>
            <PortOfDischarge>
              <Port Country=""United States"" City=""Los Angeles"">USLAX</Port>
              <EstimatedDateTime>2009-05-10T00:00:00</EstimatedDateTime>
              <ActualDateTime>2009-05-11T00:00:00</ActualDateTime>
            </PortOfDischarge>
            <TransportType>OnForwarding</TransportType>
            <LegOrderNumber>2</LegOrderNumber>
            <RoadRailFlight>
              <ETD>2009-04-10T00:00:00</ETD>
              <ETA>2009-05-10T00:00:00</ETA>
              <ATD>2009-04-11T00:00:00</ATD>
              <ATA>2009-05-11T00:00:00</ATA>
              <FlightNoJourneyNoTruckRegNo>TRAIN323</FlightNoJourneyNoTruckRegNo>
            </RoadRailFlight>
          </Routing>
        </Routings>
        <Events>";
			int validationStart = xmlData.IndexOf("<ValidationResponse>", StringComparison.Ordinal) + "<ValidationResponse>".Length;
			int validationEnd = xmlData.IndexOf("</ValidationResponse>", StringComparison.Ordinal);
			if (validationStart >= "<ValidationResponse>".Length && validationEnd >= validationStart)
			{
				xmlData = xmlData.Remove(validationStart, validationEnd - validationStart);
			}

			AssertContains(expectedXML, xmlData);
			expectedXML = @"          <Event>
            <Source>CusISFHeader</Source>
            <Code>ADD</Code>
            <CodeDescription>Added a record to the system</CodeDescription>
            <DateTime>2009-06-12T00:00:00+02:00</DateTime>
            <PostedDateTime>2009-06-12T00:00:00Z</PostedDateTime>
            <User>" + GlbStaff.CurrentUser.GS_Code + @"</User>
            <UserName>" + GlbStaff.CurrentUser.GS_FullName + " (" + GlbStaff.CurrentUser.GS_Code + @")</UserName>
            <IsEstimatedDate>false</IsEstimatedDate>
          </Event>";
			AssertContains(expectedXML, xmlData);
			expectedXML = @"          <Event>
            <Source>CusISFBill</Source>
            <Code>ADD</Code>
            <CodeDescription>Added a record to the system</CodeDescription>
            <DateTime>2009-06-12T00:00:00+02:00</DateTime>
            <PostedDateTime>2009-06-12T00:00:00Z</PostedDateTime>
            <User>" + GlbStaff.CurrentUser.GS_Code + @"</User>
            <UserName>" + GlbStaff.CurrentUser.GS_FullName + " (" + GlbStaff.CurrentUser.GS_Code + @")</UserName>
            <IsEstimatedDate>false</IsEstimatedDate>
          </Event>";
			AssertContains(expectedXML, xmlData);
		}

		public void TestBondDetailsAreNotOverrideIfNotSpecified()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG" + new Random().Next(1000000).ToString();
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123-12-5689", Core.Constants.CountryCodes.UnitedStates);
			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(org);
			CusBondDetailCollection bondDetails = wrapper.BondDetails;
			CusBondDetail bondData = bondDetails.AddNew();
			bondData.PW_ActivityCode = ActivityCodeList.Codes._16;
			bondData.PW_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			bondData.PW_SuretyCode = "795";
			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddMonths(-1);
			bondData.PW_BondExpiryDate = ZDateTime.Today.AddYears(1);
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_JobReference = "ISF2342343";
			header.BF_CustomsReference = "XS232342";
			header.BF_ShipmentType = ShipmentTypeList.Codes.InternationalMailShipments; // need a shipment type not valid with bond details
			header.BF_OH_Importer = org.PK;
			header.BF_BondNumberOrHolder = "123-12-1234";
			header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.CustodianOfBondedMerchandise;
			header.BF_BondType = Enterprise.Customs.US.Business.ImporterBondTypeList.Codes.ContinuousBond;
			header.BF_SuretyCode = "791";
			header.BF_BondReferenceNumber = "BD323423";
			Factory.Save();
			ImporterSecurityFilingDataAdapter adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			NotificationBuffer notify = new NotificationBuffer();
			Xsd.ISF value = adapter.ExportToValueObject(header, new ValueObjectExportContext(notify));
			value.Action.Type = Xsd.ISFActionType.Replace;
			notify.Clear();
			adapter.ImportFromValueObject(header, value, new ValueObjectImportContext(Factory, notify));
			AssertEquals("123-12-1234", header.BF_BondNumberOrHolder);
			AssertEquals(ISFBondActivityCodeList.Codes.CustodianOfBondedMerchandise, header.BF_BondActivityCode);
			AssertEquals(Enterprise.Customs.US.Business.ImporterBondTypeList.Codes.ContinuousBond, header.BF_BondType);
			AssertEquals("791", header.BF_SuretyCode);
			AssertEquals("BD323423", header.BF_BondReferenceNumber);
			AssertNull(header.ReferenceDatas[Common.US.ISF.BillTypeList.Codes.SuretyCode, "795"]);
			value.ShipmentType = Xsd.ISFShipmentType.Item01;
			notify.Clear();
			adapter.ImportFromValueObject(header, value, new ValueObjectImportContext(Factory, notify));
			AssertEquals("123-12-1234", header.BF_BondNumberOrHolder);
			AssertEquals(ISFBondActivityCodeList.Codes.CustodianOfBondedMerchandise, header.BF_BondActivityCode);
			AssertEquals(Enterprise.Customs.US.Business.ImporterBondTypeList.Codes.ContinuousBond, header.BF_BondType);
			AssertEquals("791", header.BF_SuretyCode);
			AssertEquals("BD323423", header.BF_BondReferenceNumber);
			AssertNull(header.ReferenceDatas[Common.US.ISF.BillTypeList.Codes.SuretyCode, "795"]);
			value.ShipmentType = Xsd.ISFShipmentType.Item01;
			value.BondHolderSpecified = false;
			value.BondHolder = "";
			value.BondTypeSpecified = false;
			value.BondActivityCodeSpecified = false;
			notify.Clear();
			adapter.ImportFromValueObject(header, value, new ValueObjectImportContext(Factory, notify));
			AssertEquals("123-12-5689", header.BF_BondNumberOrHolder);
			AssertEquals(ISFBondActivityCodeList.Codes.ISFBond16, header.BF_BondActivityCode);
			AssertEquals(Enterprise.Customs.US.Business.ImporterBondTypeList.Codes.SingleTransactionBond, header.BF_BondType);
			AssertEquals("791", header.BF_SuretyCode);
			AssertEquals("BD323423", header.BF_BondReferenceNumber);
			AssertNull(header.ReferenceDatas[Common.US.ISF.BillTypeList.Codes.SuretyCode, "795"]);
		}

		public void TestBillCustomsDetailsIsExported()
		{
			ImporterSecurityFilingDataAdapter adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBill bill1 = header.ReferenceDatas.AddNew();
			bill1.BB_BillType = Common.US.ISF.BillTypeList.Codes.HouseBillOfLading;
			bill1.BB_BillNum = "HB1";
			bill1.BB_CustomsStatus = DispositionCodeList.Codes.S2;
			CusISFBill bill2 = header.ReferenceDatas.AddNew();
			bill2.BB_BillType = Common.US.ISF.BillTypeList.Codes.HouseBillOfLading;
			bill2.BB_BillNum = "HB2";
			bill2.BB_CustomsStatus = DispositionCodeList.Codes.S1;
			bill2.BB_MatchDate = new ZDateTime(2010, 3, 10);
			var message = header.Messages.AddNew(typeof(MQEDIMessage));
			message.EM_ReceiveTransmit = US.Business.EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			Xsd.ISF xsdISF = adapter.ExportToValueObject(header, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals(true, xsdISF.Status.IsSpecified);
			AssertEquals(true, xsdISF.Status.CustomsResponse.IsSpecified);
			AssertEquals(true, xsdISF.Status.CustomsResponse.Bills.IsSpecified);
			AssertEquals(2, xsdISF.Status.CustomsResponse.Bills.Count);
			var xsdBill1 = xsdISF.Status.CustomsResponse.Bills[0];
			var xsdBill2 = xsdISF.Status.CustomsResponse.Bills[1];
			if (xsdBill1.Number == "HB2")
			{
				xsdBill1 = xsdISF.Status.CustomsResponse.Bills[1];
				xsdBill2 = xsdISF.Status.CustomsResponse.Bills[0];
			}

			AssertBillStatus(xsdBill1, "HB1", Xsd.ISFDispositionCode.S2, ZDate.Empty);
			AssertBillStatus(xsdBill2, "HB2", Xsd.ISFDispositionCode.S1, new ZDate(2010, 3, 10));
		}

		[TestDate(2009, 3, 1)]
		public void TestExportAndImportLowValueDetails()
		{
			var adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			var header = Factory.New<CusISFHeader>();
			header.BF_OceanBill = "SCDS034234233";
			header.BF_ShipmentType = ShipmentTypeList.Codes.Informal;
			header.BF_ShipmentSubType = ShipmentSubTypeList.Codes.InformalShipments;
			header.BF_EstimatedValue = 1000m;
			header.BF_EstimatedQuantity = 150;
			header.BF_EstimatedQuantityUQ = ShippingOrPackingingUnitList.Codes.Bucket;
			header.BF_EstimatedWeight = 200;
			header.BF_EstimatedWeightUQ = Core.Constants.Weight.Kilograms;
			var xsdISF = adapter.ExportToValueObject(header, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals(Xsd.ISFShipmentType.Item11, xsdISF.ShipmentType);
			AssertEquals(Xsd.ISFShipmentSubType.Item02, xsdISF.ShipmentSubType);
			AssertEquals(1000m, xsdISF.EstimatedValue);
			AssertEquals(150m, xsdISF.EstimatedQuantity.Value);
			AssertEquals(ShippingOrPackingingUnitList.Codes.Bucket, xsdISF.EstimatedQuantity.DimensionType);
			AssertEquals(200m, xsdISF.EstimatedWeight.Value);
			AssertEquals(Core.Constants.Weight.Kilograms, xsdISF.EstimatedWeight.DimensionType);
			header = Factory.New<CusISFHeader>();
			adapter.ImportFromValueObject(header, xsdISF, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals(ShipmentTypeList.Codes.Informal, header.BF_ShipmentType);
			AssertEquals(ShipmentSubTypeList.Codes.InformalShipments, header.BF_ShipmentSubType);
			AssertEquals(1000m, header.BF_EstimatedValue);
			AssertEquals(150, header.BF_EstimatedQuantity);
			AssertEquals(ShippingOrPackingingUnitList.Codes.Bucket, header.BF_EstimatedQuantityUQ);
			AssertEquals(200, header.BF_EstimatedWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, header.BF_EstimatedWeightUQ);
		}

		[TestDate(2009, 7, 1)]
		public override void TestExportToValueObject_ForFullyPopulatedBizO()
		{
			Assert(true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportedOverriddenJobDocAddresses_SetToMAN()
		{
			var importer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
			var adapter = (ImporterSecurityFilingDataAdapter)GetNewBizObjXmlDataAdapter();
			var dataImporter = new XmlDataImporter(adapter);
			string fileName = BaseTestFilePath + "TwoManufacturersOneBuyer.xml";
			dataImporter.ImportData(fileName, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			var header = Factory.LoadTop1<CusISFHeader>(new ZQuery(CusISFHeaderSchema.BF_OH_Importer, importer.PK));
			var line = header.Lines.FirstOrDefault();
			AssertEquals("Manufacturer doc address should be of status MAN", "MAN", line.ManufacturerDocAddress.E2_ValidationStatus);
			AssertEquals("Buyer doc address should be of status MAN", "MAN", header.BuyingParty.E2_ValidationStatus);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestExportToValueObject_ForEmptyBizO()
		{
			base.TestExportToValueObject_ForEmptyBizO();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestExportToValueObject_ForPopulatedBizObjWithEmptyFields()
		{
			base.TestExportToValueObject_ForPopulatedBizObjWithEmptyFields();
		}

		protected override string TestingCountry => null;

		protected override bool AllowDifferentImportContextFactory => true;

		protected override string ExpectedRootCollectionElementName => "ISFs";

		protected override string ExpectedRootElementName => "ISF";

		protected override ValueObjectDataAdapter<CusISFHeader, Xsd.ISF> GetNewBizObjXmlDataAdapter() => new ImporterSecurityFilingDataAdapter();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Base tests have SOURCE_CODE")]
		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new EntryFiler()
			{
				EntryFilerCode = "XJ5"
			});
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "8888");
			var uSCompany = Factory.New<GlbCompany>();
			uSCompany.GC_Code = "Z1Z";
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			uSCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var uSBranch = uSCompany.Branches.AddNew();
			uSBranch.GB_Code = "Z1Z";
			uSBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var header = Factory.New<CusISFHeader>();
			header.BF_GB = uSBranch.PK;
			return new BusinessObjectAndExpectedOutputFileName(header, BaseTestFilePath + "EmptyImporterSecurityFiling.xml", ValidationKind.None, "Empty ImporterSecurity Filing");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Base tests have SOURCE_CODE")]
		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample() => GetEmptyBizObjSample();

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var factory = new BusinessObjectFactory();
			var importer = factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER COMPANY";
			importer.OH_RL_NKClosestPort = "USCHI";
			importer.MainAddress.OA_Address1 = "IMPORTER ADDRESS 1";
			importer.MainAddress.OA_Address2 = "IMPORTER ADDRESS 2";
			importer.MainAddress.OA_City = "CHICAGO";
			importer.MainAddress.OA_State = "IL";
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "PAS1233343");
			var consolidator = factory.New<OrgHeader>();
			consolidator.OH_FullName = "CONSOLIDATOR COMPANY";
			consolidator.OH_RL_NKClosestPort = "USCHI";
			consolidator.MainAddress.OA_Address1 = "CONSOLIDATOR ADDRESS 1";
			consolidator.MainAddress.OA_Address2 = "CONSOLIDATOR ADDRESS 2";
			consolidator.MainAddress.OA_City = "CHICAGO";
			consolidator.MainAddress.OA_State = "IL";
			var manufacturer1 = factory.New<OrgHeader>();
			manufacturer1.OH_FullName = "MANUFACTURER1 COMPANY";
			manufacturer1.OH_RL_NKClosestPort = "AUSYD";
			manufacturer1.MainAddress.OA_Address1 = "MANUFACTURER1 ADDRESS 1";
			manufacturer1.MainAddress.OA_Address2 = "MANUFACTURER1 ADDRESS 2";
			manufacturer1.MainAddress.OA_City = "SYDNEY";
			manufacturer1.MainAddress.OA_State = "NSW";
			var part = (OrgSupplierPart)factory.New<Integration.Customs.US.IOrgSupplierPart>();
			part.OP_PartNum = "DUMMYTEST1PART";
			part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part.RelatedOrganisations.AddOrganisationIfNotExist(manufacturer1.PK, OrgPartRelation.RelationshipTypes.Supplier);
			var helper = new DeclarationTestHelper(factory);
			var vessel = helper.VesselWithCountry;
			var uSCompany = factory.New<GlbCompany>();
			uSCompany.GC_Code = "Z1Z";
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			uSCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var uSBranch = uSCompany.Branches.AddNew();
			uSBranch.GB_Code = "Z1Z";
			uSBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			factory.Save();
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new EntryFiler()
			{
				EntryFilerCode = "XJ5"
			});
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "8888");
			var header = Factory.New<CusISFHeader>();
			header.BF_GB = uSBranch.PK;
			header.BF_ImporterCode = ZString.Empty;
			header.BF_ImporterCodeType = ZString.Empty;
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_OH_Importer = importer.PK;
			header.BF_SuretyCode = "798";
			header.BF_OwnerReference = "MYREF";
			header.BF_HouseBill = "HB1232112";
			header.BF_ConsigneeCodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			header.BF_ConsigneeCode = "12-3456789XY";
			header.BF_CountryOfIssue = Core.Constants.CountryCodes.UnitedStates;
			header.BF_DateOfBirth = new ZDateTime(1980, 2, 3);
			header.BF_JobReference = ZString.Empty;
			header.BF_RL_NKPlaceOfDelivery = "USLAX";
			header.BF_RL_NKPortOfUnload = "USLAX";
			header.BF_SCAC = "SVSM";
			header.BF_ShipmentType = ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects;
			header.BF_TransportMode = Business.TransportModeCodes.Codes.OceanVesselNonContainerized;
			header.CustomAttribute1 = "HDRATTRIB1";
			header.CustomAttribute2 = "HDRATTRIB2";
			header.SetUserDefinedValue("Hello", new ZString("MyHello"));
			AddTransport(header.Transports, Core.Constants.TransportModes.Sea, Core.Constants.TransportPlanningType.MainVessel, vessel.RV_Code, "434", "AUSYD", "USCHI", new ZDateTime(2009, 3, 10), new ZDateTime(2009, 3, 11), new ZDateTime(2009, 4, 10), new ZDateTime(2009, 4, 11), 1);
			AddTransport(header.Transports, Core.Constants.TransportModes.Rail, Core.Constants.TransportPlanningType.OnForwarding, "", "TRAIN323", "USCHI", "USLAX", new ZDateTime(2009, 4, 10), new ZDateTime(2009, 4, 11), new ZDateTime(2009, 5, 10), new ZDateTime(2009, 5, 11), 2);
			AddBill(header, "MB1232112", BillTypeList.Codes.MasterBillOfLading);
			AddBill(header, "XJF69783256", BillTypeList.Codes.USCBPEntryNumber);
			header.SellingParty.E2_AddressOverride = true;
			header.SellingParty.E2_CompanyName = "SELLING COMPANY";
			header.SellingParty.E2_Address1 = "SELLING ADDRESS 1";
			header.SellingParty.E2_Address2 = "SELLING ADDRESS 2";
			header.SellingParty.E2_City = "SYDNEY";
			header.SellingParty.E2_Contact = "BOB THE BUILDER";
			header.SellingParty.E2_Email = "BOB@BUILDER.COM";
			header.SellingParty.E2_Fax = "+61 (2) 8456 6846";
			header.SellingParty.E2_Phone = "+61 (2) 8456 6855";
			header.SellingParty.E2_Postcode = "2214";
			header.SellingParty.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			header.SellingParty.E2_State = "NSW";
			header.SellingParty.E2_GovRegNumType = OrgCusCode.USACodeTypes.FIRMSCode;
			header.SellingParty.E2_GovRegNum = "ADDRESS1";
			header.SellingParty.E2_Mobile = "+61 403 112 456";
			var sellingParty2 = header.DocAddresses.FindOrCreateWithRequirement(header.ISFDocAddressRequirementProvider.GetOtherPartyISFDocAddressRequirement(DocAddressType.SellingParty, SubmissionTypeList.Codes.ISF10, true), 1);
			sellingParty2.E2_AddressOverride = true;
			sellingParty2.E2_CompanyName = "SELLING2 COMPANY";
			sellingParty2.E2_Address1 = "SELLING2 ADDRESS 1";
			sellingParty2.E2_Address2 = "SELLING2 ADDRESS 2";
			sellingParty2.E2_City = "SYDNEY";
			sellingParty2.E2_Contact = "BOB THE BUILDER";
			sellingParty2.E2_Email = "BOB@BUILDER.COM";
			sellingParty2.E2_Fax = "+61 (2) 8456 6846";
			sellingParty2.E2_Phone = "+61 (2) 8456 6855";
			sellingParty2.E2_Postcode = "2214";
			sellingParty2.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			sellingParty2.E2_State = "NSW";
			sellingParty2.E2_GovRegNumType = OrgCusCode.USACodeTypes.FIRMSCode;
			sellingParty2.E2_GovRegNum = "ADDRESS1";
			sellingParty2.E2_Mobile = "+61 403 112 456";
			var buyingParty = header.BuyingParty;
			buyingParty.E2_AddressOverride = true;
			buyingParty.E2_CompanyName = "BUYING COMPANY";
			buyingParty.E2_Address1 = "BUYING ADDRESS 1";
			buyingParty.E2_Address2 = "BUYING ADDRESS 2";
			buyingParty.E2_City = "SYDNEY";
			buyingParty.E2_Contact = "BUYER THE BUILDER";
			buyingParty.E2_Email = "BUYER@BUILDER.COM";
			buyingParty.E2_Fax = "+61 (2) 6953 6846";
			buyingParty.E2_Phone = "+61 (2) 6953 6855";
			buyingParty.E2_Postcode = "2200";
			buyingParty.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			buyingParty.E2_State = "NSW";
			buyingParty.E2_GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			buyingParty.E2_GovRegNum = "912-21-456812031978";
			buyingParty.E2_Mobile = "+61 403 864 456";
			var buyingParty2 = header.DocAddresses.FindOrCreateWithRequirement(header.ISFDocAddressRequirementProvider.GetOtherPartyISFDocAddressRequirement(DocAddressType.BuyingParty, SubmissionTypeList.Codes.ISF10, true), 1);
			buyingParty2.E2_AddressOverride = true;
			buyingParty2.E2_CompanyName = "BUYING2 COMPANY";
			buyingParty2.E2_Address1 = "BUYING2 ADDRESS 1";
			buyingParty2.E2_Address2 = "BUYING2 ADDRESS 2";
			buyingParty2.E2_City = "SYDNEY";
			buyingParty2.E2_Contact = "BUYER2 THE BUILDER";
			buyingParty2.E2_Email = "BUYER2@BUILDER.COM";
			buyingParty2.E2_Fax = "+61 (2) 6953 6846";
			buyingParty2.E2_Phone = "+61 (2) 6953 6855";
			buyingParty2.E2_Postcode = "2200";
			buyingParty2.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			buyingParty2.E2_State = "NSW";
			buyingParty2.E2_GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			buyingParty2.E2_GovRegNum = "695-32-456823111987";
			buyingParty2.E2_Mobile = "+61 403 864 456";
			var stuffingLocation = header.StuffingLocation;
			stuffingLocation.E2_AddressOverride = true;
			stuffingLocation.E2_CompanyName = "STUFFING LOCATION COMPANY";
			stuffingLocation.E2_Address1 = "STUFFING LOCATION ADDRESS 1";
			stuffingLocation.E2_Address2 = "STUFFING LOCATION ADDRESS 2";
			stuffingLocation.E2_City = "CHICAGO";
			stuffingLocation.E2_Contact = "STUFFER THE BUILDER";
			stuffingLocation.E2_Email = "STUFFER@BUILDER.COM";
			stuffingLocation.E2_Postcode = "61022";
			stuffingLocation.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			stuffingLocation.E2_State = "RM";
			var stuffingLocation2 = header.DocAddresses.FindOrCreateWithRequirement(header.ISFDocAddressRequirementProvider.GetOtherPartyISFDocAddressRequirement(DocAddressType.ScheduledContainerStuffingLocation, SubmissionTypeList.Codes.ISF10, false), 1);
			stuffingLocation2.E2_AddressOverride = true;
			stuffingLocation2.E2_CompanyName = "STUFFING2 LOCATION COMPANY";
			stuffingLocation2.E2_Address1 = "STUFFING2 LOCATION ADDRESS 1";
			stuffingLocation2.E2_Address2 = "STUFFING2 LOCATION ADDRESS 2";
			stuffingLocation2.E2_City = "CHICAGO";
			stuffingLocation2.E2_Contact = "STUFFER THE BUILDER";
			stuffingLocation2.E2_Email = "STUFFER2@BUILDER.COM";
			stuffingLocation2.E2_Postcode = "61022";
			stuffingLocation2.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			stuffingLocation2.E2_State = "RM";
			var consolidatorDocAddress = header.Consolidator;
			consolidatorDocAddress.E2_OA_Address = consolidator.MainAddress.PK;
			var consolidator2 = header.DocAddresses.FindOrCreateWithRequirement(header.ISFDocAddressRequirementProvider.GetOtherPartyISFDocAddressRequirement(DocAddressType.Consolidator, SubmissionTypeList.Codes.ISF10, false), 1);
			consolidator2.E2_AddressOverride = true;
			consolidator2.E2_CompanyName = "CONSOLIDATOR2 LOCATION COMPANY";
			consolidator2.E2_Address1 = "CONSOLIDATOR2 LOCATION ADDRESS 1";
			consolidator2.E2_Address2 = "CONSOLIDATOR2 LOCATION ADDRESS 2";
			consolidator2.E2_City = "CHICAGO";
			consolidator2.E2_Contact = "CONSOLIDATOR2 THE BUILDER";
			consolidator2.E2_Email = "CONSOLIDATOR2@BUILDER.COM";
			consolidator2.E2_Postcode = "61022";
			consolidator2.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			consolidator2.E2_State = "RM";
			var bookingPartyDocAddress = header.BookingParty;
			bookingPartyDocAddress.E2_AddressOverride = true;
			bookingPartyDocAddress.E2_CompanyName = "BOOKING PARTY COMPANY";
			bookingPartyDocAddress.E2_Address1 = "BOOKING PARTY ADDRESS 1";
			bookingPartyDocAddress.E2_Address2 = "BOOKING PARTY ADDRESS 2";
			bookingPartyDocAddress.E2_City = "MELBOURNE";
			bookingPartyDocAddress.E2_Contact = "WENDY THE BUILDER";
			bookingPartyDocAddress.E2_Email = "WENDY@BUILDER.COM";
			bookingPartyDocAddress.E2_Fax = "+61 (3) 8456 6846";
			bookingPartyDocAddress.E2_Phone = "+61 (3) 8456 6855";
			bookingPartyDocAddress.E2_Postcode = "3014";
			bookingPartyDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			bookingPartyDocAddress.E2_State = "VIC";
			bookingPartyDocAddress.E2_Mobile = "+61 403 112 695";
			var shipToParty = header.MainShipToParty;
			shipToParty.E2_AddressOverride = true;
			shipToParty.E2_CompanyName = "SHIP TO PARTY COMPANY";
			shipToParty.E2_Address1 = "SHIP TO PARTY ADDRESS 1";
			shipToParty.E2_Address2 = "SHIP TO PARTY ADDRESS 2";
			shipToParty.E2_City = "MELBOURNE";
			shipToParty.E2_Contact = "SHIP TO THE BUILDER";
			shipToParty.E2_Email = "SHIPTO@BUILDER.COM";
			shipToParty.E2_Fax = "+61 (3) 8456 6846";
			shipToParty.E2_Phone = "+61 (3) 8456 6855";
			shipToParty.E2_Postcode = "3014";
			shipToParty.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			shipToParty.E2_State = "VIC";
			shipToParty.E2_Mobile = "+61 403 112 695";
			var shipToParty2 = header.ExtraShipToPartyAddresses.AddNew();
			shipToParty2.E2_AddressOverride = true;
			shipToParty2.E2_CompanyName = "SHIP2 TO PARTY COMPANY";
			shipToParty2.E2_Address1 = "SHIP2 TO PARTY ADDRESS 1";
			shipToParty2.E2_Address2 = "SHIP2 TO PARTY ADDRESS 2";
			shipToParty2.E2_City = "MELBOURNE";
			shipToParty2.E2_Contact = "SHIP2 TO THE BUILDER";
			shipToParty2.E2_Email = "SHIP2TO@BUILDER.COM";
			shipToParty2.E2_Fax = "+61 (3) 8456 6846";
			shipToParty2.E2_Phone = "+61 (3) 8456 6855";
			shipToParty2.E2_Postcode = "3014";
			shipToParty2.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			shipToParty2.E2_State = "VIC";
			shipToParty.E2_Mobile = "+61 403 112 695";
			header.Equipments.DeleteAll();
			var container1 = header.Equipments.AddNew();
			container1.BE_ContainerNum = "TURE2323333";
			container1.BE_EquipCode = "20";
			container1.BE_ContainerISO = "20FR";
			var container2 = header.Equipments.AddNew();
			container2.BE_ContainerNum = "TURE1110111";
			container2.BE_EquipCode = "40";
			container2.BE_ContainerISO = "40FR";
			var manufacturer1DocAddress = header.ManufacturerAddresses.AddNew();
			manufacturer1DocAddress.E2_AddressType = DocAddressTypes.Codes.Manufacturer;
			manufacturer1DocAddress.E2_OA_Address = manufacturer1.MainAddress.PK;
			var manufacturer2DocAddress = header.ManufacturerAddresses.AddNew();
			manufacturer2DocAddress.E2_AddressType = DocAddressTypes.Codes.Manufacturer;
			manufacturer2DocAddress.E2_AddressOverride = true;
			manufacturer2DocAddress.E2_CompanyName = "MANUFACTURER2 COMPANY";
			manufacturer2DocAddress.E2_Address1 = "MANUFACTURER2 ADDRESS 1";
			manufacturer2DocAddress.E2_Address2 = "MANUFACTURER2 ADDRESS 2";
			manufacturer2DocAddress.E2_City = "MELBOURNE";
			manufacturer2DocAddress.E2_Contact = "BOB THE DESTROYER";
			manufacturer2DocAddress.E2_Email = "BOB@DOOM.COM";
			manufacturer2DocAddress.E2_Fax = "+61 (2) 6666 6666";
			manufacturer2DocAddress.E2_Phone = "+61 (2) 9999 9999";
			manufacturer2DocAddress.E2_Postcode = "3666";
			manufacturer2DocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			manufacturer2DocAddress.E2_State = "VIR";
			manufacturer2DocAddress.E2_Mobile = "+61 433 666 666";
			header.Lines.DeleteAll();
			var line1 = header.Lines.AddNew();
			line1.BL_HarmonisedNum = "10.10.8120";
			line1.BL_ManufacturerDocAddressPK = manufacturer1DocAddress.PK;
			line1.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			line1.CustomAttribute1 = "LNEATTRIB1";
			line1.CustomAttribute2 = "LNEATTRIB2";
			line1.SetUserDefinedValue("Hello", new ZString("MyHello"));
			var line2 = header.Lines.AddNew();
			line2.BL_HarmonisedNum = "20.20.8220";
			line2.BL_ManufacturerDocAddressPK = manufacturer2DocAddress.PK;
			line2.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			var line3 = header.Lines.AddNew();
			line3.BL_HarmonisedNum = "30.30.8320";
			line3.BL_ManufacturerDocAddressPK = manufacturer1DocAddress.PK;
			line3.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			line3.BL_TextProductCode = part.OP_PartNum;
			var line4 = header.Lines.AddNew();
			line4.BL_HarmonisedNum = "40.40.8420";
			line4.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.NewZealand;
			header.BF_CustomsReference = ZString.Empty;
			header.BF_CustomsStatus = MessageStatusList.Codes.NotSentISF;
			header.BF_LineMergeStyle = MergeStyleList.Codes.Default;
			header.BF_NumOfHarmChars = NumberOfHarmonizedDigitsList.Codes.Ten;
			return new BusinessObjectAndExpectedOutputFileName(header, BaseTestFilePath + "PopulatedImporterSecurityFiling.xml", ValidationKind.Xsd, "Populated ImporterSecurityFiling");
		}

		protected override void OnBeforeImportFromValueObjectForExportImportExportTest(BusinessObject bizObjOriginallyExportedFrom, BusinessObject bizObjToImportTo)
		{
			var header = bizObjOriginallyExportedFrom as CusISFHeader;
			if (header != null)
			{
				header.BF_MasterBill = ZString.Empty;
			}

			base.OnBeforeImportFromValueObjectForExportImportExportTest(bizObjOriginallyExportedFrom, bizObjToImportTo);
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => Array.Empty<BusinessObjectAndExpectedOutputFileName>();

		protected override CusISFHeader NewBusinessObject()
		{
			var factory = new BusinessObjectFactory();
			var uSCompany = factory.New<GlbCompany>();
			uSCompany.GC_Code = "Z1Z";
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			uSCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var uSBranch = uSCompany.Branches.AddNew();
			uSBranch.GB_Code = "Z1Z";
			uSBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var header = factory.New<CusISFHeader>();
			header.BF_GB = uSBranch.PK;
			header.BF_CustomsStatus = MessageStatusList.Codes.NotSentISF;
			return header;
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest => new string[]
				{
					"Action",
					"BondIndicator",
					"Workflow",
					"Status",
					"BondActivityCode",
					"BondType",
					"TransactionNumber",
					"JobReference",
					"BondHolder",
					"Routings/Item",
					"Events",
					"ImporterOfRecord/FullLegalName",
					"ShipmentSubType",
					"EstimatedValue",
					"EstimatedWeight",
					"EstimatedQuantity"
				};

		Transport AddTransport(TransportCollection transports, ZString transportMode, ZString transportType, ZString vessel, ZString voyageFlight, ZString loadPort, ZString dischargePort, ZDateTime etd, ZDateTime atd, ZDateTime eta, ZDateTime ata, ZByte legOrder)
		{
			Transport transport = transports.AddNew();
			transport.JW_TransportMode = transportMode;
			transport.JW_TransportType = transportType;
			transport.JW_Vessel = vessel;
			transport.JW_VoyageFlight = voyageFlight;
			transport.JW_RL_NKLoadPort = loadPort;
			transport.JW_RL_NKDiscPort = dischargePort;
			transport.JW_ETD = etd;
			transport.JW_ATD = atd;
			transport.JW_ETA = eta;
			transport.JW_ATA = ata;
			transport.JW_LegOrder = legOrder;
			return transport;
		}

		void AddBill(CusISFHeader header, ZString billNum, ZString billType)
		{
			var bill = header.ReferenceDatas.AddNew();
			bill.BB_BillNum = billNum;
			bill.BB_BillType = billType;
		}

		void AssertBillStatus(Xsd.ISFStatusCustomsResponseBill xsdBill, ZString billNumber, Xsd.ISFDispositionCode dispositionCode, ZDate matchedDate)
		{
			AssertEquals(true, xsdBill.IsSpecified);
			AssertEquals(billNumber, xsdBill.Number);
			AssertEquals(dispositionCode, xsdBill.DispositionCode.Code);
			AssertEquals(matchedDate, xsdBill.MatchedDate);
		}

		string BaseTestFilePath => BaseSourcePath + @"Enterprise\Product\Operations\Customs\US\ISF\DataTransfer.Test\TestFiles\";
	}
}
