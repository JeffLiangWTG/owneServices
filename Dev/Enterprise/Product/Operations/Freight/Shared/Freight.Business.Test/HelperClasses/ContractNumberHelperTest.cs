using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using CustomsReferenceNumberCodes = Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ContractNumberHelperTest : TestCaseWithFactory
	{
		public void TestHasUniqueNonBlankCLC()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(dummy);

			AssertEquals(false, numbers.HasClientContractNumberRulesViolation());

			// AddOrSkipContractNumber has rules stopping us from adding numbers here. Let's go around it with lower level adding function.

			var newNumber = AddNewCLC(numbers, "", Constants.CountryCodes.Australia);
			AssertEquals(false, numbers.HasClientContractNumberRulesViolation());

			AddNewCLC(numbers, "AAA", Constants.CountryCodes.Australia);
			AssertEquals("bad: 2 numbers in a country", true, numbers.HasClientContractNumberRulesViolation());

			// remove blank number, AU-AAA left
			numbers.RemoveAndDelete(newNumber);

			AddNewCLC(numbers, "AAA", Constants.CountryCodes.China);
			AssertEquals("good: only 2 AAAs found in 2 countries", false, numbers.HasClientContractNumberRulesViolation());

			AddNewCLC(numbers, "", Constants.CountryCodes.UnitedStates);
			AssertEquals("good: only 2 AAAs and 1 blank found in 3 countries", false, numbers.HasClientContractNumberRulesViolation());

			AddNewCLC(numbers, "BBB", Constants.CountryCodes.Brazil);
			AssertEquals("bad: found both AAAs and BBB in different countries", true, numbers.HasClientContractNumberRulesViolation());
		}

		CusEntryNumber AddNewCLC(CusEntryNumAdditionalReferenceCollection numbers, string number, string countryCode)
		{
			var newNumber = numbers.AddNew();
			newNumber.CE_EntryType = CustomsReferenceNumberCodes.CLC;
			newNumber.CE_EntryNum = number;
			newNumber.CE_RN_NKCountryCode = countryCode;

			return newNumber;
		}

		public void TestAddContractNumber_NotCountrySpecific()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(dummy);

			AssertContainsExactElementsInAnyOrder("Nothing added yet.",
				Array.Empty<string>(),
				numbers.ListContractNumbers());

			AssertEquals(true, numbers.AddOrSkipContractNumber("Number1"));
			AssertContainsExactElementsInAnyOrder("Add non existing revenue contract numbers.",
				new[] { "Number1" },
				numbers.ListContractNumbers().Select(x => x.CE_EntryNum));

			AssertEquals(true, numbers.AddOrSkipContractNumber("Number2"));
			AssertContainsExactElementsInAnyOrder("Add non existing revenue contract numbers.",
				new[] { "Number1", "Number2" },
				numbers.ListContractNumbers().Select(x => x.CE_EntryNum));

			AssertEquals(true, numbers.AddOrSkipContractNumber("Number2"));
			AssertContainsExactElementsInAnyOrder("Don't add duplicates",
				new[] { "Number1", "Number2" },
				numbers.ListContractNumbers().Select(x => x.CE_EntryNum));

			AssertEquals(true, numbers.AddOrSkipContractNumber(""));
			AssertContainsExactElementsInAnyOrder("Don't add empty",
				new[] { "Number1", "Number2" },
				numbers.ListContractNumbers().Select(x => x.CE_EntryNum));

			AssertEquals(true, numbers.AddOrSkipContractNumber("", CustomsReferenceNumberCodes.CON, false));
			AssertContainsExactElementsInAnyOrder("Don't add empty",
				new[] { "Number1", "Number2" },
				numbers.ListContractNumbers().Select(x => x.CE_EntryNum));

			AssertEquals(true, numbers.AddOrSkipContractNumber("", CustomsReferenceNumberCodes.CON, false));
			AssertContainsExactElementsInAnyOrder("Don't add empty",
				new[] { "Number1", "Number2" },
				numbers.ListContractNumbers().Select(x => x.CE_EntryNum));

			AssertEquals(true, numbers.AddOrSkipContractNumber("", CustomsReferenceNumberCodes.CON, true));
			AssertContainsExactElementsInAnyOrder("Force to add empty",
				new[] { "Number1", "Number2", "" },
				numbers.ListContractNumbers().Select(x => x.CE_EntryNum));

			AssertEquals(true, numbers.AddOrSkipContractNumber("Number3", CustomsReferenceNumberCodes.CLC));
			AssertContainsExactElementsInAnyOrder("Add a new number for a new type",
				new[] { "Number3" },
				numbers.ListContractNumbers(CustomsReferenceNumberCodes.CLC).Select(x => x.CE_EntryNum));

			AssertEquals(true, numbers.AddOrSkipContractNumber("Number3", CustomsReferenceNumberCodes.CLC));
			AssertContainsExactElementsInAnyOrder("Don't add duplicates",
				new[] { "Number3" },
				numbers.ListContractNumbers(CustomsReferenceNumberCodes.CLC).Select(x => x.CE_EntryNum));

			AssertEquals(true, numbers.AddOrSkipContractNumber("", CustomsReferenceNumberCodes.CLC));
			AssertContainsExactElementsInAnyOrder("CLC accepts empty by default",
				new[] { "Number3", "" },
				numbers.ListContractNumbers(CustomsReferenceNumberCodes.CLC).Select(x => x.CE_EntryNum));
		}

		public void TestAddClientContractNumberByCountry_ShouldAlwaysHaveSingleValuePerCountry()
		{
			var newBranch1 = CreateBranchInANewCountryAndNewCountry(Constants.CountryCodes.China, "CNSHG");
			var newBranch2 = CreateBranchInANewCountryAndNewCountry(Constants.CountryCodes.Brazil, "BRSPR");
			Factory.Save();

			var originalLoginCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString();
			var dummy = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(dummy);

			AssertEquals(true, numbers.AddOrSkipContractNumber("", CustomsReferenceNumberCodes.CLC, countryCode: originalLoginCountry));
			var expectedNumbers = new[] { (originalLoginCountry, "CLC", "") };
			AssertContractNumbers(expectedNumbers, numbers, "first CLC should be added successfully");

			AssertEquals(true, numbers.AddOrSkipContractNumber("abc", CustomsReferenceNumberCodes.CLC, countryCode: originalLoginCountry));
			AssertContractNumbers(expectedNumbers, numbers, "no new CLC should be added for this country");

			AssertEquals(true, numbers.AddOrSkipContractNumber("", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.Brazil));
			expectedNumbers = new[]
			{
				(originalLoginCountry, "CLC", ""),
				("BR", "CLC", "")
			};
			var reason = "new CLC should be added for one country from another country even when the numbers are the same";
			AssertContractNumbers(expectedNumbers, numbers, reason);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, newBranch1.PK.ToGuid(), Guid.NewGuid()))
			{
				AssertEquals("CN", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals(true, numbers.AddOrSkipContractNumber("abc", CustomsReferenceNumberCodes.CLC, countryCode: "CN"));

				expectedNumbers = new[]
				{
					(originalLoginCountry, "CLC", ""),
					("BR", "CLC", ""),
					("CN", "CLC", "abc")
				};
				reason = "in a new country, a new CLC should be added successfully";
				AssertContractNumbers(expectedNumbers, numbers, reason);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, newBranch2.PK.ToGuid(), Guid.NewGuid()))
			{
				AssertEquals("BR", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals(true, numbers.AddOrSkipContractNumber("xyz", CustomsReferenceNumberCodes.CLC, countryCode: "BR"));

				expectedNumbers = new[]
				{
					(originalLoginCountry, "CLC", ""),
					("BR", "CLC", ""),
					("CN", "CLC", "abc")
				};
				reason = "in this country, there is an existing CLC, should NOT be able to add a new one";
				AssertContractNumbers(expectedNumbers, numbers, reason);
			}
		}

		public void TestAddClientContractNumber_BlankCLCCannotBeAddedForCountriesHavingNumber()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(dummy);
			AssertEquals(true, numbers.AddOrSkipContractNumber("", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.Australia));
			AssertEquals(1, numbers.Count);

			AssertEquals(true, numbers.AddOrSkipContractNumber("", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.Australia));
			var expectedNumbers = new[]
			{
				("AU", "CLC", "")
			};
			AssertContractNumbers(expectedNumbers, numbers, "There should be only 1 blank number in 1 country");

			numbers.RemoveAndDeleteAll();
			AssertEquals(true, numbers.AddOrSkipContractNumber("AAA", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.Australia));
			AssertEquals(1, numbers.Count);

			AssertEquals(true, numbers.AddOrSkipContractNumber("", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.Australia));
			expectedNumbers = new[]
			{
				("AU", "CLC", "AAA")
			};
			AssertContractNumbers(expectedNumbers, numbers, "Country has a non blank number, should not be able to add another one");
		}

		public void TestAddClientContractNumber_BlankCLCCanBeAddedToDifferentCountries()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(dummy);
			AssertEquals(true, numbers.AddOrSkipContractNumber("", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.Australia));
			AssertEquals(1, numbers.Count);

			AssertEquals(true, numbers.AddOrSkipContractNumber("", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.China));
			var expectedNumbers = new[]
			{
				("AU", "CLC", ""),
				("CN", "CLC", "")
			};
			AssertContractNumbers(expectedNumbers, numbers, "Different countries can have same blank numbers");
		}

		public void TestAddClientContractNumber_NonBlankCLCCannotBeAddedToCountriesHavingNumber()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(dummy);
			AssertEquals(true, numbers.AddOrSkipContractNumber("AAA", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.Australia));
			AssertEquals(1, numbers.Count);

			AssertEquals(true, numbers.AddOrSkipContractNumber("AAA", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.Australia));
			var expectedNumbers = new[]
			{
				("AU", "CLC", "AAA"),
			};
			AssertContractNumbers(expectedNumbers, numbers, "Country has a non blank number, should not be able to add the same one");

			AssertEquals(true, numbers.AddOrSkipContractNumber("BBB", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.Australia));
			expectedNumbers = new[]
			{
				("AU", "CLC", "AAA"),
			};
			AssertContractNumbers(expectedNumbers, numbers, "Country has a non blank number, should not be able to add another one");
		}

		public void TestAddClientContractNumber_NoNonBlankCLCInAnyCountry_NonBlankCLCCanBeAdded()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(dummy);
			AssertEquals(true, numbers.AddOrSkipContractNumber("", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.Australia));
			AssertEquals(1, numbers.Count);

			AssertEquals(true, numbers.AddOrSkipContractNumber("AAA", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.China));
			var expectedNumbers = new[]
			{
				("AU", "CLC", ""),
				("CN", "CLC", "AAA"),
			};
			AssertContractNumbers(expectedNumbers, numbers, "should be able to add a non blank number in other country");
		}

		public void TestAddClientContractNumber_NonBlankCLCShouldBeTheSameForDifferentCountries()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(dummy);
			AssertEquals(true, numbers.AddOrSkipContractNumber("AAA", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.Australia));
			AssertEquals(1, numbers.Count);

			AssertEquals("non-blank Client Contract Number should be the same for all countries",
				false,
				numbers.AddOrSkipContractNumber("BBB", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.China));
			var expectedNumbers = new[]
			{
				("AU", "CLC", "AAA"),
			};
			var reason = "non-blank number presents in a country, should NOT be able to add another non blank number in another country";
			AssertContractNumbers(expectedNumbers, numbers, reason);

			AssertEquals(true, numbers.AddOrSkipContractNumber("AAA", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.China));
			expectedNumbers = new[]
			{
				("AU", "CLC", "AAA"),
				("CN", "CLC", "AAA"),
			};
			reason = "non-blank number presents in a country, should be able to add the same non blank number in another country";
			AssertContractNumbers(expectedNumbers, numbers, reason);
		}

		public void TestAddClientContractNumber_HavingDifferentNonBlankCLCs_ShouldFail()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(dummy);
			AssertEquals(true, numbers.AddOrSkipContractNumber("AAA", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.Australia));

			// It is impossible to add another CLC BBB in another country through AddOrSkipContractNumber because of the rules in it.
			// Go around them by accessing the number collection directly
			var newNumber = numbers.AddNew();
			newNumber.CE_RN_NKCountryCode = Constants.CountryCodes.NewZealand;
			newNumber.CE_EntryType = CustomsReferenceNumberCodes.CLC;
			newNumber.CE_EntryNum = "BBB";

			AssertEquals("Bad data should be detected while trying to add one of the existing numbers.",
				false,
				numbers.AddOrSkipContractNumber("AAA", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.UnitedStates));

			AssertEquals("Bad data should be detected while trying to add one of the existing numbers.",
				false,
				numbers.AddOrSkipContractNumber("BBB", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.UnitedStates));

			AssertEquals("Bad data should be detected while trying to add a new number.",
				false,
				numbers.AddOrSkipContractNumber("CCC", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.UnitedStates));

			AssertEquals("Bad data should be detected while trying to add a blank number.",
				false,
				numbers.AddOrSkipContractNumber("", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.UnitedStates));
		}

		public void TestListClientContractNumbers()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(dummy);

			var currentLoginCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			numbers.AddOrSkipContractNumber("", CustomsReferenceNumberCodes.CLC, countryCode: currentLoginCountry);
			numbers.AddOrSkipContractNumber("", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.China);
			numbers.AddOrSkipContractNumber("", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.NewZealand);
			numbers.AddOrSkipContractNumber("", CustomsReferenceNumberCodes.CLC, countryCode: Constants.CountryCodes.UnitedStates);

			var result = numbers.ListContractNumbers(CustomsReferenceNumberCodes.CLC);
			AssertEquals("all numbers are returned", 4, result.Count());

			result = numbers.ListContractNumbers(CustomsReferenceNumberCodes.CLC, currentLoginCountry);
			AssertEquals("only number from a country selected", 1, result.Count());

			// Add a generic CLC without country code.
			// The number should be added and its country code is automatically set to current login country.
			numbers.AddOrSkipContractNumber("abc", CustomsReferenceNumberCodes.CLC);
			AssertEquals("a generic CLC number can be added", 5, numbers.Count);

			// because of above, now we have more than 1 numbers from current country.
			result = numbers.ListContractNumbers(CustomsReferenceNumberCodes.CLC, currentLoginCountry);
			AssertEquals("only numbers from a country selected", 2, result.Count());
		}

		static void AssertContractNumbers(IEnumerable<(string countryCode, string entryType, string entryNumber)> expectedNumbers, IEnumerable actualNumbers, string reason = "")
		{
			var numbers = actualNumbers
				.OfType<CusEntryNumber>()
				.Select(x => (x.CE_RN_NKCountryCode.ToString(), x.CE_EntryType.ToString(), x.CE_EntryNum.ToString()));

			AssertEquals(expectedNumbers.Count(), numbers.Count());
			foreach (var (actualNumber, expectedNumber) in numbers.Zip(expectedNumbers, (a, e) => (a, e)))
			{
				AssertEquals(reason, expectedNumber, actualNumber);
			}
		}

		GlbBranch CreateBranchInANewCountryAndNewCountry(string countryCode, string unloco)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = countryCode + " Company";
			company.GC_RN_NKCountryCode = countryCode;

			var branch = company.Branches.AddNew();
			branch.GB_Code = company.GC_Code;
			branch.GB_BranchName = unloco + " Branch";
			branch.GB_RL_NKHomePort = unloco;

			return branch;
		}
	}
}
