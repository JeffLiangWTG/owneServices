using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.CNCustoms.Transforms.DecResult2UniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.CNCustoms.Transforms.UniversalShipment2DecMessage;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.Core.Transforms.Helper;
using System.Collections.Generic;
using Rhino.Mocks;

namespace Tests
{
	[TestClass]
	public class DecMessage2ZippedMessageTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DecMessage2ZippedMessage_Test()
		{
			string sourceFile = "DecMessage2ZippedMessage_Input.DecMessage2ZippedMessage_Input.xml";
			string outputFile = "DecMessage2ZippedMessage_Output.DecMessage2ZippedMessage_Output.xml";

			var ctx = InitialiseTestingMessageContext();
			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), ComparerToExcludeDateTimeNodes);
			mapTester.Execute<DecMessage2ZippedMessage>(sourceFile, outputFile);
		}

		static TestingMessageContext InitialiseTestingMessageContext()
		{
			var bizTlkPropertiesNs = "http://schemas.microsoft.com/BizTalk/2003/system-properties";
			var ctx = new TestingMessageContext();
			ctx.Write("SourceParty", bizTlkPropertiesNs, "Test11223");
			ctx.Write("DestinationParty", bizTlkPropertiesNs, "Test11223");
			ctx.Write("DestinationPartyQualifier", bizTlkPropertiesNs, "");
			ctx.Write("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06", "");
			ctx.Write("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "000000000000210183_201904041627338935104");
			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(ctx);

			return ctx;
		}

		ICompare ComparerToExcludeDateTimeNodes
		{
			get
			{
				if (comparer == null)
				{
					comparer = new ExcludingComparer(new List<string>() { "/*[local-name()='GenericMessageInterchange']/*[local-name()='Body']/*[local-name()='ZippedMessage']" });
				}

				return comparer;
			}
		}

		ICompare comparer;
	}
}
