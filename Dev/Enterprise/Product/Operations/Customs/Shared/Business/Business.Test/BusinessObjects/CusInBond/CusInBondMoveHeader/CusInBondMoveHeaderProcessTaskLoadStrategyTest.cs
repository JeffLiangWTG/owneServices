using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusInBondMoveHeaderProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var strategy = new CusInBondHeaderProcessTaskLoadStrategy();
				var expectedType = ObjectFactory.GetType<Integration.Customs.EU.NCTS.INctsHeaderProcessTask>();
				var header = (CusInBondHeader)Factory.New<Integration.Customs.EU.NCTS.ICusInBondHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertNull(strategy.GetTypeForLoad(CusInBondMoveHeaderSchema.Constants.Prefix, ZGuid.Empty, Factory));
				AssertNull(strategy.GetTypeForLoad(CusInBondMoveHeaderSchema.Constants.Prefix, ZGuid.Invalid, Factory));
				AssertEquals(expectedType, strategy.GetTypeForLoad(header.TablePrefix, header.PK, Factory));
				AssertNull(strategy.GetTypeForLoad("Z@", header.PK, Factory));

				var strategy2 = new CusInBondMoveHeaderProcessTaskLoadStrategy();
				var ariivalMoveHeader = (CusInBondMoveHeader)Factory.New<Integration.Customs.EU.NCTS.IArrivalMovementHeader>();
				ariivalMoveHeader.BM_BH = header.PK;
				ariivalMoveHeader.BM_SubApplicationCode = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes.NctsArrivalNotification.Substring(0, 1);
				AssertEquals(ObjectFactory.GetType<Integration.Customs.EU.NCTS.IArrivalMovementHeaderProcessTask>(), strategy2.GetTypeForLoad(ariivalMoveHeader.TablePrefix, ariivalMoveHeader.PK, Factory));

				var departureMoveHeader = (CusInBondMoveHeader)Factory.New<Integration.Customs.EU.NCTS.IDepartureMovementHeader>();
				departureMoveHeader.BM_BH = header.PK;
				departureMoveHeader.BM_SubApplicationCode = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes.NctsDeparture.Substring(0, 1);
				AssertEquals(ObjectFactory.GetType<Integration.Customs.EU.NCTS.IDepartureMovementHeaderProcessTask>(), strategy2.GetTypeForLoad(departureMoveHeader.TablePrefix, departureMoveHeader.PK, Factory));
			}
		}
	}
}
