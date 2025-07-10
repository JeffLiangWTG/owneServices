using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public sealed class DtbBookingConsolidationMultiJobDataObjectReader : ShipmentDataObjectReader<DtbBookingConsolidation>
	{
		public DtbBookingConsolidationMultiJobDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransportBookingConsolidation; }
		}

		protected override DtbBookingConsolidation GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return null;
		}

		protected override IMatchingBusinessEntityFinder<DtbBookingConsolidation> GetCombinedReferenceMatcher()
		{
			return null;
		}

		protected override void PopulateBusinessObject(DtbBookingConsolidation consolidation)
		{
			if (consolidation.KB_JobType != TransportConsolidationJobTypes.Codes.BookingTransportConsolidation && consolidation.Bookings.Count > 0)
			{
				throw new DataObjectReadFailureException(DtbBookingConsolidationSchema.Constants.KB_JobType + " cannot be changed from " + consolidation.KB_JobType + " if the Consolidation already has Bookings.");
			}
			SetValue(consolidation, DtbBookingConsolidationSchema.KB_JobType, TransportConsolidationJobTypes.Codes.BookingTransportConsolidation);

			PopulateRelatedEntities(consolidation);
		}

		void PopulateRelatedEntities(DtbBookingConsolidation consolidation)
		{
			PopulateAddresses(consolidation);
			PopulateChildConsolidations(consolidation);
		}

		void PopulateAddresses(DtbBookingConsolidation consolidation)
		{
			var organisationCollection = dataObject.OrganizationAddressCollection;
			if (organisationCollection != null)
			{
				foreach (var orgAddressDataObject in organisationCollection)
				{
					var jobDocAddress = new OrganisationDataObjectReader(orgAddressDataObject, logger, factory).GetMatchedOrNew(consolidation);
					if (jobDocAddress != null)
					{
						consolidation.DocAddresses.Add(jobDocAddress);
					}
				}
			}
		}

		void PopulateChildConsolidations(DtbBookingConsolidation consolidation)
		{
			if (dataObject.SubShipmentCollection != null)
			{
				foreach (var consolidationShipment in dataObject.SubShipmentCollection)
				{
					var singleJobConsolidation = new DtbBookingConsolidationDataObjectReader(consolidationShipment, logger, factory).ReadIntoBusinessObject();
					consolidation.Bookings.AddRange(singleJobConsolidation.Bookings);
				}
			}
		}

		protected override bool IsImportConsolCostsAllowed(DtbBookingConsolidation targetBO)
		{
			return true;
		}
	}
}
