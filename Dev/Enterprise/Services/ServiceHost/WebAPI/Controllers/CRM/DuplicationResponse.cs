using System;

namespace Enterprise.Services.ServiceHost
{
	public class DuplicationResponse
	{
		public DuplicationResponse()
		{
		}

		public Guid PK;
		public double Score;
		public string MatchingBrandName;
		public string MatchingAddress1;
		public string MatchingAddress2;
		public string MatchingCity;
		public string MatchingState;
		public string MatchingCountryRegion;
	}
}
