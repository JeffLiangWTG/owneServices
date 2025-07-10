namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffProcessTasksCollection : ProcessTaskCollection
	{
		public GlbStaffProcessTasksCollection(GlbStaff glbStaff) : base(glbStaff) { }

		public new GlbStaffProcessTask this[int index]
		{
			get { return (GlbStaffProcessTask)Elements[index]; }
		}

		public new GlbStaffProcessTask AddNew()
		{
			return (GlbStaffProcessTask)base.AddNew();
		}

		public new GlbStaff Parent
		{
			get { return (GlbStaff)base.Parent; }
		}
	}
}
