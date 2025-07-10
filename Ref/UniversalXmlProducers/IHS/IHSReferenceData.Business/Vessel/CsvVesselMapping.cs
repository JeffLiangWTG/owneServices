using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using TinyCsvParser.Mapping;

namespace CargoWise.RefDbRepo.IHSReferenceData.Business.Vessel
{
	class CsvVesselMapping : CsvMapping<RefVessel>
	{
		public CsvVesselMapping()
		{
			MapProperty(0, x => x.RV_LloydsNumber);
			MapProperty(1, x => x.RV_Code);
			MapProperty(16, x => x.RV_RadioCallSign);
			MapProperty(26, x => x.RV_RN_NKCountryOfReg);
			MapProperty(67, x => x.RV_YearOfConstruction);
			MapProperty(4, x => x.RV_StatusCode);
			MapProperty(7, x => x.RV_StatCode5);
			MapProperty(41, x => x.RV_MaritimeMobileServiceIdentity);
			MapProperty(65, x => x.RV_TEU);
			MapProperty(15, x => x.RV_Breadth);
			MapProperty(37, x => x.RV_Length);
			MapProperty(23, x => x.RV_Deadweight);
			MapProperty(24, x => x.RV_GrossTonnage);
			MapProperty(25, x => x.RV_Draught);
			MapProperty(29, x => x.RV_IsGearless, new IhsBoolTypeConverter());
			MapProperty(30, x => x.RV_GrainCapacity);
			MapProperty(40, x => x.RV_LiquidCapacity);
			MapProperty(42, x => x.RV_CarsNumber, new IhsNonNullableIntConverter());
			MapProperty(45, x => x.RV_TanksNumber, new IhsNonNullableShortConverter());
			MapProperty(54, x => x.RV_ReeferPointsNumber);
			MapProperty(20, x => x.RV_RoroLanesClearHeight);
			MapProperty(66, x => x.RV_RoroLanesWidth);
			MapProperty(43, x => x.RV_RoroLanesNumber);
			MapProperty(44, x => x.RV_RoroRampsNumber, new IhsNonNullableByteConverter());
			MapProperty(38, x => x.RV_RoroLanesLength);
		}
	}
}
