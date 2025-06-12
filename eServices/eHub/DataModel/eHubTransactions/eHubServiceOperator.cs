using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubServiceOperator
	{
		#region Fields
		[Key]
		public Guid SO_PK { get; set; }
		public string SO_ID { get; set; }
		public string SO_Name { get; set; } 
		#endregion

		#region Relationships
		[InverseProperty("eHubServiceOperator")]
		public virtual List<eHubServiceOperatorRegistration> eHubServiceOperatorRegistrations { get; set; } 
		#endregion

		#region Default Constructor
		public eHubServiceOperator()
		{
			SO_PK = Guid.NewGuid();
			SO_ID = String.Empty;
			SO_Name = String.Empty;
			eHubServiceOperatorRegistrations = new List<eHubServiceOperatorRegistration>();
		} 
		#endregion
	}
}
