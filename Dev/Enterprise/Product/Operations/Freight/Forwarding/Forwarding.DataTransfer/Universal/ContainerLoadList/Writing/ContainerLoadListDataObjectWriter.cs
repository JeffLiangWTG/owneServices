using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalLoadMode = Enterprise.UniversalDataBuss.DataObjects.Universal.LoadMode;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class ContainerLoadListDataObjectWriter : TopLevelDataObjectWriter<CommonContainerLoadList, UniversalShipment>
	{
		readonly ContainerLoadListContainerLinkManager containerLinkManager;

		public ContainerLoadListDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
			this.containerLinkManager = new ContainerLoadListContainerLinkManager();
		}

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.ContainerLoadList;

		protected override void PopulateDataObject(CommonContainerLoadList sourceBO, UniversalShipment dataObject)
		{
			dataObject.GoodsDescription = sourceBO.CLH_GoodsDescription;
			dataObject.MarksAndNumbers = sourceBO.CLH_MarksAndNumbers;
			dataObject.TransportMode = ListHelper.GetWithDescription<UniversalCodeDescriptionPair>(sourceBO.CLH_PlannedTransportMode, OrdersConstants.GetTransportModeList());
			dataObject.PortOfLoading = ListHelper.GetWithName(sourceBO.CLH_RL_NKPlannedLoadPort, sourceBO.Lookups.PlannedLoadPorts);
			dataObject.PortOfDischarge = ListHelper.GetWithName(sourceBO.CLH_RL_NKPlannedDischargePort, sourceBO.Lookups.PlannedDischargePorts);
			dataObject.ShipmentStatus = ListHelper.GetWithDescription<UniversalCodeDescriptionPair>(sourceBO.CLH_Status, new CommonContainerLoadListStatusList());
			dataObject.LoadMode = ListHelper.GetWithDescription<UniversalLoadMode>(sourceBO.CLH_LoadMode, new SupplierBookingLoadModeList());

			var supplierBookingDataObject = new UniversalShipment(writeManager.WriterStrategy);
			supplierBookingDataObject.DataContext = DataContextFactory.New();
			supplierBookingDataObject.DataContext.AddDataSource(DataContextType.JobSupplierBooking, sourceBO.Booking?.JSB_BookingId ?? ZString.Empty);
			dataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment> { supplierBookingDataObject });

			dataObject.AddOrgAddress(writeManager, sourceBO.LoadListParty, DocAddressType.BookingPartyDocumentaryAddress);
			PopulateContainers(sourceBO, dataObject);
			PopulateContainerLoadListLines(sourceBO, dataObject);
			PopulateNotes(sourceBO, dataObject);
		}

		void PopulateContainers(CommonContainerLoadList sourceBO, UniversalShipment dataObject)
		{
			var containers = sourceBO.Booking.Containers;

			var shipments = new List<UniversalShipment>();
			foreach (var containersOfConsol in containers.GroupBy(container => (container as ForwardingContainer).JC_JK))
			{
				var result = new UniversalShipment(writeManager.WriterStrategy);
				result.DataContext = DataContextFactory.New();
				result.DataContext.AddDataSource(DataContextType.ForwardingConsol, (containersOfConsol.First() as ForwardingContainer).Consol.JK_UniqueConsignRef);
				result.SetContainerCollection(() => ProcessCollection(containersOfConsol, new ContainerLoadListContainerDataObjectWriter(writeManager, containerLinkManager), CollectionContent.Complete));
				shipments.Add(result);
			}

			dataObject.SetParentShipmentCollection(() => shipments);
		}

		void PopulateContainerLoadListLines(CommonContainerLoadList sourceBO, UniversalShipment dataObject)
		{
			var data = ProcessCollection(sourceBO.LoadListLines, new ContainerLoadListLineDataObjectWriter(writeManager, GetContainerLoadListLineAndShipmentIDMapping(sourceBO), containerLinkManager));
			dataObject.SetSubShipmentCollection(() => data != null ? new DataObjectList<UniversalShipment>(data) : null);
		}

		Dictionary<ZGuid, string> GetContainerLoadListLineAndShipmentIDMapping(CommonContainerLoadList sourceBO)
		{
			var collection = new DynamicBusinessObjectCollection(sourceBO.Factory);
			var query = FormattableString.Invariant($@"
				SELECT {ContainerLoadListLineSchema.Constants.PK}, {JobShipmentSchema.Constants.JS_UniqueConsignRef}
				FROM {ContainerLoadListLineSchema.Constants.SqlSchemaName}.{ContainerLoadListLineSchema.Constants.TableName}
				INNER JOIN {JobPackLinesSchema.Constants.SqlSchemaName}.{JobPackLinesSchema.Constants.TableName}
				ON {ContainerLoadListLineSchema.Constants.CLL_JL_PackLine} = {JobPackLinesSchema.Constants.PK}
				INNER JOIN {JobShipmentSchema.Constants.SqlSchemaName}.{JobShipmentSchema.Constants.TableName}
				ON {JobPackLinesSchema.Constants.JL_JS} = {JobShipmentSchema.Constants.PK}
				WHERE {ContainerLoadListLineSchema.Constants.CLL_CLH_LoadListHeader} = '{sourceBO.PK}'
			");
			collection.Load(query);
			return collection.Select(x => new KeyValuePair<ZGuid, string>((ZGuid)x[ContainerLoadListLineSchema.Constants.PK], Convert.ToString(x[JobShipmentSchema.Constants.JS_UniqueConsignRef])))
				.ToDictionary(x => x.Key, x => x.Value);
		}

		void PopulateNotes(CommonContainerLoadList sourceBO, UniversalShipment dataObject)
		{
			var notes = sourceBO.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);

			dataObject.SetNoteCollection(() => ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial));
		}
	}
}
