using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.MasterData.Business.Tests
{
	public class OrgHeaderDuplicationFinderWithThresholdOverrideTest : TestCaseWithFactory
	{
		public void TestDefaultThreshold()
		{
			var finder = new OrgHeaderDuplicationFinderWithThresholdOverrideForTest(Factory.New<OrgHeader>());
			AssertEquals(ConfidenceRating.None, finder.ThresholdOverride);
		}

		public void TestFindPotentialDuplicates_WithThresholdEqualsNone_IncludesLowOrBetterResults()
		{
			const ConfidenceRating matchThreshold = ConfidenceRating.None;

			var masterOrg = Factory.NewWithValidTestData<OrgHeader>();
			masterOrg.OH_FullName = "COSTCO PTY";
			masterOrg.MainAddress.OA_Address1 = "72 O'Riordan Street";
			masterOrg.MainAddress.OA_City = "SYDNEY";
			masterOrg.MainAddress.OA_PostCode = "2015";
			masterOrg.MainAddress.OA_State = "NSW";
			var contact = masterOrg.Contacts.AddNew();
			contact.OC_ContactName = "John Masden";
			contact.OC_Phone = "+61449743938";

			var hashedMasterName = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(masterOrg.OH_FullName, "AU"));

			var mediumMatchOrg = Factory.NewWithValidTestData<OrgHeader>();
			mediumMatchOrg.OH_FullName = "COSTCO";
			mediumMatchOrg.MainAddress.OA_Address1 = "70 O'Riordan Street";
			mediumMatchOrg.MainAddress.OA_City = "SYDNEY";
			mediumMatchOrg.MainAddress.OA_PostCode = "2015";
			mediumMatchOrg.MainAddress.OA_State = "NSW";
			var mediumMatchContact = mediumMatchOrg.Contacts.AddNew();
			mediumMatchContact.OC_ContactName = "John Masden";
			mediumMatchContact.OC_Phone = "+61449742654";
			CreatePatternMatchingName(mediumMatchOrg, hashedMasterName);

			var lowMatchOrg = Factory.NewWithValidTestData<OrgHeader>();
			lowMatchOrg.OH_FullName = "SINTCO";
			lowMatchOrg.MainAddress.OA_Address1 = "Anderson Street";
			lowMatchOrg.MainAddress.OA_City = "SYDNEY";
			lowMatchOrg.MainAddress.OA_PostCode = "2032";
			lowMatchOrg.MainAddress.OA_State = "NSW";
			var lowMatchContact = lowMatchOrg.Contacts.AddNew();
			lowMatchContact.OC_ContactName = "James Smith";
			lowMatchContact.OC_Phone = "+6141008249";
			CreatePatternMatchingName(lowMatchOrg, hashedMasterName);

			var noneMatchOrg = Factory.NewWithValidTestData<OrgHeader>();
			noneMatchOrg.OH_FullName = "SCP FOUNDATION AU";
			noneMatchOrg.MainAddress.OA_Address1 = "HYDE PARK";
			noneMatchOrg.MainAddress.OA_City = "SYDNEY";
			noneMatchOrg.MainAddress.OA_PostCode = "2601";
			noneMatchOrg.MainAddress.OA_State = "ACT";
			var nonMatchContact = noneMatchOrg.Contacts.AddNew();
			nonMatchContact.OC_ContactName = "Alto Clef";
			nonMatchContact.OC_Phone = "+6165248524";
			CreatePatternMatchingName(noneMatchOrg, hashedMasterName);

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DeDuplicationMinimumConfidenceRating.Codes.Medium))
			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				((IDeduplicatable)masterOrg).ShouldRunDeduplication = true;

				var duplicationFinder = new OrgHeaderDuplicationFinderWithThresholdOverrideForTest(masterOrg, matchThreshold);
				var duplicates = duplicationFinder.FindPotentialDuplicates(false).ToList();

				AssertContainsExactElementsInAnyOrder("Should return results for medium and low matches, even though registry threshold is set to > medium",
					new[] { mediumMatchOrg.PK, lowMatchOrg.PK },
					duplicates.Select(match => match.TargetPK));

				AssertEquals(ConfidenceRating.Low, duplicates.Single(match => match.TargetPK == lowMatchOrg.PK).ConfidenceRating);
				AssertEquals(ConfidenceRating.Medium, duplicates.Single(match => match.TargetPK == mediumMatchOrg.PK).ConfidenceRating);
			}
		}

		public void TestFindPotentialDuplicates_WithThresholdEqualsLow_IncludesMediumOrBetterResults()
		{
			const ConfidenceRating matchThreshold = ConfidenceRating.Low;

			var masterOrg = Factory.NewWithValidTestData<OrgHeader>();
			masterOrg.OH_FullName = "COSTCO PTY";
			masterOrg.MainAddress.OA_Address1 = "72 O'Riordan Street";
			masterOrg.MainAddress.OA_City = "SYDNEY";
			masterOrg.MainAddress.OA_PostCode = "2015";
			masterOrg.MainAddress.OA_State = "NSW";
			var contact = masterOrg.Contacts.AddNew();
			contact.OC_ContactName = "John Masden";
			contact.OC_Phone = "+61449743938";

			var hashedMasterName = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(masterOrg.OH_FullName, "AU"));

			var mediumMatchOrg = Factory.NewWithValidTestData<OrgHeader>();
			mediumMatchOrg.OH_FullName = "COSTCO";
			mediumMatchOrg.MainAddress.OA_Address1 = "70 O'Riordan Street";
			mediumMatchOrg.MainAddress.OA_City = "SYDNEY";
			mediumMatchOrg.MainAddress.OA_PostCode = "2015";
			mediumMatchOrg.MainAddress.OA_State = "NSW";
			var mediumMatchContact = mediumMatchOrg.Contacts.AddNew();
			mediumMatchContact.OC_ContactName = "John Masden";
			mediumMatchContact.OC_Phone = "+61449742654";
			CreatePatternMatchingName(mediumMatchOrg, hashedMasterName);

			var lowMatchOrg = Factory.NewWithValidTestData<OrgHeader>();
			lowMatchOrg.OH_FullName = "SINTCO";
			lowMatchOrg.MainAddress.OA_Address1 = "Anderson Street";
			lowMatchOrg.MainAddress.OA_City = "SYDNEY";
			lowMatchOrg.MainAddress.OA_PostCode = "2032";
			lowMatchOrg.MainAddress.OA_State = "NSW";
			var lowMatchContact = lowMatchOrg.Contacts.AddNew();
			lowMatchContact.OC_ContactName = "James Smith";
			lowMatchContact.OC_Phone = "+6141008249";
			CreatePatternMatchingName(lowMatchOrg, hashedMasterName);

			var noneMatchOrg = Factory.NewWithValidTestData<OrgHeader>();
			noneMatchOrg.OH_FullName = "SCP FOUNDATION AU";
			noneMatchOrg.MainAddress.OA_Address1 = "HYDE PARK";
			noneMatchOrg.MainAddress.OA_City = "SYDNEY";
			noneMatchOrg.MainAddress.OA_PostCode = "2601";
			noneMatchOrg.MainAddress.OA_State = "ACT";
			var nonMatchContact = noneMatchOrg.Contacts.AddNew();
			nonMatchContact.OC_ContactName = "Alto Clef";
			nonMatchContact.OC_Phone = "+6165248524";
			CreatePatternMatchingName(noneMatchOrg, hashedMasterName);

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DeDuplicationMinimumConfidenceRating.Codes.Medium))
			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				((IDeduplicatable)masterOrg).ShouldRunDeduplication = true;

				var duplicationFinder = new OrgHeaderDuplicationFinderWithThresholdOverrideForTest(masterOrg, matchThreshold);
				var duplicates = duplicationFinder.FindPotentialDuplicates(false).ToList();

				AssertContainsExactElementsInAnyOrder("Should return results for medium and low matches, even though registry threshold is set to > medium",
					new[] { mediumMatchOrg.PK },
					duplicates.Select(match => match.TargetPK));

				AssertEquals(ConfidenceRating.Medium, duplicates.Single(match => match.TargetPK == mediumMatchOrg.PK).ConfidenceRating);
			}
		}

		public void TestGetScoreThresholdFromConfidenceRating()
		{
			AssertConfidenceScore(ConfidenceRating.None);
			AssertConfidenceScore(ConfidenceRating.Low);
			AssertConfidenceScore(ConfidenceRating.Medium);
			AssertConfidenceScore(ConfidenceRating.High);
		}

		void AssertConfidenceScore(ConfidenceRating confidenceThreshold)
		{
			var finder = new OrgHeaderDuplicationFinderWithThresholdOverrideForTest(Factory.New<OrgHeader>(), confidenceThreshold);
			var expectedThreshold = ScoringResult.ConfidenceRatingToScoreThreshold(confidenceThreshold);
			AssertEquals($"Finder's score threshold should match for confidence threshold: {confidenceThreshold}", expectedThreshold, finder.GetScoreThresholdFromConfidenceRatingForTest());
		}

		#region Implementation

		PatternMatchingName CreatePatternMatchingName(OrgHeader orgheader, int hashValue)
		{
			var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_OH = orgheader.PK;
			patternMatchingName.PMN_ParentId = orgheader.PK;
			patternMatchingName.PMN_HashedValue = hashValue;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";
			patternMatchingName.PMN_ParentTableCode = "OH";
			patternMatchingName.PMN_IsActive = true;
			return patternMatchingName;
		}

		class OrgHeaderDuplicationFinderWithThresholdOverrideForTest : OrgHeaderDuplicationFinderWithThresholdOverride
		{
			public OrgHeaderDuplicationFinderWithThresholdOverrideForTest(OrgHeader masterOrg)
				: base(masterOrg, true, true)
			{
			}

			public OrgHeaderDuplicationFinderWithThresholdOverrideForTest(OrgHeader masterOrg, ConfidenceRating thresholdOverride)
				: base(masterOrg, true, true, thresholdOverride)
			{
			}

			public double GetScoreThresholdFromConfidenceRatingForTest() => GetScoreThresholdFromConfidenceRating();
		}

		#endregion
	}
}
