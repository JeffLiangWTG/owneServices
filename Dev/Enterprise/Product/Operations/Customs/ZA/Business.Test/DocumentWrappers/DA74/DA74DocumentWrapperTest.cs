using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business.Business.Utilities;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	sealed class DA74DocumentWrapperTest : TestCaseWithFactory
	{
		public void TestTransportDocumentNumberInBusinessLogic()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			dec.JE_MasterBill = "MSCU123450";
			dec.JE_CarrierCode = "MSC";
			var testHeader = dec.CustomsEntryHeaders.AddNew();
			var tester = new DA74DocumentWrapper(testHeader);
			CombineAssertions(() =>
			{
				using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(true))
				{
					AssertEquals("SEA, TDTSegmentSplit is enabled", "MSC MSCU123450", tester.TransportDocumentNumberInBusinessLogic);
				}

				using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(false))
				{
					AssertEquals("SEA, TDTSegmentSplit is disabled", "MSC MSCU123450", tester.TransportDocumentNumberInBusinessLogic);
				}

				dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				AssertEquals("AIR", "MSC-U123450", tester.TransportDocumentNumberInBusinessLogic);
			});
		}

		public void TestPackagesSplitValues()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			entryHeader.CH_Packages = 9876;
			var tester = new DA74DocumentWrapper(entryHeader);
			CombineAssertions("TestPackage-Thousands-Hundreds-Tens-Units", () =>
			{
				AssertEquals("Thousands", 9, tester.PackageThousandsValue);
				AssertEquals("Hundreds", 8, tester.PackageHundredsValue);
				AssertEquals("Tens", 7, tester.PackageTensValue);
				AssertEquals("Ones", 6, tester.PackageOnesValue);
			});
		}
	}
}
