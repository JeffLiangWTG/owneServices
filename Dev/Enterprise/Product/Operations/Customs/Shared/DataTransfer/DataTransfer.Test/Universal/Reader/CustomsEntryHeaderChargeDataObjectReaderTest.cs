using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestBasicCusEntryHeaderChargesFieldMappings()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryHeaderChargeDataObject = SetupEntryHeaderCharge();
			var entryHeaderChargeBO = new CustomsEntryHeaderChargeDataObjectReader(entryHeaderChargeDataObject, logger, CurrentCompanyHelper, entryHeader).ReadIntoBusinessObject();
			AssertCusEntryHeaderChargeContents(entryHeaderChargeBO, entryHeader.PK);

			AssertNotEquals("PreCondition", 1000.10m, entryHeaderChargeBO.C1_ChargeAmount);
			entryHeaderChargeBO.C1_ChargeAmount = 1000.10m;
			entryHeaderChargeBO = new CustomsEntryHeaderChargeDataObjectReader(entryHeaderChargeDataObject, logger, CurrentCompanyHelper, entryHeader).ReadIntoBusinessObject();
			AssertNotEquals("Should have been updated", 1000.10m, entryHeaderChargeBO.C1_ChargeAmount);
			AssertCusEntryHeaderChargeContents(entryHeaderChargeBO, entryHeader.PK);
		}

		[ExpectExceptionMessage(typeof(DataObjectReadFailureException), "The charge code BCH is not unique in this CusEntryHeaderChargesCollection.")]
		public void TestDuplicateChargesFieldMappings()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.IsImportingData = true;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var chargeToDuplicate = entryHeader.Charges.AddNew();
			var duplicatedCharge = entryHeader.Charges.AddNew();
			chargeToDuplicate.C1_ChargeType = "BCH";
			duplicatedCharge.C1_ChargeType = "BCH";
		}
	}
}
