using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.NZ.Business.Data.Testing
{
	[TestedType(typeof(NZStandAloneInvoiceValueObjectDataAdapter))]
	sealed class NZStandAloneInvoiceValueObjectDataAdapterTest : DataTransfer.Testing.StandAloneInvoiceValueObjectDataAdapterTest
	{
		public void TestImportInvoiceHeaderAdditionalInfo()
		{
			new NZInvoiceDataTransferToolTest(InvoiceDataAdapter, GetInvoiceHeader, Factory).TestImportInvoiceHeaderAdditionalInfo();
		}

		public void TestImportInvoiceLinesAdditionalInfo()
		{
			new NZInvoiceDataTransferToolTest(InvoiceDataAdapter, GetInvoiceHeader, Factory).TestImportInvoiceLinesAdditionalInfo();
		}

		public void TestGetXmlInvoiceHeaderAdditionalInfo()
		{
			new NZInvoiceDataTransferToolTest(InvoiceDataAdapter, GetInvoiceHeader, Factory).TestGetXmlInvoiceHeaderAdditionalInfo();
		}

		public void TestExportInvoiceLinesAdditionalInfo()
		{
			new NZInvoiceDataTransferToolTest(InvoiceDataAdapter, GetInvoiceHeader, Factory).TestExportInvoiceLinesAdditionalInfo();
		}

		public void TestImportNZPreferenceCode()
		{
			new NZInvoiceDataTransferToolTest(InvoiceDataAdapter, GetInvoiceHeader, Factory).TestImportNZPreferenceCode();
		}

		public void TestExportNZPreferenceCode()
		{
			new NZInvoiceDataTransferToolTest(InvoiceDataAdapter, GetInvoiceHeader, Factory).TestExportNZPreferenceCode();
		}

		public void TestSetInvoiceLineCharges()
		{
			new NZInvoiceDataTransferToolTest(InvoiceDataAdapter, GetInvoiceHeader, Factory).TestSetInvoiceLineCharges();
		}

		protected override BaseJobComInvoiceHeader NewBusinessObject()
		{
			return JobDec.Invoices.AddNew();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(GetEmptyInvoiceHeader(), FileReader.ExtractEmbeddedResourceToFile(testFilesResourceLocation, TempDir.DirectoryName, "NZEmptyStandAloneInvoice.xml"), ValidationKind.None, "Empty Invoice");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(GetNZInvoiceHeaderWithTestData(), FileReader.ExtractEmbeddedResourceToFile(testFilesResourceLocation, TempDir.DirectoryName, "NZPopulatedStandAloneInvoice.xml"), ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated Invoice");
		}

		BaseJobComInvoiceHeader GetNZInvoiceHeaderWithTestData()
		{
			var invoiceHeader = (JobComInvoiceHeader)GetInvoiceHeaderWithTestData();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoiceHeader.Charges[0].J7_RX_NKCurrency = Core.Constants.CurrencyCodes.NewZealand;

			return invoiceHeader;
		}

		protected override ValueObjectDataAdapter<BaseJobComInvoiceHeader, Xsd.InvoiceHeader> GetNewBizObjXmlDataAdapter()
		{
			return new NZStandAloneInvoiceValueObjectDataAdapter();
		}

		protected override ZString TariffNumber
		{
			get { return "7215.50.20.31K"; }
		}

		protected override void TearDown()
		{
			base.TearDown();

			tempDir?.Dispose();
		}

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.NewZealand; }
		}

		protected override IValueObjectDataAdapter GetInvoiceValueObjectDataAdapter()
		{
			return new NZStandAloneInvoiceValueObjectDataAdapter();
		}

		NZStandAloneInvoiceValueObjectDataAdapter InvoiceDataAdapter
		{
			get { return (NZStandAloneInvoiceValueObjectDataAdapter)invoiceDataAdapter; }
		}

		protected override BaseCusClassification CreateTestCusClassification()
		{
			var result = base.CreateTestCusClassification();

			result.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;

			return result;
		}

		TestFileReader FileReader => fileReader ?? (fileReader = new TestFileReader(typeof(NZStandAloneInvoiceValueObjectDataAdapterTest)));
		TestFileReader fileReader;

		TempDirectory TempDir => tempDir ?? (tempDir = new TempDirectory());
		TempDirectory tempDir;

		const string testFilesResourceLocation = "Enterprise.Customs.NZ.Business.Test.DataImportExport.Xml.Testing";
	}
}
