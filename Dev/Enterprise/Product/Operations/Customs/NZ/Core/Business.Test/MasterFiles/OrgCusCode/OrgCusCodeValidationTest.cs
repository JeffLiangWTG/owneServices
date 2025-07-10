using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Testing
{
	using CargoWise.EntityFramework.Testing;

	public class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOK_CustomsRegNo()
		{
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
			cusCode.OK_CustomsRegNo = "49091850";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "The entered GST Number is not valid.\r\nA GST Number must be 8 or 9 digits and can be formatted as 99-999-999 or 999-999-999 with the final digit a check-digit.");

			cusCode.OK_CustomsRegNo = "35901981";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "The entered GST Number is not valid.\r\nA GST Number must be 8 or 9 digits and can be formatted as 99-999-999 or 999-999-999 with the final digit a check-digit.");

			cusCode.OK_CustomsRegNo = "49098576";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "The entered GST Number is not valid.\r\nA GST Number must be 8 or 9 digits and can be formatted as 99-999-999 or 999-999-999 with the final digit a check-digit.");

			cusCode.OK_CustomsRegNo = "136410132";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "The entered GST Number is not valid.\r\nA GST Number must be 8 or 9 digits and can be formatted as 99-999-999 or 999-999-999 with the final digit a check-digit.");

			cusCode.OK_CustomsRegNo = "136410133";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "The entered GST Number is not valid.\r\nA GST Number must be 8 or 9 digits and can be formatted as 99-999-999 or 999-999-999 with the final digit a check-digit.");

			cusCode.OK_CustomsRegNo = "9125568";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "The entered GST Number is not valid.\r\nA GST Number must be 8 or 9 digits and can be formatted as 99-999-999 or 999-999-999 with the final digit a check-digit.");
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			organisation = Factory.New<OrgHeader>();
			organisation.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
		}

		OrgCusCode cusCode;
		OrgHeader organisation;

		#endregion
	}
}
