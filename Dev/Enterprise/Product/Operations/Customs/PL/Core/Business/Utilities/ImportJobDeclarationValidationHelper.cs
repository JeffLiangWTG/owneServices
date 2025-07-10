using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

internal static class ImportJobDeclarationValidationHelper
{
	public static void CheckRuleR489(ZPropertyInfoString targetInfo, JobDeclaration declaration)
	{
		if (targetInfo.Value.IsEmpty
			&& declaration != null
			&& declaration.JE_TransportIDInland.IsEmpty
			&& declaration.CustomsEntryInstructions.Any(entry => IsFeasibleR489SubStyle(entry.CEI_SubStyle)
																&& entry.CEI_Procedure != Constants.ProcedureCodes._71
																&& entry.InvoiceLines.Cast<JobComInvoiceLine>().Any(inv => inv.PreviousProcedureCode != Constants.ProcedureCodes._71)))
		{
			targetInfo.AddMessageError(Res.GetString("ImportJobDeclarationValidation|CheckRuleR489", "(R489) Transport ID [21] or Transport ID Inland [18] is missing."));
		}
	}

	static bool IsFeasibleR489SubStyle(ZString subStyle) => subStyle == Constants.SubStyleCodes.A
															|| subStyle == Constants.SubStyleCodes.B
															|| subStyle == Constants.SubStyleCodes.C
															|| subStyle == Constants.SubStyleCodes.X
															|| subStyle == Constants.SubStyleCodes.Y;

	public static void CheckRuleR233(ZPropertyInfoString targetInfo)
	{
		if (targetInfo.BizObj is JobDeclaration declaration
			&& declaration.IsRoad
			&& targetInfo.Value.HasInvalidTransportIdCharacters())
		{
			targetInfo.AddMessageError(Res.GetString("ImportJobDeclarationValidation|CheckRuleR233", "(R233) Transport ID contains invalid characters (only capital letters A through Z, digits 0 to 9 and the / character are allowed)."));
		}
	}

	public static void CheckRuleR208(ZPropertyInfoString targetInfo)
	{
		if (targetInfo.BizObj is JobDeclaration declaration
			&& declaration.IsRoadInland
			&& targetInfo.Value.HasInvalidTransportIdCharacters())
		{
			targetInfo.AddMessageError(Res.GetString("ImportJobDeclarationValidation|CheckRuleR208", "(R208) Transport ID contains invalid characters (only capital letters A through Z, digits 0 to 9 and the / character are allowed)."));
		}
	}

	static bool HasInvalidTransportIdCharacters(this ZString value) => Regex.IsMatch(value, @"[^A-Z0-9\/]");
}
