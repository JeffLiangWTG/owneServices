using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	abstract class VesselRoutingVoyageCollectionTestBase : BusinessObjectCollectionTestCase
	{
		public abstract void TestLoadingVoyages();

		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		public void TestGetSelectedVoyages()
		{
			VesselRoutingVoyage voyage = Voyages.AddNew();
			voyage.E8_Voyage = "Voyage";
			VesselRoutingPortPair portPair = voyage.PortPairs.AddNew();
			portPair.E9_RL_NKLoadPort = "AUSYD";
			portPair.E9_RL_NKDischargePort = "AUMEL";
			portPair.E9_IsSelected = true;

			VesselRoutingVoyage decoyVoyage = Voyages.AddNew();
			decoyVoyage.E8_Voyage = "DcyVoyage";
			VesselRoutingPortPair decoyPortPair = decoyVoyage.PortPairs.AddNew();
			decoyPortPair.E9_RL_NKLoadPort = "AUSYD";
			decoyPortPair.E9_RL_NKDischargePort = "AUMEL";
			decoyPortPair.E9_IsSelected = false;

			AssertEquals("Expected only selected voyages to be returned", 1, Voyages.GetSelectedVoyages().Length);
			AssertEquals("Expected only selected voyages to be returned", true, Voyages.GetSelectedVoyages()[0].E8_IsSelected);
		}

		#region Implementation

		protected VesselRoutingVoyageCollection Voyages
		{
			get
			{
				if (voyages == null)
				{
					voyages = new VesselRoutingVoyageCollection(Factory);
				}

				return voyages;
			}
		}

		protected abstract ZString DataProvider { get; }

		protected VesselRoutingVoyageCollection voyages;

		protected JobVesselSchedule NewJobVesselSchedule(ZString portName, ZDateTime eTA, ZDateTime eTD, ZString lloyds, ZString voyageIn, ZString voyageOut)
		{
			JobVesselSchedule result = Factory.New<JobVesselSchedule>();
			result.EV_RL_NKPortCode = portName;
			result.EV_ETA = eTA;
			result.EV_ETD = eTD;
			result.EV_IMOLloydsNumber = lloyds;
			result.EV_ShipOperatorVoyageIn = voyageIn;
			result.EV_ShipOperatorVoyageOut = voyageOut;
			result.EV_DataProvider = DataProvider;

			CloneJobVesselScheduleWithDifferentDataProvider(result);

			return result;
		}

		protected JobVesselSchedule CloneJobVesselScheduleWithDifferentDataProvider(JobVesselSchedule schedule)
		{
			JobVesselSchedule result = Factory.New<JobVesselSchedule>();
			result.CopyPersistentValuesFrom(schedule);
			result.EV_DataProvider = DataProviderFromOtherCountry(schedule.EV_DataProvider);
			return result;
		}

		ZString DataProviderFromOtherCountry(ZString provider)
		{
			string result;

			switch (provider)
			{
				case FreightConstants.VesselDataProviders.DBH:
				case FreightConstants.VesselDataProviders.DAKOSY:
					result = FreightConstants.VesselDataProviders.OneStop;
					break;

				case FreightConstants.VesselDataProviders.OneStop:
					result = FreightConstants.VesselDataProviders.DBH;
					break;

				default:
					result = "XXX";
					break;
			}

			return result;
		}

		protected void AssertContainsLloydsVoyage(VesselRoutingVoyageCollection voyages, ZString lloyds, ZString voyageNum)
		{
			bool voyageFound = false;
			foreach (VesselRoutingVoyage voyage in voyages)
			{
				if (voyage.E8_Voyage == voyageNum && voyage.E8_LloydsNumber == lloyds)
				{
					voyageFound = true;
					break;
				}
			}

			Assert("Could not find voyage with Lloyds='" + lloyds + "' Voyage='" + voyageNum + "'", voyageFound);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new VesselRoutingVoyageCollection(Factory);
		}

		#endregion
	}
}
