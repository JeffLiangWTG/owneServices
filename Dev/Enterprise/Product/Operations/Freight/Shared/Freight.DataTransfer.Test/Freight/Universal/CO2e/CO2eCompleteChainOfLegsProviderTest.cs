using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing;

public class CO2eCompleteChainOfLegsProviderTest : TestCaseWithFactory
{
	public void TestGetCompleteChainOfLegsByPortsFromShipment()
	{
		SetupAndAssertChainOfLegsForShipment("AUMEL", "FRPAR", "AUSYD", "SGSIN", "AEDXB", "DEBRE",
			expectedChainOfLegs: new[]
			{
				("AUSYD", "SGSIN", false),
				("SGSIN", "AEDXB", true),
				("AEDXB", "DEBRE", false),
			});

		SetupAndAssertChainOfLegsForShipment("AUMEL", "FRPAR", "AUSYD", "SGSIN", "", "DEBRE",
			expectedChainOfLegs: new[]
			{
				("AUSYD", "SGSIN", false),
				("SGSIN", "DEBRE", true),
			});

		SetupAndAssertChainOfLegsForShipment("AUMEL", "FRPAR", "AUSYD", "SGSIN", "AEDXB", "",
			expectedChainOfLegs: new[]
			{
				("AUSYD", "SGSIN", false),
				("SGSIN", "AEDXB", true),
			});

		SetupAndAssertChainOfLegsForShipment("AUMEL", "FRPAR", "AUSYD", "", "AEDXB", "DEBRE",
			expectedChainOfLegs: new[]
			{
				("AUSYD", "AEDXB", true),
				("AEDXB", "DEBRE", false),
			});

		SetupAndAssertChainOfLegsForShipment("AUMEL", "FRPAR", "AUSYD", "", "", "DEBRE",
			expectedChainOfLegs: new[]
			{
				("AUSYD", "DEBRE", true),
			});

		SetupAndAssertChainOfLegsForShipment("AUMEL", "FRPAR", "AUSYD", "", "AEDXB", "",
			expectedChainOfLegs: new[]
			{
				("AUSYD", "AEDXB", true),
			});

		SetupAndAssertChainOfLegsForShipment("AUMEL", "FRPAR", "", "SGSIN", "AEDXB", "DEBRE",
			expectedChainOfLegs: new[]
			{
				("SGSIN", "AEDXB", true),
				("AEDXB", "DEBRE", false),
			});

		SetupAndAssertChainOfLegsForShipment("AUMEL", "FRPAR", "", "SGSIN", "", "DEBRE",
			expectedChainOfLegs: new[]
			{
				("SGSIN", "DEBRE", true),
			});

		SetupAndAssertChainOfLegsForShipment("AUMEL", "FRPAR", "", "SGSIN", "AEDXB", "",
			expectedChainOfLegs: new[]
			{
				("SGSIN", "AEDXB", true),
			});

		#region Shipment with Planned Load/Disc and without Legs

		SetupAndAssertChainOfLegsForShipment("AUMEL", "FRPAR", "", "", "", "", "AUSYD", "DEBRE",
			expectedChainOfLegs: new[]
			{
				("AUSYD", "DEBRE", true),
			});

		SetupAndAssertChainOfLegsForShipment("", "FRPAR", "", "", "", "", "AUSYD", "DEBRE",
			expectedChainOfLegs: new[]
			{
				("AUSYD", "DEBRE", true),
			});

		SetupAndAssertChainOfLegsForShipment("AUMEL", "", "", "", "", "", "AUSYD", "DEBRE",
			expectedChainOfLegs: new[]
			{
				("AUSYD", "DEBRE", true),
			});

		SetupAndAssertChainOfLegsForShipment("", "", "", "", "", "", "AUSYD", "DEBRE",
			expectedChainOfLegs: new[]
			{
				("AUSYD", "DEBRE", true),
			});

		#endregion

		#region Shipment with Planned Load/Disc and Legs

		SetupAndAssertChainOfLegsForShipment("AUMEL", "FRPAR", "AUBNE", "AEDXB", "", "", "AUSYD", "DEBRE",
			expectedChainOfLegs: new[]
			{
				("AUBNE", "AEDXB", false),
			});

		SetupAndAssertChainOfLegsForShipment("", "FRPAR", "", "SGSIN", "AEDXB", "DEBRE", "AUSYD", "DEBRE",
			expectedChainOfLegs: new[]
			{
				("AUSYD", "SGSIN", true),
				("SGSIN", "AEDXB", true),
				("AEDXB", "DEBRE", false),
			});

		SetupAndAssertChainOfLegsForShipment("AUMEL", "", "AUBNE", "SGSIN", "AEDXB", "", "AUSYD", "DEBRE",
			expectedChainOfLegs: new[]
			{
				("AUBNE", "SGSIN", false),
				("SGSIN", "AEDXB", true),
				("AEDXB", "DEBRE", true)
			});

		#endregion
	}

