using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using System;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;

	public class CREJobComInvoiceLineWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new CREJobComInvoiceLineWrapper(null);
		}

		public void TestCREJobComInvoiceLineWrapper()
		{
			AssertNotNull(new CREJobComInvoiceLineWrapper(Factory.NewWithValidTestData<JobComInvoiceLine>()));
		}

		public void TestIsEmptyContainer()
		{
			ICREConsignmentItem wrappedInvoiceLine = new CREJobComInvoiceLineWrapper(Factory.NewWithValidTestData<JobComInvoiceLine>());
			Assert(!wrappedInvoiceLine.IsEmptyContainer);
		}

		public void TestGoodsDescription()
		{
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			testInvoiceLine.JI_Description = "TEST DESCRIPTION";
			ICREConsignmentItem wrappedInvoiceLine = new CREJobComInvoiceLineWrapper(testInvoiceLine);
			AssertEquals("TEST DESCRIPTION", wrappedInvoiceLine.GoodsDescription);
		}

		public void TestIdentifiers()
		{
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			var product1 = testInvoiceLine.CommodityProducts.AddNew();
			product1.NZ_ProductID = "PRODUCT1";
			product1.NZ_ProductIDType = "TP1";
			var product2 = testInvoiceLine.CommodityProducts.AddNew();
			product2.NZ_ProductID = "PRODUCT2";
			product2.NZ_ProductIDType = "TP2";
			ICREConsignmentItem wrappedInvoiceLine = new CREJobComInvoiceLineWrapper(testInvoiceLine);
			var identifiers = wrappedInvoiceLine.Identifiers;
			Assert(identifiers.IsCountEqualTo(2));
			Assert(identifiers.Any(x => x.CommodityNumber == "PRODUCT1" && x.CommodityType == "TP1"));
			Assert(identifiers.Any(x => x.CommodityNumber == "PRODUCT2" && x.CommodityType == "TP2"));
		}

		public void TestValue()
		{
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			testInvoiceLine.JI_LinePrice = 12.34m;
			ICREConsignmentItem wrappedInvoiceLine = new CREJobComInvoiceLineWrapper(testInvoiceLine);
			AssertEquals(12.34m, wrappedInvoiceLine.Value);
		}

		public void TestCurrency()
		{
			var testInvoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			testInvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Cambodia;
			ICREConsignmentItem wrappedInvoiceLine = new CREJobComInvoiceLineWrapper((JobComInvoiceLine)testInvoiceHeader.InvoiceLines.AddNew());
			AssertEquals(Core.Constants.CurrencyCodes.Cambodia, wrappedInvoiceLine.Currency);
		}

		public void TestClassifications()
		{
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			var classification1 = testInvoiceLine.CommodityLines.AddNew();
			classification1.NZ_Classification = "CLASSIFICATION1";
			classification1.NZ_ClassificationType = "TP1";
			var classification2 = testInvoiceLine.CommodityLines.AddNew();
			classification2.NZ_Classification = "CLASSIFICATION2";
			classification2.NZ_ClassificationType = "TP2";
			ICREConsignmentItem wrappedInvoiceLine = new CREJobComInvoiceLineWrapper(testInvoiceLine);
			var classifications = wrappedInvoiceLine.Classifications;
			Assert(classifications.IsCountEqualTo(2));
			Assert(classifications.Any(x => x.Classification == "CLASSIFICATION1" && x.ClassificationTypeCode == "TP1"));
			Assert(classifications.Any(x => x.Classification == "CLASSIFICATION2" && x.ClassificationTypeCode == "TP2"));
		}

		public void TestGrossWeightInKg()
		{
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			testInvoiceLine.JI_Weight = 12.78m;
			testInvoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			ICREConsignmentItem wrappedInvoiceLine = new CREJobComInvoiceLineWrapper(testInvoiceLine);
			AssertEquals(12.78m, wrappedInvoiceLine.GrossWeightInKg);
		}

		public void TestGoodsOriginCountry()
		{
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			testInvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Comoros;
			ICREConsignmentItem wrappedInvoiceLine = new CREJobComInvoiceLineWrapper(testInvoiceLine);
			AssertEquals(Core.Constants.CountryCodes.Comoros, wrappedInvoiceLine.GoodsOriginCountry);
		}

		public void TestPackageQty()
		{
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			testInvoiceLine.JI_InvoiceQuantity = 2m;
			ICREConsignmentItem wrappedInvoiceLine = new CREJobComInvoiceLineWrapper(testInvoiceLine);
			AssertEquals(2, wrappedInvoiceLine.PackageQty);
		}

		public void TestPackageType()
		{
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			testInvoiceLine.JI_InvoiceUQ = "NMB";
			ICREConsignmentItem wrappedInvoiceLine = new CREJobComInvoiceLineWrapper(testInvoiceLine);
			AssertEquals("NMB", wrappedInvoiceLine.PackageType);
		}

		public void TestContainerNumber()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testContainer1 = testDeclaration.CusContainers.AddNew();
			testContainer1.CO_ContainerNumber = "CONTAINER1";
			var testContainer2 = testDeclaration.CusContainers.AddNew();
			testContainer2.CO_ContainerNumber = "CONTAINER2";

			var testInvoiceHeader = testDeclaration.Invoices.AddNew();
			var testInvoiceLine = testInvoiceHeader.JobComInvoiceLines.AddNew();
			testInvoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().ForEach(x => x.IsForInvoiceLine = ZBool.True);

			ICREConsignmentItem wrappedInvoiceLine = new CREJobComInvoiceLineWrapper(testInvoiceLine);
			AssertEquals("CONTAINER1", wrappedInvoiceLine.ContainerNumber);
		}

		public void TestUNDGHazardousGoodsCode()
		{
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			ICREConsignmentItem wrappedInvoiceLine = new CREJobComInvoiceLineWrapper(testInvoiceLine);
			Assert(wrappedInvoiceLine.UNDGHazardousGoodsCode.IsEmpty);
			var undg = testInvoiceLine.UNDGs.AddNew();
			undg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			AssertEquals("0004a", wrappedInvoiceLine.UNDGHazardousGoodsCode);
		}
	}
}
