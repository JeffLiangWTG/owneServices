using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class JobDeclarationDeepCloneStrategyAbstractTest<T> : TestCaseWithFactory where T : BaseJobDeclaration
	{
		protected JobDeclarationDeepCloneStrategy GetJobDeclarationDeepCloneStrategyToTest(T declarationToClone, CloneType cloneType) => GetJobDeclarationDeepCloneStrategyToTest(declarationToClone, cloneType, declarationToClone.Factory);

		protected abstract JobDeclarationDeepCloneStrategy GetJobDeclarationDeepCloneStrategyToTest(T declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn);

		public void TestDocAddressesAreClonedForSJobWithOverridenDeclaration()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var declaration = Factory.NewWithValidTestData<T>();
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_JS = shipment.PK;
			ErrorReporter.Clear();
			declaration.JE_OverrideFreightDefaults = true;
			Assert("Pre-condition: Importer Documentary Address is not empty", !declaration.ImporterDocumentaryAddress.IsEmpty);

			Factory.Save();
			var clonedDeclaration = (T)GetJobDeclarationDeepCloneStrategyToTest(declaration, CloneType.TemplateCopy).Clone();
			Assert("Importer Documentary Address is empty", !clonedDeclaration.ImporterDocumentaryAddress.IsEmpty);
			var importerDocumentaryAddressPK = clonedDeclaration.ImporterDocumentaryAddress.PK;

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var importerDocumentaryAddress = newFactory.Load<JobDocAddress>(importerDocumentaryAddressPK);
			AssertNotNull(importerDocumentaryAddress);
		}

		protected virtual ZString AddInfoTestData => "ZZZ";

		public void TestJobDeclarationClone()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			var importer = Factory.New<OrgHeader>();
			importer.MainAddress.OA_Address1 = "I AM THE IMPORTER";
			importer.OH_Code = "IMPCLNTST1";
			var supplier = Factory.New<OrgHeader>();
			supplier.MainAddress.OA_Address1 = "I AM THE SUPPLIER";
			supplier.OH_Code = "SUPCLNTST1";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "DUMMY VESSEL";

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.New<CommonShipment>();
			var declaration = Factory.New<T>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.AutoCreateChargesBasedOnIncoTerm = false;
			declaration.JE_AddInfo = AddInfoTestData;
			declaration.JE_EntryStatus = "AAA";
			declaration.JE_ConsolidatedCargoStatus = "BBB";
			declaration.JE_MasterBill = "M1";
			declaration.JE_HouseBill = "H1";
			declaration.JE_GoodsDescription = "LALA";
			declaration.JE_VesselName = "DUMMY VESSEL";
			declaration.JE_VoyageFlightNo = "V3245";
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			declaration.JE_Folio = "568";
			declaration.Transports[0].JW_IsLinked = false;
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			var originalTransport = declaration.Transports.AddNew();
			originalTransport.JW_Vessel = "HMS Pinafore";
			originalTransport.JW_VoyageFlight = "W323";

			var container = declaration.CusContainers.AddNew();
			var packGroup = (declaration.PrimaryHouseBill.PackingGroups.Count > 0) ? declaration.PrimaryHouseBill.PackingGroups[0] : declaration.PrimaryHouseBill.PackingGroups.AddNew();
			packGroup.CR_CO_Container = container.PK;

			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].Charges.AddNew("OFT", 10m, declaration.LocalCurrencyCode);
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();

			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			declaration.ImporterDocumentaryAddress.E2_CompanyName = "HELLO WORLD";

			var clonedDeclaration = (BaseJobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, CloneType.TemplateCopy).Clone();
			AssertSameFactoryTemplateCopyCloneWithoutShipment(clonedDeclaration);

			var clonedDeclaration2 = (BaseJobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, CloneType.CountryToCountryCopy).Clone();
			AssertSameFactoryCountryToCountryCopyCloneWithoutShipment(clonedDeclaration2);

			var clonedDeclaration3 = (BaseJobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, CloneType.DeepTemplateCopy).Clone();
			AssertSameFactoryDeepTemplateCopyCloneWithoutShipment(clonedDeclaration3);

			declaration.JE_JS = shipment.PK;
			ErrorReporter.Clear();
			var clonedDeclaration4 = (BaseJobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, CloneType.TemplateCopy).Clone();
			AssertSameFactoryTemplateCopyCloneWithShipment(clonedDeclaration4);

			var clonedDeclaration5 = (BaseJobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, CloneType.CountryToCountryCopy).Clone();
			AssertSameFactoryCountryToCountryCopyCloneWithShipment(clonedDeclaration5);

			var clonedDeclaration6 = (BaseJobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, CloneType.DeepTemplateCopy).Clone();
			AssertSameFactoryDeepTemplateCopyCloneWithShipment(clonedDeclaration6);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertSameFactoryTemplateCopyCloneWithoutShipment(newFactory.Load<BaseJobDeclaration>(clonedDeclaration.PK));
			AssertSameFactoryCountryToCountryCopyCloneWithoutShipment(newFactory.Load<BaseJobDeclaration>(clonedDeclaration2.PK));
			AssertSameFactoryDeepTemplateCopyCloneWithoutShipment(newFactory.Load<BaseJobDeclaration>(clonedDeclaration3.PK));
		}

		void AssertSameFactoryDeepTemplateCopyCloneWithShipment(BaseJobDeclaration declaration)
		{
			Assert(declaration.Transports.Count == 0);

			AssertEquals(AddInfoTestData, declaration.JE_AddInfo);
			AssertEquals("", declaration.JE_EntryStatus);
			AssertEquals("", declaration.JE_ConsolidatedCargoStatus);
			AssertEquals("LALA", declaration.JE_GoodsDescription);
			AssertEquals("", declaration.JE_GS_NKCusAgent);

			AssertShipmentDetailsAreCopied(declaration);
			AssertInvoiceDetailsAreCopied(declaration);
		}

		void AssertSameFactoryDeepTemplateCopyCloneWithoutShipment(BaseJobDeclaration declaration)
		{
			AssertEquals(AddInfoTestData, declaration.JE_AddInfo);
			AssertEquals("", declaration.JE_EntryStatus);
			AssertEquals("", declaration.JE_ConsolidatedCargoStatus);
			AssertEquals("LALA", declaration.JE_GoodsDescription);
			AssertEquals("", declaration.JE_GS_NKCusAgent);

			bool hasRightImporterDocAddress = false;
			bool hasRightSupplierDocAddress = false;

			foreach (JobDocAddress add in declaration.DocAddresses)
			{
				if (add.DocAddressType == DocAddressType.ImporterDocumentaryAddress && add.E2_Address1 == "I AM THE IMPORTER")
				{
					hasRightImporterDocAddress = true;
				}
				else if (add.DocAddressType == DocAddressType.SupplierDocumentaryAddress && add.E2_Address1 == "I AM THE SUPPLIER")
				{
					hasRightSupplierDocAddress = true;
				}
			}

			Assert("Found correct ImporterDocumentaryAddress on cloned dec", hasRightImporterDocAddress);
			Assert("Found correct SupplierDocumentaryAddress on cloned dec", hasRightSupplierDocAddress);

			AssertShipmentDetailsAreCopied(declaration);
			AssertInvoiceDetailsAreCopied(declaration);
			AssertTransportDetailsAreCopied(declaration);

			AssertEquals(declaration.PK, declaration.ImporterDocumentaryAddress.E2_ParentID);
			AssertEquals(true, declaration.ImporterDocumentaryAddress.E2_AddressOverride);
			AssertEquals("HELLO WORLD", declaration.ImporterDocumentaryAddress.E2_CompanyName);
		}

		void AssertShipmentDetailsAreCopied(BaseJobDeclaration declaration)
		{
			AssertEquals("M1", declaration.JE_MasterBill);
			AssertEquals("H1", declaration.JE_HouseBill);
			AssertEquals("DUMMY VESSEL", declaration.JE_VesselName);
			AssertEquals("V3245", declaration.JE_VoyageFlightNo);
			AssertEquals("568", declaration.JE_Folio);
			AssertEquals(2, declaration.Bills.Count);
			Bill bill1 = declaration.Bills[0];
			Bill bill2 = declaration.Bills[1];
			if (bill1.CU_BillNum == "H1")
			{
				bill1 = declaration.Bills[1];
				bill2 = declaration.Bills[0];
			}
			AssertEquals("M1", bill1.CU_BillNum);
			AssertEquals(ZGuid.Empty, bill1.CU_CU_ParentBill);
			AssertEquals(0, bill1.PackingGroups.Count);
			AssertEquals("H1", bill2.CU_BillNum);
			AssertEquals(bill1.PK, bill2.CU_CU_ParentBill);
			AssertEquals(1, bill2.PackingGroups.Count);
			AssertEquals(1, declaration.CusContainers.Count);
		}

		void AssertInvoiceDetailsAreCopied(BaseJobDeclaration declaration)
		{
			AssertEquals(1, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.Count);
			AssertEquals(1, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].Charges.Count);
			AssertEquals(1, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);
			AssertEquals(1, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.Count);
			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals(1, declaration.InvoiceLines.Count);
		}

		void AssertTransportDetailsAreCopied(BaseJobDeclaration declaration)
		{
			AssertEquals(2, declaration.Transports.Count);
			AssertEquals("DUMMY VESSEL", declaration.Transports[0].JW_Vessel);
			AssertEquals("V3245", declaration.Transports[0].JW_VoyageFlight);
			AssertEquals("HMS Pinafore", declaration.Transports[1].JW_Vessel);
			AssertEquals("W323", declaration.Transports[1].JW_VoyageFlight);
		}

		void AssertSameFactoryCountryToCountryCopyCloneWithoutShipment(BaseJobDeclaration declaration)
		{
			AssertEquals("", declaration.JE_AddInfo);
			AssertEquals("", declaration.JE_EntryStatus);
			AssertEquals("", declaration.JE_ConsolidatedCargoStatus);
			AssertEquals("LALA", declaration.JE_GoodsDescription);
			AssertEquals("", declaration.JE_GS_NKCusAgent);

			AssertShipmentDetailsAreCopied(declaration);
			AssertInvoiceDetailsAreCopied(declaration);
			AssertTransportDetailsAreCopied(declaration);

			AssertEquals(declaration.PK, declaration.ImporterDocumentaryAddress.E2_ParentID);
			AssertEquals(false, declaration.ImporterDocumentaryAddress.IsSavedByFactory);
			AssertNotEquals("HELLO WORLD", declaration.ImporterDocumentaryAddress.E2_CompanyName);
		}

		void AssertSameFactoryCountryToCountryCopyCloneWithShipment(BaseJobDeclaration declaration)
		{
			AssertEquals("", declaration.JE_AddInfo);
			AssertEquals("", declaration.JE_EntryStatus);
			AssertEquals("", declaration.JE_ConsolidatedCargoStatus);
			AssertEquals("LALA", declaration.JE_GoodsDescription);
			AssertEquals("", declaration.JE_GS_NKCusAgent);

			AssertShipmentDetailsAreCopied(declaration);
			AssertInvoiceDetailsAreCopied(declaration);
			AssertEquals(0, declaration.Transports.Count);

			AssertEquals(declaration.PK, declaration.ImporterDocumentaryAddress.E2_ParentID);
			AssertEquals(false, declaration.ImporterDocumentaryAddress.IsSavedByFactory);
			AssertNotEquals("HELLO WORLD", declaration.ImporterDocumentaryAddress.E2_CompanyName);
		}

		void AssertSameFactoryTemplateCopyCloneWithoutShipment(BaseJobDeclaration declaration)
		{
			AssertEquals(AddInfoTestData, declaration.JE_AddInfo);
			AssertEquals("", declaration.JE_EntryStatus);
			AssertEquals("", declaration.JE_ConsolidatedCargoStatus);
			AssertEquals("", declaration.JE_MasterBill);
			AssertEquals("", declaration.JE_HouseBill);
			AssertEquals("", declaration.JE_VesselName);
			AssertEquals("", declaration.JE_VoyageFlightNo);
			AssertEquals("", declaration.JE_Folio);
			AssertEquals("LALA", declaration.JE_GoodsDescription);
			AssertEquals("", declaration.JE_GS_NKCusAgent);

			bool hasRightImporterDocAddress = false;
			bool hasRightSupplierDocAddress = false;

			foreach (JobDocAddress add in declaration.DocAddresses)
			{
				if (add.DocAddressType == DocAddressType.ImporterDocumentaryAddress && add.E2_Address1 == "I AM THE IMPORTER")
				{
					hasRightImporterDocAddress = true;
				}
				else if (add.DocAddressType == DocAddressType.SupplierDocumentaryAddress && add.E2_Address1 == "I AM THE SUPPLIER")
				{
					hasRightSupplierDocAddress = true;
				}
			}

			Assert("Found correct ImporterDocumentaryAddress on cloned dec", hasRightImporterDocAddress);
			Assert("Found correct SupplierDocumentaryAddress on cloned dec", hasRightSupplierDocAddress);

			AssertEquals(0, declaration.Bills.Count);
			AssertEquals(0, declaration.CusContainers.Count);
			AssertEquals(0, declaration.PackingGroups.Count);
			AssertInvoiceDetailsAreCopied(declaration);
			AssertEquals(0, declaration.Transports.Count);

			AssertEquals(declaration.PK, declaration.ImporterDocumentaryAddress.E2_ParentID);
			AssertEquals(true, declaration.ImporterDocumentaryAddress.E2_AddressOverride);
			AssertEquals("HELLO WORLD", declaration.ImporterDocumentaryAddress.E2_CompanyName);
		}

		void AssertSameFactoryTemplateCopyCloneWithShipment(BaseJobDeclaration declaration)
		{
			Assert(declaration.Transports.Count == 0);

			AssertEquals(AddInfoTestData, declaration.JE_AddInfo);
			AssertEquals("", declaration.JE_EntryStatus);
			AssertEquals("", declaration.JE_ConsolidatedCargoStatus);
			AssertEquals("", declaration.JE_MasterBill);
			AssertEquals("", declaration.JE_HouseBill);
			AssertEquals("", declaration.JE_VesselName);
			AssertEquals("", declaration.JE_VoyageFlightNo);
			AssertEquals("", declaration.JE_Folio);
			AssertEquals("LALA", declaration.JE_GoodsDescription);
			AssertEquals("", declaration.JE_GS_NKCusAgent);

			AssertEquals(0, declaration.Bills.Count);
			AssertEquals(0, declaration.CusContainers.Count);
			AssertEquals(0, declaration.PackingGroups.Count);
			AssertInvoiceDetailsAreCopied(declaration);
		}

		public void TestApportionedChargesNotCloned()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var declaration = Factory.New<T>();
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoice.Charges.AddNew("OFT", 100m, declaration.LocalCurrencyCode);

				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 10000m;
				declaration.ResumeApportionment();
				AssertEquals(1, invoiceLine.ApportionedCharges.Count);
				invoiceLine.ApportionedCharges[0].J7_Amount = 150m;

				var clonedDeclaration = (BaseJobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, CloneType.TemplateCopy).Clone();
				clonedDeclaration.ResumeApportionment();

				AssertEquals("ApportionedCharge.Amount", 100m, clonedDeclaration.InvoiceLines[0].ApportionedCharges[0].J7_Amount);
			}
		}

		public void TestCloneUsingDifferentFactory()
		{
			var shipment = Factory.New<CommonShipment>();
			var declaration = Factory.New<T>();
			declaration.JE_AddInfo = AddInfoTestData;
			declaration.JE_EntryStatus = "AAA";
			declaration.JE_ConsolidatedCargoStatus = "BBB";
			declaration.JE_MasterBill = "M1";
			declaration.JE_HouseBill = "H1";
			declaration.JE_GoodsDescription = "LALA";

			var container = declaration.CusContainers.AddNew();
			var packGroup = (declaration.PrimaryHouseBill.PackingGroups.Count > 0) ? declaration.PrimaryHouseBill.PackingGroups[0] : declaration.PrimaryHouseBill.PackingGroups.AddNew();
			packGroup.CR_CO_Container = container.PK;

			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].Charges.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();

			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			declaration.ImporterDocumentaryAddress.E2_CompanyName = "HELLO WORLD";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var clonedDeclaration = (BaseJobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, CloneType.TemplateCopy, factory2).Clone();
			AssertTemplateCopyCloneUsingDifferentFactoryWithoutShipment(factory2, clonedDeclaration);

			var clonedDeclaration2 = (BaseJobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, CloneType.CountryToCountryCopy, factory2).Clone();
			AssertCountryToCountryCopyCloneUsingDifferentFactory(factory2, clonedDeclaration2);

			declaration.JE_JS = shipment.PK;
			ErrorReporter.Clear();
			var clonedDeclaration3 = (BaseJobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, CloneType.TemplateCopy, factory2).Clone();
			AssertTemplateCopyCloneUsingDifferentFactoryWithShipment(factory2, clonedDeclaration3);
		}

		void AssertTemplateCopyCloneUsingDifferentFactoryWithoutShipment(BusinessObjectFactory factory2, BaseJobDeclaration clonedDeclaration)
		{
			AssertEquals(factory2, clonedDeclaration.Factory);
			AssertEquals(0, clonedDeclaration.Bills.Count);
			AssertEquals(0, clonedDeclaration.CusContainers.Count);
			AssertEquals(0, clonedDeclaration.PackingGroups.Count);
			AssertEquals(factory2, clonedDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].Factory);
			AssertEquals(factory2, clonedDeclaration.Invoices[0].Factory);
			AssertEquals(factory2, clonedDeclaration.InvoiceLines[0].Factory);

			AssertEquals(clonedDeclaration.PK, clonedDeclaration.ImporterDocumentaryAddress.E2_ParentID);
			AssertEquals(true, clonedDeclaration.ImporterDocumentaryAddress.E2_AddressOverride);
			AssertEquals("HELLO WORLD", clonedDeclaration.ImporterDocumentaryAddress.E2_CompanyName);

			factory2.Save();

			var reloadedDeclaration = Factory.Load<BaseJobDeclaration>(clonedDeclaration.PK);
			AssertEquals(reloadedDeclaration.PK, reloadedDeclaration.ImporterDocumentaryAddress.E2_ParentID);
			AssertEquals(true, reloadedDeclaration.ImporterDocumentaryAddress.E2_AddressOverride);
			AssertEquals("HELLO WORLD", reloadedDeclaration.ImporterDocumentaryAddress.E2_CompanyName);
		}

		void AssertCountryToCountryCopyCloneUsingDifferentFactory(BusinessObjectFactory factory2, BaseJobDeclaration clonedDeclaration)
		{
			AssertEquals(factory2, clonedDeclaration.Factory);
			AssertEquals(factory2, clonedDeclaration.Bills[0].Factory);
			var clonedContainer = clonedDeclaration.CusContainers[0];
			AssertEquals(factory2, clonedContainer.Factory);
			AssertEquals(factory2, clonedContainer.JobContainer.Factory);
			AssertEquals(factory2, clonedDeclaration.PackingGroups[0].Factory);
			AssertEquals(factory2, clonedDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].Factory);
			AssertEquals(factory2, clonedDeclaration.Invoices[0].Factory);
			AssertEquals(factory2, clonedDeclaration.InvoiceLines[0].Factory);

			AssertEquals(clonedDeclaration.PK, clonedDeclaration.ImporterDocumentaryAddress.E2_ParentID);
			AssertEquals(false, clonedDeclaration.ImporterDocumentaryAddress.E2_AddressOverride);

			factory2.Save();

			var reloadedDeclaration = Factory.Load<BaseJobDeclaration>(clonedDeclaration.PK);
			AssertEquals(reloadedDeclaration.PK, reloadedDeclaration.ImporterDocumentaryAddress.E2_ParentID);
			AssertEquals(false, reloadedDeclaration.ImporterDocumentaryAddress.E2_AddressOverride);
		}

		void AssertTemplateCopyCloneUsingDifferentFactoryWithShipment(BusinessObjectFactory factory2, BaseJobDeclaration clonedDeclaration)
		{
			AssertEquals(factory2, clonedDeclaration.Factory);
			AssertEquals(0, clonedDeclaration.Bills.Count);
			AssertEquals(0, clonedDeclaration.CusContainers.Count);
			AssertEquals(0, clonedDeclaration.PackingGroups.Count);
			AssertEquals(factory2, clonedDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].Factory);
			AssertEquals(factory2, clonedDeclaration.Invoices[0].Factory);
			AssertEquals(factory2, clonedDeclaration.InvoiceLines[0].Factory);
		}

		public void TestJobDeclarationNotesClone()
		{
			var declaration = Factory.New<T>();
			declaration.Notes.AddNew();

			var clonedDeclaration = (BaseJobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, CloneType.TemplateCopy).Clone();
			AssertEquals(1, clonedDeclaration.Notes.GetAllNotes().Count);

			var shipment = Factory.New<CommonShipment>();
			declaration.JE_JS = shipment.PK;

			var clonedDeclaration2 = (BaseJobDeclaration)GetJobDeclarationDeepCloneStrategyToTest(declaration, CloneType.TemplateCopy).Clone();
			AssertEquals(0, clonedDeclaration2.Notes.GetAllNotes().Count);
		}

		public void TestCloningADeclarationWithNoGroupInvoiceHeaders_ShouldCreateOneBeforeCloning()
		{
			var declaration = Factory.NewWithValidTestData<T>();
			var invoice = declaration.Invoices.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Add(invoice);
			declaration.JobComInvoiceGroupHeaders.RemoveAll();
			var strategy = GetJobDeclarationDeepCloneStrategyToTest(declaration, CloneType.CountryToCountryCopyWithinShipment);
			var cloned = (BaseJobDeclaration)strategy.Clone();
			AssertEquals("Declaration to be cloned does not have the correct number of group invoice headers.", 0, declaration.JobComInvoiceGroupHeaders.Count(i => i != null));
			AssertEquals("Cloned declaration does not have the correct number of group invoice headers.", 1, cloned.JobComInvoiceGroupHeaders.Count(i => i != null));
			AssertEquals("Invoice was not copied during the cloning process.", 1, cloned.Invoices.Count);
		}

		public void TestTemplateCopyCusPackingList()
		{
			var declaration = Factory.New<T>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew().JI_InvoiceQuantity = 3;
			if (declaration.SupportsCusPackingList)
			{
				var packingList = declaration.LoadOrCreateCusPackingList(Factory);
				packingList.CUL_PackageDescription = "test desc1";
				packingList.PackageJob.Packages.AddNew();
			}

			Factory.Save();

			var clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			var clonedPackingList = clonedDeclaration.LoadCusPackingList(clonedDeclaration.Factory);
			if (clonedDeclaration.SupportsCusPackingList)
			{
				AssertNotNull("CusPackingList should be cloned when target supports PackingList", clonedPackingList);
				AssertEquals("test desc1", clonedPackingList.CUL_PackageDescription);
				AssertEquals("One Package should be cloned", 1, clonedPackingList.PackageJob.Packages.Count);
				Assert("Validation should be suspended", !clonedPackingList.HasNotifications());
				Assert("Setting HasChanges should be suspended", !clonedPackingList.HasChanges);

				clonedPackingList.Validation.ValidateAll();
				Assert("Cloned CusPackingList should NOT be have errors", !clonedPackingList.HasErrors);
			}
			else
			{
				AssertNull("CusPackingList should NOT be cloned when target does not supports PackingList", clonedPackingList);
			}
		}
	}
}
