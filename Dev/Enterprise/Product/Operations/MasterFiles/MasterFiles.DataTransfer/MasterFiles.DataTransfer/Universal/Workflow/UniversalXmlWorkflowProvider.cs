using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Workflow
{
	class UniversalXmlWorkflowProvider : IUniversalXmlWorkflowProvider
	{
		public IUniversalXmlWorkflowProcessor GetUniversalXmlWorkflowProcessor(IUniversalActionInfo actionInfo, IMessageProcessorCommunicationModesResult communicationModesGetter, Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter, BusinessObject exportedBO, IEventInfo eventInfo = null, IXmlWriter xmlWriter = null, IUniversalXmlSchema schema = null)
		{
			if (actionInfo.ParentBO is IForwardingShipmentDeclarationProvider)
			{
				return new UniversalShipmentXmlWorkflowProcessor(actionInfo, communicationModesGetter, dataWriterGetter, exportedBO, eventInfo, xmlWriter, schema);
			}

			return new UniversalXmlWorkflowProcessor(actionInfo, communicationModesGetter, dataWriterGetter, exportedBO, eventInfo, xmlWriter, schema);
		}
	}
}
