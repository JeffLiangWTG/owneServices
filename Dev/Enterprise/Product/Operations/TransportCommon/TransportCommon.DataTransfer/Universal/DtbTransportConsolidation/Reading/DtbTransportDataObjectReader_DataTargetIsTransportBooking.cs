using System;
using CargoWise.EntityFramework;
using Enterprise.TransportCommon.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportCommon.DataTransfer.Universal
{
	/// <summary>
	/// This is only used when the uXML has a data target of Transport Booking.
	/// Because we always require a Consolidation and it's our entry point, create the Consolidation Reader and pass in the targeted Booking.
	/// </summary>
	public abstract class DtbTransportDataObjectReader_DataTargetIsTransportBooking<TConsolidation, TTransport> : ShipmentDataObjectReader<TTransport>, ITopLevelDataObjectReader
		where TConsolidation : DtbTransportConsolidation
		where TTransport : DtbTransport
	{
		protected DtbTransportDataObjectReader_DataTargetIsTransportBooking(Shipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(shipment, logger, factory)
		{
		}

		#region GetCombinedReferenceMatcher

		protected override IMatchingBusinessEntityFinder<TTransport> GetCombinedReferenceMatcher()
		{
			return null;
		}

		#endregion

		#region GetExistingBusinessObjectUsingModuleSpecificBusinessRules

		protected override TTransport GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return null;
		}

		#endregion

		#region IsImportJobCostingAllowed

		protected override bool IsImportJobCostingAllowed(TTransport targetBO)
		{
			return false;
		}

		#endregion

		#region PopulateBusinessObject

		protected sealed override void PopulateBusinessObject(TTransport targetBO)
		{
			throw new InvalidOperationException("This reader should never be used to populate a business object, it should only be used to redirect to the consolidation reader.");
		}

		protected abstract DtbTransportConsolidationDataObjectReader<TConsolidation> GetNewConsolidationReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, TTransport targetBO);

		#endregion

		#region ITopLevelDataObjectReader

		BusinessObject ITopLevelDataObjectReader.ReadIntoTopLevelBusinessObject()
		{
			var booking = GetBusinessObjectFromContextKey() ?? GetNewBusinessObject();

			var reader = GetNewConsolidationReader(dataObject, logger, factory, booking);
			reader.ReadIntoBusinessObject();

			return booking;
		}

		#endregion
	}
}
