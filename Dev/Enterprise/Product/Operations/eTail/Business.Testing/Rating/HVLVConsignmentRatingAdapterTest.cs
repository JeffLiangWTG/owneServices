using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.eTail.Business.Rating;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVConsignmentRatingAdapterTest : TestCaseWithFactory
	{
		public void TestDeliveryAddressIsConsigneeAddress()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment.HVC_ConsigneeState = "NSW";
			consignment.HVC_ConsigneePostcode = "2222";
			consignment.HVC_ConsigneeCity = "Mascot";

			var adapter = new HVLVConsignmentRatingAdapter(consignment);
			var address = adapter.DeliveryAddress;

			CombineAssertions(() =>
			{
				AssertEquals("AU", address.CountryCode);
				AssertEquals("NSW", address.E2_State);
				AssertEquals("2222", address.E2_Postcode);
				AssertEquals("Mascot", address.E2_City);
			});
		}

		public void TestHasRateLineConditionsSupporter()
		{
			var (_, _, consignment) = GetBasicTestData();
			var adapter = new HVLVConsignmentRatingAdapter(consignment) as IAutoRatingFreightConditionsSupportable;
			AssertNotNull("HVLVConsignmentRatingAdapter should be a IAutoRatingFreightConditionsSupportable", adapter);
			AssertNotNull("HVLVConsignmentRatingAdapter should have a valid ConditionsSupporter", adapter.ConditionsSupporter);
			AssertType<ShipmentRateLineConditionsSupporter>("HVLVConsignmentRatingAdapter's ConditionsSupporter should be a ShipmentRateLineConditionsSupporter", adapter.ConditionsSupporter);
		}

		public void TestConstantProperties()
		{
			var (consol, shipment, consignment1) = GetBasicTestData();
			var adapter = new HVLVConsignmentRatingAdapter(consignment1);

			CombineAssertions(delegate
			{
				AssertExceptionThrown<ArgumentNullException>(() => new HVLVConsignmentRatingAdapter(null));
				AssertEquals("Adapter Type", AdapterType.HVLVShipment, adapter.AdapterType);
				AssertEquals("Operational Job Code", consignment1.HVC_WaybillNumber, adapter.OperationalJobCode);
				AssertEquals("Consumer Type", JobInvoicingConsumerTypes.eManifest, adapter.ConsumerType);
				AssertEquals("Merge Charges", MergeChargeOptions.HLSMerge, adapter.MergeCharges);
				AssertEquals("Rate Type", RateType.Forwarding, adapter.RateTypeToUse);
				AssertEquals("Job Direction", Directions.Import, adapter.JobDirection);

				AssertEquals("Service Level", "ABC", adapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
				AssertEquals("Release Type", "REL", adapter.HousebillReleaseType);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
				AssertEquals(shipment.ConsignorDocumentaryAddress.OrganisationPK, adapter.Consignor.PK);
				AssertEquals(shipment.ConsigneeDocumentaryAddress.OrganisationPK, adapter.Consignee.PK);

				AssertNull("Carrier", adapter.Carrier);
				AssertNull("Via", adapter.GetVia(CostSell.Cost));
				AssertNull("Via", adapter.GetVia(CostSell.Revenue));

				Assert("Status Information", adapter.StatusInformation.CanExecute);
				AssertType<CommonShipmentJobDatesProvider>("Job Dates Provider", adapter.JobDatesProvider);
			});
		}

		public void TestMeasures()
		{
			var (consol, shipment, consignment1) = GetBasicTestData();

			var box = consignment1.Items.AddNew();
			box.HVI_ActualWeight = 5m;
			box.HVI_ActualVolume = 0.005;
			box.HVI_JS_LoadedOnShipment = shipment.PK;
			box.HVI_F3_NKPackType = "BOX";

			var carton = consignment1.Items.AddNew();
			carton.HVI_ActualWeight = 10m;
			carton.HVI_ActualVolume = 0.2;
			carton.HVI_JS_LoadedOnShipment = shipment.PK;
			carton.HVI_F3_NKPackType = "CTN";

			var adapter = new HVLVConsignmentRatingAdapter(consignment1);

			CombineAssertions(delegate
			{
				var measures = (RateableMeasureSet)adapter.RateableMeasures;
				AssertEquals(4, measures.MeasureTypeCount);

				AssertEquals(Weight.Kilograms, measures.GetUnit(MeasureType.Weight));
				AssertEquals(15m, measures.GetActual(MeasureType.Weight));

				AssertEquals(Volume.CubicMetres, measures.GetUnit(MeasureType.Volume));
				AssertEquals(0.205m, measures.GetActual(MeasureType.Volume));

				var units = measures.GetPartList(MeasureType.Unit).OfType<IRateableContainer>().ToArray();
				AssertEquals("Should have 2 unit measures.", 2, units.Length);
				AssertEquals("Package Type should be correct for the first unit.", "BOX", units[0].PackageType);
				AssertEquals("Weight should be correct for the first unit.", 5m, units[0].ContainerWeightInKG);
				AssertEquals("Volume should be correct for the first unit.", 0.005m, units[0].ContainerVolumeInM3);

				AssertEquals("Package Type should be correct for the first unit.", "CTN", units[1].PackageType);
				AssertEquals("Weight should be correct for the second unit.", 10m, units[1].ContainerWeightInKG);
				AssertEquals("Volume should be correct for the second unit.", 0.2m, units[1].ContainerVolumeInM3);

				AssertEquals(true, measures.PackageCountHasContainerType);
				AssertEquals((ZDecimal)2, measures.GetActual(MeasureType.Package));
			});
		}

		public void TestMeasures_PrioritiseManifestedValuesIfActualIsZero()
		{
			var (_, _, consignment1) = GetBasicTestData();
			var item1 = consignment1.Items.AddNew();
			item1.HVI_ManifestedWeight = 5m;
			item1.HVI_ManifestedVolume = 0.001m;
			item1.HVI_F3_NKPackType = PkgUnit.Bag;

			var item2 = consignment1.Items.AddNew();
			item2.HVI_ManifestedWeight = 5m;
			item2.HVI_ManifestedVolume = 0.001m;
			item2.HVI_F3_NKPackType = PkgUnit.Bag;

			var adapter = new HVLVConsignmentRatingAdapter(consignment1);

			CombineAssertions("Should only use manifested weight/volume since actual is 0.", delegate
			{
				var measures = (RateableMeasureSet)adapter.RateableMeasures;
				AssertEquals(10m, measures.GetActual(MeasureType.Weight));
				AssertEquals(0.002m, measures.GetActual(MeasureType.Volume));

				var units = measures.GetPartList(MeasureType.Unit).OfType<IRateableContainer>().ToArray();
				AssertEquals("Should have 2 unit measures.", 2, units.Length);
				AssertEquals("Both unit measures should have the same values.", true, units.All(x => x.ContainerWeightInKG == 5m && x.ContainerVolumeInM3 == 0.001m));
			});

			item1.HVI_ActualWeight = 10m;
			item1.HVI_ActualVolume = 0.004m;

			CombineAssertions("Should now use actual weights for item1 since they are non zero.", delegate
			{
				var measures = (RateableMeasureSet)adapter.RateableMeasures;
				AssertEquals(15m, measures.GetActual(MeasureType.Weight));
				AssertEquals(0.005m, measures.GetActual(MeasureType.Volume));

				var units = measures.GetPartList(MeasureType.Unit).OfType<IRateableContainer>().ToArray();
				AssertEquals("Should have 2 unit measures.", 2, units.Length);
				AssertEquals("Weight should be correct for the first unit.", 10m, units[0].ContainerWeightInKG);
				AssertEquals("Volume should be correct for the first unit.", 0.004m, units[0].ContainerVolumeInM3);

				AssertEquals("Weight should be correct for the second unit.", 5m, units[1].ContainerWeightInKG);
				AssertEquals("Volume should be correct for the second unit.", 0.001m, units[1].ContainerVolumeInM3);
			});

			item1.HVI_ActualWeight = 0m;
			item1.HVI_ActualVolume = 0m;

			CombineAssertions("The measures should now reset to using manifested since they are zero again", delegate
			{
				var measures = (RateableMeasureSet)adapter.RateableMeasures;
				AssertEquals(10m, measures.GetActual(MeasureType.Weight));
				AssertEquals(0.002m, measures.GetActual(MeasureType.Volume));

				var units = measures.GetPartList(MeasureType.Unit).OfType<IRateableContainer>().ToArray();
				AssertEquals("Should have 2 unit measures.", 2, units.Length);
				AssertEquals("Weight should be correct for the first unit.", 5m, units[0].ContainerWeightInKG);
				AssertEquals("Volume should be correct for the first unit.", 0.001m, units[0].ContainerVolumeInM3);

				AssertEquals("Weight should be correct for the second unit.", 5m, units[1].ContainerWeightInKG);
				AssertEquals("Volume should be correct for the second unit.", 0.001m, units[1].ContainerVolumeInM3);
			});
		}

		public void TestDestination()
		{
			var (consol, shipment, consignment1) = GetBasicTestData();
			var adapter = new HVLVConsignmentRatingAdapter(consignment1);

			shipment.JS_RL_NKDestination = "AUBNE";
			consol.JK_RL_NKDischargePort = "AUSYD";

			AssertEquals("AUBNE", adapter.Destination.Code);

			shipment.JS_RL_NKDestination = null;

			AssertEquals("AUSYD", adapter.Destination.Code);
		}

		public void TestOrigin()
		{
			var (consol, shipment, consignment1) = GetBasicTestData();
			var adapter = new HVLVConsignmentRatingAdapter(consignment1);

			shipment.JS_RL_NKOrigin = "AUBNE";
			consol.JK_RL_NKLoadPort = "AUSYD";

			AssertEquals("AUBNE", adapter.Origin.Code);

			shipment.JS_RL_NKOrigin = null;

			AssertEquals("AUSYD", adapter.Origin.Code);
		}

		public void TestConsignorPickupAddress()
		{
			var (_, shipment, consignment) = GetBasicTestData();

			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			header.Consignments.Add(consignment);

			var orgAddress1 = Factory.New<OrgAddress>();
			orgAddress1.OA_Address1 = "Address1";

			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_Address1 = "Address2";

			consignment.BookingHeader.HVH_OA_DispatchAddress = orgAddress2.PK;
			shipment.ConsignorPickupAddress.E2_OA_Address = orgAddress1.PK;

			var adapter = new HVLVConsignmentRatingAdapter(consignment);

			AssertEquals("Address2", adapter.PickupAddress.E2_Address1);

			consignment.BookingHeader.HVH_OA_DispatchAddress = ZGuid.Empty;

			AssertEquals("Address1", adapter.PickupAddress.E2_Address1);
		}

		public void TestConsignorPickupAddress_NoBookingHeader()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.LCL;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RS_NKServiceLevel = "ABC";
			shipment.JS_ReleaseType = "REL";

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "Address";

			shipment.ConsignorPickupAddress.E2_OA_Address = orgAddress.PK;

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = consignmentHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();

			var adapter = new HVLVItemRatingAdapter(item, consignment, shipment);

			AssertNoExceptionThrown("Should not throw exception when accessing Pickup Address when there is no booking header", () =>
			{
				AssertEquals("Should return Consignor Pickup Address from shipment", "Address", adapter.PickupAddress.E2_Address1);
			});
		}

		public void TestCartageEquipment()
		{
			var (_, shipment, consignment1) = GetBasicTestData();
			var docAndCartageAddress = shipment.DocsAndCartage;
			docAndCartageAddress.JP_FCLPickupEquipmentNeeded = "ZZZ";
			docAndCartageAddress.JP_FCLDeliveryEquipmentNeeded = "YYY";

			var adapter = new HVLVConsignmentRatingAdapter(consignment1);

			AssertEquals("ZZZ", adapter.PickupCartageEquipment);
			AssertEquals("YYY", adapter.DeliveryCartageEquipment);
		}

		public void TestPaymentTerm()
		{
			var (_, shipment, consignment1) = GetBasicTestData();
			var adapter = new HVLVConsignmentRatingAdapter(consignment1);
			shipment.JS_INCO = IncoTerms.ExWorks;

			AssertPaymentTerm(adapter, PaymentTermType.Incoterm, CostSell.Revenue, IncoTerms.ExWorks);

			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.IsDomesticFreight = true;
			shipment.JS_INCO = DomesticPaymentTerms.Prepaid;

			AssertPaymentTerm(adapter, PaymentTermType.DomesticPaymentTerm, CostSell.Revenue, DomesticPaymentTerms.Prepaid);
		}

		public void TestChargeCodeGroups()
		{
			var (consol, _, consignment1) = GetBasicTestData();
			var adapter = new HVLVConsignmentRatingAdapter(consignment1);

			AssertEquals(ChargeCodeFilter.AutorateAll, adapter.ChargeCodeGroups.SellChargesFilter);
			AssertEquals(ChargeCodeFilter.AutorateAll, adapter.ChargeCodeGroups.CostChargesFilter);

			consol.TopLevelShipments.AddNew();
			AssertEquals(ChargeCodeFilter.AutorateAll, adapter.ChargeCodeGroups.SellChargesFilter);

			var cost = (BusinessObject)Factory.New<IJobConsolCost>();
			cost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				cost[JobConsolCostSchema.E6_ParentID] = consol.PK;
				cost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				cost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}

			cost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;

			AssertEquals(ChargeCodeFilter.AutorateNonConsolLevelOnly, adapter.ChargeCodeGroups.CostChargesFilter);
		}

		public void TestFreightMode()
		{
			var (_, shipment, consignment1) = GetBasicTestData();
			var adapter = new HVLVConsignmentRatingAdapter(consignment1);
			var containerMask = FreightMode.NonContainerised | FreightMode.Containerised;

			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			AssertEquals("Sea FCL is reported as an FCL job", FreightMode.FCL, adapter.FreightMode);

			shipment.JS_PackingMode = ContainerModes.Liquid;
			AssertEquals("Liquid should show as NonContainerised", FreightMode.NonContainerised, adapter.FreightMode & containerMask);

			shipment.JS_TransportMode = TransportModes.AirSea;
			shipment.JS_PackingMode = ContainerModes.Loose;
			AssertEquals("First Air then Sea, Loose, is reported as an AIR Loose job", FreightMode.LSE, adapter.FreightMode);

			shipment.JS_PackingMode = ContainerModes.ULD;
			AssertEquals("First Air then Sea, ULD, is reported as an AIR ULD job", FreightMode.ULD, adapter.FreightMode);

			shipment.JS_TransportMode = TransportModes.SeaAir;
			shipment.JS_PackingMode = ContainerModes.LCL;
			AssertEquals("First Sea then Air LCL is reported as an LCL job", FreightMode.LCL, adapter.FreightMode);

			shipment.JS_PackingMode = ContainerModes.LTL;
			AssertEquals("First Sea then Air LTL is reported as an LCL job", FreightMode.LCL, adapter.FreightMode);

			shipment.JS_TransportMode = TransportModes.Road;
			shipment.JS_PackingMode = ContainerModes.FTL;
			AssertEquals("Road FTL is reported as an FTL job", FreightMode.FTL, adapter.FreightMode);

			shipment.JS_TransportMode = TransportModes.Rail;
			shipment.JS_PackingMode = ContainerModes.LCL;
			AssertEquals("Rail LCL is reported as an LRA job", FreightMode.LRA, adapter.FreightMode);

			shipment.JS_TransportMode = TransportModes.Rail;
			shipment.JS_PackingMode = ContainerModes.FCL;
			AssertEquals("Rail FCL is reported as an FRA job", FreightMode.FRA, adapter.FreightMode);

			shipment.JS_TransportMode = TransportModes.Road;
			shipment.JS_PackingMode = ContainerModes.LCL;
			AssertEquals("Road LCL is reported as an LRO job", FreightMode.LRO, adapter.FreightMode);

			shipment.JS_TransportMode = TransportModes.Road;
			shipment.JS_PackingMode = ContainerModes.FCL;
			AssertEquals("Road FCL is reported as an FRO job", FreightMode.FRO, adapter.FreightMode);

			shipment.JS_TransportMode = TransportModes.Courier;
			AssertEquals("Courier is reported as an OBC job", FreightMode.OBC, adapter.FreightMode);
		}

		public void TestWharfCTOAddress()
		{
			var shipment = Factory.New<CommonShipment>();

			var consignment1 = Factory.New<HVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;

			var originConsol = shipment.Consols.AddNew();
			var destinationConsol = shipment.Consols.AddNew();

			var adapter = new HVLVConsignmentRatingAdapter(consignment1);

			originConsol.JK_OA_DepartureCTOAddress = Factory.New<OrgAddress>().PK;
			originConsol.JK_OA_PackDepotAddress = Factory.New<OrgAddress>().PK;
			destinationConsol.JK_OA_ArrivalCTOAddress = Factory.New<OrgAddress>().PK;
			destinationConsol.JK_OA_UnpackDepotAddress = Factory.New<OrgAddress>().PK;

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			originConsol.JK_RL_NKLoadPort = "USLAX";
			originConsol.JK_RL_NKDischargePort = "NZAKL";
			destinationConsol.JK_RL_NKLoadPort = "NZAKL";
			destinationConsol.JK_RL_NKDischargePort = "AUSYD";

			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Australia);
			Assert(adapter.IsImport());

			shipment.JS_TransportMode = TransportModes.Sea;
			originConsol.JK_TransportMode = TransportModes.Sea;
			destinationConsol.JK_TransportMode = TransportModes.Sea;
			AssertEquals(destinationConsol.JK_OA_ArrivalCTOAddress, adapter.WharfCTOAddress.PK);

			shipment.JS_TransportMode = TransportModes.Rail;
			originConsol.JK_TransportMode = TransportModes.Rail;
			destinationConsol.JK_TransportMode = TransportModes.Rail;
			AssertEquals(destinationConsol.JK_OA_ArrivalCTOAddress, adapter.WharfCTOAddress.PK);

			shipment.JS_TransportMode = TransportModes.Air;
			originConsol.JK_TransportMode = TransportModes.Air;
			destinationConsol.JK_TransportMode = TransportModes.Air;
			AssertEquals(destinationConsol.JK_OA_ArrivalCTOAddress, adapter.WharfCTOAddress.PK);

			shipment.JS_TransportMode = TransportModes.Road;
			originConsol.JK_TransportMode = TransportModes.Road;
			destinationConsol.JK_TransportMode = TransportModes.Road;
			AssertEquals(destinationConsol.JK_OA_UnpackDepotAddress, adapter.WharfCTOAddress.PK);

			shipment.JS_OA_ImportReleaseDepot = Factory.New<OrgAddress>().PK;
			AssertEquals(shipment.JS_OA_ImportReleaseDepot, adapter.WharfCTOAddress.PK);

			GlbCompany.CurrentCompany.SetCountry(CountryCodes.UnitedStates);
			Assert(!adapter.IsImport());

			shipment.JS_TransportMode = TransportModes.Sea;
			originConsol.JK_TransportMode = TransportModes.Sea;
			destinationConsol.JK_TransportMode = TransportModes.Sea;
			AssertEquals(originConsol.JK_OA_DepartureCTOAddress, adapter.WharfCTOAddress.PK);

			shipment.JS_TransportMode = TransportModes.Rail;
			originConsol.JK_TransportMode = TransportModes.Rail;
			destinationConsol.JK_TransportMode = TransportModes.Rail;
			AssertEquals(originConsol.JK_OA_DepartureCTOAddress, adapter.WharfCTOAddress.PK);

			shipment.JS_TransportMode = TransportModes.Air;
			originConsol.JK_TransportMode = TransportModes.Air;
			destinationConsol.JK_TransportMode = TransportModes.Air;
			AssertEquals(originConsol.JK_OA_DepartureCTOAddress, adapter.WharfCTOAddress.PK);

			shipment.JS_TransportMode = TransportModes.Road;
			originConsol.JK_TransportMode = TransportModes.Road;
			destinationConsol.JK_TransportMode = TransportModes.Road;
			AssertEquals(originConsol.JK_OA_PackDepotAddress, adapter.WharfCTOAddress.PK);

			shipment.JS_OA_ExportReceivingDepot = Factory.New<OrgAddress>().PK;
			AssertEquals(shipment.JS_OA_ExportReceivingDepot, adapter.WharfCTOAddress.PK);
		}

		#region Helpers

		(CommonConsol consol, ForwardingShipment shipment, HVLVConsignment consignment) GetBasicTestData()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.LCL;
			shipment.ConsignorPK = client.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RS_NKServiceLevel = "ABC";
			shipment.JS_ReleaseType = "REL";

			consol.Shipments.Add(shipment);

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			return (consol, shipment, consignment);
		}

		void AssertPaymentTerm(HVLVConsignmentRatingAdapter adapter, PaymentTermType paymentTermType, CostSell costOrSell, string value)
		{
			var info = adapter.PaymentTerm.GetPaymentTermInfo(costOrSell);

			AssertNotNull(info);
			AssertEquals(paymentTermType, info.InfoType);
			AssertEquals(value, info.Value);
		}

		#endregion
	}
}
