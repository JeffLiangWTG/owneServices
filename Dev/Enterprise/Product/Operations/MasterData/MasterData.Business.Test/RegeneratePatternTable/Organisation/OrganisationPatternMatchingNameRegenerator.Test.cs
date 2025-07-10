using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	public class OrganisationPatternMatchingNameRegeneratorForTest : OrganisationPatternMatchingNameRegenerator
	{
		public OrganisationPatternMatchingNameRegeneratorForTest(PatternMatchingRecalculator<OrgHeader> recalculator) : base(recalculator)
		{
		}

		public List<string> TablesPrefixListForTest => TablesPrefixList;
	}

	public class PatternMatchingNameGeneratorForOrgHeaderTest : PatternGeneratorForOrgHeaderTest<OrganisationPatternMatchingNameRegeneratorForTest, PatternMatchingName>
	{
		protected override SchemaGuidColumn PatternMatchingOrgHeaderColumn { get { return PatternMatchingNameSchema.PMN_OH; } }

		protected override SchemaStringColumn PatternMatchingParentTableCodeColumn { get { return PatternMatchingNameSchema.PMN_ParentTableCode; } }

		public override void TestGenerate()
		{
			#region Set up data & Test Add New Data

			List<OrgAddress> testAddressDataList = new List<OrgAddress>();
			int testAddressCount = 4;

			OrgHeader.MainAddress.Address1 = "72 ORiordan Street";
			OrgHeader.MainAddress.OA_RN_NKCountryCode = "AU";
			OrgHeader.MainAddress.OA_CompanyNameOverride = "Steak";
			testAddressDataList.Add(OrgHeader.MainAddress);

			for (int i = 0; i < testAddressCount; i++)
			{
				var address = OrgHeader.Addresses.AddNew();
				address.OA_RN_NKCountryCode = "AU";
				address.OA_Code = "ACD" + i;
				address.OA_CompanyNameOverride = "Cookie" + ((char)(96 + i)).ToString();
				address.OA_Address1 = "Bourke Street" + (i + 1).ToString();
				testAddressDataList.Add(address);
			}
			testAddressCount = testAddressDataList.Count;

			int testOrgBrandsOrRelatedNameCount = 3;
			List<OrgBrandOrRelatedName> testOrgBrandsOrRelatedNameDataList = new List<OrgBrandOrRelatedName>();

			for (int i = 0; i < testOrgBrandsOrRelatedNameCount; i++)
			{
				var orgBrandsOrRelatedName = OrgHeader.BrandsOrRelatedNames.AddNew();
				orgBrandsOrRelatedName.P1_RelatedName = string.Format(CultureInfo.InvariantCulture, "Cake{0}", i);
				testOrgBrandsOrRelatedNameDataList.Add(orgBrandsOrRelatedName);
			}

			int testOrgContactCount = 3;
			List<OrgContact> testOrgContactDataList = new List<OrgContact>();

			for (int i = 0; i < testOrgContactCount; i++)
			{
				var contact = OrgHeader.Contacts.AddNew();
				contact.OC_Email = string.Format(CultureInfo.InvariantCulture, "tom{0}@wisetechglobal.com", i);
				contact.OC_ContactName = "Tom" + i.ToString();
				testOrgContactDataList.Add(contact);
			}

			int testOrgHeaderCount = 1;
			List<OrgHeader> testHeaderDataList = new List<OrgHeader>() { OrgHeader };

			Factory.Save();

			var patternMatchingRecalculator = new PatternMatchingRecalculator<OrgHeader>(OrgHeader);
			var patternMatchingNameGenerator = new OrganisationPatternMatchingNameRegeneratorForTest(patternMatchingRecalculator);

			patternMatchingNameGenerator.InitializeDataCount(OrgHeader, Factory);
			patternMatchingNameGenerator.Regenerate(OrgHeader, Factory);

			var query = new ZQuery(PatternMatchingNameSchema.PMN_OH, OrgHeader.PK);
			query.AddToFilter(PatternMatchingNameSchema.PMN_ParentTableCode, patternMatchingNameGenerator.TablesPrefixListForTest);

			var result = Factory.Load<PatternMatchingName>(query);
			AssertEquals("Expected:create correct number of patterns.", result.Length, testAddressCount + testOrgBrandsOrRelatedNameCount + testOrgHeaderCount);

			#endregion

			#region Change Data

			//delete data
			var delAddress = Factory.Load<OrgAddress>(testAddressDataList[testAddressCount - 1].PK);
			ZGuid delAddressPK = delAddress.PK;
			delAddress.Delete();
			testAddressCount--;

			var delContact = Factory.Load<OrgContact>(testOrgContactDataList[testOrgContactCount - 1].PK);
			ZGuid delContactPK = delContact.PK;
			delContact.Delete();
			testOrgContactCount--;

			//update data
			var updateAddress = Factory.Load<OrgAddress>(testAddressDataList[testAddressCount - 1].PK);
			updateAddress.Address1 = "Bell Street CC";
			updateAddress.OA_Email = "Bell@www.com";
			updateAddress.OA_CompanyNameOverride = "Onion";
			updateAddress.OA_RN_NKCountryCode = "CN";

			var updateHeader = Factory.Load<OrgHeader>(testHeaderDataList[0].PK);
			updateHeader.OH_FullName = "Hello Tomato!";

			//new data
			for (int i = testAddressCount; i < testAddressCount + 2; i++)
			{
				var addAddress = Factory.NewWithValidTestData<OrgAddress>();
				addAddress.OA_OH = OrgHeader.PK;
				addAddress.OA_RN_NKCountryCode = "AU";
				addAddress.OA_Address1 = "Bourke Street Wow";
				addAddress.OA_Code = "WOW" + i;
				addAddress.OA_CompanyNameOverride = "Cookie" + ((char)(96 + i)).ToString();
				testAddressDataList.Add(addAddress);
			}
			testAddressCount += 2;

			Factory.Save();

			#endregion

			#region Regenerate

			patternMatchingNameGenerator = new OrganisationPatternMatchingNameRegeneratorForTest(patternMatchingRecalculator);
			int totalCount = patternMatchingNameGenerator.InitializeDataCount(OrgHeader, Factory);
			int effectCount = patternMatchingNameGenerator.Regenerate(OrgHeader, Factory);

			#endregion

			#region Assert

			var result_recalculated = Factory.Load<PatternMatchingName>(query);
			AssertEquals("Expected: should generate correct numbers of name patterns ", result_recalculated.Length, testOrgHeaderCount + testAddressCount + testOrgBrandsOrRelatedNameCount);
			AssertEquals("Expected: plan numbers must equal actual numbers ", totalCount, effectCount);

			foreach (var addr in testAddressDataList)
			{
				var mat = result_recalculated.FirstOrDefault(a => a.PMN_ParentId.Equals(addr.PK));

				if (delAddressPK.Equals(addr.PK))
				{
					AssertNull("Expected: deleted matching pattern sholud be null", mat);
					continue;
				}
				else
				{
					AssertNotNull("Expected: matching patterns should not be null", mat);
				}

				string needHashValue = addr.OA_CompanyNameOverride;
				int correctHashValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(needHashValue, OrgHeader.CountryCode));
				AssertEquals("Hash value must be corrected ", correctHashValue, mat.PMN_HashedValue);
				Assert("Country Code must be corrected ", mat.Country.Equals(addr.Country));
			}

			foreach (var brandsOrRelatedName in testOrgBrandsOrRelatedNameDataList)
			{
				var mat = result_recalculated.FirstOrDefault(a => a.PMN_ParentId.Equals(brandsOrRelatedName.PK));
				string needHashValue = brandsOrRelatedName.P1_RelatedName;
				int correctHashValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(needHashValue, OrgHeader.CountryCode));
				AssertEquals("Hash value must be corrected ", correctHashValue, mat.PMN_HashedValue);
				Assert("Country Code must be corrected ", mat.PMN_RN_NKCountryCode.Equals(OrgHeader.CountryCode));
			}

			#endregion
		}

		protected override void AssertResult(PatternMatchingName[] result)
		{
			AssertEquals("Expected: Placeholder name is not added", 0, result.Length);
		}

		protected override void CreateExtraBusinessObjects()
		{
		}

		protected override void CreateExtraPatternMatchingRecords()
		{
		}
	}
}
