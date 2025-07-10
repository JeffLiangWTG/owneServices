using System;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.eManifest.Business.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Registry.Business;

	internal class SupplierBookingLineValidationTest : BusinessObjectValidationTestCase
	{
		#region TestCheckDL_ConsigneeReference

		#region Unique per Booking Header

		public void TestCheckDL_ConsigneeReference()
		{
			const string reference1 = "SH4633637134723412";
			const string reference2 = "SH4725437134410109";

			var bookingHeader = Factory.NewWithValidTestData<SupplierBookingHeader>();
			var bookingLine1 = bookingHeader.BookingLines.AddNew();
			var bookingLine2 = bookingHeader.BookingLines.AddNew();
			var bookingLine3 = bookingHeader.BookingLines.AddNew();

			bookingLine1.DL_ConsigneeReference = reference1;
			bookingLine2.DL_ConsigneeReference = "";

			AssertNoError("Expected no errors as the reference is unique for this manifest", bookingLine1.DL_ConsigneeReferenceInfo, uniquePerManifestMessage);
			AssertNoError("Expected no errors on a blank reference as it should be populated after saving", bookingLine2.DL_ConsigneeReferenceInfo, uniquePerManifestMessage);

			bookingLine3.DL_ConsigneeReference = reference1;

			AssertHasError(bookingLine3.DL_ConsigneeReferenceInfo, uniquePerManifestMessage);

			bookingLine3.DL_ConsigneeReference = reference2;

			AssertNoError(bookingLine3.DL_ConsigneeReferenceInfo, uniquePerManifestMessage);

			var newBookingHeader = Factory.NewWithValidTestData<SupplierBookingHeader>();
			var bookingLine4 = newBookingHeader.BookingLines.AddNew();
			bookingLine4.DL_ConsigneeReference = reference1;

			AssertNoError("Expected no errors as the reference as it is unique for it's own manifest", bookingLine4.DL_ConsigneeReferenceInfo, uniquePerManifestMessage);
		}

		const string uniquePerManifestMessage = "A consignment reference should be unique for the eManifest.";

		#endregion

		#region Empty References require a GS1 Prefix

		public void TestCheckDL_ConsigneeReference_NoGS1Prefix()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_ConsigneeReference = "SH4633637134723412";

			AssertNoError("Expected no error as bookline has a consignment reference", bookingLine.DL_ConsigneeReferenceInfo, needGS1PrefixMessage);

			bookingLine.DL_ConsigneeReference = "";

			AssertHasError("Expected an error as bookline has no consignment ref and cannot generate a SSCC number", bookingLine.DL_ConsigneeReferenceInfo, needGS1PrefixMessage);
		}

		public void TestCheckDL_ConsigneeReference_GS1PrefixOnShipper()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorCustomCode = consignor.CustomsCodes.AddNew();
			consignorCustomCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			consignorCustomCode.OK_CustomsRegNo = "1234567";

			Factory.Save();

			var bookingHeader = Factory.NewWithValidTestData<SupplierBookingHeader>();
			bookingHeader.DH_OA_Consignor = consignor.MainAddress.PK;

			var bookingLine = bookingHeader.BookingLines.AddNew();
			bookingLine.DL_ConsigneeReference = "";

			AssertNoErrors("Expected no error as bookline reference should be generateable with the consignor GS1 prefix", bookingLine.DL_ConsigneeReferenceInfo);
		}

		public void TestCheckDL_ConsigneeReference_GS1PrefixOnOrgProxy()
		{
			var orgProxy = Factory.Load<OrgHeader>(Env.CurrentCompany.OrganisationPK);
			var orgProxyCustomCode = orgProxy.CustomsCodes.AddNew();
			orgProxyCustomCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			orgProxyCustomCode.OK_CustomsRegNo = "987654321";

			var bookingHeader = Factory.NewWithValidTestData<SupplierBookingHeader>();
			var bookingLine = bookingHeader.BookingLines.AddNew();
			bookingLine.DL_ConsigneeReference = "";

			AssertNoErrors("Expected no error as bookline should be able to generate a SSCC number for the consignee reference", bookingLine.DL_ConsigneeReferenceInfo);
		}

		const string needGS1PrefixMessage = "A GS1 Prefix is needed to generate a SSCC number for booking lines without a consignment reference. Please add this line's unique consignment reference or add a GS1 Prefix to the eManifest's Consignor under Organization > Config > Registration Numbers / Codes.";

		#endregion

		#endregion

		public void TestValidate_DLGrossWeight()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_GrossWeight = -0.01m;

			AssertHasError(bookingLine.DL_GrossWeightInfo, "Gross Weight cannot be negative.");

			bookingLine.DL_GrossWeight = 0.01m;
			AssertNoWarnings(bookingLine.DL_GrossWeightInfo);
		}

		public void TestValidate_DLGrossWeightUQ()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_GrossWeightUQ = "ZZ";

			AssertHasError(bookingLine.DL_GrossWeightUQInfo, "Enter a valid Weight Unit.");

			bookingLine.DL_GrossWeightUQ = "KG";
			AssertNoWarnings(bookingLine.DL_GrossWeightUQInfo);
		}

		public void TestValidate_DLCubicUQ()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_CubicUQ = "ZZ";

			AssertHasError(bookingLine.DL_CubicUQInfo, "Enter a valid Volume Unit.");

			bookingLine.DL_CubicUQ = "M3";
			AssertNoWarnings(bookingLine.DL_CubicUQInfo);
		}

		public void TestValidate_DLRSNKServiceLevel()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_RS_NKServiceLevel = "XXX";

			AssertHasError(bookingLine.DL_RS_NKServiceLevelInfo, "Enter a valid Service Level.");

			bookingLine.DL_RS_NKServiceLevel = "D2D";
			AssertNoWarnings(bookingLine.DL_RS_NKServiceLevelInfo);
		}

		public void TestValidate_DLF3NKPackType()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_F3_NKPackType = "XXX";

			AssertHasError(bookingLine.DL_F3_NKPackTypeInfo, "Enter a valid Pack Type.");

			bookingLine.DL_F3_NKPackType = "PLT";
			AssertNoWarnings(bookingLine.DL_F3_NKPackTypeInfo);
		}

		public void TestValidationSection()
		{
			AssertEquals(AddressValidationSection.SupplierBookingLine, Factory.New<SupplierBookingLine>().ValidationSection);
		}

		#region Consignee

		public void TestValidate_ConsigneeName()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.Validation.ValidateDL_ConsigneeName();

			AssertHasError(bookingLine.DL_ConsigneeNameInfo, "Please enter a value.");

			bookingLine.DL_ConsigneeName = "ConsigneeName";
			AssertNoErrors(bookingLine.DL_ConsigneeNameInfo);
		}

		public void TestValidate_ConsigneeAddress1()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.Validation.ValidateDL_ConsigneeAddress1();

			AssertHasError(bookingLine.DL_ConsigneeAddress1Info, "Please enter a value.");

			bookingLine.DL_ConsigneeAddress1 = "Address1";
			AssertNoErrors(bookingLine.DL_ConsigneeAddress1Info);
		}

		public void TestValidate_ConsigneeCity()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.Validation.ValidateDL_ConsigneeCity();
			AssertHasError(bookingLine.DL_ConsigneeCityInfo, "Please enter a value.");

			bookingLine.DL_ConsigneeCity = "City";
			bookingLine.Validation.ValidateDL_ConsigneeCity();
			AssertNoErrors(bookingLine.DL_ConsigneeCityInfo);

			RawDataRegistry.Instance.JobAddressValidation_CityMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			bookingLine.DL_ConsigneeCity = string.Empty;
			bookingLine.Validation.ValidateDL_ConsigneeCity();
			AssertNoErrors(bookingLine.DL_ConsigneeCityInfo);
		}

		public void TestValidate_ConsigneeState()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_RN_NKConsigneeCountryCode = "AU";
			bookingLine.DL_ConsigneeState = "aaa";
			bookingLine.Validation.ValidateDL_ConsigneeState();
			AssertHasError(bookingLine.DL_ConsigneeStateInfo, "Enter a valid State.");

			bookingLine.DL_ConsigneeState = "NSW";
			bookingLine.Validation.ValidateDL_ConsigneeState();
			AssertNoErrors(bookingLine.DL_ConsigneeStateInfo);

			RawDataRegistry.Instance.JobAddressValidation_UseStateRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			bookingLine.DL_ConsigneeState = "aaa";
			bookingLine.Validation.ValidateDL_ConsigneeState();
			AssertNoErrors(bookingLine.DL_ConsigneeStateInfo);
		}

		public void TestValidate_ConsigneePostCode()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_RN_NKConsigneeCountryCode = "AU";
			bookingLine.Validation.ValidateDL_ConsigneePostCode();
			string errorMessage =
				@"You must enter a postcode. The postcode validation rule for the country/region Australia is currently set to ""Must Be Entered"".

