using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.RatingTests.Testing;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business.Test.GUI
{
	public class CYDTransportationUnitIntegrationTest : BaseRatingIntegrationTest
	{
		public void TestAutoRating_TransportationUnit_GenericMatch()
		{
			var (receiveTransportationUnit, dispatchTransportationUnit) = CreateTransporationUnitForTesting();

			AutorateAndAssertTransportationUnit(receiveTransportationUnit, [(ChargeCode.DGCIN, 14), (ChargeCode.WBFIN, 10), (ChargeCode.YACCF, 100)]);
			AutorateAndAssertTransportationUnit(dispatchTransportationUnit, [(ChargeCode.DGCOUT, 3), (ChargeCode.INFOUT, 2), (ChargeCode.YACCF, 100)]);
		}

		public void TestAutoRating_TransportationUnit_SpecificYard()
		{
			var (receiveTransportationUnit, dispatchTransportationUnit) = CreateTransporationUnitForTesting();

			Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
			{
				YardPk = yard.PK,
				DepotGateInCharge = 6,
				WeighBridgeFee = 4,
				VehicleAccessFee = 50,
				DepotGateOutCharge = 4,
				InfrastructureLevy = 3
			});

			var otherYard = Helper.CreateCYDWarehouse("OTH");
			Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
			{
				YardPk = otherYard.PK,
				DepotGateInCharge = 8,
				WeighBridgeFee = 3,
				VehicleAccessFee = 150,
				DepotGateOutCharge = 4,
				InfrastructureLevy = 5
			});

			Factory.Save();

			AutorateAndAssertTransportationUnit(receiveTransportationUnit, [(ChargeCode.DGCIN, 12), (ChargeCode.WBFIN, 8), (ChargeCode.YACCF, 50)]);
			AutorateAndAssertTransportationUnit(dispatchTransportationUnit, [(ChargeCode.DGCOUT, 4), (ChargeCode.INFOUT, 3), (ChargeCode.YACCF, 50)]);
		}

		public void TestAutoRating_TransportationUnit_SpecificMode()
		{
			var (receiveTransportationUnit, dispatchTransportationUnit) = CreateTransporationUnitForTesting();

			Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
			{
				Mode = "ROA",
				DepotGateInCharge = 6,
				WeighBridgeFee = 4,
				VehicleAccessFee = 50,
				DepotGateOutCharge = 4,
				InfrastructureLevy = 3
			});

			Factory.Save();

			AutorateAndAssertTransportationUnit(receiveTransportationUnit, [(ChargeCode.DGCIN, 12), (ChargeCode.WBFIN, 8), (ChargeCode.YACCF, 50)]);
			AutorateAndAssertTransportationUnit(dispatchTransportationUnit, [(ChargeCode.DGCOUT, 4), (ChargeCode.INFOUT, 3), (ChargeCode.YACCF, 50)]);
		}

		public void TestAutoRating_TransportationUnit_SpecificUnitType()
		{
			var (receiveTransportationUnit, dispatchTransportationUnit) = CreateTransporationUnitForTesting();

			Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
			{
				UnitType = "CNT",
				DepotGateInCharge = 6,
				WeighBridgeFee = 4,
				VehicleAccessFee = 0,
				DepotGateOutCharge = 4,
				InfrastructureLevy = 3
			});

			Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
			{
				UnitType = "CHS",
				DepotGateInCharge = 8,
				WeighBridgeFee = 3,
				VehicleAccessFee = 0,
				DepotGateOutCharge = 4,
				InfrastructureLevy = 5
			});

			Factory.Save();

			AutorateAndAssertTransportationUnit(receiveTransportationUnit, [(ChargeCode.DGCIN, 12), (ChargeCode.WBFIN, 8), (ChargeCode.YACCF, 100)]);
			AutorateAndAssertTransportationUnit(dispatchTransportationUnit, [(ChargeCode.DGCOUT, 4), (ChargeCode.INFOUT, 3), (ChargeCode.YACCF, 100)]);
		}

		public void TestAutoRating_TransportationUnit_SpecificUnitLoad()
		{
			var (receiveTransportationUnit, dispatchTransportationUnit) = CreateTransporationUnitForTesting();

			Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
			{
				UnitLoad = "EMP",
				DepotGateInCharge = 8,
				WeighBridgeFee = 3,
				VehicleAccessFee = 0,
				DepotGateOutCharge = 5,
				InfrastructureLevy = 4
			});

			Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
			{
				UnitLoad = "LAD",
				DepotGateInCharge = 6,
				WeighBridgeFee = 6,
				VehicleAccessFee = 0,
				DepotGateOutCharge = 4,
				InfrastructureLevy = 3
			});

			Factory.Save();

			AutorateAndAssertTransportationUnit(receiveTransportationUnit, [(ChargeCode.DGCIN, 8), (ChargeCode.DGCIN, 6), (ChargeCode.WBFIN, 3), (ChargeCode.WBFIN, 6), (ChargeCode.YACCF, 100)]);
			AutorateAndAssertTransportationUnit(dispatchTransportationUnit, [(ChargeCode.DGCOUT, 5), (ChargeCode.INFOUT, 4), (ChargeCode.YACCF, 100)]);
		}

		public void TestAutoRating_TransportationUnit_SpecificContainerLoad_UseDefault()
		{
			var (receiveTransportationUnit, dispatchTransportationUnit) = CreateTransporationUnitForTesting();

			Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
			{
				UnitLoad = "LAD",
				DepotGateInCharge = 6,
				WeighBridgeFee = 6,
				VehicleAccessFee = 0,
				DepotGateOutCharge = 4,
				InfrastructureLevy = 3
			});

			Factory.Save();

			AutorateAndAssertTransportationUnit(receiveTransportationUnit, [(ChargeCode.DGCIN, 7), (ChargeCode.DGCIN, 6), (ChargeCode.WBFIN, 5), (ChargeCode.WBFIN, 6), (ChargeCode.YACCF, 100)]);
			AutorateAndAssertTransportationUnit(dispatchTransportationUnit, [(ChargeCode.DGCOUT, 3), (ChargeCode.INFOUT, 2), (ChargeCode.YACCF, 100)]);
		}

		public void TestAutoRating_TransportationUnit_SpecificContainerType()
		{
			var (receiveTransportationUnit, dispatchTransportationUnit) = CreateTransporationUnitForTesting();

			Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
			{
				ContainerType = "20GP",
				DepotGateInCharge = 8,
				WeighBridgeFee = 3,
				VehicleAccessFee = 0,
				DepotGateOutCharge = 5,
				InfrastructureLevy = 4
			});

			Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
			{
				ContainerType = "40GP",
				DepotGateInCharge = 6,
				WeighBridgeFee = 6,
				VehicleAccessFee = 0,
				DepotGateOutCharge = 4,
				InfrastructureLevy = 3
			});

			Factory.Save();

			AutorateAndAssertTransportationUnit(receiveTransportationUnit, [(ChargeCode.DGCIN, 8), (ChargeCode.DGCIN, 6), (ChargeCode.WBFIN, 3), (ChargeCode.WBFIN, 6), (ChargeCode.YACCF, 100)]);
			AutorateAndAssertTransportationUnit(dispatchTransportationUnit, [(ChargeCode.DGCOUT, 4), (ChargeCode.INFOUT, 3), (ChargeCode.YACCF, 100)]);
		}

		public void TestAutoRating_TransportationUnit_SpecificClient()
		{
			var (receiveTransportationUnit, dispatchTransportationUnit) = CreateTransporationUnitForTesting();

			Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
			{
				Client = client.MainAddress.OA_OH,
				DepotGateInCharge = 6,
				WeighBridgeFee = 6,
				VehicleAccessFee = 0,
				DepotGateOutCharge = 4,
				InfrastructureLevy = 3
			});

			Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
			{
				Client = otherClient.MainAddress.OA_OH,
				DepotGateInCharge = 5,
				WeighBridgeFee = 4,
				VehicleAccessFee = 0,
				DepotGateOutCharge = 3,
				InfrastructureLevy = 4
			});

			Factory.Save();

			AutorateAndAssertTransportationUnit(receiveTransportationUnit, [(ChargeCode.DGCIN, 6), (ChargeCode.DGCIN, 5), (ChargeCode.WBFIN, 6), (ChargeCode.WBFIN, 4), (ChargeCode.YACCF, 100)]);
			AutorateAndAssertTransportationUnit(dispatchTransportationUnit, [(ChargeCode.DGCOUT, 3), (ChargeCode.INFOUT, 4), (ChargeCode.YACCF, 100)]);
		}

		public void TestAutoRating_TransportationUnit_UseStandardDate()
		{
			var (receiveTransportationUnit, dispatchTransportationUnit) = CreateTransporationUnitForTesting();

			Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
			{
				StartDate = ZDateTime.Now.AddDays(-1).Date,
				EndDate = ZDateTime.Now.AddDays(-1).Date,
				UnitType = "CNT",
				DepotGateInCharge = 8,
				WeighBridgeFee = 3,
				VehicleAccessFee = 0,
				DepotGateOutCharge = 5,
				InfrastructureLevy = 4
			});

			Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
			{
				StartDate = ZDateTime.Now.Date,
				EndDate = ZDateTime.Now.Date,
				UnitType = "CNT",
				DepotGateInCharge = 6,
				WeighBridgeFee = 6,
				VehicleAccessFee = 0,
				DepotGateOutCharge = 4,
				InfrastructureLevy = 3
			});

			Factory.Save();

			AutorateAndAssertTransportationUnit(receiveTransportationUnit, [(ChargeCode.DGCIN, 16), (ChargeCode.WBFIN, 6), (ChargeCode.YACCF, 100)]);
			AutorateAndAssertTransportationUnit(dispatchTransportationUnit, [(ChargeCode.DGCOUT, 4), (ChargeCode.INFOUT, 3), (ChargeCode.YACCF, 100)]);
		}

		[TestDate(2025, 1, 18)]
		public void TestAutoRating_ReceiveTransportationUnit_UseDepartureDate()
		{
			var autoRateDateConfig = new AutoRateDateByChargeGroupConfiguration();
			autoRateDateConfig.FilterType = RatingDateFilterTypes.Codes.Departure;
			using (AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, autoRateDateConfig))
			{
				Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
				{
					StartDate = ZDateTime.Now.AddDays(-1).Date,
					EndDate = ZDateTime.Now.AddDays(-1).Date,
					UnitType = "CNT",
					DepotGateInCharge = 8,
					WeighBridgeFee = 3,
					VehicleAccessFee = 0,
					DepotGateOutCharge = 5,
					InfrastructureLevy = 4
				});

				Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
				{
					StartDate = ZDateTime.Now.Date,
					EndDate = ZDateTime.Now.Date,
					UnitType = "CNT",
					DepotGateInCharge = 6,
					WeighBridgeFee = 6,
					VehicleAccessFee = 0,
					DepotGateOutCharge = 4,
					InfrastructureLevy = 3
				});

				var receiveTransportationUnit = CreateReceiveTransportationUnitPendingGateOut();

				AutorateAndAssertTransportationUnit(receiveTransportationUnit, []);

				Helper.GateOutTransportationUnit(receiveTransportationUnit, ZDateTimeOffset.Today.AddDays(-1));

				Factory.Save();

				AutorateAndAssertTransportationUnit(receiveTransportationUnit, [(ChargeCode.DGCIN, 8), (ChargeCode.WBFIN, 3), (ChargeCode.YACCF, 100)]);
			}
		}

		[TestDate(2025, 1, 18)]
		public void TestAutoRating_ReceiveTransportationUnit_UseCustomDate_FallBackIsDisabled()
		{
			var autoRateDateConfig = new AutoRateDateByChargeGroupConfiguration();
			autoRateDateConfig.FilterType = RatingDateFilterTypes.Codes.Custom;

			var autoRateDateOfCGI = autoRateDateConfig.AutoRateDateByChargeGroups
				.Cast<AutoRateDateByChargeGroup>()
				.Single(x => x.ChargeGroup == ChargeCodeGroupList.Codes.YardTransportationUnitGateIn);

			var chargeGroupSettingOfYTU = autoRateDateOfCGI.ChargeGroupSettings.AddNew();
			chargeGroupSettingOfYTU.JobType = JobInvoicingConsumerTypes.CYDTransportationUnitJobCode;
			chargeGroupSettingOfYTU.DateType = JobDateTypes.Codes.GateOutDate;
			chargeGroupSettingOfYTU.IsFallbackDisabled = true;

			using (AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, autoRateDateConfig))
			{
				Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
				{
					StartDate = ZDateTime.Now.AddDays(-1).Date,
					EndDate = ZDateTime.Now.AddDays(-1).Date,
					UnitType = "CNT",
					DepotGateInCharge = 8,
					WeighBridgeFee = 3,
					VehicleAccessFee = 0,
					DepotGateOutCharge = 5,
					InfrastructureLevy = 4
				});

				Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
				{
					StartDate = ZDateTime.Now.Date,
					EndDate = ZDateTime.Now.Date,
					UnitType = "CNT",
					DepotGateInCharge = 6,
					WeighBridgeFee = 6,
					VehicleAccessFee = 0,
					DepotGateOutCharge = 4,
					InfrastructureLevy = 3
				});

				var receiveTransportationUnit = CreateReceiveTransportationUnitPendingGateOut();

				AutorateAndAssertTransportationUnit(receiveTransportationUnit, []);

				Helper.GateOutTransportationUnit(receiveTransportationUnit, ZDateTimeOffset.Today.AddDays(-1));

				Factory.Save();

				AutorateAndAssertTransportationUnit(receiveTransportationUnit, [(ChargeCode.DGCIN, 8), (ChargeCode.WBFIN, 3), (ChargeCode.YACCF, 100)]);
			}
		}

		public void TestAutoRating_ReceiveTransportationUnit_UseCustomDate_FallBackIsNotDisabled()
		{
			var autoRateDateConfig = new AutoRateDateByChargeGroupConfiguration();
			autoRateDateConfig.FilterType = RatingDateFilterTypes.Codes.Custom;

			var autoRateDateOfCGI = autoRateDateConfig.AutoRateDateByChargeGroups
				.Cast<AutoRateDateByChargeGroup>()
				.Single(x => x.ChargeGroup == ChargeCodeGroupList.Codes.YardTransportationUnitGateIn);

			var chargeGroupSettingOfYTU = autoRateDateOfCGI.ChargeGroupSettings.AddNew();
			chargeGroupSettingOfYTU.JobType = JobInvoicingConsumerTypes.CYDTransportationUnitJobCode;
			chargeGroupSettingOfYTU.DateType = JobDateTypes.Codes.GateOutDate;
			chargeGroupSettingOfYTU.IsFallbackDisabled = false;

			using (AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, autoRateDateConfig))
			{
				Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
				{
					StartDate = ZDateTime.Now.AddDays(-1).Date,
					EndDate = ZDateTime.Now.AddDays(-1).Date,
					UnitType = "CNT",
					DepotGateInCharge = 8,
					WeighBridgeFee = 3,
					VehicleAccessFee = 0,
					DepotGateOutCharge = 5,
					InfrastructureLevy = 4
				});

				Helper.CreateRateEntry(clientRate, new CYURateEntryParameters
				{
					StartDate = ZDateTime.Now.Date,
					EndDate = ZDateTime.Now.Date,
					UnitType = "CNT",
					DepotGateInCharge = 6,
					WeighBridgeFee = 6,
					VehicleAccessFee = 0,
					DepotGateOutCharge = 4,
					InfrastructureLevy = 3
				});

				var receiveTransportationUnit = CreateReceiveTransportationUnitPendingGateOut();

				AutorateAndAssertTransportationUnit(receiveTransportationUnit, [(ChargeCode.DGCIN, 8), (ChargeCode.WBFIN, 3), (ChargeCode.YACCF, 100)]);
			}
		}

		#region Implementation

		new CYDYardTestHelper Helper
		{
			get { return helper ??= new CYDYardTestHelper(Factory); }
		}

		CYDYardTestHelper helper;

		WhsWarehouse yard;

		WhsLocation location;

		OrgHeader client;

		OrgHeader otherClient;

		OrgHeader transportor;

		ClientRate clientRate;

		protected override void SetUp()
		{
			base.SetUp();

			yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			location = row.Locations[0];
			client = Helper.CreateClient();
			otherClient = Helper.CreateClient("OTHER");
			transportor = SetupClientForRating("Transportor", new CYURateEntryParameters(), out RateEntry rateEntry, out ClientRate clientRate);
			this.clientRate = clientRate;

			Factory.Save();
		}

		OrgHeader SetupClientForRating(string clientCode, RateEntryParameters rateEntryParams, out RateEntry rateEntry, out ClientRate clientRate)
		{
			var client = Helper.CreateClient(clientCode);
			client.OH_IsDebtor = true;
			clientRate = Helper.CreateClientRate(client);
			rateEntry = Helper.CreateRateEntry(clientRate, rateEntryParams);

			return client;
		}

		[TestDate(2025, 1, 18, 8, 0, 0)]
		(CYDTransportationUnit, CYDTransportationUnit) CreateTransporationUnitForTesting()
		{
			var receiveAdvice = Helper.CreateReceiveAdvice(client, yard, "PRA0001");
			var receiveAdviceOfOtherClient = Helper.CreateReceiveAdvice(otherClient, yard, "PRA0002");

			var gatedInYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNT1234", "20GP", false);
			var gatedOutYardUnit = Helper.AddReceiveAdviceLine(receiveAdviceOfOtherClient, "CNT5678", "40GP", true);

			var receiveTransportationUnit = Helper.CreateTransportationUnit("TPU0001", transportor, yard);
			Helper.AddDelivery(receiveTransportationUnit, gatedInYardUnit);
			Helper.AddDelivery(receiveTransportationUnit, gatedOutYardUnit);
			Helper.GateInTransportationUnit(receiveTransportationUnit, ZDateTimeOffset.Today.AddDays(-1), location);
			Helper.GateOutTransportationUnit(receiveTransportationUnit, ZDateTimeOffset.Today.AddDays(-1));

			var releaseAdvice = Helper.CreateReleaseAdvice(otherClient, yard, "REL0001");
			Helper.AddReleaseAdviceLine(releaseAdvice, gatedInYardUnit);
			Helper.AddReleaseAdviceLine(releaseAdvice, gatedOutYardUnit);

			var dispatchTransportationUnit = Helper.CreateTransportationUnit("TPU0002", transportor, yard);
			Helper.AddPickup(dispatchTransportationUnit, gatedOutYardUnit);
			Helper.GateInTransportationUnit(dispatchTransportationUnit, ZDateTimeOffset.Today, location);
			Helper.GateOutTransportationUnit(dispatchTransportationUnit, ZDateTimeOffset.Today);

			Factory.Save();

			return (receiveTransportationUnit, dispatchTransportationUnit);
		}

		CYDTransportationUnit CreateReceiveTransportationUnitPendingGateOut()
		{
			var receiveAdvice = Helper.CreateReceiveAdvice(client, yard, "PRA0001");
			var gatedInYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNT1234", "20GP", false);
			var receiveTransportationUnit = Helper.CreateTransportationUnit("TPU0001", transportor, yard);
			Helper.AddDelivery(receiveTransportationUnit, gatedInYardUnit);
			Helper.GateInTransportationUnit(receiveTransportationUnit, ZDateTimeOffset.Today.AddDays(-1), location);

			Factory.Save();

			return receiveTransportationUnit;
		}

		void AutorateAndAssertTransportationUnit(CYDTransportationUnit transportationUnit, List<(ChargeCode, int)> expectedCharges)
		{
			var expected = expectedCharges.Select(static charge => new AssertionCharge
			{
				ChargeCode = Enum.GetName(typeof(ChargeCode), charge.Item1),
				JR_OSSellAmt = charge.Item2,
			});
			var interactor = new TestInteractor();
			AutorateAndAssert(expected, transportationUnit, transportor, autorateCosts: false, testInteractor: interactor);
		}

		#endregion
	}
}
