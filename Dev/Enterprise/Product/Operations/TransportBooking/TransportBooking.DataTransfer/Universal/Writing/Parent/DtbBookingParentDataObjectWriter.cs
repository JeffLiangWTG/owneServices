using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Options;
using Enterprise.TransportBookings.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using EZC = Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public class DtbBookingParentDataObjectWriter : TopLevelDataObjectWriter<BusinessObject, UniversalShipment>
	{
		public DtbBookingParentDataObjectWriter(TransportBookingDocumentOptions options, UniversalShipment topLevelDataObject, IDataWritingManager outboundSessionTracker)
			: base(outboundSessionTracker)
		{
			Options = options;
			TopLevelDataObject = topLevelDataObject;
		}

		readonly TransportBookingDocumentOptions Options;
		readonly UniversalShipment TopLevelDataObject;

		protected override void PopulateDataObject(BusinessObject parentBO, UniversalShipment bookingDataObject)
		{
			// we only want the parent DO and merge in our additional info ... see InsertParents
		}

		protected override void InsertParents(BusinessObject sourceBO, ref UniversalShipment dataObject)
		{
			base.InsertParents(sourceBO, ref dataObject);

			dataObject = TopLevelDataObject;
			PopulateTransportBookingInfo(TopLevelDataObject, sourceBO);
			if (Globals.IsUserInteractive)
			{
				dataObject.Branch = Branch.New(GlbBranch.CurrentBranch);
			}
		}

		void PopulateTransportBookingInfo(UniversalShipment topLevelDO, BusinessObject sourceBO)
		{
			AddBookingConsolidationDataTarget(topLevelDO, sourceBO);

			var sourceDO = UniversalShipment.GetSourceDataObject(topLevelDO);

			var containersToDeliver = Options.ContainersToDeliver;
			if (!containersToDeliver.Any())
			{
				PopulateLooseBookingInfo(sourceDO, sourceBO);
			}
			else
			{
				PopulateContainerisedBookingInfo(sourceDO, sourceBO, containersToDeliver);
			}
		}

		void AddBookingConsolidationDataTarget(UniversalShipment topLevelDO, BusinessObject sourceBO)
		{
			var existingConsolidation = DtbBookingConsolidation.FindExistingTransportBookingConsolidation(new BusinessObjectFactory(), (IDtbBookingParent)sourceBO, Options.Direction);
			if (existingConsolidation != null)
			{
				topLevelDO.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, existingConsolidation.KB_JobID);
			}
		}

		void PopulateLooseBookingInfo(UniversalShipment sourceDO, BusinessObject sourceBO)
		{
			sourceDO.TransportBookingDirection = new TransportBookingDirection() { Code = Options.Direction.ToString(), Description = DtbBookingDirectionDescription.GetDescription(Options.Direction) };
			sourceDO.LocalTransportJobType = ListHelper.GetWithDescription<CodeDescriptionPair4Char>(Options.Template, new BindToLists(sourceBO.Factory).BookingTemplates);
		}

		void PopulateContainerisedBookingInfo(UniversalShipment sourceDO, BusinessObject sourceBO, IEnumerable<DtbDocumentContainerOption> containersToDeliver)
		{
			// currenty reads from top universal shipment
			sourceDO.TransportBookingDirection = new TransportBookingDirection() { Code = Options.Direction.ToString(), Description = DtbBookingDirectionDescription.GetDescription(Options.Direction) };

			if (sourceDO.SubShipmentCollection == null)
			{
				sourceDO.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			}

			UniversalShipment booking = null;

			foreach (var container in containersToDeliver)
			{
				if (booking == null || !Options.CombineContainers)
				{
					booking = new UniversalShipment(writeManager.WriterStrategy);
					booking.ShipmentType = new CodeDescriptionPair() { Code = "TBK", Description = (EZC.NoResString)"Transport Booking" };
					booking.TransportBookingDirection = new TransportBookingDirection() { Code = Enterprise.TransportCommon.Shared.Directions.GetDirectionCodeFromDtbBookingDirection(Options.Direction), Description = DtbBookingDirectionDescription.GetDescription(Options.Direction) };
					booking.LocalTransportJobType = ListHelper.GetWithDescription<CodeDescriptionPair4Char>(Options.Template, new BindToLists(sourceBO.Factory).BookingTemplates);
					booking.SetContainerCollection(() => new DataObjectList<Container>());

					sourceDO.SubShipmentCollection?.Add(booking);
				}

				if (booking.ContainerCollection != null)
				{
					booking.ContainerCollection.Add(new Container(writeManager.WriterStrategy)
					{
						ContainerNumber = container.ContainerNumber,
						ContainerType = ListHelper.GetWithDescription<ContainerType>(container.ContainerType, new BindToLists(sourceBO.Factory).ConfirmationTypes),
						Seal = container.Seal,
						Link = container.Link,
						ReleaseNum = container.ReleaseNumber
					});
				}
			}
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.TransportBookingConsolidation; // don't think this is used anymore Options.ParentDataContext; // sometimes consol shipment?
		}
	}
}
