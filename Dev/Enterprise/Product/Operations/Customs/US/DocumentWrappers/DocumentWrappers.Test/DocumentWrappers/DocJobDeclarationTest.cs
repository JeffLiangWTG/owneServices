using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(DocDeclaration))]
	sealed class DocJobDeclarationTest : DocBaseJobDeclarationAbstractTest<JobDeclaration, DocDeclaration>
	{
		protected override DocDeclaration CreateDeclarationWrapper(JobDeclaration declaration)
		{
			var result = DocDeclaration.New(declaration, Factory);
			((IBODocDataProvider)result).SetDocWrapperContext(new Dictionary<string, object>());
			result.SetReportNameForTesting("Report Name");
			return result;
		}

		public override void TestIsExWarehouse()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(false, DeclarationWrapper.IsExWarehouse);

			Declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			AssertEquals(true, DeclarationWrapper.IsExWarehouse);

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertEquals(false, DeclarationWrapper.IsExWarehouse);
		}

		public void TestPortOfExport()
		{
			AssertNull("PortOfExport", Declaration.PortOfExport);
			AssertNull("PortOfExport", DeclarationWrapper.PortOfExport);

			Declaration.US_RL_NKPortOfExport = helper.USCHI.Code;
			AssertNotNull("PortOfExport", Declaration.PortOfExport);
			AssertNotNull("PortOfExport", DeclarationWrapper.PortOfExport);
			AssertEquals("PortOfExport is of type DocUNLOCO", typeof(Enterprise.DocumentWrappers.DocUNLOCO), DeclarationWrapper.PortOfExport.GetType());
		}

		public void TestInvoiceHeaders()
		{
			AssertEquals("InvoiceHeaders' type", typeof(DocJobComInvoiceHeaderCollection), DeclarationWrapper.InvoiceHeaders.GetType());
		}

		public void TestEntryHeaders()
		{
			AssertEquals("EntryHeaders' type", typeof(DocCusEntryHeaderCollection), DeclarationWrapper.EntryHeaders.GetType());
		}

		public void TestInvoiceGroupHeaders()
		{
			AssertEquals("InvoiceGroupHeaders' type", typeof(DocJobComInvoiceGroupHeaderCollection), DeclarationWrapper.InvoiceGroupHeaders.GetType());
		}

		public void TestDocDelcarationForAI()
		{
			DocDeclaration declarationWrapper = DocDeclaration.New(Declaration, Factory);
			AssertNotNull("Can create AI DeclarationWrapper", declarationWrapper);
		}

		public override void TestTransportModeDescription()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			AssertEquals("TransportModeDescription", "Air", DeclarationWrapper.TransportModeDescription);

			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			AssertEquals("TransportModeDescription", "Sea", DeclarationWrapper.TransportModeDescription);
		}

		public void TestReconciliationProperties()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0712", "CHAMPLAIN-ROUSES POINT", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "Test Importer";
			Declaration.IOROrgPK = importer.PK;
			Declaration.JE_OH_Importer = importer.PK;

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_FullName = "Test Org For Recon";

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var declarationWrapper = CreateDeclarationWrapper(reconDeclaration.ReconWrappedJobDeclaration);
			reconDeclaration.JE_OH_Importer = organisation.PK;
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			reconDeclaration.US_EntryFilerCode = "AAA";
			reconDeclaration.IOROrgPK = organisation.PK;
			reconDeclaration.US_SchDEntry = "0712";
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			((IAllocateNumberSupporter)reconDeclaration).DoAllocate("00000162");

			AssertEquals("Payment Type", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, declarationWrapper.PaymentType);
			AssertEquals("Payment Type description", PaymentTypeList.Descriptions.BatchedByDailyPrintDateAndFilerCode, declarationWrapper.PaymentTypeDescription);
			AssertEquals("Entry Port", "0712", declarationWrapper.EntryPortCode);
			AssertEquals("Entry Port Name", "CHAMPLAIN-ROUSES POINT", declarationWrapper.EntryPortName);
			AssertEquals("Entry Number", "AAA00000162", declarationWrapper.CustomsEntryNumber);
			AssertEquals("Importer", "Test Org For Recon", declarationWrapper.Importer.Name);
			AssertEquals("IOR", "Test Org For Recon", declarationWrapper.ImporterOfRecord.Name);
		}

		public new void TestExportDate()
		{
			Declaration.US_DateOfExport = new ZDateTime(2015, 5, 6);
			AssertEquals("Actual Export Date", new ZDateTime(2015, 5, 6), DeclarationWrapper.ExportDate);

			Declaration.JE_ExportDate = new ZDateTime(2015, 5, 7);
			AssertEquals(new ZDateTime(2015, 5, 6), DeclarationWrapper.ExportDate);

			Declaration.US_DateOfExport = ZDateTime.Empty;
			AssertEquals("Actual Export Date", new ZDateTime(2015, 5, 7), DeclarationWrapper.ExportDate);
		}

		public void TestProofOfReleaseIsSupported()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.JE_MasterBillIssuerSCAC = "APLU";
			declaration.JE_MasterBill = "MLB1111111";
			declaration.JE_HouseBillIssuerSCAC = "SCHR";
			declaration.JE_HouseBill = "HSB11111111";
			declaration.JE_TotalNoOfPacks = 100;
			declaration.JE_TotalNoOfPacksPackType = "PK";
			declaration.JE_EntryAuthorisationDate = new ZDate(2016, 10, 27);
			var declarationWrapper = CreateDeclarationWrapper(declaration);
			AssertEquals(1, declarationWrapper.BillOfLadings.Count);
			var billOfLading = declarationWrapper.BillOfLadings[0];
			AssertEquals("Master:APLUMLB1111111     House:SCHRHSB11111111", billOfLading.BillNumber);
			AssertEquals(100m, billOfLading.ManifestQty);
			AssertEquals("PK", billOfLading.UOM);
			AssertEquals(new ZDate(2016, 10, 27), declarationWrapper.ReleaseDate);
			AssertEquals(CRLReleaseStatusList.Codes.REL, declarationWrapper.ReleaseStatus);
		}

		#region Implementation
		DeclarationTestHelper helper;

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.UnitedStates; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new DeclarationTestHelper(Factory);
		}

		#endregion
	}
}
