using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgDataLoad))]
	sealed class OrganisationImportTest : OrgDataLoadTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			module = new TestOrganisationModule();
		}

		protected override void TearDown()
		{
			base.TearDown();
			module.Dispose();
		}

		TestOrganisationModule module;

		void DoDefaultImport(TempFile testFileName, bool includeWarehouse = true, bool includeBankCurrency = true, bool includeControllingAgentAndCustomer = true)
		{
			using (module)
			{
				ImportWizard wizard = new ImportWizard((module as IImportCollectionInfoProvider).ImportCollectionInfo, null,
														 new FileMapperForTest());
				wizard.FileName = testFileName.Filename;
				wizard.StartingRow = 2;

				ImportWizardMappingCollection orgLineCollection = wizard.Mapping;

				SetColumnMapping(orgLineCollection, "Code", 0);
				SetColumnMapping(orgLineCollection, "Name", 1);
				SetColumnMapping(orgLineCollection, "Address 1", 2);
				SetColumnMapping(orgLineCollection, "Address 2", 3);
				SetColumnMapping(orgLineCollection, "City", 4);
				SetColumnMapping(orgLineCollection, "State", 5);
				SetColumnMapping(orgLineCollection, "Postcode", 6);
				SetColumnMapping(orgLineCollection, "UNLOCO", 7);
				SetColumnMapping(orgLineCollection, "Country/Region", 8);
				SetColumnMapping(orgLineCollection, "Port City", 9);
				SetColumnMapping(orgLineCollection, "Phone", 10);
				SetColumnMapping(orgLineCollection, "Fax", 11);
				SetColumnMapping(orgLineCollection, "Email", 12);
				SetColumnMapping(orgLineCollection, "Web", 13);
				SetColumnMapping(orgLineCollection, "Business Registration Number", 14);
				SetColumnMapping(orgLineCollection, "Government Corporation Code", 15);
				SetColumnMapping(orgLineCollection, "Debtor", 16);
				SetColumnMapping(orgLineCollection, "Creditor", 17);
				SetColumnMapping(orgLineCollection, "Consignee", 18);
				SetColumnMapping(orgLineCollection, "Consignor", 19);
				SetColumnMapping(orgLineCollection, "Forwarder", 20);
				SetColumnMapping(orgLineCollection, "Broker", 21);
				SetColumnMapping(orgLineCollection, "Carrier", 22);
				SetColumnMapping(orgLineCollection, "Ship Line", 23);
				SetColumnMapping(orgLineCollection, "Airline", 24);
				SetColumnMapping(orgLineCollection, "Port Transport", 25);
				SetColumnMapping(orgLineCollection, "Sales Lead", 26);
				SetColumnMapping(orgLineCollection, "Services", 27);
				SetColumnMapping(orgLineCollection, "Competitor", 28);
				SetColumnMapping(orgLineCollection, "Contact Name", 29);
				SetColumnMapping(orgLineCollection, "Contact Job Title", 30);
				SetColumnMapping(orgLineCollection, "Contact Email", 31);
				SetColumnMapping(orgLineCollection, "Contact Phone", 32);
				SetColumnMapping(orgLineCollection, "Contact Mobile", 33);
				SetColumnMapping(orgLineCollection, "Contact Fax", 34);
				SetColumnMapping(orgLineCollection, "Debtor Code", 35);
				SetColumnMapping(orgLineCollection, "Debtor Group", 36);
				SetColumnMapping(orgLineCollection, "Debtor Settlement Group", 37);
				SetColumnMapping(orgLineCollection, "Currency", 38);
				SetColumnMapping(orgLineCollection, "Credit Limit", 39);
				SetColumnMapping(orgLineCollection, "Credit Rating", 40);
				SetColumnMapping(orgLineCollection, "GST", 41);
				SetColumnMapping(orgLineCollection, "Inv. Terms Standard", 42);
				SetColumnMapping(orgLineCollection, "Inv. Days Standard", 43);
				SetColumnMapping(orgLineCollection, "Inv. Terms Disbursement", 44);
				SetColumnMapping(orgLineCollection, "Inv. Days Disbursement", 45);
				SetColumnMapping(orgLineCollection, "Creditor Group", 46);
				SetColumnMapping(orgLineCollection, "Customs Agent", 47);
				SetColumnMapping(orgLineCollection, "Postal Address 1", 48);
				SetColumnMapping(orgLineCollection, "Postal Address 2", 49);
				SetColumnMapping(orgLineCollection, "Postal City", 50);
				SetColumnMapping(orgLineCollection, "Postal State", 51);
				SetColumnMapping(orgLineCollection, "Postal Postcode", 52);
				SetColumnMapping(orgLineCollection, "Delivery Address 1", 53);
				SetColumnMapping(orgLineCollection, "Delivery Address 2", 54);
				SetColumnMapping(orgLineCollection, "Delivery City", 55);
				SetColumnMapping(orgLineCollection, "Delivery State", 56);
				SetColumnMapping(orgLineCollection, "Delivery Postcode", 57);
				SetColumnMapping(orgLineCollection, "Bank", 58);
				SetColumnMapping(orgLineCollection, "Account Name", 59);
				SetColumnMapping(orgLineCollection, "Account Number", 60);
				SetColumnMapping(orgLineCollection, "BSB", 61);
				SetColumnMapping(orgLineCollection, "CCD", 62);
				SetColumnMapping(orgLineCollection, "CSC", 63);
				SetColumnMapping(orgLineCollection, "SCC", 64);
				SetColumnMapping(orgLineCollection, "CPP", 65);
				SetColumnMapping(orgLineCollection, "CCC", 66);
				SetColumnMapping(orgLineCollection, "CMP", 67);
				SetColumnMapping(orgLineCollection, "Work Notes", 68);
				SetColumnMapping(orgLineCollection, "Handling Notes", 69);
				SetColumnMapping(orgLineCollection, "Delivery Notes", 70);
				SetColumnMapping(orgLineCollection, "AR Notes", 71);
				SetColumnMapping(orgLineCollection, "AR Credit Notes", 72);
				SetColumnMapping(orgLineCollection, "AP Notes", 73);
				SetColumnMapping(orgLineCollection, "Contact Source Type", 74);
				SetColumnMapping(orgLineCollection, "Contact Date Details Verified", 75);
				SetColumnMapping(orgLineCollection, "Contact Salutation", 76);
				SetColumnMapping(orgLineCollection, "Language", 77);
				SetColumnMapping(orgLineCollection, "Main Address Language", 78);
				SetColumnMapping(orgLineCollection, "Postal Address Language", 79);
				SetColumnMapping(orgLineCollection, "Delivery Address Language", 80);

				// optional items
				int index = 80;

				if (includeBankCurrency)
				{
					SetColumnMapping(orgLineCollection, "Bank Currency", ++index);
				}

				if (includeWarehouse)
				{
					SetColumnMapping(orgLineCollection, "Warehouse", ++index);
				}

				if (includeControllingAgentAndCustomer)
				{
					SetColumnMapping(orgLineCollection, "Controlling Agent", ++index);
					SetColumnMapping(orgLineCollection, "Controlling Customer", ++index);
				}

				for (int i = 1; i <= 99; ++i)
				{
					++index;
					SetColumnMapping(orgLineCollection, "Registration Number " + i, index);
				}

				IBusinessObjectCollection collection = wizard.CollectionInfo.Collection;
				AssertNoExceptionThrown(() => wizard.ImportIntoCollection(collection));
				module.ProcessImport();
			}
		}

		protected override bool LogContains(string error)
		{
			return module.OrgFlattenedProcessor.Log.Contains(error);
		}

		protected override void TestImportOrganisationsWithInvalidNumericDataAsserts()
		{
			AssertEquals(3, module.OrgFlattenedProcessor.HeadersToCreate);
			AssertEquals(2, module.OrgFlattenedProcessor.HeadersCreated);
			AssertEquals(1, module.OrgFlattenedProcessor.HeadersExcluded);
			AssertEquals(0, module.OrgsToLinkProcessor.OrgsLinked);
			AssertEquals("Organization [Code: Z321TEST2, Name: Z321-Test Invalid Disbursement Inv Days] excluded: already importing an organization with the same code. If a multi-line record was intended, make sure the records are grouped together and all organization details are the same.\r\n", module.OrgFlattenedProcessor.Log);
		}

		protected override string errorRowIdentifier(int rowNumber)
		{
			string errorMessageIdentity = "";
			return errorMessageIdentity;
		}

		protected override void DoImportForTestImportingRegistrationDetails(TempFile testFileName)
		{
			DoDefaultImport(testFileName);
		}

		protected override void DoImportForTestImportGSTFlags(TempFile testFileName)
		{
			DoDefaultImport(testFileName);
		}

		protected override void DoImportForTestImportingRegistrationDetailThatRequiresAPremisesAddress(TempFile testFileName)
		{
			DoDefaultImport(testFileName, false, false, false);
		}

		protected override void DoImportForTestNewRegDetailColumnsDoesNotCauseDuplicateRegistrationDataIfSpecifiedInMandatoryColumns(TempFile testFileName)
		{
			DoDefaultImport(testFileName, false, false, false);
		}

		protected override void DoImportForTestCannotImportIfRegDetailContainsMissingOrInvalidFormat(TempFile testFileName)
		{
			DoDefaultImport(testFileName, false, false, false);
		}

		protected override void DoImportForTestImportOrganisation(TempFile testFileName)
		{
			DoDefaultImport(testFileName);
		}

		protected override void DoImportForTestImportOrganisationLanguageCodes(TempFile testFileName)
		{
			DoDefaultImport(testFileName, false, false, false);
		}

		protected override void DoImportForTestImportOrganisationsWithInconsistentData(TempFile testFileName)
		{
			DoDefaultImport(testFileName);
		}

		protected override void DoImportForTestImportOrganisationsWithInvalidNumericData(TempFile testFileName)
		{
			DoDefaultImport(testFileName);
		}

		protected override void DoImportForTestImportDataWithNoOrgName(TempFile testFileName)
		{
			DoDefaultImport(testFileName);
			AssertEquals(2, module.OrgFlattenedProcessor.HeadersToCreate);
			AssertEquals(1, module.OrgFlattenedProcessor.HeadersCreated);
			AssertEquals(1, module.OrgFlattenedProcessor.HeadersExcluded);
			AssertEquals(0, module.OrgsToLinkProcessor.OrgsLinked);
		}

		protected override void DoImportAndAssertForTestImportDataWithNoOrgCodes(TempFile testFileName)
		{
			DoDefaultImport(testFileName);
			AssertEquals(7, module.OrgFlattenedProcessor.HeadersToCreate);
			AssertEquals(5, module.OrgFlattenedProcessor.HeadersCreated);
			AssertEquals(2, module.OrgFlattenedProcessor.HeadersExcluded);
			AssertEquals(0, module.OrgsToLinkProcessor.OrgsLinked);
		}

		protected override void DoImportAndAssertForTestImportDataWithNoOrgCodesAgain(TempFile testFileName)
		{
			module = new TestOrganisationModule();
			DoDefaultImport(testFileName);
			AssertEquals(7, module.OrgFlattenedProcessor.HeadersToCreate);
			AssertEquals(0, module.OrgFlattenedProcessor.HeadersCreated);
			AssertEquals(7, module.OrgFlattenedProcessor.HeadersExcluded);
			AssertEquals(0, module.OrgsToLinkProcessor.OrgsLinked);
		}

		protected override void DoImportForTestOrgCodeIsUnique(TempFile testFileName)
		{
			DoDefaultImport(testFileName);
		}

		protected override void DoImportForTestImportOrganisationWithDataExceededMaxLength(TempFile testFileName)
		{
			DoDefaultImport(testFileName);
		}

		protected override void DoImportForTestGenerationOfPortCode(TempFile testFileName)
		{
			DoDefaultImport(testFileName);
		}

		protected override void DoImportForTestDrAndCrGroupsAreCreated(TempFile testFileName)
		{
			DoDefaultImport(testFileName);
		}

		protected override void DoImportForTestDrAndCrGroupsAreObtainedFromRegistryDefault(TempFile testFileName)
		{
			DoDefaultImport(testFileName);
		}

		protected override void DoImportForTestUSIdentificationCodes(TempFile testFileName)
		{
			DoDefaultImport(testFileName);
		}

		protected override void DoImportForTestImportWithoutLastColumn(TempFile testFileName)
		{
			DoDefaultImport(testFileName);
		}

		protected override void DoImportForTestImportWithRelatedOrgs(TempFile testFileName)
		{
			DoDefaultImport(testFileName);
			AssertEquals(4, module.OrgFlattenedProcessor.HeadersToCreate);
			AssertEquals(4, module.OrgFlattenedProcessor.HeadersCreated);
			AssertEquals(0, module.OrgFlattenedProcessor.HeadersExcluded);
			AssertEquals(3, module.OrgsToLinkProcessor.OrgsLinked);
			AssertNotNull("OrgsToLinkProcessor Log", module.OrgsToLinkProcessor.Log);
			Assert("OrgsToLinkProcessor Log should contain the error message", module.OrgsToLinkProcessor.Log.Contains("Organization [Code: Z321TEST2] not found on linking parse"));
		}

		static void SetColumnMapping(ImportWizardMappingCollection lineCollection, string propName, int fileColumnIndex)
		{
			ImportWizardMapping orgColMapping = lineCollection.Cast<ImportWizardMapping>().FirstOrDefault(m => m.Text == propName);

			if (orgColMapping != null)
			{
				orgColMapping.AddFileColumnIndex(fileColumnIndex);
			}
			else
			{
				throw new ArgumentException("The specified property does not exist", nameof(propName));
			}
		}
	}
}
