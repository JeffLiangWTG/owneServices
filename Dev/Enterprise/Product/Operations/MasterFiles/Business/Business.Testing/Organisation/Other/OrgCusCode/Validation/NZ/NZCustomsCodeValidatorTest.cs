using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class NZCustomsCodeValidatorTest : TestCaseWithFactory
	{
		public void TestValidateSupplierCodeCheckDigit()
		{
			NZCustomsCodeValidator validator = new NZCustomsCodeValidator(OrgCusCode.CodeTypes.SupplierCode);
			AssertEquals("00710841Y", "Y", validator.GetExpectedSupplierCodeCheckDigit("00710841Y"));
			AssertEquals("00710842W", "W", validator.GetExpectedSupplierCodeCheckDigit("00710842W"));
			AssertEquals("00710843T", "T", validator.GetExpectedSupplierCodeCheckDigit("00710843T"));
			AssertEquals("00710844Q", "Q", validator.GetExpectedSupplierCodeCheckDigit("00710844Q"));
			AssertEquals("00710845N", "N", validator.GetExpectedSupplierCodeCheckDigit("00710845N"));
			AssertEquals("00710846Z", "Z", validator.GetExpectedSupplierCodeCheckDigit("00710846Z"));
			AssertEquals("00710847X", "X", validator.GetExpectedSupplierCodeCheckDigit("00710847X"));
			AssertEquals("00710848V", "V", validator.GetExpectedSupplierCodeCheckDigit("00710848V"));
			AssertEquals("00710854M", "M", validator.GetExpectedSupplierCodeCheckDigit("00710854M"));
			AssertEquals("00710855Y", "Y", validator.GetExpectedSupplierCodeCheckDigit("00710855Y"));
			AssertEquals("00710855Y", "Y", validator.GetExpectedSupplierCodeCheckDigit("00710855Y"));
			AssertEquals("00710856W", "W", validator.GetExpectedSupplierCodeCheckDigit("00710856W"));
			AssertEquals("00710856W", "W", validator.GetExpectedSupplierCodeCheckDigit("00710856W"));
		}

		public void TestValidateClientCodeCheckDigit()
		{
			NZCustomsCodeValidator validator = new NZCustomsCodeValidator(OrgCusCode.CodeTypes.CustomsClientCode);
			AssertEquals("00727575H", "H", validator.GetExpectedClientCodeCheckDigit("00727575H"));
			AssertEquals("00728535D", "D", validator.GetExpectedClientCodeCheckDigit("00728535D"));
			AssertEquals("00765432E", "E", validator.GetExpectedClientCodeCheckDigit("00765432E"));
			AssertEquals("00780143C", "C", validator.GetExpectedClientCodeCheckDigit("00780143C"));
			AssertEquals("00782903F", "F", validator.GetExpectedClientCodeCheckDigit("00782903F"));
			AssertEquals("00799567K", "K", validator.GetExpectedClientCodeCheckDigit("00799567K"));
			AssertEquals("00945612A", "A", validator.GetExpectedClientCodeCheckDigit("00945612A"));
			AssertEquals("00957932L", "L", validator.GetExpectedClientCodeCheckDigit("00957932L"));
			AssertEquals("00984567E", "E", validator.GetExpectedClientCodeCheckDigit("00984567E"));
			AssertEquals("00987876K", "K", validator.GetExpectedClientCodeCheckDigit("00987876K"));
			AssertEquals("40006141G", "G", validator.GetExpectedClientCodeCheckDigit("40006141G"));
			AssertEquals("40006142E", "E", validator.GetExpectedClientCodeCheckDigit("40006142E"));
			AssertEquals("40006143C", "C", validator.GetExpectedClientCodeCheckDigit("40006143C"));
			AssertEquals("40006144A", "A", validator.GetExpectedClientCodeCheckDigit("40006144A"));
			AssertEquals("40006153L", "L", validator.GetExpectedClientCodeCheckDigit("40006153L"));
			AssertEquals("40006154J", "J", validator.GetExpectedClientCodeCheckDigit("40006154J"));
			AssertEquals("40006155G", "G", validator.GetExpectedClientCodeCheckDigit("40006155G"));
			AssertEquals("40006156E", "E", validator.GetExpectedClientCodeCheckDigit("40006156E"));
			AssertEquals("40006157C", "C", validator.GetExpectedClientCodeCheckDigit("40006157C"));
			AssertEquals("40006158A", "A", validator.GetExpectedClientCodeCheckDigit("40006158A"));
			AssertEquals("40006159K", "K", validator.GetExpectedClientCodeCheckDigit("40006159K"));
			AssertEquals("40006160C", "C", validator.GetExpectedClientCodeCheckDigit("40006160C"));
			AssertEquals("40006161A", "A", validator.GetExpectedClientCodeCheckDigit("40006161A"));
			AssertEquals("40006162K", "K", validator.GetExpectedClientCodeCheckDigit("40006162K"));
		}

		public void TestValidateTooShort()
		{
			CusCode.OK_CustomsRegNo = "1234T";
			AssertEquals("Too short", true, CusCode.OK_CustomsRegNoInfo.HasMessageErrors());
		}

		public void TestValidateWrongDigit()
		{
			CusCode.OK_CustomsRegNo = "1234567UT";
			AssertEquals("Wrong digit", true, CusCode.OK_CustomsRegNoInfo.HasMessageErrors());
		}

		public void TestValidateWrongLetter()
		{
			CusCode.OK_CustomsRegNo = "12345678(";
			AssertEquals("Wrong letter", true, CusCode.OK_CustomsRegNoInfo.HasMessageErrors());
		}

		public void TestValidateWrongCheckDigit()
		{
			CusCode.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;
			CusCode.OK_CustomsRegNo = "00805742A";
			AssertEquals("Wrong digit", true, CusCode.OK_CustomsRegNoInfo.HasMessageErrors());

			CusCode.OK_CustomsRegNo = "00805742W";
			AssertEquals("Valid digit", false, CusCode.OK_CustomsRegNoInfo.HasMessageErrors());
		}

		public void TestEndToEndNZSupplierCodeValidation()
		{
			CusCode.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;
			CusCode.OK_CustomsRegNo = "05742A";
			AssertEquals("HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", true, CusCode.OK_CustomsRegNoInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));
			AssertEquals("MessageErrors.Count", 1, CusCode.OK_CustomsRegNoInfo.GetMessageErrors().Count());
			CusCode.OK_CustomsRegNo = "805742A";
			AssertEquals("HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", false, CusCode.OK_CustomsRegNoInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));
			AssertEquals("HasMessageError(NZCustomsCodeValidator.CustomsCodeHasWrongCheckDigit)", true, CusCode.OK_CustomsRegNoInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeHasWrongCheckDigit + "W"));
			CusCode.OK_CustomsRegNo = "00805742A";
			AssertEquals("HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", false, CusCode.OK_CustomsRegNoInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));
			AssertEquals("HasMessageError(NZCustomsCodeValidator.CustomsCodeHasWrongCheckDigit)", true, CusCode.OK_CustomsRegNoInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeHasWrongCheckDigit + "W"));
			CusCode.OK_CustomsRegNo = "00805742W";
			AssertEquals("MessageErrors.Count", 0, CusCode.OK_CustomsRegNoInfo.GetMessageErrors().Count());
		}

		public void TestEndToEndNZClientCodeValidation()
		{
			CusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			CusCode.OK_CustomsRegNo = "47873A";
			AssertEquals("HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", true, CusCode.OK_CustomsRegNoInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));
			AssertEquals("MessageErrors.Count", 1, CusCode.OK_CustomsRegNoInfo.GetMessageErrors().Count());
			CusCode.OK_CustomsRegNo = "347873A";
			AssertEquals("HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", false, CusCode.OK_CustomsRegNoInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));
			AssertEquals("HasMessageError(NZCustomsCodeValidator.CustomsCodeHasWrongCheckDigit)", true, CusCode.OK_CustomsRegNoInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeHasWrongCheckDigit + "E"));
			CusCode.OK_CustomsRegNo = "00347873A";
			AssertEquals("HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", false, CusCode.OK_CustomsRegNoInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));
			AssertEquals("HasMessageError(NZCustomsCodeValidator.CustomsCodeHasWrongCheckDigit)", true, CusCode.OK_CustomsRegNoInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeHasWrongCheckDigit + "E"));
			CusCode.OK_CustomsRegNo = "00347873E";
			AssertEquals("MessageErrors.Count", 0, CusCode.OK_CustomsRegNoInfo.GetMessageErrors().Count());
		}

		OrgHeader Organisation;
		OrgCusCode CusCode;
		protected override void SetUp()
		{
			base.SetUp();
			Organisation = Factory.New<OrgHeader>();
			CusCode = Organisation.CustomsCodes.AddNew();
			CusCode.OK_RN_NKCodeCountry = "NZ";
			CusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
		}
	}
}
