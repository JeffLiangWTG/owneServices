using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubRegistrationType
	{
		#region Fields
		[Key]
		public Guid RT_PK { get; set; }
		public string RT_ID { get; set; }
		public string RT_Description { get; set; }
		public string RT_RegistrantType { get; set; }
		[StringLength(3)]
		public string RT_OW { get; set; }
		#endregion

		#region Relationships
		[ForeignKey("RT_OW")]
		public virtual eHubOwner eHubOwner { get; set; }
		[InverseProperty("eHubRegistrationType")]
		public virtual List<eHubClientRegistration> eHubClientRegistrations { get; set; }
		[InverseProperty("eHubRegistrationType")]
		public virtual List<eHubClientSystemRegistration> eHubClientSystemRegistrations { get; set; }
		[InverseProperty("eHubRegistrationType")]
		public virtual List<eHubServiceOperatorRegistration> eHubServiceOperatorRegistrations { get; set; }
		[InverseProperty("eHubRegistrationType")]
		public virtual List<eHubServiceProviderRequiredRegistration> eHubServiceProviderRequiredRegistrations { get; set; }
		[InverseProperty("eHubRegistrationType")]
		public virtual List<eHubAsyncPollingRegistration> eHubAsyncPollingRegistrations { get; set; }

		#endregion

		#region DefaultConstructor
		public eHubRegistrationType()
		{
			RT_PK = Guid.NewGuid();
			RT_ID = String.Empty;
			RT_Description = String.Empty;
			RT_RegistrantType = String.Empty;
			eHubClientRegistrations = new List<eHubClientRegistration>();
			eHubClientSystemRegistrations = new List<eHubClientSystemRegistration>();
			eHubServiceOperatorRegistrations = new List<eHubServiceOperatorRegistration>();
			eHubServiceProviderRequiredRegistrations = new List<eHubServiceProviderRequiredRegistration>();
		} 
		#endregion
	}
}
