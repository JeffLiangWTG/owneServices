using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService.Tests
{
	[TestFixture]
	public class RoutingEvaluationGenericInputTests
	{
		[Test]
		public void TestSetRuleIdSet_RuleIdIsWhiteSpace_ThrowArgumentException()
		{
			var routingEvaluationInput = new RoutingEvaluationGenericInput()
			{
				RuleId = ""
			};
			
			Assert.That(() => SerializeAndDeserialize(routingEvaluationInput), Throws.ArgumentException, "RuleId can not be null or whitespace");
		}

		[Test]
		public void TestOnDeserialized_PropertyFactsKeyOrValueInvalid_ThrowArgumentException()
		{
			var propertyFacts = new Dictionary<string, string>();
			propertyFacts.Add("MessageType", "messageType");

			var routingEvaluationInput = new RoutingEvaluationGenericInput
			{
				RuleId = "GLB_ELEC_INVOICING",
				PropertyFacts = propertyFacts,
				Message = new byte[0]
			};

			Assert.That(() => SerializeAndDeserialize(routingEvaluationInput), Throws.ArgumentException, "Either propertyFacts' key is not provided or white space or the key's value is null");
		}

		[Test]
		public void TestOnDeserialized_BothPropertyFactsAndMessageAreNotProvided_ThrowArgumentException()
		{
			var routingEvaluationInput = new RoutingEvaluationGenericInput
			{
				RuleId = "GLB_ELEC_INVOICING",
				PropertyFacts = new Dictionary<string, string> { { "", "" } },
				Message = new byte[0]
			};

			Assert.That(() => SerializeAndDeserialize(routingEvaluationInput), Throws.ArgumentException, "Both propertyFacts and message are not provided. At lease one of them should be provided");
		}

		public void SerializeAndDeserialize(RoutingEvaluationGenericInput routingEvaluationInput)
		{
			using (var memoryStream = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(memoryStream, routingEvaluationInput);

				memoryStream.Seek(0, SeekOrigin.Begin);

				formatter.Deserialize(memoryStream);
			}
		}
	}
}