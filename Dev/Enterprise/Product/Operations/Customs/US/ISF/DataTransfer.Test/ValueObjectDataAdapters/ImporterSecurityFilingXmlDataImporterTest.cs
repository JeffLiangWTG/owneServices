using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.US.ISF.DataTransfer.Testing
{
	sealed class ImporterSecurityFilingXmlDataImporterTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2009, 6, 10)]
		public void TestImportAndSendToCustoms()
		{
			var groupZZ1 = Factory.New<GlbGroup>();
			groupZZ1.GG_Code = "ZZ1";
			var staffZ1 = groupZZ1.Staff.AddNew();
			staffZ1.GS_Code = "Z1";
			staffZ1.GS_LoginName = "z1";
			staffZ1.GS_EmailAddress = "dong@pretend.email.com";
			Factory.Save();
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new EntryFiler()
			{
				EntryFilerCode = "XJ5"
			});
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "8888");
			ISFRegistry.Instance.ImporterSecurityFilingXMLImportNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupZZ1.PK.ToGuid());
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var dataImporter = new ImporterSecurityFilingXmlDataImporter();
			var notification = new NotificationBuffer();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var noOfISFs = Factory.GetDatabaseCount(typeof(CusISFHeader));
			var fileName = BaseTestFilePath + "ImportAndSendToCustomsImporterSecurityFilingWithError.xml";
			dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			AssertEquals(noOfISFs + 1, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var query = new ZQuery(CusISFHeaderSchema.BF_ImporterCodeType, ImporterCodeTypeList.Codes.IRS);
			query.AddToFilter(CusISFHeaderSchema.BF_ImporterCode, "91-013199123");
			query.AddToFilter(CusISFHeaderSchema.BF_EntryType, SubmissionTypeList.Codes.ISF10);
			query.AddToFilter(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.StandardOrRegularFilings);
			query.AddToFilter(CusISFHeaderSchema.BF_SCAC, "OCEA");
			query.AddToFilter(CusISFHeaderSchema.BF_ConsigneeCodeType, ImporterCodeTypeList.Codes.IRS);
			query.AddToFilter(CusISFHeaderSchema.BF_ConsigneeCode, "91-013199789");
			var header = Factory.LoadTop1<CusISFHeader>(query);
			AssertNotNull("Data should have been imported", header);
			AssertNull("No message was send to customs as only import via Batch Processor will cause the system to auto send", header.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling, EDIMessage.Direction.Transmit));
			header.Delete();
			Factory.Save();
			notification.Clear();
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			}

			AssertEquals(noOfISFs + 1, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			header = Factory.LoadTop1<CusISFHeader>(query);
			AssertNotNull("Data should have been imported", header);
			AssertEquals(0, header.Messages.Count);
			string emailSubject = "Automatically Send To Customs Failed for " + header.HumanReadableName;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find((EmailDef emailToMatched) =>
			{
				return emailToMatched.Subject == emailSubject;
			});
			AssertContains(ImporterSecurityFilingXmlDataImporter.ISFJobHasErrorMessageError, email.Body);
			AssertCollectionContains("dong@pretend.email.com", email.Recipients);
			header.Delete();
			Factory.Save();
			fileName = BaseTestFilePath + "ImportAndSendToCustomsImporterSecurityFiling.xml";
			notification.Clear();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			}

			AssertEquals(noOfISFs + 1, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			header = Factory.LoadTop1<CusISFHeader>(query);
			AssertNotNull("Data should have been imported", header);
			var message = header.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling, EDIMessage.Direction.Transmit);
			AssertNotNull("A message should have been send to customs", message);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2009, 6, 10)]
		public void TestImportAndNotSendToCustomsBecauseOfUnmatchedOrganisation()
		{
			var groupZZ1 = Factory.New<GlbGroup>();
			groupZZ1.GG_Code = "ZZ1";
			var staffZ1 = groupZZ1.Staff.AddNew();
			staffZ1.GS_Code = "Z1";
			staffZ1.GS_LoginName = "z1";
			staffZ1.GS_EmailAddress = "dong@pretend.email.com";
			Factory.Save();
			var unmatchedOrganisation = new UnmatchedOrganisation(Factory);
			unmatchedOrganisation.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrganisation);
			ISFRegistry.Instance.ImporterSecurityFilingXMLImportNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupZZ1.PK.ToGuid());
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var dataImporter = new ImporterSecurityFilingXmlDataImporter();
			var notification = new NotificationBuffer();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			int noOfISFs = Factory.GetDatabaseCount(typeof(CusISFHeader));
			string fileName = BaseTestFilePath + "ImportAndSendToCustomsWithUnmatchedOrg.xml";
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			}

			var query = new ZQuery(CusISFHeaderSchema.BF_ImporterCodeType, ImporterCodeTypeList.Codes.IRS);
			query.AddToFilter(CusISFHeaderSchema.BF_ImporterCode, "91-013199123");
			query.AddToFilter(CusISFHeaderSchema.BF_EntryType, SubmissionTypeList.Codes.ISF10);
			query.AddToFilter(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.StandardOrRegularFilings);
			query.AddToFilter(CusISFHeaderSchema.BF_SCAC, "OCEA");
			query.AddToFilter(CusISFHeaderSchema.BF_ConsigneeCodeType, ImporterCodeTypeList.Codes.IRS);
			query.AddToFilter(CusISFHeaderSchema.BF_ConsigneeCode, "91-013199789");
			AssertEquals(noOfISFs + 1, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			var header = Factory.LoadTop1<CusISFHeader>(query);
			AssertNotNull("Data should have been imported", header);
			AssertEquals(0, header.Messages.Count);
			AssertEquals(OrgHeader.UnmatchedOrganisationPK, header.SellingParty.OrganisationPK);
			string emailSubject = "Automatically Send To Customs Failed for " + header.HumanReadableName;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find((EmailDef emailToMatched) =>
			{
				return emailToMatched.Subject == emailSubject;
			});
			AssertContains(ImporterSecurityFilingXmlDataImporter.ISFJobHasErrorMessageError, email.Body);
			AssertCollectionContains("dong@pretend.email.com", email.Recipients);
			unmatchedOrganisation.IsEnabled = false;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrganisation);
			header.Delete();
			Factory.Save();
			notification.Clear();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			}

			AssertEquals(noOfISFs + 1, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			header = Factory.LoadTop1<CusISFHeader>(query);
			AssertNotNull("Data should have been imported", header);
			AssertEquals(0, header.Messages.Count);
			AssertEquals(true, header.SellingParty.IsEmpty);
			emailSubject = "Automatically Send To Customs Failed for " + header.HumanReadableName;
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find((EmailDef emailToMatched) =>
			{
				return emailToMatched.Subject == emailSubject;
			});
			AssertContains(ImporterSecurityFilingXmlDataImporter.ISFJobHasErrorMessageError, email.Body);
			AssertCollectionContains("dong@pretend.email.com", email.Recipients);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2009, 6, 10)]
		public void TestImportAndSendToCustomsWithRoutingDetails()
		{
			var groupZZ1 = Factory.New<GlbGroup>();
			groupZZ1.GG_Code = "ZZ1";
			var staffZ1 = groupZZ1.Staff.AddNew();
			staffZ1.GS_Code = "Z1";
			staffZ1.GS_LoginName = "z1";
			staffZ1.GS_EmailAddress = "dong@pretend.email.com";
			var uSCompany = Factory.New<GlbCompany>();
			uSCompany.GC_Code = "Z1Z";
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			uSCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var uSBranch = uSCompany.Branches.AddNew();
			uSBranch.GB_Code = "Z1Z";
			uSBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "CARRIER";
			carrier.OH_FullName = "Carrier";
			carrier.MainAddress.OA_Code = "CRADDR";
			carrier.MainAddress.OA_Address1 = "Carrier Address";
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			Factory.Save();
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new EntryFiler()
			{
				EntryFilerCode = "XJ5"
			});
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "8888");
			ISFRegistry.Instance.ImporterSecurityFilingXMLImportNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupZZ1.PK.ToGuid());
			var dataImporter = new ImporterSecurityFilingXmlDataImporter();
			var notification = new NotificationBuffer();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var noOfISFs = Factory.GetDatabaseCount(typeof(CusISFHeader));
			var fileName = BaseTestFilePath + "ImportAndSendToCustomsWithRouting.xml";
			notification.Clear();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			}

			AssertEquals(noOfISFs + 2, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			var query = new ZQuery(CusISFHeaderSchema.BF_ImporterCodeType, ImporterCodeTypeList.Codes.IRS);
			query.AddToFilter(CusISFHeaderSchema.BF_ImporterCode, "91-013199122");
			query.AddToFilter(CusISFHeaderSchema.BF_EntryType, SubmissionTypeList.Codes.ISF10);
			query.AddToFilter(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.StandardOrRegularFilings);
			query.AddToFilter(CusISFHeaderSchema.BF_SCAC, "OCEA");
			query.AddToFilter(CusISFHeaderSchema.BF_ConsigneeCodeType, ImporterCodeTypeList.Codes.IRS);
			query.AddToFilter(CusISFHeaderSchema.BF_ConsigneeCode, "91-013199789");
			var header1 = Factory.LoadTop1<CusISFHeader>(query);
			AssertNotNull("Data should have been imported", header1);
			var message = header1.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling, EDIMessage.Direction.Transmit);
			AssertNotNull("A message should have been send to customs", message);
			AssertEquals(1, header1.Transports.Count);
			AssertTransport(header1.Transports[0], false, "ADMIRALENGRACHT", "V/122", "AUSYD", "USLAX", new ZDateTime(2010, 4, 6), ZDateTime.Empty);
			query = new ZQuery(CusISFHeaderSchema.BF_ImporterCodeType, ImporterCodeTypeList.Codes.IRS);
			query.AddToFilter(CusISFHeaderSchema.BF_ImporterCode, "91-013199123");
			query.AddToFilter(CusISFHeaderSchema.BF_EntryType, SubmissionTypeList.Codes.ISF10);
			query.AddToFilter(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.StandardOrRegularFilings);
			query.AddToFilter(CusISFHeaderSchema.BF_SCAC, "OCEA");
			query.AddToFilter(CusISFHeaderSchema.BF_ConsigneeCodeType, ImporterCodeTypeList.Codes.IRS);
			query.AddToFilter(CusISFHeaderSchema.BF_ConsigneeCode, "91-013199789");
			var header2 = Factory.LoadTop1<CusISFHeader>(query);
			AssertNotNull("Data should have been imported", header2);
			AssertNotEquals(header1.PK, header2.PK);
			message = header2.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling, EDIMessage.Direction.Transmit);
			AssertNotNull("A message should have been send to customs", message);
			AssertEquals(1, header2.Transports.Count);
			AssertTransport(header2.Transports[0], true, "ADMIRALENGRACHT", "V/123", "AUSYD", "USLAX", new ZDateTime(2010, 4, 6), new ZDateTime(2010, 4, 16));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDeleteAction()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "Z!1";
			company1.GC_Name = "DUMMY COMPANY";
			company1.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "Z!1";
			branch1.GB_BranchName = "DUMMY BRANCH";
			branch1.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "MR IMPORTER FOR DUMMY";
			importer.OH_Code = "ABC123ZZZ123";
			importer.OH_IsConsignee = true;
			importer.MainAddress.OA_Address1 = "IMPORTER ADDRESS 1";
			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = importer.PK;
			header.BF_HouseBill = "BLMN032209B";
			header.BF_GB = branch1.PK;
			var groupZZ1 = Factory.New<GlbGroup>();
			groupZZ1.GG_Code = "ZZ1";
			var staffZ1 = groupZZ1.Staff.AddNew();
			staffZ1.GS_Code = "Z1";
			staffZ1.GS_LoginName = "z1";
			staffZ1.GS_EmailAddress = "dong@pretend.email.com";
			Factory.Save();
			ISFRegistry.Instance.ImporterSecurityFilingXMLImportNotificationGroup.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, groupZZ1.PK.ToGuid());
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var dataImporter = new ImporterSecurityFilingXmlDataImporter();
			var notification = new NotificationBuffer();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var noOfISFs = Factory.GetDatabaseCount(typeof(CusISFHeader));
			var fileName = BaseTestFilePath + "DeleteWithoutSendToCustoms.xml";
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			}

			AssertEquals(noOfISFs, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			var emailSubject = "ISF XML Import with Delete Action: " + header.HumanReadableName;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find((EmailDef emailToMatched) =>
			{
				return emailToMatched.Subject == emailSubject;
			});
			AssertNotNull(email);
			AssertCollectionContains("dong@pretend.email.com", email.Recipients);
			emailSubject = "Automatically Send To Customs Failed for " + header.HumanReadableName;
			AssertNull(Env.OutgoingCustomsMailManager.EmailsCreated.Find((EmailDef emailToMatched) =>
			{
				return emailToMatched.Subject == emailSubject;
			}));
			header.Reload();
			AssertEquals(0, header.Messages.Count);
			AssertEquals("Delete", header.Logs.MostRecentLogByEventTime(Events.DataImport).SL_Reference);
			notification.Clear();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			fileName = BaseTestFilePath + "DeleteWithSendToCustoms.xml";
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			}

			AssertEquals(noOfISFs, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			emailSubject = "ISF XML Import with Delete Action: " + header.HumanReadableName;
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find((EmailDef emailToMatched) =>
			{
				return emailToMatched.Subject == emailSubject;
			});
			AssertNotNull(email);
			AssertCollectionContains("dong@pretend.email.com", email.Recipients);
			emailSubject = "Automatically Send To Customs Failed for " + header.HumanReadableName;
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find((EmailDef emailToMatched) =>
			{
				return emailToMatched.Subject == emailSubject;
			});
			AssertNotNull(email);
			AssertCollectionContains("dong@pretend.email.com", email.Recipients);
			header.Reload();
			AssertEquals(0, header.Messages.Count);
			AssertEquals("Delete With Send To Customs", header.Logs.MostRecentLogByEventTime(Events.DataImport).SL_Reference);
			header.BF_CustomsReference = "ISD323423";
			Factory.Save();
			notification.Clear();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			}

			AssertEquals(noOfISFs, Factory.GetDatabaseCount(typeof(CusISFHeader)));
			emailSubject = "ISF XML Import with Delete Action: " + header.HumanReadableName;
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find((EmailDef emailToMatched) =>
			{
				return emailToMatched.Subject == emailSubject;
			});
			AssertNotNull(email);
			AssertCollectionContains("dong@pretend.email.com", email.Recipients);
			emailSubject = "Automatically Send To Customs Failed for " + header.HumanReadableName;
			AssertNull(Env.OutgoingCustomsMailManager.EmailsCreated.Find((EmailDef emailToMatched) =>
			{
				return emailToMatched.Subject == emailSubject;
			}));
			header.Reload();
			AssertEquals(1, header.Messages.Count);
			AssertContains("SF101  DCT                          11ISD323423                                 Y         ", header.Messages[0].EM_MessageText);
			AssertEquals("Delete With Send To Customs", header.Logs.MostRecentLogByEventTime(Events.DataImport).SL_Reference);
		}

		[TestDate(2009, 6, 10)]
		public void TestImportAndSendToCustoms_BOIsModifiedInOtherFactory()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "Z!1";
			company1.GC_Name = "DUMMY COMPANY";
			company1.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "Z!1";
			branch1.GB_BranchName = "DUMMY BRANCH";
			branch1.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "MR IMPORTER FOR DUMMY";
			importer.OH_Code = "ABC123ZZZ123";
			importer.OH_IsConsignee = true;
			importer.MainAddress.OA_Address1 = "IMPORTER ADDRESS 1";
			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = importer.PK;
			header.BF_HouseBill = "BLMN032209B";
			header.BF_GB = branch1.PK;
			header.SellingParty.E2_AddressOverride = true;
			header.SellingParty.E2_Address1 = "Address 1";
			Factory.Save();
			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var header1 = factory1.Load<CusISFHeader>(header.PK);
			header1.SellingParty.E2_Address1 = "Address 2";
			header1.LoadChildEditableObjects();
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var header2 = factory2.Load<CusISFHeader>(header.PK);
			header2.SellingParty.E2_Address1 = "Address 3";
			((ILightValidationInternals)header2.SellingParty).IsValid = true;
			factory2.Save();
			var sendToCustomsMethod = typeof(ImporterSecurityFilingXmlDataImporter).GetMethod("SendToCustoms", BindingFlags.NonPublic | BindingFlags.Instance);
			var buffer = new NotificationBuffer();
			sendToCustomsMethod.Invoke(new ImporterSecurityFilingXmlDataImporter(), new object[] { buffer, new BusinessObjectFactoryProvider(factory1), header1, UpdateActionCode.Add });
			Assert(!buffer.HasErrors);
			AssertEquals(1, header1.Messages.Count);
			var message = header1.Messages[0];
			AssertEquals(ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling, message.EM_MessageType);
			AssertEquals(true, message.IsInDatabase);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 2, 24)]
		public void TestImportAndSendToCustoms_OverriddenAddressesDoNotCauseErrors()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = true;
			var groupZZ1 = Factory.New<GlbGroup>();
			groupZZ1.GG_Code = "ZZ1";
			var staffZ1 = groupZZ1.Staff.AddNew();
			staffZ1.GS_Code = "Z1";
			staffZ1.GS_LoginName = "z1";
			staffZ1.GS_EmailAddress = "dong@pretend.email.com";
			Factory.Save();
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new EntryFiler()
			{
				EntryFilerCode = "XJ5"
			});
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "8888");
			ISFRegistry.Instance.ImporterSecurityFilingXMLImportNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupZZ1.PK.ToGuid());
			var dataImporter = new ImporterSecurityFilingXmlDataImporter();
			var notification = new NotificationBuffer();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			string fileName = BaseTestFilePath + "ImportAndSendToCustomsImporterSecurityFiling.xml";
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				dataImporter.ImportData(fileName, notification, SourceInfo.EmptySourceInfo);
			}

			var query = new ZQuery(CusISFHeaderSchema.BF_ImporterCodeType, ImporterCodeTypeList.Codes.IRS);
			query.AddToFilter(CusISFHeaderSchema.BF_ImporterCode, "91-013199123");
			query.AddToFilter(CusISFHeaderSchema.BF_EntryType, SubmissionTypeList.Codes.ISF10);
			query.AddToFilter(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.StandardOrRegularFilings);
			query.AddToFilter(CusISFHeaderSchema.BF_SCAC, "OCEA");
			query.AddToFilter(CusISFHeaderSchema.BF_ConsigneeCodeType, ImporterCodeTypeList.Codes.IRS);
			query.AddToFilter(CusISFHeaderSchema.BF_ConsigneeCode, "91-013199789");
			var header = Factory.LoadTop1<CusISFHeader>(query);
			var message = header.Messages.GetLastMessage(EDIInterchange.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling, EDIInterchange.Direction.Transmit);
			AssertNotNull("A message should have been send to customs", message);
		}

		void AssertTransport(Transport transport, ZBool isLinked, ZString vessel, ZString voyage, ZString load, ZString disc, ZDateTime etd, ZDateTime eta)
		{
			AssertEquals(isLinked, transport.JW_IsLinked);
			AssertEquals(vessel, transport.JW_Vessel);
			AssertEquals(voyage, transport.JW_VoyageFlight);
			AssertEquals(load, transport.JW_RL_NKLoadPort);
			AssertEquals(disc, transport.JW_RL_NKDiscPort);
			AssertEquals(etd, transport.JW_ETD);
			AssertEquals(eta, transport.JW_ETA);
		}

		string BaseTestFilePath => BaseSourcePath + @"Enterprise\Product\Operations\Customs\US\ISF\DataTransfer.Test\TestFiles\";
	}
}
