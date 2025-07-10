using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Rating.DataTransfer
{
	public class CompanyTariffDataContextManager : RatingHeaderDataContextManager<CompanyTariff>
	{
		protected override DataContextType GetDataContextTypeCore()
		{
			return DataContextType.GlobalRate;
		}
	}
}


