using System;
using System.Net;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Tracking.Testing
{
	public class TrackingUrlBulderTest : TestCase
	{
		public void TestRootPathNotSet()
		{
			AssertEquals("Should return empty if root path is not set", string.Empty, TrackingUrlBuilder.BuildUrl(string.Empty, Guid.NewGuid(), TrackingConstants.BusinessContext.WarehouseOrder, Guid.NewGuid()));
			AssertEquals("Should return empty if root path is not set", string.Empty, TrackingUrlBuilder.BuildUrl(" ", Guid.NewGuid(), TrackingConstants.BusinessContext.WarehouseOrder, Guid.NewGuid()));
		}

		public void TestInvalidRoothPath()
		{
			AssertEquals("Should return empty if root path invalid", string.Empty, TrackingUrlBuilder.BuildUrl("http:///somethign///help///me///invalid", Guid.NewGuid(), TrackingConstants.BusinessContext.WarehouseOrder, Guid.NewGuid()));
		}

		public void TestBuildUrlForPK()
		{
			var contactPK = Guid.NewGuid();
			var businessContext = TrackingConstants.BusinessContext.WarehouseOrder;
			var businessContextPK = Guid.NewGuid();
			var additionalRef1 = Guid.NewGuid();
			var additionalRef2 = Guid.NewGuid();

			var queryString = new SecureQueryString();
			queryString.Add(TrackingConstants.AutoLogin.ContactPKKey, contactPK.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextKey, businessContext.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextPKKey, businessContextPK.ToString());
			string generatedUrl = TrackingUrlBuilder.BuildUrl("www.tracking.edi.com.au", contactPK, businessContext, businessContextPK);
			string expectedUrl = string.Format("http://www.tracking.edi.com.au/{0}?{1}={2}",
				TrackingConstants.RelativePath.AutoLoginRequestHandler,
				TrackingConstants.AutoLogin.SecureQueryStringDataKey,
				WebUtility.UrlEncode(queryString.ToString()));
			AssertEquals(expectedUrl, generatedUrl);

			queryString = new SecureQueryString();
			queryString.Add(TrackingConstants.AutoLogin.ContactPKKey, contactPK.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextKey, businessContext.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextPKKey, businessContextPK.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextAdditionalRefsKey, string.Format("{0},{1}", additionalRef1, additionalRef2));
			generatedUrl = TrackingUrlBuilder.BuildUrl("www.tracking.edi.com.au", contactPK, businessContext, businessContextPK, new Guid[] { additionalRef1, additionalRef2 });
			expectedUrl = string.Format("http://www.tracking.edi.com.au/{0}?{1}={2}",
				TrackingConstants.RelativePath.AutoLoginRequestHandler,
				TrackingConstants.AutoLogin.SecureQueryStringDataKey,
				WebUtility.UrlEncode(queryString.ToString()));
			AssertEquals(expectedUrl, generatedUrl);

			queryString = new SecureQueryString();
			queryString.Add(TrackingConstants.AutoLogin.ContactPKKey, contactPK.ToString());
			queryString.Add(TrackingConstants.AutoLogin.RequireLoginKey, true.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextKey, businessContext.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextPKKey, businessContextPK.ToString());
			generatedUrl = TrackingUrlBuilder.BuildUrl("www.tracking.edi.com.au", contactPK, businessContext, businessContextPK, true);
			expectedUrl = string.Format("http://www.tracking.edi.com.au/{0}?{1}={2}",
				TrackingConstants.RelativePath.AutoLoginRequestHandler,
				TrackingConstants.AutoLogin.SecureQueryStringDataKey,
				WebUtility.UrlEncode(queryString.ToString()));
			AssertEquals(expectedUrl, generatedUrl);

			queryString = new SecureQueryString();
			queryString.Add(TrackingConstants.AutoLogin.ContactPKKey, contactPK.ToString());
			queryString.Add(TrackingConstants.AutoLogin.RequireLoginKey, true.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextKey, businessContext.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextPKKey, businessContextPK.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextAdditionalRefsKey, string.Format("{0},{1}", additionalRef1, additionalRef2));
			generatedUrl = TrackingUrlBuilder.BuildUrl("www.tracking.edi.com.au", contactPK, businessContext, businessContextPK, true, new Guid[] { additionalRef1, additionalRef2 });
			expectedUrl = string.Format("http://www.tracking.edi.com.au/{0}?{1}={2}",
				TrackingConstants.RelativePath.AutoLoginRequestHandler,
				TrackingConstants.AutoLogin.SecureQueryStringDataKey,
				WebUtility.UrlEncode(queryString.ToString()));
			AssertEquals(expectedUrl, generatedUrl);
		}

		public void TestBuildUrlForNK()
		{
			var contactPK = Guid.NewGuid();
			var businessContext = TrackingConstants.BusinessContext.WarehouseOrder;
			var businessContextNK = "123456789";

			var queryString = new SecureQueryString();
			queryString.Add(TrackingConstants.AutoLogin.ContactPKKey, contactPK.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextKey, businessContext.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextNKKey, businessContextNK);
			string generatedUrl = TrackingUrlBuilder.BuildUrl("www.tracking.edi.com.au", contactPK, businessContext, businessContextNK);
			string expectedUrl = string.Format("http://www.tracking.edi.com.au/{0}?{1}={2}",
				TrackingConstants.RelativePath.AutoLoginRequestHandler,
				TrackingConstants.AutoLogin.SecureQueryStringDataKey,
				WebUtility.UrlEncode(queryString.ToString()));
			AssertEquals(expectedUrl, generatedUrl);

			queryString = new SecureQueryString();
			queryString.Add(TrackingConstants.AutoLogin.ContactPKKey, contactPK.ToString());
			queryString.Add(TrackingConstants.AutoLogin.RequireLoginKey, true.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextKey, businessContext.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextNKKey, businessContextNK);
			generatedUrl = TrackingUrlBuilder.BuildUrl("www.tracking.edi.com.au", contactPK, businessContext, businessContextNK, true);
			expectedUrl = string.Format("http://www.tracking.edi.com.au/{0}?{1}={2}",
				TrackingConstants.RelativePath.AutoLoginRequestHandler,
				TrackingConstants.AutoLogin.SecureQueryStringDataKey,
				WebUtility.UrlEncode(queryString.ToString()));
			AssertEquals(expectedUrl, generatedUrl);
		}

		public void TestBuildNeoUrl()
		{
			var businessContextPK = Guid.NewGuid();

			var generatedUrl = TrackingUrlBuilder.BuildNeoUrl("https://glowdev/Portals", "SomeFormFlow", businessContextPK);

			AssertEquals($"https://glowdev/Portals/goto/SomeFormFlow?entityPK={businessContextPK}", generatedUrl);
		}

		public void TestBuildNeoGuestTrackingUrl()
		{
			var businessContextPK = Guid.NewGuid();

			var generatedUrl = TrackingUrlBuilder.BuildNeoGuestTrackingUrl("https://glowdev/Portals", businessContextPK);

			AssertEquals($"https://glowdev/Portals/NEO/Desktop#/tracker?trackingKey={businessContextPK}", generatedUrl);
		}

		public void TestBuildNeoUrl_NK()
		{
			var businessContextNK = "BIZO1234";

			var generatedUrl = TrackingUrlBuilder.BuildNeoUrl("https://glowdev/Portals", "SomeFormFlow", businessContextNK);

			AssertEquals($"https://glowdev/Portals/goto/SomeFormFlow?entityNK={businessContextNK}", generatedUrl);
		}

		public void TestBuildNeoGuestTrackingUrl_NK()
		{
			var businessContextNK = "BIZO1234";

			var generatedUrl = TrackingUrlBuilder.BuildNeoGuestTrackingUrl("https://glowdev/Portals", businessContextNK);

			AssertEquals($"https://glowdev/Portals/NEO/Desktop#/tracker?trackingNumber={businessContextNK}", generatedUrl);
		}
	}
}
