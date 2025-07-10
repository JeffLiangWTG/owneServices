using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ImportJobDeclarationOfficeCodeValidationTest : CusCodeDataValidationTest
{
	public void TestCheckForRuleR1586AdditionalInfoInDeclaration()
	{
		const string messageError = "(R1586) – If there is an Additional Information code 00100, the Code of the Supervising Customs Office (SCO) is required and the first 6 characters must be the same as the first 6 characters of the Decl.Customs Office.";
		CombineAssertions(() =>
		{
			additionalInfoInDeclaration.CSI_Code = Constants.AdditionalInfoCodes._00100;
			customOffice.Validation.ValidateCY_Data();
			AssertHasMessageError("Additional Document code is 00100", customOffice.CY_DataInfo, messageError);

			customOffice.CY_Data = "PL451010";
			AssertHasMessageError("Custom Office code is not Empty", customOffice.CY_DataInfo, messageError);

			declaration.JE_CustomsOffice = "PL451001";
			customOffice.Validation.ValidateCY_Data();
			AssertNoMessageError("The first 6 characters of Custom Office code are as same as the first 6 characters of Decl.Custom Office code", customOffice.CY_DataInfo, messageError);

			customOffice.CY_Data = "PL451110";
			AssertHasMessageError("The first 6 characters of Custom Office code are not as same as the first 6 characters of Decl.Custom Office code", customOffice.CY_DataInfo, messageError);

			declaration.JE_CustomsOffice = ZString.Empty;
			customOffice.CY_Data = ZString.Empty;
			AssertHasMessageError("Custom Office code is Empty", customOffice.CY_DataInfo, messageError);

			additionalInfoInDeclaration.CSI_Code = Constants.AdditionalInfoCodes._00200;
			customOffice.Validation.ValidateCY_Data();
			AssertNoMessageError("Additional Document code is not 00100", customOffice.CY_DataInfo, messageError);
		});
	}

	public void TestCheckForRuleR1586AdditionalInfoInInvoiceHeader()
	{
		const string messageError = "(R1586) – If there is an Additional Information code 00100, the Code of the Supervising Customs Office (SCO) is required and the first 6 characters must be the same as the first 6 characters of the Decl.Customs Office.";
		CombineAssertions(() =>
		{
			additionalInfoInInvoiceHeader.CSI_Code = Constants.AdditionalInfoCodes._00100;
			customOffice.Validation.ValidateCY_Data();
			AssertHasMessageError("Additional Document code is 00100", customOffice.CY_DataInfo, messageError);

			customOffice.CY_Data = "PL451010";
			AssertHasMessageError("Custom Office code is not Empty", customOffice.CY_DataInfo, messageError);

			declaration.JE_CustomsOffice = "PL451001";
			customOffice.Validation.ValidateCY_Data();
			AssertNoMessageError("The first 6 characters of Custom Office code are as same as the first 6 characters of Decl.Custom Office code", customOffice.CY_DataInfo, messageError);

			customOffice.CY_Data = "PL451110";
			AssertHasMessageError("The first 6 characters of Custom Office code are not as same as the first 6 characters of Decl.Custom Office code", customOffice.CY_DataInfo, messageError);

			declaration.JE_CustomsOffice = ZString.Empty;
			customOffice.CY_Data = ZString.Empty;
			AssertHasMessageError("Custom Office code is Empty", customOffice.CY_DataInfo, messageError);

			additionalInfoInInvoiceHeader.CSI_Code = Constants.AdditionalInfoCodes._00200;
			customOffice.Validation.ValidateCY_Data();
			AssertNoMessageError("Additional Document code is not 00100", customOffice.CY_DataInfo, messageError);
		});
	}

	public void TestCheckForRuleR1586AdditionalInfoInInvoiceLine()
	{
		const string messageError = "(R1586) – If there is an Additional Information code 00100, the Code of the Supervising Customs Office (SCO) is required and the first 6 characters must be the same as the first 6 characters of the Decl.Customs Office.";
		CombineAssertions(() =>
		{
			additionalInfoInInvoiceLine.CSI_Code = Constants.AdditionalInfoCodes._00100;
			customOffice.Validation.ValidateCY_Data();
			AssertHasMessageError("Additional Document code is 00100", customOffice.CY_DataInfo, messageError);

			customOffice.CY_Data = "PL451010";
			AssertHasMessageError("Custom Office code is not Empty", customOffice.CY_DataInfo, messageError);

			declaration.JE_CustomsOffice = "PL451001";
			customOffice.Validation.ValidateCY_Data();
			AssertNoMessageError("The first 6 characters of Custom Office code are as same as the first 6 characters of Decl.Custom Office code", customOffice.CY_DataInfo, messageError);

			customOffice.CY_Data = "PL451110";
			AssertHasMessageError("The first 6 characters of Custom Office code are not as same as the first 6 characters of Decl.Custom Office code", customOffice.CY_DataInfo, messageError);

			declaration.JE_CustomsOffice = ZString.Empty;
			customOffice.CY_Data = ZString.Empty;
			AssertHasMessageError("Custom Office code is Empty", customOffice.CY_DataInfo, messageError);

			additionalInfoInInvoiceLine.CSI_Code = Constants.AdditionalInfoCodes._00200;
			customOffice.Validation.ValidateCY_Data();
			AssertNoMessageError("Additional Document code is not 00100", customOffice.CY_DataInfo, messageError);
		});
	}

	public void TestNoOfficeCodeInvalidNotification()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var office = declaration.CustomsOffices.AddNew("ABC");
		AssertNoNotifications(office.CY_CodeInfo);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		additionalInfoInDeclaration = declaration.AdditionalInfos.AddNew();
		additionalInfoInInvoiceHeader = declaration.Invoices.AddNew().AdditionalInfos.AddNew();
		additionalInfoInInvoiceLine = declaration.InvoiceLines.AddNew().AdditionalInfos.AddNew();
		customOffice = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.AuthorityControlCode);
	}

	JobDeclaration declaration;
	AdditionalInfo additionalInfoInDeclaration;
	AdditionalInfo additionalInfoInInvoiceHeader;
	AdditionalInfo additionalInfoInInvoiceLine;
	EuOfficeCode customOffice;
}
