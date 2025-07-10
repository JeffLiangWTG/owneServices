using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Workflow
{
	class UniversalShipmentXmlWorkflowProcessor : UniversalXmlWorkflowProcessor
	{
		public UniversalShipmentXmlWorkflowProcessor(IUniversalActionInfo actionInfo, IMessageProcessorCommunicationModesResult communicationModesGetter, Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter, BusinessObject exportedBO, IEventInfo eventInfo = null, IXmlWriter xmlWriter = null, IUniversalXmlSchema schema = null)
			: base(actionInfo, communicationModesGetter, dataWriterGetter, exportedBO, eventInfo, xmlWriter, schema)
		{
		}

		protected override BusinessObject GetDataContextBO(ITopLevelDataObjectWriter dataWriter, BusinessObject dataContextBO)
		{
			if (dataWriter is IDeclarationDataObjectWriter && dataContextBO is IForwardingShipmentDeclarationProvider declarationProvider)
			{
				return declarationProvider.GetDeclaration();
			}

			return dataContextBO;
		}
	}
}
