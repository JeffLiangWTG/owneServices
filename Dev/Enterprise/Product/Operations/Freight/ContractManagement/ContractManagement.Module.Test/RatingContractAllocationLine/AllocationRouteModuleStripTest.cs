using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ContractManagement.Module.Testing
{
	public class AllocationRouteModuleStripTest
		: TestCaseWithFactory
	{
		public void TestShowRelatedUNLOCOsFilter()
		{
			using (var strip = new AllocationRouteModuleStripForTest())
			{
				var showRelatedUNLOCOsFilter = new AllocationRouteCoveringLocationFilter(AllocationRouteFilterConstants.LoadDischargePort, new LocationCollection(Factory));
				var currentControlsOnStrip = strip.GetCurrentFilterControlsForTest(showRelatedUNLOCOsFilter);

				try
				{
					AssertEquals(1, currentControlsOnStrip.Length);
					AssertEquals(typeof(LoadDischargeFilterControl), currentControlsOnStrip[0].GetType());
				}
				finally
				{
					DisposeControls(currentControlsOnStrip);
				}
			}
		}

		#region Test Class

		class AllocationRouteModuleStripForTest : AllocationRouteModuleStrip
		{
			public Control[] GetCurrentFilterControlsForTest(ModuleFilter filter)
			{
				return base.GetCurrentFilterControls(filter);
			}
		}

		#endregion

		void DisposeControls(Control[] controls)
		{
			foreach (var controlToDispose in controls)
			{
				controlToDispose.Dispose();
			}
		}
	}
}
