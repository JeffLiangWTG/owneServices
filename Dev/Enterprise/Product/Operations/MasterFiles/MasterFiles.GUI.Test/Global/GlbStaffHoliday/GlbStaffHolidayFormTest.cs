using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbStaffHolidayForm))]
	sealed class GlbStaffHolidayFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestNoEdocs()
		{
			using var form = GetFormToBash();
			Assert("Disallow any viewing or editing of edocs through CW1, users must use HRMS and its appropriate entity security", !form.PlugIns.IsPlugInAvailable(ControllerIDs.eDocsPlugIn));
		}

		new GlbStaffHolidayForm GetFormToBash()
			=> (GlbStaffHolidayForm)base.GetFormToBash();

		protected override Form GetFormToBashCore()
		{
			return NewGlbStaffForm(Factory.New<GlbStaffHoliday>());
		}

		GlbStaffHolidayForm NewGlbStaffForm(GlbStaffHoliday holiday)
		{
			return new GlbStaffHolidayForm(holiday) { ControllerID = ControllerIDs.GlbStaffHoliday };
		}

		public void TestControlsReadOnly()
		{
			var hiringRequest = Factory.NewWithValidTestData<GlbStaffHoliday>();
			using (var form = GetFormToBash())
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(true, ((ZDropEdit)form.Controls.Find("zDropEditStatus", true)[0]).ReadOnly);
				AssertEquals(true, ((ZDropEdit)form.Controls.Find("zDropEditType", true)[0]).ReadOnly);

				AssertEquals(true, ((ZDateEdit)form.Controls.Find("zDateEditStartDate", true)[0]).ReadOnly);
				AssertEquals(true, ((ZDateEdit)form.Controls.Find("zDateEditEndDate", true)[0]).ReadOnly);

				AssertEquals(true, ((ZTextBox)form.Controls.Find("zTextBoxDays", true)[0]).ReadOnly);
				AssertEquals(true, ((ZTextBox)form.Controls.Find("zTextBoxFullName", true)[0]).ReadOnly);
			}
		}

		class TestGlbStaffHolidayForm : GlbStaffHolidayForm
		{
			public IEnumerable<ZDropEdit> GetZDropEdit => new[] { zDropEditStatus, zDropEditType };
			public IEnumerable<ZDateEdit> GetZDateEdit => new[] { zDateEditStartDate, zDateEditEndDate };
			public IEnumerable<ZTextBox> GetZTextBox => new[] { zTextBoxDays, zTextBoxFullName };

			public TestGlbStaffHolidayForm(GlbStaffHoliday businessEntity) : base(businessEntity)
			{
			}
		}
	}
}
