using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.HRM.Common
{
	public class HrlLeaveProcessedLog : AutoHrlLeaveProcessedLog
	{
		public HrlLeaveProcessedLog(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(StaffHoliday))]
		public override ZGuid LLD_GA_StaffHoliday
		{
			get => base.LLD_GA_StaffHoliday;
			set => base.LLD_GA_StaffHoliday = value;
		}

		public GlbStaffHoliday StaffHoliday => Factory.Load<GlbStaffHoliday>(LLD_GA_StaffHoliday);

		[RelatedBusinessObject(nameof(ProcessingRun))]
		public override ZGuid LLD_LLR_ProcessingRun
		{
			get => base.LLD_LLR_ProcessingRun;
			set => base.LLD_LLR_ProcessingRun = value;
		}

		public HrlProcessingRun ProcessingRun => Factory.Load<HrlProcessingRun>(LLD_LLR_ProcessingRun);
	}
}
