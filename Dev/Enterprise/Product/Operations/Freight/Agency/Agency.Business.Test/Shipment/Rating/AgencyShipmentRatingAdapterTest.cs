using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal abstract class AgencyShipmentRatingAdapterTest<T> : TestCaseWithFactory where T : AgencyShipmentJobDatesProvider
	{
		public void TestJobDatesProvider()
		{
			var shipment = CreateShipment();
			AssertType<T>((shipment.RatingAdapter).JobDatesProvider);
		}

		public void TestAdapterTypeAndID()
		{
			var shipment = CreateShipment();
			AssertEquals(AdapterType.BillOfLading, shipment.RatingAdapter.AdapterType);
			AssertEquals(shipment.JS_UniqueConsignRef, shipment.RatingAdapter.OperationalJobCode);
		}

		#region ISpotRate
		public void TestSellAutoratingMode()
		{
			var shipment = CreateShipment();
			AssertEquals(GetExpectedSellAutoratingMode(), ((ISpotRate)shipment.RatingAdapter).SellSpotRateInfo.AutoratedMode);
			AssertEquals(AutoratedValueType.ClientRate, ((ISpotRate)shipment.RatingAdapter).SellSpotRateInfo.AutoratedValueType);
		}

		public void TestCostAutoratingMode()
		{
			var shipment = CreateShipment();
			AssertEquals(GetExpectedCostAutoratingMode(), ((ISpotRate)shipment.RatingAdapter).CostSpotRateInfo.AutoratedMode);
			AssertEquals(AutoratedValueType.Cost, ((ISpotRate)shipment.RatingAdapter).CostSpotRateInfo.AutoratedValueType);
		}

		protected abstract string GetExpectedSellAutoratingMode();
		protected abstract string GetExpectedCostAutoratingMode();

		#endregion

		public void TestDistanceMeasures()
		{
			var shipment = CreateShipment();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USNYC";
			var route1 = shipment.Transports.AddNew();
			var route2 = shipment.Transports.AddNew();
			var route3 = shipment.Transports.AddNew();
			var route4 = shipment.Transports.AddNew();
			var route5 = shipment.Transports.AddNew();
			route1.JW_RL_NKLoadPort = "AUBNE";
			route1.JW_RL_NKDiscPort = "AUBNE";
			route2.JW_RL_NKLoadPort = "AUBNE";
			route2.JW_RL_NKDiscPort = "AUSYD";
			route3.JW_RL_NKLoadPort = "AUSYD";
			route3.JW_RL_NKDiscPort = "USLAX";
			route4.JW_RL_NKLoadPort = "USLAX";
			route4.JW_RL_NKDiscPort = "USNYC";
			route5.JW_RL_NKLoadPort = "USNYC";
			route5.JW_RL_NKDiscPort = "USNYC";
			route1.JW_TransportMode = Constants.TransportModes.Road;
			route2.JW_TransportMode = Constants.TransportModes.Road;
			route3.JW_TransportMode = Constants.TransportModes.Sea;
			route4.JW_TransportMode = Constants.TransportModes.Road;
			route5.JW_TransportMode = Constants.TransportModes.Road;
			route3.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			route1.JW_TransportType = Constants.TransportPlanningType.PreCarriage;
			route1.JW_OA_DepartureLocation = Factory.New<OrgHeader>().MainAddress.PK;
			route4.JW_OA_ArrivalLocation = Factory.New<OrgHeader>().MainAddress.PK;
			route1.JW_DistanceUnit = Constants.Length.Kilometres;
			route2.JW_DistanceUnit = Constants.Length.Kilometres;
			route4.JW_DistanceUnit = Constants.Length.Kilometres;
			route5.JW_DistanceUnit = Constants.Length.Kilometres;
			route1.JW_Distance = 20m;
			route2.JW_Distance = 1000m;
			route4.JW_Distance = 2000m;
			route5.JW_Distance = 30m;
			var measures = (RateableMeasureSet)shipment.RatingAdapter.RateableMeasures;
			AssertEquals(20m, measures.PickupDistance.Amount);
			AssertEquals(Constants.Length.Kilometres, measures.PickupDistance.Unit);
			AssertEquals(2000m, measures.DeliveryDistance.Amount);
			AssertEquals(Constants.Length.Kilometres, measures.DeliveryDistance.Unit);
		}

		public void TestDistanceMeasures_DoesntAccessDeletedTransports()
		{
			var shipment = CreateShipment();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "MDKIV";
			var pickup = shipment.Transports.AddNew();
			pickup.JW_TransportMode = Constants.TransportModes.Road;
			pickup.JW_RL_NKLoadPort = "AUSYD";
			pickup.JW_OA_DepartureLocation = Factory.New<OrgAddress>().PK;
			pickup.JW_TransportType = Constants.TransportPlanningType.Other;
			var delivery = shipment.Transports.AddNew();
			delivery.JW_TransportMode = Constants.TransportModes.Road;
			delivery.JW_RL_NKDiscPort = "MDKIV";
			delivery.JW_OA_ArrivalLocation = Factory.New<OrgAddress>().PK;
			delivery.JW_TransportType = Constants.TransportPlanningType.Other;
			var accessingMeasuresToEnsureTransportLegsAreNotCached = shipment.RatingAdapter.RateableMeasures;
			pickup.Delete();
			delivery.Delete();
			object o;
			AssertNoExceptionThrown(() => o = shipment.RatingAdapter.RateableMeasures);
		}

		public void TestUpdateClientContractNumber_PopulateContractNumbersFromRevenueRates()
		{
			var shipment = CreateShipment();
			shipment.BookedShippingLinePK = shipment.PK;
			var jobDataUpdater = (IJobDataUpdater)shipment.RatingAdapter;
			using (AgencyRegistry.Instance.PopulateContractNumbersFromRevenueRates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert(shipment.Numbers.ListContractNumbers("CON").IsNullOrEmpty());
				var updateToken = new UpdateCarrierContractNumberToken(new[] { "Cost0" });
				jobDataUpdater.UpdateCarrierContractNumber(updateToken);
				jobDataUpdater.UpdateClientContractNumber(new[] { "Revenue0" });
				Assert(shipment.Numbers.ListContractNumbers("CON").IsNullOrEmpty());
			}

			using (AgencyRegistry.Instance.PopulateContractNumbersFromRevenueRates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var updateToken = new UpdateCarrierContractNumberToken(new[] { "Cost1" });
				jobDataUpdater.UpdateCarrierContractNumber(updateToken);
				jobDataUpdater.UpdateClientContractNumber(new[] { "Revenue1" });
				AssertContainsExactElementsInAnyOrder("Add non existing revenue contract numbers.", new[] { "Revenue1" }, shipment.Numbers.ListContractNumbers("CON").Select(x => x.CE_EntryNum));
				updateToken = new UpdateCarrierContractNumberToken(new[] { "Cost2" });
				jobDataUpdater.UpdateCarrierContractNumber(updateToken);
				jobDataUpdater.UpdateClientContractNumber(new[] { "Revenue2" });
				AssertContainsExactElementsInAnyOrder("Add non existing revenue contract number while the list has a number.", new[] { "Revenue1", "Revenue2" }, shipment.Numbers.ListContractNumbers("CON").Select(x => x.CE_EntryNum));
				jobDataUpdater.UpdateCarrierContractNumber(updateToken);
				jobDataUpdater.UpdateClientContractNumber(new[] { "Revenue1" });
				AssertContainsExactElementsInAnyOrder("Don't add duplicates", new[] { "Revenue1", "Revenue2" }, shipment.Numbers.ListContractNumbers("CON").Select(x => x.CE_EntryNum));
				jobDataUpdater.UpdateClientContractNumber(new[] { string.Empty });
				AssertContainsExactElementsInAnyOrder("Empty string is allowed to add from BillOfLading", new[] { "Revenue1", "Revenue2", "" }, shipment.Numbers.ListContractNumbers("CON").Select(x => x.CE_EntryNum));
			}
		}

		public void TestUpdateClientContractNumber_ReplaceExistingContractNumbersWithNewNumbers()
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = CreateShipment();
			shipment.JS_OA_BookedShippingLineAddress = shippingLine.MainAddress.PK;
			var jobDataUpdater = (IJobDataUpdater)shipment.RatingAdapter;
			using (AgencyRegistry.Instance.ReplaceExistingContractNumbersWithNewNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert(shipment.Numbers.ListContractNumbers("CON").IsNullOrEmpty());
				var updateToken = new UpdateCarrierContractNumberToken(new[] { "Cost0" });
				jobDataUpdater.UpdateCarrierContractNumber(updateToken);
				jobDataUpdater.UpdateClientContractNumber(new[] { "Revenue0" });
				Assert(shipment.Numbers.ListContractNumbers("CON").IsNullOrEmpty());
			}

			using (AgencyRegistry.Instance.ReplaceExistingContractNumbersWithNewNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var updateToken = new UpdateCarrierContractNumberToken(new[] { "Cost1" });
				jobDataUpdater.UpdateCarrierContractNumber(updateToken);
				jobDataUpdater.UpdateClientContractNumber(new[] { "Revenue1" });
				AssertContainsExactElementsInAnyOrder("Add non existing revenue contract numbers.", new[] { "Revenue1" }, shipment.Numbers.ListContractNumbers("CON").Select(x => x.CE_EntryNum));
				updateToken = new UpdateCarrierContractNumberToken(new[] { "Cost2" });
				jobDataUpdater.UpdateCarrierContractNumber(updateToken);
				jobDataUpdater.UpdateClientContractNumber(new[] { "Revenue2" });
				AssertContainsExactElementsInAnyOrder("New contract number should replace old one.", new[] { "Revenue2" }, shipment.Numbers.ListContractNumbers("CON").Select(x => x.CE_EntryNum));
				jobDataUpdater.UpdateClientContractNumber(new[] { string.Empty });
				AssertContainsExactElementsInAnyOrder("Empty string is allowed to add from BillOfLading", new[] { "" }, shipment.Numbers.ListContractNumbers("CON").Select(x => x.CE_EntryNum));
			}
		}

		public void TestTransportProviders()
		{
			var port1 = Factory.New<OrgHeader>();
			port1.OH_Code = "PORT1";
			var port2 = Factory.New<OrgHeader>();
			port2.OH_Code = "PORT2";
			var voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();
			var bill = CreateShipment();
			bill.JS_JX = voyage.Sailings[0].PK;
			var autorating = bill.RatingAdapter;
			AssertContainsExactElementsInAnyOrder("TransportProviders Empty", o => o.OH_Code, Array.Empty<OrgHeader>(), autorating.Creditors.AllOrgs);
			origin.JA_OA_DepartureCTOAddress = port1.MainAddress.PK;
			destination.JB_OA_ArrivalCTOAddress = port2.MainAddress.PK;
			AssertContainsExactElementsInAnyOrder("TransportProviders", o => o.OH_Code, new[] { port1, port2 }, autorating.Creditors.AllOrgs);
		}

		#region Via
		public void TestViaGetter_ViasAreNotSpecified_ReturnEmptyString()
		{
			var shipment = CreateShipment();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "UAIEV";
			shipment.JS_JX = CreateSailing("AUSYD", "UAIEV", ZDateTime.Today.AddDays(1)).PK;
			var adapterToTest = shipment.RatingAdapter;
			AssertNull("Via", adapterToTest.GetVia(CostSell.Cost));
			AssertNull("Via", adapterToTest.GetVia(CostSell.Revenue));
		}

		public void TestViaGetter_LoadViaIsSpecified_ReturnLoadVia()
		{
			var shipment = CreateShipment();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "UAIEV";
			shipment.JS_JX = CreateSailing("USNYC", "UAIEV", ZDateTime.Today.AddDays(1)).PK;
			var adapterToTest = shipment.RatingAdapter;
			AssertEquals("Via", "USNYC", adapterToTest.GetVia(CostSell.Cost).Code);
			AssertEquals("Via", "USNYC", adapterToTest.GetVia(CostSell.Revenue).Code);
		}

		public void TestViaGetter_DischargeViaIsSpecified_ReturnLoadDischargeVia()
		{
			var shipment = CreateShipment();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "UAIEV";
			shipment.JS_JX = CreateSailing("AUSYD", "USNYC", ZDateTime.Today.AddDays(1)).PK;
			var adapterToTest = shipment.RatingAdapter;
			AssertEquals("Via", "USNYC", adapterToTest.GetVia(CostSell.Cost).Code);
			AssertEquals("Via", "USNYC", adapterToTest.GetVia(CostSell.Revenue).Code);
		}

		public void TestViaGetter_ShipmentIsImportAndBothViasAreSpecified_ReturnDischargeVia()
		{
			var shipment = CreateShipment();
			shipment.JS_RL_NKOrigin = "UAIEV";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_JX = CreateSailing("USNYC", "AUMEL", ZDateTime.Today.AddDays(1)).PK;
			var adapterToTest = shipment.RatingAdapter;
			AssertEquals("Via", "AUMEL", adapterToTest.GetVia(CostSell.Cost).Code);
			AssertEquals("Via", "AUMEL", adapterToTest.GetVia(CostSell.Revenue).Code);
		}

		public void TestViaGetter_ShipmentIsExportAndBothViasAreSpecified_ReturnLoadVia()
		{
			var shipment = CreateShipment();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "UAIEV";
			shipment.JS_JX = CreateSailing("USNYC", "AUMEL", ZDateTime.Today.AddDays(1)).PK;
			var adapterToTest = shipment.RatingAdapter;
			AssertEquals("Via", "USNYC", adapterToTest.GetVia(CostSell.Cost).Code);
			AssertEquals("Via", "USNYC", adapterToTest.GetVia(CostSell.Revenue).Code);
		}

		public void TestViaGetter_ShipmentIsOffshoreAndBothViasAreSpecified_ReturnEmptyString()
		{
			var shipment = CreateShipment();
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "UAIEV";
			shipment.JS_JX = CreateSailing("USNYC", "AUMEL", ZDateTime.Today.AddDays(1)).PK;
			var adapterToTest = shipment.RatingAdapter;
			AssertNull("Via", adapterToTest.GetVia(CostSell.Cost));
			AssertNull("Via", adapterToTest.GetVia(CostSell.Revenue));
		}

		public void TestViaGetter_ShipmentIsDomesticAndBothViasAreSpecified_ReturnEmptyString()
		{
			var shipment = CreateShipment();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUCNS";
			shipment.JS_JX = CreateSailing("USNYC", "AUMEL", ZDateTime.Today.AddDays(1)).PK;
			var adapterToTest = shipment.RatingAdapter;
			AssertNull("Via", adapterToTest.GetVia(CostSell.Cost));
			AssertNull("Via", adapterToTest.GetVia(CostSell.Revenue));
		}

		#endregion

		public void TestAutoRatingPorts_Import()
		{
			var shipment = CreateShipment();
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUSYD";
			var rating = shipment.RatingAdapter;
			AssertEquals("Origin", "CNSHA", rating.Origin.Code);
			AssertEquals("Destination", "AUSYD", rating.Destination.Code);
			shipment.JS_RL_NKOrigin = "SGSIN";
			AssertEquals("Origin", "SGSIN", rating.Origin.Code);
			AssertEquals("Destination", "AUSYD", rating.Destination.Code);
		}

		public void TestAutoRatingPorts_Export()
		{
			var shipment = CreateShipment();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CNSHA";
			var rating = shipment.RatingAdapter;
			AssertEquals("Origin", "AUSYD", rating.Origin.Code);
			AssertEquals("Destination", "CNSHA", rating.Destination.Code);
			shipment.JS_RL_NKDestination = "SGSIN";
			AssertEquals("Origin", "AUSYD", rating.Origin.Code);
			AssertEquals("Destination", "SGSIN", rating.Destination.Code);
		}

		public void TestAutoRatingPorts_Domestic()
		{
			var shipment = CreateShipment();
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "AUCNS";
			var rating = shipment.RatingAdapter;
			AssertEquals("Origin", "AUMEL", rating.Origin.Code);
			AssertEquals("Destination", "AUCNS", rating.Destination.Code);
		}

		public void TestAutoRatingPorts_Offshore()
		{
			var shipment = CreateShipment();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "CNSHA";
			var rating = shipment.RatingAdapter;
			AssertEquals("Origin", "NZAKL", rating.Origin.Code);
			AssertEquals("Destination", "CNSHA", rating.Destination.Code);
		}

		public void TestInvoiceAndRatingConsumerTypes()
		{
			var rateable = CreateShipment().RatingAdapter;
			AssertNotNull(rateable.ConsumerType);
			AssertEquals(ConsumerType, rateable.ConsumerType.Code);
		}

		public void TestGetExRateSource()
		{
			SetupSailing(false, false, false);
			var shipment = CreateShipment();
			shipment.Sailings.Add(Sailing1);
			var exRateSource = ((IJobInvoicingExRateSourceProvider)shipment).GetExRateSource(ExRateSourceType.Voyage);
			AssertEquals("Should be AgencyShipmentExRateSource type", typeof(AgencyShipmentExRateSource), exRateSource.GetType());
			AssertEquals(Voyage.PK, exRateSource.SourcePK);
		}

		public void TestAutoRatingRateType()
		{
			var rating = CreateShipment().RatingAdapter;
			AssertEquals("Use the Shipping rate type", RateType.Shipping, rating.RateTypeToUse);
		}

		public void TestAutoRatingMeasureContainersCorrectly()
		{
			var shipment = CreateShipment();
			var rating = shipment.RatingAdapter;
			var measures = (RateableMeasureSet)rating.RateableMeasures;
			AssertEquals("No containers yet", 0, measures.GetAllContainers().Count());
			AgencyShipmentContainer container = AddNewContainer(shipment);
			container.JC_RC = RC_20GP_PK;
			measures = (RateableMeasureSet)rating.RateableMeasures;
			AssertEquals("A container has been added.", 1, measures.GetAllContainers().Count());
		}

		public void TestAutoRatingMeasureContainers_TopLevelPacks()
		{
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				var shipment = CreateShipment();
				shipment.JS_PackingMode = mode;
				AgencyShipmentContainer container = AddNewContainer(shipment);
				container.JC_RC = RC_20GP_PK;
				var measures = (RateableMeasureSet)shipment.RatingAdapter.RateableMeasures;
				AssertEquals("Containers are in measures for RORO", 1, measures.GetAllContainers().Count());
			}
		}

		public void TestAutoRatingMeasures_TopLevelPacks()
		{
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				var shipment = CreateShipment();
				shipment.JS_PackingMode = mode;
				var vehicle1 = shipment.Vehicles.AddNew();
				vehicle1.JC_GrossWeight = 100m;
				vehicle1.JC_GrossWeightUQ = Constants.Weight.Kilograms;
				vehicle1.JC_GrossVolume = 1.1m;
				vehicle1.JC_GrossVolumeUQ = Constants.Volume.CubicMetres;
				vehicle1.JC_ContainerCount = 1;
				vehicle1.JC_RH_NKContainerCommodityCode = "AAA";
				var vehicle2 = shipment.Vehicles.AddNew();
				vehicle2.JC_GrossWeight = 2m;
				vehicle2.JC_GrossWeightUQ = Constants.Weight.Tonnes;
				vehicle2.JC_GrossVolume = 2.2m;
				vehicle2.JC_GrossVolumeUQ = Constants.Volume.CubicMetres;
				vehicle2.JC_ContainerCount = 3;
				vehicle2.JC_RH_NKContainerCommodityCode = "BBB";
				var rateableMeasures = (RateableMeasureSet)shipment.RatingAdapter.RateableMeasures;
				AssertEquals(2100m, rateableMeasures.GetActual(MeasureType.Weight));
				AssertEquals(Constants.Weight.Kilograms, rateableMeasures.GetUnit(MeasureType.Weight));
				AssertArrayEqualsByElements(new[] { 100m, 2000m }, rateableMeasures.GetPacklineWeights_ForTest().ToArray());
				AssertArrayEqualsByElements(new[] { "AAA", "BBB" }, rateableMeasures.GetPacklineCommodities_ForTest().ToArray());
				AssertArrayEqualsByElements(new[] { ZGuid.Empty, ZGuid.Empty }, rateableMeasures.GetPacklineContainerTypes_ForTest().ToArray());
				AssertEquals(3.3m, rateableMeasures.GetActual(MeasureType.Volume));
				AssertEquals(Constants.Volume.CubicMetres, rateableMeasures.GetUnit(MeasureType.Volume));
				AssertEquals(4m, rateableMeasures.GetActual(MeasureType.Package));
				AssertEquals(0m, rateableMeasures.GetActual(MeasureType.LoadingMeters));
			}
		}

		public void TestMeasures_UnitMeasureType_PopulateFromTopLevelPacks()
		{
			var shipment = CreateShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;

			var topLevelPack = shipment.TopLevelPacks.AddNew();
			topLevelPack.JC_ContainerCount = 2;
			topLevelPack.JC_F3_NKPackType = "VEH";

			var rateableMeasures = (RateableMeasureSet)shipment.RatingAdapter.RateableMeasures;
			var partList = rateableMeasures.GetPartList(MeasureType.Unit);

			CombineAssertions(() =>
			{
				AssertEquals("RateablePartList.HasContainerType", true, partList.HasContainerType);
				AssertEquals("RateablePartList.HasPackageType", true, partList.HasPackageType);
				AssertContainsExactElementsInAnyOrder
				(
					"RateablePartList",
					new []
					{
						"ContainerWeightInKG: 0, ContainerVolumeInM3: 0, UnitCount: 2, PackageType: VEH, RefNumber: , TEU: 0, ContainerNumber: , ContainerTypePk: 00000000-0000-0000-0000-000000000000, ContainerCount: 2, ContainerPackages: 2",
					},
					partList.Cast<RateableContainer>().Select(part => $"ContainerWeightInKG: {part.ContainerWeightInKG}, ContainerVolumeInM3: {part.ContainerVolumeInM3}, UnitCount: {part.UnitCount}, PackageType: {part.PackageType}, RefNumber: {part.RefNumber}, TEU: {part.TEU}, ContainerNumber: {part.ContainerNumber}, ContainerTypePk: {part.ContainerTypePk}, ContainerCount: {part.ContainerCount}, ContainerPackages: {part.ContainerPackages}")
				);
			});
		}

		public void TestAutoRatingAgencyContainers()
		{
			var gp40 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			var shipment = CreateShipment();
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_RC = gp40.PK;
			shipment.OuterPackLines.AddNew().JL_JC = container.PK;
			shipment.JS_ActualWeight = 300m;
			var rating = shipment.RatingAdapter;
			var measures = (RateableMeasureSet)rating.RateableMeasures;
			AssertEquals(gp40.PK, measures.GetPacklineUniqueContainerTypePKs_ForTest().Single());
		}

		public void TestAutoRatingIncotermPaymentTermInfo()
		{
			var shipment = CreateShipment();
			shipment.JS_INCO = Constants.DomesticPaymentTerms.Prepaid;
			var info = shipment.RatingAdapter.PaymentTerm.GetPaymentTermInfo(CostSell.Revenue);
			AssertEquals(Constants.PaymentType.Prepaid, info.Value);
			shipment.JS_INCO = Constants.DomesticPaymentTerms.Collect;
			info = shipment.RatingAdapter.PaymentTerm.GetPaymentTermInfo(CostSell.Revenue);
			AssertEquals(Constants.PaymentType.Collect, info.Value);
			shipment.JS_INCO = "YYY";
			info = shipment.RatingAdapter.PaymentTerm.GetPaymentTermInfo(CostSell.Revenue);
			AssertNull("Expected no info to be created for invalid agency shipment term", info);
		}

		public void TestAutoRatingCarrier()
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			var principal = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = CreateShipment();
			shipment.JS_OA_BookedShippingLineAddress = shippingLine.MainAddress.PK;
			shipment.JS_OH_DeliveryAgent = principal.PK;
			var rating = shipment.RatingAdapter;
			AssertEquals(principal, rating.Carrier);
		}

		public void TestAutoRatingStatusInformation_Containerised()
		{
			var shipment = CreateShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			var rating = shipment.RatingAdapter;
			AssertEquals("No containers yet", false, rating.StatusInformation.CanExecute);
			AssertEquals("No containers yet", "This FCL shipment has no containers defined.", rating.StatusInformation.Message);
			AddNewContainer(shipment);
			AssertEquals("Has container.", true, rating.StatusInformation.CanExecute);
			AssertEquals("Has container.", "", rating.StatusInformation.Message);
		}

		public void TestAutoRatingStatusInformation_NonContainerised()
		{
			var shipment = CreateShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			var rating = shipment.RatingAdapter;
			shipment.JS_ActualWeight = 1000;
			AssertEquals("No volume", false, rating.StatusInformation.CanExecute);
			AssertEquals("No volume", "Shipment volume has not been entered.", rating.StatusInformation.Message);
			shipment.JS_ActualVolume = 1;
			AssertEquals("Has container.", true, rating.StatusInformation.CanExecute);
			AssertEquals("Has container.", "", rating.StatusInformation.Message);
			shipment.JS_ActualWeight = 0;
			AssertEquals("No weight", false, rating.StatusInformation.CanExecute);
			AssertEquals("No weight", "Shipment weight has not been entered.", rating.StatusInformation.Message);
			shipment.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			shipment.RealContainers.AddNew();
			AssertEquals("No weight", true, rating.StatusInformation.CanExecute);
			shipment.RealContainers.RemoveAndDeleteAll();
			Assert(!rating.StatusInformation.CanExecute);
			AssertEquals("This RORO shipment has no vehicles defined.", rating.StatusInformation.Message);
		}

		public void TestAutoRatingStatusInformation_NoVolumeUnitAndWeightUnit()
		{
			var shipment = CreateShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			var rating = shipment.RatingAdapter;
			shipment.JS_ActualWeight = 1000;
			shipment.JS_ActualVolume = 1;
			AssertEquals("No Packages", 1, shipment.TopLevelPacks.Count);
			shipment.TopLevelPacks[0].JC_GrossWeightUQ = "";
			shipment.TopLevelPacks[0].JC_GrossVolumeUQ = "";
			var status = rating.StatusInformation;
			AssertEquals("No Volume Unit and Weight Unit", false, status.CanExecute);
			AssertEquals("No Volume Unit and Weight Unit", "Both Volume Unit and Weight Unit for all packing lines are required to proceed with Autorating.", status.Message);
			shipment.JS_ActualWeight = 1000;
			shipment.JS_ActualVolume = 1;
			shipment.TopLevelPacks[0].JC_GrossWeightUQ = "KG";
			shipment.TopLevelPacks[0].JC_GrossVolumeUQ = "M3";
			status = rating.StatusInformation;
			AssertEquals("Has Volume Unit and Weight Unit", true, status.CanExecute);
			AssertEquals("Has Volume Unit and Weight Unit", "", status.Message);
		}

		public void TestConsignorByInterface()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = CreateShipment();
			var rating = shipment.RatingAdapter;
			IJobInvoicingPlugIn invoicing = shipment;
			AssertNull("Consignor not set.", shipment.Consignor);
			AssertNull("Booking party not set.", shipment.BookingParty);
			AssertNull("Neither Consignor nor Booking party set.", rating.DebtorOrgs[RatingDebtorOrgTypes.CNR]);
			AssertNull("Neither Consignor nor Booking party set.", invoicing.InvoicingSupporter.Consignor);
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			AssertEquals("Shipment.Consignor should always return the consignor.", consignor.PK, shipment.Consignor.PK);
			AssertEquals("IAutoRatingOrganisations.Consignor should return the consignor when booked by is not set.", consignor.PK, rating.DebtorOrgs[RatingDebtorOrgTypes.CNR].PK);
			AssertEquals("IJobInvoicingPlugIn.Consignor should return the consignor when booked by is not set.", consignor.PK, invoicing.InvoicingSupporter.Consignor.PK);
			shipment.BookingPartyDocumentaryAddress.OrganisationPK = bookingParty.PK;
			AssertEquals("Shipment.Consignor should always return the consignor.", consignor.PK, shipment.Consignor.PK);
			AssertEquals("IAutoRatingOrganisations.Consignor should return the booking party when set.", bookingParty.PK, rating.DebtorOrgs[RatingDebtorOrgTypes.CNR].PK);
			AssertEquals("IJobInvoicingPlugin.Consignor should return the booking party when set.", bookingParty.PK, invoicing.InvoicingSupporter.Consignor.PK);
		}

		public void TestIsApplicableToPaymentTermFilteringForRevenue()
		{
			var shipment = CreateShipment();
			var ratingAdapter = shipment.RatingAdapter;
			Assert(!ratingAdapter.IsApplicableToPaymentTermFiltering(ChargeCodeGroupList.Codes.Freight, CostSell.Revenue));
			Assert(!ratingAdapter.IsApplicableToPaymentTermFiltering(ChargeCodeGroupList.Codes.Insurance, CostSell.Revenue));
			Assert(!ratingAdapter.IsApplicableToPaymentTermFiltering(ChargeCodeGroupList.Codes.Origin, CostSell.Revenue));
			Assert(!ratingAdapter.IsApplicableToPaymentTermFiltering(ChargeCodeGroupList.Codes.Destination, CostSell.Revenue));
			Assert(!ratingAdapter.IsApplicableToPaymentTermFiltering(ChargeCodeGroupList.Codes.Loading, CostSell.Revenue));
			Assert(!ratingAdapter.IsApplicableToPaymentTermFiltering(ChargeCodeGroupList.Codes.Unloading, CostSell.Revenue));
		}

		public void TestGetContractNumberConfiguration_ShouldAddContractNumberQueryFilterFlag()
		{
			var shipment = CreateShipment();
			var ratingAdapter = shipment.RatingAdapter;
			var registryKey = AgencyRegistry.Instance.ReplaceExistingContractNumbersWithNewNumbers;
			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(configuration.ShouldAddContractNumberQueryFilter);
				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(configuration.ShouldAddContractNumberQueryFilter);
			}

			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(configuration.ShouldAddContractNumberQueryFilter);
				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(!configuration.ShouldAddContractNumberQueryFilter);
			}
		}

		public void TestGetContractNumberConfiguration_ShouldApplySpecificAdapterContractNumberFilterFlag()
		{
			var shipment = CreateShipment();
			var ratingAdapter = shipment.RatingAdapter;
			var registryKey = AgencyRegistry.Instance.ReplaceExistingContractNumbersWithNewNumbers;
			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(!configuration.ShouldApplySpecificAdapterContractNumberFilter);
				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(configuration.ShouldApplySpecificAdapterContractNumberFilter);
			}

			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(!configuration.ShouldApplySpecificAdapterContractNumberFilter);
				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(configuration.ShouldApplySpecificAdapterContractNumberFilter);
			}
		}

		public void TestGetContractNumberConfiguration_ShouldIgnoreJobCarrierContractNumbersFlag()
		{
			var shipment = CreateShipment();
			var ratingAdapter = shipment.RatingAdapter;
			var registryKey = AgencyRegistry.Instance.ReplaceExistingContractNumbersWithNewNumbers;
			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(!configuration.ShouldIgnoreJobCarrierContractNumbers);
				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(!configuration.ShouldIgnoreJobCarrierContractNumbers);
			}

			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(!configuration.ShouldIgnoreJobCarrierContractNumbers);
				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(!configuration.ShouldIgnoreJobCarrierContractNumbers);
			}
		}

		public void TestGetContractNumberConfiguration_ShouldIgnoreJobClientContractNumbersFlag()
		{
			var shipment = CreateShipment();
			var ratingAdapter = shipment.RatingAdapter;
			var registryKey = AgencyRegistry.Instance.ReplaceExistingContractNumbersWithNewNumbers;
			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(!configuration.ShouldIgnoreJobClientContractNumbers);
				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(!configuration.ShouldIgnoreJobClientContractNumbers);
			}

			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(configuration.ShouldIgnoreJobClientContractNumbers);
				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(configuration.ShouldIgnoreJobClientContractNumbers);
			}
		}

		public void TestGetContractNumberConfiguration_ShouldMatchJobBlankContractNumberFlag()
		{
			var shipment = CreateShipment();
			var ratingAdapter = shipment.RatingAdapter;
			var registryKey = AgencyRegistry.Instance.ReplaceExistingContractNumbersWithNewNumbers;
			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(configuration.ShouldMatchJobBlankContractNumber);
				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(configuration.ShouldMatchJobBlankContractNumber);
			}

			using (registryKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
				Assert(configuration.ShouldMatchJobBlankContractNumber);
				configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
				Assert(configuration.ShouldMatchJobBlankContractNumber);
			}
		}

		#region Implementation
		public abstract AgencyShipment CreateShipment();
		public abstract AgencyShipmentContainer AddNewContainer(AgencyShipment shipment);
		public abstract string ConsumerType { get; }

		JobSailing CreateSailing(ZString load, ZString discharge, ZDateTime etd)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = etd;
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		void SetupSailing(bool countryAllocation, bool originAllocation, bool sailingAllocation)
		{
			Voyage = Factory.New<JobVoyage>();
			Origin1 = Voyage.Origins.AddNew();
			Origin1.JA_RL_NKPortOfLoading = "AUBNE";
			Origin1.JA_E_DEP = ZDateTime.Now.AddDays(10);
			Origin2 = Voyage.Origins.AddNew();
			Origin2.JA_RL_NKPortOfLoading = "AUDRW";
			Origin2.JA_E_DEP = ZDateTime.Now.AddDays(16);
			Origin3 = Voyage.Origins.AddNew();
			Origin3.JA_RL_NKPortOfLoading = "SGSIN";
			Origin3.JA_E_DEP = ZDateTime.Now.AddDays(22);
			var destination1 = Voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUDRW";
			destination1.JB_E_ARV = ZDateTime.Now.AddDays(14);
			var destination2 = Voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "SGSIN";
			destination2.JB_E_ARV = ZDateTime.Now.AddDays(20);
			var destination3 = Voyage.Destinations.AddNew();
			destination3.JB_RL_NKPortOfDischarge = "MYBAG";
			destination3.JB_E_ARV = ZDateTime.Now.AddDays(26);
			Sailing1 = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin1.JA_RL_NKPortOfLoading, destination2.JB_RL_NKPortOfDischarge);
			Sailing2 = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin1.JA_RL_NKPortOfLoading, destination1.JB_RL_NKPortOfDischarge);
			Sailing3 = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin2.JA_RL_NKPortOfLoading, destination2.JB_RL_NKPortOfDischarge);
			Sailing4 = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin3.JA_RL_NKPortOfLoading, destination3.JB_RL_NKPortOfDischarge);
			string allocationMethod = AllocationMethodList.Codes.NotSet;
			if (countryAllocation)
			{
				allocationMethod = AllocationMethodList.Codes.Country;
				foreach (var country in new VoyageCountry[] { Origin1.VoyageCountry, Origin3.VoyageCountry })
				{
					var allocation = country.SlotAllocations.GetAllocation(ZGuid.Empty);
					allocation.SetAspect(AllocationAspectTypes.TEU, 80);
					allocation.SetAspect(AllocationAspectTypes.Tonnes, 240);
				}
			}

			if (originAllocation)
			{
				allocationMethod = AllocationMethodList.Codes.Origin;
				foreach (var origin in new VoyageOrigin[] { Origin1, Origin2, Origin3 })
				{
					var allocation = origin.SlotAllocations.GetAllocation(ZGuid.Empty);
					allocation.SetAspect(AllocationAspectTypes.TEU, 80);
					allocation.SetAspect(AllocationAspectTypes.Tonnes, 240);
				}
			}

			if (sailingAllocation)
			{
				allocationMethod = AllocationMethodList.Codes.Sailing;
				foreach (var sailing in new JobSailing[] { Sailing1, Sailing2, Sailing3, Sailing4 })
				{
					var allocation = sailing.SlotAllocations.GetAllocation(ZGuid.Empty);
					allocation.SetAspect(AllocationAspectTypes.TEU, 40);
					allocation.SetAspect(AllocationAspectTypes.Tonnes, 160);
				}
			}

			foreach (var country in new VoyageCountry[] { Origin1.VoyageCountry, Origin3.VoyageCountry })
			{
				country.J0_AllocationMethod = allocationMethod;
			}
		}

		protected ZGuid RC_20GP_PK
		{
			get
			{
				if (!rc_20GP_PK.IsValid)
				{
					rc_20GP_PK = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				}

				return rc_20GP_PK;
			}
		}

		ZGuid rc_20GP_PK;
		JobVoyage Voyage;
		VoyageOrigin Origin1;
		VoyageOrigin Origin2;
		VoyageOrigin Origin3;
		JobSailing Sailing1;
		JobSailing Sailing2;
		JobSailing Sailing3;
		JobSailing Sailing4;
		#endregion
	}
}
