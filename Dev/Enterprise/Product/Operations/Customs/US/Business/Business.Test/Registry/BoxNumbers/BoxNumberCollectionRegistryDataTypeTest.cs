using Enterprise.Customs.US.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(BoxNumberCollectionRegistryDataType))]
	sealed class BoxNumberCollectionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<BoxNumberCollectionRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "BoxNumbersCollectionRegistryItemEditor"; }
		}

		protected override BoxNumberCollectionRegistryDataType GetNewDataType()
		{
			return new BoxNumberCollectionRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var boxNumbers1 = new BoxNumberCollection();
			var boxNo1 = boxNumbers1.AddNew();
			boxNo1.TransportMode = BoxNoTransportModeList.Codes.ALL;
			boxNo1.BoxNo = "123";

			var boxNumbers2 = new BoxNumberCollection();
			var boxNo2 = boxNumbers2.AddNew();
			boxNo2.TransportMode = BoxNoTransportModeList.Codes.SEA;
			boxNo2.BoxNo = "456";

			return
			[
				new ValidSampleAndBinaryValueInDB(boxNumbers1, new BoxNumberCollectionRegistryDataType().Serialise(boxNumbers1)),
				new ValidSampleAndBinaryValueInDB(boxNumbers2, new BoxNumberCollectionRegistryDataType().Serialise(boxNumbers2))
			];
		}
	}
}
