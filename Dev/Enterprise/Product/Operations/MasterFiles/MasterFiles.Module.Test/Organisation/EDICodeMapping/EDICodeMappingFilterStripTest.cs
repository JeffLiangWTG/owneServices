using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;

namespace Enterprise.MasterFilters.Module.Testing
{
	sealed class EDICodeMappingFilterStripTest : TestCaseWithFactory
	{
		public void TestGetCurrentFilterControls()
		{
			using (var strip = new EDICodeMappingFilterStripForTest())
			{
				var controls = strip.GetCurrentFilterControlsForTest(new EDICodeMappingRelationshipLocalCodeModuleFilter("TestGetCurrentFilterControls", Factory.New<OrgPatternMatchOverride>()));

				AssertEquals(1, controls.Length);
				AssertType<EDICodeMappingRelationshipLocalCodeFilterControl>(controls.First());

				foreach (var control in controls)
				{
					control.Dispose();
				}
			}
		}
	}
}
