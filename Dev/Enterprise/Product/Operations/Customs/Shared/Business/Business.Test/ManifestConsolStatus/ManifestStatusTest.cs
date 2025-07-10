using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ManifestStatusTest : TestCase
	{
		public void TestPartiallyCleared()
		{
			AssertEquals(ManifestStatus.PartiallyCleared.AsString, "Partially Cleared");
		}
	}
}
