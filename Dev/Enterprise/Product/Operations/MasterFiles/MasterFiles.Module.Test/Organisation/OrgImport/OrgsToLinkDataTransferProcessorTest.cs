using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing
{
	sealed class OrgsToLinkDataTransferProcessorTest : TestCaseWithFactory
	{
		public void TestImportOrgFromDataFiles()
		{
			int progressCount = 0;
			var collection = new OrgFlattenedCollection(Factory);
			var orgsToLinkDict = new Dictionary<OrgFlattened, OrgHeader>();
			PopulateCollction(collection);

			var orgFlattenedProcessor = new OrgFlattenedDataTransferProcessor(new ImportCollectionInfoImplForOrgFlattened(collection), orgsToLinkDict);
			orgFlattenedProcessor.Import();

			var orgsToLinkProcessor = new OrgsToLinkDataTransferProcessor(collection, orgsToLinkDict);
			orgsToLinkProcessor.ProgressChanged += new DataTransferProcessor.ProgressChangedEventHandler((int percentageCompleted, string status) =>
			{
				if (progressCount == 0)
				{
					AssertEquals("Checking if there are related organizations to be linked.", status);
				}
				else
				{
					AssertEquals("Linking related organizations (" + progressCount + " of 2) ...", status);
				}
				AssertEquals(progressCount * 50, percentageCompleted);
				progressCount++;
				return true;
			});

			orgsToLinkProcessor.Import();

			ValidateImportResults();
		}

		void PopulateCollction(OrgFlattenedCollection collection)
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
			org1.CustomsAgent = "ORG2";

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

			OrgFlattened org3 = collection.AddNew();
			org3.OH_Code = "ORG3";
			org3.OH_FullName = "Organisation Three";
			org3.OA_Address1 = "ABC ST";
			org3.OA_Address2 = "CBA";
			org3.Country = "AU";
			org3.OA_City = "Sydney";
			org3.OA_State = "NSW";
			org3.OA_PostCode = "2000";
			org3.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
			org3.OH_RL_NKClosestPort = "AUSYD";
			org3.OA_Phone = "9787890";
			org3.OA_Fax = "709394";
			org3.OA_Email = "orgc@orgthree.com";
			org3.Debtor = "ORG1";
			org3.CustomsAgent = "ORG4";
		}

		void ValidateImportResults()
		{
			var query = new ZQuery { FetchOnlyFromLocalCache = true };
			var orgHeaders = Factory.Load<OrgHeader>(query);
			var orgAddress = Factory.Load<OrgAddress>(query);

			AssertEquals("OrgHeaders count", 3, orgHeaders.Length);
			AssertEquals("OrgAddress count", 3, orgAddress.Length);

			OrgHeader org1 = LoadOrganisationByCode("ORGONEBNE");
			AssertEquals("Organisation One".ToUpper(), org1.OH_FullName);
			OrgHeader relatedParty1 = org1.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, ZString.Empty);
			AssertNotNull(relatedParty1);
			AssertEquals("ORG2", relatedParty1.LegacyCode);

			OrgHeader org3 = LoadOrganisationByCode("ORGTHRSYD");
			AssertEquals("Organisation Three".ToUpper(), org3.OH_FullName);
			OrgHeader relatedParty2 = org3.GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Pickup);
			AssertNotNull(relatedParty2);
			AssertEquals("ORG1", relatedParty2.LegacyCode);
		}

		OrgHeader LoadOrganisationByCode(ZString code)
		{
			var query = new ZQuery { FetchOnlyFromLocalCache = true };
			query.AddToFilter(OrgHeaderSchema.OH_Code, code);

			var foundOrg = Factory.LoadTop1<OrgHeader>(query);

			AssertNotNull("Org", foundOrg);

			return foundOrg;
		}
	}
}
