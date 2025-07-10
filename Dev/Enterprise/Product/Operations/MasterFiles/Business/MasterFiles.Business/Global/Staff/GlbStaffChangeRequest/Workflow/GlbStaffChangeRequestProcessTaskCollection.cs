namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffChangeRequestProcessTaskCollection : ProcessTaskCollection
	{
		public GlbStaffChangeRequestProcessTaskCollection(GlbStaffChangeRequest parent) : base(parent)
		{
		}

		public new GlbStaffChangeRequestProcessTask this[int index] => (GlbStaffChangeRequestProcessTask)Elements[index];

		public new GlbStaffChangeRequestProcessTask AddNew() => (GlbStaffChangeRequestProcessTask)base.AddNew();

		public new GlbStaffChangeRequest Parent => (GlbStaffChangeRequest)base.Parent;
	}
}
