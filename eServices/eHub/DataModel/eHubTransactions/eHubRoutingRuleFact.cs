using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubRoutingRuleFact
	{
		#region Fields
		[Key]
		public Guid RX_PK { get; set; }
		public Guid RX_RR { get; set; }
		public string RX_Name { get; set; }
		public string RX_Type { get; set; }
		public string RX_Query { get; set; }
		public Guid? RX_RR_ComputeRule { get; set; }
		#endregion

		#region Relationships
		[ForeignKey("RX_RR")]
		public virtual eHubRoutingRule eHubRoutingRule { get; set; }
		[ForeignKey("RX_RR_ComputeRule")]
		public virtual eHubRoutingRule eHubRoutingRule_Compute { get; set; }
		#endregion

		#region Default Constructor
		public eHubRoutingRuleFact()
		{
			RX_PK = Guid.NewGuid();
		} 
		#endregion
	}
}
