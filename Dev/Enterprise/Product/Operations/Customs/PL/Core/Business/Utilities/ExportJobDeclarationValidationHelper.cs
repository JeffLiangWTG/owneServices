using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

static class ExportJobDeclarationValidationHelper
{
	public static bool IsC0890TransportIdRequired(JobDeclaration declaration) => GetC0890TransportIdPresence(declaration) == PresenceType.Required;

	public static bool IsC0890TransportIdAllowed(JobDeclaration declaration) => GetC0890TransportIdPresence(declaration) != PresenceType.Forbidden;

	public static PresenceType GetC0890TransportIdPresence(JobDeclaration declaration)
	{
		if (declaration?.JE_TransportMode.IsEmpty ?? false)
		{
			return PresenceType.Forbidden;
		}
		switch (declaration.JE_EntryStyle)
		{
			case EntryStyleListExport.Codes.ExportNormal when declaration.CustomsEntryInstructions.Any(x => IsProcedureCodeIn_10_11_23_31(x.CEI_Procedure)):
				return (declaration.IsRail || declaration.IsMail || declaration.IsFixedInstallation)
					? PresenceType.Optional
					: PresenceType.Required;
			case EntryStyleListExport.Codes.ExportToSpecialTerritory when declaration.CustomsEntryInstructions.Any(x => IsProcedureCodeIn_76_77(x.CEI_Procedure)):
				return (declaration.IsMail || declaration.IsFixedInstallation)
					? PresenceType.Forbidden
					: PresenceType.Required;
			default:
				return PresenceType.Forbidden;
		}

		bool IsProcedureCodeIn_10_11_23_31(ZString code) => code == ProcedureCodes._10 || code == ProcedureCodes._11 || code == ProcedureCodes._23 || code == ProcedureCodes._31;
		bool IsProcedureCodeIn_76_77(ZString code) => code == ProcedureCodes._76 || code == ProcedureCodes._77;
	}
}
