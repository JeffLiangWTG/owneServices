using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVItemPGAAgencyRequirementsProvider : PGAAgencyRequirementsProvider
	{
		public CusUSLVItemPGAAgencyRequirementsProvider(CusUSLVItem item)
			: base(item.Factory)
		{
			ParentItem = item;
		}

		public CusUSLVItem ParentItem { get; }

		public override bool IsPGAReqirementRelevant => true;

		public override bool DoesMatchCertificationMode(string agencyCode)
		{
			return IsPGA(agencyCode)
				|| agencyCode == GovernmentAgencyProgramCodeList.Codes.FCC
				|| agencyCode == GovernmentAgencyProgramCodeList.Codes.FSIS
				|| agencyCode == GovernmentAgencyProgramCodeList.Codes.FDA;
		}

		public override bool IsPGA(string agencyCode)
		{
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.DOT:
					return false;

				default:
					return true;
			}
		}

		public override ZPropertyInfo GetDisclaimReasonInfo(string agencyCode)
		{
			return default;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override CodeDescriptionPairList GetDisclaimReasonList(string agencyCode)
		{
			var reqCode = ZString.Empty;

			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					reqCode = ParentItem.OGARequirementCalculator.APHISRequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					reqCode = ParentItem.OGARequirementCalculator.OMCRequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					reqCode = ParentItem.OGARequirementCalculator.ACEFDARequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					reqCode = ParentItem.OGARequirementCalculator.FSISRequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					reqCode = ParentItem.OGARequirementCalculator.FWSRequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					reqCode = ParentItem.OGARequirementCalculator.ACELaceyRequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					reqCode = ParentItem.OGARequirementCalculator.ODSRequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.PST:
					reqCode = ParentItem.OGARequirementCalculator.PSTRequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					reqCode = ParentItem.OGARequirementCalculator.VNERequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes._370:
					reqCode = ParentItem.OGARequirementCalculator.NMFS370RequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.SIMP:
					reqCode = ParentItem.OGARequirementCalculator.NMFSSIMPRequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					reqCode = ParentItem.OGARequirementCalculator.TSCARequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					reqCode = ParentItem.OGARequirementCalculator.NMFSAMRRequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					reqCode = ParentItem.OGARequirementCalculator.NMFSHMSRequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					reqCode = ParentItem.OGARequirementCalculator.NHTSARequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					reqCode = ParentItem.OGARequirementCalculator.TTBRequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					reqCode = ParentItem.OGARequirementCalculator.AMSRequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					reqCode = ParentItem.OGARequirementCalculator.NOPRequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					reqCode = ParentItem.OGARequirementCalculator.CPSCRequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					reqCode = ParentItem.OGARequirementCalculator.DEARequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					reqCode = ParentItem.OGARequirementCalculator.HFCRequirementCode;
					break;
				case GovernmentAgencyProgramCodeList.Codes.ATF:
				case GovernmentAgencyProgramCodeList.Codes.DDTC:
					return new PGADisclaimReasonList();
#if DEBUG
				case "TST":
					var testResult = new CodeDescriptionPairList();
					testResult.AddPair("T", "TestReason");
					return testResult;
#endif
			}

			return reqCode.IsEmpty ? new CodeDescriptionPairList() : ParentItem.Lookups.GetDisclaimReasonList(reqCode);
		}

		public override CodeDescriptionPairList GetGovernmentAgencyProgramCodeList()
		{
			return Factory.GetCachedValue("USLVSGovernmentAgencyProgramCodeList", delegate
			{
				var result = new GovernmentAgencyProgramCodeList();
				result.RemoveCode(GovernmentAgencyProgramCodeList.Codes.EPA);
				result.RemoveCode(GovernmentAgencyProgramCodeList.Codes.FCC);
				result.RemoveCode(GovernmentAgencyProgramCodeList.Codes.COA);
				return result;
			});
		}

		public override ZPropertyInfo GetIndicatorInfo(string agencyCode)
		{
			return default;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public (string Agency, string Program) PopulateAgencyProgram(string agencyCode)
		{
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					return ("APH", "AVS");
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					return ("CPS", "CPS");
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					return ("DEA", "DEA");
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					return ("NHT", "OFF");
				case GovernmentAgencyProgramCodeList.Codes.ATF:
					return ("ATF", "ATF");
				case GovernmentAgencyProgramCodeList.Codes.DDTC:
					return ("DTC", "DTC");
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					return ("EPA", "ODS");
				case GovernmentAgencyProgramCodeList.Codes.PST:
					return ("EPA", "PS1");
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					return ("EPA", "TS1");
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					return ("EPA", "VNE");
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					return ("EPA", "HFC");
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					return ("FDA", "FDA");
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					return ("FWS", "FWS");
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					return ("APH", "APL");
				case GovernmentAgencyProgramCodeList.Codes._370:
					return ("NMF", "370");
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					return ("NMF", "AMR");
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					return ("NMF", "HMS");
				case GovernmentAgencyProgramCodeList.Codes.SIMP:
					return ("NMF", "SIM");
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					return ("OMC", "OMC");
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					return ("TTB", "TOB");
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					return ("AMS", "MO8");
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					return ("NOP", "OR1");
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					return ("FSI", "FSI");

				default:
					throw new InvalidOperationException(FormattableString.Invariant($"Unknown agency program code '{agencyCode}'."));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public string PopulateAgencyCodeWithDescription(CodeDescriptionPair program)
		{
			var result = string.Empty;
			switch (program.Code)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					result = program.CodeAndDescription;
					break;
				case GovernmentAgencyProgramCodeList.Codes._370:
				case GovernmentAgencyProgramCodeList.Codes.AMR:
				case GovernmentAgencyProgramCodeList.Codes.HMS:
				case GovernmentAgencyProgramCodeList.Codes.SIMP:
					result = "NMFS - " + program.CodeAndDescription;
					break;
				case GovernmentAgencyProgramCodeList.Codes.DDTC:
					result = program.CodeAndDescription;
					break;
				case GovernmentAgencyProgramCodeList.Codes.DOT:
				case GovernmentAgencyProgramCodeList.Codes.FCC:
					result = program.Code + " - OGA - " + program.Description;
					break;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
				case GovernmentAgencyProgramCodeList.Codes.PST:
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
				case GovernmentAgencyProgramCodeList.Codes.VNE:
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					result = "EPA - " + program.CodeAndDescription;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					result = "USDA - " + program.CodeAndDescription;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					result = "USDA - AMS - " + program.CodeAndDescription;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					result = "DOT - " + program.CodeAndDescription;
					break;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					{
						if (IsPGA(program.Code))
						{
							result = program.Code + " - PGA - " + program.Description;
						}
						else
						{
							result = program.Code + " - OGA - " + program.Description;
						}
						break;
					}
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					result = GovernmentAgencyProgramCodeList.Codes.OMC + " - " + GovernmentAgencyProgramCodeList.Descriptions.OMC;
					break;

				case GovernmentAgencyProgramCodeList.Codes.TTB:
				case GovernmentAgencyProgramCodeList.Codes.ATF:
					result = program.Code + " - PGA - " + program.Description;
					break;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					result = GovernmentAgencyProgramCodeList.Codes.CPSC + " - " + GovernmentAgencyProgramCodeList.Descriptions.CPSC;
					break;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					result = GovernmentAgencyProgramCodeList.Codes.DEA + " - " + GovernmentAgencyProgramCodeList.Descriptions.DEA;
					break;

				default:
					throw new InvalidOperationException(FormattableString.Invariant($"Unknown agency program code '{program.Code}'."));
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override ZString GetRequirementDescription(string agencyCode)
		{
			var result = ZString.Empty;

			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					result = ParentItem.OGARequirementCalculator.APHISRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					result = ParentItem.OGARequirementCalculator.OMCRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					result = ParentItem.OGARequirementCalculator.ACEFDARequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					result = ParentItem.OGARequirementCalculator.FSISRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					result = ParentItem.OGARequirementCalculator.FWSRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					result = ZString.Empty;
					break;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					result = ParentItem.OGARequirementCalculator.ODSRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.PST:
					result = ParentItem.OGARequirementCalculator.PSTRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					result = ParentItem.OGARequirementCalculator.VNERequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes._370:
					result = ParentItem.OGARequirementCalculator.NMFS370RequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					result = ParentItem.OGARequirementCalculator.TSCARequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					result = ParentItem.OGARequirementCalculator.NMFSAMRRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					result = ParentItem.OGARequirementCalculator.NMFSHMSRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.SIMP:
					result = ParentItem.OGARequirementCalculator.NMFSSIMRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					result = ParentItem.OGARequirementCalculator.NHTSARequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					result = ParentItem.OGARequirementCalculator.TTBRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					result = ParentItem.OGARequirementCalculator.AMSRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					result = ParentItem.OGARequirementCalculator.NOPRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					result = ParentItem.OGARequirementCalculator.CPSCRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					result = ParentItem.OGARequirementCalculator.DEARequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					result = ParentItem.OGARequirementCalculator.HFCRequirementDesc;
					break;
#if DEBUG
				case "TST":
					return "TestRequirement";
#endif
			}

			return result == OGARequirementCalculator.NoRequirement ? ZString.Empty : result;
		}

		public override void ValidateDisclaimReason(string agencyCode)
		{
		}

		public override void ValidateIndicator(string agencyCode)
		{
		}
	}
}
