using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test;

[TestFixture]
[TransactionedTestCase]
class RefVesselUserViewFixture
{
	[Test]
	public void View()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
		{
			var vesselUserView = new RefVesselUserView
			{
				RV_PK = Guid.NewGuid(),
				RV_Code = "Mandalay",
				RV_IsActive = true,
				RV_LloydsNumber = "",
				RV_MalaysiaVesselId = "",
				RV_RadioCallSign = "",
				RV_NetRegisterTon = 0,
				RV_VesselType = "CVB",
				RV_YearOfConstruction = 0,
				RV_CustomAttrib1 = "",
				RV_CustomAttrib2 = "",
				RV_CustomAttrib3 = "",
				RV_CustomFlag1 = false,
				RV_CustomDecimal1 = 0,
				RV_CarrierCode = "VTL",
				RV_ScreeningStatus = "UNK",
				RV_RN_NKCountryOfReg = "",
				RV_MaritimeMobileServiceIdentity = "",
				RV_StatusCode = "SRV",
				RV_StatCode5 = "",
				RV_IsGearless = false,
				RV_Length = 0,
				RV_Breadth = 0,
				RV_Draught = 0,
				RV_Deadweight = 0,
				RV_GrossTonnage = 0,
				RV_GrainCapacity = 0,
				RV_LiquidCapacity = 0,
				RV_RoroLanesLength = 0,
				RV_RoroLanesWidth = 0,
				RV_RoroLanesClearHeight = 0,
				RV_RoroLanesNumber = 0,
				RV_RoroRampsNumber = 0,
				RV_TEU = 0,
				RV_CarsNumber = 0,
				RV_ReeferPointsNumber = 0,
				RV_TanksNumber = 0,
				RV_IsPublished = true
			};
			context.RefVesselUserViews.Add(vesselUserView);
			context.SaveChanges();
			Assert.That(context.DataSetChangeHistories.Where(x => x.DCH_ParentPK == vesselUserView.RV_PK).ToArray(), Has.Length.EqualTo(1));
		}
		using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
		{
			var vesselUserView = context.RefVesselUserViews.FirstOrDefault();
			Assert.AreEqual("Mandalay", vesselUserView.RV_Code);
			Assert.AreEqual(true, vesselUserView.RV_IsActive);
			Assert.AreEqual("CVB", vesselUserView.RV_VesselType);
			Assert.AreEqual("VTL", vesselUserView.RV_CarrierCode);
			Assert.AreEqual("UNK", vesselUserView.RV_ScreeningStatus);
			Assert.AreEqual("SRV", vesselUserView.RV_StatusCode);
			Assert.AreEqual(0, vesselUserView.RV_NetRegisterTon);

			var versionControl = context.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == vesselUserView.RV_PK);
			Assert.AreEqual(false, versionControl.RVC_Deleted);

			vesselUserView.RV_Code = "Modified Code";
			vesselUserView.RV_IsActive = false;
			vesselUserView.RV_ScreeningStatus = "CLR";
			vesselUserView.RV_NetRegisterTon = 1;
			vesselUserView.RV_IsPublished = false;
			context.SaveChanges();
		}

		using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
		{
			var vesselUserView = context.RefVesselUserViews.FirstOrDefault();
			Assert.AreEqual("Modified Code", vesselUserView.RV_Code);
			Assert.AreEqual(false, vesselUserView.RV_IsActive);
			Assert.AreEqual("CLR", vesselUserView.RV_ScreeningStatus);
			Assert.AreEqual(1, vesselUserView.RV_NetRegisterTon);

			var versionControl = context.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == vesselUserView.RV_PK);
			Assert.AreEqual(true, versionControl.RVC_Deleted);
		}

		using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
		{
			var vesselUserView = context.RefVesselUserViews.FirstOrDefault();
			context.RefVesselUserViews.Remove(vesselUserView);
			Assert.Throws<DbUpdateException>(() => context.SaveChanges(), "Should not enable Delete in top-level tables");
		}
	}
}
