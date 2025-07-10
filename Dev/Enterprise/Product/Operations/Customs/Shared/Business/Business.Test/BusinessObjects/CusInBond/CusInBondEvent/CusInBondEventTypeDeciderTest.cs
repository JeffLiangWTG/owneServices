using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusInBondEventTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			AssertNull(typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertNull(typeDecider.GetTypeForNew());
		}

		public void TestGetTypeForLoad_Transshipment()
		{
			var inBondEvent = (CusInBondEvent)Factory.New<Integration.Customs.EU.NCTS.IEnRouteTransshipment>();
			inBondEvent.BN_Type = CusInBondEventTypes.Codes.Transshipment;

			var typeForLoad = typeDecider.GetTypeForLoad(((INeedRow)inBondEvent).Row, Factory);
			AssertEquals(ObjectFactory.GetType<Integration.Customs.EU.NCTS.IEnRouteTransshipment>(), typeForLoad);
		}

		public void TestGetTypeForLoad_Seal()
		{
			var bo = (CusInBondEvent)Factory.New<Integration.Customs.EU.NCTS.IEnRouteSeal>();
			bo.BN_Type = CusInBondEventTypes.Codes.Seal;

			var typeForLoad = typeDecider.GetTypeForLoad(((INeedRow)bo).Row, Factory);
			AssertEquals(ObjectFactory.GetType<Integration.Customs.EU.NCTS.IEnRouteSeal>(), typeForLoad);
		}

		public void TestGetTypeForLoad_Incident()
		{
			var bo = (CusInBondEvent)Factory.New<Integration.Customs.EU.NCTS.IEnRouteIncident>();
			bo.BN_Type = CusInBondEventTypes.Codes.Incident;

			var typeForLoad = typeDecider.GetTypeForLoad(((INeedRow)bo).Row, Factory);
			AssertEquals(ObjectFactory.GetType<Integration.Customs.EU.NCTS.IEnRouteIncident>(), typeForLoad);
		}

		public void TestGetTypeForLoad_Default()
		{
			var inBondEvent = (CusInBondEvent)Factory.New<Integration.Customs.EU.NCTS.IEnRouteTransshipment>();
			inBondEvent.BN_Type = string.Empty;

			var typeForLoad = typeDecider.GetTypeForLoad(((INeedRow)inBondEvent).Row, Factory);
			AssertEquals(typeof(CusInBondEvent), typeForLoad);
		}

		protected override void SetUp()
		{
			base.SetUp();
			typeDecider = new CusInBondEventTypeDecider();
		}
		CusInBondEventTypeDecider typeDecider;
	}
}
