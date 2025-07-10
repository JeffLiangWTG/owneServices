using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Integration.MasterFiles;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingJobNumUniqueIndexErrorHelper : IJobNumUniqueIndexErrorHelper
	{
		public string GetErrorMessage(ZSaveException ex)
		{
			var bookingJobNum = ex.BusinessObjects.Length > 0 && ex.BusinessObjects[0] is JobHeader job && job.Parent is QuotedBooking booking
				? (string)booking.Booking?.JS_UniqueConsignRef
				: GetFallbackFromException(ex);

			return !bookingJobNum.IsNullOrEmpty()
				? Res.GetString("7048352a-58fa-00a1-46d3-6a68f17ebc25", "This Job Billing header ({0}) is not currently valid as the Booking has not been converted into a Shipment. Consolidate the Booking before proceeding with Job Closure.", bookingJobNum)
				: null;
		}

		static string GetFallbackFromException(ZSaveException ex)
		{
			var quoteNum = GetQuoteJobNumFromExceptionMessage(ex.Message);
			return GetBookingJobNum(quoteNum, ex.Factory);
		}

		static string GetBookingJobNum(string quoteNum, BusinessObjectFactory factory)
		{
			var quoteSubQuery = new ZDBOnlySubQuery(typeof(RatingHeader), RatingHeaderSchema.PK);
			quoteSubQuery.AddToFilter(RatingHeaderSchema.TH_QuoteNumber, quoteNum);

			var bookingQuery = new ZDBOnlyQuery(typeof(CommonShipment));
			bookingQuery.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, false);
			bookingQuery.AddSubQuery(JobShipmentSchema.JS_TH_OneTimeQuote, quoteSubQuery, JoinCondition.And);

			var booking = factory.LoadTop1<CommonShipment>(bookingQuery);
			return booking?.JS_UniqueConsignRef;
		}

		static string GetQuoteJobNumFromExceptionMessage(string message)
		{
			var jobNumPattern = new Regex(@"\(.{4,},");
			var match = jobNumPattern.Match(message);
			var result = match.Success ? match.Value : string.Empty;
			var jobNum = !string.IsNullOrEmpty(result) ? result.Substring(1, result.Length - 2) : "";
			return jobNum;
		}
	}
}
