using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal
{
	public class LocalTransportDataContextManager : ShipmentDataContextManager<CommonCartage>
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.LocalTransport; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.JJ_ConsignmentID; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery(JobCartageSchema.JJ_ConsignmentID, matchingValues.Key);
		}

		/// <summary>
		/// Match on importing events where TransportRef matches ConsignmentID and Transport Booking ID matches OrderRef.
		/// </summary>
		/// <param name="factory"></param>
		/// <returns></returns>
		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new LocalTransportEventParentFinder(factory, this, logger);
		}

		/// <summary>
		/// When exporting events, add these contexts so the importer can match.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				result.AddIfNotEmpty(UniversalEvent.ContextTypes.ConsignmentNoteNumber, ParentBO.JJ_ConsignmentID);
				result.AddIfNotEmpty(UniversalEvent.ContextTypes.OrderNumber, ParentBO.JJ_OrderReferenceNumber);
				result.AddIfNotEmpty(UniversalEvent.ContextTypes.WaybillNumber, ParentBO.JJ_WaybillNumber);
				result.AddIfNotEmpty(UniversalEvent.ContextTypes.QuoteNumber, ParentBO.JJ_QuoteNumber);
				result.AddIfNotEmpty(UniversalEvent.ContextTypes.TransportBookingJobID, ParentBO.TransportBookingPartyReference);

				ParentBO.AdditionalReferenceNumbers.AddAdditionalReferences(result);
				result.RemoveAll(k => k.Key.Type == AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber);
			}

			return result;
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			// to keep backwards compatability
			// normally the only way to create LT jobs is to use the Service Task and target this module
			return recipientRoles.Any(o => o.Code == RecipientRoleType.DCA || o.Code == RecipientRoleType.PCA);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new LocalTransportDataObjectWriter(writeManager);
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			var sourceDO = UniversalShipment.GetSourceDataObject(universalShipment);
			var isMultiJobConsolidation = sourceDO.GetMatchingDataSource(DataContextType.TransportBookingConsolidation) != null;

			return isMultiJobConsolidation
				? new LocalTransportDataObjectReaderFromConsolidation(universalShipment, logger, factory)
				: new LocalTransportDataObjectReader(universalShipment, logger, factory);
		}

		class LocalTransportDataObjectReaderFromConsolidation : ITopLevelDataObjectReader
		{
			public LocalTransportDataObjectReaderFromConsolidation(UniversalShipment topLevelDO, IXmlImportLogger logger, UniversalObjectFactory factory)
			{
				TopLevelDO = Argument.NotNull(topLevelDO, "topLevelDO");
				Logger = Argument.NotNull(logger, "logger");
				Factory = Argument.NotNull(factory, "factory");
				BookingDataObjects = topLevelDO.SubShipmentCollection != null
					? topLevelDO.SubShipmentCollection.Where(bookingDO => bookingDO.GetMatchingDataSource(DataContextType.TransportBooking) != null)
					: Enumerable.Empty<UniversalShipment>();
			}

			readonly UniversalShipment TopLevelDO;
			readonly IXmlImportLogger Logger;
			readonly UniversalObjectFactory Factory;
			readonly IEnumerable<UniversalShipment> BookingDataObjects;

			public BusinessObject GetExistingBusinessObject()
			{
				return null;
			}

			public void ReadIntoBusinessObject(ref BusinessObject targetBO)
			{
				foreach (var bookingDO in BookingDataObjects)
				{
					new LocalTransportDataObjectReader(TopLevelDO, Logger, Factory, bookingDO).ReadIntoBusinessObject();
				}
			}

			public BusinessObject ReadIntoTopLevelBusinessObject()
			{
				var businessObject = (BusinessObject)null;
				ReadIntoBusinessObject(ref businessObject);
				return businessObject;
			}

			public IEnumerable<(string KeyValue, string KeySource)> ReadKeysForParallelism()
			{
				yield break;
			}
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}
	}
}
