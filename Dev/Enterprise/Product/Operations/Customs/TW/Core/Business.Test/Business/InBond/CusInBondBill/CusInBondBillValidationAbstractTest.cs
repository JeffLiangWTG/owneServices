using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class CusInBondBillValidationAbstractTest<TCusInBondBillValidation> : BusinessObjectValidationTestCase
		where TCusInBondBillValidation : CusInBondBillValidation
	{
		public void TestParent()
		{
			var parent = CusInBondHeader.Bills.AddNew();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestCheckB0_HouseBillNumber()
		{
			var arrivalBill = CusInBondHeader.ArrivalBill;
			arrivalBill.B0_HouseBillNumber = "!XX123";
			AssertHasMessageError(arrivalBill.B0_HouseBillNumberInfo, ValidationConstants.MessageErrorIfNonAlphanumericCharacters);
			arrivalBill.B0_HouseBillNumber = "ABC123";
			AssertNoMessageError(arrivalBill.B0_HouseBillNumberInfo, ValidationConstants.MessageErrorIfNonAlphanumericCharacters);
		}

		public void SetUpCustomsOffice()
		{
			var customsOffice = Factory.New<ZZRefCusCodeListCombined>();
			customsOffice.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
			customsOffice.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			customsOffice.ZZD_Code = "AT";
			customsOffice.ZZD_StartDate = ZDateTime.Today;
			customsOffice.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			customsOffice.ZZD_IsSea = true;
			Factory.Save();
		}

		public CusInBondHeader CusInBondHeader => fCusInBondHeader ?? (fCusInBondHeader = Factory.New<CusInBondHeader>());
		CusInBondHeader fCusInBondHeader;
	}
}
