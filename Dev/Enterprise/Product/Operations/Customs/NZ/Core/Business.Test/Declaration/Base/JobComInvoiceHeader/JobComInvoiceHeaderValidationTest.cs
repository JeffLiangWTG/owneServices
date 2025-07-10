using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing;

class JobComInvoiceHeaderValidationTest : Customs.Business.Testing.InvoiceHeaderValidationTest
{
	public void TestCheckIsGSTPrePaidAndVendorIdentifier()
	{
		var writeOffDec = Factory.NewWithValidTestData<JobDeclaration>();
		writeOffDec.JE_MessageType = JobMessageTypeList.Codes.Import;
		writeOffDec.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
		writeOffDec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
		var invoice = writeOffDec.Invoices.AddNew();
		AssertIsGSTPrePaidAndVendorIdentifier(invoice);
	}

	void AssertIsGSTPrePaidAndVendorIdentifier(JobComInvoiceHeader invoice)
	{
		invoice.Validation.ValidateAll();
		AssertNoMessageErrorContaining(invoice.JZ_IsGSTPrePaidInfo, "GST Prepaid must be entered when Overseas Registered Supplier GST Number is entered.");
		AssertNoMessageErrorContaining(invoice.JZ_SupplierGSTNumberInfo, "Overseas Registered Supplier GST Number must be entered when GST Prepaid is entered.");
		invoice.JZ_SupplierGSTNumber = "AA111";
		invoice.Validation.ValidateAll();
		AssertHasMessageErrorContaining(invoice.JZ_IsGSTPrePaidInfo, "GST Prepaid must be entered when Overseas Registered Supplier GST Number is entered.");
		AssertNoMessageErrorContaining(invoice.JZ_SupplierGSTNumberInfo, "Overseas Registered Supplier GST Number must be entered when GST Prepaid is entered.");
		invoice.JZ_IsGSTPrePaid = "Y";
		invoice.Validation.ValidateAll();
		AssertNoMessageErrorContaining(invoice.JZ_IsGSTPrePaidInfo, "The code you have selected is not in the list.");
		AssertNoMessageErrorContaining(invoice.JZ_IsGSTPrePaidInfo, "GST Prepaid must be entered when Overseas Registered Supplier GST Number is entered.");
		AssertNoMessageErrorContaining(invoice.JZ_SupplierGSTNumberInfo, "Overseas Registered Supplier GST Number must be entered when GST Prepaid is entered.");
		invoice.JZ_SupplierGSTNumber = "";
		invoice.Validation.ValidateAll();
		AssertNoMessageErrorContaining(invoice.JZ_IsGSTPrePaidInfo, "GST Prepaid must be entered when Overseas Registered Supplier GST Number is entered.");
		AssertHasMessageErrorContaining(invoice.JZ_SupplierGSTNumberInfo, "Overseas Registered Supplier GST Number must be entered when GST Prepaid is entered.");
		invoice.JZ_IsGSTPrePaid = "X";
		invoice.Validation.ValidateAll();
		AssertHasMessageErrorContaining(invoice.JZ_IsGSTPrePaidInfo, "The code you have selected is not in the list.");

		invoice.JZ_SupplierGSTNumber = "49091850";
		AssertNoMessageError(invoice.JZ_SupplierGSTNumberInfo, "The entered GST Number is not valid.\r\nA GST Number must be 8 or 9 digits and can be formatted as 99-999-999 or 999-999-999 with the final digit a check-digit.");

		invoice.JZ_SupplierGSTNumber = "35901981";
		AssertNoMessageError(invoice.JZ_SupplierGSTNumberInfo, "The entered GST Number is not valid.\r\nA GST Number must be 8 or 9 digits and can be formatted as 99-999-999 or 999-999-999 with the final digit a check-digit.");

		invoice.JZ_SupplierGSTNumber = "49098576";
		AssertNoMessageError(invoice.JZ_SupplierGSTNumberInfo, "The entered GST Number is not valid.\r\nA GST Number must be 8 or 9 digits and can be formatted as 99-999-999 or 999-999-999 with the final digit a check-digit.");

		invoice.JZ_SupplierGSTNumber = "136410132";
		AssertNoMessageError(invoice.JZ_SupplierGSTNumberInfo, "The entered GST Number is not valid.\r\nA GST Number must be 8 or 9 digits and can be formatted as 99-999-999 or 999-999-999 with the final digit a check-digit.");

		invoice.JZ_SupplierGSTNumber = "136410133";
		AssertHasMessageError(invoice.JZ_SupplierGSTNumberInfo, "The entered GST Number is not valid.\r\nA GST Number must be 8 or 9 digits and can be formatted as 99-999-999 or 999-999-999 with the final digit a check-digit.");

		invoice.JZ_SupplierGSTNumber = "9125568";
		AssertHasMessageError(invoice.JZ_SupplierGSTNumberInfo, "The entered GST Number is not valid.\r\nA GST Number must be 8 or 9 digits and can be formatted as 99-999-999 or 999-999-999 with the final digit a check-digit.");
	}

