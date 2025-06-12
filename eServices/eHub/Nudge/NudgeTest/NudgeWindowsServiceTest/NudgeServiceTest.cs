using NUnit.Framework;
using CargoWise.eHub.Nudge;

namespace CargoWise.eHub.Nudge.Tests
{
	[TestFixture]
	public class NudgeServiceTest
	{
		[Test]
		public void TestServiceInitialisation()
		{
			var service = new NudgeService();
			Assert.AreEqual("eHub - Nudge Service", service.ServiceName);
		}
	}
}
