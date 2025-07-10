using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.eTail.Business.DeniedPartyScreening.Testing
{
	[TestedType(typeof(HVLVDpsResult))]
	public class HVLVDpsResultTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new HVLVDpsResult(new List<DpsResponseWithScreeningParty>(), Factory);
		}

		public void TestConstructor()
		{
			var hvlvDpsResult = new HVLVDpsResult(DPSResponseWithScreeningParties, Factory);

			CombineAssertions(() =>
			{
				AssertNotNull(hvlvDpsResult.ResponseWithScreeningParties);
				AssertEquals(4, hvlvDpsResult.ScreenedPartiesCount);
			});
		}

		public void TestConstructor_ArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new HVLVDpsResult(null, Factory));
			AssertExceptionThrown<ArgumentNullException>(() => _ = new HVLVDpsResult(new List<DpsResponseWithScreeningParty>(), null));
		}

		public void TestCalculateCount()
		{
			var hvlvDpsResult = new HVLVDpsResult(DPSResponseWithScreeningParties, Factory);

			CombineAssertions(() =>
			{
				AssertEquals(4, hvlvDpsResult.ScreenedPartiesCount);
				AssertEquals(1, hvlvDpsResult.ClearPartiesCount);
				AssertEquals(1, hvlvDpsResult.MatchedPartiesCount);
				AssertEquals(1, hvlvDpsResult.NotScreenedPartiesCount);
				AssertEquals(1, hvlvDpsResult.UnknownPartiesCount);
			});
		}

		public void TestUpdateCount()
		{
			var hvlvDpsResult = new HVLVDpsResult(DPSResponseWithScreeningParties, Factory);

			hvlvDpsResult.UpdateCount(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Matched);

			CombineAssertions(() =>
			{
				AssertEquals(0, hvlvDpsResult.ClearPartiesCount);
				AssertEquals(2, hvlvDpsResult.MatchedPartiesCount);
			});
		}

		public void TestRefreshCountPropertyInfo()
		{
			var hvlvDpsResult = new HVLVDpsResult(DPSResponseWithScreeningParties, Factory);

			AssertNoExceptionThrown(() => hvlvDpsResult.RefreshCountPropertyInfo());
		}

		public void TestShowHasMatchesOnly()
		{
			var hvlvDpsResult = new HVLVDpsResult(DPSResponseWithScreeningParties, Factory);
			hvlvDpsResult.ShowHasMatchesOnly = true;

			Assert(hvlvDpsResult.ShowHasMatchesOnly);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header1 = CreateOrgHeader("testName1", "testCode1", ScreeningStatusesList.Codes.Clear);
			var header2 = CreateOrgHeader("testName2", "testCode2", ScreeningStatusesList.Codes.Matched);
			var header3 = CreateOrgHeader("testName3", "testCode3", ScreeningStatusesList.Codes.Unknown);
			var header4 = CreateOrgHeader("testName4", "testCode4", ScreeningStatusesList.Codes.NotScreened);

			var party1 = new ScreeningParty(header1, "EFG", header1);
			var party2 = new ScreeningParty(header2, "EFG", header2);
			var party3 = new ScreeningParty(header3, "EFG", header3);
			var party4 = new ScreeningParty(header4, "EFG", header4);

			DPSResponseWithScreeningParties = new List<DpsResponseWithScreeningParty>
			{
				CreateDpsResponse(party1),
				CreateDpsResponse(party2),
				CreateDpsResponse(party3),
				CreateDpsResponse(party4)
			};
		}

		DpsResponseWithScreeningParty CreateDpsResponse(ScreeningParty party)
		{
			var sourceListCode = "SourceList1";
			var headerPk = Guid.NewGuid();
			var headerNamePk = Guid.NewGuid();
			return new DpsResponseWithScreeningParty(party, new DpsResponse
			{
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
				RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>()
			}, new DpsRequestHeaderWithAddressMatching());
		}

		OrgHeader CreateOrgHeader(string fullName, string code, string status)
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = fullName;
			header.OH_Code = code;
			header.OH_ScreeningStatus = status;
			return header;
		}

		List<DpsResponseWithScreeningParty> DPSResponseWithScreeningParties;
	}
}

