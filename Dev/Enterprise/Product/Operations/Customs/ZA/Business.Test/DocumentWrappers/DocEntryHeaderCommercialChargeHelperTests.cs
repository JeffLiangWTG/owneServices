using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	sealed class DocEntryHeaderCommercialChargeHelperTests : TestCaseWithFactory
	{
		public void TestConsolidation_of_InvoiceLineCharges_into_ChargesCollection()
		{
			var countryCode = Core.Constants.CountryCodes.SouthAfrica;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var scenarios = new List<InvoiceLineCharges_Consolidation_TestScenario>()
				{
					GetInvoiceLineCharges_Consolidation_TestScenario_01(),
					GetInvoiceLineCharges_Consolidation_TestScenario_02()
				};

				foreach (var scenario in scenarios)
				{
					var invoiceLines = scenario.GetInvoiceLinesCollection();

					var helper = new DocEntryHeaderCommercialChargeHelper(Factory);
					helper.PrepareDocEntryHeaderCommercialChargeData(invoiceLines);
					var producedChargeCollection = helper.GetDocEntryHeaderCommercialChargesCollection();

					var expectedChargeList = scenario.GetExpectedChargeList();
					CompareCharges(scenario.Name, expectedChargeList, producedChargeCollection);
				}
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

		InvoiceLineCharges_Consolidation_TestScenario GetInvoiceLineCharges_Consolidation_TestScenario_01()
		{
			var scenario = new InvoiceLineCharges_Consolidation_TestScenario(Factory);
			scenario.Name = "Consolidation_of_InvoiceLineCharges_into_ChargesCollection_TestScenario_01";

			scenario.AddCharge("INV01", 1, 0, 1, 0, "ZAR", 100);
			scenario.AddCharge("INV01", 1, 0, 1, 0, "ZAR", 200);
			scenario.AddCharge("INV01", 1, 0, 0, 0, "ZAR", 400);

			scenario.AddExpected("INV01", 1, 0, 1, 0, "ZAR", 300);
			scenario.AddExpected("INV01", 1, 0, 0, 0, "ZAR", 400);

			return scenario;
		}

		InvoiceLineCharges_Consolidation_TestScenario GetInvoiceLineCharges_Consolidation_TestScenario_02()
		{
			var scenario = new InvoiceLineCharges_Consolidation_TestScenario(Factory);
			scenario.Name = "Consolidation_of_InvoiceLineCharges_into_ChargesCollection_TestScenario_02";

			scenario.AddCharge("ABC", 1, 0, 1, 1, "USD", 10);
			scenario.AddCharge("ABC", 1, 0, 1, 1, "USD", 20);
			scenario.AddCharge("ABC", 1, 0, 1, 1, "AUD", 30);
			scenario.AddCharge("ABC", 1, 0, 1, 1, "AUD", 40);
			scenario.AddCharge("ABC", 0, 1, 1, 1, "ZAR", 50);
			scenario.AddCharge("ABC", 0, 0, 0, 0, "ZAR", 60);
			scenario.AddCharge("XYZ", 1, 0, 1, 1, "USD", 9);
			scenario.AddCharge("XYZ", 1, 0, 1, 1, "AUD", 6);
			scenario.AddCharge("XYZ", 0, 1, 1, 1, "ZAR", 3);
			scenario.AddCharge("XYZ", 0, 0, 0, 0, "ZAR", 2);
			scenario.AddCharge("XYZ", 0, 0, 0, 0, "ZAR", 4);
			scenario.AddCharge("XYZ", 0, 0, 0, 0, "AUD", 1);
			scenario.AddCharge("XYZ", 0, 0, 0, 0, "AUD", 6);

			scenario.AddExpected("ABC", 1, 0, 1, 1, "USD", 30);
			scenario.AddExpected("ABC", 1, 0, 1, 1, "AUD", 70);
			scenario.AddExpected("ABC", 0, 1, 1, 1, "ZAR", 50);
			scenario.AddExpected("ABC", 0, 0, 0, 0, "ZAR", 60);
			scenario.AddExpected("XYZ", 1, 0, 1, 1, "USD", 9);
			scenario.AddExpected("XYZ", 1, 0, 1, 1, "AUD", 6);
			scenario.AddExpected("XYZ", 0, 1, 1, 1, "ZAR", 3);
			scenario.AddExpected("XYZ", 0, 0, 0, 0, "ZAR", 6);
			scenario.AddExpected("XYZ", 0, 0, 0, 0, "AUD", 7);

			return scenario;
		}

		class InvoiceLineCharges_Consolidation_TestScenario
		{
			public InvoiceLineCharges_Consolidation_TestScenario(BusinessObjectFactory factory)
			{
				this.factory = factory;
				expectedCharges = new DocEntryHeaderCommercialChargesCollection(this.factory);
			}

			public void AddCharge(string invoiceNo, int isFreight, int isInsurance, int dutiable, int includedInLines, string currencyCode, decimal amount)
			{
				if (!invoiceLookup.ContainsKey(invoiceNo))
				{
					var invoiceHeader = factory.New<JobComInvoiceHeader>();
					invoiceHeader.JZ_InvoiceNumber = invoiceNo;
					invoiceHeader.JZ_InvoiceDate = ZDateTime.Now;
					invoiceLookup.Add(invoiceNo, invoiceHeader);
				}

				var invHeader = invoiceLookup[invoiceNo];
				var invLine = invHeader.InvoiceLines.AddNew();
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
			}

			public void AddExpected(string invoiceNo, int isFreight, int isInsurance, int dutiable, int includedInLines, string currencyCode, decimal amount)
			{
				if (!expectedInvoiceLookup.ContainsKey(invoiceNo))
				{
					var invoiceHeader = factory.New<JobComInvoiceHeader>();
					invoiceHeader.JZ_InvoiceNumber = invoiceNo;
					invoiceHeader.JZ_InvoiceDate = ZDateTime.Now.AddDays(-2);
					expectedInvoiceLookup.Add(invoiceNo, invoiceHeader);
				}

				var invHeader = expectedInvoiceLookup[invoiceNo];
				var invLine = invHeader.InvoiceLines.AddNew();
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

				expectedCharges.Add(DocEntryHeaderCommercialCharge.New(charge, factory));
			}

			public List<BaseJobComInvoiceLine> GetInvoiceLinesCollection()
			{
				var invoiceLines = new List<BaseJobComInvoiceLine>();

				foreach (var invoice in invoiceLookup.Values)
				{
					foreach (BaseJobComInvoiceLine invLine in invoice.InvoiceLines)
					{
						invoiceLines.Add(invLine);
					}
				}
				return invoiceLines;
			}

			public DocEntryHeaderCommercialChargesCollection GetExpectedChargeList()
			{
				return expectedCharges;
			}

			public string Name;
			readonly BusinessObjectFactory factory;
			readonly Dictionary<string, JobComInvoiceHeader> invoiceLookup = new Dictionary<string, JobComInvoiceHeader>();
			readonly Dictionary<string, JobComInvoiceHeader> expectedInvoiceLookup = new Dictionary<string, JobComInvoiceHeader>();
			readonly DocEntryHeaderCommercialChargesCollection expectedCharges;
		}
	}
}
