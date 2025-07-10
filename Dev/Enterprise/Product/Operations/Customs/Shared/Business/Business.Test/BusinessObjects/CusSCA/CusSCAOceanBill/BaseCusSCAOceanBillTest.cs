using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	internal class BaseCusSCAOceanBillTest : TestCaseWithFactory
	{
		public void TestJobNumberShouldReturnCB_MessageReference()
		{
			var bill = Factory.New<TestCusSCAOceanBill>();
			bill.CB_MessageReference = ZString.Empty;
			AssertEquals(ZString.Empty, bill.JobNumber);
			bill.CB_MessageReference = "X00001234";
			AssertEquals("X00001234", bill.JobNumber);
		}

		public void TestCreateMutexForConsol()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				Factory.Save();

				using (var consolMutex1 = BaseCusSCAOceanBill.CreateMutexForConsol(consol.PK, Core.Constants.CountryCodes.UnitedStates))
				using (var consolMutex2 = BaseCusSCAOceanBill.CreateMutexForConsol(consol.PK, Core.Constants.CountryCodes.UnitedStates))
				using (var consolMutex3 = BaseCusSCAOceanBill.CreateMutexForConsol(consol.PK, Core.Constants.CountryCodes.Australia))
				{
					consolMutex1.Lock();
					Assert("Cannot lock US mutex 2 when US mutex 1 is locked", !consolMutex2.Lock());
					Assert("Can lock AU mutex when US mutex 1 is locked", consolMutex3.Lock());
					consolMutex3.Unlock();
					consolMutex1.Unlock();
					Assert("Can lock US mutex 2 when US mutex 1 is unlocked", consolMutex2.Lock());
				}
			}
		}
	}
}