	void SetupAndAssertChainOfLegsForShipment(string shipmentOrigin, string shipmentDestination, string consol1Load, string consol1Disc, string consol2Load, string consol2Disc, string plannedLoad = "", string plannedDischarge = "",
		params (string LoadPort, string DischargePort, bool IsVirtual)[] expectedChainOfLegs)
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment.JS_TransportMode = "AIR";
		shipment.JS_RL_NKOrigin = shipmentOrigin;
		shipment.JS_RL_NKDestination = shipmentDestination;
		shipment.JS_RL_NKLoadPort = plannedLoad;
		shipment.JS_RL_NKDischargePort = plannedDischarge;

		if (!string.IsNullOrEmpty(consol1Load) || !string.IsNullOrEmpty(consol1Disc))
		{
			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = consol1Load;
			consol1.JK_RL_NKDischargePort = consol1Disc;
			var consol1Leg = consol1.Transports[0];
			consol1Leg.JW_ETD = ZDateTime.Today;
			consol1Leg.JW_ETD = ZDateTime.Today.AddDays(2);
		}

		if (!string.IsNullOrEmpty(consol2Load) || !string.IsNullOrEmpty(consol2Disc))
		{
			var consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = consol2Load;
			consol2.JK_RL_NKDischargePort = consol2Disc;
			var consol2Leg = consol2.Transports[0];
			consol2Leg.JW_ETD = ZDateTime.Today.AddDays(10);
			consol2Leg.JW_ETD = ZDateTime.Today.AddDays(12);
		}

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment).GetChain();
		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", expectedChainOfLegs.Length, completeLegs.Length);
			for (var i = 0; i < completeLegs.Length; i++)
			{
				AssertEquals("LoadPort", expectedChainOfLegs[i].LoadPort, completeLegs[i].From.UNLOCO);
				AssertEquals("DischargePort", expectedChainOfLegs[i].DischargePort, completeLegs[i].To.UNLOCO);
				AssertEquals("IsVirtual", expectedChainOfLegs[i].IsVirtual, completeLegs[i].IsVirtual);
			}
		});
	}

	public void TestGetCompleteChainOfLegsByPortsFromShipment_LegsWithoutLoadPortAndDischargePort()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment.JS_TransportMode = "AIR";
		shipment.JS_RL_NKLoadPort = "AUMEL";
		shipment.JS_RL_NKDischargePort = "FRPAR";
		var transport1 = shipment.Transports.AddNew();
		transport1.JW_RL_NKLoadPort = "AUMEL";
		transport1.JW_RL_NKDiscPort = "DEHAM";
		var transport2 = shipment.Transports.AddNew();
		transport2.JW_RL_NKLoadPort = "";
		transport2.JW_RL_NKDiscPort = "";

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment).GetChain();
		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", 2, completeLegs.Length);

			AssertEquals("AUMEL", completeLegs[0].From.UNLOCO);
			AssertEquals("DEHAM", completeLegs[0].To.UNLOCO);
			AssertEquals(false, completeLegs[0].IsVirtual);

			AssertEquals("DEHAM", completeLegs[1].From.UNLOCO);
			AssertEquals("FRPAR", completeLegs[1].To.UNLOCO);
			AssertEquals(true, completeLegs[1].IsVirtual);
		});
	}

	[ExpectNoExceptions]
	public void TestGetCompleteChainOfLegsByPortsFromShipment_OneLegAfterFilterLegsWithoutLoadPortAndDischargePort()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment.JS_TransportMode = "AIR";
		shipment.JS_RL_NKLoadPort = "AUMEL";
		shipment.JS_RL_NKDischargePort = "FRPAR";

		var transport = shipment.Transports.AddNew();
		transport.JW_RL_NKLoadPort = "";
		transport.JW_RL_NKDiscPort = "";

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment).GetChain();
		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", 1, completeLegs.Length);

			AssertEquals("AUMEL", completeLegs[0].From.UNLOCO);
			AssertEquals("FRPAR", completeLegs[0].To.UNLOCO);
			AssertEquals(true, completeLegs[0].IsVirtual);
		});
	}

	public void TestGetCompleteChainOfLegsByPortsFromShipment_NoLegsFromOriginDestinationToRoutingLoadDischarge()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_RL_NKOrigin = "AUSYD";
		shipment.JS_RL_NKDestination = "VNHAN";

		var transport1 = shipment.Transports.AddNew();
		transport1.JW_RL_NKLoadPort = "AUMEL";
		transport1.JW_RL_NKDiscPort = "SGSIN";
		transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;

		var transport2 = shipment.Transports.AddNew();
		transport2.JW_RL_NKLoadPort = "MYKUL";
		transport2.JW_RL_NKDiscPort = "";
		transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;

		var transport3 = shipment.Transports.AddNew();
		transport3.JW_RL_NKLoadPort = "";
		transport3.JW_RL_NKDiscPort = "VNSGN";
		transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;

		var transport4 = shipment.Transports.AddNew();
		transport4.JW_RL_NKLoadPort = "VNSGN";
		transport4.JW_RL_NKDiscPort = "VNVNH";
		transport4.JW_TransportMode = Core.Constants.TransportModes.Air;

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment).GetChain();
		AssertEquals("Do not add legs Shipment's LoadPort to First Routing Load and LastLeg to Shipment's Last Routing Discharge", 4, completeLegs.Length);
	}

	public void TestGetCompleteChainOfLegsByPortsFromShipment_NoRoutingLeg_OneLegInChain()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_RL_NKOrigin = "AUSYD";
		shipment.JS_RL_NKLoadPort = "AUBNE";
		shipment.JS_RL_NKDischargePort = "VNVNH";
		shipment.JS_RL_NKDestination = "VNHAN";

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment).GetChain();
		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", 1, completeLegs.Length);

			AssertEquals("AUBNE", completeLegs[0].From.UNLOCO);
			AssertEquals("VNVNH", completeLegs[0].To.UNLOCO);
			AssertEquals(true, completeLegs[0].IsVirtual);
		});
	}

	public void TestGetCompleteChainOfLegsByPortsFromShipment_NoRoutingLeg_NoLegInChainWitOriginAndDestinationPort()
	{
		var shipment1 = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment1.JS_RL_NKOrigin = "AUSYD";
		shipment1.JS_RL_NKDestination = "VNHAN";

		var completeLegs1 = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment1).GetChain();
		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", 0, completeLegs1.Length);
		});
	}

	public void TestGetCompleteChainOfLegsByPortsFromShipment_NoRoutingLeg_PlannedLoadPortFallbackToExportReceivingDepot()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_RL_NKLoadPort = "";
		shipment.JS_RL_NKDischargePort = "VNVNH";

		var exportReceivingDepot = Factory.NewWithValidTestData<OrgAddress>();

		shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.PK;
		Factory.Save();

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment).GetChain();
		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", 1, completeLegs.Length);

			AssertEquals(exportReceivingDepot.PK, ((OrgAddress)completeLegs[0].From.Address).PK);
			AssertEquals("VNVNH", completeLegs[0].To.UNLOCO);
			AssertEquals(true, completeLegs[0].IsVirtual);
		});
	}

	public void TestGetCompleteChainOfLegsByPortsFromShipment_NoRoutingLeg_ExportReceivingDepotFallbackToConsignorPickupAddress()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_RL_NKLoadPort = "";
		shipment.JS_RL_NKDischargePort = "VNVNH";

		var importReleaseDepot = Factory.NewWithValidTestData<OrgAddress>();

		var orgAddress = Factory.NewWithValidTestData<OrgHeader>();
		shipment.ConsignorPickupAddress.OrganisationPK = orgAddress.PK;
		var consignorPickupAddress = shipment.ConsignorPickupAddress.Address;

		shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.PK;
		Factory.Save();

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment).GetChain();
		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", 1, completeLegs.Length);

			AssertEquals(consignorPickupAddress.PK, ((OrgAddress)completeLegs[0].From.Address).PK);
			AssertEquals("VNVNH", completeLegs[0].To.UNLOCO);
			AssertEquals(true, completeLegs[0].IsVirtual);
		});
	}

	public void TestGetCompleteChainOfLegsByPortsFromShipment_NoRoutingLeg_PlannedDischargeFallbackToImportReleaseDepot()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_RL_NKLoadPort = "VNVNH";
		shipment.JS_RL_NKDischargePort = "";

		var exportReceivingDepot = Factory.NewWithValidTestData<OrgAddress>();
		var importReleaseDepot = Factory.NewWithValidTestData<OrgAddress>();

		shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.PK;
		Factory.Save();

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment).GetChain();
		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", 1, completeLegs.Length);

			AssertEquals("VNVNH", completeLegs[0].From.UNLOCO);
			AssertEquals(importReleaseDepot.PK, ((OrgAddress)completeLegs[0].To.Address).PK);
			AssertEquals(true, completeLegs[0].IsVirtual);
		});
	}

	public void TestGetCompleteChainOfLegsByPortsFromShipment_NoRoutingLeg_ImportReleaseDepotFallbackToConsigneeDeliveryAddress()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_RL_NKLoadPort = "VNVNH";
		shipment.JS_RL_NKDischargePort = "";

		var importReleaseDepot = Factory.NewWithValidTestData<OrgAddress>();

		var orgAddress = Factory.NewWithValidTestData<OrgHeader>();
		shipment.ConsigneeDeliveryAddress.OrganisationPK = orgAddress.PK;
		var consigneeDeliveryAddress = shipment.ConsigneeDeliveryAddress.Address;

		Factory.Save();

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment).GetChain();
		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", 1, completeLegs.Length);

			AssertEquals("VNVNH", completeLegs[0].From.UNLOCO);
			AssertEquals(consigneeDeliveryAddress.PK, ((OrgAddress)completeLegs[0].To.Address).PK);
			AssertEquals(true, completeLegs[0].IsVirtual);
		});
	}

	public void TestGetCompleteChainOfLegsByPortsFromShipment_NoRoutingLeg_BothPlannedLoadandPlannedDischargeUsesCarriageFallbacks()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_RL_NKLoadPort = "";
		shipment.JS_RL_NKDischargePort = "";

		var exportReceivingDepot = Factory.NewWithValidTestData<OrgAddress>();
		var importReleaseDepot = Factory.NewWithValidTestData<OrgAddress>();
		importReleaseDepot.OA_RN_NKCountryCode = "FR";

		shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.PK;
		shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.PK;
		Factory.Save();

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment).GetChain();
		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", 1, completeLegs.Length);

			AssertEquals(exportReceivingDepot.PK, ((OrgAddress)completeLegs[0].From.Address).PK);
			AssertEquals(importReleaseDepot.PK, ((OrgAddress)completeLegs[0].To.Address).PK);
			AssertEquals(true, completeLegs[0].IsVirtual);
		});
	}

	public void TestGetCompleteChainOfLegsByPortsFromShipment_OneRoutingLeg_OneLegInChain()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_RL_NKOrigin = "AUSYD";
		shipment.JS_RL_NKLoadPort = "AUBNE";
		shipment.JS_RL_NKDischargePort = "VNVNH";
		shipment.JS_RL_NKDestination = "VNHAN";

		var transport1 = shipment.Transports.AddNew();
		transport1.JW_RL_NKLoadPort = "AUMEL";
		transport1.JW_RL_NKDiscPort = "DEHAM";

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment).GetChain();

		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", 1, completeLegs.Length);

			AssertEquals("AUMEL", completeLegs[0].From.UNLOCO);
			AssertEquals("DEHAM", completeLegs[0].To.UNLOCO);
			AssertEquals(false, completeLegs[0].IsVirtual);
		});
	}

	public void TestGetCompleteChainOfLegsByPortsFromShipment_OneRoutingLeg_OneLegInChainNoFallbackFieldsPopulated()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_RL_NKOrigin = "";
		shipment.JS_RL_NKLoadPort = "";
		shipment.JS_RL_NKDischargePort = "";
		shipment.JS_RL_NKDestination = "";

		var transport1 = shipment.Transports.AddNew();
		transport1.JW_RL_NKLoadPort = "AUMEL";
		transport1.JW_RL_NKDiscPort = "DEHAM";

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment).GetChain();

		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", 1, completeLegs.Length);

			AssertEquals("AUMEL", completeLegs[0].From.UNLOCO);
			AssertEquals("DEHAM", completeLegs[0].To.UNLOCO);
			AssertEquals(false, completeLegs[0].IsVirtual);
		});
	}

	public void TestGetCompleteChainOfLegsByPortsFromShipment_OneRoutingLeg_RoutingLoadPortFallbackToPlannedLoad()
	{
		var shipment1 = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment1.JS_RL_NKOrigin = "AUSYD";
		shipment1.JS_RL_NKLoadPort = "AUBNE";
		shipment1.JS_RL_NKDischargePort = "VNVNH";
		shipment1.JS_RL_NKDestination = "VNHAN";

		var transport1 = shipment1.Transports.AddNew();
		transport1.JW_RL_NKLoadPort = "";
		transport1.JW_RL_NKDiscPort = "DEHAM";

		var completeLegs1 = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment1).GetChain();

		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", 1, completeLegs1.Length);

			AssertEquals("AUBNE", completeLegs1[0].From.UNLOCO);
			AssertEquals("DEHAM", completeLegs1[0].To.UNLOCO);
			AssertEquals(true, completeLegs1[0].IsVirtual);
		});
	}

	public void TestGetCompleteChainOfLegsByPortsFromShipment_OneRoutingLeg_RoutingDestinationPortFallbackToPlannedDischarge()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_RL_NKOrigin = "AUSYD";
		shipment.JS_RL_NKLoadPort = "AUBNE";
		shipment.JS_RL_NKDischargePort = "VNVNH";
		shipment.JS_RL_NKDestination = "VNHAN";

		var transport1 = shipment.Transports.AddNew();
		transport1.JW_RL_NKLoadPort = "AUMEL";
		transport1.JW_RL_NKDiscPort = "";

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment).GetChain();

		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", 1, completeLegs.Length);

			AssertEquals("AUMEL", completeLegs[0].From.UNLOCO);
			AssertEquals("VNVNH", completeLegs[0].To.UNLOCO);
			AssertEquals(true, completeLegs[0].IsVirtual);
		});
	}

	public void TestGetCompleteChainOfLegsByPortsFromShipment_OneRoutingLeg_BothRoutingLoadDischargeFallbacks()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_RL_NKLoadPort = "AUSYD";
		shipment.JS_RL_NKDischargePort = "VNHAN";

		var transport1 = shipment.Transports.AddNew();
		transport1.JW_RL_NKLoadPort = "";
		transport1.JW_RL_NKDiscPort = "";

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment).GetChain();

		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", 1, completeLegs.Length);

			AssertEquals("AUSYD", completeLegs[0].From.UNLOCO);
			AssertEquals("VNHAN", completeLegs[0].To.UNLOCO);
			AssertEquals(true, completeLegs[0].IsVirtual);
		});
	}

	public void TestGetCompleteChainOfLegsByPortsFromShipment_TwoRoutingLegs_TwoLegsInChain()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_RL_NKOrigin = "AUSYD";
		shipment.JS_RL_NKLoadPort = "AUBNE";
		shipment.JS_RL_NKDischargePort = "VNVNH";
		shipment.JS_RL_NKDestination = "VNHAN";

		var transport1 = shipment.Transports.AddNew();
		transport1.JW_RL_NKLoadPort = "AUMEL";
		transport1.JW_RL_NKDiscPort = "DEHAM";

		var transport2 = shipment.Transports.AddNew();
		transport2.JW_RL_NKLoadPort = "DEHAM";
		transport2.JW_RL_NKDiscPort = "VNSGN";

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment).GetChain();

		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", 2, completeLegs.Length);

			AssertEquals("AUMEL", completeLegs[0].From.UNLOCO);
			AssertEquals("DEHAM", completeLegs[0].To.UNLOCO);
			AssertEquals(false, completeLegs[0].IsVirtual);

			AssertEquals("DEHAM", completeLegs[1].From.UNLOCO);
			AssertEquals("VNSGN", completeLegs[1].To.UNLOCO);
			AssertEquals(false, completeLegs[1].IsVirtual);
		});
	}

	public void TestGetCompleteChainOfLegsByPortsFromShipment_TwoRoutingLegs_ThreeLegsWhenFirstRoutingDischargeIsDifferentToLastRoutingLoad()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_RL_NKOrigin = "AUSYD";
		shipment.JS_RL_NKLoadPort = "AUBNE";
		shipment.JS_RL_NKDischargePort = "VNVNH";
		shipment.JS_RL_NKDestination = "VNHAN";

		var transport1 = shipment.Transports.AddNew();
		transport1.JW_RL_NKLoadPort = "AUMEL";
		transport1.JW_RL_NKDiscPort = "MYKUL";

		var transport2 = shipment.Transports.AddNew();
		transport2.JW_RL_NKLoadPort = "DEHAM";
		transport2.JW_RL_NKDiscPort = "VNSGN";

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment).GetChain();

		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", 3, completeLegs.Length);

			AssertEquals("AUMEL", completeLegs[0].From.UNLOCO);
			AssertEquals("MYKUL", completeLegs[0].To.UNLOCO);
			AssertEquals(false, completeLegs[0].IsVirtual);

			AssertEquals("MYKUL", completeLegs[1].From.UNLOCO);
			AssertEquals("DEHAM", completeLegs[1].To.UNLOCO);
			AssertEquals(true, completeLegs[1].IsVirtual);

			AssertEquals("DEHAM", completeLegs[2].From.UNLOCO);
			AssertEquals("VNSGN", completeLegs[2].To.UNLOCO);
			AssertEquals(false, completeLegs[2].IsVirtual);
		});
	}

	public void TestGetCompleteChainOfLegsByPortsFromShipment_TwoRoutingLegs_RoutingLoadPortFallbackToPlannedLoad()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_RL_NKOrigin = "AUSYD";
		shipment.JS_RL_NKLoadPort = "AUBNE";
		shipment.JS_RL_NKDischargePort = "VNVNH";
		shipment.JS_RL_NKDestination = "VNHAN";

		var transport1 = shipment.Transports.AddNew();
		transport1.JW_RL_NKLoadPort = "";
		transport1.JW_RL_NKDiscPort = "DEHAM";

		var transport2 = shipment.Transports.AddNew();
		transport2.JW_RL_NKLoadPort = "DEHAM";
		transport2.JW_RL_NKDiscPort = "VNSGN";

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment).GetChain();

		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", 2, completeLegs.Length);

			AssertEquals("AUBNE", completeLegs[0].From.UNLOCO);
			AssertEquals("DEHAM", completeLegs[0].To.UNLOCO);
			AssertEquals(true, completeLegs[0].IsVirtual);

			AssertEquals("DEHAM", completeLegs[1].From.UNLOCO);
			AssertEquals("VNSGN", completeLegs[1].To.UNLOCO);
			AssertEquals(false, completeLegs[1].IsVirtual);
		});
	}

	public void TestGetCompleteChainOfLegsByPortsFromShipment_TwoRoutingLegs_RoutingDestinationPortFallbackToPlannedDischarge()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_RL_NKOrigin = "AUSYD";
		shipment.JS_RL_NKLoadPort = "AUBNE";
		shipment.JS_RL_NKDischargePort = "VNVNH";
		shipment.JS_RL_NKDestination = "VNHAN";

		var transport1 = shipment.Transports.AddNew();
		transport1.JW_RL_NKLoadPort = "AUMEL";
		transport1.JW_RL_NKDiscPort = "DEHAM";

		var transport2 = shipment.Transports.AddNew();
		transport2.JW_RL_NKLoadPort = "DEHAM";
		transport2.JW_RL_NKDiscPort = "";

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)shipment).GetChain();

		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", 2, completeLegs.Length);

			AssertEquals("AUMEL", completeLegs[0].From.UNLOCO);
			AssertEquals("DEHAM", completeLegs[0].To.UNLOCO);
			AssertEquals(false, completeLegs[0].IsVirtual);

			AssertEquals("DEHAM", completeLegs[1].From.UNLOCO);
			AssertEquals("VNVNH", completeLegs[1].To.UNLOCO);
			AssertEquals(true, completeLegs[1].IsVirtual);
		});
	}

	public void TestGetCompleteChainOfLegsByPortsFromBooking()
	{
		CreateQuotedBookingAndAssertCompleteChainOfLegs(origin: "AUSYD", destination: "VNVNH");
		CreateQuotedBookingAndAssertCompleteChainOfLegs(origin: "AUSYD", destination: "VNVNH", loadPort: "AUMEL",
			expectedChainOfLegs: new[]
			{
				("AUSYD", "AUMEL", true),
				("AUMEL", "VNVNH", true),
			});
		CreateQuotedBookingAndAssertCompleteChainOfLegs(origin: "AUSYD", destination: "VNVNH", loadPort: "AUMEL", dischargePort: "VNSGN",
			expectedChainOfLegs: new[]
			{
				("AUSYD", "AUMEL", true),
				("AUMEL", "VNSGN", true),
				("VNSGN", "VNVNH", true),
			});
		CreateQuotedBookingAndAssertCompleteChainOfLegs(origin: "AUSYD", destination: "VNVNH", loadPort: "AUMEL", dischargePort: "VNSGN", via: "SGSIN",
			expectedChainOfLegs: new[]
			{
				("AUSYD", "AUMEL", true),
				("AUMEL", "SGSIN", true),
				("SGSIN", "VNSGN", true),
				("VNSGN", "VNVNH", true),
			});
		CreateQuotedBookingAndAssertCompleteChainOfLegs(origin: "AUSYD", destination: "VNVNH", loadPort: "AUMEL", dischargePort: "VNSGN", via: "SGSIN", sailingLoad: "AUBNE", sailingDischarge: "MYKUL",
			expectedChainOfLegs: new[]
			{
				("AUSYD", "AUMEL", true),
				("AUMEL", "AUBNE", true),
				("AUBNE", "MYKUL", false),
				("MYKUL", "VNSGN", true),
				("VNSGN", "VNVNH", true),
			});
		CreateQuotedBookingAndAssertCompleteChainOfLegs(origin: "", destination: "VNVNH", loadPort: "AUMEL", dischargePort: "", via: "SGSIN", sailingLoad: "AUBNE", sailingDischarge: "MYKUL",
			expectedChainOfLegs: new[]
			{
				("AUMEL", "AUBNE", true),
				("AUBNE", "MYKUL", false),
				("MYKUL", "VNVNH", true),
			});
		CreateQuotedBookingAndAssertCompleteChainOfLegs(origin: "AUSYD", destination: "", loadPort: "", dischargePort: "VNSGN", via: "SGSIN",
			expectedChainOfLegs: new[]
			{
				("AUSYD", "SGSIN", true),
				("SGSIN", "VNSGN", true),
			});
	}

	void CreateQuotedBookingAndAssertCompleteChainOfLegs(string origin = "", string destination = "", string loadPort = "", string dischargePort = "", string via = "", string sailingLoad = "", string sailingDischarge = "",
		params (string LoadPort, string DischargePort, bool IsVirtual)[] expectedChainOfLegs)
	{
		IQuotedBooking quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);
		quotedBooking.TransportMode = "SEA";
		quotedBooking.ForwardingShipment[JobShipmentSchema.Constants.JS_RL_NKOrigin] = origin;
		quotedBooking.ForwardingShipment[JobShipmentSchema.Constants.JS_RL_NKDestination] = destination;
		quotedBooking.ForwardingShipment[JobShipmentSchema.Constants.JS_RL_NKLoadPort] = loadPort;
		quotedBooking.ForwardingShipment[JobShipmentSchema.Constants.JS_RL_NKDischargePort] = dischargePort;
		var oneOffQuote = quotedBooking.Quote["CurrentOneOffQuote"] as BusinessObject;
		oneOffQuote[RateOneOffShipmentSchema.Constants.TT_RL_NKViaLocation] = via;

		if (!string.IsNullOrEmpty(sailingLoad) && !string.IsNullOrEmpty(sailingDischarge))
		{
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var jobVoyage = Factory.NewWithValidTestData<JobVoyage>();

			var voyageOrigin = Factory.New<VoyageOrigin>();
			voyageOrigin.JA_RL_NKPortOfLoading = sailingLoad;
			voyageOrigin.JA_JV = jobVoyage.PK;

			var voyageDestination = Factory.New<VoyageDestination>();
			voyageDestination.JB_RL_NKPortOfDischarge = sailingDischarge;
			voyageDestination.JB_JV = jobVoyage.PK;

			sailing.JX_JA = voyageOrigin.PK;
			sailing.JX_JB = voyageDestination.PK;

			quotedBooking.SailingJX = sailing.PK;
		}

		var completeLegs = new CO2eCompleteChainOfLegsProvider((ICO2eLegBasedSupporter)quotedBooking).GetChain();
		CombineAssertions("Complete chain of legs", () =>
		{
			AssertEquals("Length", expectedChainOfLegs?.Length ?? 0, completeLegs.Length);
			for (var i = 0; i < completeLegs.Length; i++)
			{
				AssertEquals("LoadPort", expectedChainOfLegs[i].LoadPort, completeLegs[i].From.UNLOCO);
				AssertEquals("DischargePort", expectedChainOfLegs[i].DischargePort, completeLegs[i].To.UNLOCO);
				AssertEquals("IsVirtual", expectedChainOfLegs[i].IsVirtual, completeLegs[i].IsVirtual);
			}
		});
	}
}
