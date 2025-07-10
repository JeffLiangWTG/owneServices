using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class ConsolidationProfitShareCollection : DependentBusinessObjectCollection<ConsolidationProfitShare, ProfitShareRedistribution>
	{
		public ConsolidationProfitShareCollection(ProfitShareRedistribution parent)
			: base(parent)
		{
		}
	}
}
