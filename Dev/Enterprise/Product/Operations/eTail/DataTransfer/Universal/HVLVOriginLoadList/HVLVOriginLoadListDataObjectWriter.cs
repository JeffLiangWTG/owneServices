using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;
using IncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLVOriginLoadListDataObjectWriter : TopLevelDataObjectWriter<HVLVOriginLoadList, UniversalShipment>
	{
		public HVLVOriginLoadListDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		public UniversalShipment ExportLoadListAsConsolUniversalShipmentForMatchOrCreateOnly(HVLVOriginLoadList loadListBO)
		{
			var dataObject = GetDataObject(loadListBO);

			dataObject.WayBillNumber = loadListBO.HVL_MasterBillNumber;
			dataObject.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master };
			dataObject.PortOfLoading = new UNLOCO() { Code = loadListBO.HVL_RL_NKOrigin };
			dataObject.PortOfDischarge = new UNLOCO() { Code = loadListBO.HVL_RL_NKDestination };
			dataObject.ContainerMode = new ContainerMode() { Code = HVLVOriginLoadListHelper.GetConsolContainerMode(loadListBO) };
			dataObject.ShipmentType = new CodeDescriptionPair { Code = AgentType.Agent };
			if (loadListBO.Carrier != null)
			{
				dataObject.OrganizationAddressCollection.Add(new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ShippingLineAddress)).GetDataObject(loadListBO.Carrier?.MainAddress));
			}

			PopulateWayBillNumberIfLoadListIsAirAndNeutralMaster(loadListBO, dataObject);

			return dataObject;
		}

		protected override void PopulateDataObject(HVLVOriginLoadList loadListBO, UniversalShipment dataObject)
		{
			dataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(loadListBO.HVL_TransportMode, loadListBO.Lookups.HVL_TransportMode_List);
			dataObject.VesselName = loadListBO.HVL_VesselName;
			dataObject.VoyageFlightNo = loadListBO.HVL_VoyageFlight;
			dataObject.IsMasterHouse = loadListBO.HVL_IsMasterHouse;
			dataObject.OperationalStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(loadListBO.HVL_Status, loadListBO.Lookups.HVL_Status_List);
			dataObject.ServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(loadListBO.HVL_RS_NKServiceLevel, loadListBO.Lookups.ServiceLevels);
			dataObject.ShipmentIncoTerm = GetIncoTerm(loadListBO);
			dataObject.PortOfOrigin = ListHelper.GetWithName(loadListBO.HVL_RL_NKOrigin, loadListBO.Lookups.Origins);
			dataObject.PortOfDestination = ListHelper.GetWithName(loadListBO.HVL_RL_NKDestination, loadListBO.Lookups.Destinations);

			PopulateContainer(loadListBO, dataObject);
			PopulateDates(loadListBO, dataObject);
			PopulateOrganizationAddresses(loadListBO, dataObject);
			PopulateBills(loadListBO, dataObject);
			PopulatePackingLines(loadListBO, dataObject);
			PopulateTransportLegs(loadListBO, dataObject);
		}

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.HVLVOriginLoadList;

		void PopulateContainer(HVLVOriginLoadList loadListBO, UniversalShipment dataObject)
		{
			if (!loadListBO.HVL_ContainerNumber.IsEmpty && loadListBO.ContainerType != null)
			{
				dataObject.SetContainerCollection(() =>
				{
					var container = new Container();
					container.ContainerNumber = loadListBO.HVL_ContainerNumber;
					container.ContainerType = ContainerType.New(loadListBO.ContainerType);
					container.Link = 1;

					var containerMode = HVLVOriginLoadListHelper.GetConsolContainerMode(loadListBO);
					if (!string.IsNullOrEmpty(containerMode))
					{
						container.FCL_LCL_AIR = new ContainerMode() { Code = containerMode };
					}

					if (loadListBO.HVL_TransportMode != TransportModes.Air)
					{
						container.DeliveryMode = DeliveryModes.Codes.CFS_CFS;
					}

					return new DataObjectList<Container>() { container };
				});
			}
		}

		void PopulateDates(HVLVOriginLoadList loadListBO, UniversalShipment dataObject)
		{
			dataObject.SetDateCollection(() =>
			{
				var dep = new Date() { Type = DateType.Departure, Value = loadListBO.HVL_E_Dep, IsEstimate = ZBool.True };
				var arr = new Date() { Type = DateType.Arrival, Value = loadListBO.HVL_E_Arv, IsEstimate = ZBool.True };
				return new List<Date>() { dep, arr };
			});
		}

		protected virtual void PopulateOrganizationAddresses(HVLVOriginLoadList loadListBO, UniversalShipment dataObject)
		{
			dataObject.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>() { };

				if (loadListBO.Carrier != null)
				{
					var carrier = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.Carrier)).GetDataObject(loadListBO.Carrier?.MainAddress);
					addresses.Add(carrier);
				}

				if (loadListBO.OriginDepot != null)
				{
					var origin = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.DepartureCFSAddress)).GetDataObject(loadListBO.OriginDepot);
					addresses.Add(origin);
				}

				if (loadListBO.DestinationDepot != null)
				{
					var destination = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ArrivalCFSAddress)).GetDataObject(loadListBO.DestinationDepot);
					addresses.Add(destination);
				}

				return addresses;
			});
		}

		void PopulateBills(HVLVOriginLoadList originLoadListBO, UniversalShipment dataObject)
		{
			dataObject.SetAdditionalBillCollection(() =>
			{
				var masterBill = new AdditionalBill
				{
					BillNumber = originLoadListBO.HVL_MasterBillNumber,
					BillType = new WayBillType() { Code = BillTypeList.Codes.MasterBill, Description = BillTypeList.Descriptions.MasterBill }
				};
				var houseBill = new AdditionalBill
				{
					BillNumber = originLoadListBO.HVL_HouseBillNumber,
					BillType = new WayBillType() { Code = BillTypeList.Codes.HouseBill, Description = BillTypeList.Descriptions.HouseBill }
				};
				return new List<AdditionalBill>() { masterBill, houseBill };
			});
		}

		protected virtual void PopulatePackingLines(HVLVOriginLoadList originLoadListBO, UniversalShipment dataObject)
		{
			dataObject.SetPackingLineCollection(() =>
			{
				var packingLines = new DataObjectList<PackingLine>();
				if (originLoadListBO.OuterPackages.Any())
				{
					packingLines.AddRange(ProcessCollection(originLoadListBO.OuterPackages, new HVLVOuterPackageWriter(writeManager, dataObject), CollectionContent.Complete));
				}

				var looseItems = originLoadListBO.ActiveItems.Where(i => i.HVI_HVO_OuterPackage.IsEmpty);
				if (looseItems.Any())
				{
					packingLines.AddRange(ProcessCollection(looseItems, new HVLVItemDataObjectWriter(writeManager, dataObject), CollectionContent.Complete));
				}

				return packingLines;
			});
		}

		void PopulateTransportLegs(HVLVOriginLoadList loadListBO, UniversalShipment dataObject)
		{
			dataObject.SetTransportLegCollection(() =>
			{
				var transportLegs = new DataObjectList<TransportLeg>();

				var transportLeg = new TransportLeg
				{
					VoyageFlightNo = loadListBO.HVL_VoyageFlight,
					PortOfLoading = ListHelper.GetWithName(loadListBO.HVL_RL_NKOrigin, loadListBO.Lookups.Origins),
					PortOfDischarge = ListHelper.GetWithName(loadListBO.HVL_RL_NKDestination, loadListBO.Lookups.Destinations),
					EstimatedDeparture = loadListBO.HVL_E_Dep,
					EstimatedArrival = loadListBO.HVL_E_Arv,
					IsCargoOnly = FreightDataRegistry.Instance.CargoOnlyVoyageDefault.Value,
					VesselName = loadListBO.HVL_VesselName,
					TransportMode = new TransportModeConverter().ToEnumValue(loadListBO.HVL_TransportMode)
				};

				if (loadListBO.Carrier != null)
				{
					var carrier = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.Carrier)).GetDataObject(loadListBO.Carrier?.MainAddress);
					transportLeg.Carrier = carrier;
				}

				transportLegs.Add(transportLeg);

				return transportLegs;
			});
		}

		protected virtual IncoTerm GetIncoTerm(HVLVOriginLoadList loadListBO)
		{
			return new IncoTerm() { Code = loadListBO.HVL_INCO };
		}

		void PopulateWayBillNumberIfLoadListIsAirAndNeutralMaster(HVLVOriginLoadList loadListBO, UniversalShipment dataObject)
		{
			if (loadListBO.HVL_IsNeutralMaster && loadListBO.HVL_TransportMode == TransportModes.Air)
			{
				dataObject.IsNeutralMaster = new IsNeutralMaster() { Value = true, CreateAndAllocateNeutralStock = true };

				if (loadListBO.HVL_MasterBillNumber.IsEmpty && !loadListBO.HVL_VoyageFlight.IsEmpty)
				{
					var code = loadListBO.HVL_VoyageFlight.SubstringSafe(0, 2);

					if (!code.IsEmpty)
					{
						var airline = RefAirline.LoadFromAirline2LetterCode(loadListBO.Factory, code);

						if (airline != null)
						{
							dataObject.WayBillNumber = airline.RM_EagleAddedAirlinePrefixOrAccountingCode;
						}
					}
				}
			}
		}
	}
}
