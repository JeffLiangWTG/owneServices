using System.Runtime.Serialization;
using eHubRoutingRuleEngine = eServices.eHubRoutingRuleEngine;

namespace CargoWise.eHub.Shared.RoutingRuleEngine
{
	[DataContract]
	public class Result
	{
		[DataMember]
		public string ErrorCode { get; set; }
		[DataMember]
		public string ErrorDescription { get; set; }
		[DataMember]
		public string Value { get; set; }
		[DataMember]
		public string RecipientId { get; set; }

		public static implicit operator Result(eHubRoutingRuleEngine.Result result)
			=> result is null ? null : new Result
			{
				ErrorCode = result.ErrorCode,
				ErrorDescription = result.ErrorDescription,
				Value = result.Value,
				RecipientId = result.RecipientId
			};
	}
}
