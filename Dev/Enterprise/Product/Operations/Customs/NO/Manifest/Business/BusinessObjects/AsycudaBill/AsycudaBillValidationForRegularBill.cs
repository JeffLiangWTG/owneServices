using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Manifest.Business;

class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
{
	public AsycudaBillValidationForRegularBill(AsycudaBill parent) : base(parent)
	{
	}

	protected override ZBool NeedsToShowABL_ManifestUQNotMappedMessageError => false;

	public override void ValidateAll()
	{
		Bill.ClearRowNotifications();

		base.ValidateAll();
		ValidateImportProcedure();
		ValidateExportProcedure();
		ValidateTransportDocumentType();
	}

	HashSet<ZString> GetLinkedPreviousDocumentCodes()
	{
		return Bill.PreviousDocuments.Select(p => p.CSI_Code).ToHashSet();
	}

	#region Import & Export Procedures

	internal void ValidateImportProcedure()
	{
		ValidateCalculatedProperty(Bill.ImportProcedureInfo);
	}

	protected virtual void CheckImportProcedure()
	{
		var bill = Bill;
		if (bill.IsHouseBill)
		{
			MandatoryValidation.MessageErrorIfNotEntered(bill.ImportProcedureInfo);
			ListValidation.MessageErrorIfInvalidCode(bill.ImportProcedureInfo);

			ValidateImportProcedureForPreviousDocumentType(GetLinkedPreviousDocumentCodes());
			ValidatePreviousDocumentTypeAnyRETRForImportAndExportProcedures(GetLinkedPreviousDocumentCodes());
		}
	}

	internal void ValidateExportProcedure()
	{
		ValidateCalculatedProperty(Bill.ExportProcedureInfo);
	}

	protected virtual void CheckExportProcedure()
	{
		var bill = Bill;
		var header = bill.Header;
		ListValidation.ErrorIfInvalidCode(bill.ExportProcedureInfo);
		if (bill.IsHouseBill)
		{
			if (header.AMA_TransportMode == Core.Constants.TransportModes.Road &&
				bill.ExportProcedure.IsEmpty)
			{
				var warningMessage = Res.GetString(resourceKey: "FB9B97BA-B0B2-4180-8F70-8A4105A1D951",
					englishText: "Transport Mode 'ROA' from Finland or Sweden requires the Export Procedure to be captured. Please be sure that border crossing is not from Finland or Sweden to Norway.");
				bill.ExportProcedureInfo.AddWarning(warningMessage);
				return;
			}

			ValidateExportProcedureForPreviousDocumentType(GetLinkedPreviousDocumentCodes());
		}
	}

	void ValidateImportProcedureForPreviousDocumentType(ICollection<ZString> linkedPreviousDocumentsCodes)
	{
		var bill = Bill;
		var importProcedure = bill.ImportProcedure;

		var isIRI = importProcedure == NOImportProcedureCodeList.Codes.IMMEDIATE_RELEASE_IMPORT;
		var isWR = importProcedure == NOImportProcedureCodeList.Codes.WAREHOUSE_RELEASE;

		if (isIRI && !linkedPreviousDocumentsCodes.Contains(PreviousDocumentConstants.Codes.CUDE))
		{
			bill.ImportProcedureInfo.AddMessageError(PreviousDocumentTypeAnyCUDEForImportProcedureMessageError);
			return;
		}

		if (isWR && !linkedPreviousDocumentsCodes.Intersect([PreviousDocumentConstants.Codes.CUDE, PreviousDocumentConstants.Codes.GONU]).Any())
		{
			bill.ImportProcedureInfo.AddMessageError(PreviousDocumentTypeAnyCUDEorGONUForImportProcedureMessageError);
		}
	}

