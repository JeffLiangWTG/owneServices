using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class ViewValueAnalysisCollection : BusinessObjectCollection<ViewValueAnalysis>
	{
		public ViewValueAnalysisCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
