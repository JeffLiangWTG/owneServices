using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	class GateManagementOrganisationAddressMatcherTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestCommunityCodeMatching_WhenNotEntered_DoesNotMatch()
		{
			var orgHeaderBO = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderBO.OH_Code = "TSTTSTTST";

			var orgCusCode = orgHeaderBO.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "CC_123", string.Empty);
			orgCusCode.OK_OA_PremisesAddress = orgHeaderBO.MainAddress.PK;

			var orgAddressDataObject = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddressDataObject.Address1 = "1 main st";
			orgAddressDataObject.City = "Sydney";
			orgAddressDataObject.Postcode = "2020";
			orgAddressDataObject.State = "NSW";
			orgAddressDataObject.Country = new Country();
			orgAddressDataObject.Country.Code = "AU";

			var logger = new DummyLogger();
			var matcher = new GateManagementOrganisationDataObjectReader(orgAddressDataObject, logger, Factory);
			var matchedAddress = matcher.GetMatched();
			AssertNull("no address should be found before community code is set", matchedAddress);
			AssertEquals("logs", "Warning - Matching '':- No match found for '[Address 1: 1 main st; City: Sydney]'.", logger.LogsString);
		}

		public void TestCommunityCodeMatching_WhenEnteredCorrectly_Matches()
		{
			var orgHeaderBO = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderBO.OH_Code = "TSTTSTTST";

			var orgCusCode = orgHeaderBO.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "CC_123", string.Empty);
			orgCusCode.OK_OA_PremisesAddress = orgHeaderBO.MainAddress.PK;

			var orgAddressDataObject = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddressDataObject.Address1 = "1 main st";
			orgAddressDataObject.City = "Sydney";
			orgAddressDataObject.Postcode = "2020";
			orgAddressDataObject.State = "NSW";
			orgAddressDataObject.Country = new Country();
			orgAddressDataObject.Country.Code = "AU";

			var registrationNumberType = new RegistrationNumberType() { Code = OrgCusCode.CodeTypes.ContainerChainCommunityCode };
			var registrationNumber = new RegistrationNumber() { Type = registrationNumberType, Value = "CC_123" };
			orgAddressDataObject.SetRegistrationNumberCollection(() => new List<RegistrationNumber>() { registrationNumber });

			var logger = new DummyLogger();
			var matcher = new GateManagementOrganisationDataObjectReader(orgAddressDataObject, logger, Factory);
			var matchedAddress = matcher.GetMatched();

			AssertNotNull("address should be found after community code is set", matchedAddress);
			AssertEquals("matched address pk's should match", matchedAddress.PK, orgHeaderBO.MainAddress.PK);
			AssertEquals("logs", "Information - Matching '':- Matched to address '#1' on 'TSTTSTTST' by Container Chain community code 'CC_123'", logger.LogsString);
		}

		public void TestCommunityCodeMatching_WhenEnteredIncorrectly_DoesNotMatch()
		{
			var orgHeaderBO = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderBO.OH_Code = "TSTTSTTST";

			var orgCusCode = orgHeaderBO.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "CC_123", string.Empty);
			orgCusCode.OK_OA_PremisesAddress = orgHeaderBO.MainAddress.PK;

			var orgAddressDataObject = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddressDataObject.Address1 = "1 main st";
			orgAddressDataObject.City = "Sydney";
			orgAddressDataObject.Postcode = "2020";
			orgAddressDataObject.State = "NSW";
			orgAddressDataObject.Country = new Country();
			orgAddressDataObject.Country.Code = "AU";

			var registrationNumberType = new RegistrationNumberType() { Code = OrgCusCode.CodeTypes.ContainerChainCommunityCode };
			var registrationNumber = new RegistrationNumber() { Type = registrationNumberType, Value = "CC_XXX" };
			orgAddressDataObject.SetRegistrationNumberCollection(() => new List<RegistrationNumber>() { registrationNumber });

			var logger = new DummyLogger();
			var matcher = new GateManagementOrganisationDataObjectReader(orgAddressDataObject, logger, Factory);
			var matchedAddress = matcher.GetMatched();

			AssertNull("address should not be found when community code doesn't match", matchedAddress);
			AssertEquals("logs", "Warning - Matching '':- Community Code 'CC_XXX' is not configured. No match found for '[Address 1: 1 main st; City: Sydney]'.", logger.LogsString);
		}

		public void TestBaseAddressMatching()
		{
			var orgHeaderBO = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderBO.OH_Code = "TSTTSTTST";
			orgHeaderBO.OH_FullName = "Test Organisation For Address Matching LTD";
			orgHeaderBO.MainAddress.City = "Sydney";
			orgHeaderBO.MainAddress.CompanyName = "Test Organisation For Address Matching LTD";
			orgHeaderBO.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgHeaderBO.MainAddress.OA_Email = "123TestAddress@gmail.com";
			orgHeaderBO.MainAddress.Postcode = "2020";
			orgHeaderBO.MainAddress.OA_Address1 = "1 main st";
			orgHeaderBO.MainAddress.OA_State = "NSW";

			Factory.SaveForTesting();

			var orgAddressDataObject = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddressDataObject.Address1 = "1 main st";
			orgAddressDataObject.City = "Sydney";
			orgAddressDataObject.Postcode = "2020";
			orgAddressDataObject.State = "NSW";
			orgAddressDataObject.Country = new Country();
			orgAddressDataObject.Country.Code = "AU";
			orgAddressDataObject.CompanyName = "Test Organisation For Address Matching LTD";
			orgAddressDataObject.Port = new UNLOCO() { Code = "AUSYD" };
			orgAddressDataObject.Email = "123TestAddress@gmail.com";

			var logger = new DummyLogger();
			var matcher = new GateManagementOrganisationDataObjectReader(orgAddressDataObject, logger, Factory);
			var matchedAddress = matcher.GetMatched();
			AssertNotNull("matching address should be found without community code, orgCode, or address code", matchedAddress);
			AssertEquals("logs", "Information - Matching '':- Matched to 'TESORGSYD' address '1 main st' with a score of 400.", logger.LogsString);
		}

		public void TestCommunityCodePopulatedWhenMatchedOnAddress()
		{
			var orgHeaderBO = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderBO.OH_Code = "TSTTSTTST";
			orgHeaderBO.OH_FullName = "Test Organisation For Address Matching LTD";
			orgHeaderBO.MainAddress.City = "Sydney";
			orgHeaderBO.MainAddress.CompanyName = "Test Organisation For Address Matching LTD";
			orgHeaderBO.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgHeaderBO.MainAddress.OA_Email = "123TestAddress@gmail.com";
			orgHeaderBO.MainAddress.Postcode = "2020";
			orgHeaderBO.MainAddress.OA_Address1 = "1 main st";
			orgHeaderBO.MainAddress.OA_State = "NSW";

			Factory.SaveForTesting();

			var orgAddressDataObject = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddressDataObject.Address1 = "1 main st";
			orgAddressDataObject.City = "Sydney";
			orgAddressDataObject.Postcode = "2020";
			orgAddressDataObject.State = "NSW";
			orgAddressDataObject.Country = new Country();
			orgAddressDataObject.Country.Code = "AU";
			orgAddressDataObject.CompanyName = "Test Organisation For Address Matching LTD";
			orgAddressDataObject.Port = new UNLOCO() { Code = "AUSYD" };
			orgAddressDataObject.Email = "123TestAddress@gmail.com";

			var registrationNumberType = new RegistrationNumberType() { Code = OrgCusCode.CodeTypes.ContainerChainCommunityCode };
			var registrationNumber = new RegistrationNumber() { Type = registrationNumberType, Value = "CC_123" };
			orgAddressDataObject.SetRegistrationNumberCollection(() => new List<RegistrationNumber>() { registrationNumber });

			var logger = new DummyLogger();
			var matcher = new GateManagementOrganisationDataObjectReader(orgAddressDataObject, logger, Factory);
			var matchedAddress = matcher.GetMatched();
			AssertNotNull("matching address should be found by address when community code does not have a match", matchedAddress);

			var communityCodeQuery = new ZQuery(OrgCusCodeSchema.OK_OA_PremisesAddress, matchedAddress.PK);
			communityCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.ContainerChainCommunityCode);
			var communityCodes = Factory.Load<OrgCusCode>(communityCodeQuery);
			AssertEquals("matching address should have a community code populated by matcher", 1, communityCodes.Length);
			AssertEquals("populated code should have correct value", "CC_123", communityCodes[0].OK_CustomsRegNo);
			AssertEquals("populated code should match to correct organisation", matchedAddress.OA_OH, communityCodes[0].OK_OH);
			AssertEquals("populated code should have no country", "", communityCodes[0].OK_RN_NKCodeCountry);
		}

		public void TestExistingCommunityCodeNotOverridenWhenMatchedOnAddress()
		{
			var orgHeaderBO = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderBO.OH_Code = "TSTTSTTST";
			orgHeaderBO.OH_FullName = "Test Organisation For Address Matching LTD";
			orgHeaderBO.MainAddress.City = "Sydney";
			orgHeaderBO.MainAddress.CompanyName = "Test Organisation For Address Matching LTD";
			orgHeaderBO.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgHeaderBO.MainAddress.OA_Email = "123TestAddress@gmail.com";
			orgHeaderBO.MainAddress.Postcode = "2020";
			orgHeaderBO.MainAddress.OA_Address1 = "1 main st";
			orgHeaderBO.MainAddress.OA_State = "NSW";

			var orgCusCode = orgHeaderBO.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "CC_123_Original", string.Empty);
			orgCusCode.OK_OA_PremisesAddress = orgHeaderBO.MainAddress.PK;
			Factory.SaveForTesting();

			var orgAddressDataObject = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddressDataObject.Address1 = "1 main st";
			orgAddressDataObject.City = "Sydney";
			orgAddressDataObject.Postcode = "2020";
			orgAddressDataObject.State = "NSW";
			orgAddressDataObject.Country = new Country();
			orgAddressDataObject.Country.Code = "AU";
			orgAddressDataObject.CompanyName = "Test Organisation For Address Matching LTD";
			orgAddressDataObject.Port = new UNLOCO() { Code = "AUSYD" };
			orgAddressDataObject.Email = "123TestAddress@gmail.com";

			var registrationNumberType = new RegistrationNumberType() { Code = OrgCusCode.CodeTypes.ContainerChainCommunityCode };
			var registrationNumber = new RegistrationNumber() { Type = registrationNumberType, Value = "CC_123_new" };
			orgAddressDataObject.SetRegistrationNumberCollection(() => new List<RegistrationNumber>() { registrationNumber });

			var logger = new DummyLogger();
			var matcher = new GateManagementOrganisationDataObjectReader(orgAddressDataObject, logger, Factory);
			var matchedAddress = matcher.GetMatched();
			AssertNotNull("matching address should be found by address when community code does not have a match", matchedAddress);

			var communityCodeQuery = new ZQuery(OrgCusCodeSchema.OK_OA_PremisesAddress, matchedAddress.PK);
			communityCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.ContainerChainCommunityCode);
			var communityCodes = Factory.Load<OrgCusCode>(communityCodeQuery);
			AssertEquals("matching address should not have a new community code populated by matcher", 1, communityCodes.Length);
			AssertEquals("populated code should have original value", "CC_123_Original", communityCodes[0].OK_CustomsRegNo);
		}

		public void TestGateManagementAddressMatcherInterface_IsResolvable()
		{
			var organizationDataObjectReader = ObjectFactory.New<IGateManagementOrganisationDataObjectReader>(
				Mock.Of<OrganizationAddress>(),
				Mock.Of<IXmlImportLogger>(),
				Factory);
			AssertNotNull("organizationAddressMatcher", organizationDataObjectReader);
			AssertEquals("organizationAddressMatcher.GetType()", typeof(GateManagementOrganisationDataObjectReader), organizationDataObjectReader.GetType());
		}
	}
}
