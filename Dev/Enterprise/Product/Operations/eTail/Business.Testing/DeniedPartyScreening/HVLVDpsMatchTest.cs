using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.DeniedPartyScreening.Common.DeniedPartyConstants;

namespace Enterprise.eTail.Business.DeniedPartyScreening.Testing
{
	[TestedType(typeof(HVLVDpsMatch))]
	public class HVLVDpsMatchTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new HVLVDpsMatch(responseWithParty, Factory);
		}

		public void TestConstructor()
		{
			var hvlvDpsMatch = new HVLVDpsMatch(responseWithParty, Factory);

			AssertNotNull(hvlvDpsMatch.ProfileHeaderCollection);
			AssertNotNull(hvlvDpsMatch.DpsResponseWithScreeningParty);
		}

		public void TestProperties()
		{
			var hvlvDpsMatch = new HVLVDpsMatch(responseWithParty, Factory);

			AssertEquals("TestCode", hvlvDpsMatch.PartyName);
			AssertEquals("TestOrgCode", hvlvDpsMatch.PartyCode);
			AssertEquals("TestOrgCode: EFG", hvlvDpsMatch.Description);
			AssertEquals(0, hvlvDpsMatch.PotentialMatchesCount);
		}

		public void TestStatus()
		{
			var hvlvDpsMatch = new HVLVDpsMatch(responseWithParty, Factory);

			AssertEquals(ScreeningStatusesList.Codes.NotScreened, hvlvDpsMatch.Status);
			AssertEquals(ScreeningStatusesList.Descriptions.NotScreened, hvlvDpsMatch.StatusDescription);

			hvlvDpsMatch.Cleared = true;
			AssertEquals(ScreeningStatusesList.Codes.Clear, hvlvDpsMatch.Status);
			AssertEquals(ScreeningStatusesList.Descriptions.Clear, hvlvDpsMatch.StatusDescription);

			hvlvDpsMatch.Matched = true;
			AssertEquals(ScreeningStatusesList.Codes.Matched, hvlvDpsMatch.Status);
			AssertEquals(ScreeningStatusesList.Descriptions.Matched, hvlvDpsMatch.StatusDescription);
		}

		public void TestHighestConfidenceScore()
		{
			var sourceListCode = "SourceList1";
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var headerPk = Guid.NewGuid();
			var headerNamePk = Guid.NewGuid();
			var responseWithParty = new DpsResponseWithScreeningParty(new ScreeningParty(header, string.Empty, header), new DpsResponse
			{
				ResponseCode = DpsResponseCode.Successful,
				ExtraMessage = "For test1",
				Profiles = new List<ProfileHeaderInfo>()
				{
					new ProfileHeaderInfo { SourceProfileID = headerPk, ProfileNotes = Array.Empty<byte>(), ProfileNames = new List<ProfileNameInfo>() {
						new ProfileNameInfo { ID = headerNamePk, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = headerPk } },
						SourceListCodes = new List<string> { sourceListCode }, TypeOfEntity = "PER" },
				},
				NameMatches = new List<NameMatchInfo>()
				{
					new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test name" },
						MatchingNameID = headerNamePk, MatchingNameScore = 90, SourceProfileID = headerPk },
				},
				AddressMatches = new List<AddressMatchInfo>()
				{
					new AddressMatchInfo { MatchingStandardizedValue = "Test address", MatchingAddressScore = 95, SourceProfileID = headerPk },
				},
				RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>()
			}, new DpsRequestHeaderWithAddressMatching());
			var dpsMatch = new HVLVDpsMatch(responseWithParty, Factory);

			AssertEquals(90, dpsMatch.HighestConfidenceScore);
		}

		public void TestCountryCodeForOrgHeader()
		{
			var hvlvDpsMatch = new HVLVDpsMatch(responseWithParty, Factory);
			AssertNullOrEmpty(hvlvDpsMatch.CountryCode);

			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_RL_NKClosestPort = "USSYD";

			var party = new ScreeningParty(header, "EFG", header);
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = sourceProfileID, ProfileNotes = Compressor.Zip("Test"), TypeOfEntity = ScreeningNameTypes.Organization },
			};
			var dpsResponse = new DpsResponse { Profiles = profiles };
			responseWithParty = new DpsResponseWithScreeningParty(party, dpsResponse, new DpsRequestHeaderWithAddressMatching());
			hvlvDpsMatch = new HVLVDpsMatch(responseWithParty, Factory);

			AssertEquals("US", hvlvDpsMatch.CountryCode);
		}

		public void TestCountryCodeForNaturalPerson()
		{
			var hvlvDpsMatch = new HVLVDpsMatch(responseWithParty, Factory);
			AssertNullOrEmpty(hvlvDpsMatch.CountryCode);

			var parent = Factory.NewWithValidTestData<OrgHeader>();

			var party = new ScreeningParty(parent, "Person", "Good King Moggle Mog XII", "Address 1", "Address 2", "City", "Postcode", "State", "Country", "Additional Address Line");
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), TypeOfEntity = ScreeningNameTypes.NaturalPerson },
			};
			var dpsResponse = new DpsResponse { Profiles = profiles };
			responseWithParty = new DpsResponseWithScreeningParty(party, dpsResponse, new DpsRequestHeaderWithAddressMatching());
			hvlvDpsMatch = new HVLVDpsMatch(responseWithParty, Factory);

			AssertEquals("Country", hvlvDpsMatch.CountryCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			sourceProfileID = Guid.NewGuid();

			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "TestCode";
			header.OH_Code = "TestOrgCode";
			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

			var party = new ScreeningParty(header, "EFG", header);
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = sourceProfileID, ProfileNotes = Compressor.Zip("Test"), TypeOfEntity = ScreeningNameTypes.Person },
			};
			var dpsResponse = new DpsResponse { Profiles = profiles };

			responseWithParty = new DpsResponseWithScreeningParty(party, dpsResponse, new DpsRequestHeaderWithAddressMatching());
		}

		DpsResponseWithScreeningParty responseWithParty;
		Guid sourceProfileID;
	}
}

