using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class AgencyShipmentContainerDataObjectReader : ShipmentDataObjectReader<AgencyShipmentContainer>
	{
		public AgencyShipmentContainerDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
			isVerifiedGrossContainerWeight = dataObject.ContainsServiceCode(ServiceCodeType.VGM);
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.AgencyShipmentContainer; }
		}

		AgencyShipment parentShipment;
		readonly bool isVerifiedGrossContainerWeight;

		protected override IMatchingBusinessEntityFinder<AgencyShipmentContainer> GetCombinedReferenceMatcher()
		{
			return new MatchingBusinessEntityFinder<AgencyShipmentContainer>(FindBestMatchingContainer);
		}

		AgencyShipmentContainer FindBestMatchingContainer()
		{
			if (dataObject.ContainerCollection == null || dataObject.ContainerCollection.Count != 1)
			{
				return null;
			}

			AgencyShipmentContainer container = null;

			var references = new AgencyShipmentContainerReferences(dataObject);
			PopulateContainerReferences(references, dataObject.ContainerCollection.FirstOrDefault());

			var matcher = new AgencyShipmentMatcher<AgencyShipment>(factory.BOFactory.GetCachedReadOnlyFactory(), references, logger);
			var bestMatchingAgencyShipment = matcher.GetBestMatch();

			if (bestMatchingAgencyShipment != null)
			{
				parentShipment = bestMatchingAgencyShipment.IsBillOfLadingStage
					? factory.Load<BillOfLading>(bestMatchingAgencyShipment.PK)
					: factory.Load<AgencyBooking>(bestMatchingAgencyShipment.PK);

				var containerMatcher = new AgencyShipmentContainerMatcher(parentShipment, references);

				if (isVerifiedGrossContainerWeight && !parentShipment.IsBillOfLadingStage)
				{
					var realContainers = parentShipment.RealContainers.Cast<AgencyShipmentContainer>();
					var bookedContainers = parentShipment.BookedContainers.Cast<AgencyShipmentContainer>();

					container = containerMatcher.GetBestMatch(realContainers) ?? containerMatcher.GetBestMatch(bookedContainers);
				}
				else
				{
					container = containerMatcher.GetBestMatch(parentShipment.ShippingContainers.Cast<AgencyShipmentContainer>());
				}
			}

			return container;
		}

		void PopulateContainerReferences(AgencyShipmentContainerReferences references, Container container)
		{
			references.ContainerNumber = container.ContainerNumber.GetValueOrDefault();
			references.ContainerISOCode = GetContainerISOCode(container.ContainerType);
			references.ContainerReleaseNumber = container.ReleaseNum.GetValueOrDefault();
		}

		ZString GetContainerISOCode(ContainerType containerType)
		{
			RefContainer refContainer = null;

			if (containerType != null && containerType.Code.HasValue)
			{
				refContainer = factory.LoadFromUniqueKey<RefContainer>(RefContainerSchema.RC_Code, (containerType.Code.Value));
			}

			return refContainer != null ? refContainer.RC_ISOType : ZString.Empty;
		}

		protected override AgencyShipmentContainer GetNewBusinessObject()
		{
			if (parentShipment == null)
			{
				return null;
			}

			if (!isVerifiedGrossContainerWeight || parentShipment.IsBillOfLadingStage)
			{
				return parentShipment.ShippingContainers.AddNew();
			}
			else
			{
				return parentShipment.RealContainers.AddNew();
			}
		}

		protected override AgencyShipmentContainer GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return null;
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(AgencyShipmentContainer targetBO)
		{
			if (dataObject.ContainerCollection == null || dataObject.ContainerCollection.Count == 0)
			{
				return Res.GetString("8e8c3439-3be5-4b20-83b3-968d0a2e8845", "Data object does not contain any containers");
			}

			if (dataObject.ContainerCollection.Count > 1)
			{
				return Res.GetString("c4ac05c5-7ef4-425d-85f5-a6a81212236f", "Data object contains more than one container");
			}

			if (targetBO != null && parentShipment == null) // when matched by job number
			{
				parentShipment = targetBO.Booking;
			}

			if (parentShipment == null)
			{
				return Res.GetString("41cd94ec-c00a-41d4-baf0-aec49ec5b41e", "Parent Booking/Bill of Lading could not be located");
			}

			if (dataObject.ShipmentStatus != null && !dataObject.ShipmentStatus.Code.GetValueOrDefault().IsEmpty)
			{
				string shipmentStatus = dataObject.ShipmentStatus.Code.GetValueOrDefault();

				if (parentShipment.IsBillOfLadingStage && !ShipmentStatusHelperMethods.IsBillOfLadingStage(shipmentStatus))
				{
					return Res.GetString("3b804fca-b57e-4910-aef4-59d9bad79ba0", "Parent has been confirmed but the universal shipment contains Booking");
				}

				if (!parentShipment.IsBillOfLadingStage && !ShipmentStatusHelperMethods.IsBookingStage(shipmentStatus))
				{
					return Res.GetString("5801a0b7-022f-4a4a-9a93-c53abb788d20", "Parent has not yet been confirmed but the universal shipment contains Bill of Lading");
				}
			}

			var hasContainerMode = dataObject.ContainerMode != null && dataObject.ContainerMode.Code.HasValue;

			if (hasContainerMode && parentShipment.JS_PackingMode != dataObject.ContainerMode.Code.Value)
			{
				return Res.GetString("7deccf1b-1717-4369-9d9e-8b8c81727a61",
					"ContainerMode of data object is not compatible with the container mode of Parent Booking/Bill of Lading and would cause existing containers and packing lines to be removed");
			}

			var isTopLevelPack = hasContainerMode
				? AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes.Contains(dataObject.ContainerMode.Code.Value)
				: parentShipment.IsTopLevelPacksMode;

			if (isTopLevelPack && (dataObject.PackingLineCollection == null || dataObject.PackingLineCollection.Count != 1))
			{
				return Res.GetString("0a01e3a0-f43a-4073-b36d-c4e9dff26d01", "Top Level Pack Bookings and Bill of Ladings must contain exactly one Packing Line");
			}

			return ZString.Empty;
		}

		protected override void PopulateBusinessObject(AgencyShipmentContainer targetBO)
		{
			var reader = GetAgencyShipmentDataObjectReader(targetBO);
			BusinessObject parentShipmentBizObj = parentShipment;
			reader.ReadIntoBusinessObject(ref parentShipmentBizObj);
		}

		ITopLevelDataObjectReader GetAgencyShipmentDataObjectReader(AgencyShipmentContainer agencyShipmentContainer)
		{
			var readStrategy = new AgencyShipmentContainerReadStrategy(agencyShipmentContainer, logger, factory, isVerifiedGrossContainerWeight);

			var bookingContainer = agencyShipmentContainer as AgencyBookingContainer;

			if (bookingContainer != null)
			{
				return new AgencyBookingDataObjectReader(dataObject, logger, factory, readStrategy);
			}

			return new BillOfLadingDataObjectReader(dataObject, logger, factory, readStrategy);
		}
	}
}






