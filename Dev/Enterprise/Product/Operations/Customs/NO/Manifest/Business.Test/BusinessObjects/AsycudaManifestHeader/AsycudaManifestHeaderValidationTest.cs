using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(AsycudaManifestHeaderValidation))]
sealed class AsycudaManifestHeaderValidationTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderValidationTest
{
	public void TestCheckAMA_TransportMeans()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var today = ZDateTime.Now;
		var yesterday = today.AddDays(-1);
		var tomorrow = today.AddDays(1);
		const string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NOTransportationMeans;
		_ = helper.CreateNewOrGetExistingCusCodeType(codeType, "CL751 Description", Core.Constants.CountryCodes.Norway);
		_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Norway, codeType, "111", "111 Description", yesterday, tomorrow);
		_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Norway, codeType, "112", "112 Description", yesterday, tomorrow);
		_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Norway, codeType, "222", "222 Description", yesterday, tomorrow);
		_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Norway, codeType, "333", "333 Description", yesterday, tomorrow);
		Factory.Save();

		CombineAssertions("MOT - SEA", () =>
		{
			ManifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			ValidationTestHelper.AssertInvalidCodeMessageError(ManifestHeader.AMA_TransportMeansInfo, "AAA", "111");
			ValidationTestHelper.AssertInvalidCodeMessageError(ManifestHeader.AMA_TransportMeansInfo, "AAA", "112");
			ValidationTestHelper.AssertInvalidCodeMessageError(ManifestHeader.AMA_TransportMeansInfo, "AAA", "222");
		});
		CombineAssertions("MOT - RAI", () =>
		{
			ManifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Rail;
			ValidationTestHelper.AssertInvalidCodeMessageError(ManifestHeader.AMA_TransportMeansInfo, "AAA", "222");
		});
	}

	public void TestCheckAMA_OA_Carrier_IfNotEntered()
	{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(ManifestHeader.AMA_OA_CarrierInfo);
	}

	public void TestCheckAMA_OA_Carrier_OrgCusCodeValidation()
	{
		var message = "The carrier must have an ID of type MVA, ORG or EOR for usage in DMO manifest.";
		var carrierOrgHeader = Factory.New<OrgHeader>();
		var carrierOrgAddress = carrierOrgHeader.Addresses.AddNew();
		var carrier = Factory.New<OrgCusCode>();

		CombineAssertions(() =>
		{
			carrier.OK_CodeType = OrgCusCode.CodeTypes.TaxFileCode;
			carrierOrgAddress.Header.CustomsCodes.Add(carrier);
			ManifestHeader.AMA_OA_Carrier = carrierOrgAddress.PK;
			AssertHasMessageErrorContaining(ManifestHeader.AMA_OA_CarrierInfo, message);

			carrier.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			ManifestHeader.AMA_OA_Carrier = carrierOrgAddress.PK;
			AssertNoMessageErrorContaining(ManifestHeader.AMA_OA_CarrierInfo, message);
		});
	}

	public void TestCheckAMA_OA_Carrier_PhoneOrEmail()
	{
		const string message = "The carrier must have phone number or an email address in an address of type 'Customs Address of Record' or 'Office Address' for usage in DMO manifest. See the Organization’s Address tab";
		var carrierOrgHeader = Factory.New<OrgHeader>();
		var address = carrierOrgHeader.Addresses.AddNew();

		CombineAssertions(() =>
		{
			AssertMessageError(hasMessageError: true, OrgAddressType.CustomsAddressOfRecord);
			AssertMessageError(hasMessageError: false, OrgAddressType.CustomsAddressOfRecord, email: "test@example.com");
			AssertMessageError(hasMessageError: false, OrgAddressType.CustomsAddressOfRecord, phone: "1234");
			AssertMessageError(hasMessageError: true, OrgAddressType.Office);
			AssertMessageError(hasMessageError: false, OrgAddressType.Office, email: "test@example.com");
			AssertMessageError(hasMessageError: false, OrgAddressType.Office, phone: "1234");
			AssertMessageError(hasMessageError: true, OrgAddressType.Receivables);
			AssertMessageError(hasMessageError: true, OrgAddressType.Receivables, email: "test@example.com");
			AssertMessageError(hasMessageError: true, OrgAddressType.Receivables, phone: "1234");
		});

		void AssertMessageError(bool hasMessageError, OrgAddressType addressType, string email = "", string phone = "")
		{
			address.AddAddressType(addressType);
			address.OA_Email = email;
			address.OA_Phone = phone;
			ManifestHeader.AMA_OA_Carrier = address.PK;
			if (hasMessageError)
			{
				AssertHasMessageErrorContaining($"When addressType is '{addressType}', email is '{email}' and phone is '{phone}'", ManifestHeader.AMA_OA_CarrierInfo, message);
			}
			else
			{
				AssertNoMessageErrorContaining($"When addressType is '{addressType}', email is '{email}' and phone is '{phone}'", ManifestHeader.AMA_OA_CarrierInfo, message);
			}
			address.DeleteAddressType(addressType);
		}
	}

	public void TestCheckAMA_OA_Carrier_OrgCusCode_And_PhoneOrEmail_Validation()
	{
		const string message2 = "The carrier must have phone number or an email address in an address of type 'Customs Address of Record' or 'Office Address' for usage in DMO manifest. See the Organization’s Address tab";
		const string message1 = "The carrier must have an ID of type MVA, ORG or EOR for usage in DMO manifest.";
		var carrierOrgHeader = Factory.New<OrgHeader>();
		var carrierOrgAddress = carrierOrgHeader.Addresses.AddNew();
		var carrier = Factory.New<OrgCusCode>();

		CombineAssertions(() =>
		{
			AssertOrgCusCodeAndContactInfo(hasMessageError1: true, hasMessageError2: true, OrgCusCode.CodeTypes.TaxFileCode, OrgAddressType.CustomsAddressOfRecord);
			AssertOrgCusCodeAndContactInfo(hasMessageError1: false, hasMessageError2: false, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, OrgAddressType.Office, email: "valid@example.com");
			AssertOrgCusCodeAndContactInfo(hasMessageError1: false, hasMessageError2: true, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, OrgAddressType.Office);
			AssertOrgCusCodeAndContactInfo(hasMessageError1: true, hasMessageError2: false, OrgCusCode.CodeTypes.TaxFileCode, OrgAddressType.Office, phone: "1234");
		});

		void AssertOrgCusCodeAndContactInfo(bool hasMessageError1, bool hasMessageError2, ZString codeType, OrgAddressType addressType, string email = "", string phone = "")
		{
			carrier.OK_CodeType = codeType;
			carrierOrgAddress.Header.CustomsCodes.Add(carrier);
			var address = carrierOrgHeader.Addresses.AddNew();
			address.AddAddressType(addressType);
			address.OA_Email = email;
			address.OA_Phone = phone;
			ManifestHeader.AMA_OA_Carrier = carrierOrgAddress.PK;
			var scenario = $"When CodeType is '{codeType}', AddressType is '{addressType}', Email is '{email}' and Phone is '{phone}'";
			if (hasMessageError1)
			{
				AssertHasMessageErrorContaining(scenario, ManifestHeader.AMA_OA_CarrierInfo, message1);
			}
			else
			{
				AssertNoMessageErrorContaining(scenario, ManifestHeader.AMA_OA_CarrierInfo, message1);
			}
			if (hasMessageError2)
			{
				AssertHasMessageErrorContaining(scenario, ManifestHeader.AMA_OA_CarrierInfo, message2);
			}
			else
			{
				AssertNoMessageErrorContaining(scenario, ManifestHeader.AMA_OA_CarrierInfo, message2);
			}
			address.DeleteAddressType(addressType);
		}
	}

	public void TestCheckAMA_TransportMeans_IfNotEntered()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(ManifestHeader.AMA_TransportMeansInfo);
	}

	public void TestCheckAMA_RN_NKConveyanceNationality_IfNotEntered()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(ManifestHeader.AMA_RN_NKConveyanceNationalityInfo);
	}

	public void TestCheckAMA_DriverName_IfNotEntered()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(ManifestHeader.AMA_DriverNameInfo);
	}

	public void TestCheckAMA_DriverCommunicationId_IfNotEntered()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(ManifestHeader.AMA_DriverCommunicationIdInfo);
	}

	public new void TestCheckAMA_DateAtCustomsOffice()
	{
		ManifestHeader.AMA_DateAtCustomsOffice = ZDateTime.Empty;
		AssertHasMessageErrorContaining(manifestHeader.AMA_DateAtCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

		ManifestHeader.AMA_DateAtCustomsOffice = ZDateTime.Now;
		AssertNoMessageErrorContaining(manifestHeader.AMA_DateAtCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
	}

	AsycudaManifestHeader ManifestHeader => manifestHeader ??= Factory.New<AsycudaManifestHeader>();
	AsycudaManifestHeader manifestHeader;
}
