using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.GPS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	internal sealed class CommonCartageLegFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoad()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			factory.ResetDatabaseLoadCount();
			CommonCartageLeg[] legs = factory.Load<CommonCartageLeg>(new ZQuery(JobContainerLegsSchema.PK, CreateContainersDivotsAndCartage()));
			string hit = "";
			foreach (CommonCartageLeg leg in legs)
			{
				hit += leg.PickupAddressCode;
				hit += leg.DeliveryAddressCode;
				hit += leg.BookedCtgMove.Container.JC_ContainerNum;
				hit += leg.WorkflowItems[0].DescriptionWithReference;
			}

			//JobBookedCtgMove: 1
			//JobCartage: 1
			//JobContainer: 1
			//JobContainerLegs: 1
			//LocalCartageJobLegType: 1
			//LocalCartageJobOrg: 1
			//LocalCartageJobType: 1
			//OrgAddress: 1
			//ProcessTask: 1
			AssertMaxDbHits(9, factory);
		}

		List<ZGuid> CreateContainersDivotsAndCartage()
		{
			List<ZGuid> result = new List<ZGuid>();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			for (int i = 0; i < 12; i++)
			{
				OrgHeader cto = factory.NewWithValidTestData<OrgHeader>();
				OrgHeader cfs = factory.NewWithValidTestData<OrgHeader>();
				OrgHeader cyd = factory.NewWithValidTestData<OrgHeader>();
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "vessel" + i;
				JobVoyage voyage = factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "voyage" + i;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
				voyage.GenerateSailings();
				CommonCartage cartage = factory.NewWithValidTestData<CommonCartage>();
				cartage.JJ_ConsignmentID = string.Format("T000001{0:00}", i);
				cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportUnpack;
				cartage.FirstDocAddress.E2_OA_Address = cto.MainAddress.PK;
				cartage.SecondDocAddress.E2_OA_Address = cfs.MainAddress.PK;
				cartage.ThirdDocAddress.E2_OA_Address = cyd.MainAddress.PK;
				cartage.JJ_JX_Sailing = voyage.Sailings[0].PK;
				var move = cartage.ContainerBookedMoves.AddNew();
				var container = move.Container;
				container.JC_ContainerNum = string.Format("TEST41000{0:00}", i);
				CommonCartageLeg leg1 = move.CartageLegs.AddNew();
				CommonCartageLeg leg2 = move.CartageLegs.AddNew();
				leg1.JU_E2PickupAddressID = cto.PK;
				leg1.JU_E2DeliveryAddressID = cfs.PK;
				leg2.JU_E2PickupAddressID = cfs.PK;
				leg2.JU_E2DeliveryAddressID = cyd.PK;
				factory.New<GPSSupporterActivity>().EN_JU = leg1.PK;
				factory.New<GPSSupporterActivity>().EN_JU = leg2.PK;
				factory.New<GenCustomAddOnValue>().XV_ParentID = leg1.PK;
				factory.New<GenCustomAddOnValue>().XV_ParentID = leg2.PK;
				result.Add(leg1.PK);
				result.Add(leg2.PK);
			}

			factory.Save();
			return result;
		}
	}
}
