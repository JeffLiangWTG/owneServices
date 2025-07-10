using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(AsycudaBillValidationForRegularBill))]
sealed class AsycudaBillValidationForRegularBillTest : AsycudaBillValidationAbstractTest
{
	public void TestCheckImportProcedure_InvalidValue()
	{
		bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
		ValidationTestHelper.AssertInvalidCodeMessageError(bill.ImportProcedureInfo, "INVALID_CODE", NOImportProcedureCodeList.Codes.WAREHOUSE_RELEASE);
	}

	public void TestCheckImportProcedure_IfNotEntered()
	{
		var regularBillValidation = bill.Validation as AsycudaBillValidationForRegularBill;
		AssertNotNull("[PRE-REQ] Regular Validation", regularBillValidation);

		CombineAssertions(() =>
		{
			bill.ABL_BolType = AsycudaBill.ChildBolCode;
			bill.ImportProcedure = ZString.Empty;
			regularBillValidation!.ValidateImportProcedure();
			AssertNoMessageErrorContaining("When MasterBill", bill.ImportProcedureInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill.ImportProcedure = ZString.Empty;
			regularBillValidation.ValidateImportProcedure();
			AssertHasMessageErrorContaining("When HouseBill", bill.ImportProcedureInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckImportProcedure_PreviousDocumentTypeAnyCUDE()
	{
		const string expectedErrorMessage = "You have not entered a Previous Document of type CUDE. One or more CUDE references must be entered when Import Procedure is IMMEDIATE_RELEASE_IMPORT.";
		var previousDocument = bill.PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			AssertCheckImportProcedure_PreviousDocumentType(NOImportProcedureCodeList.Codes.IMMEDIATE_RELEASE_IMPORT, previousDocument, "XYZ", expectedErrorMessage, true);
			AssertCheckImportProcedure_PreviousDocumentType(NOImportProcedureCodeList.Codes.IMMEDIATE_RELEASE_IMPORT, previousDocument, PreviousDocumentConstants.Codes.CUDE, expectedErrorMessage);
			AssertCheckImportProcedure_PreviousDocumentType(NOImportProcedureCodeList.Codes.TRANSIT_RELEASE, previousDocument, "XYZ", expectedErrorMessage);

			bill.ABL_BolType = Core.Constants.ShipmentTypes.CoLoadMaster;
			bill.ABL_OA_Forwarder = billAddress.PK;
			AssertCheckImportProcedure_PreviousDocumentType(NOImportProcedureCodeList.Codes.IMMEDIATE_RELEASE_IMPORT, previousDocument, "XYZ", expectedErrorMessage);
		});
	}

	void AssertCheckImportProcedure_PreviousDocumentType(
		string importProcedureType,
		PreviousDocument previousDocument,
		string previousDocumentType,
		string expectedErrorMessage,
		bool errorExpected = false)
	{
		var validation = (AsycudaBillValidationForRegularBill)bill.Validation;
		previousDocument.CSI_Code = previousDocumentType;
		bill.ImportProcedure = importProcedureType;
		validation.ValidateImportProcedure();

		if (errorExpected)
		{
			AssertHasMessageError($"When previousDocument.CSI_Code: {previousDocument.CSI_Code}, Import Procedure: {bill.ImportProcedure}", bill.ImportProcedureInfo, expectedErrorMessage);
		}
		else
		{
			AssertNoMessageError($"When previousDocument.CSI_Code: {previousDocument.CSI_Code}, Import Procedure: {bill.ImportProcedure}", bill.ImportProcedureInfo, expectedErrorMessage);
		}
	}

	public void TestCheckImportProcedure_PreviousDocumentTypeAnyCUDEorGONU()
	{
		const string expectedErrorMessage = "You have not entered a Previous Document of type CUDE or GONU. One or more CUDE/GONU references must be entered when Import Procedure is WAREHOUSE_RELEASE.";
		var previousDocument = bill.PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			AssertCheckImportProcedure_PreviousDocumentType(NOImportProcedureCodeList.Codes.WAREHOUSE_RELEASE, previousDocument, "XYZ", expectedErrorMessage, true);
			AssertCheckImportProcedure_PreviousDocumentType(NOImportProcedureCodeList.Codes.WAREHOUSE_RELEASE, previousDocument, PreviousDocumentConstants.Codes.CUDE, expectedErrorMessage);
			AssertCheckImportProcedure_PreviousDocumentType(NOImportProcedureCodeList.Codes.WAREHOUSE_RELEASE, previousDocument, PreviousDocumentConstants.Codes.GONU, expectedErrorMessage);
			AssertCheckImportProcedure_PreviousDocumentType(NOImportProcedureCodeList.Codes.TRANSIT_RELEASE, previousDocument, "XYZ", expectedErrorMessage);

			bill.ABL_BolType = Core.Constants.ShipmentTypes.CoLoadMaster;
			bill.ABL_OA_Forwarder = billAddress.PK;
			AssertCheckImportProcedure_PreviousDocumentType(NOImportProcedureCodeList.Codes.WAREHOUSE_RELEASE, previousDocument, "XYZ", expectedErrorMessage);
		});
	}

	public void TestCheckExportProcedure_InvalidValue()
	{
		ValidationTestHelper.AssertErrorIfInvalidCode(bill.ExportProcedureInfo, "XXX", NOExportProcedureCodeList.Codes.EXP);
	}

	public void TestCheckExportProcedure()
	{
		const string warningMessage = "Transport Mode 'ROA' from Finland or Sweden requires the Export Procedure to be captured. Please be sure that border crossing is not from Finland or Sweden to Norway.";
		var regularBillValidation = bill.Validation as AsycudaBillValidationForRegularBill;
		header.AMA_TransportMode = Core.Constants.TransportModes.Road;

		AssertNotNull("[Pre-Condition] Validation for regular bill", regularBillValidation);

		CombineAssertions("When HouseBill", () =>
		{
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill.ExportProcedure = NOExportProcedureCodeList.Codes.TRA;
			AssertNoWarning("When Export Procedure is not empty", bill.ExportProcedureInfo, warningMessage);

			bill.ExportProcedure = ZString.Empty;
			regularBillValidation!.ValidateExportProcedure();
			AssertHasWarning("When Export Procedure is empty", bill.ExportProcedureInfo, warningMessage);
		});

		bill.ABL_BolType = AsycudaBill.ChildBolCode;
		bill.ExportProcedure = ZString.Empty;
		regularBillValidation.ValidateExportProcedure();
		AssertNoWarning("When Not a HouseBill and Export Procedure is empty", bill.ExportProcedureInfo, warningMessage);
	}

	public void TestCheckTransportDocumentType_IfNotEntered()
	{
		var regularBillValidation = bill.Validation as AsycudaBillValidationForRegularBill;
		AssertNotNull("[PRE-REQ] Regular Validation", regularBillValidation);

		CombineAssertions(() =>
		{
			bill.TransportDocumentType = ZString.Empty;
			regularBillValidation.ValidateTransportDocumentType();
			AssertHasMessageErrorContaining(bill.TransportDocumentTypeInfo, MandatoryValidation.YouHaveNotEntered);

			bill.TransportDocumentType = ASYCUDA.Business.TransportDocumentTypes.Codes.CL754_C624;
			regularBillValidation.ValidateTransportDocumentType();
			AssertNoMessageErrorContaining(bill.TransportDocumentTypeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckTransportDocumentType_ListValidation()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(bill.TransportDocumentTypeInfo, "XXX", ASYCUDA.Business.TransportDocumentTypes.Codes.CL754_C624);
	}

	public void TestNeedsToShowABL_ManifestUQNotMappedMessageError() =>
		AssertEquals(false, new AsycudaBillValidationForRegularBillForTest(bill).NeedsToShowABL_ManifestUQNotMappedMessageError);

	public void TestCheckExportProcedure_PreviousDocumentTypeAnyN820()
	{
		const string expectedErrorMessage = "You have not entered a Previous Document of type N820. One or more N820 references must be entered when Export Procedure is TRA.";
		var previousDocument = bill.PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			AssertCheckExportProcedure_PreviousDocumentType(NOExportProcedureCodeList.Codes.TRA, previousDocument, "XYZ", expectedErrorMessage, true);
			AssertCheckExportProcedure_PreviousDocumentType(NOExportProcedureCodeList.Codes.TRA, previousDocument, PreviousDocumentConstants.Codes.N820, expectedErrorMessage);

			bill.ABL_BolType = Core.Constants.ShipmentTypes.CoLoadMaster;
			bill.ABL_OA_Forwarder = billAddress.PK;
			AssertCheckExportProcedure_PreviousDocumentType(NOExportProcedureCodeList.Codes.TRA, previousDocument, "XYZ", expectedErrorMessage);
		});
	}

	public void TestCheckExportProcedure_PreviousDocumentTypeAnyAESorEUEI()
	{
		const string expectedErrorMessage = "You have not entered a Previous Document of type AES or EUEI. One or more AES/EUEI references must be entered when Export Procedure is EXP.";
		var previousDocument = bill.PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			AssertCheckExportProcedure_PreviousDocumentType(NOExportProcedureCodeList.Codes.EXP, previousDocument, "XYZ", expectedErrorMessage, true);
			AssertCheckExportProcedure_PreviousDocumentType(NOExportProcedureCodeList.Codes.EXP, previousDocument, PreviousDocumentConstants.Codes.AES, expectedErrorMessage);
			AssertCheckExportProcedure_PreviousDocumentType(NOExportProcedureCodeList.Codes.EXP, previousDocument, PreviousDocumentConstants.Codes.EUEI, expectedErrorMessage);

			bill.ABL_BolType = Core.Constants.ShipmentTypes.CoLoadMaster;
			bill.ABL_OA_Forwarder = billAddress.PK;
			AssertCheckExportProcedure_PreviousDocumentType(NOExportProcedureCodeList.Codes.EXP, previousDocument, "XYZ", expectedErrorMessage);
		});
	}

