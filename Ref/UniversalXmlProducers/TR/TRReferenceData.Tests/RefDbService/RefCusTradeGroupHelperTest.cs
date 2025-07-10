using System.IO;
using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.RefDbService
{
	class RefCusTradeGroupHelperTest
	{
		[SetUp]
		public void Setup()
		{
			var tradeGroupCountry = new RefCusTradeGroupCountry { ZZB_RN_NKTradeGroupCountryCode = "EG", ZZB_Description = "EG Description" };
			var tradeGroupCountry1 = new RefCusTradeGroupCountry { ZZB_RN_NKTradeGroupCountryCode = "BA", ZZB_Description = "BA Description" };

			tradeGroup = new RefCusTradeGroup
			{
				RefCusTradeGroupCountries = new[] { tradeGroupCountry, tradeGroupCountry1 },
				ZZA_TradeGroup = "ALL",
				ZZA_Description = "STA Bosna Hersek"
			};
		}
		RefCusTradeGroup tradeGroup;

		[Test]
		public void GetBestMatchRefCusTradeGroupFor()
		{
			var result = RefCusTradeGroupHelper.GetBestMatchRefCusTradeGroupFor("IR");
			string expectedConsoleOutput = string.Empty;

			using (var monitor = new ConsoleOutputMonitor())
			{
				Assert.IsNotNull(result);
				Assert.AreEqual("4FASIL2IN", result.ZZA_TradeGroup);
				Assert.AreEqual("Included trading groups for I sayìlì Liste 2023, section: 4.FASIL, footnote: 2", result.ZZA_Description);
				Assert.AreEqual(expectedConsoleOutput, monitor.ToString());
			}

			using (var monitor = new ConsoleOutputMonitor())
			{
				result = RefCusTradeGroupHelper.GetBestMatchRefCusTradeGroupFor("ZZ");
				expectedConsoleOutput = "Warning: Unable to find a RefCusTradeGroup for ZZ ().";

				Assert.IsNotNull(result);
				Assert.AreEqual("ZZ", result.ZZA_TradeGroup);
				Assert.That(monitor.ToString(), Does.Contain(expectedConsoleOutput));
			}

			var countryCode = "DE";
			var expectedTradeGroup = new RefCusTradeGroup { ZZA_TradeGroup = "TG2", ZZA_Description = "Test Trade Group 2" };
			decimal matchingDegree = 0;
			RefCusTradeGroupHelper.TradeGroupDictionaryByCountries.Value[countryCode] = (expectedTradeGroup, matchingDegree);

			using (var monitor = new ConsoleOutputMonitor())
			{
				result = RefCusTradeGroupHelper.GetBestMatchRefCusTradeGroupFor("DE");
				expectedConsoleOutput = "Warning: Fuzzy lookup trying to find a RefCusTradeGroup for: DE (Germany)";

				Assert.That(monitor.ToString(), Does.Contain(expectedConsoleOutput));
				Assert.IsNotNull(result);
				Assert.AreEqual("TG2", result.ZZA_TradeGroup);
				Assert.AreEqual("Test Trade Group 2", result.ZZA_Description);
			}
		}

		[Test]
		public void TestGetCountry()
		{
			var result = RefCusTradeGroupHelper.GetCountry("DE");
			Assert.AreEqual("Germany", result.ZZB_Description);

			result = RefCusTradeGroupHelper.GetCountry("");
			Assert.IsNull(result);
		}

		[Test]
		public void CountryNotInTradeGroup()
		{
			var tradeGroup = new RefCusTradeGroup { RefCusTradeGroupCountries = new RefCusTradeGroupCountry[0] };
			var tradeGroupCountry = new RefCusTradeGroupCountry { ZZB_RN_NKTradeGroupCountryCode = "EG" };
			var result = tradeGroup.GetMatchingDegreeWith(tradeGroupCountry);
			Assert.AreEqual(0, result);
		}

		[Test]
		public void IfSingleCountryInTradeGroup()
		{
			var tradeGroupCountry = new RefCusTradeGroupCountry { ZZB_RN_NKTradeGroupCountryCode = "EG" };
			var tradeGroup = new RefCusTradeGroup { RefCusTradeGroupCountries = new[] { tradeGroupCountry } };
			var result = tradeGroup.GetMatchingDegreeWith(tradeGroupCountry);
			Assert.AreEqual(1, result);
		}

		[Test]
		public void TradeGroupHasSameSearchKey()
		{
			var tradeGroupCountry = new RefCusTradeGroupCountry { ZZB_RN_NKTradeGroupCountryCode = "EG" };

			var tradeGroup = new RefCusTradeGroup
			{
				RefCusTradeGroupCountries = new[] { tradeGroupCountry },
				ZZA_TradeGroup = "EG"
			};

			var result = tradeGroup.GetMatchingDegreeWith(tradeGroupCountry);
			Assert.AreEqual(1, result);
		}

		[Test]
		public void TradeGroupHasAllCountriesKey()
		{
			var tradeGroupCountry = new RefCusTradeGroupCountry { ZZB_RN_NKTradeGroupCountryCode = "EG" };
			var tradeGroupCountry1 = new RefCusTradeGroupCountry { ZZB_RN_NKTradeGroupCountryCode = "TR" };

			var tradeGroup = new RefCusTradeGroup
			{
				RefCusTradeGroupCountries = new[] { tradeGroupCountry, tradeGroupCountry1 },
				ZZA_TradeGroup = Constants.AllCountriesTradeGroup
			};

			var result = tradeGroup.GetMatchingDegreeWith(tradeGroupCountry);
			Assert.AreEqual(0, result);
		}

		[Test]
		public void CountryNameNotInTranslator()
		{
			var tradeGroup = new RefCusTradeGroup
			{
				RefCusTradeGroupCountries = new[] { new RefCusTradeGroupCountry { ZZB_RN_NKTradeGroupCountryCode = "EG" } },
				ZZA_TradeGroup = "ABC",
				ZZA_Description = "Test STA"
			};

			var tradeGroupCountry = new RefCusTradeGroupCountry
			{
				ZZB_RN_NKTradeGroupCountryCode = "EG",
				ZZB_Description = "Some Description"
			};

			var result = tradeGroup.GetMatchingDegreeWith(tradeGroupCountry);
			Assert.That(result, Is.InRange(0, 1));
		}

		[Test]
		public void CalculatesSimilarityBasedOnCountryName()
		{
			var tradeGroup = new RefCusTradeGroup
			{
				RefCusTradeGroupCountries = new[] { new RefCusTradeGroupCountry { ZZB_RN_NKTradeGroupCountryCode = "EG" } },
				ZZA_Description = "Test Description STA"
			};

			var tradeGroupCountry = new RefCusTradeGroupCountry
			{
				ZZB_RN_NKTradeGroupCountryCode = "EG",
				ZZB_Description = "EG Description"
			};

			var result = tradeGroup.GetMatchingDegreeWith(tradeGroupCountry);

			var expectedSimilarity = "Test Description".ToSearchKey().GetSimilarity("Mısır".ToSearchKey());
			Assert.AreEqual(expectedSimilarity, result, "The similarity score does not match the expected value.");
		}

		[Test]
		public void WhenCountryNameMatchesDescription()
		{
			var tradeGroupCountry = new RefCusTradeGroupCountry
			{
				ZZB_RN_NKTradeGroupCountryCode = "BA",
				ZZB_Description = "BA Description"
			};

			var result = tradeGroup.GetMatchingDegreeWith(tradeGroupCountry);
			Assert.IsTrue(result > 0, "The similarity score should be greater than 0 when the country name matches the description.");

		}

		[Test]
		public void ReturnsBestMatchingCountry()
		{
			var expectedBestCountry = tradeGroup.RefCusTradeGroupCountries[1];

			var result = tradeGroup.TryGetBestMatchingCountry();


			Assert.IsNotNull(result, "The result should not be null.");
			Assert.AreEqual(expectedBestCountry, result.Country);
			Assert.AreEqual(tradeGroup.GetMatchingDegreeWith(expectedBestCountry), result.MatchingDegree);
		}

		[Test]
		public void TryGetBestMatchingCountry()
		{
			var result = tradeGroup.TryGetBestMatchingCountry();
			Assert.AreEqual("BA", result.Country.ZZB_RN_NKTradeGroupCountryCode);
			Assert.IsTrue(result.MatchingDegree > 0);
		}

		[Test]
		public void GetCountryName()
		{
			var result = RefCusTradeGroupHelper.GetCountryName("BA");
			Assert.AreEqual("Bosnia and Herzegovina", result);

			result = RefCusTradeGroupHelper.GetCountryName("KK");
			Assert.AreEqual(string.Empty, result);
		}

		[Test]
		[TestCase("TR", "Turkiye", "TR", "Turkiye", true)]
		[TestCase("TR", "Turkiye", "ES", "Spain", false)]
		[TestCase(null, null, null, null, true)]
		[TestCase("TR", "Turkiye", null, null, false)]
		public void RefCusTradeGroupCountryComparerShouldBeCorrect(string code1, string desc1, string code2, string desc2, bool expectedEqual)
		{
			var comparer = new RefCusTradeGroupCountryComparer();

			var obj1 = new RefCusTradeGroupCountry { ZZB_RN_NKTradeGroupCountryCode = code1, ZZB_Description = desc1 };
			var obj2 = new RefCusTradeGroupCountry { ZZB_RN_NKTradeGroupCountryCode = code2, ZZB_Description = desc2 };
			var obj3 = (RefCusTradeGroupCountry)null;

			Assert.AreEqual(expectedEqual, comparer.Equals(obj1, obj2));
			Assert.That(comparer.Equals(obj1, obj3), Is.False);
			Assert.AreEqual(expectedEqual, comparer.GetHashCode(obj1) == comparer.GetHashCode(obj2));
		}
	}

	class ConsoleOutputMonitor : StringWriter
	{
		public ConsoleOutputMonitor()
		{
			originalOutput = Console.Out;
			originalError = Console.Error;

			Console.SetOut(this);
			Console.SetError(this);
		}

		protected override void Dispose(bool disposing)
		{
			Console.SetOut(originalOutput);
			Console.SetError(originalError);
			base.Dispose(disposing);
		}

		readonly TextWriter originalOutput;
		readonly TextWriter originalError;
	}
}