	public void TestCheckSupplierNumberPrepaidCombination()
	{
		const string expectedMessage = "The Supplier GST Number / Prepaid combination must be the same on all Invoices.";

		var writeOffDec = Factory.NewWithValidTestData<JobDeclaration>();
		writeOffDec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
		writeOffDec.JE_MessageType = JobMessageTypeList.Codes.Import;
		writeOffDec.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;

		var invoice1 = writeOffDec.Invoices.AddNew();

		AssertNoMessageErrorContaining(invoice1.JZ_IsGSTPrePaidInfo, expectedMessage);
		AssertNoMessageErrorContaining(invoice1.JZ_SupplierGSTNumberInfo, expectedMessage);

		invoice1.JZ_SupplierGSTNumber = "AA111";
		invoice1.Validation.ValidateAll();
		AssertNoMessageErrorContaining(invoice1.JZ_IsGSTPrePaidInfo, expectedMessage);
		AssertNoMessageErrorContaining(invoice1.JZ_SupplierGSTNumberInfo, expectedMessage);

		invoice1.JZ_IsGSTPrePaid = "Y";
		invoice1.Validation.ValidateAll();
		AssertNoMessageErrorContaining(invoice1.JZ_IsGSTPrePaidInfo, expectedMessage);
		AssertNoMessageErrorContaining(invoice1.JZ_SupplierGSTNumberInfo, expectedMessage);

		var invoice2 = writeOffDec.Invoices.AddNew();
		invoice2.Validation.ValidateAll();
		AssertHasMessageErrorContaining(invoice2.JZ_IsGSTPrePaidInfo, expectedMessage);
		AssertHasMessageErrorContaining(invoice2.JZ_SupplierGSTNumberInfo, expectedMessage);

		invoice1.Validation.ValidateAll();
		AssertHasMessageErrorContaining(invoice1.JZ_IsGSTPrePaidInfo, expectedMessage);
		AssertHasMessageErrorContaining(invoice1.JZ_SupplierGSTNumberInfo, expectedMessage);

		invoice2.JZ_SupplierGSTNumber = "AA111";
		invoice2.Validation.ValidateAll();
		AssertHasMessageErrorContaining(invoice2.JZ_IsGSTPrePaidInfo, expectedMessage);
		AssertHasMessageErrorContaining(invoice2.JZ_SupplierGSTNumberInfo, expectedMessage);

		invoice2.JZ_IsGSTPrePaid = "Y";
		invoice2.Validation.ValidateAll();
		AssertNoMessageErrorContaining(invoice2.JZ_IsGSTPrePaidInfo, expectedMessage);
		AssertNoMessageErrorContaining(invoice2.JZ_SupplierGSTNumberInfo, expectedMessage);

		invoice1.Validation.ValidateAll();
		AssertNoMessageErrorContaining(invoice1.JZ_IsGSTPrePaidInfo, expectedMessage);
		AssertNoMessageErrorContaining(invoice1.JZ_SupplierGSTNumberInfo, expectedMessage);

		invoice2.JZ_IsGSTPrePaid = "N";
		invoice2.Validation.ValidateAll();
		AssertHasMessageErrorContaining(invoice2.JZ_IsGSTPrePaidInfo, expectedMessage);
		AssertHasMessageErrorContaining(invoice2.JZ_SupplierGSTNumberInfo, expectedMessage);

		invoice2.JZ_IsGSTPrePaid = "Y";
		invoice2.JZ_SupplierGSTNumber = "BB222";
		invoice2.Validation.ValidateAll();
		AssertHasMessageErrorContaining(invoice2.JZ_IsGSTPrePaidInfo, expectedMessage);
		AssertHasMessageErrorContaining(invoice2.JZ_SupplierGSTNumberInfo, expectedMessage);

		invoice2.JZ_SupplierGSTNumber = "AA111";
		invoice2.Validation.ValidateAll();
		AssertNoMessageErrorContaining(invoice2.JZ_IsGSTPrePaidInfo, expectedMessage);
		AssertNoMessageErrorContaining(invoice2.JZ_SupplierGSTNumberInfo, expectedMessage);

		invoice1.JZ_SupplierGSTNumber = "CC333";
		invoice1.JZ_IsGSTPrePaid = "N";
		invoice1.Validation.ValidateAll();
		AssertHasMessageErrorContaining(invoice1.JZ_IsGSTPrePaidInfo, expectedMessage);
		AssertHasMessageErrorContaining(invoice1.JZ_SupplierGSTNumberInfo, expectedMessage);

		writeOffDec.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoice1.Validation.ValidateAll();
		AssertNoMessageErrorContaining(invoice1.JZ_IsGSTPrePaidInfo, expectedMessage);
		AssertNoMessageErrorContaining(invoice1.JZ_SupplierGSTNumberInfo, expectedMessage);

		writeOffDec.JE_MessageType = JobMessageTypeList.Codes.Import;
		writeOffDec.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
		invoice1.Validation.ValidateAll();
		AssertNoMessageErrorContaining(invoice1.JZ_IsGSTPrePaidInfo, expectedMessage);
		AssertNoMessageErrorContaining(invoice1.JZ_SupplierGSTNumberInfo, expectedMessage);
	}

