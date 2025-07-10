using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ReportTesting
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class TemplateNameAttribute : Attribute
	{
		public TemplateNameAttribute(string templateName)
		{
			this.templateName = templateName;
		}

		readonly string templateName;
		public string TemplateName => templateName;
	}

	public abstract class TemplateTestCase : TestCaseWithFactory
	{
		#region SetUp / TearDown

		protected override void SetUp()
		{
			base.SetUp();

			AssertEquals("Incorrect Number of templates returned for filter criteria " + TemplateFilter.GetAsWhereClause(true), 1, CandidateTemplates.Count);

			var template = (StmTemplate)CandidateTemplates.First();
			var excelTemplate = new ExcelTemplateReadFromStmTemplateTable(template);
			report = GetReport(excelTemplate);
		}

		protected virtual ReportForTestAllowingCacheReset GetReport(ExcelTemplate excelTemplate)
		{
			return new ReportForTestAllowingCacheReset(new DocumentPack(), excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.None);
		}

		protected override void TearDown()
		{
			if (Report != null)
			{
				Report.Dispose();
			}
			base.TearDown();
		}

		#endregion

		#region Fields and Properties

		ReportForTestAllowingCacheReset report;
		protected ReportForTestAllowingCacheReset Report => report;

		public virtual StmTemplateBaseCollection CandidateTemplates
		{
			get
			{
				if (candidateTemplates == null)
				{
					TemplateFilter.ReLoadExistingRows = true;
					candidateTemplates = new StmTemplateBaseCollection(Factory);
					candidateTemplates.Load(TemplateFilter);
				}
				return candidateTemplates;
			}
		}
		StmTemplateBaseCollection candidateTemplates;

		public virtual string TemplateLocation => TemplateFolder + TemplateName + TemplateFileType;

		protected virtual string TemplateFileType => @".xls";

		public virtual string TemplateFolder => @"Enterprise\Product\Documents\ExcelTemplates\Reports\";

		ZQuery TemplateFilter
		{
			get
			{
				if (templateFilter == null)
				{
					templateFilter = new ZQuery();
					templateFilter.AddToFilter(StmTemplateSchema.SO_Name, TemplateName);
					templateFilter.AddToFilter(StmTemplateSchema.SO_DataContext, "NONE");
					templateFilter.AddToFilter(AdditionalTemplateFilters);
				}
				return templateFilter;
			}
		}
		ZQuery templateFilter;

		public string TemplateName
		{
			get
			{
				var templateName = (TemplateNameAttribute)GetType().GetCustomAttributes(typeof(TemplateNameAttribute), false)[0];
				return templateName.TemplateName;
			}
		}

		protected virtual ZQuery AdditionalTemplateFilters => null;

		public virtual bool ReportUsesGeneratedSQL => false;

		bool SortOrderAsParameterTestRequired => ReportPassesSortOrderAsParameter && ReportUsesGeneratedSQL;

		public virtual bool ReportPassesSortOrderAsParameter => false;

		protected virtual bool ReportRequiresColumnHeadings => true;

		protected virtual IEnumerable<string> SheetsNotRequiringColumnHeadings => Array.Empty<string>();

		#endregion

		#region TestHelperMethods

		void SetupData()
		{
			var accTaxMsg = Factory.NewWithValidTestData<AccInvMsg>();
			accTaxMsg.A9_Code = "CDE";
			accTaxMsg.A9_Description = "Description";

			Factory.Save();
		}

		protected void ForEachReportTemplate<T>(Func<ExcelWorkSheet, T> worksheetTransform, Action<T> worksheetAction)
		{
			ForEachReportTemplate(worksheetTransform, (transformResult, sheetName) => worksheetAction(transformResult));
		}

		protected void ForEachReportTemplate<T>(Func<ExcelWorkSheet, T> worksheetTransform, Action<T, string> worksheetAction)
		{
			foreach (ExcelWorkSheet worksheet in Report.XlInterface.WorkSheets.Where(sheet => Enterprise.DocumentEngine.Report.IsTemplateSheet(sheet.SheetName)))
			{
				worksheetAction(worksheetTransform(worksheet), worksheet.SheetName);
			}
		}

		protected void ForEachReportTemplateRequiringColumnHeadings<T>(Func<ExcelWorkSheet, T> worksheetTransform, Action<T> requiresColumnHeadingsAction, Action<T> noColumnHeadingsAction)
		{
			ForEachReportTemplate(worksheetTransform, (transformResult, sheetName) =>
			{
				if (ReportRequiresColumnHeadings && !SheetsNotRequiringColumnHeadings.Contains(sheetName))
				{
					requiresColumnHeadingsAction(transformResult);
				}
				else
				{
					noColumnHeadingsAction(transformResult);
				}
			});
		}

		protected void ResetCandidateTemplates()
		{
			candidateTemplates = null;
		}

		protected virtual void PrepareReportForRender()
		{
			Report.PrepareForRender();
		}

		protected void SetMultipleChoiceFilterValue(string filterDisplayName, string filterValue)
		{
			var filterField = Report.FilterCollection[filterDisplayName];
			if (filterField != null)
			{
				((IValueAsStringProviderForUnitTests)filterField).ValueAsStringForSerialisation = filterValue;
			}
			else
			{
				throw new IndexOutOfRangeException(filterDisplayName + " does not exist");
			}
		}

		protected virtual void FillReportWithDefaultValues()
		{
			// There's an extra ToArray() here because calling OfType on just FilterCollection does weird things and returns 0 results
			var filterFields = Report.FilterCollection
				.ToArray()
				.OfType<FilterFieldWithUTSupport>()
				.Where(field => ((IFilterFieldForUT)field).IsRequired);

			foreach (var field in filterFields)
			{
				field.FillWithValidTestData();
				field.Factory.Save();
			}
		}

		/// <summary>
		/// Define filters values that will not be cleared by ClearAllNonDefaultFilterValues.
		/// Use this to limit amount of report data when TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter gets run
		/// </summary>
		protected virtual void ApplyNonClearableFiltersValues()
		{
		}

		protected void ClearAllNonDefaultFilterValues()
		{
			foreach (IFilter filterField in Report.FilterCollection)
			{
				var filter = filterField as FilterFieldWithUTSupport;
				if (filter != null && !((IFilterFieldForUT)filter).IsRequired)
				{
					filter.ClearValueForUnitTest();
				}
			}

			ApplyNonClearableFiltersValues();
		}

		protected virtual void SelectAllOptionalTemplates()
		{
			foreach (OptionalTemplateSheet sheet in Report.OptionalTemplateSheetCollection)
			{
				sheet.Selected = true;
			}
		}

		protected virtual void RunReport()
		{
			using (var stream = new MemoryStream())
			using (Report.SuspendFilterValidationCheckingForTesting())
			{
				try
				{
					Report.Save(stream);
				}
				catch (Exception ex)
				{
					Assert($"Error processing : {CandidateTemplates[0].ExcelTemplateFullPath} {System.Environment.NewLine} {ex}", false);
				}
			}
		}

		protected void AssertWorkSheet0(string expectedOutput, string message = default)
		{
			using (var stream = new MemoryStream())
			{
				Report.Save(stream);
				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(stream);
					AssertMultilineASCIIEquals
					(
						message,
						expectedOutput,
						excelInterface.WorkSheets[0].ToString()
					);
				}
			}
		}

		void DeselectAllSortOrders()
		{
			foreach (SortOrder sortOrder in Report.SortOrderCollection)
			{
				sortOrder.Selected = false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1058:DoNotUseDebuggerIsAttached", Justification = "Baseline")]
		void RunReportForEachNonRequiredFilter(int skipCount, int takeCount, IEnumerable<SortOrder> sortOrdersToTest)
		{
			if (skipCount > Report.FilterCollection.Count)
			{
				Assert($"FilterCollection does not have items {skipCount}..{skipCount + takeCount}: nothing to do.", true);
				return;
			}

			// WARNING, DANGER: using time in a unit test!
			var overallThresholdSeconds = 60.0 * 3;     // DAT will fail tests that take longer than 3-15 minutes  (see https://stackoverflow.com/c/wisetechglobal/a/1225/138).
			var singleReportThresholdSeconds = 10.0;
			var lastItem = Math.Min(skipCount + takeCount, Report.FilterCollection.Count);
			var datDataClient = new WTG.DevTools.Common.TestFailureDataClient();

			var sb = new StringBuilder();
			sb.AppendLine("FilterCollection.Count = " + Report.FilterCollection.Count);
			sb.AppendLine("SortOrdersToTest.Count = " + sortOrdersToTest.Count());
			if (!sortOrdersToTest.Any())
			{
				sortOrdersToTest = new SortOrder[] { null };
			}

			var overallSw = System.Diagnostics.Stopwatch.StartNew();
			for (var i = skipCount; i < lastItem; i++)
			{
				var filter = (FilterField)Report.FilterCollection[i];
				foreach (var sortOrder in sortOrdersToTest)
				{
					if (sortOrder != null)
					{
						DeselectAllSortOrders();
						sortOrder.Selected = true;
					}

					var sw = System.Diagnostics.Stopwatch.StartNew();
					ClearAllNonDefaultFilterValues();
					if (!((IFilterFieldForUT)filter).IsRequired)
					{
						filter.FillWithValidTestData();
						RunReport();
						Report.ResetCachedExcelFileForTesting();
					}
					sw.Stop();

					LogReportDurationAndQueryPlanIfSlow();
					FailTestIfTooSlowAndNotLastIteration();

					#region Local Helpers

					void LogReportDurationAndQueryPlanIfSlow()
					{
						var sortOrderPart = $"(SortOrder: {(sortOrder == null ? "<none>" : sortOrder.DisplayName)})";
						sb.AppendFormat("#{0} {1} ({2}) {3}: {4:N2}s", i, filter.FieldName, filter.DisplayName, sortOrderPart, sw.Elapsed.TotalSeconds);
						if (sw.Elapsed.TotalSeconds > singleReportThresholdSeconds)
						{
							if (Report.stmReportRun != null)
							{
								if (TestingState.IsRunningOnDAT && !System.Diagnostics.Debugger.IsAttached)
								{
									if (!Report.stmReportRun.RRI_QueryText.IsEmpty)
									{
										var queryUrl = datDataClient.Upload(new MemoryStream(Encoding.UTF8.GetBytes(Report.stmReportRun.RRI_QueryText)), "text/plain", ".sql").GetAwaiter().GetResult();
										sb.AppendFormat(" ∙ <a href=\"{0}\">Query Text.sql</a>", queryUrl);
									}
									if (!Report.stmReportRun.RRI_ExecutionPlanText.IsEmpty)
									{
										var queryUrl = datDataClient.Upload(new MemoryStream(Encoding.UTF8.GetBytes(Report.stmReportRun.RRI_ExecutionPlanText)), "text/plain", ".sqlplan").GetAwaiter().GetResult();
										sb.AppendFormat(" ∙ <a href=\"{0}\">Execution Plan.sql</a>", queryUrl);
									}
								}
								else if (!Report.stmReportRun.RRI_QueryText.IsEmpty || !Report.stmReportRun.RRI_ExecutionPlanText.IsEmpty)
								{
									sb.Append(" ∙ Attach a debugger to access query text and execution plan.");
								}
							}

							Report.stmReportRun = Factory.New<StmReportRun>();
						}
						sb.AppendLine();
					}

					void FailTestIfTooSlowAndNotLastIteration()
					{
						var isNotLastIteration = i < lastItem;
						if (isNotLastIteration && overallSw.Elapsed.TotalSeconds > overallThresholdSeconds)
						{
							// We need to fail with an assertion to get useful information in a DAT failure; a timeout just gives us a stack trace.
							Assert($"Test ran for excessive time ({overallSw.Elapsed.TotalSeconds:N1}s). See below for log of each filter attempted\n\n{sb.ToString()}", false);
						}
					}

					#endregion
				}
			}
		}

		#endregion

		#region Tests

		public void TestRunReportWithNoErrors()
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			var error = Report.ErrorManager.HasErrors ? string.Join(System.Environment.NewLine, ((IHaveReportProcessingErrorsForGUI)Report.ErrorManager).GetErrors().Select(e => e.Message)) : "";
			Assert(string.Format(CultureInfo.InvariantCulture, "There should be no errors in the report, but met the following:{0}Template path:{1}", System.Environment.NewLine + error + System.Environment.NewLine, TemplateLocation), string.IsNullOrEmpty(error));
		}

		[ExpectNoExceptions]
		[StressTest]
		[TestDate(2006, 12, 25)]
		public void TestReportRunsWithNoException()
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			FillReportWithDefaultValues();
			if (!Report.ColumnHeadingManager.CurrentConfiguration.Worksheets.IsEmpty)
			{
				var headings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
				foreach (ColumnHeading heading in headings)
				{
					heading.Hidden = false;
				}
			}
			RunReport();
		}

		[ExpectNoExceptions]
		[TestDate(2006, 12, 25)]
		public void TestEnsureAllOptionalTemplatesRunWithAllFiltersSpecified()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();
			SelectAllOptionalTemplates();
			foreach (var filter in Report.FilterCollection.OfType<FilterField>())
			{
				filter.FillWithValidTestData();
			}
			RunReport();
		}

		protected void TestEnsureAcceptableCompileMemory(long compileMemory)
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			FillReportWithDefaultValues();

			var captureQueryPlan = true;
			Report.stmReportRun = Factory.New<StmReportRun>();

			using (RawDataRegistry.Instance.ReportStatisticsLogExecutionPlan.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, captureQueryPlan))
			{
				RunReport();
			}

			AssertNotNullOrEmpty(nameof(report.stmReportRun.RRI_ExecutionPlanText), report.stmReportRun.RRI_ExecutionPlanText);

			var analyzer = new QueryPlanalyzer(report.stmReportRun.RRI_ExecutionPlanText);

			AssertGreaterThan($"CompileMemory cost for '{Report.Name}' should be parsed correctly.", analyzer.QueryPlan.CompileMemory, 0L);
			AssertLessThanOrEqualTo($"CompileMemory cost for '{Report.Name}' should be no greater than {compileMemory}", analyzer.QueryPlan.CompileMemory, compileMemory);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLatestTemplateIsLoaded()
		{
			//check actual path meets expectations
			AssertEquals("Template Path is incorrect. Either the wrong template is loaded or the template file name does not follow conventions and needs to be renamed.", TemplateLocation.ToLower(), CandidateTemplates[0].SO_ExcelTemplatePath.ToLower());
			using (var stream = AssemblyLoader.LoadAssembly("ExcelTemplates").GetManifestResourceStream("ExcelTemplates.Reports." + TemplateName + TemplateFileType))
			{
				ZBlob templateBlob;
				if (stream != null)
				{
					var buffer = new byte[stream.Length];
					stream.Read(buffer, 0, buffer.Length);
					templateBlob = new ZBlob(buffer);
				}
				else
				{
					templateBlob = StmTemplateBase.GetTemplateBlobFromFile(BaseSourcePath + TemplateLocation);
				}
				AssertEquals(TemplateName + "; Template in DB should be the same as checked in template;" + CandidateTemplates[0].SO_ExcelTemplatePath.ToString(), CandidateTemplates[0].SO_Template, templateBlob);
			}
		}

		public void TestIncorrectReportSelectUsage()
		{
			var testFailed = false;
			var incorrectReportSelectUsageMessage = "'Select *' should not be specified in the Data Source SQL for a report template: ({0}). Replace 'select *' with 'select <ReportData.SelectList>'";

			ForEachReportTemplate(
				worksheetTransform: (worksheet) =>
				{
					Report.SetWorkSheetCurrentlyBeingProcessedForTestOnly(worksheet);
					Report.AnalyzeResettingLoadedFlagsAfterwards();
					return Report.Analyser.Config.DataSourceStrings;
				},
				worksheetAction: (dataStrings) =>
				{
					var worksheetFailed = dataStrings.Any(dataSource => Regex.Match(dataSource.Replace('\r', ' ').Replace('\n', ' '), @"^(?=.*^(Data:|EDWData:)([^=]+)=(\s*select[\s\S]*([A-Za-z]+\w*\.)?\*))(?=.*\bselect\b((?!\bfrom\b).)*?\*.*?\bfrom\b).*", RegexOptions.IgnoreCase).Success);
					if (worksheetFailed)
					{
						testFailed = true;
						Assert(string.Format(CultureInfo.InvariantCulture, incorrectReportSelectUsageMessage, Report.Template.TemplateSourceLocation), temporaryExclusionsListForTestIncorrectReportSelectUsage_DO_NOT_ADD_ENTRIES_TO_THIS_LIST.Contains(GetType().FullName));
					}
				}
			);

			if (!testFailed)
			{
				Assert(string.Format(CultureInfo.InvariantCulture, "Good job on fixing your report! Please remove {0} from {1}.", GetType().FullName, nameof(temporaryExclusionsListForTestIncorrectReportSelectUsage_DO_NOT_ADD_ENTRIES_TO_THIS_LIST)), !temporaryExclusionsListForTestIncorrectReportSelectUsage_DO_NOT_ADD_ENTRIES_TO_THIS_LIST.Contains(GetType().FullName));
			}

			Assert("You've fixed the last report. Well done! Please clean up the unit test and notify a DocEngine dev.", temporaryExclusionsListForTestIncorrectReportSelectUsage_DO_NOT_ADD_ENTRIES_TO_THIS_LIST.Length > 0);
		}

		public void TestReportColumnHeadingsExistIfRequired()
		{
			var testFailed = false;
			int startingColumn;
			Report.PrepareForRender();

			ForEachReportTemplateRequiringColumnHeadings(
				worksheetTransform: (xlWorksheet) =>
				{
					var worksheet = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets.Cast<Worksheet>().FirstOrDefault(sheet => sheet.Name == xlWorksheet.SheetName);
					return worksheet == null ? Enumerable.Empty<string>() : CsvCreateDeliveryInfoStrategy.ReadColumnHeadings(Report, worksheet, out startingColumn);
				},
				requiresColumnHeadingsAction: (columns) =>
				{
					var columnHeadingsAreDefined = columns.Any();
					var allColumnHeadingsArePopulated = !columns.Any(column => string.IsNullOrWhiteSpace(column));
					if (!columnHeadingsAreDefined)
					{
						testFailed = true;
						Assert("Report should define column headings for customization and CSV/XML export", temporaryExclusionsListForTestReportColumnHeadingsExistIfRequired_DO_NOT_ADD_ENTRIES_TO_THIS_LIST.Contains(GetType().FullName));
					}
					else if (!allColumnHeadingsArePopulated)
					{
						testFailed = true;
						Assert("Report should not have empty column headings", temporaryExclusionsListForTestReportColumnHeadingsExistIfRequired_DO_NOT_ADD_ENTRIES_TO_THIS_LIST.Contains(GetType().FullName));
					}
				},
				noColumnHeadingsAction: (columns) =>
				{
					var noColumnHeadingsFailed = columns.Any();
					if (noColumnHeadingsFailed)
					{
						testFailed = true;
						Assert("Report should not define column headings as per the 'ReportRequiresColumnHeadings' test flag.", temporaryExclusionsListForTestReportColumnHeadingsExistIfRequired_DO_NOT_ADD_ENTRIES_TO_THIS_LIST.Contains(GetType().FullName));
					}
				}
			);

			if (!testFailed)
			{
				Assert(string.Format(CultureInfo.InvariantCulture, "Good job on fixing your report! Please remove {0} from {1}.", GetType().FullName, temporaryExclusionsListForTestReportColumnHeadingsExistIfRequired_DO_NOT_ADD_ENTRIES_TO_THIS_LIST), !temporaryExclusionsListForTestReportColumnHeadingsExistIfRequired_DO_NOT_ADD_ENTRIES_TO_THIS_LIST.Contains(GetType().FullName));
			}

			Assert("You've fixed the last report. Well done! Please clean up the unit test and notify a DocEngine dev.", temporaryExclusionsListForTestReportColumnHeadingsExistIfRequired_DO_NOT_ADD_ENTRIES_TO_THIS_LIST.Length > 0);
		}

		public void TestEnsureSortedReportsDontUseExecCommand()
		{
			bool testFailed = false, sortedReport = Report.XlInterface.WorkSheets.Any(sheet => sheet.SheetName.Equals("Sort", StringComparison.OrdinalIgnoreCase));
			if (sortedReport)
			{
				ForEachReportTemplate(
					worksheetTransform: (worksheet) =>
					{
						if (!testFailed)
						{
							Report.SetWorkSheetCurrentlyBeingProcessedForTestOnly(worksheet);
							Report.AnalyzeResettingLoadedFlagsAfterwards();
							return Report.Analyser.Config.DataSourceStrings;
						}
						return Enumerable.Empty<string>();
					},
					worksheetAction: (dataStrings) =>
					{
						if (!testFailed && dataStrings.All(dataString => Regex.Match(dataString, @"Data:([^=]+)=\s*EXEC\s+", RegexOptions.IgnoreCase).Success))
						{
							testFailed = true;
						}
					}
				);
			}

			if (testFailed)
			{
				Assert("Sorted Reports should not use the EXEC command in a data source. Consider using a Table-valued function instead, or removing the sort sheet if sorting is not required.", temporaryExclusionsListForTestEnsureSortedReportsDontUseExecCommand_DO_NOT_ADD_ENTRIES_TO_THIS_LIST.Contains(GetType().FullName));
			}
			else
			{
				Assert(string.Format(CultureInfo.InvariantCulture, "Good job on fixing your report! Please remove {0} from {1}.", GetType().FullName, nameof(temporaryExclusionsListForTestEnsureSortedReportsDontUseExecCommand_DO_NOT_ADD_ENTRIES_TO_THIS_LIST)), !temporaryExclusionsListForTestEnsureSortedReportsDontUseExecCommand_DO_NOT_ADD_ENTRIES_TO_THIS_LIST.Contains(GetType().FullName));
			}

			Assert("You've fixed the last report. Well done! Please clean up the unit test and notify a DocEngine dev.", temporaryExclusionsListForTestEnsureSortedReportsDontUseExecCommand_DO_NOT_ADD_ENTRIES_TO_THIS_LIST.Length > 0);
		}

		#region Temporary Exclusions Lists For Incorrect Report Design

		//DO NOT ADD ENTRIES IN THIS LIST
		static readonly string[] temporaryExclusionsListForTestIncorrectReportSelectUsage_DO_NOT_ADD_ENTRIES_TO_THIS_LIST = new[]
		{
			"Enterprise.ReportTesting.Accounting.TestAPTotalTradeBySupplierReport",
			"Enterprise.ReportTesting.Accounting.TestBankReconciliationTemplate",
			"Enterprise.ReportTesting.Accounting.TestCashFlowStatement",
			"Enterprise.ReportTesting.Accounting.TestGLProfitAndLossMulitlingualReport",
			"Enterprise.ReportTesting.Accounting.TestGLTransactionsMultilingualReport",
			"Enterprise.ReportTesting.Accounting.TestPayablesAnalysisTurnoverCreditorPeriodAnalysisTemplate",
			"Enterprise.ReportTesting.Accounting.TestPayablesOrderLineReportTemplate",
			"Enterprise.ReportTesting.Accounting.TestTaxTransactionAnalysisDetail",
			"Enterprise.ReportTesting.Accounting.TestTaxTransactionAnalysisSummary",
			"Enterprise.ReportTesting.Common.Test3rdPartySoftwareReport",
			"Enterprise.ReportTesting.Common.TestBankStatementTransactionsListingReport",
			"Enterprise.ReportTesting.Common.TestBudgetTrialBalanceReport",
			"Enterprise.ReportTesting.Common.TestGLBalanceSheetMultilingualReport",
			"Enterprise.ReportTesting.Common.TestJobSummarySelectedClientReport",
			"Enterprise.ReportTesting.Common.TestTransportMovementReport",
			"Enterprise.ReportTesting.Customs.Shared.TestLandedCostingSummary",
			"Enterprise.ReportTesting.Freight.Forwarding.TestIATASalesReport",
			"Enterprise.ReportTesting.Freight.Shared.TestLocalTransportJobProfileReport",
			"Enterprise.ReportTesting.Freight.TestCFSShipmentswithNoInvoicingJobReport",
			"Enterprise.ReportTesting.Freight.TestShipmentsForecastFreightAndCFS",
			"Enterprise.ReportTesting.MasterFiles.TestAgentReferralTemplate",
			"Enterprise.ReportTesting.MasterFiles.TestCarrierProfileReport",
			"Enterprise.ReportTesting.Recruiter.TestCertificateExaminationResultsReport",
			"Enterprise.ReportTesting.ReferenceFiles.TestEquipmentCertificatesExpiryReport",
			"Enterprise.Client.CEO.Testing.TestCEOClientOrderLinesReportRoutine"
		};

		//DO NOT ADD ENTRIES IN THIS LIST
		static readonly string[] temporaryExclusionsListForTestReportColumnHeadingsExistIfRequired_DO_NOT_ADD_ENTRIES_TO_THIS_LIST = new[]
		{
			"Enterprise.ReportTesting.Accounting.TestAnalysisLocalClientByPeriodReport",
			"Enterprise.ReportTesting.Accounting.TestAnalysisTurnoverbyDebtorandTransactionTypeReport",
			"Enterprise.ReportTesting.Accounting.TestAnalysisTurnoverbyDebtorwithPriorYearComparisonReport",
			"Enterprise.ReportTesting.Accounting.TestAPAgedOutstandingTransactionsDetailinInvoiceCurrency",
			"Enterprise.ReportTesting.Accounting.TestAPAgedOutstandingTransactionsDetailinLocalCurrency",
			"Enterprise.ReportTesting.Accounting.TestAPAgedOutstandingTransactionsSummaryinInvoiceCurrency",
			"Enterprise.ReportTesting.Accounting.TestAPAgedOutstandingTransactionsSummaryinLocalCurrency",
			"Enterprise.ReportTesting.Accounting.TestAPTotalTradeBySupplierReport",
			"Enterprise.ReportTesting.Accounting.TestARAgedOutstandingDetailInInvoiceCurrencyReport",
			"Enterprise.ReportTesting.Accounting.TestARAgedOutstandingDetailInLocalCurrencyReport",
			"Enterprise.ReportTesting.Accounting.TestARAgedOutstandingSummaryInInvoiceCurrencyReport",
			"Enterprise.ReportTesting.Accounting.TestARAgedOutstandingSummaryInLocalCurrencyReport",
			"Enterprise.ReportTesting.Accounting.TestARAPAgedSummaryInLocalCurrencyByOrganisationByBookType",
			"Enterprise.ReportTesting.Accounting.TestAUBASSummaryReport",
			"Enterprise.ReportTesting.Accounting.TestBalanceSheetPeriodAnalysis",
			"Enterprise.ReportTesting.Accounting.TestBankReconciliationTemplate",
			"Enterprise.ReportTesting.Accounting.TestCashBookRegisterReport",
			"Enterprise.ReportTesting.Accounting.TestCashBookSummaryReport",
			"Enterprise.ReportTesting.Accounting.TestChinaReportsBreakdownByCategories",
			"Enterprise.ReportTesting.Accounting.TestFreightHoldStatusOverridesTemplate",
			"Enterprise.ReportTesting.Accounting.TestGLJournalListingReport",
			"Enterprise.ReportTesting.Accounting.TestGLProfitAndLossMulitlingualReport",
			"Enterprise.ReportTesting.Accounting.TestGLTransactionsChineseReport",
			"Enterprise.ReportTesting.Accounting.TestGLTrialBalanceMulitlingualReport",
			"Enterprise.ReportTesting.Accounting.TestGLTrialBalanceReport",
			"Enterprise.ReportTesting.Accounting.TestGSTSummaryReport",
			"Enterprise.ReportTesting.Accounting.TestJobCountByOpenDateReport",
			"Enterprise.ReportTesting.Accounting.TestNetAgedARAPSummaryInLocalCurrencyByOrganisation",
			"Enterprise.ReportTesting.Accounting.TestNZGSTSummaryReport",
			"Enterprise.ReportTesting.Accounting.TestPayablesAnalysisTurnoverCreditorPeriodAnalysisTemplate",
			"Enterprise.ReportTesting.Accounting.TestPaymentsToIRS1099FormEligibleOrganisationsSummaryTemplate",
			"Enterprise.ReportTesting.Accounting.TestProfitAndLossbyDepartmentAsAtPeriodReport",
			"Enterprise.ReportTesting.Accounting.TestProfitLossPeriodAnalysis",
			"Enterprise.ReportTesting.Accounting.TestQSTSummaryReport",
			"Enterprise.ReportTesting.Accounting.TestReceiptListingReport",
			"Enterprise.ReportTesting.Accounting.TestTrialBalanceListofMovementsbyAccountPeriodBranchandDepartmentReport",
			"Enterprise.ReportTesting.Accounting.TestTrialBalancePeriodsAnalysis",
			"Enterprise.ReportTesting.Accounting.TestVATAnalysisSummaryReportForEUCompanies",
			"Enterprise.ReportTesting.Common.Test3rdPartySoftwareReport",
			"Enterprise.ReportTesting.Common.TestBankStatementTransactionsListingReport",
			"Enterprise.ReportTesting.Common.TestBudgetTrialBalanceReport",
			"Enterprise.ReportTesting.Common.TestGLBalanceSheetMultilingualReport",
			"Enterprise.ReportTesting.Common.TestJobSummarySelectedClientReport",
			"Enterprise.ReportTesting.Common.TestProductListing",
			"Enterprise.ReportTesting.Common.TestTariffLookupListing",
			"Enterprise.ReportTesting.Common.TestTransportMovementReport",
			"Enterprise.ReportTesting.Freight.Shared.TestLocalTransportJobProfileReport",
			"Enterprise.ReportTesting.MasterFiles.TestAPProfileReport",
			"Enterprise.ReportTesting.MasterFiles.TestARAPOrganisationByConsolidationCategory",
			"Enterprise.ReportTesting.MasterFiles.TestCarrierProfileReport",
			"Enterprise.ReportTesting.MasterFiles.TestExchangeRateReport",
			"Enterprise.ReportTesting.ReferenceFiles.TestEquipmentCertificatesExpiryReport"
		};

		//DO NOT ADD ENTRIES IN THIS LIST
		static readonly string[] temporaryExclusionsListForTestEnsureSortedReportsDontUseExecCommand_DO_NOT_ADD_ENTRIES_TO_THIS_LIST = new[]
		{
			"Enterprise.ReportTesting.Accounting.TestAnalysisLocalClientByPeriodReport",
			"Enterprise.ReportTesting.Accounting.TestAPAgedOutstandingTransactionsDetailinInvoiceCurrency",
			"Enterprise.ReportTesting.Accounting.TestAPAgedOutstandingTransactionsDetailinLocalCurrency",
			"Enterprise.ReportTesting.Accounting.TestAPAgedOutstandingTransactionsSummaryinInvoiceCurrency",
			"Enterprise.ReportTesting.Accounting.TestAPAgedOutstandingTransactionsSummaryinLocalCurrency",
			"Enterprise.ReportTesting.Accounting.TestAPTransactionsByPayableAccountTemplate",
			"Enterprise.ReportTesting.Accounting.TestAPTransactionsByTransactionType",
			"Enterprise.ReportTesting.Accounting.TestAPTransactionsSummaryAnalysisbyTransactionType",
			"Enterprise.ReportTesting.Accounting.TestARTransactionsSummaryAnalysisbyTransactionType",
			"Enterprise.ReportTesting.Accounting.TestGLTrialBalanceReport",
			"Enterprise.ReportTesting.Accounting.TestJobProfitAll",
			"Enterprise.ReportTesting.Accounting.TestJobProfitCFSCTO",
			"Enterprise.ReportTesting.Accounting.TestJobProfitLandTransport",
			"Enterprise.ReportTesting.Accounting.TestJobProfitLocalTransport",
			"Enterprise.ReportTesting.Accounting.TestJobProfitMAWBStock",
			"Enterprise.ReportTesting.Accounting.TestJobProfitTransactionTotalsbyBranchDept",
			"Enterprise.ReportTesting.Accounting.TestJobProfitWarehouse",
			"Enterprise.ReportTesting.Freight.Schedules.RoutePlanningTemplate"
		};

		#endregion

		public void TestSaveToFieldIsUniqueInReport()
		{
			const string savesToKeyword = "savesto=";
			var saveTos = new HashSet<string>();

			for (int x = 0; x < Report.XlInterface.WorkSheets.Count; x++)
			{
				ExcelWorkSheet workSheet = Report.XlInterface.WorkSheets[x];

				int startOfFirstAreaAfterConfig;
				for (startOfFirstAreaAfterConfig = 1; startOfFirstAreaAfterConfig < Report.XlInterface.MaxRowCountSupportedByCurrentExcelFile; startOfFirstAreaAfterConfig++)
				{
					if (workSheet[startOfFirstAreaAfterConfig, 0].ToString().StartsWith("#", StringComparison.OrdinalIgnoreCase))
					{
						break;
					}
				}
				int endOfConfigArea = startOfFirstAreaAfterConfig - 1;

				for (int i = 1; i <= endOfConfigArea; i++)
				{
					string configCellValue = workSheet[i, 0].ToString().Trim();
					if (configCellValue.StartsWith(Constants.ConfigAreaParameters.ColumnHeadingsSignature, StringComparison.OrdinalIgnoreCase))
					{
						int savesToKeywordPosition = configCellValue.IndexOf(savesToKeyword, StringComparison.OrdinalIgnoreCase);

						if (savesToKeywordPosition > -1)
						{
							saveTos.Add(configCellValue.Substring(savesToKeywordPosition + savesToKeyword.Length));
						}
					}
				}
			}

			Assert("The ColumnHeadings:SavesTo= value should be unique across all sheets of a report. The different values are: " + string.Join(", ", saveTos), saveTos.Count < 2);
		}

		/*	The rewrite of this unit test has been completed.
		 *	Waiting for creating new Workitem, then it will be assigned to related team to modify their document template??
		public void TestHeaderAndFooterNotOverlapping()
		{
			var xls = Report.XlInterface.Xls;
			var extraMargin = 0.2;

			for (var sheet = 1; sheet <= xls.SheetCount; sheet++)
			{
				xls.ActiveSheet = sheet;
				if (xls.SheetVisible == TXlsSheetVisible.Visible && DocumentEngine.Report.IsTemplateSheet(xls.SheetName))
				{
					var xlsMargins = xls.GetPrintMargins();
					CombineAssertions(() =>
					{
						if (xls.PageHeader != null)
						{
							Assert(string.Format(CultureInfo.InvariantCulture, "Header overlaps body in the following report : {0}", Report.Template.TemplateName.ToString()), xlsMargins.Top > xlsMargins.Header + extraMargin);
						}
						if (xls.PageFooter != null)
						{
							Assert(string.Format(CultureInfo.InvariantCulture, "Footer overlaps body in the following report : {0}", Report.Template.TemplateName.ToString()), xlsMargins.Bottom > xlsMargins.Footer + extraMargin);
						}
					});
				}
			}
		}
		*/

		public void TestNoSpaceBeforeEmailSubject()
		{
			Report.PrepareForRender();

			ForEachReportTemplate(
				worksheetTransform: (worksheet) =>
				{
					Report.SetWorkSheetCurrentlyBeingProcessedForTestOnly(worksheet);
					Report.AnalyzeResettingLoadedFlagsAfterwards();
					return Report.Analyser.Config.DataSourceStrings;
				},
				worksheetAction: (dataStrings) =>
				{
					Assert("Space found at start of EmailSubject in template: " + Report.Name.ToString(), !Report.EmailSubject.StartsWith(" ", StringComparison.Ordinal));
				});
		}

		[DeveloperOnlyTest]
		public void TestNoTitleInSameColumnWithColumnHeadings()
		{
			var templateErrorBuilder = new ZStringBuilder();

			foreach (ExcelWorkSheet workSheet in Report.XlInterface.WorkSheets.Where(x => Enterprise.DocumentEngine.Report.IsTemplateSheet(x.SheetName) && !GetExcludedSheetNames().Contains(x.SheetName)))
			{
				int columnHeadingStartColumnIndex = 0;
				int columnHeadingEndColumnIndex = 0;
				int columnHeadingRowIndex = 0;

				for (int i = 0; i < workSheet.RowCount; i++)
				{
					if (columnHeadingRowIndex == 0 && workSheet[i, 0].ToString().StartsWith(Constants.ConfigAreaParameters.ColumnHeadingsSignature, StringComparison.OrdinalIgnoreCase))
					{
						columnHeadingRowIndex = i;
						break;
					}
				}

				if (columnHeadingRowIndex == 0)
				{
					continue;
				}

				for (int i = 1; i < workSheet.ColumnCount; i++)
				{
					string cellValue = workSheet[columnHeadingRowIndex, i].ToString().Trim();
					if (RegexProvider.ColumnHeadingDisplayLabelRegex.IsMatch(cellValue))
					{
						if (columnHeadingStartColumnIndex == 0)
						{
							columnHeadingStartColumnIndex = i;
						}
						columnHeadingEndColumnIndex = i;
					}
				}

				if (columnHeadingStartColumnIndex == 0)
				{
					continue;
				}

				bool isCheckOver = false;

				for (int i = columnHeadingRowIndex + 1; i < workSheet.RowCount; i++)
				{
					for (int j = columnHeadingStartColumnIndex; j <= columnHeadingEndColumnIndex; j++)
					{
						ZString cellValue = workSheet[i, j].ToString().Trim();
						if (RegexProvider.CustomisedColumnRegex.IsMatch(cellValue))
						{
							isCheckOver = true;
							break;
						}
						else if (!cellValue.IsEmpty)
						{
							templateErrorBuilder.AppendLine($"Sheet Name: {workSheet.SheetName}, Row: {i}, Column: {j}");
							isCheckOver = true;
							break;
						}
					}

					if (isCheckOver)
					{
						break;
					}
				}
			}

			if (templateErrorBuilder.Length > 0)
			{
				templateErrorBuilder.Prepend(string.Format(CultureInfo.InvariantCulture, "Non-empty cells in DocumentHeader section are detected staying in the same column with column headers in the following sheets of the template: {0}, they could be hidden if the column header in the same column is set to hidden by user on the report runtime option form, try to move them to other columns so that they will always be visible regardless of the column display setting. \r\n", Report.Template.TemplateName));
				Fail(templateErrorBuilder.ToString());
			}

			Assert(true);
		}

		[DeveloperOnlyTest]
		public void TestDateTimeAsStringNotUsedInSectionBody()
		{
			var errorBuilder = new ZStringBuilder();
			PrepareReportForRender();
			SelectAllOptionalTemplates();

			ForEachReportTemplate(
				worksheetTransform: (worksheet) =>
				{
					Report.SetWorkSheetCurrentlyBeingProcessedForTestOnly(worksheet);
					Report.AnalyzeResettingLoadedFlagsAfterwards();
					return Report.Analyser.Areas.Where(a => a.GetType().Name == "SectionBodyArea");
				},
				worksheetAction: (areas, sheetName) =>
				{
					foreach (var area in areas)
					{
						for (int row = area.StartingRow + 1; row <= area.End; row++)
						{
							for (int column = 1; column <= area.ColumnCount; column++)
							{
								var cellContent = Report.WorkSheetCurrentlyBeingProcessed[row, column].ToString();
								if (!string.IsNullOrEmpty(cellContent) && cellContent.Contains("DateTimeAsString"))
								{
									errorBuilder.Append($"Sheet Name: {sheetName}, Cell: {ExcelWorkSheet.GetCellName(row, column)}");
								}
							}
						}
					}
				});

			if (errorBuilder.Length > 0)
			{
				errorBuilder.Prepend($"DateTimeAsString macro should not be used in a report SectionBody because it returns a string that cannot be filtered / sorted easily in Excel. The following SectionBody cells in the {Report.Template.TemplateName} template include DateTimeAsString:");
				Fail(errorBuilder.ToStringWithNewLineBetweenAppends());
			}

			Assert(true);
		}

		protected virtual List<string> GetExcludedSheetNames() => new List<string>();

		#region TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter

		[SnailTest]
		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter_ForFilters0to4()
			=> TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter(0, 5);

		[SnailTest]
		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter_ForFilters5to9()
			=> TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter(5, 5);

		[SnailTest]
		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter_ForFilters10to14()
			=> TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter(10, 5);

		[SnailTest]
		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter_ForFilters15to19()
			=> TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter(15, 5);

		[SnailTest]
		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter_ForFilters20to24()
			=> TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter(20, 5);

		[SnailTest]
		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter_ForFilters25to29()
			=> TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter(25, 5);

		[SnailTest]
		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter_ForFilters30to34()
			=> TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter(30, 5);

		[SnailTest]
		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter_ForFilters35to39()
			=> TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter(35, 5);

		[SnailTest]
		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter_ForFilters40to44()
			=> TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter(40, 5);

		[SnailTest]
		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter_ForFilters45to49()
			=> TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter(45, 5);

		[SnailTest]
		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter_ForFilters50to54()
			=> TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter(50, 5);

		[SnailTest]
		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter_ForFilters55to59()
			=> TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter(55, 5);

		public void TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter_ForFilters60Plus()
		{
			Assert("Reports with over 60 filters are not tested. Please add additional tests to cover filters 60 and up. (And consider if you really need that many filters!)", Report.FilterCollection.Count <= 60);
		}

		void TestEnsureAllTemplatesRunWithDefaultFiltersSpecifiedAndEachIndividualOptionalFilter(int skipCount, int takeCount)
		{
			SetupData();
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			FillReportWithDefaultValues();
			var sortOrdersToTest = SortOrderAsParameterTestRequired ? ((IBusiness)Report.SortOrderCollection).Children.Cast<SortOrder>() : Enumerable.Empty<SortOrder>();
			RunReportForEachNonRequiredFilter(skipCount, takeCount, sortOrdersToTest);
		}

		#endregion

		#region TestReportTemplateShouldReplaceCountryWithCountryRegion

		public void TestReportTemplateShouldReplaceCountryWithCountryRegion()
		{
			if (WhiteListForCountryRegion.Any(x => x.Equals(TemplateLocation, StringComparison.OrdinalIgnoreCase)))
			{
				Assert("The current unit test is on the whitelist, skipping execution.", true);
			}
			else
			{
				AssertReportTemplateShouldReplateCountryToCountryRegion();
			}
		}

		void AssertReportTemplateShouldReplateCountryToCountryRegion()
		{
			var errorMessage = new StringBuilder();
			Report.PrepareForRender();

			ForEachReportTemplate(
				worksheetTransform: (worksheet) =>
				{
					//Column Headings:
					var worksheetForColumnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets.Cast<Worksheet>().FirstOrDefault(sheet => sheet.Name == worksheet.SheetName);
					if (worksheetForColumnHeadings != null)
					{
						var columnErrorMessage = new StringBuilder();
						foreach (ColumnHeading columnHeading in worksheetForColumnHeadings.ColumnHeadings)
						{
							var fullNameForCheck = $"{columnHeading.TagName} {columnHeading.HeadingText}";
							AddErrorMessageIfReplacedValueStillContainsCountry(fullNameForCheck, columnErrorMessage, columnHeading.HeadingText);
						}

						AddErrorMessage(columnErrorMessage, "Column Headings:");
					}

					// EmailSubject
					if (!string.IsNullOrEmpty(Report.EmailSubject))
					{
						var replacedValue = RegexProvider.OutermostMacroRegex.Replace(Report.EmailSubject, match => "");
						AddErrorMessageIfReplacedValueStillContainsCountry(replacedValue, errorMessage, $"EmailSubject:\r\n{Report.EmailSubject}");
					}

					CheckTemplateSheetAreaForCountryToCountryRegion(worksheet.SheetName);

					return Enumerable.Empty<string>();
				},
				worksheetAction: (transformResult, sheetName) => { });

			CheckTemplateFilterSheetForCountryToCountryRegion();
			CheckTemplateSortSheetForCountryToCountryRegion();
			CheckTemplateGroupBySheetForCountryToCountryRegion();

			if (errorMessage.Length > 0)
			{
				errorMessage.AppendLine($"TemplateLocation: {TemplateLocation}");
			}

			AssertEquals("Should use Country/Region instead of Country", "", errorMessage.ToString());

			void CheckTemplateSheetAreaForCountryToCountryRegion(string sheetName)
			{
				var areaErrorMessage = new StringBuilder();
				foreach (var area in Report.Analyser.Areas)
				{
					if (area.GetType().Name != "ConfigArea")
					{
						for (int row = area.StartingRow + 1; row <= area.End; row++)
						{
							for (int column = 1; column <= area.ColumnCount; column++)
							{
								var cellContent = area.ParentReport.WorkSheetCurrentlyBeingProcessed[row, column].ToString();
								if (!string.IsNullOrEmpty(cellContent))
								{
									var replacedValue = RegexProvider.OutermostMacroRegex.Replace(cellContent, match => "");
									AddErrorMessageIfReplacedValueStillContainsCountry(replacedValue, areaErrorMessage, $"[{row + 1},{column + 1}] {cellContent}");
								}
							}
						}
					}
				}

				AddErrorMessage(areaErrorMessage, $"[{sheetName}] Sheet :");
			}

			void CheckTemplateFilterSheetForCountryToCountryRegion()
			{
				var filterErrorMessage = new StringBuilder();
				var filterSheet = Report.XlInterface.WorkSheets.FirstOrDefault(sheet => Enterprise.DocumentEngine.Report.IsFilterSheetName(sheet.SheetName));
				if (filterSheet != null)
				{
					for (var row = 0; row < filterSheet.RowCount; row++)
					{
						for (var column = 0; column < filterSheet.ColumnCount; column++)
						{
							var cellContent = filterSheet[row, column].ToString().Trim();
							if (!string.IsNullOrEmpty(cellContent))
							{
								if (column == 1 && !(cellContent.Equals("Tab", StringComparison.OrdinalIgnoreCase) || cellContent.Equals("Option", StringComparison.OrdinalIgnoreCase)))
								{
									break;
								}
								AddErrorMessageIfReplacedValueStillContainsCountry(cellContent, filterErrorMessage, $"[{row + 1},{column + 1}] {cellContent}");
							}
						}
					}
				}

				AddErrorMessage(filterErrorMessage, "Filter Sheet:");
			}

			void CheckTemplateSortSheetForCountryToCountryRegion()
			{
				var sortErrorMessage = new StringBuilder();
				foreach (SortOrder sortOrder in Report.SortOrderCollection)
				{
					AddErrorMessageIfReplacedValueStillContainsCountry(sortOrder.DisplayName, sortErrorMessage);
				}

				AddErrorMessage(sortErrorMessage, "Sort Sheet:");
			}

			void CheckTemplateGroupBySheetForCountryToCountryRegion()
			{
				var groupByErrorMessage = new StringBuilder();
				foreach (GroupBy groupBy in Report.GroupByCollection)
				{
					AddErrorMessageIfReplacedValueStillContainsCountry(groupBy.DisplayName, groupByErrorMessage);
				}

				AddErrorMessage(groupByErrorMessage, "GroupBy Sheet:");
			}

			void AddErrorMessageIfReplacedValueStillContainsCountry(string content, StringBuilder currentErrorMessage, string errorMessage = null)
			{
				if (!string.IsNullOrEmpty(content))
				{
					var replacedValue = Regex.Replace(content, @"Country\s*/\s*Region", "", RegexOptions.IgnoreCase);
					if (replacedValue.IndexOf("Country", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						currentErrorMessage.AppendLine(errorMessage ?? content);
					}
				}
			}

			void AddErrorMessage(StringBuilder currentErrorMessage, string lable)
			{
				if (currentErrorMessage.Length > 0)
				{
					errorMessage.AppendLine(lable)
						.AppendLine(currentErrorMessage.ToString())
						.AppendLine();
				}
			}
		}

		static readonly IEnumerable<string> WhiteListForCountryRegion = new HashSet<string>()
		{
			@"Enterprise\Product\Documents\ExcelTemplates\Reports\BE NCTS P5 Registry of Departure.xls",
			@"Enterprise\Product\Documents\ExcelTemplates\Reports\CA Import Invoice Lines Report.xls",
			@"Enterprise\Product\Documents\ExcelTemplates\Reports\NO MVA settlement report.xls",
			@"Enterprise\Product\Documents\ExcelTemplates\Reports\US Product Listing.xls",
			@"Enterprise\Product\Documents\ExcelTemplates\Reports\EDI\Incident Billing Worksheet.xls"
		};

		#endregion

		#endregion
	}
}
