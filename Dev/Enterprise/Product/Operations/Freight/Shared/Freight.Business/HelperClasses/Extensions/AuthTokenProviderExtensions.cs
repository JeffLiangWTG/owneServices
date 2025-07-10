using System;
using System.Threading;
using AuthenticationService.Client.Models;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	static class AuthTokenProviderExtensions
	{
		public static (string Token, string ValidationMessage) GetToken(this IAuthTokenProvider authTokenProvider, string correlationId, TimeSpan expirationTime, OrgContact contact = null, CancellationToken cancellationToken = default)
		{
			if (authTokenProvider == null)
			{
				throw new ArgumentNullException(nameof(authTokenProvider));
			}

			return authTokenProvider.GetToken(correlationId, (int)expirationTime.TotalSeconds, OverrideLoginInfoWithContactDetails, cancellationToken);

			void OverrideLoginInfoWithContactDetails(LoginInfo loginInfo)
			{
				if (contact != null)
				{
					loginInfo.UserFullName = contact.OC_ContactName;
					loginInfo.UserEmail = contact.OC_Email;
					if (contact.ParentOrg != null)
					{
						loginInfo.ClientCompanyCode = contact.ParentOrg.OH_Code;
						loginInfo.ClientCompanyName = contact.ParentOrg.OH_FullName;
					}
				}
			}
		}
	}
}
