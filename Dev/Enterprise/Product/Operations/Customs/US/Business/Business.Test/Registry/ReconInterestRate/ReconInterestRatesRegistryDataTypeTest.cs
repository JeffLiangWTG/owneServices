using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(ReconInterestRatesRegistryDataType))]
	sealed class ReconInterestRatesRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ReconInterestRatesRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "ReconInterestRatesRegistryItemEditor"; }
		}

		protected override ReconInterestRatesRegistryDataType GetNewDataType()
		{
			return new ReconInterestRatesRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new ReconInterestRateCollection();
			var rate1 = collection1.AddNew();
			rate1.StartDate = new ZDateTime(2008, 4, 1);
			rate1.EndDate = new ZDateTime(2008, 6, 30);
			rate1.Rate = 6m;

			var collection2 = new ReconInterestRateCollection();
			var rate2 = collection2.AddNew();
			rate2.StartDate = new ZDateTime(2025, 4, 1);
			rate2.EndDate = new ZDateTime(2025, 6, 30);
			rate2.Rate = 4m;

			return
			[
				new ValidSampleAndBinaryValueInDB(collection1, new ReconInterestRatesRegistryDataType().Serialise(collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, new ReconInterestRatesRegistryDataType().Serialise(collection2))
			];
		}
	}
}
