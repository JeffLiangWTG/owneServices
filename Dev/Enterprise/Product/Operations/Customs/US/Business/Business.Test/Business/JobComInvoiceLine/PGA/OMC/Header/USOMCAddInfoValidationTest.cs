using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USOMCAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestOMCValidateCharactorsForAddressDescription()
		{
			string addressDescriptionWarning = "Address Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			string addressCodeWarning = "Address Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";

			var party1 = Factory.New<OrgHeader>();
			var orgAddress1 = party1.MainAddress;
			orgAddress1.OA_City = "KYIV";
			orgAddress1.OA_Address1 = "éééÄöß";
			orgAddress1.OA_Address2 = "Address2Äöß";
			orgAddress1.OA_Code = "öß";

			var party2 = Factory.New<OrgHeader>();
			var orgAddress2 = party2.MainAddress;
			orgAddress2.OA_City = "KYIV";
			orgAddress2.OA_Address1 = "address1";
			orgAddress2.OA_Address2 = "address2";
			orgAddress2.OA_Code = "code";

			Header.US_OA_Exporter = orgAddress1.PK;
			AssertHasWarning(Header.US_OA_ExporterInfo, addressDescriptionWarning);
			AssertHasWarning(Header.US_OA_ExporterInfo, addressCodeWarning);
			Header.US_OA_Exporter = orgAddress2.PK;
			AssertNoWarning(Header.US_OA_ExporterInfo, addressDescriptionWarning);
			AssertNoWarning(Header.US_OA_ExporterInfo, addressCodeWarning);

			Header.US_OA_ResponsibleGovernmentOfficial = orgAddress1.PK;
			AssertHasWarning(Header.US_OA_ResponsibleGovernmentOfficialInfo, addressDescriptionWarning);
			AssertHasWarning(Header.US_OA_ResponsibleGovernmentOfficialInfo, addressCodeWarning);
			Header.US_OA_ResponsibleGovernmentOfficial = orgAddress2.PK;
			AssertNoWarning(Header.US_OA_ResponsibleGovernmentOfficialInfo, addressDescriptionWarning);
			AssertNoWarning(Header.US_OA_ResponsibleGovernmentOfficialInfo, addressCodeWarning);

			Header.US_OA_AquacultureFacility = orgAddress1.PK;
			AssertHasWarning(Header.US_OA_AquacultureFacilityInfo, addressDescriptionWarning);
			AssertHasWarning(Header.US_OA_AquacultureFacilityInfo, addressCodeWarning);
			Header.US_OA_AquacultureFacility = orgAddress2.PK;
			AssertNoWarning(Header.US_OA_AquacultureFacilityInfo, addressDescriptionWarning);
			AssertNoWarning(Header.US_OA_AquacultureFacilityInfo, addressCodeWarning);
		}

		public void TestOMCValidateState()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "MX";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			OMCValidateState(Header, Header.US_OA_ExporterInfo);
			OMCValidateState(Header, Header.US_OA_ResponsibleGovernmentOfficialInfo);
		}

		void OMCValidateState(OMCHeader header, ZPropertyInfo propertyInfo)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			var address = orgHeader.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_State = "XXXX";
			address.OA_RL_NKRelatedPortCode = "CAABC";
			propertyInfo.Value = address.PK;
			header.Validation.ValidateAll();
			AssertHasMessageErrorContaining(propertyInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			propertyInfo.Value = address.PK;
			header.Validation.ValidateAll();
			AssertNoMessageErrorContaining(propertyInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			propertyInfo.Value = address.PK;
			header.Validation.ValidateAll();
			AssertNoMessageErrorContaining(propertyInfo, "The state is not a valid");
		}

		public void TestCheckUS_ExporterPGAContactPhoneNo()
		{
			Header.US_ExporterPGAContactEmail = "";
			Header.US_ExporterPGAContactPhoneNo = "";
			AssertHasMessageErrorContaining(Header.US_ExporterPGAContactPhoneNoInfo, "Either Phone number or Email Address is required");

			Header.US_ExporterPGAContactEmail = "";
			Header.US_ExporterPGAContactPhoneNo = "+61401123123";
			AssertNoMessageErrorContaining(Header.US_ExporterPGAContactPhoneNoInfo, "Either Phone number or Email Address is required");

			Header.US_ExporterPGAContactEmail = "test@test.com";
			Header.US_ExporterPGAContactPhoneNo = "";
			AssertNoMessageErrorContaining(Header.US_ExporterPGAContactPhoneNoInfo, "Either Phone number or Email Address is required");
		}

		public void TestCheckUS_ExporterPGAContactEmail()
		{
			Header.US_ExporterPGAContactPhoneNo = "";
			Header.US_ExporterPGAContactEmail = "";
			AssertHasMessageErrorContaining(Header.US_ExporterPGAContactEmailInfo, "Either Phone number or Email Address is required");

			Header.US_ExporterPGAContactPhoneNo = "+61401123123";
			Header.US_ExporterPGAContactEmail = "";
			AssertNoMessageErrorContaining(Header.US_ExporterPGAContactEmailInfo, "Either Phone number or Email Address is required");

			Header.US_ExporterPGAContactPhoneNo = "";
			Header.US_ExporterPGAContactEmail = "test@test.com";
			AssertNoMessageErrorContaining(Header.US_ExporterPGAContactEmailInfo, "Either Phone number or Email Address is required");
		}

		public void TestCheckUS_OfficialPGAContactPhoneNo()
		{
			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7B;

			Header.US_OfficialPGAContactEmail = "";
			Header.US_OfficialPGAContactPhoneNo = "";
			AssertNoMessageErrorContaining(Header.US_OfficialPGAContactPhoneNoInfo, "Either Phone number or Email Address is required");

			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7A1;
			Header.US_OfficialPGAContactPhoneNo = "";
			AssertHasMessageErrorContaining(Header.US_OfficialPGAContactPhoneNoInfo, "Either Phone number or Email Address is required");

			Header.US_OfficialPGAContactEmail = "";
			Header.US_OfficialPGAContactPhoneNo = "+61401123123";
			AssertNoMessageErrorContaining(Header.US_OfficialPGAContactPhoneNoInfo, "Either Phone number or Email Address is required");

			Header.US_ExporterPGAContactEmail = "test@test.com";
			Header.US_ExporterPGAContactPhoneNo = "";
			AssertNoMessageErrorContaining(Header.US_OfficialPGAContactPhoneNoInfo, "Either Phone number or Email Address is required");
		}

		public void TestCheckUS_OfficialPGAContactEmail()
		{
			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7B;

			Header.US_OfficialPGAContactPhoneNo = "";
			Header.US_OfficialPGAContactEmail = "";
			AssertNoMessageErrorContaining(Header.US_OfficialPGAContactEmailInfo, "Either Phone number or Email Address is required");

			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7A1;
			Header.US_OfficialPGAContactEmail = "";
			AssertHasMessageErrorContaining(Header.US_OfficialPGAContactEmailInfo, "Either Phone number or Email Address is required");

			Header.US_OfficialPGAContactPhoneNo = "+61401123123";
			Header.US_OfficialPGAContactEmail = "";
			AssertNoMessageErrorContaining(Header.US_OfficialPGAContactEmailInfo, "Either Phone number or Email Address is required");

			Header.US_ExporterPGAContactPhoneNo = "";
			Header.US_ExporterPGAContactEmail = "test@test.com";
			AssertNoMessageErrorContaining(Header.US_OfficialPGAContactEmailInfo, "Either Phone number or Email Address is required");
		}

		public void TestUS_OfficialPGAContactName()
		{
			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7A1;
			Header.US_OfficialPGAContactName = ZString.Empty;
			AssertHasMessageErrorContaining(Header.US_OfficialPGAContactNameInfo, MandatoryValidation.YouHaveNotEnteredMessage("value"));

			Header.US_OfficialPGAContactName = "Bob";
			AssertNoMessageErrorContaining(Header.US_OfficialPGAContactNameInfo, MandatoryValidation.YouHaveNotEnteredMessage("value"));

			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7B;
			Header.US_OfficialPGAContactName = ZString.Empty;
			AssertNoMessageErrorContaining(Header.US_OfficialPGAContactNameInfo, MandatoryValidation.YouHaveNotEnteredMessage("value"));
		}

		public void TestUS_OA_ResponsibleGovernmentOfficial()
		{
			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7A1;
			Header.US_OA_ResponsibleGovernmentOfficial = ZGuid.Empty;
			AssertHasMessageErrorContaining(Header.US_OA_ResponsibleGovernmentOfficialInfo, MandatoryValidation.YouHaveNotEnteredMessage("Responsib Gov't Official"));

			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7B;
			Header.US_OA_ResponsibleGovernmentOfficial = ZGuid.Empty;
			AssertNoMessageErrorContaining(Header.US_OA_ResponsibleGovernmentOfficialInfo, MandatoryValidation.YouHaveNotEnteredMessage("Responsib Gov't Official"));
		}

		public void TestUS_OfficialCertificationDate()
		{
			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7A1;
			Header.US_OfficialCertificationDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(Header.US_OfficialCertificationDateInfo, MandatoryValidation.YouHaveNotEnteredMessage("value"));

			Header.US_OfficialCertificationDate = ZDateTime.BrettsBirthday;
			AssertNoMessageErrorContaining(Header.US_OfficialCertificationDateInfo, MandatoryValidation.YouHaveNotEnteredMessage("value"));

			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7B;
			Header.US_OA_ResponsibleGovernmentOfficial = ZGuid.Empty;
			AssertNoMessageErrorContaining(Header.US_OfficialCertificationDateInfo, MandatoryValidation.YouHaveNotEnteredMessage("value"));
		}

		public void TestCheckUS_ElectronicImageSubmitted()
		{
			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7B;
			Header.US_ElectronicImageSubmitted = false;
			AssertNoMessageErrorContaining(Header.US_ElectronicImageSubmittedInfo, "Please make sure inspection document has been submitted.");

			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7A1;
			Header.US_ElectronicImageSubmitted = false;
			AssertHasMessageErrorContaining(Header.US_ElectronicImageSubmittedInfo, "Please make sure inspection document has been submitted.");
		}

		public void TestCheckUS_SourceCountry()
		{
			Setup();
			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7A2;
			var countries7A2 = Header.AddInfoLookups.USCountries;
			countries7A2.Load();
			Header.US_SourceCountry = Core.Constants.CountryCodes.France;
			AssertHasMessageErrorContaining(Header.US_SourceCountryInfo, ListValidation.InvalidCodeMessageError);
			Header.US_SourceCountry = Core.Constants.CountryCodes.Australia;
			AssertNoMessageErrorContaining(Header.US_SourceCountryInfo, ListValidation.InvalidCodeMessageError);

			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7A4;
			var countries7A4 = Header.AddInfoLookups.USCountries;
			countries7A4.Load();
			Header.US_SourceCountry = Core.Constants.CountryCodes.China;
			AssertHasMessageErrorContaining(Header.US_SourceCountryInfo, ListValidation.InvalidCodeMessageError);
			Header.US_SourceCountry = Core.Constants.CountryCodes.KoreaSouth;
			AssertNoMessageErrorContaining(Header.US_SourceCountryInfo, ListValidation.InvalidCodeMessageError);

			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7B;
			var countries7B = Header.AddInfoLookups.USCountries;
			countries7B.Load();
			Header.US_SourceCountry = Core.Constants.CountryCodes.Pakistan;
			AssertHasMessageErrorContaining(Header.US_SourceCountryInfo, ListValidation.InvalidCodeMessageError);
			Header.US_SourceCountry = Core.Constants.CountryCodes.Uruguay;
			AssertNoMessageErrorContaining(Header.US_SourceCountryInfo, ListValidation.InvalidCodeMessageError);

			Header.US_DeclarationCode = ConformanceDeclarationCodeList.Codes._7A1;
			var countries7A1 = Header.AddInfoLookups.USCountries;
			countries7A1.Load();

			Header.US_SourceCountry = Core.Constants.CountryCodes.China;
			AssertNoMessageErrorContaining(Header.US_SourceCountryInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(Header.US_SourceCountryInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_SourceCountry = ZString.Empty;
			AssertHasMessageErrorContaining(Header.US_SourceCountryInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_OA_Exporter()
		{
			var org = Factory.New<OrgHeader>();
			var address1 = org.Addresses.AddNew();
			Header.US_OA_Exporter = address1.PK;
			AssertNoMessageErrorContaining(Header.US_OA_ExporterInfo, MandatoryValidation.YouHaveNotEntered);
			Header.US_OA_Exporter = ZGuid.Empty;
			AssertHasMessageErrorContaining(Header.US_OA_ExporterInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_NetWeightAndUQ()
		{
			Header.US_NetWeight = 100m;
			AssertNoMessageErrorContaining(Header.US_NetWeightInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Header.US_NetWeightInfo, USOMCAddInfoValidation.EnterNumberGreaterThanZero);

			Header.US_NetWeight = -1m;
			AssertHasMessageErrorContaining(Header.US_NetWeightInfo, USOMCAddInfoValidation.EnterNumberGreaterThanZero);

			Header.US_NetWeight = 0m;
			AssertHasMessageErrorContaining(Header.US_NetWeightInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Header.US_NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_NetWeight = 10m;
			Header.US_NetWeightUQ = "";
			AssertHasMessageErrorContaining(Header.US_NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_NetWeightUQ = "KG";
			AssertNoMessageErrorContaining(Header.US_NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_NetWeightUQ = "~";
			AssertHasMessageErrorContaining(Header.US_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);

			Header.US_NetWeightUQ = "KG";
			AssertNoMessageErrorContaining(Header.US_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);
		}

		OMCHeader Header
		{
			get
			{
				if (header == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableCRL = true;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					header = invoiceLine.OMCHeaders.AddNew();
				}
				return header;
			}
		}
		OMCHeader header;

		void Setup()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ConformanceDeclarationCodeList.Codes._7A2, ConformanceDeclarationCodeList.Codes._7A2);
			helper.CreateNewOrGetExistingCusCodeType(ConformanceDeclarationCodeList.Codes._7A4, ConformanceDeclarationCodeList.Codes._7A4);
			helper.CreateNewOrGetExistingCusCodeType(ConformanceDeclarationCodeList.Codes._7B, ConformanceDeclarationCodeList.Codes._7B);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, ConformanceDeclarationCodeList.Codes._7A2, Core.Constants.CountryCodes.Australia, "Australia", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, ConformanceDeclarationCodeList.Codes._7A4, Core.Constants.CountryCodes.KoreaSouth, "Korea, South", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, ConformanceDeclarationCodeList.Codes._7B, Core.Constants.CountryCodes.Uruguay, "Uruguay", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}
	}
}
