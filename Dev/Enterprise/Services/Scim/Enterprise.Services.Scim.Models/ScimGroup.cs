using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Enterprise.Services.Scim.Models
{
	public class ScimGroup : ScimBase
	{
		[JsonProperty(AttributeNames.DisplayName)]
		public string DisplayName { get; set; }

		[JsonProperty(AttributeNames.Category)]
		public string Category { get; set; }

		IEnumerable<GroupMember> members = Enumerable.Empty<GroupMember>();
		[JsonProperty(AttributeNames.Members)]
		public IEnumerable<GroupMember> Members
		{
			get => members ?? Enumerable.Empty<GroupMember>();
			set => members = value;
		}
	}
}
