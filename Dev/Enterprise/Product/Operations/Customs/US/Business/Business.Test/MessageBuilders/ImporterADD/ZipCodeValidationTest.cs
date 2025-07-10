using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ZipCodeValidationTest : TestCaseWithFactory
	{
		public void TestValidateZipCodeAndGetMessage()
		{
			USCZipCode zipCodeForXX = Factory.New<USCZipCode>();
			zipCodeForXX.UZ_State = "XX";
			zipCodeForXX.UZ_BeginZipCodeRange = "00001";
			zipCodeForXX.UZ_EndZipCodeRange = "30000";

			AssertEquals(string.Format(ZipCodeValidation.InvalidUSZIPCodeEntered, "XX", "00001", "30000"), ZipCodeValidation.ValidateForZipCode(Factory, "99999", "XX", Core.Constants.CountryCodes.UnitedStates));
			AssertEquals(ZipCodeValidation.InvalidPostCodeDigitsForUS, ZipCodeValidation.ValidateForZipCode(Factory, "002", "XX", Core.Constants.CountryCodes.UnitedStates));
			AssertEquals(ZipCodeValidation.InvalidPostCodeDigitsForUS, ZipCodeValidation.ValidateForZipCode(Factory, "01234-", "XX", Core.Constants.CountryCodes.UnitedStates));
			AssertEquals(ZString.Empty, ZipCodeValidation.ValidateForZipCode(Factory, "012345678", "XX", Core.Constants.CountryCodes.UnitedStates));
			AssertEquals(ZString.Empty, ZipCodeValidation.ValidateForZipCode(Factory, "01234", "XX", Core.Constants.CountryCodes.UnitedStates));
			AssertEquals(ZString.Empty, ZipCodeValidation.ValidateForZipCode(Factory, "01234-5678", "XX", Core.Constants.CountryCodes.UnitedStates));
			AssertEquals(ZipCodeValidation.InvalidPostCodeDigitsForCA, ZipCodeValidation.ValidateForZipCode(Factory, "01234-5678", "NO", Core.Constants.CountryCodes.Canada));
			AssertEquals(ZString.Empty, ZipCodeValidation.ValidateForZipCode(Factory, "123 567", "NO", Core.Constants.CountryCodes.Canada));
			AssertEquals(ZString.Empty, ZipCodeValidation.ValidateForZipCode(Factory, "012356", "NO", Core.Constants.CountryCodes.Canada));
			AssertEquals(ZString.Empty, ZipCodeValidation.ValidateForZipCode(Factory, "0123 567", "NO", Core.Constants.CountryCodes.China));
			AssertEquals("The zip code entered for NH is not valid. The first five numbers of the zip code should fall between 03000 and 03899", ZipCodeValidation.ValidateForZipCode(Factory, "11111", "NH", Core.Constants.CountryCodes.UnitedStates));
			AssertEquals(ZString.Empty, ZipCodeValidation.ValidateForZipCode(Factory, "03001", "NH", Core.Constants.CountryCodes.UnitedStates));
			AssertEquals(ZString.Empty, ZipCodeValidation.ValidateForZipCode(Factory, "11", "NH", Core.Constants.CountryCodes.Netherlands));
		}

		public void TestValidateZipCodeLengthAndGetMessage()
		{
			AssertEquals(ZString.Empty, ZipCodeValidation.ValidateZipCodeLengthAndGetMessage(ZString.Empty, 3));
			AssertEquals("Postal code must have length 3", ZipCodeValidation.ValidateZipCodeLengthAndGetMessage("1234", 3));
			AssertEquals(ZString.Empty, ZipCodeValidation.ValidateZipCodeLengthAndGetMessage("1234", null));
		}

		public void TestValidateCAZipcode()
		{
			AssertEquals("", ZipCodeValidation.ValidateCAZipCodeAndGetMessage("A1A 1A1"));
			AssertEquals("", ZipCodeValidation.ValidateCAZipCodeAndGetMessage("A1A1A1"));
			AssertEquals(ZipCodeValidation.InvalidPostCodeForCA, ZipCodeValidation.ValidateCAZipCodeAndGetMessage("A1A  1A1"));
			AssertEquals(ZipCodeValidation.InvalidPostCodeForCA, ZipCodeValidation.ValidateCAZipCodeAndGetMessage("AAA 1A1"));
			AssertEquals(ZipCodeValidation.InvalidPostCodeForCA, ZipCodeValidation.ValidateCAZipCodeAndGetMessage("A3A 1AD"));
		}

		public void TestValidateMXZipcode()
		{
			AssertEquals("", ZipCodeValidation.ValidateMXZipCodeAndGetMessage("12345"));
			AssertEquals(ZipCodeValidation.InvalidPostCodeForMX, ZipCodeValidation.ValidateMXZipCodeAndGetMessage("A2345"));
			AssertEquals(ZipCodeValidation.InvalidPostCodeForMX, ZipCodeValidation.ValidateMXZipCodeAndGetMessage("7890-"));
			AssertEquals(ZipCodeValidation.InvalidPostCodeForMX, ZipCodeValidation.ValidateMXZipCodeAndGetMessage("12 45"));
		}

		public void TestValidateUSZipCodesWithMultipleRanges()
		{
			USCZipCode zipCodeForXXRange1 = Factory.New<USCZipCode>();
			zipCodeForXXRange1.UZ_State = "XX";
			zipCodeForXXRange1.UZ_BeginZipCodeRange = "00100";
			zipCodeForXXRange1.UZ_EndZipCodeRange = "00210";

			USCZipCode zipCodeForXXRange2 = Factory.New<USCZipCode>();
			zipCodeForXXRange2.UZ_State = "XX";
			zipCodeForXXRange2.UZ_BeginZipCodeRange = "10000";
			zipCodeForXXRange2.UZ_EndZipCodeRange = "12099";

			USCZipCode zipCodeForXXRange3 = Factory.New<USCZipCode>();
			zipCodeForXXRange3.UZ_State = "XX";
			zipCodeForXXRange3.UZ_BeginZipCodeRange = "96000";
			zipCodeForXXRange3.UZ_EndZipCodeRange = "96999";

			AssertEquals("The zip code entered for XX is not valid. The first five numbers of the zip code should fall between 00100 and 00210,\r\nor between 10000 and 12099,\r\nor between 96000 and 96999", ZipCodeValidation.ValidateForZipCode(Factory, "00610", "XX", Core.Constants.CountryCodes.UnitedStates));
			AssertEquals(ZString.Empty, ZipCodeValidation.ValidateForZipCode(Factory, "00210", "XX", Core.Constants.CountryCodes.UnitedStates));

			AssertEquals("The zip code entered for XX is not valid. The first five numbers of the zip code should fall between 00100 and 00210,\r\nor between 10000 and 12099,\r\nor between 96000 and 96999", ZipCodeValidation.ValidateForZipCode(Factory, "09500", "XX", Core.Constants.CountryCodes.UnitedStates));
			AssertEquals("The zip code entered for XX is not valid. The first five numbers of the zip code should fall between 00100 and 00210,\r\nor between 10000 and 12099,\r\nor between 96000 and 96999", ZipCodeValidation.ValidateForZipCode(Factory, "12100", "XX", Core.Constants.CountryCodes.UnitedStates));
			AssertEquals(ZString.Empty, ZipCodeValidation.ValidateForZipCode(Factory, "12099", "XX", Core.Constants.CountryCodes.UnitedStates));

			AssertEquals("The zip code entered for XX is not valid. The first five numbers of the zip code should fall between 00100 and 00210,\r\nor between 10000 and 12099,\r\nor between 96000 and 96999", ZipCodeValidation.ValidateForZipCode(Factory, "97000", "XX", Core.Constants.CountryCodes.UnitedStates));
			AssertEquals("The zip code entered for XX is not valid. The first five numbers of the zip code should fall between 00100 and 00210,\r\nor between 10000 and 12099,\r\nor between 96000 and 96999", ZipCodeValidation.ValidateForZipCode(Factory, "95910", "XX", Core.Constants.CountryCodes.UnitedStates));
			AssertEquals(ZString.Empty, ZipCodeValidation.ValidateForZipCode(Factory, "96510", "XX", Core.Constants.CountryCodes.UnitedStates));
		}

		public void TestValidateZIPForAddress()
		{
			var party = Factory.New<OrgHeader>();
			var address = party.Addresses.AddNew();
			address.OA_Address1 = "TEST ADDRESS";
			address.OA_RN_NKCountryCode = "US";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "invoice1";
			var line = invoice.InvoiceLines.AddNew();
			line.JI_LinePrice = 300m;
			var fda = line.ACE_FDALines.AddNew();

			fda.US_OA_ShipperAddress = address.PK;
			AssertHasMessageError(fda.US_OA_ShipperAddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			address.OA_PostCode = "2005";
			fda.AddInfoValidation.ValidateUS_OA_ShipperAddress();
			AssertNoMessageError(fda.US_OA_ShipperAddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
		}

		public void TestValidateForEmptyZIPForUSAddress()
		{
			var party = Factory.New<OrgHeader>();
			var address = party.Addresses.AddNew();
			address.OA_Address1 = "TEST ADDRESS";
			address.OA_RN_NKCountryCode = "US";

			var mock = Factory.NewMoq<DummyBusinessObjectJobDocAddress>();
			var dummyObj = mock.Object;
			var validation = new TestDummyBizoJobDocAddressValidation(dummyObj);
			mock.Protected().Setup<DummyBizoValidation>("GetNewValidation").Returns(validation);

			dummyObj.Z0_Guid = ZGuid.Empty;
			AssertNoMessageError(dummyObj.Z0_GuidInfo, ZipCodeValidation.ZIPCanNotBeEmpty);

			dummyObj.Z0_Guid = address.PK;
			AssertHasMessageError(dummyObj.Z0_GuidInfo, ZipCodeValidation.ZIPCanNotBeEmpty);

			address.Postcode = "10086";
			validation.ValidateZ0_Guid();
			AssertNoMessageError(dummyObj.Z0_GuidInfo, ZipCodeValidation.ZIPCanNotBeEmpty);

			address.OA_RN_NKCountryCode = "CN";
			validation.ValidateZ0_Guid();
			AssertNoMessageError(dummyObj.Z0_GuidInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
		}

		class TestDummyBizoJobDocAddressValidation : DummyBusinessObjectJobDocAddressValidation
		{
			public TestDummyBizoJobDocAddressValidation(DummyBusinessObjectJobDocAddress parent)
				: base(parent)
			{
			}

			protected override void CheckZ0_Guid()
			{
				base.CheckZ0_Guid();
				ZipCodeValidation.ValidateForEmptyZIPForUSAddress(ParentDocAddress.Z0_GuidInfo, ParentDocAddress);
			}
		}

		ZipCodeValidation zipCodeValidation;
		ZipCodeValidation ZipCodeValidation => zipCodeValidation ?? (zipCodeValidation = new ZipCodeValidation());
	}
}
