using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	[TestedType(typeof(VesselRoutingVoyageCollection))]
	sealed class VesselRoutingVoyageCollectionDBHTest : VesselRoutingVoyageCollectionTestBase
	{
		public override void TestLoadingVoyages()
		{
			NewJobVesselSchedule("DEBRE", ZDateTime.Empty, ZDateTime.Today.AddDays(1), "Lloyds1", "", "VoyageOut");
			NewJobVesselSchedule("DEHAM", ZDateTime.Empty, ZDateTime.Today.AddDays(2), "Lloyds1", "", "VoyageOut");
			NewJobVesselSchedule("DECUX", ZDateTime.Empty, ZDateTime.Today.AddDays(3), "Lloyds1", "", "VoyageOut");
			NewJobVesselSchedule("DEBRV", ZDateTime.Empty, ZDateTime.Today.AddDays(4), "Lloyds1", "", "VoyageOut");

			NewJobVesselSchedule("DEBRE", ZDateTime.Today.AddDays(31), ZDateTime.Empty, "Lloyds1", "VoyageIn", "");
			NewJobVesselSchedule("DEHAM", ZDateTime.Today.AddDays(32), ZDateTime.Empty, "Lloyds1", "VoyageIn", "");
			NewJobVesselSchedule("DECUX", ZDateTime.Today.AddDays(33), ZDateTime.Empty, "Lloyds1", "VoyageIn", "");
			NewJobVesselSchedule("DEBRV", ZDateTime.Today.AddDays(34), ZDateTime.Empty, "Lloyds1", "VoyageIn", "");

			Factory.Save();

			Voyages.PortPairTypeFilter = PortPairTypes.All;
			var query = new ZQuery(ViewVesselRoutingVoyagesSchema.E8_LloydsNumber, SQLComparisonOperator.StartsWith, "Lloyds");
			query.AddToFilter(ViewVesselRoutingVoyagesSchema.E8_DataProvider, DataProvider);
			Voyages.Load(query);

			var voyages = Voyages.Cast<VesselRoutingVoyage>().OrderBy(v => v.E8_Voyage).ToList();
			AssertEquals("There should be 2 voyages", 2, voyages.Count);
			AssertEquals("VoyageIn", voyages[0].E8_Voyage);
			AssertEquals("VoyageOut", voyages[1].E8_Voyage);
		}

		protected override ZString DataProvider { get { return FreightConstants.VesselDataProviders.DBH; } }
	}
}
