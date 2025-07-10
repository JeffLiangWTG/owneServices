using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.eTail.Business.DeniedPartyScreening.Testing
{
	[TestedType(typeof(HVLVDpsMatchCollectionView))]
	sealed class HVLVDpsMatchCollectionViewTest : NonPersistentBusinessObjectCollectionViewTestCase<HVLVDpsMatchCollectionView>
	{
		protected override HVLVDpsMatchCollectionView GetCollectionToTest() => new HVLVDpsMatchCollectionView(new HVLVDpsMatchCollection(new List<DpsResponseWithScreeningParty>(), Factory));

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var sourceListCode = "SourceList1";
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var headerPk = Guid.NewGuid();
			var headerNamePk = Guid.NewGuid();
			var responseWithParty = new DpsResponseWithScreeningParty(new ScreeningParty(header, string.Empty, header), new DpsResponse
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
			return new HVLVDpsMatch(responseWithParty, Factory);
		}
	}
}

