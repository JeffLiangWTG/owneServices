using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Container = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using ContainerPenaltyDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.ContainerPenalty;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	public class ContainerDataObjectReaderTest : TestCaseWithFactory
	{
		public void TestPopulateContainerCommodities()
		{
			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject.Commodity = new Commodity() { Code = "SHIP", Description = "Shipping ships for shipment" };
			containerDataObject.RatingCommodity = new Commodity() { Code = "COAL", Description = "Santa's gift" };

			var reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), (c) => null);
			var containerBO = reader.ReadIntoBusinessObject();

			AssertEquals("SHIP", containerBO.JC_RH_NKContainerCommodityCode);
			AssertEquals("COAL", containerBO.JC_RH_NKRatingCommodityCode);
		}

		public void TestPopulateContainerPenalties()
		{
			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			var importPenalty = SetupDataObject();
			importPenalty.ProcessType = new CodeDescriptionPair { Code = "IMP" };
			importPenalty.TotalCost = 111;
			var exportPenalty = SetupDataObject();
			exportPenalty.ProcessType = new CodeDescriptionPair { Code = "EXP" };
			exportPenalty.TotalCost = 222;
			var deliveryPenalty = SetupDataObject();
			deliveryPenalty.ProcessType = new CodeDescriptionPair { Code = "DLV" };
			deliveryPenalty.TotalCost = 333;
			var pickupPenalty = SetupDataObject();
			pickupPenalty.ProcessType = new CodeDescriptionPair { Code = "PIC" };
			pickupPenalty.TotalCost = 444;

			containerDataObject.SetContainerPenaltyCollection(() => new List<ContainerPenaltyDataObject> { importPenalty, exportPenalty, deliveryPenalty, pickupPenalty });
			containerDataObject.ArrivalTruckWaitCost = 0m;
			containerDataObject.DepartureTruckWaitCost = 0m;

			var reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), (c) => null);
			var containerBO = reader.ReadIntoBusinessObject();
			AssertEquals(1, containerBO.ImportPenalties.Count);
			AssertEquals(1, containerBO.ExportPenalties.Count);
			AssertEquals(1, containerBO.DeliveryPenalties.Count);
			AssertEquals(1, containerBO.PickupPenalties.Count);
			AssertEquals(true, containerBO.ImportPenalties.Any(x => x.CPY_TotalCost == 111));
			AssertEquals(true, containerBO.ExportPenalties.Any(x => x.CPY_TotalCost == 222));
			AssertEquals(true, containerBO.DeliveryPenalties.Any(x => x.CPY_TotalCost == 333));
			AssertEquals(true, containerBO.PickupPenalties.Any(x => x.CPY_TotalCost == 444));
		}

		public void TestRoundingRoundDownDecimalToItsPrecisionAndScaleOnDataImport()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight);
			var registryEntry = new DefaultNumberOfDecimals();
			registryEntry.UnitOfMeasure = "LB";
			registryEntry.TransportMode = "SEA";
			registryEntry.NumberOfDecimals = 1;
			registryEntry.RoundingMode = Registry.Business.RoundingModes.Up;
			collection.Add(registryEntry);

			var registryEntryWeight = new DefaultNumberOfDecimals();
			registryEntryWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			registryEntryWeight.TransportMode = Core.Constants.TransportModes.Air;
			registryEntryWeight.NumberOfDecimals = 1;
			registryEntryWeight.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryWeight);

			var registryEntryVolume = new DefaultNumberOfDecimals();
			registryEntryVolume.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			registryEntryVolume.TransportMode = Core.Constants.TransportModes.Air;
			registryEntryVolume.NumberOfDecimals = 1;
			registryEntryVolume.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryVolume);

			var registryEntryDimension = new DefaultNumberOfDecimals();
			registryEntryDimension.UnitOfMeasure = Core.Constants.Dimension.Metres;
			registryEntryDimension.TransportMode = Core.Constants.TransportModes.Air;
			registryEntryDimension.NumberOfDecimals = 1;
			registryEntryDimension.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryDimension);

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var containerCollection = new DataObjectList<Container>();
			containerCollection.Content = CollectionContent.Complete;

			var containerDataObject = ContainerDataObjectTestHelper.SetupContainer();
			containerDataObject.TareWeight = 999999.999m;
			containerDataObject.DunnageWeight = 0m;
			containerDataObject.WeightCapacity = 999999.999m;
			containerDataObject.WeightUnit = new UnitOfWeight { Code = Core.Constants.Weight.Kilograms };
			containerDataObject.VolumeUnit = new UnitOfVolume { Code = Core.Constants.Volume.CubicMetres };
			containerDataObject.LengthUnit = new UnitOfLength { Code = Core.Constants.Dimension.Metres };
			containerCollection.Add(containerDataObject);

			var reader = new ConsolContainerCollectionReader<CommonContainer, CommonConsol>(containerCollection, logger, new UniversalObjectFactory(), consol, new ContainerLinkManager<CommonConsol>(null));

			using (FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				reader.ReadIntoCollection();
			}

			AssertEquals(1, consol.Containers.Count);

			var containerBO = consol.Containers[0];
			AssertEquals((ZDecimal)999999.9, containerBO.JC_TareWeight);
			AssertEquals((ZDecimal)0, containerBO.JC_DunnageWeight);
			AssertEquals("It calculates gross weight by: TareWeight + DunnageWeight", (ZDecimal)999999.9, containerBO.JC_GrossWeight);
			AssertEquals((ZDecimal)999999.9, containerBO.JC_WeightCapacity);
		}

		public void TestBasicContainerLevelFieldMappings()
		{
			var containerDataObject = ContainerDataObjectTestHelper.SetupContainer();
			containerDataObject.IsGrossWeightOverridden = true;
			containerDataObject.PivotBreak = 1101m;
			var reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), (c) => null);
			var containerBO = reader.ReadIntoBusinessObject();

			AssertNotNull(containerBO);

			#region Check Contents of Business Object

			CombineAssertions(delegate
			{
				ContainerDataObjectTestHelper.AssertContents(containerBO);
				AssertEquals(true, containerBO.JC_IsGrossWeightOverridden);
				AssertEquals(1101m, containerBO.JC_PivotBreak);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Matching 'ContainerYardEmptyPickupAddress':- Matched to 'XVBQP68SIYXQ' by code, address 'DEP123' (only address).
Information - Matching 'ContainerYardEmptyReturnAddress':- Matched to 'H5ZX52PAMCOI' by code, address 'ARV456' (only address).
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'FOO':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestNonOperatingReefer()
		{
			var containerDataObject = ContainerDataObjectTestHelper.SetupContainer();
			containerDataObject.NonOperatingReefer = true;
			var reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), (c) => null);
			var containerBO = reader.ReadIntoBusinessObject();

			AssertEquals(containerBO.JC_IsNonOperativeReefer, true);

			containerDataObject.NonOperatingReefer = false;
			containerDataObject.ContainerType = new ContainerType
			{
				Category = new ContainerTypeCategory
				{
					Code = Core.Constants.ContainerTypes.DryStorage
				},
				ISOCode = "22R1"
			};
			reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), (c) => null);
			containerBO = reader.ReadIntoBusinessObject();
			AssertEquals(containerBO.JC_IsNonOperativeReefer, true);

			containerDataObject.NonOperatingReefer = null;
			containerDataObject.ContainerType = new ContainerType
			{
				Category = new ContainerTypeCategory
				{
					Code = Core.Constants.ContainerTypes.DryStorage
				},
				ISOCode = "22R1"
			};
			reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), (c) => null);
			containerBO = reader.ReadIntoBusinessObject();
			AssertEquals(containerBO.JC_IsNonOperativeReefer, true);

			containerDataObject.NonOperatingReefer = null;
			containerDataObject.ContainerType = new ContainerType
			{
				Category = new ContainerTypeCategory
				{
					Code = Core.Constants.ContainerTypes.DryStorage
				},
				ISOCode = "22C1"
			};
			reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), (c) => null);
			containerBO = reader.ReadIntoBusinessObject();
			AssertEquals(containerBO.JC_IsNonOperativeReefer, false);
		}

		public void TestRenameDemurrageAndStorage()
		{
			var containerDataObject = ContainerDataObjectTestHelper.SetupContainer();

			containerDataObject.DepartureTruckWaitTime = new ZDateTime(2019, 11, 21);
			containerDataObject.DepartureTruckWaitCost = 87.33m;
			containerDataObject.ArrivalCTOStorageStartDate = new ZDateTime(2019, 11, 20);
			containerDataObject.ArrivalCTOStorageDays = new ZByte(9);
			containerDataObject.ArrivalCTOStorageCost = 100.02m;
			containerDataObject.ArrivalTruckWaitTime = new ZDateTime(2019, 11, 19);
			containerDataObject.ArrivalTruckWaitCost = 53.47m;
			containerDataObject.ArrivalCarrierDetentionDays = new ZByte(10);
			containerDataObject.ArrivalCarrierDetentionCost = 90.23m;

			var reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), (c) => null);
			var containerBO = reader.ReadIntoBusinessObject();

			AssertNotNull(containerBO);

			#region Check Contents of Business Object

			CombineAssertions(delegate
			{
				AssertEquals("containerBO.DepartureTruckWaitTime", new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 11, 21), containerBO.DepartureTruckWaitTime);
				AssertEquals("containerBO.DepartureTruckWaitCost", 87.33m, containerBO.DepartureTruckWaitCost);
				AssertEquals("containerBO.JC_ArrivalCTOStorageStartDate", new ZDateTime(2019, 11, 20), containerBO.JC_ArrivalCTOStorageStartDate);
				AssertEquals("containerBO.ArrivalCTOStorageDays", new ZByte(9), containerBO.ArrivalCTOStorageDays);
				AssertEquals("containerBO.ArrivalCTOStorageCost", 100.02m, containerBO.ArrivalCTOStorageCost);
				AssertEquals("containerBO.ArrivalTruckWaitTime", new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 11, 19), containerBO.ArrivalTruckWaitTime);
				AssertEquals("containerBO.ArrivalTruckWaitCost", 53.47m, containerBO.ArrivalTruckWaitCost);
				AssertEquals("containerBO.ArrivalCarrierDetentionDays", new ZByte(10), containerBO.ArrivalCarrierDetentionDays);
				AssertEquals("containerBO.ArrivalCarrierDetentionCost", 90.23m, containerBO.ArrivalCarrierDetentionCost);

				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Matching 'ContainerYardEmptyPickupAddress':- Matched to 'XVBQP68SIYXQ' by code, address 'DEP123' (only address).
