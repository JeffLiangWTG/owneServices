using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusOutturnHeaderProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var strategy = new CusOutturnHeaderProcessTaskLoadStrategy();
			AssertNull(strategy.GetTypeForLoad(CusOutturnHeaderSchema.Constants.Prefix, ZGuid.Empty, Factory));
			AssertNull(strategy.GetTypeForLoad(CusOutturnHeaderSchema.Constants.Prefix, ZGuid.Invalid, Factory));
			AssertNull(strategy.GetTypeForLoad("Z@", Factory.New<CusOutturnHeader>().PK, Factory));

			ErrorReporter.Clear();
			var auOutturnHeader = Factory.New<Integration.Customs.AU.ICusOutturnHeader>();
			var auProcessTaskType = ObjectFactory.GetType<Integration.Customs.AU.ICusOutturnHeaderProcessTask>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals(auProcessTaskType, strategy.GetTypeForLoad(CusOutturnHeaderSchema.Constants.Prefix, auOutturnHeader.PK, Factory));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				AssertEquals(auProcessTaskType, strategy.GetTypeForLoad(CusOutturnHeaderSchema.Constants.Prefix, auOutturnHeader.PK, Factory));
			}

			AssertEquals("", ErrorReporter.LastKeyReported);
		}
	}
}
