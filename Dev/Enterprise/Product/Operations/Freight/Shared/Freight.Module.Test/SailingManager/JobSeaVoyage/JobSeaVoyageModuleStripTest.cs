using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Module.Testing
{
	sealed class JobSeaVoyageModuleStripTest : BaseFreightTest
	{
		public void TestVessalVoyageFilterControl()
		{
			var filter = new VoyageVesselModuleFilter("filterDescription", SampleQuery, BindToLists.GetCachedLists(Factory).RefVessel_List);

			using (TestJobSeaVoyageModuleStrip strip = new TestJobSeaVoyageModuleStrip())
			{
				Control[] controls = strip.GetCurrentFilterControls(filter);

				try
				{
					AssertEquals("Should be 1 control", 1, controls.Length);
					AssertControl<VoyageVesselModuleFilterControl>(strip, controls[0], "");
				}
				finally
				{
					DisposeControls(controls);
				}
			}
		}

		ZQuery SampleQuery(SQLComparisonOperator textValueComparisonOperator, ZString flightOrVoyageNo, ZString vesselNK, ZBool includeArchived)
		{
			return new ZQuery();
		}

		class TestJobSeaVoyageModuleStrip : JobSeaVoyageModuleStrip
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

		void AssertControl<T>(TestJobSeaVoyageModuleStrip strip, Control control, string bindingMember)
		{
			AssertType("Control's Type", typeof(T), control);
			AssertEquals("Control's BindingMember", bindingMember, strip.FilterControlBindingSource.GetBindingMember(control));
		}

		void DisposeControls(Control[] controls)
		{
			foreach (Control control in controls)
			{
				control.Dispose();
			}
		}
	}
}
