using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	abstract class VesselRoutingPortLoaderTestBase : LoaderTestCase
	{
		public abstract void TestLoad();
		protected abstract string DataProvider { get; }

		#region Implementation

		protected JobVoyage Voyage
		{
			get
			{
				if (voyage == null)
				{
					voyage = Factory.New<JobVoyage>();
					voyage.Origins.AddNew();
					voyage.Destinations.AddNew();
				}

				return voyage;
			}
		}
		JobVoyage voyage;

		protected VoyageOrigin Origin
		{
			get { return Voyage.Origins[0]; }
		}

		protected VoyageDestination Destination
		{
			get { return Voyage.Destinations[0]; }
		}

		protected RefVessel RefVessel
		{
			get
			{
				if (refVessel == null)
				{
					refVessel = Factory.New<RefVessel>();
					refVessel.RV_Name = "Vessel";
					refVessel.RV_LloydsNumber = "Lloyds";
				}

				return refVessel;
			}
		}
		RefVessel refVessel;

		protected VesselRoutingPort.Loader Loader
		{
			get
			{
				if (loader == null)
				{
					loader = new VesselRoutingPort.Loader(Factory);
				}

				return loader;
			}
		}
		VesselRoutingPort.Loader loader;

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new VesselRoutingPort.Loader(Factory);
		}

		protected JobVesselSchedule NewJobVesselSchedule(ZString portName, ZDateTime eTA, ZDateTime eTD, ZString lloydsNumber, ZString voyageIn, ZString voyageOut)
		{
			JobVesselSchedule result = Factory.New<JobVesselSchedule>();
			result.EV_RL_NKPortCode = portName;
			result.EV_ETA = eTA;
			result.EV_ETD = eTD;
			result.EV_IMOLloydsNumber = lloydsNumber;
			result.EV_ShipOperatorVoyageIn = voyageIn;
			result.EV_ShipOperatorVoyageOut = voyageOut;
			result.EV_DataProvider = DataProvider;
			return result;
		}

		#endregion
	}
}
