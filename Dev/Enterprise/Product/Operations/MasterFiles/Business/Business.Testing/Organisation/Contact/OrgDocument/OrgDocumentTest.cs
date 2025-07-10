using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.OrgDocumentLookups;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgDocument))]
	sealed class OrgDocumentTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			var document = Factory.NewWithValidTestData<OrgDocument>();
			document.OD_OC = orgContact.PK;
			return document;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var orgContact = factory.NewWithValidTestData<OrgContact>();
			var document = factory.NewWithValidTestData<OrgDocument>();
			document.OD_OC = orgContact.PK;
			return document;
		}

		public void TestIsSuppressedForSpecificJob()
		{
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>() as IStmMenuItem;
			menuItem.SU_ContactType = ContactType.Warehouse.Code;
			Document.OD_SU_MenuItem = menuItem.PK;

			var jobDocumentExclusion = Factory.New<JobDocumentExclusion>();
			var shipment1 = Factory.New<Forwarding.IForwardingShipment>() as BusinessObject;
			jobDocumentExclusion.JDE_ParentID = shipment1.PK;
			jobDocumentExclusion.JDE_ParentTableCode = shipment1.TablePrefix;
			jobDocumentExclusion.JDE_OD_Document = Document.PK;
			Factory.Save();
			Assert("Document should be suppressed for shipment1.", Document.IsSuppressedForSpecificJob(shipment1.PK, shipment1.TablePrefix, menuItem));

			var shipment2 = Factory.New<Forwarding.IForwardingShipment>() as BusinessObject;
			Factory.Save();
			Assert("Document should not be suppressed for shipment2.", !Document.IsSuppressedForSpecificJob(shipment2.PK, shipment2.TablePrefix, menuItem));

			jobDocumentExclusion.JDE_ParentTableCode = jobDocumentExclusion.TablePrefix;
			Factory.Save();
			Assert("Document should not be suppressed for shipment1 when the table code is not matched.", !Document.IsSuppressedForSpecificJob(shipment1.PK, shipment1.TablePrefix, menuItem));

			var jobDocumentDelivery = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery.JDC_SU_MenuItem = menuItem.PK;
			jobDocumentDelivery.JDC_ParentID = shipment1.PK;
			jobDocumentDelivery.JDC_ParentTableCode = shipment1.TablePrefix;
			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.DoNotDeliver;
			jobDocumentDelivery.JDC_OC_Contact = Document.OD_OC;
			Factory.Save();
			Assert("Document should be suppressed when the job specific document delivery method is DoNotDeliver for shipment1.", Document.IsSuppressedForSpecificJob(shipment1.PK, shipment1.TablePrefix, menuItem));

			jobDocumentDelivery.JDC_SU_MenuItem = ZGuid.Empty;
			jobDocumentDelivery.JDC_DocumentGroup = ContactType.Warehouse.Code;
			Document.OD_SU_MenuItem = ZGuid.Empty;
			Document.OD_DocumentGroup = ContactType.Warehouse.Code;
			Factory.Save();
			Assert("Document should be suppressed when the job specific document group delivery method is DoNotDeliver for shipment1.", Document.IsSuppressedForSpecificJob(shipment1.PK, shipment1.TablePrefix, menuItem));

			Document.OD_SU_MenuItem = menuItem.PK;
			Document.OD_DocumentGroup = "";
			Factory.Save();
			Assert("Document should be suppressed when the job specific document group is matched with OrgDocument MenuItem group and the delivery method is DoNotDeliver", Document.IsSuppressedForSpecificJob(shipment1.PK, shipment1.TablePrefix, menuItem));

			Document.OD_SU_MenuItem = ZGuid.Empty;
			Document.OD_DocumentGroup = ContactType.Warehouse.Code;
			jobDocumentDelivery.JDC_DocumentGroup = "";
			jobDocumentDelivery.JDC_SU_MenuItem = menuItem.PK;
			Assert("Document should be suppressed when the job specific document's group is matched with OrgDocument group and the delivery method is DoNotDeliver", Document.IsSuppressedForSpecificJob(shipment1.PK, shipment1.TablePrefix, menuItem));

			jobDocumentDelivery.Delete();
			Factory.Save();
			Assert("Document should not be suppressed when NO job specific document delivery method is DoNotDeliver for shipment1.", !Document.IsSuppressedForSpecificJob(shipment1.PK, shipment1.TablePrefix, menuItem));
		}

		public void TestEmailSubjectMacro()
		{
			var cnt = OrgInDB.Contacts.AddNew();
			var doc = cnt.Documents.AddNew();
			doc.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			doc.OD_EmailSubjectMacro = "Some <Macro>";
			doc.OD_DeliverBy = Constants.ContactNotifyModes.Fax;
			Assert(doc.OD_EmailSubjectMacro_ReadOnly);

			doc.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			Assert(!doc.OD_EmailSubjectMacro_ReadOnly);
			AssertEquals("Some <Macro>", doc.OD_EmailSubjectMacro);
		}

		public void TestRelatedPartyReadOnly()
		{
			OrgHeader related = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgContact cnt = OrgInDB.Contacts.AddNew();
			OrgDocument doc = cnt.Documents.AddNew();
			Assert(!doc.OD_OH_RelatedFilterByPartyInfo.ReadOnly);
			doc.OD_OH_RelatedFilterByParty = related.PK;

			doc.OD_DocumentGroup = ContactType.Sales.Code;
			Assert(!doc.OD_OH_RelatedFilterByPartyInfo.ReadOnly);
			AssertEquals(related.PK, doc.OD_OH_RelatedFilterByParty);

			doc.OD_DocumentGroup = ContactType.Receivables.Code;
			Assert(doc.OD_OH_RelatedFilterByPartyInfo.ReadOnly);
			AssertEquals(ZGuid.Empty, doc.OD_OH_RelatedFilterByParty);

			doc.OD_DocumentGroup = ContactType.Warehouse.Code;
			Assert(!doc.OD_OH_RelatedFilterByPartyInfo.ReadOnly);
			doc.OD_OH_RelatedFilterByParty = related.PK;
			AssertEquals(related.PK, doc.OD_OH_RelatedFilterByParty);

			doc.OD_DocumentGroup = ContactType.Payables.Code;
			Assert(doc.OD_OH_RelatedFilterByPartyInfo.ReadOnly);
			AssertEquals(ZGuid.Empty, doc.OD_OH_RelatedFilterByParty);
		}

		public void TestFilterLocalAndForeignPortReadOnly()
		{
			OrgHeader related = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgContact cnt = OrgInDB.Contacts.AddNew();
			OrgDocument doc = cnt.Documents.AddNew();

			doc.OD_FilterLocalPort = "NZ";
			doc.OD_FilterForeignPort = "AUSYD";
			doc.OD_DocumentGroup = ContactType.Receivables.Code;
			AssertFilterLocalAndForeignPortReadOnly(true, doc);

			doc.OD_FilterLocalPort = "NZ";
			doc.OD_FilterForeignPort = "AUSYD";
			doc.OD_DocumentGroup = ContactType.Payables.Code;
			AssertFilterLocalAndForeignPortReadOnly(true, doc);

			doc.OD_FilterLocalPort = "NZ";
			doc.OD_FilterForeignPort = "AUSYD";
			doc.OD_DocumentGroup = ContactType.Sales.Code;
			AssertFilterLocalAndForeignPortReadOnly(false, doc);

			doc.OD_DocumentGroup = string.Empty;
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Receivables.Code;
			Factory.Save();
			doc.OD_SU_MenuItem = menuItem.PK;
			AssertFilterLocalAndForeignPortReadOnly(true, doc);

			doc.OD_FilterLocalPort = "NZ";
			doc.OD_FilterForeignPort = "AUSYD";
			menuItem.SU_ContactType = ContactType.Sales.Code;
			Factory.Save();
			doc.OD_SU_MenuItem = menuItem.PK;
			AssertFilterLocalAndForeignPortReadOnly(false, doc);

			menuItem.SU_ContactType = ContactType.Payables.Code;
			Factory.Save();
			doc.OD_SU_MenuItem = menuItem.PK;
			AssertFilterLocalAndForeignPortReadOnly(true, doc);
		}

		void AssertFilterLocalAndForeignPortReadOnly(bool isReadOnly, OrgDocument doc)
		{
			AssertEquals(isReadOnly, doc.OD_FilterLocalPortInfo.ReadOnly);
			AssertEquals(isReadOnly, doc.OD_FilterForeignPortInfo.ReadOnly);
			if (isReadOnly)
			{
				AssertEquals(ZString.Empty, doc.OD_FilterLocalPort);
				AssertEquals(ZString.Empty, doc.OD_FilterForeignPort);
			}
		}

		public void TestFilterLocalAndForeignPortNotReadOnlyWithEXPAndIMP()
		{
			OrgHeader related = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgContact cnt = OrgInDB.Contacts.AddNew();
			OrgDocument doc = cnt.Documents.AddNew();

			doc.OD_FilterLocalPort = "NZ";
			doc.OD_FilterForeignPort = "AUSYD";

			doc.OD_DocumentGroup = ContactType.Receivables.Code;
			doc.OD_FilterDirection = FilterDirectionConstants.Codes.Export;
			AssertFilterLocalAndForeignPortReadOnly(false, doc);
			doc.OD_FilterDirection = FilterDirectionConstants.Codes.Import;
			AssertFilterLocalAndForeignPortReadOnly(false, doc);
			doc.OD_FilterDirection = FilterDirectionConstants.Codes.All;
			AssertFilterLocalAndForeignPortReadOnly(true, doc);

			doc.OD_DocumentGroup = ContactType.Payables.Code;
			doc.OD_FilterDirection = FilterDirectionConstants.Codes.Export;
			AssertFilterLocalAndForeignPortReadOnly(false, doc);
			doc.OD_FilterDirection = FilterDirectionConstants.Codes.Import;
			AssertFilterLocalAndForeignPortReadOnly(false, doc);
			doc.OD_FilterDirection = FilterDirectionConstants.Codes.All;
			AssertFilterLocalAndForeignPortReadOnly(true, doc);

			doc.OD_DocumentGroup = string.Empty;
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Receivables.Code;
			Factory.Save();
			doc.OD_SU_MenuItem = menuItem.PK;
			doc.OD_FilterDirection = FilterDirectionConstants.Codes.Import;
			AssertFilterLocalAndForeignPortReadOnly(false, doc);
			doc.OD_FilterDirection = FilterDirectionConstants.Codes.Export;
			AssertFilterLocalAndForeignPortReadOnly(false, doc);
			doc.OD_FilterDirection = FilterDirectionConstants.Codes.All;
			AssertFilterLocalAndForeignPortReadOnly(true, doc);

			menuItem.SU_ContactType = ContactType.Payables.Code;
			Factory.Save();
			doc.OD_SU_MenuItem = menuItem.PK;
			doc.OD_FilterDirection = FilterDirectionConstants.Codes.Import;
			AssertFilterLocalAndForeignPortReadOnly(false, doc);
			doc.OD_FilterDirection = FilterDirectionConstants.Codes.Export;
			AssertFilterLocalAndForeignPortReadOnly(false, doc);
			doc.OD_FilterDirection = FilterDirectionConstants.Codes.All;
			AssertFilterLocalAndForeignPortReadOnly(true, doc);
		}

		public void TestDelete_RemovesRelatedJobDocumentExclusionRecords()
		{
			var exclusion1 = Factory.New<JobDocumentExclusion>();
			exclusion1.JDE_OD_Document = Document.PK;
			exclusion1.JDE_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			exclusion1.JDE_ParentID = ZGuid.NewZGuid();

			var exclusion2 = Factory.New<JobDocumentExclusion>();
			exclusion2.JDE_OD_Document = Document.PK;
			exclusion2.JDE_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			exclusion2.JDE_ParentID = ZGuid.NewZGuid();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var anotherOrgDocument = org.Contacts.AddNew().Documents.AddNew();

			var exclusion3 = Factory.New<JobDocumentExclusion>();
			exclusion3.JDE_OD_Document = anotherOrgDocument.PK;
			exclusion3.JDE_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			exclusion3.JDE_ParentID = ZGuid.NewZGuid();

			AssertEquals("Precondition", 2, GetRelatedJobDocumentExclusionCount(Document.PK));
			AssertEquals("Precondition", 1, GetRelatedJobDocumentExclusionCount(anotherOrgDocument.PK));

			Document.Delete();

			AssertEquals("Related JobDocumentExclusion records should be deleted alongside their OrgDocument", 0, GetRelatedJobDocumentExclusionCount(Document.PK));
			AssertEquals("Unrelated JobDocumentExclusion records should not be deleted", 1, GetRelatedJobDocumentExclusionCount(anotherOrgDocument.PK));

			int GetRelatedJobDocumentExclusionCount(ZGuid orgDocumentPK)
			{
				return Factory.Load<JobDocumentExclusion>(new ZQuery(JobDocumentExclusionSchema.JDE_OD_Document, orgDocumentPK)).Length;
			}
		}

		public void TestFilterBranchCompanyDepartmentReadOnly()
		{
			(ContactType contactType, bool featureEnabled)[] docGroups = {
				(ContactType.All, false),
				(ContactType.Receivables, false),
				(ContactType.Payables, false),
				(ContactType.Consignee, false),
				(ContactType.Consignor, false),
				(ContactType.TransportServices, false),
				(ContactType.Warehouse, false),
				(ContactType.Sales, false),
				(ContactType.Marketing, false),
				(ContactType.FreightAgent, false),
				(ContactType.ExportFreightAgent, false),
				(ContactType.ImportFreightAgent, false),
				(ContactType.ImportSeaFreightAgent, false),
				(ContactType.ImportAirFreightAgent, false),
				(ContactType.ExportAirFreightAgent, false),
				(ContactType.ExportSeaFreightAgent, false),
				(ContactType.ShippingLine, false),
				(ContactType.CTO, false),
				(ContactType.Depot, false),
				(ContactType.ExportDepot, false),
				(ContactType.ImportDepot, false),
				(ContactType.ExportSeaDepot, false),
				(ContactType.ExportAirDepot, false),
				(ContactType.ImportSeaDepot, false),
				(ContactType.ImportAirDepot, false),
				(ContactType.AirWholesaler, false),
				(ContactType.LocalTransport, false),
				(ContactType.Warehouse3PL, false),
				(ContactType.CustomerService, false),
				(ContactType.Administration, false),
				(ContactType.NotifyParty, false),
				(ContactType.Miscellaneous, false),
				(ContactType.NoContactType, false),
				(ContactType.LocalClient, false),
				(ContactType.ExportBroker, false),
				(ContactType.ImportBroker, false),
				(ContactType.CommissionAgreementRecipient, false),
				(ContactType.TransitWarehouse, false),
				(ContactType.VerifiedGrossWeightContact, false),
				(ContactType.NettingParticipantStatement, false),
				(ContactType.NettingClearingJournal, false),
				(ContactType.ControllingCustomer, false),
				(ContactType.ControllingAgent, false),
				(ContactType.Applicant, false),
				(ContactType.Importer, false) };

			foreach (var docGroup in docGroups)
			{
				AssertFilterBranchCompanyDepartmentReadOnly(docGroup.contactType, docGroup.featureEnabled);
			}
		}

		void AssertFilterBranchCompanyDepartmentReadOnly(ContactType contactType, bool featureEnabled)
		{
			OrgContact cnt = OrgInDB.Contacts.AddNew();
			OrgDocument doc = cnt.Documents.AddNew();
			doc.OD_DocumentGroup = contactType.Code;
			AssertEquals("FilterBranchReadOnly", !featureEnabled, doc.OD_GB_FilterBranchInfo.ReadOnly);
			AssertEquals("FilterCompanyReadOnly", !featureEnabled, doc.OD_GC_FilterCompanyInfo.ReadOnly);
			AssertEquals("FilterDepartmentReadOnly", !featureEnabled, doc.OD_GE_FilterDepartmentInfo.ReadOnly);
		}

		#region ReadOnly Security

		public void TestReadOnlySecurity()
		{
			bool oldDocDeliveryValue = Env.Security.OrgContactModifyDocDeliveryDetails.IsAllowed;

			try
			{
				OrgContact testContact = OrgInDB.Contacts.AddNew();
				OrgDocument document = testContact.Documents.AddNew();

				Env.Security.OrgContactModifyDocDeliveryDetails.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !document.OD_AttachmentTypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !document.OD_DefaultContactInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !document.OD_DeliverByInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !document.OD_DocumentGroupInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !document.OD_FilterDirectionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !document.OD_FilterForeignPortInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !document.OD_FilterLocalPortInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !document.OD_FilterShipmentModeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !document.OD_OH_RelatedFilterByPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !document.OD_SU_MenuItemInfo.ReadOnly);

				Env.Security.OrgContactModifyDocDeliveryDetails.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", document.OD_AttachmentTypeInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", document.OD_DefaultContactInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", document.OD_DeliverByInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", document.OD_DocumentGroupInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", document.OD_FilterDirectionInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", document.OD_FilterForeignPortInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", document.OD_FilterLocalPortInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", document.OD_FilterShipmentModeInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", document.OD_OH_RelatedFilterByPartyInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", document.OD_SU_MenuItemInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgContactModifyDocDeliveryDetails.IsAllowed = oldDocDeliveryValue;
			}
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion

		#region Default Values

		public void TestDefaultValues()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			OrgDocument doc = contact.Documents.AddNew();

			AssertEquals(OrgConstants.AttachmentType.PDF, doc.OD_AttachmentType);
			AssertEquals(Constants.TransportModes.All, doc.OD_FilterShipmentMode);
			AssertEquals(OrgDocumentLookups.FilterDirectionConstants.Codes.All, doc.OD_FilterDirection);
		}

		#endregion

		#region Logging

		public void TestNoAuditLogging()
		{
			Document.Delete();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Mary";
			var doc = contact.Documents.AddNew();
			doc.OD_DocumentGroup = "BLB";
			Factory.Save();
			AssertEquals("No 'Add' Log on document", 0, doc.Logs.GetAllLogs().Count);

			doc.OD_DocumentGroup = "PLD";
			Factory.Save();
			AssertEquals("No 'EDT' Logs on document", 0, doc.Logs.GetAllLogs().Count);

			System.Threading.Thread.Sleep(1);
			doc.OD_DocumentGroup = ZString.Empty;
			var item = Factory.NewWithValidTestData<StmMenuItem>();
			item.SU_MenuName = "Report";
			doc.OD_SU_MenuItem = item.PK;

			Factory.Save();
			AssertEquals("No 'EDT' Logs on document", 0, doc.Logs.GetAllLogs().Count);
		}

		#endregion

		#region Lookups

		public void TestOD_DeliverBy_List()
		{
			Assert("OD_DeliverBy_List.Count > 0", Document.Lookups.OD_DeliverBy_List.Count > 0);
		}

		public void TestOD_DocumentGroup_List()
		{
			Assert("OD_DocumentGroup_List.Count > 0", Document.Lookups.OD_DocumentGroup_List.Count > 0);
		}

		public void TestOD_AttachmentType_List()
		{
			AssertEquals("OD_AttachmentType_List.Count", 8, Document.Lookups.OD_AttachmentType_List.Count);
			Assert("Attachment types should contain XLS.", Document.Lookups.OD_AttachmentType_List.ContainsCode(AttachmentTypeList.Codes.Xls));
			Assert("Attachment types should contain XLSX.", Document.Lookups.OD_AttachmentType_List.ContainsCode(AttachmentTypeList.Codes.Xlsx));
			Assert("Attachment types should contain PDF.", Document.Lookups.OD_AttachmentType_List.ContainsCode(AttachmentTypeList.Codes.Pdf));
			Assert("Attachment types should contain PDF/A.", Document.Lookups.OD_AttachmentType_List.ContainsCode(AttachmentTypeList.Codes.Pdfa));
			Assert("Attachment types should contain PDFC.", Document.Lookups.OD_AttachmentType_List.ContainsCode(AttachmentTypeList.Codes.Pdfc));
			Assert("Attachment types should contain TIF.", Document.Lookups.OD_AttachmentType_List.ContainsCode(AttachmentTypeList.Codes.Tif));
			Assert("Attachment types should contain HTML.", Document.Lookups.OD_AttachmentType_List.ContainsCode(AttachmentTypeList.Codes.Html));
			Assert("Attachment types should contain HTMF.", Document.Lookups.OD_AttachmentType_List.ContainsCode(AttachmentTypeList.Codes.Htmf));
		}

		#endregion

		#region Properties

		public void TestSuppressDocument()
		{
			Document.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			Assert(!Document.SuppressDocument);

			Document.OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;
			Assert(Document.SuppressDocument);
		}

		public void TestOD_AttachmentType()
		{
			Document.Contact.OC_AttachmentType = "XLS";

			Document.OD_DeliverBy = Constants.ContactNotifyModes.Fax;
			Assert("OD_AttachmentType should be readonly", Document.OD_AttachmentTypeInfo.ReadOnly);
			AssertEquals("OD_AttachmentType", "", Document.OD_AttachmentType);

			Document.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			Assert("OD_AttachmentType should not be readonly", !Document.OD_AttachmentTypeInfo.ReadOnly);
			AssertEquals("OD_AttachmentType", OrgConstants.AttachmentType.XLS, Document.OD_AttachmentType);

			Document.OD_DeliverBy = Constants.ContactNotifyModes.EPrint;
			Assert("OD_AttachmentType should not be readonly", !Document.OD_AttachmentTypeInfo.ReadOnly);
			AssertEquals("OD_AttachmentType", OrgConstants.AttachmentType.XLS, Document.OD_AttachmentType);

			Document.Contact.OC_AttachmentType = "PDF";

			Document.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			Assert("OD_AttachmentType should not be readonly", !Document.OD_AttachmentTypeInfo.ReadOnly);
			AssertEquals("OD_AttachmentType", OrgConstants.AttachmentType.PDF, Document.OD_AttachmentType);

			Document.OD_DeliverBy = Constants.ContactNotifyModes.EPrint;
			Assert("OD_AttachmentType should not be readonly", !Document.OD_AttachmentTypeInfo.ReadOnly);
			AssertEquals("OD_AttachmentType", OrgConstants.AttachmentType.PDF, Document.OD_AttachmentType);
		}

		public void TestOD_SU_MenuItem()
		{
			StmMenuItem electronicBLMenu = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.StartsWith, "Send Electronic Original Bill of Lading"));
			Document.OD_SU_MenuItem = electronicBLMenu.PK;
			AssertEquals("OD_DeliveryBy set to EML", Core.Constants.ContactNotifyModes.Email, Document.OD_DeliverBy);
			AssertEquals("OD_DeliveryBy ReadOnly", true, Document.OD_DeliverByInfo.ReadOnly);
			AssertEquals("OD_AttachmentType set to PDF", OrgConstants.AttachmentType.PDF, Document.OD_AttachmentType);
			AssertEquals("OD_AttachmentType ReadOnly", true, Document.OD_AttachmentTypeInfo.ReadOnly);

			StmMenuItem otherMenu = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.StartsWith, "Arrival Notice"));
			Document.OD_SU_MenuItem = otherMenu.PK;
			AssertEquals("OD_DeliveryBy not ReadOnly", false, Document.OD_DeliverByInfo.ReadOnly);
			AssertEquals("OD_AttachmentType not ReadOnly", false, Document.OD_AttachmentTypeInfo.ReadOnly);
		}

		public void TestOD_CarbonCopyRecipientsAsString()
		{
			// Arrange
			Document.OD_DeliverBy = Constants.ContactNotifyModes.Fax;
			// Act
			bool isReadOnly = Document.OD_CarbonCopyRecipientsAsStringInfo.ReadOnly;
			Document.OD_CarbonCopyRecipientsAsString = "test1@test.com, test2@test.com";
			// Assert
			Assert(isReadOnly);
			AssertEquals(string.Empty, Document.OD_CarbonCopyRecipientsAsString);

			// Arrange
			Document.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			// Act
			isReadOnly = Document.OD_CarbonCopyRecipientsAsStringInfo.ReadOnly;
			var carbonCopyRecipients = Document.CarbonCopyRecipients;
			// Assert
			Assert(!isReadOnly);
			AssertEquals(2, carbonCopyRecipients.Count);
			AssertCollectionContains(carbonCopyRecipients, ccr => ccr.ODR_EmailAddress == "test1@test.com");
			AssertCollectionContains(carbonCopyRecipients, ccr => ccr.ODR_EmailAddress == "test2@test.com");
		}

		public void TestOD_BlindCarbonCopyRecipientsAsString()
		{
			// Arrange
			Document.OD_DeliverBy = Constants.ContactNotifyModes.Fax;
			// Act
			bool isReadOnly = Document.OD_BlindCarbonCopyRecipientsAsStringInfo.ReadOnly;
			Document.OD_BlindCarbonCopyRecipientsAsString = "test1@test.com, test2@test.com";
			// Assert
			Assert(isReadOnly);
			AssertEquals(string.Empty, Document.OD_BlindCarbonCopyRecipientsAsString);

			// Arrange
			Document.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			// Act
			isReadOnly = Document.OD_BlindCarbonCopyRecipientsAsStringInfo.ReadOnly;
			Document.OD_BlindCarbonCopyRecipientsAsString = "test1@test.com, test2@test.com";
			var carbonCopyRecipients = Document.BlindCarbonCopyRecipients;
			// Assert
			Assert(!isReadOnly);
			AssertEquals(2, carbonCopyRecipients.Count);
			AssertCollectionContains(carbonCopyRecipients, ccr => ccr.ODR_EmailAddress == "test1@test.com");
			AssertCollectionContains(carbonCopyRecipients, ccr => ccr.ODR_EmailAddress == "test2@test.com");
		}

		public void TestCarbonCopyRecipientsAreDeletedUponSavingWhenDisabled()
		{
			// Arrange
			Document.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			Document.OD_CarbonCopyRecipientsAsString = "test1@test.com, test2@test.com";
			Factory.Save();
			Assert(!Document.OD_CarbonCopyRecipientsAsStringInfo.ReadOnly);
			AssertEquals(2, Document.CarbonCopyRecipients.Count);
			// Act
			Document.OD_DeliverBy = Constants.ContactNotifyModes.Fax;
			Assert(Document.OD_CarbonCopyRecipientsAsStringInfo.ReadOnly);
			Factory.Save();
			// Assert
			AssertEquals(0, Document.CarbonCopyRecipients.Count);
		}

		public void TestBlindCarbonCopyRecipientsAreDeletedUponSavingWhenDisabled()
		{
			// Arrange
			Document.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			Document.OD_BlindCarbonCopyRecipientsAsString = "test1@test.com, test2@test.com";
			Factory.Save();
			Assert(!Document.OD_BlindCarbonCopyRecipientsAsStringInfo.ReadOnly);
			AssertEquals(2, Document.BlindCarbonCopyRecipients.Count);
			// Act
			Document.OD_DeliverBy = Constants.ContactNotifyModes.Fax;
			Assert(Document.OD_BlindCarbonCopyRecipientsAsStringInfo.ReadOnly);
			Factory.Save();
			// Assert
			AssertEquals(0, Document.BlindCarbonCopyRecipients.Count);
		}

		public void TestMustSendViaEmailPDF()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			OrgDocument doc1 = contact.Documents.AddNew();
			OrgDocument doc2 = contact.Documents.AddNew();

			StmMenuItem menu1 = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.StartsWith, "Send Electronic Original Bill of Lading"));
			doc1.OD_SU_MenuItem = menu1.PK;
			StmMenuItem menu2 = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Manifest"));
			doc2.OD_SU_MenuItem = menu2.PK;

			contact.Documents.Load();
			AssertEquals("Count in Document Collection is 2", 2, contact.Documents.Count);
			foreach (OrgDocument doc in contact.Documents)
			{
				if (doc.MenuItem.PK == menu1.PK)
				{
					AssertEquals("OD_DeliveryBy for Menu1 set to EML", Core.Constants.ContactNotifyModes.Email, doc.OD_DeliverBy);
					AssertEquals("OD_DeliveryBy for Menu1 ReadOnly", true, doc.OD_DeliverByInfo.ReadOnly);
					AssertEquals("OD_AttachmentType for Menu1 set to PDF", OrgConstants.AttachmentType.PDF, doc.OD_AttachmentType);
					AssertEquals("OD_AttachmentType for Menu1 ReadOnly", true, doc.OD_AttachmentTypeInfo.ReadOnly);
				}
				else
				{
					AssertEquals("OD_DeliveryBy for Menu2 not ReadOnly", false, doc.OD_DeliverByInfo.ReadOnly);
					AssertEquals("OD_AttachmentType for Menu2 not ReadOnly", false, doc.OD_AttachmentTypeInfo.ReadOnly);
				}
			}
		}

		public void TestCheckEmailAddressWhenSettingDeliveryMethod()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Email = string.Empty;
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_Email = string.Empty;

			OrgDocument document = contact.Documents.AddNew();
			document.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			document.OD_DocumentGroup = "ALL";
			document.OD_AttachmentType = "PDF";
			document.OD_FilterShipmentMode = "ALL";
			document.OD_FilterDirection = "ALL";

			Assert("Contact should display validation error because email address is empty.", contact.HasErrors());
			AssertEquals("Error - Contact: Please enter an email address to be used for correspondence with this Organization.", contact.GetErrors().GetFirstMessage());

			document.OD_DeliverBy = Constants.ContactNotifyModes.Print;
			AssertEquals("Validation error removed when delivery method is not email.", false, contact.HasErrors());
		}

		public void TestOD_SendIndividually_ValueSaved()
		{
			AssertEquals("OD_SendIndividually", false, Document.OD_SendIndividually);

			Document.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			Document.OD_SendIndividually = true;
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var document = factory.Load<OrgDocument>(Document.PK);
			AssertEquals("OD_SendIndividually", true, document.OD_SendIndividually);
		}

		public void TestOD_SendIndividually_SetDefault_WhenDeliveryMethodChanged()
		{
			Document.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			AssertEquals("OD_SendIndividually", false, Document.OD_SendIndividually);

			Document.OD_DeliverBy = Constants.ContactNotifyModes.Print;
			AssertEquals("OD_SendIndividually", true, Document.OD_SendIndividually);

			Document.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			AssertEquals("OD_SendIndividually", false, Document.OD_SendIndividually);

			Document.OD_SendIndividually = true;
			Document.OD_DeliverBy = Constants.ContactNotifyModes.Print;
			Document.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			AssertEquals("OD_SendIndividually", false, Document.OD_SendIndividually);
		}

		public void TestOD_SendIndividually_ReadOnly()
		{
			Document.OD_DeliverBy = Constants.ContactNotifyModes.Print;
			AssertEquals("OD_SendIndividually_ReadOnly", true, Document.OD_SendIndividually_ReadOnly);

			Document.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			AssertEquals("OD_SendIndividually_ReadOnly", false, Document.OD_SendIndividually_ReadOnly);
		}

		#endregion

		#region Implementation

		OrgDocument Document;

		protected override void SetUp()
		{
			base.SetUp();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Document = org.Contacts.AddNew().Documents.AddNew();
		}

		#endregion
	}
}
