using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusInvPackTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var pack = (AutoCusInvPack)Factory.New<Integration.Customs.EU.NCTS.INctsPackage>();
			var row = ((INeedRow)pack).Row;

			var typeDecider = new CusInvPackTypeDecider();
			typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals("Cannot determine the CusInvPack object, because parent could not be determined (TableCode: )", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			var header = (AutoCusInBondHeader)Factory.New<Integration.Customs.EU.NCTS.ICusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

			pack.B5_ParentID = header.PK;
			pack.B5_ParentTableCode = header.TablePrefix;
			row = ((INeedRow)pack).Row;

			typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals("Cannot determine the CusInvPack object, because Enterprise.Customs.EU.NCTS.Business.NctsHeader has not implement Enterprise.Customs.Business.ICusInvPackTypeSupporter", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			var movementHeader = (AutoCusInBondMoveHeader)Factory.New<Integration.Customs.EU.NCTS.IDepartureMovementHeader>();
			movementHeader.BM_BH = header.PK;
			var goodsItem = (AutoCusInBondCargoDesc)Factory.New<Integration.Customs.EU.NCTS.IDepartureCargoDesc>();
			goodsItem.BY_ParentID = movementHeader.PK;
			goodsItem.BY_ParentTableCode = movementHeader.TablePrefix;

			pack.B5_ParentID = goodsItem.PK;
			pack.B5_ParentTableCode = goodsItem.TablePrefix;

			row = ((INeedRow)pack).Row;

			var typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals("Enterprise.Customs.EU.NCTS.Business.NctsPackage", typeForLoad.FullName);
		}
	}
}
