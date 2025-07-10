using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	partial class PGADisclaimReasonList
	{
		public static bool IsValidForDoesRequireTariff(string code)
		{
			return code == Codes.C || code == Codes.D;
		}

		public static CodeDescriptionPairList GetDisclaimReasonList(string requirementCode, BusinessObjectFactory factory, bool shouldCheckEntryType, string entryType, string programCodeWithNoFlagged = "")
		{
			return factory.GetCachedValue("PGADisclaimReasonListFor" + requirementCode + shouldCheckEntryType + entryType + programCodeWithNoFlagged, delegate
			{
				return GetDisclaimReasonList(requirementCode, shouldCheckEntryType, entryType, programCodeWithNoFlagged);
			});
		}

		static CodeDescriptionPairList GetDisclaimReasonList(string requirementCode, bool shouldCheckEntryType, string entryType, string programCodeWithNoFlagged = "")
		{
			var result = new PGADisclaimReasonList();

			if (requirementCode == ZString.Empty && programCodeWithNoFlagged == GovernmentAgencyProgramCodeList.Codes.FWS)
			{
				result.RemoveCode(PGADisclaimReasonList.Codes.A);
				result.RemoveCode(PGADisclaimReasonList.Codes.B);
				result.RemoveCode(PGADisclaimReasonList.Codes.F);
				result.RemoveCode(PGADisclaimReasonList.Codes.G);

				return result;
			}

			if (requirementCode != OGARequirementList.Codes.FW1)
			{
				result.RemoveCode(PGADisclaimReasonList.Codes.E);
			}

			if ((requirementCode != OGARequirementList.Codes.FD1
				&& requirementCode != OGARequirementList.Codes.FD2
				&& requirementCode != OGARequirementList.Codes.FD3
				&& requirementCode != OGARequirementList.Codes.FD4)
				|| (shouldCheckEntryType && entryType != EntryTypeList.Codes.Warehouse))
			{
				result.RemoveCode(PGADisclaimReasonList.Codes.F);
			}

			if (requirementCode != OGARequirementList.Codes.AL1
				&& requirementCode != OGARequirementList.Codes.AL2)
			{
				result.RemoveCode(PGADisclaimReasonList.Codes.G);
			}

			switch (requirementCode)
			{
				case OGARequirementList.Codes.AQ1:
					result.RemoveCode(PGADisclaimReasonList.Codes.C);
					break;
				case OGARequirementList.Codes.AQ2:
					result.RemoveCode(PGADisclaimReasonList.Codes.D);
					break;
				case OGARequirementList.Codes.DT1:
				case OGARequirementList.Codes.FD1:
				case OGARequirementList.Codes.FD2:
				case OGARequirementList.Codes.FD3:
				case OGARequirementList.Codes.FD4:
				case OGARequirementList.Codes.DE1:
				case OGARequirementList.Codes.OM1:
				case OGARequirementList.Codes.AM7:
				case OGARequirementList.Codes.EH1:
					result.RemoveCode(PGADisclaimReasonList.Codes.B);
					result.RemoveCode(PGADisclaimReasonList.Codes.C);
					result.RemoveCode(PGADisclaimReasonList.Codes.D);
					break;
				case OGARequirementList.Codes.NM1:
				case OGARequirementList.Codes.NM3:
				case OGARequirementList.Codes.NM5:
				case OGARequirementList.Codes.AM1:
				case OGARequirementList.Codes.AM3:
				case OGARequirementList.Codes.CP1:
				case OGARequirementList.Codes.CP2:
					result.RemoveCode(PGADisclaimReasonList.Codes.C);
					result.RemoveCode(PGADisclaimReasonList.Codes.D);
					break;
				case OGARequirementList.Codes.FW2:
					result.Clear();
					break;
				case OGARequirementList.Codes.FW1:
				case OGARequirementList.Codes.FW3:
					result.RemoveCode(PGADisclaimReasonList.Codes.A);
					result.RemoveCode(PGADisclaimReasonList.Codes.B);
					break;
				case OGARequirementList.Codes.EP5:
					result.RemoveCode(PGADisclaimReasonList.Codes.B);
					break;
				case OGARequirementList.Codes.TB3:
					result.RemoveCode(PGADisclaimReasonList.Codes.B);
					result.RemoveCode(PGADisclaimReasonList.Codes.D);
					break;
				case OGARequirementList.Codes.TB1:
					result.RemoveCode(PGADisclaimReasonList.Codes.B);
					result.RemoveCode(PGADisclaimReasonList.Codes.D);
					result.RemoveCode(PGADisclaimReasonList.Codes.A);
					break;
				case OGARequirementList.Codes.AL2:
					result.RemoveCode(PGADisclaimReasonList.Codes.A);
					break;
			}
			return result;
		}

		public static CodeDescriptionPairList GetDisclaimReasonListWithoutTransactionalCodes(string requirementCode, BusinessObjectFactory factory, bool shouldCheckEntryType, string entryType)
		{
			return factory.GetCachedValue("ProductPGADisclaimReasonListFor" + requirementCode + shouldCheckEntryType + entryType, delegate
			{
				var result = GetDisclaimReasonList(requirementCode, shouldCheckEntryType, entryType);
				switch (requirementCode)
				{
					case OGARequirementList.Codes.AL2:
						result.RemoveCode(PGADisclaimReasonList.Codes.A);
						result.RemoveCode(PGADisclaimReasonList.Codes.B);
						break;
					default:
						result.RemoveCode(PGADisclaimReasonList.Codes.C);
						result.RemoveCode(PGADisclaimReasonList.Codes.D);
						break;
				}
				return result;
			});
		}
	}
}
