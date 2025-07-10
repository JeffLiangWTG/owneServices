using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class AirBookingUriProviderTest : TestCaseWithFactory
	{
		public void TestGetCarrierConfigurationUri()
		{
			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				using (FreightDataRegistry.Instance.EBookingServicesApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new EBookingApiUrls(string.Empty)))
				{
					(var uri, var errorMessage) = AirBookingUriProvider.GetCarrierConfigurationUri();
					AssertEquals($"eBooking API URL hasn't been configured in the registry. Please configure it here: {FreightDataRegistry.Instance.EBookingServicesApiUrl.GetLocationInEnglish()}", errorMessage);
					AssertEquals(null, uri);
				}

				using (FreightDataRegistry.Instance.EBookingServicesApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new EBookingApiUrls(EBookingApiUrls.Constants.ProdCode)))
				{
					(var uri, var errorMessage) = AirBookingUriProvider.GetCarrierConfigurationUri();
					AssertEquals(null, errorMessage);
					AssertEquals("https://abe.wisegrid.net/v1/carriers", uri.ToString());
				}

				using (FreightDataRegistry.Instance.EBookingServicesApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new EBookingApiUrls(EBookingApiUrls.Constants.TestCode)))
				{
					(var uri, var errorMessage) = AirBookingUriProvider.GetCarrierConfigurationUri();
					AssertEquals(null, errorMessage);
					AssertEquals("https://abe-test.wisegrid.net/v1/carriers", uri.ToString());
				}
			}

			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://test.com"))
			{
				(var uri, var errorMessage) = AirBookingUriProvider.GetCarrierConfigurationUri();
				AssertEquals(null, errorMessage);
				AssertEquals("http://test.com/carriers", uri.ToString());
			}

			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, @"http://test).com/"))
			{
				(var uri, var errorMessage) = AirBookingUriProvider.GetCarrierConfigurationUri();
				AssertEquals($"The eBooking API URL is not formatted correctly in {FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.GetLocationInEnglish()}", errorMessage);
				AssertEquals(null, uri);
			}
		}

		public void TestGetBookingUri()
		{
			var mAwb = "618-73808291";
			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				using (FreightDataRegistry.Instance.EBookingServicesApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new EBookingApiUrls(string.Empty)))
				{
					(var uri, var errorMessage) = AirBookingUriProvider.GetBookingUri(mAwb);
					AssertEquals($"eBooking API URL hasn't been configured in the registry. Please configure it here: {FreightDataRegistry.Instance.EBookingServicesApiUrl.GetLocationInEnglish()}", errorMessage);
					AssertEquals(null, uri);
				}

				using (FreightDataRegistry.Instance.EBookingServicesApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new EBookingApiUrls(EBookingApiUrls.Constants.ProdCode)))
				{
					(var uri, var errorMessage) = AirBookingUriProvider.GetBookingUri(mAwb);
					AssertEquals(null, errorMessage);
					AssertEquals("https://abe.wisegrid.net/v1/booking/618-73808291", uri.ToString());
				}

				using (FreightDataRegistry.Instance.EBookingServicesApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new EBookingApiUrls(EBookingApiUrls.Constants.TestCode)))
				{
					(var uri, var errorMessage) = AirBookingUriProvider.GetBookingUri(mAwb);
					AssertEquals(null, errorMessage);
					AssertEquals("https://abe-test.wisegrid.net/v1/booking/618-73808291", uri.ToString());
				}
			}

			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://test.com"))
			{
				(var uri, var errorMessage) = AirBookingUriProvider.GetBookingUri(mAwb);
				AssertEquals(null, errorMessage);
				AssertEquals("http://test.com/booking/618-73808291", uri.ToString());
			}

			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, @"http://test).com/"))
			{
				(var uri, var errorMessage) = AirBookingUriProvider.GetBookingUri(mAwb);
				AssertEquals($"The eBooking API URL is not formatted correctly in {FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.GetLocationInEnglish()}", errorMessage);
				AssertEquals(null, uri);
			}
		}

		public void TestGetCancellationUri()
		{
			var mAwb = "618-73808291";
			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				using (FreightDataRegistry.Instance.EBookingServicesApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new EBookingApiUrls(string.Empty)))
				{
					(var uri, var errorMessage) = AirBookingUriProvider.GetCancellationUri(mAwb);
					AssertEquals($"eBooking API URL hasn't been configured in the registry. Please configure it here: {FreightDataRegistry.Instance.EBookingServicesApiUrl.GetLocationInEnglish()}", errorMessage);
					AssertEquals(null, uri);
				}

				using (FreightDataRegistry.Instance.EBookingServicesApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new EBookingApiUrls(EBookingApiUrls.Constants.ProdCode)))
				{
					(var uri, var errorMessage) = AirBookingUriProvider.GetCancellationUri(mAwb);
					AssertEquals(null, errorMessage);
					AssertEquals("https://abe.wisegrid.net/v1/booking/618-73808291/cancel", uri.ToString());
				}

				using (FreightDataRegistry.Instance.EBookingServicesApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new EBookingApiUrls(EBookingApiUrls.Constants.TestCode)))
				{
					(var uri, var errorMessage) = AirBookingUriProvider.GetCancellationUri(mAwb);
					AssertEquals(null, errorMessage);
					AssertEquals("https://abe-test.wisegrid.net/v1/booking/618-73808291/cancel", uri.ToString());
				}
			}

			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://test.com"))
			{
				(var uri, var errorMessage) = AirBookingUriProvider.GetCancellationUri(mAwb);
				AssertEquals(null, errorMessage);
				AssertEquals("http://test.com/booking/618-73808291/cancel", uri.ToString());
			}

			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, @"http://test).com/"))
			{
				(var uri, var errorMessage) = AirBookingUriProvider.GetCancellationUri(mAwb);
				AssertEquals($"The eBooking API URL is not formatted correctly in {FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.GetLocationInEnglish()}", errorMessage);
				AssertEquals(null, uri);
			}
		}
	}
}
