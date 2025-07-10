using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;
using static Enterprise.DeniedPartyScreening.Common.DeniedPartyConstants;
using static Enterprise.eTail.Business.DeniedPartyScreening.ProfileHeader;

namespace Enterprise.eTail.Business.DeniedPartyScreening.Testing
{
	[TestedType(typeof(ProfileHeader))]
	public class ProfileHeaderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ProfileHeader(new ProfileHeaderInfo(), new DpsResponse(), Factory);
		}

		public void TestConstructor()
		{
			var dpsResponse = new DpsResponse();
			var profileHeaderInfo = new ProfileHeaderInfo { SourceProfileID = sourceProfileID, ProfileNotes = Compressor.Zip("Test"), TypeOfEntity = ScreeningNameTypes.Person };
			var header = new ProfileHeader(profileHeaderInfo, dpsResponse, Factory);

			CombineAssertions(() =>
			{
				AssertNotNull(header);
				AssertEquals(0, header.NameMatchCollection.Count);
				AssertEquals(0, header.CountryMatchCollection.Count);
				AssertEquals(0, header.AddressMatchCollection.Count);
				AssertEquals(0, header.RegistrationMatchCollection.Count);
			});

			dpsResponse.NameMatches = new NameMatchInfo[] { new NameMatchInfo { MatchingStandardizedValue = "Test1", SourceProfileID = sourceProfileID } };
			dpsResponse.AddressMatches = new AddressMatchInfo[] { new AddressMatchInfo { MatchingStandardizedValue = "Test2", SourceProfileID = sourceProfileID } };
			dpsResponse.CountryMatches = new CountryMatchInfo[] { new CountryMatchInfo { MatchingStandardizedValue = "Test3", SourceProfileID = sourceProfileID } };
			dpsResponse.RegistrationCodeMatches = new RegistrationCodeMatchInfo[] { new RegistrationCodeMatchInfo { MatchingStandardizedValue = "Test4", SourceProfileID = sourceProfileID } };
			profileHeaderInfo.ProfileNames = new ProfileNameInfo[] { new ProfileNameInfo { FullName = "Test5" } };
			profileHeaderInfo.ProfileAddresses = new ProfileAddressInfo[] { new ProfileAddressInfo { ID = Guid.NewGuid(), Street = "软件大道", City = "天安数码城", StateProvince = "A", Country = "B", PostCode = "C", Language = "ZH-CN", SourceProfileID = sourceProfileID } };

			header = new ProfileHeader(profileHeaderInfo, dpsResponse, Factory);

			CombineAssertions(() =>
			{
				AssertNotNull(header);
				AssertEquals(1, header.NameMatchCollection.Count);
				AssertEquals("Test5", header.NameMatchCollection[0].Name);
				AssertEquals(1, header.AddressMatchCollection.Count);
				AssertEquals("BCCA天安数码城软件大道", header.AddressMatchCollection[0].Name);
				AssertEquals(0, header.CountryMatchCollection.Count);
				AssertEquals(0, header.RegistrationMatchCollection.Count);
			});
		}

		public void TestConstructor_ArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new ProfileHeader(null, new DpsResponse(), Factory));
			AssertExceptionThrown<ArgumentNullException>(() => _ = new ProfileHeader(new ProfileHeaderInfo(), null, Factory));
		}

		public void TestProperties()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Test5", header.Name);
				AssertEquals(80, header.Score);

				using (OrganisationsDataRegistry.Instance.MatchingConfidenceThresholdsForOrganisations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DpsConfidenceThresholdsBusinessObject(65, 90)))
				{
					AssertEquals("Medium Risk - Review Required", header.Title);
					AssertEquals(RiskLevel.Medium, header.Level);
					AssertEquals("Medium", header.LevelOfRisk);
					Assert(header.ShowTitle);
					AssertEquals("PER", header.Type);
					AssertEquals(4, header.Notes.Length);
				}
			});
		}

		public void TestName()
		{
			AssertEquals("Test5", header.Name);

			profileHeaderInfo.ProfileNames = new ProfileNameInfo[] { new() { FullName = "Test5" }, new() { FullName = "Test6", IsPrimaryName = true } };
			header = new ProfileHeader(profileHeaderInfo, new DpsResponse(), Factory);
			AssertEquals("Test6", header.Name);

			profileHeaderInfo.ProfileNames = Array.Empty<ProfileNameInfo>();
			header = new ProfileHeader(profileHeaderInfo, new DpsResponse(), Factory);
			AssertEquals("Unknown", header.Name);
		}

		public void TestSourceListNames()
		{
			var complianceListItem1 = Factory.NewWithValidTestData<RefComplianceList>();
			complianceListItem1.RCL_ListCode = "SourceList1";
			var complianceListItem2 = Factory.NewWithValidTestData<RefComplianceList>();
			complianceListItem2.RCL_ListCode = "SourceList2";
			var complianceListItem3 = Factory.NewWithValidTestData<RefComplianceList>();
			complianceListItem3.RCL_ListCode = "SourceList3";
			Factory.Save();

			var dpsResponse = new DpsResponse();
			var profileHeaderInfo = new ProfileHeaderInfo
			{
				SourceProfileID = sourceProfileID,
				TypeOfEntity = ScreeningNameTypes.Person,
				SourceListCodes = new List<string>() { "SourceList1", "SourceList2" }
			};

			var header = new ProfileHeader(profileHeaderInfo, dpsResponse, Factory);
			AssertEquals(2, header.SourceListNames.Count);
			AssertCollectionNotContains(header.SourceListNames, x => x.RCL_ListCode == "SourceList3");
		}

		public void TestIsExcludedAndIsValid()
		{
			var complianceList1 = Factory.NewWithValidTestData<RefComplianceList>();
			complianceList1.RCL_ListCode = "SourceList1";
			complianceList1.RCL_IsExcluded = true;
			var complianceList2 = Factory.NewWithValidTestData<RefComplianceList>();
			complianceList2.RCL_ListCode = "SourceList2";
			complianceList2.RCL_IsExcluded = false;
			Factory.Save();

			var profileHeaderInfo = new ProfileHeaderInfo
			{
				SourceProfileID = Guid.NewGuid(),
				ProfileNotes = Array.Empty<byte>(),
				ProfileNames = new List<ProfileNameInfo>(),
				ProfileAddresses = new List<ProfileAddressInfo>(),
				ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
				ProfileCountries = new List<ProfileCountryInfo>(),
				SourceListCodes = new List<string>()
				{
					"SourceList1",
					"SourceList2",
				},
				TypeOfEntity = "PER"
			};

			DpsResponse response = new DpsResponse
			{
				AddressMatches = new List<AddressMatchInfo>(),
				NameMatches = new List<NameMatchInfo>(),
				RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>(),
				CountryMatches = new List<CountryMatchInfo>(),
				Profiles = new List<ProfileHeaderInfo>() { profileHeaderInfo },
			};
			var model = new ProfileHeader(profileHeaderInfo, response, Factory);
			AssertEquals("Execute IsExcluded for the first time.", false, model.IsExcluded);
			AssertEquals("Execute IsValid for the first time.", true, model.IsValid);

			profileHeaderInfo = new ProfileHeaderInfo
			{
				SourceProfileID = Guid.NewGuid(),
				ProfileNotes = Array.Empty<byte>(),
				ProfileNames = new List<ProfileNameInfo>(),
				ProfileAddresses = new List<ProfileAddressInfo>(),
				ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
				ProfileCountries = new List<ProfileCountryInfo>(),
				SourceListCodes = new List<string>()
				{
					"SourceList1",
				},
				TypeOfEntity = "PER"
			};

			model = new ProfileHeader(profileHeaderInfo, response, Factory);
			AssertEquals("Execute IsExcluded for the second time.", true, model.IsExcluded);
			AssertEquals("Execute IsValid for the second time.", true, model.IsValid);

			profileHeaderInfo = new ProfileHeaderInfo
			{
				SourceProfileID = Guid.NewGuid(),
				ProfileNotes = Array.Empty<byte>(),
				ProfileNames = new List<ProfileNameInfo>(),
				ProfileAddresses = new List<ProfileAddressInfo>(),
				ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
				ProfileCountries = new List<ProfileCountryInfo>(),
				SourceListCodes = new List<string>()
				{
					"UnKnownList",
				},
				TypeOfEntity = "PER"
			};

			model = new ProfileHeader(profileHeaderInfo, response, Factory);
			AssertEquals("Execute IsExcluded for the third time.", false, model.IsExcluded);
			AssertEquals("Execute IsValid for the third time.", true, model.IsValid);

			profileHeaderInfo = new ProfileHeaderInfo
			{
				SourceProfileID = Guid.NewGuid(),
				ProfileNotes = Array.Empty<byte>(),
				ProfileNames = new List<ProfileNameInfo>(),
				ProfileAddresses = new List<ProfileAddressInfo>(),
				ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
				ProfileCountries = new List<ProfileCountryInfo>(),
				SourceListCodes = new List<string>(),
				TypeOfEntity = "PER"
			};

			model = new ProfileHeader(profileHeaderInfo, response, Factory);
			AssertEquals("Execute IsExcluded for the fourth time.", true, model.IsExcluded);
			AssertEquals("Execute IsExcluded for the fourth time.", false, model.IsValid);
		}

		protected override void SetUp()
		{
			base.SetUp();
			sourceProfileID = Guid.NewGuid();
			var dpsResponse = new DpsResponse();
			profileHeaderInfo = new ProfileHeaderInfo { SourceProfileID = sourceProfileID, ProfileNotes = Compressor.Zip("Test"), TypeOfEntity = ScreeningNameTypes.Person };

			dpsResponse.NameMatches = new NameMatchInfo[] { new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "ORG", FullName = "Test Full Name" }, MatchingStandardizedValue = "Test1", MatchingNameScore = 80, SourceProfileID = sourceProfileID } };
			dpsResponse.AddressMatches = new AddressMatchInfo[] { new AddressMatchInfo { MatchingStandardizedValue = "Test2", MatchingAddressScore = 90, SourceProfileID = sourceProfileID } };
			dpsResponse.CountryMatches = new CountryMatchInfo[] { new CountryMatchInfo { MatchingStandardizedValue = "Test3", MatchingCountryScore = 60, SourceProfileID = sourceProfileID } };
			dpsResponse.RegistrationCodeMatches = new RegistrationCodeMatchInfo[] { new RegistrationCodeMatchInfo { MatchingStandardizedValue = "Test4", MatchingRegistrationCodeScore = 50, SourceProfileID = sourceProfileID } };
			profileHeaderInfo.ProfileNames = new ProfileNameInfo[] { new ProfileNameInfo { FullName = "Test5" } };
			header = new ProfileHeader(profileHeaderInfo, dpsResponse, Factory);
		}

		Guid sourceProfileID;
		ProfileHeader header;
		ProfileHeaderInfo profileHeaderInfo;
	}
}

