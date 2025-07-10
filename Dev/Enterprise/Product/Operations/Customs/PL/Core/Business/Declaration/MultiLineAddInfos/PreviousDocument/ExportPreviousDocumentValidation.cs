using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ExportPreviousDocumentValidation : PreviousDocumentValidation
{
	public ExportPreviousDocumentValidation(PreviousDocument parent) : base(parent)
	{
	}

	protected override void CheckCSI_Quantity2()
	{
		const int maximumPackages = 99999999;

		base.CheckCSI_Quantity2();

		var parent = Parent;
		if (parent.CSI_Quantity2 > maximumPackages)
		{
			parent.CSI_Quantity2Info.AddMessageError(Res.GetString("AA405DC1-B839-4DEB-B945-66AEE763BF07", "The number of packages is too large. {0} is the maximum allowed.", maximumPackages));
		}
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();

		CheckRuleR0032E();
		CheckRuleR0024E();
		CheckRuleR0022E();
	}

	protected override void CheckCSI_LineNo()
	{
		base.CheckCSI_LineNo();

		var parent = Parent;
		if (parent.Parent is JobComInvoiceLine && CSICodeIsSpecialProcedureCode())
		{
			var propertyDescription = Res.GetString("F4EAFAC6-2A33-4D5D-8F28-7BA4ADD53E24", "Line No. (Goods Item Number)");
			MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_LineNoInfo, propertyDescription);
		}

		bool CSICodeIsSpecialProcedureCode() => ValidationLists.SpecialProcedureCodesForPreviousDocuments(parent.Factory).Contains(parent.CSI_Code);
	}

	void CheckRuleR0024E()
	{
		var parent = Parent;
		if (parent.CSI_Code == PreviousDocumentCodes.CLE && IsReferenceNumberInvalid())
		{
			parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("PLExportPreviousDocumentValidation|CheckRuleR0024E"
				, "(R0024E) Invalid Reference number format, expected YYYYMMDD-n"));
		}

		bool IsReferenceNumberInvalid()
		{
			var splitReferenceNumber = parent.CSI_ReferenceNumber.Split('-');
			return splitReferenceNumber.Length != 2
					|| splitReferenceNumber[0][0] == '0'
					|| !ZDateTime.TryParseExact(splitReferenceNumber[0], out _, "yyyyMMdd")
					|| !ZInt.TryParse(splitReferenceNumber[1], out _);
		}
	}

	void CheckRuleR0022E()
	{
		var parent = Parent;
		if (parent.CSI_Code == PreviousDocumentCodes.AAD
			&& parent.CSI_ReferenceNumber.Length != 21)
		{
			parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("PLExportPreviousDocumentValidation|CheckRuleR0022E"
				, "(R0022E) Reference number must be 21 characters long"));
		}
	}

	void CheckRuleR0032E()
	{
		var parent = Parent;
		if (parent.CSI_ReferenceNumber.SubstringSafe(2, 2) != CountryCodes.Poland
			&& parent.CSI_Code == PreviousDocumentCodes.ZZZ
			&& parent.HasEntryInstructionProcedureCode(ProcedureCodes._31)
			&& parent.HasPreviousProcedureCode(ValidationLists.R0032EPreviousProcedures(parent.Factory))
			&& !parent.HasSupportingDocumentCodes(ValidationLists.R0032ESupportingDocuments(parent.Factory)))
		{
			parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("PLExportPreviousDocumentValidation|CheckRuleR0032E",
				"(R0032E) A supporting document C710 or 4DK3 is required for each Entry Line"));
		}
	}
}
