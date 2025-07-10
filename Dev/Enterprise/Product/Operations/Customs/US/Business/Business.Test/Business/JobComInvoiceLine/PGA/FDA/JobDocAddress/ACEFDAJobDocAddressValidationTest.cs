using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.Business
{
	public class ACEFDAJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public virtual void TestCheckLaboratoryDocAddress()
		{
			TestDocAddressCommonValidations(DocAddressType.Laboratory);
		}

		public virtual void TestCheckManufacturerDocAddress()
		{
			TestDocAddressCommonValidations(DocAddressType.Manufacturer);
		}

		void TestDocAddressCommonValidations(DocAddressType addressType)
		{
			var cusCode = USAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "AU34567");

			var docAddress = FDA.DocAddresses.CreateWithAddressType(addressType);
			docAddress.E2_OA_Address = USAddress.PK;
			docAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, OrganisationValidation.FEICodeFormat);
			AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "State should not be empty.");

			cusCode.OK_CustomsRegNo = "12345678950";
			docAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, OrganisationValidation.FEICodeFormat);

			cusCode.OK_CustomsRegNo = "1234567895";
			USAddress.OA_State = USStateList.Codes.Illinois;
			docAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, "State should not be empty.");
			AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, OrganisationValidation.FEICodeFormat);
			AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);

			USAddress.OA_PostCode = "1023";
			docAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);

			USAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			USAddress.OA_State = ZString.Empty;
			docAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "State should not be empty.");

			USAddress.OA_State = CanadaStatesList.Codes.AB;
			docAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, "State should not be empty.");
		}

		public virtual void TestCheckDeliverToPartyDocAddress()
		{
			var cusCode = USAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "AU34567");
			FDA.US_DeliverToPartyAddress = USAddress.PK;
			FDA.DeliverToPartyDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(FDA.DeliverToPartyDocAddress.E2_OA_AddressInfo, OrganisationValidation.DUNSCodeFormat);
			AssertHasMessageErrorContaining(FDA.DeliverToPartyDocAddress.E2_OA_AddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertHasMessageErrorContaining(FDA.DeliverToPartyDocAddress.E2_OA_AddressInfo, "State should not be empty.");

			cusCode.OK_CustomsRegNo = "123456789";
			USAddress.OA_State = USStateList.Codes.Illinois;
			FDA.DeliverToPartyDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(FDA.DeliverToPartyDocAddress.E2_OA_AddressInfo, OrganisationValidation.DUNSCodeFormat);
			AssertNoMessageErrorContaining(FDA.DeliverToPartyDocAddress.E2_OA_AddressInfo, "State should not be empty.");

			USAddress.OA_PostCode = "1123";
			FDA.DeliverToPartyDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(FDA.DeliverToPartyDocAddress.E2_OA_AddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);

			USAddress.OA_RL_NKRelatedPortCode = "AU111";
			FDA.DeliverToPartyDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(FDA.DeliverToPartyDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoValidation.DeliveryToPartyShouldBeUSAddress);

			USAddress.OA_RL_NKRelatedPortCode = "US111";
			FDA.DeliverToPartyDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(FDA.DeliverToPartyDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoValidation.DeliveryToPartyShouldBeUSAddress);

			USAddress.OA_RL_NKRelatedPortCode = "PR111";
			FDA.DeliverToPartyDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(FDA.DeliverToPartyDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoValidation.DeliveryToPartyShouldBeUSAddress);

			USAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			USAddress.OA_State = ZString.Empty;
			FDA.DeliverToPartyDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(FDA.DeliverToPartyDocAddress.E2_OA_AddressInfo, "State should not be empty.");

			USAddress.OA_State = CanadaStatesList.Codes.AB;
			FDA.DeliverToPartyDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(FDA.DeliverToPartyDocAddress.E2_OA_AddressInfo, "State should not be empty.");
		}

		public void TestCheckDeliverToPartyDocAddress_Country_Override()
		{
			FDA.US_DeliverToPartyAddress = USAddress.PK;
			FDA.DeliverToPartyDocAddress.E2_AddressOverride = true;
			FDA.DeliverToPartyDocAddress.E2_RN_NKCountryCode = "AU";
			AssertHasMessageErrorContaining(FDA.DeliverToPartyDocAddress.E2_RN_NKCountryCodeInfo, USACEFDAAddInfoValidation.DeliveryToPartyShouldBeUSAddress);

			FDA.DeliverToPartyDocAddress.E2_RN_NKCountryCode = "US";
			AssertNoMessageErrorContaining(FDA.DeliverToPartyDocAddress.E2_RN_NKCountryCodeInfo, USACEFDAAddInfoValidation.DeliveryToPartyShouldBeUSAddress);

			FDA.DeliverToPartyDocAddress.E2_RN_NKCountryCode = "PR";
			AssertNoMessageErrorContaining(FDA.DeliverToPartyDocAddress.E2_RN_NKCountryCodeInfo, USACEFDAAddInfoValidation.DeliveryToPartyShouldBeUSAddress);
		}

		public virtual void TestCheckFSVPImporterDocAddress()
		{
			var usZip = Factory.New<USCZipCode>();
			usZip.UZ_BeginZipCodeRange = "15000";
			usZip.UZ_EndZipCodeRange = "19699";
			usZip.UZ_State = "NY";
			Factory.Save();

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_FDAForcePN = false;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;

			FDA.US_FSVPImporterAddress = USAddress.PK;
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertHasMessageErrorContaining(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, "State should not be empty.");

			USAddress.OA_State = "NY";
			USAddress.OA_PostCode = "19444";
			USAddress.OA_State = USStateList.Codes.Illinois;
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertNoMessageErrorContaining(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, "State should not be empty.");

			USAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "12345678");
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, OrganisationValidation.DUNSCodeFormat);

			USAddress.CustomsCodes[0].OK_CustomsRegNo = "123456789";
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, OrganisationValidation.DUNSCodeFormat);

			USAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, "FSVP Importer must have a US Address");

			USAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, "FSVP Importer must have a US Address");

			USAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			USAddress.OA_State = ZString.Empty;
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, "State should not be empty.");

			USAddress.OA_State = CanadaStatesList.Codes.AB;
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, "State should not be empty.");
		}

		public void TestCheckFSVPImporterDocAddress_DUNSNumberValidation()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_FDAForcePN = false;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;

			FDA.US_FSVPImporterAddress = USAddress.PK;
			AssertHasMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoValidation.DUNSCodeRequired);

			var dunsNumber = FDA.FSVPImporterAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, USACEFDAAddInfoValidation.DUNSUnknown, "US");
			FDA.FSVPImporterDocAddress.Address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "123456", "US");
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoValidation.DUNSCodeRequired);

			dunsNumber.OK_CustomsRegNo = "423";
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, OrganisationValidation.DUNSCodeFormat);
		}

		public void TestCheckFSVPImporterDocAddress_Country_Override()
		{
			FDA.US_FSVPImporterAddress = USAddress.PK;
			FDA.FSVPImporterDocAddress.E2_AddressOverride = true;
			FDA.FSVPImporterDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			AssertHasMessageError(FDA.FSVPImporterDocAddress.E2_RN_NKCountryCodeInfo, "FSVP Importer must have a US Address");

			FDA.FSVPImporterDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertNoMessageError(FDA.FSVPImporterDocAddress.E2_RN_NKCountryCodeInfo, "FSVP Importer must have a US Address");
		}

		public virtual void TestCheckShipperDocAddress()
		{
			var cusCode = USAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "AU34567");
			FDA.US_OA_ShipperAddress = USAddress.PK;
			FDA.ShipperDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(FDA.ShipperDocAddress.E2_OA_AddressInfo, OrganisationValidation.DUNSCodeFormat);
			AssertHasMessageErrorContaining(FDA.ShipperDocAddress.E2_OA_AddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertHasMessageError(FDA.ShipperDocAddress.E2_OA_AddressInfo, "State should not be empty.");

			USAddress.OA_PostCode = "2008";
			USAddress.OA_State = USStateList.Codes.Illinois;
			cusCode.OK_CustomsRegNo = "123456789";
			FDA.ShipperDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(FDA.ShipperDocAddress.E2_OA_AddressInfo, OrganisationValidation.DUNSCodeFormat);
			AssertNoMessageErrorContaining(FDA.ShipperDocAddress.E2_OA_AddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertNoMessageError(FDA.ShipperDocAddress.E2_OA_AddressInfo, "State should not be empty.");

			USAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			USAddress.OA_State = ZString.Empty;
			FDA.ShipperDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.ShipperDocAddress.E2_OA_AddressInfo, "State should not be empty.");

			USAddress.OA_State = CanadaStatesList.Codes.NL;
			FDA.ShipperDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(FDA.ShipperDocAddress.E2_OA_AddressInfo, "State should not be empty.");
		}

		public virtual void TestCheckShipperDocAddressShouldBeCanada()
		{
			USAddress.OA_State = USStateList.Codes.Illinois;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			FDA.US_OA_ShipperAddress = USAddress.PK;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_804;
			FDA.ShipperDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.ShipperDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoValidation.DRU_804_ShouldBeCanadaAddress);

			USAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			FDA.ShipperDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(FDA.ShipperDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoValidation.DRU_804_ShouldBeCanadaAddress);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_INV;
			USAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			FDA.ShipperDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(FDA.ShipperDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoValidation.DRU_804_ShouldBeCanadaAddress);
		}

		public virtual void TestCheckShipperDocAddress_Country_Override()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			FDA.US_OA_ShipperAddress = USAddress.PK;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_804;
			FDA.ShipperDocAddress.E2_AddressOverride = true;
			FDA.ShipperDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			FDA.ShipperDocAddress.Validation.ValidateE2_RN_NKCountryCode();
			AssertHasMessageError(FDA.ShipperDocAddress.E2_RN_NKCountryCodeInfo, USACEFDAAddInfoValidation.DRU_804_ShouldBeCanadaAddress);

			FDA.ShipperDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			AssertNoMessageError(FDA.ShipperDocAddress.E2_RN_NKCountryCodeInfo, USACEFDAAddInfoValidation.DRU_804_ShouldBeCanadaAddress);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_INV;
			FDA.ShipperDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertNoMessageError(FDA.ShipperDocAddress.E2_RN_NKCountryCodeInfo, USACEFDAAddInfoValidation.DRU_804_ShouldBeCanadaAddress);
		}

		public virtual void TestCheckFDAImporterDocAddress()
		{
			var usZip = Factory.New<USCZipCode>();
			usZip.UZ_BeginZipCodeRange = "15000";
			usZip.UZ_EndZipCodeRange = "19699";
			usZip.UZ_State = "NY";
			Factory.Save();

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;

			var nlOrg = Factory.New<OrgHeader>();
			var nlOrgAddress = nlOrg.MainAddress;
			nlOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
			nlOrgAddress.OA_State = "NH";
			nlOrgAddress.OA_PostCode = "11";

			FDA.US_FDAImporterAddress = USAddress.PK;
			AssertHasMessageErrorContaining(FDA.FDAImporterDocAddress.E2_OA_AddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertHasMessageErrorContaining(FDA.FDAImporterDocAddress.E2_OA_AddressInfo, "State should not be empty.");

			USAddress.OA_State = USStateList.Codes.NewYork;
			USAddress.OA_PostCode = "19444";
			FDA.FDAImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(FDA.FDAImporterDocAddress.E2_OA_AddressInfo, "The zip code entered for NY is not valid. The first five numbers of the zip code should fall between 15000 and 19699,\r\nor between 00400 and 00599,\r\nor between 09000 and 14999");
			AssertNoMessageErrorContaining(FDA.FDAImporterDocAddress.E2_OA_AddressInfo, "State should not be empty.");

			USAddress.OA_PostCode = "88881";
			FDA.FDAImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.FDAImporterDocAddress.E2_OA_AddressInfo, "The zip code entered for NY is not valid. The first five numbers of the zip code should fall between 15000 and 19699,\r\nor between 00400 and 00599,\r\nor between 09000 and 14999");

			USAddress.OA_State = USStateList.Codes.Missouri;
			USAddress.OA_PostCode = "19444";
			FDA.FDAImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.FDAImporterDocAddress.E2_OA_AddressInfo, "The zip code entered for MO is not valid. The first five numbers of the zip code should fall between 63000 and 65899");

			USAddress.OA_State = USStateList.Codes.NewYork;
			FDA.FDAImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(FDA.FDAImporterDocAddress.E2_OA_AddressInfo, "The zip code entered for NY is not valid. The first five numbers of the zip code should fall between 15000 and 19699,\r\nor between 00400 and 00599,\r\nor between 09000 and 14999");
			USAddress.OA_PostCode = "15123";
			FDA.FDAImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(FDA.FDAImporterDocAddress.E2_OA_AddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);

			USAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			USAddress.OA_State = ZString.Empty;
			FDA.FDAImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(FDA.FDAImporterDocAddress.E2_OA_AddressInfo, "State should not be empty.");

			USAddress.OA_State = CanadaStatesList.Codes.AB;
			FDA.FDAImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(FDA.FDAImporterDocAddress.E2_OA_AddressInfo, "State should not be empty.");

			USAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			USAddress.OA_State = USStateList.Codes.NewHampshire;
			USAddress.OA_PostCode = "11";
			FDA.FDAImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(FDA.FDAImporterDocAddress.E2_OA_AddressInfo, "The zip code entered for NH is not valid. The first five numbers of the zip code should fall between 03000 and 03899");

			FDA.FDAImporterDocAddress.E2_OA_Address = nlOrgAddress.PK;
			FDA.FDAImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(FDA.FDAImporterDocAddress.E2_OA_AddressInfo, "The zip code entered for NH is not valid. The first five numbers of the zip code should fall between 03000 and 03899");
		}

		public void TestCheckFDAImporterDocAddress_Postcode_Override()
		{
			var usZip = Factory.New<USCZipCode>();
			usZip.UZ_BeginZipCodeRange = "15000";
			usZip.UZ_EndZipCodeRange = "19699";
			usZip.UZ_State = "NY";
			Factory.Save();

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_FDAImporterAddress = USAddress.PK;
			FDA.FDAImporterDocAddress.E2_AddressOverride = true;
			FDA.FDAImporterDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			FDA.FDAImporterDocAddress.Validation.ValidateE2_Postcode();
			AssertHasMessageErrorContaining(FDA.FDAImporterDocAddress.E2_PostcodeInfo, ZipCodeValidation.ZIPCanNotBeEmpty);

			FDA.FDAImporterDocAddress.State = USStateList.Codes.NewYork;
			FDA.FDAImporterDocAddress.Postcode = "19444";
			AssertNoMessageError(FDA.FDAImporterDocAddress.E2_PostcodeInfo, "The zip code entered for NY is not valid. The first five numbers of the zip code should fall between 15000 and 19699,\r\nor between 00400 and 00599,\r\nor between 09000 and 14999");

			FDA.FDAImporterDocAddress.Postcode = "88881";
			AssertHasMessageError(FDA.FDAImporterDocAddress.E2_PostcodeInfo, "The zip code entered for NY is not valid. The first five numbers of the zip code should fall between 15000 and 19699,\r\nor between 00400 and 00599,\r\nor between 09000 and 14999");

			FDA.FDAImporterDocAddress.State = USStateList.Codes.Missouri;
			FDA.FDAImporterDocAddress.Postcode = "19444";
			AssertHasMessageError(FDA.FDAImporterDocAddress.E2_PostcodeInfo, "The zip code entered for MO is not valid. The first five numbers of the zip code should fall between 63000 and 65899");

			FDA.FDAImporterDocAddress.State = USStateList.Codes.NewYork;
			FDA.FDAImporterDocAddress.Validation.ValidateE2_Postcode();
			AssertNoMessageError(FDA.FDAImporterDocAddress.E2_PostcodeInfo, "The zip code entered for NY is not valid. The first five numbers of the zip code should fall between 15000 and 19699,\r\nor between 00400 and 00599,\r\nor between 09000 and 14999");
			FDA.FDAImporterDocAddress.Postcode = "15123";
			AssertNoMessageErrorContaining(FDA.FDAImporterDocAddress.E2_PostcodeInfo, ZipCodeValidation.ZIPCanNotBeEmpty);

			FDA.FDAImporterDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			FDA.FDAImporterDocAddress.State = USStateList.Codes.NewHampshire;
			FDA.FDAImporterDocAddress.Postcode = "11";
			AssertHasMessageErrorContaining(FDA.FDAImporterDocAddress.E2_PostcodeInfo, "The zip code entered for NH is not valid. The first five numbers of the zip code should fall between 03000 and 03899");
		}

		public virtual void TestCheckGoodsOwnerDocAddress()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			var docAddress = FDA.DocAddresses.CreateWithAddressType(DocAddressType.GoodsOwner);
			docAddress.E2_OA_Address = USAddress.PK;
			var cusCode = USAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "AU34567");
			docAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, OrganisationValidation.DUNSCodeFormat);
			AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "State should not be empty.");

			cusCode.OK_CustomsRegNo = "123456789";
			USAddress.OA_PostCode = "1123";
			USAddress.OA_State = USStateList.Codes.Illinois;
			docAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, OrganisationValidation.DUNSCodeFormat);
			AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, "State should not be empty.");

			USAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			USAddress.OA_State = ZString.Empty;
			docAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "State should not be empty.");

			USAddress.OA_State = CanadaStatesList.Codes.AB;
			docAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, "State should not be empty.");
		}

		public void TestCheckGoodsLocationDocAddress()
		{
			var docAddress = FDA.DocAddresses.CreateWithAddressType(DocAddressType.GoodsLocation);
			docAddress.E2_OA_Address = USAddress.PK;
			docAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertHasMessageError(docAddress.E2_OA_AddressInfo, "State should not be empty.");
			AssertNoMessageError(docAddress.E2_OA_AddressInfo, USACEFDAAddInfoValidation.GoodsLocationShouldBeUSAddress);

			USAddress.OA_PostCode = "2008";
			USAddress.OA_State = USStateList.Codes.Illinois;
			docAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertNoMessageError(docAddress.E2_OA_AddressInfo, "State should not be empty.");

			USAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			USAddress.OA_State = ZString.Empty;
			docAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(docAddress.E2_OA_AddressInfo, "State should not be empty.");
			AssertHasMessageError(docAddress.E2_OA_AddressInfo, USACEFDAAddInfoValidation.GoodsLocationShouldBeUSAddress);

			USAddress.OA_State = CanadaStatesList.Codes.NL;
			docAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(docAddress.E2_OA_AddressInfo, "State should not be empty.");
		}

		public void TestCheckGoodsLocationDocAddress_Country_Override()
		{
			var docAddress = FDA.DocAddresses.CreateWithAddressType(DocAddressType.GoodsLocation);
			docAddress.E2_AddressOverride = true;
			docAddress.E2_RN_NKCountryCode = "US";
			AssertNoMessageError(docAddress.E2_RN_NKCountryCodeInfo, USACEFDAAddInfoValidation.GoodsLocationShouldBeUSAddress);

			docAddress.E2_RN_NKCountryCode = "CA";
			AssertHasMessageError(docAddress.E2_RN_NKCountryCodeInfo, USACEFDAAddInfoValidation.GoodsLocationShouldBeUSAddress);
		}

		public void TestCheckGoodsLocationDocAddress_ProgramDEV()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			var docAddress = FDA.DocAddresses.CreateWithAddressType(DocAddressType.GoodsLocation);
			docAddress.E2_OA_Address = USAddress.PK;
			docAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError("Goods Location should not be entered when Program Code is 'DEV'", docAddress.E2_OA_AddressInfo, USACEFDAAddInfoValidation.GoodsLocationNotRequired);

			docAddress.E2_OA_Address = ZGuid.Empty;
			docAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError("Goods Location should not be entered when Program Code is 'DEV'", docAddress.E2_OA_AddressInfo, USACEFDAAddInfoValidation.GoodsLocationNotRequired);
		}

		public void TestCheckE2_GovRegNum()
		{
			var docAddress = FDA.DocAddresses.CreateWithAddressType(DocAddressType.Shipper);
			docAddress.E2_AddressOverride = true;
			AssertNoMessageErrors(docAddress.E2_GovRegNumInfo);
			docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
			AssertHasMessageErrorContaining(docAddress.E2_GovRegNumInfo, MandatoryValidation.YouHaveNotEntered);
			docAddress.E2_GovRegNum = "A111";
			AssertHasWarning(docAddress.E2_GovRegNumInfo, EstablishmentIdentifierValidator.EstablishmentIdentifierRightFormat);
			docAddress.E2_GovRegNum = "11111111111";
			AssertHasWarning(docAddress.E2_GovRegNumInfo, EstablishmentIdentifierValidator.EstablishmentIdentifierRightFormat);
			docAddress.E2_GovRegNum = "1111111111";
			AssertNoNotifications(docAddress.E2_GovRegNumInfo);
			docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			docAddress.E2_GovRegNum = "A111";
			AssertHasWarning(docAddress.E2_GovRegNumInfo, DataUniversalNumberingSystemValidator.DUNSNumberFormat);
			docAddress.E2_GovRegNum = "1111111111";
			AssertHasWarning(docAddress.E2_GovRegNumInfo, DataUniversalNumberingSystemValidator.DUNSNumberFormat);
			docAddress.E2_GovRegNum = "111111111";
			AssertNoNotifications(docAddress.E2_GovRegNumInfo);
		}

		public void TestCheckE2_GovRegNumType()
		{
			var docAddress = FDA.DocAddresses.CreateWithAddressType(DocAddressType.Shipper);
			docAddress.E2_AddressOverride = true;
			AssertNoMessageErrors(docAddress.E2_GovRegNumTypeInfo);
			docAddress.E2_GovRegNumType = "XXX";
			AssertHasMessageErrorContaining(docAddress.E2_GovRegNumTypeInfo, ListValidation.InvalidCodeMessageError.ToString());
			docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
			AssertNoMessageErrors(docAddress.E2_GovRegNumTypeInfo);
			docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			AssertNoMessageErrors(docAddress.E2_GovRegNumTypeInfo);
			docAddress.E2_GovRegNumType = "";
			AssertNoMessageErrors(docAddress.E2_GovRegNumTypeInfo);
			docAddress.E2_GovRegNum = "111111111";
			AssertHasMessageErrorContaining(docAddress.E2_GovRegNumTypeInfo, MandatoryValidation.YouHaveNotEntered);

			docAddress = FDA.DocAddresses.CreateWithAddressType(DocAddressType.FSVPImporter);
			docAddress.E2_AddressOverride = true;
			AssertHasMessageError(docAddress.E2_GovRegNumTypeInfo, ACEFDAJobDocAddressValidation.DUNSNumberIsRequiredMessage);
			docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			AssertNoMessageErrors(docAddress.E2_GovRegNumTypeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			registryDisposition = USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			registryDisposition.Dispose();
		}

		IDisposable registryDisposition;

		protected ACEFDA FDA
		{
			get
			{
				if (fda == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EntryFilerCode = "XJ5";
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					fda = invoiceLine.ACE_FDALines.AddNew();
				}
				return fda;
			}
		}
		ACEFDA fda;

		protected OrgAddress USAddress
		{
			get
			{
				if (usAddress == null)
				{
					var party = Factory.New<OrgHeader>();
					usAddress = party.MainAddress;
					usAddress.OA_RN_NKCountryCode = "US";
				}
				return usAddress;
			}
		}
		OrgAddress usAddress;
	}
}