Information - Matching 'ContainerYardEmptyReturnAddress':- Matched to 'H5ZX52PAMCOI' by code, address 'ARV456' (only address).
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'FOO':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});

			#endregion
		}

		#region Container Penalty

		public void TestContainerPenalty()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG1";

			var address = Factory.New<OrgAddress>();
			address.OA_OH = org.PK;
			address.OA_Code = "TESTADDR1";
			address.OA_Address1 = "1804 Fudrucker Way";

			Factory.Save();

			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);

			var penaltyDataObject = SetupDataObject();
			containerDataObject.SetContainerPenaltyCollection(() => new List<ContainerPenaltyDataObject>
			{
				penaltyDataObject,
				new ContainerPenaltyDataObject
				{
					PenaltyType = new CodeDescriptionPair { Code = Core.Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime },
					CreditorType = new CodeDescriptionPair { Code = Core.Constants.ContainerPenaltyCreditorType.Codes.Transport },
					Creditor = penaltyDataObject.Creditor,
					Location = new UNLOCO { Code = "AUSYD" },
					FreeTime = new ZDateTime(2019, 1, 1),
					Duration = new ZDateTime(2019, 11, 21),
					TimeUnit = UniversalDataBuss.DataObjects.Universal.TimeUnit.Days,
					PerUnitCost = 87.33m,
					ProcessType = new CodeDescriptionPair { Code = "IMP" },
					Currency = new Currency { Code = "AUD" }
				}
			});

			var reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), (c) => null);
			var containerBO = reader.ReadIntoBusinessObject();

			AssertNotNull(containerBO);

			#region Check Contents of Container Penalties

			CombineAssertions(delegate
			{
				AssertEquals(2, containerBO.ImportPenalties.Count);

				var penalty = containerBO.ImportPenalties.FirstOrDefault(p =>
					p.CPY_PenaltyType == Core.Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime
						&& p.CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.Transport);
				AssertNotNull(penalty);
				AssertEquals(new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 11, 21), penalty.CPY_Duration);
				AssertEquals(87.33m, penalty.CPY_PerUnitCost);

				penalty = containerBO.ImportPenalties.FirstOrDefault(p =>
					p.CPY_PenaltyType == Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage
						&& p.CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.CTO);
				AssertNotNull(penalty);
				AssertEquals("AUSYD", penalty.CPY_RL_NKLocation);
				AssertEquals(ZDateTime.Empty, penalty.CPY_FreeTime);
				AssertEquals(org.PK, penalty.CPY_OH_Creditor);
				AssertEquals((ZDateTime)TimeSpan.FromDays(9), penalty.CPY_Duration);
				AssertEquals(Core.Constants.ContainerPenaltyTimeUnit.Codes.Days, penalty.CPY_TimeUnit);
				AssertEquals(3.2m, penalty.CPY_PerUnitCost);
				AssertEquals(500m, penalty.CPY_TotalCost);
				AssertEquals("AUD", penalty.CPY_RX_NKCurrency);

				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - No matching ContainerPenalty found, creating new ContainerPenalty.
Information - Populating ContainerPenalty...
Information - Matching 'Creditor':- Matched to 'TESTORG1' by code, address '1804 Fudrucker Way' (only address).
Information - No matching ContainerPenalty found, creating new ContainerPenalty.
Information - Populating ContainerPenalty...
Information - Matching 'Creditor':- Matched to 'TESTORG1' by code, address '1804 Fudrucker Way' (only address).
".Trim(), logger.Logs);
			});

			#endregion
		}

		ContainerPenaltyDataObject SetupDataObject()
		{
			var creditor = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.Creditor),
				OrganizationCode = "TESTORG1",
				Address1 = "1804 Fudrucker Way"
			};

			return new ContainerPenaltyDataObject
			{
				PenaltyType = new CodeDescriptionPair { Code = Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage },
				CreditorType = new CodeDescriptionPair { Code = Core.Constants.ContainerPenaltyCreditorType.Codes.CTO },
				Creditor = creditor,
				Location = new UNLOCO { Code = "AUSYD" },
				FreeTime = new ZDateTime(2020, 1, 1),
				Duration = new ZDateTime(2020, 1, 10),
				TimeUnit = UniversalDataBuss.DataObjects.Universal.TimeUnit.Days,
				PerUnitCost = 3.2m,
				TotalCost = 500m,
				ProcessType = new CodeDescriptionPair { Code = "IMP" },
				Currency = new Currency { Code = "AUD" }
			};
		}

		#endregion

		public void TestReaderWithProviderContainerCreator()
		{
			var containerDataObject = ContainerDataObjectTestHelper.SetupContainer();
			var reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(),
				c => null, c => Factory.New<CommonContainer>());
			var containerBO = reader.ReadIntoBusinessObject();

			AssertNotNull(containerBO);

			#region Check Contents of Business Object

			CombineAssertions(delegate
			{
				ContainerDataObjectTestHelper.AssertContents(containerBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Matching 'ContainerYardEmptyPickupAddress':- Matched to 'XVBQP68SIYXQ' by code, address 'DEP123' (only address).
Information - Matching 'ContainerYardEmptyReturnAddress':- Matched to 'H5ZX52PAMCOI' by code, address 'ARV456' (only address).
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'FOO':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestStandAloneCustomsContainer()
		{
			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);

			var reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(),
				c => null, c => Factory.New<CommonContainer>());
			var containerBO = reader.ReadIntoBusinessObject();

			AssertEquals("containerBO.StandAloneCustomsContainer", false, containerBO.StandAloneCustomsContainer);

			containerDataObject.GrossWeight = 40.01m;

			reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(),
				c => null, c => Factory.New<CommonContainer>());
			containerBO = reader.ReadIntoBusinessObject();

			AssertEquals("containerBO.StandAloneCustomsContainer", true, containerBO.StandAloneCustomsContainer);
		}

		public void TestInactiveContainerType()
		{
			var ref23JP = Factory.NewWithValidTestData<RefContainer>();
			ref23JP.RC_Code = "23JP";
			ref23JP.RC_ISOType = "21G5";

			Factory.Save();

			var containerDataObject = ContainerDataObjectTestHelper.SetupContainer();
			containerDataObject.ContainerType.Code = "23JP";

			var reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), (c) => null);
			var containerBO = reader.ReadIntoBusinessObject();

			AssertNotNull(containerBO);

			#region Check Contents of Business Object

			AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching Container Type.
