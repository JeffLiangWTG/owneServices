//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGenExportBatchSequenceLookups
//
//    This class should be used for overriding collections in AutoGenExportBatchSequenceLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;
using DataExportBatchSubTypes = Enterprise.Core.Constants.DataExportBatchSubTypes;
using DataExportBatchTypes = Enterprise.Core.Constants.DataExportBatchTypes;

namespace Enterprise.MasterFiles.Business
{
	public class GenExportBatchSequenceLookups : AutoGenExportBatchSequenceLookups
	{
		public GenExportBatchSequenceLookups(AutoGenExportBatchSequence parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList DataExportBatchTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(DataExportBatchTypes.Codes.LegacyTransactionXML, DataExportBatchTypes.Descriptions.LegacyTransactionXML);
				result.AddPair(DataExportBatchTypes.Codes.UniversalTransactionXML, DataExportBatchTypes.Descriptions.UniversalTransactionXML);
				result.AddPair(DataExportBatchTypes.Codes.GLTransactionCSV, DataExportBatchTypes.Descriptions.GLTransactionCSV);
				result.AddPair(DataExportBatchTypes.Codes.ExportChequePayments, DataExportBatchTypes.Descriptions.ExportChequePayments);
				result.AddPair(DataExportBatchTypes.Codes.PositivePay, DataExportBatchTypes.Descriptions.PositivePay);
				result.AddPair(DataExportBatchTypes.Codes.GLConsolidations, DataExportBatchTypes.Descriptions.GLConsolidations);
				return result;
			}
		}

		public CodeDescriptionPairList DataExportBatchSubTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(DataExportBatchSubTypes.Codes.AccountingTransactionHeaderExport, DataExportBatchSubTypes.Descriptions.TransactionPosted);
				result.AddPair(DataExportBatchSubTypes.Codes.AccountingTransactionPostLineExport, DataExportBatchSubTypes.Descriptions.TransactionPosted);
				result.AddPair(DataExportBatchSubTypes.Codes.AccountingTransactionReverseLineExport, DataExportBatchSubTypes.Descriptions.TransactionReversed);
				result.AddPair(DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServicePost, DataExportBatchSubTypes.Descriptions.TransactionPosted);
				result.AddPair(DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServiceReverse, DataExportBatchSubTypes.Descriptions.TransactionRecognizedReversal);
				result.AddPair(DataExportBatchSubTypes.Codes.GeneralLedgerPost, DataExportBatchSubTypes.Descriptions.TransactionPosted);
				result.AddPair(DataExportBatchSubTypes.Codes.GeneralLedgerReverse, DataExportBatchSubTypes.Descriptions.TransactionRecognizedReversal);
				result.AddPair(DataExportBatchSubTypes.Codes.PositivePayFile, DataExportBatchSubTypes.Descriptions.Payment);
				result.AddPair(DataExportBatchSubTypes.Codes.GLConsolidationsEliminationNotRequiredPosting, DataExportBatchSubTypes.Descriptions.EliminationNotRequiredPosting);
				result.AddPair(DataExportBatchSubTypes.Codes.GLConsolidationsEliminationNotRequiredReversal, DataExportBatchSubTypes.Descriptions.EliminationNotRequiredReversal);
				result.AddPair(DataExportBatchSubTypes.Codes.GLConsolidationsEliminationRequiredPosting, DataExportBatchSubTypes.Descriptions.EliminationRequiredPosting);
				result.AddPair(DataExportBatchSubTypes.Codes.GLConsolidationsEliminationRequiredReversal, DataExportBatchSubTypes.Descriptions.EliminationRequiredReversal);
				return result;
			}
		}
	}
}
