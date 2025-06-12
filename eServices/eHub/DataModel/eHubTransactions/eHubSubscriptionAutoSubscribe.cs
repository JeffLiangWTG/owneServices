using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubSubscriptionAutoSubscribe
	{
		#region Fields
		[Key]
		public virtual Guid SA_PK { get; set; }
		public virtual Guid SA_ST { get; set; }
		public virtual Nullable<Guid> SA_CC_Recipient { get; set; }
		public virtual Nullable<Guid> SA_DT { get; set; }
		public virtual string SA_ValueXpath { get; set; }
		public virtual string SA_ValueProperty { get; set; }
		public virtual string SA_ReferenceXpath { get; set; }
		public virtual string SA_ReferenceProperty { get; set; }
		#endregion

		#region Relationships
		[ForeignKey("SA_ST")]
		public virtual eHubSubscriptionType eHubSubscriptionType { get; set; }
		[ForeignKey("SA_CC_Recipient")]
		public virtual eHubClient eHubClient_Recipient { get; set; }
		[ForeignKey("SA_DT")]
		public virtual eHubMessageType eHubMessageType { get; set; }
		#endregion

		#region Default Constructor
		public eHubSubscriptionAutoSubscribe()
		{

		}
		#endregion
	}
}
