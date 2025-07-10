using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	class DateRangeInterchangeRequeueRequestFilterProcessor : IInterchangeRequeueRequestFilterProcessor
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public ZQuery Process(IEnumerable<InterchangeRequeueRequestFilter> filters, out string errorMessage)
		{
			errorMessage = string.Empty;

			var dateFromFilter = filters.Where(f => (string)f.Type == InterchangeRequeueRequestFilter.FilterTypeCreateDateUTCFrom);

			if (!dateFromFilter.Any() || dateFromFilter.Skip(1).Any())
			{
				errorMessage = FormattableString.Invariant($"{ErrorMessagePrefix}The request must contain exactly one {InterchangeRequeueRequestFilter.FilterTypeCreateDateUTCFrom} filter.");
				return null;
			}

			if (!DateTime.TryParse(dateFromFilter.First().Value, out DateTime dateFrom))
			{
				errorMessage = FormattableString.Invariant($"{ErrorMessagePrefix}Wrong value of {InterchangeRequeueRequestFilter.FilterTypeCreateDateUTCFrom} filter (should be in a valid datetime format e.g. '06/08/2017 23:19:45').");
				return null;
			}

			var result = new ZQuery(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, dateFrom);

			var dateToFilter = filters.Where(f => (string)f.Type == InterchangeRequeueRequestFilter.FilterTypeCreateDateUTCTo);

			if (dateToFilter.Any())
			{
				if (dateToFilter.Skip(1).Any())
				{
					errorMessage = FormattableString.Invariant($"{ErrorMessagePrefix}The request must contain no more than one {InterchangeRequeueRequestFilter.FilterTypeCreateDateUTCTo} filter.");
					return null;
				}

				if (!DateTime.TryParse(dateToFilter.First().Value, out DateTime dateTo))
				{
					errorMessage = FormattableString.Invariant($"{ErrorMessagePrefix}Wrong value of {InterchangeRequeueRequestFilter.FilterTypeCreateDateUTCTo} filter (should be in a valid datetime format e.g. '06/08/2017 23:19:45').");
					return null;
				}

				if ((dateTo - dateFrom) <= TimeSpan.Zero)
				{
					errorMessage = FormattableString.Invariant($"{ErrorMessagePrefix}{InterchangeRequeueRequestFilter.FilterTypeCreateDateUTCTo} must be greater than {InterchangeRequeueRequestFilter.FilterTypeCreateDateUTCFrom}.");
					return null;
				}

				if ((dateTo - dateFrom) > TimeSpan.FromHours(24))
				{
					errorMessage = FormattableString.Invariant($"{ErrorMessagePrefix}Maximum allowable range for a single update is 24 hours.");
					return null;
				}

				result.AddToFilter(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.LessThan, dateTo);
			}
			else
			{
				if ((DateTime.UtcNow - dateFrom) > TimeSpan.FromHours(24))
				{
					errorMessage = FormattableString.Invariant($"{ErrorMessagePrefix}When {InterchangeRequeueRequestFilter.FilterTypeCreateDateUTCTo} is not specified the value of {InterchangeRequeueRequestFilter.FilterTypeCreateDateUTCFrom} must be less than 24 hours in the past.");
					return null;
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "a part of an xml response")]
		const string ErrorMessagePrefix = "Invalid Date Range - ";
	}
}
