using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefAirlineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRM_AirlineName1()
		{
			RefAirline airline = Factory.New<RefAirline>();
			airline.RM_AirlineName1 = "";
			AssertHasErrors("Please enter an Airline Name 1.", airline.RM_AirlineName1Info);
			airline.RM_AirlineName1 = "valid";
			AssertNoErrors("RM_Airline name should not be empty", airline.RM_AirlineName1Info);
			airline.RM_IsActive = true;
			Factory.Save();

			RefAirline airline2 = Factory.New<RefAirline>();
			airline2.RM_IsActive = true;
			airline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "666";
			airline2.RM_ThreeLetterCode = "XXX";
			airline2.RM_AirlineName1 = "AirlineName1";
			Factory.Save();

			RefAirline airline3 = Factory.New<RefAirline>();
			airline3.RM_IsActive = false;
			airline3.RM_EagleAddedAirlinePrefixOrAccountingCode = "666";
			airline3.RM_ThreeLetterCode = "XXX";
			airline3.RM_AirlineName1 = "AirlineName1";
			airline3.Validation.ValidateRM_AirlineName1();
			AssertHasErrors("This Airline Name 1 already exists. Please enter another name.", airline3.RM_AirlineName1Info);
		}

		public void TestCheckRM_ThreeLetterCode()
		{
			RefAirline airline = Factory.New<RefAirline>();
			RefAirline airline2 = Factory.New<RefAirline>();

			airline.RM_ThreeLetterCode = "";
			AssertNoErrors(airline.RM_EagleAddedAirlinePrefixOrAccountingCodeInfo);
			airline.RM_ThreeLetterCode = "A1B";
			airline.RM_IsActive = true;
			airline2.RM_ThreeLetterCode = "A2B";
			airline2.RM_IsActive = false;
			Factory.Save();

			RefAirline airline3 = Factory.New<RefAirline>();
			airline3.RM_IsActive = true;
			airline3.RM_ThreeLetterCode = "A1B";
			AssertHasErrors("This Three Letter Code already exists. Please ensure you have entered the correct code.", airline3.RM_ThreeLetterCodeInfo);
			airline3.RM_IsActive = false;
			airline3.Validation.ValidateRM_ThreeLetterCode();
			AssertNoErrors("Deactive RefAirline can have duplicate Three Letter Code.", airline3.RM_ThreeLetterCodeInfo);
			airline3.RM_IsActive = true;
			airline3.RM_ThreeLetterCode = "A2B";
			airline3.Validation.ValidateRM_ThreeLetterCode();
			AssertNoErrors("Deactive RefAirline can have duplicate Three Letter Code.", airline3.RM_ThreeLetterCodeInfo);
		}

		public void TestCheckRM_EagleAddedAirlinePrefixOrAccountingCode()
		{
			RefAirline airline = Factory.New<RefAirline>();

			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "";
			AssertNoErrors(airline.RM_EagleAddedAirlinePrefixOrAccountingCodeInfo);
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "A1B";
			AssertHasError(airline.RM_EagleAddedAirlinePrefixOrAccountingCodeInfo, "Only numeric code is allowed.");
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "002";
			airline.RM_IsActive = true;
			AssertNoErrors("RM_EagleAddedAirlinePrefixOrAccountingCode should not be empty", airline.RM_EagleAddedAirlinePrefixOrAccountingCodeInfo);
			RefAirline airline_Deactive = Factory.New<RefAirline>();
			airline_Deactive.RM_EagleAddedAirlinePrefixOrAccountingCode = "003";
			airline_Deactive.RM_IsActive = false;

			Factory.Save();

			RefAirline airline2 = Factory.New<RefAirline>();
			airline2.RM_IsActive = true;
			airline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "002";
			AssertHasErrors("This Airline Numeric Code already exists. Please ensure you have entered the correct Airline Numeric Code.", airline2.RM_EagleAddedAirlinePrefixOrAccountingCodeInfo);
			airline2.RM_IsActive = false;
			airline2.Validation.ValidateRM_EagleAddedAirlinePrefixOrAccountingCode();
			AssertNoErrors("Deactive RefAirline can have duplicate Airline Numeric Code.", airline2.RM_EagleAddedAirlinePrefixOrAccountingCodeInfo);
			airline2.RM_IsActive = true;
			airline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "003";
			airline2.Validation.ValidateRM_EagleAddedAirlinePrefixOrAccountingCode();
			AssertNoErrors("Deactive RefAirline can have duplicate Airline Numeric Code.", airline2.RM_EagleAddedAirlinePrefixOrAccountingCodeInfo);

			RefAirline airline3 = Factory.Load<RefAirline>(airline.PK);
			AssertNoErrors("RM_EagleAddedAirlinePrefixOrAccountingCode should not be empty", airline3.RM_EagleAddedAirlinePrefixOrAccountingCodeInfo);
			AssertNoWarnings("RM_EagleAddedAirlinePrefixOrAccountingCode should not be empty", airline3.RM_EagleAddedAirlinePrefixOrAccountingCodeInfo);
		}

		public void TestCheckRM_EagleAddedAirlinePrefixOrAccountingCode_WhenAirLineIsIATAMember()
		{
			RefAirline airline = Factory.New<RefAirline>();
			airline.RM_MembershipFlagIATA = true;

			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "";
			AssertNoErrors(airline.RM_EagleAddedAirlinePrefixOrAccountingCodeInfo);
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "A1B";
			AssertNoErrors("When Airline is an IATA member, alpha-numeric characters are allowed in the airline prefix and there should be no errors", airline.RM_EagleAddedAirlinePrefixOrAccountingCodeInfo);
		}

		public void TestCheckRM_TwoCharacterCode()
		{
			RefAirline airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "~~";
			AssertNoErrors("RM_TwoCharacterCode should not be empty", airline.RM_TwoCharacterCodeInfo);
			airline.RM_TwoCharacterCode = "";
			AssertHasError(airline.RM_TwoCharacterCodeInfo, "Please enter a Two Character Code.");
		}

		public void TestCheckRM_AddressLine1()
		{
			RefAirline airline = Factory.New<RefAirline>();
			airline.RM_AddressLine1 = "valid";
			AssertNoErrors("RM_AddressLine1 should not be empty", airline.RM_AddressLine1Info);
			airline.RM_AddressLine1 = "";
			AssertHasError(airline.RM_AddressLine1Info, "Please enter an Address Line 1.");
		}

		public void TestCheckRM_AirlineCity()
		{
			RefAirline airline = Factory.New<RefAirline>();
			airline.RM_AirlineCity = "valid";
			AssertNoErrors("RM_AirlineCity should not be empty", airline.RM_AirlineCityInfo);
			airline.RM_AirlineCity = "";
			AssertHasError(airline.RM_AirlineCityInfo, "Please enter an Airline City.");
		}

		public void TestCheckRM_AirlineCountry()
		{
			RefAirline airline = Factory.New<RefAirline>();
			airline.RM_AirlineCountry = "valid";
			AssertNoErrors("RM_AirlineCountry should not be empty", airline.RM_AirlineCountryInfo);
			airline.RM_AirlineCountry = "";
			AssertHasError(airline.RM_AirlineCountryInfo, "Please enter an Airline Country/Region.");
		}

		public void TestCheckRM_ContactNameOCIIdentifier_Valid()
		{
			var airline = Factory.New<RefAirline>();

			airline.RM_ContactNameOCIIdentifier = ZString.Empty;
			AssertNoErrors("RM_ContactNameOCIIdentifier can be empty", airline.RM_ContactNameOCIIdentifierInfo);

			airline.RM_ContactNameOCIIdentifier = "A";
			AssertNoErrors("RM_ContactNameOCIIdentifier can have one alpha character", airline.RM_ContactNameOCIIdentifierInfo);

			airline.RM_ContactNameOCIIdentifier = "AB";
			AssertNoErrors("RM_ContactNameOCIIdentifier can have two alpha characters", airline.RM_ContactNameOCIIdentifierInfo);
		}

		public void TestCheckRM_ContactNameOCIIdentifier_Invalid()
		{
			var airline = Factory.New<RefAirline>();

			airline.RM_ContactNameOCIIdentifier = "1";
			AssertHasErrors("RM_ContactNameOCIIdentifier can be one or two alpha characters", airline.RM_ContactNameOCIIdentifierInfo);

			airline.RM_ContactNameOCIIdentifier = "7A";
			AssertHasErrors("RM_ContactNameOCIIdentifier can have one alpha character", airline.RM_ContactNameOCIIdentifierInfo);
		}

		public void TestRM_ContactPhoneOCIIdentifier_Valid()
		{
			var airline = Factory.New<RefAirline>();

			airline.RM_ContactPhoneOCIIdentifier = ZString.Empty;
			AssertNoErrors("RM_ContactPhoneOCIIdentifier can be empty", airline.RM_ContactPhoneOCIIdentifierInfo);

			airline.RM_ContactPhoneOCIIdentifier = "A";
			AssertNoErrors("RM_ContactPhoneOCIIdentifier can have one alpha character", airline.RM_ContactPhoneOCIIdentifierInfo);

			airline.RM_ContactPhoneOCIIdentifier = "AB";
			AssertNoErrors("RM_ContactPhoneOCIIdentifier can have two alpha characters", airline.RM_ContactPhoneOCIIdentifierInfo);
		}

		public void TestCheckRM_ContactPhoneOCIIdentifier_Invalid()
		{
			var airline = Factory.New<RefAirline>();

			airline.RM_ContactPhoneOCIIdentifier = "1";
			AssertHasErrors("RM_ContactPhoneOCIIdentifier can be one or two alpha characters", airline.RM_ContactPhoneOCIIdentifierInfo);

			airline.RM_ContactPhoneOCIIdentifier = "7A";
			AssertHasErrors("RM_ContactPhoneOCIIdentifier can have one alpha character", airline.RM_ContactPhoneOCIIdentifierInfo);
		}

		public void TestCheckRM_IsUpdatable()
		{
			var airline = Factory.New<RefAirline>();

			airline.RM_IsUpdatable = true;
			Assert(!airline.RM_IsUpdatableInfo.HasWarnings());

			airline.RM_IsUpdatable = false;
			airline.Validation.ValidateRM_IsUpdatable();
			Assert(airline.RM_IsUpdatableInfo.HasWarning("When unselected, no updates will be provided for this airline. User settings will be kept."));
		}
	}
}
