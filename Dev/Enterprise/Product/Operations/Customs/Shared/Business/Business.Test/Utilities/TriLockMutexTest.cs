using System.Threading;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Business.Testing
{
	sealed class TriLockMutexTest : TestCaseWithFactory
	{
		public void TestMutexLocksOnThirdAttempt()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				Factory.Save();

				using (var consolMutex1 = BaseCusSCAOceanBill.CreateMutexForConsol(consol.PK, Core.Constants.CountryCodes.UnitedStates))
				using (var consolMutex2 = BaseCusSCAOceanBill.CreateMutexForConsol(consol.PK, Core.Constants.CountryCodes.UnitedStates))
				{
					consolMutex1.Lock();
					Assert("Cannot lock mutex 2 when mutex 1 is locked", !consolMutex2.Lock());

					consolMutex2.OnLockFailed += (object sender, ProgressEventArgs e) =>
					{
						if (e.CurrentProgress == 2)
						{
							consolMutex1.Unlock();

							// Allow the DB update to complete.
							Thread.Sleep(2000);
						}
					};

					Assert("Can lock mutex 2 when mutex 1 unlocks before third attempt", consolMutex2.Lock());
				}
			}
		}
	}
}
