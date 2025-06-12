using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubZone
	{
		#region Fields
		[Key]
		public Guid ZZ_PK { get; set; }
		public string ZZ_ID { get; set; } 
		#endregion

		#region Relationships
		[InverseProperty("eHubZone")]
		public virtual List<eHubClient> eHubClients { get; set; } 
		#endregion

		#region Default Constructor
		public eHubZone()
		{
			ZZ_PK = Guid.NewGuid();
			eHubClients = new List<eHubClient>();
		} 
		#endregion
	}
}
