using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesRegistry;

namespace Enterprise.MasterFiles.DataTransfer.OrgMatching.Testing
{
	class OrganisationMatcherTest : TestCaseWithFactory
	{
		public void TestAddressMatchingWithMatchingCodeAndAddressShortCode()
		{
			var testData = new TestOrganisationCreator(false);
			var pADAddress1 = testData.Organization.Addresses.AddNew(OrgAddressType.PickupAndDelivery, true);
			pADAddress1.OA_Address1 = "PAD";
			pADAddress1.OA_Address1 = "PAD1";
			pADAddress1.OA_Code = "PAD1";
			var pADAddress2 = testData.Organization.Addresses.AddNew(OrgAddressType.PickupAndDelivery, false);
			pADAddress2.OA_Address1 = "PAD";
			pADAddress2.OA_Address1 = "PAD2";
			pADAddress2.OA_Code = "PAD2";
			testData.Organization.Factory.Save();

			var matcherFactory = new BusinessObjectFactory();
			var orgMatchingData = new OrgHeaderForMatching(matcherFactory);

			orgMatchingData.OH_FullName = "FREDA CARLOS PICTOGRAPHICS";
			orgMatchingData.OH_RL_NKClosestPort = "AUSYD";
			orgMatchingData.OH_Code = "FRECARSYD";

			var matcher = new OrganisationMatcher(matcherFactory, IfUnmatched.TakeBehaviourFromOverallSetting);

			var addressData = new OrgAddressForMatching();
			orgMatchingData.Addresses.Add(addressData);
			addressData.OA_Code = "PAD1";
			addressData.OA_Address1 = "PAD";
			addressData.OA_Address1 = "PAD1";

			var matchedAddress1 = matcher.GetMatchingAddress(orgMatchingData, "PAD1", false);

			orgMatchingData.Addresses.Clear();
			var testAddressData2 = new OrgAddressForMatching();
			orgMatchingData.Addresses.Add(testAddressData2);
			testAddressData2.OA_Code = "PAD2";
			testAddressData2.OA_Address1 = "PAD";
			testAddressData2.OA_Address1 = "PAD2";

			var matchedAddress2 = matcher.GetMatchingAddress(orgMatchingData, "PAD2", false);
			CombineAssertions(() =>
			{
				AssertNotNull("matchedAddress", matchedAddress1);
				AssertEquals("matchedAddress.OA_Address1", pADAddress1.OA_Address1, matchedAddress1.OA_Address1);
				AssertEquals("matchedAddress.OA_Address2", pADAddress1.OA_Address2, matchedAddress1.OA_Address2);
				AssertEquals("matchedAddress.PK", pADAddress1.PK, matchedAddress1.PK);

				AssertNotNull("matchedAddress", matchedAddress2);
				AssertEquals("matchedAddress.OA_Address1", pADAddress2.OA_Address1, matchedAddress2.OA_Address1);
				AssertEquals("matchedAddress.OA_Address2", pADAddress2.OA_Address2, matchedAddress2.OA_Address2);
				AssertEquals("matchedAddress.PK", pADAddress2.PK, matchedAddress2.PK);
			});
		}

		public void TestAddressMatchingWithMatchingCodeGetsRightAddress()
		{
			var testData = new TestOrganisationCreator(true);

			var matcherFactory = new BusinessObjectFactory();
			var orgMatchingData = new OrgHeaderForMatching(matcherFactory);

			orgMatchingData.OH_FullName = "FREDA CARLOS PICTOGRAPHICS";
			orgMatchingData.OH_RL_NKClosestPort = "AUSYD";
			orgMatchingData.OH_Code = "FRECARSYD";

			var addressData = new OrgAddressForMatching();
			orgMatchingData.Addresses.Add(addressData);
			addressData.OA_Address1 = "212 FREDDO FROG DRIVE";
			addressData.OA_City = "CADBURY";
			addressData.OA_State = "NSW";
			addressData.OA_PostCode = "2434";

			var matcher = new OrganisationMatcher(matcherFactory, IfUnmatched.TakeBehaviourFromOverallSetting);
			var matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);
			AssertNotNull("matchedAddress", matchedAddress);
			AssertEquals("matchedAddress.OA_Address1", testData.Address2.OA_Address1, matchedAddress.OA_Address1);
			AssertEquals("matchedAddress.PK", testData.Address2.PK, matchedAddress.PK);
		}

