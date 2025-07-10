using System.Collections.Generic;
using Enterprise.DataTransfer.BatchProcessor.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal.InterchangeRequeue.RequestProcessors.Testing
{
	class OrFilterProcessorTest : BaseLoggedDataBatchProcessTestCase
	{
		public void TestAllowsEmptyFilter()
		{
			var filterProcessor = new NumberRangeInterchangeRequeueRequestFilterProcessor();

			var query = filterProcessor.Process(new List<InterchangeRequeueRequestFilter>(), out string errMessage);

			Assert(query != null);
			Assert(string.IsNullOrEmpty(query.FilterString));
			Assert(string.IsNullOrEmpty(errMessage));
		}

		public void TestCanCreateRegularQueryByFilter()
		{
			var filterProcessor = new OrInterchangeRequeueRequestFilterProcessor(EDIInterchangeSchema.EI_ApplicationCode, InterchangeRequeueRequestFilter.FilterTypeApplicationCode);

			var filters = GetFilters(InterchangeRequeueRequestFilter.FilterTypeApplicationCode, "123", "456");

			var query = filterProcessor.Process(filters, out string errMessage);

			Assert(query != null);
			Assert(string.IsNullOrEmpty(errMessage));

			Assert(query.GetAsWhereClause(true) == $"\r\n\tWHERE {EDIInterchangeSchema.EI_ApplicationCode.Name} = '123' or {EDIInterchangeSchema.EI_ApplicationCode.Name} = '456'");
		}

		public void TestCanCreateSubQueryByFilter()
		{
			var filterProcessor = new OrInterchangeRequeueRequestFilterProcessor(typeof(IEDIInterchange), EDIInterchangeSchema.EI_GB, typeof(IGlbBranch), GlbBranchSchema.GB_Code, InterchangeRequeueRequestFilter.FilterTypeBranchCode);

			var filters = GetFilters(InterchangeRequeueRequestFilter.FilterTypeBranchCode, "ABC", "DEF");

			var query = filterProcessor.Process(filters, out string errMessage);

			Assert(query != null);
			Assert(string.IsNullOrEmpty(errMessage));

			Assert(query.GetAsWhereClause(true) == $"\r\n\tWHERE {EDIInterchangeSchema.EI_GB.Name} IN (SELECT {GlbBranchSchema.PK.Name} FROM dbo.GlbBranch WHERE {GlbBranchSchema.GB_Code.Name} = 'ABC' or {GlbBranchSchema.GB_Code.Name} = 'DEF')");
		}

		List<InterchangeRequeueRequestFilter> GetFilters(string filterType, params string[] values)
		{
			var result = new List<InterchangeRequeueRequestFilter>();

			foreach (var value in values)
			{
				result.Add(new InterchangeRequeueRequestFilter
				{
					Type = filterType,
					Value = value
				});
			}

			return result;
		}
	}
}
