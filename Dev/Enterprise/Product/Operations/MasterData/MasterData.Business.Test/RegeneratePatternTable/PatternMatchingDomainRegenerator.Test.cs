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
	public class PatternMatchingDomainRegeneratorForTest<TBizo> : PatternMatchingDomainRegenerator<TBizo>
	where TBizo : BusinessObject, IDeduplicatable, IPatternMatchingRegenerationEntities<TBizo>
	{
		public PatternMatchingDomainRegeneratorForTest(PatternMatchingRecalculator<TBizo> recalculator) : base(recalculator)
		{
			patternMatchingRecalculator = recalculator;
		}

		public List<string> TablesPrefixListForTest => TablesPrefixList;
	}

	public class PatternMatchingDomainGeneratorForOrgHeaderTest : PatternGeneratorForOrgHeaderTest<PatternMatchingDomainRegeneratorForTest<OrgHeader>, PatternMatchingDomain>
	{
		protected override SchemaGuidColumn PatternMatchingOrgHeaderColumn { get { return PatternMatchingDomainSchema.PMD_OH; } }

		protected override SchemaStringColumn PatternMatchingParentTableCodeColumn { get { return PatternMatchingDomainSchema.PMD_ParentTableCode; } }

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

			int testOrgWebURLCount = 3;
			List<OrgWebURL> testOrgWebURLDataList = new List<OrgWebURL>();

			OrgHeader.MainWebURL.PU_URL = "www.wisetechglobal.com.au";
			testOrgWebURLDataList.Add(OrgHeader.MainWebURL);

			for (int i = 0; i < testOrgWebURLCount; i++)
			{
				var orgWebURL = OrgHeader.OrgWebURLs.AddNew();
				orgWebURL.PU_URL = string.Format(CultureInfo.InvariantCulture, "www.wisetechglobal{0}.com.au", i);
				testOrgWebURLDataList.Add(orgWebURL);
			}

			testOrgWebURLCount = testOrgWebURLDataList.Count;

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
			var patternMatchingDomainGenerator = new PatternMatchingDomainRegeneratorForTest<OrgHeader>(patternMatchingRecalculator);

			patternMatchingDomainGenerator.InitializeDataCount(OrgHeader, Factory);
			patternMatchingDomainGenerator.Regenerate(OrgHeader, Factory);
			var query = new ZQuery(PatternMatchingDomainSchema.PMD_OH, OrgHeader.PK);
			query.AddToFilter(PatternMatchingDomainSchema.PMD_ParentTableCode, patternMatchingDomainGenerator.TablesPrefixListForTest);
			var result = Factory.Load<PatternMatchingDomain>(query);

			Assert("Expected:correct amount of domain patterns should be created.", result.Length == testAddressCount + testOrgContactCount + testOrgWebURLCount);

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
			updateAddress.OA_Email = "stephen@wisetechglobal.com";
			updateAddress.OA_RN_NKCountryCode = "CN";

			var blankContact = Factory.Load<OrgContact>(testOrgContactDataList[testOrgContactCount - 1].PK);
			blankContact.OC_Email = "";
			ZGuid blankContactPK = blankContact.PK;

			//new data
			for (int i = testOrgWebURLCount; i < testOrgWebURLCount + 2; i++)
			{
				var orgWebURL = OrgHeader.OrgWebURLs.AddNew();
				orgWebURL.PU_URL = string.Format(CultureInfo.InvariantCulture, "www.toll{0}.com.au", i);
				testOrgWebURLDataList.Add(orgWebURL);
			}

			testOrgWebURLCount += 2;
			Factory.Save();

			#endregion

			#region Regenerate

			patternMatchingDomainGenerator = new PatternMatchingDomainRegeneratorForTest<OrgHeader>(patternMatchingRecalculator);
			int totalCount = patternMatchingDomainGenerator.InitializeDataCount(OrgHeader, Factory);
			int effectCount = patternMatchingDomainGenerator.Regenerate(OrgHeader, Factory);

			#endregion

			#region Assert

			var result_recalculated = Factory.Load<PatternMatchingDomain>(query);

			AssertEquals("Expected: the amount of patterns should be correct ", result_recalculated.Length, testAddressCount + testOrgContactCount + testOrgWebURLCount - 1);
			AssertEquals("Expected: plan numbers must equal actual numbers ", totalCount, effectCount);

			foreach (var addr in testAddressDataList)
			{
				var mat = result_recalculated.Where(a => a.PMD_ParentId.Equals(addr.PK)).FirstOrDefault();
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
				int correctHashValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.ExtractEmailDomain(needHashValue));
				AssertEquals("Expected:hash value must be corrected ", correctHashValue, mat.PMD_HashedValue);
				Assert("Expected: country Code  must be corrected ", mat.Country.Equals(addr.Country));
			}

			foreach (var contact in testOrgContactDataList)
			{
				var mat = result_recalculated.Where(a => a.PMD_ParentId.Equals(contact.PK)).FirstOrDefault();

				if (blankContactPK.Equals(contact.PK) || delContactPK.Equals(contact.PK))
				{
					AssertNull("Expected: deleted matching patterns should be null", mat);
					continue;
				}
				else
				{
					AssertNotNull("Expected: matching patterns should not be null", mat);
				}
				string needHashValue = contact.OC_Email;
				int correctHashValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.ExtractEmailDomain(needHashValue));

				AssertEquals("Expected: hash value must be equal ", correctHashValue, mat.PMD_HashedValue);
				string correctCountryCode = OrgHeader.CountryCode;
				Assert("Expected: country code  must be equal ", mat.PMD_RN_NKCountryCode.Equals(correctCountryCode));
			}

			foreach (var webUrl in testOrgWebURLDataList)
			{
				var mat = result_recalculated.Where(a => a.PMD_ParentId.Equals(webUrl.PK)).FirstOrDefault();
				string needHashValue = webUrl.PU_URL;
				int correctHashValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.ExtractEmailDomain(needHashValue));

				AssertEquals("Expected: hash value must be equal ", correctHashValue, mat.PMD_HashedValue);
				Assert("Expected: country code  must be equal ", mat.PMD_RN_NKCountryCode.Equals(OrgHeader.CountryCode));
			}

			#endregion
		}

		protected override void AssertResult(PatternMatchingDomain[] result)
		{
			Assert("Expected: Placeholder domain is not added", result.Length == 0);
		}

		OrgAddress orgAddress;
		OrgWebURL webURL;

		protected override void CreateExtraBusinessObjects()
		{
			orgAddress = OrgHeader.Addresses.AddNew();
			webURL = OrgHeader.OrgWebURLs.AddNew();

			orgAddress.OA_Address1 = "72 Bouke Street";
			orgAddress.OA_PostCode = "2015";
			orgAddress.OA_City = "Sydney";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Email = "info@gmail.com";

			webURL.PU_URL = "www.yahoo.com";
		}

		protected override void CreateExtraPatternMatchingRecords()
		{
			var patternRecord = Factory.NewWithValidTestData<PatternMatchingDomain>();

			patternRecord.PMD_HashedValue = 2238232;
			patternRecord.PMD_IsActive = true;
			patternRecord.PMD_OH = OrgHeader.PK;
			patternRecord.PMD_ParentId = orgAddress.PK;
			patternRecord.PMD_ParentTableCode = "OA";
			patternRecord.PMD_RN_NKCountryCode = OrgHeader.CountryCode;
		}
	}
}