		public void TestAddressMatchingWithMatchingCodeFallsBackToMainAddress()
		{
			var testData = new TestOrganisationCreator(true);

			var matcherFactory = new BusinessObjectFactory();
			var orgMatchingData = new OrgHeaderForMatching(matcherFactory);

			orgMatchingData.OH_FullName = "FREDA CARLOS PICTOGRAPHICS";
			orgMatchingData.OH_RL_NKClosestPort = "AUSYD";
			orgMatchingData.OH_Code = "FRECARSYD";

			var addressData = new OrgAddressForMatching();
			orgMatchingData.Addresses.Add(addressData);
			addressData.OA_Address1 = "565 CHICKEN FILLET PLACE";
			addressData.OA_City = "KENTUCKY";
			addressData.OA_State = "VIC";
			addressData.OA_PostCode = "2934";

			var matcher = new OrganisationMatcher(matcherFactory, IfUnmatched.TakeBehaviourFromOverallSetting);
			var matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);
			AssertNotNull("matchedAddress", matchedAddress);
			AssertEquals("matchedAddress.OA_Address1", testData.Address1.OA_Address1, matchedAddress.OA_Address1);
			AssertEquals("matchedAddress.PK", testData.Address1.PK, matchedAddress.PK);
		}

		public void TestAddressMatchingWithoutMatchingCodeGetsRightAddress()
		{
			var testData = new TestOrganisationCreator(true);

			var matcherFactory = new BusinessObjectFactory();
			var orgMatchingData = new OrgHeaderForMatching(matcherFactory);

			orgMatchingData.OH_FullName = "FREDA CARLOS PICTOGRAPHICS";
			orgMatchingData.OH_RL_NKClosestPort = "AUSYD";
			orgMatchingData.OH_Code = "FRISBEERA!!";

			var addressData = new OrgAddressForMatching();
			orgMatchingData.Addresses.Add(addressData);
			addressData.OA_Address1 = "212 FREDDO FROG DRIVE";
			addressData.OA_City = "CADBURY";
			addressData.OA_State = "NSW";
			addressData.OA_PostCode = "2434";

			var matcher = new OrganisationMatcher(matcherFactory, IfUnmatched.TakeBehaviourFromOverallSetting);
			var matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);
			AssertNotNull("matchedAddress", matchedAddress);
			AssertEquals("matchedAddress.OA_Address1", testData.Address2.OA_Address1, matchedAddress.OA_Address1);
			AssertEquals("matchedAddress.PK", testData.Address2.PK, matchedAddress.PK);
		}

		public void TestAddressMatchingWithNoMatchFallsBackToDefaultAddressIfRegistryItemHasThisEnabled()
		{
			var unmatchedOrgPK = OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(true);

			var matcherFactory = new BusinessObjectFactory();
			var orgMatchingData = new OrgHeaderForMatching(matcherFactory);

			orgMatchingData.OH_FullName = "SOME OTHER BUGGER";
			orgMatchingData.OH_RL_NKClosestPort = "ZADSD";
			orgMatchingData.OH_Code = "UNKNOWN!!";

			var addressData = new OrgAddressForMatching();
			orgMatchingData.Addresses.Add(addressData);
			addressData.OA_Address1 = "000888 UNKNOWN UNKNOWN UNKNOWN";
			addressData.OA_City = "UNKNOWN";
			addressData.OA_State = "UNKNOWN";
			addressData.OA_PostCode = "000888";

			var matcher = new OrganisationMatcher(matcherFactory, IfUnmatched.TakeBehaviourFromOverallSetting);
			var matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);
			AssertNotNull("matchedAddress", matchedAddress);
			AssertEquals("matchedAddress.Header.PK", unmatchedOrgPK, matchedAddress.Header.PK);
		}

		public void TestAddressMatchingShouldNotUseUnmatched()
		{
			OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(true);
			var matcherFactory = new BusinessObjectFactory();
			var orgMatchingData = new OrgHeaderForMatching(matcherFactory);

			var organisationMatcher = new OrganisationMatcher(matcherFactory, IfUnmatched.ReturnNull);
			AssertNull(organisationMatcher.GetMatchingAddress(orgMatchingData, null, false));
		}

		public void TestAddressMatchingWithNoMatchAndNoDefaultFallsBackToNull()
		{
			OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(false);

			var matcherFactory = new BusinessObjectFactory();
			var orgMatchingData = new OrgHeaderForMatching(matcherFactory);

			orgMatchingData.OH_FullName = "SOME OTHER BUGGER";
			orgMatchingData.OH_RL_NKClosestPort = "ZADSD";
			orgMatchingData.OH_Code = "UNKNOWN!!";

			var addressData = new OrgAddressForMatching();
			orgMatchingData.Addresses.Add(addressData);
			addressData.OA_Address1 = "000888 UNKNOWN UNKNOWN UNKNOWN";
			addressData.OA_City = "UNKNOWN";
			addressData.OA_State = "UNKNOWN";
			addressData.OA_PostCode = "000888";

			var matcher = new OrganisationMatcher(matcherFactory, IfUnmatched.TakeBehaviourFromOverallSetting);
			var matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);
			AssertNull("matchedAddress", matchedAddress);
		}

		[ExpectNoExceptions]
		public void TestMatch_MatchByCode()
		{
			localCodeMatcher.Setup(l => l.Match(It.IsAny<ZString>(), It.IsAny<IOrgHeaderForMatching>())).Returns(orgHeader);
			matcher.GetMatchingOrganization(org);

			similarityMatcher.Verify(s => s.GetMatchingOrganisation(It.IsAny<IOrgHeaderForMatching>(), It.IsAny<bool>(), It.IsAny<ISimpleLogger>()), Times.Never);
			defaultValueMatcher.Verify(d => d.Match(), Times.Never);
			mocks.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestMatch_MatchBySimilarity()
		{
			localCodeMatcher.Setup(l => l.Match(It.IsAny<ZString>(), It.IsAny<IOrgHeaderForMatching>())).Returns((OrgHeader)null);
			similarityMatcher.Setup(s => s.GetMatchingOrganisation(It.IsAny<IOrgHeaderForMatching>(), It.IsAny<bool>(), It.IsAny<ISimpleLogger>())).Returns(orgHeader);

			matcher.GetMatchingOrganization(org);

			similarityMatcher.Verify(s => s.GetMatchingOrganisation(It.IsAny<IOrgHeaderForMatching>(), It.IsAny<bool>(), It.IsAny<ISimpleLogger>()), Times.Once);
			defaultValueMatcher.Verify(d => d.Match(), Times.Never);
			mocks.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestMatch_MatchByDefaultValue()
		{
			localCodeMatcher.Setup(l => l.Match(It.IsAny<ZString>(), It.IsAny<IOrgHeaderForMatching>())).Returns((OrgHeader)null);
			similarityMatcher.Setup(s => s.GetMatchingOrganisation(It.IsAny<IOrgHeaderForMatching>(), It.IsAny<bool>(), It.IsAny<ISimpleLogger>())).Returns((OrgHeader)null);
			defaultValueMatcher.Setup(d => d.Match()).Returns(orgHeader);

			matcher.GetMatchingOrganization(org);

			defaultValueMatcher.Verify(d => d.Match(), Times.Once);
			mocks.VerifyAll();
		}

		public void TestAddressMatchingWithRegistration()
		{
			var factory = new BusinessObjectFactory();
			var org1 = factory.New<OrgHeader>();
			org1.OH_Code = "AB3";
			org1.MainAddress.OA_Address1 = "ADD 1";
			var org2 = factory.New<OrgHeader>();
			org2.OH_Code = "AB2";
			org2.MainAddress.OA_Address1 = "ADD 1";
			var org3 = factory.New<OrgHeader>();
			org3.OH_Code = "AB1";
			org3.MainAddress.OA_Address1 = "ADD 1";
			var org4 = factory.New<OrgHeader>();
			org4.OH_Code = "AB4";
			org4.MainAddress.OA_Address1 = "ADD 1";
			var code1 = org1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "CODE1", Core.Constants.CountryCodes.UnitedStates);
			var code2 = org2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "CODE1", Core.Constants.CountryCodes.UnitedStates);
			var code3 = org3.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "CODE3", Core.Constants.CountryCodes.Afghanistan);
			var code4 = org4.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "CODE2", Core.Constants.CountryCodes.UnitedStates);
			var orgMatchingData = new OrgHeaderForMatching(factory);
			var orgCusCodeMatch = new OrgCusCodeForMatching();
			orgMatchingData.CustomsCodes.Add(orgCusCodeMatch);
			orgCusCodeMatch.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			orgCusCodeMatch.OK_CustomsRegNo = "CODE1";
			orgCusCodeMatch.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			var logger = new TestErrorLogger();
			var matcher = new OrganisationMatcher(factory, IfUnmatched.TakeBehaviourFromOverallSetting, logger);
			var matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);
			AssertNull("Duplicate address match for CODE1", matchedAddress);
			var expectedLog = OrganisationMatcherLoggingTestHelper.GetLogWhenMultipleOrgsMatchRegistrationDetails(code1);
			AssertContains(expectedLog, logger.Logs);
			logger.ClearLogs();
			var matchedOrganization = matcher.GetMatchingOrganization(orgMatchingData);
			AssertNull("Duplicate organization match for CODE1", matchedOrganization);
			AssertContains(expectedLog, logger.Logs);

			logger.ClearLogs();
			orgCusCodeMatch.OK_CustomsRegNo = "CODE2";
			matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);
			AssertEquals("Address match for CODE2", org4.MainAddress, matchedAddress);
			AssertContains(OrganisationMatcherLoggingTestHelper.GetLogForMatchedAddressOnRegistrationDetails(matchedAddress, code4), logger.Logs);
			matchedAddress = matcher.GetMatchingAddress(orgMatchingData, "", false);
			AssertNull("Should not match address for CODE2 when shortcode is specified", matchedAddress);
			logger.ClearLogs();
			matchedOrganization = matcher.GetMatchingOrganization(orgMatchingData);
			AssertEquals("organisation match for CODE2", org4, matchedOrganization);
			AssertContains(OrganisationMatcherLoggingTestHelper.GetLogForMatchedOrgOnRegistrationDetails(org4, code4), logger.Logs);

			orgCusCodeMatch.OK_CustomsRegNo = "CODE3";
			matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);
			AssertNull("Should not match address for CODE3 when country is not US", matchedAddress);
			matchedAddress = matcher.GetMatchingAddress(orgMatchingData, "", false);
			AssertNull("Should not match address for CODE3 when country is not US", matchedAddress);
			matchedOrganization = matcher.GetMatchingOrganization(orgMatchingData);
			AssertNull("Should not match organisation for CODE3 when country is not US", matchedOrganization);

			logger.ClearLogs();
			code1.OK_CustomsRegNo = "CODE3";
			matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);
			AssertEquals("Address match for CODE3", org1.MainAddress, matchedAddress);
			AssertContains(OrganisationMatcherLoggingTestHelper.GetLogForMatchedAddressOnRegistrationDetails(matchedAddress, code1), logger.Logs);
			logger.ClearLogs();
			matchedAddress = matcher.GetMatchingAddress(orgMatchingData, "", false);
			AssertNull("Should not match address for CODE3 when shortcode is specified", matchedAddress);
			logger.ClearLogs();
			matchedOrganization = matcher.GetMatchingOrganization(orgMatchingData);
			AssertEquals("organisation match for CODE3", org1, matchedOrganization);
			AssertContains(OrganisationMatcherLoggingTestHelper.GetLogForMatchedOrgOnRegistrationDetails(org1, code1), logger.Logs);

			logger.ClearLogs();
			orgCusCodeMatch.OK_CustomsRegNo = "CODE1";
			matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);
			AssertEquals("Address match for CODE1", org2.MainAddress, matchedAddress);
			AssertContains(OrganisationMatcherLoggingTestHelper.GetLogForMatchedAddressOnRegistrationDetails(matchedAddress, code2), logger.Logs);
			matchedAddress = matcher.GetMatchingAddress(orgMatchingData, "", false);
			AssertNull("Should not match address for CODE1 when shortcode is specified", matchedAddress);
			logger.ClearLogs();
			matchedOrganization = matcher.GetMatchingOrganization(orgMatchingData);
			AssertEquals("organisation match for CODE1", org2, matchedOrganization);
			AssertContains(OrganisationMatcherLoggingTestHelper.GetLogForMatchedOrgOnRegistrationDetails(org2, code2), logger.Logs);
		}

		public void TestAddressAndOrganizationIsNotMatchingWithTaxRegistrationCodeType_WhenCountryComplianceInfoIsNull()
		{
			var (orgHeader, orgMatchingData, matcher, logger, orgHeaderOrgCusCode_AUABN) = SetupRequiredForAddressAndOrganizationMatching();
			var orgMatchingDataOrgCusCode_AUABN = CreateOrgCusCodeForMatching(orgMatchingData, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "ABN", CountryCodes.Australia);

			var mockComplianceFactory = new Mock<ICountryComplianceFactory>();
			ObjectFactory.Substitute(mockComplianceFactory.Object);

			SetupComplianceFactoryMock(null);
			AssertAddressAndOrganizationMatchingWithRegistryEnabled("No match", orgMatchingData, matcher, logger, orgMatchingDataOrgCusCode_AUABN, true, expectedAddressLogCode: logMessageTypes.AssignedToUnmatchedOrg);

			var mockCountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();
			mockCountryComplianceInfoBase.As<ICountryComplianceInfo>().Setup(x => x.GetConsumptionTaxRegistrationCode()).Returns("ABN");
			SetupComplianceFactoryMock(mockCountryComplianceInfoBase.Object);
			AssertAddressAndOrganizationMatchingWithRegistryEnabled("Matched", orgMatchingData, matcher, logger, orgMatchingDataOrgCusCode_AUABN, false, expectedAddressLogCode: logMessageTypes.MatchedAddressOnRegistrationDetails, expectedOrgLogCode: logMessageTypes.MatchedOrgOnRegistrationDetails, orgHeaderOrgCusCode_AUABN, orgHeader);

			void SetupComplianceFactoryMock(ICountryComplianceInfoBase complianceInfoBase)
			{
				mockComplianceFactory.Reset();
				mockComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>())).Returns(complianceInfoBase);
			}
		}

		public void TestAddressAndOrganizationIsNotMatchingWithTaxRegistrationCodeType_WhenTaxRegistrationCodeTypeIsNull()
		{
			var (orgHeader, orgMatchingData, matcher, logger, orgHeaderOrgCusCode_AUABN) = SetupRequiredForAddressAndOrganizationMatching();
			var orgMatchingDataOrgCusCode_AUABN = CreateOrgCusCodeForMatching(orgMatchingData, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "ABN", CountryCodes.Australia);

			var mockComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockCountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();
			mockCountryComplianceInfoBase.As<ICountryComplianceInfo>().Setup(x => x.GetConsumptionTaxRegistrationCode()).Returns((string)null);
			mockComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>())).Returns(mockCountryComplianceInfoBase.Object);
			ObjectFactory.Substitute(mockComplianceFactory.Object);
			AssertAddressAndOrganizationMatchingWithRegistryEnabled("No match", orgMatchingData, matcher, logger, orgMatchingDataOrgCusCode_AUABN, true, expectedAddressLogCode: logMessageTypes.AssignedToUnmatchedOrg);

			mockCountryComplianceInfoBase.As<ICountryComplianceInfo>().Setup(x => x.GetConsumptionTaxRegistrationCode()).Returns("ABN");
			AssertAddressAndOrganizationMatchingWithRegistryEnabled("Matched", orgMatchingData, matcher, logger, orgMatchingDataOrgCusCode_AUABN, false, expectedAddressLogCode: logMessageTypes.MatchedAddressOnRegistrationDetails, expectedOrgLogCode: logMessageTypes.MatchedOrgOnRegistrationDetails, orgHeaderOrgCusCode_AUABN, orgHeader);
		}

		public void TestAddressAndOrganizationIsNotMatchingWithTaxRegistrationCodeType_WhenRegistryIsNotEnabled()
		{
			var (orgHeader, orgMatchingData, matcher, logger, orgHeaderOrgCusCode_AUABN) = SetupRequiredForAddressAndOrganizationMatching();
			var orgMatchingDataOrgCusCode_AUABN = CreateOrgCusCodeForMatching(orgMatchingData, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "ABN", CountryCodes.Australia);

			var mockComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockCountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();
			mockCountryComplianceInfoBase.As<ICountryComplianceInfo>().Setup(x => x.GetConsumptionTaxRegistrationCode()).Returns("ABN");
			mockComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>())).Returns(mockCountryComplianceInfoBase.Object);
			ObjectFactory.Substitute(mockComplianceFactory.Object);
			AssertAddressAndOrganizationMatching("No match", orgMatchingData, matcher, logger, orgMatchingDataOrgCusCode_AUABN, true, expectedAddressLogCode: logMessageTypes.AssignedToUnmatchedOrg);	

			AssertAddressAndOrganizationMatchingWithRegistryEnabled("Matched", orgMatchingData, matcher, logger, orgMatchingDataOrgCusCode_AUABN, false, expectedAddressLogCode: logMessageTypes.MatchedAddressOnRegistrationDetails, expectedOrgLogCode: logMessageTypes.MatchedOrgOnRegistrationDetails, orgHeaderOrgCusCode_AUABN, orgHeader);
		}

		public void TestAddressAndOrganizationIsMatchingWithTaxRegistrationCodeType_WhenMatchingTaxRegistrationCodeExists()
		{
			var (orgHeader, orgMatchingData, matcher, logger, orgCusCode_AUABN) = SetupRequiredForAddressAndOrganizationMatching();
			var orgCusCodeMatch_AUVAT = CreateOrgCusCodeForMatching(orgMatchingData, OrgCusCode.CodeTypes.VATCode, "VATCode", CountryCodes.Australia);

			var mockComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockCountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();
			mockCountryComplianceInfoBase.As<ICountryComplianceInfo>().Setup(x => x.GetConsumptionTaxRegistrationCode()).Returns("ABN");
			mockComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>())).Returns(mockCountryComplianceInfoBase.Object);
			ObjectFactory.Substitute(mockComplianceFactory.Object);
			AssertAddressAndOrganizationMatchingWithRegistryEnabled("No match", orgMatchingData, matcher, logger, orgCusCodeMatch_AUVAT, true, expectedAddressLogCode: logMessageTypes.AssignedToUnmatchedOrg);

			var orgCusCodeMatch_AUABN = CreateOrgCusCodeForMatching(orgMatchingData, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "ABN", CountryCodes.Australia);
			AssertAddressAndOrganizationMatchingWithRegistryEnabled("Matched", orgMatchingData, matcher, logger, orgCusCodeMatch_AUABN, false, expectedAddressLogCode: logMessageTypes.MatchedAddressOnRegistrationDetails, expectedOrgLogCode: logMessageTypes.MatchedOrgOnRegistrationDetails, orgCusCode_AUABN, orgHeader);
		}

		void AssertAddressAndOrganizationMatchingWithRegistryEnabled(string assertMessage, OrgHeaderForMatching orgMatchingData, OrganisationMatcher matcher, TestErrorLogger logger, IOrgCusCodeForMatching matchingOrgCusCode, bool isResultNull, logMessageTypes expectedAddressLogCode, logMessageTypes expectedOrgLogCode = logMessageTypes.Unmatched, OrgCusCode orgCusCode = null, OrgHeader expectedOrganisation = null)
		{
			var registryValue = new CodeDescriptionBoolCollection(OrganizationMatcherVATRegistrationNumberContextTypeList());
			registryValue.Set(OrganisationMatchingByVATRegistrationNumberContexts.Codes.Payables, true);

			using (AccountingMasterFilesRegistry.Instance.UseVATRegistrationNumberAsOrganizationMatchingCriteria.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				AssertAddressAndOrganizationMatching(assertMessage, orgMatchingData, matcher, logger, matchingOrgCusCode, isResultNull, expectedAddressLogCode, expectedOrgLogCode, orgCusCode, expectedOrganisation);
			}
		}

		void AssertAddressAndOrganizationMatching(string assertMessage, OrgHeaderForMatching orgMatchingData, OrganisationMatcher matcher, TestErrorLogger logger, IOrgCusCodeForMatching matchingOrgCusCode, bool isResultNull, logMessageTypes expectedAddressLogCode, logMessageTypes expectedOrgLogCode = logMessageTypes.Unmatched, OrgCusCode orgCusCode = null, OrgHeader expectedOrganisation = null)
		{
			OrgAddress matchedAddress = null;
			OrgHeader matchedOrganization = null;

			logger.ClearLogs();
			matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);
			AssertEquals($"{assertMessage} address match for {matchingOrgCusCode.OK_CustomsRegNo}", isResultNull, matchedAddress == null);
			Assert($"{assertMessage} organization match for {matchingOrgCusCode.OK_CustomsRegNo}", isResultNull || matchedAddress.Header.OH_Code == expectedOrganisation.OH_Code);
			AssertContains($"{assertMessage} address match for {matchingOrgCusCode.OK_CustomsRegNo}", OrganisationMatcherLoggingTestHelper.GetExpectedLogMessage(expectedAddressLogCode, orgMatchingData, orgCusCode, matchedAddress, matchedOrganization, matchingOrgCusCode), logger.Logs);

			logger.ClearLogs();

			matchedOrganization = matcher.GetMatchingOrganization(orgMatchingData);
			AssertEquals($"{assertMessage} organization match for {matchingOrgCusCode.OK_CustomsRegNo}", isResultNull, matchedOrganization == null);
			Assert($"{assertMessage} organization match for {matchingOrgCusCode.OK_CustomsRegNo}", isResultNull || matchedOrganization.OH_Code == expectedOrganisation.OH_Code);
			AssertContains(
				$"{assertMessage} organization match for {matchingOrgCusCode.OK_CustomsRegNo}", expectedOrgLogCode == 0
				? OrganisationMatcherLoggingTestHelper.GetExpectedLogMessage(expectedAddressLogCode, orgMatchingData, orgCusCode, matchedAddress, matchedOrganization, matchingOrgCusCode)
				: OrganisationMatcherLoggingTestHelper.GetExpectedLogMessage(expectedOrgLogCode, orgMatchingData, orgCusCode, matchedAddress, matchedOrganization, matchingOrgCusCode),
				logger.Logs);
		}

		public void TestRegistrationMatching_InactiveOrganisation()
		{
			var factory = new BusinessObjectFactory();
			var org1 = factory.New<OrgHeader>();
			org1.OH_Code = "AB3";
			org1.MainAddress.OA_Address1 = "ADD 1";
			var org2 = factory.New<OrgHeader>();
			org2.OH_IsActive = false;
			org2.OH_Code = "AB2";
			org2.MainAddress.OA_Address1 = "ADD 1";

			var code1 = org1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "CODE1", Core.Constants.CountryCodes.UnitedStates);
			var code2 = org2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "CODE1", Core.Constants.CountryCodes.UnitedStates);
			factory.Save();

			var orgMatchingData = new OrgHeaderForMatching(factory);
			var orgCusCodeMatch = new OrgCusCodeForMatching();
			orgMatchingData.CustomsCodes.Add(orgCusCodeMatch);
			orgCusCodeMatch.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			orgCusCodeMatch.OK_CustomsRegNo = "CODE1";
			orgCusCodeMatch.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			var logger = new TestErrorLogger();
			var matcher = new OrganisationMatcher(factory, IfUnmatched.TakeBehaviourFromOverallSetting, logger);
			var matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);
			AssertEquals("Since org2 is inactive we should match org1", org1.PK, matchedAddress.Header.PK);

			var matchedOrganization = matcher.GetMatchingOrganization(orgMatchingData);
			AssertEquals("Since org2 is inactive we should match org1", org1.PK, matchedOrganization.PK);
		}

		public void TestCodeMatching_InactiveOrganisation()
		{
			var factory = new BusinessObjectFactory();
			var org1 = factory.New<OrgHeader>();
			org1.OH_Code = "AB3";
			org1.MainAddress.OA_Address1 = "ADD 1";

			var orgMatchingData = new OrgHeaderForMatching(factory);
			orgMatchingData.OH_Code = "AB3";

			var addressData = new OrgAddressForMatching();
			orgMatchingData.Addresses.Add(addressData);
			addressData.OA_Address1 = "ADD 1";

			var logger = new TestErrorLogger();
			var matcher = new OrganisationMatcher(factory, IfUnmatched.TakeBehaviourFromOverallSetting, logger);
			var matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);
			var matchingOrg = matcher.GetMatchingOrganization(orgMatchingData);
			AssertNotNull("Precondition: The filter works", matchedAddress);
			AssertNotNull("Precondition: The filter works", matchingOrg);

			org1.OH_IsActive = false;
			matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);
			AssertNull("Should not match inactive org", matchedAddress);
			AssertContains(OrganisationMatcherLoggingTestHelper.GetLogForInactiveOrg(org1), logger.Logs);

			logger.ClearLogs();

			matchingOrg = matcher.GetMatchingOrganization(orgMatchingData);
			AssertNull("Should not match inactive org", matchingOrg);
			AssertContains(OrganisationMatcherLoggingTestHelper.GetLogForInactiveOrg(org1), logger.Logs);
		}

		public void TestGetMatchingOrgAddress_IfUnmatched_ReturnUnmatchedOrganization()
		{
			var factory = new BusinessObjectFactory();
			var logger = new TestErrorLogger();
			var matcher = new OrganisationMatcher(factory, IfUnmatched.ReturnUnmatchedOrganisation, logger);
			var unmatchedOrganisation = new UnmatchedOrganisation(Factory) { IsEnabled = false };
			using (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrganisation))
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var address = matcher.GetMatchingAddress(new OrgHeaderForMatching(factory), "", false);
				AssertEquals("Return unmatched address when no matching result even the registry is off", OrgHeader.UnmatchOrg(factory).MainAddress.PK, address.PK);
				AssertContains(OrganisationMatcherLoggingTestHelper.GetLogForAssignedToUnmatchedOrg(), logger.Logs);

				logger.ClearLogs();
				var org = matcher.GetMatchingOrganization(new OrgHeaderForMatching(factory));
				AssertEquals("Return unmatched organization when no matching result even the registry is off", OrgHeader.UnmatchOrg(factory).PK, org.PK);
				AssertContains(OrganisationMatcherLoggingTestHelper.GetLogForAssignedToUnmatchedOrg(), logger.Logs);
			}
		}

		public void TestGetMatchingOrgAddress_IfUnmatched_ReturnNull()
		{
			var factory = new BusinessObjectFactory();
			var logger = new TestErrorLogger();
			var matcher = new OrganisationMatcher(factory, IfUnmatched.ReturnNull, logger);
			var unmatchedOrganisation = new UnmatchedOrganisation(Factory) { IsEnabled = true };
			using (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrganisation))
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var address = matcher.GetMatchingAddress(new OrgHeaderForMatching(factory), "", false);
				AssertNull("Return null when no matching result even the registry is on", address);
				AssertNotContains(OrganisationMatcherLoggingTestHelper.GetLogForAssignedToUnmatchedOrg(), logger.Logs);

				var org = matcher.GetMatchingOrganization(new OrgHeaderForMatching(factory));
				AssertNull("Return null when no matching result even the registry is on", org);
				AssertNotContains(OrganisationMatcherLoggingTestHelper.GetLogForAssignedToUnmatchedOrg(), logger.Logs);
			}
		}

		public void TestGetMatchingOrgAddress_IfUnmatched_TakeBehaviourFromOverallSetting_ReturnUnmatchedOrganization()
		{
			var factory = new BusinessObjectFactory();
			var logger = new TestErrorLogger();
			var matcher = new OrganisationMatcher(factory, IfUnmatched.TakeBehaviourFromOverallSetting, logger);
			var unmatchedOrganisation = new UnmatchedOrganisation(Factory) { IsEnabled = true };
			using (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrganisation))
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var address = matcher.GetMatchingAddress(new OrgHeaderForMatching(factory), "", false);
				AssertEquals("Return unmatched address when no matching result", OrgHeader.UnmatchOrg(factory).MainAddress.PK, address.PK);
				AssertContains(OrganisationMatcherLoggingTestHelper.GetLogForAssignedToUnmatchedOrg(), logger.Logs);

				logger.ClearLogs();
				var org = matcher.GetMatchingOrganization(new OrgHeaderForMatching(factory));
				AssertEquals("Return unmatched organization when no matching result", OrgHeader.UnmatchOrg(factory).PK, org.PK);
				AssertContains(OrganisationMatcherLoggingTestHelper.GetLogForAssignedToUnmatchedOrg(), logger.Logs);
			}
		}

		public void TestGetMatchingOrgAddress_IfUnmatched_TakeBehaviourFromOverallSetting_ReturnNull()
		{
			var factory = new BusinessObjectFactory();
			var logger = new TestErrorLogger();
			var matcher = new OrganisationMatcher(factory, IfUnmatched.TakeBehaviourFromOverallSetting, logger);
			var unmatchedOrganisation = new UnmatchedOrganisation(Factory) { IsEnabled = false };
			using (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrganisation))
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var address = matcher.GetMatchingAddress(new OrgHeaderForMatching(factory), "", false);
				AssertNull("Return null when no matching result", address);
				AssertNotContains(OrganisationMatcherLoggingTestHelper.GetLogForAssignedToUnmatchedOrg(), logger.Logs);

				var org = matcher.GetMatchingOrganization(new OrgHeaderForMatching(factory));
				AssertNull("Return null when no matching result", org);
				AssertNotContains(OrganisationMatcherLoggingTestHelper.GetLogForAssignedToUnmatchedOrg(), logger.Logs);
			}
		}

		public void TestOrgModuleValueMatcher()
		{
			var factory = new BusinessObjectFactory();
			var logger = new TestErrorLogger();

			SetRegistryValue(false);
			var matcher = new OrganisationMatcher(factory, IfUnmatched.TakeBehaviourFromUXMLModuleSepcifiedSetting, logger, DataContextType.OrderManagerOrder);
			AssertNull(matcher.defaultValueMatcher.Match());

			SetRegistryValue(true);
			matcher = new OrganisationMatcher(factory, IfUnmatched.TakeBehaviourFromUXMLModuleSepcifiedSetting, logger, DataContextType.OrderManagerOrder);
			AssertEquals(OrgHeader.UnmatchOrg(factory), matcher.defaultValueMatcher.Match());
		}

		#region Implementation

		void SetRegistryValue(bool useUnmatchedOrg)
		{
			var registryValue = new CodeDescriptionBoolCollection
			{
				{ OrganisationsDataRegistry.JobTypeCodes.Order, (NoResString)"Order (Forwarding)", useUnmatchedOrg },
			};

			OrganisationsDataRegistry.Instance.UnmatchedOrganisationConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
		}

		(OrgHeader orgHeader, OrgHeaderForMatching orgMatchingData, OrganisationMatcher matcher, TestErrorLogger logger, OrgCusCode orgCusCode_AUABN) SetupRequiredForAddressAndOrganizationMatching()
		{
			OrganisationMatcherContexts context = OrganisationMatcherContexts.Payables;
			var factory = new BusinessObjectFactory();

			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = "AB1";
			orgHeader.MainAddress.OA_Address1 = "ADD 1";
			var orgCusCode_AUABN = orgHeader.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "ABN", CountryCodes.Australia);
			var orgMatchingData = new OrgHeaderForMatching(factory);
			orgMatchingData.OH_FullName = "Test Organization Name";

			var logger = new TestErrorLogger();
			factory.SetContext(context);

			var matcher = new OrganisationMatcher(factory, IfUnmatched.TakeBehaviourFromOverallSetting, logger);
			return (orgHeader, orgMatchingData, matcher, logger, orgCusCode_AUABN);
		}

		OrgCusCodeForMatching CreateOrgCusCodeForMatching(OrgHeaderForMatching orgMatching, string code, string value, string countryCode)
		{
			var orgCusCodeMatch = new OrgCusCodeForMatching() { OK_CodeType = code, OK_CustomsRegNo = value, OK_RN_NKCodeCountry = countryCode };
			orgMatching.CustomsCodes.Add(orgCusCodeMatch);

			return orgCusCodeMatch;
		}

		protected override void SetUp()
		{
			base.SetUp();

			mocks = new MockRepository(MockBehavior.Loose);
			localCodeMatcher = mocks.Create<IOrgLocalCodeMatcher>();
			similarityMatcher = mocks.Create<IOrgSimilarMatcher>();
			defaultValueMatcher = mocks.Create<IOrgDefaultValueMatcher>();

			matcher = new OrganisationMatcher(Factory, localCodeMatcher.Object, similarityMatcher.Object, defaultValueMatcher.Object, new DummyLogger());

			var query = new ZQuery();
			orgHeader = Factory.LoadTop1<OrgHeader>(query);
			org = new OrgHeaderForMatching(Factory);
		}

		MockRepository mocks;
		OrganisationMatcher matcher;
		Mock<IOrgDefaultValueMatcher> defaultValueMatcher;
		Mock<IOrgLocalCodeMatcher> localCodeMatcher;
		Mock<IOrgSimilarMatcher> similarityMatcher;
		OrgHeader orgHeader;
		OrgHeaderForMatching org;

		class TestOrganisationCreator
		{
			internal TestOrganisationCreator(bool addNewAddress)
			{
				var setupFactory = new BusinessObjectFactory();

				Organization = setupFactory.New<OrgHeader>();
				Organization.OH_FullName = "FREDA CARLOS PICTOGRAPHICS";
				Organization.OH_RL_NKClosestPort = "AUSYD";
				Organization.OH_Code = "FRECARSYD";

				Address1 = Organization.MainAddress;
				Address1.OA_Address1 = "343 FREEKIE DEEK LANE";
				Address1.OA_City = "FREEKVALE";
				Address1.OA_State = "NSW";
				Address1.OA_PostCode = "2787";

				if (addNewAddress)
				{
					Address2 = Organization.Addresses.AddNew(OrgAddressType.PickupAndDelivery, true);
					Address2.OA_Address1 = "212 FREDDO FROG DRIVE";
					Address2.OA_City = "CADBURY";
					Address2.OA_State = "NSW";
					Address2.OA_PostCode = "2434";
				}

				setupFactory.Save();
			}

			internal readonly OrgHeader Organization;
			internal readonly OrgAddress Address1;
			internal readonly OrgAddress Address2;
		}

		#endregion
	}
}
