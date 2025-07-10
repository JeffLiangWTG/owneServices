using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCusEntryHeader))]
	class DocCusEntryHeaderTest : DocBaseCusEntryHeaderAbstractTest<CusEntryHeader, DocCusEntryHeader>
	{
		#region Individual EntryLines

		public void TestIndividualEntryLines()
		{
			AssertNotNull("Entry Line One is not null", EntryHeaderWrapper.FirstEntryLine);
			AssertNull("Entry Line Two is null", EntryHeaderWrapper.SecondEntryLine);
			AssertNull("Entry Line Three is null", EntryHeaderWrapper.ThirdEntryLine);
			AssertNull("Entry Line Four is null", EntryHeaderWrapper.FourthEntryLine);
			AssertNull("Entry Line Five is null", EntryHeaderWrapper.FifthEntryLine);
			AssertNull("Entry Line Six is null", EntryHeaderWrapper.SixthEntryLine);
			AssertNull("Entry Line Seven is null", EntryHeaderWrapper.SeventhEntryLine);

			entryHeader.MergedLines.AddNew();
			AssertNotNull("Entry Line One is not null", EntryHeaderWrapper.FirstEntryLine);
			AssertNotNull("Entry Line Two is not null", EntryHeaderWrapper.SecondEntryLine);
			AssertNull("Entry Line Three is null", EntryHeaderWrapper.ThirdEntryLine);
			AssertNull("Entry Line Four is null", EntryHeaderWrapper.FourthEntryLine);
			AssertNull("Entry Line Five is null", EntryHeaderWrapper.FifthEntryLine);
			AssertNull("Entry Line Six is null", EntryHeaderWrapper.SixthEntryLine);
			AssertNull("Entry Line Seven is null", EntryHeaderWrapper.SeventhEntryLine);

			entryHeader.MergedLines.AddNew();
			AssertNotNull("Entry Line One is not null", EntryHeaderWrapper.FirstEntryLine);
			AssertNotNull("Entry Line Two is not null", EntryHeaderWrapper.SecondEntryLine);
			AssertNotNull("Entry Line Three not is null", EntryHeaderWrapper.ThirdEntryLine);
			AssertNull("Entry Line Four is null", EntryHeaderWrapper.FourthEntryLine);
			AssertNull("Entry Line Five is null", EntryHeaderWrapper.FifthEntryLine);
			AssertNull("Entry Line Six is null", EntryHeaderWrapper.SixthEntryLine);
			AssertNull("Entry Line Seven is null", EntryHeaderWrapper.SeventhEntryLine);

			entryHeader.MergedLines.AddNew();
			AssertNotNull("Entry Line One is not null", EntryHeaderWrapper.FirstEntryLine);
			AssertNotNull("Entry Line Two is not null", EntryHeaderWrapper.SecondEntryLine);
			AssertNotNull("Entry Line Three is not null", EntryHeaderWrapper.ThirdEntryLine);
			AssertNotNull("Entry Line Four is not null", EntryHeaderWrapper.FourthEntryLine);
			AssertNull("Entry Line Five is null", EntryHeaderWrapper.FifthEntryLine);
			AssertNull("Entry Line Six is null", EntryHeaderWrapper.SixthEntryLine);
			AssertNull("Entry Line Seven is null", EntryHeaderWrapper.SeventhEntryLine);

			entryHeader.MergedLines.AddNew();
			AssertNotNull("Entry Line One is not null", EntryHeaderWrapper.FirstEntryLine);
			AssertNotNull("Entry Line Two is not null", EntryHeaderWrapper.SecondEntryLine);
			AssertNotNull("Entry Line Three is not null", EntryHeaderWrapper.ThirdEntryLine);
			AssertNotNull("Entry Line Four is not null", EntryHeaderWrapper.FourthEntryLine);
			AssertNotNull("Entry Line Five is not null", EntryHeaderWrapper.FifthEntryLine);
			AssertNull("Entry Line Six is null", EntryHeaderWrapper.SixthEntryLine);
			AssertNull("Entry Line Seven is null", EntryHeaderWrapper.SeventhEntryLine);

			entryHeader.MergedLines.AddNew();
			AssertNotNull("Entry Line One is not null", EntryHeaderWrapper.FirstEntryLine);
			AssertNotNull("Entry Line Two is not null", EntryHeaderWrapper.SecondEntryLine);
			AssertNotNull("Entry Line Three is not null", EntryHeaderWrapper.ThirdEntryLine);
			AssertNotNull("Entry Line Four is not null", EntryHeaderWrapper.FourthEntryLine);
			AssertNotNull("Entry Line Five is not null", EntryHeaderWrapper.FifthEntryLine);
			AssertNotNull("Entry Line Six is not null", EntryHeaderWrapper.SixthEntryLine);
			AssertNull("Entry Line Seven is null", EntryHeaderWrapper.SeventhEntryLine);

			entryHeader.MergedLines.AddNew();
			AssertNotNull("Entry Line One is not null", EntryHeaderWrapper.FirstEntryLine);
			AssertNotNull("Entry Line Two is not null", EntryHeaderWrapper.SecondEntryLine);
			AssertNotNull("Entry Line Three is not null", EntryHeaderWrapper.ThirdEntryLine);
			AssertNotNull("Entry Line Four is not null", EntryHeaderWrapper.FourthEntryLine);
			AssertNotNull("Entry Line Five is not null", EntryHeaderWrapper.FifthEntryLine);
			AssertNotNull("Entry Line Six is not null", EntryHeaderWrapper.SixthEntryLine);
			AssertNotNull("Entry Line Seven is not null", EntryHeaderWrapper.SeventhEntryLine);
		}

		#endregion

		#region Wrapper Fields

		public void TestDeclaration()
		{
			AssertNotNull("Declaration", EntryHeaderWrapper.Declaration);
			AssertEquals("Declaration is of type DocDeclaration", typeof(DocDeclaration), EntryHeaderWrapper.Declaration.GetType());
		}

		public void TestFirstEntryLine()
		{
			AssertEquals("CusEntryHeader", entryHeader.PK, ((BusinessObject)EntryHeaderWrapper.WrappedObject).PK);
			AssertNotNull("First Entry Line", EntryHeaderWrapper.FirstEntryLine);
			AssertEquals("FirstEntryLine is of type DocCusEntryLine", typeof(DocCusEntryLine), EntryHeaderWrapper.FirstEntryLine.GetType());
		}

		[TestDate(2005, 10, 09)]
		public void TestAssessmentDate()
		{
			AssertEquals(entryHeader.EntryInstructionAssessmentDate, EntryHeaderWrapper.AssessmentDate);
			AssertEquals("2005-10-09", EntryHeaderWrapper.AssessmentDate.Date.ToString("yyyy-MM-dd"));
		}

		#endregion

		#region Collections

		public void TestEntryLines()
		{
			AssertEquals("One entry in EntryLine", 1, EntryHeaderWrapper.EntryLines.Count);

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals("Two entries in EntryLine", 2, EntryHeaderWrapper.EntryLines.Count);
		}

		public void TestEntryLinesWithoutFirstLineForExport()
		{
			Dictionary<string, object> hashtable = new Dictionary<string, object>();
			hashtable.Add(DocumentEngineIntegration.Constants.TemplateDefined.DocumentDirection, "DEP");
			DocCusEntryHeader currentWrapper = EntryHeaderWrapper;
			currentWrapper.SetTemplateConstants(hashtable);

			AssertEquals("No entry in EntryLine", 0, currentWrapper.EntryLinesWithoutFirstLine.Count);

			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			currentWrapper = DocCusEntryHeader.New(entryHeader, Factory);
			AssertEquals("Three entries in EntryLine", 1, currentWrapper.EntryLinesWithoutFirstLine.Count);

			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			JobComInvoiceLine invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine3 = entryHeader.MergedLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			JobComInvoiceLine invoiceLine4 = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine4 = entryHeader.MergedLines.AddNew();
			invoiceLine4.JI_CL = entryLine4.PK;
			currentWrapper = DocCusEntryHeader.New(entryHeader, Factory);
			AssertEquals("Six entries in EntryLine", 4, currentWrapper.EntryLinesWithoutFirstLine.Count);
		}

		public void TestEntryLinesWithoutFirstLineForImport()
		{
			Dictionary<string, object> hashtable = new Dictionary<string, object>();
			hashtable.Add(DocumentEngineIntegration.Constants.TemplateDefined.DocumentDirection, "ARV");
			DocCusEntryHeader currentWrapper = EntryHeaderWrapper;
			currentWrapper.SetTemplateConstants(hashtable);

			AssertEquals("No entry in EntryLine", 0, currentWrapper.EntryLinesWithoutFirstLine.Count);

			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			currentWrapper = DocCusEntryHeader.New(entryHeader, Factory);
			AssertEquals("Three entries in EntryLine", 1, currentWrapper.EntryLinesWithoutFirstLine.Count);

			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			JobComInvoiceLine invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine3 = entryHeader.MergedLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			JobComInvoiceLine invoiceLine4 = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine4 = entryHeader.MergedLines.AddNew();
			invoiceLine4.JI_CL = entryLine4.PK;
			currentWrapper = DocCusEntryHeader.New(entryHeader, Factory);
			AssertEquals("Six entries in EntryLine", 4, currentWrapper.EntryLinesWithoutFirstLine.Count);

			JobComInvoiceLine invoiceLine5 = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine5 = entryHeader.MergedLines.AddNew();
			invoiceLine5.JI_CL = entryLine5.PK;
			currentWrapper = DocCusEntryHeader.New(entryHeader, Factory);
			AssertEquals("Six entries in EntryLine", 5, currentWrapper.EntryLinesWithoutFirstLine.Count);

			JobComInvoiceLine invoiceLine6 = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine6 = entryHeader.MergedLines.AddNew();
			invoiceLine6.JI_CL = entryLine6.PK;
			currentWrapper = DocCusEntryHeader.New(entryHeader, Factory);
			AssertEquals("Six entries in EntryLine", 6, currentWrapper.EntryLinesWithoutFirstLine.Count);
		}

		public void TestEntryLinesWithoutFirstLineCount()
		{
			Dictionary<string, object> hashtable = new Dictionary<string, object>();
			hashtable.Add(DocumentEngineIntegration.Constants.TemplateDefined.DocumentDirection, "ARV");
			DocCusEntryHeader currentWrapper = EntryHeaderWrapper;
			currentWrapper.SetTemplateConstants(hashtable);

			AssertEquals("One entry lines in EntryHeader", 1, entryHeader.MergedLines.Count);
			AssertEquals("No entry in EntryLine", 0, currentWrapper.EntryLinesWithoutFirstLine.Count);
			entryHeader.MergedLines.AddNew();
			currentWrapper = EntryHeaderWrapper; //Reconstruct EntryHeaderWrapper

			AssertEquals("Two entry lines in EntryHeader", 2, entryHeader.MergedLines.Count);
			AssertEquals("One entry lines in EntryHeaderWrapper.EntryLinesWithoutFirstLine", 1, currentWrapper.EntryLinesWithoutFirstLine.Count);
		}

		public void TestInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			var headerWrapper = CreateEntryHeaderWrapper(entryHeader);
			AssertEquals("InvoiceLines", 1, headerWrapper.InvoiceLines.Count);
		}

		public void TestAllCharges_and_Charges_Collections()
		{
			var scenarios = new List<AllCharges_and_Charges_TestScenario>()
			{
				Get_AllCharges_and_Charges_TestScenario_01(),
				Get_AllCharges_and_Charges_TestScenario_02(),
				Get_AllCharges_and_Charges_TestScenario_03()
			};

			foreach (var scenario in scenarios)
			{
				var cusEntryHeader = scenario.InputChargeListTestHelper.GetCusEntryHeader();
				var docCusEntryHeader = DocCusEntryHeader.New(cusEntryHeader, Factory);

				var expectedAllChargesList = scenario.ExpectedAllChargesListTestHelper.GetExpectedListOfCharges();
				CompareCharges(scenario.Name + " - AllCharges", expectedAllChargesList, docCusEntryHeader.AllCharges);

				var expectedChargesList = scenario.ExpectedChargesListTestHelper.GetExpectedListOfCharges();
				CompareCharges(scenario.Name + " - Charges", expectedChargesList, docCusEntryHeader.Charges);
			}
		}

		void CompareCharges(string assertMessage, DocEntryHeaderCommercialChargesCollection expectedCharges, DocEntryHeaderCommercialChargesCollection producedCharges)
		{
			AssertEquals(assertMessage, expectedCharges.Count, producedCharges.Count);

			var expectedList = new List<DocEntryHeaderCommercialCharge>();
			var producedList = new List<DocEntryHeaderCommercialCharge>();

			expectedCharges.CopyToList(expectedList);
			producedCharges.CopyToList(producedList);

			foreach (DocEntryHeaderCommercialCharge charge in expectedList)
			{
				var producedCharge = producedList.FirstOrDefault(x => x.InvoiceNumber == charge.InvoiceNumber
																&& x.IsFreight == charge.IsFreight
																&& x.IsInsurance == charge.IsInsurance
																&& x.IsDutiable == charge.IsDutiable
																&& x.IsIncludedInLines == charge.IsIncludedInLines
																&& x.CurrencyCode == charge.CurrencyCode
																&& x.ChargeAmount == charge.ChargeAmount);
				Assert(assertMessage, producedCharge != null);
				producedList.Remove(producedCharge);
			}

			AssertEquals(assertMessage, 0, producedList.Count);
		}

		AllCharges_and_Charges_TestScenario Get_AllCharges_and_Charges_TestScenario_01()
		{
			var scenario = new AllCharges_and_Charges_TestScenario();
			scenario.Name = "AllCharges_and_Charges_TestScenario_01";
			{
				var inputChargesHelper = new ChargeListTestHelper(Factory);

				inputChargesHelper.Add("ABC", 1, 0, 1, 1, "USD", 900);
				inputChargesHelper.Add("ABC", 1, 0, 1, 1, "AUD", 600);
				inputChargesHelper.Add("ABC", 0, 1, 1, 1, "ZAR", 300);
				inputChargesHelper.Add("ABC", 0, 0, 0, 0, "ZAR", 200);
				inputChargesHelper.Add("DEF", 1, 0, 1, 1, "USD", 9);
				inputChargesHelper.Add("DEF", 1, 0, 1, 1, "AUD", 6);
				inputChargesHelper.Add("DEF", 0, 1, 1, 1, "ZAR", 3);
				inputChargesHelper.Add("DEF", 0, 0, 0, 0, "ZAR", 2);
				inputChargesHelper.Add("DEF", 0, 0, 0, 0, "AUD", 1);
				inputChargesHelper.Add("GHI", 1, 1, 1, 1, "AUD", 1000);

				scenario.InputChargeListTestHelper = inputChargesHelper;
			}

			{
				var expectedAllChargesHelper = new ChargeListTestHelper(Factory);

				expectedAllChargesHelper.Add("ABC", 1, 0, 1, 1, "USD", 900);
				expectedAllChargesHelper.Add("ABC", 1, 0, 1, 1, "AUD", 600);
				expectedAllChargesHelper.Add("ABC", 0, 1, 1, 1, "ZAR", 300);
				expectedAllChargesHelper.Add("ABC", 0, 0, 0, 0, "ZAR", 200);
				expectedAllChargesHelper.Add("DEF", 1, 0, 1, 1, "USD", 9);
				expectedAllChargesHelper.Add("DEF", 1, 0, 1, 1, "AUD", 6);
				expectedAllChargesHelper.Add("DEF", 0, 1, 1, 1, "ZAR", 3);
				expectedAllChargesHelper.Add("DEF", 0, 0, 0, 0, "ZAR", 2);
				expectedAllChargesHelper.Add("DEF", 0, 0, 0, 0, "AUD", 1);
				expectedAllChargesHelper.Add("GHI", 1, 1, 1, 1, "AUD", 1000);

				scenario.ExpectedAllChargesListTestHelper = expectedAllChargesHelper;
			}

			{
				var expectedChargesHelper = new ChargeListTestHelper(Factory);

				expectedChargesHelper.Add("ABC", 1, 0, 1, 1, "USD", 900);
				expectedChargesHelper.Add("ABC", 1, 0, 1, 1, "AUD", 600);
				expectedChargesHelper.Add("ABC", 0, 1, 1, 1, "ZAR", 300);
				expectedChargesHelper.Add("ABC", 0, 0, 0, 0, "ZAR", 200);
				expectedChargesHelper.Add("DEF", 1, 0, 1, 1, "USD", 9);
				expectedChargesHelper.Add("DEF", 1, 0, 1, 1, "AUD", 6);
				expectedChargesHelper.Add("DEF", 0, 1, 1, 1, "ZAR", 3);
				expectedChargesHelper.Add("DEF", 0, 0, 0, 0, "ZAR", 2);
				expectedChargesHelper.Add("DEF", 0, 0, 0, 0, "AUD", 1);
				expectedChargesHelper.Add("GHI", 1, 1, 1, 1, "AUD", 1000);

				scenario.ExpectedChargesListTestHelper = expectedChargesHelper;
			}
			return scenario;
		}

		AllCharges_and_Charges_TestScenario Get_AllCharges_and_Charges_TestScenario_02()
		{
			var scenario = new AllCharges_and_Charges_TestScenario();
			scenario.Name = "AllCharges_and_Charges_TestScenario_02";
			{
				var inputChargesHelper = new ChargeListTestHelper(Factory);

				inputChargesHelper.Add("INV01", 1, 0, 1, 0, "ZAR", 100);
				inputChargesHelper.Add("INV01", 1, 0, 1, 0, "ZAR", 200);
				inputChargesHelper.Add("INV01", 1, 0, 0, 0, "ZAR", 400);

				scenario.InputChargeListTestHelper = inputChargesHelper;
			}

			{
				var expectedAllChargesHelper = new ChargeListTestHelper(Factory);

				expectedAllChargesHelper.Add("INV01", 1, 0, 1, 0, "ZAR", 300);
				expectedAllChargesHelper.Add("INV01", 1, 0, 0, 0, "ZAR", 400);

				scenario.ExpectedAllChargesListTestHelper = expectedAllChargesHelper;
			}

			{
				var expectedChargesHelper = new ChargeListTestHelper(Factory);

				expectedChargesHelper.Add("INV01", 1, 0, 1, 0, "ZAR", 300);
				expectedChargesHelper.Add("INV01", 1, 0, 0, 0, "ZAR", 400);

				scenario.ExpectedChargesListTestHelper = expectedChargesHelper;
			}
			return scenario;
		}

		AllCharges_and_Charges_TestScenario Get_AllCharges_and_Charges_TestScenario_03()
		{
			var scenario = new AllCharges_and_Charges_TestScenario();
			scenario.Name = "AllCharges_and_Charges_TestScenario_03";
			{
				var inputChargesHelper = new ChargeListTestHelper(Factory);

				inputChargesHelper.Add("ABC", 1, 0, 1, 1, "USD", 10);
				inputChargesHelper.Add("ABC", 1, 0, 1, 1, "USD", 20);
				inputChargesHelper.Add("ABC", 1, 0, 1, 1, "AUD", 30);
				inputChargesHelper.Add("ABC", 1, 0, 1, 1, "AUD", 40);
				inputChargesHelper.Add("ABC", 0, 1, 1, 1, "ZAR", 50);
				inputChargesHelper.Add("ABC", 0, 0, 0, 0, "ZAR", 60);
				inputChargesHelper.Add("XYZ", 1, 0, 1, 1, "USD", 9);
				inputChargesHelper.Add("XYZ", 1, 0, 1, 1, "AUD", 6);
				inputChargesHelper.Add("XYZ", 0, 1, 1, 1, "ZAR", 3);
				inputChargesHelper.Add("XYZ", 0, 0, 0, 0, "ZAR", 2);
				inputChargesHelper.Add("XYZ", 0, 0, 0, 0, "ZAR", 4);
				inputChargesHelper.Add("XYZ", 0, 0, 0, 0, "AUD", 1);
				inputChargesHelper.Add("XYZ", 0, 0, 0, 0, "AUD", 6);

				scenario.InputChargeListTestHelper = inputChargesHelper;
			}

			{
				var expectedAllChargesHelper = new ChargeListTestHelper(Factory);

				expectedAllChargesHelper.Add("ABC", 1, 0, 1, 1, "USD", 30);
				expectedAllChargesHelper.Add("ABC", 1, 0, 1, 1, "AUD", 70);
				expectedAllChargesHelper.Add("ABC", 0, 1, 1, 1, "ZAR", 50);
				expectedAllChargesHelper.Add("ABC", 0, 0, 0, 0, "ZAR", 60);
				expectedAllChargesHelper.Add("XYZ", 1, 0, 1, 1, "USD", 9);
				expectedAllChargesHelper.Add("XYZ", 1, 0, 1, 1, "AUD", 6);
				expectedAllChargesHelper.Add("XYZ", 0, 1, 1, 1, "ZAR", 3);
				expectedAllChargesHelper.Add("XYZ", 0, 0, 0, 0, "ZAR", 6);
				expectedAllChargesHelper.Add("XYZ", 0, 0, 0, 0, "AUD", 7);

				scenario.ExpectedAllChargesListTestHelper = expectedAllChargesHelper;
			}

			{
				var expectedChargesHelper = new ChargeListTestHelper(Factory);

				expectedChargesHelper.Add("ABC", 1, 0, 1, 1, "USD", 30);
				expectedChargesHelper.Add("ABC", 1, 0, 1, 1, "AUD", 70);
				expectedChargesHelper.Add("ABC", 0, 1, 1, 1, "ZAR", 50);
				expectedChargesHelper.Add("ABC", 0, 0, 0, 0, "ZAR", 60);
				expectedChargesHelper.Add("XYZ", 1, 0, 1, 1, "USD", 9);
				expectedChargesHelper.Add("XYZ", 1, 0, 1, 1, "AUD", 6);
				expectedChargesHelper.Add("XYZ", 0, 1, 1, 1, "ZAR", 3);
				expectedChargesHelper.Add("XYZ", 0, 0, 0, 0, "ZAR", 6);
				expectedChargesHelper.Add("XYZ", 0, 0, 0, 0, "AUD", 7);

				scenario.ExpectedChargesListTestHelper = expectedChargesHelper;
			}
			return scenario;
		}

		class AllCharges_and_Charges_TestScenario
		{
			public string Name;
			public ChargeListTestHelper InputChargeListTestHelper;
			public ChargeListTestHelper ExpectedAllChargesListTestHelper;
			public ChargeListTestHelper ExpectedChargesListTestHelper;
		}

		class ChargeListTestHelper
		{
			public ChargeListTestHelper(BusinessObjectFactory factory)
			{
				this.factory = factory;
				cusEntryHeader = this.factory.New<CusEntryHeader>();
				cusEntryHeader.MergedLines.AddNew();
			}

			public void Add(string invoiceNo, int isFreight, int isInsurance, int dutiable, int includedInLines, string currencyCode, decimal amount)
			{
				if (!invoiceLookup.ContainsKey(invoiceNo))
				{
					var invoiceHeader = factory.New<JobComInvoiceHeader>();
					invoiceHeader.JZ_InvoiceNumber = invoiceNo;
					invoiceHeader.JZ_InvoiceDate = ZDateTime.Now;
					var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
					invoiceLookup.Add(invoiceNo, invoiceHeader);
					cusEntryHeader.MergedLines[0].InvoiceLines.Add(invoiceLine);
				}

				var invHeader = invoiceLookup[invoiceNo];
				var invLine = invHeader.InvoiceLines[0];
				var charge = invLine.ApportionedCharges.AddNew();

				if (isFreight == 1)
				{
					charge.J7_ChargeType = "OFT";
				}

				if (isInsurance == 1)
				{
					charge.J7_ChargeType = "ONS";
				}

				if (isFreight != 1 && isInsurance != 1)
				{
					charge.J7_ChargeType = "OTH";
				}

				charge.J7_IsDutiable = dutiable == 1;
				charge.J7_IsNotIncludedInInvoice = !(includedInLines == 1);
				charge.J7_RX_NKCurrency = currencyCode;
				charge.J7_Amount = amount;

				apportionedChargesList.Add(charge);
			}

			public CusEntryHeader GetCusEntryHeader()
			{
				return cusEntryHeader;
			}

			public DocEntryHeaderCommercialChargesCollection GetExpectedListOfCharges()
			{
				var chargeCollection = new DocEntryHeaderCommercialChargesCollection(factory);

				foreach (Enterprise.Customs.Business.BaseInvoiceLineApportionedCharge charge in apportionedChargesList)
				{
					chargeCollection.Add(DocEntryHeaderCommercialCharge.New(charge, factory));
				}
				return chargeCollection;
			}

			readonly BusinessObjectFactory factory;
			readonly CusEntryHeader cusEntryHeader;
			readonly Dictionary<string, JobComInvoiceHeader> invoiceLookup = new Dictionary<string, JobComInvoiceHeader>();
			readonly List<Enterprise.Customs.Business.BaseInvoiceLineApportionedCharge> apportionedChargesList = new List<Enterprise.Customs.Business.BaseInvoiceLineApportionedCharge>();
		}

		#endregion

		#region ZString Fields

		public void TestUniqueConsignmentNumber()
		{
			AssertEquals("UniqueConsignmentNumber", entryHeader.UniqueConsignmentReference, EntryHeaderWrapper.UniqueConsignmentNumber);
		}

		public void TestPageNumber()
		{
			Dictionary<string, object> hashtable = new Dictionary<string, object>();
			hashtable.Add(DocumentEngineIntegration.Constants.TemplateDefined.DocumentDirection, "DEP");
			DocCusEntryHeader currentWrapper = EntryHeaderWrapper;
			currentWrapper.SetTemplateConstants(hashtable);

			AssertEquals("No Page Number", ZString.Empty, currentWrapper.PageNumber);

			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			currentWrapper = DocCusEntryHeader.New(entryHeader, Factory);
			AssertEquals("Page number", "Page 1 of 2", currentWrapper.PageNumber);

			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			JobComInvoiceLine invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine3 = entryHeader.MergedLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			JobComInvoiceLine invoiceLine4 = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine4 = entryHeader.MergedLines.AddNew();
			invoiceLine4.JI_CL = entryLine4.PK;
			currentWrapper = DocCusEntryHeader.New(entryHeader, Factory);
			AssertEquals("Page number", "Page 1 of 3", currentWrapper.PageNumber);
		}

		public void TestCustomsProcedureCode()
		{
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "11";
			invoiceLine.JI_CEI = testInst.PK;
			AssertEquals("11", EntryHeaderWrapper.CustomsProcedureCode);
		}

		public void TestLRN()
		{
			MockCusEntryHeader.Setup(m => m.CH_BGMReference).Returns("LRN123");
			AssertEquals("LRN", "LRN123", MockCusEntryHeaderWrapper.LRN);
		}

		public void TestMRN()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("JSA201610101234567", ZDateTime.Today);
			var headerWrapper = CreateEntryHeaderWrapper(entryHeader);
			AssertEquals("MRN", "JSA201610101234567", headerWrapper.MRN);
		}

		public void TestAgent()
		{
			OrgHeader agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.OH_FullName = "MyAgent";
			agent.SetAgentCode(RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.SouthAfrica), "ABCDE");

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_AGTCode = "ABCDE";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var headerWrapper = CreateEntryHeaderWrapper(entryHeader);
			AssertEquals("AgentName", agent.OH_FullName, headerWrapper.Agent.Name);
		}

		public void TestImporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			OrgHeader importer = OrgHeader.New(Factory);
			importer.OH_FullName = "MyImporter";
			declaration.JE_OH_Importer = importer.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var headerWrapper = CreateEntryHeaderWrapper(entryHeader);
			AssertEquals("ImporterName - Importer", importer.OH_FullName, headerWrapper.Importer.Name);
		}

		public void TestHouseBill()
		{
			declaration.JE_HouseBill = "12345";
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = testInst.PK;

			var headerWrapper = CreateEntryHeaderWrapper(entryHeader);

			AssertEquals("House Bill", declaration.JE_HouseBill, headerWrapper.HouseBill);

			testInst.CEI_HAWBOverride = "ABCDE";
			headerWrapper = CreateEntryHeaderWrapper(entryHeader);
			AssertEquals("House Bill", testInst.CEI_HAWBOverride, headerWrapper.HouseBill);
		}

		#endregion

		#region Implementation

		protected JobComInvoiceHeader invoiceHeader;
		protected JobComInvoiceLine invoiceLine;
		protected CusEntryLine entryLine;
		protected JobDeclaration declaration;
		protected CusEntryHeader entryHeader;

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.SouthAfrica; }
		}

		protected override CusEntryHeader GetNewEntryHeader()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
				declaration.JE_ExportDate = ZDateTime.Today;
				JobComInvoiceGroupHeader invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
				invoiceHeader = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();
				invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
			}
			return entryHeader;
		}

		protected override DocCusEntryHeader CreateEntryHeaderWrapper(CusEntryHeader entryHeader)
		{
			return DocCusEntryHeader.New(entryHeader, Factory);
		}

		protected DocCusEntryHeader EntryHeaderWrapper
		{
			get { return EntryHeaderWrapperInternal; }
		}

		Mock<CusEntryHeader> MockCusEntryHeader
		{
			get
			{
				if (fmockCusEntryHeader == null)
				{
					fmockCusEntryHeader = Factory.NewMoq<CusEntryHeader>();
				}
				return fmockCusEntryHeader;
			}
		}
		Mock<CusEntryHeader> fmockCusEntryHeader;

		DocCusEntryHeader MockCusEntryHeaderWrapper
		{
			get
			{
				if (fmockCusEntryHeaderWrapper == null)
				{
					fmockCusEntryHeaderWrapper = DocCusEntryHeader.New(MockCusEntryHeader.Object, Factory);
				}
				return fmockCusEntryHeaderWrapper;
			}
		}
		DocCusEntryHeader fmockCusEntryHeaderWrapper;

		protected void CreateNote(JobDeclaration declarationInternal, ZString noteTypeDesc, ZString data, BusinessObjectFactory factory)
		{
			StmNote note = declarationInternal.Notes.AddNew();
			note.ST_Description = noteTypeDesc;
			note.ST_Table = declarationInternal.TableName;
			note.ST_ParentID = declarationInternal.PK;
			note.ST_NoteDataAsText = data;
			factory.Save();
		}

		#endregion
	}
}
