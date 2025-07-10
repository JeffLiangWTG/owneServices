using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public static class AllowSendingBookingConfirmationRegistryHelper
	{
		public static bool IsAllowed(ZGuid principalPK)
		{
			var principalConfig = SearchPrincipalConfiguration(principalPK, AgencyRegistry.Instance.AllowSendingBookingConfirmationEDIAfterATD.Value);
			if (principalConfig == null || !principalConfig.Enabled)
			{
				var systemLevelRetriever = new RegistryItemProposedValueAccessor(AgencyRegistry.Instance.AllowSendingBookingConfirmationEDIAfterATD, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
				principalConfig = SearchPrincipalConfiguration(principalPK, (AllowSendingBookingConfirmationCollection)systemLevelRetriever.GetCurrentValue().Value);
			}

			if (principalConfig != null)
			{
				return principalConfig.Enabled;
			}

			return false;
		}

		static AllowSendingBookingConfirmation SearchPrincipalConfiguration(ZGuid principalPK, AllowSendingBookingConfirmationCollection principals)
		{
			var principalConfig = principals.OfType<AllowSendingBookingConfirmation>().FirstOrDefault(x => x.PrincipalPK == ZGuid.Empty);
			if (principalConfig != null)
			{
				return principalConfig;
			}

			return principals.OfType<AllowSendingBookingConfirmation>().FirstOrDefault(x => x.PrincipalPK == principalPK);
		}
	}
}
