using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageChildHelperTest : TestCaseWithFactory
	{
		public void TestGetConsolsWithShipmentsNotApprovedForPassengerFlights_MultipleSailings_ReturnUnapprovedConsols()
		{
			var factory = new BusinessObjectFactory();
			var voyage = CreateVoyage(factory, Constants.TransportModes.Air);

			var sailing1 = voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "NZAKL");
			var sailing2 = voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USLAX");
			var sailing3 = voyage.Sailings.GetSailingFromLoadAndDischarge("USNYC", "UAIEV");

			var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.Value;
			inspectionTypes.Types.Add("ABC", (NoResString)"ABC", false, false);
			inspectionTypes.Types.Add("QQQ", (NoResString)"QQQ", false, false);

			using (FreightDataRegistry.Instance.ShipmentInspectionTypes_Australia.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			{
				var consol1 = CreateConsol(sailing1, "Perez", BaseJobShipmentLookups.InspectionType_Approved);
				var consol2 = CreateConsol(sailing2, "McLaren", "ABC");
				var consol3 = CreateConsol(sailing3, "Button", BaseJobShipmentLookups.InspectionType_Approved);
				var consol4 = CreateConsol(sailing3, "Raikkonen", "QQQ");

				factory.Save();

				var sailings = new List<JobSailing> { sailing1, sailing2, sailing3 };
				var consols = sailings.GetConsolsWithShipmentsNotApprovedForPassengerFlights();
				AssertContainsExactElementsInAnyOrder(new ZString[] { "McLaren" }, consols.Select(c => c.JK_UniqueConsignRef));
			}

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var consol1 = CreateConsol(sailing1, "Perez", BaseJobShipmentLookups.InspectionType_Approved);
				var consol2 = CreateConsol(sailing2, "McLaren", "ABC");
				var consol3 = CreateConsol(sailing3, "Button", BaseJobShipmentLookups.InspectionType_Approved);
				var consol4 = CreateConsol(sailing3, "Raikkonen", "QQQ");

				factory.Save();

				var sailings = new List<JobSailing> { sailing1, sailing2, sailing3 };
				var consols = sailings.GetConsolsWithShipmentsNotApprovedForPassengerFlights();
				consols = sailings.GetConsolsWithShipmentsNotApprovedForPassengerFlights();
				AssertContainsExactElementsInAnyOrder(new ZString[] { "Raikkonen" }, consols.Select(c => c.JK_UniqueConsignRef));
			}
		}

		#region Implementation

		JobVoyage CreateVoyage(BusinessObjectFactory factory, string transportMode)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = transportMode;

			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "USNYC";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "UAIEV";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";
			voyage.GenerateSailings();

			return voyage;
		}

		CommonConsol CreateConsol(JobSailing sailing, string consolReference, params string[] shipmentExportControlStatuses)
		{
			CommonConsol consol = sailing.Factory.New<CommonConsol>();
			consol.Transports[0].JW_JX = sailing.PK;
			consol.JK_UniqueConsignRef = consolReference;
			consol.JK_RL_NKLoadPort = sailing.Origin.JA_RL_NKPortOfLoading;

			foreach (string exportControlStatus in shipmentExportControlStatuses)
			{
				consol.Shipments.AddNew().JS_InspectionTypeCode = exportControlStatus;
			}

			return consol;
		}

		#endregion
	}
}
