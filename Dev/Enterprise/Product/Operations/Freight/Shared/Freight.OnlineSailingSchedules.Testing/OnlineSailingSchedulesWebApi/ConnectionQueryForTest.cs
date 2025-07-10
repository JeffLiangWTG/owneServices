using System;
using Enterprise.Freight.Integration.OnlineSailingSchedulesWebApi;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	internal class ConnectionQueryForTest : IConnectionQuery
	{
		public DateTime StartDate { get; set; }

		public DateTime ExpiryDate { get; set; }

		public string LoadPort { get; set; }

		public string DischargePort { get; set; }

		public bool AllowRelatedUNLOCOs { get; set; }

		public bool DirectRoutesOnly { get; set; }

		public IGSSTradeLane TradeLane { get; set; }
	}

	class GSSTradeLaneForTest : IGSSTradeLane
	{
		public string Code { get; set; }

		public string Name { get; set; }
	}
}
