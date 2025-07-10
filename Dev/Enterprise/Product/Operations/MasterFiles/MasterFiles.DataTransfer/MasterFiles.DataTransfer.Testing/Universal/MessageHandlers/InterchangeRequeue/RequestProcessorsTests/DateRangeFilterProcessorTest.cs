using System;
using System.Collections.Generic;
using Enterprise.DataTransfer.BatchProcessor.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.MasterFiles.DataTransfer.Universal.InterchangeRequeue.RequestProcessors.Testing
{
	class DateRangeFilterProcessorTest : BaseLoggedDataBatchProcessTestCase
	{
		public void TestReturnProperQueryByCorrectFilter()
		{
			var dtTo = DateTime.UtcNow;
			var dtFrom = dtTo.AddHours(-24);
			AssertSuccess(dtFrom, dtTo);
			AssertSuccess(DateTime.UtcNow.AddHours(-23.99));
		}

		public void TestProcessCorrectlyWrongNumberOfFilters()
		{
			var filters = new List<InterchangeRequeueRequestFilter>();

			AssertRaisesError("Invalid Date Range - The request must contain exactly one CreateDateUTCFrom filter.", filters);

			var dtTo = DateTime.UtcNow;
			var dtFrom = DateTime.UtcNow.AddHours(-23.9);
			filters = GetFilters(dtFrom);

			filters.Add(new InterchangeRequeueRequestFilter
			{
				Type = InterchangeRequeueRequestFilter.FilterTypeCreateDateUTCFrom,
				Value = dtFrom.AddMinutes(10).ToString(),
			});

			AssertRaisesError("Invalid Date Range - The request must contain exactly one CreateDateUTCFrom filter.", filters);

			filters = GetFilters(dtFrom, dtTo);

			filters.Add(new InterchangeRequeueRequestFilter
			{
				Type = InterchangeRequeueRequestFilter.FilterTypeCreateDateUTCTo,
				Value = dtTo.AddMinutes(10).ToString(),
			});

			AssertRaisesError("Invalid Date Range - The request must contain no more than one CreateDateUTCTo filter.", filters);
		}

		public void TestDoesNotAllowMoreThan24hPeriod()
		{
			var dtTo = DateTime.UtcNow;
			var dtFrom = DateTime.UtcNow.AddHours(-24.001);

			var filters = GetFilters(dtFrom, dtTo);
			AssertRaisesError("Invalid Date Range - Maximum allowable range for a single update is 24 hours.", filters);

			filters = GetFilters(dtFrom);
			AssertRaisesError("Invalid Date Range - When CreateDateUTCTo is not specified the value of CreateDateUTCFrom must be less than 24 hours in the past.", filters);
		}

		public void TestDoesNotAllowNegativeDateRange()
		{
			var dtTo = DateTime.UtcNow;
			var dtFrom = dtTo;

			var filters = GetFilters(dtFrom, dtTo);
			AssertRaisesError("Invalid Date Range - CreateDateUTCTo must be greater than CreateDateUTCFrom.", filters);
		}

		public void TestInformUserIfFilterValueIsNotValid()
		{
			var filters = new List<InterchangeRequeueRequestFilter>();

			filters.Add(new InterchangeRequeueRequestFilter
			{
				Type = InterchangeRequeueRequestFilter.FilterTypeCreateDateUTCFrom,
				Value = "(not a date)"
			});

			AssertRaisesError("Invalid Date Range - Wrong value of CreateDateUTCFrom filter (should be in a valid datetime format e.g. '06/08/2017 23:19:45').", filters);

			filters = GetFilters(DateTime.Now);

			filters.Add(new InterchangeRequeueRequestFilter
			{
				Type = InterchangeRequeueRequestFilter.FilterTypeCreateDateUTCTo,
				Value = "(not a date)"
			});

			AssertRaisesError("Invalid Date Range - Wrong value of CreateDateUTCTo filter (should be in a valid datetime format e.g. '06/08/2017 23:19:45').", filters);
		}

		void AssertSuccess(DateTime dtFrom, DateTime? dtTo = null)
		{
			var filters = GetFilters(dtFrom, dtTo);

			var filterProcessor = new DateRangeInterchangeRequeueRequestFilterProcessor();

			var query = filterProcessor.Process(filters, out string errMessage);

			Assert(errMessage, query != null);

			var expectedFilter = dtTo != null
				? $"\r\n\tWHERE EI_SystemCreateTimeUtc >= #{dtFrom.ToString("yyyy-MM-dd HH:mm:ss.000")}# and EI_SystemCreateTimeUtc < #{dtTo.Value.ToString("yyyy-MM-dd HH:mm:ss.000")}#"
				: $"\r\n\tWHERE EI_SystemCreateTimeUtc >= #{dtFrom.ToString("yyyy-MM-dd HH:mm:ss.000")}#"; //2017-06-08 00:34:46.000

			Assert(query.GetAsWhereClause(true) == expectedFilter);
		}

		void AssertRaisesError(string expectedError, IEnumerable<InterchangeRequeueRequestFilter> filters)
		{
			var filterProcessor = new DateRangeInterchangeRequeueRequestFilterProcessor();

			var query = filterProcessor.Process(filters, out string errMessage);

			Assert(query == null);

			Assert(errMessage == expectedError);
		}

		List<InterchangeRequeueRequestFilter> GetFilters(DateTime dtFrom, DateTime? dtTo = null)
		{
			var result = new List<InterchangeRequeueRequestFilter>
			{
				new InterchangeRequeueRequestFilter()
				{
					Type = InterchangeRequeueRequestFilter.FilterTypeCreateDateUTCFrom,
					Value = dtFrom.ToString(),
				}
			};

			if (dtTo != null)
			{
				result.Add(new InterchangeRequeueRequestFilter()
				{
					Type = InterchangeRequeueRequestFilter.FilterTypeCreateDateUTCTo,
					Value = dtTo.Value.ToString(),
				});
			}

			return result;
		}
	}
}
