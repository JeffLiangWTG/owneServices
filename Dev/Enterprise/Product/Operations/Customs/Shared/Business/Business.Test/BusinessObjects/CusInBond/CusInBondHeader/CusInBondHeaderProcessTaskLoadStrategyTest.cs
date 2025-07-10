using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusInBondHeaderProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				var strategy = new CusInBondHeaderProcessTaskLoadStrategy();
				var expectedType = ObjectFactory.GetType<Integration.Customs.US.eManifest.ICusInBondHeaderProcessTask>();
				var header = (BusinessObject)Factory.New<Integration.Customs.US.eManifest.ICusInBondHeader>();
				AssertNull(strategy.GetTypeForLoad(CusInBondHeaderSchema.Constants.Prefix, ZGuid.Empty, Factory));
				AssertNull(strategy.GetTypeForLoad(CusInBondHeaderSchema.Constants.Prefix, ZGuid.Invalid, Factory));
				AssertEquals(expectedType, strategy.GetTypeForLoad(header.TablePrefix, header.PK, Factory));
				AssertNull(strategy.GetTypeForLoad("Z@", header.PK, Factory));

				header = (BusinessObject)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
				AssertEquals(ObjectFactory.GetType<Integration.Customs.US.InBond.ICusInBondHeaderProcessTask>(), strategy.GetTypeForLoad(header.TablePrefix, header.PK, Factory));
				header = (BusinessObject)Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
				AssertEquals(ObjectFactory.GetType<Integration.Customs.US.USAMS.ICusInBondHeaderProcessTask>(), strategy.GetTypeForLoad(header.TablePrefix, header.PK, Factory));
			}
		}
	}
}
