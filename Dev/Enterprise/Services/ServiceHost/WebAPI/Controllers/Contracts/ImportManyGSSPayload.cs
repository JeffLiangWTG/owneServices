using System;
using System.Collections.Generic;
using Enterprise.Freight.Integration.OnlineSailingSchedulesWebApi;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.Contracts
{
	public class ImportManyGSSPayload : IImportManyGSSPayload
	{
		public List<ConnectionQuery> ConnectionQueries { get; set; }

		IEnumerable<IConnectionQuery> IImportManyGSSPayload.ConnectionQueries => ConnectionQueries;
	}

	public class ConnectionQuery : IConnectionQuery
	{
		public DateTime StartDate { get; set; }

		public DateTime ExpiryDate { get; set; }

		public string LoadPort { get; set; }

		public string DischargePort { get; set; }

		public bool AllowRelatedUNLOCOs { get; set; }

		public GSSTradeLane TradeLane { get; set; }

		public bool DirectRoutesOnly { get; set; }

		IGSSTradeLane IConnectionQuery.TradeLane => TradeLane;
	}

	public class GSSTradeLane : IGSSTradeLane
	{
		public string Code { get; set; }

		public string Name { get; set; }
	}
}
