using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CusUSClassificationValidation : AutoCusUSClassificationValidation
	{
		public static class AgencyName
		{
			public const string LaceyAct = "Lacey Act";
		}

		public CusUSClassificationValidation(AutoCusUSClassification parent)
			: base(parent)
		{
		}

		public CusClassPartPivot ParentPivot
		{
			get { return Parent.Parent; }
		}

		public new CusUSClassification Parent
		{
			get { return (CusUSClassification)base.Parent; }
		}

		protected override void CheckCD_OMCIndicator()
		{
			base.CheckCD_OMCIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_OMCIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);
		}

		// TODO: Move all HTI check to HTICusUSClassificationValidation
		protected override void CheckCD_AMSIndicator()
		{
			base.CheckCD_AMSIndicator();

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_AMSIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);

				var hasEGG = ParentPivot.PGARequirementIndicator.HasAMSEGGRequirement;
				var hasORD = ParentPivot.PGARequirementIndicator.HasAMSORDRequirement;
				var hasPNT = ParentPivot.PGARequirementIndicator.HasAMSPNTRequirement;

				if (hasEGG)
				{
					AgencyRequirementsValidator.ValidatePGA(Parent.CD_AMSIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.AMS, Parent.Parent.IsAMSEGGEffective, hasEGG);
				}
				else if (hasORD)
				{
					AgencyRequirementsValidator.ValidatePGA(Parent.CD_AMSIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.AMS, true, hasORD);
				}
				else if (hasPNT)
				{
					AgencyRequirementsValidator.ValidatePGA(Parent.CD_AMSIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.AMS, Parent.Parent.IsAMSPNTEffective, hasPNT);
				}
				else
				{
					AgencyRequirementsValidator.ValidatePGA(Parent.CD_AMSIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.AMS, false, false);
				}

				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_AMSIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.AMS, ParentPivot.AMSLines.Cast<AMS>().Where(x => AMSProgramList.IsAMSProgramButNotNOP(ParentPivot.Factory, x.US_Program)).Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_AMSIndicatorInfo, ParentPivot.PGARequirementIndicator.RequireAMS);

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.AMS, ParentPivot.OGAAgencyRequirements);
				ValidateCD_AMSDisclaimReason();
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_AMSIndicatorInfo, Parent.Lookups.US_OGAIndicatorWithoutDisclaimerList);
				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.AMS, ParentPivot.ExportPGAAgencyRequirements);
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_AMSIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.AMS, true, ParentPivot.ExportPGARequirementIndicator.RequireAMS);
			}
		}

		protected override void CheckCD_AMSDisclaimReason()
		{
			base.CheckCD_AMSDisclaimReason();
			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_AMSDisclaimReasonInfo, Parent.CD_AMSIndicator, ParentPivot.Lookups.AMSDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.AMS, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_OMCDisclaimReason()
		{
			base.CheckCD_OMCDisclaimReason();
			if (ZZCustomsFunctionality.IsOMCEffective && ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_OMCDisclaimReasonInfo, Parent.CD_OMCIndicator, ParentPivot.Lookups.OMCDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.OMC, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_NOPIndicator()
		{
			base.CheckCD_NOPIndicator();
			var parentPivot = ParentPivot;
			if (parentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_NOPIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_NOPIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.NOP, false, ParentPivot.PGARequirementIndicator.HasNOPRequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_NOPIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.NOP, parentPivot.AMSLines.Cast<AMS>().Where(x => x.IsNOPProgram).Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_NOPIndicatorInfo, parentPivot.PGARequirementIndicator.RequireNOP);

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.NOP, parentPivot.OGAAgencyRequirements);
				ValidateCD_AMSDisclaimReason();
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_NOPIndicatorInfo, Parent.Lookups.US_OGAIndicatorWithoutDisclaimerList);
				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.NOP, parentPivot.ExportPGAAgencyRequirements);
			}
		}

		protected override void CheckCD_NOPDisclaimReason()
		{
			base.CheckCD_NOPDisclaimReason();
			var parentPivot = ParentPivot;
			if (parentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_NOPDisclaimReasonInfo, Parent.CD_NOPIndicator, parentPivot.Lookups.NOPDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.NOP, parentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_NMFS370DisclaimReason()
		{
			base.CheckCD_NMFS370DisclaimReason();
			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_NMFS370DisclaimReasonInfo, Parent.CD_NMFS370Indicator, ParentPivot.Lookups.NMFS370DisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes._370, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_NMFS370Indicator()
		{
			base.CheckCD_NMFS370Indicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_NMFS370IndicatorInfo, Parent.Lookups.US_OGAIndicatorList);
			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_NMFS370IndicatorInfo, GovernmentAgencyProgramCodeList.Codes._370, true, ParentPivot.PGARequirementIndicator.HasNMFS370Requirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_NMFS370IndicatorInfo, GovernmentAgencyProgramCodeList.Codes._370, ParentPivot.NMFS370Lines.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_NMFS370IndicatorInfo, ParentPivot.PGARequirementIndicator.RequireNMFS370);

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes._370, ParentPivot.OGAAgencyRequirements);
				ValidateCD_NMFS370DisclaimReason();
			}
		}

		protected override void CheckCD_NMFSCOAIndicator()
		{
			base.CheckCD_NMFSCOAIndicator();
			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_NMFSCOAIndicatorInfo, Parent.Lookups.US_OGAIndicatorWithoutDisclaimerList);
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_NMFSCOAIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.COA, ZZCustomsFunctionality.IsNMFSCOAACTIVEEffective, ParentPivot.PGARequirementIndicator.RequireNMFSCOA);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_NMFSCOAIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.COA, ParentPivot.NMFSCOALines.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.COA, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_NMFSAMRDisclaimReason()
		{
			base.CheckCD_NMFSAMRDisclaimReason();
			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_NMFSAMRDisclaimReasonInfo, Parent.CD_NMFSAMRIndicator, ParentPivot.Lookups.NMFSAMRDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.AMR, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_NMFSAMRIndicator()
		{
			base.CheckCD_NMFSAMRIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_NMFSAMRIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_NMFSAMRIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.AMR, true, ParentPivot.PGARequirementIndicator.HasNMFSAMRRequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_NMFSAMRIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.AMR, ParentPivot.NMFSAMRLines.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_NMFSAMRIndicatorInfo, ParentPivot.PGARequirementIndicator.RequireNMFSAMR);

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.AMR, ParentPivot.OGAAgencyRequirements);
				ValidateCD_NMFSAMRDisclaimReason();
			}
		}

		protected override void CheckCD_NMFSHMSDisclaimReason()
		{
			base.CheckCD_NMFSHMSDisclaimReason();
			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_NMFSHMSDisclaimReasonInfo, Parent.CD_NMFSHMSIndicator, ParentPivot.Lookups.NMFSHMSDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.HMS, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_NMFSHMSIndicator()
		{
			base.CheckCD_NMFSHMSIndicator();

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_NMFSHMSIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_NMFSHMSIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.HMS, true, ParentPivot.PGARequirementIndicator.HasNMFSHMSRequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_NMFSHMSIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.HMS, ParentPivot.NMFSHMSLines.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_NMFSHMSIndicatorInfo, ParentPivot.PGARequirementIndicator.RequireNMFSHMS);

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.HMS, ParentPivot.OGAAgencyRequirements);
				ValidateCD_NMFSHMSDisclaimReason();
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_NMFSHMSIndicatorInfo, Parent.Lookups.US_OGAIndicatorWithoutDisclaimerList);

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.NMFS, ParentPivot.ExportPGAAgencyRequirements);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndDataForExport(Parent.CD_NMFSHMSIndicatorInfo, GovernmentAgencyProgramCodeList.NMFS, ParentPivot.NMFSLines.Count);
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_NMFSHMSIndicatorInfo, GovernmentAgencyProgramCodeList.NMFS, true, ParentPivot.ExportPGARequirementIndicator.RequireNMFS);
			}
		}

		protected override void CheckCD_NMFSSIMPIndicator()
		{
			base.CheckCD_NMFSSIMPIndicator();

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_NMFSSIMPIndicatorInfo, Parent.Lookups.US_OGAIndicatorWithoutDisclaimerList);
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_NMFSSIMPIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.SIMP, true, ParentPivot.PGARequirementIndicator.HasNMFSSIMRequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_NMFSSIMPIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.SIMP, ParentPivot.NMFSSIMPLines.Cast<IPGADataCorrection>());

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.SIMP, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_FWSIndicator()
		{
			base.CheckCD_FWSIndicator();

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_FWSIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_FWSIndicatorInfo, "FWS", true, ParentPivot.PGARequirementIndicator.HasFWSRequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_FWSIndicatorInfo, "FWS", ParentPivot.FWSLines.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_FWSIndicatorInfo, ParentPivot.PGARequirementIndicator.RequireFWS);

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.FWS, ParentPivot.OGAAgencyRequirements);
				ValidateCD_FWSDisclaimReason();
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_FWSIndicatorInfo, Parent.Lookups.US_OGAIndicatorWithoutDisclaimerList);
				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.FWS, ParentPivot.ExportPGAAgencyRequirements);
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_FWSIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.FWS, true, ParentPivot.ExportPGARequirementIndicator.RequireFWS);
			}
		}

		protected override void CheckCD_FWSDisclaimReason()
		{
			base.CheckCD_FWSDisclaimReason();
			if (ZZCustomsFunctionality.IsFWSEffective && ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_FWSDisclaimReasonInfo, Parent.CD_FWSIndicator, ParentPivot.Lookups.FWSDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.FWS, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_ACEFDAIndicator()
		{
			base.CheckCD_ACEFDAIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_ACEFDAIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_ACEFDAIndicatorInfo, "PGA FDA", true, ParentPivot.PGARequirementIndicator.HasACEFDARequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_ACEFDAIndicatorInfo, "PGA FDA", ParentPivot.ACEFDAs.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_ACEFDAIndicatorInfo, ParentPivot.PGARequirementIndicator.RequireACEFDA);
			}
		}

		protected override void CheckCD_ACEFDADisclaimReason()
		{
			base.CheckCD_ACEFDADisclaimReason();

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_ACEFDADisclaimReasonInfo, Parent.CD_ACEFDAIndicator, ParentPivot.Lookups.FDADisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.FDA, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_LaceyActIndicator()
		{
			base.CheckCD_LaceyActIndicator();

			ListValidation.MessageErrorIfInvalidCode(Parent.CD_LaceyActIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_LaceyActIndicatorInfo, "Lacey Act", true, ParentPivot.PGARequirementIndicator.HasLaceyActRequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_LaceyActIndicatorInfo, "Lacey Act", ParentPivot.PGAs.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_LaceyActIndicatorInfo, ParentPivot.PGARequirementIndicator.RequireACE_LaceyData);

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.Lacey, ParentPivot.OGAAgencyRequirements);
				ValidateCD_LaceyActDisclaimReason();
			}
		}

		protected override void CheckCD_LaceyActDisclaimReason()
		{
			base.CheckCD_LaceyActDisclaimReason();

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_LaceyActDisclaimReasonInfo, Parent.CD_LaceyActIndicator, ParentPivot.Lookups.LaceyDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.Lacey, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_CPSCIndicator()
		{
			base.CheckCD_CPSCIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_CPSCIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_CPSCIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.CPSC, ZZCustomsFunctionality.IsCPSCEffective, ParentPivot.PGARequirementIndicator.HasCPSCRequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_CPSCIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.CPSC, ParentPivot.CPSCLines.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_CPSCIndicatorInfo, ParentPivot.PGARequirementIndicator.RequireCPSC);

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.CPSC, ParentPivot.OGAAgencyRequirements);
				ValidateCD_CPSCDisclaimReason();
			}
		}

		protected override void CheckCD_CPSCDisclaimReason()
		{
			base.CheckCD_CPSCDisclaimReason();
			if (ZZCustomsFunctionality.IsCPSCEffective && ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_CPSCDisclaimReasonInfo, Parent.CD_CPSCIndicator, ParentPivot.Lookups.CPSCDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.CPSC, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_NHTSAIndicator()
		{
			base.CheckCD_NHTSAIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_NHTSAIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_NHTSAIndicatorInfo, "NHTSA", true, ParentPivot.PGARequirementIndicator.HasNHTSARequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_NHTSAIndicatorInfo, "NHTSA", ParentPivot.NHTSALines.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_NHTSAIndicatorInfo, ParentPivot.PGARequirementIndicator.RequireNHTSA);

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.NHTSA, ParentPivot.OGAAgencyRequirements);
				ValidateCD_NHTSADisclaimReason();
			}
		}

		protected override void CheckCD_NHTSADisclaimReason()
		{
			base.CheckCD_NHTSADisclaimReason();
			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_NHTSADisclaimReasonInfo, Parent.CD_NHTSAIndicator, ParentPivot.Lookups.NHTSADisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.NHTSA, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_ODSIndicator()
		{
			base.CheckCD_ODSIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_ODSIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_ODSIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.ODS, true, ParentPivot.PGARequirementIndicator.HasODSRequirement);
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_ODSIndicatorInfo, ParentPivot.PGARequirementIndicator.RequireODS);

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.ODS, ParentPivot.OGAAgencyRequirements);
				ValidateCD_ODSDisclaimReason();
			}
		}

		protected override void CheckCD_ODSDisclaimReason()
		{
			base.CheckCD_ODSDisclaimReason();

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_ODSDisclaimReasonInfo, Parent.CD_ODSIndicator, ParentPivot.Lookups.ODSDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.ODS, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_TSCAClaimIndicator()
		{
			base.CheckCD_TSCAClaimIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_TSCAClaimIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_TSCAClaimIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.TSCA, true, ParentPivot.PGARequirementIndicator.HasTSCARequirement);
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_TSCAClaimIndicatorInfo, ParentPivot.PGARequirementIndicator.RequireTSCA);

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.TSCA, ParentPivot.OGAAgencyRequirements);
				ValidateCD_TSCADisclaimReason();
			}
		}

		protected override void CheckCD_TSCADisclaimReason()
		{
			base.CheckCD_TSCADisclaimReason();

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_TSCADisclaimReasonInfo, Parent.CD_TSCAClaimIndicator, ParentPivot.Lookups.TSCADisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.TSCA, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_PSTIndicator()
		{
			base.CheckCD_PSTIndicator();

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_PSTIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_PSTIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.PST, true, ParentPivot.PGARequirementIndicator.HasPSTRequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_PSTIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.PST, ParentPivot.PSTLines.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_PSTIndicatorInfo, ParentPivot.PGARequirementIndicator.RequirePST);

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.PST, ParentPivot.OGAAgencyRequirements);
				ValidateCD_PSTDisclaimReason();
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_PSTIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);
				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.EPA, ParentPivot.ExportPGAAgencyRequirements);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndDataForExport(Parent.CD_PSTIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.EPA, ParentPivot.PSTLines.Count, ParentPivot.ExportPGARequirementIndicator.RequireEPA);
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_PSTIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.EPA, true, ParentPivot.ExportPGARequirementIndicator.HasEPARequirement);
			}
		}

		protected override void CheckCD_PSTDisclaimReason()
		{
			base.CheckCD_PSTDisclaimReason();

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_PSTDisclaimReasonInfo, Parent.CD_PSTIndicator, ParentPivot.Lookups.PSTDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.PST, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_HFCIndicator()
		{
			base.CheckCD_HFCIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_HFCIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);
		}

		protected override void CheckCD_HFCDisclaimReason()
		{
			base.CheckCD_HFCDisclaimReason();

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_HFCDisclaimReasonInfo, Parent.CD_HFCIndicator, ParentPivot.Lookups.HFCDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.HFC, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_VNEIndicator()
		{
			base.CheckCD_VNEIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_VNEIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_VNEIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.VNE, true, ParentPivot.PGARequirementIndicator.HasVNERequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_VNEIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.VNE, ParentPivot.VehicleLines.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_VNEIndicatorInfo, ParentPivot.PGARequirementIndicator.RequireVNE);

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.VNE, ParentPivot.OGAAgencyRequirements);
				ValidateCD_VNEDisclaimReason();
			}
		}

		protected override void CheckCD_VNEDisclaimReason()
		{
			base.CheckCD_VNEDisclaimReason();

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_VNEDisclaimReasonInfo, Parent.CD_VNEIndicator, ParentPivot.Lookups.VNEDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.VNE, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_ATFIndicator()
		{
			base.CheckCD_ATFIndicator();

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_ATFIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_ATFIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.ATF, ParentPivot.ATFLines.Cast<IPGADataCorrection>());
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_ATFIndicatorInfo, Parent.Lookups.US_OGAIndicatorWithoutDisclaimerList);
				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.ATF, ParentPivot.ExportPGAAgencyRequirements);
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_ATFIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.ATF, true, ParentPivot.ExportPGARequirementIndicator.RequireATF);
			}
		}

		protected override void CheckCD_TTBIndicator()
		{
			base.CheckCD_TTBIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_TTBIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_TTBIndicatorInfo, "TTB", true, ParentPivot.PGARequirementIndicator.HasTTBRequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_TTBIndicatorInfo, "TTB", ParentPivot.TTBLines.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_TTBIndicatorInfo, ParentPivot.PGARequirementIndicator.RequireTTB);

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.TTB, ParentPivot.OGAAgencyRequirements);
				ValidateCD_TTBDisclaimReason();
			}
			else
			{
				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.TTB, ParentPivot.ExportPGAAgencyRequirements);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndDataForExport(Parent.CD_TTBIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.TTB, ParentPivot.TTBLines.Count, ParentPivot.ExportPGARequirementIndicator.RequireTTB);
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_TTBIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.TTB, true, ParentPivot.ExportPGARequirementIndicator.RequireTTB);
			}
		}

		protected override void CheckCD_TTBDisclaimReason()
		{
			base.CheckCD_TTBDisclaimReason();

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_TTBDisclaimReasonInfo, Parent.CD_TTBIndicator, ParentPivot.Lookups.TTBDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.TTB, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_ADDCaseNo()
		{
			base.CheckCD_ADDCaseNo();
			if (Parent.Parent != null)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_ADDCaseNoInfo, Parent.Parent.Lookups.ADDCaseNumberList);
			}

			ValidateCD_ADCVDStat();
		}

		protected override void CheckCD_ADDDepositRateInd()
		{
			base.CheckCD_ADDDepositRateInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_ADDDepositRateIndInfo, Parent.Lookups.AntidumpingDutyDepositRates);
		}

		protected override void CheckCD_CVDCaseNo()
		{
			base.CheckCD_CVDCaseNo();
			if (Parent.Parent != null)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_CVDCaseNoInfo, Parent.Parent.Lookups.CVDCaseNumberList);
			}

			ValidateCD_ADCVDStat();
		}

		protected override void CheckCD_CVDDepositRateInd()
		{
			base.CheckCD_CVDDepositRateInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_CVDDepositRateIndInfo, Parent.Lookups.CountervailingDutyDepositRates);
		}

		protected override void CheckCD_ReconIssue()
		{
			base.CheckCD_ReconIssue();
			if (Parent.Parent != null)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_ReconIssueInfo, Parent.Parent.USClassificationLookups.OtherReconIssueList);
			}
		}

		protected override void CheckCD_UC_NKCountryOfOrigin()
		{
			base.CheckCD_UC_NKCountryOfOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_UC_NKCountryOfOriginInfo, Parent.Lookups.USCountryList);
			CheckRestrictedCountry(Parent.CD_UC_NKCountryOfOriginInfo);
		}

		protected override void CheckCD_UC_NKCountryOfExport()
		{
			base.CheckCD_UC_NKCountryOfExport();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_UC_NKCountryOfExportInfo, Parent.Lookups.USCountryList);
			CheckRestrictedCountry(Parent.CD_UC_NKCountryOfExportInfo);
		}

		void CheckRestrictedCountry(ZPropertyInfo info)
		{
			var country = Parent.Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, info.Value.ToString());
			if (country != null && country.IsRestrictedCountry())
			{
				info.AddMessageError(country.UC_Name + " is a restricted country");
			}
		}

		protected override void CheckCD_RulingType()
		{
			base.CheckCD_RulingType();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_RulingTypeInfo, Parent.Lookups.CD_RulingTypeList);
		}

		protected override void CheckCD_ZoneStatus()
		{
			base.CheckCD_ZoneStatus();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_ZoneStatusInfo, Parent.Lookups.CD_ZoneStatusList);
		}

		protected override void CheckCD_SPI()
		{
			base.CheckCD_SPI();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_SPIInfo, Parent.Lookups.SPIList);
		}

		protected override void CheckCD_TSCAIndicator()
		{
			base.CheckCD_TSCAIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_TSCAIndicatorInfo, Parent.Lookups.CD_TSCAIndicatorList);
		}

		protected override void CheckCD_9802ValuePerUnit()
		{
			base.CheckCD_9802ValuePerUnit();
			ValidateCD_RX_NK9802ValuePerUnitCurr();
		}

		protected override void CheckCD_RX_NK9802ValuePerUnitCurr()
		{
			base.CheckCD_RX_NK9802ValuePerUnitCurr();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_RX_NK9802ValuePerUnitCurrInfo);
			if (Parent.CD_9802ValuePerUnit > ZDecimal.Zero && Parent.CD_RX_NK9802ValuePerUnitCurr.IsEmpty)
			{
				Parent.CD_RX_NK9802ValuePerUnitCurrInfo.AddMessageError(CurrencyIsRequired);
			}
		}

		protected override void CheckCD_PerUnitCost()
		{
			base.CheckCD_PerUnitCost();
			ValidateCD_RX_NKPerUnitCostCurr();
		}

		protected override void CheckCD_RX_NKPerUnitCostCurr()
		{
			base.CheckCD_RX_NKPerUnitCostCurr();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_RX_NKPerUnitCostCurrInfo);
			if (Parent.CD_PerUnitCost > ZDecimal.Zero && Parent.CD_RX_NKPerUnitCostCurr.IsEmpty)
			{
				Parent.CD_RX_NKPerUnitCostCurrInfo.AddMessageError(CurrencyIsRequired);
			}
		}
		public const string CurrencyIsRequired = "Please enter a Currency.\r\nThe system will not default the value to the invoice line if the Currency does not match the Invoice Currency.";

		protected override void CheckCD_TaxApplicability()
		{
			base.CheckCD_TaxApplicability();
			if (Parent.Parent != null && Parent.Parent.IsImportClassification)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_TaxApplicabilityInfo, Parent.Parent.USClassificationLookups.TaxApplyList);

				var importTariff = Parent.Parent.ImportTariff;
				if (importTariff != null)
				{
					importTariff.CheckTaxApply(Parent.CD_TaxApplicabilityInfo, Parent.CD_TaxCode);
				}

				ValidateCD_TaxCode();
			}
		}

		protected override void CheckCD_TaxCode()
		{
			base.CheckCD_TaxCode();
			if (Parent.Parent != null && Parent.Parent.IsImportClassification)
			{
				if (TaxApplyList.IsTaxApplicable(Parent.CD_TaxApplicability))
				{
					if (!(Parent.CD_TaxApplicability == TaxApplyList.Codes.Yes && Parent.CD_TaxApplicabilityInfo.HasNotifications()))
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.Parent.CD_TaxCodeInfo, "tax code");
					}
				}

				ListValidation.MessageErrorIfInvalidCode(Parent.CD_TaxCodeInfo, Parent.Parent.USClassificationLookups.TaxCodeList);
				ValidateCD_TaxApplicability();
			}
		}

		protected override void CheckCD_TaxRateDesc()
		{
			base.CheckCD_TaxRateDesc();

			if (Parent.Parent != null && Parent.Parent.IsImportClassification)
			{
				if (TaxApplyList.IsTaxApplicable(Parent.CD_TaxApplicability))
				{
					if (!(Parent.CD_TaxApplicability == TaxApplyList.Codes.Yes && Parent.CD_TaxApplicabilityInfo.HasNotifications()))
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.Parent.CD_TaxRateDescInfo, "tax rate");
					}
				}

				ListValidation.MessageErrorIfInvalidCode(Parent.CD_TaxRateDescInfo, Parent.Parent.USClassificationLookups.TaxRateList);
			}
		}

		protected override void CheckCD_TaxRate()
		{
			base.CheckCD_TaxRate();

			if (ParentPivot.CD_TaxRate <= 0m && (Parent.CD_TaxRateDesc.EqualsIgnoringCase(AppendixBTaxRateList.Codes.Specify) || Parent.CD_TaxRateDesc.EqualsIgnoringCase(AppendixBTaxRateList.CBMAEligible) || ParentPivot.IsCBMAProductClaim))
			{
				Parent.CD_TaxRateInfo.AddMessageError(USAddInfoValidation.TaxRateShouldBeEntered);
			}
		}

		protected override void CheckCD_TTBRateDesignationCode()
		{
			base.CheckCD_TTBRateDesignationCode();

			if (Parent.Parent != null && Parent.Parent.IsImportClassification)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_TTBRateDesignationCodeInfo, Parent.Parent.USClassificationLookups.CBMATaxRateList);
			}
		}

		protected override void CheckCD_CBMADefaultTaxRate()
		{
			base.CheckCD_CBMADefaultTaxRate();
			if (Parent.Parent != null && Parent.Parent.IsImportClassification)
			{
				MandatoryValidation.MessageErrorIfIsNegative(Parent.CD_CBMADefaultTaxRateInfo);
			}
		}

		protected override void CheckCD_CottonFeeExempt()
		{
			base.CheckCD_CottonFeeExempt();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_CottonFeeExemptInfo, Parent.Lookups.US_YesNoList);
		}

		protected override void CheckCD_ProductClaim()
		{
			base.CheckCD_ProductClaim();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_ProductClaimInfo, Parent.Lookups.ProductClaimList, (NoResString)ProductClaimShouldBeInList);
		}

		const string ProductClaimShouldBeInList = "Please enter a valid Product Claim Code. The code you have selected is not in the Product Claim codes List.";

		protected override void CheckCD_ActiveIngredientPercentage()
		{
			base.CheckCD_ActiveIngredientPercentage();
			ClassificationValidator.CheckPercentageActiveIngredient(Parent.CD_ActiveIngredientPercentageInfo);
		}

		protected override void CheckCD_DDTCLicenceType()
		{
			base.CheckCD_DDTCLicenceType();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_DDTCLicenceTypeInfo, Parent.Lookups.DDTCLicenseTypeCodes);
		}

		protected override void CheckCD_ITARExemptionNo()
		{
			base.CheckCD_ITARExemptionNo();

			if (ParentPivot != null && ParentPivot.IsImportClassification)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_ITARExemptionNoInfo, Parent.Lookups.DDTCExemptionCodes);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_ITARExemptionNoInfo, Parent.Lookups.CD_ITARExemptionNoCodes);
				ClassificationValidator.CheckLicNo(Parent.CD_ITARExemptionNoInfo, USAESLicenseCode.IsDDTCITARExemptionRequired(Parent.CD_LicenceType), false,
					ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage,
					ClassificationValidator.DDTCITARExemptionNumberShouldNotBeEntered);
			}
		}

		protected override void CheckCD_DDTCRegoNo()
		{
			base.CheckCD_DDTCRegoNo();

			if (ParentPivot != null && !ParentPivot.IsImportClassification)
			{
				ClassificationValidator.CheckLicNo(Parent.CD_DDTCRegoNoInfo, USAESLicenseCode.IsDDTCRegistrationNumberRequired(Parent.CD_LicenceType), USAESLicenseCode.IsDDTCRegistrationNumberAllowed(Parent.CD_LicenceType),
					ClassificationValidator.DDTCRegistrationNumberIsRequiredMessage,
					ClassificationValidator.DDTCRegistrationNumberShouldNotBeEntered);
			}
		}

		protected override void CheckCD_MilitaryEquipInd()
		{
			base.CheckCD_MilitaryEquipInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_MilitaryEquipIndInfo, Parent.Lookups.US_YesNoList);
			ClassificationValidator.CheckLicNo(Parent.CD_MilitaryEquipIndInfo, USAESLicenseCode.IsDDTCDataRequired(Parent.CD_LicenceType), false,
				ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage,
				ClassificationValidator.DDTCMilitaryEquipmentIndicatorShouldNotBeEntered);
		}

		protected override void CheckCD_PartyCertInd()
		{
			base.CheckCD_PartyCertInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_PartyCertIndInfo, Parent.Lookups.US_YesNoList);
			ClassificationValidator.CheckLicNo(Parent.CD_PartyCertIndInfo, USAESLicenseCode.IsDDTCPartyCertIndicatorRequired(Parent.CD_LicenceType), false,
				ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage,
				ClassificationValidator.DDTCPartyCertificationIndicatorShouldNotBeEntered);
		}

		protected override void CheckCD_DDTCUSMLCategoryCode()
		{
			base.CheckCD_DDTCUSMLCategoryCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_DDTCUSMLCategoryCodeInfo, Parent.Lookups.CD_DDTCUSMLCategoryCodes);
			ClassificationValidator.CheckLicNo(Parent.CD_DDTCUSMLCategoryCodeInfo, USAESLicenseCode.IsDDTCDataRequired(Parent.CD_LicenceType), false,
				ClassificationValidator.DDTCUSMLCategoryCodeIsRequiredMessage,
				ClassificationValidator.DDTCUSMLCategoryCodeShouldNotBeEntered);
		}

		protected override void CheckCD_DDTCJurisdictionNumber()
		{
			base.CheckCD_DDTCJurisdictionNumber();
			if (ZZCustomsFunctionality.IsAESJurisdictionNumberEffective)
			{
				if (Parent.CD_DDTCUSMLCategoryCode == USMLCategoryCodes.Codes.MiscellaneousArticles && !Regex.IsMatch(Parent.CD_DDTCJurisdictionNumber, @"^CJ((\d{7})|(\s\d{4}-\d{2}))$"))
				{
					Parent.CD_DDTCJurisdictionNumberInfo.AddMessageError(JurisdictionNumberInvalid);
				}
			}
		}
		public const string JurisdictionNumberInvalid = "Category XXI Determination Number format is incorrect. Format should be CJ NNNN-NN or CJNNNNNNN, where N is a number.";

		protected override void CheckCD_ADCVDStat()
		{
			base.CheckCD_ADCVDStat();

			if (Parent.CD_ADCVDStat.IsEmpty)
			{
				if (!Parent.CD_ADDCaseNo.IsEmpty)
				{
					Parent.CD_ADCVDStatInfo.AddMessageError(ACEImportAddInfoJobComInvoiceLineValidation.EmptyADDStatement);
				}
			}
			else if (Parent.CD_ADCVDStat != ADDCVDNonReimbursementList.Codes.Declared && Parent.CD_ADCVDStat != ADDCVDNonReimbursementList.Codes.OnceOff)
			{
				Parent.CD_ADCVDStatInfo.AddMessageError(ValidCD_CVDStatement);
			}
			else if (Parent.CD_ADDCaseNo.IsEmpty)
			{
				Parent.CD_ADCVDStatInfo.AddMessageError(ACEImportAddInfoJobComInvoiceLineValidation.ADDStatementMadeOnALineWithoutADDCase);
			}

			ValidateCD_ADDDecID();
		}

		protected override void CheckCD_ADDDecID()
		{
			base.CheckCD_ADDDecID();

			if (Parent.CD_ADCVDStat == ADDCVDNonReimbursementList.Codes.Declared &&
				!Parent.CD_ADDCaseNo.IsEmpty &&
				Parent.CD_ADDDecID.IsEmpty)
			{
				Parent.CD_ADDDecIDInfo.AddMessageError(ACEImportAddInfoJobComInvoiceLineValidation.StatementMadeWithoutDeclarationID);
			}
		}
		internal const string ValidCD_CVDStatement = "An ADD Non-Reimbursement statement is required to be Declared or OnceOff.";

		protected override void CheckCD_DEAIndicator()
		{
			base.CheckCD_DEAIndicator();

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_DEAIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_DEAIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.DEA, true, ParentPivot.PGARequirementIndicator.HasDEARequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_DEAIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.DEA, ParentPivot.DEAHeaders.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_DEAIndicatorInfo, false);

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.DEA, ParentPivot.OGAAgencyRequirements);
				ValidateCD_DEADisclaimReason();
			}
			else
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.CD_DEAIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.DEA, true, ParentPivot.ExportPGARequirementIndicator.MayRequireDEA);
				ListValidation.MessageErrorIfInvalidCode(Parent.CD_DEAIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.DEA, ParentPivot.ExportPGAAgencyRequirements);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndDataForExport(Parent.CD_DEAIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.DEA, ParentPivot.DEAHeaders.Count, ParentPivot.ExportPGARequirementIndicator.RequireDEA);
			}
		}

		protected override void CheckCD_DEADisclaimReason()
		{
			base.CheckCD_DEADisclaimReason();
			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_DEADisclaimReasonInfo, Parent.CD_DEAIndicator, ParentPivot.Lookups.DEADisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.DEA, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_APHISIndicator()
		{
			base.CheckCD_APHISIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_APHISIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);
			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				if (ParentPivot.PGARequirementIndicator.MayRequireAPHISNoDisclaimRequired)
				{
					if (ParentPivot.CD_APHISIndicator.IsEmpty)
					{
						ParentPivot.CD_APHISIndicatorInfo.AddWarning(APHISDataMayBeRequired);
					}
				}
				else
				{
					AgencyRequirementsValidator.ValidatePGA(Parent.CD_APHISIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.APHIS, true, ParentPivot.PGARequirementIndicator.HasAPHISRequirement);
				}
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_APHISIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.APHIS, ParentPivot.APHISHeaders.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_APHISIndicatorInfo, ParentPivot.PGARequirementIndicator.RequireAPHIS);

				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.APHIS, ParentPivot.OGAAgencyRequirements);
				ValidateCD_APHISDisclaimReason();
			}
		}
		internal const string APHISDataMayBeRequired = "APHIS Data May be required, no disclaim is required if APHIS does not apply";

		protected override void CheckCD_APHISDisclaimReason()
		{
			base.CheckCD_APHISDisclaimReason();

			if (ParentPivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.CD_APHISDisclaimReasonInfo, Parent.CD_APHISIndicator, ParentPivot.Lookups.APHISDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.APHIS, ParentPivot.OGAAgencyRequirements);
			}
		}

		protected override void CheckCD_FlavorContentCreditIndicator()
		{
			base.CheckCD_FlavorContentCreditIndicator();

			if (Parent.CD_FlavorContentCreditIndicator && Parent.CD_TaxCode != Core.Constants.USCustoms.FeeCodes.DistilledSpirits)
			{
				Parent.CD_FlavorContentCreditIndicatorInfo.AddWarning(Res.GetString("USSpell|5E04CAE5-24EB-4C52-9107-D6ECBEA1189F", "The Flavor Content Credit Indicator usually only applies to spirits."));
			}
		}
	}
}
