using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.MasterData.Business.Tests
{
	public class OrgHeaderMatchEngineFinderTest : TestCaseWithFactory
	{
		#region GetPatternMatchingResultModels

		public void TestGetPatternMatchingResultModels_FiltersBasedOnOrgPK()
		{
			var master = Factory.NewWithValidTestData<OrgHeader>();
			master.OH_FullName = "COSTCO PTY";
			master.MainAddress.OA_Address1 = "72 O'Riordan Street";
			master.MainAddress.OA_City = "SYDNEY";
			master.MainAddress.OA_PostCode = "2015";
			master.MainAddress.OA_State = "NSW";

			var masterHashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(master.OH_FullName, "AU"));

			var target1 = Factory.NewWithValidTestData<OrgHeader>();
			target1.OH_FullName = "COSTCO PTY";
			target1.MainAddress.OA_Address1 = "72 O'Riordan Street";
			target1.MainAddress.OA_City = "SYDNEY";
			target1.MainAddress.OA_PostCode = "2015";
			target1.MainAddress.OA_State = "NSW";
			CreatePatternMatchingName(target1, masterHashedValue);

			var target2 = Factory.NewWithValidTestData<OrgHeader>();
			target2.OH_FullName = "COSTCO";
			target2.MainAddress.OA_Address1 = "72 O'Riordan Street";
			target2.MainAddress.OA_City = "SYDNEY";
			target2.MainAddress.OA_PostCode = "2015";
			target2.MainAddress.OA_State = "NSW";
			CreatePatternMatchingName(target2, masterHashedValue);

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.UXMLOrganisationMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 50))
			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				((IDeduplicatable)master).ShouldRunDeduplication = true;

				var dedupOrgMaster = new DeduplicationOrgHeader(master);
				var duplicationFinder = new OrgHeaderMatchEngineFinderForTest(dedupOrgMaster, false, true, onlyScoreTargetsWithThisOrgPK: target1.PK);

				var duplicates = duplicationFinder.FindPotentialDuplicates(false);

				AssertEquals("All except target 1 should be filtered out", target1.PK, duplicates.Single().TargetPK);
			}
		}

		public void TestGetPatternMatchingResultModels_DoesNotFilterIfNoPK()
		{
			var master = Factory.NewWithValidTestData<OrgHeader>();
			master.OH_FullName = "COSTCO PTY";
			master.MainAddress.OA_Address1 = "72 O'Riordan Street";
			master.MainAddress.OA_City = "SYDNEY";
			master.MainAddress.OA_PostCode = "2015";
			master.MainAddress.OA_State = "NSW";

			var masterHashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(master.OH_FullName, "AU"));

			var target1 = Factory.NewWithValidTestData<OrgHeader>();
			target1.OH_FullName = "COSTCO PTY";
			target1.MainAddress.OA_Address1 = "72 O'Riordan Street";
			target1.MainAddress.OA_City = "SYDNEY";
			target1.MainAddress.OA_PostCode = "2015";
			target1.MainAddress.OA_State = "NSW";
			CreatePatternMatchingName(target1, masterHashedValue);

			var target2 = Factory.NewWithValidTestData<OrgHeader>();
			target2.OH_FullName = "COSTCO";
			target2.MainAddress.OA_Address1 = "72 O'Riordan Street";
			target2.MainAddress.OA_City = "SYDNEY";
			target2.MainAddress.OA_PostCode = "2015";
			target2.MainAddress.OA_State = "NSW";
			CreatePatternMatchingName(target2, masterHashedValue);

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.UXMLOrganisationMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 50))
			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				((IDeduplicatable)master).ShouldRunDeduplication = true;

				var dedupOrgMaster = new DeduplicationOrgHeader(master);
				var duplicationFinder = new OrgHeaderMatchEngineFinderForTest(dedupOrgMaster, false, true);

				var duplicates = duplicationFinder.FindPotentialDuplicates(false);

				AssertContainsExactElementsInAnyOrder("No filtering, all targets should be present", new[] { target1.PK, target2.PK }, duplicates.Select(d => d.TargetPK));
			}
		}

		#endregion

		#region ExcludesWithScore

		OrgHeader[] SetScoreExclusionOrgs()
		{
			// Master
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "COSTCO PTY";
			org.MainAddress.OA_Address1 = "72 O'Riordan Street";
			org.MainAddress.OA_City = "SYDNEY";
			org.MainAddress.OA_PostCode = "2015";
			org.MainAddress.OA_State = "NSW";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "John Masden";
			contact.OC_Phone = "+61449743938";

			// setup the pattern match, forcing targets got the same hash as the Master
			var masterHashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(org.OH_FullName, "AU"));

			// Targets
			// Percent: 100%
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "COSTCO PTY";
			org1.MainAddress.OA_Address1 = "72 O'Riordan Street";
			org1.MainAddress.OA_City = "SYDNEY";
			org1.MainAddress.OA_PostCode = "2015";
			org1.MainAddress.OA_State = "NSW";
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "John Masden";
			contact1.OC_Phone = "+61449743938";
			CreatePatternMatchingName(org1, masterHashedValue);

			// Percent: 93%
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "COSTCO";
			org2.MainAddress.OA_Address1 = "72 O'Riordan Street";
			org2.MainAddress.OA_City = "SYDNEY";
			org2.MainAddress.OA_PostCode = "2015";
			org2.MainAddress.OA_State = "NSW";
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "John Masden";
			contact2.OC_Phone = "+61449743938";
			CreatePatternMatchingName(org2, masterHashedValue);

			// Percent: 60%
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_FullName = "COSTCO";
			org3.MainAddress.OA_Address1 = "Anderson Street";
			org3.MainAddress.OA_City = "SYDNEY";
			org3.MainAddress.OA_PostCode = "2032";
			org3.MainAddress.OA_State = "NSW";
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_ContactName = "John Smith";
			contact3.OC_Phone = "+61449742654";
			CreatePatternMatchingName(org3, masterHashedValue);

			// Percent: 52%
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			org4.OH_FullName = "SINTCO";
			org4.MainAddress.OA_Address1 = "Anderson Street";
			org4.MainAddress.OA_City = "SYDNEY";
			org4.MainAddress.OA_PostCode = "2032";
			org4.MainAddress.OA_State = "NSW";
			var contact4 = org4.Contacts.AddNew();
			contact4.OC_ContactName = "James Smith";
			contact4.OC_Phone = "+6141008249";
			CreatePatternMatchingName(org4, masterHashedValue);

			// Percent: 47%
			var org4_1 = Factory.NewWithValidTestData<OrgHeader>();
			org4_1.OH_FullName = "SINECO";
			org4_1.MainAddress.OA_Address1 = "Anderson Street";
			org4_1.MainAddress.OA_City = "SYDNEY";
			org4_1.MainAddress.OA_PostCode = "2032";
			org4_1.MainAddress.OA_State = "NSW";
			var contact4_1 = org4_1.Contacts.AddNew();
			contact4_1.OC_ContactName = "James Smith";
			contact4_1.OC_Phone = "+6141008249";
			CreatePatternMatchingName(org4_1, masterHashedValue);

			// Percent: 3%
			var org5 = Factory.NewWithValidTestData<OrgHeader>();
			org5.OH_FullName = "SCP FOUNDATION AU";
			org5.MainAddress.OA_Address1 = "HYDE PARK";
			org5.MainAddress.OA_City = "SYDNEY";
			org5.MainAddress.OA_PostCode = "2601";
			org5.MainAddress.OA_State = "ACT";
			var contact5 = org5.Contacts.AddNew();
			contact5.OC_ContactName = "Alto Clef";
			contact5.OC_Phone = "+6165248524";
			CreatePatternMatchingName(org5, masterHashedValue);

			Factory.Save();

			return new[] { org, org1, org2, org3, org4, org4_1, org5 };
		}

		public void TestExcludesWithScoreThreshold_0()
		{
			AssertExcludesWithScoreThreshold(0, 6);
		}

		public void TestExcludesWithScoreThreshold_40()
		{
			AssertExcludesWithScoreThreshold(0.40, 5);
		}

		public void TestExcludesWithScoreThreshold_55()
		{
			AssertExcludesWithScoreThreshold(0.55, 3);
		}

		public void TestExcludesWithScoreThreshold_95()
		{
			AssertExcludesWithScoreThreshold(0.95, 1);
		}

		void AssertExcludesWithScoreThreshold(double scoreThreshold, int expectedMatchCount)
		{
			var orgs = SetScoreExclusionOrgs();
			var orgMaster = orgs[0];
			var orgTargets = orgs.Skip(1).ToArray();

			var scorePercentage = (int)Math.Floor(scoreThreshold * 100);
			using (OrganisationsDataRegistry.Instance.UXMLOrganisationMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, scorePercentage))
			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				((IDeduplicatable)orgMaster).ShouldRunDeduplication = true;

				var dedupOrgMaster = new DeduplicationOrgHeader(orgMaster);
				var duplicationFinder = new OrgHeaderMatchEngineFinderForTest(dedupOrgMaster, false, true);
				duplicationFinder.SetMaximumScoringResult(10);

				var duplicates = duplicationFinder.FindPotentialDuplicates(false).ToList().OrderBy(dup => dup.Score);

				foreach (var dup in duplicates)
				{
					AssertGreaterThan(dup.Score, scoreThreshold);
				}
				AssertEquals(expectedMatchCount, duplicates.Count());
			}
		}
		#endregion

		#region Implementation

		PatternMatchingName CreatePatternMatchingName(OrgHeader orgheader, int hashedValue)
		{
			var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_OH = orgheader.PK;
			patternMatchingName.PMN_ParentId = orgheader.PK;
			patternMatchingName.PMN_HashedValue = hashedValue;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";
			patternMatchingName.PMN_ParentTableCode = "OH";
			patternMatchingName.PMN_IsActive = true;
			return patternMatchingName;
		}

		public class OrgHeaderMatchEngineFinderForTest : OrgHeaderMatchEngineFinder
		{
			public OrgHeaderMatchEngineFinderForTest(DeduplicationOrgHeader header, bool shouldUseCache, bool useMaxRecords, ZGuid onlyScoreTargetsWithThisOrgPK)
				: base(header, shouldUseCache, useMaxRecords, onlyScoreTargetsWithThisOrgPK)
			{
			}

			public OrgHeaderMatchEngineFinderForTest(DeduplicationOrgHeader header, bool shouldUseCache, bool useMaxRecords)
				: base(header, shouldUseCache, useMaxRecords)
			{
			}

			int maxScoringResult = 4;

			public void SetMaximumScoringResult(int maxNum)
			{
				maxScoringResult = maxNum;
			}

			protected override int MaxScoringResult => maxScoringResult;

			public void GenerateTargetGlows(OrgHeader[] targetHeaderBizos)
			{
				var list = new List<IOrgHeader>();
				foreach (var target in targetHeaderBizos)
				{
					list.Add(ConvertMasterToGlowModel(target));
				}
				TargetGlows = list;
			}
		}

		#endregion

	}
}

