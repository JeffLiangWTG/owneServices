using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class CountryStatesGlbHolidayFilterStripTest : TestCaseWithFactory
	{
		public void TestHandlesCountryStatesGlbHolidayFilter()
		{
			AssertProvidesControlsFor(GetNewCountryStatesGlbHolidayCountryStateModuleFilter());
		}

		public void TestFilterBusinessObjectDefaultsIsCountryCode()
		{
			using (var strip = new CountryStatesGlbHolidayFilterStrip())
			using (var codeBox = new ZGuidFindBox())
			{
				codeBox.CodeBox.Text = "AU";
				var collection = strip.GetCountryStatesCollectionForPopup(codeBox);
				AssertEquals("Default value should be the country code", "AU", collection.FilterBusinessObjectDefaults["Country:Property"].Value);
			}
		}

		#region Implementation

		CountryStatesGlbHolidayCountryStateModuleFilter GetNewCountryStatesGlbHolidayCountryStateModuleFilter()
		{
			return new CountryStatesGlbHolidayCountryStateModuleFilter("TEST FILTER", GlbHolidaySchema.GH_ParentID, GlbHolidaySchema.GH_ParentTableCode);
		}

		void AssertProvidesControlsFor(ModuleFilter filter)
		{
			using (var strip = new CountryStatesGlbHolidayFilterStrip())
			{
				Control[] controls = GetCurrentFilterControls(strip, filter);
				Assert("Should provide controls for " + filter.GetType().Name, controls.Length > 0);
				foreach (Control control in controls)
				{
					control.Dispose();
				}
			}
		}

		Control[] GetCurrentFilterControls(CountryStatesGlbHolidayFilterStrip strip, ModuleFilter filter)
		{
			MethodInfo info = typeof(CountryStatesGlbHolidayFilterStrip).GetMethod(
				"GetCurrentFilterControls", BindingFlags.Instance | BindingFlags.NonPublic);
			return (Control[])info.Invoke(strip, new object[] { filter });
		}

		#endregion
	}
}
