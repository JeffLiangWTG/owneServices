using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Manifest.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	sealed class ABLEntryNumValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCE_EntryNum_ABT()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var entryNum1 = bill.CustomsEntryNumbers.AddNew();
			entryNum1.CE_EntryType = ZaLRNTypes.Codes.ABT;
			entryNum1.CE_EntryNum = ZString.Empty;
			AssertNoMessageErrors(entryNum1.CE_EntryNumInfo);
			var entryNum2 = bill.CustomsEntryNumbers.AddNew();
			entryNum2.CE_EntryType = ZaLRNTypes.Codes.ABT;
			entryNum2.CE_EntryNum = ZString.Empty;
			AssertNoMessageErrors(entryNum2.CE_EntryNumInfo);
			entryNum1.CE_EntryNum = "1234";
			entryNum2.CE_EntryNum = "1234";
			AssertHasMessageError(entryNum2.CE_EntryNumInfo, ABLEntryNumValidation.LRNNumbersCanOnlyBeEnteredOnce(ZaLRNTypes.Codes.ABT, true));
			entryNum2.CE_EntryNum = "1235";
			AssertNoMessageError(entryNum2.CE_EntryNumInfo, ABLEntryNumValidation.LRNNumbersCanOnlyBeEnteredOnce(ZaLRNTypes.Codes.ABT, true));
		}

		public void TestCheckCE_EntryNum_AFM()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var entryNum1 = bill.CustomsEntryNumbers.AddNew();
			entryNum1.CE_EntryType = ZaLRNTypes.Codes.AFM;
			entryNum1.CE_EntryNum = ZString.Empty;
			AssertNoMessageErrors(entryNum1.CE_EntryNumInfo);
			var entryNum2 = bill.CustomsEntryNumbers.AddNew();
			entryNum2.CE_EntryType = ZaLRNTypes.Codes.AFM;
			entryNum2.CE_EntryNum = ZString.Empty;
			AssertNoMessageErrors(entryNum2.CE_EntryNumInfo);
			entryNum1.CE_EntryNum = "1234";
			entryNum2.CE_EntryNum = "1234";
			AssertHasMessageError(entryNum2.CE_EntryNumInfo, ABLEntryNumValidation.LRNNumbersCanOnlyBeEnteredOnce(ZaLRNTypes.Codes.AFM, true));
			entryNum2.CE_EntryNum = "1235";
			AssertNoMessageError(entryNum2.CE_EntryNumInfo, ABLEntryNumValidation.LRNNumbersCanOnlyBeEnteredOnce(ZaLRNTypes.Codes.AFM, true));
		}

		public void TestCheckCE_EntryNum_MultipleBills()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var entryNum1 = bill1.CustomsEntryNumbers.AddNew();
			entryNum1.CE_EntryType = ZaLRNTypes.Codes.AFM;
			entryNum1.CE_EntryNum = "1234";
			var entryNum2 = bill1.CustomsEntryNumbers.AddNew();
			entryNum2.CE_EntryType = ZaLRNTypes.Codes.AFM;
			entryNum2.CE_EntryNum = "1234";

			var bill2 = header.Bills.AddNew();
			var entryNum3 = bill2.CustomsEntryNumbers.AddNew();
			entryNum3.CE_EntryType = ZaLRNTypes.Codes.AFM;
			entryNum3.CE_EntryNum = "1234";

			AssertHasMessageErrorContaining(entryNum2.CE_EntryNumInfo, ZaLRNTypes.Codes.AFM);
			AssertNoMessageErrorContaining(entryNum3.CE_EntryNumInfo, ZaLRNTypes.Codes.AFM);

			entryNum2.CE_EntryNum = "5678";
			AssertNoMessageErrorContaining(entryNum2.CE_EntryNumInfo, ZaLRNTypes.Codes.AFM);
		}
	}
}
