using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EInvoicingCredentials))]
	sealed class EInvoicingCredentialsTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidatation()
		{
			var credentials = new EInvoicingCredentials();
			AssertNoError(credentials.ClientIdInfo, "Please enter a Client ID.");
			AssertNoError(credentials.ClientSecretInfo, "Please enter a Client Secret.");

			credentials.ClientSecret = "secret";
			AssertHasError(credentials.ClientIdInfo, "Please enter a Client ID.");
			AssertNoError(credentials.ClientSecretInfo, "Please enter a Client Secret.");

			credentials.ClientId = "id";
			AssertNoError(credentials.ClientIdInfo, "Please enter a Client ID.");
			AssertNoError(credentials.ClientSecretInfo, "Please enter a Client Secret.");

			credentials.ClientSecret = "";
			AssertNoError(credentials.ClientIdInfo, "Please enter a Client ID.");
			AssertHasError(credentials.ClientSecretInfo, "Please enter a Client Secret.");

			credentials.ClientId = "";
			AssertNoError(credentials.ClientIdInfo, "Please enter a Client ID.");
			AssertNoError(credentials.ClientSecretInfo, "Please enter a Client Secret.");

			credentials.APIKey = "api-key";
			AssertNoErrors(credentials.APIKeyInfo);

			credentials.APIKey = "";
			AssertNoErrors(credentials.APIKeyInfo);
		}

		protected override bool RequiresFactory => false;
		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
			=> new EInvoicingCredentials
			{
				APIKey = "124567890ABCDE",
				ClientId = "Registry Client Id",
				ClientSecret = "Registry Client Secret",
			};

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
	}
}
