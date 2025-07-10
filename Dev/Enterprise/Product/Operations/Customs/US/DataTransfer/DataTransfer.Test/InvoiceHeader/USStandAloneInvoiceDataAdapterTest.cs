using System;
using System.IO;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(USStandAloneInvoiceDataAdapter))]
	sealed class USStandAloneInvoiceDataAdapterTest : Customs.DataTransfer.Testing.StandAloneInvoiceValueObjectDataAdapterTest
	{
		public void TestImportUSInvoiceHeader()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			new USInvoiceDataAdapterToolTest(InvoiceDataAdapter, GetInvoiceHeader, GetUSInvoiceHeaderWithTestData, Factory).TestImportUSInvoiceHeader();
		}

		public void TestExportUSInvoiceHeader()
		{
			new USInvoiceDataAdapterToolTest(InvoiceDataAdapter, GetInvoiceHeader, GetUSInvoiceHeaderWithTestData, Factory).TestExportUSInvoiceHeader();
		}

		public override void TestImportConsigneeValue()
		{
			new USInvoiceDataAdapterToolTest(InvoiceDataAdapter, GetInvoiceHeader, GetUSInvoiceHeaderWithTestData, Factory).TestExportImportForJZ_OH_Buyer();
		}

		public void TestImportUSInvoiceLines()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			new USInvoiceDataAdapterToolTest(InvoiceDataAdapter, GetInvoiceHeader, GetUSInvoiceHeaderWithTestData, Factory).TestImportUSInvoiceLines();
		}

		public void TestImportUSInvoiceLines_ImportNetWeightSetsSecondQtyIfItsEmpty()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			new USInvoiceDataAdapterToolTest(InvoiceDataAdapter, GetInvoiceHeader, GetUSInvoiceHeaderWithTestData, Factory).TestImportUSInvoiceLines_ImportNetWeightSetsSecondQtyIfItsEmpty();
		}

		public void TestExportUSInvoiceLines()
		{
			new USInvoiceDataAdapterToolTest(InvoiceDataAdapter, GetInvoiceHeader, GetUSInvoiceHeaderWithTestData, Factory).TestExportUSInvoiceLines();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCS00182123ForUS()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "OMRONKYO";
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "OMRELEJMH";
			var product = Factory.New<Customs.Business.OrgSupplierPart>();
			product.OP_StockKeepingUnit = "PCE";
			product.OP_PartNum = "A6H 0031F";
			product.OP_Desc = "TACTILE SWITCH";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_Relationship = "OWN";
			relation.OU_OH = importer.PK;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";
			pivot.CI_TariffNum = "8600783474";
			Factory.Save();
			var fileName = Path.Combine(BaseSourcePath, "Enterprise", "Product", "Operations", "Customs", "US", "DataTransfer", "DataTransfer.Test", "Testing", @"683_20120703172421.CSV");
			var flatFileImporter = new FlatFileUnattachedInvoiceDataImporter(fileName);
			flatFileImporter.Import();
			var invoice = Factory.LoadTop1<JobComInvoiceHeader>(new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, "C22178JB"));
			AssertNotNull(invoice);
			AssertEquals("IMP", invoice.JZ_StandAloneInvoiceDirection);
			AssertEquals("One invoice line", 1, invoice.JobComInvoiceLines.Count);
			var invoiceLine = invoice.JobComInvoiceLines[0];
			AssertEquals("JI_OP", product.PK, invoiceLine.JI_OP);
			AssertEquals("Tariff should be set from a pivot, rather than from a csv file.", "8600783474", invoiceLine.JI_Tariff);
			AssertEquals("Country of origin", "CN", invoiceLine.US_UC_NKCountryOfOrigin);
			AssertEquals("PreviousPivot should have been calculated", pivot.PK, invoiceLine.US_CI_PreviousPivot);
			var declaration = new BusinessObjectFactory().New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = US.Business.JobMessageTypeList.Codes.Import;
			var invoiceLoaded = declaration.Factory.Load<JobComInvoiceHeader>(invoice.PK);
			invoiceLoaded.JZ_JE = declaration.PK;
			AssertEquals("One invoice line", 1, invoiceLoaded.JobComInvoiceLines.Count);
			var invoiceLineLoaded = invoiceLoaded.JobComInvoiceLines[0];
			AssertEquals("JI_OP", product.PK, invoiceLineLoaded.JI_OP);
			AssertEquals("Tariff should be set from a pivot, rather than from a csv file.", "8600783474", invoiceLineLoaded.JI_Tariff);
			AssertEquals("Country of origin", "CN", invoiceLineLoaded.US_UC_NKCountryOfOrigin);
			AssertEquals("PreviousPivot should have been calculated", pivot.PK, invoiceLineLoaded.US_CI_PreviousPivot);
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
			=> new BusinessObjectAndExpectedOutputFileName(GetEmptyInvoiceHeader(), TestFileHelper.GetPathForResourceName("USEmptyStandAloneInvoice.xml", Assembly.GetExecutingAssembly()), ValidationKind.None, "Empty Invoice");

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample() => GetEmptyBizObjSample();

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
			=> new BusinessObjectAndExpectedOutputFileName(GetUSInvoiceHeaderWithTestData(), TestFileHelper.GetPathForResourceName("USPopulatedStandAloneInvoice.xml", Assembly.GetExecutingAssembly()), ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated Invoice");

		protected override BaseJobComInvoiceHeader GetInvoiceHeaderWithTestData()
		{
			var result = base.GetInvoiceHeaderWithTestData();
			result.JZ_MessageType = Customs.US.Business.JobMessageTypeList.Codes.Import;
			result.IsJZ_InvoiceCurrExRateUserEnterable = true;
			return result;
		}

		protected override BaseJobComInvoiceHeader GetInvoiceHeader()
		{
			var result = base.GetInvoiceHeader();
			result.JZ_MessageType = Customs.US.Business.JobMessageTypeList.Codes.Import;
			return result;
		}

		protected override BaseJobComInvoiceHeader GetEmptyInvoiceHeader()
		{
			var result = base.GetEmptyInvoiceHeader();
			result.JZ_MessageType = Customs.US.Business.JobMessageTypeList.Codes.Import;
			return result;
		}

		protected override ValueObjectDataAdapter<BaseJobComInvoiceHeader, Xsd.InvoiceHeader> GetNewBizObjXmlDataAdapter() => new USStandAloneInvoiceDataAdapter();

		protected override string ExpectedRootCollectionElementName => "Invoices";

		protected override ZString TariffNumber => "7215502031";

		protected override BaseCusClassification CreateTestCusClassification()
		{
			var result = Factory.New<CusClassification>();
			result.CC_Description = "TEST FOR INVOICE XML DATA ADAPTER";
			result.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			result.CC_IsActive = true;
			result.CC_LookupCode = "TARIFF";
			result.CC_TariffNum = TariffNumber;
			return result;
		}

		protected override void DecorateLineToHaveDuty(BaseJobComInvoiceLine invoiceLine, ZDecimal dutyAmount)
		{
			base.DecorateLineToHaveDuty(invoiceLine, dutyAmount);
			((JobComInvoiceLine)invoiceLine).US_Duty = dutyAmount;
		}

		protected override string TestingCountry => Core.Constants.CountryCodes.UnitedStates;

		protected override IValueObjectDataAdapter GetInvoiceValueObjectDataAdapter() => new USStandAloneInvoiceDataAdapter();

		JobComInvoiceHeader GetUSInvoiceHeaderWithTestData()
		{
			var invoiceHeader = (JobComInvoiceHeader)GetInvoiceHeaderWithTestData();
			invoiceHeader.JZ_MessageType = Customs.US.Business.JobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable = true;
			invoiceHeader.Charges[0].J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine = invoiceHeader.JobComInvoiceLines[0];
			return invoiceHeader;
		}

		USStandAloneInvoiceDataAdapter InvoiceDataAdapter => (USStandAloneInvoiceDataAdapter)invoiceDataAdapter;
	}
}
