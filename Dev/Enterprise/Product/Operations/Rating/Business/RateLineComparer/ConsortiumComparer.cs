using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Business
{
	abstract class ConsortiumComparer : BaseRateLineComparer
	{
		protected int Compare(OrgHeader provider1, OrgHeader provider2)
		{
			if (provider1 != null && provider2 != null && provider1.PK != provider2.PK)
			{
				if (provider1.OH_IsShippingConsortium && provider2.OH_IsShippingConsortium)
				{
					throw new AutoRaterException(string.Format(GetExceptionMessage(provider1.OH_Code, provider2.OH_Code)));
				}

				if (provider2.OH_IsShippingConsortium)
				{
					return 1;
				}

				if (provider1.OH_IsShippingConsortium)
				{
					return -1;
				}
			}

			return 0;
		}

		public static string GetExceptionMessage(string consortiumOrgCode1, string consortiumOrgCode2)
		{
			return Res.GetString("e3a7d35a-8f5f-4aa5-a2f2-306f360f6574", "Valid rates exist for more than 1 consortium - {0} and {1}.\r\nThe system is unable to identify which consortium's rates should be used.\r\nPlease specify a rate directly for this supplier, or remove one of the consortium rates.", consortiumOrgCode1, consortiumOrgCode2);
		}
	}
}
