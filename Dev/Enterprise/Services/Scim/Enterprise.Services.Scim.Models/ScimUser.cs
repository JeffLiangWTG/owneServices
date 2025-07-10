using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Enterprise.Services.Scim.Models
{
	public class ScimUser : ScimBase
	{
		[JsonProperty(AttributeNames.Active)]
		public bool Active { get; set; }
		[JsonProperty(AttributeNames.UserName)]
		public string UserName { get; set; } = string.Empty;
		[JsonProperty(AttributeNames.NameFormatted)]
		public string NameFormatted { get; set; } = string.Empty;
		[JsonProperty(AttributeNames.FamilyName)]
		public string FamilyName { get; set; } = string.Empty;
		[JsonProperty(AttributeNames.GivenName)]
		public string GivenName { get; set; } = string.Empty;
		[JsonProperty(AttributeNames.MiddleName)]
		public string MiddleName { get; set; } = string.Empty;

		[JsonProperty(AttributeNames.NameHonorificPrefix)]
		public string NameHonorificPrefix { get; set; } = string.Empty;

		[JsonProperty(AttributeNames.NameHonorificSuffix)]
		public string NameHonorificSuffix { get; set; } = string.Empty;

		[JsonProperty(AttributeNames.PhoneNumbersMobile)]
		public string PhoneNumbersMobile { get; set; } = string.Empty;

		[JsonProperty(AttributeNames.PhoneNumbersWork)]
		public string PhoneNumbersWork { get; set; } = string.Empty;

		[JsonProperty(AttributeNames.PhoneNumbersHome)]
		public string PhoneNumbersHome { get; set; } = string.Empty;

		[JsonProperty(AttributeNames.PhoneNumbersFax)]
		public string PhoneNumbersFax { get; set; } = string.Empty;

		[JsonProperty(AttributeNames.AddressesStreetAddress)]
		public string AddressesStreetAddress { get; set; } = string.Empty;

		[JsonProperty(AttributeNames.AddressesLocality)]
		public string AddressesLocality { get; set; } = string.Empty;

		[JsonProperty(AttributeNames.AddressesRegion)]
		public string AddressesRegion { get; set; } = string.Empty;

		[JsonProperty(AttributeNames.AddressesPostalCode)]
		public string AddressesPostalCode { get; set; } = string.Empty;

		[JsonProperty(AttributeNames.AddressesCountry)]
		public string AddressesCountry { get; set; } = string.Empty;

		[JsonProperty(AttributeNames.Title)]
		public string Title { get; set; } = string.Empty;

		[JsonProperty(AttributeNames.Email)]
		public string Email { get; set; } = string.Empty;

		[JsonProperty(AttributeNames.NickName)]
		public string NickName { get; set; } = string.Empty;

		[JsonProperty(AttributeNames.PreferredLanguage)]
		public string PreferredLanguage { get; set; } = string.Empty;

		IEnumerable<GroupMemberWithDisplay> members = Enumerable.Empty<GroupMemberWithDisplay>();
		[JsonProperty(AttributeNames.Groups)]
		public IEnumerable<GroupMemberWithDisplay> Groups
		{
			get => members ?? Enumerable.Empty<GroupMemberWithDisplay>();
			set => members = value;
		}

		[JsonProperty(AttributeNames.HomeBranch)]
		public string HomeBranch { get; set; } = string.Empty;

		[JsonProperty(AttributeNames.HomeDepartment)]
		public string HomeDepartment { get; set; } = string.Empty;

		[JsonProperty(AttributeNames.OtherReferences)]
		public string OtherReferences { get; set; } = string.Empty;
	}
}
