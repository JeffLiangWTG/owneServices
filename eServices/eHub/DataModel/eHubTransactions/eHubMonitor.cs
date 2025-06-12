using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubMonitor
	{
		#region Fields

		[Key]
		public Guid MO_PK { get; set; }
		[MaxLength(100)]
		public string MO_ID { get; set; }
		[MaxLength(100)]
		public string MO_Description { get; set; }
		public int MO_ErrorDelayMins { get; set; }
		[MaxLength(100)]
		public string MO_Type { get; set; }
		public bool MO_Enabled { get; set; }
		public string MO_Notes { get; set; }
		public int? MO_MaintenanceStartDayOfWeek { get; set; }
		public TimeSpan? MO_MaintenanceStartTime { get; set; }
		public int? MO_MaintenanceEndDayOfWeek { get; set; }
		public TimeSpan? MO_MaintenanceEndTime { get; set; }

#endregion

#region Relationships
public virtual List<eHubClient> eHubClients { get; set; }
		#endregion

		#region DefaultConstructor

		public eHubMonitor()
		{
			MO_PK = Guid.NewGuid();
		}

		#endregion
	}
}
