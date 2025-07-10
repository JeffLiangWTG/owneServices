using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalContainerMode = Enterprise.UniversalDataBuss.DataObjects.Universal.ContainerMode;
using UniversalDateType = Enterprise.UniversalDataBuss.DataObjects.Universal.DateType;
using UniversalIncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using UniversalLoadMode = Enterprise.UniversalDataBuss.DataObjects.Universal.LoadMode;
using UniversalOrganizationAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalUnitOfVolume = Enterprise.UniversalDataBuss.DataObjects.Universal.UnitOfVolume;
using UniversalUnitOfWeight = Enterprise.UniversalDataBuss.DataObjects.Universal.UnitOfWeight;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	internal class JobSupplierBookingDataObjectWriter : TopLevelDataObjectWriter<JobSupplierBooking, UniversalShipment>
	{
		public JobSupplierBookingDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.JobSupplierBooking;

		protected override void PopulateDataObject(JobSupplierBooking sourceBO, UniversalShipment dataObject)
		{
			PopulateDataObjectCore(sourceBO, dataObject);
			PopulateDates(sourceBO, dataObject);
			PopulateOrganisations(sourceBO, dataObject);
			PopulateSupplierBookingLines(sourceBO, dataObject);
			PopulateContainers(sourceBO, dataObject);
			PopulateNotes(sourceBO, dataObject);

			CustomLabelsCustomizedFieldDataObjectWriter.Write(JobSupplierBookingSchema.Instance, sourceBO, dataObject, null);
		}

		void PopulateNotes(JobSupplierBooking sourceBO, UniversalShipment dataObject)
		{
			var notes = sourceBO.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);

			dataObject.SetNoteCollection(() => ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial));
		}

		void PopulateContainers(JobSupplierBooking sourceBO, UniversalShipment dataObject)
		{
			if (sourceBO.JSB_LoadMode == Core.Constants.SupplierBookingLoadMode.ContainerYard)
			{
				if (sourceBO.PlannedContainers.Any())
				{
					dataObject.SetContainerCollection(() => ProcessCollection(sourceBO.PlannedContainers, new OrderContainerDataObjectWriter<JobSupplierBookingPlannedContainer>(writeManager), CollectionContent.Complete));
				}

				if (sourceBO.Containers.Any())
				{
					dataObject.SetRelatedShipmentCollection(() => ProcessCollection(sourceBO.Containers, new SupplierBookingAllocatedContainerDataObjectWriter(writeManager), CollectionContent.Complete).ToList());
				}
			}
		}

		protected override IEnumerable<IPropertyValue> GetAllCustomPropertiesWithDefaultValue(JobSupplierBooking sourceBO)
		{
			return sourceBO.GetUserDefinedValues();
		}

		void PopulateSupplierBookingLines(JobSupplierBooking sourceBO, UniversalShipment dataObject)
		{
			var data = ProcessCollection(sourceBO.SupplierBookingLines, new JobSupplierBookingLineDataObjectWriter(writeManager, new LineRelatedDataWriterHelper(sourceBO)));
			dataObject.SetSubShipmentCollection(() => data != null ? new DataObjectList<UniversalShipment>(data) : null);
		}

		static void PopulateDataObjectCore(JobSupplierBooking sourceBO, UniversalShipment dataObject)
		{
			dataObject.ShipmentStatus = ListHelper.GetWithDescription<UniversalCodeDescriptionPair>(sourceBO.JSB_Status, new SupplierBookingStatusList());
			dataObject.PortOfDischarge = ListHelper.GetWithName(sourceBO.JSB_RL_NKDischargePort, sourceBO.Lookups.DischargePorts);
			dataObject.PortOfLoading = ListHelper.GetWithName(sourceBO.JSB_RL_NKLoadPort, sourceBO.Lookups.LoadPorts);
			dataObject.ShipmentIncoTerm = ListHelper.GetWithDescription<UniversalIncoTerm>(sourceBO.JSB_IncoTerm, new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms));
			dataObject.TotalVolume = sourceBO.TotalVolume;
			dataObject.TotalVolumeUnit = ListHelper.GetWithDescription<UniversalUnitOfVolume>(sourceBO.TotalVolumeUnit, new CodeDescriptionPairList(OLookUpEditType.Volume));
			dataObject.TotalWeight = sourceBO.TotalWeight;
			dataObject.TotalWeightUnit = ListHelper.GetWithDescription<UniversalUnitOfWeight>(sourceBO.TotalWeightUnit, new CodeDescriptionPairList(OLookUpEditType.Weight));
			dataObject.TransportMode = ListHelper.GetWithDescription<UniversalCodeDescriptionPair>(sourceBO.JSB_TransportMode, OrdersConstants.GetTransportModeList());
			dataObject.GoodsDescription = sourceBO.JSB_GoodsDescription;
			dataObject.MarksAndNumbers = sourceBO.JSB_MarksAndNumbers;
			dataObject.LoadMode = ListHelper.GetWithDescription<UniversalLoadMode>(sourceBO.JSB_LoadMode, new SupplierBookingLoadModeList());
			dataObject.ContainerMode = ListHelper.GetWithDescription<UniversalContainerMode>(sourceBO.JSB_ContainerMode, OrdersConstants.GetContainerModeList(sourceBO.JSB_TransportMode));
			dataObject.PortOfOrigin = ListHelper.GetWithName(sourceBO.JSB_RL_NKOrigin, sourceBO.Lookups.Origins);
			dataObject.PortOfDestination = ListHelper.GetWithName(sourceBO.JSB_RL_NKDestination, sourceBO.Lookups.Destinations);
		}

		static void PopulateDates(JobSupplierBooking sourceBO, UniversalShipment dataObject)
		{
			dataObject.TryAddDateToCollection(UniversalDateType.BookedOnDate, sourceBO.JSB_BookedOnDate);
			dataObject.TryAddDateToCollection(UniversalDateType.CargoAvailableDate, sourceBO.JSB_CargoAvailableDate);
		}

		void PopulateOrganisations(JobSupplierBooking sourceBO, UniversalShipment dataObject)
		{
			dataObject.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress>());

			JobSupplierBookingDataObjectHelper.TryPopulateOrganisations(writeManager, dataObject, DocAddressType.BookingPartyDocumentaryAddress, sourceBO.BookingParty?.MainAddress);
			JobSupplierBookingDataObjectHelper.TryPopulateOrganisations(writeManager, dataObject, DocAddressType.SupplierDocumentaryAddress, sourceBO.SupplierAddress?.Address);
			JobSupplierBookingDataObjectHelper.TryPopulateOrganisations(writeManager, dataObject, DocAddressType.ControllingCustomer, sourceBO.ControllingCustomerAddress?.Address);
			JobSupplierBookingDataObjectHelper.TryPopulateOrganisations(writeManager, dataObject, DocAddressType.LocalCartageCFS, sourceBO.LocalCartageCFSAddress?.Address);
			JobSupplierBookingDataObjectHelper.TryPopulateOrganisations(writeManager, dataObject, DocAddressType.ConsigneeDocumentaryAddress, sourceBO.ConsigneeDocumentaryAddress?.Address);
			JobSupplierBookingDataObjectHelper.TryPopulateOrganisations(writeManager, dataObject, DocAddressType.ArrivalCFSAddress, sourceBO.CFSAddress);
		}
	}
}
