using System;
using System.Runtime.Serialization;
using Common.Logging;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService
{
	[DataContract]
	[Serializable]
	public class RoutingEvaluationGenericInput : RoutingEvaluationInput
	{
		[DataMember(IsRequired = true)]
		public string RuleId { get; set; }

		protected override void Validate(StreamingContext context, ILog logger)
		{
			if (string.IsNullOrWhiteSpace(RuleId))
			{
				logger.Error($"RuleId [{RuleId}] is invalid");
				throw new ArgumentException("RuleId can not be null or whitespace");
			}

			base.Validate(context, logger);
		}
	}
}