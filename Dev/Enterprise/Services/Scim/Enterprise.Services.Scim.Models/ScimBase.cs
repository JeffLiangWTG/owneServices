using System;
using Newtonsoft.Json;

namespace Enterprise.Services.Scim.Models
{
	public class ScimBase
	{
		[JsonProperty(AttributeNames.Id)]
		public Guid Id { get; set; }
		[JsonProperty(AttributeNames.ExternalId)]
		public string ExternalId { get; set; }
		public string Created { get; set; }
		public string LastModified { get; set; }
	}
}
