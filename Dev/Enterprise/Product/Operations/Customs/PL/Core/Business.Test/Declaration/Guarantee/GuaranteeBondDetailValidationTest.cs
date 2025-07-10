using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class GuaranteeBondDetailValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckPW_BondType_ListValidation()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(detail.PW_BondTypeInfo, "~", GuaranteeBondTypeList.Codes.Comprehensive);
	}

	public void TestCheckPW_BondType_Mandatory()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(detail.PW_BondTypeInfo);
	}

	public void TestCheckPW_BondNumber()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(detail.PW_BondNumberInfo);
		detail.PW_BondType = GuaranteeBondTypeList.Codes.Comprehensive;
		detail.PW_BondNumber = "BN";
		AssertNoMessageErrors(detail.PW_BondNumberInfo);

		detail.PW_BondNumber = "BA";
		AssertHasMessageError(detail.PW_BondNumberInfo, $"With Type {GuaranteeBondTypeList.Codes.Comprehensive} Guarantee Number can't contain \"A\"");

		detail.PW_BondType = GuaranteeBondTypeList.Codes.Individual;
		detail.PW_BondNumber = "BN";
		AssertNoMessageErrors(detail.PW_BondNumberInfo);

		detail.PW_BondNumber = "BA";
		AssertNoMessageErrors(detail.PW_BondNumberInfo);

		var dec = Factory.New<JobDeclaration>();
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();
		var detail1 = entryInstruction.Guarantees.AddNew();
		detail1.PW_BondNumber = "BN";
		var invoiceLine = dec.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		AssertNoMessageErrors(detail1.PW_BondNumberInfo);

		invoiceLine.JI_Procedure = "71";
		detail1.PW_BondNumber = "BA";
		AssertHasMessageError(detail1.PW_BondNumberInfo, "Guarantees not allowed, because an invoice line has procedure code starting with 71");
	}

	public void TestCheckPW_BondNumberNoUnnecessaryMessageError()
	{
		var expectedBothEmptyMessage = "At least a Reference (GRN) or a Reference 2 is required";
		var expectedBothEnteredMessage = "Both Reference (GRN) and Reference 2 cannot be entered in the same line";

		CombineAssertions(() =>
		{
			detail.PW_BondNumber2 = ZString.Empty;
			detail.PW_BondNumber = ZString.Empty;
			AssertEquals("Not Expect Both Empty PW_BondNumber Message", false, detail.PW_BondNumberInfo.Notifications.Any(x => x.Message == expectedBothEmptyMessage));

			detail.PW_BondNumber2 = "ABC";
			detail.PW_BondNumber = "ABC";
			AssertEquals("Not Expect Both Entered PW_BondNumber Message", false, detail.PW_BondNumberInfo.Notifications.Any(x => x.Message == expectedBothEnteredMessage));
		});
	}

	public void TestCheckPW_HolderIdentification()
	{
		detail.PW_BondType = GuaranteeBondTypeList.Codes.Comprehensive;
		AssertNoNotifications(detail.PW_HolderIdentificationInfo);

		detail.PW_BondType = GuaranteeBondTypeList.Codes.Individual;
		detail.PW_HolderIdentification = "HID";
		ValidationTestHelper.AssertIfIsEnteredMessageError(detail.PW_HolderIdentificationInfo);
	}

	public void TestCheckPW_Password_Comprehensive()
	{
		detail.PW_BondType = GuaranteeBondTypeList.Codes.Comprehensive;
		AssertNoNotifications(detail.PW_PasswordInfo);
	}

	public void TestCheckPW_Password_Individual()
	{
		detail.PW_BondType = GuaranteeBondTypeList.Codes.Individual;
		ValidationTestHelper.AssertIfIsEnteredMessageError(detail.PW_PasswordInfo);
	}

	public void TestCheckPW_Password_DigitsOnly()
	{
		CombineAssertions(() =>
		{
			const string accessCodeOnlyDigitsMessage = "Access Code should contain digits only";
			detail.Validation.ValidatePW_Password();
			AssertNoMessageError("No digits only message - empty", detail.PW_PasswordInfo, accessCodeOnlyDigitsMessage);
			detail.PW_Password = "FDSA";
			AssertHasMessageError("Digits only message", detail.PW_PasswordInfo, accessCodeOnlyDigitsMessage);
			detail.PW_Password = "222";
			AssertNoMessageError("No digits only message - correct", detail.PW_PasswordInfo, accessCodeOnlyDigitsMessage);
		});
	}

	public void TestCheckPW_Password_4Characters()
	{
		CombineAssertions(() =>
		{
			const string accessCodeFourCharactersMessage = "Access Code should contain 4 characters";
			detail.Validation.ValidatePW_Password();
			AssertNoMessageError("No four characters message - empty", detail.PW_PasswordInfo, accessCodeFourCharactersMessage);
			detail.PW_Password = "FDSA";
			AssertNoMessageError("No four characters message - correct", detail.PW_PasswordInfo, accessCodeFourCharactersMessage);
			detail.PW_Password = "222";
			AssertHasMessageError("Four characters message", detail.PW_PasswordInfo, accessCodeFourCharactersMessage);
		});
	}

	public void TestCheckPW_BondAmount_CannotBeNegative()
	{
		ValidationTestHelper.AssertErrorIfValueIsNegative(detail.PW_BondAmountInfo);
	}

	public void TestCheckPW_BondAmount()
	{
		CusEntryInstruction cusEntryInstruction = Factory.New<CusEntryInstruction>();
		GuaranteeBondDetail detail1 = cusEntryInstruction.Guarantees.AddNew();
		detail1.PW_BondAmount = 0;
		AssertNoMessageErrors(detail1.PW_BondAmountInfo);

		GuaranteeBondDetail detail2 = cusEntryInstruction.Guarantees.AddNew();
		detail2.PW_BondAmount = 0;
		AssertHasMessageError(detail2.PW_BondAmountInfo, "With multiple guarantees the amount can't be zero");

		detail1.PW_BondAmount = 10;
		AssertNoMessageErrors(detail1.PW_BondAmountInfo);

		detail2.PW_BondAmount = 20;
		AssertNoMessageErrors(detail2.PW_BondAmountInfo);
	}

	public void TestCheckPW_CPH_Guarantee()
	{
		const string invalidAccessCodeLengthMessageError = "Access Code must have 4 digits";
		const string missingAccessCodeMessageError = "Selected Guarantee requires Access Code.";

		detail.PW_BondType = GuaranteeBondTypeList.Codes.Comprehensive;

		detail.PW_Password = ZString.Empty;
		detail.Validation.ValidatePW_CPH_Guarantee();
		AssertHasMessageError(detail.PW_CPH_GuaranteeInfo, missingAccessCodeMessageError);

		detail.PW_Password = "PAS";
		detail.Validation.ValidatePW_CPH_Guarantee();
		AssertHasMessageError(detail.PW_CPH_GuaranteeInfo, invalidAccessCodeLengthMessageError);

		detail.PW_Password = "123";
		detail.Validation.ValidatePW_CPH_Guarantee();
		AssertHasMessageError(detail.PW_CPH_GuaranteeInfo, invalidAccessCodeLengthMessageError);

		detail.PW_Password = "PASS";
		detail.Validation.ValidatePW_CPH_Guarantee();
		AssertHasMessageError(detail.PW_CPH_GuaranteeInfo, invalidAccessCodeLengthMessageError);

		detail.PW_Password = "1234";
		detail.Validation.ValidatePW_CPH_Guarantee();
		AssertNoMessageError(detail.PW_CPH_GuaranteeInfo, invalidAccessCodeLengthMessageError);

		var missingHolderIDMessageError = "Selected Guarantee requires TIN.";
		detail.PW_HolderIdentification = ZString.Empty;
		detail.Validation.ValidatePW_CPH_Guarantee();
		AssertHasMessageError(detail.PW_CPH_GuaranteeInfo, missingHolderIDMessageError);

		detail.PW_HolderIdentification = "ABC";
		detail.Validation.ValidatePW_CPH_Guarantee();
		AssertNoMessageError(detail.PW_CPH_GuaranteeInfo, missingHolderIDMessageError);

		AssertNoMessageErrors(detail.PW_CPH_GuaranteeInfo);
	}

	public void TestCheckPW_BondFiledPort()
	{
		detail.Validation.ValidateAll();
		AssertNoNotifications("PW_BondFiledPort is not in use for Poland", detail.PW_BondFiledPortInfo);
	}

	protected override void SetUp()
	{
		base.SetUp();
		detail = Factory.New<GuaranteeBondDetail>();
	}
	GuaranteeBondDetail detail;
}
