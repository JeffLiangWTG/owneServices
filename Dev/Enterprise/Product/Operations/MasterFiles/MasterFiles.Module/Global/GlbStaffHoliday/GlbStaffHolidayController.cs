using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class GlbStaffHolidayController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public GlbStaffHolidayController()
		{
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.GlbStaffHoliday;

		public override ControllerID ID => ControllerIDs.GlbStaffHoliday;

		public override Type TypeOfTopLevelBusinessObject => typeof(GlbStaffHoliday);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WorkItemEditModifyStaffAssignment;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WorkItemEditModifyStaffAssignment;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WorkItemEditModifyStaffAssignment;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.WorkItemEditModifyStaffAssignment;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			GlbStaffHoliday group = (GlbStaffHoliday)businessEntity;
			return new GlbStaffHolidayForm(group);
		}
	}
}
