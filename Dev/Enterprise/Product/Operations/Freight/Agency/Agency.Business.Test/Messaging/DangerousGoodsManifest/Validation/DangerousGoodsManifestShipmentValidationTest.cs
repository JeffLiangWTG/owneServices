using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Test
{
	public class DangerousGoodsManifestShipmentValidationTest : BaseAgencyTest
	{
		public void TestVoyageVesselForBinding()
		{
			const string message = "Vessel Lloyds/IMO Number of linked Sailing Schedule is missing.";

			var sailing = Shipment.Sailing;
			var voyage = sailing.Voyage;

			var vessel = RefVessel.LookupVesselByName("AMERICA STAR", Factory).First();
			voyage.JV_RV_NKVessel = vessel.RV_FK;

			vessel.RV_LloydsNumber = ZString.Empty;
			Shipment.Validation.ValidateVoyageVesselForBinding();
			AssertHasMessageError(Shipment.VoyageVesselForBindingInfo, message);

			vessel.RV_LloydsNumber = "123456";
			Shipment.Validation.ValidateVoyageVesselForBinding();
			AssertNoMessageError(Shipment.VoyageVesselForBindingInfo, message);
		}

		#region Implementation

		BillOfLading Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Factory.New<BillOfLading>();
					shipment.JS_JX = CreateSailing("AUSYD", "AUBNE", ZDateTime.Now).PK;
				}

				return shipment;
			}
		}
		BillOfLading shipment;

		JobSailing CreateSailing(ZString load, ZString discharge, ZDateTime etd)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = etd;

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;

			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			sailing.JX_IsPublished = true;

			return sailing;
		}

		protected override void SetUp()
		{
			base.SetUp();
			DangerousGoodsManifestMessageValidationStrategy.RegisterForFactory(Factory);
		}

		#endregion
	}
}
