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
	[TestedType(typeof(HVLVDpsMatchCollection))]
	sealed class HVLVDpsMatchCollectionTest : NonPersistentBusinessObjectCollectionTestCase<HVLVDpsMatchCollection>
	{
		protected override HVLVDpsMatchCollection GetCollectionToTest() => new HVLVDpsMatchCollection(new List<DpsResponseWithScreeningParty>(), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "TestCode";
			header.OH_Code = "TestOrgCode";
			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

			var party = new ScreeningParty(header, "EFG", header);
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { ProfileNotes = Compressor.Zip("Test"), TypeOfEntity = ScreeningNameTypes.Person },
			};
			var dpsResponse = new DpsResponse { Profiles = profiles };

			var responseWithParty = new DpsResponseWithScreeningParty(party, dpsResponse, new DpsRequestHeaderWithAddressMatching());
			return new HVLVDpsMatch(responseWithParty, Factory);
		}
	}
}

