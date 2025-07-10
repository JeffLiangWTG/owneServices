using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefVesselVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefVessel()
				{
					RV_PK = Guid.NewGuid(),
					RV_Code = "Mandalay",
					RV_LloydsNumber = "",
					RV_MalaysiaVesselId = "",
					RV_RadioCallSign = "",
					RV_NetRegisterTon = 0,
					RV_VesselType = "CV",
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
					RV_TanksNumber = 0
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			((RefVessel)data).RV_Code = "New Code";
			return true;
		}
	}
}
