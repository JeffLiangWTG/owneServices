using System;
using Microsoft.AspNetCore.Http;

namespace CargoWise.Blazor.SessionBroker
{
	// Taken from Yarp.ReverseProxy Version=1.0.0-preview.11.21223.5 to maintain compatability with CustomSessionAffinityMiddleware
	public class AffinityCookieOptionsProvider
	{
		public static readonly string DefaultCookieName = ".Yarp.ReverseProxy.Affinity";

		public CookieBuilder Cookie
		{
			get
			{
				return _cookieBuilder;
			}
			set
			{
				_cookieBuilder = value ?? throw new ArgumentNullException(nameof(value));
			}
		}

		class AffinityCookieBuilder : CookieBuilder
		{
			public override TimeSpan? Expiration
			{
				get { return null; }
				set { throw new InvalidOperationException("Expiration cannot be set for the cookie defined by CookieSessionAffinityProviderOptions"); }
			}

			public AffinityCookieBuilder()
			{
				Name = DefaultCookieName;
				SecurePolicy = CookieSecurePolicy.None;
				SameSite = SameSiteMode.Unspecified;
				HttpOnly = true;
				IsEssential = false;
			}
		}

		CookieBuilder _cookieBuilder = new AffinityCookieBuilder();
	}
}
