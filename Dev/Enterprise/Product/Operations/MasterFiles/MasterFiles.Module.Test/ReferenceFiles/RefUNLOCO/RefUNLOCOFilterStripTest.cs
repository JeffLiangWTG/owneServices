using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Module.ReferenceFiles.RefUNLOCO;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RefUNLOCOFilterStripTest : TestCaseWithFactory
	{
		public void TestHandlesRefCityTownStateModuleFilter()
		{
			AssertProvidesControlsFor(GetNewRefCityTownStateModuleFilter());
		}

		public void TestFilterBusinessObjectDefaultsIsCountryCode()
		{
			using (var strip = new RefUNLOCOFilterStrip())
			using (var codeBox = new ZCodeFindBox())
			{
				codeBox.CodeBox.Text = "AU";
				var collection = strip.GetCountryStatesCollectionForPopup(codeBox);
				AssertEquals("Default value should be the country code", "AU", collection.FilterBusinessObjectDefaults["Country:Property"].Value);
			}
		}

		#region Implementation

		RefUNLOCOCountryStateModuleFilter GetNewRefCityTownStateModuleFilter()
		{
			return new RefUNLOCOCountryStateModuleFilter("TEST FILTER", RefUNLOCOSchema.RL_RN_NKCountryCode, RefUNLOCOSchema.RL_RW);
		}

		void AssertProvidesControlsFor(ModuleFilter filter)
		{
			using (RefUNLOCOFilterStrip strip = new RefUNLOCOFilterStrip())
			{
				Control[] controls = GetCurrentFilterControls(strip, filter);
				Assert("Should provide controls for " + filter.GetType().Name, controls.Length > 0);
				foreach (Control control in controls)
				{
					control.Dispose();
				}
			}
		}

		Control[] GetCurrentFilterControls(RefUNLOCOFilterStrip strip, ModuleFilter filter)
		{
			MethodInfo info = typeof(RefUNLOCOFilterStrip).GetMethod(
				"GetCurrentFilterControls", BindingFlags.Instance | BindingFlags.NonPublic);
			return (Control[])info.Invoke(strip, new object[] { filter });
		}

		#endregion
	}
}
