using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusPartShipTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			AssertEquals("Type - AU", ObjectFactory.GetType<Integration.Customs.AU.ICusPartShip>(), Decider.GetTypeForLoad(((INeedRow)AU_CusPartShip).Row, Factory));
			AssertEquals("Type - GB house", ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ISplitHouse>(), Decider.GetTypeForLoad(((INeedRow)GB_SplitHouse).Row, Factory));
			AssertEquals("Type - GB basic", ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ISplitBasic>(), Decider.GetTypeForLoad(((INeedRow)GB_SplitBasic).Row, Factory));
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

		CusPartShipTypeDecider Decider
		{
			get { return new CusPartShipTypeDecider(); }
		}

		CusPartShip AU_CusPartShip
		{
			get
			{
				var mAWB = (CusMAWB)Factory.New<Integration.Customs.AU.ICusMAWB>();
				var hAWB = (CusHAWB)Factory.New<Integration.Customs.AU.ICusHAWB>();
				hAWB.CS_CM = mAWB.PK;
				CusPartShip cps = (CusPartShip)Factory.New<Integration.Customs.AU.ICusPartShip>();
				cps.CG_CS = hAWB.PK;
				return cps;
			}
		}

		CusPartShip GB_SplitHouse
		{
			get
			{
				var mAWB = (CusMAWB)Factory.New<Integration.Customs.GB.CCSUK.ICusMAWB>();
				var hAWB = (CusHAWB)Factory.New<Integration.Customs.GB.CCSUK.ICusHAWB>();
				hAWB.CS_CM = mAWB.PK;
				CusPartShip cps = (CusPartShip)Factory.New<Integration.Customs.GB.CCSUK.ISplitHouse>();
				cps.CG_CS = hAWB.PK;
				return cps;
			}
		}
		CusPartShip GB_SplitBasic
		{
			get
			{
				var mAWB = (CusMAWB)Factory.New<Integration.Customs.GB.CCSUK.ICusMAWB>();
				CusPartShip cps = (CusPartShip)Factory.New<Integration.Customs.GB.CCSUK.ISplitBasic>();
				cps.CG_CM_LinkToPartMaster = mAWB.PK;
				return cps;
			}
		}

		#endregion

	}
}
