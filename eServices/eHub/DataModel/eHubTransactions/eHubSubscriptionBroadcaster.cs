using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
    public partial class eHubSubscriptionBroadcaster
    {
        #region Fields
		[Key]
		public virtual Guid SB_PK { get; set; }
		public virtual Guid SB_ST { get; set; }
		public virtual Guid SB_CC_Sender { get; set; }
		public virtual Nullable<Guid> SB_CC_Recipient { get; set; }
        public virtual string SB_SubscriberSelectSql { get; set; }
        #endregion

        #region Relationships
		[ForeignKey("SB_ST")]
		public virtual eHubSubscriptionType eHubSubscriptionType { get; set; }
		[ForeignKey("SB_CC_Sender")]
		public virtual eHubClient eHubClient_Sender { get; set; }
        [ForeignKey("SB_CC_Recipient")]
        public virtual eHubClient eHubClient_Recipient { get; set; }
        #endregion

        #region Default Constructor
        public eHubSubscriptionBroadcaster()
		{

		}
		#endregion
	}
}
