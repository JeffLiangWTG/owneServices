using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	[TestedType(typeof(VesselRoutingVoyageCollection))]
	sealed class VesselRoutingVoyageCollectionOneStopTest : VesselRoutingVoyageCollectionTestBase
	{
		public override void TestLoadingVoyages()
		{
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds1", "Voyage1", "Voyage1");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Lloyds1", "Voyage1", "Voyage1");
			NewJobVesselSchedule("AUPER", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(6), "Lloyds1", "Voyage1", "Voyage1");

			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds1", "Voyage2", "Voyage2");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Lloyds1", "Voyage2", "Voyage2");
			NewJobVesselSchedule("AUPER", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(6), "Lloyds1", "Voyage2", "Voyage2");

			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds2", "Voyage1", "Voyage1");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Lloyds2", "Voyage1", "Voyage1");
			NewJobVesselSchedule("AUPER", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(6), "Lloyds2", "Voyage1", "Voyage1");

			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds2", "Voyage2", "Voyage2");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Lloyds2", "Voyage2", "Voyage2");
			NewJobVesselSchedule("AUPER", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(6), "Lloyds2", "Voyage2", "Voyage2");

			Factory.Save();
			Voyages.PortPairTypeFilter = PortPairTypes.Domestic;
			ZQuery query = new ZQuery(ViewVesselRoutingVoyagesSchema.E8_LloydsNumber, SQLComparisonOperator.StartsWith, "Lloyds");
			query.AddToFilter(ViewVesselRoutingVoyagesSchema.E8_DataProvider, DataProvider);
			Voyages.Load(query);

			AssertEquals("There should be 4 voyages", 4, Voyages.Count);
			AssertContainsLloydsVoyage(Voyages, "Lloyds1", "Voyage1");
			AssertContainsLloydsVoyage(Voyages, "Lloyds1", "Voyage2");
			AssertContainsLloydsVoyage(Voyages, "Lloyds2", "Voyage1");
			AssertContainsLloydsVoyage(Voyages, "Lloyds2", "Voyage2");

			foreach (VesselRoutingVoyage voyage in Voyages)
			{
				AssertEquals("Expect 3 port pairs per voyage", 9, voyage.PortPairs.Count);
			}
		}

		protected override ZString DataProvider { get { return FreightConstants.VesselDataProviders.OneStop; } }
	}
}
