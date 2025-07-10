using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class RefZoneHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestZoneTypes()
		{
			RefZoneHeaderLookups lookups = GetRefZoneHeader().Lookups;
			AssertEquals(ExpectedZoneTypesListType, lookups.ZoneTypes.GetType());
			Assert(lookups.ZoneTypes.ContainsCode(RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean));
			Assert(lookups.ZoneTypes.ContainsCode(RefZoneHeaderLookups.ZoneTypeCodes.Contract));
			Assert(lookups.ZoneTypes.ContainsCode(RefZoneHeaderLookups.ZoneTypeCodes.Schedules));
			Assert(lookups.ZoneTypes.ContainsCode(RefZoneHeaderLookups.ZoneTypeCodes.HVLVGateway));
		}

		public void TestGetOriginAndDestinationGateway()
		{
			RefZoneHeaderLookups lookups = GetRefZoneHeader().Lookups;
			AssertEquals("Should contain the code for Origin Gateway zone type", true, lookups.ZoneTypes.ContainsCode(ZoneTypeCodeDescriptionPair.OriginGateway.Code));
			AssertEquals("Should contain the code for Destination Gateway zone type", true, lookups.ZoneTypes.ContainsCode(ZoneTypeCodeDescriptionPair.DestinationGateway.Code));
		}

		public void TestGetCarrierList()
		{
			var zone = Factory.New<RefZoneHeader>();
			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.OriginGateway;
			var lookups = zone.Lookups;

			AssertType<ForwarderCollection>(lookups.RelatedParties);

			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;

			AssertType<OrganisationsFindBoxCollection>(lookups.RelatedParties);

			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.DestinationGateway;

			AssertType<ForwarderCollection>(lookups.RelatedParties);
		}

		#region Implementation

		protected virtual RefZoneHeader GetRefZoneHeader()
		{
			return Factory.New<RefZoneHeader>();
		}

		protected virtual Type ExpectedZoneTypesListType
		{
			get { return typeof(ZoneTypeCodePairList); }
		}

		#endregion Implementation
	}
}
