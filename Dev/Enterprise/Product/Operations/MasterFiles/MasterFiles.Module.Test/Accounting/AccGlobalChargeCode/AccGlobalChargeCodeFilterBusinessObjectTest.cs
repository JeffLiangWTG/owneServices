using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccGlobalChargeCodeFilterBusinessObject))]
	sealed class AccGlobalChargeCodeFilterBusinessObjectTest : AccChargeCodeFilterBusinessObjectTest
	{
		public override void TestLinkedToGlobalListFilter()
		{
			AssertNull("No Linked To Global Filters in Global Module", GetModuleTextFilter("LinkedToGlobal"));
		}

		public override void TestGovernmentChargeCodeFilter()
		{
			AssertNull("No Government Charge Code Filters in Global Module", GetModuleTextFilter("Government Charge Code"));
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccGlobalChargeCodeFilterBusinessObject();
		}

		#endregion
	}
}
