using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class LinkedOrganizationsFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestMaxFilterStripPanelHeight()
		{
			using (var control = CreateControl())
			{
				AssertEquals("Should have default filter strip height", ControlDpiScalingHelper.ScaleToCurrentDpiY(200), control.MaxFilterStripPanelHeight_ForTestOnly);
			}
		}

		[RequiresSTA]
		public void TestUpdateRecordsFoundLabel()
		{
			using (var control = CreateControl())
			{
				AssertEquals("Pre-condition", string.Empty, control.ToolStripRecordsFoundLabel_ForTestOnly.Text);

				control.UpdateRecordsFoundLabel("TEST", false);

				AssertEquals("Tool Strip message should be updated", "TEST", control.ToolStripRecordsFoundLabel_ForTestOnly.Text);
				AssertEquals("Tool Strip message should be updated", Color.Blue, control.ToolStripRecordsFoundLabel_ForTestOnly.ForeColor);

				control.UpdateRecordsFoundLabel("TEST2", true);
				AssertEquals("Tool Strip message should be updated", "TEST2", control.ToolStripRecordsFoundLabel_ForTestOnly.Text);
				AssertEquals("Tool Strip message should be updated", Color.Red, control.ToolStripRecordsFoundLabel_ForTestOnly.ForeColor);
			}
		}

		#region Implementation

		LinkedOrganizationsFilterControlForTest CreateControl()
		{
			var filterBizO = new LinkedOrganizationsFilterBusinessObject();
			return new LinkedOrganizationsFilterControlForTest(filterBizO);
		}

		class LinkedOrganizationsFilterControlForTest : LinkedOrganizationsFilterControl
		{
			public LinkedOrganizationsFilterControlForTest(FilterStripBusinessObject filterBusinessObject) : base(filterBusinessObject)
			{
			}

			public int MaxFilterStripPanelHeight_ForTestOnly => MaxFilterStripPanelHeight;

			public ZLabel ToolStripRecordsFoundLabel_ForTestOnly => ToolStripRecordsFoundLabel;
		}

		#endregion
	}
}
