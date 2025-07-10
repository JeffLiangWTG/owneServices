using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(EntryHeaderFilterBusinessObject))]
	sealed class EntryHeaderFilterBusinessObjectTest : Customs.Module.Testing.EntryHeaderFilterBusinessObjectAbstractTest
	{
		public void TestEntryNumberFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var entryNumberFilter = (ModuleNumberFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.EntryNumber];
			AssertEquals("Entry Number", entryNumberFilter.Description);
			AssertEquals("Entry Number (MRN)", entryNumberFilter.LocalizedDescription);
		}

		public void TestReferenceNumberFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var referenceNumberFilter = (ModuleNumberFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.ReferenceNumber];
			AssertEquals("Reference Number", referenceNumberFilter.Description);
			AssertEquals("Reference Number (LRN)", referenceNumberFilter.LocalizedDescription);
		}

		public void TestMessageStatusList()
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var list = filterObj.Lookups.MessageStatusList();
			var expectedList = new Common.ZA.ZAMessageStatusList();
			AssertEquals(expectedList.Count, list.Count);
			expectedList.RemoveCode(Common.ZA.ZAMessageStatusList.Codes.NotSent);
			AssertEquals(Common.ZA.ZAMessageStatusList.Descriptions.NotSent, list.GetDescriptionFromCode(DeclarationFilterConstants.EntryStatus.NotSentForFilter));
			foreach (ICodeDescription pair in expectedList)
			{
				AssertEquals(pair.Code, pair.Description, list.GetDescriptionFromCode(pair.Code));
			}
		}

		public void TestModuleFiltersAreAdded()
		{
			var moduleFilters = new List<ZString>()
			{
				DeclarationFilterConstants.CPCAndPPC,
				DeclarationFilterConstants.DateFilterTypes.AcquitByDate,
				DeclarationFilterConstants.DateFilterTypes.AcquittedDate
			};
			var filterObj = GetNewFilterStripBusinessObject();
			foreach (var filter in moduleFilters)
			{
				AssertNotNull(filterObj[filter]);
			}
		}

		public void TestAcquittedDate()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_BondAcquittedDate = new ZDate(2019, 7, 1);
			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_BondAcquittedDate = new ZDate(2015, 7, 1);
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject().ModuleFilters;
			var aquittedDateFilter = (ModuleDateFilter)filterObj[DeclarationFilterConstants.DateFilterTypes.AcquittedDate];
			aquittedDateFilter.IsActive = true;
			aquittedDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			aquittedDateFilter.Property1 = new ZDateTime(2015, 06, 1);
			aquittedDateFilter.Property2 = new ZDateTime(2015, 08, 1);
			var query = filterObj.GetFilterQuery(new ModuleFilter[] { aquittedDateFilter });
			AssertEquals(true, entry2.MatchesFilter(query));
			AssertEquals(false, entry1.MatchesFilter(query));
			aquittedDateFilter.Property1 = new ZDateTime(2019, 06, 1);
			aquittedDateFilter.Property2 = new ZDateTime(2019, 08, 1);
			query = filterObj.GetFilterQuery(new ModuleFilter[] { aquittedDateFilter });
			AssertEquals(false, entry2.MatchesFilter(query));
			AssertEquals(true, entry1.MatchesFilter(query));
		}

		public void TestValidToDate()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_BondValidToDate = new ZDate(2019, 7, 1);
			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_BondValidToDate = new ZDate(2010, 7, 1);
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject().ModuleFilters;
			var acquitByDateFilter = (ModuleDateFilter)filterObj[DeclarationFilterConstants.DateFilterTypes.AcquitByDate];
			acquitByDateFilter.IsActive = true;
			acquitByDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			acquitByDateFilter.Property1 = new ZDateTime(2019, 06, 1);
			acquitByDateFilter.Property2 = new ZDateTime(2019, 08, 1);
			var query = filterObj.GetFilterQuery(new ModuleFilter[] { acquitByDateFilter });
			AssertEquals(true, entry1.MatchesFilter(query));
			AssertEquals(false, entry2.MatchesFilter(query));
			acquitByDateFilter.Property1 = new ZDateTime(2010, 06, 1);
			acquitByDateFilter.Property2 = new ZDateTime(2010, 08, 1);
			query = filterObj.GetFilterQuery(new ModuleFilter[] { acquitByDateFilter });
			AssertEquals(false, entry1.MatchesFilter(query));
			AssertEquals(true, entry2.MatchesFilter(query));
		}

		public void TestGetCPCAndPPCQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var instruction1 = declaration1.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entry1.CH_CEI_Instruction = instruction1.PK;
			instruction1.CEI_Style = ProcedureCodes._11;
			var invoice1 = declaration1.Invoices.AddNew();
			var line1 = invoice1.InvoiceLines.AddNew();
			line1.JI_CEI = instruction1.PK;
			line1.JI_Procedure = line1.EntryInstruction.CEI_Style + ProcedureCodes._20;
			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			var instruction2 = declaration2.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entry2.CH_CEI_Instruction = instruction2.PK;
			instruction2.CEI_Style = ProcedureCodes._12;
			var invoice2 = declaration2.Invoices.AddNew();
			var line2 = invoice2.InvoiceLines.AddNew();
			line2.JI_CEI = instruction2.PK;
			line2.JI_Procedure = line2.EntryInstruction.CEI_Style + ProcedureCodes._20;
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ProcedureCodesModuleFilter)filterObj[DeclarationFilterConstants.CPCAndPPC];
			filter.Property1 = ProcedureCodes._11;
			filter.Property2 = ProcedureCodes._20;
			filter.IsActive = true;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			filter.Property1 = ZString.Empty;
			filter.Property2 = ProcedureCodes._20;
			filter.IsActive = true;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			filter.Property1 = ProcedureCodes._12;
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
		}

		public void TestUCRNumberFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumber1 = Factory.New<CusEntryNumber>();
			cusEntryNumber1.CE_ParentID = cusEntryHeader.PK;
			cusEntryNumber1.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber1.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			cusEntryNumber1.CE_EntryNum = "test1";
			var cusEntryNumber2 = Factory.New<CusEntryNumber>();
			cusEntryNumber2.CE_ParentID = cusEntryHeader.PK;
			cusEntryNumber2.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber2.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			cusEntryNumber2.CE_EntryNum = "test2";
			var cusEntryNumber3 = Factory.New<CusEntryNumber>();
			cusEntryNumber3.CE_ParentID = cusEntryHeader.PK;
			cusEntryNumber3.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber3.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber3.CE_EntryNum = "test3";
			var cusEntryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumber4 = Factory.New<CusEntryNumber>();
			cusEntryNumber4.CE_ParentID = cusEntryHeader1.PK;
			cusEntryNumber4.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber4.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber4.CE_EntryNum = "test4";
			var cusEntryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumber5 = Factory.New<CusEntryNumber>();
			cusEntryNumber5.CE_ParentID = cusEntryHeader2.PK;
			cusEntryNumber5.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber5.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			cusEntryNumber5.CE_EntryNum = "test5";
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleNumberFilter)filterObj[DeclarationFilterConstants.UniqueConsignmentReference];
			filter.Property = "test1";
			filter.IsActive = true;
			Assert(cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "test2";
			filter.IsActive = true;
			Assert(cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "test3";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "test4";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "test5";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.Property = "test1";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "test1";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = "test1";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.IsActive = true;
			Assert(cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestRelPrintIndFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_RelPrintInd = "";
			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_RelPrintInd = "Y";
			var declaration3 = Factory.New<JobDeclaration>();
			var entry3 = declaration3.CustomsEntryHeaders.AddNew();
			entry3.CH_RelPrintInd = "N";
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var printFilter = (ModuleTextFilter)filterObj[DeclarationFilterConstants.StatusFilterTypes.ReleasePrinterIndicator];
			printFilter.Property = "";
			printFilter.IsActive = true;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3.MatchesFilter(filterObj.Filter));
			printFilter.Property = "Y";
			printFilter.IsActive = true;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(!entry3.MatchesFilter(filterObj.Filter));
			printFilter.Property = "N";
			printFilter.IsActive = true;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3.MatchesFilter(filterObj.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryHeaderFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			try
			{
				//GlbCompany.CurrentCompany.SetCountry changes GC_RN_NKCountryCode, and needs to be saved to db as JobDeclarationFilter(DBOnlyQuery) is performed in FilterObject.
				GlbCompany.CurrentCompany.Factory.Save();
			}
			finally
			{
				((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = false;
			}
		}
	}
}
