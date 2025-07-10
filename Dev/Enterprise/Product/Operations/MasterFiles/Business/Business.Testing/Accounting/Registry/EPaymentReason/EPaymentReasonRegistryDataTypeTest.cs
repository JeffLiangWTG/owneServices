using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EPaymentReasonRegistryDataType))]
	sealed class EPaymentReasonRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EPaymentReasonRegistryDataType>
	{
		protected override string ExpectedEditorName => "EPaymentReasonsRegistryItemEditor";

		protected override EPaymentReasonRegistryDataType GetNewDataType() => new EPaymentReasonRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var reasonCollection1 = new EPaymentReasonCollection();
			var reason1InCollection1 = reasonCollection1.AddNew();
			reason1InCollection1.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			reason1InCollection1.ReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
			reason1InCollection1.ReasonDescription = EPaymentReasonCodes.OFXReasonCodes.CodesList.GetDescriptionFromCode(EPaymentReasonCodes.OFXReasonCodes.AccountingServices);
			var reason2InCollection1 = reasonCollection1.AddNew();
			reason2InCollection1.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			reason2InCollection1.ReasonCode = EPaymentReasonCodes.OFXReasonCodes.EmployeePaymentSalaryWages;
			reason2InCollection1.ReasonDescription = EPaymentReasonCodes.OFXReasonCodes.CodesList.GetDescriptionFromCode(EPaymentReasonCodes.OFXReasonCodes.EmployeePaymentSalaryWages);

			var reasonCollection2 = new EPaymentReasonCollection();
			var reason1InCollection2 = reasonCollection2.AddNew();
			reason1InCollection2.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			reason1InCollection2.ReasonCode = EPaymentReasonCodes.OFXReasonCodes.HardwareConsultancyImplementation;
			reason1InCollection2.ReasonDescription = EPaymentReasonCodes.OFXReasonCodes.CodesList.GetDescriptionFromCode(EPaymentReasonCodes.OFXReasonCodes.HardwareConsultancyImplementation);
			var reason2InCollection2 = reasonCollection2.AddNew();
			reason2InCollection2.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			reason2InCollection2.ReasonCode = EPaymentReasonCodes.OFXReasonCodes.GoodsPaymentPurchase;
			reason2InCollection2.ReasonDescription = EPaymentReasonCodes.OFXReasonCodes.CodesList.GetDescriptionFromCode(EPaymentReasonCodes.OFXReasonCodes.GoodsPaymentPurchase);

			var byteArray1 = DataType.Serialise(reasonCollection1);
			var byteArray2 = DataType.Serialise(reasonCollection2);

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(reasonCollection1, byteArray1)
				, new ValidSampleAndBinaryValueInDB(reasonCollection2, byteArray2)
			};
		}
	}
}
