using System;
using System.Linq;
using CargoWise.Types;

#region AuthorityToLeaveHelper

namespace Enterprise.MasterFiles.Business
{
	public static class AuthorityToLeaveHelper
	{
		#region GetConsignorAuthorityToLeave

		public static ZBool GetConsignorAuthorityToLeave(JobDocAddress consignorDocAddress, OrgAddress consigneeAddress)
		{
			var result = false;

			if (consignorDocAddress != null)
			{
				if (consignorDocAddress.E2_AddressOverride)
				{
					result = true;
				}
				else
				{
					// includes fallback to registry value
					var consignorAddress = consignorDocAddress.Address;
					result = GetAuthorityToLeaveWithFallback(consignorAddress, consigneeAddress, consignorAddress, o => o.MiscServ.CNRisAuthorisedToLeaveWithFallback);
				}
			}

			return result;
		}

		static ZBool GetAuthorityToLeaveWithFallback(OrgAddress consignorAddress, OrgAddress consigneeAddress, OrgAddress relevantAddress, Func<OrgHeader, ZBool> getDefaultFallback)
		{
			var result = false;

			var organisation = relevantAddress?.Header;
			if (organisation != null)
			{
				var atlCode = relevantAddress.OA_AuthorityToLeave;
				if (atlCode == AuthorityToLeaveOptions.Codes.DEF)
				{
					var supplierBuyerLink = GetSupplierBuyerLink(consignorAddress, consigneeAddress);
					if (supplierBuyerLink != null)
					{
						atlCode = supplierBuyerLink.OL_AuthorityToLeave;
					}
				}

				result = GetAuthorityToLeave(atlCode, organisation, getDefaultFallback);
			}

			return result;
		}

		static ZBool GetAuthorityToLeave(ZString atlCode, OrgHeader organisation, Func<OrgHeader, ZBool> getDefaultFallback)
		{
			ZBool result;

			if (atlCode == AuthorityToLeaveOptions.Codes.DEF)
			{
				result = getDefaultFallback(organisation);
			}
			else
			{
				result = atlCode == AuthorityToLeaveOptions.Codes.YES;
			}

			return result;
		}

		#endregion

		#region GetConsigneeAuthorityToLeave

		public static ZBool GetConsigneeAuthorityToLeave(OrgAddress consigneeAddress, OrgAddress consignorAddress)
		{
			// includes fallback to registry value
			return GetAuthorityToLeaveWithFallback(consignorAddress, consigneeAddress, consigneeAddress, o => o.MiscServ.CNEisAuthorisedToLeaveWithFallback);
		}

		#endregion

		#region GetClientAuthorityToLeave

		public static ZBool GetClientAuthorityToLeave(OrgAddress clientAddress)
		{
			var result = false;

			var clientOrganisation = clientAddress?.Header;
			if (clientOrganisation != null)
			{
				// includes fallback to registry value
				result = GetAuthorityToLeave(clientAddress.OA_AuthorityToLeave, clientOrganisation, o => o.MiscServ.CMisAuthorisedToLeaveWithFallback);
			}

			return result;
		}

		#endregion

		#region GetSupplierBuyerLink

#if DEBUG
		public
#endif
		static OrgSupplierBuyerLink GetSupplierBuyerLink(OrgAddress consignor, OrgAddress consignee)
		{
			OrgSupplierBuyerLink supplierBuyerLink = null;

			if (consignor != null && consignee != null)
			{
				supplierBuyerLink = consignee.Header.SupplierLinks.Cast<OrgSupplierBuyerLink>().FirstOrDefault(o => o.OL_OH_Supplier == consignor.Header.PK);
			}

			return supplierBuyerLink;
		}

		#endregion
	}
}

#endregion
