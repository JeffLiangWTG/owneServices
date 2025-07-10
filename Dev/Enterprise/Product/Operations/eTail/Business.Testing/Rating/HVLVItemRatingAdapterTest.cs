using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
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
	public class HVLVItemRatingAdapterTest : TestCaseWithFactory
	{
		public void TestDeliveryAddressIsConsigneeAddress()
		{
			var (_, shipment, consignment, item) = GetBasicTestData();
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment.HVC_ConsigneeState = "NSW";
			consignment.HVC_ConsigneePostcode = "2222";
			consignment.HVC_ConsigneeCity = "Mascot";

			var adapter = new HVLVItemRatingAdapter(item, consignment, shipment);
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
			var (_, shipment, consignment, item) = GetBasicTestData();
			var adapter = new HVLVItemRatingAdapter(item, consignment, shipment) as IAutoRatingFreightConditionsSupportable;
			AssertNotNull("HVLVItemRatingAdapter should be a IAutoRatingFreightConditionsSupportable", adapter);
			AssertNotNull("HVLVItemRatingAdapter should have a valid ConditionsSupporter", adapter.ConditionsSupporter);
			AssertType<HVLVItemRateLineConditionsSupporter>("HVLVItemRatingAdapter's ConditionsSupporter should be a HVLVItemRateLineConditionsSupporter", adapter.ConditionsSupporter);
		}

		public void TestConstantProperties()
		{
			var (_, shipment, consignment, item) = GetBasicTestData();
			var adapter = new HVLVItemRatingAdapter(item, consignment, shipment);
			consignment.HVC_RS_NKServiceLevel = "STD";

			CombineAssertions(delegate
			{
				AssertExceptionThrown<ArgumentNullException>("A null exception should be thrown", () => new HVLVItemRatingAdapter(null, null, null));
				AssertEquals("Adapter Type", AdapterType.HVLVShipment, adapter.AdapterType);
				AssertEquals("Operational Job Code", item.HVI_ItemId, adapter.OperationalJobCode);
				AssertEquals("Consumer Type", JobInvoicingConsumerTypes.eManifest, adapter.ConsumerType);
				AssertEquals("Merge Charges", MergeChargeOptions.HLSMerge, adapter.MergeCharges);
				AssertEquals("Rate Type", RateType.Forwarding, adapter.RateTypeToUse);
				AssertEquals("Job Direction", Directions.Import, adapter.JobDirection);

				AssertEquals("Service Level", consignment.HVC_RS_NKServiceLevel, adapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
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
			var (_, shipment, consignment, itemFromSetup) = GetBasicTestData();
			var listOfPackageUnitTypes = typeof(PkgUnit).GetFields().Select(field => field.GetValue(null) as string);
			itemFromSetup.HVI_ActualWeight = 5m;
			itemFromSetup.HVI_ActualVolume = 0.005m;
			itemFromSetup.HVI_F3_NKPackType = "PKG";

			foreach (var pkgType in listOfPackageUnitTypes)
			{
				var newItem = consignment.Items.AddNew();
				newItem.HVI_ActualWeight = 5m;
				newItem.HVI_ActualVolume = 0.005m;
				newItem.HVI_JS_LoadedOnShipment = shipment.PK;
				newItem.HVI_F3_NKPackType = pkgType;
			}

			foreach (HVLVItem item in consignment.Items)
			{
				CombineAssertions(delegate
				{
					var measures = (RateableMeasureSet)new HVLVItemRatingAdapter(item, consignment, shipment).RateableMeasures;

					AssertEquals("Item ID: " + item.HVI_ItemId + " Weight Unit:", Weight.Kilograms, measures.GetUnit(MeasureType.Weight));
					AssertEquals("Item ID: " + item.HVI_ItemId + " Weight Value:", 5m, measures.GetActual(MeasureType.Weight));

					AssertEquals("Item ID: " + item.HVI_ItemId + " Volume Unit:", Volume.CubicMetres, measures.GetUnit(MeasureType.Volume));
					AssertEquals("Item ID: " + item.HVI_ItemId + " Volume Value:", 0.005m, measures.GetActual(MeasureType.Volume));
				});
			}
		}

		public void TestMeasures_Packages()
		{
			var (_, shipment, consignment, _) = GetBasicTestData();
			consignment.Items.RemoveAndDeleteAll();
			consignment.HVC_WeightUQ = "T";

			var pkg1 = consignment.Items.AddNew();
			pkg1.HVI_ActualWeight = 5;
			pkg1.HVI_ActualVolume = 2;
			pkg1.HVI_F3_NKPackType = "PLT";

			var pkg2 = consignment.Items.AddNew();
			pkg2.HVI_ActualWeight = 7;
			pkg2.HVI_ActualVolume = 3;
			pkg2.HVI_F3_NKPackType = "BOX";

			var measures1 = (RateableMeasureSet)new HVLVItemRatingAdapter(pkg1, consignment, shipment).RateableMeasures;
			AssertUnitAndPackageContainers(measures1, "PLT", 5000m, 2m);

			var measures2 = (RateableMeasureSet)new HVLVItemRatingAdapter(pkg2, consignment, shipment).RateableMeasures;
			AssertUnitAndPackageContainers(measures2, "BOX", 7000m, 3m);

			void AssertUnitAndPackageContainers(
				RateableMeasureSet actualMeasureSet,
				string expectedPackageType,
				decimal expectedWeightInKg,
				decimal expectedVolumeInM3)
			{
				AssertContainerMeasure((IRateableContainer)actualMeasureSet.GetPartList(MeasureType.Unit).Single());
				AssertContainerMeasure((IRateableContainer)actualMeasureSet.GetPartList(MeasureType.Package).Single());

				void AssertContainerMeasure(IRateableContainer container)
				{
					CombineAssertions(
						"Rateable Container should have the correct values mapped.",
						() =>
						{
							AssertEquals(nameof(IRateableContainer.PackageType), expectedPackageType, container.PackageType);
							AssertEquals(nameof(IRateableContainer.ContainerWeightInKG), expectedWeightInKg, container.ContainerWeightInKG);
							AssertEquals(nameof(IRateableContainer.ContainerVolumeInM3), expectedVolumeInM3, container.ContainerVolumeInM3);
							AssertEquals(nameof(IRateableContainer.ContainerCount), 1, container.ContainerCount);
							AssertEquals(nameof(IRateableContainer.PackageCount), 1m, container.PackageCount);
							AssertEquals(nameof(IRateableContainer.UnitCount), 1m, container.UnitCount);
						});
				}
			}
		}

		public void TestMeasures_PrioritiseManifestedValuesIfActualIsZero()
		{
			var (_, shipment, consignment, item) = GetBasicTestData();
			item.HVI_ManifestedWeight = 5m;
			item.HVI_ManifestedVolume = 0.001m;
			item.HVI_F3_NKPackType = PkgUnit.Bag;

			var adapter = new HVLVItemRatingAdapter(item, consignment, shipment);

			CombineAssertions("Should only use manifested weight/volume since actual is 0.", () =>
			{
				var measures = (RateableMeasureSet)new HVLVItemRatingAdapter(item, consignment, shipment).RateableMeasures;
				AssertEquals(5m, measures.GetActual(MeasureType.Weight));
				AssertEquals(0.001m, measures.GetActual(MeasureType.Volume));
			});

			item.HVI_ActualWeight = 10m;
			item.HVI_ActualVolume = 0.004m;

			CombineAssertions("Should now use actual weights for item1 since they are non zero.", () =>
			{
				var measures = (RateableMeasureSet)new HVLVItemRatingAdapter(item, consignment, shipment).RateableMeasures;
				AssertEquals(10m, measures.GetActual(MeasureType.Weight));
				AssertEquals(0.004m, measures.GetActual(MeasureType.Volume));
			});

			item.HVI_ActualWeight = 0m;
			item.HVI_ActualVolume = 0m;

			CombineAssertions("The measures should now reset to using manifested since they are zero again", () =>
			{
				var measures = (RateableMeasureSet)new HVLVItemRatingAdapter(item, consignment, shipment).RateableMeasures;
				AssertEquals(5m, measures.GetActual(MeasureType.Weight));
				AssertEquals(0.001m, measures.GetActual(MeasureType.Volume));
			});
		}

		public void TestCreditors()
		{
			var (_, shipment, consignment, item) = GetBasicTestData();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_FullName = "Last Mile Carrier";
			consignment.HVC_OH_LastMileCarrier = carrier.PK;

			var exportBroker = Factory.NewWithValidTestData<OrgHeader>();
			exportBroker.OH_FullName = "Export Broker";
			shipment.JS_OH_ExportBroker = exportBroker.PK;

			var pickupTransport = Factory.NewWithValidTestData<OrgHeader>();
			pickupTransport.OH_FullName = "Pickup Transport";
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = pickupTransport.MainAddress.PK;

			var pickupAgent = Factory.NewWithValidTestData<OrgHeader>();
			pickupAgent.OH_FullName = "Pickup Agent";
			shipment.PickupAgentDocumentaryAddress.OrganisationPK = pickupAgent.PK;

			var pickupWarehouse = Factory.NewWithValidTestData<OrgHeader>();
			pickupWarehouse.OH_FullName = "Pickup Warehouse";
			shipment.JS_OA_ExportReceivingDepot = pickupWarehouse.MainAddress.PK;

			var importBroker = Factory.NewWithValidTestData<OrgHeader>();
			importBroker.OH_FullName = "Import Broker";
			shipment.JS_OH_ImportBroker = importBroker.PK;

			var deliveryTransport = Factory.NewWithValidTestData<OrgHeader>();
			deliveryTransport.OH_FullName = "Delivery Transport";
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = deliveryTransport.MainAddress.PK;

			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			deliveryAgent.OH_FullName = "Delivery Agent";
			shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;

			var deliveryWarehouse = Factory.NewWithValidTestData<OrgHeader>();
			deliveryWarehouse.OH_FullName = "Delivery Warehouse";
			shipment.JS_OA_ImportReleaseDepot = deliveryWarehouse.MainAddress.PK;

			var adapter = new HVLVItemRatingAdapter(item, consignment, shipment);
			var creditors = adapter.Creditors.AllOrgs.Select(x => x.OH_FullName).ToList();

			var expectedCreditors = new[]
			{
				"Export Broker", "Pickup Transport", "Pickup Agent", "Pickup Warehouse",
				"Import Broker", "Delivery Transport", "Delivery Agent", "Delivery Warehouse",
				"Last Mile Carrier"
			};

			CombineAssertions(() =>
			{
				AssertEquals("Should contain 9 creditors", 9, creditors.Count);
				AssertContainsExactElementsInAnyOrder(expectedCreditors, creditors);
			});
		}

		public void TestDestination()
		{
			var (consol, shipment, consignment, item) = GetBasicTestData();
			var adapter = new HVLVItemRatingAdapter(item, consignment, shipment);

			shipment.JS_RL_NKDestination = "AUBNE";
			consol.JK_RL_NKDischargePort = "AUSYD";

			AssertEquals("AUBNE", adapter.Destination.Code);

			shipment.JS_RL_NKDestination = null;

			AssertEquals("AUSYD", adapter.Destination.Code);
		}

		public void TestOrigin()
		{
			var (consol, shipment, consignment, item) = GetBasicTestData();
			var adapter = new HVLVItemRatingAdapter(item, consignment, shipment);

			shipment.JS_RL_NKOrigin = "AUBNE";
			consol.JK_RL_NKLoadPort = "AUSYD";

			AssertEquals("AUBNE", adapter.Origin.Code);

			shipment.JS_RL_NKOrigin = null;

			AssertEquals("AUSYD", adapter.Origin.Code);
		}

		public void TestConsignorPickupAddress()
		{
			var (_, shipment, consignment, item) = GetBasicTestData();

			var orgAddress1 = Factory.New<OrgAddress>();
			orgAddress1.OA_Address1 = "Address1";

			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_Address1 = "Address2";

			shipment.ConsignorPickupAddress.E2_OA_Address = orgAddress2.PK;

			var adapter = new HVLVItemRatingAdapter(item, consignment, shipment);

			AssertEquals("Address2", adapter.PickupAddress.E2_Address1);

			shipment.ConsignorPickupAddress.E2_OA_Address = orgAddress1.PK;

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
			var (_, shipment, consignment, item) = GetBasicTestData();
			var docAndCartageAddress = shipment.DocsAndCartage;
			docAndCartageAddress.JP_FCLPickupEquipmentNeeded = "ZZZ";
			docAndCartageAddress.JP_FCLDeliveryEquipmentNeeded = "YYY";

			var adapter = new HVLVItemRatingAdapter(item, consignment, shipment);

			AssertEquals("ZZZ", adapter.PickupCartageEquipment);
			AssertEquals("YYY", adapter.DeliveryCartageEquipment);
		}

		public void TestPaymentTerm()
		{
			var (_, shipment, consignment, item) = GetBasicTestData();
			var adapter = new HVLVItemRatingAdapter(item, consignment, shipment);
			shipment.JS_INCO = IncoTerms.ExWorks;

			AssertPaymentTerm(adapter, PaymentTermType.Incoterm, CostSell.Revenue, IncoTerms.ExWorks);

			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.IsDomesticFreight = true;
			shipment.JS_INCO = DomesticPaymentTerms.Prepaid;

			AssertPaymentTerm(adapter, PaymentTermType.DomesticPaymentTerm, CostSell.Revenue, DomesticPaymentTerms.Prepaid);
		}

		public void TestChargeCodeGroups()
		{
			var (consol, shipment, consignment, item) = GetBasicTestData();
			var adapter = new HVLVItemRatingAdapter(item, consignment, shipment);

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

			CombineAssertions(() =>
			{
				Assert("Brokerage is included in the charge codes", adapter.ChargeCodeGroups.Contains(ChargeCodeGroupList.Codes.Brokerage));
				Assert("Origin is included in the charge codes", adapter.ChargeCodeGroups.Contains(ChargeCodeGroupList.Codes.Brokerage));
			});
		}

		public void TestFreightMode()
		{
			var (_, shipment, consignment, item) = GetBasicTestData();
			var adapter = new HVLVItemRatingAdapter(item, consignment, shipment);
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
			var shipment = Factory.New<ForwardingShipment>();

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var item = consignment.Items.AddNew();

			var originConsol = shipment.Consols.AddNew();
			var destinationConsol = shipment.Consols.AddNew();

			var adapter = new HVLVItemRatingAdapter(item, consignment, shipment);

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

		public void TestAccessAdapterProperties_NoDbHitForHVLVConsignmentHVLVItemAndJobShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			for (var i = 0; i < 5; i++)
			{
				var consignment = header.Consignments.AddNew();
				consignment.Items.AddNew();
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);

			var provider = new HVLVRatingAdapterProvider(loadedShipment);
			var adapters = provider.GetAdapters(null, AutoRateOptions.AutorateRevenue).OfType<HVLVItemRatingAdapter>();

			var publicOverrideProperties = typeof(HVLVItemRatingAdapter)
				.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
				.Where(property =>
				{
					var getMethod = property.GetMethod;
					return getMethod != getMethod.GetBaseDefinition();
				});

			var expectedDbHits = new Dictionary<string, int>
			{
				{ HVLVConsignmentSchema.Constants.TableName, 0 },
				{ HVLVItemSchema.Constants.TableName, 0 },
				{ JobShipmentSchema.Constants.TableName, 0 },
			};

			newFactory.ResetDatabaseLoadCount();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				adapters.ForEach(adapter =>
				{
					publicOverrideProperties.ForEach(property => property.GetValue(adapter));
				});
			}
		}

		#region Helpers

		(CommonConsol consol, ForwardingShipment shipment, HVLVConsignment consignment, HVLVItem item) GetBasicTestData()
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

			var header = HVLVConsignmentHeader.GetOrCreate(shipment);
			var consignment = header.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var item = consignment.Items.AddNew();

			Factory.Save();

			return (consol, shipment, consignment, item);
		}

		void AssertPaymentTerm(HVLVItemRatingAdapter adapter, PaymentTermType paymentTermType, CostSell costOrSell, string value)
		{
			var info = adapter.PaymentTerm.GetPaymentTermInfo(costOrSell);

			AssertNotNull(info);
			AssertEquals(paymentTermType, info.InfoType);
			AssertEquals(value, info.Value);
		}

		#endregion
	}
}
