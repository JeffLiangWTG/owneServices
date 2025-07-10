using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	[Serializable]
	class ContainerAutomationDeclarationSubscriptionUpdater : ContainerAutomationSubscriptionUpdater
	{
		// 35 character limit on this string to be written into the DB
		public override string Name => "CADeclarationSubscriptionUpdater";

		public override string FriendlyName => (NoResString)"Container Auto Declaration Subscription Updater"; // this is a commonly used approach for FriendlyName

		protected override Type GetBusinessObjectType()
		{
			return ObjectFactory.GetType<IBaseJobDeclaration>();
		}

		protected override EventDataObjectWriter GetEventDataObjectWriter(IDataWritingManager dataWritingManager, BusinessObject parent)
		{
			var containerTrackingProvider = parent as IContainerTrackingProvider;
			return new ContainerAutomationDeclarationEventDataObjectWriter(dataWritingManager, (ITransportParent)parent)
			{
				AcceptsOnlyProvidedContainers = true,
				CoLoadBookingReference = string.Empty,
				CoLoadBillNumber = string.Empty,
				CoLoadWithName = string.Empty,
				CoLoadWithCode = string.Empty,
				CoLoadWithC1CCode = string.Empty,
				ContainerMode = containerTrackingProvider.ContainerMode,
				PopulateAdditionalContexts = true,
				TransportMode = containerTrackingProvider.TransportMode
			};
		}
	}
}
