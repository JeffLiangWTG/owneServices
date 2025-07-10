using System.Collections.Generic;
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
	public class OrganisationPatternMatchingAddressRegeneratorForTest : OrganisationPatternMatchingAddressRegenerator
	{
		public OrganisationPatternMatchingAddressRegeneratorForTest(PatternMatchingRecalculator<OrgHeader> recalculator) : base(recalculator)
		{
		}

		public List<string> TablesPrefixListForTest => TablesPrefixList;
	}

	public class OrganisationPatternMatchingAddressRegeneratorTest : PatternGeneratorForOrgHeaderTest<OrganisationPatternMatchingAddressRegeneratorForTest, PatternMatchingAddress>
	{
		protected override SchemaGuidColumn PatternMatchingOrgHeaderColumn { get { return PatternMatchingAddressSchema.PMA_OH; } }

		protected override SchemaStringColumn PatternMatchingParentTableCodeColumn { get { return PatternMatchingAddressSchema.PMA_ParentTableCode; } }

		public override void TestGenerate()
		{
			#region Set up data & Test Add New Data

			var testAddressDataList = new List<OrgAddress>();
			int testAddressCount = 4;

			OrgHeader.MainAddress.Address1 = "72 ORiordan Street";
			OrgHeader.MainAddress.OA_RN_NKCountryCode = "AU";
			testAddressDataList.Add(OrgHeader.MainAddress);

			for (int i = 0; i < testAddressCount; i++)
			{
				var address = OrgHeader.Addresses.AddNew();
				address.OA_RN_NKCountryCode = "AU";
				address.OA_Code = "ACD" + i;
				address.OA_Address1 = "Bourke Street" + (i + 1).ToString();
				testAddressDataList.Add(address);
			}

			testAddressCount = testAddressDataList.Count;

			Factory.Save();

			var patternMatchingRecalculator = new PatternMatchingRecalculator<OrgHeader>(OrgHeader);
			var patternMatchingAddressGenerator = new OrganisationPatternMatchingAddressRegeneratorForTest(patternMatchingRecalculator);

			patternMatchingAddressGenerator.InitializeDataCount(OrgHeader, Factory);
			patternMatchingAddressGenerator.Regenerate(OrgHeader, Factory);

			var query = new ZQuery(PatternMatchingAddressSchema.PMA_OH, OrgHeader.PK);
			query.AddToFilter(PatternMatchingAddressSchema.PMA_ParentTableCode, patternMatchingAddressGenerator.TablesPrefixListForTest);

			var result = Factory.Load<PatternMatchingAddress>(query);
			AssertEquals("Expected: correct amount of address patterns were created", result.Length, testAddressCount);
			AssertContainsExactElementsInAnyOrder(OrgHeader.Addresses.Cast<OrgAddress>().Select(u => TextStandardizerHelper.ComputeStringHashFast(GetValueToUpper(u))), result.Select(u => (int)u.HashedValue));

			#endregion

			#region Change Data

			//delete data
			var delAddress = Factory.Load<OrgAddress>(testAddressDataList[testAddressCount - 1].PK);
			ZGuid delAddressPK = delAddress.PK;
			delAddress.Delete();
			testAddressCount--;

			//update data
			var updateAddress1 = Factory.Load<OrgAddress>(testAddressDataList[testAddressCount - 1].PK);
			updateAddress1.OA_Address2 = "Bell Street CC2";
			updateAddress1.OA_RN_NKCountryCode = "CN";

			var updateAddress2 = Factory.Load<OrgAddress>(testAddressDataList[testAddressCount - 2].PK);
			updateAddress2.OA_Address1 = OrgHeader.MainAddress.Address1.ToUpper();
			updateAddress2.OA_State = OrgHeader.MainAddress.OA_State;

			//new data
			for (int i = testAddressCount; i < testAddressCount + 2; i++)
			{
				var addAddress = Factory.NewWithValidTestData<OrgAddress>();
				addAddress.OA_OH = OrgHeader.PK;
				addAddress.OA_RN_NKCountryCode = "AU";
				addAddress.OA_Address1 = "Bourke Street Wow";
				addAddress.OA_Code = "WOW" + i;
				testAddressDataList.Add(addAddress);
			}

			testAddressCount += 2;
			Factory.Save();
			#endregion

			#region ReGenerate

			patternMatchingAddressGenerator = new OrganisationPatternMatchingAddressRegeneratorForTest(patternMatchingRecalculator);

			var totalCount = patternMatchingAddressGenerator.InitializeDataCount(OrgHeader, Factory);
			int effectCount = patternMatchingAddressGenerator.Regenerate(OrgHeader, Factory);

			#endregion

			#region Assert

			var result_recalculated = Factory.Load<PatternMatchingAddress>(query);

			AssertEquals("Expected: should generate correct numbers of address patterns ", result_recalculated.Length, testAddressCount);
			AssertEquals("Expected: plan numbers must equal actual numbers ", totalCount, effectCount);

			foreach (var addr in testAddressDataList)
			{
				var mat = result_recalculated.Where(a => a.PMA_ParentId.Equals(addr.PK)).FirstOrDefault();

				if (delAddressPK.Equals(addr.PK))
				{
					AssertNull("Expected: deleted matching patterns should be null", mat);
					continue;
				}
				else if (updateAddress2.PK.Equals(addr.PK))
				{
					AssertEquals("Expected: should generator the same hashed value", result_recalculated.First(u => u.PMA_ParentId == OrgHeader.MainAddress.PK).PMA_HashedValue, mat.PMA_HashedValue);
				}
				else
				{
					AssertNotNull("Expected: matching patterns should not be null", mat);
				}

				string needHashValue = GetValueToUpper(addr);
				int correctHashValue = TextStandardizerHelper.ComputeStringHashFast(needHashValue);

				AssertEquals("Expected: hash value must be equal ", correctHashValue, mat.PMA_HashedValue);
				Assert("Expected: country code  must be equal ", mat.Country.Equals(addr.Country));
			}

			#endregion
		}

		protected override void AssertResult(PatternMatchingAddress[] result)
		{
			AssertEquals("Expected: Placeholder address is not added", 0, result.Length);
		}

		protected override void CreateExtraBusinessObjects()
		{
			OrgHeader.MainAddress.Address1 = "Test";
		}

		protected override void CreateExtraPatternMatchingRecords()
		{
			var patternRecord = Factory.NewWithValidTestData<PatternMatchingAddress>();

			patternRecord.PMA_HashedValue = 2238232;
			patternRecord.PMA_IsActive = true;
			patternRecord.PMA_OH = OrgHeader.PK;
			patternRecord.PMA_ParentId = OrgHeader.MainAddress.PK;
			patternRecord.PMA_ParentTableCode = "OA";
			patternRecord.PMA_RN_NKCountryCode = OrgHeader.CountryCode;
		}

		string GetValueToUpper(OrgAddress address)
		{
			var valueToHash = address.OA_Address1 + address.OA_Address2 + address.OA_City + address.OA_PostCode + address.OA_State;

			return valueToHash == null ? string.Empty : (address.OA_Address1 + address.OA_Address2 + address.OA_City + address.OA_PostCode + address.OA_State).ToUpperInvariant();
		}
	}
}
