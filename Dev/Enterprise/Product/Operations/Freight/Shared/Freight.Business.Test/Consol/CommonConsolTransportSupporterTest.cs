using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonConsolTransportSupporterTest : TransportSupporterTestCase<CommonConsolTransportSupporter<CommonConsol>>
	{
		[RunInExtraTransaction]
		public void TestConsignmentRef()
		{
			ITransportParent parent = Factory.New<CommonConsol>();
			AssertEquals("", parent.TransportSupporter.ConsignmentRef);

			parent.TransportSupporter.SetConsignmentRefIfNotSet();
			Assert("ConsignmentRef should now be populated from the number fountain", Regex.IsMatch(parent.TransportSupporter.ConsignmentRef, "C[0-9]{8}"));
		}

		public void TestTerminalAvailabilityDate()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirline2LetterCode(Factory, "NZ").PK;

			var consol = Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>() as CommonConsol;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var container = consol.Containers.AddNew();
			Assert("Should not have Import STO Penalty", container.ImportPenalties.All(x => x.CPY_PenaltyType != "STO"));
			consol.Transports[0].JW_TerminalAvailabilityDate = ZDateTime.Now;
			Assert("Should have Import STO Penalty", container.ImportPenalties.Any(x => x.CPY_PenaltyType == "STO"));
		}

		public void TestDefaultCarrierOnConsolWhenFlightNumberChanges()
		{
			OrgHeader qantasCarrier = Factory.New<OrgHeader>();
			qantasCarrier.OH_IsShippingLine = true;
			qantasCarrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirline2LetterCode(Factory, "QF").PK;
			OrgHeader airNZCarrier = Factory.New<OrgHeader>();
			airNZCarrier.OH_IsShippingLine = true;
			airNZCarrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirline2LetterCode(Factory, "NZ").PK;
			OrgHeader singaporeCarrier = Factory.New<OrgHeader>();
			singaporeCarrier.OH_IsShippingLine = true;
			singaporeCarrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirline2LetterCode(Factory, "SQ").PK;

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			Transport transport1 = consol.Transports[0];
			transport1.JW_VoyageFlight = "NZ01";

			AssertEquals("Consol carrier defaulted", airNZCarrier.MainAddress.PK, consol.JK_OA_ShippingLineAddress);

			transport1.JW_TransportType = Constants.TransportPlanningType.Flight2;
			transport1.JW_RL_NKLoadPort = "NZAKL";
			transport1.JW_RL_NKDiscPort = "USLAX";
			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_TransportType = Constants.TransportPlanningType.Flight1;
			transport2.JW_VoyageFlight = "QF01";
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "NZAKL";

			AssertEquals("Consol carrier not changed, as it corresponds to another flight leg", airNZCarrier.MainAddress.PK, consol.JK_OA_ShippingLineAddress);

			transport2.JW_TransportType = Constants.TransportPlanningType.Flight2;
			transport1.JW_TransportType = Constants.TransportPlanningType.Flight1;
			transport1.JW_VoyageFlight = "SQ01";

			AssertEquals("Consol carrier changed, as it no longer corresponds to any other flight leg", singaporeCarrier.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
		}

		public void TestOnlyAirGeneratesTransportCarrierFromVoyage()
		{
			OrgHeader airNZCarrier = Factory.New<OrgHeader>();
			airNZCarrier.OH_IsShippingLine = true;
			airNZCarrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirline2LetterCode(Factory, "NZ").PK;

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_TransportMode = Constants.TransportModes.Air;

			Transport transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Constants.TransportModes.Air;
			transport1.JW_VoyageFlight = "NZ03";
			AssertEquals("Consol carrier should default from Voyage/Flight for air legs", airNZCarrier.MainAddress.PK, transport1.JW_OA_CarrierAddress);

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Road;
			transport2.JW_TransportType = Constants.TransportPlanningType.Other;
			transport2.JW_VoyageFlight = "NZ23";
			AssertNotEquals("Consol carrier should not default from Voyage/Flight for non air legs", airNZCarrier.MainAddress.PK, transport2.JW_OA_CarrierAddress);

			CommonConsol consol2 = Factory.New<CommonConsol>();
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "USLAX";
			consol2.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport3 = consol2.Transports[0];
			transport3.JW_TransportMode = Constants.TransportModes.Air;
			transport3.JW_VoyageFlight = "NZ02";
			AssertNotEquals("Consol carrier should not default from Voyage/Flight for non air consols", airNZCarrier.MainAddress.PK, transport3.JW_OA_CarrierAddress);
		}

		public void TestConsolsShippingLineIsDefaultedWhenSeaSailingChanged()
		{
			var helper = new VoyageTestHelper(Factory);
			var carrier1 = helper.CreateCarrier("MAERSK");
			var carrier2 = helper.CreateCarrier("KLINE");

			var voyage1 = helper.CreateSeaVoyage("Visund", "123", carrier1.PK, "AUSYD", "NZAKL");
			var voyage2 = helper.CreateSeaVoyage("Visund", "456", carrier2.PK, "AUSYD", "NZAKL");

			Factory.Save();

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var mainTransport = consol.Transports[0];
			mainTransport.JW_IsLinked = true;
			mainTransport.JW_JX = voyage1.Sailings[0].PK;
			AssertEquals("Consol carrier defaulted", carrier1.PK, consol.ShippingLinePK);

			mainTransport.JW_JX = voyage2.Sailings[0].PK;
			AssertEquals("Consol carrier defaulted", carrier2.PK, consol.ShippingLinePK);

			var anotherCarrierAddress = carrier1.Addresses.AddNew();
			consol.JK_OA_ShippingLineAddress = anotherCarrierAddress.PK;

			mainTransport.JW_JX = voyage1.Sailings[0].PK;
			AssertEquals("Consol carrier address not changed", anotherCarrierAddress.PK, consol.JK_OA_ShippingLineAddress);

			var anotherTransport = consol.Transports.AddNew();
			anotherTransport.JW_IsLinked = true;
			anotherTransport.JW_JX = voyage2.Sailings[0].PK;
			AssertEquals("Defaulting from the main transport only", carrier1.PK, consol.ShippingLinePK);
		}

		public void TestNotifyVoyageUpdated_ShouldCreateOrUpdateCargoAvailableEventOnShipments()
		{
			Action<ZString, ZDateTime, ZDateTime, ZDateTime> assert = (containerMode, fclDate, lclDate, expectedDate) =>
			{
				var consol = Factory.New<CommonConsol>();
				var shipment = consol.Shipments.AddNew();

				shipment.JS_PackingMode = containerMode;
				shipment.DocsAndCartage.JP_FCLAvailable = fclDate;
				shipment.DocsAndCartage.JP_LCLAvailable = lclDate;
				shipment.Logs.RemoveAndDeleteAll();

				var supporter = new CommonConsolTransportSupporter<CommonConsol>(consol);
				supporter.NotifyVoyageUpdated(null);

				var log = shipment.Logs.MostRecentLogByEventTime(Events.CargoAvailable);

				if (!expectedDate.IsEmpty)
				{
					AssertNotNull(string.Format("{0} Log", Events.CargoAvailable), log);
					AssertEquals("Event time", expectedDate, log.SL_EventTime);
				}
				else
				{
					AssertNull(string.Format("{0} Log", Events.CargoAvailable), log);
				}
			};

			assert(Constants.ContainerModes.FCL, ZDateTime.Empty, 4.DaysAgo(), ZDateTime.Empty);
			assert(Constants.ContainerModes.FCL, 2.DaysAgo(), 4.DaysAgo(), 2.DaysAgo());
			assert(Constants.ContainerModes.LCL, 2.DaysAgo(), ZDateTime.Empty, ZDateTime.Empty);
			assert(Constants.ContainerModes.LCL, 2.DaysAgo(), 4.DaysAgo(), 4.DaysAgo());
		}

		public void TestIsArrivalContainerModeFCLorULD_ComplexTest()
		{
			var consol = Factory.New<CommonConsol>();
			var supporter = new CommonConsolTransportSupporter<CommonConsol>(consol);

			foreach (var mode in Constants.ContainerModes.FCLTypes)
			{
				consol.JK_ConsolMode = mode;
				AssertEquals(string.Format("The value when the container mode is '{0}'", mode), true, supporter.IsArrivalContainerModeFCLorULD);
			}

			foreach (var mode in Constants.ContainerModes.LCLTypes)
			{
				consol.JK_ConsolMode = mode;
				AssertEquals(string.Format("The value when the container mode is '{0}'", mode), false, supporter.IsArrivalContainerModeFCLorULD);
			}

			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			AssertEquals(string.Format("The value when the consol is '{0}'", Constants.ContainerModes.BuyersConsol), true, supporter.IsArrivalContainerModeFCLorULD);
		}

		public void TestIsDepartureContainerModeFCLorULD_ComplexTest()
		{
			var consol = Factory.New<CommonConsol>();
			var supporter = new CommonConsolTransportSupporter<CommonConsol>(consol);

			foreach (var mode in Constants.ContainerModes.FCLTypes)
			{
				consol.JK_ConsolMode = mode;
				AssertEquals(string.Format("The value when the container mode is '{0}'", mode), true, supporter.IsDepartureContainerModeFCLorULD);
			}

			foreach (var mode in Constants.ContainerModes.LCLTypes)
			{
				consol.JK_ConsolMode = mode;
				AssertEquals(string.Format("The value when the container mode is '{0}'", mode), false, supporter.IsDepartureContainerModeFCLorULD);
			}

			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			AssertEquals(string.Format("The value when the consol is '{0}'", Constants.ContainerModes.BuyersConsol), false, supporter.IsDepartureContainerModeFCLorULD);
		}

		public void TestBillOfLading()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = "MAWB1111";
			Factory.Save();

			var supporter = new CommonConsolTransportSupporter<CommonConsol>(consol);
			AssertEquals("MAWB1111", supporter.BillOfLading);
			AssertEquals(false, supporter.BillOfLadingHasChanges);

			consol.JK_MasterBillNum = "MAWB000";
			AssertEquals("MAWB000", supporter.BillOfLading);
			AssertEquals(7, supporter.BillOfLading.Length);
			AssertEquals(true, supporter.BillOfLadingHasChanges);

			consol.JK_MasterBillNum = "MAWB12345670000011111";
			AssertEquals("MAWB12345670000011111", supporter.BillOfLading);
			AssertEquals(21, supporter.BillOfLading.Length);
		}

		public void TestCarrierWouldNotBeOverridenByTransportWhenImportFromUXML()
		{
			var carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_IsShippingLine = true;
			var carrier2 = Factory.New<OrgHeader>();
			carrier2.OH_IsShippingLine = true;

			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_ShippingLineAddress = carrier1.MainAddress.PK;

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(carrier1.MainAddress.PK, transport.JW_OA_CarrierAddress);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			((ISupportDataImporting)consol).IsImportingData = true;

			transport.JW_OA_CarrierAddress = carrier2.MainAddress.PK;
			AssertEquals("NOT override consol carrier by transport when importing from UXML", ZGuid.Empty, consol.JK_OA_ShippingLineAddress);
		}

		#region Implementation

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceForwarding; }
		}

		protected override TransportSupporter GetNewTransportSupporter()
		{
			ITransportParent parent = Factory.New<CommonConsol>();
			return parent.TransportSupporter;
		}

		protected override ZString TestingCountry
		{
			get { return Constants.CountryCodes.Australia; }
		}

		#endregion
	}
}
