using System;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class ExportPGAAgencyRequirementCollection : OGAAgencyRequirementCollection
	{
		public ExportPGAAgencyRequirementCollection(PGAAgencyRequirementsProvider provider)
			: base(provider)
		{
		}

		protected override void PopulateAgencyCodeWithDescription(OGAAgencyRequirement requirement, CodeDescriptionPair program)
		{
			switch (program.Code)
			{
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					requirement.AgencyCodeWithDescription = "AMS - Agricultural Marketing Service";
					break;
				case GovernmentAgencyProgramCodeList.Codes.ATF:
					requirement.AgencyCodeWithDescription = "ATF - Bureau of Alcohol, Tobacco, Firearms and Explosives";
					break;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					requirement.AgencyCodeWithDescription = "FWS - Fish & Wildlife Service";
					break;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					requirement.AgencyCodeWithDescription = "DEA - Drug Enforcement Administration";
					break;
				case GovernmentAgencyProgramCodeList.Codes.EPA:
					requirement.AgencyCodeWithDescription = "EPA - Environmental Protection Agency";
					break;
				case GovernmentAgencyProgramCodeList.NMFS:
					requirement.AgencyCodeWithDescription = "NMFS - National Oceanic and Atmospheric Administration, National Marine Fisheries";
					break;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					requirement.AgencyCodeWithDescription = "TTB - Alcohol and Tobacco Tax and Trade Bureau";
					break;
				default:
					throw new InvalidOperationException(ZString.Format("Unknown agency program code '{0}'.", program.Code));
			}
		}
	}
}
