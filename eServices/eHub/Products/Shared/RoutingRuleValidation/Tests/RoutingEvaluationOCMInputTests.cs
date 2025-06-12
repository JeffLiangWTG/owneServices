using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Common.Logging;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService.Tests
{
	[TestFixture]
	public class RoutingEvaluationOCMInputTests
	{
		[Test]
		public void TestAssignSourcePartyIfNotExists_PropertyFactsHasSourceParty_ReturnSourceParty()
		{
			var propertyFacts = new Dictionary<string, string> {{"SourceParty", "HYEJEYABC"}};
			var msgAsByteArray = GetMessageFromFile("SqlMessage_eManifest.txt");
			var routingEvaluationOCMInput = new RoutingEvaluationOCMInput
			{
				PropertyFacts = propertyFacts,
				Message = msgAsByteArray
			};

			routingEvaluationOCMInput.AssignSourcePartyIfNotExists();
			StringAssert.AreEqualIgnoringCase("HYEJEYABC", propertyFacts["SourceParty"] );
		}

		[Test]
		public void TestAssignSourcePartyIfNotExists_PropertyFactsDoesNotHaveSourceParty_AssignItBySearchingFromMsg()
		{
			var msgAsByteArray = GetMessageFromFile("SqlMessage_eManifest.txt");
			var routingEvaluationOCMInput = new RoutingEvaluationOCMInput
			{
				Message = msgAsByteArray
			};

			routingEvaluationOCMInput.AssignSourcePartyIfNotExists();
			StringAssert.AreEqualIgnoringCase("HYEDCNUAT", routingEvaluationOCMInput.PropertyFacts["SourceParty"]);
		}

		[Test]
		public void TestValidate_NoSourcePartyNeitherPropertyFactsNorMessage_ExceptionThrown()
		{
			var msgAsByteArray = GetMessageFromFile("SqlMessage_eManifest_NoSourceParty.txt");
			var routingEvaluationOCMInput = new RoutingEvaluationOCMInputForTest()
			{
				Message = msgAsByteArray
			};

			Assert.That(() => routingEvaluationOCMInput.RunValidate(new StreamingContext(), MockRepository.GenerateMock<ILog>()), Throws.ArgumentException, "SourceParty was not provided with neither propertyFacts nor message");
		}

		public byte[] GetMessageFromFile(string fileName)
		{
			var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames()
				.Single(str => str.EndsWith(fileName));

			string message;

			using (var stream = GetType().Assembly.GetManifestResourceStream(resourceName))
			{
				using (var reader = new StreamReader(stream))
				{
					message = reader.ReadToEnd();
				}
			}

			return Encoding.UTF8.GetBytes(message);
		}
	}

	public class RoutingEvaluationOCMInputForTest : RoutingEvaluationOCMInput
	{
		public void RunValidate(StreamingContext context, ILog logger)
		{
			base.Validate(context, logger);
		}
	}
}