	void AssertCheckExportProcedure_PreviousDocumentType(
		string exportProcedureType,
		PreviousDocument previousDocument,
		string previousDocumentType,
		string expectedMessageError,
		bool errorExpected = false)
	{
		var validation = (AsycudaBillValidationForRegularBill)bill.Validation;
		bill.ClearRowNotificationsContaining(expectedMessageError);
		bill.ExportProcedure = exportProcedureType;
		previousDocument.CSI_Code = previousDocumentType;
		validation.ValidateExportProcedure();

		if (errorExpected)
		{
			AssertHasMessageError($"When HouseBill: {bill.IsHouseBill}, ExportProcedure: {exportProcedureType}, CSI_Code: {previousDocument.CSI_Code}", bill.ExportProcedureInfo, expectedMessageError);
		}
		else
		{
			AssertNoMessageError($"When HouseBill: {bill.IsHouseBill}, ExportProcedure: {exportProcedureType}, CSI_Code: {previousDocument.CSI_Code}", bill.ExportProcedureInfo, expectedMessageError);
		}
	}

	public void TestCheckImportAndExportProcedure_PreviousDocumentTypeAnyRETR()
	{
		const string expectedMessageError = "You have not entered a Previous Document of type RETR. One or more RETR must be entered when Import Procedure is TRANSIT_RELEASE and Export Procedure is EXP.";
		var previousDocument = bill.PreviousDocuments.AddNew();
		previousDocument.CSI_Code = PreviousDocumentConstants.Codes.RETR;

		CombineAssertions(() =>
		{
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			AssertCheckImportAndExportProcedure_PreviousDocumentType(NOImportProcedureCodeList.Codes.TRANSIT_RELEASE,
				NOExportProcedureCodeList.Codes.EXP, PreviousDocumentConstants.Codes.RETR, previousDocument, expectedMessageError);
			AssertCheckImportAndExportProcedure_PreviousDocumentType(NOImportProcedureCodeList.Codes.TRANSIT_RELEASE,
				NOExportProcedureCodeList.Codes.EXP, "XYZ", previousDocument, expectedMessageError, true);

			AssertCheckImportAndExportProcedure_PreviousDocumentType(NOImportProcedureCodeList.Codes.TRANSIT_RELEASE,
				NOExportProcedureCodeList.Codes.TRA, PreviousDocumentConstants.Codes.RETR, previousDocument, expectedMessageError);
			AssertCheckImportAndExportProcedure_PreviousDocumentType(NOImportProcedureCodeList.Codes.IMMEDIATE_RELEASE_VOEC,
				NOExportProcedureCodeList.Codes.EXP, PreviousDocumentConstants.Codes.RETR, previousDocument, expectedMessageError);
			AssertCheckImportAndExportProcedure_PreviousDocumentType(NOImportProcedureCodeList.Codes.COLLECTIVE_RELEASE,
				NOExportProcedureCodeList.Codes.EXP, PreviousDocumentConstants.Codes.RETR, previousDocument, expectedMessageError);
			AssertCheckImportAndExportProcedure_PreviousDocumentType(NOImportProcedureCodeList.Codes.DOCUMENTS_NOT_OBLIGED_RELEASE,
				NOExportProcedureCodeList.Codes.EXP, PreviousDocumentConstants.Codes.RETR, previousDocument, expectedMessageError);
			AssertCheckImportAndExportProcedure_PreviousDocumentType(NOImportProcedureCodeList.Codes.WAREHOUSE_RELEASE,
				NOExportProcedureCodeList.Codes.EXP, PreviousDocumentConstants.Codes.RETR, previousDocument, expectedMessageError);
			AssertCheckImportAndExportProcedure_PreviousDocumentType(NOImportProcedureCodeList.Codes.TRANSIT_IMPORT,
				NOExportProcedureCodeList.Codes.EXP, PreviousDocumentConstants.Codes.RETR, previousDocument, expectedMessageError);

			bill.ABL_BolType = Core.Constants.ShipmentTypes.CoLoadMaster;
			bill.ABL_OA_Forwarder = billAddress.PK;
			AssertCheckImportAndExportProcedure_PreviousDocumentType(NOImportProcedureCodeList.Codes.TRANSIT_RELEASE,
				NOExportProcedureCodeList.Codes.EXP, "XYZ", previousDocument, expectedMessageError);
		});
	}

