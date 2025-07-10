namespace Enterprise.MasterFiles.Business
{
	public class StaffViewStmNumsCollection : ViewStmNumsCollection<StaffViewStmNums>
	{
		public StaffViewStmNumsCollection(GlbStaff staff) : base(staff, StaffViewStmNums.Schema.SN_NamePrefix)
		{
		}
	}
}
