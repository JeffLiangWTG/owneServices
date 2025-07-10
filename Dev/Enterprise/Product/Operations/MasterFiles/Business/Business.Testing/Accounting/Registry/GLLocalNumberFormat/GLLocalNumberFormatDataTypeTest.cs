using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GLLocalNumberFormatRegistryDataType))]
	sealed class GLLocalNumberFormatDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<GLLocalNumberFormatRegistryDataType>
	{
		#region Implementation
		protected override GLLocalNumberFormatRegistryDataType GetNewDataType()
		{
			return new GLLocalNumberFormatRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "GLLocalNumberFormatRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			GLLocalNumberFormatCollection collection = new GLLocalNumberFormatCollection();
			byte[] byteArrayValue = System.Array.Empty<byte>();

			return new[] { new ValidSampleAndBinaryValueInDB(collection, byteArrayValue) };
		}

		#endregion
	}
}
