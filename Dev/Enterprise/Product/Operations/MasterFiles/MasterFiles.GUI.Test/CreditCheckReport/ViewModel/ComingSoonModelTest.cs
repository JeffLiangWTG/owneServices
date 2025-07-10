using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	class ComingSoonModelTest : TestCase
	{
		public void TestComingSoonModel()
		{
			var model = new ComingSoonModel("CN");

			AssertEquals("Credit Reports will soon be available for CN organizations", model.ComingSoonString);
		}
	}
}
