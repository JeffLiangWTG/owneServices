using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.Module.Testing
{
	public class HVLVConsignmentModuleStripTest : TestCaseWithFactory
	{
		public void TestHandlesReferenceNumberFilter()
		{
			ReferenceNumberFilter filter = new ReferenceNumberFilter("description", delegate
			{ return new ZQuery(); }, new RefCountryCollection(Factory));
			AssertProvidesControlsFor(filter);
		}

		#region Test Classes

		class TestHVLVConsignmentModuleStrip : HVLVConsignmentModuleStrip
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

		#region Implementation

		void AssertProvidesControlsFor(ModuleFilter filter)
		{
			using (TestHVLVConsignmentModuleStrip strip = new TestHVLVConsignmentModuleStrip())
			{
				Control[] controls = strip.GetCurrentFilterControls(filter);
				Assert("Should provide controls for " + filter.GetType().Name, controls.Length > 0);

				foreach (Control control in controls)
				{
					if (!IsExemptFromBindingRequirement(control))
					{
						bool isBound = !string.IsNullOrEmpty(strip.FilterControlBindingSource.GetBindingMember(control));
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
	}
}
