using System;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.GUI
{
	[CodeAlive("This is used for the Task List screen where a Task for a GlbStaffHoliday can be listed and once clicked will display this form.")]
	public partial class GlbStaffHolidayForm : ZForm, ICustomerServiceMenuSectionCodeOverridable
	{
		public string SectionCode => ModuleTreeCustomerServiceMenuSectionList.Codes.System;

		public GlbStaffHolidayForm()
		{
		}

		public GlbStaffHolidayForm(GlbStaffHoliday businessEntity) : base(businessEntity)
		{
			CaptionRenderingEnabled = true;
			CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffHoliday|35C0F160-32E4-4CBE-88D3-2D97254A7B9E", "Staff Holiday");

			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);

			if (!this.IsDesignMode())
			{
				WorkflowTabPage.Initialize(businessEntity);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			zDropEditStatus.ReadOnly = true;
			zDateEditStartDate.ReadOnly = true;
			zDateEditEndDate.ReadOnly = true;
			zDropEditType.ReadOnly = true;
			zTextBoxDays.ReadOnly = true;
			zTextBoxFullName.ReadOnly = true;
		}
	}
}
