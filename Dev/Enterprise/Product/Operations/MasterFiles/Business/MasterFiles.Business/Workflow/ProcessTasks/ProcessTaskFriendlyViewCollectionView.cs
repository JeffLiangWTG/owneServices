using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskFriendlyViewCollectionView : NonPersistentBusinessObjectCollection<ProcessTaskFriendlyView>
	{
		public ProcessTaskFriendlyViewCollectionView(ProcessTaskCollectionView processTaskCollectionView)
			: base(processTaskCollectionView.Factory)
		{
			processTaskCollectionView.ToList().ForEach(x => Add(new ProcessTaskFriendlyView((ProcessTask)x)));
		}

		public ProcessTaskFriendlyViewCollectionView(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var task = Factory.New<ProcessTask>();
			return new ProcessTaskFriendlyView(task);
		}
	}
}
