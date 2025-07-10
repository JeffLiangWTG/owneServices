namespace Enterprise.ReportTesting.Customs.Shared.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.DocumentEngine.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Module;

	[TemplateName("Permit Transaction Report")]
	public class TestPermitTransactionsReport : TemplateTestCase
	{
	}

	public class TestPermitTransactionsReportMenuSetup : ReportTestCase
	{
		public new void TestMenuItemSetupCorrectly()
		{
			AssertEquals(2, CandidateMenuItems.Count);

			List<StmMenuItemBase> menuItemList = new List<StmMenuItemBase>();
			CandidateMenuItems.CopyToList(menuItemList);

			var hint = "Permit Transaction Report for the USA";
			var filter = "BKRCTY=US";
			CheckMenuItem(menuItemList, filter, hint);

			hint = "Permit Transaction Report for South Africa";
			filter = "BKRCTY=ZA";
			CheckMenuItem(menuItemList, filter, hint);
		}

		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustFilesReports(); }
		}

		public override string MenuName => "Permit Transaction Report";

		public override string Hint => "";

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestPermitTransactionsReport();
		}

		void CheckMenuItem(List<StmMenuItemBase> menuItemList, string filter, string hint)
		{
			var menuItem = menuItemList.Find(x => x.SU_FilterList == filter);

			AssertNotNull("Must find menu item with filter: " + filter, menuItem);
			Assert("Hint must be less than 1024 characters", hint.Length <= 1024);
			AssertMultilineASCIIEquals("Incorrect Menu hint", hint, menuItem.SU_Hint);

			AssertEquals("Checking menu name of menu item with filter: " + filter, "Permit Transaction Report", menuItem.SU_MenuName);
			AssertEquals("Checking menu type of menu item with filter: " + filter, "DOC", menuItem.SU_MenuType);
			AssertEquals("Checking business context of menu item with filter: " + filter, "RepCustFilesReports", menuItem.SU_BusinessContext);
		}
	}

	public class PermitTransactionsReportTests : ReportDataChecker
	{
		public PermitTransactionsReportTests()
		{
			reportDataObjectDetails = new PermitTransactionsReport_DataObjectDetails();

			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@RefCountryCode", Core.Constants.CountryCodes.SouthAfrica);
			parameterHelper.SetParameterValue("@TransactionCategory", "CUM");

			scenario = new ReportTestScenario();
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
		}

		#region Instance Attributes:

		readonly Dictionary<BaseCusPermitHeader, decimal> ValueBalanceLookup = new Dictionary<BaseCusPermitHeader, decimal>();
		readonly Dictionary<BaseCusPermitHeader, decimal> QtyBalanceLookup = new Dictionary<BaseCusPermitHeader, decimal>();

		#endregion Instance Attributes.

		public void TestPermitTransactionsReport()
		{
			#region Prepare Test Data:

			var permitList = PreparePermits();
			Factory.Save();

			Action<BaseCusPermitHeader> prepareBalancesAction = x =>
			{
				ValueBalanceLookup[x] = CalculateValueBalance(x);
				QtyBalanceLookup[x] = CalculateQtyBalance(x);
			};

			permitList.ForEach(prepareBalancesAction);

			#endregion Prepare Test Data.

			var scenarioList = new List<ReportTestScenario>()
			{
				CreateTestScenario_FilterOn_CountryCode(),
				CreateTestScenario_FilterOn_PermitNumber(),
				CreateTestScenario_FilterOn_StartDate(),
				CreateTestScenario_FilterOn_EndDate(),
				CreateTestScenario_FilterOn_QtyValIndicator(),
				CreateTestScenario_FilterOn_TransactionCategory(),
				CreateTestScenario_FilterOn_AllFilterFields()
			};

			var allTransactions = CollectAllTransactions(permitList);

			foreach (var scenario in scenarioList)
			{
				var expectedOutput = DetermineExpectedOutput(scenario, allTransactions);

				var rowCount = expectedOutput.GetRowData().Count;
				Assert($"No expected data rows for scenario: {scenario.ScenarioName}", rowCount > 0);

				CheckReportData(scenario, expectedOutput);
			}
		}

		#region Create the Scenarios:

		ReportTestScenario CreateTestScenario_FilterOn_CountryCode()
		{
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@RefCountryCode", Core.Constants.CountryCodes.Canada);

			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on country code.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();

			scenario.ExpectedRowCount = 2;

			scenario.AddExpectedText(1, "[PermitNumber]='PN-01'");
			scenario.AddExpectedText(1, "[TransactionComment]='COMMENT-01'");

			scenario.AddExpectedText(2, "[PermitNumber]='PN-01'");
			scenario.AddExpectedText(2, "[TransactionComment]='COMMENT-02'");

			return scenario;
		}

		ReportTestScenario CreateTestScenario_FilterOn_PermitNumber()
		{
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@RefCountryCode", Core.Constants.CountryCodes.SouthAfrica);
			parameterHelper.SetParameterValue("@PermitNumberFrom", "PN-03");
			parameterHelper.SetParameterValue("@PermitNumberTo", "PN-03");

			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on permit number.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();

			scenario.ExpectedRowCount = 3;

			scenario.AddExpectedText(1, "[PermitNumber]='PN-03'");
			scenario.AddExpectedText(1, "[TransactionComment]='COMMENT-05'");

			scenario.AddExpectedText(2, "[PermitNumber]='PN-03'");
			scenario.AddExpectedText(2, "[TransactionComment]='COMMENT-06'");

			scenario.AddExpectedText(3, "[PermitNumber]='PN-03'");
			scenario.AddExpectedText(3, "[TransactionComment]='COMMENT-07'");

			return scenario;
		}

		ReportTestScenario CreateTestScenario_FilterOn_StartDate()
		{
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@RefCountryCode", Core.Constants.CountryCodes.SouthAfrica);
			parameterHelper.SetParameterValue("@StartDateFrom", (new ZDateTime(2019, 2, 25)).ToISO8601String());
			parameterHelper.SetParameterValue("@StartDateTo", (new ZDateTime(2019, 3, 25)).ToISO8601String());

			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on start date.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();

			scenario.ExpectedRowCount = 2;

			scenario.AddExpectedText(1, "[PermitNumber]='PN-05'");
			scenario.AddExpectedText(1, "[TransactionComment]='COMMENT-12'");

			scenario.AddExpectedText(2, "[PermitNumber]='PN-05'");
			scenario.AddExpectedText(2, "[TransactionComment]='COMMENT-13'");

			return scenario;
		}

		ReportTestScenario CreateTestScenario_FilterOn_EndDate()
		{
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@RefCountryCode", Core.Constants.CountryCodes.SouthAfrica);
			parameterHelper.SetParameterValue("@EndDateFrom", (new ZDateTime(2021, 1, 25)).ToISO8601String());
			parameterHelper.SetParameterValue("@EndDateTo", (new ZDateTime(2021, 2, 25)).ToISO8601String());

			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on end date.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();

			scenario.ExpectedRowCount = 3;

			scenario.AddExpectedText(1, "[PermitNumber]='PN-06'");
			scenario.AddExpectedText(1, "[TransactionComment]='COMMENT-14'");

			scenario.AddExpectedText(2, "[PermitNumber]='PN-06'");
			scenario.AddExpectedText(2, "[TransactionComment]='COMMENT-15'");

			scenario.AddExpectedText(3, "[PermitNumber]='PN-06'");
			scenario.AddExpectedText(3, "[TransactionComment]='COMMENT-16'");

			return scenario;
		}

		ReportTestScenario CreateTestScenario_FilterOn_QtyValIndicator()
		{
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@RefCountryCode", Core.Constants.CountryCodes.SouthAfrica);
			parameterHelper.SetParameterValue("@QtyValIndicator", "BTH");

			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on QtyValIndicator.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();

			scenario.ExpectedRowCount = 6;

			scenario.AddExpectedText(1, "[PermitNumber]='PN-03'");
			scenario.AddExpectedText(1, "[TransactionComment]='COMMENT-05'");

			scenario.AddExpectedText(6, "[PermitNumber]='PN-06'");
			scenario.AddExpectedText(6, "[TransactionComment]='COMMENT-16'");

			return scenario;
		}

		ReportTestScenario CreateTestScenario_FilterOn_TransactionCategory()
		{
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@RefCountryCode", Core.Constants.CountryCodes.SouthAfrica);
			parameterHelper.SetParameterValue("@TransactionCategory", "VAL");

			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on transaction category.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();

			scenario.ExpectedRowCount = 2;

			scenario.AddExpectedText(1, "[PermitNumber]='PN-02'");
			scenario.AddExpectedText(1, "[TransactionComment]='COMMENT-03'");

			scenario.AddExpectedText(2, "[PermitNumber]='PN-02'");
			scenario.AddExpectedText(2, "[TransactionComment]='COMMENT-04'");

			return scenario;
		}

		ReportTestScenario CreateTestScenario_FilterOn_AllFilterFields()
		{
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@RefCountryCode", Core.Constants.CountryCodes.SouthAfrica);
			parameterHelper.SetParameterValue("@PermitNumberFrom", "PN-04");
			parameterHelper.SetParameterValue("@PermitNumberTo", "PN-04");
			parameterHelper.SetParameterValue("@StartDateFrom", (new ZDateTime(2019, 3, 29)).ToISO8601String());
			parameterHelper.SetParameterValue("@StartDateTo", (new ZDateTime(2019, 3, 29)).ToISO8601String());
			parameterHelper.SetParameterValue("@EndDateFrom", (new ZDateTime(2020, 11, 29)).ToISO8601String());
			parameterHelper.SetParameterValue("@EndDateTo", (new ZDateTime(2020, 11, 29)).ToISO8601String());
			parameterHelper.SetParameterValue("@QtyValIndicator", "QTY");
			parameterHelper.SetParameterValue("@TransactionCategory", "CUM");

			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on all filter fields.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();

			scenario.ExpectedRowCount = 4;

			scenario.AddExpectedText(1, "[PermitNumber]='PN-04'");
			scenario.AddExpectedText(1, "[TransactionComment]='COMMENT-08'");

			scenario.AddExpectedText(3, "[PermitNumber]='PN-04'");
			scenario.AddExpectedText(3, "[TransactionComment]='COMMENT-10'");

			scenario.AddExpectedText(4, "[PermitNumber]='PN-04'");
			scenario.AddExpectedText(4, "[TransactionComment]='COMMENT-11'");

			return scenario;
		}

		#endregion Create the Scenarios.

		List<BaseCusPermitLineTransaction> CollectAllTransactions(List<BaseCusPermitHeader> permitList)
		{
			return permitList
				.SelectMany(x => x.CusPermitLineTransactions)
				.ToList();
		}

		ExpectedReportOutput DetermineExpectedOutput(ReportTestScenario scenario, List<BaseCusPermitLineTransaction> allTransactions)
		{
			var filteredList = ApplyFiltersUsed(allTransactions, scenario.FiltersUsed);

			var expectedOutput = new ExpectedReportOutput();
			filteredList.ForEach(x => expectedOutput.AddRow(GetRowText_from_Transaction(x)));
			return expectedOutput;
		}

		string GetRowText_from_Transaction(BaseCusPermitLineTransaction transaction)
		{
			var permit = transaction.PermitHeader;
			var holder = permit.PermitHolder;
			var columnHelper = new ReportColumnHelper(reportDataObjectDetails.ExpectedColumnsInOrder);

			columnHelper.SetColumnValue("PermitNumber", permit.CPH_Number);
			columnHelper.SetColumnValue("PermitHolder", holder.OH_Code);
			columnHelper.SetColumnValue("PermitHolderName", holder.OH_FullName);

			columnHelper.SetColumnValue("StartDate", permit.CPH_StartDate);
			columnHelper.SetColumnValue("EndDate", permit.CPH_EndDate);

			columnHelper.SetColumnValue("PermitType", permit.CPH_Type);
			columnHelper.SetColumnValue("PermitSubType", permit.CPH_SubType);
			columnHelper.SetColumnValue("QtyValIndicator", permit.CPH_QtyValIndicator);
			columnHelper.SetColumnValue("UnitOfMeasure", permit.CPH_UnitOfMeasure);

			columnHelper.SetColumnValue("TransactionDate", transaction.CPL_TransactionDate);
			columnHelper.SetColumnValue("TransactionCategory", transaction.CPL_TransactionCategory);
			columnHelper.SetColumnValue("TransactionType", transaction.CPL_TransactionType);

			columnHelper.SetColumnValue("TransactionDescription", GetTransactionDesc(transaction.CPL_TransactionType));

			columnHelper.SetColumnValue("TransactionReference", transaction.CPL_Reference);
			columnHelper.SetColumnValue("TransactionComment", transaction.CPL_Comment);
			columnHelper.SetColumnValue("TransactionValue", Convert.ToDecimal(transaction.CPL_TranValue), 7);
			columnHelper.SetColumnValue("TransactionQuantity", Convert.ToDecimal(transaction.CPL_TranQty));

			var valueBalance = ValueBalanceLookup[permit];
			var qtyBalance = QtyBalanceLookup[permit];

			columnHelper.SetColumnValue("ValueBalance", valueBalance, 7);
			columnHelper.SetColumnValue("QtyBalance", qtyBalance);

			var hasBalance = DetermineHasBalance(permit);
			columnHelper.SetColumnValue("HasBalance", hasBalance);
			columnHelper.SetColumnValue("HasBalanceDescription", GetHasBalanceDesc(hasBalance));

			columnHelper.SetColumnValue("CPH_OH_PermitHolder", permit.CPH_OH_PermitHolder);
			return columnHelper.GetRowText();
		}

		string GetTransactionDesc(string type)
		{
			var retVal = "";

			if (type == "OBL")
			{
				retVal = "Opening Balance";
			}
			else if (type == "TRA")
			{
				retVal = "Automated Transaction";
			}
			else if (type == "ADJ")
			{
				retVal = "Manual Adjustment";
			}
			return retVal;
		}

		decimal CalculateValueBalance(BaseCusPermitHeader permit)
		{
			decimal valueBalance = 0;

			Action<BaseCusPermitLineTransaction> sumAction = x =>
			{
				if ((x.CPL_TransactionCategory == "CUM") && (x.CPL_TransactionStatus != "DEL"))
				{
					valueBalance += x.CPL_TranValue;
				}
			};

			permit.CusPermitLineTransactions.ToList().ForEach(sumAction);
			return valueBalance;
		}

		decimal CalculateQtyBalance(BaseCusPermitHeader permit)
		{
			decimal qtyBalance = 0;

			Action<BaseCusPermitLineTransaction> sumAction = x =>
			{
				if ((x.CPL_TransactionCategory == "CUM") && (x.CPL_TransactionStatus != "DEL"))
				{
					qtyBalance += x.CPL_TranQty;
				}
			};

			permit.CusPermitLineTransactions.ToList().ForEach(sumAction);
			return qtyBalance;
		}

		string DetermineHasBalance(BaseCusPermitHeader permit)
		{
			var retVal = "F";
			var indicator = permit.CPH_QtyValIndicator;

			if (((indicator == "VAL") || (indicator == "BTH")) && (ValueBalanceLookup[permit] > 0))
			{
				retVal = "R";
			}

			if (((indicator == "QTY") || (indicator == "BTH")) && (QtyBalanceLookup[permit] > 0))
			{
				retVal = "R";
			}

			return retVal;
		}

		string GetHasBalanceDesc(string code)
		{
			var retVal = "";

			if (code == "R")
			{
				retVal = "With Remaining Balance";
			}
			else if (code == "F")
			{
				retVal = "Permit Fully Utilised";
			}
			return retVal;
		}

		List<BaseCusPermitLineTransaction> ApplyFiltersUsed(List<BaseCusPermitLineTransaction> allTransactions, Dictionary<string, string> filtersUsed)
		{
			var transactions = new List<BaseCusPermitLineTransaction>(allTransactions);
			var predicates = new List<Predicate<BaseCusPermitLineTransaction>>();

			Action<string> compilePredicate = filterField =>
			{
				var filterValue = filtersUsed[filterField];

				switch (filterField)
				{
					case "@RefCountryCode":
						predicates.Add(x => x.PermitHeader.CPH_RN_NKCountryCode == filterValue);
						break;

					case "@PermitNumberFrom":
						predicates.Add(x => x.PermitHeader.CPH_Number.CompareTo(filterValue) >= 0);
						break;

					case "@PermitNumberTo":
						predicates.Add(x => x.PermitHeader.CPH_Number.CompareTo(filterValue) <= 0);
						break;

					case "@StartDateFrom":
						predicates.Add(x => x.PermitHeader.CPH_StartDate >= new ZDateTime(filterValue));
						break;

					case "@StartDateTo":
						predicates.Add(x => x.PermitHeader.CPH_StartDate <= new ZDateTime(filterValue));
						break;

					case "@EndDateFrom":
						predicates.Add(x => x.PermitHeader.CPH_EndDate >= new ZDateTime(filterValue));
						break;

					case "@EndDateTo":
						predicates.Add(x => x.PermitHeader.CPH_EndDate <= new ZDateTime(filterValue));
						break;

					case "@QtyValIndicator":
						predicates.Add(x => x.PermitHeader.CPH_QtyValIndicator == filterValue);
						break;

					case "@TransactionCategory":
						predicates.Add(x => x.CPL_TransactionCategory == filterValue);
						break;
				}
			};

			filtersUsed.Keys.ToList().ForEach(compilePredicate);
			predicates.ForEach(x => transactions = transactions.FindAll(x));
			return transactions;
		}

		List<BaseCusPermitHeader> PreparePermits()
		{
			var permitFactory = new PermitFactory(Factory);

			var permitList = new List<BaseCusPermitHeader>()
			{
				permitFactory.CreatePermit(noOfTransactions: 2, countryCode: "CA"),
				permitFactory.CreatePermit(noOfTransactions: 2, category: "VAL"),
				permitFactory.CreatePermit(noOfTransactions: 3),
				permitFactory.CreatePermit(noOfTransactions: 4),
				permitFactory.CreatePermit(noOfTransactions: 2),
				permitFactory.CreatePermit(noOfTransactions: 3),
				permitFactory.CreatePermit(noOfTransactions: 4)
			};

			return permitList;
		}

		protected override void PrepareTestData()
		{
			var permitFactory = new PermitFactory(Factory);
			permitFactory.CreatePermit(noOfTransactions: 1);
		}

		protected override void AssertTestResults(System.Data.DataTable resultsOrderedByExpectedColumnNames)
		{
			AssertEquals(1, resultsOrderedByExpectedColumnNames.Rows.Count);
			var row1 = FormatRowsValues(resultsOrderedByExpectedColumnNames.Rows[0], resultsOrderedByExpectedColumnNames);

			var fieldValues = new List<string>()
			{
				"[PermitNumber]='PN-01'",
				"[PermitHolder]='IMP01'",
				"[PermitHolderName]='IMP01_FullName'",
				"[StartDate]='2019-06-29T00:00:00'",
				"[EndDate]='2020-08-29T00:00:00'",
				"[PermitType]='EXP'",
				"[PermitSubType]='S2'",
				"[QtyValIndicator]='QTY'",
				"[UnitOfMeasure]='LTR'",
				"[TransactionDate]='2019-06-30T00:00:00'",
				"[TransactionCategory]='CUM'",
				"[TransactionType]='OBL'",
				"[TransactionDescription]='Opening Balance'",
				"[TransactionReference]='REF-01'",
				"[TransactionComment]='COMMENT-01'",
				"[TransactionValue]='5001.0000000'",
				"[TransactionQuantity]='3001.00000'",
				"[ValueBalance]='5001.0000000'",
				"[QtyBalance]='3001.00000'",
				"[HasBalance]='R'",
				"[HasBalanceDescription]='With Remaining Balance'"
			};

			var builder = new StringBuilder();
			fieldValues.ForEach(x => builder.Append(x + "; "));

			var expectedData = builder.ToString();
			expectedData = expectedData.Substring(0, expectedData.Length - 2);

			AssertContainsMoreHelpfully(expectedData, row1);
		}
	}

	class PermitTransactionsReport_DataObjectDetails : ReportDataObjectDetails
	{
		public PermitTransactionsReport_DataObjectDetails()
		{
			ObjectName = "Report_PermitTransactions";
			SqlObjectType = Enterprise.ReportTesting.SqlObjectType.FunctionTable;
			ExpectedColumnsInOrder = PopulateExpectedColumnsInOrder();
			PopulateParameterDetails();
		}

		List<ReportSchemaColumn> PopulateExpectedColumnsInOrder()
		{
			var columns = new List<ReportSchemaColumn>()
			{
				new ReportSchemaColumn(typeof(string), "PermitNumber"),
				new ReportSchemaColumn(typeof(string), "PermitHolder"),
				new ReportSchemaColumn(typeof(string), "PermitHolderName"),
				new ReportSchemaColumn(typeof(DateTime), "StartDate"),
				new ReportSchemaColumn(typeof(DateTime), "EndDate"),
				new ReportSchemaColumn(typeof(string), "PermitType"),
				new ReportSchemaColumn(typeof(string), "PermitSubType"),
				new ReportSchemaColumn(typeof(string), "QtyValIndicator"),
				new ReportSchemaColumn(typeof(string), "UnitOfMeasure"),
				new ReportSchemaColumn(typeof(DateTime), "TransactionDate"),
				new ReportSchemaColumn(typeof(string), "TransactionCategory"),
				new ReportSchemaColumn(typeof(string), "TransactionType"),
				new ReportSchemaColumn(typeof(string), "TransactionDescription"),
				new ReportSchemaColumn(typeof(string), "TransactionReference"),
				new ReportSchemaColumn(typeof(string), "TransactionComment"),
				new ReportSchemaColumn(typeof(decimal), "TransactionValue"),
				new ReportSchemaColumn(typeof(decimal), "TransactionQuantity"),
				new ReportSchemaColumn(typeof(decimal), "ValueBalance"),
				new ReportSchemaColumn(typeof(decimal), "QtyBalance"),
				new ReportSchemaColumn(typeof(string), "HasBalance"),
				new ReportSchemaColumn(typeof(string), "HasBalanceDescription"),
				new ReportSchemaColumn(typeof(Guid), "CPH_OH_PermitHolder")
			};
			return columns;
		}

		void PopulateParameterDetails()
		{
			AddParameter("@RefCountryCode", typeof(string));
			AddParameter("@PermitNumberFrom", typeof(string));
			AddParameter("@PermitNumberTo", typeof(string));
			AddParameter("@StartDateFrom", typeof(DateTime));
			AddParameter("@StartDateTo", typeof(DateTime));
			AddParameter("@EndDateFrom", typeof(DateTime));
			AddParameter("@EndDateTo", typeof(DateTime));
			AddParameter("@QtyValIndicator", typeof(string));
			AddParameter("@TransactionCategory", typeof(string));
		}
	}

	class PermitFactory
	{
		public PermitFactory(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		#region Factory Instance Attributes:

		readonly BusinessObjectFactory Factory;
		int PermitCounter;
		int TransactionCounter;
		readonly ZDate PermitTestDate = new ZDate(2020, 01, 29);

		readonly List<string> PermitTypeList = new List<string>() { "IMP", "EXP", "RCC", "REB" };
		readonly List<string> PermitSubTypeList = new List<string>() { "S1", "S2", "S3", "S4" };
		readonly List<string> QtyValIndicatorList = new List<string>() { "BTH", "QTY", "VAL" };
		readonly List<string> UnitOfMeasureList = new List<string>() { "KG", "LTR", "LB", "CBM", "MTR" };
		readonly List<string> TransactionTypeList = new List<string>() { "TRA", "ADJ" };

		#endregion Factory Instance Attributes.

		public BaseCusPermitHeader CreatePermit(int noOfTransactions, string countryCode = "ZA", string category = "CUM")
		{
			PermitCounter++;
			var importer = CreateImporter();

			var permit = Factory.New<BaseCusPermitHeader>();
			permit.CPH_OH_PermitHolder = importer.PK;
			permit.CPH_Number = "PN-" + PermitCounter.ToString("0#", CultureInfo.InvariantCulture);
			permit.CPH_RN_NKCountryCode = countryCode;

			permit.CPH_StartDate = PermitTestDate.AddMonths((-1 * PermitCounter) - 6);
			permit.CPH_EndDate = PermitTestDate.AddMonths(PermitCounter + 6);

			permit.CPH_Type = GetValueFromList(PermitCounter, PermitTypeList);
			permit.CPH_SubType = GetValueFromList(PermitCounter, PermitSubTypeList);
			permit.CPH_QtyValIndicator = GetValueFromList(PermitCounter, QtyValIndicatorList);
			permit.CPH_UnitOfMeasure = GetValueFromList(PermitCounter, UnitOfMeasureList);

			for (int a = 0; a < noOfTransactions; a++)
			{
				CreateTransaction(permit, category, makeOpeningBalance: a == 0);
			}

			return permit;
		}

		void CreateTransaction(BaseCusPermitHeader permit, string category, bool makeOpeningBalance)
		{
			TransactionCounter++;
			var transaction = permit.CusPermitLineTransactions.AddNew();

			if (makeOpeningBalance)
			{
				transaction.CPL_TransactionType = "OBL";
				transaction.CPL_TransactionDate = permit.CPH_StartDate.AddDays(1);
				transaction.CPL_TranValue = 5000 + TransactionCounter;
				transaction.CPL_TranQty = 3000 + TransactionCounter;
			}
			else
			{
				transaction.CPL_TransactionType = GetValueFromList(TransactionCounter, TransactionTypeList);
				transaction.CPL_TransactionDate = permit.CPH_StartDate.AddDays(2 * TransactionCounter);
				transaction.CPL_TranValue = -2 * TransactionCounter;
				transaction.CPL_TranQty = -3 * TransactionCounter;
			}

			transaction.CPL_TransactionCategory = category;
			transaction.CPL_Reference = "REF-" + TransactionCounter.ToString("0#", CultureInfo.InvariantCulture);
			transaction.CPL_Comment = "COMMENT-" + TransactionCounter.ToString("0#", CultureInfo.InvariantCulture);
		}

		string GetValueFromList(int counter, List<string> list)
		{
			return list[counter % list.Count];
		}

		OrgHeader CreateImporter()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMP" + PermitCounter.ToString("0#", CultureInfo.InvariantCulture);
			importer.OH_FullName = importer.OH_Code + "_FullName";
			return importer;
		}
	}
}
