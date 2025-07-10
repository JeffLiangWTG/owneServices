using System.Collections.Generic;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Moq;
using NUnit.Framework;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AddressCleansingResultItemExtensionsTest : TestCase
	{
		#region Filter UnparsedInformation

		public void TestFilterUnparsedInformation_WhenContainingUsefulTerms_ShouldIncludeThem()
		{
			// Arrange.

			var resultItem = new AddressCleansingResultItem
			{
				ValidationResultItem = new ValidationResultItem
				{
					UnparsedAddressInformation = "1. This is extreme-ly useful information! Don't throw it away..."
				}
			};

			// Act.

			resultItem.FilterUnparsedInformation();

			// Assert.

			AssertEquals(
				"1. This is extreme-ly useful information! Don't throw it away...",
				resultItem.ValidationResultItem.UnparsedAddressInformation);
		}

		public void TestFilterUnparsedInformation_WhenContainingUselessTerms_ShouldExcludeThem()
		{
			// Arrange.

			var resultItem = new AddressCleansingResultItem
			{
				ValidationResultItem = new ValidationResultItem
				{
					UnparsedAddressInformation = @"!@#$%^&*() -_+= :;<>,. {}[]?/\|~`'"
				}
			};

			// Act.

			resultItem.FilterUnparsedInformation();

			// Assert.

			AssertEquals(
				string.Empty,
				resultItem.ValidationResultItem.UnparsedAddressInformation);
		}

		public void TestFilterUnparsedInformation_WhenContainingMixedTerms_ShouldIncludeOnlyUsefulOnes()
		{
			// Arrange.

			var resultItem = new AddressCleansingResultItem
			{
				ValidationResultItem = new ValidationResultItem
				{
					UnparsedAddressInformation = "1-2-3 Ready, Set, and   !#@$   Go, Mr. X! -- Quick..."
				}
			};

			// Act.

			resultItem.FilterUnparsedInformation();

			// Assert.

			AssertEquals(
				"1-2-3 Ready, Set, and Go, Mr. X! Quick...",
				resultItem.ValidationResultItem.UnparsedAddressInformation);
		}

		#endregion

		#region Transliteration

		public void TestTransliterateAddressToEnglish()
		{
			const string address1 = "Нахимовский пр";
			const string city = "Москва";
			const string postcode = "117335";
			const string country = "Russia";

			var resultItem = new AddressCleansingResultItem
			{
				ValidationResultItem = new ValidationResultItem
				{
					Address1 = address1,
					City = city,
					Postcode = postcode,
					Country = country
				}
			};

			resultItem.TransliterateAddressToEnglish();

			CombineAssertions(() =>
			{
				AssertEquals(WesternLanguageTransliterationHelper.TransliterateToEnglish(address1), resultItem.ValidationResultItem.Address1);
				AssertEquals(WesternLanguageTransliterationHelper.TransliterateToEnglish(city), resultItem.ValidationResultItem.City);
				AssertEquals(WesternLanguageTransliterationHelper.TransliterateToEnglish(postcode), resultItem.ValidationResultItem.Postcode);
				AssertEquals(WesternLanguageTransliterationHelper.TransliterateToEnglish(country), resultItem.ValidationResultItem.Country);

				AssertNull(resultItem.ValidationResultItem.Address2);
				AssertNull(resultItem.ValidationResultItem.UnmatchedApartmentPrefix);
				AssertNull(resultItem.ValidationResultItem.UnmatchedApartmentSuffix);
				AssertNull(resultItem.ValidationResultItem.Locality);
				AssertNull(resultItem.ValidationResultItem.State);
			});
		}

		public void TransliterateCityTownsToEnglish()
		{
			const string state1 = "Περιφέρεια Αττικής";
			const string city1 = "Glyfada";
			const string city2 = "Ηράκλειο";
			const string postcode1 = "166 74";
			const string postcode2 = "712 01";

			var cityTowns = new List<CandidateCityTown>
			{
				new CandidateCityTown()
				{
					State = state1,
					City = city1,
					Postcode = postcode1
				},
				new CandidateCityTown()
				{
					City = city2,
					Postcode = postcode2
				}
			};

			AddressCleansingResultItemExtensions.TransliterateCityTownsToEnglish(cityTowns);

			CombineAssertions(() =>
			{
				AssertEquals(WesternLanguageTransliterationHelper.TransliterateToEnglish(state1), cityTowns[0].State);
				AssertEquals(WesternLanguageTransliterationHelper.TransliterateToEnglish(city1), cityTowns[0].City);
				AssertEquals(WesternLanguageTransliterationHelper.TransliterateToEnglish(postcode1), cityTowns[0].Postcode);

				AssertNull(cityTowns[1].State);
				AssertEquals(WesternLanguageTransliterationHelper.TransliterateToEnglish(city2), cityTowns[1].City);
				AssertEquals(WesternLanguageTransliterationHelper.TransliterateToEnglish(postcode2), cityTowns[1].Postcode);
			});
		}

		#endregion

		#region Adjust Letter Case

		public void TestAdjustLetterCase_WhenMixedCaseNotAllowedAndAddressIsUpperCase_ShouldConvertFieldToUpperCase()
		{
			// Arrange.

			var stubAddress = new Mock<ISupportWebAddressValidation>();
			stubAddress.Setup(m => m.Address1).Returns("MOCK ADDRESS LINE 1");
			stubAddress.Setup(m => m.City).Returns("MOCK CITY");
			stubAddress.Setup(m => m.State).Returns("MOCK STATE");

			var resultItem = CreateValidResultItem();

			// Act.

			resultItem.AdjustAddressCase(stubAddress.Object, false);

			// Assert.

			AssertEquals("VERIFIED ADDRESS LINE 1", resultItem.ValidationResultItem.Address1);
			AssertEquals("VERIFIED ADDRESS LINE 2", resultItem.ValidationResultItem.Address2);
			AssertEquals("VERIFIED CITY", resultItem.ValidationResultItem.City);
			AssertEquals("VERIFIED POSTCODE", resultItem.ValidationResultItem.Postcode);
			AssertEquals("VERIFIED STATE", resultItem.ValidationResultItem.State);

			AssertEquals("SUGGESTION 1 ADDRESS LINE 1", resultItem.Suggestions[0].Address1);
			AssertEquals("SUGGESTION 1 ADDRESS LINE 2", resultItem.Suggestions[0].Address2);
			AssertEquals("SUGGESTION 1 CITY", resultItem.Suggestions[0].City);
			AssertEquals("SUGGESTION 1 POSTCODE", resultItem.Suggestions[0].Postcode);
			AssertEquals("SUGGESTION 1 STATE", resultItem.Suggestions[0].State);

			AssertEquals("SUGGESTION 2 ADDRESS LINE 1", resultItem.Suggestions[1].Address1);
			AssertEquals("SUGGESTION 2 ADDRESS LINE 2", resultItem.Suggestions[1].Address2);
			AssertEquals("SUGGESTION 2 CITY", resultItem.Suggestions[1].City);
			AssertEquals("SUGGESTION 2 POSTCODE", resultItem.Suggestions[1].Postcode);
			AssertEquals("SUGGESTION 2 STATE", resultItem.Suggestions[1].State);

			stubAddress.VerifyAll();
		}

		public void TestAdjustLetterCase_WhenMixedCaseNotAllowedAndAddressIsTitleCase_ShouldConvertFieldToUpperCase()
		{
			// Arrange.

			var stubAddress = new Mock<ISupportWebAddressValidation>();
			stubAddress.Setup(m => m.Address1).Returns("Mock Address Line 1");
			stubAddress.Setup(m => m.City).Returns("Mock City");
			stubAddress.Setup(m => m.State).Returns("Mock State");

			var resultItem = CreateValidResultItem();

			// Act.

			resultItem.AdjustAddressCase(stubAddress.Object, false);

			// Assert.

			AssertEquals("VERIFIED ADDRESS LINE 1", resultItem.ValidationResultItem.Address1);
			AssertEquals("VERIFIED ADDRESS LINE 2", resultItem.ValidationResultItem.Address2);
			AssertEquals("VERIFIED CITY", resultItem.ValidationResultItem.City);
			AssertEquals("VERIFIED POSTCODE", resultItem.ValidationResultItem.Postcode);
			AssertEquals("VERIFIED STATE", resultItem.ValidationResultItem.State);

			AssertEquals("SUGGESTION 1 ADDRESS LINE 1", resultItem.Suggestions[0].Address1);
			AssertEquals("SUGGESTION 1 ADDRESS LINE 2", resultItem.Suggestions[0].Address2);
			AssertEquals("SUGGESTION 1 CITY", resultItem.Suggestions[0].City);
			AssertEquals("SUGGESTION 1 POSTCODE", resultItem.Suggestions[0].Postcode);
			AssertEquals("SUGGESTION 1 STATE", resultItem.Suggestions[0].State);

			AssertEquals("SUGGESTION 2 ADDRESS LINE 1", resultItem.Suggestions[1].Address1);
			AssertEquals("SUGGESTION 2 ADDRESS LINE 2", resultItem.Suggestions[1].Address2);
			AssertEquals("SUGGESTION 2 CITY", resultItem.Suggestions[1].City);
			AssertEquals("SUGGESTION 2 POSTCODE", resultItem.Suggestions[1].Postcode);
			AssertEquals("SUGGESTION 2 STATE", resultItem.Suggestions[1].State);

			stubAddress.VerifyAll();
		}

		public void TestAdjustLetterCase_WhenMixedCaseNotAllowedAndAddressIsLowerCase_ShouldConvertFieldToUpperCase()
		{
			// Arrange.

			var stubAddress = new Mock<ISupportWebAddressValidation>();
			stubAddress.Setup(m => m.Address1).Returns("mock address line 1");
			stubAddress.Setup(m => m.City).Returns("mock city");
			stubAddress.Setup(m => m.State).Returns("mock state");

			var resultItem = CreateValidResultItem();

			// Act.

			resultItem.AdjustAddressCase(stubAddress.Object, false);

			// Assert.

			AssertEquals("VERIFIED ADDRESS LINE 1", resultItem.ValidationResultItem.Address1);
			AssertEquals("VERIFIED ADDRESS LINE 2", resultItem.ValidationResultItem.Address2);
			AssertEquals("VERIFIED CITY", resultItem.ValidationResultItem.City);
			AssertEquals("VERIFIED POSTCODE", resultItem.ValidationResultItem.Postcode);
			AssertEquals("VERIFIED STATE", resultItem.ValidationResultItem.State);

			AssertEquals("SUGGESTION 1 ADDRESS LINE 1", resultItem.Suggestions[0].Address1);
			AssertEquals("SUGGESTION 1 ADDRESS LINE 2", resultItem.Suggestions[0].Address2);
			AssertEquals("SUGGESTION 1 CITY", resultItem.Suggestions[0].City);
			AssertEquals("SUGGESTION 1 POSTCODE", resultItem.Suggestions[0].Postcode);
			AssertEquals("SUGGESTION 1 STATE", resultItem.Suggestions[0].State);

			AssertEquals("SUGGESTION 2 ADDRESS LINE 1", resultItem.Suggestions[1].Address1);
			AssertEquals("SUGGESTION 2 ADDRESS LINE 2", resultItem.Suggestions[1].Address2);
			AssertEquals("SUGGESTION 2 CITY", resultItem.Suggestions[1].City);
			AssertEquals("SUGGESTION 2 POSTCODE", resultItem.Suggestions[1].Postcode);
			AssertEquals("SUGGESTION 2 STATE", resultItem.Suggestions[1].State);

			stubAddress.VerifyAll();
		}

		public void TestAdjustLetterCase_WhenMixedCaseNotAllowed_ShouldSetAnyApartmentRelatedCaseToUpperCase()
		{
			// Arrange.

			var stubAddress = new Mock<ISupportWebAddressValidation>();
			stubAddress.Setup(m => m.Address1).Returns("Mock Address Line 1");
			stubAddress.Setup(m => m.City).Returns("Mock City");
			stubAddress.Setup(m => m.State).Returns("Mock State");

			var resultItem = CreateValidResultItem();

			// Act.

			resultItem.AdjustAddressCase(stubAddress.Object, false);

			// Assert.

			AssertEquals("VERIFIED APARTMENT PREFIX", resultItem.ValidationResultItem.UnmatchedApartmentPrefix);
			AssertEquals("VERIFIED APARTMENT SUFFIX", resultItem.ValidationResultItem.UnmatchedApartmentSuffix);

			AssertEquals("SUGGESTION 1 APARTMENT PREFIX", resultItem.Suggestions[0].UnmatchedApartmentPrefix);
			AssertEquals("SUGGESTION 1 APARTMENT SUFFIX", resultItem.Suggestions[0].UnmatchedApartmentSuffix);

			AssertEquals("SUGGESTION 2 APARTMENT PREFIX", resultItem.Suggestions[1].UnmatchedApartmentPrefix);
			AssertEquals("SUGGESTION 2 APARTMENT SUFFIX", resultItem.Suggestions[1].UnmatchedApartmentSuffix);

			stubAddress.VerifyAll();
		}

		public void TestAdjustLetterCase_WhenMixedCaseNotAllowed_ShouldSetLocalityCountyCaseToUpperCase()
		{
			// Arrange.

			var stubAddress = new Mock<ISupportWebAddressValidation>();
			stubAddress.Setup(m => m.Address1).Returns("Mock Address Line 1");
			stubAddress.Setup(m => m.City).Returns("Mock City");
			stubAddress.Setup(m => m.State).Returns("Mock State");

			var resultItem = CreateValidResultItem();

			// Act.

			resultItem.AdjustAddressCase(stubAddress.Object, false);

			// Assert.

			AssertEquals("VERIFIED COUNTY", resultItem.ValidationResultItem.County);
			AssertEquals("VERIFIED LOCALITY", resultItem.ValidationResultItem.Locality);

			AssertEquals("SUGGESTION 1 COUNTY", resultItem.Suggestions[0].County);
			AssertEquals("SUGGESTION 1 LOCALITY", resultItem.Suggestions[0].Locality);

			AssertEquals("SUGGESTION 2 COUNTY", resultItem.Suggestions[1].County);
			AssertEquals("SUGGESTION 2 LOCALITY", resultItem.Suggestions[1].Locality);

			stubAddress.VerifyAll();
		}

		public void TestAdjustLetterCase_WhenMixedCaseAllowedAndAddressIsUpperCase_ShouldConvertFieldToUpperCase()
		{
			// Arrange.

			var stubAddress = new Mock<ISupportWebAddressValidation>();
			stubAddress.Setup(m => m.Address1).Returns("MOCK ADDRESS LINE 1");
			stubAddress.Setup(m => m.City).Returns("MOCK CITY");
			stubAddress.Setup(m => m.State).Returns("MOCK STATE");

			var resultItem = CreateValidResultItem();

			// Act.

			resultItem.AdjustAddressCase(stubAddress.Object, true);

			// Assert.

			AssertEquals("VERIFIED ADDRESS LINE 1", resultItem.ValidationResultItem.Address1);
			AssertEquals("VERIFIED ADDRESS LINE 2", resultItem.ValidationResultItem.Address2);
			AssertEquals("VERIFIED CITY", resultItem.ValidationResultItem.City);
			AssertEquals("VERIFIED POSTCODE", resultItem.ValidationResultItem.Postcode);
			AssertEquals("VERIFIED STATE", resultItem.ValidationResultItem.State);

			AssertEquals("SUGGESTION 1 ADDRESS LINE 1", resultItem.Suggestions[0].Address1);
			AssertEquals("SUGGESTION 1 ADDRESS LINE 2", resultItem.Suggestions[0].Address2);
			AssertEquals("SUGGESTION 1 CITY", resultItem.Suggestions[0].City);
			AssertEquals("SUGGESTION 1 POSTCODE", resultItem.Suggestions[0].Postcode);
			AssertEquals("SUGGESTION 1 STATE", resultItem.Suggestions[0].State);

			AssertEquals("SUGGESTION 2 ADDRESS LINE 1", resultItem.Suggestions[1].Address1);
			AssertEquals("SUGGESTION 2 ADDRESS LINE 2", resultItem.Suggestions[1].Address2);
			AssertEquals("SUGGESTION 2 CITY", resultItem.Suggestions[1].City);
			AssertEquals("SUGGESTION 2 POSTCODE", resultItem.Suggestions[1].Postcode);
			AssertEquals("SUGGESTION 2 STATE", resultItem.Suggestions[1].State);

			stubAddress.VerifyAll();
		}

		public void TestAdjustLetterCase_WhenMixedCaseAllowedAndAddressIsTitleCase_ShouldConvertFieldToTitleCase()
		{
			// Arrange.

			var stubAddress = new Mock<ISupportWebAddressValidation>();
			stubAddress.Setup(m => m.Address1).Returns("Mock Address Line 1");
			stubAddress.Setup(m => m.City).Returns("Mock City");
			stubAddress.Setup(m => m.State).Returns("Mock State");

			var resultItem = CreateValidResultItem();

			// Act.

			resultItem.AdjustAddressCase(stubAddress.Object, true);

			// Assert.

			AssertEquals("Verified Address Line 1", resultItem.ValidationResultItem.Address1);
			AssertEquals("Verified Address Line 2", resultItem.ValidationResultItem.Address2);
			AssertEquals("Verified City", resultItem.ValidationResultItem.City);
			AssertEquals("VERIFIED POSTCODE", resultItem.ValidationResultItem.Postcode);
			AssertEquals("VERIFIED STATE", resultItem.ValidationResultItem.State);

			AssertEquals("Suggestion 1 Address Line 1", resultItem.Suggestions[0].Address1);
			AssertEquals("Suggestion 1 Address Line 2", resultItem.Suggestions[0].Address2);
			AssertEquals("Suggestion 1 City", resultItem.Suggestions[0].City);
			AssertEquals("SUGGESTION 1 POSTCODE", resultItem.Suggestions[0].Postcode);
			AssertEquals("SUGGESTION 1 STATE", resultItem.Suggestions[0].State);

			AssertEquals("Suggestion 2 Address Line 1", resultItem.Suggestions[1].Address1);
			AssertEquals("Suggestion 2 Address Line 2", resultItem.Suggestions[1].Address2);
			AssertEquals("Suggestion 2 City", resultItem.Suggestions[1].City);
			AssertEquals("SUGGESTION 2 POSTCODE", resultItem.Suggestions[1].Postcode);
			AssertEquals("SUGGESTION 2 STATE", resultItem.Suggestions[1].State);

			stubAddress.VerifyAll();
		}

		public void TestAdjustLetterCase_WhenMixedCaseAllowedAndAddressIsLowerCase_ShouldConvertFieldToTitleCase()
		{
			// Arrange.

			var stubAddress = new Mock<ISupportWebAddressValidation>();
			stubAddress.Setup(m => m.Address1).Returns("mock address line 1");
			stubAddress.Setup(m => m.City).Returns("mock city");
			stubAddress.Setup(m => m.State).Returns("mock state");

			var resultItem = CreateValidResultItem();

			// Act.

			resultItem.AdjustAddressCase(stubAddress.Object, true);

			// Assert.

			AssertEquals("Verified Address Line 1", resultItem.ValidationResultItem.Address1);
			AssertEquals("Verified Address Line 2", resultItem.ValidationResultItem.Address2);
			AssertEquals("Verified City", resultItem.ValidationResultItem.City);
			AssertEquals("VERIFIED POSTCODE", resultItem.ValidationResultItem.Postcode);
			AssertEquals("VERIFIED STATE", resultItem.ValidationResultItem.State);

			AssertEquals("Suggestion 1 Address Line 1", resultItem.Suggestions[0].Address1);
			AssertEquals("Suggestion 1 Address Line 2", resultItem.Suggestions[0].Address2);
			AssertEquals("Suggestion 1 City", resultItem.Suggestions[0].City);
			AssertEquals("SUGGESTION 1 POSTCODE", resultItem.Suggestions[0].Postcode);
			AssertEquals("SUGGESTION 1 STATE", resultItem.Suggestions[0].State);

			AssertEquals("Suggestion 2 Address Line 1", resultItem.Suggestions[1].Address1);
			AssertEquals("Suggestion 2 Address Line 2", resultItem.Suggestions[1].Address2);
			AssertEquals("Suggestion 2 City", resultItem.Suggestions[1].City);
			AssertEquals("SUGGESTION 2 POSTCODE", resultItem.Suggestions[1].Postcode);
			AssertEquals("SUGGESTION 2 STATE", resultItem.Suggestions[1].State);

			stubAddress.VerifyAll();
		}

		public void TestAdjustLetterCase_WhenMixedCaseAllowed_ShouldSyncAddress2CaseToAddress1Case()
		{
			// Arrange.

			var stubAddress = new Mock<ISupportWebAddressValidation>();
			stubAddress.Setup(m => m.Address1).Returns("Mock Address Line 1");
			stubAddress.Setup(m => m.City).Returns("MOCK CITY");
			stubAddress.Setup(m => m.State).Returns("MOCK STATE");

			var resultItem = CreateValidResultItem();

			// Act.

			resultItem.AdjustAddressCase(stubAddress.Object, true);

			// Assert.

			AssertEquals("Verified Address Line 1", resultItem.ValidationResultItem.Address1);
			AssertEquals("Verified Address Line 2", resultItem.ValidationResultItem.Address2);

			AssertEquals("Suggestion 1 Address Line 1", resultItem.Suggestions[0].Address1);
			AssertEquals("Suggestion 1 Address Line 2", resultItem.Suggestions[0].Address2);

			AssertEquals("Suggestion 2 Address Line 1", resultItem.Suggestions[1].Address1);
			AssertEquals("Suggestion 2 Address Line 2", resultItem.Suggestions[1].Address2);

			stubAddress.VerifyAll();
		}

		public void TestAdjustLetterCase_WhenMixedCaseAllowed_ShouldSyncAnyApartmentRelatedCaseToAddress1Case()
		{
			// Arrange.

			var stubAddress = new Mock<ISupportWebAddressValidation>();
			stubAddress.Setup(m => m.Address1).Returns("Mock Address Line 1");
			stubAddress.Setup(m => m.City).Returns("MOCK CITY");
			stubAddress.Setup(m => m.State).Returns("MOCK STATE");

			var resultItem = CreateValidResultItem();

			// Act.

			resultItem.AdjustAddressCase(stubAddress.Object, true);

			// Assert.

			AssertEquals("Verified Apartment Prefix", resultItem.ValidationResultItem.UnmatchedApartmentPrefix);
			AssertEquals("Verified Apartment Suffix", resultItem.ValidationResultItem.UnmatchedApartmentSuffix);

			AssertEquals("Suggestion 1 Apartment Prefix", resultItem.Suggestions[0].UnmatchedApartmentPrefix);
			AssertEquals("Suggestion 1 Apartment Suffix", resultItem.Suggestions[0].UnmatchedApartmentSuffix);

			AssertEquals("Suggestion 2 Apartment Prefix", resultItem.Suggestions[1].UnmatchedApartmentPrefix);
			AssertEquals("Suggestion 2 Apartment Suffix", resultItem.Suggestions[1].UnmatchedApartmentSuffix);

			stubAddress.VerifyAll();
		}

		public void TestAdjustLetterCase_WhenMixedCaseAllowed_ShouldSyncLocalityCountyCaseToStateCase()
		{
			// Arrange.

			var stubAddress = new Mock<ISupportWebAddressValidation>();
			stubAddress.Setup(m => m.Address1).Returns("MOCK ADDRESS LINE 1");
			stubAddress.Setup(m => m.City).Returns("MOCK CITY");
			stubAddress.Setup(m => m.State).Returns("Mock State");

			var resultItem = CreateValidResultItem();

			// Act.

			resultItem.AdjustAddressCase(stubAddress.Object, true);

			// Assert.

			AssertEquals("Verified County", resultItem.ValidationResultItem.County);
			AssertEquals("Verified Locality", resultItem.ValidationResultItem.Locality);

			AssertEquals("Suggestion 1 County", resultItem.Suggestions[0].County);
			AssertEquals("Suggestion 1 Locality", resultItem.Suggestions[0].Locality);

			AssertEquals("Suggestion 2 County", resultItem.Suggestions[1].County);
			AssertEquals("Suggestion 2 Locality", resultItem.Suggestions[1].Locality);

			stubAddress.VerifyAll();
		}

		public void TestAdjustLetterCase_WhenMixedCaseAllowedAndWordHasHyphen_ShouldAdjustCaseForIndividualSubword()
		{
			// Arrange.

			var stubAddress = new Mock<ISupportWebAddressValidation>();
			stubAddress.Setup(m => m.Address1).Returns("Mock Address Line 1");
			stubAddress.Setup(m => m.City).Returns("Mock City");
			stubAddress.Setup(m => m.State).Returns("Mock State");

			var resultItem = CreateValidHyphenatedResultItem();

			// Act.

			resultItem.AdjustAddressCase(stubAddress.Object, true);

			// Assert.

			AssertEquals("Verified-Address-Line-1", resultItem.ValidationResultItem.Address1);
			AssertEquals("Verified-Address-Line-2", resultItem.ValidationResultItem.Address2);
			AssertEquals("Verified-City", resultItem.ValidationResultItem.City);
			AssertEquals("VERIFIED-POSTCODE", resultItem.ValidationResultItem.Postcode);
			AssertEquals("VERIFIED-STATE", resultItem.ValidationResultItem.State);

			AssertEquals("Suggestion-1-Address-Line-1", resultItem.Suggestions[0].Address1);
			AssertEquals("Suggestion-1-Address-Line-2", resultItem.Suggestions[0].Address2);
			AssertEquals("Suggestion-1-City", resultItem.Suggestions[0].City);
			AssertEquals("SUGGESTION-1-POSTCODE", resultItem.Suggestions[0].Postcode);
			AssertEquals("SUGGESTION-1-STATE", resultItem.Suggestions[0].State);

			AssertEquals("Suggestion-2-Address-Line-1", resultItem.Suggestions[1].Address1);
			AssertEquals("Suggestion-2-Address-Line-2", resultItem.Suggestions[1].Address2);
			AssertEquals("Suggestion-2-City", resultItem.Suggestions[1].City);
			AssertEquals("SUGGESTION-2-POSTCODE", resultItem.Suggestions[1].Postcode);
			AssertEquals("SUGGESTION-2-STATE", resultItem.Suggestions[1].State);

			stubAddress.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestAdjustLetterCase_WhenValidationResultItemNull_ShouldNotThrowException()
		{
			// Arrange.

			var stubAddress = new Mock<ISupportWebAddressValidation>();

			var resultItem = new AddressCleansingResultItem
			{
				ValidationResultItem = null
			};

			resultItem.Suggestions.Add(null);

			// Act.

			resultItem.AdjustAddressCase(stubAddress.Object, false);
			resultItem.AdjustAddressCase(stubAddress.Object, true);

			stubAddress.VerifyAll();
		}

		public void TestAdjustLetterCase_WhenValidationResultItemFieldNull_ShouldLeaveItAsIs()
		{
			// Arrange.

			var stubAddress = new Mock<ISupportWebAddressValidation>();
			stubAddress.Setup(m => m.Address1).Returns("MOCK ADDRESS LINE 1");
			stubAddress.Setup(m => m.City).Returns("MOCK CITY");
			stubAddress.Setup(m => m.State).Returns("MOCK STATE");

			var resultItem = CreateEmptyResultItem();

			// Act.

			resultItem.AdjustAddressCase(stubAddress.Object, true);

			// Assert.

			AssertEquals(null, resultItem.ValidationResultItem.Address1);
			AssertEquals(null, resultItem.ValidationResultItem.Address2);
			AssertEquals(null, resultItem.ValidationResultItem.City);
			AssertEquals(null, resultItem.ValidationResultItem.Postcode);
			AssertEquals(null, resultItem.ValidationResultItem.State);
			AssertEquals(null, resultItem.ValidationResultItem.UnmatchedApartmentPrefix);
			AssertEquals(null, resultItem.ValidationResultItem.UnmatchedApartmentSuffix);
			AssertEquals(null, resultItem.ValidationResultItem.County);
			AssertEquals(null, resultItem.ValidationResultItem.Locality);

			AssertEquals(null, resultItem.Suggestions[0].Address1);
			AssertEquals(null, resultItem.Suggestions[0].Address2);
			AssertEquals(null, resultItem.Suggestions[0].City);
			AssertEquals(null, resultItem.Suggestions[0].Postcode);
			AssertEquals(null, resultItem.Suggestions[0].State);
			AssertEquals(null, resultItem.Suggestions[0].UnmatchedApartmentPrefix);
			AssertEquals(null, resultItem.Suggestions[0].UnmatchedApartmentSuffix);
			AssertEquals(null, resultItem.Suggestions[0].County);
			AssertEquals(null, resultItem.Suggestions[0].Locality);

			AssertEquals(null, resultItem.Suggestions[1].Address1);
			AssertEquals(null, resultItem.Suggestions[1].Address2);
			AssertEquals(null, resultItem.Suggestions[1].City);
			AssertEquals(null, resultItem.Suggestions[1].Postcode);
			AssertEquals(null, resultItem.Suggestions[1].State);
			AssertEquals(null, resultItem.Suggestions[1].UnmatchedApartmentPrefix);
			AssertEquals(null, resultItem.Suggestions[1].UnmatchedApartmentSuffix);
			AssertEquals(null, resultItem.Suggestions[1].County);
			AssertEquals(null, resultItem.Suggestions[1].Locality);

			stubAddress.VerifyAll();
		}

		public void TestAdjustLetterCase_WhenAddressFieldNull_ShouldLeaveValidationResultItemFieldAsIs()
		{
			// Arrange.

			var stubAddress = new Mock<ISupportWebAddressValidation>();
			stubAddress.Setup(m => m.Address1).Returns((ZString)null);
			stubAddress.Setup(m => m.City).Returns((ZString)null);
			stubAddress.Setup(m => m.State).Returns((ZString)null);

			var resultItem = CreateValidResultItem();

			// Act.

			resultItem.AdjustAddressCase(stubAddress.Object, true);

			// Assert.

			AssertEquals("VERIFIED ADDRESS LINE 1", resultItem.ValidationResultItem.Address1);
			AssertEquals("VERIFIED ADDRESS LINE 2", resultItem.ValidationResultItem.Address2);
			AssertEquals("VERIFIED CITY", resultItem.ValidationResultItem.City);
			AssertEquals("VERIFIED POSTCODE", resultItem.ValidationResultItem.Postcode);
			AssertEquals("VERIFIED STATE", resultItem.ValidationResultItem.State);
			AssertEquals("VERIFIED APARTMENT PREFIX", resultItem.ValidationResultItem.UnmatchedApartmentPrefix);
			AssertEquals("VERIFIED APARTMENT SUFFIX", resultItem.ValidationResultItem.UnmatchedApartmentSuffix);
			AssertEquals("VERIFIED COUNTY", resultItem.ValidationResultItem.County);
			AssertEquals("VERIFIED LOCALITY", resultItem.ValidationResultItem.Locality);

			AssertEquals("SUGGESTION 1 ADDRESS LINE 1", resultItem.Suggestions[0].Address1);
			AssertEquals("SUGGESTION 1 ADDRESS LINE 2", resultItem.Suggestions[0].Address2);
			AssertEquals("SUGGESTION 1 CITY", resultItem.Suggestions[0].City);
			AssertEquals("SUGGESTION 1 POSTCODE", resultItem.Suggestions[0].Postcode);
			AssertEquals("SUGGESTION 1 STATE", resultItem.Suggestions[0].State);
			AssertEquals("SUGGESTION 1 APARTMENT PREFIX", resultItem.Suggestions[0].UnmatchedApartmentPrefix);
			AssertEquals("SUGGESTION 1 APARTMENT SUFFIX", resultItem.Suggestions[0].UnmatchedApartmentSuffix);
			AssertEquals("SUGGESTION 1 COUNTY", resultItem.Suggestions[0].County);
			AssertEquals("SUGGESTION 1 LOCALITY", resultItem.Suggestions[0].Locality);

			AssertEquals("SUGGESTION 2 ADDRESS LINE 1", resultItem.Suggestions[1].Address1);
			AssertEquals("SUGGESTION 2 ADDRESS LINE 2", resultItem.Suggestions[1].Address2);
			AssertEquals("SUGGESTION 2 CITY", resultItem.Suggestions[1].City);
			AssertEquals("SUGGESTION 2 POSTCODE", resultItem.Suggestions[1].Postcode);
			AssertEquals("SUGGESTION 2 STATE", resultItem.Suggestions[1].State);
			AssertEquals("SUGGESTION 2 APARTMENT PREFIX", resultItem.Suggestions[1].UnmatchedApartmentPrefix);
			AssertEquals("SUGGESTION 2 APARTMENT SUFFIX", resultItem.Suggestions[1].UnmatchedApartmentSuffix);
			AssertEquals("SUGGESTION 2 COUNTY", resultItem.Suggestions[1].County);
			AssertEquals("SUGGESTION 2 LOCALITY", resultItem.Suggestions[1].Locality);

			stubAddress.VerifyAll();
		}

		public void TestGetApplicableCasing()
		{
			var address = new Mock<ISupportWebAddressValidation>();
			address.Setup(m => m.Address1).Returns("72 O'Riordan Street");

			var cityTowns = GetCityTowns();
			AddressCleansingResultItemExtensions.GetApplicableCityCasing(cityTowns, address.Object, true);
			AssertEquals(cityTowns[0].City, "Alexandria");
			AssertEquals(cityTowns[1].City, "New York");

			var cityTowns1 = GetCityTowns();
			AddressCleansingResultItemExtensions.GetApplicableCityCasing(cityTowns1, address.Object, false);
			AssertEquals(cityTowns1[0].City, "ALEXANDRIA");
			AssertEquals(cityTowns1[1].City, "NEW YORK");

			address.Setup(m => m.Address1).Returns("72 O'RIORDAN STREET");
			var cityTowns2 = GetCityTowns();
			AddressCleansingResultItemExtensions.GetApplicableCityCasing(cityTowns2, address.Object, true);
			AssertEquals(cityTowns2[0].City, "ALEXANDRIA");
			AssertEquals(cityTowns2[1].City, "NEW YORK");

			var cityTowns3 = GetCityTowns();
			AddressCleansingResultItemExtensions.GetApplicableCityCasing(cityTowns3, address.Object, false);
			AssertEquals(cityTowns3[0].City, "ALEXANDRIA");
			AssertEquals(cityTowns3[1].City, "NEW YORK");

			address.VerifyAll();
		}

		#endregion

		#region Helper

		static List<CandidateCityTown> GetCityTowns()
		{
			var cityTowns = new List<CandidateCityTown>
			{
				new CandidateCityTown
				{
					City = "ALEXANDRIA",
					Postcode = "2015",
					State = "NSW"
				},

				new CandidateCityTown
				{
					City = "NEW YORK",
					Postcode = "10022",
					State = "NY"
				}
			};

			return cityTowns;
		}

		static AddressCleansingResultItem CreateValidResultItem()
		{
			var resultItem = new AddressCleansingResultItem
			{
				ValidationResultItem = new ValidationResultItem
				{
					Address1 = "VERIFIED ADDRESS LINE 1",
					Address2 = "VERIFIED ADDRESS LINE 2",
					City = "VERIFIED CITY",
					Postcode = "VERIFIED POSTCODE",
					State = "VERIFIED STATE",
					UnmatchedApartmentPrefix = "VERIFIED APARTMENT PREFIX",
					UnmatchedApartmentSuffix = "VERIFIED APARTMENT SUFFIX",
					County = "VERIFIED COUNTY",
					Locality = "VERIFIED LOCALITY"
				}
			};

			resultItem.Suggestions.Add(new ValidationResultItem
			{
				Address1 = "SUGGESTION 1 ADDRESS LINE 1",
				Address2 = "SUGGESTION 1 ADDRESS LINE 2",
				City = "SUGGESTION 1 CITY",
				Postcode = "SUGGESTION 1 POSTCODE",
				State = "SUGGESTION 1 STATE",
				UnmatchedApartmentPrefix = "SUGGESTION 1 APARTMENT PREFIX",
				UnmatchedApartmentSuffix = "SUGGESTION 1 APARTMENT SUFFIX",
				County = "SUGGESTION 1 COUNTY",
				Locality = "SUGGESTION 1 LOCALITY"
			});

			resultItem.Suggestions.Add(new ValidationResultItem
			{
				Address1 = "SUGGESTION 2 ADDRESS LINE 1",
				Address2 = "SUGGESTION 2 ADDRESS LINE 2",
				City = "SUGGESTION 2 CITY",
				Postcode = "SUGGESTION 2 POSTCODE",
				State = "SUGGESTION 2 STATE",
				UnmatchedApartmentPrefix = "SUGGESTION 2 APARTMENT PREFIX",
				UnmatchedApartmentSuffix = "SUGGESTION 2 APARTMENT SUFFIX",
				County = "SUGGESTION 2 COUNTY",
				Locality = "SUGGESTION 2 LOCALITY"
			});

			return resultItem;
		}

		static AddressCleansingResultItem CreateValidHyphenatedResultItem()
		{
			var resultItem = new AddressCleansingResultItem
			{
				ValidationResultItem = new ValidationResultItem
				{
					Address1 = "VERIFIED-ADDRESS-LINE-1",
					Address2 = "VERIFIED-ADDRESS-LINE-2",
					City = "VERIFIED-CITY",
					Postcode = "VERIFIED-POSTCODE",
					State = "VERIFIED-STATE",
					UnmatchedApartmentPrefix = "VERIFIED-APARTMENT-PREFIX",
					UnmatchedApartmentSuffix = "VERIFIED-APARTMENT-SUFFIX",
					County = "VERIFIED-COUNTY",
					Locality = "VERIFIED-LOCALITY"
				}
			};

			resultItem.Suggestions.Add(new ValidationResultItem
			{
				Address1 = "SUGGESTION-1-ADDRESS-LINE-1",
				Address2 = "SUGGESTION-1-ADDRESS-LINE-2",
				City = "SUGGESTION-1-CITY",
				Postcode = "SUGGESTION-1-POSTCODE",
				State = "SUGGESTION-1-STATE",
				UnmatchedApartmentPrefix = "SUGGESTION-1-APARTMENT-PREFIX",
				UnmatchedApartmentSuffix = "SUGGESTION-1-APARTMENT-SUFFIX",
				County = "SUGGESTION-1-COUNTY",
				Locality = "SUGGESTION-1-LOCALITY"
			});

			resultItem.Suggestions.Add(new ValidationResultItem
			{
				Address1 = "SUGGESTION-2-ADDRESS-LINE-1",
				Address2 = "SUGGESTION-2-ADDRESS-LINE-2",
				City = "SUGGESTION-2-CITY",
				Postcode = "SUGGESTION-2-POSTCODE",
				State = "SUGGESTION-2-STATE",
				UnmatchedApartmentPrefix = "SUGGESTION-2-APARTMENT-PREFIX",
				UnmatchedApartmentSuffix = "SUGGESTION-2-APARTMENT-SUFFIX",
				County = "SUGGESTION-2-COUNTY",
				Locality = "SUGGESTION-2-LOCALITY"
			});

			return resultItem;
		}

		static AddressCleansingResultItem CreateEmptyResultItem()
		{
			var resultItem = new AddressCleansingResultItem
			{
				ValidationResultItem = new ValidationResultItem()
			};

			resultItem.Suggestions.Add(new ValidationResultItem());
			resultItem.Suggestions.Add(new ValidationResultItem());

			return resultItem;
		}

		#endregion
	}
}
