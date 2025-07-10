namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	using System.Windows.Forms;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.ZArchitecture.Business;
	using Freight.Common.Business;
	using Freight.GUI;

	public class OnlineSailingSchedulesFilterStripTest : TestCaseWithFactory
	{
		public void TestOnlineSailingSchedulesFilterStrip_ReturnsVoyageVesselControl()
		{
			var filter = new OnlineSchedulesVoyageVesselFilter("filterDescription", (a, b, c) => new ZQuery(), BindToLists.GetCachedLists(Factory).RefVessel_List);

			using (var strip = new TestOnlineSailingSchedulesFilterStrip())
			{
				Control[] controls = strip.GetCurrentFilterControls(filter);

				try
				{
					AssertEquals("Should return 1 control", 1, controls.Length);
					AssertType("Should return OnlineSchedulesVoyageVesselFilterControl", typeof(OnlineSchedulesVoyageVesselFilterControl), controls[0]);
				}
				finally
				{
					foreach (var control in controls)
					{
						control.Dispose();
					}
				}
			}
		}

		class TestOnlineSailingSchedulesFilterStrip : OnlineSailingSchedulesFilterStrip
		{
			public new Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
			{
				return base.GetCurrentFilterControls(currentModuleFilter);
			}
		}
	}
}
