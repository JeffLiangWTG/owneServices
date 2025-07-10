using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Module.ReferenceFiles.RefCityTown;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RefCityTownFilterStripTest : TestCaseWithFactory
	{
		public void TestHandlesRefCityTownStateModuleFilter()
		{
			AssertProvidesControlsFor(GetNewRefCityTownStateModuleFilter());
		}

		#region Implementation

		RefCityTownCountryStateModuleFilter GetNewRefCityTownStateModuleFilter()
		{
			return new RefCityTownCountryStateModuleFilter("TEST FILTER", RefCityTownSchema.R9_RN_NKCountry, RefCityTownSchema.R9_RW_NKState);
		}

		void AssertProvidesControlsFor(ModuleFilter filter)
		{
			using (RefCityTownFilterStrip strip = new RefCityTownFilterStrip())
			{
				Control[] controls = GetCurrentFilterControls(strip, filter);
				Assert("Should provide controls for " + filter.GetType().Name, controls.Length > 0);
				foreach (Control control in controls)
				{
					control.Dispose();
				}
			}
		}

		Control[] GetCurrentFilterControls(RefCityTownFilterStrip strip, ModuleFilter filter)
		{
			MethodInfo info = typeof(RefCityTownFilterStrip).GetMethod(
				"GetCurrentFilterControls", BindingFlags.Instance | BindingFlags.NonPublic);
			return (Control[])info.Invoke(strip, new object[] { filter });
		}

		#endregion
	}
}
