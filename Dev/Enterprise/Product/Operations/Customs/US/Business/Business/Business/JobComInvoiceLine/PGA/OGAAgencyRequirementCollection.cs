using System;
using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class OGAAgencyRequirementCollection : NonPersistentBusinessObjectCollection<OGAAgencyRequirement>
	{
		public OGAAgencyRequirementCollection(PGAAgencyRequirementsProvider provider)
			: base(provider.Factory)
		{
			this.fpgaProvider = provider;
		}

		readonly PGAAgencyRequirementsProvider fpgaProvider;

		internal void Populate()
		{
			this.RemoveAll();

			if (fpgaProvider.IsPGAReqirementRelevant)
			{
				var programs = fpgaProvider.GetGovernmentAgencyProgramCodeList();

				foreach (CodeDescriptionPair program in programs)
				{
					var code = program.Code;
					if (fpgaProvider.DoesMatchCertificationMode(code))
					{
						var requirement = new OGAAgencyRequirement(fpgaProvider.Factory,
							() => fpgaProvider.GetRequirementDescription(code),
							fpgaProvider.GetIndicatorInfo(code),
							() => fpgaProvider.ValidateIndicator(code),
							fpgaProvider.GetDisclaimReasonInfo(code),
							() => fpgaProvider.ValidateDisclaimReason(code),
							() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);

						PopulateAgencyCodeWithDescription(requirement, program);
						requirement.AgencyCode = program.Code;
						requirement.CanDisclaim = fpgaProvider.CanDisclaim(code);

						this.Add(requirement);
					}
				}
			}
		}

		protected virtual void PopulateAgencyCodeWithDescription(OGAAgencyRequirement requirement, CodeDescriptionPair program)
		{
			switch (program.Code)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					requirement.AgencyCodeWithDescription = program.CodeAndDescription;
					break;
				case GovernmentAgencyProgramCodeList.Codes._370:
				case GovernmentAgencyProgramCodeList.Codes.AMR:
				case GovernmentAgencyProgramCodeList.Codes.HMS:
				case GovernmentAgencyProgramCodeList.Codes.SIMP:
				case GovernmentAgencyProgramCodeList.Codes.COA:
					requirement.AgencyCodeWithDescription = "NMFS - " + program.CodeAndDescription;
					break;
				case GovernmentAgencyProgramCodeList.Codes.DDTC:
					requirement.AgencyCodeWithDescription = program.CodeAndDescription;
					break;
				case GovernmentAgencyProgramCodeList.Codes.DOT:
				case GovernmentAgencyProgramCodeList.Codes.FCC:
					requirement.AgencyCodeWithDescription = program.Code + " - OGA - " + program.Description;
					break;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
				case GovernmentAgencyProgramCodeList.Codes.PST:
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
				case GovernmentAgencyProgramCodeList.Codes.VNE:
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					requirement.AgencyCodeWithDescription = "EPA - " + program.CodeAndDescription;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					requirement.AgencyCodeWithDescription = "USDA - " + program.CodeAndDescription;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					requirement.AgencyCodeWithDescription = "USDA - AMS - " + program.CodeAndDescription;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					requirement.AgencyCodeWithDescription = "DOT - " + program.CodeAndDescription;
					break;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					{
						if (fpgaProvider.IsPGA(program.Code))
						{
							requirement.AgencyCodeWithDescription = program.Code + " - PGA - " + program.Description;
						}
						else
						{
							requirement.AgencyCodeWithDescription = program.Code + " - OGA - " + program.Description;
						}
						break;
					}
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					requirement.AgencyCodeWithDescription = GovernmentAgencyProgramCodeList.Codes.OMC + " - " + GovernmentAgencyProgramCodeList.Descriptions.OMC;
					break;

				case GovernmentAgencyProgramCodeList.Codes.TTB:
				case GovernmentAgencyProgramCodeList.Codes.ATF:
					requirement.AgencyCodeWithDescription = program.Code + " - PGA - " + program.Description;
					break;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					requirement.AgencyCodeWithDescription = GovernmentAgencyProgramCodeList.Codes.CPSC + " - " + GovernmentAgencyProgramCodeList.Descriptions.CPSC;
					break;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					requirement.AgencyCodeWithDescription = GovernmentAgencyProgramCodeList.Codes.DEA + " - " + GovernmentAgencyProgramCodeList.Descriptions.DEA;
					break;

				default:
					throw new InvalidOperationException(string.Format("Unknown agency program code '{0}'.", program.Code));
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException();
		}
	}
}
