using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefCountryStatesFilterBusinessObject))]
	sealed class RefCountryStatesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestSetAdditionalFilterDefaults()
		{
			FilterStripBizO.SetAdditionalFilterDefaults("", new RefCountryStatesCollection(Factory));
			AssertEquals(2, FilterStripBizO.AlwaysVisibleModuleFilters.Count);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefCountryStatesFilterBusinessObject();
		}

		RefCountryStatesFilterBusinessObject FilterStripBizO
		{
			get { return fFilterStripBizO ?? (fFilterStripBizO = new RefCountryStatesFilterBusinessObject()); }
		}

		RefCountryStatesFilterBusinessObject fFilterStripBizO;

		#endregion
	}
}
