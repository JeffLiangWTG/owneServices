using System.Collections.Generic;
using Enterprise.DataTransfer.BatchProcessor.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.MasterFiles.DataTransfer.Universal.InterchangeRequeue.RequestProcessors.Testing
{
	class NumberRangeFilterProcessorTest : BaseLoggedDataBatchProcessTestCase
	{
		public void TestReturnProperQueryByCorrectFilter()
		{
			AssertSuccess("20", "30");

			var filterProcessor = new NumberRangeInterchangeRequeueRequestFilterProcessor();

			var query = filterProcessor.Process(new List<InterchangeRequeueRequestFilter>(), out string errMessage);

			Assert(query != null);
			Assert(string.IsNullOrEmpty(query.FilterString));
			Assert(string.IsNullOrEmpty(errMessage));
		}

		public void TestProcessSingleCorrectlyWrongNumberOfFilters()
		{
			AssertSuccess(null, "273");

			AssertSuccess("25", null);
		}

		void AssertSuccess(string numberFrom, string numberTo)
		{
			var filters = GetFilters(numberFrom, numberTo);

			var filterProcessor = new NumberRangeInterchangeRequeueRequestFilterProcessor();

			var query = filterProcessor.Process(filters, out string errMessage);

			Assert(errMessage, query != null);

			string expectedFilter = string.Empty;
			if (!string.IsNullOrEmpty(numberFrom) && !string.IsNullOrEmpty(numberTo))
			{
				expectedFilter = $"\r\n\tWHERE EI_InterchangeNum >= '{numberFrom}' and EI_InterchangeNum <= '{numberTo}'";
			}
			else if (!string.IsNullOrEmpty(numberFrom))
			{
				expectedFilter = $"\r\n\tWHERE EI_InterchangeNum >= '{numberFrom}'";
			}
			else if (!string.IsNullOrEmpty(numberTo))
			{
				expectedFilter = $"\r\n\tWHERE EI_InterchangeNum <= '{numberTo}'";
			}

			Assert(query.GetAsWhereClause(true) == expectedFilter);
		}

		List<InterchangeRequeueRequestFilter> GetFilters(string numberFrom, string numberTo)
		{
			var result = new List<InterchangeRequeueRequestFilter>();

			if (!string.IsNullOrEmpty(numberFrom))
			{
				result.Add(new InterchangeRequeueRequestFilter()
				{
					Type = InterchangeRequeueRequestFilter.FilterTypeInterchangeNumberFrom,
					Value = numberFrom,
				});
			}

			if (!string.IsNullOrEmpty(numberTo))
			{
				result.Add(new InterchangeRequeueRequestFilter()
				{
					Type = InterchangeRequeueRequestFilter.FilterTypeInterchangeNumberTo,
					Value = numberTo,
				});
			}

			return result;
		}
	}
}
