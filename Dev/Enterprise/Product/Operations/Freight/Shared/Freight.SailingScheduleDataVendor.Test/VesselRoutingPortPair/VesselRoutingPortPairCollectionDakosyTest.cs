using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	[TestedType(typeof(TestVesselRoutingPortPairCollection))]
	sealed class VesselRoutingPortPairCollectionDakosyTest : VesselRoutingPortPairCollectionTestBase
	{
		public void TestSelectPortPairsMatchingPort()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				NewJobVesselSchedule("DEHAM", ZDateTime.Empty, ZDateTime.Today.AddDays(2), "Lloy999", ZString.Empty, "VoyageTest", "DEMO SHIP", "VLS", "VANGUARD LOGISTICS");
				NewJobVesselSchedule("CYLMS", ZDateTime.Today.AddDays(4), ZDateTime.Empty, "Lloy999", "VoyageTest", ZString.Empty, "DEMO SHIP", "VLS", "VANGUARD LOGISTICS");
				Factory.Save();

				VoyageCollection.PortPairTypeFilter = PortPairTypes.All;
				VesselRoutingVoyage voyage = LoadVoyageByLloyds("Lloy999", DataProvider);

				AssertPortPairCount(voyage, 1);
				AssertPortPairValues("Expected port pair for test", voyage.PortPairs[0], "DEHAM", "CYLMS", ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(4), PortPairTypes.Export);

				voyage.PortPairs.SelectPortPairsMatchingPort("AUSYD");
				Assert(!voyage.PortPairs[0].E9_IsSelected);

				voyage.PortPairs.SelectPortPairsMatchingPort("DEHAM");
				Assert(voyage.PortPairs[0].E9_IsSelected);

				voyage.PortPairs.SelectPortPairsMatchingPort("CYLMS");
				Assert(voyage.PortPairs[0].E9_IsSelected);
			}
		}

		protected override ZString DataProvider => FreightConstants.VesselDataProviders.DAKOSY;
	}
}
