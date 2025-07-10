using System.Reflection;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class SailingsGridTest : BaseFreightTest
	{
		public void TestShowContextMenu()
		{
			SetUpVoyage();

			using (ZJobVoyageForm form = new ZJobVoyageForm(fSeaVoyage))
			{
				form.Show();
				Application.DoEvents();

				SailingsGrid.TestHelper helper = new SailingsGrid.TestHelper(FindSailingsGrid(form));
				helper.Grid.ResetColumns();

				AssertEquals("precondition: need a read-write column", false, helper.Grid.Columns[3].ColumnStyle.ReadOnly);
				helper.FireRightClick(0, 3);
				AssertEquals("The CopyTo menu item should be visible", true, helper.CopyTo.Visible);

				AssertEquals("precondition: need a read-only column", true, helper.Grid.Columns[1].ColumnStyle.ReadOnly);
				helper.FireRightClick(0, 1);
				AssertEquals("The CopyTo menu item should be visible", false, helper.CopyTo.Visible);
			}
		}

		public void TestCopyToLabel_SEA()
		{
			GenericTestCopyToLabel(Core.Constants.TransportModes.Sea, "Copy To Sailings With the Same Load");
		}

		public void TestCopyToLabel_AIR()
		{
			GenericTestCopyToLabel(Core.Constants.TransportModes.Air, "Copy To Flights With the Same Load");
		}

		public void TestCopyToLabel_ROA()
		{
			GenericTestCopyToLabel(Core.Constants.TransportModes.Road, "Copy To Sectors With the Same Load");
		}

		public void TestCopyToLabel_RAI()
		{
			GenericTestCopyToLabel(Core.Constants.TransportModes.Rail, "Copy To Sectors With the Same Load");
		}

		#region Implementation

		SailingsGrid FindSailingsGrid(ZJobVoyageForm form)
		{
			VoyageDetailsControl control = (VoyageDetailsControl)typeof(ZJobVoyageForm).InvokeMember("detailsControl", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, form, System.Array.Empty<object>());
			return (SailingsGrid)typeof(VoyageDetailsControl).InvokeMember("jobSailingBoundGrid", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, control, System.Array.Empty<object>());
		}

		void GenericTestCopyToLabel(ZString transportMode, string label)
		{
			SetUpVoyage();

			fSeaVoyage.JV_AirSeaRoad = transportMode;
			using (ZJobVoyageForm form = new ZJobVoyageForm(fSeaVoyage))
			{
				form.Show();
				Application.DoEvents();

				SailingsGrid.TestHelper helper = new SailingsGrid.TestHelper(FindSailingsGrid(form));
				AssertEquals("Incorrect menu item label", label, helper.CopyTo.Text);
			}
		}

		#endregion
	}
}
