using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusHAWBProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestCusHAWBProcessTaskLoadStrategyGetTypeForLoad()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				RunTypeTest<Integration.Customs.AU.ICusHAWBProcessTask>((CusMAWB)Factory.New<Integration.Customs.AU.ICusMAWB>());
				RunTypeTest<Integration.Customs.NZ.ICusHAWBProcessTask>((CusMAWB)Factory.New<Integration.Customs.NZ.ICusMAWB>());
				RunTypeTest<Integration.Customs.GB.CCSUK.ICusHAWBProcessTask>((CusMAWB)Factory.New<Integration.Customs.GB.CCSUK.ICusMAWB>());
			}
		}

		void RunTypeTest<TProcess>(CusMAWB mawb)
		{
			var strategy = new CusHAWBProcessTaskLoadStrategy();
			var expectedType = ObjectFactory.GetType<TProcess>();
			var hawb = mawb.ChildBills.AddNew();
			AssertNull(strategy.GetTypeForLoad(CusHAWBSchema.Constants.Prefix, ZGuid.Empty, Factory));
			AssertNull(strategy.GetTypeForLoad(CusHAWBSchema.Constants.Prefix, ZGuid.Invalid, Factory));
			ErrorReporter.Clear();
			AssertEquals(expectedType, strategy.GetTypeForLoad(hawb.TablePrefix, hawb.PK, Factory));
			AssertNull(strategy.GetTypeForLoad("Z@", hawb.PK, Factory));
			AssertEquals("", ErrorReporter.LastKeyReported);
			mawb = Factory.New<CusMAWB>();
			hawb = mawb.ChildBills.AddNew();
			AssertEquals(typeof(CusHAWBProcessTask), strategy.GetTypeForLoad(hawb.TablePrefix, hawb.PK, Factory));
			Assert(ErrorReporter.LastMessageReported.Contains("CusHAWBProcessTaskLoadStrategy cannot load a CusHAWBProcessTask for this HAWB "));
			ErrorReporter.Clear();
		}
	}
}
