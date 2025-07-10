using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public interface IJobDeclarationEventProcessor
	{
		bool ProcessMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject);
	}

	static class JobDeclarationEventProcessorsFactory
	{
		internal static IEnumerable<IJobDeclarationEventProcessor> All
		{
			get
			{
				var types = ObjectFactory.Get<Hashtable>("JobDeclarationEventProcessors");
				foreach (string key in types.Keys)
				{
					yield return ((ObjectHandle)types[key]).GetObject() as IJobDeclarationEventProcessor;
				}
			}
		}
	}
}
