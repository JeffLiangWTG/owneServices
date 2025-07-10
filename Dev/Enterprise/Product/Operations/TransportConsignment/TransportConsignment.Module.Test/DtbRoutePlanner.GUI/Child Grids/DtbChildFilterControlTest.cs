using System.Reflection;
using CargoWise.Types;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;

namespace Enterprise.TransportConsignment.GUI.Testing
{
	public abstract class DtbChildFilterControlTest : DtbBookingConsignmentTestCaseWithFactory
	{
		#region TestIsFilterVisible

		public void TestIsFilterVisible()
		{
			using (var control = GetNewFilterControl())
			{
				AssertEquals("ChildFilterControls should never have the Filters visible, filtering is done by a Parent FilterControl", false, control.IsFilterVisible);
			}
		}

		#endregion

		#region TestShouldPerformSearch

		public void TestShouldPerformSearch()
		{
			using (var control = GetNewFilterControl())
			{
				var shouldPerformSearch = control.GetType().GetMethod("ShouldPerformSearch", BindingFlags.NonPublic | BindingFlags.Instance);

				control.Visible = false;
				AssertEquals("Filters should not be visible.", false, (ZBool)shouldPerformSearch.Invoke(control, null));

				control.Visible = true;
				AssertEquals("Filters should not be visible.", false, (ZBool)shouldPerformSearch.Invoke(control, null));

				control.SetDataBinding(new DtbRoutePlanner(Factory), "");
				AssertEquals("Filters should be visible.", true, (ZBool)shouldPerformSearch.Invoke(control, null));
			}
		}

		#endregion

		protected abstract DtbChildFilterControl GetNewFilterControl();
	}
}
