using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public static class OrgHeaderExtensionMethods
	{
		public static OrgAddress GetCustomsAddressDetailsFallingBackToMainAddress(this OrgHeader organisation)
		{
			OrgAddress result = null;

			if (organisation != null)
			{
				result = GetCustomsAddressOfRecord(organisation);

				if (result == null)
				{
					result = organisation.MainAddress;
				}
			}

			return result;
		}

		public static OrgAddress GetCustomsAddressOfRecord(this OrgHeader organisation)
		{
			OrgAddress result = null;

			if (organisation != null)
			{
				var customsAddresses = organisation.Addresses.AddressesOfType(OrgConstants.AddressType.CustomsAddressOfRecord);
				if (customsAddresses.Count > 0)
				{
					result = customsAddresses[0];
				}
			}

			return result;
		}

		public static bool IsUSOrganisation(this OrgHeader organisation)
		{
			return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(organisation.OH_RL_NKClosestPort.Left(2)) == Core.Constants.CountryCodes.UnitedStates;
		}

		public static bool IsCustomsDisbursementCreditor(this OrgHeader organisation)
		{
			var result = false;
			if (organisation != null)
			{
				var factory = organisation.Factory;
				var companies = factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Common.US.USCustomsJurisdiction.Countries));
				foreach (var company in companies)
				{
					var orgPKInRegistry = RatingDataRegistry.Instance.CustomsDisbursementCreditor.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
					if (organisation.PK == orgPKInRegistry)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}
	}
}
