using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	internal sealed class CommonBookedCtgMoveFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoad()
		{
			var movesCreated = CreateContainersDivotsAndCartageInNewFactory();
			Factory.ResetDatabaseLoadCount();
			var moves = Factory.Load<CommonBookedCtgMove>(new ZQuery(JobBookedCtgMoveSchema.PK, movesCreated));
			foreach (var move in moves)
			{
				var accessProperty = move.Cartage.FirstDocAddress.Address.OA_Code;
				accessProperty = move.Cartage.SecondDocAddress.Address.OA_Code;
				accessProperty = move.Cartage.ThirdDocAddress.Address.OA_Code;
				accessProperty = move.Container.JC_ContainerNum;
				accessProperty = move.UNDGs.Count.ToString();
				accessProperty = move.Cartage.JJ_ConsignmentID;
			}

			//CusContainer: 1
			//JobBookedCtgMove: 1
			//JobCartage: 1
			//JobContainer: 1
			//JobDocAddress: 1
			//LocalCartageJobLegType: 1
			//LocalCartageJobOrg: 1
			//LocalCartageJobType: 1
			//OrgAddress: 1
			//UNDGDataItem: 1
			AssertMaxDbHits(10, Factory);
		}

		List<ZGuid> CreateContainersDivotsAndCartageInNewFactory()
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();
			for (int i = 0; i < 12; i++)
			{
				var cto = factory.NewWithValidTestData<OrgHeader>();
				var cfs = factory.NewWithValidTestData<OrgHeader>();
				var cyd = factory.NewWithValidTestData<OrgHeader>();
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "vessel" + i;
				var voyage = factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "voyage" + i;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
				voyage.GenerateSailings();
				var cartage = factory.NewWithValidTestData<CommonCartage>();
				cartage.JJ_ConsignmentID = string.Format("T000001{0:00}", i);
				cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportUnpack;
				cartage.FirstDocAddress.E2_OA_Address = cto.MainAddress.PK;
				cartage.SecondDocAddress.E2_OA_Address = cfs.MainAddress.PK;
				cartage.ThirdDocAddress.E2_OA_Address = cyd.MainAddress.PK;
				cartage.JJ_JX_Sailing = voyage.Sailings[0].PK;
				var move = cartage.ContainerBookedMoves.AddNew();
				var container = move.Container;
				container.JC_ContainerNum = string.Format("TEST41000{0:00}", i);
				var leg1 = move.CartageLegs.AddNew();
				var leg2 = move.CartageLegs.AddNew();
				leg1.JU_E2PickupAddressID = cto.PK;
				leg1.JU_E2DeliveryAddressID = cfs.PK;
				leg2.JU_E2PickupAddressID = cfs.PK;
				leg2.JU_E2DeliveryAddressID = cyd.PK;
				result.Add(move.PK);
			}

			factory.Save();
			return result;
		}
	}
}
