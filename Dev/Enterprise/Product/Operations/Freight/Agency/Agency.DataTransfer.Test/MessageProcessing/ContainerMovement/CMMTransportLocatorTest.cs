using System;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.ContainerMovement.Testing
{
	internal class CMMTransportLocatorTest : TestCaseWithFactory
	{
		public void TestFindBestGuessVoyage()
		{
			//       ROA       SEA       SEA       SEA       RAI
			// AUMEL --> AUBNE --> SGSIN --> GBLON --> NLAMS --> NLNES
			ZDateTime now = ZDateTime.Now;
			string[] ports = { "AUMEL", "AUBNE", "SGSIN", "GBLON", "NLAMS", "NLNES" };
			var voyageDetails = new[] { new
			{
			Mode = Constants.TransportModes.Road, Main = false
			}

			, new
			{
			Mode = Constants.TransportModes.Sea, Main = true
			}

			, new
			{
			Mode = Constants.TransportModes.Sea, Main = false
			}

			, new
			{
			Mode = Constants.TransportModes.Sea, Main = false
			}

			, new
			{
			Mode = Constants.TransportModes.Rail, Main = false
			}

			, };
			JobVoyage[] voyages = new JobVoyage[voyageDetails.Length];
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			for (int i = 0; i < voyageDetails.Length; i++)
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "Vessel" + i;
				JobVoyage voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = voyageDetails[i].Mode;
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "Voyage" + i;
				VoyageOrigin origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = ports[i];
				origin.JA_E_DEP = now.AddDays((i << 2) + 1);
				VoyageDestination destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = ports[i + 1];
				destination.JB_E_ARV = now.AddDays((i << 2) + 3);
				voyages[i] = voyage;
				if (voyageDetails[i].Main)
				{
					shipment.JS_JX = voyage.Sailings[0].PK;
				}
				else
				{
					Transport transport = shipment.Transports.AddNew();
					transport.JW_IsLinked = true;
					transport.JW_TransportMode = voyageDetails[i].Mode;
					transport.JW_JX = voyage.Sailings[0].PK;
				}
			}

			shipment.JS_RL_NKOrigin = ports[0];
			shipment.JS_RL_NKDestination = ports[ports.Length - 1];
			AssertVoyages(shipment, "AUMEL", null, null, voyages[1], voyages[1], voyages[1]);
			AssertVoyages(shipment, "AUBNE", voyages[1], null, voyages[1], voyages[1], voyages[1]);
			AssertVoyages(shipment, "SGSIN", voyages[2], voyages[1], voyages[1], voyages[3], null);
			AssertVoyages(shipment, "GBLON", voyages[3], voyages[2], voyages[1], voyages[3], null);
			AssertVoyages(shipment, "NLAMS", null, voyages[3], voyages[3], voyages[3], voyages[3]);
			AssertVoyages(shipment, "NLNES", null, null, voyages[3], voyages[3], voyages[3]);
		}

		#region Implementation
		static void AssertVoyages(AgencyShipment shipment, string port, JobVoyage loadVoyage, JobVoyage dischargeVoyage, JobVoyage originVoyage, JobVoyage destinationVoyage, JobVoyage depotVoyage)
		{
			CombineAssertions(delegate
			{
				AssertVoyage(shipment, port, ContainerMovementTypes.Codes.YardGateOut, originVoyage);
				AssertVoyage(shipment, port, ContainerMovementTypes.Codes.WharfGateIn, loadVoyage);
				AssertVoyage(shipment, port, ContainerMovementTypes.Codes.Load, loadVoyage);
				AssertVoyage(shipment, port, ContainerMovementTypes.Codes.Discharge, dischargeVoyage);
				AssertVoyage(shipment, port, ContainerMovementTypes.Codes.WharfGateOut, dischargeVoyage);
				AssertVoyage(shipment, port, ContainerMovementTypes.Codes.YardGateIn, destinationVoyage);
				AssertVoyage(shipment, port, ContainerMovementTypes.Codes.DepotGateIn, depotVoyage);
				AssertVoyage(shipment, port, ContainerMovementTypes.Codes.DepotGateOut, depotVoyage);
			});
		}

		static void AssertVoyage(AgencyShipment shipment, string port, string movementType, JobVoyage voyage)
		{
			JobVoyage actualVoyage = CMMTransportLocator.FindBestGuessVoyage(shipment, port, movementType);
			if (actualVoyage == voyage)
			{
				Assert(true);
			}
			else
			{
				Converter<JobVoyage, string> displayTextProvider = delegate(JobVoyage v)
				{
					return v == null ? null : string.Format("{0}: {1}/{2}", v.JV_AirSeaRoad, v.JV_RV_NKVessel, v.JV_VoyageFlight);
				};
				StringBuilder builder = new StringBuilder();
				builder.Append(Html(string.Format("{0}: {1}", port, movementType)));
				builder.Append("<br/>");
				builder.Append(HtmlFormatGoodValue(voyage, displayTextProvider));
				builder.Append(HtmlFormatBadValue(actualVoyage, displayTextProvider));
				HtmlFail(builder.ToString());
			}
		}
		#endregion
	}
}
