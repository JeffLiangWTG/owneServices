using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Common.Testing
{
	class WarehouseValidationHelperTest : TestCase
	{
		public void TestValidateDestinationPalletID()
		{
			AssertEquals("No Error message is returned.", string.Empty, WarehouseValidationHelper.ValidatePalletID("ABC"));
			AssertEquals("No Error message is returned.", string.Empty, WarehouseValidationHelper.ValidatePalletID(""));

			var destinationPalletId = "1234567890123456789012345678901";
			Assert("Precondition: Destination pallet id exceeds maximum length for pallet ids.", destinationPalletId.Length > WhsDocketLineSchema.WE_PalletID.MaxLength);
			AssertEquals("Error response is returned.",
					"WE_PalletID exceeds maximum length allowed. The maximum length of this property is 30 characters, but 31 were entered.",
					WarehouseValidationHelper.ValidatePalletID(destinationPalletId));
		}

		public void TestValidateExternalReference()
		{
			AssertEquals("No Error message is returned.", string.Empty, WarehouseValidationHelper.ValidateDocketExternalReference("ABC", "Reference"));
			AssertEquals("No Error message is returned.", string.Empty, WarehouseValidationHelper.ValidateDocketExternalReference("", "Reference"));

			var externalReference = "123456789012345678901234567890123456";
			Assert("Precondition: External reference exceeds maximum length for WD_ExternalReference.", externalReference.Length > WhsDocketSchema.WD_ExternalReference.MaxLength);
			AssertEquals("Error response is returned.",
					"Reference exceeds maximum length allowed. The maximum length of this property is 35 characters, but 36 were entered.",
					WarehouseValidationHelper.ValidateDocketExternalReference(externalReference, "Reference"));
		}
	}
}
