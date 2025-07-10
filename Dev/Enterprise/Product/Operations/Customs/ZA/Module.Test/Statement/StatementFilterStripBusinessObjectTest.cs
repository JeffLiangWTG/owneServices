using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(CusStatementFilterStripBusinessObject))]
	sealed class StatementFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestLRNQuery()
		{
			var statement = Factory.New<CusStatementHeader>();
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "123";
			var lineCharge = statementLine.Charges.AddNew();
			var statement2 = Factory.New<CusStatementHeader>();
			var statementLine2 = statement2.StatementLines.AddNew();
			statementLine2.B3_EntryNum = "234";
			statementLine2.Charges.AddNew();
			Factory.Save();
			CusStatementFilterStripBusinessObject filter = new CusStatementFilterStripBusinessObject();
			ModuleTextFilter lrnFilter = (ModuleTextFilter)filter[CusStatementFilterStripBusinessObject.Schema.LRN];
			lrnFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			lrnFilter.Property = "123";
			lrnFilter.IsActive = true;
			CusStatementLineChargeCollection coll = new CusStatementLineChargeCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(lineCharge, coll[0]);
		}

		public void TestFANQuery()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_AccountNo = "123";
			var statementLine = statement.StatementLines.AddNew();
			var lineCharge = statementLine.Charges.AddNew();
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_AccountNo = "456";
			var statementLine2 = statement2.StatementLines.AddNew();
			statementLine2.Charges.AddNew();
			Factory.Save();
			CusStatementFilterStripBusinessObject filter = new CusStatementFilterStripBusinessObject();
			ModuleTextFilter fanFilter = (ModuleTextFilter)filter[CusStatementFilterStripBusinessObject.Schema.FAN];
			fanFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			fanFilter.Property = "123";
			fanFilter.IsActive = true;
			CusStatementLineChargeCollection coll = new CusStatementLineChargeCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(lineCharge, coll[0]);
		}

		public void TestChargeTypeQuery()
		{
			var statement = Factory.New<CusStatementHeader>();
			var statementLine = statement.StatementLines.AddNew();
			var lineCharge = statementLine.Charges.AddNew();
			lineCharge.B4_ChargeType = "Z";
			var statement2 = Factory.New<CusStatementHeader>();
			var statementLine2 = statement2.StatementLines.AddNew();
			var lineCharge2 = statementLine2.Charges.AddNew();
			lineCharge2.B4_ChargeType = "C";
			Factory.Save();
			CusStatementFilterStripBusinessObject filter = new CusStatementFilterStripBusinessObject();
			ModuleTextFilter chargeTypeFilter = (ModuleTextFilter)filter[CusStatementFilterStripBusinessObject.Schema.ChargeType];
			chargeTypeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			chargeTypeFilter.Property = "Z";
			chargeTypeFilter.IsActive = true;
			CusStatementLineChargeCollection coll = new CusStatementLineChargeCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(lineCharge, coll[0]);
		}

		public void TestStatementDateQuery()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_ProcessDate = ZDateTime.Now;
			var statementLine = statement.StatementLines.AddNew();
			var lineCharge = statementLine.Charges.AddNew();
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_ProcessDate = ZDateTime.Now.AddYears(-1);
			var statementLine2 = statement2.StatementLines.AddNew();
			statementLine2.Charges.AddNew();
			Factory.Save();
			CusStatementFilterStripBusinessObject filter = new CusStatementFilterStripBusinessObject();
			ModuleDateFilter lrnFilter = (ModuleDateFilter)filter[CusStatementFilterStripBusinessObject.Schema.StatementDate];
			lrnFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			lrnFilter.Property1 = ZDateTime.Today;
			lrnFilter.Property2 = ZDateTime.Empty;
			lrnFilter.IsActive = true;
			CusStatementLineChargeCollection coll = new CusStatementLineChargeCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(lineCharge, coll[0]);
		}

		public void TestFilters()
		{
			var filter = new CusStatementFilterStripBusinessObject();
			AssertNotNull(filter[CusStatementFilterStripBusinessObject.Schema.ChargeType]);
			AssertNotNull(filter[CusStatementFilterStripBusinessObject.Schema.FAN]);
			AssertNotNull(filter[CusStatementFilterStripBusinessObject.Schema.LRN]);
			AssertNotNull(filter[CusStatementFilterStripBusinessObject.Schema.StatementDate]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CusStatementFilterStripBusinessObject();
	}
}
