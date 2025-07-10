using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class StopPhraseMatcherTest : TestCaseWithFactory
	{
		public void TestFindMatchingStopPhrasesExact()
		{
			AssertMatchExact("Pillow");
			AssertMatchExact("Carpet");
			AssertMatchExact("Traditional medicine", "Traditional medicine");
			AssertMatchExact("Traditional blah medicine pillow traditional medicine", "Traditional medicine");
			AssertMatchExact("pillow \t-tRaditional mediCine+\t", "Traditional medicine");
			AssertMatchExact("Traditional,blah,medicine,pill,traditional medicine", "Traditional medicine", "Pill");
			AssertMatchExact("pillow traditional-medicine", "Traditional medicine");
			AssertMatchExact("pillow **traditional**medicine**", "Traditional medicine");

			void AssertMatchExact(string text, params string[] expectedPhrasesToMatch)
			{
				var matches = StopPhraseMatcher.FindMatchingStopPhrasesExact(text, TestStopPhrases);
				AssertEquals(matches.Count(), expectedPhrasesToMatch.Length);
				foreach (string expectedPhraseToMatch in expectedPhrasesToMatch)
				{
					Assert(matches.Contains(expectedPhraseToMatch));
				}
			}
		}

		public void TestFindMatchingStopPhrases_WithPlurals()
		{
			AssertMatchPluralized("Pillow");
			AssertMatchPluralized("Pillows");
			AssertMatchPluralized("Gun", "Gun");
			AssertMatchPluralized("Guns", "Gun");
			AssertMatchPluralized("Gas", "Gas");
			AssertMatchPluralized("Gases", "Gas");
			AssertMatchPluralized("Pyrotechnic device", "Pyrotechnic device");
			AssertMatchPluralized("Pyrotechnic devices", "Pyrotechnic device");
			AssertMatchPluralized("Brandy", "Brandy");
			AssertMatchPluralized("Brandies", "Brandy");
			AssertMatchPluralized("Strippey", "Strippey");
			AssertMatchPluralized("Strippeies");

			void AssertMatchPluralized(string text, params string[] expectedPhrasesToMatch)
			{
				var matches = StopPhraseMatcher.FindMatchingStopPhrasesPluralized(text, TestStopPhrases);
				AssertEquals(matches.Count(), expectedPhrasesToMatch.Length);
				foreach (string expectedPhraseToMatch in expectedPhrasesToMatch)
				{
					Assert(matches.Contains(expectedPhraseToMatch));
				}
			}
		}

		string[] TestStopPhrases
		{
			get
			{
				return new string[]
				{
					"Brandy",
					"Traditional medicine",
					"Prescription medicine",
					"Gun",
					"Gas",
					"Pyrotechnic device",
					"Truck",
					"Rifle",
					"Toxin",
					"Pill",
					"Strippey",
				};
			}
		}
	}
}