If you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.";

			AssertHasError(bookingLine.DL_ConsigneePostCodeInfo, errorMessage);

			bookingLine.DL_ConsigneePostCode = "aaaa";
			bookingLine.Validation.ValidateDL_ConsigneePostCode();
			AssertNoErrors(bookingLine.DL_ConsigneePostCodeInfo);

			RawDataRegistry.Instance.JobAddressValidation_UsePostcodeRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			bookingLine.DL_ConsigneePostCode = string.Empty;
			bookingLine.Validation.ValidateDL_ConsigneePostCode();
			AssertNoErrors(bookingLine.DL_ConsigneePostCodeInfo);
		}

		public void TestValidate_ConsigneeCountry()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.Validation.ValidateDL_RN_NKConsigneeCountryCode();
			AssertNoErrors(bookingLine.DL_RN_NKConsigneeCountryCodeInfo);

			RawDataRegistry.Instance.JobAddressValidation_CountryMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			bookingLine.Validation.ValidateDL_RN_NKConsigneeCountryCode();
			AssertHasError(bookingLine.DL_RN_NKConsigneeCountryCodeInfo, "Please enter a value.");

			bookingLine.DL_RN_NKConsigneeCountryCode = "11";
			bookingLine.Validation.ValidateDL_RN_NKConsigneeCountryCode();
			AssertHasError(bookingLine.DL_RN_NKConsigneeCountryCodeInfo, "Enter a valid selection.");

			bookingLine.DL_RN_NKConsigneeCountryCode = "AU";
			bookingLine.Validation.ValidateDL_RN_NKConsigneeCountryCode();
			AssertNoErrors(bookingLine.DL_RN_NKConsigneeCountryCodeInfo);
		}

		public void TestValidate_ConsigneePhone()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_ConsigneePhone = "ЫЫЫЫЫ";
			bookingLine.Validation.ValidateDL_ConsigneePhone();
			AssertHasError(bookingLine.DL_ConsigneePhoneInfo, "Consignee Phone only accepts Western European languages characters.");

			bookingLine.DL_ConsigneePhone = "322223";
			bookingLine.Validation.ValidateDL_ConsigneePhone();
			AssertNoErrors(bookingLine.DL_ConsigneePhoneInfo);
		}

		public void TestValidate_ConsigneeMobile()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_ConsigneeMobile = "ЫЫЫЫЫ";
			bookingLine.Validation.ValidateDL_ConsigneeMobile();
			AssertHasError(bookingLine.DL_ConsigneeMobileInfo, "Consignee Mobile only accepts Western European languages characters.");

			bookingLine.DL_ConsigneeMobile = "322223";
			bookingLine.Validation.ValidateDL_ConsigneeMobile();
			AssertNoErrors(bookingLine.DL_ConsigneeMobileInfo);
		}

		public void TestValidate_ConsigneeFax()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_ConsigneeFax = "ЫЫЫЫЫ";
			bookingLine.Validation.ValidateDL_ConsigneeFax();
			AssertHasError(bookingLine.DL_ConsigneeFaxInfo, "Consignee Fax only accepts Western European languages characters.");

			bookingLine.DL_ConsigneeFax = "322223";
			bookingLine.Validation.ValidateDL_ConsigneeFax();
			AssertNoErrors(bookingLine.DL_ConsigneeFaxInfo);
		}

		#endregion

		#region Consignor

		public void TestValidate_ConsignorName()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.Validation.ValidateDL_ConsignorName();

			AssertHasError(bookingLine.DL_ConsignorNameInfo, "Please enter a value.");

			bookingLine.DL_ConsignorName = "ConsignorName";
			AssertNoErrors(bookingLine.DL_ConsignorNameInfo);
		}

		public void TestValidate_ConsignorAddress1()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.Validation.ValidateDL_ConsignorAddress1();

			AssertHasError(bookingLine.DL_ConsignorAddress1Info, "Please enter a value.");

			bookingLine.DL_ConsignorAddress1 = "Address1";
			AssertNoErrors(bookingLine.DL_ConsignorAddress1Info);
		}

		public void TestValidate_ConsignorCity()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.Validation.ValidateDL_ConsignorCity();
			AssertHasError(bookingLine.DL_ConsignorCityInfo, "Please enter a value.");

			bookingLine.DL_ConsignorCity = "City";
			bookingLine.Validation.ValidateDL_ConsignorCity();
			AssertNoErrors(bookingLine.DL_ConsignorCityInfo);

			RawDataRegistry.Instance.JobAddressValidation_CityMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			bookingLine.DL_ConsignorCity = string.Empty;
			bookingLine.Validation.ValidateDL_ConsignorCity();
			AssertNoErrors(bookingLine.DL_ConsignorCityInfo);
		}

		public void TestValidate_ConsignorState()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_RN_NKConsignorCountryCode = "AU";
			bookingLine.DL_ConsignorState = "aaa";
			bookingLine.Validation.ValidateDL_ConsignorState();
			AssertHasError(bookingLine.DL_ConsignorStateInfo, "Enter a valid State.");

			bookingLine.DL_ConsignorState = "NSW";
			bookingLine.Validation.ValidateDL_ConsignorState();
			AssertNoErrors(bookingLine.DL_ConsignorStateInfo);

			RawDataRegistry.Instance.JobAddressValidation_UseStateRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			bookingLine.DL_ConsignorState = "aaa";
			bookingLine.Validation.ValidateDL_ConsignorState();
			AssertNoErrors(bookingLine.DL_ConsignorStateInfo);
		}

		public void TestValidate_ConsignorPostCode()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_RN_NKConsignorCountryCode = "AU";
			bookingLine.Validation.ValidateDL_ConsignorPostCode();
			string errorMessage =
				@"You must enter a postcode. The postcode validation rule for the country/region Australia is currently set to ""Must Be Entered"".

