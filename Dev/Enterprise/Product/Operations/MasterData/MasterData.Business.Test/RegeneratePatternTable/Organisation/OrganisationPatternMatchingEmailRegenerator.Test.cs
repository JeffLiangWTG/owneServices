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
	public class OrganisationPatternMatchingEmailRegeneratorForTest : OrganisationPatternMatchingEmailRegenerator
	{
		public OrganisationPatternMatchingEmailRegeneratorForTest(PatternMatchingRecalculator<OrgHeader> recalculator) : base(recalculator)
		{
		}

		public List<string> TablesPrefixListForTest => TablesPrefixList;
	}

	public class OrganisationPatternMatchingEmailRegeneratorTest : PatternGeneratorForOrgHeaderTest<OrganisationPatternMatchingEmailRegeneratorForTest, PatternMatchingEmail>
	{
		protected override SchemaGuidColumn PatternMatchingOrgHeaderColumn { get { return PatternMatchingEmailSchema.PME_OH; } }

		protected override SchemaStringColumn PatternMatchingParentTableCodeColumn { get { return PatternMatchingEmailSchema.PME_ParentTableCode; } }

		public override void TestGenerate()
		{
			#region Set up data & Test Add New Data

			List<OrgAddress> testAddressDataList = new List<OrgAddress>();
			int testAddressCount = 4;

			OrgHeader.MainAddress.Address1 = "72 ORiordan Street";
			OrgHeader.MainAddress.OA_RN_NKCountryCode = "AU";
			OrgHeader.MainAddress.OA_Email = "edward@wisetechglobal.com";
			testAddressDataList.Add(OrgHeader.MainAddress);

			for (int i = 0; i < testAddressCount; i++)
			{
				var address = OrgHeader.Addresses.AddNew();
				address.OA_RN_NKCountryCode = "AU";
				address.OA_Code = "ACD" + i;
				address.OA_Address1 = "Bourke Street" + (i + 1).ToString();
				address.OA_Email = "edward" + (i + 1).ToString() + "@wisetechglobal.com";
				testAddressDataList.Add(address);
			}
			testAddressCount = testAddressDataList.Count;

			int testOrgContactCount = 3;
			List<OrgContact> testOrgContactDataList = new List<OrgContact>();

			for (int i = 0; i < testOrgContactCount; i++)
			{
				var contact = OrgHeader.Contacts.AddNew();
				contact.OC_Email = string.Format(CultureInfo.InvariantCulture, "tom{0}@wisetechglobal.com", i);
				contact.OC_ContactName = "Tom" + i.ToString();
				testOrgContactDataList.Add(contact);
			}
			testOrgContactCount = testOrgContactDataList.Count;

			Factory.Save();

			var patternMatchingRecalculator = new PatternMatchingRecalculator<OrgHeader>(OrgHeader);
			var patternMatchingEmailGenerator = new OrganisationPatternMatchingEmailRegeneratorForTest(patternMatchingRecalculator);

			patternMatchingEmailGenerator.InitializeDataCount(OrgHeader, Factory);
			patternMatchingEmailGenerator.Regenerate(OrgHeader, Factory);
			var query = new ZQuery(PatternMatchingEmailSchema.PME_OH, OrgHeader.PK);
			query.AddToFilter(PatternMatchingEmailSchema.PME_ParentTableCode, patternMatchingEmailGenerator.TablesPrefixListForTest);
			var result = Factory.Load<PatternMatchingEmail>(query);
			AssertEquals("Expected:correct amount of domain patterns should be created.", result.Length, testAddressCount + testOrgContactCount);

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
			updateAddress.OA_RN_NKCountryCode = "CN";

			var blankContact = Factory.Load<OrgContact>(testOrgContactDataList[testOrgContactCount - 1].PK);
			blankContact.OC_Email = "";
			ZGuid blankContactPK = blankContact.PK;

			Factory.Save();

			#endregion

			#region Regenerate

			patternMatchingEmailGenerator = new OrganisationPatternMatchingEmailRegeneratorForTest(patternMatchingRecalculator);
			int totalCount = patternMatchingEmailGenerator.InitializeDataCount(OrgHeader, Factory);
			int effectCount = patternMatchingEmailGenerator.Regenerate(OrgHeader, Factory);

			#endregion

			#region Assert

			var result_recalculated = Factory.Load<PatternMatchingEmail>(query);
			AssertEquals("Expected: the amount of patterns should be correct ", result_recalculated.Length, testAddressCount + testOrgContactCount - 1);
			AssertEquals("Expected: plan numbers must equal actual numbers ", totalCount, effectCount);

			foreach (var addr in testAddressDataList)
			{
				var mat = result_recalculated.Where(a => a.PME_ParentId.Equals(addr.PK)).FirstOrDefault();

				if (delAddressPK.Equals(addr.PK))
				{
					AssertNull("Expected: deleted matching pattern should be null", mat);
					continue;
				}
				else
				{
					AssertNotNull("Expected: matching patterns should not be null", mat);
				}
				string needHashValue = addr.OA_Email;
				int correctHashValue = TextStandardizerHelper.ComputeStringHashFast(needHashValue);
				int correctHashValue2 = TextStandardizerHelper.ComputeStringHashFast("test@www.com3");
				AssertEquals("Expected: hash value must be corrected ", correctHashValue, mat.PME_HashedValue);
				Assert("Expected: country Code  must be corrected ", mat.Country.Equals(addr.Country));
			}

			foreach (var contact in testOrgContactDataList)
			{
				var mat = result_recalculated.Where(a => a.PME_ParentId.Equals(contact.PK)).FirstOrDefault();

				if (blankContactPK.Equals(contact.PK) || delContactPK.Equals(contact.PK))
				{
					AssertNull("Expected: deleted matching pattern should be null", mat);
					continue;
				}
				else
				{
					AssertNotNull("Expected: matching patterns should not be null", mat);
				}
				string needHashValue = contact.OC_Email;
				int correctHashValue = TextStandardizerHelper.ComputeStringHashFast(needHashValue);
				AssertEquals("Hash value must be corrected ", correctHashValue, mat.PME_HashedValue);
				Assert("Country Code  must be corrected ", mat.PME_RN_NKCountryCode.Equals(OrgHeader.CountryCode));
			}

			#endregion
		}

		protected override void AssertResult(PatternMatchingEmail[] result)
		{
			AssertEquals("Expected: Placeholder email is not added", 0, result.Length);
		}

		OrgAddress orgAddress;

		protected override void CreateExtraBusinessObjects()
		{
			orgAddress = OrgHeader.Addresses.AddNew();

			orgAddress.OA_Address1 = "72 Bouke Street";
			orgAddress.OA_PostCode = "2015";
			orgAddress.OA_City = "Sydney";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Email = "info@wisetechglobal.com";
		}

		protected override void CreateExtraPatternMatchingRecords()
		{
			var patternRecord = Factory.NewWithValidTestData<PatternMatchingEmail>();

			patternRecord.PME_HashedValue = 2238232;
			patternRecord.PME_IsActive = true;
			patternRecord.PME_OH = OrgHeader.PK;
			patternRecord.PME_ParentId = orgAddress.PK;
			patternRecord.PME_ParentTableCode = "OA";
			patternRecord.PME_RN_NKCountryCode = OrgHeader.CountryCode;
		}
	}
}
