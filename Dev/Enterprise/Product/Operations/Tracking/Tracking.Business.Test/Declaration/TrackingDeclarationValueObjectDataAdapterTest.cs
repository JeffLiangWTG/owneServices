using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business.Declaration;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business.Data.Testing
{
	[TestedType(typeof(TrackingDeclarationValueObjectDataAdapter))]
	sealed class TrackingDeclarationValueObjectDataAdapterTest : ValueObjectDataAdapterTest<TrackingDeclaration, Xsd.WebShipment>
	{
		protected override ValueObjectDataAdapter<TrackingDeclaration, Xsd.WebShipment> GetNewBizObjXmlDataAdapter()
		{
			return new TrackingDeclarationValueObjectDataAdapter();
		}

		protected override string ExpectedRootCollectionElementName => "WebShipments";

		protected override string ExpectedRootElementName => "WebShipment";

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var emptyDeclaration = new TrackingDeclaration(Factory.New<BaseJobDeclaration>());
			var emptyDeclarationXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.EmptyDeclaration.xml", "EmptyDeclaration.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyDeclaration, emptyDeclarationXmlPath, ValidationKind.None, "Empty Declaration");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "TESTNUMBER";
			declaration.JE_HouseBill = "HouseBill";
			declaration.JE_OH_Importer = TestOrg.PK;
			declaration.JE_OH_Supplier = TestOrg.PK;
			declaration.JE_CartageCompleted = new ZDateTime(2005, 3, 12);
			declaration.JE_DateAtOrigin = new ZDateTime(2005, 2, 13);
			declaration.JE_DateAtFinalDestination = new ZDateTime(2005, 4, 15);
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "USLAX";
			declaration.JE_RS_NKServiceLevel = "TST";
			declaration.JE_TotalNoOfPacks = 1;
			declaration.JE_TotalNoOfPacksPackType = "KG";
			declaration.JE_TotalVolume = 2;
			declaration.JE_TotalVolumeUnit = "M3";
			declaration.JE_TotalWeight = 3;
			declaration.JE_TotalWeightUnit = "MT";
			declaration.JE_GoodsDescription = "description";

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "TEST";

			var order = Factory.NewWithValidTestData<Order>();
			declaration.AttachedOrders.Add(order);
			order.JD_Packs = 2;
			order.JD_F3_NKPackType = "ABC";
			order.JD_OrderStatus = "XYZ";
			order.JD_OrderDate = new ZDateTime(2005, 2, 3);

			declaration.Notes.RemoveAndDeleteAll();
			var note = declaration.Notes.AddNew(true, "long", "text");
			note.ST_NoteType = nameof(StmNoteVisibility.PUB);

			var populatedDeclaration = new TrackingDeclaration(declaration, TestSiteUser);

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentID = declaration.PK;

			var invoice = Factory.NewWithValidTestData<ARInvoice>(TestBusinessObjectKind.NoData);
			invoice.AH_OH = TestOrg.PK;
			invoice.AH_JH = job.PK;
			invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;

			var fullDeclarationXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.FullDeclaration.xml", "FullDeclaration.xml");
			return new BusinessObjectAndExpectedOutputFileName(populatedDeclaration, fullDeclarationXmlPath, ValidationKind.Xsd, "Populated Declaration");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => Array.Empty<BusinessObjectAndExpectedOutputFileName>();

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample() => GetEmptyBizObjSample();

		protected override TrackingDeclaration NewBusinessObject()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = Guid.NewGuid().ToString().Substring(2);
			return new TrackingDeclaration(declaration);
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
					{
						"Shipper",
						"Consignee",
						"ClientReference",
						"Consols/MasterBill",
						"Consols/ConsolMode",
						"Consols/TransportMode",
						"Consols/VesselName",
						"Consols/VoyageFlight",
						"Consols/LoadPort/Country",
						"Consols/LoadPort/City",
						"Consols/LoadPort/Value",
						"Consols/DischargePort/Country",
						"Consols/DischargePort/City",
						"Consols/DischargePort/Value",
						"Consols/ATD",
						"Consols/ATA",
						"Packings/PackType",
						"Packings/LinePrice/CurrencyCode",
						"Packings/Weight/DimensionType",
						"Packings/Volume/DimensionType",
						"Packings/Description",
						"Packings/ContainerNumber",
						"Containers/NumberOfContainers",
						"Containers/SealNumber",
						"Containers/Mode",
						"Containers/Weight/DimensionType",
						"Containers/DeliveryMode",
						"Notes/DateTime",
						"Consignee/OrganisationDetails/EDITransmissionDetails/Address",
						"Notes/NoteCreatedDateTime",
						"Shipper/OrganisationDetails/EDITransmissionDetails/Address",
						"Packings/Weight/Description",
						"Packings/Volume/Description",
						"Quantity/Description",
						"Orders/Packs/Description",
						"Weight/Description",
						"Containers/Weight/Description",
						"Size/Description",
						"Orders/QuoteNumber",
						"Custom",
						"Deliver",
						"Shipper/OrganisationDetails/BrandNames/Value",
						"Shipper/OrganisationDetails/EDICodeMappings/Relationship",
						"Shipper/OrganisationDetails/EDICodeMappings/EDICode",
						"Shipper/OrganisationDetails/EDICodeMappings/ForeignCode",
						"Consignee/OrganisationDetails/BrandNames/Value",
						"Consignee/OrganisationDetails/EDICodeMappings/Relationship",
						"Consignee/OrganisationDetails/EDICodeMappings/EDICode",
						"Consignee/OrganisationDetails/EDICodeMappings/ForeignCode",
						"DocumentLinks/Date",
						"DocumentLinks/Link",
						"DocumentLinks/Description",
						"DocumentLinks/Link",
						"RelatedInvoiceLinks/InvoiceNumber",
						"RelatedInvoiceLinks/IssuerName",
						"RelatedInvoiceLinks/TransactionType",
						"RelatedInvoiceLinks/InvoiceTerm",
						"RelatedInvoiceLinks/InvoiceDate",
						"RelatedInvoiceLinks/DueDate",
						"RelatedInvoiceLinks/Currency",
						"RelatedInvoiceLinks/FullyPaidDate",
						"RelatedInvoiceLinks/Link"
					};
			}
		}

		protected override bool IsImportFromValueObjectSupported => false;

		protected override bool IsExportToCollectionSupported => false;

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		OrgHeader TestOrg
		{
			get
			{
				if (testOrg == null)
				{
					testOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.NoData);
					testOrg.OH_FullName = "FullName";
					testOrg.OH_Code = "FULLNASYD";
					testOrg.OH_RL_NKClosestPort = "AUSYD";
					PopulateOrgAddress(testOrg.Addresses[0]);
					var contact = testOrg.Contacts.AddNew();
					contact.OC_ContactName = "ContactName";
					testOrg.MainWebURL.PU_URL = "Web";
					testOrg.PrimaryRegistrationNumber.Number = "LocalBusinessNumber";
				}
				return testOrg;
			}
		}
		OrgHeader testOrg;

		OrgContact TestContact
		{
			get
			{
				if (testContact == null)
				{
					testContact = Factory.NewWithValidTestData<OrgContact>(TestBusinessObjectKind.NoData);
					testContact.OC_OH = TestOrg.PK;
					testContact.OC_ContactName = "TestContact";
				}
				return testContact;
			}
		}
		OrgContact testContact;

		TrackingSiteUser TestSiteUser
		{
			get
			{
				if (testSiteUser == null)
				{
					testSiteUser = new TrackingSiteUser();
					TestContact.OC_WebAccessEnabled = true;
					TestContact.OC_Email = "test@cargowise.com";
					TestContact.SetHashedPassword("test");
					testSiteUser.Login(TestOrg.OH_Code, TestContact.OC_Email, TestContact.PasswordForTesting);
				}
				return testSiteUser;
			}
		}
		TrackingSiteUser testSiteUser;

		void PopulateOrgAddress(OrgAddress address)
		{
			address.OA_Code = "AUSYD";
			address.OA_Address2 = "Address2";
			address.OA_City = "City";
			address.OA_State = "State";
			address.OA_PostCode = "PostCode";
			address.OA_Phone = "Phone";
			address.OA_Mobile = "Mobile";
			address.OA_Email = "Email";
		}
	}
}
