using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	class NumberRangeInterchangeRequeueRequestFilterProcessor : IInterchangeRequeueRequestFilterProcessor
	{
		public ZQuery Process(IEnumerable<InterchangeRequeueRequestFilter> filters, out string errorMessage)
		{
			var query = new ZQuery();

			ProcessSimpleUniqueFilter(filters, InterchangeRequeueRequestFilter.FilterTypeInterchangeNumberFrom, query, EDIInterchangeSchema.EI_InterchangeNum, SQLComparisonOperator.GreaterThanOrEqualTo, out errorMessage);

			if (!string.IsNullOrEmpty(errorMessage))
			{
				return null;
			}

			ProcessSimpleUniqueFilter(filters, InterchangeRequeueRequestFilter.FilterTypeInterchangeNumberTo, query, EDIInterchangeSchema.EI_InterchangeNum, SQLComparisonOperator.LessThanOrEqualTo, out errorMessage);

			if (!string.IsNullOrEmpty(errorMessage))
			{
				return null;
			}

			return query;
		}

		void ProcessSimpleUniqueFilter(IEnumerable<InterchangeRequeueRequestFilter> filters, string filterType, ZQuery query, CargoWise.Schema.SchemaColumn column, SQLComparisonOperator sqlOperator, out string errorMessage)
		{
			errorMessage = string.Empty;
			var filtersOfTheGivenType = filters.Where(f => (string)f.Type == filterType);

			if (!filtersOfTheGivenType.Any())
			{
				return;
			}

			if (filtersOfTheGivenType.Skip(1).Any())
			{
				errorMessage = FormattableString.Invariant($"{ErrorMessagePrefix}The request must contain no more than one {filterType} filter");
				return;
			}

			query.AddToFilter(column, sqlOperator, filtersOfTheGivenType.First().Value);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "a part of an xml response")]
		const string ErrorMessagePrefix = "Invalid Interchange Number Range - ";
	}
}
