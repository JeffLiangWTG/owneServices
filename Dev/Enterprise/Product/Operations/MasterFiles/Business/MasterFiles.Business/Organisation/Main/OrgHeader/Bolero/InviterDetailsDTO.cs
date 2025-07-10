using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Newtonsoft.Json;

namespace Enterprise.MasterFiles.Business
{
	public class InviterDetailsDTO
	{
		public InviterDetailsDTO(BoleroInvitationDetails details)
		{
			var productRegistration = ObjectFactory.Get<IProductRegistration>();
			var registrationKey = productRegistration.Key;
			var names = details.YourName.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			var lastName = names.Length > 1 ? names.Last() : string.Empty;
			var firstName = names.Length > 1 ? string.Join(" ", names.Take(names.Length - 1).ToArray()) : details.YourName.ToString();
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy ?? GlbCompany.CurrentCompany.OrgProxy;

			InviterEnterpriseId = registrationKey.EnterpriseCode;
			InviterServerId = registrationKey.ServerCode;
			InviterOrgCode = orgProxy.PK;
			InviterOrgName = orgProxy.OH_FullName;
			InviterFirstName = firstName;
			InviterLastName = lastName;
			InviterEmail = details.YourEmail.ToString();
			InviterMessage = details.YourMessage.ToString();
		}

		[JsonProperty("inviterEnterpriseId")]
		[System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Missing Field")]
		public string InviterEnterpriseId { get; set; }

		[JsonProperty("inviterServerId")]
		[System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Missing Field")]
		public string InviterServerId { get; set; }

		[JsonProperty("inviterOrgCode")]
		[System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Missing Field")]
		public ZGuid InviterOrgCode { get; set; }

		[JsonProperty("inviterOrgName")]
		[System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Missing Field")]
		public string InviterOrgName { get; set; }

		[JsonProperty("inviterFirstName")]
		[System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Missing Field")]
		public string InviterFirstName { get; set; }

		[JsonProperty("inviterLastName")]
		public string InviterLastName { get; set; }

		[JsonProperty("inviterEmail")]
		[System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Missing Field")]
		public string InviterEmail { get; set; }

		[JsonProperty("inviterMessage")]
		[System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Missing Field")]
		public string InviterMessage { get; set; }
	}
}
