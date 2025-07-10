using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.ZA.Business.MessageManagers.Testing;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;

namespace Enterprise.Customs.ZA.Business.Report.Testing
{
	sealed class Report_ZACustomsEntryPayment : ReportFunctionalTestCase
	{
		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				// How to prepare this. 
				// Profile CW1 in SQLProfiler - include RPC:Completed event. 
				// Run the report in the GUI, selecting all the available columns.
				// Grab this hit fromt he profiler, and grab all the columns names from the select list.
				// User notepad++ to wrpap the names in quotes.
				// Then divvy up the list into data types. 

				var allCols = new List<ReportSchemaColumn>();
				foreach (var stringCol in new string[] { "JOBNUMBER", "IMPORTER", "CUSTOMSOFFICE", "FINANCIALACCOUNTNUMBER", "CPCCODE", "ENTRYDESCRIPTION", "LRNNUMBER", "MRNNUMBER",
															"MASTERTRANSPORTDOCUMENTNUMBER", "HOUSETRANSPORTDOCUMENTNUMBER" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(string), stringCol));
				}

				foreach (var dateCol in new string[] { "ACCOUNTINGDATE" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(DateTime), dateCol));
				}

				foreach (var intCol in new string[] { "DUTYDEFERRED", "VATDEFERRED", "TOTALDEFERRED", "DUTYCASH", "VATCASH", "OTHERCASH", "TOTALCASH", "GRANDTOTAL" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(decimal), intCol));
				}

				foreach (var guidCol in new string[] { "IMPORTERPK", })
				{
					allCols.Add(new ReportSchemaColumn(typeof(Guid), guidCol));
				}

				return allCols;
			}
		}

		protected override List<string> ParametersValuesList
		{
			get { return CustomsEntryPaymentReportTestHelper.ParametersValuesList; }
		}

		protected override void PrepareTestData()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("6");
			Factory.Save();

			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			var testInst1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst1.CEI_Style = "11";
			var testInvHeader = declaration.Invoices.AddNew();
			var testInvLine1 = testInvHeader.InvoiceLines.AddNew();
			testInvLine1.JI_CEI = testInst1.PK;
			testInvLine1.JI_Procedure = testInvLine1.EntryInstruction.CEI_Style + "00";
			Factory.Save();
			new LineMerger(declaration).DoMerge();

			var entry = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().First(x => x.CustomsProcedureCode == "11");
			entry.CH_Status = "AWA";
			entry.CH_BGMReference = "BGM11";

			CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.CustomsDefaultToCurrentLoginDept.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Factory.Save();
			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var notification = new MessageNotificationCollector_ForTest();
			var messageManager = new MessageManagers.MessageManager(objectParent, notification);
			notification.NextAnswer = true;
			messageManager.SendMessages();

			ZDateTime paymentDate = ZDateTime.UtcNow;
			foreach (CusEntryPayInfo payInfo in entry.EntryPayInfos)
			{
				// report will ignore these as they are Pending status
				payInfo.C9_PaymentDate = paymentDate;
				payInfo.C9_PaymentAmount = 5;
				payInfo.C9_PaymentParty = "V";
				payInfo.C9_TransactionType = "DTY";
				paymentDate = paymentDate.AddDays(-1);
			}

			entry.EntryPayInfos[3].C9_PaymentStatus = "CR"; // The report will return this row
		}

		protected override SqlObjectType SqlObjectType
		{
			get { return Enterprise.ReportTesting.SqlObjectType.FunctionTable; }
		}

		protected override ZString ObjectName
		{
			get { return "Report_ZACustomsEntryPayment"; }
		}

		protected override void AssertTestResults(System.Data.DataTable results)
		{
			AssertEquals("We should only find 1 row because only 1 payment info is not pending (Payment Status = 'CR'", 1, results.Rows.Count);
		}
	}
}
