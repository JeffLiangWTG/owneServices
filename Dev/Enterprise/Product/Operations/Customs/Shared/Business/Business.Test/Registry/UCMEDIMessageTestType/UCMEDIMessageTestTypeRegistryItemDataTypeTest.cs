using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(UCMEDIMessageTestTypeRegistryItemDataType))]
	sealed class UCMEDIMessageTestTypeRegistryItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<UCMEDIMessageTestTypeRegistryItemDataType>
	{
		protected override string ExpectedEditorName => "UCMEDIMessageTestTypesRegistryItemEditor";
		protected override UCMEDIMessageTestTypeRegistryItemDataType GetNewDataType() => new UCMEDIMessageTestTypeRegistryItemDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var testTypleCollection1 = new UCMEDIMessageTestTypeCollection();
			var testType1 = testTypleCollection1.AddNew();
			testType1.ApplicationCode = "_T1";
			testType1.UCKDelayTimeInMilliseconds = 10;
			testType1.UCQDelayTimeInMilliseconds = 20;
			testType1.UCUDelayTimeInMilliseconds = 30;
			var testTypleCollection2 = new UCMEDIMessageTestTypeCollection();
			var testType2 = testTypleCollection2.AddNew();
			testType2.ApplicationCode = "_T2";
			testType2.UCKDelayTimeInMilliseconds = 40;
			testType2.UCQDelayTimeInMilliseconds = 50;
			testType2.UCUDelayTimeInMilliseconds = 60;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(testTypleCollection1, new UCMEDIMessageTestTypeRegistryItemDataType().Serialise(testTypleCollection1)),
				new ValidSampleAndBinaryValueInDB(testTypleCollection2, new UCMEDIMessageTestTypeRegistryItemDataType().Serialise(testTypleCollection2))
			};
		}
	}
}
