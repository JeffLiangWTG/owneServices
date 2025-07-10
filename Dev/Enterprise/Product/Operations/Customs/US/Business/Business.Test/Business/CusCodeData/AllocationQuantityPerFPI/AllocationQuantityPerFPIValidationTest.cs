using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AllocationQuantityPerFPIValidationTest : TestCaseWithFactory
	{
		public void TestCheckManufacturerOrgPK()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			AssertNoNotifications(QuantityPerFPI.ManufacturerOrgPKInfo);
			QuantityPerFPI.US_AllocationQuantity = 100m;
			AssertHasMessageErrorContaining(QuantityPerFPI.ManufacturerOrgPKInfo, MandatoryValidation.YouHaveNotEntered);
			QuantityPerFPI.US_OA_ManufacturerAddress = organization.MainAddress.PK;
			AssertNoMessageErrorContaining(QuantityPerFPI.ManufacturerOrgPKInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCY_Code()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			AssertNoNotifications(QuantityPerFPI.CY_CodeInfo);
			QuantityPerFPI.US_OA_ManufacturerAddress = organization.MainAddress.PK;
			AssertHasMessageErrorContaining(QuantityPerFPI.CY_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			QuantityPerFPI.US_OA_ManufacturerAddress = ZGuid.Empty;
			QuantityPerFPI.US_AllocationQuantity = 123m;
			AssertHasMessageErrorContaining(QuantityPerFPI.CY_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			var address = organization.Addresses.AddNew();
			address.FillWithValidTestData();
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ForeignProducerIdentifierBeer, "B1234");
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ForeignProducerIdentifierSpirits, "S5678");
			QuantityPerFPI.US_OA_ManufacturerAddress = address.PK;
			QuantityPerFPI.US_ForeignProducerIdentifier = "B5678";
			AssertNoMessageErrorContaining(QuantityPerFPI.CY_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(QuantityPerFPI.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			QuantityPerFPI.US_ForeignProducerIdentifier = "S5678";
			AssertNoMessageErrorContaining(QuantityPerFPI.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_AllocationQuantity()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			AssertNoNotifications(QuantityPerFPI.US_AllocationQuantityInfo);
			QuantityPerFPI.US_OA_ManufacturerAddress = organization.MainAddress.PK;
			AssertHasMessageErrorContaining(QuantityPerFPI.US_AllocationQuantityInfo, MandatoryValidation.YouHaveNotEntered);
			QuantityPerFPI.US_OA_ManufacturerAddress = ZGuid.Empty;
			QuantityPerFPI.US_ForeignProducerIdentifier = "1234";
			AssertHasMessageErrorContaining(QuantityPerFPI.US_AllocationQuantityInfo, MandatoryValidation.YouHaveNotEntered);
			QuantityPerFPI.US_AllocationQuantity = 1m;
			AssertNoMessageErrorContaining(QuantityPerFPI.US_AllocationQuantityInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoErrors(QuantityPerFPI.US_AllocationQuantityInfo);
			QuantityPerFPI.US_AllocationQuantity = 123456789m;
			AssertHasError(QuantityPerFPI.US_AllocationQuantityInfo, "The number 123,456,789 is too large, the maximum value allowed for Allocation Quantity is 99,999,999.9999.");
		}

		AllocationQuantityPerFPI fQuantityPerFPI;
		AllocationQuantityPerFPI QuantityPerFPI => fQuantityPerFPI ?? (fQuantityPerFPI = Factory.New<AllocationQuantityPerFPI>());
	}
}
