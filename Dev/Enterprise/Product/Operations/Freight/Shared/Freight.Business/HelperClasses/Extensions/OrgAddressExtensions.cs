using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Extensions
{
	using Enterprise.MasterFiles.Business;

	public static class OrgAddressExtensions
	{
		public static OrgAddress With(this OrgAddress address, string oA_RL_NKRelatedPortCode = null)
		{
			address.OA_RL_NKRelatedPortCode = oA_RL_NKRelatedPortCode;

			return address;
		}

		public static OrgAddress SingleAddressMatchesCountryCode(this IEnumerable<OrgAddress> addresses, ZString countryCode)
		{
			if (addresses == null || countryCode.Length < 2)
			{
				return null;
			}

			var matches = addresses.Where(address => address.OA_RL_NKRelatedPortCode.Left(2) == countryCode.Left(2)).Take(2).ToArray();
			return matches.Length == 1 ? matches[0] : null;
		}

		public static OrgAddress SingleAddressMatchesPortCode(this IEnumerable<OrgAddress> addresses, ZString portCode)
		{
			if (addresses == null)
			{
				return null;
			}

			var matches = addresses.Where(address => address.OA_RL_NKRelatedPortCode == portCode).Take(2).ToArray();
			return matches.Length == 1 ? matches[0] : null;
		}
	}
}
