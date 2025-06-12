using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Xml.XPath;
using Common.Logging;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService
{
	public class RoutingEvaluationOCMInput : RoutingEvaluationInput
	{
		protected override void Validate(StreamingContext context, ILog logger)
		{
			base.Validate(context, logger);

			AssignSourcePartyIfNotExists();

			if (string.IsNullOrWhiteSpace(PropertyFacts?["SourceParty"]))
			{
				logger.Error("SourceParty was not provided with neither propertyFacts nor message");
				throw new ArgumentException("SourceParty was not provided with neither propertyFacts nor message");
			}
		}

		internal void AssignSourcePartyIfNotExists()
		{
			if (PropertyFacts != null && PropertyFacts.ContainsKey("SourceParty")) return;

			string sourceParty;
			using (var ms = new MemoryStream(Message))
			{
				var xpDoc = XDocument.Load(ms);
				sourceParty = xpDoc.XPathSelectElement(
						"//*[local-name()='UniversalInterchange']/*[local-name()='Header']/*[local-name()='SenderID']")?.Value;
			}

			if (PropertyFacts == null)
			{
				PropertyFacts = new Dictionary<string, string>();
			}

			PropertyFacts.Add("SourceParty", sourceParty);
		}
	}
}