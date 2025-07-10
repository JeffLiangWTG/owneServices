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
	internal sealed class CommonWorkSheetFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoad()
		{
			using (RowFactory.SetCachedTables())
			{
				var factory = new BusinessObjectFactory();
				CommonWorkSheet[] runSheets = factory.Load<CommonWorkSheet>(new ZQuery(JobCartageRunSheetSchema.PK, CreatePopulatedRunSheets()));
				string hit = "";
				foreach (CommonWorkSheet runSheet in runSheets)
				{
					hit = runSheet.TransportCo.OH_Code;
					hit = runSheet.Truck.RQ_ShortCode;
					hit = runSheet.TruckDriver.GS_Code;
					hit = runSheet.CartageLegs[0].JU_AdditionalService;
					hit = runSheet.Status;
				}

				// GlbStaff: 1
				// JobCartageRunSheet: 1
				// JobContainerLegs: 1
				// OrgHeader: 1
				// RefEquipment: 1
				// StmALog: 1
				// Hits: 6/1
				AssertMaxDbHits(6, factory);
			}
		}

		protected override void SetUp()
		{
			// Preload ProcessFieldChangeRules in UberCache to avoid DBHit confusion
			new BusinessObjectFactory().New<DummyBusinessObject>();
			base.SetUp();
		}

		static List<ZGuid> CreatePopulatedRunSheets()
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();
			for (int i = 0; i < 12; i++)
			{
				var cto = factory.NewWithValidTestData<OrgHeader>();
				var cfs = factory.NewWithValidTestData<OrgHeader>();
				var cyd = factory.NewWithValidTestData<OrgHeader>();
				var vessel = factory.NewWithValidTestData<RefVessel>();
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
				var runSheet = factory.New<CommonWorkSheet>();
				var driver = factory.NewWithValidTestData<GlbStaff>();
				var transportCo = factory.NewWithValidTestData<OrgHeader>();
				var truck = factory.NewWithValidTestData<RefEquipment>();
				driver.GS_Code = "D" + i;
				transportCo.OH_Code = "TRNCO" + i;
				truck.RQ_ShortCode = "TRK" + i;
				truck.RQ_Registration = "REG" + i;
				runSheet.EY_GS_NKTruckDriver = driver.GS_Code;
				runSheet.EY_OH_TransportCo = transportCo.PK;
				runSheet.EY_RQ_Truck = truck.PK;
				runSheet.CartageLegs.Add(leg1);
				runSheet.CartageLegs.Add(leg2);
				result.Add(runSheet.PK);
			}

			factory.Save();
			return result;
		}
	}
}
