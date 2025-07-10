using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	internal class AsycudaManifestHeaderDataObjectWriter : TopLevelDataObjectWriter<AsycudaManifestHeader, Shipment>
	{
		public AsycudaManifestHeaderDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.ZAOutTurn;

		protected override void PopulateDataObject(AsycudaManifestHeader manifestHeaderBO, Shipment uxmlShipmentData)
		{
			if (manifestHeaderBO != null)
			{
				var helper = CreateAsycudaManifestHeaderDataObjectWriterHelper(manifestHeaderBO);

				PopulateDates(manifestHeaderBO, uxmlShipmentData);

				uxmlShipmentData.AddOrgAddress(writeManager, manifestHeaderBO.ShippingAgent, DocAddressType.ControllingAgent);

				PopulateAsycudeManifestHeaderAddInfosData(manifestHeaderBO, uxmlShipmentData);

				if (manifestHeaderBO.AMA_TransportMode == Core.Constants.TransportModes.Sea)
				{
					uxmlShipmentData.SetContainerCollection(() => CreateContainerCollection(manifestHeaderBO.Containers, helper));
					uxmlShipmentData.ContainerMode = helper.GetContainerMode(manifestHeaderBO.AMA_ContainerMode, manifestHeaderBO.Lookups.ContainerModeList);
					uxmlShipmentData.VesselName = manifestHeaderBO.AMA_VesselName;
				}

				uxmlShipmentData.TransportMode = helper.GetCodeDescriptionPair(manifestHeaderBO.AMA_TransportMode, manifestHeaderBO.Lookups.TransportModeList);
				uxmlShipmentData.DeclarantType = helper.GetCodeDescriptionPair(manifestHeaderBO.AMA_AgentType, manifestHeaderBO.Lookups.AgentTypeList);
				uxmlShipmentData.VoyageFlightNo = manifestHeaderBO.AMA_Voyage;
				uxmlShipmentData.AddOrgAddress(writeManager, manifestHeaderBO.Carrier, DocAddressType.Carrier);
				var lookupValue = manifestHeaderBO.Lookups.OutturnProviderList?.Where(x => x.ZZD_Code == manifestHeaderBO.OutturnProvider)?.FirstOrDefault();
				var descriptionValue = lookupValue == null ? string.Empty : lookupValue.ToString();
				uxmlShipmentData.LocationAtClearance = helper.GetCodeDescriptionPair35Char(manifestHeaderBO.OutturnProvider, descriptionValue);
				uxmlShipmentData.CustomsOffice = helper.GetCodeDescriptionPair10Char(manifestHeaderBO.AMA_CustomsOffice, manifestHeaderBO.Lookups.CustomsOffices);
				uxmlShipmentData.ShipmentType = helper.GetCodeDescriptionPair(manifestHeaderBO.AMA_Nature, manifestHeaderBO.Lookups.Natures);
				uxmlShipmentData.PortOfDischarge = ListHelper.GetWithName(manifestHeaderBO.MasterBill?.ABL_RL_NKPortOfDischarge ?? ZString.Empty, manifestHeaderBO.Factory.GetRefUNLOCOList());
				uxmlShipmentData.PortOfLoading = ListHelper.GetWithName(manifestHeaderBO.MasterBill?.ABL_RL_NKPortOfLoading ?? ZString.Empty, manifestHeaderBO.Factory.GetRefUNLOCOList());
				uxmlShipmentData.AddOrgAddress(writeManager, manifestHeaderBO.DeconsolidateAddress, DocAddressType.CustomsContainerYardAddress);
				uxmlShipmentData.AddOrgAddress(writeManager, manifestHeaderBO.DischargeTerminalAddress, DocAddressType.CustomsContainerTerminalOperatorAddress);
				uxmlShipmentData.ExportGoodsType = helper.GetCodeDescriptionPair(manifestHeaderBO.AMA_ManifestType, manifestHeaderBO.Lookups.ManifestTypeList);
				uxmlShipmentData.MessageStatus = helper.GetCodeDescriptionPair(manifestHeaderBO.RegistrationStatus, manifestHeaderBO.Lookups.RegistrationStatusList);
				uxmlShipmentData.EntryStatus = helper.GetEntryStatus(manifestHeaderBO.GateInOutCustomsStatus, manifestHeaderBO.Lookups.CustomsStatusList);
				uxmlShipmentData.QuoteNumber = manifestHeaderBO.BookingNumber;
				uxmlShipmentData.WayBillNumber = manifestHeaderBO.MasterBill?.ABL_BillNumber;

				uxmlShipmentData.SetSubShipmentCollection(() => PopulateSubShipmentCollection(manifestHeaderBO, helper));  // House Bills
			}
		}

		DataObjectList<Container> CreateContainerCollection(ManifestBase.AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> containers, AsycudaManifestHeaderDataObjectWriterHelper helper)
		{
			var writer = CreateNewAsycudaContainerDataObjectWriter(helper);
			var list = new DataObjectList<Container>(containers.OfType<AsycudaContainer>().Select(c => writer.GetDataObject(c)));
			list.Content = CollectionContent.Complete;
			return list;
		}

		protected virtual IAsycudaContainerDataObjectWriter CreateNewAsycudaContainerDataObjectWriter(AsycudaManifestHeaderDataObjectWriterHelper helper) => new AsycudaContainerDataObjectWriter<AsycudaContainer>(writeManager, helper);

		protected virtual AsycudaManifestHeaderDataObjectWriterHelper CreateAsycudaManifestHeaderDataObjectWriterHelper(AsycudaManifestHeader header)
		{
			return new AsycudaManifestHeaderDataObjectWriterHelper(header);
		}

		DataObjectList<Shipment> PopulateSubShipmentCollection(AsycudaManifestHeader headerBO, AsycudaManifestHeaderDataObjectWriterHelper helper)
		{
			DataObjectList<Shipment> result = null;
			if (headerBO != null)
			{
				var bills = headerBO.Bills?.ToArray();
				if (bills != null && bills.Any())
				{
					var writer = new AsycudaBillDataObjectWriter(writeManager, helper);
					var billData = bills.Select(b => writer?.GetDataObject((AsycudaBill)b));
					if (billData != null)
					{
						result = new DataObjectList<Shipment>(billData);
					}
				}
				else
				{
					result = new DataObjectList<Shipment>();
				}
			}
			return result;
		}

		void PopulateAsycudeManifestHeaderAddInfosData(AsycudaManifestHeader manifestHeaderBO, Shipment uxmlShipmentData)
		{
			uxmlShipmentData.SetAddInfoCollection(() =>
			{
				var list = new List<AddInfo>();

				if (manifestHeaderBO != null)
				{
					if (manifestHeaderBO.AMA_ManifestType == ManifestTypeList.Codes.AirExcessOutturnReport || manifestHeaderBO.AMA_ManifestType == ManifestTypeList.Codes.VesselOutturnReport)
					{
						list.AddOrUpdate(AddInfoConstants.AsycudaManifestHeader.ExcessIndicator, manifestHeaderBO.ExcessIndicator);
					}

					if (manifestHeaderBO.AMA_ManifestType == ManifestTypeList.Codes.BulkBreakBulkOutturnReport
						|| manifestHeaderBO.AMA_ManifestType == ManifestTypeList.Codes.VesselOutturnReport
						|| manifestHeaderBO.AMA_ManifestType == ManifestTypeList.Codes.AirCargoOutturnReport
						|| manifestHeaderBO.AMA_ManifestType == ManifestTypeList.Codes.AirExcessOutturnReport
						|| manifestHeaderBO.AMA_ManifestType == ManifestTypeList.Codes.AirLoadDischarge)
					{
						list.AddOrUpdate(AddInfoConstants.AsycudaManifestHeader.FullyLoadedUnloadedDate, manifestHeaderBO.FullyLoadedUnloadedDate.ToString());
					}

					if (manifestHeaderBO.AMA_TransportMode == Core.Constants.TransportModes.Air)
					{
						list.AddOrUpdate(AddInfoConstants.AsycudaManifestHeader.GateInOutDate, manifestHeaderBO.GateInOutDate.ToString());
					}

					list.AddOrUpdate(AddInfoConstants.AsycudaManifestHeader.ParentBill, manifestHeaderBO.ParentBill);
					list.AddOrUpdate(AddInfoConstants.AsycudaManifestHeader.GateInOutMessageType, manifestHeaderBO.GateInOutMessageType);
					list.AddOrUpdate(AddInfoConstants.AsycudaManifestHeader.UnpackedDate, manifestHeaderBO.UnpackedDate.ToString());
				}

				return list;
			});
		}

		void PopulateDates(AsycudaManifestHeader manifestHeaderBO, Shipment uxmlShipmentData)
		{
			uxmlShipmentData.SetDateCollection(() =>
			{
				var list = new List<Date>();
				if (manifestHeaderBO != null)
				{
					list.Add(DateType.BillIssued, ZBool.False, manifestHeaderBO.MasterBill.ABL_BillIssueDate);
					list.Add(DateType.Departure, ZBool.False, manifestHeaderBO.MasterBill.ABL_E_DEP);
					list.Add(DateType.Arrival, ZBool.False, manifestHeaderBO.MasterBill.ABL_E_ARV);
					list.Add(DateType.Unpack, ZBool.False, manifestHeaderBO.UnpackedDate);

					if (uxmlShipmentData?.TransportMode?.Code?.ToString() == Core.Constants.TransportModes.Sea
						&& manifestHeaderBO.AMA_ManifestType == ManifestTypeList.Codes.BulkBreakBulkOutturnReport)
					{
						list.Add(DateType.LoadingDate, ZBool.False, manifestHeaderBO.FullyLoadedUnloadedDate);
					}
					else if (uxmlShipmentData?.TransportMode?.Code?.ToString() == Core.Constants.TransportModes.Sea
							&& manifestHeaderBO.AMA_ManifestType == ManifestTypeList.Codes.VesselOutturnReport
							&& manifestHeaderBO.AMA_ContainerMode != Core.Constants.ContainerModeDescriptions.Containerised.ToString())
					{
						list.Add(DateType.LoadingDate, ZBool.False, manifestHeaderBO.FullyLoadedUnloadedDate);
					}
					else if (uxmlShipmentData?.TransportMode?.Code?.ToString() == Core.Constants.TransportModes.Air)
					{
						list.Add(DateType.LoadingDate, ZBool.False, manifestHeaderBO.FullyLoadedUnloadedDate);
						list.Add(DateType.WarehouseRelease, ZBool.False, manifestHeaderBO.GateInOutDate);
					}
				}

				return list;
			});
		}
	}
}
