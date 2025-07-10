using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgRelatedPartiesModuleFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidatePartyType()
		{
			var filter = GetNewFilter();
			filter.PartyType = RelatedPartyTypeList.Codes.ClientCFS;
			AssertNoErrors(filter.PartyTypeInfo);

			filter.PartyType = "XXX";
			AssertHasErrors(filter.PartyTypeInfo);
		}

		public void TestValidateTransportMode()
		{
			var filter = GetNewFilter();
			filter.TransportMode = Core.Constants.TransportModes.Sea;
			AssertNoErrors(filter.TransportModeInfo);

			filter.TransportMode = "XXX";
			AssertHasErrors(filter.TransportModeInfo);
		}

		public void TestValidateContainerMode()
		{
			var filter = GetNewFilter();
			filter.ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertNoErrors(filter.ContainerModeInfo);

			filter.ContainerMode = "XXX";
			AssertHasErrors(filter.ContainerModeInfo);
		}

		public void TestValidateDirection()
		{
			var filter = GetNewFilter();
			filter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			AssertNoErrors(filter.DirectionInfo);

			filter.Direction = "XXX";
			AssertHasErrors(filter.DirectionInfo);
		}

		public void TestValidateRelatedParty()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var filter = GetNewFilter();
			filter.RelatedParty = orgHeader.PK;
			AssertNoErrors(filter.RelatedPartyInfo);

			filter.RelatedParty = ZGuid.Invalid;
			AssertHasErrors(filter.RelatedPartyInfo);
		}

		OrgRelatedPartiesModuleFilter GetNewFilter()
		{
			return new OrgRelatedPartiesModuleFilter("Test");
		}
	}
}
