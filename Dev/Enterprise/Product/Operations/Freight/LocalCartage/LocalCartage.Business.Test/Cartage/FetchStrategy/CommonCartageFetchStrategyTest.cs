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
	internal sealed class CommonCartageFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoad()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			factory.ResetDatabaseLoadCount();
			CommonCartage[] cartages = factory.Load<CommonCartage>(new ZQuery(JobCartageSchema.PK, CreateContainersAndDivots()));
			string hit = "";
			foreach (CommonCartage cartage in cartages)
			{
				CommonCartageLeg leg1 = cartage.ContainerBookedMoves[0].CartageLegs[0];
				CommonCartageLeg leg2 = cartage.ContainerBookedMoves[0].CartageLegs[0];
				hit = leg1.PickupAddressCode;
				hit = leg1.DeliveryAddressCode;
				hit = leg2.PickupAddressCode;
				hit = leg2.DeliveryAddressCode;
			}

			//JobBookedCtgMove: 1
			//JobCartage: 1
			//JobContainer: 1
			//JobContainerLegs: 1
			//LocalCartageJobLegType: 1
			//LocalCartageJobOrg: 1
			//LocalCartageJobType: 1
			//OrgAddress: 1
			//Hits: 9/0
			AssertMaxDbHits(9, factory);
		}

		static List<ZGuid> CreateContainersAndDivots()
		{
			List<ZGuid> result = new List<ZGuid>();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			for (int i = 0; i < 12; i++)
			{
				OrgHeader cto = factory.NewWithValidTestData<OrgHeader>();
				OrgHeader cfs = factory.NewWithValidTestData<OrgHeader>();
				OrgHeader cyd = factory.NewWithValidTestData<OrgHeader>();
				var vessel = factory.NewWithValidTestData<RefVessel>();
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
				CommonBookedCtgMove move = cartage.ContainerBookedMoves.AddNew();
				CommonContainer container = move.Container;
				container.JC_ContainerNum = string.Format("TEST41000{0:00}", i);
				CommonCartageLeg leg1 = move.CartageLegs.AddNew();
				CommonCartageLeg leg2 = move.CartageLegs.AddNew();
				leg1.JU_E2PickupAddressID = cto.PK;
				leg1.JU_E2DeliveryAddressID = cfs.PK;
				leg2.JU_E2PickupAddressID = cfs.PK;
				leg2.JU_E2DeliveryAddressID = cyd.PK;
				result.Add(cartage.PK);
			}

			factory.Save();
			return result;
		}

		public void TestFetchForView_JH_ProfitLossReasonCode()
		{
			AssertFetchForView(nameof(CommonCartage.Job) + "+" + nameof(CommonCartage.Job.JH_ProfitLossReasonCode), new Dictionary<string, int>
			{
				{ JobHeaderSchema.Constants.TableName, 1 }
			});
		}

		public void TestFetchForView_JH_TotalProfitRevenueMargin()
		{
			AssertFetchForView(nameof(CommonCartage.Job) + "+" + nameof(CommonCartage.Job.JH_TotalProfitRevenueMargin), new Dictionary<string, int>
			{
				{ JobHeaderSchema.Constants.TableName, 1 }
			});
		}

		void AssertFetchForView(string propertyName, Dictionary<string, int> expectedDbHits)
		{
			for (var i = 0; i <= 10; i++)
			{
				CreateCommonCartage();
			}
			Factory.Save();

			var newFactory = NewFactory();
			var headers = newFactory.Load<CommonCartage>(new ZQuery());
			newFactory.ResetDatabaseLoadCount();

			foreach (var header in headers)
			{
				header.FetchStrategy.FetchForView(new[]
				{
					new TableColumn(string.Empty, propertyName)
				});
			}

			foreach (var header in headers)
			{
				_ = header[propertyName];
			}

			AssertDbHits(expectedDbHits, newFactory);
		}

		CommonCartage CreateCommonCartage() => Factory.NewWithValidTestData<CommonCartage>();
	}
}