If you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.";

			AssertHasError(bookingLine.DL_ConsignorPostCodeInfo, errorMessage);

			bookingLine.DL_ConsignorPostCode = "aaaa";
			bookingLine.Validation.ValidateDL_ConsignorPostCode();
			AssertNoErrors(bookingLine.DL_ConsignorPostCodeInfo);

			RawDataRegistry.Instance.JobAddressValidation_UsePostcodeRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			bookingLine.DL_ConsignorPostCode = string.Empty;
			bookingLine.Validation.ValidateDL_ConsignorPostCode();
			AssertNoErrors(bookingLine.DL_ConsignorPostCodeInfo);
		}

		public void TestValidate_ConsignorCountry()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.Validation.ValidateDL_RN_NKConsignorCountryCode();
			AssertNoErrors(bookingLine.DL_RN_NKConsignorCountryCodeInfo);

			RawDataRegistry.Instance.JobAddressValidation_CountryMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			bookingLine.Validation.ValidateDL_RN_NKConsignorCountryCode();
			AssertHasError(bookingLine.DL_RN_NKConsignorCountryCodeInfo, "Please enter a value.");

			bookingLine.DL_RN_NKConsignorCountryCode = "11";
			bookingLine.Validation.ValidateDL_RN_NKConsignorCountryCode();
			AssertHasError(bookingLine.DL_RN_NKConsignorCountryCodeInfo, "Enter a valid selection.");

			bookingLine.DL_RN_NKConsignorCountryCode = "AU";
			bookingLine.Validation.ValidateDL_RN_NKConsignorCountryCode();
			AssertNoErrors(bookingLine.DL_RN_NKConsignorCountryCodeInfo);
		}

		public void TestValidate_ConsignorEmail()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_ConsignorEmail = "aaa@@@";
			bookingLine.Validation.ValidateDL_ConsignorEmail();
			AssertHasError(bookingLine.DL_ConsignorEmailInfo, "Email Address is not valid .");

			bookingLine.DL_ConsignorEmail = "aaa@gmail.com";
			bookingLine.Validation.ValidateDL_ConsignorEmail();
			AssertNoErrors(bookingLine.DL_ConsignorEmailInfo);
		}

		public void TestValidate_ConsignorPhone()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_ConsignorPhone = "ЫЫЫЫЫ";
			bookingLine.Validation.ValidateDL_ConsignorPhone();
			AssertHasError(bookingLine.DL_ConsignorPhoneInfo, "Consignor Phone only accepts Western European languages characters.");

			bookingLine.DL_ConsignorPhone = "322223";
			bookingLine.Validation.ValidateDL_ConsignorPhone();
			AssertNoErrors(bookingLine.DL_ConsignorPhoneInfo);
		}

		public void TestValidate_ConsignorMobile()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_ConsignorMobile = "ЫЫЫЫЫ";
			bookingLine.Validation.ValidateDL_ConsignorMobile();
			AssertHasError(bookingLine.DL_ConsignorMobileInfo, "Consignor Mobile only accepts Western European languages characters.");

			bookingLine.DL_ConsignorMobile = "322223";
			bookingLine.Validation.ValidateDL_ConsignorMobile();
			AssertNoErrors(bookingLine.DL_ConsignorMobileInfo);
		}

		public void TestValidate_ConsignorFax()
		{
			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_ConsignorFax = "ЫЫЫЫЫ";
			bookingLine.Validation.ValidateDL_ConsignorFax();
			AssertHasError(bookingLine.DL_ConsignorFaxInfo, "Consignor Fax only accepts Western European languages characters.");

			bookingLine.DL_ConsignorFax = "322223";
			bookingLine.Validation.ValidateDL_ConsignorFax();
			AssertNoErrors(bookingLine.DL_ConsignorFaxInfo);
		}

		#endregion
	}
}
