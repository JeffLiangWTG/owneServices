using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Service.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.PermitService;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[UseSnapshotProtection]
	public class FTZSynchronizeWithOrdersTest : TestCase
	{
		public void TestFDAIndicatorDoNotOverwritten()
		{
			var orgheader = factory.NewWithValidTestData<OrgHeader>();
			var manufacturer = factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "MA~";
			factory.Save();

			CreatePermit(orgheader.PK, manufacturer.MainAddress.PK);

			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = "31";
			declaration.US_CertifyCargoRelease = true;
			declaration.US_CargoReleaseType = "ACE";
			declaration.JE_OH_Importer = orgheader.PK;
			declaration.US_EntryFilerCode = "XJ5";
			orgheader.OH_IsWarehouseClient = true;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.PK;

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			var invoiceLine4 = invoice2.InvoiceLines.AddNew();
			AssertEquals("Declaration should have two invoices", 2, declaration.Invoices.Count);
			AssertEquals("Declaration should have 4 invoiceLines", 4, declaration.InvoiceLines.Count);
			AssertEquals("Invoice 1 should have two invoiceLines", 2, declaration.Invoices[0].InvoiceLines.Count);
			AssertEquals("Invoice 2 should have two invoiceLines", 2, declaration.Invoices[1].InvoiceLines.Count);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.PK;
			var entryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull(entryHeader);
			declaration.ImportEntryNumber = "00003877";

			var tariff = factory.New<USCTariff>();
			tariff.UE_Tariff = "2001100000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Unit1 = "KG";
			tariff.UE_Unit2 = "CPS";
			factory.Save();

			var whsHelper = ObjectFactory.New<Warehouse.Integration.IWhsTransactionTestHelper>(factory);
			var org2 = declaration.Importer;
			org2.OH_Code = "ORG2";
			org2.OH_IsWarehouseClient = true;
			var part = whsHelper.CreateProduct(org2.PK, "PRODUCTFORTESTING");
			var warehouse = whsHelper.CreateFTZWarehouse(declaration.WarehouseAddress.PK);
			factory.Save();
			whsHelper.CreateWhsReceiveWithInventory(org2.PK, warehouse.PK, "REC2", part.PK, 10m);

			var order2 = whsHelper.CreateWhsOrder(org2.PK, warehouse.PK, factory.NewWithValidTestData<OrgHeader>().PK, "ORD2");
			whsHelper.SetOrderType(order2.PK, "CPS");
			var addinfo = "FWSInd=D*FDAIndicator=D*CPSCInd=D";
			whsHelper.CreateWhsOrderLine(order2.PK, part.PK, 5m, "", "XJ5-00003877", "2001.10.0000", "US", "E", 10m, 15m, "KG", 20m, "CPS", addinfo, 5m, manufacturer.MainAddress.PK, "P");
			factory.Save();

			whsHelper.CreatePickNew(true, true, order2.PK);
			factory.Save();

			new FTZSynchronizeWithOrders(declaration).Synchronize();
			factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reLoadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals("2 Invoices in the Declaration", 2, reLoadedDeclaration.Invoices.Count);
			AssertEquals("1 InvoiceLines in the Declaration", 1, reLoadedDeclaration.InvoiceLines.Count);

			var reloadedInvoice1 = reLoadedDeclaration.Invoices[0];
			var reloadedInvoice2 = reLoadedDeclaration.Invoices[1];
			AssertEquals("Only 1 InvoiceLine in Invoice1", 1, reloadedInvoice1.InvoiceLines.Count);
			AssertEquals("0 InvoiceLines in Invoice2", 0, reloadedInvoice2.InvoiceLines.Count);
			var newCreatedInvoiceLine = reLoadedDeclaration.InvoiceLines[0];
			AssertEquals("The newly created line will be the first one", reloadedInvoice1.InvoiceLines[0].PK, newCreatedInvoiceLine.PK);
			AssertNotEquals("It really is newly created line", invoiceLine1.PK, newCreatedInvoiceLine.PK);
			AssertNotEquals("It really is newly created line", invoiceLine2.PK, newCreatedInvoiceLine.PK);
			AssertNotEquals("It really is newly created line", invoiceLine3.PK, newCreatedInvoiceLine.PK);
			AssertNotEquals("It really is newly created line", invoiceLine4.PK, newCreatedInvoiceLine.PK);

			var reLoadInvoice = reLoadedDeclaration.Invoices[0];
			var newInvoiceLine = reLoadInvoice.InvoiceLines[0];
			AssertEquals("FDA Indicator SHOULD NOT be defaulted as entry type is '31'", ZString.Empty, newInvoiceLine.US_FDAIndicator);
			AssertEquals("FWS Indicator SHOULD NOT be defaulted as entry type is '31'", ZString.Empty, newInvoiceLine.US_FWSInd);
			AssertEquals("CPSC Indicator SHOULD NOT be defaulted as entry type is '31'", ZString.Empty, newInvoiceLine.US_CPSCInd);

			AssertEquals("PRODUCTFORTESTING", newInvoiceLine.JI_PartNo);
			AssertEquals("2001.10.0000", newInvoiceLine.JI_FormattedTariff);
			AssertEquals(10m, newInvoiceLine.JI_LinePrice);
			AssertEquals("KG", newInvoiceLine.JI_CustomsUnitQty);
			AssertEquals(15m, newInvoiceLine.JI_CustomsQuantity);
			AssertEquals(20m, newInvoiceLine.JI_CustomsSecondQuantity);
			AssertEquals("US", newInvoiceLine.JI_CountryOfOrigin);
			AssertEquals(5m, newInvoiceLine.JI_InvoiceQuantity);
			AssertEquals("UNT", newInvoiceLine.JI_InvoiceUQ);
		}

		void CreatePermit(ZGuid owerPK, ZGuid manufacturerPK)
		{
			var factory = new BusinessObjectFactory();
			var createWarehouseAddress = WarehouseAddress;
			var dataHelper = new PermitTestDataHelper(factory);
			PermitHeader = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owerPK, "PERMIT1", ZDate.Today.AddMonths(-5), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 200m, 10000m, "UNT", "XJ5-00003877", manufacturerPK);
			dataHelper.CreatePermitRule(PermitHeader, "FRM", "FM32", "FM32");
			dataHelper.CreatePermitRule(PermitHeader, "TAR", "2000", "3000");
			dataHelper.CreatePermitRule(PermitHeader, "COO", "US", "US");
			dataHelper.CreatePermitRule(PermitHeader, "ZST", "P", "P");

			var result = new PermitWithdrawRequestForTesting()
			{
				Owner = factory.Load<OrgAddress>(owerPK),
				Warehouse = WarehouseAddress,
				Manufacturer = factory.Load<OrgAddress>(manufacturerPK),
				DetailedTrackingEnabled = false,
				PermitType = PermitType.FTZ,
				CountryOfOrigin = Core.Constants.CountryCodes.Australia,
				Tariff = "1020304050",
				ProductCode = "PART1",
				UQ = "NO",
				AddInfo = "BOB=WHERE",
				ZoneStatus = ZString.Empty,
				Qty = 50,
				ReceiveTotalQty = 100,
				ReceiveTotalCustomsValue = 1000m,
				PermitTransactionRefNumber = (ZString)"ORD3242-1"
			};
			var warehouse = result.Warehouse;
			if (warehouse != null)
			{
				warehouse.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			}
			factory.Save();
		}

		OrgAddress WarehouseAddress
		{
			get
			{
				if (warehouseAddress == null)
				{
					warehouseAddress = GetAddress(factory, "INTTEL");
					if (!warehouseAddress.CustomsCodes.Cast<OrgCusCode>().Any(x => x.OK_CodeType == "FM32"))
					{
						warehouseAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "FM32");
					}
				}
				warehouseAddress.Header.OH_RL_NKClosestPort = "USLAX";
				warehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				warehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
				warehouseAddress.OA_RN_NKCountryCode = "US";
				return warehouseAddress;
			}
		}
		OrgAddress warehouseAddress;

		internal static OrgAddress GetAddress(BusinessObjectFactory factory, ZString orgCode)
		{
			return factory?.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, orgCode)?.MainAddress;
		}

		BaseCusPermitHeader PermitHeader
		{
			get { return permitHeader; }
			set { permitHeader = value; }
		}
		BaseCusPermitHeader permitHeader;

		public void TestSyncOrdersWithoutDomesticLines()
		{
			var orgheader = factory.NewWithValidTestData<OrgHeader>();
			var manufacturer = factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "MA~";
			factory.Save();

			CreatePermit(orgheader.PK, manufacturer.MainAddress.PK);

			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.JE_OH_Importer = orgheader.PK;
			declaration.US_EntryFilerCode = "XJ5";
			orgheader.OH_IsWarehouseClient = true;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.PK;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INVNO1";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLine1.JI_Tariff = "2001100000";
			invoiceLine1.JI_LinePrice = 33m;

			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoiceLine2.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			invoiceLine2.JI_Tariff = "2001100000";
			invoiceLine2.JI_LinePrice = 11m;

			var invoiceLine3 = invoice1.InvoiceLines.AddNew();
			invoiceLine3.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLine3.JI_Tariff = "2001100000";
			invoiceLine3.JI_LinePrice = 44m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.PK;
			var entryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull(entryHeader);
			entryHeader.EntryNumber = "00003877";

			var tariff = factory.New<USCTariff>();
			tariff.UE_Tariff = "2001100000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Unit1 = "KG";
			tariff.UE_Unit2 = "CPS";
			factory.Save();

			var whsHelper = ObjectFactory.New<Warehouse.Integration.IWhsTransactionTestHelper>(factory);
			var org2 = declaration.Importer;
			org2.OH_Code = "ORG2";
			org2.OH_IsWarehouseClient = true;
			var part = whsHelper.CreateProduct(org2.PK, "ProductForTesting");
			var warehouse = whsHelper.CreateFTZWarehouse(declaration.WarehouseAddress.PK);
			factory.Save();
			whsHelper.CreateWhsReceiveWithInventory(org2.PK, warehouse.PK, "REC2", part.PK, 40m);

			var order1 = whsHelper.CreateWhsOrder(org2.PK, warehouse.PK, factory.NewWithValidTestData<OrgHeader>().PK, "ORD1");
			whsHelper.SetOrderType(order1.PK, "CPS");
			whsHelper.CreateWhsOrderLine(order1.PK, part.PK, 5m, "", "XJ5-00003877", "2001.10.0000", "US", "E", 7m, 15m, "KG", 13m, "CPS", "VisaNo=2", 5m, manufacturer.MainAddress.PK, "P");
			factory.Save();
			whsHelper.CreatePickNew(true, true, order1.PK);
			factory.Save();

			var order2 = whsHelper.CreateWhsOrder(org2.PK, warehouse.PK, factory.NewWithValidTestData<OrgHeader>().PK, "ORD2");
			whsHelper.SetOrderType(order2.PK, "CPS");
			whsHelper.CreateWhsOrderLine(order2.PK, part.PK, 5m, "", "XJ5-00003877", "2001.10.0000", "US", "E", 8m, 17m, "KG", 14m, "CPS", "VisaNo=2*ZoneStatus=D*Test=P", 5m, manufacturer.MainAddress.PK, "P");
			factory.Save();
			var pick2 = whsHelper.CreatePickNew(true, true, order2.PK);
			factory.Save();

			new FTZSynchronizeWithOrders(declaration).Synchronize();
			declaration.Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var reLoadDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(1, reLoadDeclaration.Invoices.Count);

			var reLoadInvoice = reLoadDeclaration.Invoices[0];
			AssertEquals(7m, reLoadInvoice.JZ_InvoiceAmount);
			AssertEquals("INVNO1", reLoadInvoice.JZ_InvoiceNumber);
		}

		public void TestDeleteAndCreateNewInvoiceLine()
		{
			var orgheader = factory.NewWithValidTestData<OrgHeader>();
			var manufacturer = factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "MA~";
			factory.Save();

			CreatePermit(orgheader.PK, manufacturer.MainAddress.PK);

			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.JE_OH_Importer = orgheader.PK;
			declaration.US_EntryFilerCode = "XJ5";
			orgheader.OH_IsWarehouseClient = true;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.PK;

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var invoiceLine2 = invoice1.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.PK;
			var entryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull(entryHeader);
			entryHeader.EntryNumber = "00003877";

			var tariff = factory.New<USCTariff>();
			tariff.UE_Tariff = "2001100000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Unit1 = "KG";
			tariff.UE_Unit2 = "CPS";
			tariff.UE_Unit3 = "G";
			factory.Save();

			var whsHelper = ObjectFactory.New<Warehouse.Integration.IWhsTransactionTestHelper>(factory);
			var org2 = declaration.Importer;
			org2.OH_Code = "ORG2";
			org2.OH_IsWarehouseClient = true;
			var part = whsHelper.CreateProduct(org2.PK, "ProductForTesting");
			var warehouse = whsHelper.CreateFTZWarehouse(declaration.WarehouseAddress.PK);
			factory.Save();
			whsHelper.CreateWhsReceiveWithInventory(org2.PK, warehouse.PK, "REC2", part.PK, 10m);
			var order2 = whsHelper.CreateWhsOrder(org2.PK, warehouse.PK, factory.NewWithValidTestData<OrgHeader>().PK, "ORD2");
			whsHelper.SetOrderType(order2.PK, "CPS");
			whsHelper.CreateWhsOrderLine(order2.PK, part.PK, 5m, "", "XJ5-00003877", "2001.10.0000", "US", "E", 10m, 15m, "KG", 20m, "CPS", "VisaNo=2*CustomsThirdQuantity=5*CustomsThirdQuantityUnit=G", 10m, manufacturer.MainAddress.PK, "P");
			factory.Save();

			whsHelper.CreatePickNew(true, true, order2.PK);
			factory.Save();

			var messageText = new FTZSynchronizeWithOrders(declaration).Synchronize();
			factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reLoadDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals("Remove all invoice headers", 1, reLoadDeclaration.Invoices.Count);
			var reLoadInvoice = reLoadDeclaration.Invoices[0];
			AssertEquals("Remove all invoice lines", 1, reLoadInvoice.InvoiceLines.Count);

			var newInvoiceLine = reLoadInvoice.InvoiceLines[0];
			AssertNotEquals("Create New Invoice Line", newInvoiceLine.PK, invoiceLine1.PK);
			AssertNotEquals("Create New Invoice Line", newInvoiceLine.PK, invoiceLine2.PK);

			Assert(newInvoiceLine.JI_AddInfo.Contains("VisaNo=2"));
			AssertEquals(5, newInvoiceLine.US_ManifestQty);
			AssertEquals("UNT", reLoadDeclaration.JE_TotalNoOfPacksPackType);
			AssertEquals("2001100000", newInvoiceLine.JI_Tariff);
			AssertEquals(5m, newInvoiceLine.JI_LinePrice);
			AssertEquals(7.5m, newInvoiceLine.JI_CustomsQuantity);
			AssertEquals("KG", newInvoiceLine.JI_CustomsUnitQty);
			AssertEquals(10m, newInvoiceLine.JI_CustomsSecondQuantity);
			AssertEquals("CPS", newInvoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("PRODUCTFORTESTING", newInvoiceLine.JI_PartNo);
			AssertEquals("should not override data", ZString.Empty, newInvoiceLine.US_UC_NKCountryOfOrigin);
			AssertEquals("should not override data", ZString.Empty, newInvoiceLine.US_SPI);
			AssertEquals(2.5m, newInvoiceLine.JI_CustomsThirdQuantity);
			AssertEquals("G", newInvoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals(5m, reLoadInvoice.JZ_InvoiceAmount);
			AssertEquals("UNT", reLoadDeclaration.JE_TotalNoOfPacksPackType);

			tariff.UE_Unit1 = "T";
			tariff.UE_Unit2 = "KG";
			tariff.UE_Unit3 = "";
			factory.Save();
			new FTZSynchronizeWithOrders(declaration).Synchronize();
			declaration.Factory.Save();
			newFactory = new BusinessObjectFactory();
			reLoadDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			reLoadInvoice = reLoadDeclaration.Invoices[0];
			newInvoiceLine = reLoadInvoice.InvoiceLines[0];
			AssertEquals("UQ should be set when JI_Tariff is set", ZDecimal.Zero, newInvoiceLine.JI_CustomsQuantity);
			AssertEquals("UQ should be set when JI_Tariff is set", ZDecimal.Zero, newInvoiceLine.JI_CustomsSecondQuantity);
			AssertEquals("UQ should be set when JI_Tariff is set", ZDecimal.Zero, newInvoiceLine.JI_CustomsThirdQuantity);
		}

		public void TestCreateSecondaryInvoiceLinesIfRequired()
		{
			var orgheader = factory.NewWithValidTestData<OrgHeader>();
			var manufacturer = factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "MA~";
			factory.Save();

			CreatePermit(orgheader.PK, manufacturer.MainAddress.PK);

			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = orgheader.PK;
			declaration.JE_OH_Supplier = factory.NewWithValidTestData<OrgHeader>().PK;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			declaration.US_EnableENS = false;
			orgheader.OH_IsWarehouseClient = true;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.PK;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.PK;
			var entryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull(entryHeader);
			entryHeader.EntryNumber = "00003877";

			var product = GetPartWithRelationship(declaration.Importer);
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "6203434030";

			var relatedPivot1 = pivot.Children.AddNew();
			relatedPivot1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			relatedPivot1.CI_TariffNum = "9801001010";

			var relatedPivot2 = pivot.Children.AddNew();
			relatedPivot2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			relatedPivot2.CI_TariffNum = "9701001010";

			var relatedPivot3 = pivot.Children.AddNew();
			relatedPivot3.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			relatedPivot3.CI_TariffNum = "9601001010";

			factory.Save();

			var whsHelper = ObjectFactory.New<Warehouse.Integration.IWhsTransactionTestHelper>(factory);

			var org2 = declaration.Importer;
			org2.OH_Code = "ORG2";
			org2.OH_IsWarehouseClient = true;
			var warehouse = whsHelper.CreateFTZWarehouse(declaration.WarehouseAddress.PK);
			factory.Save();
			var receive1 = whsHelper.CreateWhsReceiveWithInventory(org2.PK, warehouse.PK, "REC2", product.PK, 10m);

			var order2 = whsHelper.CreateWhsOrder(org2.PK, warehouse.PK, factory.NewWithValidTestData<OrgHeader>().PK, "ORD2");
			whsHelper.SetOrderType(order2.PK, "CPS");
			whsHelper.CreateWhsOrderLine(order2.PK, product.PK, 5m, "", "XJ5-00003877", "2001.10.0000", "US", "E", 2.3m, 1000m, "KG", 500m, "CPS", "VisaNo=2", 1m, manufacturer.MainAddress.PK, "P");
			factory.Save();
			whsHelper.CreatePickNew(true, true, order2.PK);
			factory.Save();
			var messageText = new FTZSynchronizeWithOrders(declaration).Synchronize();
			Assert(messageText.IsEmpty);
			declaration.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reLoadDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals("Remove all invoice headers", 1, reLoadDeclaration.Invoices.Count);
			var reLoadInvoice = reLoadDeclaration.Invoices[0];

			var invoiceLine = reLoadInvoice.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Tariff == "2001100000");
			AssertNotNull("Parent Line", invoiceLine);
			AssertEquals(2, invoiceLine.ChildLines.Count());

			AssertNotNull("Secondary InvoiceLine", invoiceLine.ChildLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Tariff == "9801001010"));
			AssertNotNull("Secondary InvoiceLine", invoiceLine.ChildLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Tariff == "9701001010"));

			AssertEquals("Product Related Line", 1, invoiceLine.ProductRelatedLines.Count());
		}

		Customs.Business.OrgSupplierPart GetPartWithRelationship(OrgHeader importer)
		{
			var result = factory.New<Customs.Business.OrgSupplierPart>();
			result.OP_PartNum = "ProductForTesting";
			result.RelatedOrganisations.AddOwner(importer);
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			factory = new BusinessObjectFactory();
			transactionManager = ((IDbConnected)factory).Connection.BeginTransactionWithManager();
		}

		protected override void TearDown()
		{
			transactionManager.Dispose();
			transactionManager = null;
			base.TearDown();
		}

		ITransactionManager transactionManager;
		BusinessObjectFactory factory;
	}
}
