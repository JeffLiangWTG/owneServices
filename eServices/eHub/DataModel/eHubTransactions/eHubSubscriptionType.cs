using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubSubscriptionType
	{
		#region Fields
		[Key]
		public virtual Guid ST_PK { get; set; }
		public virtual string ST_ID { get; set; }
		public virtual string ST_Name { get; set; }
		public virtual Nullable<int> ST_ExpiryDays { get; set; }
		#endregion

		#region Relationships
		[InverseProperty("eHubSubscriptionType")]
		public virtual List<eHubSubscriptionAutoSubscribe> eHubSubscriptionAutoSubscribes { get; set; }
		[InverseProperty("eHubSubscriptionType")]
		public virtual List<eHubSubscriptionLookup> eHubSubscriptionLookups { get; set; }
		[InverseProperty("eHubSubscriptionType")]
		public virtual List<eHubSubscriptionValue> eHubSubscriptionValues { get; set; }
		[InverseProperty("eHubSubscriptionType")]
		public virtual List<eHubSubscriptionBroadcaster> eHubSubscriptionBroadcasters { get; set; }
		#endregion

		#region Default Constructor
		public eHubSubscriptionType()
		{
			eHubSubscriptionAutoSubscribes = new List<eHubSubscriptionAutoSubscribe>();
			eHubSubscriptionLookups = new List<eHubSubscriptionLookup>();
			eHubSubscriptionValues = new List<eHubSubscriptionValue>();
			eHubSubscriptionBroadcasters = new List<eHubSubscriptionBroadcaster>();
		}
		#endregion
	}
}
