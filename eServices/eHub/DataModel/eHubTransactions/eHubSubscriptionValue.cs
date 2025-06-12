using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubSubscriptionValue
	{
		#region Fields
		[Key]
		public virtual Guid SV_PK { get; set; }
		public virtual Guid SV_ST { get; set; }
		public virtual Guid SV_CC_Sender { get; set; }
		public virtual string SV_Value { get; set; }
		public virtual string SV_Reference { get; set; }
		public virtual DateTime SV_SubscribedUTC { get; set; }
		public virtual Nullable<DateTime> SV_ExpiryUTC { get; set; }
		public virtual Guid SV_CC_Recipient { get; set; }
		public virtual string SV_ReferenceType { get; set; }
		#endregion

		#region Relationships
		[ForeignKey("SV_CC_Sender")]
		public virtual eHubClient eHubClient_Provider { get; set; }
		[ForeignKey("SV_CC_Recipient")]
		public virtual eHubClient eHubClient_Subscriber { get; set; }
		[ForeignKey("SV_ST")]
		public virtual eHubSubscriptionType eHubSubscriptionType { get; set; }
		#endregion

		#region Default Constructor
		public eHubSubscriptionValue()
		{
		}
		#endregion
	}
}
