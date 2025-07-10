using CargoWise.EntityFramework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public interface IUSCATAIRMessageEventParentFinder
	{
		BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent eventDataObject, BusinessObjectFactory factory);
	}
}
