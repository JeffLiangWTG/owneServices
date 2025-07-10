using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public class DtbBookingConsolidationDataContextManager : ShipmentDataContextManager<DtbBookingConsolidation>
	{
		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			// add the received by

			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new DtbBookingConsolidationEventParentFinder(factory, this, logger);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new DtbBookingConsolidationDataObjectWriter(writeManager, true);
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			ITopLevelDataObjectReader reader;
			var shipmentType = universalShipment.ShipmentType;

			if (shipmentType != null && universalShipment.ShipmentType.GetCodeAsUpperCase() == TransportConsolidationJobTypes.Codes.BookingTransportConsolidation)
			{
				reader = new DtbBookingConsolidationMultiJobDataObjectReader(universalShipment, logger, factory);
			}
			else
			{
				reader = new DtbBookingConsolidationDataObjectReader(universalShipment, logger, factory);
			}
			return reader;
		}

		/// <summary>
		/// CTG - Cartage Agent is used by Cartage Advices
		/// TPC - Transport Co is used by modules to create TB's. It can also be used by TB but can only send to external systems.
		/// BKP - If registry TestCbaId (test system only) is set then treat this as per TPC. This is for testing only currently
		/// </summary>
		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			var result = recipientRoles.Any(rr => rr.Code == RecipientRoleType.CTG);
			if (!result)
			{
				var isTransportCo = recipientRoles.Any(rr => rr.Code == RecipientRoleType.TPC || (rr.Code == RecipientRoleType.BKP && !string.IsNullOrEmpty(TransportRegistry.Instance.TestCbaId?.Value)));
				if (isTransportCo)
				{
					var isPublishedExternally = importSessionLogger == null || importSessionLogger.OutboundSessionTracker == null;
					var isSourceTB = dataSources != null && dataSources.Any(ds =>
							ds.Type.GetValueOrDefault().EqualsIgnoringCase(nameof(DataContextType.TransportBookingConsolidation)) ||
							ds.Type.GetValueOrDefault().EqualsIgnoringCase(nameof(DataContextType.TransportBooking)));
					result = !isSourceTB || isPublishedExternally;
				}
			}

			return result;
		}

		public sealed override ZString DataContextKey
		{
			get { return ParentBO.KB_JobID; }
		}

		protected sealed override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var query = new ZQuery();
			query.AddToFilter(DtbBookingConsolidationSchema.KB_JobID, matchingValues.Key);
			query.AddToFilter(DtbBookingConsolidationSchema.KB_JobType, GetConsolidationJobTypes());

			return query;
		}

		string[] GetConsolidationJobTypes()
		{
			return new[] { TransportConsolidationJobTypes.Codes.Booking, TransportConsolidationJobTypes.Codes.BookingTransportConsolidation };
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransportBookingConsolidation; }
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}
	}
}
