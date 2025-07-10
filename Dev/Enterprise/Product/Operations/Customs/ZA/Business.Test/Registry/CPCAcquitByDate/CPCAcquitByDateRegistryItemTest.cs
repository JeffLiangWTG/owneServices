using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.Business.Testing
{
	[TestedType(typeof(CPCAcquitByDateRegistryItem))]
	sealed class CPCAcquitByDateRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<CPCAcquitByDate>
	{
		protected override StronglyTypedRegistryItem<CPCAcquitByDate, CPCAcquitByDate> GetNewRegistryItem() => new CPCAcquitByDateRegistryItem("", null, null, null, RegistryStorageFlags.Company);

		protected override CPCAcquitByDate ValidValue => new CPCAcquitByDate();
	}

	[TestedType(typeof(CPCAcquitByDateRegistryDataType))]
	sealed class CPCAcquitByDateRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CPCAcquitByDateRegistryDataType>
	{
		protected override string ExpectedEditorName => "CPCAcquitByDateRegistryItemEditor";

		protected override CPCAcquitByDateRegistryDataType GetNewDataType() => new CPCAcquitByDateRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var data1 = new CPCAcquitByDate() { Quantity = 30, Unit = "Day(s)" };
			var data2 = new CPCAcquitByDate() { Quantity = 6, Unit = "Month(s)" };
			var dataType = new CPCAcquitByDateRegistryDataType();

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(data1, dataType.Serialise(data1)),
				new ValidSampleAndBinaryValueInDB(data2, dataType.Serialise(data2))
			};
		}
	}
}
