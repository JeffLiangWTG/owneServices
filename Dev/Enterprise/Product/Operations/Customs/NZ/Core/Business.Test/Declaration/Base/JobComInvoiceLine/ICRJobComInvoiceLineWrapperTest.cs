using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using System;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.Business;
	using NUnit.Framework;

	public class ICRJobComInvoiceLineWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new ICRJobComInvoiceLineWrapper(null);
		}

		public void TestICRJobComInvoiceLineWrapper()
		{
			AssertNotNull(new ICRJobComInvoiceLineWrapper(Factory.NewWithValidTestData<JobComInvoiceLine>()));
		}

		public void TestIsEmptyContainer()
		{
			IICRConsignmentItem wrappedInvoiceLine = new ICRJobComInvoiceLineWrapper(Factory.NewWithValidTestData<JobComInvoiceLine>());
			Assert(!wrappedInvoiceLine.IsEmptyContainer);
		}

		public void TestGoodsDescription()
		{
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			testInvoiceLine.JI_Description = "TEST DESCRIPTION";
			IICRConsignmentItem wrappedInvoiceLine = new ICRJobComInvoiceLineWrapper(testInvoiceLine);
			AssertEquals("TEST DESCRIPTION", wrappedInvoiceLine.GoodsDescription);
		}

		public void TestIdentityNumber()
		{
			IICRConsignmentItem wrappedInvoiceLine = new ICRJobComInvoiceLineWrapper(Factory.NewWithValidTestData<JobComInvoiceLine>());
			Assert(wrappedInvoiceLine.IdentityNumber.IsEmpty);
		}

		public void TestValue()
		{
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			testInvoiceLine.JI_LinePrice = 12.34m;
			IICRConsignmentItem wrappedInvoiceLine = new ICRJobComInvoiceLineWrapper(testInvoiceLine);
			AssertEquals(12.34m, wrappedInvoiceLine.Value);
		}

		public void TestCurrency()
		{
			var testInvoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			testInvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Cambodia;
			var testInvoiceLine = testInvoiceHeader.JobComInvoiceLines.AddNew();
			IICRConsignmentItem wrappedInvoiceLine = new ICRJobComInvoiceLineWrapper(testInvoiceLine);
			AssertEquals(Core.Constants.CurrencyCodes.Cambodia, wrappedInvoiceLine.Currency);
		}

		public void TestIdentityType()
		{
			IICRConsignmentItem wrappedInvoiceLine = new ICRJobComInvoiceLineWrapper(Factory.NewWithValidTestData<JobComInvoiceLine>());
			Assert(wrappedInvoiceLine.IdentityType.IsEmpty);
		}

		public void TestClassifications()
		{
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			testInvoiceLine.JI_Tariff = "3926.90.69.79F";
			IICRConsignmentItem wrappedInvoiceLine = new ICRJobComInvoiceLineWrapper(testInvoiceLine);
			AssertNotNull(wrappedInvoiceLine.Classifications);
			AssertEquals(1, wrappedInvoiceLine.Classifications.Count());
			foreach (var classification in wrappedInvoiceLine.Classifications)
			{
				AssertEquals("3926906979F", classification.Classification);
				AssertEquals("HS", classification.ClassificationTypeCode);
			}
		}

		public void TestFlashpointTempInCelsius()
		{
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			testInvoiceLine.UNDGs.FirstItemForBinding[0].DI_DGFlashPoint = -5m;
			IICRConsignmentItem wrappedInvoiceLine = new ICRJobComInvoiceLineWrapper(testInvoiceLine);
			AssertEquals(-5m, wrappedInvoiceLine.FlashpointTempInCelsius);
		}

		public void TestTemperatures()
		{
			var expectedTemps = new TemperatureWrapper(0, -10m, 15m);
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			testInvoiceLine.JI_TemperatureDetailsToBeSent = true;
			testInvoiceLine.JI_MinTemp = -10m;
			testInvoiceLine.JI_MaxTemp = 15m;
			testInvoiceLine.JI_StorageTemp = 0m;
			IICRConsignmentItem wrappedInvoiceLine = new ICRJobComInvoiceLineWrapper(testInvoiceLine);
			AssertNotNull(wrappedInvoiceLine.Temperatures);
			AssertEquals("Temperature details to be sent", true, wrappedInvoiceLine.Temperatures != null);
			AssertEquals("Max Temp", 15m, wrappedInvoiceLine.Temperatures.MaxStorageTemp);
			AssertEquals("Max Temp unit", "CEL", wrappedInvoiceLine.Temperatures.MaxStorageTempUnit);
			AssertEquals("Min Temp", -10m, wrappedInvoiceLine.Temperatures.MinStorageTemp);
			AssertEquals("Min Temp unit", "CEL", wrappedInvoiceLine.Temperatures.MinStorageTempUnit);
			AssertEquals("Storage Temp", 0m, wrappedInvoiceLine.Temperatures.StorageTemp);
			AssertEquals("Storage Temp unit", "CEL", wrappedInvoiceLine.Temperatures.StorageTempUnit);
		}

		public void TestGrossWeightInKg()
		{
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			testInvoiceLine.JI_Weight = 12.78m;
			testInvoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			IICRConsignmentItem wrappedInvoiceLine = new ICRJobComInvoiceLineWrapper(testInvoiceLine);
			AssertEquals(12.78m, wrappedInvoiceLine.GrossWeightInKg);
		}

		public void TestGoodsOriginCountry()
		{
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			testInvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Comoros;
			IICRConsignmentItem wrappedInvoiceLine = new ICRJobComInvoiceLineWrapper(testInvoiceLine);
			AssertEquals(Core.Constants.CountryCodes.Comoros, wrappedInvoiceLine.GoodsOriginCountry);
		}

		public void TestPackageQty()
		{
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			testInvoiceLine.JI_InvoiceQuantity = 2m;
			IICRConsignmentItem wrappedInvoiceLine = new ICRJobComInvoiceLineWrapper(testInvoiceLine);
			AssertEquals(2, wrappedInvoiceLine.PackageQty);
		}

		public void TestPackageType()
		{
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			testInvoiceLine.JI_InvoiceUQ = "NMB";
			IICRConsignmentItem wrappedInvoiceLine = new ICRJobComInvoiceLineWrapper(testInvoiceLine);
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

			IICRConsignmentItem wrappedInvoiceLine = new ICRJobComInvoiceLineWrapper(testInvoiceLine);
			AssertEquals("CONTAINER1", wrappedInvoiceLine.ContainerNumber);
		}
	}
}
