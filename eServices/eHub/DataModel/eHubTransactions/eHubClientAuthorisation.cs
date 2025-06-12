using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubClientAuthorisation
	{
		#region Fields

		[Key, Column(Order = 1)]
		public virtual Guid CA_CC_Sender { get; set; }
		[Key, Column(Order = 2)]
		public virtual Guid CA_CC_Recipient { get; set; }
		public DateTime CA_CreatedUTC { get; set; }

		#endregion

		#region Relationships

		[ForeignKey("CA_CC_Sender")]
		public virtual eHubClient eHubClient_Sender { get; set; }
		[ForeignKey("CA_CC_Recipient")]
		public virtual eHubClient eHubClient_Recipient { get; set; }

		#endregion

		#region DefaultConstructor

		public eHubClientAuthorisation() { }

		#endregion
	}
}
