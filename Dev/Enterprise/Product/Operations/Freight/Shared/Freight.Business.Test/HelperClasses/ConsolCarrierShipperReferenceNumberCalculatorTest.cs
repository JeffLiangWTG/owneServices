using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConsolCarrierShipperReferenceNumberCalculatorTest : TestCaseWithFactory
	{
		public void TestGetCSRAdditionalReferenceNumber()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C0000005";

			AssertEquals(string.Empty, ConsolCarrierShipperReferenceNumberCalculator.GetCarrierShipperReferenceNumber(consol));

			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryNum = "C0000005-V8";

			AssertEquals("C0000005-V8", ConsolCarrierShipperReferenceNumberCalculator.GetCarrierShipperReferenceNumber(consol));
		}

		public void TestHasMaximumCarrierShipperReferenceNumber()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C0000005";

			Assert(!ConsolCarrierShipperReferenceNumberCalculator.IsCRSNumberOverLimit(consol));

			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryNum = "C0000005-V8";

			Assert(!ConsolCarrierShipperReferenceNumberCalculator.IsCRSNumberOverLimit(consol));

			entryNum.CE_EntryNum = "C0000005-V9";
			Assert(ConsolCarrierShipperReferenceNumberCalculator.IsCRSNumberOverLimit(consol));
		}

		public void TestPopulateShipperReferenceNumber()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C0000005";

			Factory.Save();

			AssertPopulateShipperReferenceNumber("C0000005-V1", consol);
			AssertPopulateShipperReferenceNumber("C0000005-V2", consol);
			AssertPopulateShipperReferenceNumber("C0000005-V3", consol);
			AssertPopulateShipperReferenceNumber("C0000005-V4", consol);
			AssertPopulateShipperReferenceNumber("C0000005-V5", consol);
			AssertPopulateShipperReferenceNumber("C0000005-V6", consol);
			AssertPopulateShipperReferenceNumber("C0000005-V7", consol);
			AssertPopulateShipperReferenceNumber("C0000005-V8", consol);
			AssertPopulateShipperReferenceNumber("C0000005-V9", consol);
			AssertPopulateShipperReferenceNumber("C0000005-V10", consol);
			AssertPopulateShipperReferenceNumber("C0000005-V11", consol);
		}

		public void TestGetVersionFromCarrierShipperReferenceNumber()
		{
			AssertEquals(1, ConsolCarrierShipperReferenceNumberCalculator.GetVersionFromCarrierShipperReferenceNumber("C00001-V1"));
			AssertEquals(2, ConsolCarrierShipperReferenceNumberCalculator.GetVersionFromCarrierShipperReferenceNumber("C00001-V2"));
			AssertEquals(0, ConsolCarrierShipperReferenceNumberCalculator.GetVersionFromCarrierShipperReferenceNumber("C00001"));
			AssertEquals(0, ConsolCarrierShipperReferenceNumberCalculator.GetVersionFromCarrierShipperReferenceNumber("C00001-VX"));
		}

		public void TestGetConsolIDFromCarrierShipperReferenceNumber()
		{
			AssertEquals("C00001", ConsolCarrierShipperReferenceNumberCalculator.GetConsolIDFromCarrierShipperReferenceNumber("C00001-V1"));
			AssertEquals("C00001", ConsolCarrierShipperReferenceNumberCalculator.GetConsolIDFromCarrierShipperReferenceNumber("C00001-V2"));
			AssertEquals(string.Empty, ConsolCarrierShipperReferenceNumberCalculator.GetConsolIDFromCarrierShipperReferenceNumber("C00001"));
			AssertEquals(string.Empty, ConsolCarrierShipperReferenceNumberCalculator.GetConsolIDFromCarrierShipperReferenceNumber("C00001-VX"));
		}

		public void TestIsValidCarrierShipperReferenceNumber()
		{
			Assert(ConsolCarrierShipperReferenceNumberCalculator.IsValidCarrierShipperReferenceNumber("C00001-V1"));
			Assert(ConsolCarrierShipperReferenceNumberCalculator.IsValidCarrierShipperReferenceNumber("C00001-V2"));
			Assert(!ConsolCarrierShipperReferenceNumberCalculator.IsValidCarrierShipperReferenceNumber("C00001"));
			Assert(!ConsolCarrierShipperReferenceNumberCalculator.IsValidCarrierShipperReferenceNumber("C00001-VX"));
			Assert(!ConsolCarrierShipperReferenceNumberCalculator.IsValidCarrierShipperReferenceNumber("C00%01-VX"));
		}

		void AssertPopulateShipperReferenceNumber(string expectedVersion, CommonConsol consol)
		{
			ConsolCarrierShipperReferenceNumberCalculator.PopulateShipperReferenceNumber(consol);
			AssertEquals(expectedVersion, FindCSRAdditionalReferenceNumber(consol).CE_EntryNum);
		}

		static CusEntryNumber FindCSRAdditionalReferenceNumber(CommonConsol consol)
		{
			return consol.Numbers.Find(num => num.CE_EntryType == ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference
									&& num.CE_EntryIsSystemGenerated).FirstOrDefault();
		}
	}
}
