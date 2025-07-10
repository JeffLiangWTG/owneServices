using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	abstract class ConsolIncomingMessageCodeMappingTest : TestCaseWithFactoryAndMessagingHelpers
	{
		protected void CreateOCMUnlocoMappings(OrgHeader orgHeader, IReadOnlyDictionary<string, string> mappings)
		{
			var unlocoLoader = new RefUNLOCO.Loader(Factory.BOFactory);
			var patternMatchOverrides = new OrgPatternMatchOverrideCollection(orgHeader, Factory.BOFactory);

			foreach (var mapping in mappings)
			{
				var unlocoPattern = patternMatchOverrides.AddNew();
				unlocoPattern.OO_Context = "OCM";
				unlocoPattern.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
				unlocoPattern.OO_ForeignCode = mapping.Key;
				unlocoPattern.OO_LocalGuid = unlocoLoader.Load(mapping.Value).PK;
			}
		}

		protected ServiceTaskLogForTesting CreateAndProcessUniversalShipment(UniversalShipment shipment)
		{
			var message = GetQueuedUniversalShipmentMessage(shipment);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			return serviceTaskLog;
		}

		protected sealed class ForwardingConsolDataContextManagerProxyForTest : ForwardingConsolDataContextManager
		{
			public UniversalShipment UniversalShipmentToRead { get; private set; }
			protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
			{
				UniversalShipmentToRead = universalShipment;
				return base.GetShipmentDataObjectReader(universalShipment, logger, factory);
			}
		}
	}
}
