
namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffHolidayProcessTasksCollection : ProcessTaskCollection
	{
		public GlbStaffHolidayProcessTasksCollection(GlbStaffHoliday glbStaffHoliday) : base(glbStaffHoliday) { }

		public new GlbStaffHolidayProcessTask this[int index] => (GlbStaffHolidayProcessTask)Elements[index];

		public new GlbStaffHolidayProcessTask AddNew() => (GlbStaffHolidayProcessTask)base.AddNew();

		public new GlbStaffHoliday Parent => (GlbStaffHoliday)base.Parent;
	}
}
