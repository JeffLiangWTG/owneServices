using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(DefaultContainerModesRegistryDataType))]
	sealed class DefaultContainerModesRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DefaultContainerModesRegistryDataType>
	{
		#region Implementation

		protected override DefaultContainerModesRegistryDataType GetNewDataType()
		{
			return new DefaultContainerModesRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "DefaultContainerModesRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			DefaultContainerModesCollection collection = new DefaultContainerModesCollection();

			DefaultContainerModes defaultContainerModes = collection.AddNew();
			defaultContainerModes.DoNotPerformListValidationOnContainerMode = true;
			defaultContainerModes.TransportMode = "ROA";
			defaultContainerModes.ContainerMode = "FCL";

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
				0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,67,0,111,0,110,0,116,0,97,0,105,0,110,0,101,0,114,0,77,0,111,
				0,100,0,101,0,115,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,
				0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,
				0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,
				0,109,0,97,0,34,0,62,0,60,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,67,0,111,0,110,0,116,0,97,0,105,0,110,0,101,0,114,0,77,0,111,0,100,0,101,0,115,0,62,0,60,0,84,0,114,0,97,0,110,0,115,
				0,112,0,111,0,114,0,116,0,77,0,111,0,100,0,101,0,62,0,82,0,79,0,65,0,60,0,47,0,84,0,114,0,97,0,110,0,115,0,112,0,111,0,114,0,116,0,77,0,111,0,100,0,101,0,62,0,60,0,67,0,111,0,110,0,116,
				0,97,0,105,0,110,0,101,0,114,0,77,0,111,0,100,0,101,0,62,0,70,0,67,0,76,0,60,0,47,0,67,0,111,0,110,0,116,0,97,0,105,0,110,0,101,0,114,0,77,0,111,0,100,0,101,0,62,0,60,0,47,0,68,0,101,
				0,102,0,97,0,117,0,108,0,116,0,67,0,111,0,110,0,116,0,97,0,105,0,110,0,101,0,114,0,77,0,111,0,100,0,101,0,115,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,68,0,101,0,102,0,97,
				0,117,0,108,0,116,0,67,0,111,0,110,0,116,0,97,0,105,0,110,0,101,0,114,0,77,0,111,0,100,0,101,0,115,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
