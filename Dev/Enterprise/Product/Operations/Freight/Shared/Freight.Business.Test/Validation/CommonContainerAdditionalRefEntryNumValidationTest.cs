using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonContainerAdditionalRefEntryNumValidationTest : TestCaseWithFactory
	{
		public void TestValidateCMREntryType() => AssertEntryTypeValidation(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CarrierMessageReference);

		void AssertEntryTypeValidation(string entryType)
		{
			var expectedError =  $"{entryType} is reserved for system use and cannot be manually entered.";

			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			var entryNum = container.AdditionalReferenceNumbers.AddNew();
			entryNum.CE_EntryType = entryType;
			AssertHasError(entryNum.CE_EntryTypeInfo, expectedError);

			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.Validation.ValidateCE_EntryType();
			AssertNoError(entryNum.CE_EntryTypeInfo, expectedError);

			Factory.Save();

			var container2 = new BusinessObjectFactory().Load<CommonContainer>(container.PK);
			var entryNum2 = container2.AdditionalReferenceNumbers.Cast<CusEntryNumber>().First();
			AssertEquals(entryType, entryNum2.CE_EntryType);

			entryNum2.Validation.ValidateCE_EntryType();
			AssertNoError(entryNum2.CE_EntryTypeInfo, expectedError);
		}
	}
}
