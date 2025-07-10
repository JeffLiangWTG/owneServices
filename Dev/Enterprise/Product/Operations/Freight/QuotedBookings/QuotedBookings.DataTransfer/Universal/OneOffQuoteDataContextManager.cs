using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	//For export, we are using QuotedBookingDataContextManager and writing OneOffQuote as DataContextType in XML for OneOffQuote.
	//For Import, we use this class for reading XML.
	public class OneOffQuoteDataContextManager : ShipmentDataContextManager<QuotedBooking>
	{
		public override DataContextType DataContextType => DataContextType.OneOffQuote;

		public override ZString DataContextKey => ZString.Empty;

		public override string DefaultOutputDirectory => null;

		public override bool ManagesShipments => true;

		#region Universal Event Management

		public override bool ManagesEvents
		{
			get { return ParentBO == null || ParentBO.Quote != null; }
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new OneOffQuoteEventParentFinder(factory, this, logger);
		}

		#endregion

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			if (matchingValues.DataObject is ShipmentRequest ||
				(matchingValues.DataObject is UniversalShipment ooQ && ooQ.IsCO2eResponse()))
			{
				var query = new ZQuery(RatingHeaderSchema.TH_QuoteNumber, matchingValues.Key);
				query.AddToFilter(RatingHeaderSchema.TH_IsCancelled, false);

				var ratingHeader = factory.LoadTop1<RatingHeader>(query);
				if (ratingHeader == null)
				{
					return null;
				}

				var bookingQuery = new ZQuery(ViewQuotedBookingSchema.VB_TH, ratingHeader.PK);
				bookingQuery.AddToFilter(ViewQuotedBookingSchema.VB_IsCanceled, false);

				return bookingQuery;
			}

			return null;
		}

		protected override QuotedBooking[] LoadBusinessObjects(BusinessObjectFactory factory, ZQuery query)
		{
			return factory.Load<ViewQuotedBooking>(query).Select(view => view.QuotedBooking).ToArray();
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager) => null;

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger) => false;

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new OneOffQuoteDataObjectReader(universalShipment, logger, factory);
		}
	}
}
