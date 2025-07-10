using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class EquipmentTypesTest : TestCaseWithFactory
	{
		public void TestGetEquipmentTypes()
		{
			var equipmentAndConveyanceTypes = new EquipmentTypes();
			equipmentAndConveyanceTypes.AddRangeOverwriteIfExists(new ConveyanceTypes());
			var codeMap = Factory.NewWithValidTestData<RefContainerCodeMap>();
			codeMap.RCM_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			var equipmentTypes = codeMap.Lookups.CodeList;
			foreach (ICodeDescription type in equipmentAndConveyanceTypes)
			{
				AssertEquals(string.Format("RefEquipment.EquipmentTypes should contain type '{0}'.", type.Code), true, equipmentTypes.ContainsCode(type.Code));
			}
		}

		public void TestSealsAndLicensePlatesRequired()
		{
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.Container20FtSeaOpenTop, false, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.Container20FtSeaClosedTop, false, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.Container40FtSeaOpenTop, false, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.Container40FtSeaClosedTop, false, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.BeverageRackTrailer, true, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.GooseneckTrailer, true, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.Chassis, true, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.OtherLengthSeaContainerClosedTop, false, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.OtherLengthSeaContainerOpenTop, false, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.RefrigeratedContainer, false, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.DoubleDropTrailer, true, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.DropBackTrailer, true, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.FlatBedTrailerWithHeadboards, false, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.FlatBedTrailerWithNoHeadboards, false, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.FlatRackTrailer, true, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.FlatbedPlatformTrailer, true, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.HopperTrailerCovered, true, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.HorseTrailer, true, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.HopperTrailerOpen, true, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.HopperTrailerCoveredPneumaticDischarge, true, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.TankTrailerLiquidsNotHeatedNotInsulated, true, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.TankTrailerLiquidsHeatedNotInsulated, true, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.TankTrailerLiquidsNotHeatedInsulated, true, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.TankTrailerLiquidsHeatedInsulated, true, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.LivestockTrailer, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.NoEquipment, false, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.Other, false, false);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.FixedRackDoubleDropTrailer, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.GondolaClosed, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.GondolaOpen, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.FixedRackSingleDropTrailer, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.ControlledTemperature, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.SingleDropTrailer, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.TankTrailerGasNotHeatedNotInsulated, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.TankTrailerGasHeatedNotInsulated, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.TankTrailerGasNotHeatedInsulated, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.TankTrailerGasHeatedInsulated, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.TankTrailerChemicalsNotHeatedNotInsulated, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.TankTrailerChemicalsHeatedNotInsulated, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.TankTrailerChemicalsNotHeatedInsulated, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.TankTrailerChemicalsHeatedInsulated, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.AutoCarrierTrailer, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.TrailerDryFreight, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.TankTrailerFoodGradeLiquids, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.SemiTruckTrailer, true, true);
			AssertSealsAndLicensePlatesRequired(EquipmentTypes.Codes.ControlledTemperatureTrailer, true, true);
		}

		void AssertSealsAndLicensePlatesRequired(string type, bool sealsRequired, bool licensePlatesRequired)
		{
			AssertEquals("Are license plates required for " + type, licensePlatesRequired, EquipmentTypes.IsLicensePlateRequired(Factory, type));
		}
	}
}
