namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierPartProcessTasksCollection : ProcessTaskCollection
	{
		public OrgSupplierPartProcessTasksCollection(OrgSupplierPart orgSupplierPart)
			: base(orgSupplierPart)
		{
		}

		public new OrgSupplierPartProcessTask this[int index]
		{
			get { return (OrgSupplierPartProcessTask)Elements[index]; }
		}

		public new OrgSupplierPartProcessTask AddNew()
		{
			return (OrgSupplierPartProcessTask)base.AddNew();
		}

		public new OrgSupplierPart Parent
		{
			get { return (OrgSupplierPart)base.Parent; }
		}
	}
}
