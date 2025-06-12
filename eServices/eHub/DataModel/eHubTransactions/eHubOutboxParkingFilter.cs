using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubOutboxParkingFilter
	{
		[Key]
		[Column(Order = 1)]
		public Guid OP_OI_CC_Sender { get; set; }
		[Key]
		[Column(Order = 2)]
		public Guid OP_OI_CC_Recipient { get; set; }
	}
}
