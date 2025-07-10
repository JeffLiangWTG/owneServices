using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class CommunicationDataContextManager : EventDataContextManager<OrgSalesCall>
	{
		public override DataContextType DataContextType => DataContextType.Communication;

		public override ZString DataContextKey => ParentBO.OQ_CommunicationID;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
			=> new ZQuery(OrgSalesCallSchema.OQ_CommunicationID, matchingValues.Key);

		public override string DefaultOutputDirectory => null;

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => new CommunicationEventParentFinder(factory, this, logger);

		class CommunicationEventParentFinder : EventParentFinder
		{
			internal CommunicationEventParentFinder(BusinessObjectFactory factory, CommunicationDataContextManager manager, IXmlImportLogger logger)
				: base(factory, manager, logger)
			{
			}

			protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent) => null;
		}
	}
}

