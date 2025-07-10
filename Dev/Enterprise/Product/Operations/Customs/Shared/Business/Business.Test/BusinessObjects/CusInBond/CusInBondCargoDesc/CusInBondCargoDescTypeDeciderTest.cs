using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusInBondCargoDescTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var header = (AutoCusInBondHeader)Factory.New<Integration.Customs.US.eManifest.ICusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			var bill = (AutoCusInBondBill)Factory.New<Integration.Customs.US.eManifest.ICusInBondBill>();
			bill.B0_BH = header.PK;
			var cargoDesc = (AutoCusInBondCargoDesc)Factory.New<Integration.Customs.US.eManifest.ICusInBondCargoDesc>();
			cargoDesc.BY_ParentID = bill.PK;
			cargoDesc.BY_ParentTableCode = bill.TablePrefix;

			var row = ((INeedRow)cargoDesc).Row;

			var typeDecider = new CusInBondCargoDescTypeDecider();
			var typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals("Enterprise.Customs.US.eManifest.Business.Commodity", typeForLoad.FullName);
			typeForLoad = typeDecider.GetTypeForLoad(cargoDesc, Factory);
			AssertEquals("Enterprise.Customs.US.eManifest.Business.Commodity", typeForLoad.FullName);
			typeForLoad = typeDecider.GetTypeForLoad(bill.TablePrefix, bill.PK, Factory);
			AssertEquals("Enterprise.Customs.US.eManifest.Business.Commodity", typeForLoad.FullName);
		}
	}
}
