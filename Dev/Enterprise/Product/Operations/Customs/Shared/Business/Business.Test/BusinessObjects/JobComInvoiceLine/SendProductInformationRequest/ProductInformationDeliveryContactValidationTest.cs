using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing;

sealed class ProductInformationDeliveryContactValidationTest : TestCaseWithFactory
{
	public void TestValidateSupplier()
	{
		var supplierList = new CodeDescriptionPairList();
		supplierList.AddPair("SUP1", "Supplier 1");
		var productInformationDeliveryContact = new ProductInformationDeliveryContact(Factory, supplierList);

		productInformationDeliveryContact.Validation.ValidateAll();
		AssertHasError(productInformationDeliveryContact.SupplierInfo, "Please enter a Supplier.");

		productInformationDeliveryContact.Supplier = "SUP0";
		AssertHasError(productInformationDeliveryContact.SupplierInfo, "Enter a valid Supplier.");

		productInformationDeliveryContact.Supplier = "SUP1";
		AssertNoErrors(productInformationDeliveryContact.SupplierInfo);
	}
}
