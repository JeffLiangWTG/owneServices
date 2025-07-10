using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.UniversalData
{
	public interface IPublishUniversalEvent
	{
		IXmlEventValueObject[] Publish(BusinessObjectFactory factory, DataContextType dataContextType, ZString key, BusinessObject dataProvider, StmALog eventBO);
	}

	public abstract class PublishUniversalEventCore
	{
		protected PublishUniversalEventCore() { }

		public static IXmlEventValueObject[] PublishUniversalEvent(BusinessObjectFactory factory, BusinessObject sendFrom, BusinessObject sendTo, StmALog log)
		{
			var manager = sendTo.GetUniversalDataContextManager();
			var publisher = ObjectFactory.Get<IPublishUniversalEvent>();
			return publisher.Publish(factory, manager.DataContextType, manager.DataContextKey, sendFrom, log);
		}
	}
}
