using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ImportAdditionalInfoValidation : AdditionalInfoValidation
{
	public ImportAdditionalInfoValidation(AdditionalInfo parent) : base(parent)
	{
	}

	#region CSI_Code

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();

		var parent = Parent;
		if (!parent.CSI_Code.IsEmpty)
		{
			if (parent is CusSupportingInfo parentInfo
				&& parentInfo.Parent is BusinessObject baseParent)
			{
				switch (baseParent)
				{
					case JobComInvoiceLine invoiceLine:
						AddCheckForInvoiceLine(invoiceLine);
						break;
					case JobComInvoiceHeader:
						AddCheckForInvoiceHeader();
						break;
					case CusEntryInstruction:
						AddCheckForCusEntryInstruction();
						break;
					default:
						break;
				}

				CheckForDuplicatedCSI_Code(parentInfo);
			}
			CheckRuleR626();
			CheckRuleR629();
			CheckRuleR632();
			CheckRuleR206();
			CheckRuleR634();
			CheckRuleR820();
			CheckRuleR1540();
			CheckRuleR1586();
			CheckRuleR1630();
		}

		void AddCheckForInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			CheckRuleForInvoiceLine(CheckForRuleR633, invoiceLine, RuleR633MessageError);
			CheckRuleForInvoiceLine(CheckForRuleR862, invoiceLine, RuleR862MessageError);
			CheckRuleForInvoiceLine(CheckForRuleR954, invoiceLine, RuleR954MessageError);
			CheckRuleForInvoiceLine(CheckForRuleR978, invoiceLine, RuleR978MessageError);
			CheckRuleForInvoiceLine(CheckForRuleR979, invoiceLine, RuleR979MessageError);
			CheckRuleForInvoiceLine(CheckForRuleR981, invoiceLine, RuleR981MessageError);
		}

		void AddCheckForInvoiceHeader()
		{
			CheckRuleForInvoiceHeader(CheckForRuleR633, Constants.AdditionalInfoCodes._00100, RuleR633MessageError);
			CheckRuleForInvoiceHeader(CheckForRuleR862, rule862CSI_CodeList, RuleR862MessageError);
			CheckRuleForInvoiceHeader(CheckForRuleR954, Constants.AdditionalInfoCodes._4PL07, RuleR954MessageError);
			CheckRuleForInvoiceHeader(CheckForRuleR978, rule978CSI_CodeList, RuleR978MessageError);
			CheckRuleForInvoiceHeader(CheckForRuleR979, Constants.AdditionalInfoCodes._1PL15, RuleR979MessageError);
			CheckRuleForInvoiceHeader(CheckForRuleR981, Constants.AdditionalInfoCodes._00100, RuleR981MessageError);
		}

		void AddCheckForCusEntryInstruction()
		{
			CheckRuleForEntryInstruction(CheckForRuleR633, Constants.AdditionalInfoCodes._00100, RuleR633MessageError);
			CheckRuleForEntryInstruction(CheckForRuleR862, rule862CSI_CodeList, RuleR862MessageError);
			CheckRuleForEntryInstruction(CheckForRuleR954, Constants.AdditionalInfoCodes._4PL07, RuleR954MessageError);
			CheckRuleForEntryInstruction(CheckForRuleR978, rule978CSI_CodeList, RuleR978MessageError);
			CheckRuleForEntryInstruction(CheckForRuleR979, Constants.AdditionalInfoCodes._1PL15, RuleR979MessageError);
			CheckRuleForEntryInstruction(CheckForRuleR981, Constants.AdditionalInfoCodes._00100, RuleR981MessageError);
		}
	}

	#region CheckForRuleX

	static string RuleR633MessageError => Res.GetString("PLAdditionalInfo|R633", "(R633) - Additional Info with Code '0PL05'...'0PL10' is required when additional information 00100 is present.");

	static bool CheckForRuleR633(JobComInvoiceLine invoiceLine)
	{
		var csiCodes = GetAllCSI_CodesForInvoiceLine(invoiceLine);
		if (csiCodes.Contains(Constants.AdditionalInfoCodes._00100))
		{
			var itemList = new List<ZString> { Constants.AdditionalInfoCodes._0PL05, Constants.AdditionalInfoCodes._0PL06, Constants.AdditionalInfoCodes._0PL07, Constants.AdditionalInfoCodes._0PL08, Constants.AdditionalInfoCodes._0PL09, Constants.AdditionalInfoCodes._0PL10 };
			if (!csiCodes.Any(x => itemList.Contains(x)))
			{
				return false;
			}
		}
		return true;
	}

	static string RuleR862MessageError => Res.GetString("PLImportJobComInvoiceLineValidation|R862", "R862 - Additional Info with Code '00100' is required with Additional Info code 0PL05...0PL10.");

	readonly IReadOnlyCollection<ZString> rule862CSI_CodeList = new List<ZString> { "0PL05", "0PL06", "0PL07", "0PL08", "0PL09", "0PL10" }.AsReadOnly();
	bool CheckForRuleR862(JobComInvoiceLine invoiceLine)
	{
		var subStyle = invoiceLine.EntryInstruction?.CEI_SubStyle ?? string.Empty;
		if (subStyle == Constants.SubStyleCodes.A || subStyle == Constants.SubStyleCodes.D)
		{
			var csiCodes = GetAllCSI_CodesForInvoiceLine(invoiceLine);
			if (!csiCodes.Contains(Constants.AdditionalInfoCodes._00100))
			{
				if (IsCSI_CodeFromItemList(rule862CSI_CodeList, csiCodes))
				{
					return false;
				}
			}
		}
		return true;
	}

	static string RuleR954MessageError => Res.GetString("PLAdditionalInfo|R954", "R954 - Additional Information code '04PL07' is valid for Requested Procedure code '51', '53' or '48'.");

	bool CheckForRuleR954(JobComInvoiceLine invoiceLine)
	{
		var csiCodes = GetAllCSI_CodesForInvoiceLine(invoiceLine);
		var currentCusProcedure = invoiceLine?.ProcedureCodeBase ?? ZString.Empty;
		return !csiCodes.Contains(Constants.AdditionalInfoCodes._4PL07) || currentCusProcedure == Constants.ProcedureCodes._48 || currentCusProcedure == Constants.ProcedureCodes._51 || currentCusProcedure == Constants.ProcedureCodes._53;
	}

	static string RuleR978MessageError => Res.GetString("PLAdditionalInfo|R978", "R978 - Additional Info with Code '1PL12', '1PL13' and '1PL14' is not valid when Sub Style is 'D', 'E' and 'F'.");

	readonly IReadOnlyCollection<ZString> rule978CSI_CodeList = new List<ZString> { "1PL12", "1PL13", "1PL14" };
	bool CheckForRuleR978(JobComInvoiceLine invoiceLine)
	{
		var csiCodes = GetAllCSI_CodesForInvoiceLine(invoiceLine);
		if (IsCSI_CodeFromItemList(rule978CSI_CodeList, csiCodes))
		{
			var itemList = new List<ZString> { Constants.SubStyleCodes.D, Constants.SubStyleCodes.E, Constants.SubStyleCodes.F };
			if (IsSubStyleFromItemList(itemList, invoiceLine))
			{
				return false;
			}
		}
		return true;
	}

	static string RuleR979MessageError => Res.GetString("PLAdditionalInfo|R979", "R979 - Additional Info with Code '1PL15' is not allowed for Sub Style code 'B', 'C', 'E', 'F', 'X', 'Y' and 'Z'.");

	static bool CheckForRuleR979(JobComInvoiceLine invoiceLine)
	{
		var csiCodes = GetAllCSI_CodesForInvoiceLine(invoiceLine);
		if (csiCodes.Contains(Constants.AdditionalInfoCodes._1PL15))
		{
			var itemList = new List<ZString> { Constants.SubStyleCodes.B, Constants.SubStyleCodes.C, Constants.SubStyleCodes.E, Constants.SubStyleCodes.F
				, Constants.SubStyleCodes.X, Constants.SubStyleCodes.Y, Constants.SubStyleCodes.Z };
			if (IsSubStyleFromItemList(itemList, invoiceLine))
			{
				return false;
			}
		}
		return true;
	}

	static string RuleR981MessageError => Res.GetString("PLAdditionalInfo|R981", "R981 - Additional Info with Code '00100' is not allowed for Sub Style code 'B', 'C', 'E', 'F', 'X', 'Y' and 'Z'.");

	static bool CheckForRuleR981(JobComInvoiceLine invoiceLine)
	{
		var csiCodes = GetAllCSI_CodesForInvoiceLine(invoiceLine);
		if (csiCodes.Contains(Constants.AdditionalInfoCodes._00100))
		{
			var itemList = new List<ZString> { Constants.SubStyleCodes.B, Constants.SubStyleCodes.C, Constants.SubStyleCodes.E, Constants.SubStyleCodes.F
				, Constants.SubStyleCodes.X, Constants.SubStyleCodes.Y, Constants.SubStyleCodes.Z };
			if (IsSubStyleFromItemList(itemList, invoiceLine))
			{
				return false;
			}
		}
		return true;
	}

	#endregion

	#region CheckRuleForInvoiceLine

	static bool CheckRuleForInvoiceLine(Func<JobComInvoiceLine, bool> checkMethod, JobComInvoiceLine invoiceLine, string messageError)
	{
		var checkResult = checkMethod(invoiceLine);

		if (!checkResult)
		{
			invoiceLine.AddRowMessageError(messageError);
		}
		else
		{
			invoiceLine.RemoveRowMessageError(messageError);
		}

		return checkResult;
	}

	#endregion

	#region CheckRuleForInvoiceHeader

	void CheckRuleForInvoiceHeader(Func<JobComInvoiceLine, bool> checkMethod, IEnumerable<ZString> expectedCSICodeListToExecuteCheck, string messageError)
	{
		if (expectedCSICodeListToExecuteCheck.Contains(Parent.CSI_Code))
		{
			CheckRuleForInvoiceHeader(checkMethod, messageError);
		}
	}

	void CheckRuleForInvoiceHeader(Func<JobComInvoiceLine, bool> checkMethod, ZString expectedCSICodeToExecuteCheck, string messageError)
	{
		if (Parent.CSI_Code == expectedCSICodeToExecuteCheck)
		{
			CheckRuleForInvoiceHeader(checkMethod, messageError);
		}
	}

	void CheckRuleForInvoiceHeader(Func<JobComInvoiceLine, bool> checkMethod, string messageError)
	{
		var parent = Parent;
		var invoiceLines = (parent.Parent as JobComInvoiceHeader)?.InvoiceLines;
		if (invoiceLines is not null && invoiceLines.Count != 0)
		{
			foreach (var invoiceLine in invoiceLines)
			{
				var line = invoiceLine as JobComInvoiceLine;
				if (!CheckRuleForInvoiceLine(checkMethod, line, messageError))
				{
					parent.CSI_CodeInfo.AddMessageError(messageError);
					break;
				}
			}
		}
	}

	#endregion

	#region CheckRuleForJobDeclaration

	void CheckRuleForEntryInstruction(Func<JobComInvoiceLine, bool> checkMethod, ZString expectedCSICodeToExecuteCheck, string messageError)
	{
		if (Parent.CSI_Code == expectedCSICodeToExecuteCheck)
		{
			CheckRuleForEntryInstruction(checkMethod, messageError);
		}
	}

	void CheckRuleForEntryInstruction(Func<JobComInvoiceLine, bool> checkMethod, IReadOnlyCollection<ZString> expectedCSICodeListToExecuteCheck, string messageError)
	{
		if (expectedCSICodeListToExecuteCheck.Contains(Parent.CSI_Code))
		{
			CheckRuleForEntryInstruction(checkMethod, messageError);
		}
	}

	void CheckRuleForEntryInstruction(Func<JobComInvoiceLine, bool> checkMethod, string messageError)
	{
		var parent = Parent;
		var invoiceLines = (parent.Parent as CusEntryInstruction)?.InvoiceLines;
		if (invoiceLines is not null && invoiceLines.Any())
		{
			foreach (var invoiceLine in invoiceLines)
			{
				var line = invoiceLine;
				if (!CheckRuleForInvoiceLine(checkMethod, line, messageError))
				{
					parent.CSI_CodeInfo.AddMessageError(messageError);
					break;
				}
			}
		}
	}

	#endregion

	void CheckForDuplicatedCSI_Code(CusSupportingInfo parentInfo)
	{
		var code = Parent.CSI_Code;
		var parent = parentInfo.Parent;

		var csiCodeOccurrences = parent switch
		{
			JobDeclaration => GetAllCSI_CodesForJobDeclaration().Count(x => x == code),
			JobComInvoiceHeader => GetAllCSI_CodesForInvoiceHeader().Count(x => x == code),
			JobComInvoiceLine invoiceLine => GetAllCSI_CodesForInvoiceLine(invoiceLine).Count(x => x == code),
			_ => 0
		};

		if (csiCodeOccurrences > 1)
		{
			Parent.CSI_CodeInfo.AddMessageError(Res.GetString("PLAdditionalInfo|DuplicatedCSI_Code", "The Code:\'{0}\' has already been specified for this Invoice Line or corresponding Invoice Header's / Declaration's Additional Info list", code));
		}
	}

	#endregion

	static bool IsCSI_CodeFromItemList(IEnumerable<ZString> itemList, IEnumerable<ZString> csiCodes) => csiCodes.Any(itemList.Contains);

	static bool IsSubStyleFromItemList(List<ZString> itemList, JobComInvoiceLine invoiceLine) => itemList.Contains(GetCEI_SubStyle(invoiceLine));

	static ZString GetCEI_SubStyle(JobComInvoiceLine invoiceLine) => invoiceLine?.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;

	static List<ZString> GetAllCSI_CodesForInvoiceLine(JobComInvoiceLine invoiceLine)
	{
		var result = new List<ZString>();

		AddAdditionalInfosCodesToList(result, invoiceLine?.AdditionalInfos);
		AddAdditionalInfosCodesToList(result, invoiceLine?.InvoiceHeader?.AdditionalInfos);
		AddAdditionalInfosCodesToList(result, invoiceLine?.EntryInstruction?.AdditionalInfos);

		return result;
	}

	IEnumerable<ZString> GetAllCSI_CodesForInvoiceHeader()
	{
		var result = new List<ZString>();

		if (Parent.Parent is JobComInvoiceHeader header)
		{
			AddAdditionalInfosCodesToList(result, header.AdditionalInfos);
			AddAdditionalInfosCodesToList(result, header.JobDeclaration?.AdditionalInfos);
			foreach (var line in header.InvoiceLines)
			{
				AddAdditionalInfosCodesToList(result, ((JobComInvoiceLine)line).AdditionalInfos);
			}
		}

		return result;
	}

	IEnumerable<ZString> GetAllCSI_CodesForJobDeclaration()
	{
		var result = new List<ZString>();
		if (Parent.Parent is JobDeclaration declaration)
		{
			AddAdditionalInfosCodesToList(result, declaration.AdditionalInfos);
			foreach (var header in declaration.Invoices)
			{
				AddAdditionalInfosCodesToList(result, ((JobComInvoiceHeader)header).AdditionalInfos);
			}
			foreach (var line in declaration.InvoiceLines)
			{
				AddAdditionalInfosCodesToList(result, ((JobComInvoiceLine)line).AdditionalInfos);
			}
		}
		return result;
	}

	static void AddAdditionalInfosCodesToList(List<ZString> list, AdditionalInfoCollection additionalInfo)
	{
		if (additionalInfo != null)
		{
			list.AddRange(additionalInfo.Select(x => ((AdditionalInfo)x).CSI_Code).ToList());
		}
	}

	void CheckRuleR206()
	{
		if (Parent.CSI_Code == Constants.AdditionalInfoCodes._00500
			&& new ZString(Parent.JobDeclaration?.JE_DeclarantType) == PLRepresentationTypeList.Codes._5Indirect)
		{
			Parent.CSI_CodeInfo.AddMessageError(Res.GetString("PLImportAdditionalInfoValidation|CheckRuleR206"
				, "(R206) – Additional Info code 00500 is not allowed for Indirect Representation."));
		}
	}

	void CheckRuleR626()
	{
		if (Parent.CSI_Code == Constants.AdditionalInfoCodes._00100
			&& Parent.HasEntryInstructionProcedureCode(Constants.ProcedureCodes._51)
			&& Parent.HasSupportingDocumentCode(Constants.SupportingDocumentCodes.C601))
		{
			Parent.CSI_CodeInfo.AddMessageError(Res.GetString("ImportPLAdditionalInfo|CheckRuleR626"
				, "(R626) Additional Information code 00100 and Supporting Document/Authorization code C601 cannot be used together."));
		}
	}

	void CheckRuleR634()
	{
		if (Parent.CSI_Code == Constants.AdditionalInfoCodes._00100
			&& Parent.HasEntryInstructionProcedureCode(Constants.ProcedureCodes._44)
			&& Parent.HasSupportingDocumentCodeWithReferenceNumber(Constants.SupportingDocumentCodes.N990))
		{
			Parent.CSI_CodeInfo.AddMessageError(Res.GetString("ImportPLAdditionalInfo|CheckRuleR634"
				, "(R634) – The 00100 Additional Information code cannot exist for requested procedure code 44 with Supporting Document N990."));
		}
	}

	void CheckRuleR820()
	{
		if (Parent.CSI_Code == Constants.AdditionalInfoCodes._4PL09 && Parent.HasEntryInstructionProcedureCode(Constants.ProcedureCodes._48)
																	&& (!Parent.HasSupportingDocumentCode(Constants.SupportingDocumentCodes.C019) && !Parent.HasAdditionalDocumentCode(Constants.AdditionalInfoCodes._00100)))
		{
			Parent.CSI_CodeInfo.AddMessageError(Res.GetString("ImportPLAdditionalInfo|CheckRuleR820"
				, "(R820) – Additional Information code 00100 or Supporting Document code C019 is required."));
		}
	}

	void CheckRuleR1540()
	{
		if (Parent.CSI_Code == Constants.AdditionalInfoCodes._00100
			&& Parent.HasEntryInstructionProcedureCode(Constants.ProcedureCodes._44)
			&& !Parent.HasAdditionalDocumentCode(Constants.AdditionalInfoCodes._4PL07)
			&& !Parent.HasAdditionalDocumentCode(Constants.AdditionalInfoCodes._4PL15))
		{
			Parent.CSI_CodeInfo.AddMessageError(Res.GetString("ImportPLAdditionalInfo|CheckRuleR1540"
				, "(R1540) – For end use procedure with 00100 code, Additional Information code 4PL15 or 4PL07 must exist."));
		}
	}

	void CheckRuleR1586()
	{
		var offices = Parent.JobDeclaration?.CustomsOfficesForBinding;
		if (Parent.CSI_Code == Constants.AdditionalInfoCodes._00100
			&& (null == offices || offices.Count == 0 || !offices.Cast<EuOfficeCode>().Any(office => office.CY_Code == EuOfficeCodesTypes.Codes.AuthorityControlCode)))
		{
			Parent.CSI_CodeInfo.AddMessageError(Res.GetString("ImportPLAdditionalInfo|CheckRuleR1586"
				, "(R1586) – If there is an Additional Information code 00100, the Code of the Supervising Customs Office (SCO) is required."));
		}
	}

	void CheckRuleR1630()
	{
		if (Parent.CSI_Code == Constants.AdditionalInfoCodes._00100
			&& Parent.HasEntryInstructionProcedureCode(Constants.ProcedureCodes._46))
		{
			Parent.CSI_CodeInfo.AddMessageError(Res.GetString("ImportPLAdditionalInfo|CheckRuleR1630"
				, "(R1630) – For requested procedure code 46, the Additional Information code 00100 is not allowed."));
		}
	}

	void CheckRuleR629()
	{
		if (Parent.CSI_Code == Constants.AdditionalInfoCodes._00100
			&& Parent.HasEntryInstructionProcedureCode(Constants.ProcedureCodes._53)
			&& Parent.HasSupportingDocumentCode(Constants.SupportingDocumentCodes.C516))
		{
			Parent.CSI_CodeInfo.AddMessageError(Res.GetString("ImportPLAdditionalInfo|CheckRuleR629",
				"(R629) Additional Information code 00100 and Supporting Document/Authorization code C516 cannot be used together."));
		}
	}

	void CheckRuleR632()
	{
		if (Parent.CSI_Code == Constants.AdditionalInfoCodes._00100
			&& Parent.HasEntryInstructionProcedureCode(Constants.ValidationLists.R632Procedures(Parent.Factory))
			&& Parent.HasSupportingDocumentCode(Constants.SupportingDocumentCodes.C019)
			&& !Parent.HasAdditionalDocumentCode(Constants.AdditionalInfoCodes._4PL09))
		{
			Parent.CSI_CodeInfo.AddMessageError(Res.GetString("ImportPLAdditionalInfo|CheckRuleR632",
				"(R632) Additional Information code 00100 and Supporting Document/Authorization code C019 can coexist only with Additional Information code 4PL09."));
		}
	}
}
