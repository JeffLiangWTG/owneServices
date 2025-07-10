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
	public class OrganisationPatternMatchingRegCodeRegeneratorForTest : OrganisationPatternMatchingRegCodeRegenerator
	{
		public OrganisationPatternMatchingRegCodeRegeneratorForTest(PatternMatchingRecalculator<OrgHeader> recalculator) : base(recalculator)
		{
		}

		public List<string> TablesPrefixListForTest => TablesPrefixList;
	}

	public class OrganisationPatternMatchingRegCodeRegeneratorTest : PatternGeneratorForOrgHeaderTest<OrganisationPatternMatchingRegCodeRegeneratorForTest, PatternMatchingRegCode>
	{
		protected override SchemaGuidColumn PatternMatchingOrgHeaderColumn { get { return PatternMatchingRegCodeSchema.PMR_OH; } }

		protected override SchemaStringColumn PatternMatchingParentTableCodeColumn { get { return PatternMatchingRegCodeSchema.PMR_ParentTableCode; } }

		public override void TestGenerate()
		{
			#region Set up data & Test Add New Data

			List<OrgCusCode> testCusCodeDataList = new List<OrgCusCode>();
			int testCusCodeCount = 4;
			for (int i = 0; i < testCusCodeCount; i++)
			{
				var addCusCode = Factory.NewWithValidTestData<OrgCusCode>();
				addCusCode.OK_OH = OrgHeader.PK;
				addCusCode.OK_CustomsRegNo = "123456" + ((char)(96 + i)).ToString();
				addCusCode.OK_CodeType = "CC" + ((char)(96 + i)).ToString();
				addCusCode.OK_RN_NKCodeCountry = "AU";
				testCusCodeDataList.Add(addCusCode);
			}

			Factory.Save();

			var patternMatchingRecalculator = new PatternMatchingRecalculator<OrgHeader>(OrgHeader);
			var patternMatchingCusCodeGenerator = new OrganisationPatternMatchingRegCodeRegeneratorForTest(patternMatchingRecalculator);

			patternMatchingCusCodeGenerator.InitializeDataCount(OrgHeader, Factory);
			patternMatchingCusCodeGenerator.Regenerate(OrgHeader, Factory);
			var query = new ZQuery(PatternMatchingRegCodeSchema.PMR_OH, OrgHeader.PK);
			query.AddToFilter(PatternMatchingRegCodeSchema.PMR_ParentTableCode, patternMatchingCusCodeGenerator.TablesPrefixListForTest);
			var result = Factory.Load<PatternMatchingRegCode>(query);
			AssertEquals("Expected: correct amount of regcode patterns were created.", result.Length, testCusCodeCount);

			#endregion

			#region Change Data

			//delete data
			var delCusCode = Factory.Load<OrgCusCode>(testCusCodeDataList[testCusCodeCount - 1].PK);
			ZGuid delCusCodePK = delCusCode.PK;
			delCusCode.Delete();
			testCusCodeCount--;

			//update data
			var updateCusCode = Factory.Load<OrgCusCode>(testCusCodeDataList[testCusCodeCount - 1].PK);
			updateCusCode.OK_CustomsRegNo = "857 55B";

			var blankCusCode = Factory.Load<OrgCusCode>(testCusCodeDataList[testCusCodeCount - 1].PK);
			ZGuid blankCusCodePk = blankCusCode.PK;
			blankCusCode.OK_CustomsRegNo = "";

			//new data
			for (int i = testCusCodeCount; i < testCusCodeCount + 2; i++)
			{
				var addCusCode = Factory.NewWithValidTestData<OrgCusCode>();
				addCusCode.OK_OH = OrgHeader.PK;
				addCusCode.OK_CustomsRegNo = "654321" + ((char)(96 + i)).ToString();
				addCusCode.OK_RN_NKCodeCountry = "CN";
				testCusCodeDataList.Add(addCusCode);
			}

			testCusCodeCount += 2;
			Factory.Save();

			#endregion

			#region Regenerate

			patternMatchingCusCodeGenerator = new OrganisationPatternMatchingRegCodeRegeneratorForTest(patternMatchingRecalculator);
			int totalCount = patternMatchingCusCodeGenerator.InitializeDataCount(OrgHeader, Factory);
			int effectCount = patternMatchingCusCodeGenerator.Regenerate(OrgHeader, Factory);

			#endregion

			#region Assert

			var result_recalculated = Factory.Load<PatternMatchingRegCode>(query);

			AssertEquals("Expected:should regenerate correct amount of patterns ", result_recalculated.Length, testCusCodeCount - 1);
			AssertEquals("Expected:plan numbers must equal actual numbers ", totalCount, effectCount);

			foreach (var cusCode in testCusCodeDataList)
			{
				var mat = result_recalculated.Where(a => a.PMR_ParentId.Equals(cusCode.PK)).FirstOrDefault();

				if (delCusCodePK.Equals(cusCode.PK) || blankCusCodePk.Equals(cusCode.PK))
				{
					AssertNull("Expected: deleted matching patterns should be null", mat);
					continue;
				}
				else
				{
					AssertNotNull("Expected: matching patterns should not be null", mat);
				}

				string needHashValue = Utils.RemoveAllWhiteSpaceCharacters(cusCode.OK_CustomsRegNo);
				int correctHashValue = TextStandardizerHelper.ComputeStringHashFast(needHashValue);

				AssertEquals("Expected: hash value must be corrected ", correctHashValue, mat.PMR_HashedValue);
				Assert("Expected: country Code  must be corrected ", mat.PMR_RN_NKCountryCode.Equals(cusCode.OK_RN_NKCodeCountry));
			}

			#endregion
		}

		protected override void AssertResult(PatternMatchingRegCode[] result)
		{
			AssertEquals("Expected: Placeholder regcode is not added", 0, result.Length);
		}

		OrgCusCode orgCusCode;

		protected override void CreateExtraBusinessObjects()
		{
			orgCusCode = OrgHeader.CustomsCodes.AddNew();

			orgCusCode.OK_CustomsRegNo = "1111111";
		}

		protected override void CreateExtraPatternMatchingRecords()
		{
			var patternRecord = Factory.NewWithValidTestData<PatternMatchingRegCode>();

			patternRecord.PMR_HashedValue = 2238232;
			patternRecord.PMR_IsActive = true;
			patternRecord.PMR_OH = OrgHeader.PK;
			patternRecord.PMR_ParentId = orgCusCode.PK;
			patternRecord.PMR_ParentTableCode = "OK";
			patternRecord.PMR_RN_NKCountryCode = OrgHeader.CountryCode;
		}
	}
}
