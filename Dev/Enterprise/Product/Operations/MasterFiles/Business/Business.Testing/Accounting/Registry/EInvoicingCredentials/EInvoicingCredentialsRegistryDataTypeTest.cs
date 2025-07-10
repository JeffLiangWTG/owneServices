using System.Text;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EInvoicingCredentialsRegistryDataType))]
	sealed class EInvoicingCredentialsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EInvoicingCredentialsRegistryDataType>
	{
		#region Implementation

		protected override EInvoicingCredentialsRegistryDataType GetNewDataType()
			=> new EInvoicingCredentialsRegistryDataType();

		protected override string ExpectedEditorName => "EInvoicingCredentialsRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
			=> new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(
					new EInvoicingCredentials()
					{
						APIKey = "CAAAAAG2SUNI5RKN4GNVWTDUKL26TDSDBP7TOPJQSROETUDLNGQXOH7FBW7QSKB6ZMOIIDF2YBNWYSDYUTFA",
						ClientId = "The Client Id",
						ClientSecret = "The Client Secret",
					},
					Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><EInvoicingCredentials><APIKey>2a5LfEzGcaMBYAT8jC186f0YTGnOT+zj4cagKkLvyIxA1o5rFbETAooMvi0FWecIi0dfAIHVVjYHrO7XHOcrDAyEZTjAb3QwbOEQqqol2yQChDacJwEjNqlWZ9V0Q06kllm4qjd95Tz6JEbV2ON2ldsW5nleLY4VKeUo5+VG0TIZY75QDW+7GimZAdXVWMFr1vp5uXRnR3NbIrtdjk6ITH9U9E+nhSNvGA5IHUCk6gE=</APIKey><ClientId>The Client Id</ClientId><ClientSecret>EsP6WJHxDYe5W2aVoBQo1WAFgpqFTGUMXwwR/GC32z9cCkmzRSTWsJ/2vT0L9ZtG</ClientSecret></EInvoicingCredentials>")),

				new ValidSampleAndBinaryValueInDB(
					new EInvoicingCredentials(),
					Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><EInvoicingCredentials><APIKey>Grl7AU7oSr1WvdsuqS66bQ==</APIKey><ClientId /><ClientSecret>Grl7AU7oSr1WvdsuqS66bQ==</ClientSecret></EInvoicingCredentials>")),
			};

		#endregion
	}
}
