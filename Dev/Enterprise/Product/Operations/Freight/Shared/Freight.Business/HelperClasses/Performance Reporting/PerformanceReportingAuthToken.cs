using System.Collections.Generic;

namespace Enterprise.Freight.Business
{
	public class PerformanceReportingAuthToken : IPerformanceReportingAuthToken
	{
		public PerformanceReportingAuthToken(string value)
		{
			Value = value;
		}

		public string Value { get; }
		public override bool Equals(object obj)
		{
			return obj is PerformanceReportingAuthToken token && Value == token.Value;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return -1937169414 + EqualityComparer<string>.Default.GetHashCode(Value);
			}
		}
	}
}