	void AssertCheckImportAndExportProcedure_PreviousDocumentType(
		string importProcedureType,
		string exportProcedureType,
		string previousDocumentType,
		PreviousDocument previousDocument,
		string expectedMessageError,
		bool errorExpected = false)
	{
		bill.ClearRowNotificationsContaining(expectedMessageError);
		var validation = bill.Validation as AsycudaBillValidationForRegularBill;
		bill.ImportProcedure = importProcedureType;
		bill.ExportProcedure = exportProcedureType;
		previousDocument.CSI_Code = previousDocumentType;
		validation.ValidateImportProcedure();

		if (errorExpected)
		{
			AssertHasMessageError($"When HouseBill: {bill.IsHouseBill}, Import Procedure: {bill.ImportProcedure}, Export Procedure: {bill.ExportProcedure}, and CSI_Code: {previousDocument.CSI_Code}",
				bill.ImportProcedureInfo, expectedMessageError);
		}
		else
		{
			AssertNoMessageError($"When HouseBill: {bill.IsHouseBill}, Import Procedure: {bill.ImportProcedure}, Export Procedure: {bill.ExportProcedure}, and CSI_Code: {previousDocument.CSI_Code}",
				bill.ImportProcedureInfo, expectedMessageError);
		}
	}

	protected override AsycudaBill GetAsycudaBillForTests() => bill;

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<AsycudaManifestHeader>();
		bill = Factory.New<AsycudaBill>();
		header.Bills.Add(bill);
		bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
		billAddress = Factory.New<OrgAddress>();
	}

	AsycudaBill bill;
	AsycudaManifestHeader header;
	OrgAddress billAddress;

	class AsycudaBillValidationForRegularBillForTest : AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBillForTest(AsycudaBill parent) : base(parent)
		{
		}

		public new ZBool NeedsToShowABL_ManifestUQNotMappedMessageError => base.NeedsToShowABL_ManifestUQNotMappedMessageError;
	}
}
