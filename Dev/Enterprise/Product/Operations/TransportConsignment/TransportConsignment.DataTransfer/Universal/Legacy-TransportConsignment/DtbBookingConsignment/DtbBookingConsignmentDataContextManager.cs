using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.DataTransfer.Universal;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	public class DtbBookingConsignmentDataContextManager : DtbTransportDataContextManager<DtbBookingConsignment>
	{
		#region Context

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransportConsignment; }
		}

		protected override string[] GetConsolidationJobTypes()
		{
			return new[] { TransportConsolidationJobTypes.Codes.Consignment };
		}

		#endregion

		#region Events

		/// <summary>
		/// When exporting events, add these contexts so the importer can match.
		/// </summary>
		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				result.AddIfNotEmpty(UniversalEvent.ContextTypes.TransportBookingJobID, ParentBO.TransportBookingPartyReference);
			}

			return result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new DtbBookingConsignmentEventParentFinder(this, factory, logger);
		}

		#endregion

		#region Shipments

		#region GetShipmentDataObjectReader

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new DtbBookingConsignmentDataObjectReader_DataTargetIsTransportBooking(universalShipment, logger, factory);
		}

		#endregion

		#region GetShipmentDataObjectWriter

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new DtbBookingConsignmentDataObjectWriter(writeManager);
		}

		#endregion

		#endregion
	}
}


