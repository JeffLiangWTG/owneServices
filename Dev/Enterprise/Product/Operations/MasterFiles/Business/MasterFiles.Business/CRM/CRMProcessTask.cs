using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class CRMProcessTask : ProcessTask
	{
		public CRMProcessTask(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		#region Reminder Time Zone

		protected override ITimeZone ReminderTimeZone
		{
			get
			{
				if (this.AssignedStaffMember == null || this.AssignedStaffMember.HomeBranch == null)
				{
					return base.ReminderTimeZone;
				}

				RefUNLOCO loco = this.AssignedStaffMember.HomeBranch.HomePort;

				if (loco != null && loco.TimeZoneSet != null)
				{
					return loco.TimeZoneSet.GetCalculationTimeZone();
				}
				else
				{
					return base.ReminderTimeZone;
				}
			}
		}

		#endregion
	}
}
