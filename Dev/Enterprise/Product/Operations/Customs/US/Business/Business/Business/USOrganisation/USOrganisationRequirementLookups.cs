using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USOrganisationRequirementLookups
	{
		public USOrganisationRequirementLookups(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public CodeDescriptionPairList GetUSPPIDocAddressRegNumTypes(JobDocAddressLookups lookups)
		{
			var countryCode = lookups.Parent.Country?.Code ?? ZString.Empty;
			return factory.GetCachedValue("USPPIDocAddressRegNumTypes|" + countryCode, () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "Employer Identification Number");
				result.AddPair(OrgCusCode.CodeTypes.PassportID, "Passport ID");
				if (countryCode != Core.Constants.CountryCodes.UnitedStates
				&& countryCode != Core.Constants.CountryCodes.PuertoRico
				&& countryCode != Core.Constants.CountryCodes.VirginIslands)
				{
					result.AddPair(OrgCusCode.USACodeTypes.ForeignRegistrationNumber, "Foreign Registration Number");
					result.AddPair(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUNS Data Universal Numbering System");
				}
				return result;
			});
		}

		public CodeDescriptionPairList GetConsigneeDocAddressRegNumTypes(JobDocAddressLookups lookups)
		{
			return factory.GetCachedValue("ConsigneeDocAddressRegNumTypes", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "Employer Identification Number");
				result.AddPair(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUNS Data Universal Numbering System");
				return result;
			});
		}
	}
}
