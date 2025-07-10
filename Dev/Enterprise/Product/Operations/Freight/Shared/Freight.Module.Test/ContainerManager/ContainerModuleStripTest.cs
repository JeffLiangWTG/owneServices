using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	sealed class ContainerModuleStripTest : BaseFreightTest
	{
		public void TestReferenceNumbers()
		{
			AssertProvidesControlsFor(new ReferenceNumberFilter("description", delegate
			{ return new ZQuery(); }, new RefCountryCollection(Factory)));
		}

		public void TestServiceTypeDate()
		{
			AssertProvidesControlsFor(new ServiceTypeDateFilter(new FilterStripBusinessObjectForTest(), typeof(CommonContainer), false));
		}

		#region Implementation

		void AssertProvidesControlsFor(ModuleFilter filter)
		{
			using (var strip = new TestContainerModuleStrip())
			{
				var controls = strip.GetCurrentFilterControls(filter);
				Assert("Should provide controls for " + filter.GetType().Name, controls.Length > 0);

				foreach (var control in controls)
				{
					if (!IsExemptFromBindingRequirement(control))
					{
						var isBound = !string.IsNullOrEmpty(strip.FilterControlBindingSource.GetBindingMember(control));
						AssertEquals("BindingMember set on FilterControlBindingSource", true, isBound);
					}

					control.Dispose();
				}
			}
		}

		bool IsExemptFromBindingRequirement(Control control)
		{
			return control is ZLabel;
		}

		#endregion

		#region Test Classes

		class TestContainerModuleStrip : ContainerModuleStrip
		{
			public new ZBindingSource FilterControlBindingSource
			{
				get { return base.FilterControlBindingSource; }
			}

			public new Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
			{
				return base.GetCurrentFilterControls(currentModuleFilter);
			}
		}

		#endregion
	}
}
