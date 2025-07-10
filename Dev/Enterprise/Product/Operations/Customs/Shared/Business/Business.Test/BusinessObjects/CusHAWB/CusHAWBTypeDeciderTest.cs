using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusHAWBTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			AssertEquals("Type", ObjectFactory.GetType<Integration.Customs.NZ.ICusHAWB>(), Decider.GetTypeForLoad(((INeedRow)NZ_CusHAWB).Row, Factory));
			AssertEquals("Type", ObjectFactory.GetType<Integration.Customs.AU.ICTOCusHAWB>(), Decider.GetTypeForLoad(((INeedRow)AU_CTOCusHAWB).Row, Factory));
			AssertEquals("Type", ObjectFactory.GetType<Integration.Customs.AU.ICusHAWB>(), Decider.GetTypeForLoad(((INeedRow)AU_CusHAWB).Row, Factory));
			AssertEquals("Type", ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusHAWB>(), Decider.GetTypeForLoad(((INeedRow)GB_CusHAWB).Row, Factory));

			var detachedAUHAWB = (CusHAWB)Factory.New<Integration.Customs.AU.ICusHAWB>();
			detachedAUHAWB.CS_ApplicationCode = Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("Type", ObjectFactory.GetType<Integration.Customs.AU.ICusHAWB>(), Decider.GetTypeForLoad(((INeedRow)detachedAUHAWB).Row, Factory));

			var nzTSWHawb = (CusHAWB)Factory.New<Integration.Customs.NZ.ICusHAWB>();
			nzTSWHawb.CS_ApplicationCode = Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			AssertEquals("TSW Type is recognised as a NZ CusHawb", ObjectFactory.GetType<Integration.Customs.NZ.ICusHAWB>(), Decider.GetTypeForLoad(((INeedRow)NZ_CusHAWB).Row, Factory));

			var nzECIHawb = (CusHAWB)Factory.New<Integration.Customs.NZ.ICusHAWB>();
			nzECIHawb.CS_ApplicationCode = Constants.Customs.ExpressApplicationCodes.NZ.ECIWriteOff;
			AssertEquals("NZ Legacy Type continues to be recognised as a NZ CusHawb", ObjectFactory.GetType<Integration.Customs.NZ.ICusHAWB>(), Decider.GetTypeForLoad(((INeedRow)NZ_CusHAWB).Row, Factory));
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

		CusHAWBTypeDecider Decider
		{
			get { return new CusHAWBTypeDecider(); }
		}

		CusHAWB AU_CTOCusHAWB
		{
			get
			{
				CusMAWB mAWB = (CusMAWB)Factory.New<Integration.Customs.AU.ICTOCusMAWB>();
				CusHAWB hAWB = (CusHAWB)Factory.New<Integration.Customs.AU.ICTOCusHAWB>();
				hAWB.CS_CM = mAWB.PK;
				return hAWB;
			}
		}

		CusHAWB AU_CusHAWB
		{
			get
			{
				CusMAWB mAWB = (CusMAWB)Factory.New<Integration.Customs.AU.ICusMAWB>();
				CusHAWB hAWB = (CusHAWB)Factory.New<Integration.Customs.AU.ICusHAWB>();
				hAWB.CS_CM = mAWB.PK;
				return hAWB;
			}
		}

		CusHAWB NZ_CusHAWB
		{
			get
			{
				CusMAWB mAWB = (CusMAWB)Factory.New<Integration.Customs.NZ.ICusMAWB>();
				CusHAWB hAWB = (CusHAWB)Factory.New<Integration.Customs.NZ.ICusHAWB>();
				hAWB.CS_CM = mAWB.PK;
				return hAWB;
			}
		}

		CusHAWB GB_CusHAWB
		{
			get
			{
				CusMAWB mAWB = (CusMAWB)Factory.New<Integration.Customs.GB.CCSUK.ICusMAWB>();
				CusHAWB hAWB = (CusHAWB)Factory.New<Integration.Customs.GB.CCSUK.ICusHAWB>();
				hAWB.CS_CM = mAWB.PK;
				return hAWB;
			}
		}

		#endregion

	}
}
