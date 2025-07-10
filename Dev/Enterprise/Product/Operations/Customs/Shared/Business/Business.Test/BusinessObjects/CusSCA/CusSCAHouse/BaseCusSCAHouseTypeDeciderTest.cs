using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusSCAHouseTypeDeciderTest : TestCaseWithFactory
	{
		public void TestTypeForNew()
		{
			var decider = new BaseCusSCAHouseTypeDeciderForTest();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals(ObjectFactory.GetType<Integration.Customs.AU.ICusSCAHouse>(), decider.GetTypeForNew());
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PapuaNewGuinea))
			{
				AssertNull(decider.GetTypeForNew());
			}
		}
	}

	class BaseCusSCAHouseTypeDeciderForTest : BaseCusSCAHouseTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			ZGuid cusOceanBillPK = (row != null) ? new ZGuid(row[BaseCusSCAHouse.Schema.CA_CB]) : ZGuid.Empty;
			BaseCusSCAOceanBill cusSCAOceanBill = factory.Load<BaseCusSCAOceanBill>(cusOceanBillPK);
			ZString applicationCode = (cusSCAOceanBill != null) ? cusSCAOceanBill.CB_ApplicationCode : ZString.Empty;
			switch (applicationCode)
			{
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIAir:
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea:
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRail:
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRoad:
					return ObjectFactory.GetType<Integration.Customs.CA.ICusSCAHouse>();
				case Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff:
					return ObjectFactory.GetType<Integration.Customs.NZ.ICusSCAHouse>();
				case Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages:
					return ObjectFactory.GetType<Integration.Customs.AU.ICusSCAHouse>();
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.BaseTesting:
					return typeof(TestCusSCAHouse);
				default:
					return typeof(DefaultCusSCAHouse);
			}
		}
	}
}
