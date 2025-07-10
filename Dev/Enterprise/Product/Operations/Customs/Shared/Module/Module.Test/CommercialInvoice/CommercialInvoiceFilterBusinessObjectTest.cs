using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceFilterBusinessObject))]
	public class CommercialInvoiceFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected sealed override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return (FilterStripBusinessObject)Activator.CreateInstance(GetExpectedBusinessObjectType());
		}

		public void TestCustomFields()
		{
			BaseJobComInvoiceHeader header1 = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
			BaseJobComInvoiceLine line1 = header1.JobComInvoiceLines.AddNew();
			line1.JI_CustomAttrib1 = "blh";

			BaseJobComInvoiceHeader header2 = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
			BaseJobComInvoiceLine line2 = header2.JobComInvoiceLines.AddNew();
			line2.JI_CustomAttrib1 = "hlb";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)filterBizObj["ComInvoiceLine.CustomAttribute1"];
			filter.Property = "hl";
			filter.IsActive = true;

			BusinessObject[] filteredCommercialInvoices = Factory.Load(typeof(BaseJobComInvoiceHeader), filterBizObj.Filter);

			AssertEquals(1, filteredCommercialInvoices.Length);
			AssertEquals(header2, filteredCommercialInvoices[0]);
		}

		public void TestSupplierName()
		{
			standaloneInvoice1.JZ_OH_Supplier = org1.PK;
			standaloneInvoice2.JZ_OH_Supplier = org2.PK;
			standaloneInvoice3.JZ_OH_Supplier = ZGuid.Empty;

			declarationInvoice1.JZ_OH_Supplier = ZGuid.Empty;
			declarationInvoice1.JobDeclaration.JE_OH_Supplier = org1.PK;

			declarationInvoice2.JZ_OH_Supplier = org2.PK;
			declarationInvoice2.JobDeclaration.JE_OH_Supplier = org1.PK;

			declarationInvoice3.JZ_OH_Supplier = org1.PK;
			declarationInvoice3.JobDeclaration.JE_OH_Supplier = org2.PK;

			Factory.Save();

			var filter = (ModuleTextFilter)filterBizObj[CommercialInvoiceFilterConstants.SupplierName];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "";

			AssertQueryResults(
				filterBizObj.Filter,
				(standaloneInvoice1, true),
				(standaloneInvoice2, true),
				(standaloneInvoice3, true),
				(declarationInvoice1, true),
				(declarationInvoice2, true),
				(declarationInvoice3, true)
			);

			filter.Property = "A";
			AssertQueryResults(
				filterBizObj.Filter,
				(standaloneInvoice1, true),
				(standaloneInvoice2, true),
				(standaloneInvoice3, false),
				(declarationInvoice1, true),
				(declarationInvoice2, true),
				(declarationInvoice3, true)
			);

			filter.Property = "AA";
			AssertQueryResults(
				filterBizObj.Filter,
				(standaloneInvoice1, true),
				(standaloneInvoice2, false),
				(standaloneInvoice3, false),
				(declarationInvoice1, true),
				(declarationInvoice2, false),
				(declarationInvoice3, true)
			);
		}

		public void TestImporterName()
		{
			standaloneInvoice1.JZ_OH_Buyer = org1.PK;
			standaloneInvoice2.JZ_OH_Buyer = org2.PK;
			standaloneInvoice3.JZ_OH_Buyer = ZGuid.Empty;

			declarationInvoice1.JZ_OH_Buyer = ZGuid.Empty;
			declarationInvoice1.JobDeclaration.JE_OH_Importer = org1.PK;

			declarationInvoice2.JZ_OH_Buyer = org2.PK;
			declarationInvoice2.JobDeclaration.JE_OH_Importer = org1.PK;

			declarationInvoice3.JZ_OH_Buyer = org1.PK;
			declarationInvoice3.JobDeclaration.JE_OH_Importer = org2.PK;

			Factory.Save();

			var filter = (ModuleTextFilter)filterBizObj[CommercialInvoiceFilterConstants.ImporterName];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "";

			AssertQueryResults(
				filterBizObj.Filter,
				(standaloneInvoice1, true),
				(standaloneInvoice2, true),
				(standaloneInvoice3, true),
				(declarationInvoice1, true),
				(declarationInvoice2, true),
				(declarationInvoice3, true)
			);

			filter.Property = "A";
			AssertQueryResults(
				filterBizObj.Filter,
				(standaloneInvoice1, true),
				(standaloneInvoice2, true),
				(standaloneInvoice3, false),
				(declarationInvoice1, true),
				(declarationInvoice2, true),
				(declarationInvoice3, true)
			);

			filter.Property = "AA";
			AssertQueryResults(
				filterBizObj.Filter,
				(standaloneInvoice1, true),
				(standaloneInvoice2, false),
				(standaloneInvoice3, false),
				(declarationInvoice1, true),
				(declarationInvoice2, false),
				(declarationInvoice3, true)
			);
		}

		public void TestInvoiceNumber()
		{
			var filter = (ModuleTextFilter)filterBizObj[CommercialInvoiceFilterConstants.InvoiceNumber];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			filter.Property = "";

			AssertQueryResults(
				filter.Query,
				(standaloneInvoice1, true),
				(standaloneInvoice2, true),
				(declarationInvoice1, true),
				(declarationInvoice2, true)
			);

			filter.Property = "ST";
			AssertQueryResults(
				filter.Query,
				(standaloneInvoice1, true),
				(standaloneInvoice2, true),
				(declarationInvoice1, false),
				(declarationInvoice2, false)
			);

			filter.Property = "JD";
			AssertQueryResults(
				filter.Query,
				(standaloneInvoice1, false),
				(standaloneInvoice2, false),
				(declarationInvoice1, true),
				(declarationInvoice2, true)
			);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "-INV-0";
			AssertQueryResults(
				filter.Query,
				(standaloneInvoice1, true),
				(standaloneInvoice2, true),
				(declarationInvoice1, true),
				(declarationInvoice2, true)
			);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "-INV-01";
			AssertQueryResults(
				filter.Query,
				(standaloneInvoice1, true),
				(standaloneInvoice2, false),
				(declarationInvoice1, true),
				(declarationInvoice2, false)
			);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "-INV-01";
			AssertQueryResults(
				filter.Query,
				(standaloneInvoice1, false),
				(standaloneInvoice2, false),
				(declarationInvoice1, false),
				(declarationInvoice2, false)
			);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "ST-INV-01";
			AssertQueryResults(
				filter.Query,
				(standaloneInvoice1, true),
				(standaloneInvoice2, false),
				(declarationInvoice1, false),
				(declarationInvoice2, false)
			);
		}

		public void TestImporterSupplierSearch_Importer()
		{
			standaloneInvoice1.JZ_OH_Buyer = ZGuid.Empty;
			standaloneInvoice2.JZ_OH_Buyer = org1.PK;
			standaloneInvoice3.JZ_OH_Buyer = org2.PK;

			declarationInvoice1.JZ_OH_Buyer = ZGuid.Empty;
			declarationInvoice1.JobDeclaration.JE_OH_Importer = org1.PK;

			declarationInvoice2.JZ_OH_Buyer = ZGuid.Empty;
			declarationInvoice2.JobDeclaration.JE_OH_Importer = org2.PK;

			declarationInvoice3.JZ_OH_Buyer = org2.PK;
			declarationInvoice3.JobDeclaration.JE_OH_Importer = org1.PK;

			Factory.Save();

			var filter = (ModuleGuidsFilter)filterBizObj[CommercialInvoiceFilterConstants.ImporterSupplier];
			filter.IsActive = true;
			filter.Property1 = org1.PK;

			AssertQueryResults(
				filter.Query,
				(standaloneInvoice1, false),
				(standaloneInvoice2, true),
				(standaloneInvoice3, false),
				(declarationInvoice1, true),
				(declarationInvoice2, false),
				(declarationInvoice3, false)
			);
		}

		public void TestImporterSupplierSearch_Supplier()
		{
			standaloneInvoice1.JZ_OH_Supplier = ZGuid.Empty;
			standaloneInvoice2.JZ_OH_Supplier = org1.PK;
			standaloneInvoice3.JZ_OH_Supplier = org2.PK;

			declarationInvoice1.JZ_OH_Supplier = ZGuid.Empty;
			declarationInvoice1.JobDeclaration.JE_OH_Supplier = org1.PK;

			declarationInvoice2.JZ_OH_Supplier = ZGuid.Empty;
			declarationInvoice2.JobDeclaration.JE_OH_Supplier = org2.PK;

			declarationInvoice3.JZ_OH_Supplier = org2.PK;
			declarationInvoice3.JobDeclaration.JE_OH_Supplier = org1.PK;

			Factory.Save();

			var filter = (ModuleGuidsFilter)filterBizObj[CommercialInvoiceFilterConstants.ImporterSupplier];
			filter.IsActive = true;
			filter.Property2 = org1.PK;

			AssertQueryResults(
				filter.Query,
				(standaloneInvoice1, false),
				(standaloneInvoice2, true),
				(standaloneInvoice3, false),
				(declarationInvoice1, true),
				(declarationInvoice2, false),
				(declarationInvoice3, false)
			);
		}

		public void TestImporterSupplierSearch_ImporterAndSupplier()
		{
			standaloneInvoice1.JZ_OH_Buyer = org1.PK;
			standaloneInvoice1.JZ_OH_Supplier = ZGuid.Empty;

			standaloneInvoice2.JZ_OH_Buyer = ZGuid.Empty;
			standaloneInvoice2.JZ_OH_Supplier = org2.PK;

			standaloneInvoice3.JZ_OH_Buyer = org1.PK;
			standaloneInvoice3.JZ_OH_Supplier = org2.PK;

			standaloneInvoice4.JZ_OH_Buyer = org2.PK;
			standaloneInvoice4.JZ_OH_Supplier = org1.PK;

			declarationInvoice1.JZ_OH_Buyer = org1.PK;
			declarationInvoice1.JZ_OH_Supplier = ZGuid.Empty;
			declarationInvoice1.JobDeclaration.JE_OH_Importer = ZGuid.Empty;
			declarationInvoice1.JobDeclaration.JE_OH_Supplier = org2.PK;

			declarationInvoice2.JZ_OH_Buyer = ZGuid.Empty;
			declarationInvoice2.JZ_OH_Supplier = org2.PK;
			declarationInvoice2.JobDeclaration.JE_OH_Importer = org1.PK;
			declarationInvoice2.JobDeclaration.JE_OH_Supplier = ZGuid.Empty;

			declarationInvoice3.JZ_OH_Buyer = ZGuid.Empty;
			declarationInvoice3.JZ_OH_Supplier = ZGuid.Empty;
			declarationInvoice3.JobDeclaration.JE_OH_Importer = org1.PK;
			declarationInvoice3.JobDeclaration.JE_OH_Supplier = org2.PK;

			declarationInvoice4.JZ_OH_Buyer = org2.PK;
			declarationInvoice4.JZ_OH_Supplier = ZGuid.Empty;
			declarationInvoice4.JobDeclaration.JE_OH_Importer = org1.PK;
			declarationInvoice4.JobDeclaration.JE_OH_Supplier = org2.PK;

			declarationInvoice5.JZ_OH_Buyer = ZGuid.Empty;
			declarationInvoice5.JZ_OH_Supplier = org1.PK;
			declarationInvoice5.JobDeclaration.JE_OH_Importer = org1.PK;
			declarationInvoice5.JobDeclaration.JE_OH_Supplier = org2.PK;

			Factory.Save();

			var filter = (ModuleGuidsFilter)filterBizObj[CommercialInvoiceFilterConstants.ImporterSupplier];
			filter.IsActive = true;
			filter.Property1 = org1.PK;
			filter.Property2 = org2.PK;

			AssertQueryResults(
				filter.Query,
				(standaloneInvoice1, false),
				(standaloneInvoice2, false),
				(standaloneInvoice3, true),
				(standaloneInvoice4, false),
				(declarationInvoice1, true),
				(declarationInvoice2, true),
				(declarationInvoice3, true),
				(declarationInvoice4, false),
				(declarationInvoice5, false)
			);
		}

		#region CompanyFilter

		public void TestCompanyFilter()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "Y?";
			country.RN_Desc = "Dummy Country";

			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "Z1Z";
			company1.GC_RN_NKCountryCode = country.Code;

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "Z2Z";
			company2.GC_RN_NKCountryCode = country.Code;

			var branch11 = company1.Branches.AddNew();
			branch11.GB_Code = "Z11";

			var branch12 = company1.Branches.AddNew();
			branch12.GB_Code = "Z12";

			var branch21 = company2.Branches.AddNew();
			branch21.GB_Code = "Z22";

			standaloneInvoice1.JZ_GB = GlbBranch.CurrentBranch.PK;
			standaloneInvoice2.JZ_GB = branch11.PK;
			standaloneInvoice3.JZ_GB = branch12.PK;
			standaloneInvoice4.JZ_GB = branch21.PK;

			declarationInvoice1.JobDeclaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declarationInvoice2.JobDeclaration.JE_GB = branch11.PK;
			declarationInvoice3.JobDeclaration.JE_GB = branch12.PK;
			declarationInvoice4.JobDeclaration.JE_GB = branch21.PK;

			Factory.Save();

			filterBizObj = new CommercialInvoiceFilterBusinessObject();
			DisableAttachedToDeclarationFilter(filterBizObj);

			var filter = (ModuleGuidFilter)filterBizObj[CommercialInvoiceFilterConstants.Company];
			AssertEquals(FilterVisibility.AlwaysAppliedAndHidden, filter.Visibility);

			AssertQueryResults(filterBizObj.Filter,
				(standaloneInvoice1, true),
				(standaloneInvoice2, false),
				(standaloneInvoice3, false),
				(standaloneInvoice4, false),
				(declarationInvoice1, true),
				(declarationInvoice2, false),
				(declarationInvoice3, false),
				(declarationInvoice4, false)
			);

			filter.Property = company1.PK;
			AssertQueryResults(filterBizObj.Filter,
				(standaloneInvoice1, false),
				(standaloneInvoice2, true),
				(standaloneInvoice3, true),
				(standaloneInvoice4, false),
				(declarationInvoice1, false),
				(declarationInvoice2, true),
				(declarationInvoice3, true),
				(declarationInvoice4, false)
			);

			filter.Property = company2.PK;
			AssertQueryResults(filterBizObj.Filter,
				(standaloneInvoice1, false),
				(standaloneInvoice2, false),
				(standaloneInvoice3, false),
				(standaloneInvoice4, true),
				(declarationInvoice1, false),
				(declarationInvoice2, false),
				(declarationInvoice3, false),
				(declarationInvoice4, true)
			);
		}

		public void TestCompanyFilterForNonCurrentCompany()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "Y?";
			country.RN_Desc = "Dummy Country";

			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "Z1Z";
			company1.GC_RN_NKCountryCode = country.Code;

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "Z2Z";
			company2.GC_RN_NKCountryCode = country.Code;

			var branch11 = company1.Branches.AddNew();
			branch11.GB_Code = "Z11";

			var branch12 = company1.Branches.AddNew();
			branch12.GB_Code = "Z12";

			var branch21 = company2.Branches.AddNew();
			branch21.GB_Code = "Z22";

			standaloneInvoice1.JZ_GB = GlbBranch.CurrentBranch.PK;
			standaloneInvoice2.JZ_GB = branch11.PK;
			standaloneInvoice3.JZ_GB = branch12.PK;
			standaloneInvoice4.JZ_GB = branch21.PK;

			declarationInvoice1.JobDeclaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declarationInvoice2.JobDeclaration.JE_GB = branch11.PK;
			declarationInvoice3.JobDeclaration.JE_GB = branch12.PK;
			declarationInvoice4.JobDeclaration.JE_GB = branch21.PK;

			Factory.Save();

			filterBizObj = new CommercialInvoiceFilterBusinessObject(true);
			DisableAttachedToDeclarationFilter(filterBizObj);

			var filter = (ModuleGuidFilter)filterBizObj[CommercialInvoiceFilterConstants.Company];
			AssertEquals(FilterVisibility.AlwaysAppliedAndHidden, filter.Visibility);

			AssertQueryResults(filterBizObj.Filter,
				(standaloneInvoice1, false),
				(standaloneInvoice2, false),
				(standaloneInvoice3, false),
				(standaloneInvoice4, false),
				(declarationInvoice1, false),
				(declarationInvoice2, true),
				(declarationInvoice3, true),
				(declarationInvoice4, true)
			);

			filter.Property = company1.PK;
			AssertQueryResults(filterBizObj.Filter,
				(standaloneInvoice1, false),
				(standaloneInvoice2, false),
				(standaloneInvoice3, false),
				(standaloneInvoice4, false),
				(declarationInvoice1, true),
				(declarationInvoice2, false),
				(declarationInvoice3, false),
				(declarationInvoice4, true)
			);

			filter.Property = company2.PK;
			AssertQueryResults(filterBizObj.Filter,
				(standaloneInvoice1, false),
				(standaloneInvoice2, false),
				(standaloneInvoice3, false),
				(standaloneInvoice4, false),
				(declarationInvoice1, true),
				(declarationInvoice2, true),
				(declarationInvoice3, true),
				(declarationInvoice4, false)
			);
		}

		public void TestCompanyFilterLiteralTextADO()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "Z1Z";
			company.GC_RN_NKCountryCode = "AU";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "Z1Z";
			Factory.Save();

			var companyFilter = (ModuleGuidFilter)filterBizObj[CommercialInvoiceFilterConstants.Company];
			companyFilter.Property = company.PK;
			AssertEquals("should include extra 'JZ_PK IN' to avoid duplicate", string.Format(ExpectCompanyFilterLiteralTextADO, branch.PK), filterBizObj.Filter.LiteralTextADO);
		}

		protected virtual string ExpectCompanyFilterLiteralTextADO => "JZ_PK IN (SELECT JZ_PK FROM dbo.JobComInvoiceHeader WHERE JZ_GB = CONVERT('{0}', 'System.Guid') or (JZ_JE IN (SELECT JE_PK FROM dbo.JobDeclaration WHERE JE_GB = CONVERT('{0}', 'System.Guid') and JE_ApplicationCode <> 'EMC' and JE_IsCancelled = 0)))";

		#endregion

		public void TestEMCSDeclaration()
		{
			filterBizObj = new CommercialInvoiceFilterBusinessObject();
			DisableAttachedToDeclarationFilter(filterBizObj);

			var emcsDeclaration = (BaseJobDeclaration)Factory.New<Integration.Customs.EUEMCS.IJobDeclaration>();
			var emcsInvoice = emcsDeclaration.Invoices.AddNew();
			emcsInvoice.JZ_InvoiceNumber = "EMCS-INV";

			Factory.Save();

			AssertQueryResults(filterBizObj.Filter,
				(standaloneInvoice1, true),
				(declarationInvoice1, true),
				(emcsInvoice, false)
			);
		}

		public void TestAttachedToDeclarationFilter()
		{
			filterBizObj = new CommercialInvoiceFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizObj[CommercialInvoiceFilterConstants.AttachedToDeclaration];
			AssertEquals(AttachedToDeclarationFilterOptions.Codes.NotAttachedToDeclaration, filter.Property);

			AssertQueryResults(filterBizObj.Filter,
				(standaloneInvoice1, true),
				(standaloneInvoice2, true),
				(declarationInvoice1, false),
				(declarationInvoice2, false)
			);

			filter.Property = AttachedToDeclarationFilterOptions.Codes.AttachedToDeclaration;
			AssertQueryResults(filterBizObj.Filter,
				(standaloneInvoice1, false),
				(standaloneInvoice2, false),
				(declarationInvoice1, true),
				(declarationInvoice2, true)
			);

			filter.Property = AttachedToDeclarationFilterOptions.Codes.All;
			AssertQueryResults(filterBizObj.Filter,
				(standaloneInvoice1, true),
				(standaloneInvoice2, true),
				(declarationInvoice1, true),
				(declarationInvoice2, true)
			);
		}

		public void TestInvoiceDate()
		{
			var today = ZDateTime.Today;
			standaloneInvoice1.JZ_InvoiceDate = today.AddDays(-10);
			standaloneInvoice3.JZ_InvoiceDate = today;
			standaloneInvoice2.JZ_InvoiceDate = today.AddDays(+10);
			Factory.Save();

			var filter = (ModuleDateFilter)filterBizObj[CommercialInvoiceFilterConstants.InvoiceDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;
			filter.Property1 = today.AddDays(-4);
			AssertQueryResults
			(
				filter.Query,
				(standaloneInvoice1, false),
				(standaloneInvoice2, true),
				(standaloneInvoice3, true)
			);

			filter.Property1 = today.AddDays(-10);
			AssertQueryResults
			(
				filter.Query,
				(standaloneInvoice1, true),
				(standaloneInvoice2, true),
				(standaloneInvoice3, true)
			);
		}

		public void TestInvoiceTotal()
		{
			standaloneInvoice1.JZ_InvoiceAmount = 100m;
			standaloneInvoice2.JZ_InvoiceAmount = 101m;
			standaloneInvoice3.JZ_InvoiceAmount = 102m;
			declarationInvoice1.JZ_InvoiceAmount = 100m;
			declarationInvoice2.JZ_InvoiceAmount = 101m;
			declarationInvoice3.JZ_InvoiceAmount = 102m;
			Factory.Save();

			var filter = (ModuleNumberRangeFilter)filterBizObj[CommercialInvoiceFilterConstants.InvoiceTotal];
			filter.IsActive = true;
			filter.Property1 = 100m;
			filter.Property2 = 101m;

			AssertQueryResults(
				filter.Query,
				(standaloneInvoice1, true),
				(standaloneInvoice2, true),
				(standaloneInvoice3, false),
				(declarationInvoice1, true),
				(declarationInvoice2, true),
				(declarationInvoice3, false)
			);

			filter.Property1 = 101m;
			filter.Property2 = 102m;

			AssertQueryResults(
				filter.Query,
				(standaloneInvoice1, false),
				(standaloneInvoice2, true),
				(standaloneInvoice3, true),
				(declarationInvoice1, false),
				(declarationInvoice2, true),
				(declarationInvoice3, true)
			);
		}

		public void TestShipmentType()
		{
			standaloneInvoice1.JZ_MessageType = "EXP";
			standaloneInvoice2.JZ_MessageType = "IMP";

			declarationInvoice1.JobDeclaration.JE_MessageType = "EXP";
			declarationInvoice2.JobDeclaration.JE_MessageType = "IMP";

			Factory.Save();

			var filter = (ModuleTextFilter)filterBizObj[CommercialInvoiceFilterConstants.ShipmentType];
			AssertEquals(FilterCategories.ModesAndTypes, filter.Category);

			filter.IsActive = true;
			filter.Property = "IMP";

			AssertQueryResults(
				filter.Query,
				(standaloneInvoice1, false),
				(standaloneInvoice2, true),
				(declarationInvoice1, false),
				(declarationInvoice2, true)
			);
		}

		public void TestDefaultCreatedTimeFilter()
		{
			var filterBO = new CommercialInvoiceFilterBusinessObject();
			filterBO.QueryObjectType = typeof(BaseJobComInvoiceHeader);
			var filter = filterBO.ModuleFilters["Created Time"] as ModuleDateFilter;
			AssertEquals(true, filter.Visible);
			AssertEquals(FilterVisibility.AlwaysVisible, filter.Visibility);
			AssertEquals(ModuleDateFilter.DateRangeSearchTexts.Last3Mths, filter.PropertySearch);
		}

		#region Routing Filters

		public void TestFiltersForNotLinkedRouting_StandaloneInvoice()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "Z1Z";
			company.GC_RN_NKCountryCode = "AU";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "Z1Z";

			var companyFilter = (ModuleGuidFilter)filterBizObj[CommercialInvoiceFilterConstants.Company];
			companyFilter.Property = company.PK;

			var comInvoice1 = Factory.New<BaseJobComInvoiceHeader>();
			comInvoice1.JZ_GB = branch.PK;
			comInvoice1.JZ_InvoiceNumber = "INV1";
			comInvoice1.JZ_OH_Buyer = org1.PK;
			comInvoice1.JZ_OH_Supplier = org2.PK;

			var comInvoice2 = Factory.New<BaseJobComInvoiceHeader>();
			comInvoice2.JZ_GB = branch.PK;
			comInvoice2.JZ_InvoiceNumber = "INV2";
			comInvoice2.JZ_OH_Buyer = org2.PK;
			comInvoice2.JZ_OH_Supplier = org3.PK;

			var comInvoice3 = Factory.New<BaseJobComInvoiceHeader>();
			comInvoice3.JZ_GB = branch.PK;
			comInvoice3.JZ_InvoiceNumber = "INV3";
			comInvoice3.JZ_OH_Buyer = org1.PK;
			comInvoice3.JZ_OH_Supplier = org3.PK;

			var transport1 = comInvoice1.Transports.AddNew();
			transport1.JW_Vessel = "ABC VESSEL";
			transport1.JW_VoyageFlight = "V32D4";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "USLAX";
			transport1.JW_ETA = new ZDateTime(2009, 7, 29);
			transport1.CarrierPK = org1.PK;

			var transport2 = comInvoice2.Transports.AddNew();
			transport2.JW_Vessel = "CDE VESSEL";
			transport2.JW_VoyageFlight = "V64D4";
			transport2.JW_RL_NKLoadPort = "AUMEL";
			transport2.JW_RL_NKDiscPort = "USCHI";
			transport2.JW_ETA = new ZDateTime(2011, 5, 30);
			transport2.CarrierPK = org2.PK;

			Factory.Save();

			var filter = (ModuleNkFilter)filterBizObj[CommercialInvoiceFilterConstants.Vessel];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			filter.Property = "ABC VESSEL";

			AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, false), (comInvoice3, false));
			AssertEquals(FilterCategories.ModesAndTypes, filter.Category);

			filter.Property = "ZZZ";
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, false), (comInvoice2, false), (comInvoice3, false));

			filter.Property = "";
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, true), (comInvoice3, true));

			var filter1 = (ModuleLocationFilter)filterBizObj[CommercialInvoiceFilterConstants.LoadDischarge];
			filter1.IsActive = true;
			filter1.Property1 = "AUSYD";
			filter1.Property2 = "USLAX";
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, false), (comInvoice3, false));

			filter1.Property1 = "AUSYD";
			filter1.Property2 = "USCHI";
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, false), (comInvoice2, false), (comInvoice3, false));

			filter1.Property1 = "AUMEL";
			filter1.Property2 = "USCHI";
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, false), (comInvoice2, true), (comInvoice3, false));

			filter1.Property1 = ZString.Empty;
			filter1.Property2 = ZString.Empty;
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, true), (comInvoice3, true));

			var dateFilter = (ModuleDateFilter)filterBizObj[CommercialInvoiceFilterConstants.ETA];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			dateFilter.Property1 = new ZDateTime(2009, 7, 29);
			dateFilter.Property2 = new ZDateTime(2009, 7, 29);
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, false), (comInvoice3, false));

			dateFilter.Property1 = ZDateTime.Empty;
			dateFilter.Property2 = ZDateTime.Empty;
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, true), (comInvoice3, true));

			var carrierFilter = (ModuleGuidFilter)filterBizObj[CommercialInvoiceFilterConstants.RoutingCarrier];
			carrierFilter.IsActive = true;
			carrierFilter.Property = org1.PK;
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, false), (comInvoice3, false));
		}

		public void TestFiltersForNotLinkedRouting_Declaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var company = Factory.New<GlbCompany>();
				company.GC_Code = "Z1Z";
				company.GC_RN_NKCountryCode = "AU";
				var branch = company.Branches.AddNew();
				branch.GB_Code = "Z1Z";

				var companyFilter = (ModuleGuidFilter)filterBizObj[CommercialInvoiceFilterConstants.Company];
				companyFilter.Property = company.PK;

				var declaration1 = Factory.New<BaseJobDeclaration>();
				declaration1.JE_GB = branch.PK;

				var comInvoice1 = declaration1.Invoices.AddNew();
				comInvoice1.JZ_InvoiceNumber = "INV1";
				comInvoice1.JZ_OH_Buyer = org1.PK;
				comInvoice1.JZ_OH_Supplier = org2.PK;

				var declaration2 = Factory.New<BaseJobDeclaration>();
				declaration2.JE_GB = branch.PK;

				var comInvoice2 = declaration2.Invoices.AddNew();
				comInvoice2.JZ_InvoiceNumber = "INV2";
				comInvoice2.JZ_OH_Buyer = org2.PK;
				comInvoice2.JZ_OH_Supplier = org3.PK;

				var declaration3 = Factory.New<BaseJobDeclaration>();
				declaration3.JE_GB = branch.PK;

				var comInvoice3 = declaration3.Invoices.AddNew();
				comInvoice3.JZ_InvoiceNumber = "INV3";
				comInvoice3.JZ_OH_Buyer = org1.PK;
				comInvoice3.JZ_OH_Supplier = org3.PK;

				var transport1 = declaration1.Transports.AddNew();
				transport1.JW_Vessel = "ABC VESSEL";
				transport1.JW_VoyageFlight = "V32D4";
				transport1.JW_RL_NKLoadPort = "AUSYD";
				transport1.JW_RL_NKDiscPort = "USLAX";
				transport1.JW_ETA = new ZDateTime(2009, 7, 29);
				transport1.CarrierPK = org1.PK;

				var transport2 = declaration2.Transports.AddNew();
				transport2.JW_Vessel = "CDE VESSEL";
				transport2.JW_VoyageFlight = "V64D4";
				transport2.JW_RL_NKLoadPort = "AUMEL";
				transport2.JW_RL_NKDiscPort = "USCHI";
				transport2.JW_ETA = new ZDateTime(2011, 5, 30);
				transport2.CarrierPK = org2.PK;

				Factory.Save();

				var filter = (ModuleNkFilter)filterBizObj[CommercialInvoiceFilterConstants.Vessel];
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.IsActive = true;
				filter.Property = "ABC VESSEL";

				AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, false), (comInvoice3, false));
				AssertEquals(FilterCategories.ModesAndTypes, filter.Category);

				filter.Property = "ZZZ";
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, false), (comInvoice2, false), (comInvoice3, false));

				filter.Property = "";
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, true), (comInvoice3, true));

				var filter1 = (ModuleLocationFilter)filterBizObj[CommercialInvoiceFilterConstants.LoadDischarge];
				filter1.IsActive = true;
				filter1.Property1 = "AUSYD";
				filter1.Property2 = "USLAX";
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, false), (comInvoice3, false));

				filter1.Property1 = "AUSYD";
				filter1.Property2 = "USCHI";
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, false), (comInvoice2, false), (comInvoice3, false));

				filter1.Property1 = "AUMEL";
				filter1.Property2 = "USCHI";
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, false), (comInvoice2, true), (comInvoice3, false));

				filter1.Property1 = ZString.Empty;
				filter1.Property2 = ZString.Empty;
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, true), (comInvoice3, true));

				var dateFilter = (ModuleDateFilter)filterBizObj[CommercialInvoiceFilterConstants.ETA];
				dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				dateFilter.IsActive = true;
				dateFilter.Property1 = new ZDateTime(2009, 7, 29);
				dateFilter.Property2 = new ZDateTime(2009, 7, 29);
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, false), (comInvoice3, false));

				dateFilter.Property1 = ZDateTime.Empty;
				dateFilter.Property2 = ZDateTime.Empty;
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, true), (comInvoice3, true));

				var carrierFilter = (ModuleGuidFilter)filterBizObj[CommercialInvoiceFilterConstants.RoutingCarrier];
				carrierFilter.IsActive = true;
				carrierFilter.Property = org1.PK;
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, false), (comInvoice3, false));
			}
		}

		public void TestFiltersForLinkedRouting_StandaloneInvoice()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "Z1Z";
			company.GC_RN_NKCountryCode = "AU";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "Z1Z";

			var companyFilter = (ModuleGuidFilter)filterBizObj[CommercialInvoiceFilterConstants.Company];
			companyFilter.Property = company.PK;

			var voyage1 = CreateVoyage(TestVessel1.RV_Code, "1234", Core.Constants.TransportModes.Sea);
			var voyage2 = CreateVoyage(TestVessel2.RV_Code, "1234", Core.Constants.TransportModes.Sea);
			var voyage3 = CreateVoyage(TestVessel2.RV_Code, "1234", Core.Constants.TransportModes.Sea);

			var sailing1 = GetOrCreateSailing(voyage1, "AUSYD", "USLAX");
			var sailing2 = GetOrCreateSailing(voyage2, "AUSYD", "USLAX");
			var sailing3 = GetOrCreateSailing(voyage3, "AUMEL", "USCHI");

			var comInvoice1 = Factory.New<BaseJobComInvoiceHeader>();
			comInvoice1.JZ_GB = branch.PK;
			comInvoice1.JZ_InvoiceNumber = "INV1";
			comInvoice1.JZ_OH_Buyer = org1.PK;
			comInvoice1.JZ_OH_Supplier = org2.PK;

			var transport1 = comInvoice1.Transports.AddNew();
			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing1.PK;
			transport1.CarrierPK = org1.PK;

			var comInvoice2 = Factory.New<BaseJobComInvoiceHeader>();
			comInvoice2.JZ_GB = branch.PK;
			comInvoice2.JZ_InvoiceNumber = "INV2";
			comInvoice2.JZ_OH_Buyer = org2.PK;
			comInvoice2.JZ_OH_Supplier = org3.PK;

			var transport2 = comInvoice2.Transports.AddNew();
			transport2.JW_IsLinked = true;
			transport2.JW_JX = sailing2.PK;

			var address2 = org2.Addresses.AddNew();
			address2.OA_Address1 = "OA_Address1";
			transport2.JW_OA_CarrierAddress = address2.PK;

			var comInvoice3 = Factory.New<BaseJobComInvoiceHeader>();
			comInvoice3.JZ_GB = branch.PK;
			comInvoice3.JZ_InvoiceNumber = "INV3";
			comInvoice3.JZ_OH_Buyer = org2.PK;
			comInvoice3.JZ_OH_Supplier = org3.PK;

			var transport3 = comInvoice3.Transports.AddNew();
			transport3.JW_IsLinked = true;
			transport3.JW_JX = sailing3.PK;
			transport3.JW_ETA = ZDateTime.Today.AddDays(10);

			var address3 = org2.Addresses.AddNew();
			address3.OA_Address1 = "OA_Address1";
			transport3.JW_OA_CarrierAddress = address3.PK;

			Factory.Save();

			var filter = (ModuleNkFilter)filterBizObj[CommercialInvoiceFilterConstants.Vessel];
			filter.IsActive = true;
			filter.Property = "APL EMERALD";
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, false), (comInvoice3, false));

			filter.Property = "TASCO";
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, false), (comInvoice2, true), (comInvoice3, true));
			filter.Property = "";

			var filter1 = (ModuleLocationFilter)filterBizObj[CommercialInvoiceFilterConstants.LoadDischarge];
			filter1.IsActive = true;
			filter1.Property1 = "AUSYD";
			filter1.Property2 = "USLAX";
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, true), (comInvoice3, false));

			filter1.Property1 = "AUMEL";
			filter1.Property2 = ZString.Empty;
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, false), (comInvoice2, false), (comInvoice3, true));

			filter1.Property1 = ZString.Empty;
			filter1.Property2 = "USCHI";
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, false), (comInvoice2, false), (comInvoice3, true));

			filter1.Property1 = ZString.Empty;
			filter1.Property2 = ZString.Empty;
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, true), (comInvoice3, true));

			var dateFilter = (ModuleDateFilter)filterBizObj[CommercialInvoiceFilterConstants.ETA];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			dateFilter.Property1 = new ZDateTime(2010, 1, 1);
			dateFilter.Property2 = ZDateTime.Today;
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, true), (comInvoice3, false));

			dateFilter.Property1 = ZDateTime.Today.AddDays(5);
			dateFilter.Property2 = ZDateTime.Today.AddDays(15);
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, false), (comInvoice2, false), (comInvoice3, true));

			dateFilter.Property1 = ZDateTime.Empty;
			dateFilter.Property2 = ZDateTime.Empty;
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, true), (comInvoice3, true));

			var carrierFilter = (ModuleGuidFilter)filterBizObj[CommercialInvoiceFilterConstants.RoutingCarrier];
			carrierFilter.IsActive = true;
			carrierFilter.Property = org1.PK;
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, false), (comInvoice3, false));

			carrierFilter.Property = org2.PK;
			AssertQueryResults(filterBizObj.Filter, (comInvoice1, false), (comInvoice2, true), (comInvoice3, true));
		}

		public void TestFiltersForLinkedRouting_DeclarationInvoice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var company = Factory.New<GlbCompany>();
				company.GC_Code = "Z1Z";
				company.GC_RN_NKCountryCode = "AU";
				var branch = company.Branches.AddNew();
				branch.GB_Code = "Z1Z";

				var companyFilter = (ModuleGuidFilter)filterBizObj[CommercialInvoiceFilterConstants.Company];
				companyFilter.Property = company.PK;

				var voyage1 = CreateVoyage(TestVessel1.RV_Code, "1234", Core.Constants.TransportModes.Sea);
				var voyage2 = CreateVoyage(TestVessel2.RV_Code, "1234", Core.Constants.TransportModes.Sea);
				var voyage3 = CreateVoyage(TestVessel2.RV_Code, "1234", Core.Constants.TransportModes.Sea);

				var sailing1 = GetOrCreateSailing(voyage1, "AUSYD", "USLAX");
				var sailing2 = GetOrCreateSailing(voyage2, "AUSYD", "USLAX");
				var sailing3 = GetOrCreateSailing(voyage3, "AUMEL", "USCHI");

				var declaration1 = Factory.New<BaseJobDeclaration>();
				declaration1.JE_GB = branch.PK;

				var comInvoice1 = declaration1.Invoices.AddNew();
				comInvoice1.JZ_InvoiceNumber = "INV1";
				comInvoice1.JZ_OH_Buyer = org1.PK;
				comInvoice1.JZ_OH_Supplier = org2.PK;

				var transport1 = declaration1.Transports.AddNew();
				transport1.JW_IsLinked = true;
				transport1.JW_JX = sailing1.PK;
				transport1.CarrierPK = org1.PK;

				var declaration2 = Factory.New<BaseJobDeclaration>();
				declaration2.JE_GB = branch.PK;

				var comInvoice2 = declaration2.Invoices.AddNew();
				comInvoice2.JZ_InvoiceNumber = "INV2";
				comInvoice2.JZ_OH_Buyer = org2.PK;
				comInvoice2.JZ_OH_Supplier = org3.PK;

				var transport2 = declaration2.Transports.AddNew();
				transport2.JW_IsLinked = true;
				transport2.JW_JX = sailing2.PK;

				var address2 = org2.Addresses.AddNew();
				address2.OA_Address1 = "OA_Address1";
				transport2.JW_OA_CarrierAddress = address2.PK;

				var declaration3 = Factory.New<BaseJobDeclaration>();
				declaration3.JE_GB = branch.PK;

				var comInvoice3 = declaration3.Invoices.AddNew();
				comInvoice3.JZ_InvoiceNumber = "INV3";
				comInvoice3.JZ_OH_Buyer = org2.PK;
				comInvoice3.JZ_OH_Supplier = org3.PK;

				var transport3 = declaration3.Transports.AddNew();
				transport3.JW_IsLinked = true;
				transport3.JW_JX = sailing3.PK;
				transport3.JW_ETA = ZDateTime.Today.AddDays(10);

				var address3 = org2.Addresses.AddNew();
				address3.OA_Address1 = "OA_Address1";
				transport3.JW_OA_CarrierAddress = address3.PK;

				Factory.Save();

				var filter = (ModuleNkFilter)filterBizObj[CommercialInvoiceFilterConstants.Vessel];
				filter.IsActive = true;
				filter.Property = "APL EMERALD";
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, false), (comInvoice3, false));

				filter.Property = "TASCO";
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, false), (comInvoice2, true), (comInvoice3, true));
				filter.Property = "";

				var filter1 = (ModuleLocationFilter)filterBizObj[CommercialInvoiceFilterConstants.LoadDischarge];
				filter1.IsActive = true;
				filter1.Property1 = "AUSYD";
				filter1.Property2 = "USLAX";
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, true), (comInvoice3, false));

				filter1.Property1 = "AUMEL";
				filter1.Property2 = ZString.Empty;
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, false), (comInvoice2, false), (comInvoice3, true));

				filter1.Property1 = ZString.Empty;
				filter1.Property2 = "USCHI";
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, false), (comInvoice2, false), (comInvoice3, true));

				filter1.Property1 = ZString.Empty;
				filter1.Property2 = ZString.Empty;
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, true), (comInvoice3, true));

				var dateFilter = (ModuleDateFilter)filterBizObj[CommercialInvoiceFilterConstants.ETA];
				dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				dateFilter.IsActive = true;
				dateFilter.Property1 = new ZDateTime(2010, 1, 1);
				dateFilter.Property2 = ZDateTime.Today;
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, true), (comInvoice3, false));

				dateFilter.Property1 = ZDateTime.Today.AddDays(5);
				dateFilter.Property2 = ZDateTime.Today.AddDays(15);
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, false), (comInvoice2, false), (comInvoice3, true));

				dateFilter.Property1 = ZDateTime.Empty;
				dateFilter.Property2 = ZDateTime.Empty;
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, true), (comInvoice3, true));

				var carrierFilter = (ModuleGuidFilter)filterBizObj[CommercialInvoiceFilterConstants.RoutingCarrier];
				carrierFilter.IsActive = true;
				carrierFilter.Property = org1.PK;
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, true), (comInvoice2, false), (comInvoice3, false));

				carrierFilter.Property = org2.PK;
				AssertQueryResults(filterBizObj.Filter, (comInvoice1, false), (comInvoice2, true), (comInvoice3, true));
			}
		}

		JobVoyage CreateVoyage(ZString vessel, ZString voyageFlight, ZString transportMode)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = voyageFlight;
			voyage.JV_RV_NKVessel = vessel;
			voyage.JV_AirSeaRoad = transportMode;
			return voyage;
		}

		#region Vessels

		RefVessel TestVessel1
		{
			get
			{
				if (vessel1 == null)
				{
					vessel1 = LoadOrCreateVessel(TestVessel1Name, "7819369");
				}
				return vessel1;
			}
		}
		RefVessel vessel1;
		const string TestVessel1Name = "APL EMERALD";

		protected RefVessel TestVessel2
		{
			get
			{
				if (vessel2 == null)
				{
					vessel2 = LoadOrCreateVessel(TestVessel2Name, "8309581");
				}
				return vessel2;
			}
		}
		RefVessel vessel2;
		const string TestVessel2Name = "TASCO";

		RefVessel LoadOrCreateVessel(string name, string lloydsNumber)
		{
			var vessel = Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, name);
			if (vessel == null)
			{
				vessel = Factory.New<RefVessel>();
				vessel.RV_LloydsNumber = lloydsNumber;
				vessel.RV_Code = name;
			}

			return vessel;
		}

		#endregion

		JobSailing GetOrCreateSailing(JobVoyage voyage, ZString load, ZString disc)
		{
			JobSailing sailing = voyage.Sailings.GetSailingFromLoadAndDischarge(load, disc);

			if (sailing == null)
			{
				VoyageOrigin origin = voyage.Origins.GetOriginFromLoading(load);
				if (origin == null)
				{
					origin = voyage.Origins.AddNew();
					origin.JA_RL_NKPortOfLoading = load;
				}

				VoyageDestination destination = voyage.Destinations.GetDestinationFromDischarge(disc);
				if (destination == null)
				{
					destination = voyage.Destinations.AddNew();
					destination.JB_RL_NKPortOfDischarge = disc;
				}
				destination.JB_E_ARV = ZDateTime.Today;

				voyage.GenerateSailings();
				sailing = voyage.Sailings.GetSailingFromLoadAndDischarge(load, disc);
			}

			return sailing;
		}

		#endregion

		#region Implementation

		CommercialInvoiceFilterBusinessObject filterBizObj;
		OrgHeader org1;
		OrgHeader org2;
		OrgHeader org3;

		BaseJobComInvoiceHeader standaloneInvoice1;
		BaseJobComInvoiceHeader standaloneInvoice2;
		BaseJobComInvoiceHeader standaloneInvoice3;
		BaseJobComInvoiceHeader standaloneInvoice4;
		BaseJobComInvoiceHeader declarationInvoice1;
		BaseJobComInvoiceHeader declarationInvoice2;
		BaseJobComInvoiceHeader declarationInvoice3;
		BaseJobComInvoiceHeader declarationInvoice4;
		BaseJobComInvoiceHeader declarationInvoice5;

		protected override void SetUp()
		{
			base.SetUp();
			filterBizObj = (CommercialInvoiceFilterBusinessObject)GetNewFilterStripBusinessObject();
			DisableAttachedToDeclarationFilter(filterBizObj);
			SetupCommercialInvoices();
			SetupCurrentCompanyCountry();
			Factory.Save();
		}

		protected static void DisableAttachedToDeclarationFilter(CommercialInvoiceFilterBusinessObject bizObj)
		{
			((ModuleTextFilter)bizObj[CommercialInvoiceFilterConstants.AttachedToDeclaration]).Property = AttachedToDeclarationFilterOptions.Codes.All;
		}

		void SetupCurrentCompanyCountry()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			((IBusinessObjectInternals)currentCompany).Row["GC_RN_NKCountryCode"] = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			currentCompany.HasChanges = true;
		}

		void SetupCommercialInvoices()
		{
			org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "AA";
			org1.OH_Code = "AAXX";
			org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "AB";
			org2.OH_Code = "ABXX";
			org3 = Factory.New<OrgHeader>();
			org3.OH_FullName = "AC";
			org3.OH_Code = "ACXX";

			standaloneInvoice1 = Factory.New<BaseJobComInvoiceHeader>();
			standaloneInvoice1.JZ_InvoiceNumber = "ST-INV-01";
			standaloneInvoice2 = Factory.New<BaseJobComInvoiceHeader>();
			standaloneInvoice2.JZ_InvoiceNumber = "ST-INV-02";
			standaloneInvoice3 = Factory.New<BaseJobComInvoiceHeader>();
			standaloneInvoice3.JZ_InvoiceNumber = "ST-INV-03";
			standaloneInvoice4 = Factory.New<BaseJobComInvoiceHeader>();
			standaloneInvoice4.JZ_InvoiceNumber = "ST-INV-04";

			declarationInvoice1 = Factory.New<BaseJobDeclaration>().Invoices.AddNew();
			declarationInvoice1.JZ_InvoiceNumber = "JD-INV-01";
			declarationInvoice2 = Factory.New<BaseJobDeclaration>().Invoices.AddNew();
			declarationInvoice2.JZ_InvoiceNumber = "JD-INV-02";
			declarationInvoice3 = Factory.New<BaseJobDeclaration>().Invoices.AddNew();
			declarationInvoice3.JZ_InvoiceNumber = "JD-INV-03";
			declarationInvoice4 = Factory.New<BaseJobDeclaration>().Invoices.AddNew();
			declarationInvoice4.JZ_InvoiceNumber = "JD-INV-04";
			declarationInvoice5 = Factory.New<BaseJobDeclaration>().Invoices.AddNew();
			declarationInvoice5.JZ_InvoiceNumber = "JD-INV-05";
		}

		#endregion

		#region Test Workflow Filters

		public void TestWorkflowFilters()
		{
			CommercialInvoiceFilterBusinessObject testFilterStrip = (CommercialInvoiceFilterBusinessObject)GetNewBusinessObject();
			AssertNotNull("Filterstrip contains workflow filters", testFilterStrip["Milestone Date"]);
			AssertNotNull("Filterstrip contains workflow filters", testFilterStrip["Milestone Completed"]);
			AssertNotNull("Filterstrip contains workflow filters", testFilterStrip["Next Milestone"]);
			AssertNotNull("Filterstrip contains workflow filters", testFilterStrip["Last Completed Milestone"]);
		}

		#endregion

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = new List<Tuple<string, string>>();

			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, CommercialInvoiceFilterConstants.ImporterName));
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, CommercialInvoiceFilterConstants.SupplierName));

			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, CommercialInvoiceFilterConstants.LineOrderNumber));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "Part Attribute 1"));

			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, CommercialInvoiceFilterConstants.LineOrderNumber));
			result.AddRange(GetFiltersExcludedFromSubgroupCheck());
			return result;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var result = new List<Tuple<string, string>>();

			// Justification: the reference filters used two columns, J2_Reference and J2_ReferenceType. Combining multiple fiters into a single zquery clause will give incorrect results. 
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, CommercialInvoiceFilterConstants.References));
			result.Add(TableFilter(JobComInvoiceHeaderRefsSchema.Constants.TableName, CommercialInvoiceFilterConstants.References));

			result.Add(TableFilter(JobConsolSchema.Constants.TableName, CommercialInvoiceFilterConstants.Vessel));
			result.Add(TableFilter(JobSailingSchema.Constants.TableName, CommercialInvoiceFilterConstants.Vessel));
			result.Add(TableFilter(JobVoyageSchema.Constants.TableName, CommercialInvoiceFilterConstants.Vessel));
			result.Add(TableFilter(JobVoyDestinationSchema.Constants.TableName, CommercialInvoiceFilterConstants.Vessel));
			result.Add(TableFilter(JobConsolTransportSchema.Constants.TableName, CommercialInvoiceFilterConstants.Vessel));

			result.Add(TableFilter(OrgAddressSchema.Constants.TableName, CommercialInvoiceFilterConstants.RoutingCarrier));
			result.Add(TableFilter(JobSailingSchema.Constants.TableName, CommercialInvoiceFilterConstants.RoutingCarrier));
			result.Add(TableFilter(JobVoyageSchema.Constants.TableName, CommercialInvoiceFilterConstants.RoutingCarrier));
			result.Add(TableFilter(JobVoyOriginSchema.Constants.TableName, CommercialInvoiceFilterConstants.RoutingCarrier));
			result.Add(TableFilter(JobConsolTransportSchema.Constants.TableName, CommercialInvoiceFilterConstants.RoutingCarrier));

			result.Add(TableFilter(JobSailingSchema.Constants.TableName, CommercialInvoiceFilterConstants.LoadDischarge));
			result.Add(TableFilter(JobVoyOriginSchema.Constants.TableName, CommercialInvoiceFilterConstants.LoadDischarge));
			result.Add(TableFilter(JobConsolTransportSchema.Constants.TableName, CommercialInvoiceFilterConstants.LoadDischarge));

			return result;
		}
	}
}
