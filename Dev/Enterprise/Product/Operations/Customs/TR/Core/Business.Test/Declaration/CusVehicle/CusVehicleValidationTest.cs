using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	class CusVehicleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCVH_RegistrationNumber_IsLettersAndNumbersOnly()
		{
			Vehicle.CVH_RegistrationNumber = "$100";
			AssertHasMessageErrorContaining(Vehicle.CVH_RegistrationNumberInfo, "Registration No should consist of alphanumeric characters.");

			Vehicle.CVH_RegistrationNumber = "S100";
			AssertNoMessageErrors(Vehicle.CVH_RegistrationNumberInfo);
		}

		public void TestCheckBrandValue_MandatoryValidation()
		{
			var vehicle = Factory.New<CusVehicle>();
			vehicle.Validation.ValidateAll();
			AssertHasMessageErrorContaining(vehicle.BrandValueInfo, MandatoryValidation.YouHaveNotEntered);

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			vehicle = invoiceLine.Vehicles.AddNew();
			invoiceLine.JI_LinePrice = 100m;
			AssertNoMessageErrors(vehicle.BrandValueInfo);
		}

		public void TestCheckBrandValueInTRY_MandatoryValidation()
		{
			var vehicle = Factory.New<CusVehicle>();
			vehicle.Validation.ValidateBrandValueInTRY();
			AssertHasMessageErrorContaining(vehicle.BrandValueInTRYInfo, MandatoryValidation.YouHaveNotEntered);

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			vehicle = invoiceLine.Vehicles.AddNew();
			invoiceLine.JI_LinePrice = 100m;
			vehicle.Validation.ValidateBrandValueInTRY();
			AssertHasMessageErrorContaining(vehicle.BrandValueInTRYInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceHeader.JZ_InvoiceCurrExRate = 9.985700m;
			vehicle.Validation.ValidateBrandValueInTRY();
			AssertNoMessageErrors(vehicle.BrandValueInTRYInfo);

			invoiceLine.JI_LinePrice = 0m;
			vehicle.Validation.ValidateBrandValueInTRY();
			AssertHasMessageErrorContaining(vehicle.BrandValueInTRYInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCVH_SerialNumber_MandatoryValidation()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Vehicle.CVH_SerialNumberInfo);
		}

		public void TestCheckCVH_ModelYear_MandatoryValidation()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Vehicle.CVH_ModelYearInfo);
		}

		public void TestCheckCVH_ModelYear_ValidYear()
		{
			Vehicle.CVH_ModelYear = "AC12";
			AssertHasMessageErrorContaining(Vehicle.CVH_ModelYearInfo, "Please enter a valid Model Year.");

			Vehicle.CVH_ModelYear = "2021";
			AssertNoMessageErrors(Vehicle.CVH_ModelYearInfo);
		}

		public void TestCheckCVH_ModelName_MandatoryValidation()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Vehicle.CVH_ModelNameInfo);
		}

		public void TestCheckCVH_Color_MandatoryValidation()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Vehicle.CVH_ColorInfo);
		}

		public void TestCheckCVH_VehicleIdentificationNumber_MandatoryValidation()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Vehicle.CVH_VehicleIdentificationNumberInfo);
		}

		public void TestCheckCVH_VehicleIdentificationNumber_Length()
		{
			Vehicle.CVH_VehicleIdentificationNumber = "ABC123";
			AssertHasMessageErrorContaining("Should be 17 characters long", Vehicle.CVH_VehicleIdentificationNumberInfo, "VIN should be 17 alphanumeric characters.");

			Vehicle.CVH_VehicleIdentificationNumber = "1234567890ABCDEFG";
			AssertNoMessageErrors("Should be 17 characters long", Vehicle.CVH_VehicleIdentificationNumberInfo);
		}

		public void TestCheckCVH_VehicleIdentificationNumber_IsLettersAndNumbersOnly()
		{
			Vehicle.CVH_VehicleIdentificationNumber = "$1234567890ABCDEF";
			AssertHasMessageErrorContaining("Should be alphanumeric characters", Vehicle.CVH_VehicleIdentificationNumberInfo, "VIN should be 17 alphanumeric characters.");

			Vehicle.CVH_VehicleIdentificationNumber = "1234567890ABCDEFG";
			AssertNoMessageErrors("Should be alphanumeric characters", Vehicle.CVH_VehicleIdentificationNumberInfo);
		}

		public void TestCheckCVH_VehicleIdentificationNumber_UniqueNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var vehicle1 = invoiceLine.Vehicles.AddNew();
			vehicle1.CVH_VehicleIdentificationNumber = "1234567890ABCDEFG";
			AssertNoMessageErrors("Should be Unique Number", vehicle1.CVH_VehicleIdentificationNumberInfo);

			var vehicle2 = invoiceLine.Vehicles.AddNew();
			vehicle2.CVH_VehicleIdentificationNumber = "1234567890ABCDEFG";
			AssertHasMessageErrorContaining("Should be Unique Number", vehicle2.CVH_VehicleIdentificationNumberInfo, "This VIN Number already exists in Invoice.");

			vehicle2.CVH_VehicleIdentificationNumber = "1234567890ABCDEFX";
			AssertNoMessageErrors("Should be Unique Number", vehicle2.CVH_VehicleIdentificationNumberInfo);
		}

		public void TestCheckBrandNameMandatoryValidation()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Vehicle.CVH_BrandNameInfo);
		}

		CusVehicle Vehicle => vehicle ?? (vehicle = Factory.New<CusVehicle>());
		CusVehicle vehicle;
	}
}
