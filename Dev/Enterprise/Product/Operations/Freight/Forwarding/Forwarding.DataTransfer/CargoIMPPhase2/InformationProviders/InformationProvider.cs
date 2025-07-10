using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class InformationProvider
	{
		public InformationProvider(BusinessObjectFactory factory)
		{
			Factory = factory;
		}
		BusinessObjectFactory Factory { get; set; }

		protected ZString GetPartyId(OrgAddress address)
		{
			ZString result = ZString.Empty;
			if (address != null)
			{
				result = GetPartyId(address.Header);
			}

			return result;
		}

		protected ZString GetPartyId(OrgHeader orgHeader)
		{
			ZString result = ZString.Empty;
			if (IsOrganisationValid(orgHeader))
			{
				const string validCharacters = @"A-Z0-9\-./ "; // RegEx constant
				result = Regex.Replace(orgHeader.OH_Code.ToUpper(), "[^" + validCharacters + "]", "");
			}

			return result;
		}

		protected ZString GetPartyId(JobDocAddress address)
		{
			ZString result = ZString.Empty;
			if (address != null && address.HasRealOrganisation)
			{
				result = GetPartyId(address.Organisation);
			}

			return result;
		}

		protected ZString GetLocationCode(JobDocAddress jobDocAddress)
		{
			ZString result = ZString.Empty;
			if (jobDocAddress != null && jobDocAddress.HasRealAddress)
			{
				result = GetLocationCode(jobDocAddress.Address);
			}

			return result;
		}

		protected ZString GetLocationCode(OrgAddress address)
		{
			ZString result = ZString.Empty;
			if (address != null && IsOrganisationValid(address.Header))
			{
				if (!address.OA_RL_NKRelatedPortCode.IsEmpty)
				{
					result = GetLocationCode(address.OA_RL_NKRelatedPortCode);
				}
				else
				{
					result = GetLocationCode(address.Header.OH_RL_NKClosestPort);
				}
			}

			return result;
		}

		protected ZString GetLocationCode(ZString code)
		{
			ZString result = ZString.Empty;
			if (!code.IsEmpty)
			{
				ZQuery query = new ZQuery(RefUNLOCOSchema.RL_Code, code);
				RefUNLOCO[] unlocos = Factory.Load<RefUNLOCO>(query);
				if (unlocos != null && unlocos.Length == 1)
				{
					RefUNLOCO unloco = unlocos[0];
					if (!unloco.RL_IATA.IsEmpty)
					{
						result = unloco.RL_IATA;
					}
					else
					{
						result = code;
					}
				}
			}

			return result;
		}

		protected bool IsOrganisationValid(OrgHeader org)
		{
			return org != null && !org.IsMiscellaneous && org.PK != OrgHeader.UnmatchedOrganisationPK;
		}

		protected bool IsOrganisationValid(OrgAddress address)
		{
			return address != null && IsOrganisationValid(address.Header);
		}

		protected bool IsOrganisationValid(JobDocAddress address)
		{
			return address.HasRealOrganisation && IsOrganisationValid(address.Organisation);
		}
	}
}
