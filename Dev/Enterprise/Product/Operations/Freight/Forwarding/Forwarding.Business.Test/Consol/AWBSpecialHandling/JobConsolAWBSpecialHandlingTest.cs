using Enterprise.Core;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(JobConsolAWBSpecialHandling))]
	internal abstract class JobConsolAWBSpecialHandlingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var specialHandlingItem = consol.AWBSpecialHandlingItems.AddNew();
			AssertEquals("Human readable name", "Special Handling", specialHandlingItem.HumanReadableName);
		}
	}
}
