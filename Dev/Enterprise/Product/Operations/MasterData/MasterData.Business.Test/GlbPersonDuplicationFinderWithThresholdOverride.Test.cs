using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business.Tests
{
	using System;
	using System.Linq;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Registry.Business;

	public class GlbPersonDuplicationFinderWithThresholdOverrideTest : TestCaseWithFactory
	{
		public void TestDefaultThreshold()
		{
			var finder = new GlbPersonDuplicationFinderWithThresholdOverrideForTest(Factory.New<GlbPerson>());
			AssertEquals(ConfidenceRating.None, finder.ThresholdOverride);
		}

		public void TestFindPotentialDuplicates_WithThresholdEqualsNone_IncludesLowOrBetterResults()
		{
			const ConfidenceRating matchThreshold = ConfidenceRating.None;
			var masterPerson = Factory.NewWithValidTestData<GlbPerson>();
			masterPerson.PER_FullName = "Harry Potter";
			masterPerson.PER_HomeAddress1 = "4 Privet Drive, Little Whinging";
			masterPerson.PER_City = "SURREY";
			masterPerson.PER_EmailAddress = "Harry.Potter@Hogwarts.com";
			masterPerson.PER_HomePhone = "+61 444 753 159";

			var hashedMasterName = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizePersonName(masterPerson.PER_FullName));

			var mediumMatchPerson = Factory.NewWithValidTestData<GlbPerson>();
			mediumMatchPerson.PER_FullName = "Barry Trotter";
			mediumMatchPerson.PER_HomeAddress1 = "4 Privet Drive, Little Whinging";
			mediumMatchPerson.PER_City = "SURREY";
			mediumMatchPerson.PER_EmailAddress = "Barry.Trotter@Hogwarts.com";
			mediumMatchPerson.PER_HomePhone = "+61 444 753 159";
			CreatePatternMatchingName(mediumMatchPerson, hashedMasterName);

			var lowMatchPerson = Factory.NewWithValidTestData<GlbPerson>();
			lowMatchPerson.PER_FullName = "Larry Snotter";
			lowMatchPerson.PER_HomeAddress1 = "4 Privet Drive, Little Whinging";
			lowMatchPerson.PER_City = "SURREY";
			lowMatchPerson.PER_EmailAddress = "Larry.Snotter@Hogwarts.com";
			lowMatchPerson.PER_HomePhone = "+61 444 456 321";
			CreatePatternMatchingName(lowMatchPerson, hashedMasterName);

			var noneMatchPerson = Factory.NewWithValidTestData<GlbPerson>();
			noneMatchPerson.PER_FullName = "Ron Weasley";
			noneMatchPerson.PER_HomeAddress1 = "The Burrow - Ottery St Catchpole";
			noneMatchPerson.PER_City = "DEVON";
			noneMatchPerson.PER_EmailAddress = "Ron.Weasley@Hogwarts.com";
			noneMatchPerson.PER_HomePhone = "+61 444 789 654";
			CreatePatternMatchingName(noneMatchPerson, hashedMasterName);

			Factory.Save();

			using (SystemDataRegistry.Instance.PersonsDeduplicationMinimumConfidenceResult.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DeDuplicationMinimumConfidenceRating.Codes.Medium))
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				((IDeduplicatable)masterPerson).ShouldRunDeduplication = true;

				var duplicationFinder = new GlbPersonDuplicationFinderWithThresholdOverrideForTest(masterPerson, matchThreshold);
				var duplicates = duplicationFinder.FindPotentialDuplicates(false).ToList();

				AssertContainsExactElementsInAnyOrder("Should return results for medium and low matches, even though registry threshold is set to > medium",
					new[] { mediumMatchPerson.PK, lowMatchPerson.PK },
					duplicates.Select(match => match.TargetPK));

				AssertEquals(ConfidenceRating.Low, duplicates.Single(match => match.TargetPK == lowMatchPerson.PK).ConfidenceRating);
				AssertEquals(ConfidenceRating.Medium, duplicates.Single(match => match.TargetPK == mediumMatchPerson.PK).ConfidenceRating);
			}
		}

		public void TestFindPotentialDuplicates_WithThresholdEqualsLow_IncludesMediumOrBetterResults()
		{
			const ConfidenceRating matchThreshold = ConfidenceRating.Low;
			var masterPerson = Factory.NewWithValidTestData<GlbPerson>();
			masterPerson.PER_FullName = "Harry Potter";
			masterPerson.PER_HomeAddress1 = "4 Privet Drive, Little Whinging";
			masterPerson.PER_City = "SURREY";
			masterPerson.PER_EmailAddress = "Harry.Potter@Hogwarts.com";
			masterPerson.PER_HomePhone = "+61 444 753 159";

			var hashedMasterName = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizePersonName(masterPerson.PER_FullName));

			var mediumMatchPerson = Factory.NewWithValidTestData<GlbPerson>();
			mediumMatchPerson.PER_FullName = "Barry Trotter";
			mediumMatchPerson.PER_HomeAddress1 = "4 Privet Drive, Little Whinging";
			mediumMatchPerson.PER_City = "SURREY";
			mediumMatchPerson.PER_EmailAddress = "Barry.Trotter@Hogwarts.com";
			mediumMatchPerson.PER_HomePhone = "+61 444 753 159";
			CreatePatternMatchingName(mediumMatchPerson, hashedMasterName);

			var lowMatchPerson = Factory.NewWithValidTestData<GlbPerson>();
			lowMatchPerson.PER_FullName = "Larry Snotter";
			lowMatchPerson.PER_HomeAddress1 = "4 Privet Drive, Little Whinging";
			lowMatchPerson.PER_City = "SURREY";
			lowMatchPerson.PER_EmailAddress = "Larry.Snotter@Hogwarts.com";
			lowMatchPerson.PER_HomePhone = "+61 444 456 321";
			CreatePatternMatchingName(lowMatchPerson, hashedMasterName);

			var noneMatchPerson = Factory.NewWithValidTestData<GlbPerson>();
			noneMatchPerson.PER_FullName = "Ron Weasley";
			noneMatchPerson.PER_HomeAddress1 = "The Burrow - Ottery St Catchpole";
			noneMatchPerson.PER_City = "DEVON";
			noneMatchPerson.PER_EmailAddress = "Ron.Weasley@Hogwarts.com";
			noneMatchPerson.PER_HomePhone = "+61 444 789 654";
			CreatePatternMatchingName(noneMatchPerson, hashedMasterName);

			Factory.Save();

			using (SystemDataRegistry.Instance.PersonsDeduplicationMinimumConfidenceResult.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DeDuplicationMinimumConfidenceRating.Codes.Medium))
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				((IDeduplicatable)masterPerson).ShouldRunDeduplication = true;

				var duplicationFinder = new GlbPersonDuplicationFinderWithThresholdOverrideForTest(masterPerson, matchThreshold);
				var duplicates = duplicationFinder.FindPotentialDuplicates(false).ToList();

				AssertContainsExactElementsInAnyOrder("Should return results for medium matches, even though registry threshold is set to > Medium",
					new[] { mediumMatchPerson.PK },
					duplicates.Select(match => match.TargetPK));

				AssertEquals(ConfidenceRating.Medium, duplicates.Single(match => match.TargetPK == mediumMatchPerson.PK).ConfidenceRating);
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
			var finder = new GlbPersonDuplicationFinderWithThresholdOverrideForTest(Factory.New<GlbPerson>(), confidenceThreshold);
			var expectedThreshold = ScoringResult.ConfidenceRatingToScoreThreshold(confidenceThreshold);
			AssertEquals($"Finder's score threshold should match for confidence threshold: {confidenceThreshold}", expectedThreshold, finder.GetScoreThresholdFromConfidenceRatingForTest());
		}

		#region Implementation

		PatternMatchingName CreatePatternMatchingName(GlbPerson person, int hashValue)
		{
			var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_PER = person.PK;
			patternMatchingName.PMN_ParentId = person.PK;
			patternMatchingName.PMN_HashedValue = hashValue;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";
			patternMatchingName.PMN_ParentTableCode = person.TablePrefix;
			patternMatchingName.PMN_IsActive = true;
			return patternMatchingName;
		}

		class GlbPersonDuplicationFinderWithThresholdOverrideForTest : GlbPersonDuplicationFinderWithThresholdOverride
		{
			public GlbPersonDuplicationFinderWithThresholdOverrideForTest(GlbPerson masterPerson)
				: base(masterPerson, true, true)
			{
			}

			public GlbPersonDuplicationFinderWithThresholdOverrideForTest(GlbPerson masterPerson, ConfidenceRating thresholdOverride)
			: base(masterPerson, true, true, thresholdOverride)
			{
			}

			public double GetScoreThresholdFromConfidenceRatingForTest() => GetScoreThresholdFromConfidenceRating();
		}

		#endregion
	}
}
