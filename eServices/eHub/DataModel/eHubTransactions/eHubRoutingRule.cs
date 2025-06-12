using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubRoutingRule
	{
		[Key]
		public Guid RR_PK { get; set; }
		public string RR_Group_Name { get; set; }
		public string RR_Condition_Expression { get; set; }
		public Nullable<Guid> RR_Group_RR_GroupRule { get; set; }
		public Nullable<bool> RR_Group_MatchMultiple { get; set; }
		public Nullable<Guid> RR_Success_SP_Provider { get; set; }
		public Nullable<Guid> RR_Success_CC_Recipient { get; set; }
		public Nullable<Guid> RR_Success_RR_SubRule { get; set; }
		public string RR_Failed_ErrorCode { get; set; }
		public string RR_Failed_ErrorDescription { get; set; }
		public Nullable<Guid> RR_Failed_RR_SubRule { get; set; }
		public string RR_Result_Value { get; set; }
		[Column(TypeName = "datetime2")]
		public Nullable<DateTime> RR_LastUpdateUTC { get; set; }
		public int? RR_Group_Ordering { get; set; }

		[InverseProperty("eHubRoutingRule")]
		public virtual List<eHubClient> eHubClients { get; set; }
		[InverseProperty("eHubRoutingRule")]
		public virtual List<eHubServiceProvider> eHubServiceProviders { get; set; }
		[ForeignKey("RR_Success_CC_Recipient")]
		public virtual eHubClient eHubClient_Recipient { get; set; }
		[InverseProperty("eHubRoutingRule_Group")]
		public virtual List<eHubRoutingRule> eHubRoutingRules_Group { get; set; }
		[ForeignKey("RR_Group_RR_GroupRule")]
		public virtual eHubRoutingRule eHubRoutingRule_Group { get; set; }
		[InverseProperty("eHubRoutingRule_Success")]
		public virtual List<eHubRoutingRule> eHubRoutingRules_Success { get; set; }
		[ForeignKey("RR_Success_RR_SubRule")]
		public virtual eHubRoutingRule eHubRoutingRule_Success { get; set; }
		[InverseProperty("eHubRoutingRule_Failed")]
		public virtual List<eHubRoutingRule> eHubRoutingRules_Failed { get; set; }
		[ForeignKey("RR_Failed_RR_SubRule")]
		public virtual eHubRoutingRule eHubRoutingRule_Failed { get; set; }
		[InverseProperty("eHubRoutingRule")]
		public virtual List<eHubRoutingRuleFact> eHubRoutingRuleFacts { get; set; }
		[InverseProperty("eHubRoutingRule_Compute")]
		public virtual List<eHubRoutingRuleFact> eHubRoutingRule_Computes { get; set; }
		[ForeignKey("RR_Success_SP_Provider")]
		public virtual eHubServiceProvider eHubServiceProvider { get; set; }

		public eHubRoutingRule()
		{
			RR_PK = Guid.NewGuid();
			eHubClients = new List<eHubClient>();
			eHubServiceProviders = new List<eHubServiceProvider>();
			eHubRoutingRules_Group = new List<eHubRoutingRule>();
			eHubRoutingRules_Success = new List<eHubRoutingRule>();
			eHubRoutingRules_Failed = new List<eHubRoutingRule>();
			eHubRoutingRuleFacts = new List<eHubRoutingRuleFact>();
		}
	}
}
