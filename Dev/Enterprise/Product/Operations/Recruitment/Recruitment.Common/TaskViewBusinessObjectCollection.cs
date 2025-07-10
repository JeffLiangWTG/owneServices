using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruitment.Common
{
	public class TaskViewBusinessObjectCollection : NonPersistentBusinessObjectCollection<ProcessTaskView>
	{
		public TaskViewBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override bool AllowNewCore => AllowNew;

		public new bool AllowNew;

		public TaskViewBusinessObjectCollection(BusinessObjectFactory factory, ProcessTaskCollectionView ptcv)
			: base(factory)
		{
			foreach (var task in ptcv)
			{
				Add(new ProcessTaskView((ProcessTask)task));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new ProcessTaskView(Factory.New<ProcessTask>());
	}
}
