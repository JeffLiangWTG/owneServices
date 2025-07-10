using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusMAWBTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForForwarding()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals(ObjectFactory.GetType<Integration.Customs.AU.ICusMAWB>(), CusMAWBTypeDecider.GetTypeForForwarding());
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PapuaNewGuinea))
			{
				AssertNull(CusMAWBTypeDecider.GetTypeForForwarding());
			}
		}

		public void TestGetApplicationCodesForForwarding()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals(Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages, CusMAWBTypeDecider.GetApplicationCodesForForwarding()[0]);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PapuaNewGuinea))
			{
				AssertEquals(0, CusMAWBTypeDecider.GetApplicationCodesForForwarding().Length);
			}
		}

		public void TestGetTypeForLoad()
		{
			AssertEquals("AU Type", ObjectFactory.GetType<Integration.Customs.AU.ICTOCusMAWB>(), Decider.GetTypeForLoad(((INeedRow)AU_CTOCusMAWB).Row, Factory));
			AssertEquals("AU Type", ObjectFactory.GetType<Integration.Customs.AU.ICusMAWB>(), Decider.GetTypeForLoad(((INeedRow)AU_CusMAWB).Row, Factory));

			AssertEquals("GB Type", ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusMAWB>(), Decider.GetTypeForLoad(((INeedRow)GB_CusMAWB).Row, Factory));

			AssertEquals("NZ Type", ObjectFactory.GetType<Integration.Customs.NZ.ICusMAWB>(), Decider.GetTypeForLoad(((INeedRow)NZ_CusMAWB).Row, Factory));
			AssertEquals("NZ Type - NZ TSW CusMawb", ObjectFactory.GetType<Integration.Customs.NZ.ICusMAWB>(), Decider.GetTypeForLoad(((INeedRow)NZ_TSWCusMAWB).Row, Factory));
		}

		public void TestGetTypeForCountry()
		{
			AssertEquals("AU Type", ObjectFactory.GetType<Integration.Customs.AU.ICusMAWB>(), Decider.GetTypeForCountry(Core.Constants.CountryCodes.Australia));
			AssertEquals("GB Type", ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusMAWB>(), Decider.GetTypeForCountry(Core.Constants.CountryCodes.UnitedKingdom));
			AssertEquals("NZ Type", ObjectFactory.GetType<Integration.Customs.NZ.ICusMAWB>(), Decider.GetTypeForCountry(Core.Constants.CountryCodes.NewZealand));
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals("Type", null, Decider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertEquals("Type", null, Decider.GetTypeForNew());
		}

		#region Implementation
		CusMAWBTypeDecider Decider => new CusMAWBTypeDecider();

		CusMAWB AU_CTOCusMAWB => (CusMAWB)Factory.New<Integration.Customs.AU.ICTOCusMAWB>();

		CusMAWB AU_CusMAWB => (CusMAWB)Factory.New<Integration.Customs.AU.ICusMAWB>();

		CusMAWB NZ_CusMAWB => (CusMAWB)Factory.New<Integration.Customs.NZ.ICusMAWB>();

		CusMAWB NZ_TSWCusMAWB
		{
			get
			{
				CusMAWB result = (CusMAWB)Factory.New<Integration.Customs.NZ.ICusMAWB>();
				result.CM_ApplicationCode = Enterprise.Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
				return result;
			}
		}

		CusMAWB GB_CusMAWB => (CusMAWB)Factory.New<Integration.Customs.GB.CCSUK.ICusMAWB>();

		#endregion
	}
}
