using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using DataExportBatchSubTypes = Enterprise.Core.Constants.DataExportBatchSubTypes;
using DataExportBatchTypes = Enterprise.Core.Constants.DataExportBatchTypes;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GenExportBatchSequence))]
	sealed class GenExportBatchSequenceTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCanDelete()
		{
			Assert(!GetNewBusinessObject().CanDelete);
		}

		#region Properties

		public void TestType()
		{
			var batch = Factory.New<GenExportBatchSequence>();
			batch.XB_Type = DataExportBatchSubTypes.Codes.AccountingTransactionHeaderExport;
			AssertEquals("SubType HEX", DataExportBatchTypes.Codes.LegacyTransactionXML, batch.Type);

			batch.XB_Type = DataExportBatchSubTypes.Codes.AccountingTransactionPostLineExport;
			AssertEquals("SubType LEX", DataExportBatchTypes.Codes.LegacyTransactionXML, batch.Type);

			batch.XB_Type = DataExportBatchSubTypes.Codes.AccountingTransactionReverseLineExport;
			AssertEquals("SubType LRX", DataExportBatchTypes.Codes.LegacyTransactionXML, batch.Type);

			batch.XB_Type = DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServicePost;
			AssertEquals("SubType APS", DataExportBatchTypes.Codes.UniversalTransactionXML, batch.Type);

			batch.XB_Type = DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServiceReverse;
			AssertEquals("SubType ARV", DataExportBatchTypes.Codes.UniversalTransactionXML, batch.Type);

			batch.XB_Type = DataExportBatchSubTypes.Codes.GeneralLedgerPost;
			AssertEquals("SubType GPS", DataExportBatchTypes.Codes.GLTransactionCSV, batch.Type);

			batch.XB_Type = DataExportBatchSubTypes.Codes.GeneralLedgerReverse;
			AssertEquals("SubType GPV", DataExportBatchTypes.Codes.GLTransactionCSV, batch.Type);

			batch.XB_Type = DataExportBatchSubTypes.Codes.GLConsolidationsEliminationNotRequiredPosting;
			AssertEquals("SubType ENP", DataExportBatchTypes.Codes.GLConsolidations, batch.Type);

			batch.XB_Type = DataExportBatchSubTypes.Codes.GLConsolidationsEliminationNotRequiredReversal;
			AssertEquals("SubType ENR", DataExportBatchTypes.Codes.GLConsolidations, batch.Type);

			batch.XB_Type = DataExportBatchSubTypes.Codes.GLConsolidationsEliminationRequiredPosting;
			AssertEquals("SubType ERP", DataExportBatchTypes.Codes.GLConsolidations, batch.Type);

			batch.XB_Type = DataExportBatchSubTypes.Codes.GLConsolidationsEliminationRequiredReversal;
			AssertEquals("SubType ERR", DataExportBatchTypes.Codes.GLConsolidations, batch.Type);

			batch.XB_Type = DataExportBatchSubTypes.Codes.PositivePayFile;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("SubType PPF [non-US]", DataExportBatchTypes.Codes.ExportChequePayments, batch.Type);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				AssertEquals("SubType PPF [US]", DataExportBatchTypes.Codes.PositivePay, batch.Type);
			}
		}

		public void TestTypeDescription()
		{
			var batch = Factory.New<GenExportBatchSequence>();
			batch.XB_Type = DataExportBatchSubTypes.Codes.AccountingTransactionHeaderExport;
			AssertEquals("SubType HEX", DataExportBatchTypes.Descriptions.LegacyTransactionXML, batch.TypeDescription);

			batch.XB_Type = DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServicePost;
			AssertEquals("SubType APS", DataExportBatchTypes.Descriptions.UniversalTransactionXML, batch.TypeDescription);

			batch.XB_Type = DataExportBatchSubTypes.Codes.GeneralLedgerPost;
			AssertEquals("SubType GPS", DataExportBatchTypes.Descriptions.GLTransactionCSV, batch.TypeDescription);

			batch.XB_Type = DataExportBatchSubTypes.Codes.PositivePayFile;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("SubType PPF [non-US]", DataExportBatchTypes.Descriptions.ExportChequePayments, batch.TypeDescription);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				AssertEquals("SubType PPF [US]", DataExportBatchTypes.Descriptions.PositivePay, batch.TypeDescription);
			}
		}

		public void TestSubType()
		{
			var batch = Factory.New<GenExportBatchSequence>();
			batch.XB_Type = "WOW";
			AssertEquals("SubType is not in DataExportBatchSubTypes", "", batch.SubType);
			batch.XB_Type = DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServicePost;
			AssertEquals("SubType is in DataExportBatchSubTypes", DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServicePost, batch.SubType);
		}

		public void TestSubTypeDescription()
		{
			var batch = Factory.New<GenExportBatchSequence>();
			batch.XB_Type = DataExportBatchSubTypes.Codes.GeneralLedgerPost;
			AssertEquals("SubType GPS", DataExportBatchSubTypes.Descriptions.TransactionPosted, batch.SubTypeDescription);
			batch.XB_Type = DataExportBatchSubTypes.Codes.GeneralLedgerReverse;
			AssertEquals("SubType GRV", DataExportBatchSubTypes.Descriptions.TransactionRecognizedReversal, batch.SubTypeDescription);
		}

		public void TestBatchNumber()
		{
			var batch = Factory.New<GenExportBatchSequence>();
			batch.XB_BatchNumber = 5;
			AssertEquals(5, batch.BatchNumber);
		}

		#endregion
	}
}
