using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ZA.Business.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(DA306DeclarationWrapper))]
	class DA306DeclarationWrapperTest : TestCaseWithFactory
	{
		public void TestImporter()
		{
			AssertEquals(declaration.ImporterDocumentaryAddress, wrapper.Importer);
		}

		public void TestImporterAddress()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER NAME";
			importer.MainAddress.Address1 = "ADDRESS 1";
			importer.MainAddress.Address2 = "ADDRESS 2";
			importer.MainAddress.City = "CITY";
			importer.MainAddress.State = "STATE";
			importer.MainAddress.Postcode = "123456";
			declaration.JE_OH_Importer = importer.PK;
			AssertMultilineASCIIEquals("ADDRESS 1\r\nADDRESS 2\r\nCITY STATE 123456", wrapper.ImporterAddress);
		}

		public void TestImporterID()
		{
			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals(ZString.Empty, wrapper.ImporterID);

			importer.SetLocalCustomsCode(OrgCusCode.SouthAfricaCodeTypes.IDNumber, "I1234");
			AssertEquals("I1234", wrapper.ImporterID);

			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.PassportID, "P1234");
			AssertEquals("P1234", wrapper.ImporterID);

			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "C1234");
			AssertEquals("C1234", wrapper.ImporterID);
		}

		public void TestSupplierName()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_FullName = "SUPPLIER FULL NAME";
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals(supplier.OH_FullName, wrapper.SupplierName);
		}

		public void TestTransportDetails()
		{
			declaration.JE_MasterBill = "MB1234";
			declaration.JE_MasterBillIssuedDate = ZDateTime.Today;
			declaration.JE_HouseBill = "HB1234";
			AssertEquals($"MB1234 / {ZDateTime.Today:dd-MMM-yyyy} / HB1234", wrapper.TransportDetails);
		}

		public void TestVoyageFlight()
		{
			AssertEquals(ZString.Empty, wrapper.VoyageFlight);

			declaration.JE_TransportMode = TransportModes.Air;
			declaration.JE_VoyageFlightNo = "F1234";
			declaration.JE_DateAtOrigin = ZDateTime.Today;
			AssertEquals($"F1234 / {ZDateTime.Today:dd-MMM-yyyy}", wrapper.VoyageFlight);

			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.JE_VoyageFlightNo = "F1234";
			declaration.JE_VesselName = "V1234";
			declaration.JE_LloydsIMO = "I1234";
			AssertEquals("V1234 / F1234 / I1234", wrapper.VoyageFlight);
		}

		public void TestVoyageFlightNo()
		{
			declaration.JE_VoyageFlightNo = "F1234";
			AssertEquals("F1234", wrapper.VoyageFlightNo);
		}

		public void TestMasterBillNumber()
		{
			declaration.JE_MasterBill = "MB1234";
			AssertEquals("MB1234", wrapper.MasterBillNumber);
		}

		public void TestHouseBillNumber()
		{
			declaration.JE_HouseBill = "HB1234";
			AssertEquals("HB1234", wrapper.HouseBillNumber);
		}

		public void TestTotalPackages()
		{
			declaration.JE_TotalNoOfPacks = 1234;
			AssertEquals(1234, wrapper.TotalPackages);
		}

		public void TestTotalPackagesUnit()
		{
			declaration.JE_TotalNoOfPacksPackType = PkgUnit.Package;
			AssertEquals(PkgUnit.Package, wrapper.TotalPackagesUnit);
		}

		public void TestCustomsValue()
		{
			AssertEquals(0m, wrapper.CustomsValue);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 123.4m;
			AssertEquals(123.4m, wrapper.CustomsValue);
		}

		public void TestMarksNumbers()
		{
			declaration.JE_TotalWeight = 123.445;
			declaration.JE_TotalWeightUnit = Weight.Kilograms;
			AssertMultilineASCIIEquals("123.45 KG", wrapper.MarksNumbers);

			declaration.JE_MarksAndNumbers = "Mark 1\r\nMark 2";
			AssertMultilineASCIIEquals("Mark 1\r\nMark 2\r\n123.45 KG", wrapper.MarksNumbers);

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "C1234-1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C1234-2";
			AssertMultilineASCIIEquals("Mark 1\r\nMark 2\r\n123.45 KG\r\nC1234-1, C1234-2", wrapper.MarksNumbers);
		}

		public void TestGoodsDescription()
		{
			declaration.JE_GoodsDescription = "GOODS DESCRIPTIONS";
			AssertEquals("GOODS DESCRIPTIONS", wrapper.GoodsDescription);
		}

		public void TestPortOfDischarge()
		{
			declaration.JE_RL_NKPortOfArrival = "ZAJNB";
			AssertEquals("Johannesburg", wrapper.PortOfDischarge);
		}

		public void TestInvoiceLines()
		{
			AssertEquals(0, wrapper.InvoiceLines.Count());

			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals(1, wrapper.InvoiceLines.Count());
		}

		JobDeclaration declaration;
		DA306DeclarationWrapper wrapper;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			wrapper = new DA306DeclarationWrapper(declaration);
		}
	}
}