	public void TestRelationshipIndicator()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();

		invoiceHeader.JZ_RelationshipIndicator = "";
		AssertHasMessageErrorContaining(invoiceHeader.JZ_RelationshipIndicatorInfo, JobComInvoiceHeaderValidation.MessageErrorMissingRelationshipIndicator);
		invoiceHeader.JZ_RelationshipIndicator = RelationshipIndicatorList.Codes.NotRelated;
		AssertNoMessageErrors(invoiceHeader.JZ_RelationshipIndicatorInfo);
		invoiceHeader.JZ_RelationshipIndicator = "$";
		AssertHasMessageErrorContaining(invoiceHeader.JZ_RelationshipIndicatorInfo, JobComInvoiceHeaderValidation.MessageErrorMissingRelationshipIndicator);
		invoiceHeader.JZ_RelationshipIndicator = RelationshipIndicatorList.Codes.Related;
		AssertNoMessageErrors(invoiceHeader.JZ_RelationshipIndicatorInfo);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoiceHeader.JZ_RelationshipIndicator = "";
		AssertNoMessageErrors(invoiceHeader.JZ_RelationshipIndicatorInfo);
	}

	public void TestQualifiesForPreferentialDuty()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceHeader.JZ_QualifiesForPreferentialDuty = "";
		AssertNoMessageErrors(invoiceHeader.JZ_QualifiesForPreferentialDutyInfo);
		invoiceHeader.JZ_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
		AssertNoMessageErrors(invoiceHeader.JZ_QualifiesForPreferentialDutyInfo);
		invoiceHeader.JZ_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.NonQualifying;
		AssertNoMessageErrors(invoiceHeader.JZ_QualifiesForPreferentialDutyInfo);
		invoiceHeader.JZ_QualifiesForPreferentialDuty = "Z";
		AssertHasMessageError(invoiceHeader.JZ_QualifiesForPreferentialDutyInfo, JobComInvoiceHeaderValidation.MessageErrorRemoveOrFixDefaultQualForPrefDutyFlag);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoiceHeader.JZ_QualifiesForPreferentialDuty = "";
		AssertNoNotifications(invoiceHeader.JZ_QualifiesForPreferentialDutyInfo);
		invoiceHeader.JZ_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
		AssertNoNotifications(invoiceHeader.JZ_QualifiesForPreferentialDutyInfo);
		invoiceHeader.JZ_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.NonQualifying;
		AssertNoNotifications(invoiceHeader.JZ_QualifiesForPreferentialDutyInfo);
		invoiceHeader.JZ_QualifiesForPreferentialDuty = "Z";
		AssertNoNotifications(invoiceHeader.JZ_QualifiesForPreferentialDutyInfo);
	}

	public void TestValidateCountryOfExport()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceHeader.JZ_RN_NKCountryOfExport = ZString.Empty;
		AssertNoMessageErrors(invoiceHeader.JZ_RN_NKCountryOfExportInfo);
		invoiceHeader.JZ_RN_NKCountryOfExport = "ZZ";
		AssertHasMessageError(invoiceHeader.JZ_RN_NKCountryOfExportInfo, JobComInvoiceHeaderValidation.MessageErrorRemoveOrFixDefaultCountryOfExport);
		invoiceHeader.JZ_RN_NKCountryOfExport = "AU";
		AssertNoMessageErrors(invoiceHeader.JZ_RN_NKCountryOfExportInfo);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoiceHeader.JZ_RN_NKCountryOfExport = ZString.Empty;
		AssertNoNotifications(invoiceHeader.JZ_RN_NKCountryOfExportInfo);
		invoiceHeader.JZ_RN_NKCountryOfExport = "ZZ";
		AssertNoNotifications(invoiceHeader.JZ_RN_NKCountryOfExportInfo);
		invoiceHeader.JZ_RN_NKCountryOfExport = "AU";
		AssertNoNotifications(invoiceHeader.JZ_RN_NKCountryOfExportInfo);
	}

	public void TestCheckJZ_IsZeroRatedDuty()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
		string flagMessage = JobComInvoiceHeaderValidation.MessageErrorMustHaveValidZeroRatedDutyFlag;

		invoiceHeader.JZ_IsZeroRatedDuty = "";
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedDutyInfo);
		invoiceHeader.JZ_IsZeroRatedDuty = Enterprise.Customs.Business.YesNoList.Codes.Yes;
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedDutyInfo);
		invoiceHeader.JZ_IsZeroRatedDuty = Enterprise.Customs.Business.YesNoList.Codes.No;
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedDutyInfo);
		invoiceHeader.JZ_IsZeroRatedDuty = "B";
		AssertHasMessageError(invoiceHeader.JZ_IsZeroRatedDutyInfo, flagMessage);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoiceHeader.JZ_IsZeroRatedDuty = "";
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedDutyInfo);
		invoiceHeader.JZ_IsZeroRatedDuty = Enterprise.Customs.Business.YesNoList.Codes.Yes;
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedDutyInfo);
		invoiceHeader.JZ_IsZeroRatedDuty = Enterprise.Customs.Business.YesNoList.Codes.No;
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedDutyInfo);
		invoiceHeader.JZ_IsZeroRatedDuty = "B";
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedDutyInfo);
	}

	public void TestCheckJZ_IsZeroRatedExcise()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
		string flagMessage = JobComInvoiceHeaderValidation.MessageErrorMustHaveValidZeroRatedExciseFlag;

		invoiceHeader.JZ_IsZeroRatedExcise = "";
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedExciseInfo);
		invoiceHeader.JZ_IsZeroRatedExcise = Enterprise.Customs.Business.YesNoList.Codes.Yes;
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedExciseInfo);
		invoiceHeader.JZ_IsZeroRatedExcise = Enterprise.Customs.Business.YesNoList.Codes.No;
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedExciseInfo);
		invoiceHeader.JZ_IsZeroRatedExcise = "B";
		AssertHasMessageError(invoiceHeader.JZ_IsZeroRatedExciseInfo, flagMessage);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoiceHeader.JZ_IsZeroRatedExcise = "";
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedExciseInfo);
		invoiceHeader.JZ_IsZeroRatedExcise = Enterprise.Customs.Business.YesNoList.Codes.Yes;
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedExciseInfo);
		invoiceHeader.JZ_IsZeroRatedExcise = Enterprise.Customs.Business.YesNoList.Codes.No;
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedExciseInfo);
		invoiceHeader.JZ_IsZeroRatedExcise = "B";
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedExciseInfo);
	}

	public void TestCheckJZ_IsZeroRatedGST()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
		string flagMessage = JobComInvoiceHeaderValidation.MessageErrorMustHaveValidZeroRatedGSTFlag;

		invoiceHeader.JZ_IsZeroRatedGST = "";
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedGSTInfo);
		invoiceHeader.JZ_IsZeroRatedGST = Enterprise.Customs.Business.YesNoList.Codes.Yes;
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedGSTInfo);
		invoiceHeader.JZ_IsZeroRatedGST = Enterprise.Customs.Business.YesNoList.Codes.No;
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedGSTInfo);
		invoiceHeader.JZ_IsZeroRatedGST = "B";
		AssertHasMessageError(invoiceHeader.JZ_IsZeroRatedGSTInfo, flagMessage);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoiceHeader.JZ_IsZeroRatedGST = "";
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedGSTInfo);
		invoiceHeader.JZ_IsZeroRatedGST = Enterprise.Customs.Business.YesNoList.Codes.Yes;
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedGSTInfo);
		invoiceHeader.JZ_IsZeroRatedGST = Enterprise.Customs.Business.YesNoList.Codes.No;
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedGSTInfo);
		invoiceHeader.JZ_IsZeroRatedGST = "B";
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedGSTInfo);
	}

	public void TestCheckJZ_IsZeroRatedLevies()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
		string flagMessage = JobComInvoiceHeaderValidation.MessageErrorMustHaveValidZeroRatedLeviesFlag;

		invoiceHeader.JZ_IsZeroRatedLevies = "";
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedLeviesInfo);
		invoiceHeader.JZ_IsZeroRatedLevies = Enterprise.Customs.Business.YesNoList.Codes.Yes;
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedLeviesInfo);
		invoiceHeader.JZ_IsZeroRatedLevies = Enterprise.Customs.Business.YesNoList.Codes.No;
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedLeviesInfo);
		invoiceHeader.JZ_IsZeroRatedLevies = "B";
		AssertHasMessageError(invoiceHeader.JZ_IsZeroRatedLeviesInfo, flagMessage);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoiceHeader.JZ_IsZeroRatedLevies = "";
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedLeviesInfo);
		invoiceHeader.JZ_IsZeroRatedLevies = Enterprise.Customs.Business.YesNoList.Codes.Yes;
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedLeviesInfo);
		invoiceHeader.JZ_IsZeroRatedLevies = Enterprise.Customs.Business.YesNoList.Codes.No;
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedLeviesInfo);
		invoiceHeader.JZ_IsZeroRatedLevies = "B";
		AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedLeviesInfo);
	}
}
