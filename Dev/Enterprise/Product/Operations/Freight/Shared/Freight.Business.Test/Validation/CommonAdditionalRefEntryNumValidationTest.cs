using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonAdditionalRefEntryNumValidationTest : TestCaseWithFactory
	{
		public void TestValidateHIREntryType() => AssertEntryTypeValidation(CusEntryNumLookups.HIR);

		public void TestValidateCMREntryType() => AssertEntryTypeValidation(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CarrierMessageReference);

		void AssertEntryTypeValidation(string entryType)
		{
			var expectedError =  $"{entryType} is reserved for system use and cannot be manually entered.";

			var shipment = Factory.New<CommonShipment>();
			var entryNum = shipment.Numbers.AddNew();
			entryNum.CE_EntryType = entryType;
			AssertHasError(entryNum.CE_EntryTypeInfo, expectedError);

			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.Validation.ValidateCE_EntryType();
			AssertNoError(entryNum.CE_EntryTypeInfo, expectedError);

			Factory.Save();

			var shipment2 = new BusinessObjectFactory().Load<CommonShipment>(shipment.PK);
			var entryNum2 = shipment2.Numbers[0];
			AssertEquals(entryType, entryNum2.CE_EntryType);

			entryNum2.Validation.ValidateCE_EntryType();
			AssertNoError(entryNum2.CE_EntryTypeInfo, expectedError);
		}
	}
}
