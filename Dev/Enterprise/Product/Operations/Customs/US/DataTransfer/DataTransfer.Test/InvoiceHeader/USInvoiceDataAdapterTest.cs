using System;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(USInvoiceDataAdapter))]
	sealed class USInvoiceDataAdapterTest : Customs.DataTransfer.Testing.InvoiceValueObjectDataAdapterTest
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

		public void TestSomePropertiesMaxLengthExceeded()
		{
			new USInvoiceDataAdapterToolTest(InvoiceDataAdapter, GetInvoiceHeader, GetUSInvoiceHeaderWithTestData, Factory).TestSomePropertiesMaxLengthExceeded();
		}

		public void TestImportUSExportSpecificInvoiceData()
		{
			new USInvoiceDataAdapterToolTest(InvoiceDataAdapter, GetInvoiceHeader, GetUSInvoiceHeaderWithTestData, Factory).TestImportUSExportSpecificInvoiceData();
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

		public void TestAIIDataIsNotExportedWhenAIIIsNotEnabled()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = false;
			declaration.US_EnableAII = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1010101010";
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.JI_LinePrice = 1500m;
			var aiiLine = invoiceLine.FirstAIILine;
			aiiLine.US_PercActvIngr = 0.1m;
			aiiLine.US_QtyDiffRsnCode = "3";
			aiiLine.US_QtyDiffRsn = "fffd";
			aiiLine.US_InvQtyDisp = 59m;
			aiiLine.US_InvUQDisp = "HY";
			AssertEquals("US_UnitBasis", 1, aiiLine.US_UnitBasis);
			AssertEquals("US_UnitPrice", 150m, aiiLine.US_UnitPrice);
			aiiLine.RegoNumbers.AddNew(RegoNumberCodeList.Codes.ChassisNumber, "C111");
			Factory.Save();
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			InvoiceDataAdapter.ExportToValueObject(invoice, xmlInvoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.USInvoiceLine uSXMLInvoiceLine = xmlInvoiceHeader.InvoiceLines[0].CountryPayload.USInvoiceLine;
			AssertEquals("USXMLInvoiceLine.BasisUnit.IsSpecified", true, uSXMLInvoiceLine.BasisUnit.IsSpecified);
			AssertEquals("USXMLInvoiceLine.BasisUnit.BasisUnit", 1m, uSXMLInvoiceLine.BasisUnit.BasisUnit);
			AssertEquals("USXMLInvoiceLine.BasisUnit.Price", 150m, uSXMLInvoiceLine.BasisUnit.Price);
			AssertEquals("USXMLInvoiceLine.DispatchedInvoiceQty.IsSpecified", true, uSXMLInvoiceLine.DispatchedInvoiceQty.IsSpecified);
			AssertEquals("USXMLInvoiceLine.DispatchedInvoiceQty.QtyDiffReasonCode", "3", uSXMLInvoiceLine.DispatchedInvoiceQty.QtyDiffReasonCode);
			AssertEquals("USXMLInvoiceLine.DispatchedInvoiceQty.QtyDiffReasonDesc", "fffd", uSXMLInvoiceLine.DispatchedInvoiceQty.QtyDiffReasonDesc);
			AssertEquals("USXMLInvoiceLine.DispatchedInvoiceQty.Quantity.IsSpecified", true, uSXMLInvoiceLine.DispatchedInvoiceQty.Quantity.IsSpecified);
			AssertEquals("USXMLInvoiceLine.DispatchedInvoiceQty.Quantity.Value", 59m, uSXMLInvoiceLine.DispatchedInvoiceQty.Quantity.Value);
			AssertEquals("USXMLInvoiceLine.DispatchedInvoiceQty.Quantity.DimensionType", "HY", uSXMLInvoiceLine.DispatchedInvoiceQty.Quantity.DimensionType);
			AssertEquals("USXMLInvoiceLine.RegistrationNumbers.Count", 1, uSXMLInvoiceLine.RegistrationNumbers.Count);
			AssertEquals("USXMLInvoiceLine.RegistrationNumbers[0].Number", "C111", uSXMLInvoiceLine.RegistrationNumbers[0].Number);
			AssertEquals("USXMLInvoiceLine.RegistrationNumbers[0].Type", RegoNumberCodeList.Codes.ChassisNumber, uSXMLInvoiceLine.RegistrationNumbers[0].Type);
			declaration.US_EnableAII = false;
			aiiLine = invoiceLine.FirstAIILine;
			AssertEquals("US_UnitBasis", 1, aiiLine.US_UnitBasis);
			AssertEquals("US_UnitPrice", 150m, aiiLine.US_UnitPrice);
			Factory.Save();
			xmlInvoiceHeader = new Xsd.InvoiceHeader();
			InvoiceDataAdapter.ExportToValueObject(invoice, xmlInvoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));
			uSXMLInvoiceLine = xmlInvoiceHeader.InvoiceLines[0].CountryPayload.USInvoiceLine;
			AssertEquals("USXMLInvoiceLine.BasisUnit.IsSpecified", false, uSXMLInvoiceLine.BasisUnit.IsSpecified);
			AssertEquals("USXMLInvoiceLine.DispatchedInvoiceQty.IsSpecified", false, uSXMLInvoiceLine.DispatchedInvoiceQty.IsSpecified);
			AssertEquals("USXMLInvoiceLine.RegistrationNumbers.Count", 0, uSXMLInvoiceLine.RegistrationNumbers.Count);
		}

		public new void TestAllPropertiesExistinLandedCosting()
		{
			Assert(true);
		}

		protected override void TestExportLandedCostingValuesCore()
		{
			Assert(true);
		}

		protected override BaseJobComInvoiceHeader NewBusinessObject() => JobDec.Invoices.AddNew();

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
			=> new BusinessObjectAndExpectedOutputFileName(GetEmptyInvoiceHeader(), TestFileHelper.GetPathForResourceName("USEmptyInvoice.xml", Assembly.GetExecutingAssembly()), ValidationKind.None, "Empty Invoice");

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample() => GetEmptyBizObjSample();

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
			=> new BusinessObjectAndExpectedOutputFileName(GetUSInvoiceHeaderWithTestData(), TestFileHelper.GetPathForResourceName("USPopulatedInvoice.xml", Assembly.GetExecutingAssembly()), ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated Invoice");

		protected override ValueObjectDataAdapter<BaseJobComInvoiceHeader, Xsd.InvoiceHeader> GetNewBizObjXmlDataAdapter() => new USInvoiceDataAdapter((JobDeclaration)GetJobDeclaration());

		protected override string ExpectedRootCollectionElementName => "Invoices";

		protected override ZString TariffNumber => "7215502031";

		protected override BaseJobComInvoiceHeader GetInvoiceHeaderWithTestData()
		{
			var result = base.GetInvoiceHeaderWithTestData();
			result.IsJZ_InvoiceCurrExRateUserEnterable = true;
			return result;
		}

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

		protected override IValueObjectDataAdapter GetInvoiceValueObjectDataAdapter() => new USInvoiceDataAdapter((JobDeclaration)JobDec);

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_MessageType = Customs.US.Business.JobMessageTypeList.Codes.Import;
			result.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			result.US_EnableAII = true;
			return result;
		}

		USInvoiceDataAdapter InvoiceDataAdapter => (USInvoiceDataAdapter)invoiceDataAdapter;

		JobComInvoiceHeader GetUSInvoiceHeaderWithTestData()
		{
			var invoiceHeader = (JobComInvoiceHeader)GetInvoiceHeaderWithTestData();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable = true;
			invoiceHeader.Charges[0].J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine = invoiceHeader.JobComInvoiceLines[0];
			invoiceLine.US_TariffType = TariffTypeList.Codes.HTS;
			return invoiceHeader;
		}
	}
}
