using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.PL;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class AddInfoCusEntryInstructionValidationTest : EU.Business.Declaration.Testing.AddInfoCusEntryInstructionValidationTest
{
	public void TestCheckZG_TemporaryLocationCodeType()
	{
		var dec = Factory.New<JobDeclaration>();
		var entry = dec.CustomsEntryInstructions.AddNew();

		entry.ZG_ExportManifest = false;
		entry.ZG_TemporaryLocationCodeType = "aaaa";
		AssertNoNotifications(entry.ZG_TemporaryLocationCodeTypeInfo);

		entry.ZG_ExportManifest = true;
		entry.ZG_TemporaryLocationCodeType = "bbbb";
		AssertHasMessageError(entry.ZG_TemporaryLocationCodeTypeInfo, ListValidation.InvalidCodeMessageError);

		entry.ZG_TemporaryLocationCodeType = TemporaryLocationCodeTypeList.Codes.CODE;
		AssertNoNotifications(entry.ZG_TemporaryLocationCodeTypeInfo);
	}

	public void TestCheckZG_TemporaryLocation()
	{
		var dec = Factory.New<JobDeclaration>();
		var entry = dec.CustomsEntryInstructions.AddNew();

		entry.ZG_ExportManifest = false;
		entry.ZG_TemporaryLocationCodeType = "aaaa";
		AssertNoNotifications(entry.ZG_TemporaryLocationInfo);

		entry.ZG_ExportManifest = true;
		entry.ZG_TemporaryLocationCodeType = TemporaryLocationCodeTypeList.Codes.CODE;
		entry.ZG_TemporaryLocation = "asd";
		AssertNoNotifications(entry.ZG_TemporaryLocationInfo);

		entry.ZG_TemporaryLocationCodeType = TemporaryLocationCodeTypeList.Codes.CODE;
		entry.ZG_TemporaryLocation = string.Empty;
		AssertHasMessageError(entry.ZG_TemporaryLocationInfo, "Number is required but is empty.");
	}

	public void TestCheckZG_EADPrintOut()
	{
		var dec = Factory.New<JobDeclaration>();
		var entry = dec.CustomsEntryInstructions.AddNew();
		entry.ZG_EADPrintOut = "a";
		AssertHasMessageError(entry.ZG_EADPrintOutInfo, ListValidation.InvalidCodeMessageError);

		entry.ZG_EADPrintOut = EadPrintOutList.Codes._0;
		AssertNoNotifications(entry.ZG_EADPrintOutInfo);
	}

	public void TestCheckZG_EADPrintOut_ExitSummary() => CombineAssertions(() =>
	{
		var dec = Factory.New<JobDeclaration>();
		var entry = dec.CustomsEntryInstructions.AddNew();

		dec.JE_MessageType = PLJobMessageTypeList.Codes.Import;
		entry.ZG_EADPrintOut = "a";
		AssertHasMessageError("List Validation is enabled for non-EXS.", entry.ZG_EADPrintOutInfo, ListValidation.InvalidCodeMessageError);

		dec.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		entry.ZG_EADPrintOut = "a";
		AssertNoNotifications("List Validation is disabled for EXS.", entry.ZG_EADPrintOutInfo);

		dec.JE_MessageType = PLJobMessageTypeList.Codes.Import;
		entry.ZG_EADPrintOut = string.Empty;
		AssertHasMessageErrorContaining("CEI_Procedure is mandatory for non-EXS.", entry.ZG_EADPrintOutInfo, MandatoryValidation.YouHaveNotEntered);

		dec.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		entry.ZG_EADPrintOut = string.Empty;
		AssertNoNotifications("CEI_Procedure is not mandatory for EXS.", entry.ZG_EADPrintOutInfo);
	});

	public void TestCheckZG_OfficeOfExitArrivalTimeLimit_InvLine()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entry = dec.CustomsEntryInstructions.AddNew();
		var header = dec.Invoices.AddNew();
		var invoiceLine = header.InvoiceLines.AddNew();

		invoiceLine.JI_CEI = entry.PK;

		entry.ZG_OfficeOfExitArrivalTimeLimit = ZDateTime.Now.AddDays(-2);
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = "C513";
		entry.ZG_OfficeOfExitArrivalTimeLimitInfo.RunAdditionalValidation();
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		supportingDocument.CSI_Type = "SUP";
		entry.AddInfoValidation.ValidateZG_OfficeOfExitArrivalTimeLimit();
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		dec.JE_LocationQualifier = GoodsLocationTypeList.Codes.GLC;
		entry.AddInfoValidation.ValidateZG_OfficeOfExitArrivalTimeLimit();
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		dec.JE_LocationOfGoods = string.Empty;
		entry.AddInfoValidation.ValidateZG_OfficeOfExitArrivalTimeLimit();
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		dec.JE_LocationOfGoods = "ASD";
		entry.AddInfoValidation.ValidateZG_OfficeOfExitArrivalTimeLimit();
		AssertHasMessageError(entry.ZG_OfficeOfExitArrivalTimeLimitInfo, "Office Of Exit Arrival Time Limit must be in the future.");

		supportingDocument.CSI_Code = "C514";
		entry.ZG_OfficeOfExitArrivalTimeLimitInfo.RunAdditionalValidation();
		AssertHasMessageError(entry.ZG_OfficeOfExitArrivalTimeLimitInfo, "Office Of Exit Arrival Time Limit must be in the future.");

		entry.ZG_OfficeOfExitArrivalTimeLimit = ZDateTime.Now.AddDays(2);
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		entry.ZG_OfficeOfExitArrivalTimeLimit = ZDateTime.Now.AddDays(-2);
		AssertHasMessageError(entry.ZG_OfficeOfExitArrivalTimeLimitInfo, "Office Of Exit Arrival Time Limit must be in the future.");

		dec.JE_LocationQualifier = GoodsLocationTypeList.Codes.OTH;
		entry.AddInfoValidation.ValidateZG_OfficeOfExitArrivalTimeLimit();
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		dec.JE_LocationQualifier = GoodsLocationTypeList.Codes.GLC;
		supportingDocument.CSI_Code = "C511";
		entry.ZG_OfficeOfExitArrivalTimeLimitInfo.RunAdditionalValidation();
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		entry.ZG_OfficeOfExitArrivalTimeLimit = ZDateTime.Now.AddDays(2);
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		supportingDocument.CSI_Code = "C513";
		entry.ZG_OfficeOfExitArrivalTimeLimitInfo.RunAdditionalValidation();
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		entry.ZG_OfficeOfExitArrivalTimeLimit = ZDateTime.Empty;
		AssertHasMessageError(entry.ZG_OfficeOfExitArrivalTimeLimitInfo, "Office Of Exit Arrival Time Limit must be in the future.");
	}

	public void TestCheckZG_OfficeOfExitArrivalTimeLimit_InvHeader()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entry = dec.CustomsEntryInstructions.AddNew();
		var header = dec.Invoices.AddNew();
		var invoiceLine = header.InvoiceLines.AddNew();

		invoiceLine.JI_CEI = entry.PK;

		entry.ZG_OfficeOfExitArrivalTimeLimit = ZDateTime.Now.AddDays(-2);
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		var supportingDocument = header.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = "C513";
		entry.ZG_OfficeOfExitArrivalTimeLimitInfo.RunAdditionalValidation();
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		supportingDocument.CSI_Type = "SUP";
		entry.AddInfoValidation.ValidateZG_OfficeOfExitArrivalTimeLimit();
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		dec.JE_LocationQualifier = GoodsLocationTypeList.Codes.GLC;
		entry.AddInfoValidation.ValidateZG_OfficeOfExitArrivalTimeLimit();
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		dec.JE_LocationOfGoods = string.Empty;
		entry.AddInfoValidation.ValidateZG_OfficeOfExitArrivalTimeLimit();
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		dec.JE_LocationOfGoods = "ASD";
		entry.AddInfoValidation.ValidateZG_OfficeOfExitArrivalTimeLimit();
		AssertHasMessageError(entry.ZG_OfficeOfExitArrivalTimeLimitInfo, "Office Of Exit Arrival Time Limit must be in the future.");

		supportingDocument.CSI_Code = "C514";
		entry.ZG_OfficeOfExitArrivalTimeLimitInfo.RunAdditionalValidation();
		AssertHasMessageError(entry.ZG_OfficeOfExitArrivalTimeLimitInfo, "Office Of Exit Arrival Time Limit must be in the future.");

		entry.ZG_OfficeOfExitArrivalTimeLimit = ZDateTime.Now.AddDays(2);
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		entry.ZG_OfficeOfExitArrivalTimeLimit = ZDateTime.Now.AddDays(-2);
		AssertHasMessageError(entry.ZG_OfficeOfExitArrivalTimeLimitInfo, "Office Of Exit Arrival Time Limit must be in the future.");

		dec.JE_LocationQualifier = GoodsLocationTypeList.Codes.OTH;
		entry.AddInfoValidation.ValidateZG_OfficeOfExitArrivalTimeLimit();
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		dec.JE_LocationQualifier = GoodsLocationTypeList.Codes.GLC;
		supportingDocument.CSI_Code = "C511";
		entry.ZG_OfficeOfExitArrivalTimeLimitInfo.RunAdditionalValidation();
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		entry.ZG_OfficeOfExitArrivalTimeLimit = ZDateTime.Now.AddDays(2);
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		supportingDocument.CSI_Code = "C513";
		entry.ZG_OfficeOfExitArrivalTimeLimitInfo.RunAdditionalValidation();
		AssertNoNotifications(entry.ZG_OfficeOfExitArrivalTimeLimitInfo);

		entry.ZG_OfficeOfExitArrivalTimeLimit = ZDateTime.Empty;
		AssertHasMessageError(entry.ZG_OfficeOfExitArrivalTimeLimitInfo, "Office Of Exit Arrival Time Limit must be in the future.");
	}
}
