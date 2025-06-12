using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Common.Logging;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService
{
	[DataContract]
	[Serializable]
	public class RoutingEvaluationInput
	{
		[DataMember]
		public Dictionary<string, string> PropertyFacts { get; set; }

		[DataMember]
		public byte[] Message { get; set; }

		[OnDeserialized]
		internal void OnDeserialized(StreamingContext context)
		{
			var logger = LogManager.GetLogger(typeof(RoutingEvaluationInput));

			Validate(context, logger);
		}

		protected virtual void Validate(StreamingContext context, ILog logger)
		{
			if (PropertyFacts != null)
			{
				foreach (var unit in PropertyFacts)
				{
					if (string.IsNullOrWhiteSpace(unit.Key) || unit.Value == null)
					{
						logger.Error($"PropertyFacts' key [{unit.Key}] or value [{unit.Value}] is invalid");
						throw new ArgumentException("Either propertyFacts' key is not provided or white space or the key's value is null");
					}
				}
			}

			if ((PropertyFacts == null || PropertyFacts.Count == 0) && (Message == null || Message.Length == 0))
			{
				logger.Error("Both propertyFacts and message are unprovided");
				throw new ArgumentException(
					"Both propertyFacts and message are not provided. At lease one of them should be provided");
			}
		}
	}
}