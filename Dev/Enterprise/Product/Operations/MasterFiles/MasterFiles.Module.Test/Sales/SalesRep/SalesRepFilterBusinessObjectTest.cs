using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(SalesRepFilterBusinessObject))]
	sealed class SalesRepFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Active

		public void TestIsActive()
		{
			var glbStaffFilter = new SalesRepFilterBusinessObjectForTest();
			AssertEquals(false, glbStaffFilter.IsActiveStatusFilterAlwaysAppliedExposed());
		}
		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new SalesRepFilterBusinessObject();
		}

		public class SalesRepFilterBusinessObjectForTest : SalesRepFilterBusinessObject
		{
			public bool IsActiveStatusFilterAlwaysAppliedExposed()
			{
				return IsActiveStatusFilterAlwaysApplied();
			}
		}

		#endregion
	}
}
