using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class CYContainerLoadListDataObjectReader : ShipmentDataObjectReader<CYContainerLoadList>
	{
		readonly ContainerLoadListContainerLinkManager containerLinkManager;

		public CYContainerLoadListDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
			this.containerLinkManager = new ContainerLoadListContainerLinkManager();
		}

		public override DataContextType DataContextType => DataContextType.ContainerLoadList;

		protected override CYContainerLoadList GetExistingBusinessObjectUsingModuleSpecificBusinessRules() => null;

		protected override IMatchingBusinessEntityFinder<CYContainerLoadList> GetCombinedReferenceMatcher() => null;

		protected override void PopulateBusinessObject(CYContainerLoadList targetBO)
		{
			var supplierBookingKey = GetSupplierBookingKey();
			var supplierBooking = factory.BOFactory.LoadFromNaturalKey<JobSupplierBooking>(JobSupplierBookingSchema.JSB_BookingId, supplierBookingKey)
				?? throw new DataObjectReadFailureException(Res.GetString("6218d7f9-0f19-42d0-971e-311cba380a91", "The supplier booking {0} does not exist.", supplierBookingKey));

			if (supplierBooking.PK != targetBO.CLH_JSB_Booking)
			{
				throw new DataObjectReadFailureException(Res.GetString("8dca9d01-453f-44bc-8ab4-35797359d79f", "Could not modify the supplier booking."));
			}

			SetValue(targetBO, ContainerLoadListHeaderSchema.CLH_GoodsDescription, dataObject.GoodsDescription);
			SetValue(targetBO, ContainerLoadListHeaderSchema.CLH_MarksAndNumbers, dataObject.MarksAndNumbers);
			SetValue(targetBO, ContainerLoadListHeaderSchema.CLH_Status, dataObject.ShipmentStatus);
			SetValue(targetBO, ContainerLoadListHeaderSchema.CLH_JSB_Booking, supplierBooking.PK);
			SetValue(targetBO, ContainerLoadListHeaderSchema.CLH_PlannedTransportMode, dataObject.TransportMode);
			SetValue(targetBO, ContainerLoadListHeaderSchema.CLH_RL_NKPlannedLoadPort, dataObject.PortOfLoading);
			SetValue(targetBO, ContainerLoadListHeaderSchema.CLH_RL_NKPlannedDischargePort, dataObject.PortOfDischarge);
			targetBO.CLH_OH_LoadListParty = JobSupplierBookingDataObjectHelper.FindByDocAddressTypeAndGetMatched(dataObject, DocAddressType.BookingPartyDocumentaryAddress, logger, factory)?.OA_OH
				?? ZGuid.Empty;

			PopulateContainerLinkMapping(targetBO);
			PopulateContainerLoadListLines(targetBO);
			PopulateNotes(targetBO);
		}

		#region Populate

		void PopulateContainerLoadListLines(CYContainerLoadList targetBO)
		{
			if (dataObject.SubShipmentCollection?.Count == 0)
			{
				logger.Log(LogType.Warning, Res.GetString("6c3fa89b-20d1-4558-b3f2-9f51b7d340ea", "There are no container load list lines."));
				return;
			}

			if (dataObject.SubShipmentCollection is IList<UniversalShipment> containerLoadListLines && containerLoadListLines.Count > 0)
			{
				new ContainerLoadListLineDataObjectCollectionReader(logger, factory, targetBO, this.containerLinkManager, new DataObjectList<UniversalShipment>(containerLoadListLines.OrderBy(line => line.PackingLineCollection?.FirstOrDefault()?.Link ?? ZInt.Zero))).ReadIntoCollection();
			}
		}

		void PopulateContainerLinkMapping(CYContainerLoadList targetBO)
		{
			if (dataObject.ParentShipmentCollection?.Count == 0)
			{
				logger.Log(LogType.Warning, Res.GetString("6aa450c0-e13c-45c2-9155-20d9ee6bf2a0", "There are no containers."));
			}

			if (dataObject.ParentShipmentCollection is IList<UniversalShipment> consolDataObjects && consolDataObjects.Count > 0)
			{
				foreach (var consolDataObject in consolDataObjects)
				{
					var consolKey = consolDataObject.GetMatchingDataSource(DataContextType.ForwardingConsol)?.Key.GetValueOrDefault() ?? ZString.Empty;
					if (consolKey.IsEmpty)
					{
						logger.Log(LogType.Error, Res.GetString("b1129134-4ef1-43f3-a280-364a85e9cd24", "There is no consol key."));
						continue;
					}

					var consol = factory.BOFactory.LoadFromNaturalKey<ForwardingConsol>(JobConsolSchema.JK_UniqueConsignRef, consolKey);
					if (consol == null)
					{
						logger.Log(LogType.Error, Res.GetString("7d5a92d2-a1af-4bed-b3dd-9563d4502fc5", "The consol {0} does not exist.", consolKey));
						continue;
					}

					foreach (var containerDataObject in consolDataObject.ContainerCollection.OrderByDescending(db => db.ContainerCount))
					{
						new ContainerLoadListContainerDataObjectReader(containerDataObject, consol, containerLinkManager, logger, factory).ReadIntoBusinessObject();
					}
				}
			}
		}

		void PopulateNotes(CYContainerLoadList targetBO)
		{
			if (dataObject.NoteCollection != null)
			{
				var noteTypesToSkip = new List<ZString> { };

				var noteTypesWithEmptyText = dataObject.NoteCollection
					.Where(n => n.NoteText.GetValueOrDefault().Trim().IsEmpty)
					.Select(n => n.Description.GetValueOrDefault()).ToArray();
				if (noteTypesWithEmptyText.Any())
				{
					var message = Res.GetString("acfdbaf8-3959-4868-987e-a03ce1137467",
						"Cannot import the following notes with empty text:{0}{1}",
						System.Environment.NewLine,
						ZString.Join(",", noteTypesWithEmptyText));
					logger.LogBoth(LogType.Warning, message);

					noteTypesToSkip.AddRange(noteTypesWithEmptyText);
				}

				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, targetBO, noteTypesToSkip).ReadIntoCollection();
			}

			if (targetBO.CLH_DetailedGoodsDescription.IsEmpty)
			{
				targetBO.CLH_DetailedGoodsDescription = ZString.Empty;
			}
		}

		#endregion

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CYContainerLoadList targetBO)
		{
			if ((dataObject.LoadMode?.Code ?? ZString.Empty) == ZString.Empty)
			{
				return Res.GetString("f2a58337-a0cc-4414-b9f8-1434d6a6a269", "There is no load mode.");
			}

			if ((dataObject.LoadMode?.Code ?? ZString.Empty) != ContainerLoadListHeaderLoadMode.ContainerYard)
			{
				return Res.GetString("cdfd7869-61e9-4cb7-b583-752fcc25e72d", "Load mode only supports container yard.");
			}

			if ((dataObject.ShipmentStatus?.Code ?? ZString.Empty) == ZString.Empty)
			{
				return Res.GetString("b4027e4d-221f-4653-b579-052545f81ce5", "Status should not be null or empty.");
			}

			if (new ZString[] {
					ContainerLoadListHeaderStatus.Shipped ,
					ContainerLoadListHeaderStatus.Converted ,
					ContainerLoadListHeaderStatus.Cancelled }.Contains(dataObject.ShipmentStatus.Code ?? ZString.Empty))
			{
				return Res.GetString("9293ad56-e8b9-49b2-95c6-31a566cf471b", "Status should not be Canceled, Shipped or Converted.");
			}

			if (GetSupplierBookingKey().IsEmpty)
			{
				return Res.GetString("d37fc1b1-6268-439e-96cd-df0ee2fc5f93", "There is no supplier booking key.");
			}

			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
		}

		ZString GetSupplierBookingKey()
		{
			return dataObject.RelatedShipmentCollection?.FirstOrDefault()?.GetMatchingDataSource(DataContextType.JobSupplierBooking)?.Key.GetValueOrDefault() ?? ZString.Empty;
		}

		class ContainerLoadListLineDataObjectCollectionReader : DataObjectCollectionReader<UniversalShipment, ContainerLoadListLine>
		{
			public ContainerLoadListLineDataObjectCollectionReader(
				IXmlImportLogger logger,
				UniversalObjectFactory factory,
				CYContainerLoadList containerLoadList,
				ContainerLoadListContainerLinkManager containerLinkManager,
				DataObjectList<UniversalShipment> containerLoadListLineDataObjects)
				: base(containerLoadListLineDataObjects)
			{
				this.logger = logger;
				this.factory = factory;
				this.containerLoadList = containerLoadList;
				this.containerLinkManager = containerLinkManager;
			}

			readonly ContainerLoadListContainerLinkManager containerLinkManager;
			readonly CYContainerLoadList containerLoadList;
			readonly IXmlImportLogger logger;
			readonly UniversalObjectFactory factory;

			protected override ContainerLoadListLine[] BusinessObjects => containerLoadList.LoadListLines.ToArray();

			protected override void AddToCollection(ContainerLoadListLine businessObject)
			{
				containerLoadList.LoadListLines.Add(businessObject);
			}

			protected override ContainerLoadListLine FindMatchingBusinessObject(UniversalShipment dataObject) => null;

			protected override ContainerLoadListLine ReadIntoBusinessObject(UniversalShipment dataObject, ContainerLoadListLine businessObject)
			{
				return new ContainerLoadListLineDataObjectReader(dataObject, containerLoadList, logger, factory, containerLinkManager).ReadIntoBusinessObject();
			}

			protected override void RemoveFromCollection(ContainerLoadListLine businessObject)
			{
				containerLoadList.LoadListLines.Delete(businessObject);
			}
		}
	}
}
