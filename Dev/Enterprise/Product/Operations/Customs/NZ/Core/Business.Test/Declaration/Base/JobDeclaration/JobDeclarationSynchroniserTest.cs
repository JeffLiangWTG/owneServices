using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using SynchroniseAction = Enterprise.Customs.Business.SynchroniseAction;
using SynchroniseEventArgs = Enterprise.Customs.Business.SynchroniseEventArgs;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	public class JobDeclarationSynchroniserTest : Customs.Business.Testing.JobDeclarationSynchroniserTest
	{
		protected override string ExpectedJE_TotalNoOfPacksPackTypeForTestSynchroniserBuyersConsolLead
		{
			get { return "PX"; }
		}

		public void TestSynchroniseDirectConsol_CS00168612()
		{
			Consol.JK_MasterBillNum = "MBOL1";
			Consol.JK_AgentType = "DRT";
			Assert(Consol.IsDirect);

			Shipment.JS_HouseBill = Consol.JK_MasterBillNum;
			TestHelper.MakeConsolRelevantToDeclaration(Consol, Declaration);
			Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("Master bill - House Bill is not required for TSW directs", "MBOL1", Declaration.JE_MasterBill);
			Assert(!Declaration.JE_HouseBillInfo.HasMessageErrors());
		}

		public void TestSynchronise_ContainersForAir()
		{
			const string ContainerNumber = "CRXU1234568";
			Shipment.JS_RL_NKOrigin = "DEFRA";
			Shipment.JS_RL_NKDestination = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode))).Code;

			CommonContainer container = Consol.Containers.AddNew();
			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(Consol, container);

			container.JC_ContainerNum = ContainerNumber;
			container.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20NOR").PK;
			Factory.Save(); // Users cannot create a declaration from the Consol Screen only from the Shipment Screen, Consol on the shipment are already in database
			TestHelper.MakeConsolRelevantToDeclaration(Consol, Declaration);

			JobDeclaration declaration = Declaration;
			AssertEquals("Precondition: Shipment should have one Declaration", 1, Shipment.Declarations.Length);
			AssertEquals("Precondition: Shipment's Declaration should be the one we're looking at", declaration.PK, Shipment.Declarations[0].PK);
			AssertEquals("Precondition: Declaration should have no containers", 0, declaration.CusContainers.Count);

			Consol.JK_TransportMode = JobTransportModeList.Codes.Air;
			Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals(JobTransportModeList.Codes.Air, declaration.JE_TransportMode);
			AssertEquals("Container shouldn't be sync'ed for Air mode", 0, declaration.CusContainers.Count);

			Consol.JK_TransportMode = JobTransportModeList.Codes.Sea;
			Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals(JobTransportModeList.Codes.Sea, declaration.JE_TransportMode);
			AssertEquals("Container should be sync'ed for Sea mode", 1, declaration.CusContainers.Count);
			var cusContainer = declaration.CusContainers[0];
			AssertEquals(false, cusContainer.IsDeleted);
			AssertEquals(container, cusContainer.JobContainer);

			Consol.JK_TransportMode = JobTransportModeList.Codes.Air;
			Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals(JobTransportModeList.Codes.Air, declaration.JE_TransportMode);
			AssertEquals("Container shouldn't be sync'ed for Air mode and also removed", 0, declaration.CusContainers.Count);
			AssertEquals(true, cusContainer.IsDeleted);
			AssertEquals("Consol's container should not be deleted", false, container.IsDeleted);
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });
		}

		public void TestMISCSyncsFromOverriddenAddressesOnShipmentForSimplifiedEntries()
		{
			ZGuid miscOrgPK = Declaration.CachedMiscOrgPK;
			Declaration.JE_OverrideFreightDefaults = false;
			Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			Shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals("Declaration.JE_OH_Supplier", miscOrgPK, Declaration.JE_OH_Supplier);
			Shipment.ConsignorDocumentaryAddress.E2_CompanyName = "FRED'S SPARE NUTS";

			Shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals("Declaration.JE_OH_Importer", miscOrgPK, Declaration.JE_OH_Importer);
			Shipment.ConsigneeDocumentaryAddress.E2_CompanyName = "JOHN'S SPARE BOLTS";

			AssertEquals("FRED'S SPARE NUTS", Declaration.MiscSupplierName);
			AssertEquals("JOHN'S SPARE BOLTS", Declaration.MiscImporterName);

			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "ANOTHER BUGGER";
			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "HIS MATE";

			Shipment.ConsignorDocumentaryAddress.E2_AddressOverride = false;
			Shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;
			AssertEquals("Declaration.JE_OH_Supplier", supplier.PK, Declaration.JE_OH_Supplier);

			Shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = false;
			Shipment.ConsigneeDocumentaryAddress.OrganisationPK = importer.PK;
			AssertEquals("Declaration.JE_OH_Importer", importer.PK, Declaration.JE_OH_Importer);

			AssertEquals("", Declaration.MiscSupplierName);
			AssertEquals("", Declaration.MiscImporterName);

			Shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals("Declaration.JE_OH_Supplier", miscOrgPK, Declaration.JE_OH_Supplier);
			Shipment.ConsignorDocumentaryAddress.E2_CompanyName = "FRED'S SPARE NUTS";

			Shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals("Declaration.JE_OH_Importer", miscOrgPK, Declaration.JE_OH_Importer);
			Shipment.ConsigneeDocumentaryAddress.E2_CompanyName = "JOHN'S SPARE BOLTS";

			AssertEquals("FRED'S SPARE NUTS", Declaration.MiscSupplierName);
			AssertEquals("JOHN'S SPARE BOLTS", Declaration.MiscImporterName);
		}

		public void TestAmountGetsSyncronisedWhenADeclarationGetsFlickedAcrossToECI()
		{
			Shipment.JS_GoodsValue = 134.00m;
			Shipment.JS_RX_NKGoodsValueCurr = AUDCurrency.RX_Code;
			Declaration.JE_OverrideFreightDefaults = false;
			Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			AssertEquals("Precondition: Declaration.Invoices.Count", 0, Declaration.Invoices.Count);

			AssertEquals("Declaration.JE_ECI_InvoiceAmountInfo.ReadOnly", true, Declaration.JE_ECI_InvoiceAmountInfo.ReadOnly);
			AssertEquals("Declaration.JE_ECI_InvoiceCurrencyInfo.ReadOnly", true, Declaration.JE_ECI_InvoiceCurrencyInfo.ReadOnly);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Declaration.Invoices.Count", 1, Declaration.Invoices.Count);
			AssertEquals("Declaration.JE_ECI_InvoiceAmount", 134.00m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("Declaration.JE_ECI_InvoiceCurrency", AUDCurrency.PK, Declaration.JE_ECI_InvoiceCurrency);
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices[0];
			AssertEquals("invoiceHeader.JZ_InvoiceAmount", 134.00m, invoiceHeader.JZ_InvoiceAmount);
			AssertEquals("invoiceHeader.JZ_RX_NKInvoice_Currency", AUDCurrency.RX_Code, invoiceHeader.JZ_RX_NKInvoice_Currency);
			AssertEquals("Declaration.JE_ECI_InvoiceAmountInfo.ReadOnly", true, Declaration.JE_ECI_InvoiceAmountInfo.ReadOnly);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("Declaration.Invoices.Count", 1, Declaration.Invoices.Count);
			invoiceHeader = Declaration.Invoices[0];
			AssertEquals("invoiceHeader.JZ_InvoiceAmount", 134.00m, invoiceHeader.JZ_InvoiceAmount);
			AssertEquals("invoiceHeader.JZ_RX_NKInvoice_Currency", AUDCurrency.RX_Code, invoiceHeader.JZ_RX_NKInvoice_Currency);
			AssertEquals("Declaration.JE_ECI_InvoiceAmountInfo.ReadOnly", true, Declaration.JE_ECI_InvoiceAmountInfo.ReadOnly);

			Shipment.JS_GoodsValue = 238.00m;
			Shipment.JS_RX_NKGoodsValueCurr = USDCurrency.RX_Code;
			AssertEquals("Declaration.Invoices.Count", 1, Declaration.Invoices.Count);
			invoiceHeader = Declaration.Invoices[0];
			AssertEquals("invoiceHeader.JZ_InvoiceAmount", 134.00m, invoiceHeader.JZ_InvoiceAmount);
			AssertEquals("invoiceHeader.JZ_RX_NKInvoice_Currency", AUDCurrency.RX_Code, invoiceHeader.JZ_RX_NKInvoice_Currency);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Declaration.Invoices.Count", 1, Declaration.Invoices.Count);
			AssertEquals("Declaration.JE_ECI_InvoiceAmount", 238.00m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("Declaration.JE_ECI_InvoiceCurrency", USDCurrency.PK, Declaration.JE_ECI_InvoiceCurrency);
			invoiceHeader = Declaration.Invoices[0];
			AssertEquals("invoiceHeader.JZ_InvoiceAmount", 238.00m, invoiceHeader.JZ_InvoiceAmount);
			AssertEquals("invoiceHeader.JZ_RX_NKInvoice_Currency", USDCurrency.RX_Code, invoiceHeader.JZ_RX_NKInvoice_Currency);

			Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Stop));
			Declaration.JE_OverrideFreightDefaults = true;
			Shipment.JS_GoodsValue = 244.00m;
			Shipment.JS_RX_NKGoodsValueCurr = AUDCurrency.RX_Code;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Declaration.Invoices.Count", 1, Declaration.Invoices.Count);
			AssertEquals("Declaration.JE_ECI_InvoiceAmount", 238.00m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("Declaration.JE_ECI_InvoiceCurrency", USDCurrency.PK, Declaration.JE_ECI_InvoiceCurrency);
			invoiceHeader = Declaration.Invoices[0];
			AssertEquals("invoiceHeader.JZ_InvoiceAmount", 238.00m, invoiceHeader.JZ_InvoiceAmount);
			AssertEquals("invoiceHeader.JZ_RX_NKInvoice_Currency", USDCurrency.RX_Code, invoiceHeader.JZ_RX_NKInvoice_Currency);

			AssertEquals("Declaration.JE_ECI_InvoiceAmountInfo.ReadOnly", false, Declaration.JE_ECI_InvoiceAmountInfo.ReadOnly);
			AssertEquals("Declaration.JE_ECI_InvoiceCurrencyInfo.ReadOnly", false, Declaration.JE_ECI_InvoiceCurrencyInfo.ReadOnly);
		}

		public void TestInvoiceAmountNotSyncronisedOnFormalEntries()
		{
			AssertEquals("Precondition: Declaration.Invoices.Count", 0, Declaration.Invoices.Count);
			Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			Shipment.JS_GoodsValue = 134.00m;
			Shipment.JS_RX_NKGoodsValueCurr = AUDCurrency.RX_Code;
			AssertEquals("Declaration.Invoices.Count", 0, Declaration.Invoices.Count);
			AssertEquals("Declaration.JE_ECI_InvoiceAmount", 0.00m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("Declaration.JE_ECI_InvoiceCurrency", ZGuid.Empty, Declaration.JE_ECI_InvoiceCurrency);
		}

		public void TestECIAmountsAreSyncronisedButOnlyOnDecsWithOneInvoiceHeaderAndNoLines()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Precondition: Declaration.Invoices.Count", 0, Declaration.Invoices.Count);
			Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			Shipment.JS_GoodsValue = 134.00m;
			Shipment.JS_RX_NKGoodsValueCurr = AUDCurrency.RX_Code;
			AssertEquals("Declaration.Invoices.Count", 1, Declaration.Invoices.Count);
			JobComInvoiceHeader invoiceHeader1 = Declaration.Invoices[0];
			AssertEquals("InvoiceHeader1.JZ_InvoiceAmount", 134.00m, invoiceHeader1.JZ_InvoiceAmount);
			AssertEquals("InvoiceHeader1.JZ_RX_NKInvoice_Currency", AUDCurrency.RX_Code, invoiceHeader1.JZ_RX_NKInvoice_Currency);
			AssertEquals("Declaration.JE_ECI_InvoiceAmount", 134.00m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("Declaration.JE_ECI_InvoiceCurrency", AUDCurrency.PK, Declaration.JE_ECI_InvoiceCurrency);
			JobComInvoiceLine invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 134.00m;
			Shipment.JS_GoodsValue = 99.00m;
			Shipment.JS_RX_NKGoodsValueCurr = Factory.New<RefCurrency>().RX_Code;
			AssertEquals("InvoiceHeader1.JZ_InvoiceAmount", 134.00m, invoiceHeader1.JZ_InvoiceAmount);
			AssertEquals("InvoiceHeader1.JZ_RX_NKInvoice_Currency", AUDCurrency.RX_Code, invoiceHeader1.JZ_RX_NKInvoice_Currency);
			AssertEquals("Declaration.JE_ECI_InvoiceAmount", 134.00m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("Declaration.JE_ECI_InvoiceCurrency", AUDCurrency.PK, Declaration.JE_ECI_InvoiceCurrency);
			invoiceHeader1.JobComInvoiceLines.RemoveAndDelete(invoiceLine1);
			Shipment.JS_GoodsValue = 135.00m;
			Shipment.JS_RX_NKGoodsValueCurr = USDCurrency.RX_Code;
			AssertEquals("InvoiceHeader1.JZ_InvoiceAmount", 135.00m, invoiceHeader1.JZ_InvoiceAmount);
			AssertEquals("InvoiceHeader1.JZ_RX_NKInvoice_Currency", USDCurrency.RX_Code, invoiceHeader1.JZ_RX_NKInvoice_Currency);
			AssertEquals("Declaration.JE_ECI_InvoiceAmount", 135.00m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("Declaration.JE_ECI_InvoiceCurrency", USDCurrency.PK, Declaration.JE_ECI_InvoiceCurrency);
			JobComInvoiceHeader invoiceHeader2 = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			Shipment.JS_GoodsValue = 99.00m;
			Shipment.JS_RX_NKGoodsValueCurr = Factory.New<RefCurrency>().RX_Code;
			AssertEquals("InvoiceHeader1.JZ_InvoiceAmount", 135.00m, invoiceHeader1.JZ_InvoiceAmount);
			AssertEquals("InvoiceHeader1.JZ_RX_NKInvoice_Currency", USDCurrency.RX_Code, invoiceHeader1.JZ_RX_NKInvoice_Currency);
			AssertEquals("InvoiceHeader2.JZ_InvoiceAmount", 0.00m, invoiceHeader2.JZ_InvoiceAmount);
			Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Delete(invoiceHeader1);
			Shipment.JS_GoodsValue = 134.00m;
			Shipment.JS_RX_NKGoodsValueCurr = AUDCurrency.RX_Code;
			AssertEquals("InvoiceHeader2.JZ_InvoiceAmount", 134.00m, invoiceHeader2.JZ_InvoiceAmount);
			AssertEquals("InvoiceHeader2.JZ_RX_NKInvoice_Currency", AUDCurrency.RX_Code, invoiceHeader2.JZ_RX_NKInvoice_Currency);
		}

		public override void TestSynchronise_FromShipmentOtherDetails()
		{
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode))).Code;
			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode))).Code;

			ZQuery uSConsignorFilter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, true);
			uSConsignorFilter.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.Equal, "USLAX"); //we love deb
			var partyInCurrentCountry = Factory.LoadTop1<OrgHeader>(new ZQuery());
			partyInCurrentCountry.OH_RL_NKClosestPort = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";
			partyInCurrentCountry.OH_IsConsignee = true;

			Shipment.ConsigneePK = partyInCurrentCountry.PK;
			Shipment.ConsignorPK = Factory.LoadTop1<OrgHeader>(uSConsignorFilter).PK;
			Shipment.JS_GoodsDescription = "TEST DESCRIPTION OF THE GOODS";
			Shipment.JS_TransportMode = "AIR";
			Shipment.JS_E_ARV = new ZDateTime(2003, 12, 31);
			Shipment.JS_E_DEP = new ZDateTime(2003, 12, 30);
			Shipment.JS_ShippedOnBoardDate = new ZDateTime(2003, 12, 29);
			Shipment.JS_HouseBill = "H123456";
			Shipment.JS_ActualWeight = 10;
			Shipment.JS_UnitOfWeight = "KG";
			Shipment.JS_ActualVolume = 3;
			Shipment.JS_UnitOfVolume = "M3";
			Shipment.JS_OuterPacks = 2;
			Shipment.JS_F3_NKPackType = "PLT";
			Shipment.JS_TotalPackageCount = 3;//inner pack
			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode))).Code;

			Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("Goods Description", Shipment.JS_GoodsDescription, Declaration.JE_GoodsDescription);
			AssertEquals("Consignee", Shipment.ConsigneePK, Declaration.JE_OH_Importer);
			AssertEquals("Consignor", Shipment.ConsignorPK, Declaration.JE_OH_Supplier);
			AssertEquals("HouseBill", Shipment.JS_HouseBill, Declaration.JE_HouseBill);
			AssertEquals("ActualWeight", Shipment.JS_ActualWeight, Declaration.JE_TotalWeight);
			AssertEquals("Unit of Weight", Shipment.JS_UnitOfWeight, Declaration.JE_TotalWeightUnit);
			AssertEquals("ActualVolume", Shipment.JS_ActualVolume, Declaration.JE_TotalVolume);
			AssertEquals("Unit of Volume", Shipment.JS_UnitOfVolume, Declaration.JE_TotalVolumeUnit);
			AssertEquals("Outer packs", Shipment.JS_OuterPacks, Declaration.JE_TotalNoOfPacks);
			AssertEquals("Outer pack type", "PX", Declaration.JE_TotalNoOfPacksPackType);
			AssertEquals("Origin port", Shipment.JS_RL_NKOrigin, Declaration.JE_RL_NKOrigin);
			AssertEquals("Destination port", Shipment.JS_RL_NKDestination, Declaration.JE_RL_NKFinalDestination);
		}

		public override void TestJobDeclarationSynchroniser()
		{
			Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			Shipment.JS_RS_NKServiceLevel = "XYZ";
			AssertEquals("Declaration.JE_RS_NKServiceLevel", "XYZ", Declaration.JE_RS_NKServiceLevel);

			Shipment.JS_TransportMode = "";
			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("Declaration.JE_TransportMode", Enterprise.Core.Constants.TransportModes.Sea, Declaration.JE_TransportMode);

			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			AssertEquals("Declaration.JE_ContainerMode", Core.Constants.ContainerModes.NonContainerised, Declaration.JE_ContainerMode);

			var testConsignor = GetOverseasConsignor();
			Shipment.ConsignorPK = testConsignor.PK;
			AssertEquals("Declaration.JE_OH_Supplier", testConsignor.PK, Declaration.JE_OH_Supplier);

			var testConsignee = GetLocalConsignee();
			Shipment.ConsigneePK = testConsignee.PK;
			AssertEquals("Declaration.JE_OH_Importer", testConsignee.PK, Declaration.JE_OH_Importer);

			Transport.JW_ATA = ZDateTime.Today.AddDays(3);
			AssertEquals("Declaration.JE_DateOfArrival", Consol.JK_JX_JB_A_ARV, Declaration.JE_DateOfArrival);

			Transport.JW_ATD = ZDateTime.Today.AddDays(1);
			AssertEquals("Declaration.JE_ExportDate", Consol.JK_JX_JA_A_DEP, Declaration.JE_ExportDate);

			Transport.JW_Vessel = "12345";
			AssertEquals("Declaration.JE_VesselName", Consol.JK_JX_JV_NKVessel, Declaration.JE_VesselName);

			Shipment.JS_HouseBill = TestHouseBillNumber;
			AssertEquals("Declaration.JE_HouseBill", TestHouseBillNumber, Declaration.JE_HouseBill);

			Shipment.JS_ActualWeight = 3210.0m;
			AssertEquals("Declaration.JE_TotalWeight", 3210.0m, Declaration.JE_TotalWeight);

			Shipment.JS_UnitOfWeight = Enterprise.Core.Constants.Weight.Pounds;
			AssertEquals("Declaration.JE_TotalWeightUnit", Enterprise.Core.Constants.Weight.Pounds, Declaration.JE_TotalWeightUnit);

			Shipment.JS_ActualVolume = 13.2m;
			AssertEquals("Declaration.JE_TotalVolume", 13.2m, Declaration.JE_TotalVolume);

			Shipment.JS_UnitOfVolume = Enterprise.Core.Constants.Volume.CubicFeet;
			AssertEquals("Declaration.JE_TotalVolumeUnit", Enterprise.Core.Constants.Volume.CubicFeet, Declaration.JE_TotalVolumeUnit);

			Shipment.JS_OuterPacks = 12;
			AssertEquals("Declaration.JE_TotalNoOfPacks", 12, Declaration.JE_TotalNoOfPacks);

			Shipment.JS_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Box;
			Shipment.JS_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Pallet;

			AssertEquals("Declaration.JE_TotalNoOfPacksPackType", "PX", Declaration.JE_TotalNoOfPacksPackType);

			Shipment.JS_GoodsDescription = TestGoodsDescription;
			AssertEquals("Declaration.JE_GoodsDescription", TestGoodsDescription, Declaration.JE_GoodsDescription);

			Shipment.JS_E_ARV = ZDateTime.Today.AddDays(1);
			AssertEquals("Declaration.JE_DateOfFirstArrival", Shipment.JS_E_ARV, Declaration.JE_DateAtFinalDestination);

			Shipment.JS_E_DEP = ZDateTime.Today.AddDays(4);
			AssertEquals("Declaration.JE_DateAtOrigin", Shipment.JS_E_DEP, Declaration.JE_DateAtOrigin);
		}

		public override void TestLoadAndDischargeSyncroniseToCorrectTransport_Import()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "NZWLG";
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			Transport transport0 = consol.Transports[0];
			transport0.JW_RL_NKLoadPort = "USLAX";
			transport0.JW_RL_NKDiscPort = "JPHIU";
			transport0.JW_LegOrder = 1;
			transport0.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			transport0.JW_Vessel = "ADMIRALENGRACHT";

			Transport transport1 = consol.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "JPHIU";
			transport1.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_LegOrder = 2;
			transport1.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			transport1.JW_Vessel = "ADMIRALENGRACHT";

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "NZAKL";
			transport2.JW_RL_NKDiscPort = "NZTRG";
			transport2.JW_LegOrder = 3;
			transport2.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			transport2.JW_Vessel = "ADMIRALENGRACHT";

			Transport transport3 = consol.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "NZTRG";
			transport3.JW_RL_NKDiscPort = "NZWLG";
			transport3.JW_LegOrder = 4;
			transport3.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			transport3.JW_Vessel = "ARAFURA";

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			AssertEquals("Precondition: Declaration.JE_RL_NKPortOfLoading", "", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Precondition: Declaration.JE_RL_NKPortOfArrival", "", declaration.JE_RL_NKPortOfArrival);
			declaration.JE_JS = shipment.PK;
			decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			AssertEquals("Declaration.JE_RL_NKPortOfLoading", "JPHIU", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Declaration.JE_RL_NKPortOfArrival", "NZTRG", declaration.JE_RL_NKPortOfArrival);
		}

		public new void TestJobDeclarationSynchroniser_TransportMode()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USLAX";
			var shipment = consol.Shipments.AddNew();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Declaration.JE_TransportMode", JobTransportModeList.Codes.Sea, declaration.JE_TransportMode);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Courier;
			AssertEquals("Declaration.JE_TransportMode", JobTransportModeList.Codes.Post, declaration.JE_TransportMode);
		}

		#region Implementation

		protected override Customs.Business.BaseJobDeclaration GetJobDeclaration()
		{
			return JobDeclaration.New(Factory);
		}

		protected override bool ShouldIgnoreInfoForDetection(ZPropertyInfo info)
		{
			switch (info.Name)
			{
				case NZAddInfo.Schema.ZN_ImporterName:
				case NZAddInfo.Schema.ZN_SupplierName:
				case JobDeclaration.Schema.JE_ECI_InvoiceAmount:
				case JobDeclaration.Schema.JE_ECI_InvoiceCurrency:
					return true;
				default:
					return base.ShouldIgnoreInfoForDetection(info);
			}
		}

		protected RefCurrency USDCurrency
		{
			get { return fUSDCurrency ?? (fUSDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "USD")); }
		}
		RefCurrency fUSDCurrency;

		protected RefCurrency AUDCurrency
		{
			get { return fAUDCurrency ?? (fAUDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD")); }
		}
		RefCurrency fAUDCurrency;

		protected JobDeclaration Declaration
		{
			get { return fDeclaration ?? (fDeclaration = GetNewDeclaration()); }
		}
		JobDeclaration fDeclaration;
		JobDeclaration GetNewDeclaration()
		{
			JobDeclaration result = Factory.New<JobDeclaration>();
			result.JE_JS = Shipment.PK;
			return result;
		}

		protected new ForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<ForwardingConsol>();
					fConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
					fConsol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.FCL;

					Transport transport = fConsol.Transports[0];
					transport.JW_RL_NKDiscPort = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";
				}
				return fConsol;
			}
		}
		ForwardingConsol fConsol;

		protected new ForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Consol.Shipments.AddNew();
					fShipment.JS_HouseBill = "HB1";
					fShipment.JS_RL_NKDestination = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;
				}
				return fShipment;
			}
		}
		ForwardingShipment fShipment;

		protected JobDeclarationSynchroniser Synchroniser
		{
			get { return fSynchroniser ?? (fSynchroniser = new JobDeclarationSynchroniser(Declaration)); }
		}
		JobDeclarationSynchroniser fSynchroniser;
		#endregion
	}
}
