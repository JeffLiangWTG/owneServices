using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	class DangerousGoodBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var context = new CommonContext(Factory);

			var packLineBO = Factory.New<PackLine>();
			packLineBO.JL_RequiresTemperatureControl = true;
			packLineBO.JL_RequiredTemperatureMaximum = 10;
			packLineBO.JL_RequiredTemperatureUnit = Core.Constants.Temperature.Centigrade;

			var undgSubstanceBO = Factory.New<UNDGSubstance>();
			undgSubstanceBO.DG_Mode = Core.Constants.TransportModes.Air;
			undgSubstanceBO.DG_UNNO = "8000";
			undgSubstanceBO.DG_Variant = "12";
			undgSubstanceBO.DG_PSN = "PSN";
			undgSubstanceBO.DG_SubLabel1 = "SubLabel1";
			undgSubstanceBO.DG_SubLabel2 = "SubLabel2";
			undgSubstanceBO.DG_State = "S";
			undgSubstanceBO.DG_ExceptedQuantityCode = "E0";
			undgSubstanceBO.DG_Class = "1";
			undgSubstanceBO.DG_PG = "2";

			var undgDataItemBO = packLineBO.UNDGs.AddNew();
			undgDataItemBO.DI_DG = undgSubstanceBO.PK;
			undgDataItemBO.DI_PackageCount = 2;
			undgDataItemBO.DI_F3_NKPackType = Core.Constants.PkgUnit.Bottle;
			undgDataItemBO.DI_IsLimitedQuantity = false;
			undgDataItemBO.DI_TechnicalName = "DI_TechnicalName";
			undgDataItemBO.DI_IMOClass = "1.1";
			undgDataItemBO.DI_MaterialFormDescription = "DI_MaterialFormDescription";
			undgDataItemBO.DI_RadionuclideElementSuffix = "O.o";
			undgDataItemBO.DI_SpecialPermitNumber = "233";
			undgDataItemBO.DI_HazardousWasteCode = "111";
			undgDataItemBO.DI_IsFissileExcepted = true;
			undgDataItemBO.DI_IsExclusiveUse = true;
			undgDataItemBO.DI_IsHighwayRouteControlledQuantity = true;
			undgDataItemBO.DI_IsResidueLastContained = true;
			undgDataItemBO.DI_IsSalvagePackaging = true;
			undgDataItemBO.DI_RadioactiveTransportIndex = 1m;
			undgDataItemBO.DI_DGWeight = 1m;
			undgDataItemBO.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undgDataItemBO.DI_DGVolume = 1m;
			undgDataItemBO.DI_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			undgDataItemBO.DI_DGFlashPoint = 10;
			undgDataItemBO.DI_RadioactiveMaximumActivity = 30m;
			undgDataItemBO.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Megabecquerel;
			undgDataItemBO.DI_MPMarinePollutant = "S";

			var dangerousGood = new DangerousGoodBuilder().Build(undgDataItemBO, context);

			AssertEquals("800012", dangerousGood.Code);
			AssertEquals("8000", dangerousGood.Unno);
			AssertEquals("12", dangerousGood.Variant);
			AssertEquals(Core.Constants.TransportModes.Air, dangerousGood.TransportMode.Code);
			AssertNotNull(dangerousGood.Contact);
			AssertEquals(2, dangerousGood.Quantity);
			AssertEquals(undgDataItemBO.DI_IsLimitedQuantity, dangerousGood.PackedInLimitedQuantity);
			AssertEquals("PSN", dangerousGood.ProperShippingName);
			AssertEquals("DI_TechnicalName", dangerousGood.TechnicalName);
			AssertEquals("1", dangerousGood.IMOClass);
			AssertEquals("2", dangerousGood.PackingGroup);
			AssertEquals("SubLabel1", dangerousGood.SubLabel1);
			AssertEquals("SubLabel2", dangerousGood.SubLabel2);
			AssertEquals("S", dangerousGood.State);
			AssertEquals("DI_MaterialFormDescription", dangerousGood.MaterialFormDescription);
			AssertEquals("O.o", dangerousGood.RadionuclideElementSuffix);
			AssertEquals("233", dangerousGood.SpecialPermitNumber);
			AssertEquals("111", dangerousGood.HazardousWasteCode);
			AssertNullOrEmpty(dangerousGood.PSAGroup);
			Assert(dangerousGood.IsFissileExcepted);
			Assert(dangerousGood.IsExclusiveUse);
			Assert(dangerousGood.IsHighwayRouteControlledQuantity);
			Assert(dangerousGood.IsResidueLastContained);
			Assert(dangerousGood.IsSalvagePackaging);
			AssertEquals(1m, dangerousGood.RadioactiveTransportIndex);
			Assert(dangerousGood.RequiresTemperatureControl);
			AssertEquals(10m, dangerousGood.RequiredTemperatureMaximum.Value);
			AssertEquals(Core.Constants.Temperature.Centigrade, dangerousGood.RequiredTemperatureMaximum.Unit.Code);
			AssertEquals(1m, dangerousGood.Weight.Value);
			AssertEquals(Core.Constants.Weight.Kilograms, dangerousGood.Weight.Unit.Code);
			AssertEquals(1m, dangerousGood.Volume.Value);
			AssertEquals(Core.Constants.Volume.CubicMetres, dangerousGood.Volume.Unit.Code);
			AssertEquals(10m, dangerousGood.FlashPoint.Value);
			AssertEquals(Core.Constants.Temperature.Centigrade, dangerousGood.FlashPoint.Unit.Code);
			AssertEquals(30m, dangerousGood.RadioactiveMaximumActivity.Value);
			AssertEquals(Core.Constants.RadioactiveUnits.Megabecquerel, dangerousGood.RadioactiveMaximumActivity.Unit.Code);
			AssertEquals(Core.Constants.PkgUnit.Bottle, dangerousGood.PackageType.Code);
			AssertEquals("S", dangerousGood.MarinePollutant.Code);
			AssertEquals("E0", dangerousGood.ExceptedQuantityCode.Code);
		}
	}
}
