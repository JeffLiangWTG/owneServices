using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusGoodsLocationTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var enRouteIncident = (CusInBondEvent)Factory.New<Integration.Customs.EU.NCTS.IEnRouteIncident>();
			enRouteIncident.BN_Type = CusInBondEventTypes.Codes.Incident;
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_ParentTableCode = enRouteIncident.TablePrefix;
			cusGoodsLocation.CGL_ParentID = enRouteIncident.PK;

			var row = ((INeedRow)cusGoodsLocation).Row;
			var typeDecider = new CusGoodsLocationTypeDecider();
			var typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals("Enterprise.Customs.EU.NCTS.Business.CusGoodsLocation", typeForLoad.FullName);
		}

		public void TestGetTypeForLoad_ParentIsNotICusGoodsLocationTypeSupporter()
		{
			var dummyBusinessObject = Factory.New<DummyBaseBusinessObject>();
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_ParentTableCode = dummyBusinessObject.TablePrefix;
			cusGoodsLocation.CGL_ParentID = dummyBusinessObject.PK;

			var row = ((INeedRow)cusGoodsLocation).Row;
			var typeDecider = new CusGoodsLocationTypeDecider();
			typeDecider.GetTypeForLoad(row, Factory);

			AssertEquals("CargoWise.EntityFramework.Testing.DummyBaseBusinessObject has not implemented Enterprise.Customs.Business.ICusGoodsLocationTypeSupporter", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
