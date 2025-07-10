using System;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportConsignment.Business
{
	class DtbConsignmentActionPickedUpDeliveredUpdater : IDtbConsignmentActionPickedUpDeliveredUpdater
	{
		public void ProcessLog(BusinessObjectFactory businessObjectFactory, ZGuid dtbConsignmentActionPK)
		{
			var consignmentAction = businessObjectFactory.Load<DtbConsignmentAction>(dtbConsignmentActionPK);
			if (consignmentAction == null)
			{
				return;
			}

			if (!consignmentAction.LTA_ActualTime.IsEmpty && consignmentAction.ConsignmentAddress.ConsignmentAddressType != ConsignmentAddressTypes.Codes.Multi)
			{
				foreach (var package in consignmentAction.Packages)
				{
					var log = package.Logs.CreateRecreateOrUpdateEventLog(consignmentAction.ActionType == ActionTypes.Codes.PickUp ? Events.PickedUp : Events.Delivered, EstimateActual.Actual, consignmentAction.LTA_ActualTime);
					PublishUniversalEvent(package.Factory, package, log);
				}
			}
		}

		void PublishUniversalEvent(BusinessObjectFactory factory, BusinessObject dataProvider, StmALog eventBO)
		{
			dataProvider = dataProvider ?? throw new InvalidOperationException("The IWorkflowProvider passed in must be a BusinessObject.");
			var manager = dataProvider.GetUniversalDataContextManager() as IEventDataContextManager ?? throw new InvalidOperationException("Cannot call PublishUniversalEvent when the DataContextManager for the IWorkflowProvider (BusinessObject) specified is not capable of generating a Universal Event.");

			var communicationModesProvider = new UniversalXmlCommunicationModeProvider(() =>
			{
				return (new IEDICommunicationsMode[] { new NonPersistentEDICommunicationMode() { EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss } }, null);
			});

			var actionInfo = new ActionInfo(Array.Empty<RecipientRoleDetail>(), dataProvider, factory) { ActionType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML };
			Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = (outboundSessionTracker) => new EventDataObjectWriter(outboundSessionTracker);
			var processor = new UniversalXmlWorkflowProcessor(actionInfo, communicationModesProvider, dataWriterGetter, eventBO);

			processor.Process(new NotificationBuffer(), CancellationToken.None);
		}
	}
}
