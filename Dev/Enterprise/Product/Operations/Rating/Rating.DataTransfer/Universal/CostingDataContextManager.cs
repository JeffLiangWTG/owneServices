using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Rating.DataTransfer
{
	public class CostingDataContextManager : RatingHeaderDataContextManager<Costing>
	{
		protected override DataContextType GetDataContextTypeCore()
		{
			return DataContextType.Costing;
		}
	}
}
