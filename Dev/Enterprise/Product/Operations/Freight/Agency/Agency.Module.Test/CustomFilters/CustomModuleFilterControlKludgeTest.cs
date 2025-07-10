using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class CustomModuleFilterControlKludgeTest : TestCaseWithFactory
	{
		public void TestHandlesEntryNumberFilter()
		{
			AssertProvidesControlsFor(new EntryNumberModuleFilter("CustomsEntryTypeAndNumber Test", delegate
			{
				return null;
			}));
		}

		public void TestHandlesChargeModuleFilter()
		{
			AssertProvidesControlsFor(new ChargeModuleFilter("ChargeModuleFilter Test", (q, b) => q));
		}

		public void TestHandlesOrgRelatedPartiesModuleFilter()
		{
			AssertProvidesControlsFor(new OrgRelatedPartiesModuleFilter("OrgRelatedPartiesModuleFilter Test", delegate
			{
				return null;
			}));
		}

		public void TestHandlesRefrenceNumbers()
		{
			AssertProvidesControlsFor(new ReferenceNumberFilter("ReferenceNumberFilter Test", delegate
			{
				return null;
			}, new RefCountryCollection(Factory)));
		}

		#region Implementation
		void AssertProvidesControlsFor(ModuleFilter filter)
		{
			using (CustomModuleFilterControlKludge strip = new CustomModuleFilterControlKludge())
			{
				Control[] controls = GetCurrentFilterControls(strip, filter);
				Assert("Should provide controls for " + filter.GetType().Name, controls.Length > 0);
				foreach (Control control in controls)
				{
					control.Dispose();
				}
			}
		}

		Control[] GetCurrentFilterControls(CustomModuleFilterControlKludge strip, ModuleFilter filter)
		{
			MethodInfo info = typeof(CustomModuleFilterControlKludge).GetMethod("GetCurrentFilterControls", BindingFlags.Instance | BindingFlags.NonPublic);
			return (Control[])info.Invoke(strip, new object[] { filter });
		}
		#endregion
	}
}
