using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ForwardingConsol = Enterprise.Freight.Forwarding.Business.ForwardingConsol;
using ForwardingShipment = Enterprise.Freight.Forwarding.Business.ForwardingShipment;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ImportJobDeclaration))]
	class ImportJobDeclarationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAlwaysCloneImporterAndSupplier()
		{
			AlwaysCloneImporterAndSupplierTest(Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.NewZealand);
		}

		public void TestAlwaysCloneImporterAndSupplierFromAUToEU()
		{
			AlwaysCloneImporterAndSupplierTest(Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.UnitedKingdom);
		}

		void AlwaysCloneImporterAndSupplierTest(ZString fromCountry, ZString toCountry)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(fromCountry))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_OverrideFreightDefaults = false;

				declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
				declaration.JE_OH_Supplier = Factory.NewWithValidTestData<OrgHeader>().PK;

				declaration.BuyerDocAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
				declaration.NotifyPartyDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;

				CombineAssertions(() =>
				{
					Assert("Pre-condition: Importer Documentary Address is not empty.", !declaration.ImporterDocumentaryAddress.IsEmpty);
					Assert("Pre-condition: Supplier Documentary Address is not empty.", !declaration.SupplierDocumentaryAddress.IsEmpty);

					Assert("Pre-condition: Buyer Doc Address is not empty.", !declaration.BuyerDocAddress.IsEmpty);
					Assert("Pre-condition: Notify Party Documentary Address is not empty.", !declaration.NotifyPartyDocumentaryAddress.IsEmpty);
				});

				Factory.Save();

				declaration.JE_DeclarationReference = shipment.JS_UniqueConsignRef;
				Factory.Save();

				void AssertClonedDeclaration(BaseJobDeclaration clonedDeclaration)
				{
					CombineAssertions(() =>
					{
						AssertEquals("JE_MessageType should be 'IMP'.", JobMessageTypeList.Codes.Import, clonedDeclaration.JE_MessageType);

						Assert("Cloned Importer Documentary Address is not empty.", !clonedDeclaration.ImporterDocumentaryAddress.IsEmpty);
						Assert("Cloned Supplier Documentary Address is not empty.", !clonedDeclaration.SupplierDocumentaryAddress.IsEmpty);

						AssertEquals("Cloned Importer Documentary Address should copy the original address key.", declaration.ImporterDocumentaryAddress.E2_OA_Address, clonedDeclaration.ImporterDocumentaryAddress.E2_OA_Address);
						AssertEquals("Cloned Supplier Documentary Address should copy the original address key.", declaration.SupplierDocumentaryAddress.E2_OA_Address, clonedDeclaration.SupplierDocumentaryAddress.E2_OA_Address);

						AssertNotEquals("Cloned Importer Documentary Address is a new JobDocAddress.", declaration.ImporterDocumentaryAddress.PK, clonedDeclaration.ImporterDocumentaryAddress.PK);
						AssertNotEquals("Cloned Supplier Documentary Address is a new JobDocAddress.", declaration.SupplierDocumentaryAddress.PK, clonedDeclaration.SupplierDocumentaryAddress.PK);

						Assert("Buyer Doc Address is not cloned.", clonedDeclaration.BuyerDocAddress.IsEmpty);
						Assert("Notify Party Documentary Address is not cloned.", clonedDeclaration.NotifyPartyDocumentaryAddress.IsEmpty);
					});
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(fromCountry))
				{
					var newFactory = NewFactory();

					var decImporter = new ImportJobDeclaration(newFactory);
					decImporter.DeclarationPK = declaration.PK;

					var clonedDeclaration = decImporter.CreateNewStandAloneDeclaration(newFactory);
					AssertClonedDeclaration(clonedDeclaration);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(toCountry))
				{
					var newFactory = NewFactory();

					var decImporter = new ImportJobDeclaration(newFactory);
					decImporter.DeclarationPK = declaration.PK;

					var clonedDeclaration = decImporter.CreateDeclarationAgainstShipment();
					AssertClonedDeclaration(clonedDeclaration);
				}
			}
		}

		public void TestImportDeclarationFromCountryToCountry_JE_PaidByConstraintError()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Malaysia);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00001111";
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();

			AssertEquals(DeclarationApplicationCodeList.Codes.Interfaced, declaration.JE_ApplicationCode);
			AssertEquals("BRK", declaration.JE_PaidBy);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var factory2 = new BusinessObjectFactory();
				var decImporter = new ImportJobDeclarationForTest(factory2);
				decImporter.DeclarationPK = declaration.PK;

				var newJobDeclaration = decImporter.CreateNewStandAloneDeclaration(factory2);
				factory2.Save();
				AssertEquals("SGConstants.TradeNetVersion.FourPointOne", "4.1", newJobDeclaration.JE_ApplicationCode);
				AssertEquals("", newJobDeclaration.JE_PaidBy);
			}
		}

		public void TestEndToEndForImportingFromDeclarationInDifferentCountry()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			otherCompany.GC_Code = "XYZ";
			var newBranch = otherCompany.Branches.AddNew();
			newBranch.GB_RL_NKHomePort = "AUSYD";
			newBranch.GB_Code = "QAZ";
			Factory.Save();

			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_IsConsignee = true;
			importer.OH_RL_NKClosestPort = "NZAKL";

			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			supplier.OH_IsConsignor = true;
			supplier.OH_RL_NKClosestPort = "AUSYD";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var jobContainer = consol.Containers.AddNew();
			jobContainer.JC_ContainerNum = "CRUX1234562";

			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = importer.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;
			shipment.JS_OuterPacks = 1;

			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_JS = shipment.PK;
			declaration.JE_DeclarationReference = shipment.JS_UniqueConsignRef;
			declaration.JE_GB = newBranch.PK;

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRUX1234562";

			declaration.JE_MasterBill = "OBL123132";
			declaration.JE_HouseBill = "HB12";
			var package = declaration.Packages.AddNew();
			package.CW_ContainerNoOrEquipmentNo = "CRUX1234562";
			package.CW_HouseBill = declaration.PrimaryHouseBill.CU_BillUniqueCode;
			package.CW_PackQty = 1;
			package.CW_PackType = "PCE";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceCurrExRateType = "FIX";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var classification = Factory.New<BaseCusClassification>();
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			classification.CC_Description = "Lamb";
			classification.CC_LookupCode = "TEST";
			classification.CC_TariffNum = "0000.00.00 88";

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Lamb";

			product.ClassificationsForBinding.Add(classification);
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			product.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			AssertEquals("1 more pivot", 1, Factory.Load<BaseCusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK)).Length);
			invoiceLine.JI_PartNo = "Lamb";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var factory2 = new BusinessObjectFactory();
				var nzClassification = factory2.New<BaseCusClassification>();
				nzClassification.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
				nzClassification.CC_Description = "Lamb";
				nzClassification.CC_LookupCode = "TEST";
				nzClassification.CC_TariffNum = "0101.10.00.11E";
				nzClassification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;

				var product2 = factory2.Load<OrgSupplierPart>(product.PK);
				product2.ClassificationsForBinding.Add(nzClassification);
				factory2.Save();

				var factory3 = new BusinessObjectFactory();
				var decImporter = new ImportJobDeclaration(factory3);
				decImporter.LoadDeclarationForShipment(shipment.JS_UniqueConsignRef);
				var result = decImporter.CreateDeclarationAgainstShipment();

				AssertEquals("One package", 1, result.Packages.Count);
				AssertEquals("Pack qty", 1, result.Packages[0].CW_PackQty);
				AssertEquals("Pack Type", "PCE", result.Packages[0].CW_PackType);
				AssertEquals("JI_CC", nzClassification.PK, result.InvoiceLines[0].JI_CC);
				AssertEquals("JI_Tariff", "0101.10.00.11E", result.InvoiceLines[0].JI_Tariff);
				AssertEquals("JI_CL", ZGuid.Empty, result.InvoiceLines[0].JI_CL);
				AssertEquals("Declaration is an import job", JobMessageTypeList.Codes.Import, result.JE_MessageType);
				Assert("JZ_InvoiceCurrExRateType is empty for an import job", result.InvoiceLines[0].InvoiceHeader.JZ_InvoiceCurrExRateType.IsEmpty);
				AssertNoExceptionThrown(delegate
				{ factory3.Save(); });
				AssertEquals(shipment.PK, result.JE_JS);
			}
		}

		public void TestImportingFromUSDeclarationInAU()
		{
			GlbCompany.CurrentCompany.SetCountry("US");

			var otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			otherCompany.GC_Code = "XYZ";
			var newBranch = otherCompany.Branches.AddNew();
			newBranch.GB_RL_NKHomePort = "USPHL";
			newBranch.GB_Code = "QAZ";
			Factory.Save();

			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_IsConsignee = true;
			importer.OH_RL_NKClosestPort = "AUSYD";

			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			supplier.OH_IsConsignor = true;
			supplier.OH_RL_NKClosestPort = "USPHL";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USPHL";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var jobContainer = consol.Containers.AddNew();
			jobContainer.JC_ContainerNum = "CRUX1234562";

			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = importer.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;
			shipment.JS_OuterPacks = 1;

			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_JS = shipment.PK;
			declaration.JE_DeclarationReference = shipment.JS_UniqueConsignRef;
			declaration.JE_GB = newBranch.PK;

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRUX1234562";

			declaration.JE_MasterBill = "OBL123132";
			declaration.JE_HouseBill = "HB12";
			var package = declaration.Packages.AddNew();
			package.CW_ContainerNoOrEquipmentNo = "CRUX1234562";
			package.CW_HouseBill = declaration.PrimaryHouseBill.CU_BillUniqueCode;
			package.CW_PackQty = 1;
			package.CW_PackType = "PCE";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceCurrExRateType = "FIX";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var classification = Factory.New<BaseCusClassification>();
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			classification.CC_Description = "Lamb";
			classification.CC_LookupCode = "TEST";
			classification.CC_TariffNum = "0000.00.00 88";

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Lamb";

			product.ClassificationsForBinding.Add(classification);
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			product.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);

			var productPivots = Factory.Load<BaseCusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			AssertEquals("1 more pivot", 1, productPivots.Length);
			var pivot = productPivots[0];
			pivot.CI_ChildType = ClassificationTypeProvider.GetProviderFor(invoiceLine.CustomsCountryCode).SHBCode;
			invoiceLine.JI_PartNo = "Lamb";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var factory2 = new BusinessObjectFactory();
				var auClassification = factory2.New<BaseCusClassification>();
				auClassification.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
				auClassification.CC_Description = "Lamb";
				auClassification.CC_LookupCode = "TEST";
				auClassification.CC_TariffNum = "0101.10.00 11";
				auClassification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

				var product2 = factory2.Load<OrgSupplierPart>(product.PK);
				product2.ClassificationsForBinding.Add(auClassification);
				factory2.Save();

				var factory3 = new BusinessObjectFactory();
				var decImporter = new ImportJobDeclaration(factory3);
				decImporter.LoadDeclarationForShipment(shipment.JS_UniqueConsignRef);
				var result = decImporter.CreateDeclarationAgainstShipment();

				AssertEquals("One package", 1, result.Packages.Count);
				AssertEquals("Pack qty", 1, result.Packages[0].CW_PackQty);
				AssertEquals("Pack Type", "PCE", result.Packages[0].CW_PackType);
				AssertEquals("JI_CC", auClassification.PK, result.InvoiceLines[0].JI_CC);
				AssertEquals("JI_Tariff", "0101.10.00 11", result.InvoiceLines[0].JI_Tariff);
				AssertEquals("JI_CL", ZGuid.Empty, result.InvoiceLines[0].JI_CL);
				AssertEquals("Declaration is an import job", JobMessageTypeList.Codes.Import, result.JE_MessageType);
				Assert("JZ_InvoiceCurrExRateType is empty for an import job", result.InvoiceLines[0].InvoiceHeader.JZ_InvoiceCurrExRateType.IsEmpty);
				AssertNoExceptionThrown(delegate
				{ factory3.Save(); });
				AssertEquals(shipment.PK, result.JE_JS);
			}
		}

		public void TestClassificationIssuesForMultiCountries()
		{
			BaseJobDeclaration nzDeclaration;

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("NZ"))
			{
				nzDeclaration = Factory.New<BaseJobDeclaration>();
				nzDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				nzDeclaration.JE_GB = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_RL_NKHomePort, SQLComparisonOperator.StartsWith, Core.Constants.CountryCodes.NewZealand)).PK;

				var invoice = nzDeclaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();

				var classification = Factory.New<BaseCusClassification>();
				classification.CC_LookupCode = "TestLookup";
				classification.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
				classification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
				classification.CC_TariffNum = "0101.10.00.11E";
				invoiceLine.JI_CC = classification.PK;
				Factory.Save();
			}

			var factory2 = new BusinessObjectFactory();
			var importer = new ImportJobDeclarationForTest(factory2);
			importer.LoadDeclarationForShipment(nzDeclaration.JE_DeclarationReference);

			AssertNoExceptionThrown(() => importer.CreateNewStandAloneDeclaration(factory2));
			AssertEquals("JI_CC is empty", ZGuid.Empty, importer.ImportedDeclaration.InvoiceLines[0].JI_CL);
		}

		public void TestDeclaration()
		{
			decToImport.DeclarationPK = declaration.PK;
			AssertNotNull("Declaration", decToImport.Declaration);

			decToImport.DeclarationPK = ZGuid.Invalid;
			AssertNull("Declaration", decToImport.Declaration);
		}

		public void TestDeclarationReadOnlyAndClearing()
		{
			string countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry("NZ");

				AssertEquals("Read Only", true, decToImport.DeclarationPKInfo.ReadOnly);

				decToImport.CountryCode = "+1";
				AssertEquals("Read Only", true, decToImport.DeclarationPKInfo.ReadOnly);

				decToImport.CountryCode = "NZ";
				AssertEquals("Read Only", true, decToImport.DeclarationPKInfo.ReadOnly);

				decToImport.CountryCode = "AU";
				AssertEquals("Read Only", false, decToImport.DeclarationPKInfo.ReadOnly);

				decToImport.DeclarationPK = declaration.PK;
				AssertEquals("Dec PK is not empty", false, decToImport.DeclarationPK.IsEmpty);

				decToImport.CountryCode = "+1";
				AssertEquals("Read Only", true, decToImport.DeclarationPKInfo.ReadOnly);
				AssertEquals("Dec PK is empty", true, decToImport.DeclarationPK.IsEmpty);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}
		}

		public void TestValidation()
		{
			AssertNotNull("Validation", decToImport.Validation);
		}

		public void TestLookups()
		{
			AssertNotNull("Lookups", decToImport.Lookups);
		}

		public void TestCreateNewStandAloneDeclaration()
		{
			decToImport.DeclarationPK = declaration.PK;
			decToImport.CountryCode = "NZ";

			var newJobDeclaration = decToImport.CreateNewStandAloneDeclaration(Factory);

			AssertEquals("Message Type is import", "IMP", newJobDeclaration.JE_MessageType);
			AssertEquals("Transport Mode", declaration.JE_TransportMode, newJobDeclaration.JE_TransportMode);
			AssertEquals("Container Mode", declaration.JE_ContainerMode, newJobDeclaration.JE_ContainerMode);
			AssertEquals("Master Bill", declaration.JE_MasterBill, newJobDeclaration.JE_MasterBill);
			AssertEquals("Vessel", declaration.JE_VesselName, newJobDeclaration.JE_VesselName);
			AssertEquals("Voyage No", declaration.JE_VoyageFlightNo, newJobDeclaration.JE_VoyageFlightNo);
			AssertEquals("Port of Loading", declaration.JE_RL_NKPortOfLoading, newJobDeclaration.JE_RL_NKPortOfLoading);
			AssertEquals("Port of Arrival", declaration.JE_RL_NKPortOfArrival, newJobDeclaration.JE_RL_NKPortOfArrival);
			AssertEquals("Port of First Arrival", declaration.JE_RL_NKPortOfFirstArrival, newJobDeclaration.JE_RL_NKPortOfFirstArrival);
			AssertEquals("Export Date", declaration.JE_ExportDate, newJobDeclaration.JE_ExportDate);
			AssertEquals("Date of arrival", declaration.JE_DateOfArrival, newJobDeclaration.JE_DateOfArrival);
			AssertEquals("Date of First Arrival", declaration.JE_DateOfFirstArrival, newJobDeclaration.JE_DateOfFirstArrival);
			AssertEquals("House bill", declaration.JE_HouseBill, newJobDeclaration.JE_HouseBill);
			AssertEquals("Origin", declaration.JE_RL_NKOrigin, newJobDeclaration.JE_RL_NKOrigin);
			AssertEquals("Destination", declaration.JE_RL_NKFinalDestination, newJobDeclaration.JE_RL_NKFinalDestination);
			AssertEquals("Date of Origin", declaration.JE_DateAtOrigin, newJobDeclaration.JE_DateAtOrigin);
			AssertEquals("Date at final destination", declaration.JE_DateAtFinalDestination, newJobDeclaration.JE_DateAtFinalDestination);
			AssertEquals("Description", declaration.JE_GoodsDescription, newJobDeclaration.JE_GoodsDescription);
			AssertEquals("Owner Ref", declaration.JE_OwnerRef, newJobDeclaration.JE_OwnerRef);
			AssertEquals("Override shipment values", false, newJobDeclaration.JE_OverrideFreightDefaults);

			AssertEquals("House bill count", 4, newJobDeclaration.Bills.Count);
			AssertEquals("Container Count", 1, newJobDeclaration.CusContainers.Count);
			AssertEquals("Container Number", declaration.CusContainers[0].CO_ContainerNumber, newJobDeclaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals("Container Seal", declaration.CusContainers[0].CO_Seal, newJobDeclaration.CusContainers[0].CO_Seal);
			AssertEquals("Container Type", declaration.CusContainers[0].CO_RC, newJobDeclaration.CusContainers[0].CO_RC);
			AssertEquals("Container Mode", declaration.CusContainers[0].CO_FCL_LCL_AIR, newJobDeclaration.CusContainers[0].CO_FCL_LCL_AIR);

			AssertEquals("Main InvoiceGroupHeader", 1, newJobDeclaration.JobComInvoiceGroupHeaders.Count);
			AssertEquals("Sub InvoiceGroupHeader", 1, newJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.Count);
			AssertEquals("Sub InvoiceGroupHeader invoiceheader count", 1, newJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);

			AssertEquals("Invoice Header Count", 1, newJobDeclaration.Invoices.Count);
			AssertEquals("Invoice Number", declaration.Invoices[0].JZ_InvoiceNumber, newJobDeclaration.Invoices[0].JZ_InvoiceNumber);
			AssertEquals("Invoice Amount", declaration.Invoices[0].JZ_InvoiceAmount, newJobDeclaration.Invoices[0].JZ_InvoiceAmount);
			AssertEquals("Invoice Currency", declaration.Invoices[0].JZ_RX_NKInvoice_Currency, newJobDeclaration.Invoices[0].JZ_RX_NKInvoice_Currency);

			AssertEquals("Invoice Line Count", 1, declaration.Invoices[0].JobComInvoiceLines.Count);
			AssertEquals("Invoice Tariff", declaration.InvoiceLines[0].JI_Tariff, declaration.Invoices[0].JobComInvoiceLines[0].JI_Tariff);
			AssertEquals("Invoice Description", declaration.InvoiceLines[0].JI_Description, declaration.Invoices[0].JobComInvoiceLines[0].JI_Description);

			AssertEquals("No Entry Headers", 0, newJobDeclaration.CustomsEntryHeaders.Count);
		}

		public void TestCheckDeclarationValuesAfterImport()
		{
			declaration.JE_EntryStatus = "STS";
			declaration.JE_MessageStatus = "MSM";
			declaration.JE_MessageSubType = "MMM";
			declaration.JE_PaymentMethod = "PPP";
			declaration.JE_AddInfo = "Add Info";
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();

			decToImport.DeclarationPK = declaration.PK;
			decToImport.CountryCode = "NZ";

			var newJobDeclaration = decToImport.CreateNewStandAloneDeclaration(Factory);

			AssertEquals("Entry Status", ZString.Empty, newJobDeclaration.JE_EntryStatus);
			AssertEquals("Message Status", ZString.Empty, newJobDeclaration.JE_MessageStatus);
			AssertEquals("Message Sub Type", ZString.Empty, newJobDeclaration.JE_MessageSubType);
			AssertEquals("Declaration Ref", ZString.Empty, newJobDeclaration.JE_DeclarationReference);
			AssertEquals("Payment Method", ZString.Empty, newJobDeclaration.JE_PaymentMethod);
			AssertEquals("Add Info", ZString.Empty, newJobDeclaration.JE_AddInfo);
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, newJobDeclaration.JE_GB);
		}

		public void TestCheckGroupHeaderValuesAfterImport()
		{
			var charge1 = groupHeader.Charges.AddNew();
			charge1.J7_Amount = 56.56M;
			var charge2 = groupHeader.Charges.AddNew();
			charge2.J7_Amount = 564.45M;

			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);

			Factory.Save();

			decToImport.DeclarationPK = declaration.PK;
			decToImport.CountryCode = "NZ";

			var newJobDeclaration = decToImport.CreateNewStandAloneDeclaration(Factory);
			AssertEquals("One main Group Headers", 1, newJobDeclaration.JobComInvoiceGroupHeaders.Count);
			AssertEquals("One sub Group Headers", 1, newJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.Count);
			AssertEquals("AddInfo is empty", ZString.Empty, newJobDeclaration.JobComInvoiceGroupHeaders[0].JZ_AddInfo);
			AssertEquals("AddInfo is empty", ZString.Empty, newJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JZ_AddInfo);
			AssertEquals("Group header has charges", 2, newJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].Charges.Count);
		}

		public void TestMultiGroupHeadersWithInvoiceHeadersAndLines()
		{
			//Start with main group header with a sub group header and invoice header.
			var mainGroupHeader = declaration.JobComInvoiceGroupHeaders[0];

			//Added invoice header to main group header.
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_JZ_GroupInvoiceFK = mainGroupHeader.PK;

			//Add sub group header without invoice header
			var newSubMainGroupHeader = mainGroupHeader.JobComInvoiceGroupHeaders.AddNew();

			//Add a sub group to NewSubMainGroupHeader with an invoice.
			var newSubSubMainGroupHeader1 = newSubMainGroupHeader.JobComInvoiceGroupHeaders.AddNew();
			var newInvoiceHeader = newSubSubMainGroupHeader1.JobComInvoiceHeaders.AddNew();
			var newInvoiceLine = newInvoiceHeader.JobComInvoiceLines.AddNew();

			//Add another sub group to NewSubMainGroupHeader with invoice header.
			var newSubSubMainGroupHeader2 = newSubMainGroupHeader.JobComInvoiceGroupHeaders.AddNew();
			var newInvoiceHeader2 = newSubSubMainGroupHeader2.JobComInvoiceHeaders.AddNew();
			var newInvoiceLine2 = newInvoiceHeader2.JobComInvoiceLines.AddNew();

			//Add another sub group to NewSubMainGroupHeader without invoice header.
			var newSubSubMainGroupHeader3 = newSubMainGroupHeader.JobComInvoiceGroupHeaders.AddNew();

			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);

			Factory.Save();

			decToImport.DeclarationPK = declaration.PK;
			decToImport.CountryCode = "NZ";

			var newJobDeclaration = decToImport.CreateNewStandAloneDeclaration(Factory);
			AssertEquals("One main Group Header", 1, newJobDeclaration.JobComInvoiceGroupHeaders.Count);
			AssertEquals("One invoice header for main group", 1, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);
			AssertEquals("Two sub Group Headers", 2, newJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.Count);

			AssertEquals("Sub group 1 has invoice Header", 1, newJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);

			AssertEquals("Sub group 2 has no invoice headers", 0, newJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[1].JobComInvoiceHeaders.Count);
			AssertEquals("Sub group 2 has three more sub groups", 3, newJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[1].JobComInvoiceGroupHeaders.Count);
			AssertEquals("Sub group 1 of sub group 2 has one invoice header", 1, newJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[1].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);
			AssertEquals("Sub group 2 of sub group 2 has one invoice header", 1, newJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[1].JobComInvoiceGroupHeaders[1].JobComInvoiceHeaders.Count);
			AssertEquals("Sub group 2 of sub group 3 has one invoice header", 0, newJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[1].JobComInvoiceGroupHeaders[2].JobComInvoiceHeaders.Count);
		}

		public void TestCheckInvoiceHeaderValuesAfterImport()
		{
			invoiceHeader.JZ_RX_NKInvoice_Currency = invoiceHeader.JobDeclaration.LocalCurrencyCode;
			invoiceHeader.JZ_AddInfo = "RelationshipIndicator=Y";
			invoiceHeader.JZ_InvoiceCurrLandedCostExRate = 134.56M;
			invoiceHeader.JZ_PaymentExRate = 987.43M;

			var charge1 = invoiceHeader.Charges.AddNew();
			charge1.J7_Amount = 45.68M;
			var charge2 = invoiceHeader.Charges.AddNew();
			charge2.J7_Amount = 156.26M;

			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();

			AssertEquals("Precondition: Add Info", "RelationshipIndicator=Y", invoiceHeader.JZ_AddInfo);
			AssertEquals("Precondition: Invoice Curr Ex Rate", 1M, invoiceHeader.JZ_InvoiceCurrExRate);
			AssertEquals("Precondition: Landed cost ex rate", 134.56M, invoiceHeader.JZ_InvoiceCurrLandedCostExRate);
			AssertEquals("Precondition: Payment Ex Rate", 987.43M, invoiceHeader.JZ_PaymentExRate);
			AssertEquals("Precondition: Charges", 2, invoiceHeader.Charges.Count);

			decToImport.DeclarationPK = declaration.PK;
			decToImport.CountryCode = "NZ";

			var newJobDeclaration = decToImport.CreateNewStandAloneDeclaration(Factory);
			var newInvoiceHeader = newJobDeclaration.Invoices[0];

			AssertEquals("Add Info", ZString.Empty, newInvoiceHeader.JZ_AddInfo);
			AssertEquals("Group Header FK not empty", true, newInvoiceHeader.JZ_JZ_GroupInvoiceFK.IsValid);
			AssertEquals("House Bill", "HOUSE BILL 2", ((Bill)newJobDeclaration.Bills.FindByPK(newInvoiceHeader.JZ_CU_RelatedHouseBill)).CU_HouseBill);
			AssertEquals("Charges", 2, newInvoiceHeader.Charges.Count);
		}

		public void TestCheckInvoiceLineValuesAfterImport()
		{
			invoiceLine.JI_AddInfo = "Add Info";
			invoiceLine.JI_CC = Factory.New<BaseCusClassification>().PK;
			invoiceLine.JI_CustomsUnitQty = "KG";

			var charge1 = invoiceLine.Charges.AddNew();
			charge1.J7_Amount = 123.45M;
			var charge2 = invoiceLine.Charges.AddNew();
			charge2.J7_Amount = 562.32M;

			AssertEquals("Precondition: Add Info", "Add Info", invoiceLine.JI_AddInfo);
			AssertEquals("Precondition: Classification", true, invoiceLine.JI_CC.IsValid);
			AssertEquals("Precondition: Customs Unit Qty", "KG", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("Precondition: Charges", 2, invoiceLine.Charges.Count);

			decToImport.DeclarationPK = declaration.PK;
			decToImport.CountryCode = "NZ";

			var newJobDeclaration = decToImport.CreateNewStandAloneDeclaration(Factory);
			var newInvoiceLine = newJobDeclaration.InvoiceLines[0];

			AssertEquals("Add Info", ZString.Empty, newInvoiceLine.JI_AddInfo);
			AssertEquals("Classification", true, newInvoiceLine.JI_CC.IsEmpty);
			AssertEquals("Customs Unit Qty", "", newInvoiceLine.JI_CustomsUnitQty);
			AssertEquals("Charges", 2, newInvoiceLine.Charges.Count);
		}

		public void TestCreateDeclarationAgainstShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "Test";
			declaration.JE_JS = shipment.PK;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();

			string currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry("AU");

				decToImport.LoadDeclarationForShipment(shipment.JS_UniqueConsignRef);
				var newJobDeclaration = decToImport.CreateDeclarationAgainstShipment();
				AssertEquals("Message Type is import", "IMP", newJobDeclaration.JE_MessageType);
				AssertEquals("Transport Mode", declaration.JE_TransportMode, newJobDeclaration.JE_TransportMode);
				//					AssertEquals("Container Mode", Declaration.JE_ContainerMode, NewJobDeclaration.JE_ContainerMode);
				AssertEquals("Master Bill", declaration.JE_MasterBill, newJobDeclaration.JE_MasterBill);
				AssertEquals("Vessel", declaration.JE_VesselName, newJobDeclaration.JE_VesselName);
				AssertEquals("Voyage No", declaration.JE_VoyageFlightNo, newJobDeclaration.JE_VoyageFlightNo);
				AssertEquals("Port of Loading", declaration.JE_RL_NKPortOfLoading, newJobDeclaration.JE_RL_NKPortOfLoading);
				AssertEquals("Port of Arrival", declaration.JE_RL_NKPortOfArrival, newJobDeclaration.JE_RL_NKPortOfArrival);
				AssertEquals("Port of First Arrival", declaration.JE_RL_NKPortOfFirstArrival, newJobDeclaration.JE_RL_NKPortOfFirstArrival);
				AssertEquals("Export Date", declaration.JE_ExportDate, newJobDeclaration.JE_ExportDate);
				AssertEquals("Date of arrival", declaration.JE_DateOfArrival, newJobDeclaration.JE_DateOfArrival);
				AssertEquals("Date of First Arrival", declaration.JE_DateOfFirstArrival, newJobDeclaration.JE_DateOfFirstArrival);
				AssertEquals("House bill", declaration.JE_HouseBill, newJobDeclaration.JE_HouseBill);
				AssertEquals("Origin", declaration.JE_RL_NKOrigin, newJobDeclaration.JE_RL_NKOrigin);
				AssertEquals("Destination", declaration.JE_RL_NKFinalDestination, newJobDeclaration.JE_RL_NKFinalDestination);
				AssertEquals("Date of Origin", declaration.JE_DateAtOrigin, newJobDeclaration.JE_DateAtOrigin);
				AssertEquals("Date at final destination", declaration.JE_DateAtFinalDestination, newJobDeclaration.JE_DateAtFinalDestination);
				AssertEquals("Description", declaration.JE_GoodsDescription, newJobDeclaration.JE_GoodsDescription);
				AssertEquals("Marks and numbers", declaration.JE_MarksAndNumbers, newJobDeclaration.JE_MarksAndNumbers);
				AssertEquals("Owner Ref", declaration.JE_OwnerRef, newJobDeclaration.JE_OwnerRef);
				AssertEquals("Override shipment values", true, newJobDeclaration.JE_OverrideFreightDefaults);

				AssertEquals("House bill count", declaration.Bills.Count, newJobDeclaration.Bills.Count);

				AssertEquals("Container Count", 1, newJobDeclaration.CusContainers.Count);
				AssertEquals("Container Number", declaration.CusContainers[0].CO_ContainerNumber, newJobDeclaration.CusContainers[0].CO_ContainerNumber);
				AssertEquals("Container Seal", declaration.CusContainers[0].CO_Seal, newJobDeclaration.CusContainers[0].CO_Seal);
				AssertEquals("Container Type", declaration.CusContainers[0].CO_RC, newJobDeclaration.CusContainers[0].CO_RC);
				AssertEquals("Container Mode", declaration.CusContainers[0].CO_FCL_LCL_AIR, newJobDeclaration.CusContainers[0].CO_FCL_LCL_AIR);

				AssertEquals("Main InvoiceGroupHeader", 1, newJobDeclaration.JobComInvoiceGroupHeaders.Count);
				AssertEquals("Sub InvoiceGroupHeader", 1, newJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.Count);
				AssertEquals("Sub InvoiceGroupHeader invoiceheader count", 1, newJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);

				AssertEquals("Invoice Header Count", 1, newJobDeclaration.Invoices.Count);
				AssertEquals("Invoice Number", declaration.Invoices[0].JZ_InvoiceNumber, newJobDeclaration.Invoices[0].JZ_InvoiceNumber);
				AssertEquals("Invoice Amount", declaration.Invoices[0].JZ_InvoiceAmount, newJobDeclaration.Invoices[0].JZ_InvoiceAmount);
				AssertEquals("Invoice Currency", declaration.Invoices[0].JZ_RX_NKInvoice_Currency, newJobDeclaration.Invoices[0].JZ_RX_NKInvoice_Currency);

				AssertEquals("Invoice Line Count", 1, newJobDeclaration.Invoices[0].JobComInvoiceLines.Count);
				AssertEquals("Invoice Tariff", ZString.Empty, newJobDeclaration.Invoices[0].JobComInvoiceLines[0].JI_Tariff);
				AssertEquals("Invoice Description", declaration.InvoiceLines[0].JI_Description, newJobDeclaration.Invoices[0].JobComInvoiceLines[0].JI_Description);

				AssertEquals("No Entry Headers", 0, newJobDeclaration.CustomsEntryHeaders.Count);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountry);
			}
		}

		public void TestCreateDeclarationForMultiDecsAgainstShipmentWithDestination()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "Test";

			declaration.JE_JS = shipment.PK;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();

			var zACompany = Factory.New<GlbCompany>();
			zACompany.GC_Code = "ZAC";
			var zABranch = zACompany.Branches.AddNew();
			zABranch.GB_Code = "ZAB";
			zABranch.GB_RL_NKHomePort = "ZAAAM";

			var zADeclaration = BaseJobDeclaration.New(Factory);
			zADeclaration.JE_MessageType = "EXP";
			zADeclaration.JE_GB = zABranch.PK;
			zADeclaration.JE_RL_NKFinalDestination = "AUSYD";
			zADeclaration.JE_RL_NKOrigin = "ZAAAM";
			zADeclaration.JE_JS = shipment.PK;
			var count = zADeclaration.JobComInvoiceGroupHeaders.Count;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(zADeclaration.MergeManager);

			Factory.Save();

			string currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry("AU");

				decToImport.LoadDeclarationForShipment(shipment.JS_UniqueConsignRef);
				var newJobDeclaration = decToImport.CreateDeclarationAgainstShipment();
				AssertNotNull("DeclarationToLoad", decToImport.Declaration);
				AssertEquals("ZA Branch as it has AU destination", zABranch.PK, decToImport.Declaration.JE_GB);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountry);
			}
		}

		public void TestCreateDeclarationForMultiDecsAgainstShipmentWithOrigin()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "Test";
			declaration.JE_JS = shipment.PK;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);

			Factory.Save();

			var zACompany = Factory.New<GlbCompany>();
			zACompany.GC_Code = "ZAC";
			var zABranch = zACompany.Branches.AddNew();
			zABranch.GB_Code = "ZAB";
			zABranch.GB_RL_NKHomePort = "ZAAAM";

			var zADeclaration = BaseJobDeclaration.New(Factory);
			zADeclaration.JE_MessageType = "EXP";
			zADeclaration.JE_GB = zABranch.PK;
			zADeclaration.JE_RL_NKFinalDestination = "ZAAAM";
			zADeclaration.JE_RL_NKOrigin = "AUSYD";
			zADeclaration.JE_JS = shipment.PK;
			var count = zADeclaration.JobComInvoiceGroupHeaders.Count;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(zADeclaration.MergeManager);

			Factory.Save();

			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry("AU");

				decToImport.LoadDeclarationForShipment(shipment.JS_UniqueConsignRef);
				var newJobDeclaration = decToImport.CreateDeclarationAgainstShipment();
				AssertNotNull("DeclarationToLoad", decToImport.Declaration);
				AssertEquals("ZA Branch as it has NZ origin", zABranch.PK, decToImport.Declaration.JE_GB);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountry);
			}
		}

		public void TestSetShipmentTypeBaseOnCountryContext()
		{
			var cnCompany = Factory.New<GlbCompany>();
			cnCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			cnCompany.GC_Code = "CNC";
			var cnBranch = cnCompany.Branches.AddNew();
			cnBranch.GB_RL_NKHomePort = "CNSHA";
			cnBranch.GB_Code = "CNB";

			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			usCompany.GC_Code = "USC";
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_RL_NKHomePort = "USPHL";
			usBranch.GB_Code = "USB";

			Factory.Save();

			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_IsConsignee = true;
			importer.OH_RL_NKClosestPort = "USPHL";

			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			supplier.OH_IsConsignor = true;
			supplier.OH_RL_NKClosestPort = "CNSHA";

			Factory.Save();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration1.JE_OverrideFreightDefaults = true;
			declaration1.JE_JS = shipment1.PK;
			declaration1.JE_OH_Importer = importer.PK;
			declaration1.JE_OH_Supplier = supplier.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration2.JE_OverrideFreightDefaults = true;
			declaration2.JE_JS = shipment2.PK;
			declaration2.JE_OH_Supplier = supplier.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var declaration3 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration3.JE_OverrideFreightDefaults = true;
			declaration3.JE_JS = shipment3.PK;

			Factory.Save();

			using (DisposableEnvironment.ForBranch(usBranch.PK.ToGuid()))
			{
				var newFactory = NewFactory();

				var decImporter = new ImportJobDeclaration(newFactory);
				decImporter.DeclarationPK = declaration1.PK;
				AssertEquals("JE_MessageType set to IMP when Importer is in local", JobMessageTypeList.Codes.Import, decImporter.CreateDeclarationAgainstShipment().JE_MessageType);

				decImporter = new ImportJobDeclaration(newFactory);
				decImporter.DeclarationPK = declaration2.PK;
				AssertEquals("JE_MessageType set to IMP when Supplier is not in local", JobMessageTypeList.Codes.Import, decImporter.CreateDeclarationAgainstShipment().JE_MessageType);

				decImporter = new ImportJobDeclaration(newFactory);
				decImporter.DeclarationPK = declaration3.PK;
				AssertEquals("JE_MessageType set to IMP when Importer & Supplier is null", JobMessageTypeList.Codes.Import, decImporter.CreateDeclarationAgainstShipment().JE_MessageType);
			}

			using (DisposableEnvironment.ForBranch(cnBranch.PK.ToGuid()))
			{
				var newFactory = NewFactory();

				var decImporter = new ImportJobDeclaration(newFactory);
				decImporter.DeclarationPK = declaration1.PK;
				AssertEquals("JE_MessageType set to EXP when Importer is not in local", JobMessageTypeList.Codes.Export, decImporter.CreateDeclarationAgainstShipment().JE_MessageType);

				decImporter = new ImportJobDeclaration(newFactory);
				decImporter.DeclarationPK = declaration2.PK;
				AssertEquals("JE_MessageType set to IMP when Supplier is in local", JobMessageTypeList.Codes.Export, decImporter.CreateDeclarationAgainstShipment().JE_MessageType);

				decImporter = new ImportJobDeclaration(newFactory);
				decImporter.DeclarationPK = declaration3.PK;
				AssertEquals("JE_MessageType set to IMP when Importer & Supplier is null", JobMessageTypeList.Codes.Import, decImporter.CreateDeclarationAgainstShipment().JE_MessageType);
			}
		}

		#region Implementation

		BaseJobDeclaration declaration;
		BaseJobComInvoiceHeader invoiceHeader;
		BaseJobComInvoiceLine invoiceLine;
		BaseJobComInvoiceGroupHeader groupHeader;
		ImportJobDeclarationForTest decToImport;

		class ImportJobDeclarationForTest : ImportJobDeclaration
		{
			public ImportJobDeclarationForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public new BaseJobDeclaration Declaration => base.Declaration;

			public new BaseJobDeclaration ImportedDeclaration
			{
				get => base.ImportedDeclaration;
				set => base.ImportedDeclaration = value;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetUpJobDeclaration();
			decToImport = new ImportJobDeclarationForTest(Factory);
		}

		protected void SetUpJobDeclaration()
		{
			var nZCompany = Factory.New<GlbCompany>();
			nZCompany.GC_RN_NKCountryCode = "NZ";
			var nZBranch = nZCompany.Branches.AddNew();
			nZBranch.GB_RL_NKHomePort = "NZAKL";

			declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.NZ.IJobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = "CNT";
			declaration.JE_MasterBill = "MasterBill";
			declaration.JE_VesselName = "ADMIRALENGRACHT";
			declaration.JE_VoyageFlightNo = "1234";
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_RL_NKPortOfArrival = "ZAAAM";
			declaration.JE_RL_NKPortOfFirstArrival = "ZAAAM";
			declaration.JE_ExportDate = new ZDateTime(2006, 02, 06);
			declaration.JE_DateOfArrival = new ZDateTime(2006, 02, 06);
			declaration.JE_DateOfFirstArrival = new ZDateTime(2006, 02, 06);
			declaration.JE_HouseBill = "House Bill";
			declaration.JE_RL_NKOrigin = "NZAKL";
			declaration.JE_RL_NKFinalDestination = "ZAAAM";
			declaration.JE_DateAtOrigin = new ZDateTime(2006, 02, 06);
			declaration.JE_DateAtFinalDestination = new ZDateTime(2006, 02, 06);
			declaration.JE_GoodsDescription = "Description";
			declaration.JE_MarksAndNumbers = "Marks and Nums";
			declaration.JE_OwnerRef = "Owner Ref";
			declaration.JE_GB = nZBranch.PK;

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "ConNum";
			container.CO_Seal = "Seal";
			container.CO_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			container.CO_FCL_LCL_AIR = "FCL";

			var bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = BillTypeList.Codes.MasterBill;
			bill3.CU_MasterBill = "Master Bill";

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill2.CU_HouseBill = "House Bill 2";
			bill2.CU_MasterBill = "Master Bill";

			var topGroup = declaration.JobComInvoiceGroupHeaders[0];
			groupHeader = topGroup.JobComInvoiceGroupHeaders.AddNew();
			groupHeader.JZ_InvoiceNumber = "Test";
			groupHeader.JZ_AddInfo = "Add Info";

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "Num";
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_InvoiceAmount = 1500.00M;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";
			invoiceHeader.JZ_CU_RelatedHouseBill = bill2.PK;
			invoiceHeader.JZ_JZ_GroupInvoiceFK = groupHeader.PK;

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "4001.10.00";
			invoiceLine.JI_Description = "Description";

			var formalEntryHeader = declaration.CustomsEntryHeaders.First();
			var entryLine = formalEntryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);

			Factory.Save();
		}

		#endregion

	}
}
