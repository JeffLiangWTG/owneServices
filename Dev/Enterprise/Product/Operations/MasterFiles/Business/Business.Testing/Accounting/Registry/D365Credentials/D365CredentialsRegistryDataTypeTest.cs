using System.Text;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(D365CredentialsRegistryDataType))]
	sealed class D365CredentialsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<D365CredentialsRegistryDataType>
	{
		#region Implementation

		protected override D365CredentialsRegistryDataType GetNewDataType()
			=> new D365CredentialsRegistryDataType();

		protected override string ExpectedEditorName => "D365CredentialsRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
			=> new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(
					new D365Credentials()
					{
						ClientID = "client id 1",
						ClientSecret = "client secret 1",
						TenantID = "1b111111-e1b1-1b11-ade1-11abcdefgh01",
					},
					Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><D365Credentials><ClientID>client id 1</ClientID><ClientSecret>client secret 1</ClientSecret><TenantID>1b111111-e1b1-1b11-ade1-11abcdefgh01</TenantID></D365Credentials>")),
				
				new ValidSampleAndBinaryValueInDB(
					new D365Credentials()
					{
						ClientID = "client id 2",
						ClientSecret = "client secret 2",
						TenantID = "2b222222-e1b1-1b11-ade1-11abcdefgh01",
					},
					Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><D365Credentials><ClientID>client id 2</ClientID><ClientSecret>client secret 2</ClientSecret><TenantID>2b222222-e1b1-1b11-ade1-11abcdefgh01</TenantID></D365Credentials>")),
			};

		#endregion
	}
}
