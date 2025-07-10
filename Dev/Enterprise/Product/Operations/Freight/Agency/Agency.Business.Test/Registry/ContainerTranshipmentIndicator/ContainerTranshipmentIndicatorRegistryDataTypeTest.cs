using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ContainerTranshipmentIndicatorRegistryDataType))]
	internal class ContainerTranshipmentIndicatorRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ContainerTranshipmentIndicatorRegistryDataType>
	{
		#region Implementation
		protected override ContainerTranshipmentIndicatorRegistryDataType GetNewDataType()
		{
			return new ContainerTranshipmentIndicatorRegistryDataType(ContainerTranshipmentIndicatorCollection.NewAndPopulate());
		}

		protected override string ExpectedEditorName
		{
			get
			{
				return "ContainerTranshipmentIndicatorRegistryItemEditor";
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			ContainerTranshipmentIndicatorCollection collection = new ContainerTranshipmentIndicatorCollection();
			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)) };
		}
		#endregion
	}
}
