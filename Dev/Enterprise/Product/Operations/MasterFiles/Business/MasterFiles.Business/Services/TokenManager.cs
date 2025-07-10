using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Authentication;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.SystemToSystemTrust;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[ThreadSafe]
	public class TokenManager
	{
		const int TokenExpiryBufferInMins = 5;

		static readonly ConcurrentDictionary<MDMProductCodes, TokenManager> tokenManagerInstances = new ConcurrentDictionary<MDMProductCodes, TokenManager>();

		readonly object lockObject = new object();

		readonly MDMProductCodes productCode;

		string cachedToken;

		ISystemToSystemTrustInfo cachedCertificateInfo;

		TokenManager(MDMProductCodes productCode)
		{
			this.productCode = productCode;
		}

		public static TokenManager GetInstance(MDMProductCodes productCode)
		{
			return tokenManagerInstances.GetOrAdd(productCode, key => new TokenManager(key));
		}

		public AuthenticationHeaderValue GetAuthorizationHeaderValue()
		{
			var certificateInfo = GetCertificateInfo();

			if (NeedToUpdateCachedToken(certificateInfo))
			{
				lock (lockObject)
				{
					if (NeedToUpdateCachedToken(certificateInfo))
					{
						cachedToken = ObjectFactory.Get<IAuthorizationTokenProvider>().GetToken(certificateInfo);
						cachedCertificateInfo = certificateInfo.AuthenticationInfo;
					}
				}
			}

			return new AuthenticationHeaderValue((NoResString)"Bearer", cachedToken);
		}

		#region Implementation

		bool NeedToUpdateCachedToken(AuthenticationCertificateInfo certificateInfo)
		{
			return string.IsNullOrEmpty(cachedToken) || cachedCertificateInfo != certificateInfo.AuthenticationInfo || new JwtSecurityToken(cachedToken).ValidTo.Subtract(ZDateTime.UtcNow.ToDateTime()).TotalMinutes <= TokenExpiryBufferInMins;
		}

		AuthenticationCertificateInfo GetCertificateInfo()
		{
			var certificateInfo = default(AuthenticationCertificateInfo);

			try
			{
				certificateInfo = new AuthenticationCertificateInfo(productCode);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ThrowInvalidCertificateException();
			}

			if (certificateInfo.IsEmpty)
			{
				ThrowCertificateNotFoundException();
			}

			return certificateInfo;
		}

		void ThrowInvalidCertificateException()
					=> throw new AuthenticationException((NoResString)"System to system trust certificate is invalid.");

		void ThrowCertificateNotFoundException()
					=> throw new AuthCertNotFoundException();

		#endregion
	}
}
