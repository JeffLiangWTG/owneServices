using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class SplitBookingsControlTest : TestCaseWithFactory
	{
		public void TestGridsVisibility_ComplexTest()
		{
			AssertGridsVisibility(new SplitBookingsHeader(Factory)
			{ ShowContainers = true }, typeof(BookedContainersSplitGrid));
			AssertGridsVisibility(new SplitBookingsHeader(Factory)
			{ ShowActualContainers = true }, typeof(ActualContainersSplitGrid));
			AssertGridsVisibility(new SplitBookingsHeader(Factory)
			{ ShowVehicles = true }, typeof(VehiclesSplitGrid));
			AssertGridsVisibility(new SplitBookingsHeader(Factory)
			{ ShowTopLevelPacks = true }, typeof(TopLevelPacksSplitGrid));
			AssertGridsVisibility(new SplitBookingsHeader(Factory)
			{ ShowPackLines = true }, typeof(PacksSplitGrid));
			AssertGridsVisibility(new SplitBookingsHeader(Factory)
			{ ShowContainers = true, ShowPackLines = true, ShowTopLevelPacks = true }, typeof(BookedContainersSplitGrid), typeof(PacksSplitGrid), typeof(TopLevelPacksSplitGrid));
			AssertGridsVisibility(new SplitBookingsHeader(Factory)
			{ ShowVehicles = true, ShowActualContainers = true, ShowPackLines = true }, typeof(VehiclesSplitGrid), typeof(ActualContainersSplitGrid), typeof(PacksSplitGrid));
		}

		void AssertGridsVisibility(SplitBookingsHeader header, params Type[] exptectedGrids)
		{
			using (var form = new ZForm(header))
			{
				var bindingSource = new KBindingSource(form, typeof(SplitBookingsHeader));
				var control = new SplitBookingsControl();
				control.Dock = DockStyle.Fill;
				bindingSource.SetBindingMember(control, ".");
				form.Controls.Add(control);
				form.Size = new Size(1000, 900);
				bindingSource.SetDataBinding(header, "");
				form.Show();
				Application.DoEvents();
				var actualGrids = GetAllControls<SplitGrid>(from: control).Select(c => c.GetType());
				AssertContainsExactElementsInAnyOrder("Created grids", exptectedGrids, actualGrids);
			}
		}

		IEnumerable<Control> GetAllControls<T>(Control from)
			where T : Control
		{
			var controls = from.Controls.Cast<Control>();
			return controls.SelectMany(ctrl => GetAllControls<T>(ctrl)).Concat(controls).OfType<T>();
		}
	}
}
