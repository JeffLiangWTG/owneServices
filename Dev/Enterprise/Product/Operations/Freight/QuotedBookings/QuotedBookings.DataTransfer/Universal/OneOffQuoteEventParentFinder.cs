using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	class OneOffQuoteEventParentFinder : EventParentFinder
	{
		internal OneOffQuoteEventParentFinder(BusinessObjectFactory factory, OneOffQuoteDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(Event xmlEvent)
		{
			return new OneOffQuoteEventParentFinderFinderHelper(factory, logger).GetLogParentsFor(xmlEvent);
		}

		class OneOffQuoteEventParentFinderFinderHelper : BaseShipmentEventParentFinderHelper
		{
			internal OneOffQuoteEventParentFinderFinderHelper(BusinessObjectFactory factory, IXmlImportLogger logger)
				: base(factory, logger)
			{
			}

			internal BusinessObject[] GetLogParentsFor(IXmlEventValueObject xmlEvent)
			{
				BusinessObject[] result = null;

				var oneOffQuote = GetLogParent(xmlEvent);
				if (oneOffQuote != null)
				{
					result = new BusinessObject[] { oneOffQuote };
				}

				return result;
			}

			BusinessObject GetLogParent(IXmlEventValueObject xmlEvent)
			{
				if (xmlEvent?.DataContext?.DataTargetCollection != null && xmlEvent.DataContext.DataTargetCollection.Count() == 1)
				{
					var key = xmlEvent.DataContext.DataTargetCollection.First().Key;

					if (!string.IsNullOrEmpty(key))
					{
						var query = new ZQuery(RatingHeaderSchema.TH_QuoteNumber, key);
						query.AddToFilter(RatingHeaderSchema.TH_IsCancelled, false);

						var ratingHeader = factory.LoadTop1<RatingHeader>(query);
						if (ratingHeader == null)
						{
							return null;
						}

						var bookingQuery = new ZQuery(ViewQuotedBookingSchema.VB_TH, ratingHeader.PK);
						return factory.LoadTop1<ViewQuotedBooking>(bookingQuery)?.QuotedBooking;
					}
				}

				return null;
			}
		}
	}
}
