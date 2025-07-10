using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class UNDGPermissableQuantitiesHelperTest : TestCaseWithFactory
	{
		public void TestGetPermissibleQuantitiesForPackingInstruction()
		{
			var permissibleQuantitiesPI965 = UNDGPermissableQuantitiesHelper.GetPermissibleQuantitiesForPackingInstruction(LithiumBatteryConstants.RefPackingInstructions.PI965);
			var expectedPermissibleQuantitiesPI965 = new List<UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity>()
			{
				new UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.PI965, PackingInstructionSectionTypeList.Codes.SectionIA, 0, Core.Constants.Weight.Kilograms, 35, Core.Constants.Weight.Kilograms),
				new UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.PI965, PackingInstructionSectionTypeList.Codes.SectionIB, 0, Core.Constants.Weight.Kilograms, 10, Core.Constants.Weight.Kilograms)
			};
			AssertContainsExactElementsInExactOrder(expectedPermissibleQuantitiesPI965, permissibleQuantitiesPI965);

			var permissibleQuantities = UNDGPermissableQuantitiesHelper.GetPermissibleQuantitiesForPackingInstruction(LithiumBatteryConstants.RefPackingInstructions.Forbidden);
			AssertEquals(0, permissibleQuantities.Count());

			var permissibleQuantitiesPI977 = UNDGPermissableQuantitiesHelper.GetPermissibleQuantitiesForPackingInstruction(SodiumBatteryConstants.RefPackingInstructions.PI977);
			var expectedPermissibleQuantitiesPI977 = new List<UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity>()
			{
				new UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity(SodiumBatteryConstants.RefPackingInstructions.PI977, PackingInstructionSectionTypeList.Codes.SectionI, 5, Core.Constants.Weight.Kilograms, 35, Core.Constants.Weight.Kilograms),
				new UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity(SodiumBatteryConstants.RefPackingInstructions.PI977, PackingInstructionSectionTypeList.Codes.SectionII, 5, Core.Constants.Weight.Kilograms, 5, Core.Constants.Weight.Kilograms)
			};
			AssertContainsExactElementsInExactOrder(expectedPermissibleQuantitiesPI977, permissibleQuantitiesPI977);

			var permissibleQuantitiesPI978 = UNDGPermissableQuantitiesHelper.GetPermissibleQuantitiesForPackingInstruction(SodiumBatteryConstants.RefPackingInstructions.PI978);
			var expectedPermissibleQuantitiesPI978 = new List<UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity>()
			{
				new UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity(SodiumBatteryConstants.RefPackingInstructions.PI978, PackingInstructionSectionTypeList.Codes.SectionI, 5, Core.Constants.Weight.Kilograms, 35, Core.Constants.Weight.Kilograms),
				new UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity(SodiumBatteryConstants.RefPackingInstructions.PI978, PackingInstructionSectionTypeList.Codes.SectionII, 5, Core.Constants.Weight.Kilograms, 5, Core.Constants.Weight.Kilograms)
			};
			AssertContainsExactElementsInExactOrder(expectedPermissibleQuantitiesPI978, permissibleQuantitiesPI978);

			var permissibleQuantities2 = UNDGPermissableQuantitiesHelper.GetPermissibleQuantitiesForPackingInstruction(SodiumBatteryConstants.RefPackingInstructions.Forbidden);
			AssertEquals(0, permissibleQuantities2.Count());

			var permissibleQuantities3 = UNDGPermissableQuantitiesHelper.GetPermissibleQuantitiesForPackingInstruction("467");
			AssertEquals(0, permissibleQuantities3.Count());
		}
	}
}
