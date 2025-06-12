using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubClientSystem
	{
		[Key]
		public virtual Guid EH_PK { get; set; }
		public virtual string EH_ID { get; set; }
		public virtual string EH_URL { get; set; }
		public virtual DateTime EH_InsertUTC { get; set; }
		public virtual DateTime EH_LastUpdateUTC { get; set; }

		#region Relationships

		[InverseProperty("eHubClientSystem")]
		public List<eHubAsyncPollingRegistration> eHubAsyncPollingRegistrations { get; set; }
		[InverseProperty("eHubClientSystem")]
		public List<eHubITCustomsJobStatus> eHubITCustomsJobStatuses { get; set; }
		[InverseProperty("eHubClientSystem")]
		public List<eHubClientSystemRegistration> eHubClientSystemRegistrations { get; set; }
		[InverseProperty("eHubClientSystem")]
		public List<eHubCertificate> eHubCertificates { get; set; }

		#endregion

		public eHubClientSystem()
		{
			EH_PK = Guid.NewGuid();
		}
	}
}
