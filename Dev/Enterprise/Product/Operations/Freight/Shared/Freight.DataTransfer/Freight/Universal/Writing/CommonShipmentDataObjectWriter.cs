using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalIncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public abstract class CommonShipmentDataObjectWriter<T> : TopLevelDataObjectWriter<T, UniversalShipment> where T : CommonShipment
	{
		protected CommonShipmentDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected BindToLists ListCache { get; private set; }

		#region Implementation

		protected override sealed void PopulateDataObject(T shipmentBizObj, UniversalShipment dataObject)
		{
			ListCache = BindToLists.GetCachedLists(shipmentBizObj.Factory);

			PopulateShipment(shipmentBizObj, dataObject);

			WriteAddresses(shipmentBizObj, dataObject);
			WriteDates(shipmentBizObj, dataObject);
			WriteNotes(shipmentBizObj, dataObject);
			WriteEntryNumbers(shipmentBizObj, dataObject);
			WriteAdditionalReferences(shipmentBizObj, dataObject);
			WriteContainers(shipmentBizObj, dataObject);
			WritePackLines(shipmentBizObj, dataObject);
			WriteTransportLegs(shipmentBizObj, dataObject);
		}

		protected virtual void PopulateShipment(T shipmentBizObj, UniversalShipment dataObject)
		{
			dataObject.WayBillNumber = shipmentBizObj.JS_HouseBill;
			PopulateWayBillType(shipmentBizObj, dataObject);

			dataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBizObj.JS_TransportMode, shipmentBizObj.Lookups.JS_TransportMode_List);
			dataObject.PortOfOrigin = ListHelper.GetWithName(shipmentBizObj.JS_RL_NKOrigin, shipmentBizObj.Lookups.RefUNLOCO_List);
			dataObject.PortOfDestination = ListHelper.GetWithName(shipmentBizObj.JS_RL_NKDestination, shipmentBizObj.Lookups.RefUNLOCO_List);
			dataObject.ShipmentIncoTerm = ListHelper.GetWithDescription<UniversalIncoTerm>(shipmentBizObj.JS_INCO, shipmentBizObj.Lookups.JS_INCO_List);
			dataObject.ContainerMode = ListHelper.GetWithDescription<ContainerMode>(shipmentBizObj.JS_PackingMode, shipmentBizObj.Lookups.JS_PackingMode_List);

			dataObject.GoodsDescription = shipmentBizObj.JS_GoodsDescription;
			dataObject.ReleaseType = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBizObj.JS_ReleaseType, shipmentBizObj.Lookups.JS_ReleaseType_List);
			dataObject.HBLAWBChargesDisplay = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBizObj.JS_HBLAWBChargesDisplay, shipmentBizObj.Lookups.JS_HBLAWBChargesDisplay_List);
			dataObject.ShippedOnBoard = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBizObj.JS_ShippedOnBoard, shipmentBizObj.Lookups.JS_ShippedOnBoard_List);

			dataObject.NoCopyBills = shipmentBizObj.JS_NoCopyBills;
			dataObject.NoOriginalBills = shipmentBizObj.JS_NoOriginalBills;

			dataObject.TotalVolume = shipmentBizObj.JS_ActualVolume;
			dataObject.TotalVolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(shipmentBizObj.JS_UnitOfVolume, ListCache.VolumeUnits);
			dataObject.TotalWeight = shipmentBizObj.JS_ActualWeight;
			dataObject.TotalWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(shipmentBizObj.JS_UnitOfWeight, ListCache.WeightUnits);

			dataObject.OuterPacks = shipmentBizObj.JS_OuterPacks;
			dataObject.OuterPacksPackageType = ListHelper.GetWithDescription<PackageType>(shipmentBizObj.JS_F3_NKPackType, shipmentBizObj.Lookups.JS_PackType_List);

			dataObject.InterimReceiptNumber = shipmentBizObj.JS_InterimReceipt;

			dataObject.BookingConfirmationReference = shipmentBizObj.JS_BookingReference;
			dataObject.CFSReference = shipmentBizObj.JS_CFSReference;

			dataObject.ServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(shipmentBizObj.JS_RS_NKServiceLevel, shipmentBizObj.Lookups.RefServiceLevel_List);
		}

		protected virtual void PopulateWayBillType(T shipmentBizObj, UniversalShipment dataObject)
		{
		}

		protected virtual void WriteAddresses(T shipmentBizObj, UniversalShipment dataObject)
		{
			dataObject.SetOrganizationAddressCollection(() => ProcessCollection(shipmentBizObj.DocAddresses, new JobDocAddressDataObjectWriter(writeManager)));

			var jobHeader = shipmentBizObj.ShipmentJobHeader;
			if (jobHeader != null)
			{
				dataObject.AddOrgAddress(writeManager, jobHeader.LocalChargesAddr, AddressTypes.SendersLocalClient);
				dataObject.AddOrgAddress(writeManager, jobHeader.AgentCollectAddr, AddressTypes.SendersOverseasAgent);
			}
		}

		protected virtual void WriteDates(T shipmentBizObj, UniversalShipment dataObject)
		{
			dataObject.SetDateCollection(() => new List<Date>
			{
				Date.New(DateType.BookingConfirmed, ZBool.False, shipmentBizObj.JS_A_BKD),
				Date.New(DateType.Received, ZBool.False, shipmentBizObj.JS_A_RCV),
				Date.New(DateType.Departure, ZBool.True, shipmentBizObj.JS_E_DEP),
				Date.New(DateType.Arrival, ZBool.True, shipmentBizObj.JS_E_ARV)
			});
		}

		protected virtual void WriteNotes(T shipmentBizObj, UniversalShipment dataObject)
		{
			var notes = shipmentBizObj.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
			dataObject.SetNoteCollection(() => ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial));
		}

		protected virtual void WriteEntryNumbers(T shipmentBizObj, UniversalShipment dataObject)
		{
			dataObject.SetEntryNumberCollection(() => ProcessCollection(shipmentBizObj.CusEntryNumbers, new EntryNumberDataObjectWriter(writeManager)));
		}

		protected virtual void WriteAdditionalReferences(T shipmentBizObj, UniversalShipment dataObject)
		{
			dataObject.SetAdditionalReferenceCollection(() => ProcessCollection(shipmentBizObj.Numbers, new AdditionalReferenceDataObjectWriter(writeManager), CollectionContent.Complete) ?? new DataObjectList<AdditionalReference>());
		}

		protected virtual void WriteContainers(T shipmentBizObj, UniversalShipment dataObject)
		{
			dataObject.SetContainerCollection(() => ProcessCollection(shipmentBizObj.Containers, new ContainerDataObjectWriter(ListCache, writeManager), CollectionContent.Complete, true) ??
				new DataObjectList<Container>() { Content = CollectionContent.Complete });
		}

		protected virtual void WritePackLines(T shipmentBizObj, UniversalShipment dataObject)
		{
			dataObject.SetPackingLineCollection(() => ProcessCollection(shipmentBizObj.OuterPackLines, new PackingLineDataObjectWriter<PackLine>(ListCache, writeManager), CollectionContent.Complete) ?? new DataObjectList<PackingLine>() { Content = CollectionContent.Complete });
		}

		protected virtual void WriteTransportLegs(T shipmentBizObj, UniversalShipment dataObject)
		{
			dataObject.SetTransportLegCollection(() => ProcessCollection(shipmentBizObj.Transports, new TransportLegDataObjectWriter(writeManager, shipmentBizObj), CollectionContent.Complete, true)
				?? new DataObjectList<TransportLeg>() { Content = CollectionContent.Complete });
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(T shipment)
		{
			return shipment.GetUserDefinedValues();
		}

		#endregion
	}
}
