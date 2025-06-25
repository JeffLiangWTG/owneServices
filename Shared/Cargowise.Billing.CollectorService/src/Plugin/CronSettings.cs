using System;
using System.Linq;

namespace CargoWise.Billing.CollectorService.Plugin
{
#nullable disable
	[Serializable]
	public class CronSettings : IEquatable<CronSettings>
	{
		public CronSettings()
		{
		}

		public CronSettings(CronSettings settings)
		{
			Expression = settings.Expression;
			TimezoneId = settings.TimezoneId;
			CollectFromPreviousOccurrence = settings.CollectFromPreviousOccurrence;
			NextOccurrenceDelayInMinutes = settings.NextOccurrenceDelayInMinutes;
		}

		public string Expression { get; set; }
		public string TimezoneId { get; set; }
		public bool CollectFromPreviousOccurrence { get; set; }
		public string NextOccurrenceDelayInMinutes { get; set; }

		#region Equality

		public static bool Equals(CronSettings first, CronSettings second)
		{
			if (ReferenceEquals(first, second))
				return true;
			if (ReferenceEquals(null, first))
				return false;
			if (ReferenceEquals(null, second))
				return false;
			return first.Equals(second);
		}

		public bool Equals(CronSettings other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return
				string.Equals(Expression, other.Expression) &&
				string.Equals(TimezoneId, other.TimezoneId) &&
				CollectFromPreviousOccurrence == other.CollectFromPreviousOccurrence;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is CronSettings settings && Equals(settings);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (Expression != null ? Expression.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (TimezoneId != null ? TimezoneId.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ CollectFromPreviousOccurrence.GetHashCode();
				return hashCode;
			}
		}

		#endregion // Equality
	}
}
