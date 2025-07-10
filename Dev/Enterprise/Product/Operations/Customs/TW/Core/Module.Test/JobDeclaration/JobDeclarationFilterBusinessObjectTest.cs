using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Module;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.TW.Module.JobDeclarationFilterBusinessObject;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	sealed class JobDeclarationFilterBusinessObjectTest : Customs.Module.Testing.JobDeclarationFilterBusinessObjectTest
	{
		public void TestLookups()
		{
			var filterBizObj = new JobDeclarationFilterBusinessObject();
			AssertEquals("Lookups of correct type", typeof(JobDeclarationFilterLookups), filterBizObj.Lookups.GetType());
		}

		public override void TestSupplierSearch()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.JZ_JE = declaration.PK;
			var noMatchDec = Factory.New<JobDeclaration>();
			var noMatchInvoice = noMatchDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			noMatchInvoice.JZ_JE = noMatchDec.PK;
			var decSupplier = Factory.New<OrgHeader>();
			decSupplier.MainAddress.OA_Address1 = "Dec Supplier Address 1";
			decSupplier.OH_Code = "DECSUP";
			declaration.JE_OH_Supplier = decSupplier.PK;
			Factory.Save();
			var filter = (ModuleGuidsFilter)filterBO[Customs.Module.DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier];
			filter.IsActive = true;
			filter.Property2 = decSupplier.PK;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(filterBO.Filter);
			AssertEquals("Matched supplier on Declaration", 1, collection.Count);
			AssertEquals("Matched supplier on Declaration", decSupplier.PK, collection[0].JE_OH_Supplier);
			AssertEquals("Matched supplier on Invoice Header", decSupplier.PK, collection[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_OH_Supplier);
			filter.Property2 = ZGuid.NewZGuid();
			collection.Load(filterBO.Filter);
			AssertEquals("Matched supplier on Declaration", 0, collection.Count);
		}

		public void TestConsigneeSearch()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var consignee = Factory.New<OrgHeader>();
			consignee.MainAddress.OA_Address1 = "Dec Supplier Address 1";
			consignee.OH_Code = "DECSUP";
			declaration.JE_OH_Consignee = consignee.PK;
			Factory.Save();
			var filter = (ModuleGuidFilter)filterBO[FilterTypes.Consignee];
			filter.IsActive = true;
			filter.Property = consignee.PK;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(filterBO.Filter);
			AssertEquals("Matched supplier on Declaration", 1, collection.Count);
			AssertEquals("Matched supplier on Declaration", consignee.PK, collection[0].JE_OH_Consignee);
			filter.Property = ZGuid.NewZGuid();
			collection.Load(filterBO.Filter);
			AssertEquals("Matched supplier on Declaration", 0, collection.Count);
		}

		public void TestConsignorSearch()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var consignor = Factory.New<OrgHeader>();
			consignor.MainAddress.OA_Address1 = "Dec Supplier Address 1";
			consignor.OH_Code = "DECSUP";
			declaration.JE_OH_Exporter = consignor.PK;
			Factory.Save();
			var filter = (ModuleGuidFilter)filterBO[FilterTypes.Consignor];
			filter.IsActive = true;
			filter.Property = consignor.PK;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(filterBO.Filter);
			AssertEquals("Matched supplier on Declaration", 1, collection.Count);
			AssertEquals("Matched supplier on Declaration", consignor.PK, collection[0].JE_OH_Exporter);
			filter.Property = ZGuid.NewZGuid();
			collection.Load(filterBO.Filter);
			AssertEquals("Matched supplier on Declaration", 0, collection.Count);
		}

		public void TestMailBoxSearch()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsProfile = "111";
			Factory.Save();
			var filter = (ModuleNumberFilter)filterBO[FilterTypes.MailBox];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			filter.Property = "11";
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(filterBO.Filter);
			AssertEquals("Matched supplier on Declaration", 1, collection.Count);
			AssertEquals("Matched supplier on Declaration", "111", collection[0].JE_CustomsProfile);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "11";
			collection.Load(filterBO.Filter);
			AssertEquals("Matched supplier on Declaration", 0, collection.Count);
			filter.Property = "111";
			collection.Load(filterBO.Filter);
			AssertEquals("Matched supplier on Declaration", 1, collection.Count);
			AssertEquals("Matched supplier on Declaration", "111", collection[0].JE_CustomsProfile);
			filter.Property = "222";
			collection.Load(filterBO.Filter);
			AssertEquals("Matched supplier on Declaration", 0, collection.Count);
		}

		public void TestDeclarationTypeSearch()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsProfile = "111";
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			Factory.Save();
			var filter = (ModuleTextFilter)filterBO[FilterTypes.DeclarationType];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Constants.DeclarationTypes.Import.G1;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(filterBO.Filter);
			AssertEquals("Should have found 1 declaration", 1, collection.Count);
			filter.Property = Constants.DeclarationTypes.Import.G2;
			collection.Load(filterBO.Filter);
			AssertEquals("Should have found 0 declaration", 0, collection.Count);
		}

		public void TestDeclarationDateSearch()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsProfile = "111";
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 1, 21);
			Factory.Save();
			var filter = (ModuleDateFilter)filterBO[FilterTypes.DeclarationDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2020, 1, 21);
			filter.Property2 = new ZDateTime(2020, 1, 21);
			filter.IsActive = true;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(filterBO.Filter);
			AssertEquals("Should have found 1 declaration", 1, collection.Count);
			filter.Property1 = new ZDateTime(2020, 1, 22);
			filter.Property2 = new ZDateTime(2020, 1, 25);
			collection.Load(filterBO.Filter);
			AssertEquals("Should have found 0 declaration", 0, collection.Count);
		}

		public void TestMessageStatusSearch()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var filter = (ModuleTextFilter)filterBO[Customs.Module.DeclarationFilterConstants.MessageStatusText];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			Factory.Save();
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(filterBO.Filter);
			AssertEquals("Should have found 1 declaration", 1, collection.Count);
			AssertEquals(declaration.PK, collection[0].PK);
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_Status = ZString.Empty;
			Factory.Save();
			collection.Load(filterBO.Filter);
			AssertEquals("Should have found 1 declaration", 1, collection.Count);
			AssertEquals(declaration.PK, collection[0].PK);
			entryHeader.CH_Status = JobDeclarationMessageStatusList.Codes.AWG;
			Factory.Save();
			filter.Property = JobDeclarationMessageStatusList.Codes.AWG;
			collection.Load(filterBO.Filter);
			AssertEquals("Should have found 1 declaration", 1, collection.Count);
			AssertEquals(declaration.PK, collection[0].PK);
			filter.Property = JobDeclarationMessageStatusList.Codes.AWC;
			collection.Load(filterBO.Filter);
			AssertEquals("Should have found 0 declaration", 0, collection.Count);
		}

		public override void TestCombinedMessageStatus()
		{
			var declaration0 = Factory.New<JobDeclaration>();
			declaration0.JE_MessageStatus = "AWO";
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageStatus = "AWO";
			var entryHeader0 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader0.CH_Status = "AWC";
			Factory.Save();
			var filter = (ModuleTextFilter)filterBO[filterBO.MessageStatusText];
			filter.IsActive = true;
			filter.Property = "AWC";
			var filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(declaration1.PK, filteredDecs[0].PK);
			filter.Property = "AWO";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(0, filteredDecs.Length);
		}

		public void TestClearanceStatusSearch()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1.EntryNumber = "CABF0945600030";
			entryHeader1.CusEntryNumber.CE_EntryStatus = ClearanceStatusCodeList.Codes.C1;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.CustomsEntryHeaders.AddNew();
			declaration2.EntryNumber = "CABF0945600031";

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.CustomsEntryHeaders.AddNew();

			var declaration4 = Factory.New<JobDeclaration>();
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[FilterTypes.ClearanceStatusText];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = ClearanceStatusCodeList.Codes.C1;
			AssertEquals(true, declaration1.MatchesFilter(filterBO.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterBO.Filter));
			AssertEquals(false, declaration3.MatchesFilter(filterBO.Filter));
			AssertEquals(false, declaration4.MatchesFilter(filterBO.Filter));
			filter.Property = ClearanceStatusCodeList.Codes.C2;
			AssertEquals(false, declaration1.MatchesFilter(filterBO.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterBO.Filter));
			AssertEquals(false, declaration3.MatchesFilter(filterBO.Filter));
			AssertEquals(false, declaration4.MatchesFilter(filterBO.Filter));
			filter.Property = ClearanceStatusCodeList.Codes.NOT;
			AssertEquals(false, declaration1.MatchesFilter(filterBO.Filter));
			AssertEquals(true, declaration2.MatchesFilter(filterBO.Filter));
			AssertEquals(true, declaration3.MatchesFilter(filterBO.Filter));
			AssertEquals(true, declaration4.MatchesFilter(filterBO.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = ClearanceStatusCodeList.Codes.C1;
			AssertEquals(false, declaration1.MatchesFilter(filterBO.Filter));
			AssertEquals(true, declaration2.MatchesFilter(filterBO.Filter));
			AssertEquals(true, declaration3.MatchesFilter(filterBO.Filter));
			AssertEquals(true, declaration4.MatchesFilter(filterBO.Filter));
			filter.Property = ClearanceStatusCodeList.Codes.C2;
			AssertEquals(true, declaration1.MatchesFilter(filterBO.Filter));
			AssertEquals(true, declaration2.MatchesFilter(filterBO.Filter));
			AssertEquals(true, declaration3.MatchesFilter(filterBO.Filter));
			AssertEquals(true, declaration4.MatchesFilter(filterBO.Filter));
			filter.Property = ClearanceStatusCodeList.Codes.NOT;
			AssertEquals(true, declaration1.MatchesFilter(filterBO.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterBO.Filter));
			AssertEquals(false, declaration3.MatchesFilter(filterBO.Filter));
			AssertEquals(false, declaration4.MatchesFilter(filterBO.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			AssertEquals(false, declaration1.MatchesFilter(filterBO.Filter));
			AssertEquals(true, declaration2.MatchesFilter(filterBO.Filter));
			AssertEquals(true, declaration3.MatchesFilter(filterBO.Filter));
			AssertEquals(true, declaration4.MatchesFilter(filterBO.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			AssertEquals(true, declaration1.MatchesFilter(filterBO.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterBO.Filter));
			AssertEquals(false, declaration3.MatchesFilter(filterBO.Filter));
			AssertEquals(false, declaration4.MatchesFilter(filterBO.Filter));
		}

		public override void TestImporterNameSearch()
		{
			var declaration = Factory.New<JobDeclaration>();
			var decImporter = Factory.New<OrgHeader>();
			var mainAddress = decImporter.MainAddress;
			mainAddress.OA_Address1 = "Dec Importer Address 1";
			mainAddress.OA_CompanyNameOverride = "Importer OA CompanyName";
			decImporter.OH_Code = "DECSUP";
			declaration.JE_OH_Importer = decImporter.PK;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = false;
			importerDocumentaryAddress.E2_CompanyName = "Importer E2 CompanyName";
			importerDocumentaryAddress.E2_OA_Address = mainAddress.PK;
			Factory.Save();
			var filter = (ModuleTextFilter)filterBO[FilterTypes.ImporterName];
			AssertEquals(OrgAddressSchema.OA_CompanyNameOverride.MaxLength, filter.MaxLength);
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "Importer OA CompanyName";
			var filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(declaration.PK, filteredDecs[0].PK);
			filter.Property = "Importer E2 CompanyName";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(0, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "OA CompanyName";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(0, filteredDecs.Length);
			filter.Property = "Importer OA";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(declaration.PK, filteredDecs[0].PK);
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "CompanyName";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(declaration.PK, filteredDecs[0].PK);
			filter.Property = "OA";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(declaration.PK, filteredDecs[0].PK);
			filter.Property = "E2";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(0, filteredDecs.Length);
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_CompanyName = "Importer E2 CompanyName";
			Factory.Save();
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "Importer OA CompanyName";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(0, filteredDecs.Length);
			filter.Property = "Importer E2 CompanyName";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(declaration.PK, filteredDecs[0].PK);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "E2 CompanyName";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(0, filteredDecs.Length);
			filter.Property = "Importer E2";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(declaration.PK, filteredDecs[0].PK);
			filter.Property = "Importer OA";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(0, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "E2 CompanyName";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(declaration.PK, filteredDecs[0].PK);
			filter.Property = "E2";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(declaration.PK, filteredDecs[0].PK);
			filter.Property = "OA";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(0, filteredDecs.Length);
		}

		public override void TestSupplierNameSearch()
		{
			var declaration = Factory.New<JobDeclaration>();
			var decSupplier = Factory.New<OrgHeader>();
			var mainAddress = decSupplier.MainAddress;
			mainAddress.OA_Address1 = "Dec Supplier Address 1";
			mainAddress.OA_CompanyNameOverride = "Supplier OA CompanyName";
			decSupplier.OH_Code = "DECSUP";
			declaration.JE_OH_Supplier = decSupplier.PK;
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = false;
			supplierDocumentaryAddress.E2_CompanyName = "Supplier E2 CompanyName";
			supplierDocumentaryAddress.E2_OA_Address = mainAddress.PK;
			Factory.Save();
			var filter = (ModuleTextFilter)filterBO[FilterTypes.SupplierName];
			AssertEquals(OrgAddressSchema.OA_CompanyNameOverride.MaxLength, filter.MaxLength);
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "Supplier OA CompanyName";
			var filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(declaration.PK, filteredDecs[0].PK);
			filter.Property = "Supplier E2 CompanyName";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(0, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "OA CompanyName";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(0, filteredDecs.Length);
			filter.Property = "Supplier OA";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(declaration.PK, filteredDecs[0].PK);
			filter.Property = "Supplier E2";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(0, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "OA";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(declaration.PK, filteredDecs[0].PK);
			filter.Property = "E2";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(0, filteredDecs.Length);
			filter.Property = "CompanyName";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(declaration.PK, filteredDecs[0].PK);
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_CompanyName = "Supplier E2 CompanyName";
			Factory.Save();
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "Supplier OA CompanyName";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(0, filteredDecs.Length);
			filter.Property = "Supplier E2 CompanyName";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(declaration.PK, filteredDecs[0].PK);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = " E2 CompanyName";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(0, filteredDecs.Length);
			filter.Property = "Supplier E2";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(declaration.PK, filteredDecs[0].PK);
			filter.Property = "Supplier OA";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(0, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "E2";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(declaration.PK, filteredDecs[0].PK);
			filter.Property = "OA";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(0, filteredDecs.Length);
			filter.Property = "CompanyName";
			filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(declaration.PK, filteredDecs[0].PK);
		}

		public void TestImporterNameFilterComparisonOperatorList()
		{
			var filter = (ModuleTextFilter)filterBO[FilterTypes.ImporterName];
			AssertContainsExactElementsInAnyOrder(new[] { ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.StartsWith, ModuleTextFilter.ComparisonConstants.Contains }, filter.ComparisonOperator_List.GetAllCodes());
		}

		public void TestSupplierNameFilterComparisonOperatorList()
		{
			var filter = (ModuleTextFilter)filterBO[FilterTypes.SupplierName];
			AssertContainsExactElementsInAnyOrder(new[] { ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.StartsWith, ModuleTextFilter.ComparisonConstants.Contains }, filter.ComparisonOperator_List.GetAllCodes());
		}

		public void TestImporterNameSearchWhenOA_CompanyNameOverrideIsEmpty()
		{
			var testOrg = Factory.New<OrgHeader>();
			var testAddress = testOrg.MainAddress;
			testAddress.OA_Address1 = "Address 1";
			testOrg.OH_Code = "TESTORG1";
			testOrg.OH_FullName = "Test Name";
			var testDecl = Factory.New<JobDeclaration>();
			testDecl.JE_OH_Importer = testOrg.PK;
			var importerDocumentaryAddress = testDecl.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = false;
			importerDocumentaryAddress.E2_CompanyName = "E2 Company Name(IMD)";
			importerDocumentaryAddress.E2_OA_Address = testAddress.PK;
			Factory.Save();
			var importerNameFilter = (ModuleTextFilter)filterBO[FilterTypes.ImporterName];
			importerNameFilter.IsActive = true;
			importerNameFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			importerNameFilter.Property = "Test";
			var filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(testDecl.PK, filteredDecs[0].PK);
		}

		public void TestSupplierNameSearchWhenOA_CompanyNameOverrideIsEmpty()
		{
			var testOrg = Factory.New<OrgHeader>();
			var testAddress = testOrg.MainAddress;
			testAddress.OA_Address1 = "Address 1";
			testOrg.OH_Code = "TESTORG1";
			testOrg.OH_FullName = "Test Name";
			var testDecl = Factory.New<JobDeclaration>();
			testDecl.JE_OH_Supplier = testOrg.PK;
			var supplierDocumentaryAddress = testDecl.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = false;
			supplierDocumentaryAddress.E2_CompanyName = "E2 Company Name(SUD)";
			supplierDocumentaryAddress.E2_OA_Address = testAddress.PK;
			Factory.Save();
			var supplierNameFilter = (ModuleTextFilter)filterBO[FilterTypes.SupplierName];
			supplierNameFilter.IsActive = true;
			supplierNameFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			supplierNameFilter.Property = "Test";
			var filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(testDecl.PK, filteredDecs[0].PK);
		}

		public void TestImporterAndSupplierNameCategory()
		{
			var importerNameFilter = (ModuleTextFilter)filterBO[FilterTypes.ImporterName];
			var supplierNameFilter = (ModuleTextFilter)filterBO[FilterTypes.SupplierName];
			AssertEquals(FilterCategories.Organisations, importerNameFilter.Category);
			AssertEquals(FilterCategories.Organisations, supplierNameFilter.Category);
		}

		public void TestPackingListNumberSearch()
		{
			var testDecl1 = Factory.NewWithValidTestData<JobDeclaration>();
			testDecl1.JE_DeclarationReference = "B00001285";
			var testPackingList1 = testDecl1.LoadOrCreateCusPackingList(Factory);
			testPackingList1.CUL_PackingListNumber = "PKL#001";

			var testDecl2 = Factory.NewWithValidTestData<JobDeclaration>();
			testDecl2.JE_DeclarationReference = "B00001286";
			var testPackingList2 = testDecl2.LoadOrCreateCusPackingList(Factory);
			testPackingList2.CUL_PackingListNumber = "PKL#002";

			var testDecl3 = Factory.NewWithValidTestData<JobDeclaration>();
			testDecl3.JE_DeclarationReference = "B00001288";
			var testPackingList3 = testDecl3.LoadOrCreateCusPackingList(Factory);

			var testDecl4 = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();

			var filter = (ModuleNumberFilter)filterBO[FilterTypes.PackingListNumber];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			filter.Property = "PKL";
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(filterBO.Filter);
			AssertEquals("Matched Packing on Declaration", 2, collection.Count);
			Assert(collection.Cast<JobDeclaration>().Any(x => x.JE_DeclarationReference == "B00001285"));
			Assert(collection.Cast<JobDeclaration>().Any(x => x.JE_DeclarationReference == "B00001286"));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "PKL#002";
			collection.Load(filterBO.Filter);
			AssertEquals("Matched Packing on Declaration", 1, collection.Count);
			AssertEquals("Matched Packing on Declaration", "B00001286", collection[0].JE_DeclarationReference);

			filter.Property = "PKL#003";
			collection.Load(filterBO.Filter);
			AssertEquals("Matched Packing on Declaration", 0, collection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			filter.Property = ZString.Empty;
			collection.Load(filterBO.Filter);
			AssertEquals("Matched Packing on Declaration", 1, collection.Count);
			AssertEquals("Matched Packing on Declaration", "B00001288", collection[0].JE_DeclarationReference);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filter.Property = ZString.Empty;
			collection.Load(filterBO.Filter);
			AssertEquals("Matched Packing on Declaration", 2, collection.Count);
			Assert(collection.Cast<JobDeclaration>().Any(x => x.JE_DeclarationReference == "B00001285"));
			Assert(collection.Cast<JobDeclaration>().Any(x => x.JE_DeclarationReference == "B00001286"));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = "PKL#006";
			collection.Load(filterBO.Filter);
			AssertEquals("Matched Packing on Declaration", 3, collection.Count);
			Assert(collection.Cast<JobDeclaration>().Any(x => x.JE_DeclarationReference == "B00001285"));
			Assert(collection.Cast<JobDeclaration>().Any(x => x.JE_DeclarationReference == "B00001286"));
			Assert(collection.Cast<JobDeclaration>().Any(x => x.JE_DeclarationReference == "B00001288"));
		}

		public void TestImporterChineseNameSearch()
		{
			var declaration = Factory.New<JobDeclaration>();
			var decImporter = Factory.New<OrgHeader>();
			decImporter.OH_Code = "DECSUP";
			var mainAddress = decImporter.MainAddress;
			mainAddress.OA_Address1 = "Dec Importer Address 1";
			mainAddress.OA_CompanyNameOverride = "Importer OA CompanyName";
			var zhTWtranslatedAddress1 = mainAddress.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.OTA_CompanyName = "翻譯的中文公司名";
			zhTWtranslatedAddress1.OTA_Address1 = "本地地址1";
			zhTWtranslatedAddress1.OTA_Address2 = "本地地址2";
			declaration.JE_OH_Importer = decImporter.PK;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_OA_Address = mainAddress.PK;
			importerDocumentaryAddress.E2_AddressOverride = true;
			var localAddress = importerDocumentaryAddress.LocalAddress;
			localAddress.E2_CompanyName = "綠晃科技股份有限公司";
			localAddress.E2_Address1 = "臺北加工出口區園東街6號";
			localAddress.E2_Address2 = string.Empty;
			localAddress.AdditionalAddressInformation = "5樓";
			localAddress.E2_RN_NKCountryCode = "TW";
			localAddress.E2_City = "臺北巿";
			localAddress.E2_Postcode = "90093";
			localAddress.E2_State = "TPE";
			Factory.Save();

			CombineAssertions("Address Overrided", () =>
			{
				var importerChineseNameFilter = (ModuleTextFilter)filterBO[FilterTypes.ImporterChineseName];
				importerChineseNameFilter.IsActive = true;
				importerChineseNameFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				importerChineseNameFilter.Property = "綠晃科技股份有限公司";
				var filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);

				importerChineseNameFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				importerChineseNameFilter.Property = "科技";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);

				importerChineseNameFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				importerChineseNameFilter.Property = "科技";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(0, filteredDecs.Length);

				importerChineseNameFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				importerChineseNameFilter.Property = "綠晃";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);
			});

			importerDocumentaryAddress.E2_AddressOverride = false;
			Factory.Save();
			CombineAssertions("Address is not Override", () =>
			{
				var importerChineseNameFilter = (ModuleTextFilter)filterBO[FilterTypes.ImporterChineseName];
				importerChineseNameFilter.IsActive = true;
				importerChineseNameFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				importerChineseNameFilter.Property = "翻譯的中文公司名";
				var filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);

				importerChineseNameFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				importerChineseNameFilter.Property = "中文";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);

				importerChineseNameFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				importerChineseNameFilter.Property = "中文";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(0, filteredDecs.Length);

				importerChineseNameFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				importerChineseNameFilter.Property = "翻譯";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);
			});
		}

		public void TestSupplierChineseNameSearch()
		{
			var declaration = Factory.New<JobDeclaration>();
			var decSupplier = Factory.New<OrgHeader>();
			decSupplier.OH_Code = "DECSUP";
			var mainAddress = decSupplier.MainAddress;
			mainAddress.OA_Address1 = "Dec Supplier Address 1";
			mainAddress.OA_CompanyNameOverride = "Supplier OA CompanyName";
			var zhTWtranslatedAddress1 = mainAddress.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.OTA_CompanyName = "翻譯的中文公司名";
			zhTWtranslatedAddress1.OTA_Address1 = "本地地址1";
			zhTWtranslatedAddress1.OTA_Address2 = "本地地址2";
			declaration.JE_OH_Supplier = decSupplier.PK;
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_OA_Address = mainAddress.PK;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			var localAddress = supplierDocumentaryAddress.LocalAddress;
			localAddress.E2_CompanyName = "綠晃科技股份有限公司";
			localAddress.E2_Address1 = "臺北加工出口區園東街6號";
			localAddress.E2_Address2 = string.Empty;
			localAddress.AdditionalAddressInformation = "5樓";
			localAddress.E2_RN_NKCountryCode = "TW";
			localAddress.E2_City = "臺北巿";
			localAddress.E2_Postcode = "90093";
			localAddress.E2_State = "TPE";
			Factory.Save();

			CombineAssertions("Address Overrided", () =>
			{
				var supplierChineseNameFilter = (ModuleTextFilter)filterBO[FilterTypes.SupplierChineseName];
				supplierChineseNameFilter.IsActive = true;
				supplierChineseNameFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				supplierChineseNameFilter.Property = "綠晃科技股份有限公司";
				var filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);

				supplierChineseNameFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				supplierChineseNameFilter.Property = "科技";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);

				supplierChineseNameFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				supplierChineseNameFilter.Property = "科技";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(0, filteredDecs.Length);

				supplierChineseNameFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				supplierChineseNameFilter.Property = "綠晃";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);
			});

			supplierDocumentaryAddress.E2_AddressOverride = false;
			Factory.Save();
			CombineAssertions("Address is not Override", () =>
			{
				var supplierChineseNameFilter = (ModuleTextFilter)filterBO[FilterTypes.SupplierChineseName];
				supplierChineseNameFilter.IsActive = true;
				supplierChineseNameFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				supplierChineseNameFilter.Property = "翻譯的中文公司名";
				var filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);

				supplierChineseNameFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				supplierChineseNameFilter.Property = "中文";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);

				supplierChineseNameFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				supplierChineseNameFilter.Property = "中文";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(0, filteredDecs.Length);

				supplierChineseNameFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				supplierChineseNameFilter.Property = "翻譯";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);
			});
		}

		public void TestImporterVATNumberSearch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.MainAddress;
			address.Address1 = "Address1";
			address.Address2 = "Address2";
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "96944490", Core.Constants.CountryCodes.Taiwan);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org.PK;
			var documentaryAddress = declaration.ImporterDocumentaryAddress;
			documentaryAddress.E2_OA_Address = address.PK;
			documentaryAddress.E2_AddressOverride = true;
			documentaryAddress.IDCodeType = "VAT";
			documentaryAddress.IDCode = "42521663";
			Factory.Save();

			CombineAssertions("Address Overrided", () =>
			{
				var vatFilter = (ModuleTextFilter)filterBO[FilterTypes.ImporterVATNumber];
				vatFilter.IsActive = true;
				vatFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				vatFilter.Property = "42521663";
				var filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);

				vatFilter.Property = "96944490";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(0, filteredDecs.Length);

				vatFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				vatFilter.Property = "5216";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(0, filteredDecs.Length);

				vatFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				vatFilter.Property = "4252";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);
			});

			documentaryAddress.E2_AddressOverride = false;
			Factory.Save();
			CombineAssertions("Address is not Override", () =>
			{
				var vatFilter = (ModuleTextFilter)filterBO[FilterTypes.ImporterVATNumber];
				vatFilter.IsActive = true;
				vatFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				vatFilter.Property = "96944490";
				var filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);

				vatFilter.Property = "42521663";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(0, filteredDecs.Length);

				vatFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				vatFilter.Property = "4449";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(0, filteredDecs.Length);

				vatFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				vatFilter.Property = "969";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);
			});
		}

		public void TestSupplierVATNumberSearch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.MainAddress;
			address.Address1 = "Address1";
			address.Address2 = "Address2";
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "96944490", Core.Constants.CountryCodes.Taiwan);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = org.PK;
			var documentaryAddress = declaration.SupplierDocumentaryAddress;
			documentaryAddress.E2_OA_Address = address.PK;
			documentaryAddress.E2_AddressOverride = true;
			documentaryAddress.IDCodeType = "VAT";
			documentaryAddress.IDCode = "42521663";
			Factory.Save();

			CombineAssertions("Address Overrided", () =>
			{
				var vatFilter = (ModuleTextFilter)filterBO[FilterTypes.SupplierVATNumber];
				vatFilter.IsActive = true;
				vatFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				vatFilter.Property = "42521663";
				var filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);

				vatFilter.Property = "96944490";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(0, filteredDecs.Length);

				vatFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				vatFilter.Property = "5216";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(0, filteredDecs.Length);

				vatFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				vatFilter.Property = "4252";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);
			});

			documentaryAddress.E2_AddressOverride = false;
			Factory.Save();
			CombineAssertions("Address is not Override", () =>
			{
				var vatFilter = (ModuleTextFilter)filterBO[FilterTypes.SupplierVATNumber];
				vatFilter.IsActive = true;
				vatFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				vatFilter.Property = "96944490";
				var filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);

				vatFilter.Property = "42521663";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(0, filteredDecs.Length);

				vatFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				vatFilter.Property = "4449";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(0, filteredDecs.Length);

				vatFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				vatFilter.Property = "969";
				filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(1, filteredDecs.Length);
				AssertEquals(declaration.PK, filteredDecs[0].PK);
			});
		}

		public void TestAgencyResponseCodeSearch()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			Factory.Save();

			var filterObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[FilterTypes.AgencyResponseCode];
			filter.IsActive = true;
			AssertDispositionStatusFilter(filterObj, filter, declaration, "A01", new bool[] { false, true, true, false });

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			Factory.Save();
			AssertDispositionStatusFilter(filterObj, filter, declaration, "A01", new bool[] { false, true, true, false });

			var disposition = entryHeader.CusDispositions.AddNew();
			disposition.CDI_Type = "CUS";
			disposition.CDI_StatusKey = CusDispositionStatusKeyList.Codes.ARM;
			disposition.CDI_Status = "A01";
			disposition.CDI_StatusDate = ZDateTime.Now;
			Factory.Save();
			AssertDispositionStatusFilter(filterObj, filter, declaration, "A01", new bool[] { true, false, false, true });
			AssertDispositionStatusFilter(filterObj, filter, declaration, "A02", new bool[] { false, true, false, true });
		}

		public void TestRequiredFormalitiesCodeSearch()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			Factory.Save();

			var filterObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[FilterTypes.RequiredFormalitiesCode];
			filter.IsActive = true;
			AssertDispositionStatusFilter(filterObj, filter, declaration, "A01", new bool[] { false, true, true, false });

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			Factory.Save();
			AssertDispositionStatusFilter(filterObj, filter, declaration, "A01", new bool[] { false, true, true, false });

			var disposition = entryHeader.CusDispositions.AddNew();
			disposition.CDI_Type = "CUS";
			disposition.CDI_StatusKey = CusDispositionStatusKeyList.Codes.RFM;
			disposition.CDI_Status = "A01";
			disposition.CDI_StatusDate = ZDateTime.Now;
			Factory.Save();
			AssertDispositionStatusFilter(filterObj, filter, declaration, "A01", new bool[] { true, false, false, true });
			AssertDispositionStatusFilter(filterObj, filter, declaration, "A02", new bool[] { false, true, false, true });
		}

		public void TestClearanceCodeSearch()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			Factory.Save();

			var filterObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[FilterTypes.ClearanceCode];
			filter.IsActive = true;
			AssertDispositionStatusFilter(filterObj, filter, declaration, "1", new bool[] { false, true, true, false });

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			Factory.Save();
			AssertDispositionStatusFilter(filterObj, filter, declaration, "1", new bool[] { false, true, true, false });

			var disposition = entryHeader.CusDispositions.AddNew();
			disposition.CDI_Type = "CUS";
			disposition.CDI_StatusKey = CusDispositionStatusKeyList.Codes.CLR;
			disposition.CDI_Status = "1";
			disposition.CDI_StatusDate = ZDateTime.Now;
			Factory.Save();
			AssertDispositionStatusFilter(filterObj, filter, declaration, "1", new bool[] { true, false, false, true });
			AssertDispositionStatusFilter(filterObj, filter, declaration, "2", new bool[] { false, true, false, true });
		}

		void AssertDispositionStatusFilter(JobDeclarationFilterBusinessObject filterObj, ModuleTextFilter filter, JobDeclaration declaration, ZString property, bool[] expectValue)
		{
			filter.Property = property;
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			AssertEquals(expectValue[0], declaration.MatchesFilter(filterObj.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			AssertEquals(expectValue[1], declaration.MatchesFilter(filterObj.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			AssertEquals(expectValue[2], declaration.MatchesFilter(filterObj.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			AssertEquals(expectValue[3], declaration.MatchesFilter(filterObj.Filter));
		}
	}
}
