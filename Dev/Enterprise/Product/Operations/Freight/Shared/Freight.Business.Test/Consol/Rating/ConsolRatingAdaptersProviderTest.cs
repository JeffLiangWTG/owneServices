using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConsolRatingAdaptersProviderTest : TestCaseWithFactory
	{
		public void TestIJobInvoicingHostWithAdditionalJobs_AdditionalJobs()
		{
			var consol = Factory.New<CommonConsol>();
			var provider = ((IRatingSupporter)consol).AdaptersProvider;
			AssertEquals(0, provider.GetAdditionalJobs().Count);

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var randomShipment = Factory.New<CommonShipment>();
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, provider.GetAdditionalJobs());
		}

		public void TestIRoutingSupport_UsesConsol()
		{
			RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var provider = ((IRatingSupporter)ConsolWithTransportLeg).AdaptersProvider;
			var adapters = provider.GetAdapters(null, AutoRateOptions.AutorateCosts);

			AssertEquals(1, adapters.Count);
			AssertEquals(AdapterType.Consolidation, adapters[0].AdapterType);
			AssertEquals("Should take Origin from Consol", "CNSHA", adapters[0].Origin.Code);
		}

		public void TestIRoutingSupport_UsesRouteSetRatingRoute()
		{
			RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var provider = ((IRatingSupporter)ConsolWithTransportLeg).AdaptersProvider;
			var adapters = provider.GetAdapters(null, AutoRateOptions.AutorateCosts);

			AssertEquals(1, adapters.Count);
			AssertEquals(AdapterType.Consolidation, adapters[0].AdapterType);
			AssertEquals("Should take Origin from the Transport Leg", "AUSYD", adapters[0].Origin.Code);
		}

		CommonConsol ConsolWithTransportLeg
		{
			get
			{
				var consol = Factory.NewWithValidTestData<CommonConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_UniqueConsignRef = "CON123";
				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "AUMEL";

				var transportLeg = consol.Transports[0];
				transportLeg.JW_ETD = ZDateTime.Today;
				transportLeg.JW_ETA = ZDateTime.Today.AddDays(8);
				transportLeg.JW_RL_NKLoadPort = "AUSYD";
				transportLeg.JW_RL_NKDiscPort = "AUMEL";

				AssertEquals("Pre-condition", "CNSHA", consol.JK_RL_NKLoadPort);
				AssertEquals("Pre-condition", "AUSYD", transportLeg.JW_RL_NKLoadPort);

				return consol;
			}
		}
	}
}
