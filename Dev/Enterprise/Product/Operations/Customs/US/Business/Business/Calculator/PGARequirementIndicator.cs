using System;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class PGARequirementIndicator
	{
		public PGARequirementIndicator(Func<USCTariff> getTariff, Func<USCTariff> getSupTariff, Func<ZDateTime> effectiveDate, Func<ZString, ZBool> hasApplicableEntryType, Func<ZString> getCountryOfOrigin)
		{
			this.getTariff = getTariff;
			this.getSupTariff = getSupTariff;
			this.getEffectiveDate = effectiveDate;
			this.hasApplicableEntryType = hasApplicableEntryType;
			this.getCountryOfOrigin = getCountryOfOrigin;
		}

		readonly Func<USCTariff> getTariff;
		readonly Func<USCTariff> getSupTariff;
		readonly Func<ZDateTime> getEffectiveDate;
		readonly Func<ZString, ZBool> hasApplicableEntryType;
		readonly Func<ZString> getCountryOfOrigin;

		public bool RequireDOT
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.DOT) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireDOT); }
		}

		public bool MayRequireDOT
		{
			get { return PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.MayRequireDOT); }
		}

		public bool RequireACE_LaceyData
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.Lacey) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireLaceyAct); }
		}

		public bool RequireOMC
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.OMC) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireOMC); }
		}

		public bool RequireFDA
		{
			get { return PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireFDA); }
		}

		public bool RequireACEFDA
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.FDA) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireACEFDA); }
		}

		public bool RequireNHTSA
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.NHTSA) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireNHTSA); }
		}

		public bool RequireODS
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.ODS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireODS); }
		}

		public bool RequireTSCA
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.TSCA) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireTSCA); }
		}

		public bool RequirePST
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.PST) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequirePST); }
		}

		public bool RequireHFC
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.HFC) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireHFC); }
		}

		public bool RequireVNE
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.VNE) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireVNE); }
		}

		public bool RequireTTB
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.TTB) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireTTB); }
		}

		public bool RequireAMS
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.AMS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => (x.DoesRequireAMSEG || x.DoesRequireAMSMO || x.DoesRequireAMSPeanuts)); }
		}

		public bool RequireAMSEGG
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.AMS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => (x.DoesRequireAMSEG)); }
		}

		public bool RequireAMSORD
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.AMS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => (x.DoesRequireAMSMO)); }
		}

		public bool RequireAMSPNT
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.AMS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => (x.DoesRequireAMSPeanuts)); }
		}

		public bool RequireNOP
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.NOP) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => (x.DoesRequireAMSOrganics)); }
		}

		public bool HasTTBRequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.TTB) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasTTBRequirement); }
		}

		public bool HasAPHISRequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.APHIS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasAPHISRequirement); }
		}

		public bool RequireAPHIS
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.APHIS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireAPHIS); }
		}

		public bool MayRequireAPHISNoDisclaimRequired
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.APHIS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.MayRequireAPHISNoDisclaimRequired); }
		}

		public bool RequireCPSC
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.CPSC) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireCPSC); }
		}

		public bool HasFWSRequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.FWS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasFWSRequirement); }
		}

		public bool RequireFWS
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.FWS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireFWS); }
		}

		public bool RequireNMFS370
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes._370) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireNMFS370); }
		}

		public bool RequireNMFSAMR
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.AMR) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireNMFSAMR); }
		}

		public bool RequireNMFSHMS
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.HMS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireNMFSHMS); }
		}

		public bool RequireNMFSSIM
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.SIMP) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireNMFSSIM); }
		}
		public bool RequireFSIS
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.FSIS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireFSIS); }
		}

		public bool HasFDARequirement
		{
			get
			{
				return PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasFDARequirement);
			}
		}

		public bool HasACEFDARequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.FDA) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasACEFDARequirement); }
		}

		public bool HasFSISRequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.FSIS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasFSISRequirement); }
		}

		public bool HasVNERequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.VNE) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasVNERequirement); }
		}

		public bool HasNHTSARequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.NHTSA) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasNHTSARequirement); }
		}

		public bool HasPSTRequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.PST) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasPSTRequirement); }
		}

		public bool HasHFCRequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.HFC) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasHFCRequirement); }
		}

		public bool MayRequireACSLacey
		{
			get
			{
				return PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.Applies(TariffRuleList.Codes.LaceyAct, getEffectiveDate()));
			}
		}

		public bool HasLaceyActRequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.Lacey) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasLaceyActRequirement); }
		}

		public bool HasOMCRequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.OMC) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasOMCRequirement); }
		}

		public bool HasODSRequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.ODS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasODSRequirement); }
		}

		public bool HasTSCARequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.TSCA) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasTSCARequirement); }
		}

		public bool HasCPSCRequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.CPSC) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasCPSCRequirement); }
		}

		public bool HasAMSRequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.AMS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasAMSRequirement); }
		}

		public bool HasAMSEGGRequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.AMS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasAMSEGGRequirement); }
		}

		public bool HasAMSORDRequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.AMS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasAMSORDRequirement); }
		}

		public bool HasAMSPNTRequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.AMS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasAMSPNTRequirement); }
		}

		public bool HasNOPRequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.NOP) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasNOPRequirement); }
		}

		public bool HasNMFS370Requirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes._370) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasNMFS370Requirement); }
		}

		public bool HasNMFSAMRRequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.AMR) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasNMFSAMRRequirement); }
		}

		public bool HasNMFSHMSRequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.HMS) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasNMFSHMSRequirement); }
		}

		public bool HasNMFSSIMRequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.SIMP) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasNMFSSIMRequirement); }
		}

		public bool HasDEARequirement
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.DEA) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.HasDEARequirement); }
		}

		public bool RequireNMFSCOA
		{
			get { return hasApplicableEntryType(GovernmentAgencyProgramCodeList.Codes.COA) && PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { getTariff(), getSupTariff() }, x => x.DoesRequireNMFSCOA(getCountryOfOrigin(), getEffectiveDate())); }
		}
	}
}
