using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CertificateTypeRegistryDataType))]
	sealed class CertificateTypeRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CertificateTypeRegistryDataType>
	{
		#region Implementation

		protected override CertificateTypeRegistryDataType GetNewDataType()
		{
			return new CertificateTypeRegistryDataType(new CertificateTypeCollection());
		}

		protected override string ExpectedEditorName
		{
			get { return "CertificateTypeRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			CertificateTypeCollection collection = new CertificateTypeCollection();

			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)) };
		}

		#endregion
	}
}
