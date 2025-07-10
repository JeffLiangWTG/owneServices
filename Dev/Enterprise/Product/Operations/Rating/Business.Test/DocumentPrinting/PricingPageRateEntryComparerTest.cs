using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	class PricingPageRateEntryComparerTest : RatingTestCase
	{
		public void TestTwoItems_WhenEqualThenSameHash()
		{
			var entry1 = Factory.NewWithValidTestData<RateEntry>();
			var entry2 = Factory.NewWithValidTestData<RateEntry>();

			entry1.TI_TH = entry2.TI_TH;
			entry1.TI_OriginLRC = entry2.TI_OriginLRC;
			entry1.TI_DestinationLRC = entry2.TI_DestinationLRC;
			entry1.TI_RateStartDate = entry2.TI_RateStartDate;
			entry1.TI_RateEndDate = entry2.TI_RateEndDate;
			entry1.TI_Mode = entry2.TI_Mode;
			entry1.TI_RateCategory = entry2.TI_RateCategory;
			entry1.TI_MatchContainerRateClass = entry2.TI_MatchContainerRateClass;

			var comparer = new PricingPageRateEntryComparer();

			var equals = comparer.Equals(entry1, entry2);
			AssertEquals(true, equals);

			var hash1 = comparer.GetHashCode(entry1);
			var hash2 = comparer.GetHashCode(entry2);
			AssertEquals(hash1, hash2);
		}

		public void TestTwoItems_WhenDifferentHashThenNotEqual_DifferentOrigin()
			=> TestTwoItems_WhenDifferentHashThenNotEqual(
				RateEntrySchema.TI_OriginLRC, "AUSYD", "NOSHT");

		public void TestTwoItems_WhenDifferentHashThenNotEqual_DifferentDestination()
			=> TestTwoItems_WhenDifferentHashThenNotEqual(
				RateEntrySchema.TI_DestinationLRC, "AUSYD", "NOSHT");

		public void TestTwoItems_WhenDifferentHashThenNotEqual_DifferentStartDate()
			=> TestTwoItems_WhenDifferentHashThenNotEqual(
				RateEntrySchema.TI_RateStartDate, ZDate.BrettsBirthday, ZDate.Today);

		public void TestTwoItems_WhenDifferentHashThenNotEqual_DifferentEndDate()
			=> TestTwoItems_WhenDifferentHashThenNotEqual(
				RateEntrySchema.TI_RateEndDate, ZDate.BrettsBirthday, ZDate.Today);

		public void TestTwoItems_WhenDifferentHashThenNotEqual_DifferentMode()
			=> TestTwoItems_WhenDifferentHashThenNotEqual(
				RateEntrySchema.TI_Mode, Core.Constants.RateMode.ROA, Core.Constants.RateMode.FWL);

		public void TestTwoItems_WhenDifferentHashThenNotEqual_DifferentCategory()
			=> TestTwoItems_WhenDifferentHashThenNotEqual(
				RateEntrySchema.TI_RateCategory, RatingConstants.RateCategory.AIR, RatingConstants.RateCategory.FCL);

		public void TestTwoItems_WhenDifferentHashThenNotEqual_DifferentMatchContainerRateClass()
			=> TestTwoItems_WhenDifferentHashThenNotEqual(
				RateEntrySchema.TI_MatchContainerRateClass, false, true);

		void TestTwoItems_WhenDifferentHashThenNotEqual(SchemaColumn column, object value1, object value2)
		{
			var entry1 = Factory.NewWithValidTestData<RateEntry>();
			entry1[column.Name] = value1;
			var entry2 = Factory.NewWithValidTestData<RateEntry>();
			entry2[column.Name] = value2;

			var comparer = new PricingPageRateEntryComparer();

			var hash1 = comparer.GetHashCode(entry1);
			var hash2 = comparer.GetHashCode(entry2);
			AssertNotEquals(hash1, hash2);

			var equals = comparer.Equals(entry1, entry2);
			AssertEquals(false, equals);
		}
	}
}

