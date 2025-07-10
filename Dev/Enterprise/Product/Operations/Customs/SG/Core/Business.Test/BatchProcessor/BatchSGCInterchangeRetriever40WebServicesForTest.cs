using System.Threading;
using CargoWise.Types;
using Enterprise.Customs.SG.MHUB.Mhx4Soap;
using Enterprise.Customs.SG.Registry;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor.Testing
{
	sealed class BatchSGCInterchangeRetriever40WebServicesForTest : BatchSGCInterchangeRetriever40WebServices
	{
		protected override MHAccessClient GetIgorsClient(SGGlbStaffWrapper brokerWrapper, ZString userId)
		{
			return new SG.MHUB.Mhx4Soap.Testing.MHAccessClientForTest("djc", "xyz", new MHUBSettingsProvider(), Logger, pukeOnLoginForTest: true);
		}

		public void LoginAndRetrieveAndLogoutExposed() => base.LoginAndRetrieveAndLogout(null, CancellationToken.None);
	}
}
