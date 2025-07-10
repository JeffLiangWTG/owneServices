using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OnHoldTermsRegistryDataType))]
	sealed class OnHoldTermsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<OnHoldTermsRegistryDataType>
	{
		protected override OnHoldTermsRegistryDataType GetNewDataType()
		{
			return new OnHoldTermsRegistryDataType();
		}

		protected override string ExpectedEditorName => "OnHoldTermsRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var registryValue1 = new OnHoldTerms();
			registryValue1.Terms = ARInvoiceTermsList.CashOnDelivery.Code;
			registryValue1.TermDays = 7;

			var byteArray1 = DataType.Serialise(registryValue1);

			var registryValue2 = new OnHoldTerms();
			registryValue2.Terms = ARInvoiceTermsList.FromPeriodEnd.Code;
			registryValue2.TermDays = 1;

			var byteArray2 = DataType.Serialise(registryValue2);

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(registryValue1, byteArray1)
				, new ValidSampleAndBinaryValueInDB(registryValue2, byteArray2)
			};
		}
	}
}
