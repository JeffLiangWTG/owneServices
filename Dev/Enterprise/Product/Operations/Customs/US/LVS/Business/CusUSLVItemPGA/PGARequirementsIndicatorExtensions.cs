using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.LVS
{
	public static class PGARequirementsIndicatorExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static bool IsPGAProgramRequired(this PGARequirementIndicator pgaRequirementIndicator, ZString program)
		{
			var result = false;
			switch (program)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					result = pgaRequirementIndicator.RequireAPHIS;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					result = pgaRequirementIndicator.RequireACEFDA;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					result = pgaRequirementIndicator.RequireFSIS;
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					result = pgaRequirementIndicator.RequireAMS;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					result = pgaRequirementIndicator.RequireNOP;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					result = pgaRequirementIndicator.RequireFWS;
					break;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					result = pgaRequirementIndicator.RequireACE_LaceyData;
					break;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					result = pgaRequirementIndicator.RequireODS;
					break;
				case GovernmentAgencyProgramCodeList.Codes.PST:
					result = pgaRequirementIndicator.RequirePST;
					break;
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					result = pgaRequirementIndicator.RequireVNE;
					break;
				case GovernmentAgencyProgramCodeList.Codes._370:
					result = pgaRequirementIndicator.RequireNMFS370;
					break;
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					result = pgaRequirementIndicator.RequireTSCA;
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					result = pgaRequirementIndicator.RequireNMFSAMR;
					break;
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					result = pgaRequirementIndicator.RequireOMC;
					break;
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					result = pgaRequirementIndicator.RequireNMFSHMS;
					break;
				case GovernmentAgencyProgramCodeList.Codes.SIMP:
					result = pgaRequirementIndicator.RequireNMFSSIM;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					result = pgaRequirementIndicator.RequireNHTSA;
					break;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					result = pgaRequirementIndicator.RequireTTB;
					break;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					result = pgaRequirementIndicator.RequireCPSC;
					break;
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static bool HasPGAProgram(this PGARequirementIndicator pgaRequirementIndicator, ZString program)
		{
			var result = false;
			switch (program)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					result = pgaRequirementIndicator.HasAPHISRequirement;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					result = pgaRequirementIndicator.HasACEFDARequirement;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					result = pgaRequirementIndicator.HasFSISRequirement;
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					result = pgaRequirementIndicator.HasAMSRequirement;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					result = pgaRequirementIndicator.HasNOPRequirement;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					result = pgaRequirementIndicator.HasFWSRequirement;
					break;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					result = pgaRequirementIndicator.MayRequireACSLacey;
					break;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					result = pgaRequirementIndicator.HasODSRequirement;
					break;
				case GovernmentAgencyProgramCodeList.Codes.PST:
					result = pgaRequirementIndicator.HasPSTRequirement;
					break;
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					result = pgaRequirementIndicator.HasVNERequirement;
					break;
				case GovernmentAgencyProgramCodeList.Codes._370:
					result = pgaRequirementIndicator.HasNMFS370Requirement;
					break;
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					result = pgaRequirementIndicator.HasTSCARequirement;
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					result = pgaRequirementIndicator.HasNMFSAMRRequirement;
					break;
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					result = pgaRequirementIndicator.HasOMCRequirement;
					break;
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					result = pgaRequirementIndicator.HasNMFSHMSRequirement;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					result = pgaRequirementIndicator.HasNHTSARequirement;
					break;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					result = pgaRequirementIndicator.HasTTBRequirement;
					break;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					result = pgaRequirementIndicator.HasCPSCRequirement;
					break;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					result = pgaRequirementIndicator.HasDEARequirement;
					break;
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static bool IsPGAProgramMayRequired(this PGARequirementIndicator pgaRequirementIndicator, ZString program)
		{
			var result = false;
			switch (program)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					result = pgaRequirementIndicator.HasAPHISRequirement && !pgaRequirementIndicator.RequireAPHIS;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					result = pgaRequirementIndicator.HasACEFDARequirement && !pgaRequirementIndicator.RequireACEFDA;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					result = pgaRequirementIndicator.HasFSISRequirement && !pgaRequirementIndicator.RequireFSIS;
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					result = pgaRequirementIndicator.HasAMSRequirement && !pgaRequirementIndicator.RequireAMS;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					result = pgaRequirementIndicator.HasNOPRequirement && !pgaRequirementIndicator.RequireNOP;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					result = pgaRequirementIndicator.HasFWSRequirement && !pgaRequirementIndicator.RequireFWS;
					break;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					result = pgaRequirementIndicator.MayRequireACSLacey;
					break;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					result = pgaRequirementIndicator.HasODSRequirement && !pgaRequirementIndicator.RequireODS;
					break;
				case GovernmentAgencyProgramCodeList.Codes.PST:
					result = pgaRequirementIndicator.HasPSTRequirement && !pgaRequirementIndicator.RequirePST;
					break;
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					result = pgaRequirementIndicator.HasVNERequirement && !pgaRequirementIndicator.RequireVNE;
					break;
				case GovernmentAgencyProgramCodeList.Codes._370:
					result = pgaRequirementIndicator.HasNMFS370Requirement && !pgaRequirementIndicator.RequireNMFS370;
					break;
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					result = pgaRequirementIndicator.HasTSCARequirement && !pgaRequirementIndicator.RequireTSCA;
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					result = pgaRequirementIndicator.HasNMFSAMRRequirement && !pgaRequirementIndicator.RequireNMFSAMR;
					break;
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					result = pgaRequirementIndicator.HasOMCRequirement && !pgaRequirementIndicator.RequireOMC;
					break;
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					result = pgaRequirementIndicator.HasNMFSHMSRequirement && !pgaRequirementIndicator.RequireNMFSHMS;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					result = pgaRequirementIndicator.HasNHTSARequirement && !pgaRequirementIndicator.RequireNHTSA;
					break;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					result = pgaRequirementIndicator.HasTTBRequirement && !pgaRequirementIndicator.RequireTTB;
					break;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					result = pgaRequirementIndicator.HasCPSCRequirement && !pgaRequirementIndicator.RequireCPSC;
					break;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					result = pgaRequirementIndicator.HasDEARequirement;
					break;
			}
			return result;
		}
	}
}
