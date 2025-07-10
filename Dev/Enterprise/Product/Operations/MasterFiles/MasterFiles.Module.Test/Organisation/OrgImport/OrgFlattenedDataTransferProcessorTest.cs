using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing
{
	sealed class OrgFlattenedDataTransferProcessorTest : TestCaseWithFactory
	{
		public void TestOrgWithEmptyMandatoryFieldsNotImported()
		{
			var collection = new OrgFlattenedCollection(Factory);
			var processor = new OrgFlattenedDataTransferProcessorForTest(new ImportCollectionInfoImplForOrgFlattened(collection), new Dictionary<OrgFlattened, OrgHeader>());
			var orgFlattened = new OrgFlattened();
			orgFlattened.OH_FullName = "Name Of TESTORG";
			AssertNullOrEmpty(orgFlattened.OA_Address1);
			AssertNullOrEmpty(orgFlattened.Country);
			var orgImported = processor.CreateHeaderForTest(orgFlattened);
			AssertNull("It's not imported.", orgImported);
			AssertEquals(@"Organization [Code: , Name: Name Of TESTORG] excluded: cannot import row with empty Address 1, Country
", processor.Log);
			orgFlattened.Country = "CN";
			orgFlattened.OA_Address1 = "Paradise";
			orgImported = processor.CreateHeaderForTest(orgFlattened);
			AssertNotNull("It's imported.", orgImported);
		}

		public void TestCheckAddress1LengthWhenImportData()
		{
			var collection = new OrgFlattenedCollection(Factory);
			var processor = new OrgFlattenedDataTransferProcessorForTest(new ImportCollectionInfoImplForOrgFlattened(collection), new Dictionary<OrgFlattened, OrgHeader>());
			var orgFlattened = new OrgFlattened();
			orgFlattened.OH_FullName = "Name Of TESTORG";
			orgFlattened.OA_Address1 = "ABC";
			orgFlattened.Country = "AU";
			var orgImported = processor.CreateHeaderForTest(orgFlattened);
			AssertNull("It's not imported.", orgImported);
			AssertEquals($"Organization [Code: , Name: Name Of TESTORG] excluded: cannot import row with Address 1 less than {OrgAddress.Schema.OA_Address1MinimumLength} characters{System.Environment.NewLine}", processor.Log);

			processor.Log = string.Empty;
			orgFlattened.OA_Address1 = " ABC";
			orgImported = processor.CreateHeaderForTest(orgFlattened);
			AssertNull("It's not imported.", orgImported);
			AssertEquals($"Organization [Code: , Name: Name Of TESTORG] excluded: cannot import row with Address 1 less than {OrgAddress.Schema.OA_Address1MinimumLength} characters{System.Environment.NewLine}", processor.Log);

			orgFlattened.OA_Address1 = "ABCD";
			orgImported = processor.CreateHeaderForTest(orgFlattened);
			AssertNotNull("It's imported.", orgImported);
		}

		public void TestImportWithExistingOrg_WithOrgCode()
		{
			var collection = new OrgFlattenedCollection(Factory);
			var org1 = collection.AddNew();
			org1.OH_Code = "XXX";
			org1.OH_FullName = "Organization Name";
			org1.OB_IsDebtor = true;
			org1.OA_Address1 = "Address1";
			org1.Country = "AU";
			var org2 = collection.AddNew();
			org2.OH_Code = "XXX";
			org2.OH_FullName = "Organization Name";
			org2.OB_IsDebtor = false;
			org2.OA_Address1 = "Address1";
			org2.Country = "AU";

			var processor = new OrgFlattenedDataTransferProcessor(new ImportCollectionInfoImplForOrgFlattened(collection), new Dictionary<OrgFlattened, OrgHeader>());
			processor.Import();

			AssertEquals(2, processor.HeadersToCreate);
			AssertEquals(1, processor.HeadersCreated);
			AssertEquals(1, processor.HeadersExcluded);
			AssertContains("Organization [Code: XXX, Name: Organization Name] excluded: already importing an organization with the same code. If a multi-line record was intended, make sure the records are grouped together and all organization details are the same.", processor.Log);

			Factory.Save();
			processor = new OrgFlattenedDataTransferProcessor(new ImportCollectionInfoImplForOrgFlattened(collection), new Dictionary<OrgFlattened, OrgHeader>());
			processor.Import();

			AssertEquals(2, processor.HeadersToCreate);
			AssertEquals(0, processor.HeadersCreated);
			AssertEquals(2, processor.HeadersExcluded);
			AssertContains("Organization [Code: XXX, Name: Organization Name] excluded: already exists in CargoWise table", processor.Log);
		}

		public void TestImportWithExistingOrg_WithoutOrgCode()
		{
			var collection = new OrgFlattenedCollection(Factory);
			var org1 = collection.AddNew();
			org1.OH_FullName = "Organization Name";
			org1.OB_IsDebtor = true;
			org1.OA_Address1 = "Address1";
			org1.Country = "AU";
			var org2 = collection.AddNew();
			org2.OH_FullName = "Organization Name";
			org2.OB_IsDebtor = false;
			org2.OA_Address1 = "Address1";
			org2.Country = "AU";
			var processor = new OrgFlattenedDataTransferProcessor(new ImportCollectionInfoImplForOrgFlattened(collection), new Dictionary<OrgFlattened, OrgHeader>());
			processor.Import();

			AssertEquals(2, processor.HeadersToCreate);
			AssertEquals(1, processor.HeadersCreated);
			AssertEquals(1, processor.HeadersExcluded);
			AssertContains("Organization [Code: , Name: Organization Name] excluded: already importing an organization with the same name & address. If a multi-line record was intended, make sure the records are grouped together and all organization details are the same.", processor.Log);

			Factory.Save();
			processor = new OrgFlattenedDataTransferProcessor(new ImportCollectionInfoImplForOrgFlattened(collection), new Dictionary<OrgFlattened, OrgHeader>());
			processor.Import();

			AssertEquals(2, processor.HeadersToCreate);
			AssertEquals(0, processor.HeadersCreated);
			AssertEquals(2, processor.HeadersExcluded);
			AssertContains("Organization [Code: , Name: Organization Name] excluded: already exists in CargoWise table", processor.Log);
		}

		public void TestImportOrgDoesNotCreateExtraAccounDetails()
		{
			var collection = new OrgFlattenedCollection(Factory);
			var orgsToLinkDict = new Dictionary<OrgFlattened, OrgHeader>();
			var org1 = collection.AddNew();
			org1.OH_Code = "ORG1";
			org1.OH_FullName = "Organisation One";
			org1.OA_Address1 = "31 why not St";
			org1.OA_Address2 = "Bardon";
			org1.OA_City = "Brisbane";
			org1.Country = "AU";
			org1.OA_State = "QLD";
			org1.OA_PostCode = "4000";
			org1.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
			org1.OH_RL_NKClosestPort = "AUBNE";
			org1.OA_Phone = "85555555";
			org1.OA_Fax = "85555556";
			org1.OA_Email = "orga@orgone.com";
			org1.OB_IsCreditor = true;

			var importer = new OrgFlattenedDataTransferProcessor(new ImportCollectionInfoImplForOrgFlattened(collection), orgsToLinkDict);
			importer.Import();

			var newOrg = LoadOrganisationByCode("ORGONEBNE");

			AssertEquals("Precondition : OrgHeaders Is creditor", true, newOrg.OH_IsCreditor);
			AssertEquals("Account Details should not be created automatically.", true, newOrg.CompanyData.AccountDetailsCollection.IsNullOrEmpty());
		}

		public void TestImportOrgFromMultilineDataFiles()
		{
			var collection = new OrgFlattenedCollection(Factory);

			var org1_Address1_Contact1 = AddNewOrgFlattened(collection, 1, 1, 1);
			var org1_Address2_Contact1 = AddNewOrgFlattened(collection, 1, 2, 1);
			var org1_Address1_Contact2 = AddNewOrgFlattened(collection, 1, 1, 2);
			var org1_Address2_Contact2 = AddNewOrgFlattened(collection, 1, 2, 2);

			var org2_Address1_Contact1 = AddNewOrgFlattened(collection, 2, 1, 1);
			var org2_Address1_Contact1Invalid = AddNewOrgFlattened(collection, 2, 1, 1);
			org2_Address1_Contact1Invalid.OC_Email = "different.email@cargowise.com";

			var org3 = AddNewOrgFlattened(collection, 1, 1, 1);

			var processor = new OrgFlattenedDataTransferProcessor(new ImportCollectionInfoImplForOrgFlattened(collection), new Dictionary<OrgFlattened, OrgHeader>());
			processor.Import();
			AssertEquals(3, processor.HeadersToCreate);
			AssertEquals("Only org1 and org2 should have been created", 2, processor.HeadersCreated);
			AssertEquals("Org3 should be excluded because it has the same header as org1", 1, processor.HeadersExcluded);
			AssertContains("Organization [Code: ORGONEBNE, Name: Organisation One] excluded: already importing an organization with the same code. If a multi-line record was intended, make sure the records are grouped together and all organization details are the same.", processor.Log);

			var mainAddressMerger = processor.UniqueChildrenMergers.First(childMerger => childMerger is OrgFlattenedDataTransferProcessor.ChildMainAddressMerger);
			AssertEquals(7, mainAddressMerger.ChildRecordsFound);
			AssertEquals(3, mainAddressMerger.ChildRecordsCreated);
			AssertEquals(3, mainAddressMerger.DuplicateChildRecordsFound);
			AssertEquals(1, mainAddressMerger.ChildRecordsExcluded);

			var contactMerger = processor.UniqueChildrenMergers.First(childMerger => childMerger is OrgFlattenedDataTransferProcessor.ChildContactMerger);
			AssertEquals(7, contactMerger.ChildRecordsFound);
			AssertEquals(3, contactMerger.ChildRecordsCreated);
			AssertEquals(2, contactMerger.DuplicateChildRecordsFound);
			AssertEquals(2, contactMerger.ChildRecordsExcluded);
			AssertContains("Contact [Name: Andrew] for parent Organization [Code: ORGTWOBNE, Name: ORGANISATION TWO] excluded: Can not have multiple organization contacts with the same Contact Name", processor.Log);

			var query = new ZQuery { FetchOnlyFromLocalCache = true };
			var orgHeaders = Factory.Load<OrgHeader>(query);
			var orgAddresses = Factory.Load<OrgAddress>(query);
			var orgContacts = Factory.Load<OrgContact>(query);

			AssertEquals("OrgHeaders count", 2, orgHeaders.Length);
			AssertEquals("OrgAddresses count", 3, orgAddresses.Length);
			AssertEquals("OrgContacts count", 3, orgContacts.Length);

			var org1 = orgHeaders.Cast<OrgHeader>().First(org => org.OH_Code == "ORGONEBNE");
			AssertEquals("Organisation One".ToUpper(), org1.OH_FullName);
			AssertEquals("AUBNE", org1.OH_RL_NKClosestPort);
			AssertEquals(2, org1.Addresses.Count);
			AssertEquals(2, org1.Contacts.Count);

			var org1_address1 = org1.Addresses.Cast<OrgAddress>().First(address => address.OA_Address1 == "31 Blah St");
			AssertEquals("Bardon", org1_address1.OA_Address2);
			AssertEquals("Brisbane", org1_address1.OA_City);
			AssertEquals(true, org1_address1.IsMainAddress);

			var org1_address2 = org1.Addresses.Cast<OrgAddress>().First(address => address.OA_Address1 == "29 Box");
			AssertEquals("Square St", org1_address2.OA_Address2);
			AssertEquals("Shapetown", org1_address2.OA_City);
			AssertEquals("Only the first address found becomes the main address", false, org1_address2.IsMainAddress);

			var org1_contact1 = org1.Contacts.Cast<OrgContact>().First(contact => contact.OC_ContactName == "Andrew");
			AssertEquals("andrew@cargowise.com", org1_contact1.OC_Email);

			var org1_contact2 = org1.Contacts.Cast<OrgContact>().First(contact => contact.OC_ContactName == "Luong");
			AssertEquals("luong@cargowise.com", org1_contact2.OC_Email);

			var org2 = orgHeaders.Cast<OrgHeader>().First(org => org.OH_Code == "ORGTWOBNE");
			AssertEquals("Organisation Two".ToUpper(), org2.OH_FullName);
			AssertEquals("AUBNE", org2.OH_RL_NKClosestPort);
			AssertEquals(1, org2.Addresses.Count);
			AssertEquals(1, org2.Contacts.Count);

			var org2_address1 = org2.Addresses.Cast<OrgAddress>().First(address => address.OA_Address1 == "31 Blah St");
			AssertEquals("Bardon", org2_address1.OA_Address2);
			AssertEquals("Brisbane", org2_address1.OA_City);
			AssertEquals(true, org2_address1.IsMainAddress);

			var org2_contact1 = org2.Contacts.Cast<OrgContact>().First(contact => contact.OC_ContactName == "Andrew");
			AssertEquals("andrew@cargowise.com", org2_contact1.OC_Email);

			processor.Rollback();
			ValidateAfterRollback();
		}

		public void TestImportOrgFromDataFiles()
		{
			int progressCount = 1;
			var collection = new OrgFlattenedCollection(Factory);
			var orgsToLinkDict = new Dictionary<OrgFlattened, OrgHeader>();
			PopulateCollection(collection);

			var importer = new OrgFlattenedDataTransferProcessor(new ImportCollectionInfoImplForOrgFlattened(collection), orgsToLinkDict);
			importer.ProgressChanged += new DataTransferProcessor.ProgressChangedEventHandler((int percentageCompleted, string status) =>
			{
				AssertEquals("Importing organizations (" + progressCount.ToString() + " of 3) ...", status);
				AssertEquals(progressCount * 100 / 3, percentageCompleted);
				progressCount++;
				return progressCount != 4;
			});

			importer.Import();

			ValidateImportResults();

			importer.Rollback();

			ValidateAfterRollback();
		}

		public void TestMaximumLengthOfOH_FullNameShouldBe100()
		{
			// Arrange.

			var collection = new OrgFlattenedCollection(Factory);
			var org = collection.AddNew();
			org.OH_FullName = "123456789_123456789_123456789_123456789_123456789_123456789_123456789_123456789_123456789_123456789_";
			org.OB_IsDebtor = true;
			org.OA_Address1 = "Address1";
			org.Country = "AU";
			AssertEquals("Precondition", 100, org.OH_FullName.Length);

			// Act.

			var processor = new OrgFlattenedDataTransferProcessor(new ImportCollectionInfoImplForOrgFlattened(collection), new Dictionary<OrgFlattened, OrgHeader>());
			processor.Import();
			Factory.Save();

			// Assert.

			var query = new ZQuery(OrgHeaderSchema.OH_FullName, org.OH_FullName);
			var newFactory = new BusinessObjectFactory();
			var headers = newFactory.Load<OrgHeader>(query);

			AssertEquals("The length of headers should be 1.", 1, headers.Length);
			AssertEquals("Both of OH_FullName should be equal.", headers.First().OH_FullName, org.OH_FullName);
			AssertEquals("The length of OH_FullName should be 100.", 100, headers.First().OH_FullName.Length);
		}

		public void TestProcessAdditionalAddressInfoCorrectly()
		{
			var collection = new OrgFlattenedCollection(Factory);
			var org = AddNewOrgFlattened(collection, 1, 1, 1);
			org.OA_AdditionalAddressInformation = "Additional Address Information";

			var processor = new OrgFlattenedDataTransferProcessor(new ImportCollectionInfoImplForOrgFlattened(collection), new Dictionary<OrgFlattened, OrgHeader>());
			processor.Import();

			var query = new ZQuery { FetchOnlyFromLocalCache = true };
			var orgHeaders = Factory.Load<OrgHeader>(query);
			var orgAddresses = Factory.Load<OrgAddress>(query);
			AssertEquals(1, orgHeaders.Length);
			AssertEquals(1, orgAddresses.Length);
			AssertEquals("ORGONEBNE", orgHeaders[0].OH_Code);
			AssertEquals("Additional Address Information", orgAddresses[0].OA_AdditionalAddressInformation);
		}

		public void TestDuplicateShippingLine()
		{
			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;

				var org1 = Factory.NewWithValidTestData<OrgHeader>();

				var shippingLine1 = Factory.New<RefShippingLine>();
				shippingLine1.RSL_CargoWiseOneCode = "c1bb";
				shippingLine1.RSL_StandardCarrierAlphaCode = "1234";
				shippingLine1.RSL_CarrierName = "testship1";
				shippingLine1.RSL_IsActive = true;
				shippingLine1.RSL_IsSystem = true;

				org1.OH_RSL_ShippingLine = shippingLine1.PK;

				var shippingLine2 = Factory.New<RefShippingLine>();
				shippingLine2.RSL_CargoWiseOneCode = "c1aa";
				shippingLine2.RSL_StandardCarrierAlphaCode = "5678";
				shippingLine2.RSL_CarrierName = "testship2";
				shippingLine2.RSL_IsActive = true;
				shippingLine2.RSL_IsSystem = true;

				Factory.Save();

				var collection = new OrgFlattenedCollection(Factory);
				var orgFlattened1 = collection.AddNew();
				orgFlattened1.OH_Code = "XXX";
				orgFlattened1.OH_FullName = "XXXORGTEST1";
				orgFlattened1.OA_Address1 = "Org1Address1";
				orgFlattened1.OH_IsShippingLine = true;
				orgFlattened1.CarrierCode = shippingLine1.RSL_StandardCarrierAlphaCode;
				orgFlattened1.Country = "AU";

				var orgFlattened2 = collection.AddNew();
				orgFlattened2.OH_Code = "YYY";
				orgFlattened2.OH_FullName = "YYYORGTEST2";
				orgFlattened2.OA_Address1 = "Org2Address1";
				orgFlattened2.OH_IsShippingLine = true;
				orgFlattened2.CarrierCode = shippingLine2.RSL_StandardCarrierAlphaCode;
				orgFlattened2.Country = "AU";

				var processor = new OrgFlattenedDataTransferProcessorForTest(new ImportCollectionInfoImplForOrgFlattened(collection), new Dictionary<OrgFlattened, OrgHeader>());

				AssertEquals("PreCondition: Orgs to be imported", 2, collection.Count);

				processor.Import();

				AssertNoExceptionThrown(Factory.Save);

				var query1 = new ZQuery(OrgHeaderSchema.OH_FullName, orgFlattened1.OH_FullName);
				var query2 = new ZQuery(OrgHeaderSchema.OH_FullName, orgFlattened2.OH_FullName);
				var orgImport1 = new BusinessObjectFactory().LoadTop1<OrgHeader>(query1);
				var orgImport2 = new BusinessObjectFactory().LoadTop1<OrgHeader>(query2);

				CombineAssertions(() =>
				{
					AssertNull("orgImport1 should not be imported", orgImport1);
					AssertNotNull("orgImport2 should be imported", orgImport2);
					AssertEquals("HeadersCreated", 1, processor.HeadersCreated);
					AssertEquals("HeadersExcluded", 1, processor.HeadersExcluded);
					AssertContains($"Organization [Code: {orgFlattened1.OH_Code}, Name: {orgFlattened1.OH_FullName}] excluded:", processor.Log);
					AssertNotContains($"Organization [Code: {orgFlattened2.OH_Code}, Name: {orgFlattened2.OH_FullName}] excluded:", processor.Log);
				});
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCountry;
			}
		}

		public void TestImportOrg_WithOrgCodeAlgorithmContainsNumber_CanGenerateCodeWithSufficientData()
		{
			var algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 3;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
			Env.Registry.CanUserEditOrganisationCode = false;

			var collection = new OrgFlattenedCollection(Factory);
			var org1 = collection.AddNew();
			org1.OH_FullName = "Organization Name";
			org1.OB_IsDebtor = true;
			org1.OA_Address1 = "Address1";
			org1.Country = "AU";
			var processor = new OrgFlattenedDataTransferProcessor(new ImportCollectionInfoImplForOrgFlattened(collection), new Dictionary<OrgFlattened, OrgHeader>());

			processor.Import();

			AssertNoExceptionThrown(Factory.Save);
			var query1 = new ZQuery(OrgHeaderSchema.OH_FullName, org1.OH_FullName);
			var orgImport1 = new BusinessObjectFactory().LoadTop1<OrgHeader>(query1);

			AssertEquals("ORGANIZATION NAME", orgImport1.OH_FullName);
			AssertNotNullOrEmpty(orgImport1.OH_Code);
			AssertNullOrEmpty(processor.Log);
		}

		public void TestImportOrg_CanNotGenerateCodeWithInsufficientData()
		{
			var algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.SecondName].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.SecondName].Length = 3;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
			Env.Registry.CanUserEditOrganisationCode = false;

			var collection = new OrgFlattenedCollection(Factory);
			var orgWithInsufficientData = collection.AddNew();
			orgWithInsufficientData.OH_FullName = string.Empty;
			orgWithInsufficientData.OA_Address1 = "Address1";
			orgWithInsufficientData.Country = "AU";
			var processor = new OrgFlattenedDataTransferProcessor(new ImportCollectionInfoImplForOrgFlattened(collection), new Dictionary<OrgFlattened, OrgHeader>());

			processor.Import();

			AssertNoExceptionThrown(Factory.Save);
			var query1 = new ZQuery(OrgHeaderSchema.OH_FullName, orgWithInsufficientData.OH_FullName);
			var orgImport1 = new BusinessObjectFactory().LoadTop1<OrgHeader>(query1);

			AssertNullOrEmpty(orgImport1.OH_FullName);
			AssertContains($"Organization [Code: {orgWithInsufficientData.OH_Code}, Name: {orgWithInsufficientData.OH_FullName}] excluded: Cannot generate the organization code based on the data imported, please check the Full Name of the provided data.", processor.Log);
		}

		public void TestImportOrg_WithOrgCodeAlgorithmOnlyContainsSpecificNumber_CanGenerateCode()
		{
			var algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 3;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
			Env.Registry.CanUserEditOrganisationCode = false;

			var collection = new OrgFlattenedCollection(Factory);
			var org1 = collection.AddNew();
			org1.OH_FullName = "Organization Name 1";
			org1.OB_IsDebtor = true;
			org1.OA_Address1 = "Address1";
			org1.Country = "AU";

			var org2 = collection.AddNew();
			org2.OH_FullName = "Organization Name 2";
			org2.OB_IsDebtor = true;
			org2.OA_Address1 = "Address1";
			org2.Country = "AU";

			var processor = new OrgFlattenedDataTransferProcessor(new ImportCollectionInfoImplForOrgFlattened(collection), new Dictionary<OrgFlattened, OrgHeader>());

			processor.Import();

			AssertNoExceptionThrown(Factory.Save);
			var query1 = new ZQuery(OrgHeaderSchema.OH_FullName, org1.OH_FullName);
			var orgImport1 = new BusinessObjectFactory().LoadTop1<OrgHeader>(query1);

			var query2 = new ZQuery(OrgHeaderSchema.OH_FullName, org2.OH_FullName);
			var orgImport2 = new BusinessObjectFactory().LoadTop1<OrgHeader>(query2);

			AssertEquals("ORGANIZATION NAME 1", orgImport1.OH_FullName);
			AssertEquals("ORGANIZATION NAME 2", orgImport2.OH_FullName);
			AssertEquals("001", orgImport1.OH_Code);
			AssertEquals("002", orgImport2.OH_Code);
			AssertNullOrEmpty(processor.Log);
		}

		public void TestImportOrg_WithOrgCodeAlgorithmOnlyContainsGloballyNumber_CanGenerateCode()
		{
			var algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Length = 3;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
			Env.Registry.CanUserEditOrganisationCode = false;

			var collection = new OrgFlattenedCollection(Factory);
			var org1 = collection.AddNew();
			org1.OH_FullName = "Organization Name 1";
			org1.OB_IsDebtor = true;
			org1.OA_Address1 = "Address1";
			org1.Country = "AU";

			var org2 = collection.AddNew();
			org2.OH_FullName = "Organization Name 2";
			org2.OB_IsDebtor = true;
			org2.OA_Address1 = "Address1";
			org2.Country = "AU";

			var processor = new OrgFlattenedDataTransferProcessor(new ImportCollectionInfoImplForOrgFlattened(collection), new Dictionary<OrgFlattened, OrgHeader>());

			processor.Import();

			AssertNoExceptionThrown(Factory.Save);
			var query1 = new ZQuery(OrgHeaderSchema.OH_FullName, org1.OH_FullName);
			var orgImport1 = new BusinessObjectFactory().LoadTop1<OrgHeader>(query1);

			var query2 = new ZQuery(OrgHeaderSchema.OH_FullName, org2.OH_FullName);
			var orgImport2 = new BusinessObjectFactory().LoadTop1<OrgHeader>(query2);

			AssertEquals("ORGANIZATION NAME 1", orgImport1.OH_FullName);
			AssertEquals("ORGANIZATION NAME 2", orgImport2.OH_FullName);
			AssertEquals("001", orgImport1.OH_Code);
			AssertEquals("002", orgImport2.OH_Code);
			AssertNullOrEmpty(processor.Log);
		}

		#region Implementation

		OrgFlattened AddNewOrgFlattened(OrgFlattenedCollection collection, int orgVariant, int addressVariant, int contactVariant)
		{
			var orgFlattened = collection.AddNew();
			orgFlattened.Country = "AU";
			if (orgVariant == 1)
			{
				orgFlattened.OH_Code = "ORGONEBNE";
				orgFlattened.OH_FullName = "Organisation One";
				orgFlattened.OH_RL_NKClosestPort = "AUBNE";
			}
			else if (orgVariant == 2)
			{
				orgFlattened.OH_Code = "ORGTWOBNE";
				orgFlattened.OH_FullName = "Organisation Two";
				orgFlattened.OH_RL_NKClosestPort = "AUBNE";
			}

			if (addressVariant == 1)
			{
				orgFlattened.OA_Address1 = "31 Blah St";
				orgFlattened.OA_Address2 = "Bardon";
				orgFlattened.OA_City = "Brisbane";
			}
			else if (addressVariant == 2)
			{
				orgFlattened.OA_Address1 = "29 Box";
				orgFlattened.OA_Address2 = "Square St";
				orgFlattened.OA_City = "Shapetown";
			}

			if (contactVariant == 1)
			{
				orgFlattened.OC_ContactName = "Andrew";
				orgFlattened.OC_Email = "andrew@cargowise.com";
			}
			else if (contactVariant == 2)
			{
				orgFlattened.OC_ContactName = "Luong";
				orgFlattened.OC_Email = "luong@cargowise.com";
			}

			return orgFlattened;
		}

		void PopulateCollection(OrgFlattenedCollection collection)
		{
			OrgFlattened org1 = collection.AddNew();
			org1.OH_Code = "ORG1";
			org1.OH_FullName = "Organisation One";
			org1.OA_Address1 = "31 Blah St";
			org1.OA_Address2 = "Bardon";
			org1.OA_City = "Brisbane";
			org1.Country = "AU";
			org1.OA_State = "QLD";
			org1.OA_PostCode = "4000";
			org1.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
			org1.OH_RL_NKClosestPort = "AUBNE";
			org1.OA_Phone = "85555555";
			org1.OA_Fax = "85555556";
			org1.OA_Email = "orga@orgone.com";

			OrgFlattened org2 = collection.AddNew();
			org2.OH_Code = "ORG2";
			org2.OH_FullName = "Organisation Two";
			org2.OA_Address1 = "Test St";
			org2.OA_Address2 = "Blah";
			org2.OA_City = "Sydney";
			org2.Country = "AU";
			org2.OA_State = "NSW";
			org2.OA_PostCode = "2000";
			org2.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
			org2.OH_RL_NKClosestPort = "AUSYD";
			org2.OA_Phone = "22222333";
			org2.OA_Fax = "213123354";
			org2.OA_Email = "orgb@orgtwo.com";

			org2.Postal_OA_Address1 = "Postal Test St";
			org2.Postal_OA_City = "Postal";
			org2.Postal_OA_State = "QLD";
			org2.Postal_OA_PostCode = "4001";

			OrgFlattened org3 = collection.AddNew();
			org3.OH_Code = "ORG3";
			org3.OH_FullName = "Organisation Three";
			org3.OA_Address1 = "ABC ST";
			org3.OA_Address2 = "CBA";
			org3.Country = "AU";
			org3.OA_State = "NSW";
			org3.OA_PostCode = "2000";
			org3.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
			org3.OH_RL_NKClosestPort = "AUSYD";
			org3.OA_Phone = "9787890";
			org3.OA_Fax = "709394";
			org3.OA_Email = "orgc@orgthree.com";
		}

		void ValidateImportResults()
		{
			var query = new ZQuery { FetchOnlyFromLocalCache = true };
			var orgHeaders = Factory.Load<OrgHeader>(query);
			var orgAddress = Factory.Load<OrgAddress>(query);

			AssertEquals("OrgHeaders count", 2, orgHeaders.Length);
			AssertEquals("OrgAddress count", 3, orgAddress.Length);

			OrgHeader org1 = LoadOrganisationByCode("ORGONEBNE");
			AssertEquals("Organisation One".ToUpper(), org1.OH_FullName);
			AssertEquals("31 Blah St", org1.MainAddress.OA_Address1);
			AssertEquals("Bardon", org1.MainAddress.OA_Address2);
			AssertEquals("Brisbane", org1.MainAddress.OA_City);
			AssertEquals("QLD", org1.MainAddress.OA_State);
			AssertEquals("4000", org1.MainAddress.OA_PostCode);
			AssertEquals("AUBNE", org1.OH_RL_NKClosestPort);
			AssertEquals("85555555", org1.MainAddress.OA_Phone);
			AssertEquals("85555556", org1.MainAddress.OA_Fax);
			AssertEquals("orga@orgone.com", org1.MainAddress.OA_Email);

			OrgHeader org2 = LoadOrganisationByCode("ORGTWOSYD");
			AssertEquals("Organisation Two".ToUpper(), org2.OH_FullName);
			AssertEquals("Test St", org2.MainAddress.OA_Address1);
			AssertEquals("Blah", org2.MainAddress.OA_Address2);
			AssertEquals("Sydney", org2.MainAddress.OA_City);
			AssertEquals("NSW", org2.MainAddress.OA_State);
			AssertEquals("2000", org2.MainAddress.OA_PostCode);
			AssertEquals("AUSYD", org2.OH_RL_NKClosestPort);
			AssertEquals("22222333", org2.MainAddress.OA_Phone);
			AssertEquals("213123354", org2.MainAddress.OA_Fax);
			AssertEquals("orgb@orgtwo.com", org2.MainAddress.OA_Email);

			AssertEquals("The number of addresses on the organisation should be 2", 2, org2.Addresses.Count);
			OrgAddress postalAddress = org2.Addresses[1];
			AssertEquals("Postal Test St", postalAddress.OA_Address1);
			AssertEquals("Postal", postalAddress.OA_City);
			AssertEquals("QLD", postalAddress.OA_State);
			AssertEquals("4001", postalAddress.OA_PostCode);
		}

		OrgHeader LoadOrganisationByCode(ZString code)
		{
			var query = new ZQuery { FetchOnlyFromLocalCache = true };
			query.AddToFilter(OrgHeaderSchema.OH_Code, code);

			var foundOrg = Factory.LoadTop1<OrgHeader>(query);

			AssertNotNull("Org", foundOrg);

			return foundOrg;
		}

		void ValidateAfterRollback()
		{
			var query = new ZQuery { FetchOnlyFromLocalCache = true };
			var orgHeaders = Factory.Load<OrgHeader>(query);
			var orgAddress = Factory.Load<OrgAddress>(query);
			var orgContact = Factory.Load<OrgContact>(query);

			AssertEquals("OrgHeaders count", 0, orgHeaders.Length);
			AssertEquals("OrgAddress count", 0, orgAddress.Length);
			AssertEquals("OrgContact count", 0, orgContact.Length);
		}

		#endregion
	}
}
