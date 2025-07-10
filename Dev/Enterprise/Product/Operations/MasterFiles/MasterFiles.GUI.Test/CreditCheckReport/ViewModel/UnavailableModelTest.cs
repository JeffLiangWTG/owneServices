using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	class UnavailableModelTest : TestCase
	{
		public void TestUnavailableModel()
		{
			var model = new UnavailableModel();

			AssertEquals("Credit Reports are unavailable. No organization was selected.", model.ComingSoonString);
		}
	}
}
