using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.Products.CACustoms.Transformations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.CACustoms.Tests.Transformations
{
	[TestClass]
	public class CanadianCustomsMdn2AcknowledgementReceivedTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CanadianCustomsMdn2AcknowledgementReceived()
		{
			var ctx = InitialiseTestingMessageContext();

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "Transformations.TestFiles.CanadianCustomsMdn.xml";
			string expectedFile = "Transformations.TestFiles.AcknowledgementReceived.xml";
			mapTester.Execute<CanadianCustomsMdn2AcknowledgementReceived>(sourceFile, expectedFile);

			Assert.AreEqual("HYEDAUIKB",
				ctx.Read("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
			Assert.AreEqual("1b876058-dd16-4292-a4ee-b48c0208cdbf",
				ctx.Read("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06"));
		}

		TestingMessageContext InitialiseTestingMessageContext()
		{
			var ctx = new TestingMessageContext();
			ctx.Write("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "");
			ctx.Write("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06", "");
			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(ctx);

			return ctx;
		}
	}
}