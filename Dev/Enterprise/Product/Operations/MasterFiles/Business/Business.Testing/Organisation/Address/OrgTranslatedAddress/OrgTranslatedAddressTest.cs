using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgTranslatedAddress))]
	sealed class OrgTranslatedAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDeleteAdditionalInfoMappings()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = header.OH_Code;
			address.OA_OH = header.PK;

			var addressInfo = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			addressInfo.OAI_AdditionalInfo = "Test Additional Info 1";
			addressInfo.OAI_OA_Address = address.PK;

			var translatedAddress = Factory.NewWithValidTestData<OrgTranslatedAddress>();
			translatedAddress.OTA_Address1 = "Translated Address";
			translatedAddress.OTA_OA = address.PK;
			translatedAddress.OTA_Language = SharedConstants.Languages.French;

			var translatedAddress2 = Factory.NewWithValidTestData<OrgTranslatedAddress>();
			translatedAddress2.OTA_Address1 = "Translated Address 2";
			translatedAddress2.OTA_OA = address.PK;
			translatedAddress2.OTA_Language = SharedConstants.Languages.ChineseSimplified;

			Factory.Save();

			AssertEquals(1, translatedAddress.AdditionalInfoWrapperCollection.Count);
			AssertEquals(1, translatedAddress2.AdditionalInfoWrapperCollection.Count);

			var wrapper = translatedAddress.AdditionalInfoWrapperCollection[0];

			AssertEquals(translatedAddress, wrapper.TranslatedAddress);

			translatedAddress.Delete();

			Assert(wrapper.IsDeleted);
		}

		public void TestUpdateWrapperLanguage()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();

			var translatedAddress = address.AddNewTranslatedAddress();
			translatedAddress.OTA_Address1 = "Test Translate Address";
			translatedAddress.Language = SharedConstants.Languages.ChineseSimplified;
			translatedAddress.OTA_AdditionalAddressInformation = "A";

			var additionalInfo1 = address.AdditionalInfos.AddNew();
			additionalInfo1.OAI_AdditionalInfo = "Test Info 1";
			additionalInfo1.OAI_IsPrimary = true;
			var additionalInfo2 = address.AdditionalInfos.AddNew();
			additionalInfo2.OAI_AdditionalInfo = "Test Info 2";
			additionalInfo2.OAI_IsPrimary = false;

			Factory.Save();

			var wrapper1 = (OrgTranslatedAddressAdditionalInfoWrapper)translatedAddress.AdditionalInfoWrapperCollection.FirstOrDefault(u => ((OrgTranslatedAddressAdditionalInfoWrapper)u).AddressAdditionalInfo.Equals(additionalInfo1));
			var wrapper2 = (OrgTranslatedAddressAdditionalInfoWrapper)translatedAddress.AdditionalInfoWrapperCollection.FirstOrDefault(u => ((OrgTranslatedAddressAdditionalInfoWrapper)u).AddressAdditionalInfo.Equals(additionalInfo2));
			wrapper1.TranslatedAdditionalInfo = "Test Info 1";
			wrapper2.TranslatedAdditionalInfo = "Test Info 2";

			var translatedAdditionalInfo1 = additionalInfo1.TranslatedInfos.FirstOrDefault(u => u.OTI_Language == SharedConstants.Languages.ChineseSimplified);
			var translatedAdditionalInfo2 = additionalInfo2.TranslatedInfos.FirstOrDefault(u => u.OTI_Language == SharedConstants.Languages.ChineseSimplified);

			AssertEquals(SharedConstants.Languages.ChineseSimplified, translatedAdditionalInfo1.OTI_Language);
			AssertEquals(SharedConstants.Languages.ChineseSimplified, translatedAdditionalInfo2.OTI_Language);

			translatedAddress.Language = SharedConstants.Languages.French;

			AssertEquals(SharedConstants.Languages.French, translatedAdditionalInfo1.OTI_Language);
			AssertEquals(SharedConstants.Languages.ChineseSimplified, translatedAdditionalInfo2.OTI_Language);
		}

		public void TestDisplayText()
		{
			translatedAddress1.Language = Core.SharedConstants.Languages.EnglishAmerican;
			AssertEquals("TRANSLATED:EN-US", translatedAddress1.DisplayText);
			translatedAddress1.Language = Core.SharedConstants.Languages.ChineseSimplified;
			AssertEquals("TRANSLATED:ZH-CN", translatedAddress1.DisplayText);

			translatedAddress1.DisplayText = "TRANSLATED:FR-FR";
			AssertEquals(Core.SharedConstants.Languages.French, translatedAddress1.Language);
			translatedAddress1.DisplayText = "TRANSLATED:EN-US";
			AssertEquals(Core.SharedConstants.Languages.EnglishAmerican, translatedAddress1.Language);
		}

		public void TestIsEnglish()
		{
			translatedAddress1.Language = Constants.Languages.EnglishAmerican;
			translatedAddress1.Address1 = "Hydrogen";
			translatedAddress1.Address2 = "Oxygen";
			translatedAddress1.City = "Mercury";

			Assert(translatedAddress1.IsEnglish);
			Assert(translatedAddress1.IsEnglishOnlyOrEmpty);

			translatedAddress1.Address1 = "澳村";
			Assert(translatedAddress1.IsEnglish);
			Assert(!translatedAddress1.IsEnglishOnlyOrEmpty);

			translatedAddress1.Language = Constants.Languages.ChineseSimplified;
			Assert(!translatedAddress1.IsEnglish);
		}

		public void TestSetOTA_AdditionalInfoCase_1()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test";
			address.OA_AdditionalAddressInformation = string.Empty;

			var translatedAddress = Factory.NewWithValidTestData<OrgTranslatedAddress>();
			translatedAddress.OTA_Language = "EN";
			translatedAddress.OTA_AdditionalAddressInformation = "Translated";
			translatedAddress.OTA_OA = address.PK;

			var additionalInfo = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo.OAI_AdditionalInfo = "B";
			additionalInfo.OAI_IsPrimary = true;
			additionalInfo.OAI_OA_Address = address.PK;

			var translatedAddressAdditionalInfo = Factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translatedAddressAdditionalInfo.OTI_Language = "EN";
			translatedAddressAdditionalInfo.OTI_AdditionalInfo = "Translated";
			translatedAddressAdditionalInfo.OTI_OAI = additionalInfo.PK;
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals("EN", address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("Translated", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals("EN", address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("Translated", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});

			translatedAddress.OTA_AdditionalAddressInformation = "12345";
			CombineAssertions(() =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals("EN", address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("12345", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals("EN", address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("12345", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});

			translatedAddress.OTA_AdditionalAddressInformation = string.Empty;
			CombineAssertions(() =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals("EN", address.TranslatedAddresses[0].OTA_Language);
				AssertEquals(string.Empty, address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(false, address.AdditionalInfos[0].TranslatedInfos.Any());
			});
		}

		public void TestSetOTA_AdditionalInfoCase_2()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test";
			address.OA_AdditionalAddressInformation = string.Empty;

			var translatedAddress = Factory.NewWithValidTestData<OrgTranslatedAddress>();
			translatedAddress.OTA_Language = "EN";
			translatedAddress.OTA_AdditionalAddressInformation = "Translated";
			translatedAddress.OTA_OA = address.PK;

			var additionalInfo = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo.OAI_AdditionalInfo = "B";
			additionalInfo.OAI_IsPrimary = true;
			additionalInfo.OAI_OA_Address = address.PK;

			var translatedAddressAdditionalInfo = Factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translatedAddressAdditionalInfo.OTI_Language = "EN-US";
			translatedAddressAdditionalInfo.OTI_AdditionalInfo = "Translated";
			translatedAddressAdditionalInfo.OTI_OAI = additionalInfo.PK;
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals("EN", address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("Translated", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals("EN-US", address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("Translated", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});

			translatedAddress.OTA_AdditionalAddressInformation = "12345";
			CombineAssertions(() =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals("EN", address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("12345", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(2, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].TranslatedInfos.Any(t => t.OTI_Language == "EN-US" && t.OTI_AdditionalInfo == "Translated"));
				AssertEquals(true, address.AdditionalInfos[0].TranslatedInfos.Any(t => t.OTI_Language == "EN" && t.OTI_AdditionalInfo == "12345"));
			});
		}

		public void TestSetOTA_Language()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test";
			address.OA_AdditionalAddressInformation = string.Empty;

			var translatedAddress = Factory.NewWithValidTestData<OrgTranslatedAddress>();
			translatedAddress.OTA_Language = "EN";
			translatedAddress.OTA_AdditionalAddressInformation = "Translated";
			translatedAddress.OTA_OA = address.PK;

			var additionalInfo = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo.OAI_AdditionalInfo = "B";
			additionalInfo.OAI_IsPrimary = true;
			additionalInfo.OAI_OA_Address = address.PK;

			var translatedAddressAdditionalInfo = Factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translatedAddressAdditionalInfo.OTI_Language = "EN";
			translatedAddressAdditionalInfo.OTI_AdditionalInfo = "Translated";
			translatedAddressAdditionalInfo.OTI_OAI = additionalInfo.PK;
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals("EN", address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("Translated", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals("EN", address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("Translated", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});

			translatedAddress.OTA_Language = "EN-US";
			CombineAssertions(() =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals("EN-US", address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("Translated", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals("EN-US", address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("Translated", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});
		}

		public void TestParentAddressLanguagePackWhenConcurrentDelete()
		{
			var translatedAddress = address.AddNewTranslatedAddress();
			translatedAddress.FillWithValidTestData();
			Factory.Save();

			AssertEquals(1, address.TranslatedAddresses.Count);
			AssertEquals(2, address.AddressLanguagePack.Count);

			var factoryToDelete = new BusinessObjectFactory();
			factoryToDelete.RefreshEnabled = false;
			var translatedAddressToDelete = factoryToDelete.Load<OrgTranslatedAddress>(translatedAddress.PK);
			translatedAddressToDelete.Delete();
			factoryToDelete.Save();

			translatedAddress.Address1 = "addr1";
			AssertExceptionThrown<ZSaveConcurrencyException>(() => Factory.Save());

			AssertEquals(0, address.TranslatedAddresses.Count);
			AssertEquals(1, address.AddressLanguagePack.Count);
		}

		#region ISupportWebAddressValidation

		public void TestValidationStatusIsNYVAfterReActivate()
		{
			var address = Factory.New<OrgAddress>();
			translatedAddress1 = address.AddNewTranslatedAddress();
			translatedAddress1.OTA_ValidationStatus = AddressValidationStatus.ManuallyVerified;
			AssertNotEquals(AddressValidationStatus.ToBeVerified, translatedAddress1.OTA_ValidationStatus);

			address.OA_IsActive = false;
			address.OA_IsActive = true;

			AssertEquals(AddressValidationStatus.ToBeVerified, translatedAddress1.OTA_ValidationStatus);
		}

		public void TestValidationStatus_WhenChangingAddressFieldWhileValueIsCna_ShouldKeepItAsCna()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			var address = Factory.NewWithValidTestData<OrgTranslatedAddress>();
			address.OTA_ValidationStatus = AddressValidationStatus.CountryNotAvailable;

			// Act & Assert.

			address.OTA_Address1 = "[_MOCK_ADDRESS_1_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, address.OTA_ValidationStatus);

			address.OTA_Address2 = "[_MOCK_ADDRESS_2_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, address.OTA_ValidationStatus);

			address.OTA_City = "[_MOCK_CITY_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, address.OTA_ValidationStatus);

			address.OTA_PostCode = "0000";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, address.OTA_ValidationStatus);

			address.OTA_State = "[_MOCK_STATE_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, address.OTA_ValidationStatus);
		}

		public void TestChangingAddressResetsValidationStatus()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			translatedAddress1.Address1 = "A1";
			translatedAddress1.Address2 = "A2";
			translatedAddress1.Postcode = "1234";
			translatedAddress1.City = "Syd";
			translatedAddress1.State = "NSW";
			translatedAddress1.ParentAddress.OA_RN_NKCountryCode = "AU";
			translatedAddress1.ValidationStatus = AddressValidationStatus.Verified;

			AssertValidationStatusIsReset(translatedAddress1, () => translatedAddress1.Address1 += "A");
			AssertValidationStatusIsReset(translatedAddress1, () => translatedAddress1.Address2 += "A");
			AssertValidationStatusIsReset(translatedAddress1, () => translatedAddress1.City += "A");
			AssertValidationStatusIsReset(translatedAddress1, () => translatedAddress1.Postcode += "A");
		}

		void AssertValidationStatusIsReset(ISupportWebAddressValidation address, Action action)
		{
			address.ValidationStatus = AddressValidationStatus.Verified;
			action.Invoke();
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);
		}

		public void TestRaiseAddressValidationStatusChanged()
		{
			translatedAddress1.OTA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			translatedAddress1.Address2 = "";

			translatedAddress1.AddressValidationStatusChanged += address_AddressValidationStatusChanged;
			translatedAddress1.OTA_ValidationStatus = AddressValidationStatus.Verified;
			AssertEquals("It happened", translatedAddress1.Address2);
		}

		void address_AddressValidationStatusChanged(object sender, EventArgs e)
		{
			((OrgTranslatedAddress)sender).Address2 = "It happened";
		}

		public void TestValidationStatus()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery());
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = country.Code;
			var translatedAddress = address.AddNewTranslatedAddress();
			translatedAddress.OTA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			AssertEquals(AddressValidationStatus.ToBeVerified, translatedAddress.ValidationStatus);
		}

		public void TestState_CountryHasRefData_StateMatchesRefData()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = country.Code;

			var translatedAddress = address.AddNewTranslatedAddress();

			address.OA_RN_NKCountryCode = country.Code;
			translatedAddress.OTA_State = "NSW";
			AssertEquals("New South Wales", translatedAddress.State);

			translatedAddress.State = "Victoria";
			AssertEquals("VIC", translatedAddress.OTA_State);
		}

		public void TestState_CountryHasRefData_StateDoesNotMatchRefData()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = country.Code;

			var translatedAddress = address.AddNewTranslatedAddress();

			address.OA_RN_NKCountryCode = country.Code;
			translatedAddress.OTA_State = "XYZ";
			AssertEquals("XYZ", translatedAddress.State);
		}

		public void TestState_CountryHasNoRefData()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "NL"));
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = country.Code;

			var translatedAddress = address.AddNewTranslatedAddress();

			address.OA_RN_NKCountryCode = country.Code;
			translatedAddress.OTA_State = "XYZ";
			AssertEquals("XYZ", translatedAddress.State);
		}

		public void TestNeedValidation()
		{
			var factory = new BusinessObjectFactory();

			var australia = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			australia.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = factory.NewWithValidTestData<OrgAddress>();
			var translatedAddress = address.AddNewTranslatedAddress();
			translatedAddress.Address1 = "A1";
			translatedAddress.Address2 = "A2";
			translatedAddress.Postcode = "1234";
			translatedAddress.City = "Syd";
			translatedAddress.State = "NSW";
			Assert(!translatedAddress.NeedValidation);

			address.OA_RN_NKCountryCode = "AU";
			Assert(translatedAddress.NeedValidation);

			factory.Save();
			Assert(translatedAddress.IsInDatabase);
			Assert(!translatedAddress.NeedValidation);

			translatedAddress.Address1 += "A";
			Assert(translatedAddress.NeedValidation);

			translatedAddress.Address2 = "";
			Assert(translatedAddress.NeedValidation);

			translatedAddress.Address1 = "";
			Assert(!translatedAddress.NeedValidation);
		}

		public void TestResetAddressMap()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			var translatedAddress = GetNewTranslatedAddress();
			translatedAddress.OTA_Address1 = "A1";
			translatedAddress.OTA_Address2 = "A2";
			translatedAddress.OTA_City = "C1";
			translatedAddress.OTA_PostCode = "P1";
			translatedAddress.ParentAddress.OA_RN_NKCountryCode = "AU";
			AssertAddressMap(translatedAddress, translatedAddress.OTA_Address1Info);
			AssertAddressMap(translatedAddress, translatedAddress.OTA_Address2Info);
			AssertAddressMap(translatedAddress, translatedAddress.OTA_CityInfo);
			AssertAddressMap(translatedAddress, translatedAddress.OTA_PostCodeInfo);
		}

		void AssertAddressMap(OrgTranslatedAddress address, ZPropertyInfo propertyInfo)
		{
			address.AddressMap = "ABCDE";
			propertyInfo.Value = (ZString)((ZString)propertyInfo.Value + "1");
			Assert(string.IsNullOrEmpty(address.AddressMap));
		}

		public void TestValidationSection()
		{
			AssertEquals(AddressValidationSection.OrganizationAddress, Factory.New<OrgTranslatedAddress>().ValidationSection);
		}

		public void TestAdditionalAddressInfoList()
		{
			var translatedAddress = Factory.NewWithValidTestData<OrgTranslatedAddress>();
			Factory.Save();

			AssertEquals(new CodeDescriptionPairList(), address.AdditionalAddressInfoList);
		}

		#endregion

		#region OTA_ValidationStatus

		public void TestValidationStatus_WhenSetToManuallyVerifiedFromOtherValue_ShouldStayAsIsUntilTranslatedAddressIsReloaded()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			var translatedAddress = Factory.NewWithValidTestData<OrgTranslatedAddress>();

			translatedAddress.OTA_Address1 = "42 FOOBAR STREET";
			translatedAddress.OTA_Address2 = "FUNPLACE";
			translatedAddress.OTA_PostCode = "0000";
			translatedAddress.OTA_City = "WHITERUN";
			translatedAddress.OTA_State = "TAMRIEL";

			// Act.

			translatedAddress.OTA_ValidationStatus = AddressValidationStatus.ManuallyVerified;

			translatedAddress.OTA_Address1 = "72 O'RIORDAN STREET";
			translatedAddress.OTA_Address2 = "WISETECH GLOBAL";
			translatedAddress.OTA_PostCode = "2015";
			translatedAddress.OTA_City = "ALEXANDRIA";
			translatedAddress.OTA_State = "NSW";

			// Assert.

			AssertEquals(AddressValidationStatus.ManuallyVerified, translatedAddress.OTA_ValidationStatus);
		}

		public void TestValidationStatus_WhenLoadedAsManuallyVerifiedFromDatabase_ShouldResetValueAfterChangingAddressField()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			address.OA_RN_NKCountryCode = "AU";

			var translatedAddress = Factory.NewWithValidTestData<OrgTranslatedAddress>();
			translatedAddress.OTA_OA = address.PK;

			translatedAddress.OTA_Address1 = "42 FOOBAR STREET";
			translatedAddress.OTA_Address2 = "FUNPLACE";
			translatedAddress.OTA_PostCode = "0000";
			translatedAddress.OTA_City = "WHITERUN";
			translatedAddress.OTA_State = "TAMRIEL";

			translatedAddress.OTA_ValidationStatus = AddressValidationStatus.ManuallyVerified;
			Factory.Save();

			var reloadedTranslatedAddress = new BusinessObjectFactory().Load<OrgTranslatedAddress>(translatedAddress.PK);

			// Act.

			reloadedTranslatedAddress.OTA_Address1 = "72 O'RIORDAN STREET";
			reloadedTranslatedAddress.OTA_Address2 = "WISETECH GLOBAL";
			reloadedTranslatedAddress.OTA_PostCode = "2015";
			reloadedTranslatedAddress.OTA_City = "ALEXANDRIA";
			reloadedTranslatedAddress.OTA_State = "NSW";

			// Assert.

			AssertEquals(AddressValidationStatus.ToBeVerified, reloadedTranslatedAddress.OTA_ValidationStatus);
		}

		#endregion

		#region TestAddressDescription

		public void TestAddressDescription()
		{
			RefUNLOCO.Loader uNLOCOLoader = new RefUNLOCO.Loader(Factory);
			var aUSYD = uNLOCOLoader.Load("AUSYD");

			var org = Factory.New<OrgHeader>();
			var translatedAddress = org.MainAddress.TranslatedAddresses.AddNew();
			AssertEquals("expected AddressDescription", string.Empty, translatedAddress.AddressDescription);

			org.OH_RL_NKClosestPort = aUSYD.RL_Code;
			AssertEquals("expected AddressDescription", "AUSYD", translatedAddress.AddressDescription);

			translatedAddress.OTA_State = "NSW";
			AssertEquals("expected AddressDescription", "NSW AUSYD", translatedAddress.AddressDescription);

			translatedAddress.OTA_City = "Sydney";
			AssertEquals("expected AddressDescription", "Sydney NSW AUSYD", translatedAddress.AddressDescription);

			translatedAddress.OTA_Address1 = "O'Riodan ST";
			AssertEquals("expected AddressDescription", "Sydney NSW AUSYD O'Riodan ST", translatedAddress.AddressDescription);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewTranslatedAddress();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewTranslatedAddress();
		}

		OrgTranslatedAddress GetNewTranslatedAddress()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var translatedAddress = address.AddNewTranslatedAddress();
			translatedAddress.FillWithValidTestData();
			return translatedAddress;
		}

		OrgAddress address;
		OrgTranslatedAddress translatedAddress1;

		protected override void SetUp()
		{
			base.SetUp();
			address = Factory.NewWithValidTestData<OrgAddress>();
			translatedAddress1 = GetNewTranslatedAddress();
		}

		#endregion
	}
}
