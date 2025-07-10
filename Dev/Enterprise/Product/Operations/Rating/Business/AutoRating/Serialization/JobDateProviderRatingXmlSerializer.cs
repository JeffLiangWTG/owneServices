using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	#region SuppressResourceStringsCheckRegion
	public static class JobDateProviderRatingXmlSerializer
	{
		public static string Serialize(object objectToSerialize)
		{
			var jobDatesProvider = (IJobDatesProvider)objectToSerialize;
			if (jobDatesProvider == null)
			{
				return string.Empty;
			}

			var jobTypeCodesAndDates = JobDateTypes.JobDateTypeList
				.OfType<CodeDescriptionPair>()
				.Select(x => Tuple.Create(x.Description, jobDatesProvider.GetJobDateByType(x.Code)));

			var dateLinesWithValues = jobTypeCodesAndDates.Where(x => x.Item2 != ZDateTime.Empty);
			var dateLinesWithoutValues = jobTypeCodesAndDates.Except(dateLinesWithValues);

			var stringBuilder = new ZStringBuilder();

			stringBuilder.AppendLine(ZString.Format("Type of Date Provider: {0}", jobDatesProvider.ToString()));

			stringBuilder.Append(ZString.Format("Valid Dates:"));
			stringBuilder.AppendLine(dateLinesWithValues.JobDateToString());

			stringBuilder.Append(ZString.Format("Has No Date Dates:"));
			stringBuilder.AppendLine(dateLinesWithoutValues.JobDateToString());

			stringBuilder.Append(ZString.Format("Earliest Possible Date: {0}", jobDatesProvider.EarliestPossibleDate.ToShortDateString()));
			stringBuilder.Append(ZString.Format("Latest Possible Date: {0}", jobDatesProvider.LatestPossibleDate.ToShortDateString()));

			stringBuilder.Append(ZString.Format("Transit Time: {0}", string.IsNullOrEmpty(jobDatesProvider.TransitTime) ? "N/A" : jobDatesProvider.TransitTime));

			return stringBuilder.ToStringWithNewLineBetweenAppends();
		}

		static string JobDateToString(this IEnumerable<Tuple<string, ZDateTime>> values)
		{
			return string.Join(System.Environment.NewLine, values.Select(x => ZString.Format("{0}: {1}", x.Item1, x.Item2.IsEmpty ? "N/A" : x.Item2.ToShortDateString())));
		}
	}

	#endregion
}

