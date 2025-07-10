using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(StatementFilterStripBusinessObject))]
	sealed class StatementFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestEntryNumberQuery()
		{
			var statement = Factory.New<CusStatementHeader>();
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "123";
			var statement2 = Factory.New<CusStatementHeader>();
			var statementLine2 = statement2.StatementLines.AddNew();
			statementLine2.B3_EntryNum = "234";
			Factory.Save();
			var filter = new StatementFilterStripBusinessObject();
			var entryNumberFilter = (ModuleTextFilter)filter[StatementFilterStripBusinessObject.Schema.EntryNumber];
			entryNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			entryNumberFilter.Property = "123";
			entryNumberFilter.IsActive = true;
			var coll = new StatementCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(statement, coll[0]);
		}

		public void TestShowOnlyCurrentCompanyStatements()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var statement = Factory.New<CusStatementHeader>();
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_GC = company.PK;
			Factory.Save();
			var filterBizO = new StatementFilterStripBusinessObject();
			AssertEquals(true, statement.MatchesFilter(filterBizO.Filter));
			AssertEquals(false, statement2.MatchesFilter(filterBizO.Filter));
		}

		public void TestActionAuthorizedFilter()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.AddAuthorisationLog();
			statement.AddAuthorisationLog();
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.AddAuthorisationLog();
			var statement3 = Factory.New<CusStatementHeader>();
			Factory.Save();
			var filter = new StatementFilterStripBusinessObject();
			var actionFilter = (ModuleTextFilter)filter[StatementFilterStripBusinessObject.Schema.ActionAuthorized];
			actionFilter.Property = "All";
			actionFilter.IsActive = true;
			var coll = new StatementCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(3, coll.Count);
			actionFilter.Property = "Granted";
			coll = new StatementCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(statement2, coll[0]);
		}

		public void TestStatementTypeFilter()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_IsMonthlyStatement = true;
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_IsMonthlyStatement = false;
			var statement3 = Factory.New<CusStatementHeader>();
			Factory.Save();
			var filter = new StatementFilterStripBusinessObject();
			var statementFilter = (ModuleTextFilter)filter[StatementFilterStripBusinessObject.Schema.StatementType];
			statementFilter.Property = "Daily";
			statementFilter.IsActive = true;
			var coll = new StatementCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(2, coll.Count);
			statementFilter.Property = "Monthly";
			coll = new StatementCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
		}

		public void TestFilters()
		{
			var filter = new StatementFilterStripBusinessObject();
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.ProcessDate]);
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.PrintDate]);
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.DueDate]);
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.StatementNumber]);
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.ImporterCustomsID]);
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.StatementProcessPort]);
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.StatementStatus]);
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.StatementEntryFilerCode]);
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.EntryNumber]);
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.PaymentStatus]);
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.PaymentType]);
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.Importer]);
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.PayerUnitNumber]);
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.StatementAmount]);
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.ActionAuthorized]);
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.StatementType]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new StatementFilterStripBusinessObject();
	}
}
