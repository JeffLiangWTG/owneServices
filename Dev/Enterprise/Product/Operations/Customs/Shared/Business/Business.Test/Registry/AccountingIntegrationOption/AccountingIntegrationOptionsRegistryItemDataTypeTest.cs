using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(AccountingIntegrationOptionsRegistryItemDataType))]
	sealed class AccountingIntegrationOptionsRegistryItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AccountingIntegrationOptionsRegistryItemDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "AccountingIntegrationOptionsRegistryItemRegistryItemEditor"; }
		}

		protected override AccountingIntegrationOptionsRegistryItemDataType GetNewDataType()
		{
			return new AccountingIntegrationOptionsRegistryItemDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			AccountingIntegrationOptions options = new AccountingIntegrationOptions();
			options.EnableAccountingIntegration = true;
			options.PreApprovalBillingJob = true;
			options.APPostDSB = true;
			options.ARPostDSB = true;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(options, new AccountingIntegrationOptionsRegistryItemDataType().Serialise(options))
			};
		}
	}
}
