using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Universal
{
	class JobVoyageEventParentFinder : EventParentFinder
	{
		public JobVoyageEventParentFinder(BusinessObjectFactory factory, JobVoyageDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			BusinessObject[] result = null;

			IXmlEventValueObject eventValueObject = xmlEvent;

			if (eventValueObject.Context != null && AcceptedEventTypes.Contains(eventValueObject.EventType))
			{
				var references = new JobVoyageReferences(eventValueObject.Context);
				var matcher = new JobVoyageMatcher(factory, references, logger);
				var bestMatch = matcher.GetBestMatch();

				result = bestMatch != null ? new BusinessObject[] { bestMatch } : null;
			}

			return result;
		}

		readonly List<string> AcceptedEventTypes = new List<string>()
		{
			AutoEvents.ArrivalCode,
			AutoEvents.DepartureCode,
			AutoEvents.CutOffDateCode,
			AutoEvents.CargoAvailableCode,
			AutoEvents.StorageCommencedCode,
			AutoEvents.ReceiptCommencedCode
		};
	}
}
