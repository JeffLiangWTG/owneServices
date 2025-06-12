using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubTransformationSet
	{
		#region Fields
		[Key]
		public virtual Guid TS_PK { get; set; }
		public virtual string TS_Name { get; set; }
		public virtual Nullable<Guid> TS_CC_Sender { get; set; }
		public virtual Nullable<Guid> TS_CC_Recipient { get; set; }
		public virtual Nullable<Guid> TS_DT_Source { get; set; }
		public virtual string TS_XPathPredicate { get; set; }
		public virtual string TS_BillingInterfaceName { get; set; }
		public virtual string TS_BillingElement { get; set; }
		public virtual string TS_BillingXPathSource { get; set; }
		public virtual string TS_BillingXPathTarget { get; set; }
		public virtual Nullable<bool> TS_BillSender { get; set; }
		public virtual Nullable<bool> TS_BillRecipient { get; set; }
		public virtual Nullable<Guid> TS_CC_BillOther { get; set; }
		public virtual Nullable<int> TS_BillingNumMessagesIncluded { get; set; }
		public virtual Nullable<decimal> TS_BillingFee { get; set; }
		#endregion

		#region Relationships
		[ForeignKey("TS_CC_Sender")]
		public virtual eHubClient eHubClient_Sender { get; set; }
		[ForeignKey("TS_CC_Recipient")]
		public virtual eHubClient eHubClient_Recipient { get; set; }
		[ForeignKey("TS_DT_Source")]
		public virtual eHubMessageType eHubMessageType { get; set; }
		[ForeignKey("TS_CC_BillOther")]
		public virtual eHubClient eHubClient_BillOther { get; set; }
		[InverseProperty("eHubTransformationSet")]
		public virtual List<eHubCodeSet> eHubCodeSets { get; set; }
		[InverseProperty("eHubTransformationSet")]
		public virtual List<eHubTransformationMapping> eHubTransformationMappings { get; set; }
		[InverseProperty("eHubTransformationSet")]
		public virtual List<eHubInterfaceCounter> eHubInterfaceCounters { get; set; }
		#endregion

		#region Default Constructor
		public eHubTransformationSet()
		{
			eHubCodeSets = new List<eHubCodeSet>();
			eHubTransformationMappings = new List<eHubTransformationMapping>();
		}
		#endregion
	}
}
