using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public abstract class GlbStaffResourceTime : AutoGlbStaffHoliday
	{
		public GlbStaffResourceTime(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GA_RecordType = LeaveOrTimeAllocation;
		}

		protected abstract string LeaveOrTimeAllocation { get; }

		#endregion

		#region Properties

		[List("Lookups.Types")]
		public override ZString GA_WorkHolidayType
		{
			get
			{
				return base.GA_WorkHolidayType;
			}
			set
			{
				base.GA_WorkHolidayType = value;
			}
		}

		[List("Lookups.Statuses")]
		public override ZString GA_ApprovalStatus
		{
			get
			{
				return base.GA_ApprovalStatus;
			}
			set
			{
				base.GA_ApprovalStatus = value;
			}
		}

		#region Type Description

		public ZString TypeDescription
		{
			get { return Lookups.Types.GetDescriptionFromCode(GA_WorkHolidayType); }
		}

		public ZPropertyInfo TypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(TypeDescription)); }
		}

		#endregion

		#endregion
	}
}
