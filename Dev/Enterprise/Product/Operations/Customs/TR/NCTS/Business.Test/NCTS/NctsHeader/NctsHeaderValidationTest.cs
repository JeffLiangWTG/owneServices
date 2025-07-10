using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.NCTS.Business.Test
{
	public class NctsHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckStampDutyStatus_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsHeader.StampDutyStatusInfo);
		}

		public void TestCheckStampDutyStatus_List()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsHeader.StampDutyStatusInfo, "A", StampDutyStatusCodeList.Codes.D2);
		}

		public void TestCheckStampDuty_Mandatory()
		{
			nctsHeader.StampDutyStatus = StampDutyStatusCodeList.Codes.D3;
			nctsHeader.StampDuty = ZDecimal.Zero;

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsHeader.StampDutyInfo, "Please enter Stamp Duty if Stamp Duty Status Code value is 3.");
		}

		public void TestCheckBH_OH_Carrier()
		{
			const string expectedWarning = "Only the first 35 characters will be sent in the message";

			var orgHeaderLong = Factory.New<OrgHeader>();
			orgHeaderLong.OH_Code = "XYZAAZ";
			orgHeaderLong.OH_FullName = "1234567890123456789012345678901234567890";

			var addressLong = orgHeaderLong.MainAddress;
			addressLong.OA_OH = orgHeaderLong.PK;
			addressLong.CompanyName = "Company Name";
			addressLong.Address1 = "12345678901234567890";
			addressLong.Address2 = "12345678901234567890";
			addressLong.City = "IST";
			addressLong.Postcode = "340300";
			addressLong.OA_RN_NKCountryCode = "TR";

			nctsHeader.BH_OH_Carrier = orgHeaderLong.PK;
			AssertHasWarningContaining("BH_OH_Carrier Company Name & Address is Long", nctsHeader.BH_OH_CarrierInfo, expectedWarning);

			var orgHeaderNotLong = Factory.New<OrgHeader>();
			orgHeaderNotLong.OH_Code = "XYZAAY";
			orgHeaderNotLong.OH_FullName = "12345678901234567890123456789012345";
			var addressNotLong = orgHeaderNotLong.MainAddress;
			addressNotLong.OA_OH = orgHeaderNotLong.PK;
			addressNotLong.CompanyName = "Company Name 2";
			addressNotLong.Address1 = "12345678901234567";
			addressNotLong.Address2 = "12345678901234567";
			addressNotLong.City = "IST";
			addressNotLong.Postcode = "340300";
			addressNotLong.OA_RN_NKCountryCode = "TR";

			nctsHeader.BH_OH_Carrier = orgHeaderNotLong.PK;
			AssertNoWarningContaining("BH_OH_Carrier Company Name & Address is NOT Long", nctsHeader.BH_OH_CarrierInfo, expectedWarning);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_FTZMove = true;
		}
		NctsHeader nctsHeader;
	}
}