Information - Matching 'ContainerYardEmptyPickupAddress':- Matched to 'XVBQP68SIYXQ' by code, address 'DEP123' (only address).
Information - Matching 'ContainerYardEmptyReturnAddress':- Matched to 'H5ZX52PAMCOI' by code, address 'ARV456' (only address).
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'FOO':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			AssertEquals("containerBO.RefContainer.RC_Code", "23JP", containerBO.RefContainer.RC_Code);
			AssertEquals("containerBO.RefContainer.RC_Description", ZString.Empty, containerBO.RefContainer.RC_DescriptionMultilingual);
			AssertEquals("containerBO.RefContainer.RC_ISOType", "21G5", containerBO.RefContainer.RC_ISOType);

			#endregion

			logger.ClearLogs();
			ref23JP.RC_IsActive = false;

			Factory.Save();

			reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), (c) => null);
			containerBO = reader.ReadIntoBusinessObject();

			AssertNotNull(containerBO);

			#region Check Contents of Business Object

			AssertMultilineASCIIEquals("logger.Logs", $@"
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Warning - Container Type '{ref23JP.RC_Code}' is inactive so it will not be used. Please go to the consol and choose a container type.
Information - Matching 'ContainerYardEmptyPickupAddress':- Matched to 'XVBQP68SIYXQ' by code, address 'DEP123' (only address).
Information - Matching 'ContainerYardEmptyReturnAddress':- Matched to 'H5ZX52PAMCOI' by code, address 'ARV456' (only address).
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'FOO':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			AssertNull("containerBO.RefContainer", containerBO.RefContainer);

			#endregion
		}

		public void TestContainerAdditionalAddressInfo()
		{
			var containerBO = Factory.New<CommonContainer>();

			containerBO.JC_OA_DepartureContainerYardAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			containerBO.JC_OA_ArrivalContainerYardAddress = Factory.NewWithValidTestData<OrgAddress>().PK;

			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);

			containerDataObject.SetAdditionalAddressInfoCollection(() => new List<AdditionalAddressInfo>
			{
				new AdditionalAddressInfo
				{
					AddressType = nameof(DocAddressType.DepartureCYDAddress),
					TransportMode = new CodeDescriptionPair { Code = "ROA", Description = "Road Freight" }
				},
				new AdditionalAddressInfo
				{
					AddressType = nameof(DocAddressType.ArrivalCYDAddress),
					TransportMode = new CodeDescriptionPair { Code = "IWT", Description = "Inland Waterways" }
				}
			});

			var reader = new ContainerDataObjectReader<CommonContainer>(
				containerDataObject,
				logger,
				new UniversalObjectFactory(),
				containerBizObjProvider: c => containerBO,
				containerBizObjCreator: c => containerBO
			);

			containerBO = reader.ReadIntoBusinessObject();
			AssertEquals("ROA", containerBO.EmptyPickupByTransportMode);
			AssertEquals("IWT", containerBO.EmptyReturnToTransportMode);
		}

		public void TestAdditionalServicesComplete()
		{
			var containerBO = PrepareAndReadAdditionalServices("HDMU7865432", addCollectionContent: true, CollectionContent.Complete);

			AssertEquals(2, containerBO.Services.Count);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Fumigation, containerBO.Services[0].ES_ServiceCode);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Washing, containerBO.Services[1].ES_ServiceCode);
		}

		public void TestAdditionalServicesPartial()
		{
			var containerBO = PrepareAndReadAdditionalServices("HLSU7865432", addCollectionContent: true, CollectionContent.Partial);

			AssertEquals(3, containerBO.Services.Count);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Fumigation, containerBO.Services[0].ES_ServiceCode);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Cleaning, containerBO.Services[1].ES_ServiceCode);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Washing, containerBO.Services[2].ES_ServiceCode);
		}

		public void TestAdditionalServicesWithoutContentType_ShouldWorkLikePartial()
		{
			var containerBO = PrepareAndReadAdditionalServices("HBTU7654389", addCollectionContent: false);

			AssertEquals(3, containerBO.Services.Count);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Fumigation, containerBO.Services[0].ES_ServiceCode);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Cleaning, containerBO.Services[1].ES_ServiceCode);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Washing, containerBO.Services[2].ES_ServiceCode);
		}

		CommonContainer PrepareAndReadAdditionalServices(ZString containerNum, bool addCollectionContent, CollectionContent collectionContent = CollectionContent.Complete)
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = containerNum;
			container.JC_SealNum = "SEAL";

			var fumService = container.Services.AddNew();
			fumService.ShouldPopulateServiceId = false;
			fumService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;

			var clnService = container.Services.AddNew();
			clnService.ShouldPopulateServiceId = false;
			clnService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Cleaning;

			Factory.Save();

			var containerDataObject = ContainerDataObjectTestHelper.SetupContainer();
			containerDataObject.ContainerNumber = container.JC_ContainerNum;

			var fumAdditionalService = new AdditionalService
			{
				ServiceCode = new CodeDescriptionPair { Code = Core.Constants.FreightServiceType.Codes.Fumigation, Description = Core.Constants.FreightServiceType.Descriptions.Fumigation },
				Booked = new ZDateTime(2011, 6, 1),
				Completed = new ZDateTime(2011, 6, 2),
				Contractor = null,
			};

			var wshAdditionalService = new AdditionalService
			{
				ServiceCode = new CodeDescriptionPair { Code = Core.Constants.FreightServiceType.Codes.Washing, Description = Core.Constants.FreightServiceType.Descriptions.Washing },
				Booked = new ZDateTime(2011, 6, 1),
				Completed = new ZDateTime(2011, 6, 2),
				Contractor = null,
			};

			var additionalServicesDataObject = new DataObjectList<AdditionalService>();
			additionalServicesDataObject.Add(fumAdditionalService);
			additionalServicesDataObject.Add(wshAdditionalService);

			if (addCollectionContent)
			{
				additionalServicesDataObject.Content = collectionContent;
			}

			containerDataObject.SetAdditionalServiceCollection(() => additionalServicesDataObject);

			var reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), c => null, c => container);
			var containerBO = reader.ReadIntoBusinessObject();
			return containerBO;
		}

		#region Check Correct Weights Get Imported

		public void TestAgencyBookingFromUniversalShipment_TareWeightSetsFromXML()
		{
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_Code = "12AB";
			refContainer.RC_TareWeight = 100m;

			Factory.Save();

			#region Case when tare weight > 0

			var containerDataObject = ContainerDataObjectTestHelper.SetupContainer();
			containerDataObject.ContainerType.Code = "12AB";
			containerDataObject.TareWeight = 500m;
			containerDataObject.ContainerCount = 2;
			containerDataObject.WeightUnit.Code = "KG";

			var reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), (c) => null);
			var containerBO = reader.ReadIntoBusinessObject();

			AssertEquals("containerBO.JC_TareWeight from xml", 500m, containerBO.JC_TareWeight);

			#endregion

			#region Case when tare weight is zero

			containerDataObject.TareWeight = 0m;

			reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), (c) => null);
			containerBO = reader.ReadIntoBusinessObject();

			AssertEquals("containerBO.JC_TareWeight from xml = 0", 0m, containerBO.JC_TareWeight);

			#endregion

			#region Case when tare weight is not set in xml

			containerDataObject.TareWeight = null;

			reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), (c) => null);
			containerBO = reader.ReadIntoBusinessObject();

			AssertEquals("containerBO.JC_TareWeight from container default", 200m, containerBO.JC_TareWeight);

			#endregion
		}

		public void TestAgencyBookingFromUniversalShipment_AllWeightsInCorrectUnit()
		{
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_Code = "12AB";
			refContainer.RC_TareWeight = 1000m;

			Factory.Save();

			#region Setting Tare Weight from container default converts into containers weight unit

			var containerDataObject = ContainerDataObjectTestHelper.SetupContainer();

			containerDataObject.ContainerType.Code = "12AB";
			containerDataObject.WeightUnit.Code = "T";
			containerDataObject.GrossWeight = 2m;
			containerDataObject.TareWeight = null;

			var reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), (c) => null);
			var containerBO = reader.ReadIntoBusinessObject();

			AssertEquals("containerBO.JC_WeightUnit", "T", containerBO.JC_GrossWeightUQ);
			AssertEquals("containerBO.JC_TareWeight from container's default weight", 2m, containerBO.JC_TareWeight);
			AssertEquals("containerBO.JC_GrossWeight", 2m, containerBO.JC_GrossWeight);

			#endregion

			#region Setting Tare weight from xml stays in xml weight unit

			containerDataObject.TareWeight = 1.1m;

			reader = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), (c) => null);
			containerBO = reader.ReadIntoBusinessObject();

			AssertEquals("containerBO.JC_WeightUnit", "T", containerBO.JC_GrossWeightUQ);
			AssertEquals("containerBO.JC_TareWeight from xml", 1.1m, containerBO.JC_TareWeight);
			AssertEquals("containerBO.JC_GrossWeight", 2m, containerBO.JC_GrossWeight);

			#endregion
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
		}

		TestErrorLogger logger;

		#endregion
	}
}
