using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Provides methods for reporting the discarding of rate entries or
	/// rate lines.
	///
	/// Todo: This will soon replace the BaseFallbackRateMatcher ReasonType
	/// </summary>
	public static class DiscardReporter
	{
		// todo: Make this reason private and just add more methods, one for
		// each reason - to make it staticly typed.
		public enum Reason
		{
			DateAfter,
			DateBefore,
			DateBetween,
			NotApplicableToJob,
			CrossTrade,
			VsJob,
		}

		public static string DiscardReason(Reason reasonType, SchemaColumn column, params ZString[] jobValues)
			=> DiscardReason(reasonType, column, jobValues.Select(x => (string)x).ToArray());
		public static string DiscardReason(Reason reasonType, SchemaColumn column, params string[] jobValues)
		{
			var columnName = column == null
				? string.Empty
				: DataBoundResourceStrings.GetColumnDescriptiveName(column.TableName, column.Name);

			return DiscardReason(reasonType, columnName, jobValues);
		}

		public static string DiscardReason(Reason reasonType, string columnName, params ZString[] jobValues)
			=> DiscardReason(reasonType, columnName, jobValues.Select(x => (string)x).ToArray());

		public static string DiscardReason(Reason reasonType, string columnName, params string[] jobValues)
		{
			var template = ZString.Empty;
			var delimiter = ZString.Empty;
			var combinedValue = ZString.Empty;
			var formatValues = new ArrayList();

			if (!string.IsNullOrEmpty(columnName))
			{
				formatValues.Add(columnName);
			}

			#region SuppressResourceStringsCheckRegion

			switch (reasonType)
			{
				case Reason.VsJob:
					template = "{0} didn't match job {1}";
					delimiter = ",";
					break;
				case Reason.DateAfter:
					template = "{0} is after {1}";
					break;
				case Reason.DateBefore:
					template = "{0} is before {1}";
					break;
				case Reason.DateBetween:
					template = "{0} is not between {1}";
					delimiter = " and ";
					break;
				case Reason.NotApplicableToJob:
					template = "{0} do not apply to {1} job";
					break;
				case Reason.CrossTrade:
					template = "Company {0} is in the same country with {1} or {2}";
					break;
			}

			#endregion

			if (jobValues == null)
			{
				combinedValue = (ZString)"null";
				formatValues.Add(combinedValue);
			}
			else if (jobValues.All(x => string.IsNullOrEmpty(x)))
			{
				combinedValue = (ZString)(NoResString)"empty"; // Filter log message
				formatValues.Add(combinedValue);
			}
			else if (!string.IsNullOrEmpty(delimiter))
			{
				combinedValue = string.Join(delimiter, jobValues);
				formatValues.Add(combinedValue);
			}
			else
			{
				formatValues.AddRange(jobValues);
			}

			return ZString.Format(template, formatValues.ToArray());
		}
	}
}
