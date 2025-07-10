namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DummyEnterpriseBusinessObjectProcessTaskCollection : ProcessTaskCollection
	{
		public DummyEnterpriseBusinessObjectProcessTaskCollection(DummyEnterpriseBusinessObjectWithWorkflow parent)
			: base(parent)
		{
		}

		public new DummyEnterpriseBusinessObjectProcessTask this[int index]
		{
			get { return (DummyEnterpriseBusinessObjectProcessTask)Elements[index]; }
		}

		public new DummyEnterpriseBusinessObjectProcessTask AddNew()
		{
			return (DummyEnterpriseBusinessObjectProcessTask)base.AddNew();
		}
	}
}
