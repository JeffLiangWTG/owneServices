using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	[TestedType(typeof(VesselRoutingPortPair))]
	sealed class VesselRoutingPortPairOneStopTest : VesselRoutingPortPairTestBase
	{
		public override void TestPortPairType()
		{
			AssertEquals("Expected 5 port pairs", 5, Voyage.PortPairs.Count);
			AssertEquals(PortPairTypes.Domestic, Voyage.PortPairs[0].PortPairType);
			AssertEquals(PortPairTypes.Import, Voyage.PortPairs[1].PortPairType);
			AssertEquals(PortPairTypes.Import, Voyage.PortPairs[2].PortPairType);
			AssertEquals(PortPairTypes.Export, Voyage.PortPairs[3].PortPairType);
			AssertEquals(PortPairTypes.Export, Voyage.PortPairs[4].PortPairType);
		}

		#region Overrides

		protected override VesselRoutingVoyage Voyage
		{
			get
			{
				if (voyage == null)
				{
					JobVesselSchedule domesticPort1 = NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Voyage", "Voyage");
					JobVesselSchedule domesticPort2 = NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Voyage", "Voyage");
					Factory.Save();

					VesselRoutingVoyageCollection voyages = new VesselRoutingVoyageCollection(Factory);
					voyages.PortPairTypeFilter = PortPairTypes.All;
					voyages.Load(new ZQuery(ViewVesselRoutingVoyagesSchema.E8_Voyage, "Voyage"));
					voyage = voyages[0];

					voyage.ForeignPorts.Add("MYPKG");

					voyage.E8_LloydsNumber = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, SQLComparisonOperator.NotEqual, string.Empty)).RV_LloydsNumber;
					AssertEquals("Expected 5 port pairs for the test", 5, voyage.PortPairs.Count);
				}

				return voyage;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Voyage.PortPairs[1];
		}

		protected override ZString DataProvider { get { return FreightConstants.VesselDataProviders.OneStop; } }

		#endregion
	}
}
