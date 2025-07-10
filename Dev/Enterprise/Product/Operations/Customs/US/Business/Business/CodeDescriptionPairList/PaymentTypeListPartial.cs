
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	partial class PaymentTypeList
	{
		public static bool IsPaidByBroker(string code)
		{
			return code == Codes.BatchedByPeriodicPrintDateAndFilerDate ||
				code == Codes.BatchedByDailyPrintDateAndFilerCode;
		}

		public static bool IsPaidByImporter(string code)
		{
			return code == Codes.BatchedByDailyPrintDateAndImporter ||
				code == Codes.BatchedByDailyPrintDateAndImporterWithSuffixes ||
				code == Codes.BatchedByPeriodicPrintDateAndImporter ||
				code == Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes;
		}

		public static bool IsPeriodicPayment(string code)
		{
			return code == Codes.BatchedByPeriodicPrintDateAndFilerDate ||
				code == Codes.BatchedByPeriodicPrintDateAndImporter ||
				code == Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes;
		}

		public static bool IsDailyPayment(string code)
		{
			return code == Codes.BatchedByDailyPrintDateAndFilerCode ||
				code == Codes.BatchedByDailyPrintDateAndImporter ||
				code == Codes.BatchedByDailyPrintDateAndImporterWithSuffixes;
		}

		public static bool DoesPaymentTypeRequireAccountNo(string code)
		{
			return code == Codes.BatchedByDailyPrintDateAndImporter ||
				code == Codes.BatchedByPeriodicPrintDateAndImporter;
		}

		public static PaymentTypeList GetCachedReconPaymentTypeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("US_RecondPaymentTypeList", delegate
			{ return GetNewReconPaymentTypeList(); });
		}

		public static PaymentTypeList GetNewReconPaymentTypeList()
		{
			PaymentTypeList result = new PaymentTypeList();
			result.RemoveCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate);
			result.RemoveCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter);
			result.RemoveCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes);
			return result;
		}
	}
}
