using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.MasterFiles.Business.OrgSupplierPart;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(EntryLineFilterBusinessObject))]
	public class EntryLineFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestPartAttrib()
		{
			invLine1.JI_PartAttrib1 = "123";
			invLine2.JI_PartAttrib1 = "124";
			invLine3.JI_PartAttrib1 = "XXX";
			invLine4.JI_PartAttrib1 = "YYY";
			var partAttribModule = (ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.PartAttribute1];
			Factory.Save();
			partAttribModule.IsActive = true;
			partAttribModule.Property = "12";
			partAttribModule.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filterCollection.Load(filterBO.Filter);
			AssertEquals(2, filterCollection.Count);
			partAttribModule.Property = "123";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);
			AssertEquals(line1, filterCollection[0]);
		}

		public void TestImporterDefault()
		{
			ModuleFilterCollection modulFilters = filterBO.ModuleFilters;
			AssertEquals(claimant, filterBO.Lookups.Importer);
		}

		public void TestEntryNumberFilter()
		{
			ModuleTextFilter entryNumberQuery = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.EntryNumber];
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(4, filterCollection.Count);
			line2.CL_AdValoremTariff = ZString.Empty;
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(3, filterCollection.Count);
			entryNumberQuery.IsActive = true;
			entryNumberQuery.Property = "ENTRY1";
			entryNumberQuery.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);
			entryNumberQuery.Property = "ENTRY";
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(3, filterCollection.Count);
			RefCountry otherCountry = Factory.New<RefCountry>();
			otherCountry.RN_Code = "XY";
			entryNumber1.CE_RN_NKCountryCode = otherCountry.Code;
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(2, filterCollection.Count);
			line4.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.DeletePending;
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);
			AssertEquals(line3, filterCollection[0]);
		}

		public void TestImporterFilter()
		{
			ModuleGuidFilter importerQuery = (ModuleGuidFilter)filterBO[DeclarationFilterConstants.OrgFilterTypes.Importer];
			importerQuery.IsActive = true;
			importerQuery.Property = importer1.PK;
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(4, filterCollection.Count);
			declaration1.JE_OH_Importer = importer2.PK;
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(2, filterCollection.Count);
		}

		public void TestLineNumberFilter()
		{
			ModuleNumberFilter lineNumberQuery = (ModuleNumberFilter)filterBO["Line Number"];
			lineNumberQuery.IsActive = true;
			lineNumberQuery.SqlComparisonOperator = SQLComparisonOperator.Equal;
			lineNumberQuery.Property = "1";
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);
			lineNumberQuery.SqlComparisonOperator = SQLComparisonOperator.Equal;
			lineNumberQuery.Property = "2";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(2, filterCollection.Count);
		}

		public void TestProductFilter()
		{
			ModuleGuidFilter productQuery = (ModuleGuidFilter)filterBO["Product"];
			productQuery.IsActive = true;
			productQuery.Property = part1.PK;
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);
			productQuery.Property = part2.PK;
			filterCollection.Load(filterBO.Filter);
			AssertEquals(2, filterCollection.Count);
		}

		public void TestTariffNumberFilter()
		{
			ModuleTextFilter tariffNumberQuery = (ModuleTextFilter)filterBO["Tariff Number"];
			tariffNumberQuery.IsActive = true;
			tariffNumberQuery.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			tariffNumberQuery.Property = "1111.11";
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(2, filterCollection.Count);
			tariffNumberQuery.Property = "2";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);
		}

		public void TestClassificationFilter()
		{
			ModuleGuidFilter classificationQuery = (ModuleGuidFilter)filterBO["Import Classification"];
			classificationQuery.IsActive = true;
			classificationQuery.Property = class1.PK;
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);
			classificationQuery.Property = class2.PK;
			filterCollection.Load(filterBO.Filter);
			AssertEquals(2, filterCollection.Count);
		}

		public void TestInvoiceDateFilter()
		{
			ModuleDateFilter invoiceDateQuery = (ModuleDateFilter)filterBO[DeclarationFilterConstants.DateFilterTypes.CommercialInvoiceDate];
			invoiceDateQuery.IsActive = true;
			invoiceDateQuery.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			invoiceDateQuery.Property1 = createDate.AddDays(-10);
			invoiceDateQuery.Property2 = createDate.AddDays(-3);
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			var message = string.Format("Four BOs with specific datetime:{0}.", string.Join(",", filterCollection.Cast<CusEntryLine>().Select(x => x.InvoiceLines[0].InvoiceHeader.JZ_InvoiceDate)));
			AssertEquals(message + string.Format("Well, in range {0} to {1}, we will get 4 BOs.", invoiceDateQuery.Property1, invoiceDateQuery.Property2), 4, filterCollection.Count);
			invoiceDateQuery.Property1 = createDate.AddDays(-5);
			filterCollection.Load(filterBO.Filter);
			AssertEquals(message + string.Format("Well, in range {0} to {1}, we will get 2 BOs.", invoiceDateQuery.Property1, invoiceDateQuery.Property2), 2, filterCollection.Count);
		}

		public virtual void TestFirstArrivalDateFilter()
		{
			ModuleDateFilter testArrivalDateFilter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.DateFilterTypes.FirstArrival];
			testArrivalDateFilter.IsActive = true;
			testArrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			testArrivalDateFilter.Property1 = createDate.AddDays(-10);
			testArrivalDateFilter.Property2 = createDate.AddDays(-3);
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			var message = string.Format("Four BOs with specific datetime:{0}.", string.Join(",", filterCollection.Cast<CusEntryLine>().Select(x => x.Declaration.JE_DateOfFirstArrival)));
			AssertEquals(message + string.Format("Well, in range {0} to {1}, we will get 4 BOs.", testArrivalDateFilter.Property1, testArrivalDateFilter.Property2), 4, filterCollection.Count);
			testArrivalDateFilter.Property1 = createDate.AddDays(-5);
			filterCollection.Load(filterBO.Filter);
			AssertEquals(message + string.Format("Well, in range {0} to {1}, we will get 2 BOs.", testArrivalDateFilter.Property1, testArrivalDateFilter.Property2), 2, filterCollection.Count);
		}

		public virtual void TestFirstArrivalDateFilterAdded()
		{
			AssertNotNull(filterBO[DeclarationFilterConstants.DateFilterTypes.FirstArrival]);
		}

		public void TestArrivalDateFilter()
		{
			ModuleDateFilter testArrivalDateFilter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.DateFilterTypes.DateOfArrival];
			testArrivalDateFilter.IsActive = true;
			testArrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			testArrivalDateFilter.Property1 = createDate.AddDays(-10);
			testArrivalDateFilter.Property2 = createDate.AddDays(-3);
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			var message = string.Format("Four BOs with specific datetime:{0}.", string.Join(",", filterCollection.Cast<CusEntryLine>().Select(x => x.Declaration.JE_DateOfArrival)));
			AssertEquals(message + string.Format("Well, in range {0} to {1}, we will get 4 BOs.", testArrivalDateFilter.Property1, testArrivalDateFilter.Property2), 4, filterCollection.Count);
			testArrivalDateFilter.Property1 = createDate.AddDays(-5);
			filterCollection.Load(filterBO.Filter);
			AssertEquals(message + string.Format("Well, in range {0} to {1}, we will get 2 BOs.", testArrivalDateFilter.Property1, testArrivalDateFilter.Property2), 2, filterCollection.Count);
		}

		public void TestCreatedDateFilter()
		{
			var testCreatedDateFilter = (ModuleDateFilter)filterBO["Created"];
			testCreatedDateFilter.IsActive = true;
			testCreatedDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			testCreatedDateFilter.Property1 = createDate.AddDays(-10);
			testCreatedDateFilter.Property2 = createDate.AddDays(-3);
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			var message = string.Format("Four BOs with specific datetime:{0}.", string.Join(",", filterCollection.Cast<CusEntryLine>().Select(x => x.Declaration.JE_SystemCreateTimeUtc)));
			AssertEquals(message + string.Format("Well, in range {0} to {1}, we will get 4 BOs.", testCreatedDateFilter.Property1, testCreatedDateFilter.Property2), 4, filterCollection.Count);
			testCreatedDateFilter.Property1 = createDate.AddDays(-5);
			filterCollection.Load(filterBO.Filter);
			AssertEquals(message + string.Format("Well, in range {0} to {1}, we will get 2 BOs.", testCreatedDateFilter.Property1, testCreatedDateFilter.Property2), 2, filterCollection.Count);
		}

		public void TestOrderNumberFilter()
		{
			var testOrderNumberFilter = (ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef];
			testOrderNumberFilter.IsActive = true;
			testOrderNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			testOrderNumberFilter.Property = "OREF1";
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(2, filterCollection.Count);
			testOrderNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			testOrderNumberFilter.Property = "OREF3";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(0, filterCollection.Count);
			testOrderNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			testOrderNumberFilter.Property = "OREF";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(4, filterCollection.Count);
		}

		public void TestJobNumberFilter()
		{
			var testOrderNumberFilter = (ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.DeclarationReference];
			testOrderNumberFilter.IsActive = true;
			testOrderNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			testOrderNumberFilter.Property = "JOB1";
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(2, filterCollection.Count);
			testOrderNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			testOrderNumberFilter.Property = "JOB3";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(0, filterCollection.Count);
			testOrderNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			testOrderNumberFilter.Property = "JOB";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(4, filterCollection.Count);
		}

		#region Implementation
		protected EntryLineFilterBusinessObject filterBO;
		protected GlobalCusEntryLineCollection filterCollection;
		protected BaseJobDeclaration declaration1;
		protected BaseJobDeclaration declaration2;
		protected BaseJobDeclaration drawbackDeclaration;
		protected CusEntryHeader entryHeader1;
		protected CusEntryHeader entryHeader2;
		protected CusEntryNumber entryNumber1;
		protected CusEntryNumber entryNumber2;
		protected CusEntryLine line1;
		protected CusEntryLine line2;
		protected CusEntryLine line3;
		protected CusEntryLine line4;
		protected OrgHeader importer1;
		protected OrgHeader importer2;
		protected OrgHeader claimant;
		protected BaseJobComInvoiceLine invLine1;
		protected BaseJobComInvoiceLine invLine2;
		protected BaseJobComInvoiceLine invLine3;
		protected BaseJobComInvoiceLine invLine4;
		protected OrgSupplierPart part1;
		protected OrgSupplierPart part2;
		protected BaseCusClassification class1;
		protected BaseCusClassification class2;
		protected BaseJobComInvoiceHeader invHeader1;
		protected BaseJobComInvoiceHeader invHeader2;
		protected ZDateTime createDate;
		protected override void SetUp()
		{
			base.SetUp();
			createDate = new DateTime(2017, 10, 10, 10, 10, 10);
			var country = GlbCompany.CurrentCompany.Country;
			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = country.Code;
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_RL_NKHomePort = country.Code + "XYZ";
			claimant = Factory.New<OrgHeader>();
			claimant.OH_Code = "Claimant";
			drawbackDeclaration = Factory.New<BaseJobDeclaration>();
			drawbackDeclaration.JE_OH_Importer = claimant.PK;
			filterCollection = new GlobalCusEntryLineCollection(Factory, drawbackDeclaration);
			filterBO = (EntryLineFilterBusinessObject)GetNewFilterStripBusinessObject();
			importer1 = Factory.New<OrgHeader>();
			importer1.OH_Code = "IMPORTER1";
			importer2 = Factory.New<OrgHeader>();
			importer2.OH_Code = "IMPORTER2";
			part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PART1";
			part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PART2";
			class1 = Factory.New<BaseCusClassification>();
			class1.CC_LookupCode = "CLASS1";
			class1.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			class2 = Factory.New<BaseCusClassification>();
			class2.CC_LookupCode = "CLASS2";
			class2.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_GB = newBranch.PK;
			declaration1.JE_OH_Importer = importer1.PK;
			declaration1.JE_DateOfFirstArrival = createDate.AddDays(-4);
			declaration1.JE_DateOfArrival = createDate.AddDays(-4);
			var createDate1 = Env.Time.GetUtcFromLocalTime(createDate.ToDateTime());
			declaration1.JE_SystemCreateTimeUtc = createDate1.AddDays(-4);
			declaration1.JE_OwnerRef = "OREF1";
			declaration1.JE_DeclarationReference = "JOB1";
			entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			entryNumber1 = Factory.New<CusEntryNumber>();
			entryNumber1.CE_ParentID = entryHeader1.PK;
			entryNumber1.CE_ParentTable = entryHeader1.TableName;
			entryNumber1.CE_EntryNum = "ENTRY1";
			entryNumber1.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			invHeader1 = declaration1.Invoices.AddNew();
			invHeader1.JZ_InvoiceDate = createDate.AddDays(-4);
			line1 = entryHeader1.MergedLines.AddNew();
			line1.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			line1.CL_AdValoremTariff = "1111.11.11";
			line1.CL_LineNumber = 1;
			invLine1 = invHeader1.JobComInvoiceLines.AddNew();
			invLine1.JI_CL = line1.PK;
			invLine1.JI_CC = class2.PK;
			line2 = entryHeader1.MergedLines.AddNew();
			line2.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			line2.CL_AdValoremTariff = "2222.22.22";
			line2.CL_LineNumber = 2;
			invLine2 = invHeader1.JobComInvoiceLines.AddNew();
			invLine2.JI_CL = line2.PK;
			invLine2.JI_OP = part2.PK;
			invLine2.JI_CC = class2.PK;
			declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_GB = newBranch.PK;
			declaration2.JE_OH_Importer = importer1.PK;
			declaration2.JE_DateOfFirstArrival = createDate.AddDays(-6);
			declaration2.JE_DateOfArrival = createDate.AddDays(-6);
			var createDate2 = Env.Time.GetUtcFromLocalTime(createDate.ToDateTime());
			declaration2.JE_SystemCreateTimeUtc = createDate2.AddDays(-6);
			declaration2.JE_OwnerRef = "OREF2";
			declaration2.JE_DeclarationReference = "JOB2";
			entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryNumber2 = Factory.New<CusEntryNumber>();
			entryNumber2.CE_ParentID = entryHeader2.PK;
			entryNumber2.CE_ParentTable = entryHeader2.TableName;
			entryNumber2.CE_EntryNum = "ENTRY2";
			entryNumber2.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			invHeader2 = declaration2.Invoices.AddNew();
			invHeader2.JZ_InvoiceDate = createDate.AddDays(-6);
			line3 = entryHeader2.MergedLines.AddNew();
			line3.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			line3.CL_AdValoremTariff = "1111.11.11";
			line3.CL_LineNumber = 2;
			invLine3 = invHeader2.JobComInvoiceLines.AddNew();
			invLine3.JI_CL = line3.PK;
			invLine3.JI_OP = part2.PK;
			line4 = entryHeader2.MergedLines.AddNew();
			line4.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			line4.CL_AdValoremTariff = "3333.33.33";
			line4.CL_LineNumber = 3;
			invLine4 = invHeader2.JobComInvoiceLines.AddNew();
			invLine4.JI_CL = line4.PK;
			invLine4.JI_OP = part1.PK;
			invLine4.JI_CC = class1.PK;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EntryLineFilterBusinessObject(filterCollection);
		}

		#endregion
		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var exclusions = new List<Tuple<string, string>>();
			// This is a hidden filter, it cannot be applied more than once
			exclusions.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, DeclarationFilterConstants.Country));
			exclusions.Add(TableFilter(JobDeclarationSchema.Constants.TableName, DeclarationFilterConstants.Country));
			// Not created using ZQuery but with raw SQL
			exclusions.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef));
			exclusions.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef));
			exclusions.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef));
			exclusions.Add(TableFilter(JobDeclarationSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef));
			exclusions.Add(TableFilter(JobDocsAndCartageSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef));
			exclusions.Add(TableFilter(JobOrderHeaderSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef));
			exclusions.Add(TableFilter(JobOrderItemSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef));
			// Trying to implement sub groups for this causes a world of pain
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "Import Classification"));
			return exclusions;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var exclusions = new List<Tuple<string, string>>();
			exclusions.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef));
			exclusions.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.DeclarationReference));
			exclusions.Add(TableFilter(JobDeclarationSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef));
			exclusions.Add(TableFilter(JobDeclarationSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.DeclarationReference));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "Import Classification"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.PartAttribute1));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.PartAttribute2));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.PartAttribute3));
			return exclusions;
		}
	}
}
