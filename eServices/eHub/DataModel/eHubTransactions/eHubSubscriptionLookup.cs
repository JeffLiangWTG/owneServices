using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubSubscriptionLookup
	{
		#region Fields
		[Key]
		public virtual Guid SL_PK { get; set; }
		public virtual Guid SL_ST { get; set; }
		public virtual Guid SL_DT { get; set; }
		public virtual string SL_ValueXpath { get; set; }
		public virtual string SL_ValueProperty { get; set; }
		#endregion

		#region Relationships
		[ForeignKey("SL_ST")]
		public virtual eHubSubscriptionType eHubSubscriptionType { get; set; }
		[ForeignKey("SL_DT")]
		public virtual eHubMessageType eHubMessageType { get; set; }
		#endregion

		#region Default Constructor
		public eHubSubscriptionLookup()
		{

		}
		#endregion
	}
}
