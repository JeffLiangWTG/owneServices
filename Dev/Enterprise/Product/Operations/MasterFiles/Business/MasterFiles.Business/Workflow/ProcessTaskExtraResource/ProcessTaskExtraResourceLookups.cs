namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskExtraResourceLookups : AutoProcessTaskExtraResourceLookups
	{
		public ProcessTaskExtraResourceLookups(AutoProcessTaskExtraResource parent) : base(parent)
		{
		}

		public new GlbStaffAndResourceCollection StaffOrResources
		{
			get { return new GlbStaffAndResourceCollection(Factory); }
		}
	}
}
