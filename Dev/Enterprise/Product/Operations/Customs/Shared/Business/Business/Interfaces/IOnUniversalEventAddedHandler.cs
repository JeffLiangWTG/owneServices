using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.Business.Interfaces
{
	public interface IOnUniversalEventAddedHandler
	{
		void OnUniversalEventAdded(IXmlSessionTracker logger, UniversalDataBuss.DataObjects.Universal.Event eventAdded);
	}
}
