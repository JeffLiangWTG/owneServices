using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DummyProcessTaskCollectionForReadOnly : DummyProcessTaskCollection
	{
		public DummyProcessTaskCollectionForReadOnly(BusinessObject master)
			: base(master)
		{
		}

		public new DummyProcessTaskForReadOnly this[int index]
		{
			get { return (DummyProcessTaskForReadOnly)Elements[index]; }
		}

		public new DummyProcessTaskForReadOnly AddNew()
		{
			return (DummyProcessTaskForReadOnly)base.AddNew();
		}
	}
}
