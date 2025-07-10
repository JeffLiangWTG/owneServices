
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class BrokerPaymentTypeCodeDescriptionPairList : CodeDescriptionPairList,
		Integration.Customs.US.IBrokerPaymentTypeCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public BrokerPaymentTypeCodeDescriptionPairList()
		{
			AddPair(PaymentTypeList.Codes.IndividualBasis, PaymentTypeList.Descriptions.IndividualBasis);
			AddPair(PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, PaymentTypeList.Descriptions.BatchedByDailyPrintDateAndFilerCode);
			AddPair(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter, PaymentTypeList.Descriptions.BatchedByDailyPrintDateAndImporter);
			AddPair(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporterWithSuffixes, PaymentTypeList.Descriptions.BatchedByDailyPrintDateAndImporterWithSuffixes);
			AddPair(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate, PaymentTypeList.Descriptions.BatchedByPeriodicPrintDateAndFilerDate);
			AddPair(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter, PaymentTypeList.Descriptions.BatchedByPeriodicPrintDateAndImporter);
			AddPair(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes, PaymentTypeList.Descriptions.BatchedByPeriodicPrintDateAndImporterWithSuffixes);
		}

		#region ICodeDescriptionPairListProvider Members

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return this;
		}

		#endregion
	}
}
