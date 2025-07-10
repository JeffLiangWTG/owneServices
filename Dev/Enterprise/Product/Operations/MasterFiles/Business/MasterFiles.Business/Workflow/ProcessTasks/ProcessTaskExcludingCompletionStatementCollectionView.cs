using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskExcludingCompletionStatementCollectionView : ProcessTaskCollectionView
	{
		public ProcessTaskExcludingCompletionStatementCollectionView(ProcessTaskCollection collection)
			: base(collection)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return base.IsThisPartOfTheCollection(element) && !((ProcessTask)element).IsCompletionStatement;
		}
	}
}
