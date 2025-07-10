using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business.Testing
{
	sealed class FreightRatingHelperTest : BaseFreightTest
	{
		void AssertArraysAreEquivalent(object[] expected, object[] actual, string[] excludedProperties)
		{
			AssertEquals(expected.Length, actual.Length);

			for (var i = 0; i < expected.Length; i++)
			{
				foreach (var property in expected[i].GetType().GetProperties().Where(p => !excludedProperties.Contains(p.Name)))
				{
					AssertEquals(expected[i].GetPropertyValue(property.Name), actual[i].GetPropertyValue(property.Name));
				}
			}
		}

		public void TestCalculateFreightMode_LCLContainer()
		{
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			measures.AddLCL("GEN", 3m, Constants.Weight.Kilograms, 3m, Constants.Volume.CubicMetres, 3);

			CombineAssertions(() =>
			{
				using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("Disable AutorateByBBK_BLK_ROR_BCNContainerModes: BBK", FreightMode.LCL, FreightRatingHelper.CalculateFreightMode(Constants.TransportModes.Sea, Constants.ContainerModes.BreakBulk, measures));
					AssertEquals("Disable AutorateByBBK_BLK_ROR_BCNContainerModes: BLK", FreightMode.LCL, FreightRatingHelper.CalculateFreightMode(Constants.TransportModes.Sea, Constants.ContainerModes.Bulk, measures));
					AssertEquals("Disable AutorateByBBK_BLK_ROR_BCNContainerModes: ROR", FreightMode.LCL, FreightRatingHelper.CalculateFreightMode(Constants.TransportModes.Sea, Constants.ContainerModes.RollOnRollOff, measures));
					AssertEquals("Disable AutorateByBBK_BLK_ROR_BCNContainerModes: BCN", FreightMode.LCL, FreightRatingHelper.CalculateFreightMode(Constants.TransportModes.Sea, Constants.ContainerModes.BuyersConsol, measures));
				}

				using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("Enable AutorateByBBK_BLK_ROR_BCNContainerModes: BBK", FreightMode.BBK, FreightRatingHelper.CalculateFreightMode(Constants.TransportModes.Sea, Constants.ContainerModes.BreakBulk, measures));
					AssertEquals("Enable AutorateByBBK_BLK_ROR_BCNContainerModes: BLK", FreightMode.BLK, FreightRatingHelper.CalculateFreightMode(Constants.TransportModes.Sea, Constants.ContainerModes.Bulk, measures));
					AssertEquals("Enable AutorateByBBK_BLK_ROR_BCNContainerModes: ROR", FreightMode.ROR, FreightRatingHelper.CalculateFreightMode(Constants.TransportModes.Sea, Constants.ContainerModes.RollOnRollOff, measures));
					AssertEquals("Enable AutorateByBBK_BLK_ROR_BCNContainerModes: BCN", FreightMode.LCL | FreightMode.BCN, FreightRatingHelper.CalculateFreightMode(Constants.TransportModes.Sea, Constants.ContainerModes.BuyersConsol, measures));
				}
			});
		}

		public void TestCalculateFreightMode_FCLContainer()
		{
			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var containerInfo = new MeasureInfo.ContainerInfo(3m, Constants.Weight.Kilograms, 3m, Constants.Volume.CubicMetres, 3, 3, "DFDF1111116");
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			measures.AddContainerGroup(gp20.PK, "GEN", new[] { containerInfo });

			CombineAssertions(() =>
			{
				using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("Disable AutorateByBBK_BLK_ROR_BCNContainerModes: BBK (has only LCL option)", FreightMode.LCL, FreightRatingHelper.CalculateFreightMode(Constants.TransportModes.Sea, Constants.ContainerModes.BreakBulk, measures));
					AssertEquals("Disable AutorateByBBK_BLK_ROR_BCNContainerModes: BLK (has only LCL option)", FreightMode.LCL, FreightRatingHelper.CalculateFreightMode(Constants.TransportModes.Sea, Constants.ContainerModes.Bulk, measures));
					AssertEquals("Disable AutorateByBBK_BLK_ROR_BCNContainerModes: ROR (has only LCL option)", FreightMode.LCL, FreightRatingHelper.CalculateFreightMode(Constants.TransportModes.Sea, Constants.ContainerModes.RollOnRollOff, measures));
					AssertEquals("Disable AutorateByBBK_BLK_ROR_BCNContainerModes: BCN", FreightMode.FCL, FreightRatingHelper.CalculateFreightMode(Constants.TransportModes.Sea, Constants.ContainerModes.BuyersConsol, measures));
				}

				using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("Enable AutorateByBBK_BLK_ROR_BCNContainerModes: BBK", FreightMode.BBK, FreightRatingHelper.CalculateFreightMode(Constants.TransportModes.Sea, Constants.ContainerModes.BreakBulk, measures));
					AssertEquals("Enable AutorateByBBK_BLK_ROR_BCNContainerModes: BLK", FreightMode.BLK, FreightRatingHelper.CalculateFreightMode(Constants.TransportModes.Sea, Constants.ContainerModes.Bulk, measures));
					AssertEquals("Enable AutorateByBBK_BLK_ROR_BCNContainerModes: ROR", FreightMode.ROR, FreightRatingHelper.CalculateFreightMode(Constants.TransportModes.Sea, Constants.ContainerModes.RollOnRollOff, measures));
					AssertEquals("Enable AutorateByBBK_BLK_ROR_BCNContainerModes: BCN", FreightMode.FCL | FreightMode.BCN, FreightRatingHelper.CalculateFreightMode(Constants.TransportModes.Sea, Constants.ContainerModes.BuyersConsol, measures));
				}
			});
		}

		public void TestConvert()
		{
			AssertEquals(100000m, FreightRatingHelper.Convert(100m, "KG", "G"));
			AssertEquals(3m, FreightRatingHelper.Convert(3000m, "G", "KG"));
			AssertEquals(35.314667m, FreightRatingHelper.Convert(1m, "M3", "CF"));

			bool exceptionThrown = false;
			try
			{
				FreightRatingHelper.Convert(1m, "M3", "KG");
			}
			catch
			{
				exceptionThrown = true;
			}
			AssertEquals("Exception thrown when converting from vol to weight", true, exceptionThrown);

			exceptionThrown = false;
			try
			{
				FreightRatingHelper.Convert(1m, "KG", "M3");
			}
			catch
			{
				exceptionThrown = true;
			}
			AssertEquals("Exception thrown when converting from weight to vol", true, exceptionThrown);
		}

		#region TestGetShipmentContainers_WithoutContainers

		public void TestGetShipmentContainers_WithoutContainers()
		{
			var shipment = Factory.New<CommonShipment>();

			var measures = new RateableMeasureSet();
			FreightRatingHelper.SetShippingContainers(measures, shipment.Containers, shipment);

			AssertEquals("Should be the containers unit", "CN", measures.GetUnit(MeasureType.ContainerCount));
			AssertEquals("No Containers", 0m, measures.GetActual(MeasureType.ContainerCount));
		}

		#endregion

		#region Get Containers

		public void TestGetContainersTEUs()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = RC_20GP_PK;
			container1.JC_ContainerCount = 3;
			container1.JC_GrossWeight = 30000;
			container1.JC_GrossVolume = 12000;

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = RC_40RE_PK;
			container2.JC_ContainerCount = 1;
			container2.JC_GrossWeight = 25000;
			container2.JC_GrossVolume = 7000;
			container2.JC_ContainerNum = "DFDF1212127";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_ActualVolume = 2000;
			packLine1.JL_ActualWeight = 3000;
			packLine1.JL_JC = container1.PK;
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = container2.PK;
			packLine2.JL_ActualVolume = 1000;
			packLine2.JL_ActualWeight = 2000;
			packLine2.JL_PackageCount = 5;

			var measures = (RateableMeasureSet)shipment.RatingAdapter.RateableMeasures;

			var teus = measures.GetAllContainers().Select(x => x.TEU).ToList();
			AssertEquals(3m, teus[0]);
			AssertEquals(2m, teus[1]);
		}

		public void TestGetContainerShipmentShares()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();

			var container = consol.Containers.AddNew();
			container.JC_RC = RC_20GP_PK;
			container.JC_ContainerCount = 1;
			container.JC_GrossWeight = 30000;
			container.JC_GrossVolume = 12000;
			container.JC_ContainerNum = "DFDF1212127";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Constants.TransportModes.Sea;
			shipment1.JS_PackingMode = Constants.ContainerModes.FCL;

			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_ActualVolume = 3000m;
			packLine1.JL_ActualWeight = 3000;
			packLine1.JL_JC = container.PK;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			shipment2.JS_PackingMode = Constants.ContainerModes.FCL;

			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_JC = container.PK;
			packLine2.JL_ActualVolume = 2000;
			packLine2.JL_ActualWeight = 2000;
			packLine2.JL_PackageCount = 5;

			var measures = (RateableMeasureSet)shipment1.RatingAdapter.RateableMeasures;

			var shipmentShares = measures.GetAllContainers().Select(x => x.ShipmentShare).ToList();
			AssertEquals(0.6m, shipmentShares[0]);

			measures = (RateableMeasureSet)shipment2.RatingAdapter.RateableMeasures;

			shipmentShares = measures.GetAllContainers().Select(x => x.ShipmentShare).ToList();
			AssertEquals(0.4m, shipmentShares[0]);
		}

		public void TestGetContainerShipmentSharesWhenRefContainerIsNull()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();

			var container = consol.Containers.AddNew();
			container.JC_RC = ZGuid.BrettsGuid;
			container.JC_ContainerCount = 1;
			container.JC_GrossWeight = 30000;
			container.JC_GrossVolume = 12000;
			container.JC_ContainerNum = "DFDF1212127";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Constants.TransportModes.Sea;
			shipment1.JS_PackingMode = Constants.ContainerModes.FCL;

			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_ActualVolume = 3000m;
			packLine1.JL_ActualWeight = 3000;
			packLine1.JL_JC = container.PK;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			shipment2.JS_PackingMode = Constants.ContainerModes.FCL;

			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_JC = container.PK;
			packLine2.JL_ActualVolume = 2000;
			packLine2.JL_ActualWeight = 2000;
			packLine2.JL_PackageCount = 5;

			var measures = (RateableMeasureSet)shipment1.RatingAdapter.RateableMeasures;

			var shipmentShares = measures.GetAllContainers().Select(x => x.ShipmentShare).ToList();
			AssertEquals(1m, shipmentShares[0]);

			measures = (RateableMeasureSet)shipment2.RatingAdapter.RateableMeasures;

			shipmentShares = measures.GetAllContainers().Select(x => x.ShipmentShare).ToList();
			AssertEquals(1m, shipmentShares[0]);
		}

		public void TestGetContainerSpotRates()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.CreditorPK = creditor.PK;

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var container = consol.Containers.AddNew();
			container.JC_RC = RC_20GP_PK;
			container.JC_ContainerCount = 1;
			container.JC_GrossWeight = 30000;
			container.JC_GrossVolume = 12000;
			container.JC_ContainerNum = "DFDF1212127";

			container.JC_SellSpotRate = 100m;
			container.JC_CostSpotRate = 90m;
			container.JC_CostSpotRateMode = Constants.FreightRateAutoratingModes.Code.AllInRate;
			container.JC_GatewaySellSpotRate = 80m;

			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_ActualVolume = 3000m;
			packLine1.JL_ActualWeight = 3000;
			packLine1.JL_JC = container.PK;
			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_JC = container.PK;
			packLine2.JL_ActualVolume = 2000;
			packLine2.JL_ActualWeight = 2000;
			packLine2.JL_PackageCount = 5;

			shipment1.JS_TransportMode = Constants.TransportModes.Sea;
			shipment1.JS_PackingMode = Constants.ContainerModes.FCL;

			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			shipment2.JS_PackingMode = Constants.ContainerModes.FCL;

			var measures = (RateableMeasureSet)shipment1.RatingAdapter.RateableMeasures;

			var spotRates = measures.GetContainerSpotRates_ForTest();
			AssertEquals("Should get SpotRates", 1, spotRates.Count);
			var spotCostRate = spotRates[0].CostSpotRate;
			var spotSellRate = spotRates[0].SellSpotRate;

			AssertEquals("Should use SellSpotRate", 100m, spotSellRate.Rate.Amount);
			AssertEquals("CostSpotRate", 90m, spotCostRate.Rate.Amount);
			AssertEquals("Correct SpotRateMode", Constants.FreightRateAutoratingModes.Code.AllInRate, spotCostRate.AutoratedMode);
			AssertEquals("Correct Creditor", creditor.PK, spotCostRate.Creditor.PK);
		}

		public void TestGetContainerSpotRates_GatewaySellSpotRatePreferred()
		{
			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var gatewayAgentPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "AUBNE";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var shipment = consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();
			container.JC_RC = RC_20GP_PK;

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_JC = container.PK;

			container.JC_GatewaySellSpotRate = 150m;
			container.JC_SellSpotRate = 180m;

			AssertNoWarnings(container.JC_GatewaySellSpotRateModeInfo);
			var job = new JobHeader.Loader(shipment).TryLoadOrCreate();
			job.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			var measures = (RateableMeasureSet)shipment.RatingAdapter.RateableMeasures;

			AssertNotNull(shipment.Job);
			var spotRates = measures.GetContainerSpotRates_ForTest();
			AssertEquals("Should get SpotRates", 1, spotRates.Count);
			var spotCostRate = spotRates[0].CostSpotRate;
			var spotSellRate = spotRates[0].SellSpotRate;

			AssertEquals("Should use One Off Freight Rate for Sell", 180m, spotSellRate.Rate.Amount);
			AssertEquals("Should use GatewaySellRate for SpotCost", 150m, spotCostRate.Rate.Amount);
			AssertEquals("Correct SpotRateMode", Constants.FreightRateAutoratingModes.Code.FreightPlusRate, spotCostRate.AutoratedMode);
			AssertEquals("Correct creditor", GlbCompany.CurrentCompany.OrgProxy.PK, spotCostRate.Creditor.PK);
		}

		public void TestContainerCountMeasures_DifferentContainerTypesAndContainerModes()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();

			#region Containers

			#region FCLs

			var container1 = CreateContainerForTest(consol, RC_20GP_PK, 1, "ALUM", Constants.ContainerModes.FCL, "ABC");
			CreatePackLineForTest(consol, shipment, container1, 1110, 2.11, 11);
			CreatePackLineForTest(consol, shipment, container1, 1120, 2.12, 12);

			var container2 = CreateContainerForTest(consol, RC_20GP_PK, 2, "BEDD", Constants.ContainerModes.FCL);
			CreatePackLineForTest(consol, shipment, container2, 1210, 2.21, 21);
			CreatePackLineForTest(consol, shipment, container2, 1220, 2.22, 22);

			var container3 = CreateContainerForTest(consol, RC_40GP_PK, 1, "CBLS", Constants.ContainerModes.FCL, "XYZ1");
			CreatePackLineForTest(consol, shipment, container3, 1300, 2.3, 30);

			var container4 = CreateContainerForTest(consol, RC_40GP_PK, 1, "DAIR", Constants.ContainerModes.FCL, "XYZ2");
			CreatePackLineForTest(consol, shipment, container4, 1400, 2.4, 40);

			var container5 = CreateContainerForTest(consol, RC_40GP_PK, 3, "EEQP", Constants.ContainerModes.FCL);
			CreatePackLineForTest(consol, shipment, container5, 1500, 2.5, 50);

			var container6 = CreateContainerForTest(consol, RC_40GP_PK, 4, "FBRC", Constants.ContainerModes.FCL);
			CreatePackLineForTest(consol, shipment, container6, 1600, 2.6, 60);

			// different to container1 by container number
			var container7 = CreateContainerForTest(consol, RC_20GP_PK, 1, "ALUM", Constants.ContainerModes.FCL, "DEF");
			CreatePackLineForTest(consol, shipment, container7, 1700, 2.7, 70);

			#endregion

			#region LCLs

			var container8 = CreateContainerForTest(consol, RC_20RE_PK, 1, "MACH", Constants.ContainerModes.LCL, "LCL001");
			CreatePackLineForTest(consol, shipment, container8, 1800, 2.8, 80);

			var container9 = CreateContainerForTest(consol, RC_20RE_PK, 5, "UMBR", Constants.ContainerModes.LCL);
			CreatePackLineForTest(consol, shipment, container9, 1900, 2.9, 90);

			#endregion

			#endregion

			var rateableMeasures = (RateableMeasureSet)consol.RatingAdapter.RateableMeasures;
			var containers = rateableMeasures.GetAllContainers();
			var containerCount = containers.Sum(i => i.ContainerCount);
			AssertEquals("Total number of containers (1 + 2 + 1 + 1 + 3 + 4 + 1 = 13) FCL", 13, containerCount);
			Assert(rateableMeasures.ContainerListHasContainerNumber);
			Assert(rateableMeasures.ContainerListHasContainerOwnership);

			var expectedContainerGroups = new[]
			{
				CreateContainerGroupForTest(2230, 4.23, 23, "ABC", RC_20GP_PK, "ALUM", "CAR"),
				CreateContainerGroupForTest(2430, 4.43, 43, "", RC_20GP_PK, "BEDD", "CAR", 2),
				CreateContainerGroupForTest(1300, 2.3, 30, "XYZ1", RC_40GP_PK, "CBLS", "CAR"),
				CreateContainerGroupForTest(1400, 2.4, 40, "XYZ2", RC_40GP_PK, "DAIR", "CAR"),
				CreateContainerGroupForTest(1500, 2.5, 50, "", RC_40GP_PK, "EEQP", "CAR", 3),
				CreateContainerGroupForTest(1600, 2.6, 60, "", RC_40GP_PK, "FBRC", "CAR", 4),
				CreateContainerGroupForTest(1700, 2.7, 70, "DEF", RC_20GP_PK, "ALUM", "CAR"),
				CreateContainerGroupForTest(3700, 5.7, 170, "", MeasureInfo.ContainerInfo.LCL, "", ""),
			}.OrderBy(c => c.ContainerTypePK).ToArray();

			var actualContainerGroups = rateableMeasures.GetContainerGroups().OrderBy(c => c.ContainerTypePK).ToArray();
			AssertArraysAreEquivalent(
				expectedContainerGroups,
				actualContainerGroups,
				[nameof(RateableMeasureSet.ContainerGroup.Containers)]);

			AssertEquals("Total package count (23 + 43 + 30 + 40 + 50 + 60 + 70 + 170)", 486m, rateableMeasures.GetActual(MeasureType.Package));
		}

		public void TestContainerCountMeasures_DifferentWeightAndVolumeUnits()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();

			var container = CreateContainerForTest(consol, RC_20GP_PK, 1, "ALUM", Constants.ContainerModes.FCL, "ABC");
			CreatePackLineForTest(consol, shipment, container, 1000, 1, 10);
			CreatePackLineForTest(consol, shipment, container, 1000, 1, 10, Constants.Weight.Pounds, Constants.Volume.CubicFeet);

			var rateableMeasures = (RateableMeasureSet)consol.RatingAdapter.RateableMeasures;
			var expectedContainerGroups = new[]
			{
				CreateContainerGroupForTest(1000 + 0.453592 * 1000, 1 + 1 * 0.028, 20, "ABC", RC_20GP_PK, "ALUM", "CAR"),
			};

			var actualContainerGroups = rateableMeasures.GetContainerGroups().ToList();
			AssertArraysAreEquivalent(
				expectedContainerGroups,
				actualContainerGroups.ToArray(),
				[nameof(RateableMeasureSet.ContainerGroup.Containers)]);

			AssertEquals("Total package count", 20m, rateableMeasures.GetActual(MeasureType.Package));
		}

		public void TestContainerCountMeasures_DifferentContainerNumberThenDifferentPoint()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();

			var container1 = CreateContainerForTest(consol, RC_20GP_PK, 1, "ALUM", Constants.ContainerModes.FCL);
			CreatePackLineForTest(consol, shipment, container1, 1000, 1, 10);

			var container2 = CreateContainerForTest(consol, RC_20GP_PK, 1, "ALUM", Constants.ContainerModes.FCL);
			CreatePackLineForTest(consol, shipment, container2, 1000, 1, 10);

			var container3 = CreateContainerForTest(consol, RC_20GP_PK, 1, "ALUM", Constants.ContainerModes.FCL, "NUMBER");
			CreatePackLineForTest(consol, shipment, container3, 1000, 1, 10);

			var rateableMeasures = (RateableMeasureSet)consol.RatingAdapter.RateableMeasures;
			var expectedContainerGroups = new[]
			{
				CreateContainerGroupForTest(1000, 1, 10, string.Empty, RC_20GP_PK, "ALUM", "CAR", 2),
				CreateContainerGroupForTest(1000, 1, 10, "NUMBER", RC_20GP_PK, "ALUM", "CAR", 1),
			};

			var actualContainerGroups = rateableMeasures.GetContainerGroups().ToList();
			AssertArraysAreEquivalent(
				expectedContainerGroups,
				actualContainerGroups.ToArray(),
				[nameof(RateableMeasureSet.ContainerGroup.Containers)]);

			Assert(true);
		}

		static CommonContainer CreateContainerForTest(CommonConsol consol, ZGuid containerTypePK, ZShort containerCount, ZString commodityCode, string containerMode, string containerNumber = null)
		{
			var container = consol.Containers.AddNew();
			container.JC_ContainerMode = containerMode;

			container.JC_RC = containerTypePK;
			container.JC_ContainerCount = containerCount;
			container.JC_RH_NKContainerCommodityCode = commodityCode;

			if (!string.IsNullOrWhiteSpace(containerNumber))
			{
				container.JC_ContainerNum = containerNumber;
			}

			return container;
		}

		static void CreatePackLineForTest(CommonConsol consol, CommonShipment shipment, CommonContainer container, ZDecimal weight, ZDecimal volume, ZInt packageCount, string actualWeightUQ = Constants.Weight.Kilograms, string actualVolumeUQ = Constants.Volume.CubicMetres)
		{
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);

			packLine.JL_UnitOfDimension = actualVolumeUQ;
			packLine.JL_Width = 1;
			packLine.JL_Height = 1;
			packLine.JL_Length = 1;

			packLine.JL_ActualWeight = weight;
			packLine.JL_ActualVolume = volume;
			packLine.JL_PackageCount = packageCount;

			packLine.JL_ActualWeightUQ = actualWeightUQ;
			packLine.JL_ActualVolumeUQ = actualVolumeUQ;
		}

		static RateableMeasureSet.ContainerGroup CreateContainerGroupForTest(ZDecimal weight, ZDecimal volume, ZInt packages, ZString containerNumber, ZGuid containerTypePK, ZString commodityCode, ZString containerOwnership, int containerCount = 1, string containerQuality = null)
		{
			// Note, not currently comparing ContainerInfos so only the count matters
			var containerMock = new Mock<IRateableContainer>();
			containerMock.Setup(x => x.ContainerWeightInKG).Returns(weight);
			containerMock.Setup(x => x.ContainerVolumeInM3).Returns(volume);
			containerMock.Setup(x => x.PackageCount).Returns(packages);
			containerMock.Setup(x => x.ContainerNumber).Returns(containerNumber);
			containerMock.Setup(x => x.TEU).Returns(1);
			containerMock.Setup(x => x.ContainerCount).Returns(containerCount);
			containerMock.Setup(x => x.ContainerQuality).Returns(containerQuality ?? string.Empty);

			return new RateableMeasureSet.ContainerGroup()
			{
				ContainerTypePK = containerTypePK,
				ContainerQuality = containerQuality ?? string.Empty,
				ContainerNumber = containerNumber,
				CommodityCode = commodityCode,
				Ownership = containerOwnership,
				ContainerCount = containerCount,
				Containers = new[] { containerMock.Object }
			};
		}

		#endregion

		#region TestAIRCONShipmentIsULD

		public void TestAIRCONShipmentIsULD()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_RC = RC_20GP_PK;
			container1.JC_ContainerCount = 3;
			container1.JC_GrossWeight = 30000;

			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_RC = RC_40RE_PK;
			container2.JC_ContainerCount = 1;
			container2.JC_GrossWeight = 25000;
			container2.JC_ContainerNum = "DFDF1212127";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_JC = container1.PK;
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = container2.PK;

			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "CON";

			AssertEquals(FreightMode.ULD, shipment.RatingAdapter.FreightMode);
		}

		#endregion

		#region TestContainerCommodityCode

		public void TestContainerCommodityCode()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_RC = RC_20GP_PK;
			container.JC_RH_NKContainerCommodityCode = "APPL";

			var shipment = consol.Shipments.AddNew();
			var job = new JobHeader.Loader(shipment).TryLoadOrCreate();
			var shipmentPackLine = shipment.OuterPackLines.AddNew();
			shipmentPackLine.JL_JC = container.PK;
			shipmentPackLine.JL_RH_NKCommodityCode = "BANA";

			var measures = new RateableMeasureSet();
			FreightRatingHelper.SetShippingContainers(measures, shipment.Containers, shipment);

			AssertEquals("Container itself has a commodity", "APPL", measures.GetContainerGroups().Single().CommodityCode);

			container.JC_RH_NKContainerCommodityCode = "";
			measures = new RateableMeasureSet();
			FreightRatingHelper.SetShippingContainers(measures, shipment.Containers, shipment);
			AssertEquals("1 pack-line with commodity", "BANA", measures.GetContainerGroups().Single().CommodityCode);

			var shipmentPackLine2 = shipment.OuterPackLines.AddNew();
			shipmentPackLine2.JL_JC = container.PK;
			shipmentPackLine2.JL_RH_NKCommodityCode = "BANA";
			measures = new RateableMeasureSet();
			FreightRatingHelper.SetShippingContainers(measures, shipment.Containers, shipment);
			AssertEquals("Same commodity on both pack lines", "BANA", measures.GetContainerGroups().Single().CommodityCode);

			shipmentPackLine2.JL_RH_NKCommodityCode = "GENL";
			measures = new RateableMeasureSet();
			FreightRatingHelper.SetShippingContainers(measures, shipment.Containers, shipment);
			AssertEquals("Different commodities - cannot figure out container commodity", "", measures.GetContainerGroups().Single().CommodityCode);
		}

		public void TestContainerIsNonOperatedReefer_SetShippingContainers()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_RC = RC_20GP_PK;
			container.JC_IsNonOperativeReefer = true;

			var shipment = consol.Shipments.AddNew();
			var job = new JobHeader.Loader(shipment).TryLoadOrCreate();
			var shipmentPackLine = shipment.OuterPackLines.AddNew();
			shipmentPackLine.JL_JC = container.PK;

			var measures = new RateableMeasureSet();

			FreightRatingHelper.SetShippingContainers(measures, shipment.Containers, shipment);
			var isNonOperatedReeferValue = measures.GetDistinctContainerIsNonOperatingReefers();

			AssertCollectionContains(true, isNonOperatedReeferValue);

			container.JC_IsNonOperativeReefer = false;
			measures = new RateableMeasureSet();

			FreightRatingHelper.SetShippingContainers(measures, shipment.Containers, shipment);
			isNonOperatedReeferValue = measures.GetDistinctContainerIsNonOperatingReefers();

			AssertCollectionContains(false, isNonOperatedReeferValue);
		}

		public void TestContainerIsNonOperatedReefer_SetContainers()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_RC = RC_20GP_PK;
			container.JC_IsNonOperativeReefer = true;

			var shipment = consol.Shipments.AddNew();
			var job = new JobHeader.Loader(shipment).TryLoadOrCreate();
			var shipmentPackLine = shipment.OuterPackLines.AddNew();
			shipmentPackLine.JL_JC = container.PK;

			var measures = new RateableMeasureSet();
			var addLCL = false;
			ZDecimal lclWeight = 10m;
			ZDecimal lclVolume = 20m;
			var lclInfo = addLCL
				? new MeasureInfo.ContainerInfo(lclWeight, Constants.Weight.Kilograms, lclVolume, Constants.Volume.CubicMetres, 0, 0, ZString.Empty, containerCount: 0)
			: null;

			var containerList = new List<ContainerAndMassAndVolumeHelper>
			{
				new ContainerAndMassAndVolumeHelper(container)
			};

			FreightRatingHelper.SetContainers(measures, shipment, containerList, lclInfo);
			var isNonOperatedReeferValue = measures.GetDistinctContainerIsNonOperatingReefers();

			AssertCollectionContains(true, isNonOperatedReeferValue);

			container.JC_IsNonOperativeReefer = false;
			measures = new RateableMeasureSet();

			FreightRatingHelper.SetContainers(measures, shipment, containerList, lclInfo);
			isNonOperatedReeferValue = measures.GetDistinctContainerIsNonOperatingReefers();

			AssertCollectionContains(false, isNonOperatedReeferValue);
		}

		public void TestContainerIsNonOperatedReefer_CreateContainerInfo()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_RC = RC_20GP_PK;
			container.JC_IsNonOperativeReefer = true;

			var shipment = consol.Shipments.AddNew();
			var job = new JobHeader.Loader(shipment).TryLoadOrCreate();
			var shipmentPackLine = shipment.OuterPackLines.AddNew();
			shipmentPackLine.JL_JC = container.PK;

			var containerInfo = FreightRatingHelper.CreateContainerInfo(shipment.Containers.First(), shipment);
			AssertEquals(true, containerInfo.IsNonOperatingReefer);

			container.JC_IsNonOperativeReefer = false;
			containerInfo = FreightRatingHelper.CreateContainerInfo(shipment.Containers.First(), shipment);
			AssertEquals(false, containerInfo.IsNonOperatingReefer);
		}

		public void TestContainerSpotRates_SetShippingContainers()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TST000032";
			container.JC_CostSpotRate = 25m;
			container.JC_SellSpotRate = 45m;

			container.JC_RX_NKSellSpotRateCurrency = "USD";
			container.JC_RX_NKCostSpotRateCurrency = "USD";

			var shipment = consol.Shipments.AddNew();
			var shipmentPackLine = shipment.OuterPackLines.AddNew();
			shipmentPackLine.JL_JC = container.PK;

			var measures = new RateableMeasureSet();
			FreightRatingHelper.SetShippingContainers(measures, shipment.Containers, shipment);
			var spotRates = measures.GetContainerSpotRates_ForTest();
			Assert("Should add ContainerSpotRates", spotRates.Any());
			Assert("ContainerCostSpotRate should be valid", spotRates[0].CostSpotRateIsValid);
			Assert("ContainerSellSpotRate should be valid", spotRates[0].SellSpotRateIsValid);
		}

		#endregion
			
		#region TestGetServiceInfoFromContainers

		public void TestGetServiceInfoFromContainers()
		{
			var containerCollection = new List<CommonContainer>();

			var serviceInfos = FreightRatingHelper.GetServiceInfosFromContainers(containerCollection, Constants.FreightServiceType.Codes.Fumigation, FreightRatingHelper.GetServiceInfoDefault);
			AssertEquals(1, serviceInfos.Count());
			Assert(!serviceInfos.FirstOrDefault().IsEnabled);

			var container1 = Factory.NewWithValidTestData<CommonContainer>();
			container1.JC_ContainerNum = "FAKE4100011";
			container1.JC_RC = RC_20GP_PK;
			var service1 = container1.Services.AddNew();
			service1.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			service1.ES_Booked = new ZDateTime(2012, 10, 23);
			service1.ES_Completed = new ZDateTime(2012, 10, 23);
			service1.ES_ServiceCount = 2;
			containerCollection.Add(container1);

			serviceInfos = FreightRatingHelper.GetServiceInfosFromContainers(containerCollection, Constants.FreightServiceType.Codes.Fumigation, FreightRatingHelper.GetServiceInfoDefault);
			Assert(serviceInfos.Any() && serviceInfos.All(x => x.IsEnabled));
			AssertEquals((ZDecimal)2, serviceInfos.Sum(x => x.ServiceCount));

			var container2 = Factory.NewWithValidTestData<CommonContainer>();
			container2.JC_ContainerNum = "FAKE4100027";
			container2.JC_RC = RC_20GP_PK;
			var service2 = container2.Services.AddNew();
			service2.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			service2.ES_Booked = new ZDateTime(2012, 10, 23);
			service2.ES_Completed = new ZDateTime(2012, 10, 23);
			service2.ES_ServiceCount = 5;
			containerCollection.Add(container2);

			serviceInfos = FreightRatingHelper.GetServiceInfosFromContainers(containerCollection, Constants.FreightServiceType.Codes.Fumigation, FreightRatingHelper.GetServiceInfoDefault);
			Assert(serviceInfos.Any() && serviceInfos.All(x => x.IsEnabled));
			AssertEquals((ZDecimal)7, serviceInfos.Sum(x => x.ServiceCount));

			var container3 = Factory.NewWithValidTestData<CommonContainer>();
			container3.JC_ContainerNum = "FAKE4100032";
			container3.JC_RC = RC_20GP_PK;
			var service3 = container3.Services.AddNew();
			service3.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			service3.ES_Booked = new ZDateTime(2012, 10, 23);
			service3.ES_Completed = new ZDateTime(2012, 10, 23);
			service3.ES_ServiceCount = 5;
			service3.ES_OH_Contractor = Factory.NewWithValidTestData<OrgHeader>().PK;
			containerCollection.Add(container3);

			serviceInfos = FreightRatingHelper.GetServiceInfosFromContainers(containerCollection, Constants.FreightServiceType.Codes.Fumigation, FreightRatingHelper.GetServiceInfoDefault);
			Assert(serviceInfos.Any() && serviceInfos.All(x => x.IsEnabled));
			AssertEquals((ZDecimal)0, serviceInfos.Sum(x => x.ServiceCount));
			AssertEquals("FUM service has been performed on multiple containers by multiple contractors which is not supported.", serviceInfos.FirstOrDefault().FaultMessage);
		}

		public void TestGetServiceInfoFromContainers_EachFromOneContainer()
		{
			var containers = new List<CommonContainer>();

			var refContainer20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var refContainer40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			var refContainer20RE = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;

			var baseTime = new DateTime(DateTime.Now.Year, 1, 1);

			Func<ZGuid, int, CommonContainer> createContainerWithService = (refContainerPK, i) =>
			{
				var container = Factory.NewWithValidTestData<CommonContainer>();
				container.JC_RC = refContainerPK;

				var service = container.Services.AddNew();
				service.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
				service.ES_Completed = ZDateTime.Today;
				service.ES_ServiceCount = i;
				service.ES_Duration = baseTime.AddHours(i);

				return container;
			};

			var container1 = createContainerWithService(refContainer20GP, 1);
			containers.Add(container1);

			var container2 = createContainerWithService(refContainer20GP, 2);
			containers.Add(container2);

			var container3 = createContainerWithService(refContainer40GP, 4);
			containers.Add(container3);

			var container4 = createContainerWithService(refContainer40GP, 8);
			containers.Add(container4);

			var container5 = createContainerWithService(refContainer40GP, 1);
			containers.Add(container5);

			var container6 = createContainerWithService(refContainer20RE, 0);
			containers.Add(container6);

			var serviceInfos = FreightRatingHelper.GetServiceInfosFromContainers(containers, Constants.FreightServiceType.Codes.Fumigation, FreightRatingHelper.GetServiceInfoDefault);

			var expected = new[]
			{
				$"FUM|{refContainer20GP}|{container1.PK}|1|01:00:00",
				$"FUM|{refContainer20GP}|{container2.PK}|2|02:00:00",
				$"FUM|{refContainer40GP}|{container3.PK}|4|04:00:00",
				$"FUM|{refContainer40GP}|{container4.PK}|8|08:00:00",
				$"FUM|{refContainer40GP}|{container5.PK}|1|01:00:00",
				$"FUM|{refContainer20RE}|{container6.PK}|0|00:00:00"
			};

			AssertContainsExactElementsInAnyOrder("Each container has 1 service info", expected, serviceInfos.Select(x => $"{x.ServiceCode}|{x.ContainerType}|{x.Container}|{x.ServiceCount}|{x.ServiceDuration}"));
		}

		public void TestHiddenContainerServices()
		{
			var carrier = Factory.New<OrgHeader>();
			var consolCreditor = Factory.New<OrgHeader>();
			var arrivalCTO = Factory.New<OrgHeader>();
			var arrivalUnpackCFSTransport = Factory.New<OrgHeader>();

			var penaltyCreditor1 = Factory.New<OrgHeader>();
			var penaltyCreditor2 = Factory.New<OrgHeader>();
			var penaltyCreditor3 = Factory.New<OrgHeader>();
			var penaltyCreditor4 = Factory.New<OrgHeader>();

			AssertEquals(5, FreightRatingHelper.HiddenContainerServices.Count);
			Assert(FreightRatingHelper.HiddenContainerServices.ContainsCode(ChargeCodeSubGroupList.CartageDemurrageTotal));
			Assert(FreightRatingHelper.HiddenContainerServices.ContainsCode(ChargeCodeSubGroupList.ContainerDetention));
			Assert(FreightRatingHelper.HiddenContainerServices.ContainsCode(ChargeCodeSubGroupList.Storage));
			Assert(FreightRatingHelper.HiddenContainerServices.ContainsCode(ChargeCodeSubGroupList.CarrierStorage));
			Assert(FreightRatingHelper.HiddenContainerServices.ContainsCode(ChargeCodeSubGroupList.MergedDemurrageDetention));

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var consol1 = Factory.NewWithValidTestData<CommonConsol>();
			consol1.JK_IsForwarding = true;
			consol1.CreditorPK = consolCreditor.PK;
			consol1.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol1.JK_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;
			consol1.JK_OA_ArrivalUnpackCFSTransportAddress = arrivalUnpackCFSTransport.MainAddress.PK;

			var container = consol1.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = refContainer.PK;

			Assert("hidden service is not enabled", !FreightRatingHelper.GetServiceInfoDefault(container, ChargeCodeSubGroupList.ContainerDetention).Any(x => x.IsEnabled));
			Assert(!FreightRatingHelper.GetServiceInfoDefault(container, ChargeCodeSubGroupList.Storage).Any(x => x.IsEnabled));
			Assert(!FreightRatingHelper.GetServiceInfoDefault(container, ChargeCodeSubGroupList.CarrierStorage).Any(x => x.IsEnabled));
			Assert(!FreightRatingHelper.GetServiceInfoDefault(container, ChargeCodeSubGroupList.CartageDemurrageTotal).Any(x => x.IsEnabled));
			Assert(!FreightRatingHelper.GetServiceInfoDefault(container, ChargeCodeSubGroupList.MergedDemurrageDetention).Any(x => x.IsEnabled));

			container.ArrivalCarrierDetentionDays = new ZByte(2);
			var detentionPenalty = container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false);
			detentionPenalty.CPY_PerUnitCost = 250m;
			detentionPenalty.CPY_OH_Creditor = carrier.PK;

			container.ArrivalCTOStorageDays = new ZByte(3);
			var storagePenalty = container.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(false);
			storagePenalty.CPY_PerUnitCost = 200m;
			storagePenalty.CPY_OH_Creditor = arrivalCTO.PK;

			var carrierStoragePenalty = container.ImportPenalties.FindOrCreateArrivalCarrierStoragePenalty(true, notCreateIfNotMatch: false);
			carrierStoragePenalty.DurationAsDays = new ZByte(3);
			carrierStoragePenalty.CPY_PerUnitCost = 275m;
			carrierStoragePenalty.CPY_OH_Creditor = carrier.PK;

			container.ArrivalTruckWaitTime = new TimeSpan(4, 4, 0);
			var truckWaitPenalty = container.FindArrivalTruckWaitPenalty();
			truckWaitPenalty.CPY_PerUnitCost = 100m;
			truckWaitPenalty.CPY_OH_Creditor = arrivalUnpackCFSTransport.PK;

			var detentionServiceInfo = FreightRatingHelper.GetServiceInfoDefault(container, ChargeCodeSubGroupList.ContainerDetention).Single();
			var storageServiceInfo = FreightRatingHelper.GetServiceInfoDefault(container, ChargeCodeSubGroupList.Storage).Single();
			var carrierStorageServiceInfo = FreightRatingHelper.GetServiceInfoDefault(container, ChargeCodeSubGroupList.CarrierStorage).Single();
			var demurrageServiceInfo = FreightRatingHelper.GetServiceInfoDefault(container, ChargeCodeSubGroupList.CartageDemurrageTotal).Single();

			detentionPenalty.CPY_OH_Creditor = penaltyCreditor1.PK;
			detentionPenalty.CPY_RX_NKCurrency = "USD";
			detentionPenalty.CPY_TotalCost = 500m;
			container.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(false).CPY_OH_Creditor = penaltyCreditor2.PK;
			container.FindArrivalTruckWaitPenalty().CPY_OH_Creditor = penaltyCreditor3.PK;
			container.ImportPenalties.FindOrCreateArrivalCarrierStoragePenalty(false).CPY_OH_Creditor = penaltyCreditor4.PK;

			var detentionWithCreditor = FreightRatingHelper.GetServiceInfoDefault(container, ChargeCodeSubGroupList.ContainerDetention).Single();
			var storageWithCreditor = FreightRatingHelper.GetServiceInfoDefault(container, ChargeCodeSubGroupList.Storage).Single();
			var carrierStorageWithCreditor = FreightRatingHelper.GetServiceInfoDefault(container, ChargeCodeSubGroupList.CarrierStorage).Single();
			var demurrageWithCreditor = FreightRatingHelper.GetServiceInfoDefault(container, ChargeCodeSubGroupList.CartageDemurrageTotal).Single();

			AssertHiddenService(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention,
				1m, TimeSpan.FromDays(2), carrier, 250m, JobServiceInfo.Constants.Codes.Day, 500m, "AUD", true, null, detentionServiceInfo);

			AssertHiddenService(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage,
				1m, TimeSpan.FromDays(3), arrivalCTO, 200m, JobServiceInfo.Constants.Codes.Day, 600m, "AUD", true, null, storageServiceInfo);

			AssertHiddenService(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CarrierStorage,
				1m, TimeSpan.FromDays(3), carrier, 275m, JobServiceInfo.Constants.Codes.Day, 825m, "AUD", true, null, carrierStorageServiceInfo);

			AssertHiddenService(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal,
				1m, new TimeSpan(4, 4, 0), arrivalUnpackCFSTransport, 100m, JobServiceInfo.Constants.Codes.Hour, 406.67m, "AUD", true, null, demurrageServiceInfo);

			// with another creditor
			AssertHiddenService(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention,
				1m, TimeSpan.FromDays(2), penaltyCreditor1, 250m, JobServiceInfo.Constants.Codes.Day, 500m, "USD", true, new[] { consolCreditor, carrier }, detentionWithCreditor);

			AssertHiddenService(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage,
				1m, TimeSpan.FromDays(3), penaltyCreditor2, 200m, JobServiceInfo.Constants.Codes.Day, 600m, "AUD", true, new[] { arrivalCTO }, storageWithCreditor);

			AssertHiddenService(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CarrierStorage,
				1m, TimeSpan.FromDays(3), penaltyCreditor4, 275m, JobServiceInfo.Constants.Codes.Day, 825m, "AUD", true, new[] { consolCreditor, carrier }, carrierStorageWithCreditor);

			AssertHiddenService(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal,
				1m, new TimeSpan(4, 4, 0), penaltyCreditor3, 100m, JobServiceInfo.Constants.Codes.Hour, 406.67m, "AUD", true, new[] { arrivalUnpackCFSTransport }, demurrageWithCreditor);

			container.ArrivalCarrierDetentionDays = ZByte.Zero;
			container.ArrivalCTOStorageDays = ZByte.Zero;
			container.ArrivalTruckWaitTime = ZDateTime.Empty;

			Assert("hidden service is enabled when TotalCost > 0 and no duration", FreightRatingHelper.GetServiceInfoDefault(container, ChargeCodeSubGroupList.ContainerDetention).Single().IsEnabled);

			detentionPenalty.CPY_TotalCost = 0;
			storagePenalty.CPY_TotalCost = 0;
			truckWaitPenalty.CPY_TotalCost = 0;
			Assert("hidden service is only enabled with a duration or toal cost", !FreightRatingHelper.GetServiceInfoDefault(container, ChargeCodeSubGroupList.ContainerDetention).Any(x => x.IsEnabled));
			Assert(!FreightRatingHelper.GetServiceInfoDefault(container, ChargeCodeSubGroupList.Storage).Any(x => x.IsEnabled));
			Assert(!FreightRatingHelper.GetServiceInfoDefault(container, ChargeCodeSubGroupList.CartageDemurrageTotal).Any(x => x.IsEnabled));

			AssertEquals("if this fails then need to update FreightRatingHelper.ConvertPenaltyTimeUnitToServiceTimeUnit", 2, detentionPenalty.Lookups.TimeUnitList.Count);
		}

		void AssertHiddenService(
			ZString chargeCodeGroup,
			ZString code,
			decimal serviceCount,
			TimeSpan duration,
			OrgHeader contractor,
			decimal rate,
			string unit,
			decimal totalCost,
			string currencyCode,
			bool isContractorCreditor,
			OrgHeader[] fallbackContractors,
			JobServiceInfo service)
		{
			CombineAssertions(() =>
			{
				Assert("IsEnabled", service.IsEnabled);
				AssertEquals("ChargeCodeGroup", chargeCodeGroup, service.ChargeCodeGroup);
				AssertEquals("ServiceCode", code, service.ServiceCode);
				AssertEquals("Rate", rate, service.Rate);
				AssertEquals("ServiceCount", serviceCount, service.ServiceCount);
				AssertEquals("TotalCost", totalCost, service.TotalCost);
				AssertEquals("Currency", currencyCode, service.Currency);
				AssertEquals("ServiceDuration", duration, service.ServiceDuration);
				AssertEquals("Unit", unit, service.Unit);
				AssertEquals("IsCost", true, service.IsCostForSpotRate);
				AssertEquals("Contractor " + contractor?.OH_Code + " = " + service.Contractor?.OH_Code, contractor, service.Contractor);
				AssertEquals("IsContractorCreditor", isContractorCreditor, service.IsContractorCreditor);
				var expectedFallbackCodes = fallbackContractors != null ? string.Join(", ", fallbackContractors.Select(x => (string)x.OH_Code)) : "";
				var actualFallbackCodes = service.FallbackContractors != null ? string.Join(", ", service.FallbackContractors.Select(x => (string)x.OH_Code)) : "";
				AssertEquals("FallbackContractors", expectedFallbackCodes, actualFallbackCodes);
			});
		}

		#endregion
	}
}