	void ValidateExportProcedureForPreviousDocumentType(ICollection<ZString> linkedPreviousDocumentsCodes)
	{
		var bill = Bill;
		var exportProcedure = bill.ExportProcedure;
		var isTransit = exportProcedure == NOExportProcedureCodeList.Codes.TRA;
		var isEXP = exportProcedure == NOExportProcedureCodeList.Codes.EXP;

		if (isTransit && !linkedPreviousDocumentsCodes.Contains(PreviousDocumentConstants.Codes.N820))
		{
			bill.ExportProcedureInfo.AddMessageError(PreviousDocumentTypeAnyN820ForExportProcedureMessageError);
			return;
		}

		if (isEXP && !linkedPreviousDocumentsCodes.Intersect([PreviousDocumentConstants.Codes.AES, PreviousDocumentConstants.Codes.EUEI]).Any())
		{
			bill.ExportProcedureInfo.AddMessageError(PreviousDocumentTypeAnyAESorEUEIForExportProcedureMessageError);
		}
	}
	#endregion

	#region Transport Document Type

	internal void ValidateTransportDocumentType()
	{
		ValidateCalculatedProperty(Bill.TransportDocumentTypeInfo);
	}

	protected virtual void CheckTransportDocumentType()
	{
		MandatoryValidation.MessageErrorIfNotEntered(Bill.TransportDocumentTypeInfo);
		ListValidation.MessageErrorIfInvalidCode(Bill.TransportDocumentTypeInfo);
	}

	#endregion

	#region Forwarder Address

	protected override void CheckABL_OA_Forwarder()
	{
		base.CheckABL_OA_Forwarder();
		CommonBillValidation.ValidateForwarderForCusCodes();
	}

	protected override void CheckABL_ForwarderEmail()
	{
		base.CheckABL_ForwarderEmail();
		if (!Bill.ABL_OA_Forwarder.IsEmpty)
		{
			CommonBillValidation.ValidateForwarderEmail();
		}
	}

	protected override void CheckABL_ForwarderPhone()
	{
		base.CheckABL_ForwarderPhone();
		if (!Bill.ABL_OA_Forwarder.IsEmpty)
		{
			CommonBillValidation.ValidateForwarderPhone();
		}
	}

	#endregion

	#region Previous Documents

	void ValidatePreviousDocumentTypeAnyRETRForImportAndExportProcedures(ICollection<ZString> linkedPreviousDocumentsCodes)
	{
		var bill = Bill;

		if (bill.ImportProcedure == NOImportProcedureCodeList.Codes.TRANSIT_RELEASE
			&& bill.ExportProcedure == NOExportProcedureCodeList.Codes.EXP
			&& !linkedPreviousDocumentsCodes.Contains(PreviousDocumentConstants.Codes.RETR))
		{
			bill.ImportProcedureInfo.AddMessageError(PreviousDocumentTypeAnyRETRForImportAndExportProceduresMessageError);
		}
	}

	#endregion

	AsycudaBillValidation CommonBillValidation => new(Bill);

	AsycudaBill Bill => Parent as AsycudaBill;

	static string PreviousDocumentTypeAnyCUDEForImportProcedureMessageError => Res.GetString("A0F4D1B5-8E3C-4F2A-9E7C-6D3B2F5A0E7A",
		"You have not entered a Previous Document of type CUDE. One or more CUDE references must be entered when Import Procedure is IMMEDIATE_RELEASE_IMPORT.");

	static string PreviousDocumentTypeAnyCUDEorGONUForImportProcedureMessageError => Res.GetString("6097F482-3BFB-4C78-BAEB-1C2E19C9917D",
		"You have not entered a Previous Document of type CUDE or GONU. One or more CUDE/GONU references must be entered when Import Procedure is WAREHOUSE_RELEASE.");

	static string PreviousDocumentTypeAnyN820ForExportProcedureMessageError => Res.GetString("1D920F67-AE5B-48CD-84AD-C040BE373A78",
		"You have not entered a Previous Document of type N820. One or more N820 references must be entered when Export Procedure is TRA.");

	static string PreviousDocumentTypeAnyAESorEUEIForExportProcedureMessageError => Res.GetString("89a60394-a0cd-4b53-8f21-fbecaf21ac4a",
		"You have not entered a Previous Document of type AES or EUEI. One or more AES/EUEI references must be entered when Export Procedure is EXP.");
	static string PreviousDocumentTypeAnyRETRForImportAndExportProceduresMessageError => Res.GetString("679E2A1D-04E2-4ECF-9B38-3E20A2281FBA",
		"You have not entered a Previous Document of type RETR. One or more RETR must be entered when Import Procedure is TRANSIT_RELEASE and Export Procedure is EXP.");
}
