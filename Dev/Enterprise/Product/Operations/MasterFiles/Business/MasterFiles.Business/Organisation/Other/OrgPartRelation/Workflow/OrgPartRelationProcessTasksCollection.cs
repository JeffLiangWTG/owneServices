namespace Enterprise.MasterFiles.Business
{
	public class OrgPartRelationProcessTasksCollection : ProcessTaskCollection
	{
		public OrgPartRelationProcessTasksCollection(OrgPartRelation orgPartRelation)
			: base(orgPartRelation)
		{
		}

		public new OrgPartRelationProcessTask this[int index]
		{
			get { return (OrgPartRelationProcessTask)Elements[index]; }
		}

		public new OrgPartRelationProcessTask AddNew()
		{
			return (OrgPartRelationProcessTask)base.AddNew();
		}

		public new OrgPartRelation Parent
		{
			get { return (OrgPartRelation)base.Parent; }
		}
	}
}
