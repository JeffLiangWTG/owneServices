using Newtonsoft.Json;
using SimpleIdServer.Scim.Domains;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(SCIMResourceTypes))]

namespace Enterprise.Services.Scim.Models
{
	public class GroupMember
	{
		[JsonProperty(AttributeNames.Value)]
		public string Value { get; set; }

		string type;
		[JsonProperty(AttributeNames.Type)]
		public string Type
		{
			get
			{
				return !string.IsNullOrEmpty(type)
					? type
					: !string.IsNullOrEmpty(Ref)
						? Ref.ToLowerInvariant().Contains("/groups/")
							? SCIMResourceTypes.Group
							: SCIMResourceTypes.User
						: string.Empty;
			}
			set
			{
				type = value;
			}
		}

		[JsonProperty(AttributeNames.Ref)]
		public string Ref { get; set; }
	}

	public class GroupMemberWithDisplay : GroupMember
	{
		[JsonProperty(AttributeNames.Display)]
		public string Display { get; set; }
	}
}
