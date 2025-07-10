using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(WhsCartonGroupFilterBusinessObject))]
	class WhsCartonGroupFilterBusinessObjectTestCase : FilterStripBusinessObjectTestCase
	{
		#region TestAttachedOrganisations

		public void TestAttachedOrganisations()
		{
			var org1 = Helper.CreateClient("1");
			var org2 = Helper.CreateClient("2");
			var org3 = Helper.CreateClient("3");

			var cartonGroup1 = Helper.CreateWhsCartonGroup("TS1", "Test1");
			var cartonGroup2 = Helper.CreateWhsCartonGroup("TS2", "Test2");
			var cartonGroup3 = Helper.CreateWhsCartonGroup("TS3", "Test3");
			org1.MiscServ.OM_WCG_CartonGroup = cartonGroup1.PK;
			org2.MiscServ.OM_WCG_CartonGroup = cartonGroup2.PK;
			org3.MiscServ.OM_WCG_CartonGroup = cartonGroup2.PK;
			Factory.Save();
			Asserter.AddToScope(cartonGroup1, cartonGroup2, cartonGroup3);

			var filterBizO1 = GetNewFilterStripBusinessObject();
			var moduleFilter1 = (ModuleGuidFilter)filterBizO1[WhsCartonGroupFilterBusinessObject.Schema.AttachedOrganisations];
			moduleFilter1.IsActive = true;

			moduleFilter1.Property = org1.PK;
			Asserter.AssertMatches("Should return cartonGroup1.", filterBizO1.Filter, cartonGroup1);

			moduleFilter1.Property = org2.PK;
			Asserter.AssertMatches("Should return cartonGroup2.", filterBizO1.Filter, cartonGroup2);

			moduleFilter1.Property = org3.PK;
			Asserter.AssertMatches("Should return cartonGroup2.", filterBizO1.Filter, cartonGroup2);

			moduleFilter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			Asserter.AssertMatches("Should return cartonGroup1 and cartonGroup3.", filterBizO1.Filter, cartonGroup1, cartonGroup3);

			moduleFilter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Asserter.AssertMatches("Should return cartonGroup3.", filterBizO1.Filter, cartonGroup3);

			moduleFilter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Asserter.AssertMatches("Should return cartonGroup1 and cartonGroup2.", filterBizO1.Filter, cartonGroup1, cartonGroup2);
		}

		#endregion

		#region TestCode

		public void TestCode()
		{
			var cartonGroup1 = Helper.CreateWhsCartonGroup("TS1", "Test1");
			var cartonGroup2 = Helper.CreateWhsCartonGroup("TS2", "Test2");
			Factory.Save();
			Asserter.AddToScope(cartonGroup1, cartonGroup2);

			var filterBizO1 = GetNewFilterStripBusinessObject();
			var moduleFilter1 = (ModuleTextFilter)filterBizO1[WhsCartonGroupFilterBusinessObject.Schema.Code];
			moduleFilter1.IsActive = true;
			moduleFilter1.Property = "TS1";
			Asserter.AssertMatches("Shoud return only cartonGroup1", filterBizO1.Filter, cartonGroup1);

			moduleFilter1.Property = "TS2";
			Asserter.AssertMatches("Shoud return only cartonGroup2", filterBizO1.Filter, cartonGroup2);
		}

		#endregion

		#region TestDescription

		public void TestDescription()
		{
			var cartonGroup1 = Helper.CreateWhsCartonGroup("TS1", "Test1");
			var cartonGroup2 = Helper.CreateWhsCartonGroup("TS2", "Test2");
			Factory.Save();
			Asserter.AddToScope(cartonGroup1, cartonGroup2);

			var filterBizO1 = GetNewFilterStripBusinessObject();
			var moduleFilter1 = (ModuleTextFilter)filterBizO1[WhsCartonGroupFilterBusinessObject.Schema.Description];
			moduleFilter1.IsActive = true;
			moduleFilter1.Property = "Test1";
			Asserter.AssertMatches("Shoud return only cartonGroup1", filterBizO1.Filter, cartonGroup1);

			moduleFilter1.Property = "Test2";
			Asserter.AssertMatches("Shoud return only cartonGroup2", filterBizO1.Filter, cartonGroup2);
		}

		#endregion

		#region Implementation

		protected WhsTestHelperFunctionsEnv Helper
		{
			get { return (helper = helper ?? new WhsTestHelperFunctionsEnv(Factory)); }
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WhsCartonGroupFilterBusinessObject();
		}

		FilterStripAsserter<WhsCartonGroup> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<WhsCartonGroup>(Factory, c => c.WCG_Code)); }
		}

		WhsTestHelperFunctionsEnv helper;
		FilterStripAsserter<WhsCartonGroup> asserter;

		#endregion
	}
}
