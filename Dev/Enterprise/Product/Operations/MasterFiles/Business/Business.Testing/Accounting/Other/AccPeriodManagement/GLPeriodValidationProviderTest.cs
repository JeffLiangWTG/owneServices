using CargoWise.ComponentModel;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GLPeriodValidationProviderTest : PeriodValidationProviderTest
	{
		public override void TestLedgerPeriodValidation()
		{
			using (Header.SuspendValidationTesting())
			{
				Header.AH_PostDate = PeriodHelper.PreviousOpenPeriod.AM_StartDate.AddDays(5);
				ValidationProvider.CheckDateFallsIntoValidPeriod(Header.AH_PostDateInfo);
				Assert("Should be no error on Post Date", !Header.AH_PostDateInfo.HasErrors());

				Header.AH_PostDate = PeriodHelper.PreviousGLClosedPeriod.AM_StartDate.AddDays(5);
				ValidationProvider.CheckDateFallsIntoValidPeriod(Header.AH_PostDateInfo);
				Assert("Should be error on Post Date", Header.AH_PostDateInfo.HasErrors());
				AssertEquals("Sub ledger period closed error", GeneralLedgerPeriodClosedError, Header.AH_PostDateInfo.GetErrors().GetFirstMessage());
			}
		}

		protected override PeriodValidationProvider GetValidationProvider()
		{
			return new GLPeriodValidationProvider(Factory);
		}
	}
}
