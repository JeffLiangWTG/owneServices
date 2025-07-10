using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MaximumAllowedTransactionAmountRegistryDataType))]
	sealed class MaximumAllowedTransactionAmountRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<MaximumAllowedTransactionAmountRegistryDataType>
	{
		protected override string ExpectedEditorName => "MaximumAllowedTransactionAmountRegistryItemEditor";

		protected override MaximumAllowedTransactionAmountRegistryDataType GetNewDataType() => new MaximumAllowedTransactionAmountRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var maximumAllowedTransactionAmount1 = new MaximumAllowedTransactionAmount();
			maximumAllowedTransactionAmount1.MaximumAllowedHeaderAmount = 1M;
			maximumAllowedTransactionAmount1.MaximumAllowedLineAmount = 2M;
			var byteArray1 = DataType.Serialise(maximumAllowedTransactionAmount1);

			var maximumAllowedTransactionAmount2 = new MaximumAllowedTransactionAmount();
			maximumAllowedTransactionAmount2.MaximumAllowedHeaderAmount = 3M;
			maximumAllowedTransactionAmount2.MaximumAllowedLineAmount = 4M;
			var byteArray2 = DataType.Serialise(maximumAllowedTransactionAmount2);

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(maximumAllowedTransactionAmount1, byteArray1)
				, new ValidSampleAndBinaryValueInDB(maximumAllowedTransactionAmount2, byteArray2)
			};
		}
	}
}
