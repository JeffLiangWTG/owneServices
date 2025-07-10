using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using DataExportBatchSubTypes = Enterprise.Core.Constants.DataExportBatchSubTypes;
using DataExportBatchTypes = Enterprise.Core.Constants.DataExportBatchTypes;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GenExportBatchSequenceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDataExportBatchTypeList()
		{
			AssertEquals("DataExportBatchTypeList should contain 'LegacyTransactionXML'", true, Lookups.DataExportBatchTypeList.ContainsCode(DataExportBatchTypes.Codes.LegacyTransactionXML));
			AssertEquals("DataExportBatchTypeList should contain 'UniversalTransactionXML'", true, Lookups.DataExportBatchTypeList.ContainsCode(DataExportBatchTypes.Codes.UniversalTransactionXML));
			AssertEquals("DataExportBatchTypeList should contain 'GLTransactionCSV'", true, Lookups.DataExportBatchTypeList.ContainsCode(DataExportBatchTypes.Codes.GLTransactionCSV));
			AssertEquals("DataExportBatchTypeList should contain 'ExportCheckPayments'", true, Lookups.DataExportBatchTypeList.ContainsCode(DataExportBatchTypes.Codes.ExportChequePayments));
			AssertEquals("DataExportBatchTypeList should contain 'PositivePay'", true, Lookups.DataExportBatchTypeList.ContainsCode(DataExportBatchTypes.Codes.PositivePay));
			AssertEquals("DataExportBatchTypeList should contain 'GLConsolidations'", true, Lookups.DataExportBatchTypeList.ContainsCode(DataExportBatchTypes.Codes.GLConsolidations));
		}

		public void TestDataExportBatchSubTypeList()
		{
			AssertEquals("DataExportBatchSubTypeList should contain 'AccountingTransactionHeaderExport'", true, Lookups.DataExportBatchSubTypeList.ContainsCode(DataExportBatchSubTypes.Codes.AccountingTransactionHeaderExport));
			AssertEquals("DataExportBatchSubTypeList should contain 'GeneralLedgerPost'", true, Lookups.DataExportBatchSubTypeList.ContainsCode(DataExportBatchSubTypes.Codes.GeneralLedgerPost));
			AssertEquals("DataExportBatchSubTypeList should contain 'GeneralLedgerReverse'", true, Lookups.DataExportBatchSubTypeList.ContainsCode(DataExportBatchSubTypes.Codes.GeneralLedgerReverse));
			AssertEquals("DataExportBatchSubTypeList should contain 'AccountingTransactionPostLineExport'", true, Lookups.DataExportBatchSubTypeList.ContainsCode(DataExportBatchSubTypes.Codes.AccountingTransactionPostLineExport));
			AssertEquals("DataExportBatchSubTypeList should contain 'AccountingTransactionReverseLineExport'", true, Lookups.DataExportBatchSubTypeList.ContainsCode(DataExportBatchSubTypes.Codes.AccountingTransactionReverseLineExport));
			AssertEquals("DataExportBatchSubTypeList should contain 'AccountingTransactionExportWebServicePost'", true, Lookups.DataExportBatchSubTypeList.ContainsCode(DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServicePost));
			AssertEquals("DataExportBatchSubTypeList should contain 'AccountingTransactionExportWebServiceReverse'", true, Lookups.DataExportBatchSubTypeList.ContainsCode(DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServiceReverse));
			AssertEquals("DataExportBatchSubTypeList should contain 'PositivePayFile'", true, Lookups.DataExportBatchSubTypeList.ContainsCode(DataExportBatchSubTypes.Codes.PositivePayFile));
			AssertEquals("DataExportBatchSubTypeList should contain 'GLConsolidationsEliminationNotRequiredPosting'", true, Lookups.DataExportBatchSubTypeList.ContainsCode(DataExportBatchSubTypes.Codes.GLConsolidationsEliminationNotRequiredPosting));
			AssertEquals("DataExportBatchSubTypeList should contain 'GLConsolidationsEliminationNotRequiredReversal'", true, Lookups.DataExportBatchSubTypeList.ContainsCode(DataExportBatchSubTypes.Codes.GLConsolidationsEliminationNotRequiredReversal));
			AssertEquals("DataExportBatchSubTypeList should contain 'GLConsolidationsEliminationRequiredPosting'", true, Lookups.DataExportBatchSubTypeList.ContainsCode(DataExportBatchSubTypes.Codes.GLConsolidationsEliminationRequiredPosting));
			AssertEquals("DataExportBatchSubTypeList should contain 'GLConsolidationsEliminationRequiredReversal'", true, Lookups.DataExportBatchSubTypeList.ContainsCode(DataExportBatchSubTypes.Codes.GLConsolidationsEliminationRequiredReversal));

			AssertDescription(DataExportBatchSubTypes.Codes.AccountingTransactionHeaderExport, DataExportBatchSubTypes.Descriptions.TransactionPosted);
			AssertDescription(DataExportBatchSubTypes.Codes.AccountingTransactionPostLineExport, DataExportBatchSubTypes.Descriptions.TransactionPosted);
			AssertDescription(DataExportBatchSubTypes.Codes.AccountingTransactionReverseLineExport, DataExportBatchSubTypes.Descriptions.TransactionReversed);
			AssertDescription(DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServicePost, DataExportBatchSubTypes.Descriptions.TransactionPosted);
			AssertDescription(DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServiceReverse, DataExportBatchSubTypes.Descriptions.TransactionRecognizedReversal);
			AssertDescription(DataExportBatchSubTypes.Codes.GeneralLedgerPost, DataExportBatchSubTypes.Descriptions.TransactionPosted);
			AssertDescription(DataExportBatchSubTypes.Codes.GeneralLedgerReverse, DataExportBatchSubTypes.Descriptions.TransactionRecognizedReversal);
			AssertDescription(DataExportBatchSubTypes.Codes.PositivePayFile, DataExportBatchSubTypes.Descriptions.Payment);
			AssertDescription(DataExportBatchSubTypes.Codes.GLConsolidationsEliminationNotRequiredPosting, DataExportBatchSubTypes.Descriptions.EliminationNotRequiredPosting);
			AssertDescription(DataExportBatchSubTypes.Codes.GLConsolidationsEliminationNotRequiredReversal, DataExportBatchSubTypes.Descriptions.EliminationNotRequiredReversal);
			AssertDescription(DataExportBatchSubTypes.Codes.GLConsolidationsEliminationRequiredPosting, DataExportBatchSubTypes.Descriptions.EliminationRequiredPosting);
			AssertDescription(DataExportBatchSubTypes.Codes.GLConsolidationsEliminationRequiredReversal, DataExportBatchSubTypes.Descriptions.EliminationRequiredReversal);
		}

		void AssertDescription(string code, MultilingualString description)
		{
			AssertEquals(string.Format("{0}'s Description", code), description, Lookups.DataExportBatchSubTypeList.GetDescriptionFromCode(code));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var batch = Factory.New<GenExportBatchSequence>();
			Lookups = batch.Lookups;
		}
		GenExportBatchSequenceLookups Lookups;

		#endregion
	}
}
