using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubServiceOperatorRegistration
	{
		#region Fields
		[Key, Column(Order = 1)]
		public Guid SR_SO { get; set; }
		[Key, Column(Order = 2)]
		public Guid SR_RT { get; set; }
		public string SR_Code { get; set; } 
		#endregion

		#region Relationships
		[ForeignKey("SR_SO")]
		public virtual eHubServiceOperator eHubServiceOperator { get; set; }
		[ForeignKey("SR_RT")]
		public virtual eHubRegistrationType eHubRegistrationType { get; set; } 
		#endregion

		#region Default Constructor
		public eHubServiceOperatorRegistration()
		{
			SR_Code = String.Empty;
		}
		#endregion
	}
}
