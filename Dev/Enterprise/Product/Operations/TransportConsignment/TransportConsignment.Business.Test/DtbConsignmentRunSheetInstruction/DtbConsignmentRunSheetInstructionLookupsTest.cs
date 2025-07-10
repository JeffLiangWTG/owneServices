using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentRunSheetInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestFailureReasons

		public void TestFailureReasons()
		{
			AssertEquals(typeof(SystemDefinableCodeDescriptionBoolCollection), Factory.New<DtbConsignmentRunSheetInstruction>().Lookups.FailureReasons.GetType());
		}

		#endregion
	}
}
