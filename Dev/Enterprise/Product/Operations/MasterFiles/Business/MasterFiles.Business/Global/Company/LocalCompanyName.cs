using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class LocalCompanyName
	{
		public static ZString GetCurrentCompanyLocalName()
		{
			return GlbCompany.CurrentCompany.OrgProxy == null ? ZString.Empty :
			  GetLocalCompanyName(GlbCompany.CurrentCompany.OrgProxy, OrgConstants.AddressType.Receivables);
		}

		public static ZString GetLocalCompanyName(OrgHeader orgHeader, string addressType)
		{
			OrgAddressList orgAddress = orgHeader.Addresses.AddressesOfType(addressType);

			string companyName = orgHeader.OH_FullNameTruncated;
			foreach (OrgAddress currentAddress in orgAddress)
			{
				if (currentAddress.AddressCapability.GetIsMainAddress(addressType) && !currentAddress.OA_CompanyNameOverride.IsEmpty)
				{
					companyName = currentAddress.OA_CompanyNameOverrideTruncated;
					break;
				}
			}
			return companyName;
		}
	}
}
