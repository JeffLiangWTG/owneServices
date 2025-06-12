using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubServiceProvider
	{
		[Key]
		public Guid SP_PK { get; set; }
		public Guid SP_CC_Service { get; set; }
		public Guid SP_CC_Provider { get; set; }
		public Guid? SP_RR { get; set; }
		public bool SP_AllowFallback { get; set; } = true;

		[ForeignKey("SP_CC_Service")]
		public virtual eHubClient eHubClient_Service { get; set; }
		[ForeignKey("SP_CC_Provider")]
		public virtual eHubClient eHubClient_Provider { get; set; }
		[ForeignKey("SP_RR")]
		public virtual eHubRoutingRule eHubRoutingRule { get; set; }
		[InverseProperty("eHubServiceProvider")]
		public virtual List<eHubRoutingRule> eHubRoutingRules { get; set; }
		[InverseProperty("eHubServiceProvider")]
		public virtual List<eHubServiceProviderRequiredRegistration> eHubServiceProviderRequiredRegistrations { get; set; }

		public eHubServiceProvider()
		{
			SP_PK = Guid.NewGuid();
			eHubServiceProviderRequiredRegistrations = new List<eHubServiceProviderRequiredRegistration>();
		}
	}
}
