using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	[Serializable]
	class ConsolTrackingUpdater : ContainerAutomationSubscriptionUpdater
	{
		public override string Name => "ConsolTrackingUpdater";

		public override string FriendlyName => (NoResString)"Container Automation Consol Subscription Updater"; // this is a commonly used approach for FriendlyName

		protected override Type GetBusinessObjectType()
		{
			return typeof(ForwardingConsol);
		}

		protected override EventDataObjectWriter GetEventDataObjectWriter(IDataWritingManager dataWritingManager, BusinessObject parent)
		{
			var consol = parent as ForwardingConsol;
			var containerTrackingProvider = parent as IContainerTrackingProvider;
			var isConsolCoload = consol.JK_AgentType == Core.Constants.AgentType.CoLoad;

			return new ContainerAutomationConsolEventDataObjectWriter(dataWritingManager, (ITransportParent)parent)
			{
				AcceptsOnlyProvidedContainers = isConsolCoload,
				CoLoadBookingReference = isConsolCoload ? consol.JK_CoLoadBookingReference : ZString.Empty,
				CoLoadBillNumber = isConsolCoload ? consol.JK_CoLoadMasterBill : ZString.Empty,
				CoLoadWithCode = consol.Creditor == null || !isConsolCoload ? ZString.Empty : consol.Creditor.SCACCode,
				CoLoadWithC1CCode = consol.Creditor == null || !isConsolCoload ? ZString.Empty : consol.Creditor.C1CCode,
				CoLoadWithName = consol.Creditor == null || !isConsolCoload ? ZString.Empty : consol.Creditor.OH_FullName,
				PopulateAdditionalContexts = true,
				ContainerMode = containerTrackingProvider.ContainerMode,
				TransportMode = containerTrackingProvider.TransportMode
			};
		}
	}
}
