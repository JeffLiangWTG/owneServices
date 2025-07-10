using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UserManagementUriProviderTest : TestCaseWithFactory
	{
		public void TestGetBulkCreateUri()
		{
			using (SystemDataRegistry.Instance.IdpUserSynchronisationEndpoint.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var ex = AssertExceptionThrown<IdpConfigException>(() => IdpEndpointUriProvider.GetBulkCreateUsersUri());
				AssertEquals($"Identity Provider user synchronization endpoint hasn't been configured in the registry. Please configure it here: {SystemDataRegistry.Instance.IdpUserSynchronisationEndpoint.GetLocation()}", ex.Message);
			}

			using (SystemDataRegistry.Instance.IdpUserSynchronisationEndpoint.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InvalidBaseUrl))
			{
				var ex = AssertExceptionThrown<IdpConfigException>(() => IdpEndpointUriProvider.GetBulkCreateUsersUri());
				AssertEquals($"Identity Provider user synchronization endpoint config is not formatted correctly in {SystemDataRegistry.Instance.IdpUserSynchronisationEndpoint.GetLocation()}", ex.Message);
			}

			using (SystemDataRegistry.Instance.IdpUserSynchronisationEndpoint.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TestBaseUrl))
			{
				var uri = IdpEndpointUriProvider.GetBulkCreateUsersUri();
				AssertEquals("https://wiseidp.net/api/usermanagement/bulkCreate", uri.ToString());
			}
		}

		public void TestGetIdpUserSyncBaseUri()
		{
			using (SystemDataRegistry.Instance.IdpUserSynchronisationEndpoint.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var ex = AssertExceptionThrown<IdpConfigException>(() => IdpEndpointUriProvider.GetIdpUserSyncBaseUri());
				AssertEquals($"Identity Provider user synchronization endpoint hasn't been configured in the registry. Please configure it here: {SystemDataRegistry.Instance.IdpUserSynchronisationEndpoint.GetLocation()}", ex.Message);
			}

			using (SystemDataRegistry.Instance.IdpUserSynchronisationEndpoint.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InvalidBaseUrl))
			{
				var ex = AssertExceptionThrown<IdpConfigException>(() => IdpEndpointUriProvider.GetIdpUserSyncBaseUri());
				AssertEquals($"Identity Provider user synchronization endpoint config is not formatted correctly in {SystemDataRegistry.Instance.IdpUserSynchronisationEndpoint.GetLocation()}", ex.Message);
			}

			using (SystemDataRegistry.Instance.IdpUserSynchronisationEndpoint.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TestBaseUrl))
			{
				var uri = IdpEndpointUriProvider.GetIdpUserSyncBaseUri();
				AssertEquals(TestBaseUrl, uri.ToString());
			}
		}

		const string TestBaseUrl = "https://wiseidp.net/api/usermanagement";

		const string InvalidBaseUrl = @"https://wiseidp.net)/api/usermanagement";
	}
}
