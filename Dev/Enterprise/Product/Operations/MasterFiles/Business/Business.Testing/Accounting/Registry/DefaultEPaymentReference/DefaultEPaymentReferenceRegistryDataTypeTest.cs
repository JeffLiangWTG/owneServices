using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DefaultEPaymentReferenceRegistryDataType))]
	sealed class DefaultEPaymentReferenceRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DefaultEPaymentReferenceRegistryDataType>
	{
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new DefaultEPaymentReferenceCollection();
			var ref1 = collection1.AddNew();
			ref1.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			ref1.ReferenceType = EPaymentReferenceTypes.FreeText;
			ref1.Reference = "Test";
			var byteArray1 = DataType.Serialise(collection1);

			var collection2 = new DefaultEPaymentReferenceCollection();
			var ref2 = collection2.AddNew();
			ref2.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			ref2.ReferenceType = EPaymentReferenceTypes.PaymentReferenceNum;
			var byteArray2 = DataType.Serialise(collection2);

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, byteArray1),
				new ValidSampleAndBinaryValueInDB(collection2, byteArray2)
			};
		}

		protected override string ExpectedEditorName => "DefaultEPaymentReferenceRegistryItemEditor";

		protected override DefaultEPaymentReferenceRegistryDataType GetNewDataType() => new DefaultEPaymentReferenceRegistryDataType();
	}
}
