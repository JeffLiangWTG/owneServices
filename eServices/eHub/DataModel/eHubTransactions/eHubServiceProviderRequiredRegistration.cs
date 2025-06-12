using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubServiceProviderRequiredRegistration
	{
        [Key, Column(Order = 1)]
		public Guid SX_SP { get; set; }
        [Key, Column(Order = 2)]
		public Guid SX_RT { get; set; }
		public string SX_LookupFactName { get; set; }
		public string SX_QualifierFactName { get; set; }

        [ForeignKey("SX_SP")]
		public virtual eHubServiceProvider eHubServiceProvider { get; set; }
		[ForeignKey("SX_RT")]
		public virtual eHubRegistrationType eHubRegistrationType { get; set; }
	}
}
