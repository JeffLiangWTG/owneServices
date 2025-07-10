using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(WhsCartonSizeFilterBusinessObject))]
	public class WhsCartonSizeFilterBusinessObjectTestCase : FilterStripBusinessObjectTestCase
	{
		#region TestName

		public void TestName()
		{
			var cartonSize1 = Helper.CreateWhsCartonSize("TST1");
			var cartonSize2 = Helper.CreateWhsCartonSize("TST2");
			Factory.Save();
			Asserter.AddToScope(cartonSize1, cartonSize2);

			var filterBizO1 = GetNewFilterStripBusinessObject();
			var moduleFilter1 = (ModuleTextFilter)filterBizO1[WhsCartonSizeFilterBusinessObject.Schema.Code];
			moduleFilter1.IsActive = true;
			moduleFilter1.Property = "TST1";
			Asserter.AssertMatches("Shoud  return only cartonSize1", filterBizO1.Filter, cartonSize1);

			moduleFilter1.Property = "TST2";
			Asserter.AssertMatches("Shoud return only cartonSize2", filterBizO1.Filter, cartonSize2);
		}

		#endregion

		#region Implementation

		protected WhsTestHelperFunctionsEnv Helper
		{
			get { return (helper = helper ?? new WhsTestHelperFunctionsEnv(Factory)); }
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WhsCartonSizeFilterBusinessObject();
		}

		FilterStripAsserter<WhsCartonSize> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<WhsCartonSize>(Factory, c => c.WCS_Code)); }
		}

		WhsTestHelperFunctionsEnv helper;
		FilterStripAsserter<WhsCartonSize> asserter;

		#endregion
	}
}
