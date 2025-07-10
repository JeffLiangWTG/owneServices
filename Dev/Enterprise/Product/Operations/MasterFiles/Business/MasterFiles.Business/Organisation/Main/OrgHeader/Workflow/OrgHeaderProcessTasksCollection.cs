namespace Enterprise.MasterFiles.Business
{
	public class OrgHeaderProcessTasksCollection : ProcessTaskCollection
	{
		public OrgHeaderProcessTasksCollection(OrgHeader orgHeader) : base(orgHeader) { }

		public new OrgHeaderProcessTask this[int index]
		{
			get { return (OrgHeaderProcessTask)Elements[index]; }
		}

		public new OrgHeaderProcessTask AddNew()
		{
			return (OrgHeaderProcessTask)base.AddNew();
		}

		public new OrgHeader Parent
		{
			get { return (OrgHeader)base.Parent; }
		}
	}
}
