using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Matching;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Testing.Universal.Testing
{
	public class OrganizationDataObjectReaderUseMatchEngineTest : TestCaseWithFactory
	{
		public void TestMatchUseDeduplication()
		{
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var factory = new UniversalObjectFactory();

				var org1 = OrgSetUp(factory);
				OrgSetUp(factory, "BMWMUC2", "HEIDEMANN 777");

				var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(MatchableOrganizationType.ConsigneeDocumentaryAddress),
					Address1 = "HEIDEMANNSTASSE 164",
					City = "MUENCHEN",
					CompanyName = "BMW",
					Country = new Country
					{
						Code = "DE"
					},
					Phone = "02123456789",
					Postcode = "80939",
					State = "BY",
				};
				var reader = new OrganisationDataObjectReader(addressData, logger, factory);

				CombineAssertions(() =>
				{
					var updatedAddress = reader.GetMatched();
					AssertEquals(updatedAddress.PK, org1.MainAddress.PK);
					AssertContains($"Information - Matching '{addressData.AddressType}':-  Matched to Organization '{org1.OH_Code}' and corresponding Organization Address '{org1.MainAddress.OA_Code}' OverallMatch = Exact, AddressConfidence = Exact, NameConfidence = Exact", logger.Logs);
				});
			}
		}

		public void TestAddressMinimumConfidence()
		{
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var factory = new UniversalObjectFactory();

				var org = OrgSetUp(factory);

				var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(MatchableOrganizationType.ConsigneeDocumentaryAddress),
					Address1 = "HEIDEMANNSTASSE 164",
					City = "MUENCHEN",
					CompanyName = "BMW",
					Country = new Country
					{
						Code = "DE"
					},
					Phone = "02987654321",
					Postcode = "80939",
					State = "BY",
				};

				using (OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
				{
					var reader = new OrganisationDataObjectReader(addressData, logger, factory);
					var updatedAddress = reader.GetMatched();
					AssertEquals(org.PK, updatedAddress.OA_OH);
				}

				using (OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 95))
				{
					var reader = new OrganisationDataObjectReader(addressData, logger, factory);
					var updatedAddress = reader.GetMatched();
					AssertEquals(null, updatedAddress);
				}
			}
		}

		public void TestNotAbleToMatchLogs()
		{
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var factory = new UniversalObjectFactory();

				OrgSetUp(factory);
				var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(MatchableOrganizationType.ConsigneeDocumentaryAddress),
					Address1 = "AVCD GDGDG 555",
					City = "NEW YORK",
					CompanyName = "AAACCC",
					Country = new Country
					{
						Code = "DE"
					},
					State = "BY",
					Phone = "02123456789"
				};

				var reader = new OrganisationDataObjectReader(addressData, logger, factory);

				CombineAssertions(() =>
				{
					var matchedAddress = reader.GetMatched();
					AssertNull(matchedAddress);
					AssertContains($"Matching '{addressData.AddressType}':- Unable to match address for '[Company Name: {addressData.CompanyName}; Address 1: {addressData.Address1}; City: {addressData.City}]'", logger.Logs);
				});
			}
		}

		public void TestNotMeetTheMinimumRequirementsLogs()
		{
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var factory = new UniversalObjectFactory();

				OrgSetUp(factory);
				var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(MatchableOrganizationType.ConsigneeDocumentaryAddress),
					Address1 = "AVCD GDGDG 555",
					City = "NEW YORK",
					CompanyName = "",
					Country = new Country
					{
						Code = "DE"
					},
					State = "BY"
				};

				var reader = new OrganisationDataObjectReader(addressData, logger, factory);

				CombineAssertions(() =>
				{
					var matchedAddress = reader.GetMatched();
					AssertNull(matchedAddress);
					AssertEquals($"Information - Matching '{addressData.AddressType}':- Not enough information to match address for '[Company Name: {addressData.CompanyName}; Address 1: {addressData.Address1}; City: {addressData.City}]'", logger.Logs);
				});
			}
		}

		public void TestGetMatchingOrgAddress_VerboseLogging()
		{
			var factory = new UniversalObjectFactory();
			var org1 = OrgSetUp(factory);
			OrgSetUp(factory, "BMWMUC2", "HEIDEMANN 777");

			var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(MatchableOrganizationType.ConsigneeDocumentaryAddress),
				Address1 = "HEIDEMANNSTASSE 164",
				City = "MUENCHEN",
				CompanyName = "BMW",
				Country = new Country
				{
					Code = "DE"
				},
				Phone = "02123456789",
				Postcode = "80939",
				State = "BY",
			};

			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var reader = new OrganisationDataObjectReader(addressData, logger, factory);
				reader.GetMatched();

				AssertMultilineASCIIEquals($@"Information - Matching 'ConsigneeDocumentaryAddress':-  Matched to Organization 'BMWMUC1' and corresponding Organization Address 'HEIDEMANNSTASSE 164' OverallMatch = Exact, AddressConfidence = Exact, NameConfidence = Exact 
Matching
(
	Name = BMW
	Address1 = HEIDEMANNSTASSE 164
	Address2 = 
	City = MUENCHEN
	Postcode = 80939
	State = BY
	Country = DE
)
: Matched to Organization '{org1.OH_Code}' and corresponding Organization Address '{org1.MainAddress.OA_Code}'
(
	Name = {org1.OH_FullName}
	Address1 = {org1.MainAddress.OA_Address1}
	Address2 = {org1.MainAddress.OA_Address2}
	City = {org1.MainAddress.OA_City}
	Postcode = {org1.MainAddress.OA_PostCode}
	State = {org1.MainAddress.OA_State}
	Country = {org1.MainAddress.OA_RN_NKCountryCode}
)
", logger.Logs);
			}
		}

		public void TestGetMatchingOrgAddress_LogHighestMatchInfo()
		{
			var factory = new UniversalObjectFactory();
			var orgHeader = OrgSetUp(factory, "BMWMUC2", "HEIDEMANN 777");

			var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(MatchableOrganizationType.ConsigneeDocumentaryAddress),
				Address1 = "HEIDEMANNSTASSE 164",
				City = "MUENCHEN",
				CompanyName = "BMW",
				Country = null,
				Phone = "02123456789",
				Postcode = "80939",
				State = "BY",
			};

			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.UXMLOrganisationMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			using (OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 100))
			{
				var reader = new OrganisationDataObjectReader(addressData, logger, factory);
				OrgAddress matchedAddress = null;

				CombineAssertions(() =>
				{
					AssertNoExceptionThrown(() => matchedAddress = reader.GetMatched());
					AssertNull(matchedAddress);
					AssertContains($@": No match met the threshold. The highest match was to Organization '{orgHeader.OH_Code}' and corresponding to Organization Address '{orgHeader.MainAddress.OA_Code}' but it was ignored as it did not meet the specified threshold.
	Name = {orgHeader.OH_FullName}
	Address1 = {orgHeader.MainAddress.OA_Address1}
	Address2 = {orgHeader.MainAddress.OA_Address2}
	City = {orgHeader.MainAddress.OA_City}
	Postcode = {orgHeader.MainAddress.OA_PostCode}
	State = {orgHeader.MainAddress.OA_State}
	Country = {orgHeader.MainAddress.OA_RN_NKCountryCode}
", logger.Logs);
				});
			}
		}

		public void TakeBehaviourFromOverallSetting_WhenNoMatchFound()
		{
			var unmatchedOrganisation = new UnmatchedOrganisation(Factory) { IsEnabled = true };
			using (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrganisation))
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var factory = new UniversalObjectFactory();

				var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(MatchableOrganizationType.ConsigneeDocumentaryAddress),
					Address1 = "Test Test Test",
				};

				var reader = new OrganisationDataObjectReader(addressData, logger, factory);

				var matchedAddress = reader.GetMatched(true);
				AssertEquals(matchedAddress.Header.PK, OrgHeader.UnmatchedOrganisationPK);
				AssertContains("No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)", logger.Logs);
			}

			unmatchedOrganisation = new UnmatchedOrganisation(Factory) { IsEnabled = false };
			using (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrganisation))
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var factory = new UniversalObjectFactory();

				var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(MatchableOrganizationType.ConsigneeDocumentaryAddress),
					Address1 = "Test Test Test",
				};

				var reader = new OrganisationDataObjectReader(addressData, logger, factory);

				var matchedAddress = reader.GetMatched(true);
				AssertNull(matchedAddress);
			}
		}

		public void TestReturnNull_WhenNoMatchFound()
		{
			var unmatchedOrganization = new UnmatchedOrganisation(Factory) { IsEnabled = true };
			using (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrganization))
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var factory = new UniversalObjectFactory();

				var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(MatchableOrganizationType.ConsigneeDocumentaryAddress),
					Address1 = "Test Test Test",
				};

				var reader = new OrganisationDataObjectReader(addressData, logger, factory);

				var matchedAddress = reader.GetMatched();
				AssertNull(matchedAddress);
			}
		}

		public void TestGetMatchingOrgAddress_OrgCodeNotFound_FallBackToRegularMatch_NoResult()
		{
			using (OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OrganizationCode = "ORGINDB",
					AddressShortCode = "ADDRESSNOTINDB",
					AddressType = nameof(MatchableOrganizationType.ConsigneeDocumentaryAddress),
					Address1 = "KG Fraunhoferstrae 6 80469",
					City = "MUENCHEN",
					CompanyName = "ORGANISATION",
					State = "BY",
				};

				var factory = new UniversalObjectFactory();
				var reader = new OrganisationDataObjectReader(addressData, logger, factory);
				var matchedAddress = reader.GetMatched();

				AssertNull(matchedAddress);
				AssertEquals("Information - Matching 'ConsigneeDocumentaryAddress':- Unable to match address for '[Company Name: ORGANISATION; Address 1: KG Fraunhoferstrae 6 80469; City: MUENCHEN]'", logger.Logs);
			}
		}

		public void TestGetMatchingOrgAddress_OrgCodeNotFound_FallBackToRegularMatch_HasResult()
		{
			using (OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "ORGANISATION";
				org.OH_Code = "ORGINDB";

				var mainAddress = org.MainAddress;
				mainAddress.OA_Address1 = "123 NOT A REAL STREET";
				mainAddress.OA_City = "SYDNEY";
				mainAddress.OA_State = "NSW";
				mainAddress.OA_PostCode = "2001";
				mainAddress.OA_Code = "MAINADDRESS1";

				CreatePatternMatchingName(org);
				CreatePatternMatchingAddress(mainAddress, org);

				Factory.Save();

				var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OrganizationCode = "OtherCode",
					AddressShortCode = mainAddress.OA_Code,
					AddressType = nameof(MatchableOrganizationType.ConsigneeDocumentaryAddress),
					Address1 = mainAddress.Address1,
					City = mainAddress.City,
					CompanyName = org.OH_FullName,
					State = mainAddress.State,
				};

				var factory = new UniversalObjectFactory();
				var reader = new OrganisationDataObjectReader(addressData, logger, factory);
				var matchedAddress = reader.GetMatched();

				AssertEquals(mainAddress.PK, matchedAddress.PK);
				AssertEquals("Information - Matching 'ConsigneeDocumentaryAddress':-  Matched to Organization 'ORGINDB' and corresponding Organization Address 'MAINADDRESS1' OverallMatch = High, AddressConfidence = High, NameConfidence = Exact", logger.Logs);
			}
		}

		public void TestGetMatchingOrgAddress_OrgCodeNotFound_FallBackToRegularMatch_HasResult_ReturnActiveOne()
		{
			using (OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var inActiveHigherScoreOrg = Factory.New<OrgHeader>();
				inActiveHigherScoreOrg.OH_IsActive = false;
				inActiveHigherScoreOrg.OH_FullName = "COSTCO";
				inActiveHigherScoreOrg.OH_Code = "COSTCO";
				inActiveHigherScoreOrg.OrganisationTypes = OrganisationTypes.Consignor;

				var inActiveHigherScoreOrgMainAddress = inActiveHigherScoreOrg.MainAddress;
				inActiveHigherScoreOrgMainAddress.OA_Code = "Riordan";
				inActiveHigherScoreOrgMainAddress.OA_Address1 = "72 O'Riordan Street";
				inActiveHigherScoreOrgMainAddress.OA_City = "SYDNEY";
				inActiveHigherScoreOrgMainAddress.OA_PostCode = "2015";
				inActiveHigherScoreOrgMainAddress.OA_State = "NSW";

				var activeLowerScoreOrg = Factory.New<OrgHeader>();
				activeLowerScoreOrg.OH_IsActive = true;
				activeLowerScoreOrg.OH_FullName = "COSTCO";
				activeLowerScoreOrg.OH_Code = "COSTCO1";
				activeLowerScoreOrg.OrganisationTypes = OrganisationTypes.Consignor;

				var activeLowerScoreOrgOrgMainAddress = activeLowerScoreOrg.MainAddress;
				activeLowerScoreOrgOrgMainAddress.OA_Code = "Riordan";
				activeLowerScoreOrgOrgMainAddress.OA_Address1 = "Unknown Street";
				activeLowerScoreOrgOrgMainAddress.OA_City = "SYDNEY";
				activeLowerScoreOrgOrgMainAddress.OA_PostCode = "2015";
				activeLowerScoreOrgOrgMainAddress.OA_State = "NSW";

				CreatePatternMatchingName(inActiveHigherScoreOrg);
				CreatePatternMatchingAddress(inActiveHigherScoreOrgMainAddress, inActiveHigherScoreOrg);

				CreatePatternMatchingName(activeLowerScoreOrg);
				CreatePatternMatchingAddress(activeLowerScoreOrgOrgMainAddress, activeLowerScoreOrg);

				Factory.Save();

				var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Address1 = "72 O'Riordan Street",
					City = "SYDNEY",
					CompanyName = "COSTCO",
					State = "NSW",
					OrganizationCode = "UnKnown",
					AddressShortCode = "Riordan",
					AddressType = nameof(MatchableOrganizationType.ConsignorDocumentaryAddress),
				};

				var factory = new UniversalObjectFactory();
				var reader = new OrganisationDataObjectReader(addressData, logger, factory);
				var matchedAddress = reader.GetMatched();

				AssertNotNull(matchedAddress);
				AssertEquals("Result should match to active address", activeLowerScoreOrg.MainAddress.PK, matchedAddress.PK);
				AssertEquals("Information - Matching 'ConsignorDocumentaryAddress':-  Matched to Organization 'COSTCO1' and corresponding Organization Address 'Unknown Street' OverallMatch = Medium, AddressConfidence = Medium, NameConfidence = Exact", logger.Logs);
			}
		}

		public void TestGetMatchingOrgAddress_OrgCodeFound_ReturnOnlyAddress()
		{
			using (OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "ORGANISATION";
				org.OH_Code = "ORGINDB";

				var mainAddress = org.MainAddress;
				mainAddress.OA_Address1 = "123 NOT A REAL STREET";
				mainAddress.OA_City = "SYDNEY";
				mainAddress.OA_State = "NSW";
				mainAddress.OA_PostCode = "2001";
				mainAddress.OA_Code = "MAINADDRESS1";

				Factory.Save();

				var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OrganizationCode = org.OH_Code,
					AddressShortCode = mainAddress.OA_Code,
					AddressType = nameof(MatchableOrganizationType.ConsigneeDocumentaryAddress),
					Address1 = mainAddress.Address1,
					City = mainAddress.City,
					CompanyName = org.OH_FullName,
					State = mainAddress.State,
				};

				var factory = new UniversalObjectFactory();
				var reader = new OrganisationDataObjectReader(addressData, logger, factory);
				var matchedAddress = reader.GetMatched();

				AssertEquals(mainAddress.PK, matchedAddress.PK);
				AssertEquals("Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'ORGINDB' by code, address 'MAINADDRESS1' (only address).", logger.Logs);
			}
		}

		public void TestGetMatchingOrgAddress_OrgCodeFound_WithMultipleAddresses_ReturnAddressByAddressCode()
		{
			using (OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "ORGANISATION";
				org.OH_Code = "ORGINDB";

				var mainAddress = org.MainAddress;
				mainAddress.OA_Address1 = "123 NOT A REAL STREET";
				mainAddress.OA_City = "SYDNEY";
				mainAddress.OA_State = "NSW";
				mainAddress.OA_PostCode = "2001";
				mainAddress.OA_Code = "MAINADDRESS1";

				var otherAddress = org.Addresses.AddNew();
				otherAddress.OA_Address1 = "123 NOT A REAL STREET";
				otherAddress.OA_City = "SYDNEY";
				otherAddress.OA_State = "NSW";
				otherAddress.OA_PostCode = "2001";
				otherAddress.OA_Code = "OtherAddress";

				Factory.Save();

				var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OrganizationCode = org.OH_Code,
					AddressShortCode = otherAddress.OA_Code,
					AddressType = nameof(MatchableOrganizationType.ConsigneeDocumentaryAddress),
					Address1 = mainAddress.Address1,
					City = mainAddress.City,
					CompanyName = org.OH_FullName,
					State = mainAddress.State,
				};

				var factory = new UniversalObjectFactory();
				var reader = new OrganisationDataObjectReader(addressData, logger, factory);
				var matchedAddress = reader.GetMatched();

				AssertEquals(otherAddress.PK, matchedAddress.PK);
				AssertEquals("Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'ORGINDB' by code, address 'OtherAddress' by short code.", logger.Logs);
			}
		}

		public void TestGetMatchingOrgAddress_OrgCodeFound_WithMultipleAddresses_AddressCodeNotFound_OnlyMatchByCode_ReturnNull()
		{
			AssertGetMatchingOrgAddress_OrgCodeFound_WithMultipleAddresses_AddressCodeNotFound_ReturnMostSimilarAddress(new DummyLoggerForTest());
		}

		public void TestGetMatchingOrgAddress_OrgCodeFound_WithMultipleAddresses_AddressCodeNotFound_ReturnMostSimilarAddress()
		{
			AssertGetMatchingOrgAddress_OrgCodeFound_WithMultipleAddresses_AddressCodeNotFound_ReturnMostSimilarAddress();
		}

		public void AssertGetMatchingOrgAddress_OrgCodeFound_WithMultipleAddresses_AddressCodeNotFound_ReturnMostSimilarAddress(IXmlImportLogger importLogger = null)
		{
			using (OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "ORGANISATION";
				org.OH_Code = "ORGINDB";

				var mainAddress = org.MainAddress;
				mainAddress.OA_Address1 = "123 A REAL STREET";
				mainAddress.OA_City = "SYDNEY";
				mainAddress.OA_State = "NSW";
				mainAddress.OA_PostCode = "2001";
				mainAddress.OA_Code = "MAINADDRESS1";

				var otherAddress = org.Addresses.AddNew();
				otherAddress.OA_Address1 = "123 NOT A REAL STREET";
				otherAddress.OA_City = "SYDNEY";
				otherAddress.OA_State = "NSW";
				otherAddress.OA_PostCode = "2001";
				otherAddress.OA_Code = "OtherAddress";

				CreatePatternMatchingName(org);
				CreatePatternMatchingAddress(mainAddress, org);
				CreatePatternMatchingAddress(otherAddress, org);

				Factory.Save();

				var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OrganizationCode = org.OH_Code,
					AddressShortCode = "UNKNOWNCODE",
					AddressType = nameof(MatchableOrganizationType.ConsigneeDocumentaryAddress),
					Address1 = otherAddress.Address1,
					City = otherAddress.City,
					CompanyName = org.OH_FullName,
					State = otherAddress.State,
				};

				var factory = new UniversalObjectFactory();
				var reader = new OrganisationDataObjectReader(addressData, importLogger ?? logger, factory);
				var matchedAddress = reader.GetMatched();

				if (importLogger != null)
				{
					AssertNull(matchedAddress);
				}
				else
				{
					AssertEquals(otherAddress.PK, matchedAddress.PK);
					AssertEquals("Information - Matching 'ConsigneeDocumentaryAddress':-  Matched to Organization 'ORGINDB' and corresponding Organization Address 'OtherAddress' OverallMatch = Medium, AddressConfidence = Medium, NameConfidence = Exact", logger.Logs);
				}
			}
		}

		public void TestGetMatchingOrgAddress_OrgCodeFound_WithMultipleAddresses_AddressCodeNotFound_MostSimilarAddressNotFound_ShouldReturnMainAddress()
		{
			using (OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "ORGANISATION";
				org.OH_Code = "ORGINDB";

				var mainAddress = org.MainAddress;
				mainAddress.OA_Address1 = "123 NOT A REAL STREET";
				mainAddress.OA_City = "SYDNEY";
				mainAddress.OA_State = "NSW";
				mainAddress.OA_PostCode = "2001";
				mainAddress.OA_Code = "MAINADDRESS1";

				var otherAddress = org.Addresses.AddNew();
				otherAddress.OA_Address1 = "123 NOT A REAL STREET";
				otherAddress.OA_City = "SYDNEY";
				otherAddress.OA_State = "NSW";
				otherAddress.OA_PostCode = "2001";
				otherAddress.OA_Code = "OtherAddress";

				Factory.Save();

				var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OrganizationCode = org.OH_Code,
					AddressShortCode = "UNKNOWNCODE",
					AddressType = nameof(MatchableOrganizationType.ConsigneeDocumentaryAddress),
					Address1 = otherAddress.Address1,
					City = otherAddress.City,
					CompanyName = org.OH_FullName,
					State = otherAddress.State,
				};

				var factory = new UniversalObjectFactory();
				var reader = new OrganisationDataObjectReader(addressData, logger, factory);
				var matchedAddress = reader.GetMatched();

				AssertEquals(mainAddress.PK, matchedAddress.PK);
				AssertEquals("Information - Matching 'ConsigneeDocumentaryAddress':- Unable to match address for '[Company Name: ORGANISATION; Address 1: 123 NOT A REAL STREET; City: SYDNEY]' Matched to 'ORGINDB' by code, main address used.", logger.Logs);
			}
		}

		public class DummyLoggerForTest : DummyLogger
		{
			protected override bool OrgMatchingDisabledCore => true;
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger() { TopLevelDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() } };
		}

		TestErrorLogger logger;

		IOrgHeader OrgSetUp(UniversalObjectFactory factory, string code = "BMWMUC1", string address1 = "HEIDEMANNSTASSE 164")
		{
			var org = factory.BOFactory.New<OrgHeader>();
			org.OH_FullName = "BMW";
			org.OH_Category = OrgConstants.Category.Business;
			org.OH_Language = "EN";
			org.OH_Code = code;
			org.MainAddress.OA_Address1 = address1;
			org.MainAddress.OA_City = "MUENCHEN";
			org.MainAddress.OA_State = "BY";
			org.MainAddress.OA_PostCode = "80939";
			org.MainAddress.OA_Phone = "02123456789";
			org.MainAddress.OA_RN_NKCountryCode = "DE";
			org.MainAddress.OA_Language = "EN";

			org.OrganisationTypes = OrganisationTypes.Consignor;

			var addressToHash = org.MainAddress.OA_Address1 + org.MainAddress.OA_City + org.MainAddress.OA_State + org.MainAddress.OA_PostCode;
			var patternMatchingAddress = factory.New<PatternMatchingAddress>();
			patternMatchingAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(addressToHash);
			patternMatchingAddress.PMA_OH = org.PK;
			patternMatchingAddress.PMA_ParentId = org.MainAddress.PK;
			patternMatchingAddress.PMA_RN_NKCountryCode = "DE";
			patternMatchingAddress.PMA_ParentTableCode = OrgAddressSchema.Constants.Prefix;

			var additionalAddress = org.Addresses.AddNew();
			additionalAddress.OA_Address1 = "1 Temp Rd";
			additionalAddress.OA_RN_NKCountryCode = "AU";
			var additionalPatternMatchingAddress = factory.New<PatternMatchingAddress>();
			additionalPatternMatchingAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(additionalAddress.OA_Address1);
			additionalPatternMatchingAddress.PMA_OH = org.PK;
			additionalPatternMatchingAddress.PMA_ParentId = additionalAddress.PK;
			additionalPatternMatchingAddress.PMA_RN_NKCountryCode = "AU";
			additionalPatternMatchingAddress.PMA_ParentTableCode = OrgAddressSchema.Constants.Prefix;

			var patternMatchingName = factory.New<PatternMatchingName>();
			patternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(org.OH_FullName);
			patternMatchingName.PMN_OH = org.PK;
			patternMatchingName.PMN_ParentId = org.PK;
			patternMatchingName.PMN_RN_NKCountryCode = "DE";
			patternMatchingName.PMN_ParentTableCode = OrgHeaderSchema.Constants.Prefix;

			var patternMatchingPhone = factory.New<PatternMatchingPhone>();
			patternMatchingPhone.PMP_HashedValue = TextStandardizerHelper.ComputeStringHashFast(org.MainAddress.OA_Phone);
			patternMatchingPhone.PMP_OH = org.PK;
			patternMatchingPhone.PMP_ParentId = org.MainAddress.PK;
			patternMatchingPhone.PMP_RN_NKCountryCode = "DE";
			patternMatchingPhone.PMP_ParentTableCode = OrgAddressSchema.Constants.Prefix;

			factory.SaveForTesting();

			return org;
		}

		void CreatePatternMatchingName(OrgHeader header)
		{
			var patternMatchingName = Factory.New<PatternMatchingName>();
			patternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(header.OH_FullName);
			patternMatchingName.PMN_OH = header.PK;
			patternMatchingName.PMN_ParentId = header.PK;
			patternMatchingName.PMN_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			patternMatchingName.PMN_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
		}

		void CreatePatternMatchingAddress(OrgAddress address, OrgHeader header)
		{
			var addressToHash = address.OA_Address1 + address.OA_City + address.OA_State + address.OA_PostCode;
			var patternMatchingAddress = Factory.New<PatternMatchingAddress>();
			patternMatchingAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(addressToHash);
			patternMatchingAddress.PMA_OH = header.PK;
			patternMatchingAddress.PMA_ParentId = address.PK;
			patternMatchingAddress.PMA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			patternMatchingAddress.PMA_ParentTableCode = OrgAddressSchema.Constants.Prefix;
		}

		#endregion
	}
}
