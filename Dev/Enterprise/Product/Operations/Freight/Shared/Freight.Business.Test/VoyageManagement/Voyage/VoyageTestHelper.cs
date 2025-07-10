using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	public sealed class VoyageTestHelper
	{
		public VoyageTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public readonly BusinessObjectFactory Factory;

		public OrgHeader CreateCarrier(ZString code)
		{
			var carrier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, code));
			if (carrier == null)
			{
				carrier = Factory.New<OrgHeader>();
				carrier.OH_Code = code;
				carrier.OH_FullName = code.Left(9);
			}

			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			return carrier;
		}

		public JobVoyage CreateSeaVoyage(ZString vesselName, ZString voyageNumber, ZGuid carrierPK)
		{
			return CreateSeaVoyage(vesselName, voyageNumber, carrierPK, "AUSYD", "NZAKL");
		}

		public JobVoyage CreateSeaVoyage(ZString vesselName, ZString voyageNumber, ZGuid carrierPK, ZString loadPort, ZString dischargePort)
		{
			return CreateVoyage(Constants.TransportModes.Sea, vesselName, voyageNumber, carrierPK, loadPort, dischargePort);
		}

		public JobVoyage CreateVoyage(ZString transportMode, ZString vesselName, ZString voyageNumber, ZGuid carrierPK, ZString loadPort, ZString dischargePort)
		{
			var vessel = RefVessel.LookupVesselByName(vesselName, Factory).FirstOrDefault();
			if (vessel == null && !vesselName.IsEmpty)
			{
				vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = vesselName;
			}

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = transportMode;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = voyageNumber;
			voyage.JV_OH_Line = carrierPK;

			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = loadPort;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = dischargePort;
			voyage.GenerateSailings();

			return voyage;
		}
	}
}
