using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReleaseOriginCodeListTest : TestCase
	{
		public void TestProperties()
		{
			AssertEquals(false, ReleaseOriginCodeList.ShouldRemoveReleaseDate(ReleaseOriginCodeList.Codes.CBPManifestHoldRemoved));
			AssertEquals(true, ReleaseOriginCodeList.ShouldRemoveReleaseDate(ReleaseOriginCodeList.Codes.ReleaseDateRemoved));
		}
	}
}
