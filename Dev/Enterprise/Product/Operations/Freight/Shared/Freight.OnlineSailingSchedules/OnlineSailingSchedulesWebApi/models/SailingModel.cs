using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.OnlineSailingSchedulesWebApi;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public record SailingModel : ISailingModel
	{
		readonly JobSailing sailing;

		public SailingModel(IEnumerable<JobSailing> newSailings, JobSailing sailing, IConnectionQuery connectionQuery = null)
		{
			IsNew = newSailings.Contains(sailing);
			this.sailing = sailing;
			if (connectionQuery != null)
			{
				MatchesConnectionQuery = this.sailing.JX_ServiceString == connectionQuery.TradeLane.Name;
			}
		}

		#region ISailingModel

		public Guid PK => sailing.PK.ToGuid();

		public string SailingID => sailing.JX_UniqueReference;

		public string VesselName => sailing.Vessel?.RV_Name;

		public bool IsNew { get; set; }

		public bool? MatchesConnectionQuery { get; set; }

		public DateTime Departure => (sailing as IFlightInformationProvider).ETD.ToDateTime();

		public DateTime Arrival => (sailing as IFlightInformationProvider).ETA.ToDateTime();

		#endregion
	}
}
