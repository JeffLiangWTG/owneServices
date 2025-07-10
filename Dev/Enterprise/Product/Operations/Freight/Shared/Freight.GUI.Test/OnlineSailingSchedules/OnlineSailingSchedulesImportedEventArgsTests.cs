using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.GUI.OnlineSailingSchedules.Testing
{
	sealed class OnlineSailingSchedulesImportedEventArgsTests : TestCaseWithFactory
	{
		public void TestCtor_ImportedRoutes_WouldBeEmpty_WhenNullIsPassed()
		{
			var args = new OnlineSailingSchedulesImportedEventArgs(null);
			AssertNotNull(args.ImportedRoutes);
			AssertEquals(false, args.ImportedRoutes.Any());
		}
	}
}
