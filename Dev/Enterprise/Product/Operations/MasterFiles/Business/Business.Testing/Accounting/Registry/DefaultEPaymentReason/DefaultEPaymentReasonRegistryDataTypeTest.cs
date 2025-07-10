using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DefaultEPaymentReasonRegistryDataType))]
	sealed class DefaultEPaymentReasonRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DefaultEPaymentReasonRegistryDataType>
	{
		protected override string ExpectedEditorName => "DefaultEPaymentReasonRegistryItemEditor";

		protected override DefaultEPaymentReasonRegistryDataType GetNewDataType() => new DefaultEPaymentReasonRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var reasonCollection1 = new DefaultEPaymentReasonCollection();
			var reason1InCollection1 = reasonCollection1.AddNew();
			reason1InCollection1.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			reason1InCollection1.ReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
			reason1InCollection1.ReasonDescription = EPaymentReasonCodes.OFXReasonCodes.CodesList.GetDescriptionFromCode(EPaymentReasonCodes.OFXReasonCodes.ServicesTrade);
			var byteArray1 = DataType.Serialise(reasonCollection1);

			var reasonCollection2 = new DefaultEPaymentReasonCollection();
			var reason1InCollection2 = reasonCollection2.AddNew();
			reason1InCollection2.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			reason1InCollection2.ReasonCode = EPaymentReasonCodes.OFXReasonCodes.HardwareConsultancyImplementation;
			reason1InCollection2.ReasonDescription = EPaymentReasonCodes.OFXReasonCodes.CodesList.GetDescriptionFromCode(EPaymentReasonCodes.OFXReasonCodes.HardwareConsultancyImplementation);
			var byteArray2 = DataType.Serialise(reasonCollection2);

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(reasonCollection1, byteArray1),
				new ValidSampleAndBinaryValueInDB(reasonCollection2, byteArray2)
			};
		}
	}
}
