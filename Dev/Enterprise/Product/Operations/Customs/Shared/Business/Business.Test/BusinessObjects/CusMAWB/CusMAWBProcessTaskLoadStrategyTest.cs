using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusMAWBProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestCusMAWBProcessTaskLoadStrategyGetTypeForLoad()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				RunTypeTest<Integration.Customs.AU.ICusMAWBProcessTask>((CusMAWB)Factory.New<Integration.Customs.AU.ICusMAWB>());
				RunTypeTest<Integration.Customs.NZ.ICusMAWBProcessTask>((CusMAWB)Factory.New<Integration.Customs.NZ.ICusMAWB>());
				RunTypeTest<Integration.Customs.GB.CCSUK.ICusMAWBProcessTask>((CusMAWB)Factory.New<Integration.Customs.GB.CCSUK.ICusMAWB>());
			}
		}

		void RunTypeTest<TProcess>(CusMAWB mawb)
		{
			var strategy = new CusMAWBProcessTaskLoadStrategy();
			var expectedType = ObjectFactory.GetType<TProcess>();
			AssertNull(strategy.GetTypeForLoad(CusMAWBSchema.Constants.Prefix, ZGuid.Empty, Factory));
			AssertNull(strategy.GetTypeForLoad(CusMAWBSchema.Constants.Prefix, ZGuid.Invalid, Factory));
			ErrorReporter.Clear();
			AssertEquals(expectedType, strategy.GetTypeForLoad(mawb.TablePrefix, mawb.PK, Factory));
			AssertNull(strategy.GetTypeForLoad("Z@", mawb.PK, Factory));
			AssertEquals("", ErrorReporter.LastKeyReported);
			mawb = Factory.New<CusMAWB>();
			AssertEquals(typeof(CusMAWBProcessTask), strategy.GetTypeForLoad(mawb.TablePrefix, mawb.PK, Factory));
			AssertEquals("CusMAWBProcessTaskLoadStrategy cannot load a CusMAWBProcessTask for this MAWB as the country is not recognised. Add a case for your country", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
