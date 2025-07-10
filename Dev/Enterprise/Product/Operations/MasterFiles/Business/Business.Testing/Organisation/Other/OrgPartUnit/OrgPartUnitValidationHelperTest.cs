using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.MasterFiles.Business.OrgPartUnitValidationHelper;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgPartUnitValidationHelperTest : TestCaseWithFactory
	{
		public void TestValidatePackTypes_ErrorRegistryEnabled_ValidTypes()
		{
			using (RawDataRegistry.Instance.UnitConversionPackTypesValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var part = Factory.New<OrgSupplierPart>();
				part.OP_Desc = ""; // Use this to retrieve any error message.
				var unit1 = part.PartUnits.AddNew();
				var unit2 = part.PartUnits.AddNew();
				unit1.OF_PackType = "plt ";
				unit1.OF_ParentPackType = "KG";
				unit2.OF_PackType = "L";
				unit2.OF_ParentPackType = "cc";

				var returnedMessage = "";

				var result = ValidatePackTypes(Factory, part,
					(p) => p.PartUnits.ToList<OrgPartUnit>(),
					(c) => c.OF_PackType,
					(c) => c.OF_ParentPackType,
					(p, message) => returnedMessage = message);

				Assert("Pack types should pass validation", result);
				AssertEquals("No error message should be produced", "", returnedMessage);
			}
		}
		public void TestValidatePackTypes_ErrorRegistryDisabled_InvalidTypes() => TestValidatePackTypes_InvalidTypes(registryItemEnabled: false);

		public void TestValidatePackTypes_ErrorRegistryEnabled_InvalidTypes() => TestValidatePackTypes_InvalidTypes(registryItemEnabled: true);

		void TestValidatePackTypes_InvalidTypes(bool registryItemEnabled)
		{
			using (RawDataRegistry.Instance.UnitConversionPackTypesValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItemEnabled))
			{
				var part = Factory.New<OrgSupplierPart>();
				var unit1 = part.PartUnits.AddNew();
				var unit2 = part.PartUnits.AddNew();
				unit1.OF_PackType = "plt";
				unit1.OF_ParentPackType = "XXX";
				unit2.OF_PackType = "YYY";
				unit2.OF_ParentPackType = "L";

				var returnedMessage = "";
				var numberPackTypesChecked = 0;

				var result = ValidatePackTypes(Factory, part,
					(p) => p.PartUnits.ToList<OrgPartUnit>(),
					(c) =>
					{
						var packType = c.OF_PackType;
						numberPackTypesChecked++;
						return packType;
					},
					(c) =>
					{
						var packType = c.OF_ParentPackType;
						numberPackTypesChecked++;
						return packType;
					},
					(p, message) => returnedMessage = message);

				int expectedNumberPackTypesChecked;
				string expectedMessage;
				if (registryItemEnabled)
				{
					expectedNumberPackTypesChecked = 4;
					expectedMessage = "Error during import: Invalid package types in unit conversion: XXX, YYY";
				}
				else
				{
					expectedNumberPackTypesChecked = 0;
					expectedMessage = "";
				}

				AssertEquals("Validation outcome", !registryItemEnabled, result);
				AssertEquals("Error message", expectedMessage, returnedMessage);
				AssertEquals("Number of pack types that have been checked", expectedNumberPackTypesChecked, numberPackTypesChecked);
			}
		}
	}
}
