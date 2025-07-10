using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class BaseJobSailingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCollections()
		{
			JobSailing sailing = Factory.New<JobSailing>();
			AssertNotNull(sailing.Lookups.Carrier_List);
			AssertNotNull(sailing.Lookups.CTO_List);
			AssertNotNull(sailing.Lookups.Ports);
		}

		public void TestCarrier_List()
		{
			void AsssertCarrierList(string transportMode, Type expectedCarrierListType)
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
				voyage.GenerateSailings();
				var sailing = voyage.Sailings[0];

				voyage.JV_AirSeaRoad = transportMode;
				AssertEquals($"Transport Mode: {transportMode}", expectedCarrierListType, sailing.Lookups.Carrier_List.GetType());
			}

			AsssertCarrierList(Core.Constants.TransportModes.Air, typeof(AirShippingProviderCollection));
			AsssertCarrierList(Core.Constants.TransportModes.Sea, typeof(SeaShippingProviderCollection));
			AsssertCarrierList(Core.Constants.TransportModes.Rail, typeof(TransportScheduleRailShippingProviderCollection));
			AsssertCarrierList(Core.Constants.TransportModes.Road, typeof(TransportScheduleLineHaulShippingProviderCollection));
		}
	}
}
