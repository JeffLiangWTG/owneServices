using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HLoadingLocationTest : TestCaseWithFactory
	{
		public void TestLoadingLocationProperties()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_RL_NKPortOfLoading = "TWKEL";
			bill.ABL_LocationInformation = "DESC Test";

			ILocation loadingLocation = new N5101HLoadingLocation(bill);
			CombineAssertions("LoadingLocationProperties", () =>
			{
				AssertEquals("TWKEL", loadingLocation.ID);
				AssertEquals("DESC Test", loadingLocation.Name);
				AssertEquals(ZDate.Empty, loadingLocation.LoadingDateTime);
				AssertEquals(ZString.Empty, loadingLocation.EstimatedLoadingCode);
			});
		}
	}
}
