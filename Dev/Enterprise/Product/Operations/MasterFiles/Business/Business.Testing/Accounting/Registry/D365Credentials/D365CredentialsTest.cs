using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(D365Credentials))]
	sealed class D365CredentialsTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidation()
		{
			var credentials = new D365Credentials();
			AssertNoErrors(credentials.ClientIDInfo);
			AssertNoErrors(credentials.ClientSecretInfo);
			AssertNoErrors(credentials.TenantIDInfo);

			// Test case: ClientID is set, ClientSecret and TenantID are empty
			credentials.ClientID = "id";
			credentials.ClientSecret = "";
			credentials.TenantID = "";
			AssertNoErrors(credentials.ClientIDInfo);
			AssertHasError(credentials.ClientSecretInfo, "Please enter a Client Secret.");
			AssertHasError(credentials.TenantIDInfo, "Please enter a Tenant ID.");

			// Test case: ClientID is empty, ClientSecret and TenantID are set
			credentials.ClientID = "";
			credentials.ClientSecret = "secret";
			credentials.TenantID = "tenant";
			AssertHasError(credentials.ClientIDInfo, "Please enter a Client ID.");
			AssertNoErrors(credentials.ClientSecretInfo);
			AssertNoErrors(credentials.TenantIDInfo);

			// Test case: All fields are set
			credentials.ClientID = "id";
			credentials.ClientSecret = "secret";
			credentials.TenantID = "tenant";
			AssertNoErrors(credentials.ClientIDInfo);
			AssertNoErrors(credentials.ClientSecretInfo);
			AssertNoErrors(credentials.TenantIDInfo);
		}
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		=> new D365Credentials
		{
			ClientID = "Registry Client Id",
			ClientSecret = "Registry Client Secret",
			TenantID = "Registry Tenant Id",
		};

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
	}
}
