using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubRoutingRule
    {
        public eHubRoutingRule()
        {
            InverseRR_Failed_RR_SubRuleNavigation = new HashSet<eHubRoutingRule>();
            InverseRR_Group_RR_GroupRuleNavigation = new HashSet<eHubRoutingRule>();
            InverseRR_Success_RR_SubRuleNavigation = new HashSet<eHubRoutingRule>();
            eHubClient = new HashSet<eHubClient>();
            eHubRoutingRuleFactRX_RRNavigation = new HashSet<eHubRoutingRuleFact>();
            eHubRoutingRuleFactRX_RR_ComputeRuleNavigation = new HashSet<eHubRoutingRuleFact>();
        }

        public Guid RR_PK { get; set; }
        public string RR_Condition_Expression { get; set; }
        public Guid? RR_Group_RR_GroupRule { get; set; }
        public bool? RR_Group_MatchMultiple { get; set; }
        public Guid? RR_Success_CC_Recipient { get; set; }
        public Guid? RR_Success_RR_SubRule { get; set; }
        public string RR_Failed_ErrorCode { get; set; }
        public string RR_Failed_ErrorDescription { get; set; }
        public Guid? RR_Failed_RR_SubRule { get; set; }
        public string RR_Result_Value { get; set; }
        public Guid? RR_Success_SP_Provider { get; set; }
        public DateTime? RR_LastUpdateUTC { get; set; }
        public int? RR_Group_Ordering { get; set; }

        public eHubRoutingRule RR_Failed_RR_SubRuleNavigation { get; set; }
        public eHubRoutingRule RR_Group_RR_GroupRuleNavigation { get; set; }
        public eHubClient RR_Success_CC_RecipientNavigation { get; set; }
        public eHubRoutingRule RR_Success_RR_SubRuleNavigation { get; set; }
        public eHubServiceProvider RR_Success_SP_ProviderNavigation { get; set; }
        public ICollection<eHubRoutingRule> InverseRR_Failed_RR_SubRuleNavigation { get; set; }
        public ICollection<eHubRoutingRule> InverseRR_Group_RR_GroupRuleNavigation { get; set; }
        public ICollection<eHubRoutingRule> InverseRR_Success_RR_SubRuleNavigation { get; set; }
        public ICollection<eHubClient> eHubClient { get; set; }
        public ICollection<eHubRoutingRuleFact> eHubRoutingRuleFactRX_RRNavigation { get; set; }
        public ICollection<eHubRoutingRuleFact> eHubRoutingRuleFactRX_RR_ComputeRuleNavigation { get; set; }
    }
}
