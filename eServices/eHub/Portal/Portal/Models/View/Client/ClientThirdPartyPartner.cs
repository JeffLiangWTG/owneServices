using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CargoWise.eHub.Portal.Models.View
{
	[Serializable]
	public class ClientThirdPartyPartner
	{
		[DisplayName("eHub Client Id")]
		[Required]
		[StringLength(36)]
		public string Id { get; set; }
		
		[Required]
		[StringLength(36)]
		public string Password { get; set; }
		
		[DisplayName("Organisation Code (ediProd)")]
		[Required]
		[StringLength(12)]
		public string OrgCode { get; set; }
		
		[DisplayName("Support Email")]
		[Required]
		[StringLength(128)]
		public string Email { get; set; }
	}
}