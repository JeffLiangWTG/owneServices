using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Workflow
{
	public class PublishUniversalEvent : PublishUniversalEventCore, IPublishUniversalEvent
	{
		IXmlEventValueObject[] IPublishUniversalEvent.Publish(BusinessObjectFactory factory, DataContextType dataContextType, ZString key, BusinessObject dataProvider, StmALog eventBO)
		{
			return UniversalXmlWorkflowProcessor.PublishUniversalEvent(factory, dataContextType, key, dataProvider, eventBO).Cast<IXmlEventValueObject>().ToArray();
		}
	}
}
