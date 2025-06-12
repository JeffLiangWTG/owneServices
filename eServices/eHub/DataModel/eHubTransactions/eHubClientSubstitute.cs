using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubClientSubstitute
	{
		[Key]
		[Column(Order = 1)]
		public string CS_OldeHubID { get; set; }

		public string CS_NeweHubID { get; set; }

		public DateTime CS_StartUTC { get; set; }
	}
}
