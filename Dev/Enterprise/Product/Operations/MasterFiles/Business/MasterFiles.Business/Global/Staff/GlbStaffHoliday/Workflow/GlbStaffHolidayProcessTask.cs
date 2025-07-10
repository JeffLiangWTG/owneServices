using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffHolidayProcessTask : ProcessTasks
	{
		public GlbStaffHolidayProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Parent

		public override ControllerID ParentControllerID => ControllerIDs.GlbStaffHoliday;

		protected internal override Type ParentType => typeof(GlbStaffHoliday);

		#endregion

		protected internal override bool GetShouldPropertiesBeReadOnly(PropertyDescriptor propertyDescriptor)
		{
			if (propertyDescriptor.Name == Schema.P9_GS_NKAssignedStaffMember)
			{
				return false;
			}
			return base.GetShouldPropertiesBeReadOnly(propertyDescriptor);
		}
	}
}
