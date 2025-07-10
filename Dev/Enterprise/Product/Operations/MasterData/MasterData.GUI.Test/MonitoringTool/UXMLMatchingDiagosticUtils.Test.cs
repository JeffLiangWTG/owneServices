using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.MasterData.GUI.Tests
{
	public class UXMLMatchingDiagosticUtilsTest : TestCaseWithFactory
	{
		#region GetOrganizationAddressFromXMLString

		public void TestGetOrganizationAddressFromXMLString_EmptyDTO()
		{
			var emptyOrganizationAddressStr = @"
<OrganizationAddress>
	<IsResidential> </IsResidential>
	<RegistrationNumberCollection> </RegistrationNumberCollection>
	<UniversalOfficeCode> </UniversalOfficeCode>
	<UniversalNettingCode> </UniversalNettingCode>
	<State> </State>
	<ScreeningStatus> </ScreeningStatus>
	<Postcode> </Postcode>
	<Phone> </Phone>
	<Mobile> </Mobile>
	<GovRegNumType> </GovRegNumType>
	<GovRegNum> </GovRegNum>
	<Fax> </Fax>
	<Email> </Email>
	<Country> </Country>
	<Port> </Port>
	<Contact> </Contact>
	<CompanyName> </CompanyName>
	<City> </City>
	<AddressOverride> </AddressOverride>
	<Address2> </Address2>
	<Address1> </Address1>
	<AdditionalAddressInformation> </AdditionalAddressInformation>
	<OrganizationCode> </OrganizationCode>
	<AddressShortCode> </AddressShortCode>
	<AddressType> </AddressType>
	<SuppressAddressValidationError> </SuppressAddressValidationError>
	<LocalAddressCollection> </LocalAddressCollection>
</OrganizationAddress>";
			var result = UXMLMatchingDiagnosticUtils.GetOrganizationAddressFromXMLString(emptyOrganizationAddressStr, new XMLMatchingDummyLogger());
			CombineAssertions("The properties should be null or default value", () =>
			{
				AssertEquals(false, result.IsResidential);
				AssertEquals(0, result.RegistrationNumberCollection.Count);
				AssertNullOrEmpty(result.UniversalOfficeCode);
				AssertNullOrEmpty(result.UniversalNettingCode);
				AssertNullOrEmpty(result.State);
				AssertNull(result.ScreeningStatus);
				AssertNullOrEmpty(result.Postcode);
				AssertNullOrEmpty(result.Phone);
				AssertNullOrEmpty(result.Mobile);
				AssertNull(result.GovRegNumType);
				AssertNullOrEmpty(result.GovRegNum);
				AssertNullOrEmpty(result.Fax);
				AssertNullOrEmpty(result.Email);
				AssertNull(result.Country);
				AssertNull(result.Port);
				AssertNullOrEmpty(result.Contact);
				AssertNullOrEmpty(result.CompanyName);
				AssertNullOrEmpty(result.City);
				AssertEquals(false, result.AddressOverride);
				AssertNullOrEmpty(result.Address2);
				AssertNullOrEmpty(result.Address1);
				AssertNullOrEmpty(result.AdditionalAddressInformation);
				AssertNullOrEmpty(result.OrganizationCode);
				AssertNullOrEmpty(result.AddressShortCode);
				AssertNullOrEmpty(result.AddressType);
				AssertEquals(false, result.SuppressAddressValidationError);
				AssertEquals(0, result.LocalAddressCollection.Count);
			});
		}

		#region ValidOrganizationAddressStr

		public const string ValidOrganizationAddressStr = @"
  <OrganizationAddress>
	<AddressType>PST</AddressType>
	<AdditionalAddressInformation>NO.95 ChaoYang BJ</AdditionalAddressInformation>
	<Address1>No.201 PuDong SH</Address1>
	<Address2>No.302 XuanWu NJ</Address2>
	<AddressOverride>true</AddressOverride>
	<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
	<City>NJ</City>
	<CompanyName>China South Co.</CompanyName>
	<Contact>HEATHER A WOODS</Contact>
	<Country>
	  <Code>CN</Code>
	  <Name>China</Name>
	</Country>
	<Email>Mark@hotmail.com</Email>
	<Fax>010 2346 3456</Fax>
	<GovRegNum>T4307</GovRegNum>
	<GovRegNumType>
	  <Code>OCL</Code>
	  <Description>Obssessed Crazy Local</Description>
	</GovRegNumType>
	<IsResidential>true</IsResidential>
	<Mobile>010 2245 8998</Mobile>
	<OrganizationCode>CN</OrganizationCode>
	<Phone>+861023463456</Phone>
	<Port>
	  <Code>TJ</Code>
	  <Name>TianJin</Name>
	</Port>
	<Postcode>212007</Postcode>
	<ScreeningStatus>
	  <Code>OH</Code>
	  <Description>OrgHeader</Description>
	</ScreeningStatus>
	<State>Active</State>
	<SuppressAddressValidationError>true</SuppressAddressValidationError>
	<UniversalNettingCode>AUCOR</UniversalNettingCode>
	<UniversalOfficeCode>7868 2342 234</UniversalOfficeCode>

	<LocalAddressCollection>
	  <LocalAddress>
		<Address1>No.302 XuanWu NJ</Address1>
		<Address2>No.201 PuDong SH</Address2>
		<City>SZ</City>
		<CompanyName>China South Co.</CompanyName>
		<Language>
		  <Code>ZH</Code>
		  <Description>Chinese</Description>
		</Language>
		<Postcode>212009</Postcode>
		<State>GD</State>
	  </LocalAddress>
	</LocalAddressCollection>

	<RegistrationNumberCollection>
	  <RegistrationNumber>
		<Type>
		  <Code>ATF</Code>
		  <Description>Approved Transitional Facility</Description>
		</Type>
		<CountryOfIssue>
		  <Code>US</Code>
		  <Name>America</Name>
		</CountryOfIssue>
		<Value>1234F</Value>
	  </RegistrationNumber>
	  <RegistrationNumber>
		<Type>
		  <Code>GST</Code>
		  <Description>GST Code</Description>
		</Type>
		<CountryOfIssue>
		  <Code>CN</Code>
		  <Name>China</Name>
		</CountryOfIssue>
		<Value>7234</Value>
	  </RegistrationNumber>
	</RegistrationNumberCollection>
  </OrganizationAddress>";

		#endregion

		public void TestGetOrganizationAddressFromXMLString_ValuableDTO()
		{
			var result = UXMLMatchingDiagnosticUtils.GetOrganizationAddressFromXMLString(ValidOrganizationAddressStr, new XMLMatchingDummyLogger());
			CombineAssertions("The properties should be same with the value in XML", () =>
			{
				AssertEquals(true, result.IsResidential);
				AssertEquals(2, result.RegistrationNumberCollection.Count);
				AssertEquals("US", result.RegistrationNumberCollection[0].CountryOfIssue.Code);
				AssertEquals("Approved Transitional Facility", result.RegistrationNumberCollection[0].Type.Description);
				AssertEquals("1234F", result.RegistrationNumberCollection[0].Value);
				AssertEquals("7868 2342 234", result.UniversalOfficeCode);
				AssertEquals("AUCOR", result.UniversalNettingCode);
				AssertEquals("Active", result.State);
				AssertEquals("OH", result.ScreeningStatus.Code);
				AssertEquals("212007", result.Postcode);
				AssertEquals("+861023463456", result.Phone);
				AssertEquals("010 2245 8998", result.Mobile);
				AssertEquals("OCL", result.GovRegNumType.Code);
				AssertEquals("T4307", result.GovRegNum);
				AssertEquals("010 2346 3456", result.Fax);
				AssertEquals("Mark@hotmail.com", result.Email);
				AssertEquals("CN", result.Country.Code);
				AssertEquals("TJ", result.Port.Code);
				AssertEquals("HEATHER A WOODS", result.Contact);
				AssertEquals("China South Co.", result.CompanyName);
				AssertEquals("NJ", result.City);
				AssertEquals(true, result.AddressOverride);
				AssertEquals("No.302 XuanWu NJ", result.Address2);
				AssertEquals("No.201 PuDong SH", result.Address1);
				AssertEquals("NO.95 ChaoYang BJ", result.AdditionalAddressInformation);
				AssertEquals("CN", result.OrganizationCode?.SourceValue);
				AssertEquals("PST: UNIT A1, 4TH FLOOR,", result.AddressShortCode);
				AssertEquals("PST", result.AddressType);
				AssertEquals(true, result.SuppressAddressValidationError);
				AssertEquals(1, result.LocalAddressCollection.Count);
				AssertEquals("SZ", result.LocalAddressCollection[0].City);
				AssertEquals("GD", result.LocalAddressCollection[0].State);
				AssertEquals("ZH", result.LocalAddressCollection[0].Language.Code);
				AssertEquals("212009", result.LocalAddressCollection[0].Postcode);
				AssertEquals("China South Co.", result.LocalAddressCollection[0].CompanyName);
				AssertEquals("No.302 XuanWu NJ", result.LocalAddressCollection[0].Address1);
				AssertEquals("No.201 PuDong SH", result.LocalAddressCollection[0].Address2);
			});
		}

		public void TestGetOrganizationAddressFromXMLString_HasError()
		{
			var errorOrganizationAddressStr = @"
<OrganizationAddress>
	<IsResidential> </IsResidential>
	<RegistrationNumberCollection> ";

			AssertNoExceptionThrown("Throw no exception with half-baked XML", () =>
			{
				var a = UXMLMatchingDiagnosticUtils.GetOrganizationAddressFromXMLString(errorOrganizationAddressStr, new XMLMatchingDummyLogger());
			});

			var errorTypeOrganizationAddressStr = @"
<OrganizationAddress>
	<IsResidential> </IsResidential>
	<RegistrationNumberCollection>false</RegistrationNumberCollection>
	<UniversalOfficeCode> </UniversalOfficeCode>
	<UniversalNettingCode> </UniversalNettingCode>
	<State> </State>
	<ScreeningStatus> </ScreeningStatus>
	<Postcode> </Postcode>
	<Phone> </Phone>
	<Mobile> </Mobile>
	<GovRegNumType> </GovRegNumType>
	<GovRegNum> </GovRegNum>
	<Fax> </Fax>
	<Email> </Email>
	<Country> </Country>
	<Port> </Port>
	<Contact> </Contact>
	<CompanyName> </CompanyName>
	<City> </City>
	<AddressOverride> </AddressOverride>
	<Address2> </Address2>
	<Address1> </Address1>
	<AdditionalAddressInformation> </AdditionalAddressInformation>
	<OrganizationCode> </OrganizationCode>
	<AddressShortCode> </AddressShortCode>
	<AddressType> </AddressType>
	<SuppressAddressValidationError> </SuppressAddressValidationError>
	<LocalAddressCollection> </LocalAddressCollection>
</OrganizationAddress>";

			AssertNoExceptionThrown("Throw no exception with error type", () =>
			{
				UXMLMatchingDiagnosticUtils.GetOrganizationAddressFromXMLString(errorTypeOrganizationAddressStr, new XMLMatchingDummyLogger());
			});
		}

		public void TestGetOrganizationAddressFromXMLString_ContainUnicodeCharacter()
		{
			var xmlString = @"
<OrganizationAddress>
  <AddressType>Fake Type</AddressType>
  <CompanyName>Pho{0}nomenal, Inc</CompanyName>
</OrganizationAddress>";

			var orgAddress = UXMLMatchingDiagnosticUtils.GetOrganizationAddressFromXMLString(string.Format(xmlString, '\u2019'), new XMLMatchingDummyLogger());

			AssertEquals("Pho\u2019nomenal, Inc", orgAddress.CompanyName);
		}

		#endregion

		public void TestGetMatchedOrgAddressLog()
		{
			var logger = new XMLMatchingDummyLogger();
			var result = UXMLMatchingDiagnosticUtils.GetMatchedOrgAddressLog(logger);
			AssertNullOrEmpty(result);

			logger.Log(LogType.Information, "Information Log");
			logger.Log(LogType.Debug, "Debug Log");

			var expectedLogResult = @"Information : Information Log
Debug : Debug Log
";
			result = UXMLMatchingDiagnosticUtils.GetMatchedOrgAddressLog(logger);
			AssertEquals(expectedLogResult, result);
		}

		#region CombineMatchedItemAndScoreResultsToVMs

		public void TestCombineMatchedOrgAndScoreResultsToVMsWithValue()
		{
			using (OrganisationsDataRegistry.Instance.UXMLOrganisationMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 80))
			{
				var matchedAndSelectedOrg = Factory.NewWithValidTestData<OrgHeader>();
				matchedAndSelectedOrg.OH_Code = "SHH";
				var matchedOrg = Factory.NewWithValidTestData<OrgHeader>();
				matchedOrg.OH_Code = "MAM";
				var notMatchedOrg = Factory.NewWithValidTestData<OrgHeader>();
				notMatchedOrg.OH_Code = "LSK";
				Factory.Save();

				var scorings = new List<ScoringResult>()
				{
					new ScoringResult()
					{
						TargetPK = matchedAndSelectedOrg.PK.ToGuid(),
						Score = 0.9
					},
					new ScoringResult()
					{
						TargetPK = matchedOrg.PK.ToGuid(),
						Score = 0.81
					},
					new ScoringResult()
					{
						TargetPK = notMatchedOrg.PK.ToGuid(),
						Score = 0.6
					}
				};

				var result = UXMLMatchingDiagnosticUtils.CombineMatchedOrgAndScoreResultsToVMs(matchedAndSelectedOrg, scorings, Factory);

				CombineAssertions(() =>
				{
					AssertEquals(3, result.Count);
					AssertOrgUXMLModelResult(result[0], $"{matchedAndSelectedOrg.OH_Code}", "90%", UXMLMatchingDiagnosticUtils.Constants.MatchAndSelected, matchedAndSelectedOrg.PK);
					AssertOrgUXMLModelResult(result[1], $"{matchedOrg.OH_Code}", "81%", UXMLMatchingDiagnosticUtils.Constants.Match, matchedOrg.PK);
					AssertOrgUXMLModelResult(result[2], $"{notMatchedOrg.OH_Code}", "60%", UXMLMatchingDiagnosticUtils.Constants.NotMatch, notMatchedOrg.PK);
				});
			}
		}

		public void TestCombineMatchedOrgAndScoreResultsToVMsNullValue()
		{
			using (OrganisationsDataRegistry.Instance.UXMLOrganisationMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 80))
			{
				var scorings = new List<ScoringResult>();
				var result = UXMLMatchingDiagnosticUtils.CombineMatchedOrgAndScoreResultsToVMs(null, null, Factory);
				AssertEquals(0, result.Count);
			}
		}

		public void TestCombineMatchedOrgAndScoreResultsToVMsNullMatchedItem()
		{
			using (OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DeDuplicationMinimumConfidenceRating.Codes.Medium))
			{
				var notMatchedOrg1 = Factory.NewWithValidTestData<OrgHeader>();
				notMatchedOrg1.OH_Code = "MAM";
				var notMatchedOrg2 = Factory.NewWithValidTestData<OrgHeader>();
				notMatchedOrg2.OH_Code = "LSK";
				Factory.Save();

				var scorings = new List<ScoringResult>()
				{
					new ScoringResult()
					{
						TargetPK = notMatchedOrg1.PK.ToGuid(),
						Score = 0.3
					},
					new ScoringResult()
					{
						TargetPK = notMatchedOrg2.PK.ToGuid(),
						Score = 0.6
					}
				};

				var result = UXMLMatchingDiagnosticUtils.CombineMatchedOrgAndScoreResultsToVMs(null, scorings, Factory);
				AssertEquals(2, result.Count);
				AssertEquals(false, result.Any(o => o.Result.Equals(UXMLMatchingDiagnosticUtils.Constants.MatchAndSelected)));
			}
		}

		public void TestCombineMatchedAddressAndScoreResultsToVMsWithValue()
		{
			var orgMatchThreshold = 25;
			using (OrganisationsDataRegistry.Instance.UXMLOrganisationMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, orgMatchThreshold))
			using (OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 80))
			{
				var matchedAndSelectedOrg = Factory.NewWithValidTestData<OrgHeader>();
				matchedAndSelectedOrg.OH_Code = "SHH";
				var matchedOrg = Factory.NewWithValidTestData<OrgHeader>();
				matchedOrg.OH_Code = "MAM";
				var notMatchedOrg1 = Factory.NewWithValidTestData<OrgHeader>();
				notMatchedOrg1.OH_Code = "LSK1";
				var notMatchedOrg2 = Factory.NewWithValidTestData<OrgHeader>();
				notMatchedOrg2.OH_Code = "LSK2";

				var matchedAndSelectedAddress = Factory.NewWithValidTestData<OrgAddress>();
				matchedAndSelectedAddress.OA_OH = matchedAndSelectedOrg.PK;
				matchedAndSelectedAddress.AddressCode = "ADS";

				var matchedAddress = Factory.NewWithValidTestData<OrgAddress>();
				matchedAddress.OA_OH = matchedOrg.PK;
				matchedAddress.AddressCode = "NJ1";

				var notMatchedAddress = Factory.NewWithValidTestData<OrgAddress>();
				notMatchedAddress.OA_OH = notMatchedOrg1.PK;
				notMatchedAddress.AddressCode = "NJ2";

				Factory.Save();

				var scorings = new List<ScoringResult>();
				var score1 = new ScoringResult()
				{
					TargetPK = matchedAndSelectedOrg.PK.ToGuid(),
					Score = 0.9,
				};

				(score1.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 0.9,
						MasterType = typeof(IOrgAddress),
						TargetPK = matchedAndSelectedAddress.PK.ToGuid()
					});

				var score2 = new ScoringResult()
				{
					TargetPK = matchedOrg.PK.ToGuid(),
					Score = 0.81
				};

				(score2.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 0.81,
						MasterType = typeof(IOrgAddress),
						TargetPK = matchedAddress.PK.ToGuid()
					});

				var score3 = new ScoringResult()
				{
					TargetPK = notMatchedOrg1.PK.ToGuid(),
					Score = 0.81
				};

				(score3.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 0.6,
						MasterType = typeof(IOrgAddress),
						TargetPK = notMatchedAddress.PK.ToGuid()
					});

				var score4 = new ScoringResult()
				{
					TargetPK = notMatchedOrg2.PK.ToGuid(),
					Score = 0.6
				};

				scorings.Add(score1);
				scorings.Add(score2);
				scorings.Add(score3);
				scorings.Add(score4);

				var result = UXMLMatchingDiagnosticUtils.CombineMatchedAddressAndScoreResultsToVMs(matchedAndSelectedAddress, scorings, Factory);

				CombineAssertions(() =>
				{
					AssertEquals(4, result.Count);
					AssertAddressUXMLModelResult(result[0], matchedAndSelectedOrg.OH_Code, matchedAndSelectedAddress.OA_Code, "90%", "90%", UXMLMatchingDiagnosticUtils.Constants.MatchAndSelected, matchedAndSelectedOrg.PK);
					AssertAddressUXMLModelResult(result[1], matchedOrg.OH_Code, matchedAddress.OA_Code, "81%", "81%", UXMLMatchingDiagnosticUtils.Constants.Match, matchedOrg.PK);
					AssertAddressUXMLModelResult(result[2], notMatchedOrg1.OH_Code, notMatchedAddress.OA_Code, "81%", "60%", UXMLMatchingDiagnosticUtils.Constants.NotMatch, notMatchedOrg1.PK);
					AssertOrgUXMLModelResult(result[3], notMatchedOrg2.OH_Code, "60%", UXMLMatchingDiagnosticUtils.Constants.NotMatch, notMatchedOrg2.PK);
				});
			}
		}

		public void TestCombineMatchedAddressAndScoreResultsToVMsNullValue()
		{
			using (var form = new UXMLMatchingDiagnosticToolForm())
			using (OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DeDuplicationMinimumConfidenceRating.Codes.Medium))
			{
				var scorings = new List<ScoringResult>();
				var result = UXMLMatchingDiagnosticUtils.CombineMatchedAddressAndScoreResultsToVMs(null, scorings, Factory);
				AssertEquals(0, result.Count);
			}
		}

		public void TestCombineMatchedAddressAndScoreResultsToVMs_MatchedOrgNotInScoringResults_ShouldNotThrowException()
		{
			var orgMatchThreshold = 25;
			using (OrganisationsDataRegistry.Instance.UXMLOrganisationMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, orgMatchThreshold))
			using (OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 80))
			{
				var matchedOrg = Factory.NewWithValidTestData<OrgHeader>();
				matchedOrg.OH_Code = "MAT";

				var notMatchedOrg = Factory.NewWithValidTestData<OrgHeader>();
				notMatchedOrg.OH_Code = "NAN";

				var matchedAddress = Factory.NewWithValidTestData<OrgAddress>();
				matchedAddress.OA_OH = matchedOrg.PK;
				matchedAddress.AddressCode = "NYC";

				var notMatchedAddress = Factory.NewWithValidTestData<OrgAddress>();
				notMatchedAddress.OA_OH = notMatchedOrg.PK;
				notMatchedAddress.AddressCode = "SYD";

				var scorings = new List<ScoringResult>();
				var score = new ScoringResult()
				{
					TargetPK = matchedOrg.PK.ToGuid(),
					Score = 0.81,
				};

				(score.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 0.81,
						MasterType = typeof(IOrgAddress),
						TargetPK = matchedAddress.PK.ToGuid()
					});

				scorings.Add(score);

				List<UXMLMatchingDiagnosticModel> result = default;
				AssertNoExceptionThrown(() =>
				{
					result = UXMLMatchingDiagnosticUtils.CombineMatchedAddressAndScoreResultsToVMs(notMatchedAddress, scorings, Factory);
				});

				AssertEquals(1, result.Count);
				AssertAddressUXMLModelResult(result[0], matchedOrg.OH_Code, matchedAddress.OA_Code, "81%", "81%", UXMLMatchingDiagnosticUtils.Constants.Match, matchedOrg.PK);
			}
		}

		public void TestCombineMatchedAddressAndScoreResultsToVMs_MatchedOrgIsInScoringResults_ShouldNotThrowException()
		{
			var orgMatchThreshold = 25;
			using (OrganisationsDataRegistry.Instance.UXMLOrganisationMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, orgMatchThreshold))
			using (OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 80))
			{
				var matchedAndSelectedOrg = Factory.NewWithValidTestData<OrgHeader>();
				matchedAndSelectedOrg.OH_Code = "MAT";

				var matchedAndSelectedAddress = Factory.NewWithValidTestData<OrgAddress>();
				matchedAndSelectedAddress.OA_OH = matchedAndSelectedOrg.PK;
				matchedAndSelectedAddress.AddressCode = "NYC";

				var scorings = new List<ScoringResult>();
				var score = new ScoringResult()
				{
					TargetPK = matchedAndSelectedOrg.PK.ToGuid(),
					Score = 0.81,
				};

				(score.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 0.81,
						MasterType = typeof(IOrgAddress),
						TargetPK = matchedAndSelectedAddress.PK.ToGuid()
					});

				scorings.Add(score);

				List<UXMLMatchingDiagnosticModel> result = default;
				AssertNoExceptionThrown(() =>
				{
					result = UXMLMatchingDiagnosticUtils.CombineMatchedAddressAndScoreResultsToVMs(matchedAndSelectedAddress, scorings, Factory);
				});
				AssertEquals(1, result.Count);
				AssertAddressUXMLModelResult(result[0], matchedAndSelectedOrg.OH_Code, matchedAndSelectedAddress.OA_Code, "81%", "81%", UXMLMatchingDiagnosticUtils.Constants.MatchAndSelected, matchedAndSelectedOrg.PK);
			}
		}

		public void TestSortMatchedResults_ResultsInDifferentThresholdRegions_OrdersResultsByRegion()
		{
			using (OrganisationsDataRegistry.Instance.UXMLOrganisationMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 50))
			using (OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 80))
			{
				var notMatchedOrgA = Factory.NewWithValidTestData<OrgHeader>();
				var notMatchedOrgB = Factory.NewWithValidTestData<OrgHeader>();
				var matchedAndSelectedOrgC = Factory.NewWithValidTestData<OrgHeader>();
				var notMatchedOrgD = Factory.NewWithValidTestData<OrgHeader>();

				notMatchedOrgA.OH_Code = "orgA";
				notMatchedOrgB.OH_Code = "orgB";
				matchedAndSelectedOrgC.OH_Code = "orgC";
				notMatchedOrgD.OH_Code = "orgD";

				var notMatchedAddressA = Factory.NewWithValidTestData<OrgAddress>();
				var notMatchedAddressB = Factory.NewWithValidTestData<OrgAddress>();
				var matchedAndSelectedAddressC = Factory.NewWithValidTestData<OrgAddress>();
				var notMatchedAddressD = Factory.NewWithValidTestData<OrgAddress>();

				notMatchedAddressA.OA_OH = notMatchedOrgA.PK;
				notMatchedAddressB.OA_OH = notMatchedOrgB.PK;
				matchedAndSelectedAddressC.OA_OH = matchedAndSelectedOrgC.PK;
				notMatchedAddressD.OA_OH = notMatchedOrgD.PK;

				notMatchedAddressA.AddressCode = "addressA";
				notMatchedAddressB.AddressCode = "addressB";
				matchedAndSelectedAddressC.AddressCode = "addressC";
				notMatchedAddressD.AddressCode = "addressD";

				Factory.Save();

				var score1 = new ScoringResult()
				{
					TargetPK = notMatchedOrgA.PK.ToGuid(),
					Score = 0.858,
				};
				(score1.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 0.09,
						MasterType = typeof(IOrgAddress),
						TargetPK = notMatchedAddressA.PK.ToGuid()
					}
				);

				var score2 = new ScoringResult()
				{
					TargetPK = notMatchedOrgB.PK.ToGuid(),
					Score = 0.153
				};
				(score2.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 0.909,
						MasterType = typeof(IOrgAddress),
						TargetPK = notMatchedAddressB.PK.ToGuid()
					});

				var score3 = new ScoringResult()
				{
					TargetPK = matchedAndSelectedOrgC.PK.ToGuid(),
					Score = 0.60
				};
				(score3.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 0.85,
						MasterType = typeof(IOrgAddress),
						TargetPK = matchedAndSelectedAddressC.PK.ToGuid()
					});

				var score4 = new ScoringResult()
				{
					TargetPK = notMatchedOrgD.PK.ToGuid(),
					Score = 0.40
				};
				(score4.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 0.70,
						MasterType = typeof(IOrgAddress),
						TargetPK = notMatchedAddressD.PK.ToGuid()
					});

				var scorings = new[] { score1, score2, score3, score4 };

				var result = UXMLMatchingDiagnosticUtils.CombineMatchedAddressAndScoreResultsToVMs(matchedAndSelectedAddressC, scorings, Factory);

				CombineAssertions(() =>
				{
					AssertEquals(4, result.Count);
					AssertAddressUXMLModelResult(result[0], matchedAndSelectedOrgC.OH_Code, matchedAndSelectedAddressC.OA_Code, "60%", "85%", UXMLMatchingDiagnosticUtils.Constants.MatchAndSelected, matchedAndSelectedOrgC.PK);
					AssertAddressUXMLModelResult(result[1], notMatchedOrgB.OH_Code, notMatchedAddressB.OA_Code, "15.3%", "90.9%", UXMLMatchingDiagnosticUtils.Constants.NotMatch, notMatchedOrgB.PK);
					AssertAddressUXMLModelResult(result[2], notMatchedOrgA.OH_Code, notMatchedAddressA.OA_Code, "85.8%", "9%", UXMLMatchingDiagnosticUtils.Constants.NotMatch, notMatchedOrgA.PK);
					AssertAddressUXMLModelResult(result[3], notMatchedOrgD.OH_Code, notMatchedAddressD.OA_Code, "40%", "70%", UXMLMatchingDiagnosticUtils.Constants.NotMatch, notMatchedOrgD.PK);
				});
			}
		}

		public void TestSortMatchedResults_ResultsInSameThresholdRegion_OrdersByAddressScore()
		{
			using (OrganisationsDataRegistry.Instance.UXMLOrganisationMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 50))
			using (OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 80))
			{
				var notMatchedOrgA = Factory.NewWithValidTestData<OrgHeader>();
				var notMatchedOrgB = Factory.NewWithValidTestData<OrgHeader>();
				var notMatchedOrgC = Factory.NewWithValidTestData<OrgHeader>();

				notMatchedOrgA.OH_Code = "orgA";
				notMatchedOrgB.OH_Code = "orgB";
				notMatchedOrgC.OH_Code = "orgC";

				var notMatchedAddressA = Factory.NewWithValidTestData<OrgAddress>();
				var notMatchedAddressB = Factory.NewWithValidTestData<OrgAddress>();
				var notMatchedAddressC = Factory.NewWithValidTestData<OrgAddress>();

				notMatchedAddressA.OA_OH = notMatchedOrgA.PK;
				notMatchedAddressB.OA_OH = notMatchedOrgB.PK;
				notMatchedAddressC.OA_OH = notMatchedOrgC.PK;

				notMatchedAddressA.AddressCode = "addressA";
				notMatchedAddressB.AddressCode = "addressB";
				notMatchedAddressC.AddressCode = "addressC";

				Factory.Save();

				var score1 = new ScoringResult()
				{
					TargetPK = notMatchedOrgA.PK.ToGuid(),
					Score = 0.4,
				};
				(score1.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 0.859,
						MasterType = typeof(IOrgAddress),
						TargetPK = notMatchedAddressA.PK.ToGuid()
					});

				var score2 = new ScoringResult()
				{
					TargetPK = notMatchedOrgB.PK.ToGuid(),
					Score = 0.3
				};
				(score2.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 1.0,
						MasterType = typeof(IOrgAddress),
						TargetPK = notMatchedAddressB.PK.ToGuid()
					});

				var score3 = new ScoringResult()
				{
					TargetPK = notMatchedOrgC.PK.ToGuid(),
					Score = 0.2
				};
				(score3.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 0.9,
						MasterType = typeof(IOrgAddress),
						TargetPK = notMatchedAddressC.PK.ToGuid()
					});

				var scorings = new[] { score1, score2, score3 };

				var result = UXMLMatchingDiagnosticUtils.CombineMatchedAddressAndScoreResultsToVMs(null, scorings, Factory);

				CombineAssertions(() =>
				{
					AssertEquals(3, result.Count);
					AssertAddressUXMLModelResult(result[0], notMatchedOrgB.OH_Code, notMatchedAddressB.OA_Code, "30%", "100%", UXMLMatchingDiagnosticUtils.Constants.NotMatch, notMatchedOrgB.PK);
					AssertAddressUXMLModelResult(result[1], notMatchedOrgC.OH_Code, notMatchedAddressC.OA_Code, "20%", "90%", UXMLMatchingDiagnosticUtils.Constants.NotMatch, notMatchedOrgC.PK);
					AssertAddressUXMLModelResult(result[2], notMatchedOrgA.OH_Code, notMatchedAddressA.OA_Code, "40%", "85.9%", UXMLMatchingDiagnosticUtils.Constants.NotMatch, notMatchedOrgA.PK);
				});
			}
		}

		public void TestSortMatchedResults_ResultsInSameThresholdRegion_WithSameAddressScore_OrdersResultsByOrgScore()
		{
			using (OrganisationsDataRegistry.Instance.UXMLOrganisationMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 50))
			using (OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 80))
			{
				var notMatchedOrgA = Factory.NewWithValidTestData<OrgHeader>();
				var notMatchedOrgB = Factory.NewWithValidTestData<OrgHeader>();
				var matchedAndSelectedOrgC = Factory.NewWithValidTestData<OrgHeader>();

				notMatchedOrgA.OH_Code = "orgA";
				notMatchedOrgB.OH_Code = "orgB";
				matchedAndSelectedOrgC.OH_Code = "orgC";

				var notMatchedAddressA = Factory.NewWithValidTestData<OrgAddress>();
				var notMatchedAddressB = Factory.NewWithValidTestData<OrgAddress>();
				var matchedAndSelectedAddressC = Factory.NewWithValidTestData<OrgAddress>();

				notMatchedAddressA.OA_OH = notMatchedOrgA.PK;
				notMatchedAddressB.OA_OH = notMatchedOrgB.PK;
				matchedAndSelectedAddressC.OA_OH = matchedAndSelectedOrgC.PK;

				notMatchedAddressA.AddressCode = "addressA";
				notMatchedAddressB.AddressCode = "addressB";
				matchedAndSelectedAddressC.AddressCode = "addressC";

				Factory.Save();

				var score1 = new ScoringResult()
				{
					TargetPK = notMatchedOrgA.PK.ToGuid(),
					Score = 0.808,
				};
				(score1.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 0.9,
						MasterType = typeof(IOrgAddress),
						TargetPK = notMatchedAddressA.PK.ToGuid()
					});

				var score2 = new ScoringResult()
				{
					TargetPK = notMatchedOrgB.PK.ToGuid(),
					Score = 0.7
				};
				(score2.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 0.9,
						MasterType = typeof(IOrgAddress),
						TargetPK = notMatchedAddressB.PK.ToGuid()
					});

				var score3 = new ScoringResult()
				{
					TargetPK = matchedAndSelectedOrgC.PK.ToGuid(),
					Score = 1
				};
				(score3.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 0.9,
						MasterType = typeof(IOrgAddress),
						TargetPK = matchedAndSelectedAddressC.PK.ToGuid()
					});

				var scorings = new[] { score1, score2, score3 };

				var result = UXMLMatchingDiagnosticUtils.CombineMatchedAddressAndScoreResultsToVMs(matchedAndSelectedAddressC, scorings, Factory);

				CombineAssertions(() =>
				{
					AssertEquals(3, result.Count);
					AssertAddressUXMLModelResult(result[0], matchedAndSelectedOrgC.OH_Code, matchedAndSelectedAddressC.OA_Code, "100%", "90%", UXMLMatchingDiagnosticUtils.Constants.MatchAndSelected, matchedAndSelectedOrgC.PK);
					AssertAddressUXMLModelResult(result[1], notMatchedOrgA.OH_Code, notMatchedAddressA.OA_Code, "80.8%", "90%", UXMLMatchingDiagnosticUtils.Constants.Match, notMatchedOrgA.PK);
					AssertAddressUXMLModelResult(result[2], notMatchedOrgB.OH_Code, notMatchedAddressB.OA_Code, "70%", "90%", UXMLMatchingDiagnosticUtils.Constants.Match, notMatchedOrgB.PK);
				});
			}
		}

		public void TestSortMatchedResults_ResultsInSameThresholdRegion_WithSameAddressAndOrgScore_OrdersResultsAlphabetically()
		{
			using (OrganisationsDataRegistry.Instance.UXMLOrganisationMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 50))
			using (OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 80))
			{
				var matchedOrg_Aa = Factory.NewWithValidTestData<OrgHeader>();
				var matchedOrg_abc = Factory.NewWithValidTestData<OrgHeader>();
				var matchedAndSelectedOrg_a = Factory.NewWithValidTestData<OrgHeader>();
				var matchedOrg_b = Factory.NewWithValidTestData<OrgHeader>();

				matchedOrg_Aa.OH_Code = "org_Aa";
				matchedOrg_abc.OH_Code = "org_abc";
				matchedAndSelectedOrg_a.OH_Code = "org_a";
				matchedOrg_b.OH_Code = "org_b";

				var matchedAddress_Aa = Factory.NewWithValidTestData<OrgAddress>();
				var matchedAddress_abc = Factory.NewWithValidTestData<OrgAddress>();
				var matchedAndSelectedAddress_a = Factory.NewWithValidTestData<OrgAddress>();
				var matchedAddress_b = Factory.NewWithValidTestData<OrgAddress>();

				matchedAddress_Aa.OA_OH = matchedOrg_Aa.PK;
				matchedAddress_abc.OA_OH = matchedOrg_abc.PK;
				matchedAndSelectedAddress_a.OA_OH = matchedAndSelectedOrg_a.PK;
				matchedAddress_b.OA_OH = matchedOrg_b.PK;

				matchedAddress_Aa.AddressCode = "address_Aa";
				matchedAddress_abc.AddressCode = "address_abc";
				matchedAndSelectedAddress_a.AddressCode = "address_a";
				matchedAddress_b.AddressCode = "address_b";

				Factory.Save();

				var score1 = new ScoringResult()
				{
					TargetPK = matchedOrg_Aa.PK.ToGuid(),
					Score = 0.7,
				};
				(score1.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 0.9,
						MasterType = typeof(IOrgAddress),
						TargetPK = matchedAddress_Aa.PK.ToGuid()
					});

				var score2 = new ScoringResult()
				{
					TargetPK = matchedOrg_abc.PK.ToGuid(),
					Score = 0.7
				};
				(score2.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 0.9,
						MasterType = typeof(IOrgAddress),
						TargetPK = matchedAddress_abc.PK.ToGuid()
					});

				var score3 = new ScoringResult()
				{
					TargetPK = matchedAndSelectedOrg_a.PK.ToGuid(),
					Score = 0.7
				};
				(score3.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 0.9,
						MasterType = typeof(IOrgAddress),
						TargetPK = matchedAndSelectedAddress_a.PK.ToGuid()
					});

				var score4 = new ScoringResult()
				{
					TargetPK = matchedOrg_b.PK.ToGuid(),
					Score = 0.7
				};
				(score4.ChildResults as List<ScoringResult>).Add(
					new ScoringResult()
					{
						Score = 0.9,
						MasterType = typeof(IOrgAddress),
						TargetPK = matchedAddress_b.PK.ToGuid()
					});

				var scorings = new[] { score1, score2, score3, score4 };

				var result = UXMLMatchingDiagnosticUtils.CombineMatchedAddressAndScoreResultsToVMs(matchedAndSelectedAddress_a, scorings, Factory);

				CombineAssertions(() =>
				{
					AssertEquals(4, result.Count);
					AssertAddressUXMLModelResult(result[0], matchedAndSelectedOrg_a.OH_Code, matchedAndSelectedAddress_a.OA_Code, "70%", "90%", UXMLMatchingDiagnosticUtils.Constants.MatchAndSelected, matchedAndSelectedOrg_a.PK);
					AssertAddressUXMLModelResult(result[1], matchedOrg_Aa.OH_Code, matchedAddress_Aa.OA_Code, "70%", "90%", UXMLMatchingDiagnosticUtils.Constants.Match, matchedOrg_Aa.PK);
					AssertAddressUXMLModelResult(result[2], matchedOrg_abc.OH_Code, matchedAddress_abc.OA_Code, "70%", "90%", UXMLMatchingDiagnosticUtils.Constants.Match, matchedOrg_abc.PK);
					AssertAddressUXMLModelResult(result[3], matchedOrg_b.OH_Code, matchedAddress_b.OA_Code, "70%", "90%", UXMLMatchingDiagnosticUtils.Constants.Match, matchedOrg_b.PK);
				});
			}
		}
		#endregion

		#region DummyLogger

		public void TestDummyLogger_IsSameSystem()
		{
			var loggerDifferentSystem = new XMLMatchingDummyLogger();
			var loggerSameSystem = new XMLMatchingDummyLogger(isSameSystem: true);

			CombineAssertions(() =>
			{
				AssertEquals(false, loggerDifferentSystem.TopLevelDataContext.IsFromSameSystem());
				AssertEquals(true, loggerSameSystem.TopLevelDataContext.IsFromSameSystem());
			});
		}

		#endregion

		#region Implementation

		void AssertAddressUXMLModelResult(UXMLMatchingDiagnosticModel model, string orgCode, string addressCode, string orgScore, string addressScore, string result, ZGuid orgPK)
		{
			AssertEquals(orgCode, model.MatchedOrgCode);
			AssertEquals(addressCode, model.MatchedAddressCode);
			AssertEquals(orgScore, model.OrgScore);
			AssertEquals(addressScore, model.AddressScore);
			AssertEquals(result, model.Result);
			AssertEquals(orgPK, model.OrgPK);
		}

		void AssertOrgUXMLModelResult(UXMLMatchingDiagnosticModel model, string orgCode, string orgScore, string result, ZGuid orgPK)
		{
			AssertEquals(orgCode, model.MatchedOrgCode);
			AssertEquals(orgScore, model.OrgScore);
			AssertEquals(result, model.Result);
			AssertEquals(orgPK, model.OrgPK);
		}

		#endregion
	}
}
