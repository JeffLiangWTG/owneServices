using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

public static class BaseMessageSendingObjectSecurityValidationHelper
{
	public static bool IsRuleR0095Valid(JobDeclaration declaration) =>
		declaration.JE_EntryStyle == EntryStyleListExport.Codes.ExportNormal
		&& declaration.EntryInstructionSubStyle is { } subStyles
		&& (subStyles.Contains(SubStyleCodes.A) || subStyles.Contains(SubStyleCodes.D));
}
