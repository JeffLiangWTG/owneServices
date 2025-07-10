using System;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using TestHelper = Enterprise.Customs.ZA.Business.Testing.TestHelper;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.ZA.Business.Data.Testing
{
	[TestedType(typeof(ZADeclarationValueObjectDataAdapter))]
	sealed class ZADeclarationValueObjectDataAdapterTest : DeclarationValueObjectDataAdapterAbstractTest
	{
		public void TestExportMasterBillIssueDate()
		{
			Xsd.Consol consolValue = new Xsd.Consol();
			DeclarationDataAdapter.ExportConsolValues(consolValue, FullyPopulatedDeclaration, DataExportContext);
			AssertEquals("Master bill issue date is exported", new ZDateTime(2009, 1, 20, 23, 59, 0), consolValue.ConsolDetail.MasterBillIssueDate);
		}

		public void TestImportMasterBillIssueDate()
		{
			ZDateTime billDate = new ZDateTime(2009, 1, 1, 21, 12, 59);
			Xsd.Consol consolValue = new Xsd.Consol();
			JobDeclaration jobDec = Factory.New<JobDeclaration>();
			consolValue.ConsolDetail.MasterBillIssueDate = billDate;
			DeclarationDataAdapter.ImportConsolDetails(jobDec, consolValue, DataImportContext);

			AssertEquals("Master bill issue date is imported", jobDec.JE_MasterBillIssuedDate, billDate);
		}

		public void TestExportHouseBillIssueDate()
		{
			//standalone declaration
			FullyPopulatedDeclaration.HouseBillIssuedDate = ZDateTime.Now;
			Xsd.Shipment shipmentValue = new Xsd.Shipment();
			DeclarationDataAdapter.ExportShipmentValues(shipmentValue, FullyPopulatedDeclaration, DataExportContext);
			AssertEquals("House bill issue date is exported", FullyPopulatedDeclaration.HouseBillIssuedDate, shipmentValue.ShipmentDetails.HBLIssueDate);

			shipmentValue = new Xsd.Shipment();

			//Shipment Declaration
			FullyPopulatedDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			FullyPopulatedDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>(TestBusinessObjectKind.MinimumRequiredToSave);
			shipment.JS_HouseBillIssueDate = ZDateTime.Now;
			FullyPopulatedDeclaration.JE_JS = shipment.PK;
			ErrorReporter.Clear();
			FullyPopulatedDeclaration.JE_OverrideFreightDefaults = true;

			shipmentValue = new Xsd.Shipment();
			DeclarationDataAdapter.ExportShipmentValues(shipmentValue, FullyPopulatedDeclaration, DataExportContext);
			AssertEquals("House bill issue date is retrieved from the declaration", FullyPopulatedDeclaration.HouseBillIssuedDate, shipmentValue.ShipmentDetails.HBLIssueDate);

			FullyPopulatedDeclaration.HouseBillIssuedDate = ZDateTime.Empty;
			shipmentValue = new Xsd.Shipment();
			DeclarationDataAdapter.ExportShipmentValues(shipmentValue, FullyPopulatedDeclaration, DataExportContext);
			AssertEquals("House bill issue date is retrieved from the shipment if hbl issue date is not entered on declaration ", shipment.JS_HouseBillIssueDate, shipmentValue.ShipmentDetails.HBLIssueDate);
		}

		public override void TestExportCustomsEntry()
		{
			var helper = new TestHelper(Factory);
			helper.SetExchangeRate(helper.EURCurrency, 0.1243m, new ZDateTime(2004, 10, 25));

			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			testDec.JE_ExportDate = new ZDateTime(2004, 10, 25);

			var groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew("ADD", 187.69m, helper.EURCurrency.RX_Code);
			groupHeader.Charges.AddNew("OFT", 251.63m, helper.EURCurrency.RX_Code);

			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 32696.86m;
			invoice.JZ_IncoTerm = "EXW";
			invoice.JZ_RX_NKInvoice_Currency = helper.EURCurrency.RX_Code;

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 24093.26m;
			line1.JI_Tariff = "900319007";
			line1.JI_CustomsQuantity = 1522;
			line1.JI_CountryOfOrigin = "IT";

			var merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("No Customs Header", 0, testDec.CustomsEntryHeaders.Count);
		}

		public void TestImportHouseBillIssueDate()
		{
			JobDeclaration jobDec = Factory.New<JobDeclaration>();
			Xsd.ConsolAndShipment declarationValue = new Xsd.ConsolAndShipment();
			declarationValue.Consol = new Xsd.Consol();
			declarationValue.Shipment = new Xsd.Shipment();

			declarationValue.Shipment.ShipmentDetails.HBLIssueDate = ZDateTime.Now;

			DeclarationDataAdapter.ImportShipmentDetails(jobDec, declarationValue.Consol, declarationValue.Shipment, DataImportContext);

			AssertEquals("House bill issue date", declarationValue.Shipment.ShipmentDetails.HBLIssueDate, jobDec.HouseBillIssuedDate);
		}

		public void TestImportDeclarationDetails()
		{
			JobDeclaration jobDec = Factory.New<JobDeclaration>();
			Xsd.ConsolAndShipment declarationValue = new Xsd.ConsolAndShipment();
			declarationValue.Consol = new Xsd.Consol();
			declarationValue.Shipment = new Xsd.Shipment();

			Xsd.ZADeclaration jobDecData = new Xsd.ZADeclaration();
			jobDecData.DistrictOffice = "DBN";
			jobDecData.PortOfExitOrDestination = "CTN";
			declarationValue.Shipment.Declaration.CountryPayload.ZADeclaration = jobDecData;

			DeclarationDataAdapter.ImportDeclarationDetails(jobDec, declarationValue.Shipment.Declaration, "MAWB", DataImportContext);

			AssertEquals("District office", jobDecData.DistrictOffice, jobDec.JE_CustomsOffice);
		}

		public void TestExportDeclarationDetails()
		{
			//standalone declaration
			FullyPopulatedDeclaration.JE_CustomsOffice = "DBN";
			Xsd.Declaration decXSD = new Xsd.Declaration();
			DeclarationDataAdapter.ExportDeclarationDetails(decXSD, FullyPopulatedDeclaration, DataExportContext);
			AssertEquals(expected: true, decXSD.CountryPayload.ZADeclaration.IsSpecified);

			Xsd.ZADeclaration jobdecData = decXSD.CountryPayload.ZADeclaration;
			AssertNotNull("ZADeclaration Data is not null", jobdecData);
			AssertEquals("district office", FullyPopulatedDeclaration.JE_CustomsOffice, jobdecData.DistrictOffice);
		}

		protected override void AssertDTAF(Xsd.Consol toConsol)
		{
			// Overidden as this field not used in ZA Customs.
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			TestJobDec.ResumeApportionment();
			return new BusinessObjectAndExpectedOutputFileName(TestJobDec, TestFileHelper.GetPathForResourceName("ZAPopulatedDeclaration.xml", Assembly.GetExecutingAssembly()), ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated Declaration");
		}

		protected override BaseJobDeclaration GetNewPopulatedJobDeclaration()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				JobDeclaration declaration = base.GetNewPopulatedJobDeclaration() as JobDeclaration;
				declaration.JE_MasterBillIssuedDate = new ZDateTime(2009, 1, 20, 23, 59, 0);
				declaration.HouseBillIssuedDate = new ZDateTime(2009, 1, 15, 13, 12, 0);
				declaration.ResumeApportionment();
				return declaration;
			}
		}

		protected override bool ShouldTestonFirstArrivalInfo
		{
			//first arrival date and port are not being used in ZA Customs
			get { return false; }
		}

		protected override void SetUp()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			base.SetUp();
		}

		protected override Type GetDataAdapterType() => typeof(ZADeclarationValueObjectDataAdapter);

		protected override ValueObjectDataAdapter<BaseJobDeclaration, Xsd.ConsolAndShipment> GetNewBizObjXmlDataAdapter() => new ZADeclarationValueObjectDataAdapter();

		protected override string TestingCountry => Core.Constants.CountryCodes.SouthAfrica;

		ValueObjectImportContext dataImportContext;
		ValueObjectImportContext DataImportContext => dataImportContext ?? (dataImportContext = new ValueObjectImportContext(Factory, new NotificationBuffer()));

		ValueObjectExportContext dataExportContext;
		ValueObjectExportContext DataExportContext => dataExportContext ?? (dataExportContext = new ValueObjectExportContext(new NotificationBuffer()));

		JobDeclaration fullyPopulatedDeclaration;
		JobDeclaration FullyPopulatedDeclaration => fullyPopulatedDeclaration ?? (fullyPopulatedDeclaration = (JobDeclaration)GetNewPopulatedJobDeclaration());

		ZADeclarationValueObjectDataAdapterTestClass declarationDataAdapter;
		ZADeclarationValueObjectDataAdapterTestClass DeclarationDataAdapter => declarationDataAdapter ?? (declarationDataAdapter = new ZADeclarationValueObjectDataAdapterTestClass());

		sealed class ZADeclarationValueObjectDataAdapterTestClass : ZADeclarationValueObjectDataAdapter
		{
			public new void ExportConsolValues(Xsd.Consol consolValue, BaseJobDeclaration jobDec, IValueObjectExportContext context)
			{
				base.ExportConsolValues(consolValue, jobDec, context);
			}

			public new void ImportConsolDetails(BaseJobDeclaration jobDec, Xsd.Consol consolValue, IValueObjectImportContext context)
			{
				base.ImportConsolDetails(jobDec, consolValue, context);
			}

			public new void ImportShipmentDetails(BaseJobDeclaration jobDec, Xsd.Consol consol, Xsd.Shipment shipment, IValueObjectImportContext context)
			{
				base.ImportShipmentDetails(jobDec, consol, shipment, context);
			}

			public new void ExportShipmentValues(Xsd.Shipment toShipment, BaseJobDeclaration jobDec, IValueObjectExportContext context)
			{
				base.ExportShipmentValues(toShipment, jobDec, context);
			}

			public new void ExportDeclarationDetails(Xsd.Declaration toDec, BaseJobDeclaration jobDec, IValueObjectExportContext context)
			{
				jobDec.ResumeApportionment();
				base.ExportDeclarationDetails(toDec, jobDec, context);
			}

			public new void ImportDeclarationDetails(BaseJobDeclaration jobDec, Xsd.Declaration toDec, ZString masterBillOnConsol, IValueObjectImportContext context)
			{
				base.ImportDeclarationDetails(jobDec, toDec, masterBillOnConsol, context);
			}
		}
	}
}
