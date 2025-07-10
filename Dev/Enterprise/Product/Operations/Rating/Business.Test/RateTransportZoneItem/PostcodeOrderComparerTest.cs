using CargoWise.EntityFramework.Testing;

namespace Enterprise.Rating.Business.Testing
{
	public class PostcodeOrderComparerTest : TestCaseWithFactory
	{
		public void TestComparerWithNumeric()
		{
			CombineAssertions("Numeric codes should be sorted as number", () =>
			{
				AssertPostcodeGreaterThan("1000", "200");
				AssertPostcodeLessThan("200", "1000");
				AssertPostcodeEquals("1234", "1234");
			});
		}

		public void TestComparerWithLetters()
		{
			CombineAssertions("Mixed Codes should compare numeric and non-numeric sections individually", () =>
			{
				AssertPostcodeLessThan("1A", "1B");
				AssertPostcodeGreaterThan("1B", "1A");
				AssertPostcodeLessThan("1", "1A");
				AssertPostcodeGreaterThan("1A", "1");
				AssertPostcodeEquals("1A", "1A");

				AssertPostcodeLessThan("BN7", "BN13");
				AssertPostcodeGreaterThan("PE19", "PE8");
				AssertPostcodeLessThan("A", "B");
				AssertPostcodeEquals("BN9", "BN9");
				AssertPostcodeLessThan("A3", "B13");
				AssertPostcodeLessThan("12AB1", "14AB1");
				AssertPostcodeLessThan("AB1CD", "AB2CD");
				AssertPostcodeLessThan("12AB1", "AB1CD");

				AssertPostcodeLessThan("111", "AAA");
				AssertPostcodeLessThan("A11AAA", "A111AA");
				AssertPostcodeLessThan("A11AA11", "A11AAA1");
			});
		}

		public void TestComparerWithMixedCase()
		{
			CombineAssertions("Comparison should ignore case", () =>
			{
				AssertPostcodeLessThan("aa", "BB");
				AssertPostcodeEquals("AaA", "aAa");
				AssertPostcodeLessThan("Aa", "AAB");
			});
		}

		public void TestComparerWithWhitespace()
		{
			CombineAssertions("Should compare UK postcodes correctly", () =>
			{
				AssertPostcodeLessThan("BL1 0AA", "BL11 0AA");
				AssertPostcodeLessThan("BL1 0AA", "BL5 9ZZ");
				AssertPostcodeLessThan("BL5 9ZZ", "BL11 0AA");
				AssertPostcodeLessThan("BL5 9ZZ", "BL11 9ZZ");
			});

			CombineAssertions("Should compare Canadian postcodes correctly", () =>
			{
				AssertPostcodeLessThan("B5C 3A4", "B8C 3A4");
				AssertPostcodeLessThan("B5C 6Z9", "B5C 7A1");
			});

			// ISO 3166-1 country code prefix
			CombineAssertions("Should compare postcodes with country code prefix correctly", () =>
			{
				AssertPostcodeLessThan("AB 2009", "AB 2010");
				AssertPostcodeLessThan("XX 9999", "XY 1111");
			});

			// Other Edge Cases
			CombineAssertions(() =>
			{
				AssertPostcodeLessThan("A11 AAA", "A111 AA");
				AssertPostcodeLessThan("A11 99A", "A22 22A");
			});
		}

		public void TestComparerWithNonAlphanumericCharacters()
		{
			CombineAssertions("All non-alphanumeric characters should be considered as whitespace", () =>
			{
				AssertPostcodeEquals("ABC-123", "ABC 123");
				AssertPostcodeLessThan("ABC-456", "ABC 789"); //Normally whitespace would be sorted before hyphen
				AssertPostcodeEquals("XY# &Z", "XY Z");
			});

			CombineAssertions("Whitespace or special characters should be trimmed from the start and end of postcodes", () =>
			{
				AssertPostcodeEquals(" ABC ", "ABC");
				AssertPostcodeEquals("ABC   ", "   ABC");
				AssertPostcodeEquals("--QWER TY1", "&QWER*TY1 ");
			});
		}

		readonly PostcodeOrderComparer comparer = new PostcodeOrderComparer();

		#region Implementation 

		void AssertPostcodeLessThan(string postCodeA, string postCodeB)
		{
			AssertLessThan($"{postCodeA} < {postCodeB}", comparer.Compare(postCodeA, postCodeB), 0);
		}

		void AssertPostcodeGreaterThan(string postCodeA, string postCodeB)
		{
			AssertGreaterThan($"{postCodeA} > {postCodeB}", comparer.Compare(postCodeA, postCodeB), 0);
		}

		void AssertPostcodeEquals(string postCodeA, string postCodeB)
		{
			AssertEquals($"{postCodeA} == {postCodeB}", comparer.Compare(postCodeA, postCodeB), 0);
		}

		#endregion
	}
}
