using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondAirWayBillValidatorTest : Customs.Business.Testing.BillValidatorTestClass
	{
		public void TestMasterBillFormatForAirBond()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			var bill1 = header.Bills.AddNew();
			var usCarrier = GetTestCarrierCombinedWithAirwayBillPrefixCode("578");
			bill1.B0_IssuerCode = usCarrier.UI_Code;
			bill1.B0_MasterBillNumber = "ABC123456478";
			AssertHasMessageError("Warnings for Air Master Bill number", bill1.B0_MasterBillNumberInfo, "The MAWB should contain 11 digits.");
			bill1.B0_MasterBillNumber = "78512345678";
			AssertNoMessageError(bill1.B0_MasterBillNumberInfo, "The MAWB should contain 11 digits.");
			usCarrier = GetTestCarrierCombinedWithAirwayBillPrefixCode("AMF");
			bill1.B0_MasterBillNumber = "ABC123456478";
			AssertNoMessageError(bill1.B0_MasterBillNumberInfo, "The MAWB should contain 11 digits.");
			bill1.B0_MasterBillNumber = "123";
			AssertHasMessageError("Warnings for Air Master Bill number", bill1.B0_MasterBillNumberInfo, CusInBondAirWayBillValidator.MasterBillLengthWarningMessage);
			bill1.B0_MasterBillNumber = "4781234A678";
			AssertHasMessageError("Warnings for Air Master Bill number", bill1.B0_MasterBillNumberInfo, CusInBondAirWayBillValidator.MasterBillFormatWarningMessage);
		}

		USCarrierCombined GetTestCarrierCombinedWithAirwayBillPrefixCode(ZString airwayBillPrefixCode)
		{
			var usCarrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, "A8"));
			if (usCarrier == null)
			{
				usCarrier = Factory.New<USCarrierCombined>();
				usCarrier.UI_Code = "A8";
				usCarrier.UI_ModeOfTransportation = "40";
				usCarrier.UI_Name = "Test Carrier";
			}

			usCarrier.UI_AirwayBillPrefix = airwayBillPrefixCode;
			return usCarrier;
		}
	}
}
