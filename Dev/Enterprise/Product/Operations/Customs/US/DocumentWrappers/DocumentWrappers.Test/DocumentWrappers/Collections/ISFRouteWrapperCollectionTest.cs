using Enterprise.Customs.US.ISF.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Testing;
using NUnit.Framework;
using Routing = Enterprise.Freight.Business.Transport;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(ISFRouteWrapperCollection))]
	sealed class ISFRouteWrapperCollectionTest : RouteWrapperCollectionTest
	{
		public void TestMostInterestingIndexerFromISF()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();

			Routing transport1 = header.Transports.AddNew();
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			transport1.JW_Vessel = "Vessel 1";
			Routing transport2 = header.Transports.AddNew();
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport2.JW_Vessel = "Vessel 2";
			Routing transport3 = header.Transports.AddNew();
			transport3.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			transport3.JW_Vessel = "Vessel 3";

			var collection = new ISFRouteWrapperCollection(header, Factory);

			AssertNotNull("MostInteresting indexer", collection["MostInteresting"]);
			AssertEquals("MostInteresting indexer", "Vessel 2", collection["MostInteresting"].Transport.VesselName);
		}

		public void TestLoadFromISF()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();

			Routing transport1 = header.Transports.AddNew();
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			transport1.JW_Vessel = "Vessel 1";
			Routing transport2 = header.Transports.AddNew();
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport2.JW_Vessel = "Vessel 2";
			Routing transport3 = header.Transports.AddNew();
			transport3.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			transport3.JW_Vessel = "Vessel 3";

			var collection = new ISFRouteWrapperCollection(header, Factory);
			AssertEquals("collection.Count", 3, collection.Count);
		}

		public override void TestLoadFromOrder()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();

			Routing transport1 = header.Transports.AddNew();
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			transport1.JW_Vessel = "Vessel 1";
			var collection = new ISFRouteWrapperCollection(header, Factory);

			AssertNotNull("ISFRouteWrapperCollection can only create by CusISFHeader", collection);
		}
	}
}
