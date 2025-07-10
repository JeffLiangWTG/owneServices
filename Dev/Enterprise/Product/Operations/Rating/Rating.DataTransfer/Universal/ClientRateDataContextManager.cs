using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Rating.DataTransfer
{
	public class ClientRateDataContextManager : RatingHeaderDataContextManager<ClientRate>
	{
		protected override DataContextType GetDataContextTypeCore()
		{
			return DataContextType.ClientRate;
		}
	}
}


