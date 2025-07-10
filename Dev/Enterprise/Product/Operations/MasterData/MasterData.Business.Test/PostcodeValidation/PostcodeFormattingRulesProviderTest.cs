using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterData.Business.Tests
{
	class PostcodeFormattingRulesProviderTest : TestCaseWithFactory
	{
		public void TestValidDeserialisation()
		{
			var rules = (ObjectFactory.Get<IPostcodeFormattingRulesProvider>() as PostcodeFormattingRulesProvider).Rules;

			AssertEquals("Rules should have correct count", 171, rules.Count);
			AssertEquals("No duplicate rules", 171, rules.Select(u => u.Iso).Distinct().Count());

			foreach (var rule in rules)
			{
				AssertEquals("Format should not be empty", false, string.IsNullOrEmpty(rule.Format));
				AssertEquals("ISO should not be empty", false, string.IsNullOrEmpty(rule.Iso));
			}
		}

		public void TestValidIsoCodes()
		{
			var rules = (ObjectFactory.Get<IPostcodeFormattingRulesProvider>() as PostcodeFormattingRulesProvider).Rules;
			var allCountries = new RefCountryCollection(Factory).ToList();
			foreach (var rule in rules)
			{
				var countries = allCountries.FindAll(x => x.RN_Code == rule.Iso);
				AssertEquals("ISO code should match 1 country", 1, countries.Count);
			}
		}

		public void TestSingleInstance_FormattingRulesProvider()
		{
			var instances = new ConcurrentBag<IPostcodeFormattingRulesProvider>();
			instances.Add(ObjectFactory.Get<IPostcodeFormattingRulesProvider>());
			instances.Add(ObjectFactory.Get<IPostcodeFormattingRulesProvider>());

			var instancesArray = instances.ToArray();
			AssertEquals(instancesArray[0], instancesArray[1]);
		}

		public void TestSingleInstance_PostcodeFormattingRule()
		{
			var threads = new List<Thread>();
			var sem = new Semaphore(0, 2);
			var rules = new ConcurrentBag<PostcodeFormattingRule>();

			for (var i = 0; i < 2; i++)
			{
				var t = new Thread(() =>
				{
					sem.WaitOne();
					rules.Add(ObjectFactory.Get<IPostcodeFormattingRulesProvider>().GetRuleFromIso(CountryCodes.Slovakia));
				});
				t.Start();
				threads.Add(t);
			}

			sem.Release(2);

			foreach (var t in threads)
			{
				t.Join();
			}

			var ruleArray = rules.ToArray();
			AssertEquals("Single rules instance", ruleArray[0], ruleArray[1]);
		}

		public void TestNoPublicConstructor()
		{
			var constructors = typeof(PostcodeFormattingRulesProvider).GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			AssertEquals(1, constructors.Length);
			AssertEquals("No public constructor", false, constructors.Any(u => u.IsPublic));
		}

		public void TestPostcodeIsFormatted()
		{
			CombineAssertions(() =>
			{
				AssertIsFormatted(CountryCodes.AmericanSamoa, new[] { "12345", "12345-1234" }, new[] { "1234", "12345-12345", "12345-123456" });
				AssertIsFormatted(CountryCodes.Chile, new[] { "1234567", "123-4567" }, new[] { "12345678", "123 4567" });
				AssertIsFormatted(CountryCodes.CzechRepublic, new[] { "12345", "123 45" }, new[] { "123", "123 4", "1234 5" });
				AssertIsFormatted(CountryCodes.Micronesia, new[] { "12345", "12345-1234" }, new[] { "123456", "12345-123" });
				AssertIsFormatted(CountryCodes.Slovakia, new[] { "12345", "123 45" }, new[] { "123", "123 4", "1234 5" });
				AssertIsFormatted(CountryCodes.KoreaSouth, new[] { "12345" }, new[] { "123456", "123-456", "1234, 123-45" });
				AssertIsFormatted(CountryCodes.Taiwan, new[] { "123456" }, new[] { "12345", "1234567" });
				AssertIsFormatted(CountryCodes.Uzbekistan, new[] { "123456" }, new[] { "123 456" });
			});
		}

		void AssertIsFormatted(string countryCode, string[] formattedPostcodes, string[] unformattedPostcodes)
		{
			var rule = ObjectFactory.Get<IPostcodeFormattingRulesProvider>().GetRuleFromIso(countryCode);
			AssertNotNull($"Rule for {countryCode} should exist", rule);

			foreach (var formattedPostcode in formattedPostcodes)
			{
				Assert($"{formattedPostcode} is formatted for {countryCode}", rule.IsFormatted(formattedPostcode));
			}

			foreach (var unformattedPostcode in unformattedPostcodes)
			{
				Assert($"{unformattedPostcode} is not formatted for {countryCode}", !rule.IsFormatted(unformattedPostcode));
			}
		}
	}
}
